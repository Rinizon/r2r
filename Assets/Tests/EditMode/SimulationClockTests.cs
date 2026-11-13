using System;
using System.Collections.Generic;
using InfrastructureTycoon.Simulation;
using NUnit.Framework;

public class SimulationClockTests
{
    [TestCase(SimulationSpeed.Normal, 1)]
    [TestCase(SimulationSpeed.Double, 2)]
    [TestCase(SimulationSpeed.Quadruple, 4)]
    [TestCase(SimulationSpeed.Octuple, 8)]
    public void EqualElapsedTimeAdvancesProportionally(SimulationSpeed speed, int days)
    {
        var clock = new SimulationClock { Speed = speed };
        clock.Advance(2);
        Assert.That(clock.Date, Is.EqualTo(new DateTime(1995, 1, 1).AddDays(days)));
        Assert.That(clock.DayProgress, Is.Zero);
    }

    [Test]
    public void PauseAndSpeedChangesPreserveFractionalDay()
    {
        var clock = new SimulationClock();
        int ticks = 0;
        clock.DayEnded += _ => ticks++;
        clock.Advance(0.5);
        Assert.That(clock.DayProgress, Is.EqualTo(0.25));
        clock.Speed = SimulationSpeed.Paused;
        clock.Advance(1000);
        Assert.That(clock.Date, Is.EqualTo(new DateTime(1995, 1, 1)));
        Assert.That(clock.DayProgress, Is.EqualTo(0.25));
        Assert.That(ticks, Is.Zero);
        clock.Speed = SimulationSpeed.Double;
        clock.Advance(0.5);
        Assert.That(clock.DayProgress, Is.EqualTo(0.75));
        clock.Speed = SimulationSpeed.Quadruple;
        clock.Advance(0.125);
        Assert.That(clock.Date, Is.EqualTo(new DateTime(1995, 1, 2)));
        Assert.That(clock.DayProgress, Is.Zero);
        Assert.That(ticks, Is.EqualTo(1));
    }

    [Test]
    public void SingleUpdateEmitsEveryOutgoingBoundaryOnceInChronologicalOrder()
    {
        var clock = new SimulationClock();
        var actual = new List<string>();
        clock.DayEnded += day =>
        {
            Assert.That(clock.Date, Is.EqualTo(day.AddDays(1)));
            Assert.That(clock.DayProgress, Is.Zero);
            actual.Add("D:" + day.ToString("yyyy-MM-dd"));
        };
        clock.MonthEnded += day => actual.Add("M:" + day.ToString("yyyy-MM-dd"));
        clock.YearEnded += day => actual.Add("Y:" + day.ToString("yyyy-MM-dd"));

        var start = new DateTime(1995, 1, 1);
        var end = new DateTime(1996, 3, 2);
        var expected = new List<string>();
        for (var day = start; day < end; day = day.AddDays(1))
        {
            expected.Add("D:" + day.ToString("yyyy-MM-dd"));
            if (day.Day == DateTime.DaysInMonth(day.Year, day.Month))
                expected.Add("M:" + day.ToString("yyyy-MM-dd"));
            if (day.Month == 12 && day.Day == 31)
                expected.Add("Y:" + day.ToString("yyyy-MM-dd"));
        }
        clock.Advance((end - start).TotalDays * 2 + 0.5);
        Assert.That(clock.Date, Is.EqualTo(end));
        Assert.That(clock.DayProgress, Is.EqualTo(0.25));
        CollectionAssert.AreEqual(expected, actual);
        clock.Advance(0);
        CollectionAssert.AreEqual(expected, actual);
    }

    [Test]
    public void SmallUpdatesMatchOneCombinedUpdate()
    {
        var clock = new SimulationClock();
        var combined = new SimulationClock();
        for (int i = 0; i < 1000; i++)
            clock.Advance(0.1);
        combined.Advance(100);
        Assert.That(clock.Date, Is.EqualTo(combined.Date));
        Assert.That(clock.DayProgress, Is.EqualTo(combined.DayProgress).Within(1e-10));
    }

    [TestCase(-1)]
    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.MaxValue)]
    public void InvalidElapsedTimeLeavesStateIntact(double elapsed)
    {
        var clock = new SimulationClock();
        clock.Advance(0.5);
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(elapsed));
        Assert.That(clock.Date, Is.EqualTo(new DateTime(1995, 1, 1)));
        Assert.That(clock.DayProgress, Is.EqualTo(0.25));
    }

    [Test]
    public void UnsupportedSpeedLeavesSelectionIntact()
    {
        var clock = new SimulationClock();
        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Speed = (SimulationSpeed)3);
        Assert.That(clock.Speed, Is.EqualTo(SimulationSpeed.Normal));
    }
}
