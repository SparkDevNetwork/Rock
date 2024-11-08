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

using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// The box that contains all the initialization information for the System Configuration block.
    /// </summary>
    public class SystemConfigurationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the general configuration bag.
        /// </summary>
        /// <value>
        /// The general configuration bag.
        /// </value>
        public GeneralConfigurationBag GeneralConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the UI settings configuration bag.
        /// </summary>
        /// <value>
        /// The UI settings configuration bag.
        /// </value>
        public UiSettingsConfigurationBag UiSettingsConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the observability configuration bag.
        /// </summary>
        /// <value>
        /// The observability configuration bag.
        /// </value>
        public ObservabilityConfigurationBag ObservabilityConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the experimental settings configuration bag.
        /// </summary>
        /// <value>
        /// The experimental settings configuration bag.
        /// </value>
        public ExperimentalSettingsConfigurationBag ExperimentalSettingsConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the web configuration configuration bag.
        /// </summary>
        /// <value>
        /// The web configuration configuration bag.
        /// </value>
        public WebConfigConfigurationBag WebConfigConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the family rules settings configuration bag.
        /// </summary>
        /// <value>
        /// The family rules settings configuration bag.
        /// </value>
        public FamilyRulesSettingsConfigurationBag FamilyRulesSettingsConfigurationBag { get; set; }

        /// <summary>
        /// Gets or sets the observability endpoint protocols.
        /// </summary>
        /// <value>
        /// The observability endpoint protocols.
        /// </value>
        public List<ListItemBag> ObservabilityEndpointProtocols { get; set; }

        /// <summary>
        /// Gets or sets the time zones.
        /// </summary>
        /// <value>
        /// The time zones.
        /// </value>
        public List<ListItemBag> TimeZones { get; set; }
    }
}
