using System;
using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class BottleneckTests
{
    [TestCase(ResourceKind.Cpu, 3, 1, 1, 1, "cores")]
    [TestCase(ResourceKind.Ram, 1, 3, 1, 1, "GB")]
    [TestCase(ResourceKind.Storage, 1, 1, 3, 1, "GB")]
    [TestCase(ResourceKind.Bandwidth, 1, 1, 1, 3, "Mbps")]
    public void EachResourceReportsDemandCapacityShortfallAndIdentities(ResourceKind kind, double cpu, double ram, double storage, double bandwidth, string unit)
    {
        var hardware = new HardwareInstance(new HardwareData("pc-content", "PC", new ResourceAmounts(2, 2, 2, 2), 20), "pc-runtime", Array.Empty<UpgradePurchase>(), Array.Empty<UpgradeData>());
        var offer = new OfferData("offer", "Customer", new ResourceAmounts(1, 1, 1, 1), 100);
        var growth = new GrowthMilestoneData("growth", "Growth", offer.Id, 2, new ResourceAmounts(cpu, ram, storage, bandwidth));
        var hosted = new ContractInstance(offer, "contract-runtime", 2, new[] { growth.Id }, new[] { growth }, hardware.Id);
        var unassigned = new ContractInstance(offer, "unassigned", 0, Array.Empty<string>(), new[] { growth });
        var elsewhere = new ContractInstance(offer, "elsewhere", 0, Array.Empty<string>(), new[] { growth }, "other-pc");
        var diagnostic = ResourceBottlenecks.Diagnose(hardware, new[] { hosted, unassigned, elsewhere }).Single();
        Assert.That(diagnostic.HardwareId, Is.EqualTo(hardware.Id));
        Assert.That(diagnostic.HardwareContentId, Is.EqualTo(hardware.Definition.Id));
        Assert.That(diagnostic.HardwareName, Is.EqualTo("PC"));
        Assert.That(diagnostic.Resource, Is.EqualTo(kind));
        Assert.That(diagnostic.Unit, Is.EqualTo(unit));
        Assert.That(diagnostic.Demand, Is.EqualTo(3));
        Assert.That(diagnostic.Capacity, Is.EqualTo(2));
        Assert.That(diagnostic.Shortfall, Is.EqualTo(1));
        Assert.That(diagnostic.AffectedContracts.Single().Id, Is.EqualTo(hosted.Id));
        Assert.That(diagnostic.AffectedContracts[0].ContentId, Is.EqualTo(offer.Id));
        Assert.That(diagnostic.AffectedContracts[0].DisplayName, Is.EqualTo("Customer"));
    }

    [Test]
    public void AllConstraintsAppearInStableOrderAndEqualityIsNotAnOverload()
    {
        var hardware = new HardwareInstance(new HardwareData("pc", "PC", new ResourceAmounts(2, 2, 2, 2), 20));
        var offer = new OfferData("web", "Web", new ResourceAmounts(2, 2, 2, 2), 100);
        var fitting = new ContractInstance(offer, "contract", 0, Array.Empty<string>(), Array.Empty<GrowthMilestoneData>(), hardware.Id);
        Assert.That(ResourceBottlenecks.Diagnose(hardware, new[] { fitting }), Is.Empty);
        Assert.That(ResourceBottlenecks.Diagnose(hardware, Array.Empty<ContractInstance>()), Is.Empty);
        var growth = new GrowthMilestoneData("growth", "Growth", offer.Id, 2, new ResourceAmounts(3, 4, 5, 6));
        var grown = new ContractInstance(offer, "grown", 2, new[] { growth.Id }, new[] { growth }, hardware.Id);
        var reports = ResourceBottlenecks.Diagnose(hardware, new[] { grown });
        CollectionAssert.AreEqual(new[] { ResourceKind.Cpu, ResourceKind.Ram, ResourceKind.Storage, ResourceKind.Bandwidth }, reports.Select(x => x.Resource));
        CollectionAssert.AreEqual(new[] { 1d, 2d, 3d, 4d }, reports.Select(x => x.Shortfall));
    }

    [Test]
    public void GrowthAlertsSurviveReloadAndClearOnlyWithRelevantUpgradeWithoutSideEffects()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();
        var source = new CompanySession(catalog);
        foreach (var offer in catalog.Offers)
        {
            source.TryAcceptOffer(offer.Id, out var contract, out _);
            source.TryAssignContract(contract.Id, source.Hardware[0].Id, out _);
        }
        Assert.That(source.GetBottlenecks(), Is.Empty);
        source.Clock.Advance(118);
        string before = Snapshot(source);
        var first = source.GetBottlenecks().Single();
        Assert.That(first.Demand, Is.EqualTo(3));
        Assert.That(first.Capacity, Is.EqualTo(2));
        CollectionAssert.AreEqual(source.AcceptedContracts.Select(x => x.Id), first.AffectedContracts.Select(x => x.Id));
        source.GetBottlenecks();
        Assert.That(Snapshot(source), Is.EqualTo(before), "Diagnostics must not mutate or become save state.");
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(source, "test")), catalog, out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.GetBottlenecks().Single().Resource, Is.EqualTo(ResourceKind.Cpu));
        loaded.TryPurchaseUpgrade("upgrade.starter-ram", loaded.Hardware[0].Id, out _);
        Assert.That(loaded.GetBottlenecks().Single().Resource, Is.EqualTo(ResourceKind.Cpu));
        loaded.TryPurchaseUpgrade("upgrade.starter-cpu", loaded.Hardware[0].Id, out _);
        string after = Snapshot(loaded);
        Assert.That(loaded.GetBottlenecks(), Is.Empty);
        Assert.That(loaded.Hardware[0].Resources.Used.CpuCores, Is.EqualTo(3));
        Assert.That(loaded.Hardware[0].Resources.Capacity.CpuCores, Is.EqualTo(4));
        Assert.That(Snapshot(loaded), Is.EqualTo(after));
        Assert.That(first.Capacity, Is.EqualTo(2), "Returned diagnostics are historical snapshots.");
        Assert.That(source.GetBottlenecks().Single().Capacity, Is.EqualTo(2), "Separate sessions remain independent.");
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(loaded, "test")), catalog, out var upgraded, out reason), Is.True, reason);
        Assert.That(upgraded.GetBottlenecks(), Is.Empty);
    }

    private static string Snapshot(CompanySession session)
    {
        var data = SessionSaveJson.Capture(session, "test");
        data.Timestamp = "fixed";
        return SessionSaveJson.ToJson(data);
    }
}
