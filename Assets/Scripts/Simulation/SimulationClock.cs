using System;

namespace InfrastructureTycoon.Simulation
{
    public enum SimulationSpeed
    {
        Paused = 0,
        Normal = 1,
        Double = 2,
        Quadruple = 4,
        Octuple = 8
    }

    public sealed class SimulationClock
    {
        public const double SecondsPerDay = 2;
        private SimulationSpeed speed = SimulationSpeed.Normal;

        public DateTime Date { get; private set; } = new DateTime(1995, 1, 1);
        /// <summary>Fraction of the current day completed, in [0, 1).</summary>
        public double DayProgress { get; private set; }
        public SimulationSpeed Speed
        {
            get => speed;
            set
            {
                if (value != SimulationSpeed.Paused && value != SimulationSpeed.Normal &&
                    value != SimulationSpeed.Double && value != SimulationSpeed.Quadruple &&
                    value != SimulationSpeed.Octuple)
                    throw new ArgumentOutOfRangeException(nameof(value));
                speed = value;
            }
        }

        // Each event carries the outgoing date. Date already points to the next day,
        // with zero DayProgress. At year end the order is day, month, then year.
        public event Action<DateTime> DayEnded;
        public event Action<DateTime> MonthEnded;
        public event Action<DateTime> YearEnded;

        public void Advance(double elapsedSeconds)
        {
            if (double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (speed == SimulationSpeed.Paused || elapsedSeconds == 0)
                return;

            double remainingDays = elapsedSeconds / SecondsPerDay * (int)speed;
            // Reject calendar overflow before mutating state or notifying subscribers.
            if (double.IsInfinity(remainingDays) ||
                Math.Floor(DayProgress + remainingDays) > (DateTime.MaxValue.Date - Date).Days)
                throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

            while (remainingDays >= 1 - DayProgress)
            {
                remainingDays -= 1 - DayProgress;
                var outgoing = Date;
                Date = Date.AddDays(1);
                DayProgress = 0;
                DayEnded?.Invoke(outgoing);
                if (Date.Month != outgoing.Month)
                    MonthEnded?.Invoke(outgoing);
                if (Date.Year != outgoing.Year)
                    YearEnded?.Invoke(outgoing);
            }
            DayProgress += remainingDays;
        }
    }
}
