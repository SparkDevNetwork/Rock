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

namespace Rock.ViewModels.Blocks.Workflow.ShareWorkflow
{
    /// <summary>
    /// The result of exporting a workflow type, containing the serialized content
    /// the browser should download.
    /// </summary>
    public class ShareWorkflowExportResultBag
    {
        /// <summary>
        /// Gets or sets the suggested file name for the downloaded export.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the JSON content of the exported workflow type.
        /// </summary>
        public string Json { get; set; }
    }
}
