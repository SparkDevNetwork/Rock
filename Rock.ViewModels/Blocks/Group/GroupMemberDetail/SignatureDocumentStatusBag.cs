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
    /// The signed document uploader configuration for the Group Member
    /// Detail block's Administration section.
    /// </summary>
    public class SignatureDocumentStatusBag
    {
        /// <summary>
        /// Gets or sets the signature document template name, shown as the
        /// signed document uploader label.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the binary file type guid used by the signed
        /// document uploader.
        /// </summary>
        public Guid? BinaryFileTypeGuid { get; set; }
    }
}
