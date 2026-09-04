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
    /// A bag that contains initialization information for the Spark Connected
    /// Services block.
    /// </summary>
    public class InitializationBag
    {
        /// <summary>
        /// Indicates whether the organization is invalid, which may occur if
        /// the organization is not properly configured or linked to the Spark
        /// Connected Services.
        /// </summary>
        public bool IsOrganizationInvalid { get; set; }

        /// <summary>
        /// Determines whether an attempt to upgrade the authentication
        /// method is possible. This is true if the organization is linked
        /// to a legacy authentication method.
        /// </summary>
        public bool IsUpgradePossible { get; set; }

        /// <summary>
        /// Indicates the title of the error message to display instead of the
        /// block content.
        /// </summary>
        public string ErrorTitle { get; set; }

        /// <summary>
        /// Indicates the description of the error message to display instead of
        /// the block content.
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Indicates the unique identifier of the organization, which is used
        /// to generate the URL for changing the credit card.
        /// </summary>
        public string OrganizationIdentifier { get; set; }

        /// <summary>
        /// Summary information about the currently configured credit card.
        /// </summary>
        public CreditCardSummaryBag CreditCardSummary { get; set; }

        /// <summary>
        /// Current configuration information for the Rock Intelligence
        /// service, including bundle details and usage statistics.
        /// </summary>
        public RockIntelligenceConfigurationBag RockIntelligence { get; set; }

        /// <summary>
        /// The date and time the manifest was last refreshed, as a
        /// DateTimeOffset in the Rock organization time zone.
        /// </summary>
        public DateTimeOffset? ManifestLastRefreshedDateTime { get; set; }
    }
}
