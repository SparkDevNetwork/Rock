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

using Rock.Enums.Connection;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// Additional settings for a connection type.
    /// </summary>
    public class ConnectionTypeAdditionalSettingsBag
    {
        /// <summary>
        /// Gets or sets the additional requests to show configuration rows.
        /// </summary>
        public List<ConnectionTypeAdditionalRequestToShowBag> AdditionalRequestsToShow { get; set; }

        /// <summary>
        /// Gets or sets the communication settings.
        /// </summary>
        public ConnectionTypeCommunicationSettingsBag CommunicationSettings { get; set; }

        /// <summary>
        /// Gets or sets the AI insights prompt.
        /// </summary>
        public string AIInsightsPrompt { get; set; }

        /// <summary>
        /// Gets or sets the AI summary trigger mode.
        /// </summary>
        public AISummaryTriggerMode? AISummaryTrigger { get; set; }

        /// <summary>
        /// Gets or sets the AI summary cache duration in minutes.
        /// </summary>
        public int? AISummaryCacheDurationMinutes { get; set; }
    }
}
