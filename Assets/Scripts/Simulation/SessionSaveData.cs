using System.Runtime.Serialization;

namespace InfrastructureTycoon.Simulation
{
    [DataContract]
    public sealed class SessionSaveData
    {
        [DataMember(Name = "save_version", IsRequired = true)] public int SaveVersion;
        [DataMember(Name = "game_version", IsRequired = true)] public string GameVersion;
        [DataMember(Name = "timestamp", IsRequired = true)] public string Timestamp;
        [DataMember(Name = "date", IsRequired = true)] public string Date;
        [DataMember(Name = "day_progress", IsRequired = true)] public double DayProgress;
        [DataMember(Name = "speed", IsRequired = true)] public int Speed;
        [DataMember(Name = "cash", IsRequired = true)] public decimal Cash;
        [DataMember(Name = "operating_profit", IsRequired = true)] public decimal OperatingProfit;
        [DataMember(Name = "hardware", IsRequired = true)] public HardwareSaveData[] Hardware;
        [DataMember(Name = "contracts", IsRequired = true)] public ContractSaveData[] Contracts;
        // Each history entry is also a processed billing period.
        [DataMember(Name = "billing_history", IsRequired = true)] public BillingSaveData[] BillingHistory;
    }

    [DataContract]
    public sealed class HardwareSaveData
    {
        [DataMember(Name = "id", IsRequired = true)] public string Id;
        [DataMember(Name = "content_id", IsRequired = true)] public string ContentId;
        // Optional in the reader for v1 compatibility; mandatory (including []) in v2.
        [DataMember(Name = "purchases", EmitDefaultValue = false)] public UpgradePurchaseSaveData[] Purchases;
    }

    [DataContract]
    public sealed class UpgradePurchaseSaveData
    {
        [DataMember(Name = "hardware_id", IsRequired = true)] public string HardwareId;
        [DataMember(Name = "upgrade_id", IsRequired = true)] public string UpgradeId;
        [DataMember(Name = "amount_paid", IsRequired = true)] public decimal AmountPaid;
    }

    [DataContract]
    public sealed class ContractSaveData
    {
        [DataMember(Name = "id", IsRequired = true)] public string Id;
        [DataMember(Name = "content_id", IsRequired = true)] public string ContentId;
        [DataMember(Name = "assigned_hardware_id", IsRequired = true)] public string AssignedHardwareId;
        [DataMember(Name = "paid_months", IsRequired = true)] public int PaidMonths;
        [DataMember(Name = "applied_milestone_ids", EmitDefaultValue = false)] public string[] AppliedMilestoneIds;
    }

    [DataContract]
    public sealed class BillingSaveData
    {
        [DataMember(Name = "period_end", IsRequired = true)] public string PeriodEnd;
        [DataMember(Name = "revenue", IsRequired = true)] public decimal Revenue;
        [DataMember(Name = "expenses", IsRequired = true)] public decimal Expenses;
    }
}
