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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Content Channel Type Cache
    /// </summary>
    [Serializable]
    [DataContract]
    public class ContentTopicCache : ModelCache<ContentTopicCache, ContentTopic>
    {
        #region Properties

        /// <inheritdoc cref="Rock.Model.ContentTopic.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopic.IsSystem" />
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopic.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopic.Description" />
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopic.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopic.ContentTopicDomainId" />
        [DataMember]
        public int ContentTopicDomainId { get; private set; }

        /// <summary>
        /// Gets the content topic domain.
        /// </summary>
        /// <value>
        /// The content topic domain.
        /// </value>
        public ContentTopicDomainCache ContentTopicDomain => ContentTopicDomainCache.All().Where( ctd => ctd.Id == ContentTopicDomainId ).FirstOrDefault();

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var contentTopic = entity as ContentTopic;

            if ( contentTopic == null )
            {
                return;
            }

            Description = contentTopic.Description;
            Guid = contentTopic.Guid;
            Id = contentTopic.Id;
            IsActive = contentTopic.IsActive;
            IsSystem = contentTopic.IsSystem;
            Name = contentTopic.Name;
            Order =  contentTopic.Order;
            ContentTopicDomainId = contentTopic.ContentTopicDomainId;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
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