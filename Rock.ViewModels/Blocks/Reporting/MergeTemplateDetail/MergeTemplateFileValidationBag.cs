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

namespace Rock.ViewModels.Blocks.Reporting.MergeTemplateDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class MergeTemplateFileValidationBag
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the merge template type entity.
        /// </summary>
        /// <value>
        /// The type of the merge template type entity.
        /// </value>
        public ListItemBag MergeTemplateTypeEntityType { get; set; }

        /// <summary>
        /// Gets or sets the file type warning message.
        /// </summary>
        /// <value>
        /// The file type warning message.
        /// </value>
        public string FileTypeWarningMessage { get; set; }
    }
}
