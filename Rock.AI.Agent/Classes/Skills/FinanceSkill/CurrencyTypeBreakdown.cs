namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// Payment method (currency type) summary row including share and count metrics.
    /// </summary>
    internal sealed class CurrencyTypeBreakdown
    {
        /// <summary>
        /// Human friendly tender / currency type name.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Count of distinct contributing transactions for this type.
        /// </summary>
        public int UniqueTransactionCount { get; set; }

        /// <summary>
        /// Total amount represented by this type.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Portion of overall total (0..1) represented by this type.
        /// </summary>
        public decimal PercentOfTotal { get; set; }
    }
}
