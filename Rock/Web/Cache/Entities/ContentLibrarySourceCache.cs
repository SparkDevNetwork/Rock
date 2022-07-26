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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a content library source required to obtain fast
    /// access to the library system.
    /// </summary>
    [Serializable]
    [DataContract]
    internal class ContentLibrarySourceCache : ModelCache<ContentLibrarySourceCache, ContentLibrarySource>
    {
        #region Properties

        /// <inheritdoc cref="ContentLibrarySource.EntityTypeId"/>
        [DataMember]
        public int EntityTypeId { get; private set; }

        /// <inheritdoc cref="ContentLibrarySource.EntityId"/>
        [DataMember]
        public int EntityId { get; private set; }

        /// <inheritdoc cref="ContentLibrarySource.OccurrencesToShow"/>
        [DataMember]
        public int OccurrencesToShow { get; private set; }

        /// <inheritdoc cref="ContentLibrarySource.ContentLibraryId"/>
        [DataMember]
        public int ContentLibraryId { get; private set; }

        /// <summary>
        /// Gets the entity type cache object for the source entity.
        /// </summary>
        /// <value>
        /// The entity type cache object for the source entity.
        /// </value>
        public EntityTypeCache EntityType => EntityTypeCache.Get( EntityTypeId );

        /// <summary>
        /// Gets the content library cache object this source belongs to.
        /// </summary>
        /// <value>
        /// The content library cache object this source belongs to.
        /// </value>
        public ContentLibraryCache ContentLibrary => ContentLibraryCache.Get( ContentLibraryId );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is ContentLibrarySource contentLibrarySource ) )
            {
                return;
            }

            EntityTypeId = contentLibrarySource.EntityTypeId;
            EntityId = contentLibrarySource.EntityId;
            OccurrencesToShow = contentLibrarySource.OccurrencesToShow;
            ContentLibraryId = contentLibrarySource.ContentLibraryId;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{EntityType?.Name ?? EntityTypeId.ToString()}:{EntityId}";
        }

        #endregion
    }
}