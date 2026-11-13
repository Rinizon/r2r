using System;

namespace InfrastructureTycoon.Simulation
{
    public sealed class MonthlyBillingResult
    {
        public DateTime PeriodEnd { get; }
        public decimal Revenue { get; }
        public decimal Expenses { get; }
        public decimal NetProfit => Revenue - Expenses;

        internal MonthlyBillingResult(DateTime periodEnd, decimal revenue, decimal expenses)
        {
            PeriodEnd = periodEnd;
            Revenue = revenue;
            Expenses = expenses;
        }
    }
}
