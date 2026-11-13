using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class Phase3SaveTests
{
    private const string LegacyPath = "Assets/Tests/EditMode/Fixtures/phase2-v1-two-paid-months.json";
    private static HostingCatalog Catalog() => AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();
    private static SessionSaveData LegacyData()
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(LegacyPath))))
            return (SessionSaveData)new DataContractJsonSerializer(typeof(SessionSaveData)).ReadObject(stream);
    }
    private static SessionSaveData Version2(bool upgraded)
    {
        var data = LegacyData();
        data.SaveVersion = 2;
        data.Hardware[0].Purchases = upgraded ? new[] { new UpgradePurchaseSaveData { HardwareId = "pc-original", UpgradeId = "upgrade.starter-cpu", AmountPaid = 120m } } : Array.Empty<UpgradePurchaseSaveData>();
        data.Contracts[0].AppliedMilestoneIds = new[] { "growth.photosnap.cpu-2" };
        data.Contracts[1].AppliedMilestoneIds = Array.Empty<string>();
        if (upgraded) data.Cash -= 120m;
        return data;
    }

    [Test]
    public void LegacyFixtureMigratesGrowthWithoutRebillingOrLosingAssignment()
    {
        var catalog = Catalog();
        Assert.That(SessionSaveJson.TryLoad(File.ReadAllText(LegacyPath), catalog, out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.Cash, Is.EqualTo(470m));
        Assert.That(loaded.OperatingProfit, Is.EqualTo(320m));
        Assert.That(loaded.Clock.DayProgress, Is.EqualTo(0.75));
        Assert.That(loaded.Clock.Speed, Is.EqualTo(SimulationSpeed.Paused));
        Assert.That(loaded.Hardware[0].Id, Is.EqualTo("pc-original"));
        Assert.That(loaded.Hardware[0].PurchaseRecords, Is.Empty);
        Assert.That(loaded.AcceptedContracts[0].Id, Is.EqualTo("photo-original"));
        Assert.That(loaded.AcceptedContracts[0].AssignedHardwareId, Is.EqualTo("pc-original"));
        Assert.That(loaded.AcceptedContracts[0].PaidMonths, Is.EqualTo(2));
        Assert.That(loaded.AcceptedContracts[0].AppliedMilestoneIds.Single(), Is.EqualTo("growth.photosnap.cpu-2"));
        Assert.That(loaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(loaded.Hardware[0].Resources.Capacity.CpuCores, Is.EqualTo(2));
        Assert.That(loaded.Hardware[0].Resources.Available.CpuCores, Is.Zero);
        Assert.That(loaded.TryBillMonth(new DateTime(1995, 2, 28), out _), Is.False);
        var saved = SessionSaveJson.Capture(loaded, "phase3-test");
        Assert.That(saved.SaveVersion, Is.EqualTo(2));
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(saved), catalog, out var again, out reason), Is.True, reason);
        Assert.That(again.AcceptedContracts[0].AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(again.Cash, Is.EqualTo(470m));
        again.Clock.Speed = SimulationSpeed.Normal;
        again.Clock.Advance(60.5); // March 1.75 -> April 1.
        Assert.That(again.Cash, Is.EqualTo(630m));
        Assert.That(again.BillingHistory.Count, Is.EqualTo(3));
        Assert.That(again.AcceptedContracts[0].PaidMonths, Is.EqualTo(3));
    }

    [TestCase(false, 470, 2)]
    [TestCase(true, 350, 4)]
    public void Version2RestoresUpgradesAndOverloadFromDefinitions(bool upgraded, decimal cash, double capacity)
    {
        var data = Version2(upgraded);
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), Catalog(), out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.Cash, Is.EqualTo(cash));
        Assert.That(loaded.OperatingProfit, Is.EqualTo(320m));
        Assert.That(loaded.Hardware[0].EffectiveCapacity.CpuCores, Is.EqualTo(capacity));
        Assert.That(loaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        var snapshot = SessionSaveJson.Capture(loaded, "test");
        Assert.That(snapshot.Hardware[0].Purchases.Length, Is.EqualTo(upgraded ? 1 : 0));
        Assert.That(snapshot.Contracts[0].AppliedMilestoneIds.Single(), Is.EqualTo("growth.photosnap.cpu-2"));
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(snapshot), Catalog(), out var again, out reason), Is.True, reason);
        Assert.That(again.Clock.DayProgress, Is.EqualTo(0.75));
        Assert.That(again.Cash, Is.EqualTo(cash));
        Assert.That(again.Hardware[0].AppliedUpgradeIds.Count, Is.EqualTo(upgraded ? 1 : 0));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LegacyMigrationDoesNotGrowEarlyOrUnassignedContracts(bool unassignedPhoto)
    {
        var data = LegacyData();
        data.Date = "1995-02-01";
        data.BillingHistory = data.BillingHistory.Take(1).ToArray();
        data.Contracts[0].PaidMonths = unassignedPhoto ? 0 : 1;
        data.Contracts[1].PaidMonths = 1;
        if (unassignedPhoto) data.Contracts[0].AssignedHardwareId = null;
        data.BillingHistory[0].Revenue = unassignedPhoto ? 80m : 180m;
        data.OperatingProfit = data.BillingHistory[0].Revenue - 20m;
        data.Cash = 150m + data.OperatingProfit;
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), Catalog(), out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.AcceptedContracts[0].AppliedMilestoneIds, Is.Empty);
        Assert.That(loaded.AcceptedContracts[0].EffectiveRequirements.CpuCores, Is.EqualTo(1));
        Assert.That(loaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(unassignedPhoto ? 1 : 2));
    }

    [Test]
    public void VersionOneCannotCarryPhaseThreeState()
    {
        var data = Version2(true);
        data.SaveVersion = 1;
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), Catalog(), out _, out _), Is.False);
    }

    [Test]
    public void HistoricalPriceIsPreservedAndNewAssignmentsStillNeedCapacity()
    {
        var data = Version2(true);
        data.Hardware[0].Purchases[0].AmountPaid = 119m;
        data.Cash = 351m;
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), Catalog(), out var historical, out var reason), Is.True, reason);
        Assert.That(historical.Hardware[0].PurchaseRecords[0].AmountPaid, Is.EqualTo(119m));
        var catalog = Catalog();
        var extra = new OfferData("offer.extra", "Extra", new ResourceAmounts(1, 0, 0, 0), 10);
        var extended = new HostingCatalog(catalog.StarterPc, catalog.Offers.Concat(new[] { extra }), catalog.Upgrades, catalog.Milestones);
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(Version2(false)), extended, out var overloaded, out reason), Is.True, reason);
        Assert.That(overloaded.TryAcceptOffer(extra.Id, out var accepted, out reason), Is.True, reason);
        Assert.That(overloaded.TryAssignContract(accepted.Id, overloaded.Hardware[0].Id, out reason), Is.False);
        Assert.That(reason, Does.Contain("CPU"));
        Assert.That(overloaded.AcceptedContracts[0].IsHosted, Is.True);
        Assert.That(overloaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
    }

    [TestCase("unknown-upgrade")]
    [TestCase("duplicate-purchase")]
    [TestCase("wrong-hardware")]
    [TestCase("zero-cost")]
    [TestCase("negative-cost")]
    [TestCase("missing-purchases")]
    [TestCase("unknown-milestone")]
    [TestCase("wrong-milestone-target")]
    [TestCase("duplicate-milestone")]
    [TestCase("early-milestone")]
    [TestCase("missing-milestones")]
    [TestCase("assignment")]
    [TestCase("cash")]
    [TestCase("profit")]
    [TestCase("history")]
    [TestCase("future")]
    public void InvalidVersion2StateLeavesLiveSessionUntouched(string fault)
    {
        var catalog = Catalog();
        var live = new CompanySession(catalog);
        var data = Version2(true);
        switch (fault)
        {
            case "unknown-upgrade": data.Hardware[0].Purchases[0].UpgradeId = "unknown"; break;
            case "duplicate-purchase": data.Hardware[0].Purchases = new[] { data.Hardware[0].Purchases[0], data.Hardware[0].Purchases[0] }; data.Cash -= 120; break;
            case "wrong-hardware": data.Hardware[0].Purchases[0].HardwareId = "other"; break;
            case "zero-cost": data.Hardware[0].Purchases[0].AmountPaid = 0; data.Cash = 470; break;
            case "negative-cost": data.Hardware[0].Purchases[0].AmountPaid = -1; data.Cash = 471; break;
            case "missing-purchases": data.Hardware[0].Purchases = null; break;
            case "unknown-milestone": data.Contracts[0].AppliedMilestoneIds[0] = "unknown"; break;
            case "wrong-milestone-target": data.Contracts[1].AppliedMilestoneIds = data.Contracts[0].AppliedMilestoneIds; break;
            case "duplicate-milestone": data.Contracts[0].AppliedMilestoneIds = new[] { data.Contracts[0].AppliedMilestoneIds[0], data.Contracts[0].AppliedMilestoneIds[0] }; break;
            case "early-milestone": data.Contracts[0].PaidMonths = 1; break;
            case "missing-milestones": data.Contracts[0].AppliedMilestoneIds = null; break;
            case "assignment": data.Contracts[0].AssignedHardwareId = "unknown"; break;
            case "cash": data.Cash++; break;
            case "profit": data.OperatingProfit -= 120; break;
            case "history": data.BillingHistory[1].PeriodEnd = data.BillingHistory[0].PeriodEnd; break;
            case "future": data.SaveVersion = 3; break;
        }
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), catalog, out var loaded, out var reason), Is.False);
        Assert.That(loaded, Is.Null);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(live.Cash, Is.EqualTo(150m));
        Assert.That(live.AcceptedContracts, Is.Empty);
    }
}
