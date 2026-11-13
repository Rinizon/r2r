using System;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class CompanySessionTests
{
    private static HostingCatalog Catalog() => AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime();

    [Test]
    public void NewSessionHasStarterFinancesClockAndUnusedPc()
    {
        var catalog = Catalog();
        var session = new CompanySession(catalog);
        Assert.That(session.Catalog, Is.SameAs(catalog));
        Assert.That(session.Clock.Date, Is.EqualTo(new DateTime(1995, 1, 1)));
        Assert.That(session.Clock.Speed, Is.EqualTo(SimulationSpeed.Normal));
        Assert.That(session.Clock.DayProgress, Is.Zero);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
        Assert.That(session.AcceptedContracts, Is.Empty);
        Assert.That(session.Hardware.Count, Is.EqualTo(1));
        var pc = session.Hardware[0];
        Assert.That(pc.Definition, Is.SameAs(catalog.StarterPc));
        Assert.That(pc.Id, Is.Not.Empty.And.Not.EqualTo(pc.Definition.Id));
        AssertAmounts(pc.Resources.Capacity, 2, 4, 250, 10);
        AssertAmounts(pc.Resources.Used, 0, 0, 0, 0);
        AssertAmounts(pc.Resources.Available, 2, 4, 250, 10);
    }

    [Test]
    public void SessionsHaveIndependentClocksAndHardwareWithStableInstanceIds()
    {
        var catalog = Catalog();
        var first = new CompanySession(catalog);
        var second = new CompanySession(catalog);
        string id = first.Hardware[0].Id;
        first.Clock.Advance(0.5);
        first.Clock.Speed = SimulationSpeed.Octuple;
        Assert.That(second.Clock.DayProgress, Is.Zero);
        Assert.That(second.Clock.Speed, Is.EqualTo(SimulationSpeed.Normal));
        Assert.That(first.Hardware[0], Is.Not.SameAs(second.Hardware[0]));
        Assert.That(second.Hardware[0].Id, Is.Not.EqualTo(id));
        Assert.That(first.Hardware[0].Id, Is.EqualTo(id));
        AssertAmounts(catalog.StarterPc.Capacity, 2, 4, 250, 10);
        Assert.That(catalog.Offers.Count, Is.EqualTo(2));
        Assert.That(second.Cash, Is.EqualTo(150m));
        Assert.That(second.AcceptedContracts, Is.Empty);
    }

    [Test]
    public void ResourceArithmeticKeepsAllFourUnitsSeparate()
    {
        var first = new ResourceAmounts(1, 2, 20, 4);
        var second = new ResourceAmounts(1, 1, 10, 4);
        var combined = first + second;
        AssertAmounts(combined, 2, 3, 30, 8);
        AssertAmounts(combined - second, 1, 2, 20, 4);
        var usage = new ResourceUsage(Catalog().StarterPc.Capacity, combined);
        AssertAmounts(usage.Available, 0, 1, 220, 2);
        AssertAmounts(first, 1, 2, 20, 4);
    }

    [Test]
    public void AvailableCapacityIsZeroForAnOverusedDimension()
    {
        var usage = new ResourceUsage(new ResourceAmounts(2, 4, 250, 10), new ResourceAmounts(3, 5, 251, 11));
        AssertAmounts(usage.Used, 3, 5, 251, 11);
        AssertAmounts(usage.Available, 0, 0, 0, 0);
    }

    [Test]
    public void InvalidResourceArithmeticAndMissingCatalogAreRejected()
    {
        var one = new ResourceAmounts(1, 1, 1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => { var result = default(ResourceAmounts) - one; });
        var large = new ResourceAmounts(double.MaxValue, 0, 0, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => { var result = large + large; });
        Assert.Throws<ArgumentNullException>(() => new CompanySession(null));
    }

    private static void AssertAmounts(ResourceAmounts value, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(value.CpuCores, Is.EqualTo(cpu));
        Assert.That(value.RamGb, Is.EqualTo(ram));
        Assert.That(value.StorageGb, Is.EqualTo(storage));
        Assert.That(value.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
