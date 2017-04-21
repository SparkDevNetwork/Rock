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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a collection or group of entity objects that share one or more common characteristics . A tag can either be private (owned by an individual <see cref="Rock.Model.Person"/>)
    /// or public.
    /// </summary>
    [Table( "Tag" )]
    [DataContract]
    public partial class Tag : Model<Tag>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Tag is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Tag is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> containing the entities that can use this Tag. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that contains the entities that can use this Tag.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the column/property that contains the value that can narrow the scope of entities that can receive this Tag. Entities where this 
        /// column contains the <see cref="EntityTypeQualifierValue"/> will be eligible to have this Tag. This property must be used in conjunction with the <see cref="EntityTypeQualifierValue"/>
        /// property. If all entities of the the specified <see cref="Rock.Model.EntityType"/> are eligible to use this Tag, this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the EntityTypeQualifierColumn.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }
        
        /// <summary>
        /// Gets or sets the value in the <see cref="EntityTypeQualifierColumn"/> that narrows the scope of entities that can receive this Tag. Entities that contain this value 
        /// in the <see cref="EntityTypeQualifierColumn"/> are eligible to use this Tag. This property must be used in conjunction with the <see cref="EntityTypeQualifierColumn"/> property.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the EntityTypeQualiferValue that limits which entities of the specified EntityType that can use this Tag.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the Tag. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Tag
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order of the tag. the lower the number, the higher display priority that the Tag has.  For example the Tags with the lower Order could be displayed higher on the Tag list.
        /// This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the display Order of the Tag.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the owner person alias identifier.
        /// </summary>
        /// <value>
        /// The owner person alias identifier.
        /// </value>
        [DataMember]
        public int? OwnerPersonAliasId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the owner person alias.
        /// </summary>
        /// <value>
        /// The owner person alias.
        /// </value>
        [LavaInclude]
        public virtual Model.PersonAlias OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the Entities that this Tag can be applied to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of Entities that this Tag can be applied to.
        /// </value>
        [DataMember]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.TaggedItem">TaggedItems</see> representing the entities that are tagged with this Tag.
        /// </summary>
        /// <value>
        /// A collection containing of <see cref="Rock.Model.TaggedItem">TaggedItems</see> representing the entities that use this tag.
        /// </value>
        [LavaInclude]
        public virtual ICollection<TaggedItem> TaggedItems { get; set; }
        
        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.OwnerPersonAlias != null && this.OwnerPersonAlias.Person != null && person != null && this.OwnerPersonAlias.PersonId == person.Id )
            {
                // always allow people to delete their own tags
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Tag.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Tag.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration
    
    /// <summary>
    /// Tag Configuration class.
    /// </summary>
    public partial class TagConfiguration : EntityTypeConfiguration<Tag>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagConfiguration" /> class.
        /// </summary>
        public TagConfiguration()
        {
            this.HasOptional( p => p.OwnerPersonAlias ).WithMany().HasForeignKey( p => p.OwnerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
