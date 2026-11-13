using System;
using System.Linq;
using InfrastructureTycoon.Content;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;
using UnityEditor;

public class MonthlyBillingTests
{
    private static CompanySession Starter() => new CompanySession(
        AssetDatabase.LoadAssetAtPath<StarterCatalog>("Assets/Data/StarterCatalog.asset").ToRuntime());

    private static ContractInstance Host(CompanySession session, string offer)
    {
        Assert.That(session.TryAcceptOffer(offer, out var contract, out var reason), Is.True, reason);
        Assert.That(session.TryAssignContract(contract.Id, session.Hardware[0].Id, out reason), Is.True, reason);
        return contract;
    }

    [TestCase(false, 100, 230, 80)]
    [TestCase(true, 180, 310, 160)]
    public void HostedWebsitesBillOnlyAtRollover(bool both, decimal revenue, decimal cash, decimal profit)
    {
        var session = Starter();
        var photo = Host(session, "offer.photosnap");
        if (both) Host(session, "offer.local-website");
        session.Clock.Advance(61.5);
        Assert.That(session.BillingHistory, Is.Empty);
        Assert.That(session.LastMonthlyResult, Is.Null);
        Assert.That(session.Cash, Is.EqualTo(150m));
        Assert.That(session.OperatingProfit, Is.Zero);
        Assert.That(photo.PaidMonths, Is.Zero);
        session.Clock.Advance(0.5);
        Assert.That(session.Clock.Date, Is.EqualTo(new DateTime(1995, 2, 1)));
        Assert.That(session.Cash, Is.EqualTo(cash));
        Assert.That(session.OperatingProfit, Is.EqualTo(profit));
        var result = session.BillingHistory.Single();
        Assert.That(session.LastMonthlyResult, Is.SameAs(result));
        Assert.That(result.PeriodEnd, Is.EqualTo(new DateTime(1995, 1, 31)));
        Assert.That(result.Revenue, Is.EqualTo(revenue));
        Assert.That(result.Expenses, Is.EqualTo(20m));
        Assert.That(result.NetProfit, Is.EqualTo(profit));
        Assert.That(session.AcceptedContracts.All(x => x.PaidMonths == 1), Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void PcExpenseIsChargedWithoutHostedContracts(bool acceptUnassigned)
    {
        var session = Starter();
        if (acceptUnassigned) session.TryAcceptOffer("offer.photosnap", out _, out _);
        session.Clock.Advance(62);
        Assert.That(session.Cash, Is.EqualTo(130m));
        Assert.That(session.OperatingProfit, Is.EqualTo(-20m));
        Assert.That(session.LastMonthlyResult.Revenue, Is.Zero);
        Assert.That(session.LastMonthlyResult.Expenses, Is.EqualTo(20m));
        Assert.That(session.AcceptedContracts.All(x => x.PaidMonths == 0), Is.True);
    }

    [Test]
    public void LateAssignmentReceivesFullAmountOnlyForItsFirstHostedBoundary()
    {
        var session = Starter();
        session.TryAcceptOffer("offer.photosnap", out var photo, out _);
        session.Clock.Advance(62); // Unassigned throughout January.
        session.Clock.Advance(55.5); // February 28, three quarters through the day.
        session.TryAssignContract(photo.Id, session.Hardware[0].Id, out _);
        session.Clock.Advance(0.5);
        Assert.That(session.BillingHistory.Select(x => x.Revenue), Is.EqualTo(new[] { 0m, 100m }));
        Assert.That(photo.PaidMonths, Is.EqualTo(1));
        Assert.That(session.Cash, Is.EqualTo(210m));
        Assert.That(session.OperatingProfit, Is.EqualTo(60m));
    }

    [Test]
    public void OneUpdateBillsEveryOutgoingMonthInOrderAcrossYearAndLeapDay()
    {
        var session = Starter();
        Host(session, "offer.photosnap");
        Host(session, "offer.local-website");
        int monthsAtYearEnd = -1;
        session.Clock.YearEnded += _ => monthsAtYearEnd = session.BillingHistory.Count;
        session.Clock.Advance((new DateTime(1996, 3, 1) - session.Clock.Date).TotalDays * 2);
        var expected = Enumerable.Range(0, 14).Select(i => new DateTime(1995, 2, 1).AddMonths(i).AddDays(-1));
        CollectionAssert.AreEqual(expected, session.BillingHistory.Select(x => x.PeriodEnd));
        Assert.That(monthsAtYearEnd, Is.EqualTo(12));
        Assert.That(session.Cash, Is.EqualTo(2390m));
        Assert.That(session.OperatingProfit, Is.EqualTo(2240m));
        Assert.That(session.AcceptedContracts.All(x => x.PaidMonths == 14), Is.True);
        session.Clock.Advance(0);
        Assert.That(session.BillingHistory.Count, Is.EqualTo(14));
    }

    [Test]
    public void ReplayedOrInvalidPeriodsLeaveAllBillingStateUnchanged()
    {
        var session = Starter();
        var photo = Host(session, "offer.photosnap");
        Assert.That(session.TryBillMonth(new DateTime(1995, 1, 31), out var reason), Is.False);
        Assert.That(reason, Is.Not.Empty);
        Assert.That(session.BillingHistory, Is.Empty);
        Assert.That(session.Cash, Is.EqualTo(150m));
        session.Clock.Advance(62);
        var original = session.LastMonthlyResult;
        foreach (var period in new[] { new DateTime(1995, 1, 31), new DateTime(1995, 1, 30), new DateTime(1995, 2, 28), new DateTime(1994, 12, 31) })
        {
            Assert.That(session.TryBillMonth(period, out reason), Is.False);
            Assert.That(reason, Is.Not.Empty);
            Assert.That(session.Cash, Is.EqualTo(230m));
            Assert.That(session.OperatingProfit, Is.EqualTo(80m));
            Assert.That(session.LastMonthlyResult, Is.SameAs(original));
            Assert.That(session.BillingHistory.Count, Is.EqualTo(1));
            Assert.That(photo.PaidMonths, Is.EqualTo(1));
        }
        session.Clock.Advance(56);
        Assert.That(session.Cash, Is.EqualTo(310m));
        Assert.That(photo.PaidMonths, Is.EqualTo(2));
    }
}
