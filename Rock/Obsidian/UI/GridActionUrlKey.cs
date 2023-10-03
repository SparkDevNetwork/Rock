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

namespace Rock.Obsidian.UI
{
    /// <summary>
    /// Keys for the Action URLs provided to the Grid.
    /// </summary>
    internal static class GridActionUrlKey
    {
        /// <summary>
        /// The URL to use when sending a communication.
        /// </summary>
        public const string Communicate = "communicate";

        /// <summary>
        /// The URL to use when merging Person records.
        /// </summary>
        public const string MergePerson = "mergePerson";

        /// <summary>
        /// The URL to use when merging Business records.
        /// </summary>
        public const string MergeBusiness = "mergeBusiness";

        /// <summary>
        /// The URL to use when performing a bulk update.
        /// </summary>
        public const string BulkUpdate = "bulkUpdate";

        /// <summary>
        /// The URL to use when launching a workflow for each record.
        /// </summary>
        public const string LaunchWorkflow = "launchWorkflow";

        /// <summary>
        /// The URL to use to start a merge template request.
        /// </summary>
        public const string MergeTemplate = "mergeTemplate";
    }
}
