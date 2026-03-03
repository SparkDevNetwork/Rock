namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// A breakdown of currency for various types of groupings.
    /// </summary>
    internal sealed class CurrencyBreakdown
    {
        /// <summary>
        /// The encoded identifier of this type.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// The name of the breakdown type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Count of distinct contributing transactions for this type.
        /// </summary>
        public int? UniqueTransactionCount { get; set; }

        /// <summary>
        /// Total amount represented by this type.
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Portion of overall total (0..100) represented by this type.
        /// </summary>
        public decimal? PercentOfTotal { get; set; }

        /// <summary>
        /// Portion of overall total (0..100) represented by this type.
        /// </summary>
        public decimal? PercentOfTotalCreditCards { get; set; }
    }
}
