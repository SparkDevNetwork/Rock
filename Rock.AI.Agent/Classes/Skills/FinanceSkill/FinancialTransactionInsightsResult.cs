using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill
{
    /// <summary>
    /// DTO representing the analytic summary payload returned by <see cref="SummarizeFinancialTransactions"/>.
    /// </summary>
    internal sealed class FinancialTransactionInsightsResult
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
        public List<CurrencyBreakdown> Funds { get; set; }

        /// <summary>
        /// Breakdown of totals by payment method (currency type / tender).
        /// </summary>
        public List<CurrencyBreakdown> CurrencyTypes { get; set; }

        /// <summary>
        /// Breakdown of totals by credit card type (if applicable).
        /// </summary>
        public List<CurrencyBreakdown> CreditCardTypes { get; set; }

        /// <summary>
        /// The breakdown of registration instances.
        /// </summary>
        public List<CurrencyBreakdown> RegistrationInstances { get; set; }

        /// <summary>
        /// The breakdown of scheduled frequencies.
        /// </summary>
        public List<CurrencyBreakdown> FrequencyTypes { get; set; }
    }
}
