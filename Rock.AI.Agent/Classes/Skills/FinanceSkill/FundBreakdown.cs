namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// Fund (account) summary row including contribution share and unique transaction count.
    /// </summary>
    public sealed class FundBreakdown
    {
        /// <summary>
        /// Hashed IdKey for the account/fund.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Fund display name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Total amount allocated to the fund within the selection.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Portion of overall total (0..1) represented by this fund.
        /// </summary>
        public decimal PercentOfTotal { get; set; }

        /// <summary>
        /// Distinct transactions that included this fund.
        /// </summary>
        public int UniqueTransactionCount { get; set; }
    }

}
