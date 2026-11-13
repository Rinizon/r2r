using System;
using InfrastructureTycoon.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ProgressionCatalogTests
{
    private static HostingCatalog Catalog() => AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();
    private static UpgradeData Upgrade(string id = "upgrade.test", string target = "hardware.starter-pc") =>
        new UpgradeData(id, "Upgrade", target, 120m, new ResourceAmounts(2, 0, 0, 0));
    private static GrowthMilestoneData Milestone(string id = "growth.test", string target = "offer.photosnap", int months = 2) =>
        new GrowthMilestoneData(id, "Growth", target, months, new ResourceAmounts(2, 2, 20, 4));

    [Test]
    public void AuthoredContentMatchesPhase3ValuesWithoutChangingStarterOffers()
    {
        var catalog = Catalog();
        Assert.That(catalog.Upgrades.Count, Is.EqualTo(4));
        Assert.That(catalog.Milestones.Count, Is.EqualTo(1));
        decimal[] prices = { 120m, 80m, 100m, 90m };
        string[] ids = { "upgrade.starter-cpu", "upgrade.starter-ram", "upgrade.starter-storage", "upgrade.starter-internet" };
        for (int i = 0; i < 4; i++)
        {
            Assert.That(catalog.Upgrades[i].Id, Is.EqualTo(ids[i]));
            Assert.That(catalog.Upgrades[i].TargetHardwareId, Is.EqualTo(catalog.StarterPc.Id));
            Assert.That(catalog.Upgrades[i].Price, Is.EqualTo(prices[i]));
        }
        AssertResources(catalog.Upgrades[0].CapacityDelta, 2, 0, 0, 0);
        AssertResources(catalog.Upgrades[1].CapacityDelta, 0, 4, 0, 0);
        AssertResources(catalog.Upgrades[2].CapacityDelta, 0, 0, 250, 0);
        AssertResources(catalog.Upgrades[3].CapacityDelta, 0, 0, 0, 10);
        var growth = catalog.Milestones[0];
        Assert.That(growth.Id, Is.EqualTo("growth.photosnap.cpu-2"));
        Assert.That(growth.TargetOfferId, Is.EqualTo("offer.photosnap"));
        Assert.That(growth.RequiredPaidMonths, Is.EqualTo(2));
        AssertResources(growth.Requirements, 2, 2, 20, 4);
        AssertResources(catalog.Offers[0].Requirements, 1, 2, 20, 4);
        Assert.That(catalog.Offers[0].MonthlyRevenue, Is.EqualTo(100m));
        Assert.That(catalog.StarterPc.MonthlyExpense, Is.EqualTo(20m));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void MissingIdsAndTargetsAreRejected(string missing)
    {
        Assert.Throws<ArgumentException>(() => Upgrade(id: missing));
        Assert.Throws<ArgumentException>(() => Upgrade(target: missing));
        Assert.Throws<ArgumentException>(() => Milestone(id: missing));
        Assert.Throws<ArgumentException>(() => Milestone(target: missing));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void InvalidPriceAndThresholdAreRejected(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UpgradeData("u", "Upgrade", "pc", value, new ResourceAmounts(1, 0, 0, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Milestone(months: value));
    }

    [Test]
    public void ZeroEffectUpgradeIsRejected()
    {
        Assert.Throws<ArgumentException>(() => new UpgradeData("u", "Upgrade", "pc", 120, default));
    }

    [Test]
    public void CatalogRejectsUnknownTargetsAndIdsDuplicatedAcrossAllContentTypes()
    {
        var starter = Catalog();
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, new[] { Upgrade(target: "missing") }, Array.Empty<GrowthMilestoneData>()));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, Array.Empty<UpgradeData>(), new[] { Milestone(target: "missing") }));
        foreach (var collision in new[] { starter.StarterPc.Id, starter.Offers[0].Id })
        {
            Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, new[] { Upgrade(collision) }, Array.Empty<GrowthMilestoneData>()));
            Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, Array.Empty<UpgradeData>(), new[] { Milestone(collision) }));
        }
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, new[] { Upgrade(), Upgrade() }, Array.Empty<GrowthMilestoneData>()));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, Array.Empty<UpgradeData>(), new[] { Milestone(), Milestone() }));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, new[] { Upgrade("same") }, new[] { Milestone("same") }));
    }

    [Test]
    public void MissingProgressionListsOrEntriesAreRejected()
    {
        var starter = Catalog();
        Assert.Throws<ArgumentNullException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, null, Array.Empty<GrowthMilestoneData>()));
        Assert.Throws<ArgumentNullException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, Array.Empty<UpgradeData>(), null));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, new UpgradeData[] { null }, Array.Empty<GrowthMilestoneData>()));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(starter.StarterPc, starter.Offers, Array.Empty<UpgradeData>(), new GrowthMilestoneData[] { null }));
        var asset = AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset");
        foreach (string list in new[] { "upgrades", "milestones" })
        {
            var copy = UnityEngine.Object.Instantiate(asset);
            try
            {
                var serialized = new SerializedObject(copy);
                serialized.FindProperty(list).GetArrayElementAtIndex(0).objectReferenceValue = null;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<ArgumentException>(() => copy.ToRuntime());
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
    }

    [Test]
    public void RuntimeCatalogCopiesListsAndLegacyConstructorStillWorks()
    {
        var starter = Catalog();
        var upgrades = new[] { Upgrade() };
        var milestones = new[] { Milestone() };
        var runtime = new HostingCatalog(starter.StarterPc, starter.Offers, upgrades, milestones);
        upgrades[0] = null;
        milestones[0] = null;
        Assert.That(runtime.Upgrades[0], Is.Not.Null);
        Assert.That(runtime.Milestones[0], Is.Not.Null);
        var legacy = new HostingCatalog(starter.StarterPc, starter.Offers);
        Assert.That(legacy.Upgrades, Is.Empty);
        Assert.That(legacy.Milestones, Is.Empty);
    }

    [TestCase("Assets/Data/CpuUpgrade.asset", "capacityDelta", "priceDollars")]
    [TestCase("Assets/Data/PhotoSnapGrowth.asset", "requirements", "requiredPaidMonths")]
    public void AuthoredConversionValidatesValuesAndCreatesIndependentSnapshots(string path, string resources, string positiveField)
    {
        var original = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        Assert.That(original, Is.Not.Null);
        var copy = UnityEngine.Object.Instantiate(original);
        try
        {
            copy.name = "Renamed definition";
            string originalId = original is UpgradeDefinition originalUpgrade ? originalUpgrade.ToRuntime().Id : ((GrowthMilestoneDefinition)original).ToRuntime().Id;
            string copiedId = copy is UpgradeDefinition copiedUpgrade ? copiedUpgrade.ToRuntime().Id : ((GrowthMilestoneDefinition)copy).ToRuntime().Id;
            Assert.That(copiedId, Is.EqualTo(originalId));
            var serialized = new SerializedObject(copy);
            var before = Convert(copy);
            serialized.FindProperty(resources).FindPropertyRelative("cpuCores").doubleValue = 99;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(before.CpuCores, Is.EqualTo(2));
            Assert.That(Convert(copy).CpuCores, Is.EqualTo(99));
            Assert.That(Convert(original).CpuCores, Is.EqualTo(2));
            serialized.FindProperty(positiveField).intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentOutOfRangeException>(() => Convert(copy));
            serialized.FindProperty(positiveField).intValue = 2;
            serialized.FindProperty(resources).FindPropertyRelative("cpuCores").doubleValue = -1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentOutOfRangeException>(() => Convert(copy));
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }

    private static ResourceAmounts Convert(ScriptableObject value) => value is UpgradeDefinition upgrade ? upgrade.ToRuntime().CapacityDelta : ((GrowthMilestoneDefinition)value).ToRuntime().Requirements;
    private static void AssertResources(ResourceAmounts value, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(value.CpuCores, Is.EqualTo(cpu));
        Assert.That(value.RamGb, Is.EqualTo(ram));
        Assert.That(value.StorageGb, Is.EqualTo(storage));
        Assert.That(value.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
