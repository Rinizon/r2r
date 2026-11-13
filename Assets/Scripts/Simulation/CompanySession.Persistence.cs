using System;
using System.Collections.Generic;
using System.Globalization;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public sealed partial class CompanySession
    {
        internal static CompanySession Restore(HostingCatalog catalog, SessionSaveData data)
        {
            if (data == null) throw new ArgumentException("Save data is missing.");
            if (data.SaveVersion != SessionSaveJson.CurrentVersion) throw new ArgumentException("Unsupported save version.");
            if (string.IsNullOrWhiteSpace(data.GameVersion) ||
                !DateTimeOffset.TryParseExact(data.Timestamp, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp) ||
                timestamp.Offset != TimeSpan.Zero)
                throw new ArgumentException("Save metadata is invalid.");
            return new CompanySession(catalog, data);
        }

        private CompanySession(HostingCatalog catalog, SessionSaveData data) : this(catalog)
        {
            var date = SessionSaveJson.ParseDate(data.Date);
            Clock.RestoreState(date, data.DayProgress, (SimulationSpeed)data.Speed);
            if (data.Hardware == null || data.Hardware.Length != 1 || data.Hardware[0] == null ||
                data.Hardware[0].ContentId != catalog.StarterPc.Id || data.Contracts == null || data.BillingHistory == null)
                throw new ArgumentException("Missing or unknown Phase 2 definitions.");

            var ids = new HashSet<string>(StringComparer.Ordinal);
            AddRuntimeId(ids, data.Hardware[0].Id, data.Hardware[0].ContentId);
            Hardware = Array.AsReadOnly(new[] { new HardwareInstance(catalog.StarterPc, data.Hardware[0].Id) });
            var offers = new HashSet<string>(StringComparer.Ordinal);
            int completedMonths = (date.Year - 1995) * 12 + date.Month - 1;
            foreach (var saved in data.Contracts)
            {
                if (saved == null) throw new ArgumentException("Missing contract data.");
                var offer = FindOffer(saved.ContentId);
                if (offer == null || !offers.Add(offer.Id)) throw new ArgumentException("Unknown or duplicate offer content ID.");
                AddRuntimeId(ids, saved.Id, offer.Id);
                if (saved.PaidMonths < 0 || saved.PaidMonths > completedMonths ||
                    (saved.AssignedHardwareId == null && saved.PaidMonths != 0))
                    throw new ArgumentException("Invalid paid-month count.");
                var contract = new ContractInstance(offer, saved.Id) { PaidMonths = saved.PaidMonths };
                acceptedContracts.Add(contract);
                if (saved.AssignedHardwareId != null && !TryAssignContract(contract.Id, saved.AssignedHardwareId, out var reason))
                    throw new ArgumentException("Invalid assignment: " + reason);
            }

            // Require the complete chronological ledger; missing periods could otherwise replay payments.
            if (data.BillingHistory.Length != completedMonths) throw new ArgumentException("Incomplete billing history.");
            decimal profit = 0;
            for (int i = 0; i < data.BillingHistory.Length; i++)
            {
                var saved = data.BillingHistory[i];
                if (saved == null || saved.Revenue < 0 || saved.Expenses < 0) throw new ArgumentException("Invalid monthly result.");
                var period = SessionSaveJson.ParseDate(saved.PeriodEnd);
                if (period != new DateTime(1995, 2, 1).AddMonths(i).AddDays(-1))
                    throw new ArgumentException("Duplicate, missing, or unordered billing period.");
                var result = new MonthlyBillingResult(period, saved.Revenue, saved.Expenses);
                profit += result.NetProfit;
                billingHistory.Add(result);
                processedBillingPeriods.Add(period);
            }
            if (data.OperatingProfit != profit || data.Cash != 150m + profit)
                throw new ArgumentException("Finances do not match billing history.");
            Cash = data.Cash;
            OperatingProfit = data.OperatingProfit;
        }

        private static void AddRuntimeId(HashSet<string> ids, string id, string contentId)
        {
            if (string.IsNullOrWhiteSpace(id) || id == contentId || !ids.Add(id))
                throw new ArgumentException("Missing, duplicate, or invalid runtime ID.");
        }
    }
}
