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

namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// Represents the configuration and usage information for Rock
    /// Intelligence services.
    /// </summary>
    public class RockIntelligenceConfigurationBag
    {
        /// <summary>
        /// The unique identifier of the currently selected bundle.
        /// </summary>
        public Guid? BundleIdentifier { get; set; }

        /// <summary>
        /// The name of the currently selected bundle.
        /// </summary>
        public string BundleName { get; set; }

        /// <summary>
        /// The monthly usage amount for the selected bundle, represented as
        /// a decimal value in USD.
        /// </summary>
        public decimal? MonthlyUsage { get; set; }

        /// <summary>
        /// The remaining balance for the selected bundle, represented as a
        /// decimal value in USD.
        /// </summary>
        public decimal? BalanceRemaining { get; set; }

        /// <summary>
        /// The monthly spending limit for the selected bundle, represented as
        /// a decimal value in USD.
        /// </summary>
        public decimal? MonthlySpendingLimit { get; set; }

        /// <summary>
        /// A message indicating any usage errors or issues that prevent displaying
        /// the current usage information.
        /// </summary>
        public string UsageError { get; set; }
    }
}
