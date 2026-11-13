using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using InfrastructureTycoon.Content;

namespace InfrastructureTycoon.Simulation
{
    public static class SessionSaveJson
    {
        public const int CurrentVersion = 2;

        public static SessionSaveData Capture(CompanySession session, string gameVersion)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (session.Clock.IsAdvancing) throw new InvalidOperationException("Capture only between session updates.");
            if (string.IsNullOrWhiteSpace(gameVersion)) throw new ArgumentException("Game version is required.", nameof(gameVersion));
            var data = new SessionSaveData
            {
                SaveVersion = CurrentVersion, GameVersion = gameVersion,
                Timestamp = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Date = DateText(session.Clock.Date), DayProgress = session.Clock.DayProgress, Speed = (int)session.Clock.Speed,
                Cash = session.Cash, OperatingProfit = session.OperatingProfit,
                Hardware = new HardwareSaveData[session.Hardware.Count],
                Contracts = new ContractSaveData[session.AcceptedContracts.Count],
                BillingHistory = new BillingSaveData[session.BillingHistory.Count]
            };
            for (int i = 0; i < data.Hardware.Length; i++)
            {
                var item = session.Hardware[i];
                var purchases = new UpgradePurchaseSaveData[item.PurchaseRecords.Count];
                for (int j = 0; j < purchases.Length; j++)
                {
                    var record = item.PurchaseRecords[j];
                    purchases[j] = new UpgradePurchaseSaveData { HardwareId = record.HardwareId, UpgradeId = record.UpgradeId, AmountPaid = record.AmountPaid };
                }
                data.Hardware[i] = new HardwareSaveData { Id = item.Id, ContentId = item.Definition.Id, Purchases = purchases };
            }
            for (int i = 0; i < data.Contracts.Length; i++)
            {
                var item = session.AcceptedContracts[i];
                var applied = new string[item.AppliedMilestoneIds.Count];
                item.AppliedMilestoneIds.CopyTo(applied, 0);
                data.Contracts[i] = new ContractSaveData { Id = item.Id, ContentId = item.Definition.Id, AssignedHardwareId = item.AssignedHardwareId, PaidMonths = item.PaidMonths, AppliedMilestoneIds = applied };
            }
            for (int i = 0; i < data.BillingHistory.Length; i++)
            {
                var item = session.BillingHistory[i];
                data.BillingHistory[i] = new BillingSaveData { PeriodEnd = DateText(item.PeriodEnd), Revenue = item.Revenue, Expenses = item.Expenses };
            }
            return data;
        }

        public static string ToJson(SessionSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(SessionSaveData)).WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        public static bool TryLoad(string json, HostingCatalog catalog, out CompanySession session, out string reason)
        {
            session = null;
            try
            {
                if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Save is empty.");
                EnsureSingleObject(json);
                SessionSaveData data;
                using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max))
                {
                    data = (SessionSaveData)new DataContractJsonSerializer(typeof(SessionSaveData)).ReadObject(reader);
                    if (reader.Read()) throw new SerializationException("Unexpected trailing JSON content.");
                }
                session = CompanySession.Restore(catalog, data);
                reason = string.Empty;
                return true;
            }
            catch (Exception error) when (IsInvalidSave(error))
            {
                var detail = error;
                while (detail is TargetInvocationException && detail.InnerException != null) detail = detail.InnerException;
                reason = "Invalid save: " + detail.Message;
                return false;
            }
        }

        private static bool IsInvalidSave(Exception error) => error is SerializationException || error is ArgumentException ||
            error is FormatException || error is OverflowException || error is XmlException || error is InvalidOperationException ||
            (error is TargetInvocationException && error.InnerException != null && IsInvalidSave(error.InnerException));

        // The framework reader can stop after one root and ignore trailing input.
        // Check root framing here; the JSON serializer still validates the full object syntax and types.
        private static void EnsureSingleObject(string json)
        {
            string value = json.Trim();
            if (value[0] != '{') throw new SerializationException("Expected a JSON save object.");
            int depth = 0;
            bool quoted = false, escaped = false;
            for (int i = 0; i < value.Length; i++)
            {
                char token = value[i];
                if (quoted)
                {
                    if (escaped) escaped = false;
                    else if (token == '\\') escaped = true;
                    else if (token == '"') quoted = false;
                    continue;
                }
                if (token == '"') quoted = true;
                else if (token == '{' || token == '[') depth++;
                else if (token == '}' || token == ']')
                {
                    depth--;
                    if (depth == 0 && i != value.Length - 1)
                        throw new SerializationException("Unexpected trailing JSON content.");
                    if (depth < 0) throw new SerializationException("Invalid JSON structure.");
                }
            }
            if (quoted || depth != 0 || value[value.Length - 1] != '}')
                throw new SerializationException("Incomplete JSON save object.");
        }

        internal static string DateText(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        internal static DateTime ParseDate(string value)
        {
            if (!DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                throw new ArgumentException("Invalid calendar date.");
            return date;
        }
    }
}
