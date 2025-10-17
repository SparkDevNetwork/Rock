namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// Descriptive statistics for a set of transactions (after filters & fund scoping applied).
    /// </summary>
    public sealed class FinancialTotalsBreakdown
    {
        /// <summary>
        /// Distinct transactions contributing to the statistics (post fund filtering if applied).
        /// </summary>
        public int UniqueTransactionCount { get; set; }
        /// <summary>
        /// Sum of contributing transaction amounts.
        /// </summary>
        public decimal TotalAmount { get; set; }
        /// <summary>
        /// Arithmetic mean (average) of the contributing transaction amounts.
        /// </summary>
        public decimal AverageAmountPerTransaction { get; set; }
        /// <summary>
        /// Median value of the contributing transaction amounts.
        /// </summary>
        public decimal MedianAmountPerTransaction { get; set; }
        /// <summary>
        /// Population standard deviation of the contributing transaction amounts.
        /// </summary>
        public decimal StandardDeviationAmountPerTransaction { get; set; }
    }
}
