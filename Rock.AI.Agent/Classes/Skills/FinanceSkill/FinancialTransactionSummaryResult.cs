using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// DTO representing the analytic summary payload returned by <see cref="SummarizeFinancialTransactions"/>.
    /// </summary>
    internal sealed class FinancialTransactionSummaryResult
    {
        /// <summary>
        /// ISO 4217 currency code (e.g., USD) if a single currency context is known; otherwise <c>null</c>.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Aggregate descriptive statistics for the filtered transaction set.
        /// </summary>
        public FinancialTotalsBreakdown Totals { get; set; }

        /// <summary>
        /// Breakdown of giving by fund (account) ordered by total descending.
        /// </summary>
        public List<FundBreakdown> Funds { get; set; }

        /// <summary>
        /// Breakdown of totals by payment method (currency type / tender).
        /// </summary>
        public List<CurrencyTypeBreakdown> CurrencyTypes { get; set; }
    }
}
