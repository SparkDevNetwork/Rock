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

namespace Rock.ViewModels.Blocks.Group.GroupMemberDetail
{
    /// <summary>
    /// The required signature document state for the Group Member Detail
    /// block's Administration section.
    /// </summary>
    public class SignatureDocumentStatusBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the group requires a
        /// signature document and none has been signed yet.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the signature document template name, shown as the
        /// signed document uploader label.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the unsigned-document status message shown in the
        /// warning alert.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the send button text ("Send Signature Request" or
        /// "Resend Signature Request"). Pending Open Decision A.
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// Gets or sets the binary file type guid used by the signed
        /// document uploader.
        /// </summary>
        public Guid? BinaryFileTypeGuid { get; set; }
    }
}
