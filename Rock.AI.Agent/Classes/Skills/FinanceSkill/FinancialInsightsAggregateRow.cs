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

using System;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// Represents a single transaction while building the aggregate financial
/// insights data.
/// </summary>
internal class FinancialInsightsAggregateRow
{
    /// <summary>
    /// The identifier of the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The date and time the transaction occurred.
    /// </summary>
    public DateTime? TransactionDateTime { get; set; }

    /// <summary>
    /// The type of currency used for the transaction.
    /// </summary>
    public int? CurrencyTypeId { get; set; }

    /// <summary>
    /// The name of the type of currency used for the transaction.
    /// </summary>
    public string CurrencyType { get; set; }

    /// <summary>
    /// The frequency of this transaction for scheduled giving.
    /// </summary>
    public string Frequency { get; set; }

    /// <summary>
    /// The amount of money filtered to the requested accounts.
    /// </summary>
    public decimal AmountFiltered { get; set; }
}

