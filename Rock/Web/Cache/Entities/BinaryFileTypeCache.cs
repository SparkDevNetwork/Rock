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

using System;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a binary file type which is used when rendering the file.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class BinaryFileTypeCache : ModelCache<BinaryFileTypeCache, BinaryFileType>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this BinaryFileType is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the given Name of the BinaryFileType. This value is an alternate key and is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the BinaryFileType. 
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a description of the BinaryFileType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the BinaryFileType.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the CSS class that is used for a vector/CSS icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class that is used for a vector/CSS based icon.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the Id of the storage service <see cref="Rock.Model.EntityType"/> that is used to store files of this type.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32" /> representing the Id of the storage service <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public int? StorageEntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the file on any <see cref="Rock.Model.BinaryFile"/> child entities should be cached to the server.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is <c>true</c> if caching to the server is allowed; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CacheToServerFileSystem { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether security should be checked when displaying files of this type
        /// </summary>
        /// <value>
        /// <c>true</c> if [requires view security]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresViewSecurity { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum width of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum width in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxWidth { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum height of a file type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the maximum height in pixels of a file type.
        /// </value>
        [DataMember]
        public int? MaxHeight { get; private set; }

        /// <summary>
        /// Gets or sets the preferred format of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Format"/> enum value that represents the preferred format of the file type.
        /// </value>
        [DataMember]
        public Format PreferredFormat { get; private set; }

        /// <summary>
        /// Gets or sets the preferred resolution of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Resolution"/> enum value that represents the preferred resolution of the file type.
        /// </value>
        [DataMember]
        public Resolution PreferredResolution { get; private set; }

        /// <summary>
        /// Gets or sets the preferred color depth of the file type.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.ColorDepth"/> enum value that represents the preferred color depth of the file type.
        /// </value>
        [DataMember]
        public ColorDepth PreferredColorDepth { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the preferred attributes of the file type are required
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the "preferred" attributes are required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PreferredRequired { get; private set; }

        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        /// <value>
        /// The cache control header settings.
        /// </value>
        [DataMember]
        public string CacheControlHeaderSettings { get; private set; }

        /// <summary>
        /// Gets the cache control header.
        /// </summary>
        /// <value>
        /// The cache control header.
        /// </value>
        [DataMember]
        public RockCacheability CacheControlHeader { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var blockType = entity as BinaryFileType;
            if ( blockType == null )
            {
                return;
            }

            IsSystem = blockType.IsSystem;
            CacheControlHeader = blockType.CacheControlHeader;
            Name = blockType.Name;
            Description = blockType.Description;
            CacheControlHeaderSettings = blockType.CacheControlHeaderSettings;
            CacheToServerFileSystem = blockType.CacheToServerFileSystem;
            Guid = blockType.Guid;
            IconCssClass = blockType.IconCssClass;
            Id = blockType.Id;
            StorageEntityTypeId = blockType.StorageEntityTypeId;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
