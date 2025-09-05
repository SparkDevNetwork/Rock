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

namespace Rock.ViewModels.Blocks.Communication.CommunicationSaturationReport
{
    /// <summary>
    /// The data used to fill the chart for the Communication Saturation Report. It represents
    /// the number of people who were sent X number of communications in a given time period. X is
    /// the Bucket, which is a range of numbers (e.g. 1-3).
    /// </summary>
    public class CommunicationSaturationReportChartDataBag
    {
        /// <summary>
        /// The bucket representing a number of communications sent to a person.
        /// The bucket is a range of numbers (e.g. 1-3).
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// The number of people who were sent the number of communications represented by the bucket.
        /// </summary>
        public int NumberOfRecipients { get; set; }
    }
}
