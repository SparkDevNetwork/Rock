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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Workflow.ShareWorkflow
{
    /// <summary>
    /// The request used to import a previously exported workflow type file.
    /// </summary>
    public class ShareWorkflowImportRequestBag
    {
        /// <summary>
        /// Gets or sets the uploaded file to import. The value is the binary file
        /// unique identifier produced by the file uploader.
        /// </summary>
        public ListItemBag File { get; set; }

        /// <summary>
        /// Gets or sets the category that the imported workflow type should be placed in.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the import should only be tested.
        /// When <c>true</c>, no changes are saved to the database.
        /// </summary>
        public bool IsTestOnly { get; set; }
    }
}
