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

namespace Rock.ViewModels.Blocks.Core.EntitySearchDetail
{
    /// <summary>
    /// The results from a preview query request.
    /// </summary>
    public class PreviewResultsBag
    {
        /// <summary>
        /// Gets or sets the duration of the query in milliseconds.
        /// </summary>
        /// <value>The duration in milliseconds.</value>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the number of queries generated to perform the search.
        /// </summary>
        /// <value>The query count.</value>
        public int QueryCount { get; set; }

        /// <summary>
        /// Gets or sets the data returned by the query, encoded as a JSON string.
        /// </summary>
        /// <value>The data returned by the query.</value>
        public string Data { get; set; }
    }
}
