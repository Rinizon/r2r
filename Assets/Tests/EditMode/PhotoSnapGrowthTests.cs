using System;
using System.IO;
using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class PhotoSnapGrowthTests
{
    private static CompanySession Starter() => new CompanySession(AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime());
    private static ContractInstance Host(CompanySession session, string id)
    {
        session.TryAcceptOffer(id, out var contract, out _);
        session.TryAssignContract(contract.Id, session.Hardware[0].Id, out _);
        return contract;
    }
    private static CompanySession Reload(CompanySession session)
    {
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(session, "test")), session.Catalog, out var loaded, out var reason), Is.True, reason);
        return loaded;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void GrowthOccursOnceAfterSecondPaymentAndObserversSeeCommittedBillingAndUsage(bool both)
    {
        var session = Starter();
        var photo = Host(session, "offer.photosnap");
        if (both) Host(session, "offer.local-website");
        int events = 0;
        session.MilestoneApplied += (contract, milestone) =>
        {
            events++;
            Assert.That(contract, Is.SameAs(photo));
            Assert.That(milestone.Id, Is.EqualTo("growth.photosnap.cpu-2"));
            Assert.That(contract.PaidMonths, Is.EqualTo(2));
            Assert.That(session.Cash, Is.EqualTo(both ? 470m : 310m));
            Assert.That(session.OperatingProfit, Is.EqualTo(both ? 320m : 160m));
            Assert.That(session.BillingHistory.Count, Is.EqualTo(2));
            Assert.That(session.LastMonthlyResult.PeriodEnd, Is.EqualTo(new DateTime(1995, 2, 28)));
            Assert.That(contract.EffectiveRequirements.CpuCores, Is.EqualTo(2));
            Assert.That(session.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(both ? 3 : 2));
        };
        session.Clock.Advance(62);
        session.Clock.Advance(55.5);
        Assert.That(events, Is.Zero);
        Assert.That(photo.EffectiveRequirements.CpuCores, Is.EqualTo(1));
        session.Clock.Advance(0.5);
        Assert.That(events, Is.EqualTo(1));
        Assert.That(photo.EffectiveRequirements.RamGb, Is.EqualTo(2));
        Assert.That(photo.EffectiveRequirements.StorageGb, Is.EqualTo(20));
        Assert.That(photo.EffectiveRequirements.BandwidthMbps, Is.EqualTo(4));
        Assert.That(photo.Definition.MonthlyRevenue, Is.EqualTo(100m));
        Assert.That(photo.Definition.Requirements.CpuCores, Is.EqualTo(1));
        Assert.That(photo.AssignedHardwareId, Is.EqualTo(session.Hardware[0].Id));
        Assert.That(session.Hardware[0].Resources.Available.CpuCores, Is.Zero);
        session.Clock.Advance(62);
        Assert.That(events, Is.EqualTo(1));
        Assert.That(photo.AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(photo.PaidMonths, Is.EqualTo(3));
    }

    [Test]
    public void LargeUpdateAppliesOneMilestoneWhileEveryMonthStillBills()
    {
        var session = Starter();
        var photo = Host(session, "offer.photosnap");
        Host(session, "offer.local-website");
        int events = 0;
        session.MilestoneApplied += (_, __) => events++;
        session.Clock.Advance((new DateTime(1996, 3, 1) - session.Clock.Date).TotalDays * 2);
        Assert.That(events, Is.EqualTo(1));
        Assert.That(photo.PaidMonths, Is.EqualTo(14));
        Assert.That(session.BillingHistory.Count, Is.EqualTo(14));
        Assert.That(session.Cash, Is.EqualTo(2390m));
        Assert.That(session.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
    }

    [Test]
    public void UnassignedTimeAndOtherSessionsDoNotAdvancePhotoSnapGrowth()
    {
        var session = Starter();
        var other = new CompanySession(session.Catalog);
        session.TryAcceptOffer("offer.photosnap", out var photo, out _);
        session.Clock.Advance(118);
        Assert.That(photo.PaidMonths, Is.Zero);
        Assert.That(photo.AppliedMilestoneIds, Is.Empty);
        Assert.That(photo.EffectiveRequirements.CpuCores, Is.EqualTo(1));
        session.TryAssignContract(photo.Id, session.Hardware[0].Id, out _);
        session.Clock.Advance(62);
        Assert.That(photo.PaidMonths, Is.EqualTo(1));
        Assert.That(photo.AppliedMilestoneIds, Is.Empty);
        session.Clock.Advance(60);
        Assert.That(photo.PaidMonths, Is.EqualTo(2));
        Assert.That(photo.EffectiveRequirements.CpuCores, Is.EqualTo(2));
        Assert.That(other.AcceptedContracts, Is.Empty);
        Assert.That(other.Cash, Is.EqualTo(150m));
        Assert.That(session.Catalog.Offers[0].Requirements.CpuCores, Is.EqualTo(1));
    }

    [Test]
    public void PrePurchasedCpuHeadroomAccommodatesGrowthWithoutChangingAssignments()
    {
        var session = Starter();
        var pc = session.Hardware[0];
        session.TryPurchaseUpgrade("upgrade.starter-cpu", pc.Id, out _);
        var photo = Host(session, "offer.photosnap");
        var web = Host(session, "offer.local-website");
        session.Clock.Advance(118);
        Assert.That(pc.Resources.Capacity.CpuCores, Is.EqualTo(4));
        Assert.That(pc.Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(pc.Resources.Available.CpuCores, Is.EqualTo(1));
        Assert.That(photo.AssignedHardwareId, Is.EqualTo(pc.Id));
        Assert.That(web.AssignedHardwareId, Is.EqualTo(pc.Id));
        Assert.That(session.Cash, Is.EqualTo(350m));
        Assert.That(session.OperatingProfit, Is.EqualTo(320m));
    }

    [Test]
    public void ReloadBeforeAndAfterMilestoneDoesNotDuplicateGrowthOrPayments()
    {
        var source = Starter();
        Host(source, "offer.photosnap");
        Host(source, "offer.local-website");
        source.Clock.Advance(117.5); // February 28, 0.75 day progress.
        var before = Reload(source);
        Assert.That(before.AcceptedContracts[0].AppliedMilestoneIds, Is.Empty);
        int events = 0;
        before.MilestoneApplied += (_, __) => events++;
        before.Clock.Advance(0.5);
        Assert.That(events, Is.EqualTo(1));
        Assert.That(before.Cash, Is.EqualTo(470m));
        var after = Reload(before);
        int replayed = 0;
        after.MilestoneApplied += (_, __) => replayed++;
        Assert.That(after.AcceptedContracts[0].AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(after.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(after.TryBillMonth(new DateTime(1995, 2, 28), out _), Is.False);
        after.Clock.Advance(62);
        Assert.That(replayed, Is.Zero);
        Assert.That(after.Cash, Is.EqualTo(630m));
        Assert.That(after.AcceptedContracts[0].PaidMonths, Is.EqualTo(3));
    }

    [Test]
    public void EarlierVersionTwoStateWithEligibleMissingFlagReconcilesSilently()
    {
        var source = Starter();
        Host(source, "offer.photosnap");
        Host(source, "offer.local-website");
        source.Clock.Advance(118);
        var snapshot = SessionSaveJson.Capture(source, "state-only-build");
        snapshot.Contracts[0].AppliedMilestoneIds = Array.Empty<string>();
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(snapshot), source.Catalog, out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.AcceptedContracts[0].EffectiveRequirements.CpuCores, Is.EqualTo(2));
        Assert.That(loaded.Cash, Is.EqualTo(470m));
        int events = 0;
        loaded.MilestoneApplied += (_, __) => events++;
        loaded.Clock.Advance(62);
        Assert.That(events, Is.Zero);
        Assert.That(loaded.AcceptedContracts[0].AppliedMilestoneIds.Count, Is.EqualTo(1));
        Assert.That(loaded.Cash, Is.EqualTo(630m));
        var legacy = File.ReadAllText("Assets/Tests/EditMode/Fixtures/phase2-v1-two-paid-months.json");
        Assert.That(SessionSaveJson.TryLoad(legacy, source.Catalog, out var migrated, out reason), Is.True, reason);
        Assert.That(migrated.AcceptedContracts[0].AppliedMilestoneIds.Single(), Is.EqualTo(loaded.AcceptedContracts[0].AppliedMilestoneIds.Single()));
    }
}
