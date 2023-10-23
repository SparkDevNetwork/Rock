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
using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Core.BinaryFileDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class BinaryFileBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the id of the Rock.Model.BinaryFileType that this file belongs to.
        /// </summary>
        public ListItemBag BinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the file.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        public ListItemBag File { get; set; }

        /// <summary>
        /// Gets or sets the name of the file, including any extensions. This name is usually captured when the file is uploaded to Rock and this same name will be used when the file is downloaded. This property is required.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the Mime Type for the file. This property is required
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show binary file type].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show binary file type]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBinaryFileType { get; set; }

        /// <summary>
        /// Gets or sets the re run workflow button text.
        /// </summary>
        /// <value>
        /// The re run workflow button text.
        /// </value>
        public string WorkflowButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show workflow button].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show workflow button]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowWorkflowButton { get; set; }

        /// <summary>
        /// Gets or sets the workflow notification message.
        /// </summary>
        /// <value>
        /// The workflow notification message.
        /// </value>
        public string WorkflowNotificationMessage { get; set; }

        /// <summary>
        /// Gets or sets the orphaned binary file identifier list. This holds the list of uploaded binary files that were ultimately not used and can be deleted.
        /// </summary>
        /// <value>
        /// The orphaned binary file identifier list.
        /// </value>
        public List<Guid> OrphanedBinaryFileIdList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is label file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is label file; otherwise, <c>false</c>.
        /// </value>
        public bool IsLabelFile { get; set; }
    }
}
