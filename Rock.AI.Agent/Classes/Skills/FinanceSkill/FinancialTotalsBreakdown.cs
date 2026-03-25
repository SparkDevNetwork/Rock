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
