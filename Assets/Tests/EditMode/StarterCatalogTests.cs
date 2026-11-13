using System;
using InfrastructureTycoon.Content;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class StarterCatalogTests
{
    private const string CatalogPath = "Assets/Data/StarterCatalog.asset";

    [Test]
    public void AuthoredCatalogContainsTheGddStarterValues()
    {
        var asset = AssetDatabase.LoadAssetAtPath<StarterCatalog>(CatalogPath);
        Assert.That(asset, Is.Not.Null);
        var catalog = asset.ToRuntime();
        Assert.That(catalog.StarterPc.Id, Is.EqualTo("hardware.starter-pc"));
        AssertResources(catalog.StarterPc.Capacity, 2, 4, 250, 10);
        Assert.That(catalog.StarterPc.MonthlyExpense, Is.EqualTo(20m));
        Assert.That(catalog.Offers.Count, Is.EqualTo(2));
        Assert.That(catalog.Offers[0].Id, Is.EqualTo("offer.photosnap"));
        AssertResources(catalog.Offers[0].Requirements, 1, 2, 20, 4);
        Assert.That(catalog.Offers[0].MonthlyRevenue, Is.EqualTo(100m));
        Assert.That(catalog.Offers[1].Id, Is.EqualTo("offer.local-website"));
        AssertResources(catalog.Offers[1].Requirements, 1, 1, 10, 4);
        Assert.That(catalog.Offers[1].MonthlyRevenue, Is.EqualTo(80m));
    }

    [Test]
    public void AssetNamesAndCopiesDoNotChangeContentIdsOrRuntimeData()
    {
        var asset = AssetDatabase.LoadAssetAtPath<HardwareDefinition>("Assets/Data/StarterPc.asset");
        Assert.That(asset, Is.Not.Null);
        var copy = UnityEngine.Object.Instantiate(asset);
        try
        {
            copy.name = "Renamed copy";
            var before = copy.ToRuntime();
            var serialized = new SerializedObject(copy);
            serialized.FindProperty("capacity").FindPropertyRelative("cpuCores").doubleValue = 99;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(before.Id, Is.EqualTo(asset.ToRuntime().Id));
            Assert.That(before.Capacity.CpuCores, Is.EqualTo(2));
            Assert.That(copy.ToRuntime().Capacity.CpuCores, Is.EqualTo(99));
            Assert.That(asset.ToRuntime().Capacity.CpuCores, Is.EqualTo(2));
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null)]
    public void MissingContentIdsAreRejected(string id)
    {
        Assert.Throws<ArgumentException>(() => new HardwareData(id, "PC", Resources(), 20m));
        Assert.Throws<ArgumentException>(() => new OfferData(id, "Website", Resources(), 100m));
    }

    [Test]
    public void DuplicateIdsAndMissingDefinitionsAreRejected()
    {
        var pc = new HardwareData("pc", "PC", Resources(), 20m);
        var offer = new OfferData("web", "Website", Resources(), 100m);
        Assert.Throws<ArgumentException>(() => new HostingCatalog(pc, new[] { offer, offer }));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(pc, new[] { new OfferData("pc", "Website", Resources(), 100m) }));
        Assert.Throws<ArgumentException>(() => new HostingCatalog(pc, new OfferData[] { null }));
        Assert.Throws<ArgumentNullException>(() => new HostingCatalog(null, new[] { offer }));
        Assert.Throws<ArgumentNullException>(() => new HostingCatalog(pc, null));
    }

    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    public void InvalidResourcesAreRejectedInEveryDimension(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceAmounts(value, 1, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceAmounts(1, value, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceAmounts(1, 1, value, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceAmounts(1, 1, 1, value));
    }

    [Test]
    public void NegativeMoneyIsRejectedAndZeroAmountsAreAllowed()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HardwareData("pc", "PC", Resources(), -1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OfferData("web", "Website", Resources(), -1m));
        Assert.That(new HardwareData("pc", "PC", default, 0m).MonthlyExpense, Is.Zero);
        Assert.That(new OfferData("web", "Website", default, 0m).MonthlyRevenue, Is.Zero);
    }

    [Test]
    public void RuntimeCatalogOwnsItsOfferList()
    {
        var offer = new OfferData("web", "Website", Resources(), 100m);
        var source = new[] { offer };
        var catalog = new HostingCatalog(new HardwareData("pc", "PC", Resources(), 20m), source);
        source[0] = null;
        Assert.That(catalog.Offers[0], Is.SameAs(offer));
    }

    [Test]
    public void AuthoredCatalogRejectsMissingAssetReferences()
    {
        var empty = ScriptableObject.CreateInstance<StarterCatalog>();
        try { Assert.Throws<ArgumentException>(() => empty.ToRuntime()); }
        finally { UnityEngine.Object.DestroyImmediate(empty); }
    }

    [TestCase("Assets/Data/StarterPc.asset", "capacity", "monthlyExpenseDollars")]
    [TestCase("Assets/Data/PhotoSnap.asset", "requirements", "monthlyRevenueDollars")]
    public void InvalidAuthoredValuesAreRejectedDuringConversion(string path, string resources, string money)
    {
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        Assert.That(asset, Is.Not.Null);
        var copy = UnityEngine.Object.Instantiate(asset);
        try
        {
            var serialized = new SerializedObject(copy);
            string id = serialized.FindProperty("contentId").stringValue;
            serialized.FindProperty("contentId").stringValue = " ";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentException>(() => Convert(copy));
            serialized.FindProperty("contentId").stringValue = id;
            serialized.FindProperty(money).intValue = -1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentOutOfRangeException>(() => Convert(copy));
            serialized.FindProperty(money).intValue = 0;
            serialized.FindProperty(resources).FindPropertyRelative("cpuCores").doubleValue = -1;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentOutOfRangeException>(() => Convert(copy));
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }

    [Test]
    public void AuthoredCatalogRejectsDuplicateAndMissingOfferReferences()
    {
        var original = AssetDatabase.LoadAssetAtPath<StarterCatalog>(CatalogPath);
        Assert.That(original, Is.Not.Null);
        var copy = UnityEngine.Object.Instantiate(original);
        try
        {
            var serialized = new SerializedObject(copy);
            var offers = serialized.FindProperty("websiteOffers");
            offers.GetArrayElementAtIndex(1).objectReferenceValue = offers.GetArrayElementAtIndex(0).objectReferenceValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentException>(() => copy.ToRuntime());
            offers.GetArrayElementAtIndex(1).objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.Throws<ArgumentException>(() => copy.ToRuntime());
            Assert.That(original.ToRuntime().Offers.Count, Is.EqualTo(2));
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }

    private static void Convert(ScriptableObject asset)
    {
        if (asset is HardwareDefinition hardware)
            hardware.ToRuntime();
        else
            ((ContractTemplate)asset).ToRuntime();
    }

    private static ResourceAmounts Resources() => new ResourceAmounts(1, 1, 1, 1);

    private static void AssertResources(ResourceAmounts resources, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(resources.CpuCores, Is.EqualTo(cpu));
        Assert.That(resources.RamGb, Is.EqualTo(ram));
        Assert.That(resources.StorageGb, Is.EqualTo(storage));
        Assert.That(resources.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
