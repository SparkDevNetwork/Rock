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

namespace Rock.ViewModels.Blocks.Crm.DocumentList
{
    /// <summary>
    /// Describes a document type that can be selected when adding a new
    /// document, along with the metadata the editor needs once it is selected.
    /// </summary>
    public class DocumentTypeListItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the document type.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the display name of the document type.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the binary file type that the
        /// file uploader should use when uploading a file for this document type.
        /// </summary>
        public Guid BinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the suggested document name for this type. This is the
        /// document type's default name template with merge fields already
        /// resolved, used to pre-fill the name when the type is selected.
        /// </summary>
        public string NameTemplate { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for the document type.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
