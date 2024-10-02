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
namespace Rock.AI.Automations
{
    /// <summary>
    /// The expected response format for an AI Completion response to a request for analysis of text.
    /// </summary>
    public class PrayerRequestAnalyzerResponse
    {
        /// <summary>
        /// The identifier of the sentiment chosen by the AI completion.
        /// </summary>
        public int? SentimentId;

        /// <summary>
        /// The identifier of the category chosen by the AI completion.
        /// </summary>
        public int? CategoryId;

        /// <summary>
        /// <see langword="true"/> if the text was determined to be appropriate for public viewing; otherwise, <see langword="false"/>.
        /// </summary>
        public bool? IsAppropriateForPublic;
    }
}
