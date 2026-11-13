using System;
using System.IO;
using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class UpgradePurchaseTests
{
    private static HostingCatalog Catalog() => AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();

    [TestCase(0, 30, 4, 4, 250, 10)]
    [TestCase(1, 70, 2, 8, 250, 10)]
    [TestCase(2, 50, 2, 4, 500, 10)]
    [TestCase(3, 60, 2, 4, 250, 20)]
    public void EachUpgradeCommitsOneRecordAndOnlyItsCapacity(int index, decimal cash, double cpu, double ram, double storage, double bandwidth)
    {
        var catalog = Catalog();
        var session = new CompanySession(catalog);
        var pc = session.Hardware[0];
        session.TryAcceptOffer("offer.photosnap", out var contract, out _);
        session.TryAssignContract(contract.Id, pc.Id, out _);
        Assert.That(session.TryPurchaseUpgrade(catalog.Upgrades[index].Id, pc.Id, out var reason), Is.True, reason);
        Assert.That(reason, Is.Empty);
        Assert.That(session.Hardware[0], Is.SameAs(pc));
        Assert.That(session.Cash, Is.EqualTo(cash));
        Assert.That(session.OperatingProfit, Is.Zero);
        AssertCapacity(pc, cpu, ram, storage, bandwidth);
        var record = pc.PurchaseRecords.Single();
        Assert.That(record.HardwareId, Is.EqualTo(pc.Id));
        Assert.That(record.UpgradeId, Is.EqualTo(catalog.Upgrades[index].Id));
        Assert.That(record.AmountPaid, Is.EqualTo(catalog.Upgrades[index].Price));
        Assert.That(pc.AppliedUpgradeIds.Single(), Is.EqualTo(record.UpgradeId));
        Assert.That(contract.AssignedHardwareId, Is.EqualTo(pc.Id));
        Assert.That(pc.Resources.Used.CpuCores, Is.EqualTo(1));
        Assert.That(pc.Resources.Used.RamGb, Is.EqualTo(2));
        Assert.That(pc.Resources.Used.StorageGb, Is.EqualTo(20));
        Assert.That(pc.Resources.Used.BandwidthMbps, Is.EqualTo(4));
        Assert.That(pc.Definition.MonthlyExpense, Is.EqualTo(20m));
        Assert.That(catalog.StarterPc.Capacity.CpuCores, Is.EqualTo(2));
        string json = SessionSaveJson.ToJson(SessionSaveJson.Capture(session, "test"));
        Assert.That(SessionSaveJson.TryLoad(json, catalog, out var loaded, out reason), Is.True, reason);
        AssertCapacity(loaded.Hardware[0], cpu, ram, storage, bandwidth);
        Assert.That(loaded.Cash, Is.EqualTo(cash));
        Assert.That(loaded.OperatingProfit, Is.Zero);
        Assert.That(loaded.Hardware[0].Id, Is.EqualTo(pc.Id));
        Assert.That(loaded.Hardware[0].PurchaseRecords.Single().AmountPaid, Is.EqualTo(record.AmountPaid));
        loaded.Clock.Advance(62);
        Assert.That(loaded.LastMonthlyResult.Expenses, Is.EqualTo(20m));
        Assert.That(loaded.Cash, Is.EqualTo(cash + 80m));
        Assert.That(loaded.OperatingProfit, Is.EqualTo(80m));
    }

    [Test]
    public void InsufficientFundsAndDuplicatePurchasePreserveTheEntireSession()
    {
        var session = new CompanySession(Catalog());
        var pc = session.Hardware[0];
        Assert.That(session.TryPurchaseUpgrade("upgrade.starter-cpu", pc.Id, out var reason), Is.True, reason);
        string before = Snapshot(session);
        foreach (string upgradeId in new[] { "upgrade.starter-ram", "upgrade.starter-cpu" })
        {
            Assert.That(session.TryPurchaseUpgrade(upgradeId, pc.Id, out reason), Is.False);
            Assert.That(reason, Is.Not.Empty);
            Assert.That(Snapshot(session), Is.EqualTo(before));
        }
        Assert.That(session.Cash, Is.EqualTo(30m));
        Assert.That(session.OperatingProfit, Is.Zero);
        AssertCapacity(pc, 4, 4, 250, 10);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("unknown")]
    public void InvalidIdsPreserveContractsFinancesAndResources(string invalid)
    {
        var session = new CompanySession(Catalog());
        session.TryAcceptOffer("offer.photosnap", out var photo, out _);
        session.TryAssignContract(photo.Id, session.Hardware[0].Id, out _);
        string before = Snapshot(session);
        Assert.That(session.TryPurchaseUpgrade(invalid, session.Hardware[0].Id, out var reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.TryPurchaseUpgrade("upgrade.starter-cpu", invalid, out reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(Snapshot(session), Is.EqualTo(before));
    }

    [Test]
    public void ForeignHardwareAndDefinitionIdsCannotBeUsedAsRuntimeTargets()
    {
        var catalog = Catalog();
        var first = new CompanySession(catalog);
        var second = new CompanySession(catalog);
        string before = Snapshot(first);
        foreach (string id in new[] { second.Hardware[0].Id, catalog.StarterPc.Id })
        {
            Assert.That(first.TryPurchaseUpgrade("upgrade.starter-cpu", id, out var reason), Is.False);
            Assert.That(reason, Is.Not.Empty);
            Assert.That(Snapshot(first), Is.EqualTo(before));
        }
        Assert.That(second.Hardware[0].PurchaseRecords, Is.Empty);
        Assert.That(second.Cash, Is.EqualTo(150m));
    }

    [Test]
    public void ExactAffordabilitySucceedsAndDuplicateStillExplainsRejection()
    {
        var catalog = Catalog();
        var exact = new UpgradeData("upgrade.exact", "Exact", catalog.StarterPc.Id, 150m, new ResourceAmounts(2, 0, 0, 0));
        var session = new CompanySession(new HostingCatalog(catalog.StarterPc, catalog.Offers, new[] { exact }, catalog.Milestones));
        Assert.That(session.TryPurchaseUpgrade(exact.Id, session.Hardware[0].Id, out var reason), Is.True, reason);
        Assert.That(session.Cash, Is.Zero);
        Assert.That(session.OperatingProfit, Is.Zero);
        Assert.That(session.TryPurchaseUpgrade(exact.Id, session.Hardware[0].Id, out reason), Is.False);
        Assert.That(reason, Does.Contain("already"));
    }

    [Test]
    public void UnrepresentableCapacityDoesNotPartiallyCommitPurchase()
    {
        var pc = new HardwareData("pc", "PC", new ResourceAmounts(double.MaxValue, 1, 1, 1), 20);
        var upgrade = new UpgradeData("huge", "Huge", pc.Id, 1, new ResourceAmounts(double.MaxValue, 0, 0, 0));
        var session = new CompanySession(new HostingCatalog(pc, Array.Empty<OfferData>(), new[] { upgrade }, Array.Empty<GrowthMilestoneData>()));
        string before = Snapshot(session);
        Assert.That(session.TryPurchaseUpgrade(upgrade.Id, session.Hardware[0].Id, out var reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(Snapshot(session), Is.EqualTo(before));
    }

    [Test]
    public void PurchaseRepairsMigratedOverloadWithoutChangingAssignmentsOrProfit()
    {
        string json = File.ReadAllText("Assets/Tests/EditMode/Fixtures/phase2-v1-two-paid-months.json");
        Assert.That(SessionSaveJson.TryLoad(json, Catalog(), out var session, out var reason), Is.True, reason);
        var pc = session.Hardware[0];
        var photo = session.AcceptedContracts[0];
        Assert.That(pc.Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(session.TryPurchaseUpgrade("upgrade.starter-cpu", pc.Id, out reason), Is.True, reason);
        Assert.That(session.Cash, Is.EqualTo(350m));
        Assert.That(session.OperatingProfit, Is.EqualTo(320m));
        Assert.That(pc.Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(pc.Resources.Available.CpuCores, Is.EqualTo(1));
        Assert.That(session.AcceptedContracts[0], Is.SameAs(photo));
        Assert.That(photo.AssignedHardwareId, Is.EqualTo(pc.Id));
        Assert.That(photo.AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(session, "test")), session.Catalog, out var loaded, out reason), Is.True, reason);
        Assert.That(loaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(loaded.Hardware[0].Resources.Capacity.CpuCores, Is.EqualTo(4));
        Assert.That(loaded.Cash, Is.EqualTo(350m));
    }

    [Test]
    public void AccumulatedPurchasesComposeAndDuplicateAfterReloadRemainsRejected()
    {
        var session = new CompanySession(Catalog());
        foreach (var offer in session.Catalog.Offers)
        {
            session.TryAcceptOffer(offer.Id, out var contract, out _);
            session.TryAssignContract(contract.Id, session.Hardware[0].Id, out _);
        }
        session.Clock.Advance(118); // $470 available, $320 operating profit.
        foreach (var upgrade in session.Catalog.Upgrades)
            Assert.That(session.TryPurchaseUpgrade(upgrade.Id, session.Hardware[0].Id, out var reason), Is.True, reason);
        Assert.That(session.Cash, Is.EqualTo(80m));
        Assert.That(session.OperatingProfit, Is.EqualTo(320m));
        AssertCapacity(session.Hardware[0], 4, 8, 500, 20);
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(session, "test")), session.Catalog, out var loaded, out var message), Is.True, message);
        string before = Snapshot(loaded);
        Assert.That(loaded.TryPurchaseUpgrade("upgrade.starter-cpu", loaded.Hardware[0].Id, out message), Is.False);
        Assert.That(Snapshot(loaded), Is.EqualTo(before));
    }

    private static string Snapshot(CompanySession session)
    {
        var data = SessionSaveJson.Capture(session, "test");
        data.Timestamp = "fixed";
        return SessionSaveJson.ToJson(data);
    }

    private static void AssertCapacity(HardwareInstance hardware, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(hardware.Resources.Capacity.CpuCores, Is.EqualTo(cpu));
        Assert.That(hardware.Resources.Capacity.RamGb, Is.EqualTo(ram));
        Assert.That(hardware.Resources.Capacity.StorageGb, Is.EqualTo(storage));
        Assert.That(hardware.Resources.Capacity.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
