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
using Rock.AI.Provider;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache.Entities
{
    /// <summary>
    /// Cache object for <see cref="AIProvider"/>.
    /// </summary>
    [Serializable]
    [DataContract]
    public class AIProviderCache : ModelCache<AIProviderCache, AIProvider>
    {
        #region Fields

        private AIProviderComponent _AIComponent = null;

        private readonly object _AIComponentLock = new object();

        #endregion Fields

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityTypeCache EntityType
        {
            get
            {
                if ( EntityTypeId.HasValue )
                {
                    return EntityTypeCache.Get( EntityTypeId.Value );
                }

                return null;
            }
        }

        #endregion Navigation Properties

        #region Additional Properties

        /// <summary>
        /// Gets the asset storage component.
        /// </summary>
        public AIProviderComponent AIComponent
        {
            get
            {
                if ( _AIComponent == null && EntityTypeId.HasValue )
                {
                    lock ( _AIComponentLock )
                    {
                        if ( _AIComponent == null )
                        {
                            var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                            if ( entityType != null )
                            {
                                _AIComponent = AIProviderContainer.GetComponent( entityType.Name );
                            }
                        }
                    }
                }

                return _AIComponent;
            }
        }

        #endregion Additional Properties

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var AIProvider = entity as AIProvider;
            if ( AIProvider == null )
            {
                return;
            }

            this.Name = AIProvider.Name;
            this.Order = AIProvider.Order;
            this.EntityTypeId = AIProvider.ProviderComponentEntityTypeId;
            this.IsActive = AIProvider.IsActive;
            this.Description = AIProvider.Description;

            lock ( _AIComponentLock )
            {
                _AIComponent = AIProvider.GetAIComponent();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Creates an <see cref="AIProvider"/> instance and sets its properties from this cached object's properties.
        /// </summary>
        /// <returns>An <see cref="AIProvider"/> instance representing this cached object.</returns>
        public AIProvider ToEntity()
        {
            return new AIProvider
            {
                Id = this.Id,
                Name = this.Name,
                Order = this.Order,
                ProviderComponentEntityTypeId = this.EntityTypeId,
                IsActive = this.IsActive,
                Description = this.Description,
                CreatedDateTime = this.CreatedDateTime,
                ModifiedDateTime = this.ModifiedDateTime,
                Guid = this.Guid,
                ForeignId = this.ForeignId,
                ForeignGuid = this.ForeignGuid,
                ForeignKey = this.ForeignKey,

                Attributes = this.Attributes,
                AttributeValues = this.AttributeValues
            };
        }

        #endregion Public Methods
    }
}
