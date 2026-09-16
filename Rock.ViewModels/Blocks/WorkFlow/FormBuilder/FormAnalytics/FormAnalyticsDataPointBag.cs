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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormAnalytics
{
    /// <summary>
    /// A single date-keyed value within a Form Analytics chart series.
    /// </summary>
    public class FormAnalyticsDataPointBag
    {
        /// <summary>
        /// Gets or sets the ISO 8601 date for this bucket (time component is zero).
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the interaction count recorded for this bucket.
        /// </summary>
        public int Value { get; set; }
    }
}
