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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Represents a single image attached to a financial transaction, carrying both
    /// the binary file reference and resolved URL for display in the UI.
    /// </summary>
    public class TransactionImageBag
    {
        /// <summary>
        /// Gets or sets the Guid of the underlying BinaryFile record.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the original file name of the uploaded image.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the integer Id of the BinaryFile record.
        /// </summary>
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the full public URL for displaying the image at its original size.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the public URL for displaying a thumbnail-sized version of the image.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has removed this image.
        /// When <c>true</c> the image is excluded from the UI and deleted from the
        /// transaction on save.
        /// </summary>
        public bool IsMarkedForDeletion { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFile reference as a ListItemBag, used by the ImageUploader
        /// component to track the uploaded file.
        /// </summary>
        public ListItemBag ImageItemBag { get; set; }
    }
}
