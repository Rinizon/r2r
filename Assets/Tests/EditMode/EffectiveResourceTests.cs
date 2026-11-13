using System;
using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class EffectiveResourceTests
{
    private static HostingCatalog Catalog() => AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();

    [TestCase(0, 4, 4, 250, 10)]
    [TestCase(1, 2, 8, 250, 10)]
    [TestCase(2, 2, 4, 500, 10)]
    [TestCase(3, 2, 4, 250, 20)]
    public void EachUpgradeChangesOnlyItsResourceAndPreservesRecordedPrice(int index, double cpu, double ram, double storage, double bandwidth)
    {
        var catalog = Catalog();
        var upgrade = catalog.Upgrades[index];
        var record = new UpgradePurchase("pc-instance", upgrade.Id, upgrade.Price - 1);
        var hardware = new HardwareInstance(catalog.StarterPc, "pc-instance", new[] { record }, catalog.Upgrades);
        AssertAmounts(hardware.BaseCapacity, 2, 4, 250, 10);
        AssertAmounts(hardware.EffectiveCapacity, cpu, ram, storage, bandwidth);
        AssertAmounts(hardware.Resources.Capacity, cpu, ram, storage, bandwidth);
        Assert.That(hardware.PurchaseRecords.Single(), Is.SameAs(record));
        Assert.That(hardware.PurchaseRecords[0].AmountPaid, Is.EqualTo(upgrade.Price - 1));
        Assert.That(hardware.AppliedUpgradeIds.Single(), Is.EqualTo(upgrade.Id));
        Assert.That(hardware.Id, Is.EqualTo("pc-instance"));
        AssertAmounts(catalog.StarterPc.Capacity, 2, 4, 250, 10);
    }

    [Test]
    public void CombinedUpgradesAndMilestonesProduceEffectiveUsageWithoutMutatingDefinitions()
    {
        var catalog = Catalog();
        var records = catalog.Upgrades.Select(upgrade => new UpgradePurchase("pc", upgrade.Id, upgrade.Price)).ToArray();
        var hardware = new HardwareInstance(catalog.StarterPc, "pc", records, catalog.Upgrades);
        var applied = new[] { catalog.Milestones[0].Id };
        var photo = new ContractInstance(catalog.Offers[0], "photo-contract", 2, applied, catalog.Milestones, "pc");
        var website = new ContractInstance(catalog.Offers[1], "web-contract", 0, Array.Empty<string>(), catalog.Milestones, "pc");
        AssertAmounts(photo.BaseRequirements, 1, 2, 20, 4);
        AssertAmounts(photo.EffectiveRequirements, 2, 2, 20, 4);
        Assert.That(photo.AppliedMilestoneIds.Single(), Is.EqualTo("growth.photosnap.cpu-2"));
        Assert.That(photo.Definition.MonthlyRevenue, Is.EqualTo(100m));
        var usage = ProgressionResources.CalculateUsage(hardware, new[] { photo, website });
        AssertAmounts(usage.Capacity, 4, 8, 500, 20);
        AssertAmounts(usage.Used, 3, 3, 30, 8);
        AssertAmounts(usage.Available, 1, 5, 470, 12);
        records[0] = null;
        applied[0] = "edited";
        Assert.That(hardware.PurchaseRecords[0], Is.Not.Null);
        Assert.That(photo.AppliedMilestoneIds.Single(), Is.EqualTo(catalog.Milestones[0].Id));
        var basic = new HardwareInstance(catalog.StarterPc, "pc", Array.Empty<UpgradePurchase>(), catalog.Upgrades);
        var overloaded = ProgressionResources.CalculateUsage(basic, new[] { photo, website });
        Assert.That(overloaded.Used.CpuCores, Is.EqualTo(3));
        Assert.That(overloaded.Available.CpuCores, Is.Zero);
        Assert.That(photo.AssignedHardwareId, Is.EqualTo("pc"));
        AssertAmounts(catalog.Offers[0].Requirements, 1, 2, 20, 4);
    }

    [Test]
    public void FreshSessionsHaveNoProgressionAndStayIndependentFromConstructedState()
    {
        var catalog = Catalog();
        var first = new CompanySession(catalog);
        var second = new CompanySession(catalog);
        first.TryAcceptOffer("offer.photosnap", out var photo, out _);
        first.TryAssignContract(photo.Id, first.Hardware[0].Id, out _);
        var upgraded = new HardwareInstance(catalog.StarterPc, first.Hardware[0].Id,
            new[] { new UpgradePurchase(first.Hardware[0].Id, catalog.Upgrades[0].Id, 120m) }, catalog.Upgrades);
        var grown = new ContractInstance(photo.Definition, photo.Id, 2, new[] { catalog.Milestones[0].Id }, catalog.Milestones, upgraded.Id);
        Assert.That(first.Hardware[0].PurchaseRecords, Is.Empty);
        Assert.That(second.Hardware[0].AppliedUpgradeIds, Is.Empty);
        Assert.That(photo.AppliedMilestoneIds, Is.Empty);
        Assert.That(second.AcceptedContracts, Is.Empty);
        Assert.That(first.Cash, Is.EqualTo(150m));
        Assert.That(first.OperatingProfit, Is.Zero);
        Assert.That(first.Hardware[0].EffectiveCapacity.CpuCores, Is.EqualTo(2));
        Assert.That(photo.EffectiveRequirements.CpuCores, Is.EqualTo(1));
        Assert.That(grown.EffectiveRequirements.CpuCores, Is.EqualTo(2));
        Assert.That(first.Hardware[0].Id, Is.EqualTo(upgraded.Id));
        Assert.That(photo.Id, Is.EqualTo(grown.Id));
    }

    [Test]
    public void UsageCountsOnlyContractsAssignedToTheRequestedHardware()
    {
        var catalog = Catalog();
        var hardware = new HardwareInstance(catalog.StarterPc, "pc", Array.Empty<UpgradePurchase>(), catalog.Upgrades);
        var unassigned = new ContractInstance(catalog.Offers[0], "unassigned", 0, Array.Empty<string>(), catalog.Milestones);
        var elsewhere = new ContractInstance(catalog.Offers[1], "elsewhere", 0, Array.Empty<string>(), catalog.Milestones, "other-pc");
        AssertAmounts(ProgressionResources.CalculateUsage(hardware, new[] { unassigned, elsewhere }).Used, 0, 0, 0, 0);
    }

    [Test]
    public void AbsoluteProfilesUseTheLatestAppliedThresholdRegardlessOfIdOrder()
    {
        var offer = Catalog().Offers[0];
        var early = new GrowthMilestoneData("growth.early", "Early", offer.Id, 1, new ResourceAmounts(2, 2, 20, 4));
        var late = new GrowthMilestoneData("growth.late", "Late", offer.Id, 3, new ResourceAmounts(4, 3, 40, 8));
        foreach (var ids in new[] { new[] { early.Id, late.Id }, new[] { late.Id, early.Id } })
            AssertAmounts(new ContractInstance(offer, "c", 3, ids, new[] { early, late }).EffectiveRequirements, 4, 3, 40, 8);
        AssertAmounts(new ContractInstance(offer, "c", 10, Array.Empty<string>(), new[] { early, late }).EffectiveRequirements, 1, 2, 20, 4);
        var ambiguous = new GrowthMilestoneData("growth.same-time", "Other", offer.Id, 1, new ResourceAmounts(3, 2, 20, 4));
        Assert.Throws<ArgumentException>(() => new ContractInstance(offer, "c", 3, new[] { early.Id, late.Id, ambiguous.Id }, new[] { early, late, ambiguous }));
    }

    [Test]
    public void InvalidProgressionStateIsRejectedBeforeItCanAffectResources()
    {
        var catalog = Catalog();
        var valid = new UpgradePurchase("pc", catalog.Upgrades[0].Id, 120);
        Assert.Throws<ArgumentOutOfRangeException>(() => new UpgradePurchase("pc", "u", 0));
        Assert.Throws<ArgumentException>(() => new UpgradePurchase(" ", "u", 1));
        foreach (var records in new[] { new[] { valid, valid }, new[] { new UpgradePurchase("other", valid.UpgradeId, 120) }, new[] { new UpgradePurchase("pc", "unknown", 120) } })
            Assert.Throws<ArgumentException>(() => new HardwareInstance(catalog.StarterPc, "pc", records, catalog.Upgrades));
        Assert.Throws<ArgumentException>(() => new ContractInstance(catalog.Offers[0], "c", 2, new[] { "unknown" }, catalog.Milestones));
        Assert.Throws<ArgumentException>(() => new ContractInstance(catalog.Offers[1], "c", 2, new[] { catalog.Milestones[0].Id }, catalog.Milestones));
        Assert.Throws<ArgumentException>(() => new ContractInstance(catalog.Offers[0], "c", 1, new[] { catalog.Milestones[0].Id }, catalog.Milestones));
        Assert.Throws<ArgumentException>(() => new ContractInstance(catalog.Offers[0], "c", 2, new[] { catalog.Milestones[0].Id, catalog.Milestones[0].Id }, catalog.Milestones));
        var wrongTarget = new UpgradeData("wrong-target", "Wrong", "other-hardware-content", 10, new ResourceAmounts(1, 0, 0, 0));
        Assert.Throws<ArgumentException>(() => new HardwareInstance(catalog.StarterPc, "pc", new[] { new UpgradePurchase("pc", wrongTarget.Id, 10) }, new[] { wrongTarget }));
        var contract = new ContractInstance(catalog.Offers[0], "c", 0, Array.Empty<string>(), catalog.Milestones, "pc");
        var hardware = new HardwareInstance(catalog.StarterPc, "pc", Array.Empty<UpgradePurchase>(), catalog.Upgrades);
        Assert.Throws<ArgumentException>(() => ProgressionResources.CalculateUsage(hardware, new[] { contract, contract }));
    }

    private static void AssertAmounts(ResourceAmounts value, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(value.CpuCores, Is.EqualTo(cpu));
        Assert.That(value.RamGb, Is.EqualTo(ram));
        Assert.That(value.StorageGb, Is.EqualTo(storage));
        Assert.That(value.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
