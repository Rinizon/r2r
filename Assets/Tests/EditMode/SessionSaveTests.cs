using System;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class SessionSaveTests
{
    private static CompanySession Starter() => new CompanySession(AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime());
    private static CompanySession Hosted()
    {
        var session = Starter();
        session.TryAcceptOffer("offer.photosnap", out var photo, out _);
        session.TryAssignContract(photo.Id, session.Hardware[0].Id, out _);
        session.TryAcceptOffer("offer.local-website", out _, out _);
        return session;
    }

    [Test]
    public void RoundTripBeforeAndAfterBillingPreservesStateAndPaysExactlyOnce()
    {
        var session = Hosted();
        session.Clock.Advance(61.5);
        session.Clock.Speed = SimulationSpeed.Quadruple;
        var data = SessionSaveJson.Capture(session, "0.1-test");
        var json = SessionSaveJson.ToJson(data);
        Assert.That(json, Does.Contain("\"save_version\":" + SessionSaveJson.CurrentVersion).And.Contain("\"game_version\":\"0.1-test\"").And.Contain("\"timestamp\""));
        Assert.That(SessionSaveJson.TryLoad(json, session.Catalog, out var restored, out var reason), Is.True, reason);
        Assert.That(restored.Clock.Date, Is.EqualTo(session.Clock.Date));
        Assert.That(restored.Clock.DayProgress, Is.EqualTo(0.75));
        Assert.That(restored.Clock.Speed, Is.EqualTo(SimulationSpeed.Quadruple));
        Assert.That(restored.Hardware[0].Id, Is.EqualTo(session.Hardware[0].Id));
        Assert.That(restored.Hardware[0].Resources.Used.StorageGb, Is.EqualTo(20));
        Assert.That(restored.AcceptedContracts[0].Id, Is.EqualTo(session.AcceptedContracts[0].Id));
        Assert.That(restored.AcceptedContracts[0].AssignedHardwareId, Is.EqualTo(restored.Hardware[0].Id));
        Assert.That(restored.AcceptedContracts[1].IsHosted, Is.False);
        restored.Clock.Advance(0.125);
        Assert.That(restored.Cash, Is.EqualTo(230m));
        Assert.That(restored.OperatingProfit, Is.EqualTo(80m));
        Assert.That(restored.AcceptedContracts[0].PaidMonths, Is.EqualTo(1));
        Assert.That(restored.AcceptedContracts[1].PaidMonths, Is.Zero);
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(restored, "0.1-test")), session.Catalog, out var after, out reason), Is.True, reason);
        Assert.That(after.TryBillMonth(new DateTime(1995, 1, 31), out _), Is.False);
        Assert.That(after.Cash, Is.EqualTo(230m));
        Assert.That(after.LastMonthlyResult.Revenue, Is.EqualTo(100m));
        after.Clock.Advance(14); // 28 days at 4x.
        Assert.That(after.Cash, Is.EqualTo(310m));
        Assert.That(after.BillingHistory.Count, Is.EqualTo(2));
        Assert.That(after.AcceptedContracts[0].PaidMonths, Is.EqualTo(2));
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.Clock.DayProgress, Is.EqualTo(0.75));
    }

    [Test]
    public void RestoredResourcesUseTheCatalogAndRejectDemandThatNoLongerFits()
    {
        var source = Hosted();
        var json = SessionSaveJson.ToJson(SessionSaveJson.Capture(source, "test"));
        var updated = new HostingCatalog(
            new HardwareData("hardware.starter-pc", "PC", new ResourceAmounts(4, 8, 500, 20), 20),
            new[] { new OfferData("offer.photosnap", "PhotoSnap", new ResourceAmounts(1, 3, 25, 5), 100), source.Catalog.Offers[1] });
        Assert.That(SessionSaveJson.TryLoad(json, updated, out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.Hardware[0].Resources.Capacity.RamGb, Is.EqualTo(8));
        Assert.That(loaded.Hardware[0].Resources.Used.RamGb, Is.EqualTo(3));
        Assert.That(loaded.Hardware[0].Resources.Used.StorageGb, Is.EqualTo(25));
        var tooSmall = new HostingCatalog(new HardwareData("hardware.starter-pc", "PC", new ResourceAmounts(1, 1, 1, 1), 20), updated.Offers);
        Assert.That(SessionSaveJson.TryLoad(json, tooSmall, out loaded, out reason), Is.False);
        Assert.That(loaded, Is.Null);
        Assert.That(source.Hardware[0].Resources.Used.RamGb, Is.EqualTo(2));
    }

    [TestCase(SimulationSpeed.Paused)]
    [TestCase(SimulationSpeed.Normal)]
    [TestCase(SimulationSpeed.Double)]
    [TestCase(SimulationSpeed.Quadruple)]
    [TestCase(SimulationSpeed.Octuple)]
    public void FreshSessionRoundTripsAllSelectedSpeeds(SimulationSpeed speed)
    {
        var source = Starter();
        source.Clock.Speed = speed;
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(SessionSaveJson.Capture(source, "test")), source.Catalog, out var loaded, out var reason), Is.True, reason);
        Assert.That(loaded.Clock.Speed, Is.EqualTo(speed));
        Assert.That(loaded.AcceptedContracts, Is.Empty);
        Assert.That(loaded.BillingHistory, Is.Empty);
        Assert.That(loaded.Cash, Is.EqualTo(150m));
    }

    [TestCase(" garbage")]
    [TestCase("{}")]
    public void TrailingJsonContentIsRejected(string suffix)
    {
        var source = Starter();
        var json = SessionSaveJson.ToJson(SessionSaveJson.Capture(source, "test")) + suffix;
        Assert.That(SessionSaveJson.TryLoad(json, source.Catalog, out _, out var reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
    }

    [Test]
    public void CapturesAreIndependentAndRejectedDuringBoundaryProcessing()
    {
        var session = Hosted();
        var snapshot = SessionSaveJson.Capture(session, "test");
        snapshot.Contracts[0].Id = "edited";
        Assert.That(session.AcceptedContracts[0].Id, Is.Not.EqualTo("edited"));
        int rejected = 0;
        session.Clock.MonthEnded += _ =>
        {
            Assert.Throws<InvalidOperationException>(() => SessionSaveJson.Capture(session, "test"));
            rejected++;
        };
        session.Clock.Advance(118);
        Assert.That(rejected, Is.EqualTo(2));
        Assert.That(SessionSaveJson.Capture(session, "test").BillingHistory.Length, Is.EqualTo(2));
    }

    [TestCase("version")]
    [TestCase("date")]
    [TestCase("progress")]
    [TestCase("speed")]
    [TestCase("content")]
    [TestCase("hardware-content")]
    [TestCase("duplicate-id")]
    [TestCase("duplicate-offer")]
    [TestCase("assignment")]
    [TestCase("paid-months")]
    [TestCase("billing")]
    [TestCase("cash")]
    [TestCase("missing-hardware")]
    [TestCase("missing-contracts")]
    [TestCase("timestamp")]
    public void InvalidSaveIsRejectedWithoutChangingALiveSession(string fault)
    {
        var live = Hosted();
        live.Clock.Advance(62);
        var data = SessionSaveJson.Capture(live, "test");
        switch (fault)
        {
            case "version": data.SaveVersion = 99; break;
            case "date": data.Date = "1995-02-30"; break;
            case "progress": data.DayProgress = 1; break;
            case "speed": data.Speed = 3; break;
            case "content": data.Contracts[0].ContentId = "unknown"; break;
            case "hardware-content": data.Hardware[0].ContentId = "unknown"; break;
            case "duplicate-id": data.Contracts[0].Id = data.Hardware[0].Id; break;
            case "duplicate-offer": data.Contracts[1].ContentId = data.Contracts[0].ContentId; break;
            case "assignment": data.Contracts[0].AssignedHardwareId = "missing"; break;
            case "paid-months": data.Contracts[0].PaidMonths = 2; break;
            case "billing": data.BillingHistory = Array.Empty<BillingSaveData>(); break;
            case "cash": data.Cash = 999; break;
            case "missing-hardware": data.Hardware = null; break;
            case "missing-contracts": data.Contracts = null; break;
            case "timestamp": data.Timestamp = "invalid"; break;
        }
        Assert.That(SessionSaveJson.TryLoad(SessionSaveJson.ToJson(data), live.Catalog, out var loaded, out var reason), Is.False);
        Assert.That(loaded, Is.Null);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(live.Cash, Is.EqualTo(230m));
        Assert.That(live.BillingHistory.Count, Is.EqualTo(1));
        Assert.That(live.AcceptedContracts[0].IsHosted, Is.True);
    }

    [TestCase("")]
    [TestCase("{")]
    [TestCase("{}")]
    [TestCase("null")]
    public void MalformedOrIncompleteJsonIsRejected(string json)
    {
        Assert.That(SessionSaveJson.TryLoad(json, Starter().Catalog, out var result, out var reason), Is.False);
        Assert.That(result, Is.Null);
        Assert.That(reason, Is.Not.Empty);
    }

    [TestCase(-0.1)]
    [TestCase(1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    public void InvalidClockRestoreDoesNotMutateState(double progress)
    {
        var clock = new SimulationClock();
        clock.Advance(0.5);
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.RestoreState(new DateTime(1996, 1, 1), progress, SimulationSpeed.Double));
        Assert.That(clock.Date, Is.EqualTo(new DateTime(1995, 1, 1)));
        Assert.That(clock.DayProgress, Is.EqualTo(0.25));
        Assert.That(clock.Speed, Is.EqualTo(SimulationSpeed.Normal));
    }
}
