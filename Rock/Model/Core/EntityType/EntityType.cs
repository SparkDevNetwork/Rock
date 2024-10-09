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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "EntityType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ENTITY_TYPE )]
    public partial class EntityType : Entity<EntityType>, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the full name of the EntityType (including the namespace). This value is required and is an alternate key.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of the EntityType.
        /// </value>
        [MaxLength( 100 )]
        [Index( IsUnique = true )]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the assembly name of the EntityType. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Assembly Name of the EntityType.
        /// </value>
        [MaxLength( 260 )]
        [DataMember]
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the EntityType (the class name).
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the friendly name of the Entity Type.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type implements the
        /// IEntity interface.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> value that is  <c>true</c> if this instance is an entity; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsEntity { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type implements the
        /// ISecured interface.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is secured; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSecured { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether this entity type is a commonly used entity.
        /// If so, it will grouped at the top by the entity type picker control
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is common; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCommon { get; set; }

        /// <summary>
        /// Gets or sets the single value field type identifier.
        /// </summary>
        /// <value>
        /// The single value field type identifier.
        /// </value>
        [DataMember]
        public int? SingleValueFieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets the multi value field type identifier.
        /// </summary>
        /// <value>
        /// The multi value field type identifier.
        /// </value>
        [DataMember]
        public int? MultiValueFieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexing enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is indexing enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has achievements enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is achievements enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsAchievementsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is message bus event publish enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is message bus event publish enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsMessageBusEventPublishEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether attributes of this entity type support a Pre-HTML and Post-HTML option.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attributes support pre post HTML]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AttributesSupportPrePostHtml { get; set; }

        /// <summary>
        /// Gets or sets the index result template.
        /// </summary>
        /// <value>
        /// The index result template.
        /// </value>
        public string IndexResultTemplate { get; set; }

        /// <summary>
        /// Gets or sets the index document URL.
        /// </summary>
        /// <value>
        /// The index document URL.
        /// </value>
        public string IndexDocumentUrl { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating a link to view details for this entity (i.e. "~/person/{{ Entity.Id }}").
        /// </summary>
        /// <value>
        /// The link URL.
        /// </value>
        public string LinkUrlLavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether attributes of this entity type support displaying on bulk entry forms.
        /// </summary>
        [DataMember]
        public bool AttributesSupportShowOnBulk { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this entity type will automatically
        /// have <see cref="InteractionEntity"/> records created to associate the
        /// creation of new entities with the interaction that was active at the
        /// time.
        /// </summary>
        [DataMember]
        public bool IsRelatedToInteractionTrackedOnCreate { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets a flag  indicating whether this instance is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        public virtual bool IsSystem
        {
            get { return IsSecured || IsEntity; }
        }

        /// <summary>
        /// Gets or sets the type of the single value field. 
        /// This helps determine what type of control can be used to select this type of Entity (single values)
        /// </summary>
        /// <value>
        /// The type of the single value field.
        /// </value>
        public virtual FieldType SingleValueFieldType { get; set; }

        /// <summary>
        /// Gets or sets the type of the multi value field.  
        /// This helps determine what type of control can be used to select this type of Entity (multiple values)
        /// </summary>
        /// <value>
        /// The type of the multi value field.
        /// </value>
        public virtual FieldType MultiValueFieldType { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FriendlyName;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Entity Type Configuration class.
    /// </summary>
    public partial class EntityTypeConfiguration : EntityTypeConfiguration<EntityType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration"/> class.
        /// </summary>
        public EntityTypeConfiguration()
        {
            this.HasOptional( p => p.SingleValueFieldType ).WithMany().HasForeignKey( p => p.SingleValueFieldTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.MultiValueFieldType ).WithMany().HasForeignKey( p => p.MultiValueFieldTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
