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

using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the analytics for a communication.
    /// </summary>
    public class CommunicationAnalyticsResponseBag
    {
        /// <summary>
        /// Gets or sets the error message. A non-empty value indicates that an error is preventing the analytics data
        /// from being displayed.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether to show a message indicating no activity was found for this communication.
        /// </summary>
        public bool ShowNoActivityMessage { get; set; }

        /// <summary>
        /// Gets or sets the delivery breakdown for this communication.
        /// </summary>
        public CommunicationDeliveryBreakdownBag DeliveryBreakdown { get; set; }

        /// <summary>
        /// Gets or sets the KPIs for this communication.
        /// </summary>
        public CommunicationKpisBag Kpis { get; set; }

        /// <summary>
        /// Gets or sets the unique interactions (e.g. opens, clicks) over time for this communication.
        /// </summary>
        public List<ChartNumericDataPointBag> UniqueInteractionsOverTime { get; set; }

        /// <summary>
        /// Gets or sets the activity flow of interactions, Etc. for this communication.
        /// </summary>
        public CommunicationActivityFlowBag ActivityFlow { get; set; }

        /// <summary>
        /// Gets or sets the analytics information for all links contained within this communication.
        /// </summary>
        public List<CommunicationLinkAnalyticsBag> AllLinksAnalytics { get; set; }

        /// <summary>
        /// Gets or sets the unique opens by gender for this communication.
        /// </summary>
        public List<ChartNumericDataPointBag> UniqueOpensByGender { get; set; }

        /// <summary>
        /// Gets or sets the unique opens by age range for this communication.
        /// </summary>
        public List<ChartNumericDataPointBag> UniqueOpensByAgeRange { get; set; }

        /// <summary>
        /// Gets or sets the top clients used to interact with this communication.
        /// </summary>
        public List<ChartNumericDataPointBag> TopClients { get; set; }

        /// <summary>
        /// Gets or sets all clients used to interact with this communication.
        /// </summary>
        public List<ChartNumericDataPointBag> AllClients { get; set; }
    }
}
