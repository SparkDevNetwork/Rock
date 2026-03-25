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

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// A single detail row for the financial insights result.
/// </summary>
internal class FinancialInsightsDetailRow
{
    /// <summary>
    /// The transaction identifier.
    /// </summary>
    public int? TransactionId { get; set; }

    /// <summary>
    /// The financial account identifier.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// The name of the financial account.
    /// </summary>
    public string AccountName { get; set; }

    /// <summary>
    /// The amount of money.
    /// </summary>
    public decimal Amount { get; set; }
}

