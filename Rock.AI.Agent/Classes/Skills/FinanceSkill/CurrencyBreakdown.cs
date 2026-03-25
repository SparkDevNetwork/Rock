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
