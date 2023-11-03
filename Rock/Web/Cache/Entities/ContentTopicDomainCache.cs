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
    public class ContentTopicDomainCache : ModelCache<ContentTopicDomainCache, ContentTopicDomain>
    {
        #region Properties

        /// <inheritdoc cref="Rock.Model.ContentTopicDomain.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopicDomain.IsSystem" />
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopicDomain.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopicDomain.Description" />
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="Rock.Model.ContentTopicDomain.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets the content topics for this domain.
        /// </summary>
        /// <value>
        /// The content topics.
        /// </value>
        public List<ContentTopicCache> ContentTopics => ContentTopicCache.All().Where( ct => ct.ContentTopicDomainId == Id ).ToList();

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );
            var contentTopicDomain = entity as ContentTopicDomain;

            if ( contentTopicDomain == null )
            {
                return;
            }

            Description = contentTopicDomain.Description;
            Guid = contentTopicDomain.Guid;
            Id = contentTopicDomain.Id;
            IsActive = contentTopicDomain.IsActive;
            IsSystem = contentTopicDomain.IsSystem;
            Name = contentTopicDomain.Name;
            Order =  contentTopicDomain.Order;
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