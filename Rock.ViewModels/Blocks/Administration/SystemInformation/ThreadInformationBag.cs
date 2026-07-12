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

namespace Rock.ViewModels.Blocks.Administration.SystemInformation
{
    /// <summary>
    /// Information about the application's worker thread pool usage.
    /// </summary>
    public class ThreadInformationBag
    {
        /// <summary>
        /// Gets or sets the number of worker threads currently in use.
        /// </summary>
        public int ThreadsInUse { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of worker threads available.
        /// </summary>
        public int MaxThreads { get; set; }

        /// <summary>
        /// Gets or sets the percentage of worker threads currently in use.
        /// </summary>
        public int PercentInUse { get; set; }

        /// <summary>
        /// Gets or sets the badge CSS class reflecting thread usage severity
        /// (empty when usage is low, warning, or danger).
        /// </summary>
        public string BadgeCssClass { get; set; }
    }
}
