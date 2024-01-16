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

using System;
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the Email Medium options for the Communication Entry block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.Communication.CommunicationEntry.CommunicationEntryMediumOptionsBaseBag" />
    public class CommunicationEntryEmailMediumOptionsBag : CommunicationEntryMediumOptionsBaseBag
    {
        /// <summary>
        /// Gets the type of the medium.
        /// </summary>
        /// <value>
        /// The type of the medium.
        /// </value>
        public override MediumType MediumType => MediumType.Email;

        /// <summary>
        /// Gets or sets a value indicating whether the attachment uploader is shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the attachment uploader is shown; otherwise, <c>false</c>.
        /// </value>
        public bool IsAttachmentUploaderShown { get; set; }

        /// <summary>
        /// Gets or sets the binary file type unique identifier.
        /// </summary>
        /// <value>
        /// The binary file type unique identifier.
        /// </value>
        public Guid BinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the document folder root.
        /// </summary>
        /// <value>
        /// The document folder root.
        /// </value>
        public string DocumentFolderRoot { get; set; }

        /// <summary>
        /// Gets or sets the image folder root.
        /// </summary>
        /// <value>
        /// The image folder root.
        /// </value>
        public string ImageFolderRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the root is user specific.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the root is user specific; otherwise, <c>false</c>.
        /// </value>
        public bool IsUserSpecificRoot { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        /// <value>
        /// The name of the sender.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the address of the sender.
        /// </summary>
        /// <value>
        /// The address of the sender.
        /// </value>
        public string FromAddress { get; set; }

        /// <summary>
        /// Gets or sets the recipient threshold that, once exceeded, will automatically mark the communication as a bulk email.
        /// </summary>
        /// <value>
        /// The bulk email threshold.
        /// </value>
        public int? BulkEmailThreshold { get; set; }
    }
}
