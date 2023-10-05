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
using Rock.Model;
using System.Collections.Generic;
using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Core.BinaryFileTypeDetail
{
    public class BinaryFileTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        public RockCacheabilityBag CacheControlHeaderSettings { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the file on any Rock.Model.BinaryFile child entities should be cached to the server.
        /// </summary>
        public bool CacheToServerFileSystem { get; set; }

        /// <summary>
        /// Gets or sets a description of the BinaryFileType.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this BinaryFileType is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size bytes.
        /// </summary>
        public int? MaxFileSizeBytes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum height of a file type.
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum width of a file type.
        /// </summary>
        public int? MaxWidth { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the BinaryFileType. This value is an alternate key and is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the preferred color depth of the file type.
        /// </summary>
        public ColorDepth PreferredColorDepth { get; set; }

        /// <summary>
        /// Gets or sets the preferred format of the file type.
        /// </summary>
        public Format PreferredFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preferred attributes of the file type are required
        /// </summary>
        public bool PreferredRequired { get; set; }

        /// <summary>
        /// Gets or sets the preferred resolution of the file type.
        /// </summary>
        public Resolution PreferredResolution { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether security should be checked when displaying files of this type
        /// </summary>
        public bool RequiresViewSecurity { get; set; }

        /// <summary>
        /// Gets or sets the storage mode Rock.Model.EntityType.
        /// </summary>
        public ListItemBag StorageEntityType { get; set; }

        /// <summary>
        /// Gets or sets the binary file type attributes.
        /// </summary>
        /// <value>
        /// The binary file type attributes.
        /// </value>
        public List<PublicEditableAttributeBag> BinaryFileTypeAttributes { get; set; }
        public bool RestrictedEdit { get; set; }
        public string EditModeMessage { get; set; }
    }
}
