using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class HostingOperationsTests
{
    private static CompanySession Starter() => new CompanySession(
        AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime());

    [TestCase("offer.photosnap", 1, 2, 20, 4)]
    [TestCase("offer.local-website", 1, 1, 10, 4)]
    public void OfferAcceptanceAndAssignmentAreSeparate(string offerId, double cpu, double ram, double storage, double bandwidth)
    {
        var session = Starter();
        Assert.That(session.TryAcceptOffer(offerId, out var contract, out var reason), Is.True, reason);
        Assert.That(contract.Id, Is.Not.Empty.And.Not.EqualTo(offerId));
        Assert.That(contract.Definition.Id, Is.EqualTo(offerId));
        Assert.That(session.AcceptedContracts.Single(), Is.SameAs(contract));
        Assert.That(session.AvailableOffers.Select(x => x.Id), Does.Not.Contain(offerId));
        Assert.That(contract.IsHosted, Is.False);
        Assert.That(contract.AssignedHardwareId, Is.Null);
        AssertAmounts(session.Hardware[0].Resources.Used, 0, 0, 0, 0);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.TryAssignContract(contract.Id, session.Hardware[0].Id, out reason), Is.True, reason);
        Assert.That(contract.IsHosted, Is.True);
        Assert.That(contract.AssignedHardwareId, Is.EqualTo(session.Hardware[0].Id));
        AssertAmounts(session.Hardware[0].Resources.Used, cpu, ram, storage, bandwidth);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
    }

    [Test]
    public void BothStarterOffersFitExactlyOnceWithoutMutatingSharedContentOrAnotherSession()
    {
        var first = Starter();
        var other = new CompanySession(first.Catalog);
        foreach (var offer in first.Catalog.Offers)
        {
            Assert.That(first.TryAcceptOffer(offer.Id, out var contract, out var reason), Is.True, reason);
            Assert.That(first.TryAssignContract(contract.Id, first.Hardware[0].Id, out reason), Is.True, reason);
        }
        Assert.That(first.AcceptedContracts.Select(x => x.Id).Distinct().Count(), Is.EqualTo(2));
        Assert.That(first.AvailableOffers, Is.Empty);
        AssertAmounts(first.Hardware[0].Resources.Used, 2, 3, 30, 8);
        AssertAmounts(first.Hardware[0].Resources.Available, 0, 1, 220, 2);
        Assert.That(other.AcceptedContracts, Is.Empty);
        Assert.That(other.AvailableOffers.Count, Is.EqualTo(2));
        AssertAmounts(other.Hardware[0].Resources.Used, 0, 0, 0, 0);
        AssertAmounts(first.Catalog.StarterPc.Capacity, 2, 4, 250, 10);
        Assert.That(first.Cash, Is.EqualTo(150m));
    }

    [Test]
    public void DuplicateOperationsPreserveContractsAssignmentsAndFinances()
    {
        var session = Starter();
        session.TryAcceptOffer("offer.photosnap", out var contract, out _);
        Assert.That(session.TryAcceptOffer("offer.photosnap", out var duplicate, out var reason), Is.False);
        Assert.That(duplicate, Is.Null);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.AcceptedContracts.Single(), Is.SameAs(contract));
        Assert.That(session.TryAssignContract(contract.Id, session.Hardware[0].Id, out reason), Is.True, reason);
        Assert.That(session.TryAssignContract(contract.Id, session.Hardware[0].Id, out reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(contract.AssignedHardwareId, Is.EqualTo(session.Hardware[0].Id));
        AssertAmounts(session.Hardware[0].Resources.Used, 1, 2, 20, 4);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("missing")]
    public void UnknownIdsAreRejectedWithoutChangingState(string unknown)
    {
        var session = Starter();
        Assert.That(session.TryAcceptOffer(unknown, out var missing, out var reason), Is.False);
        Assert.That(missing, Is.Null);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.AcceptedContracts, Is.Empty);
        session.TryAcceptOffer("offer.photosnap", out var contract, out _);
        Assert.That(session.TryAssignContract(unknown, session.Hardware[0].Id, out reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.TryAssignContract(contract.Id, unknown, out reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(contract.IsHosted, Is.False);
        Assert.That(session.AcceptedContracts.Single(), Is.SameAs(contract));
        Assert.That(session.AvailableOffers.Count, Is.EqualTo(1));
        AssertAmounts(session.Hardware[0].Resources.Used, 0, 0, 0, 0);
        Assert.That(session.Cash, Is.EqualTo(150m));
    }

    [TestCase(2, 1, 1, 1, "CPU")]
    [TestCase(1, 2, 1, 1, "RAM")]
    [TestCase(1, 1, 2, 1, "storage")]
    [TestCase(1, 1, 1, 2, "bandwidth")]
    public void CumulativeOverCapacityAssignmentIsRejectedAtomically(double cpu, double ram, double storage, double bandwidth, string resource)
    {
        var capacity = new ResourceAmounts(2, 2, 2, 2);
        var initial = new OfferData("first", "First", new ResourceAmounts(1, 1, 1, 1), 10m);
        var extra = new OfferData("extra", "Extra", new ResourceAmounts(cpu, ram, storage, bandwidth), 20m);
        var session = new CompanySession(new HostingCatalog(new HardwareData("pc", "PC", capacity, 20m), new[] { initial, extra }));
        session.TryAcceptOffer("first", out var hosted, out _);
        session.TryAssignContract(hosted.Id, session.Hardware[0].Id, out _);
        session.TryAcceptOffer("extra", out var unassigned, out _);
        Assert.That(session.TryAssignContract(unassigned.Id, session.Hardware[0].Id, out var reason), Is.False);
        Assert.That(reason.ToLowerInvariant(), Does.Contain(resource.ToLowerInvariant()));
        Assert.That(unassigned.IsHosted, Is.False);
        Assert.That(unassigned.AssignedHardwareId, Is.Null);
        Assert.That(hosted.AssignedHardwareId, Is.EqualTo(session.Hardware[0].Id));
        Assert.That(session.AcceptedContracts.Count, Is.EqualTo(2));
        AssertAmounts(session.Hardware[0].Resources.Used, 1, 1, 1, 1);
        AssertAmounts(session.Hardware[0].Resources.Available, 1, 1, 1, 1);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
    }

    [Test]
    public void RuntimeIdsFromAnotherSessionCannotBeAssigned()
    {
        var session = Starter();
        var other = new CompanySession(session.Catalog);
        session.TryAcceptOffer("offer.photosnap", out var own, out _);
        other.TryAcceptOffer("offer.photosnap", out var foreign, out _);
        Assert.That(session.TryAssignContract(foreign.Id, session.Hardware[0].Id, out var reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.TryAssignContract(own.Id, other.Hardware[0].Id, out reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(own.IsHosted, Is.False);
        Assert.That(foreign.IsHosted, Is.False);
        AssertAmounts(session.Hardware[0].Resources.Used, 0, 0, 0, 0);
    }

    private static void AssertAmounts(ResourceAmounts value, double cpu, double ram, double storage, double bandwidth)
    {
        Assert.That(value.CpuCores, Is.EqualTo(cpu));
        Assert.That(value.RamGb, Is.EqualTo(ram));
        Assert.That(value.StorageGb, Is.EqualTo(storage));
        Assert.That(value.BandwidthMbps, Is.EqualTo(bandwidth));
    }
}
