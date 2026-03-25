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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// A record that represents a scheduled financial transaction.
/// </summary>
internal class FinancialScheduledTransactionResult : FinancialTransactionResult
{
    /// <summary>
    /// The next payment date if there is one.
    /// </summary>
    public DateTime? NextPaymentDate { get; set; }

    /// <summary>
    /// A string that describes the frequency of the transaction.
    /// </summary>
    public string Frequency { get; set; }
}
