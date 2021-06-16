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

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class GeneratorConfig
    {
        /// <summary>
        /// The number of times that the generation process has re-started. This will default to 1 (one being the first run).
        /// </summary>
        /// <value>
        /// The run attempts.
        /// </value>
        public int RunAttempts { get; set; }

        /// <summary>
        /// Gets or sets the run date.
        /// </summary>
        /// <value>
        /// The run date.
        /// </value>
        public DateTime RunDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reports saved successfully].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reports saved successfully]; otherwise, <c>false</c>.
        /// </value>
        public bool ReportsCompleted { get; set; }

        /// <summary>
        /// Gets or sets the configured options.
        /// </summary>
        /// <value>
        /// The configured options.
        /// </value>
        public FinancialStatementGeneratorOptions ConfiguredOptions { get; set; }
    }
}
