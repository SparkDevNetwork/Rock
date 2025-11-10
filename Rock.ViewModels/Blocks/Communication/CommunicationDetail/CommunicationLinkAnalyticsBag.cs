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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains analytics information for a link contained within a communication.
    /// </summary>
    public class CommunicationLinkAnalyticsBag
    {
        /// <summary>
        /// Gets or sets the URL for this link.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the total count of times this link was clicked, including if it was already clicked once by a
        /// given recipient.
        /// </summary>
        public int TotalClicksCount { get; set; }

        /// <summary>
        /// Gets or sets the unique count of opened communications where the recipient clicked on this link at least once.
        /// </summary>
        public int UniqueClicksCount { get; set; }

        /// <summary>
        /// Gets or sets the percent of opened communications that had at least one click of this link.
        /// </summary>
        public decimal ClickThroughRate { get; set; }

        /// <summary>
        /// Gets or sets the percentage of this link's total clicks count relative to the top performing link's total
        /// clicks count.
        /// </summary>
        public decimal PercentOfTopLink { get; set; }
    }
}
