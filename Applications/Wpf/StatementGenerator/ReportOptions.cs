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
using System.Collections.Generic;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    internal class ReportOptions
    {
        /// <summary>
        /// Gets or sets the current report options
        /// </summary>
        /// <value>
        /// The current report options.
        /// </value>
        public static Rock.Client.FinancialStatementGeneratorOptions Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Loads from configuration.
        /// </summary>
        /// <param name="generatorConfig">The generator configuration.</param>
        public static void LoadFromConfig( GeneratorConfig generatorConfig )
        {
            _current = generatorConfig.ConfiguredOptions;
        }

        /// <summary>
        /// The current
        /// </summary>
        private static Rock.Client.FinancialStatementGeneratorOptions _current = new Rock.Client.FinancialStatementGeneratorOptions();
    }
}
