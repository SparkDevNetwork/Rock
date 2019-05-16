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
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a collection or group of entity objects that share one or more common characteristics . A tag can either be private (owned by an individual <see cref="Rock.Model.Person"/>)
    /// or public.
    /// </summary>
    [RockDomain( "Core" )]
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
        [DataMember]
        public int? EntityTypeId { get; set; }

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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

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

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the parent security authority of this Tag. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.CategoryId.HasValue )
                {
                    return Rock.Web.Cache.CategoryCache.Get( this.CategoryId.Value ) ?? base.ParentAuthority;
                }

                return this.Category != null ? this.Category : base.ParentAuthority;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.TAG, "The roles and/or users that have access to tag items." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }
                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;
        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified action is authorized. If the tag is personal and
        /// owned by the person, it returns true, but if it's personal and NOT owned by the person
        /// it returns false -- otherwise (not a personal tag) it returns what the chain of authority
        ///  determines, but note: the parent authority is the category (if the tag has a category).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>True if the person is authorized; false otherwise.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.OwnerPersonAlias != null && person != null && this.OwnerPersonAlias.PersonId == person.Id )
            {
                // always allow people to do anything with their own tags
                return true;
            }
            else if ( this.OwnerPersonAlias != null && person != null && this.OwnerPersonAlias.PersonId != person.Id )
            {
                // always prevent people from doing anything with someone else's tags
                return false;
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
            this.HasOptional( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
