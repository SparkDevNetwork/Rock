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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.SparkConnectedServices
{
    /// <summary>
    /// Represents the options available for Rock Intelligence services.
    /// </summary>
    public class RockIntelligenceOptionsBag
    {
        /// <summary>
        /// The list of available bundles to pick from.
        /// </summary>
        public List<ListItemBag> Bundles { get; set; }

        /// <summary>
        /// The unique identifier of the currently selected bundle.
        /// </summary>
        public Guid? SelectedBundleId { get; set; }

        /// <summary>
        /// The current spending limit for the service, represented as a decimal
        /// value in USD.
        /// </summary>
        public decimal? SpendingLimit { get; set; }
    }
}
