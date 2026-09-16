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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.DocumentList
{
    /// <summary>
    /// The information needed to add or edit a single document in the modal.
    /// </summary>
    public class DocumentBag
    {
        /// <summary>
        /// Gets or sets the identifier of the document. This is empty when
        /// adding a new document.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the document.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the selected document type. The <c>Value</c> contains
        /// the document type's unique identifier.
        /// </summary>
        public ListItemBag DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the uploaded file. The <c>Value</c> contains the binary
        /// file's unique identifier and the <c>Text</c> contains its file name.
        /// </summary>
        public ListItemBag BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the binary file type that the
        /// file uploader should use for the currently selected document type.
        /// </summary>
        public Guid? BinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document type is
        /// read-only. This is <c>true</c> when editing an existing document,
        /// since a document's type cannot be changed after creation.
        /// </summary>
        public bool IsDocumentTypeReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the document types available to choose from when adding
        /// a new document. This is only populated when adding (not editing).
        /// </summary>
        public List<DocumentTypeListItemBag> AvailableDocumentTypes { get; set; }
    }
}
