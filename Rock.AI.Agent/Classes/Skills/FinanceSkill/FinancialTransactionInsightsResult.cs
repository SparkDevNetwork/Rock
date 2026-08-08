// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Collections.Generic;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

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
