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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.AI.Provider;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an AI provider in Rock.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AIProvider" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "945A994F-F15E-43AC-B503-A54BDE70F77F" )]
    public partial class AIProvider : Model<AIProvider>, IHasActiveFlag, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? ProviderComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

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
        public virtual EntityType ProviderComponentEntityType { get; set; }

        #endregion Navigation Properties

        #region Public Methods

        /// <summary>
        /// Gets the AI provider component.
        /// </summary>
        /// <returns></returns>
        public virtual AIProviderComponent GetAIComponent()
        {
            if ( ProviderComponentEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( ProviderComponentEntityTypeId.Value );
                if ( entityType != null )
                {
                    return AIProviderContainer.GetComponent( entityType.Name );
                }
            }

            return null;
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

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AIServiceConfiguration : EntityTypeConfiguration<AIProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AIServiceConfiguration"/> class.
        /// </summary>
        public AIServiceConfiguration()
        {
            this.HasRequired( g => g.ProviderComponentEntityType ).WithMany().HasForeignKey( a => a.ProviderComponentEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
