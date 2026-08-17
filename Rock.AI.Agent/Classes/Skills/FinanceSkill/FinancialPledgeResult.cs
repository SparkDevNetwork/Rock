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
//
using System;

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// Represents a single pledge of giving to a specific account.
/// </summary>
internal class FinancialPledgeResult : EntityResultBase
{
    /// <summary>
    /// The person that made the pledge.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The start date of the pledge.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The end date of the pledge.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// The account the pledge is for.
    /// </summary>
    public FinancialAccountResult FinancialAccount { get; set; }

    /// <summary>
    /// The total amount of money pledged to be given.
    /// </summary>
    public decimal? TotalAmount { get; set; }

    /// <summary>
    /// The frequency of the expected payments (e.g. weekly, monthly, etc.).
    /// </summary>
    public string PaymentSchedule { get; set; }
}
