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
    /// A bag that contains information about the KPIs for a communication.
    /// </summary>
    public class CommunicationKpisBag
    {
        /// <summary>
        /// The percent of delivered communications that were opened at least once.
        /// </summary>
        public decimal OpenRate { get; set; }

        /// <summary>
        /// The total count of times the delivered communications were opened, including those that were already
        /// opened once.
        /// </summary>
        public int TotalOpensCount { get; set; }

        /// <summary>
        /// The unique count of delivered communications that were opened at least once.
        /// </summary>
        public int UniqueOpensCount { get; set; }

        /// <summary>
        /// The percent of opened communications that had at least one click.
        /// </summary>
        public decimal ClickThroughRate { get; set; }

        /// <summary>
        /// The total count of times any link was clicked in any of the opened communications, including those that
        /// were already clicked once.
        /// </summary>
        public int TotalClicksCount { get; set; }

        /// <summary>
        /// The unique count of opened communications where the recipient clicked on any link at least once.
        /// </summary>
        public int UniqueClicksCount { get; set; }

        /// <summary>
        /// The percent of delivered communications that were marked as spam.
        /// </summary>
        public decimal MarkedAsSpamRate { get; set; }

        /// <summary>
        /// The total count of delivered communications that were marked as spam.
        /// </summary>
        public int TotalMarkedAsSpamCount { get; set; }

        /// <summary>
        /// The percent of delivered communications that caused the recipient to unsubscribe.
        /// </summary>
        public decimal UnsubscribeRate { get; set; }

        /// <summary>
        /// The total count of delivered communications that caused the recipient to unsubscribe.
        /// </summary>
        public int TotalUnsubscribesCount { get; set; }
    }
}
