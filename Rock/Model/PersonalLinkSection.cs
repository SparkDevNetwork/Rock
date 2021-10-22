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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#if NET5_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
using DbEntityEntry = Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry;
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
#endif
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// This represents a section that a link can belong to.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PersonalLinkSection" )]
    [DataContract]
    public partial class PersonalLinkSection : Model<PersonalLinkSection>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Link Section is shared.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Link Section is shared; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsShared { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the personal links.
        /// </summary>
        /// <value>
        /// The personal links.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonalLink> PersonalLinks
        {
            get { return _personalLinks ?? ( _personalLinks = new Collection<PersonalLink>() ); }
            set { _personalLinks = value; }
        }

        private ICollection<PersonalLink> _personalLinks;

        /// <summary>
        /// Gets or sets the personal links.
        /// </summary>
        /// <value>
        /// The personal links.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonalLinkSectionOrder> PersonalLinkSectionOrders
        {
            get { return _personalLinkSectionOrders ?? ( _personalLinkSectionOrders = new Collection<PersonalLinkSectionOrder>() ); }
            set { _personalLinkSectionOrders = value; }
        }

        private ICollection<PersonalLinkSectionOrder> _personalLinkSectionOrders;

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized for <paramref name="action"/>.
        /// In the case of non-shared link section, security it limited to the person who owns that section.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if the specified action is authorized; otherwise, <c>false</c>.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // if it is non-shared personal link, than only the person that owns the link is authorized for that link. Everybody else has NO access (including admins).
            if ( !this.IsShared && this.PersonAlias != null )
            {
                return this.PersonAlias.PersonId == person.Id;
            }

            return base.IsAuthorized( action, person );
        }

        #endregion Overrides

        #region ICacheable

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry, EntityState state )
        {
            if ( entry.State == EntityState.Deleted || entry.State == EntityState.Modified )
            {
                _preSaveChangesPersonAliasId = ( int? ) ( entry.OriginalValues?["PersonAliasId"] );
                _preSaveChangesIsShared = ( bool? ) ( entry.OriginalValues?["IsShared"] ) ?? false;
            }
            else
            {
                _preSaveChangesPersonAliasId = this.PersonAliasId;
                _preSaveChangesIsShared = this.IsShared;
            }
            
            base.PreSaveChanges( dbContext, entry, state );
        }

        private int? _preSaveChangesPersonAliasId = null;
        private bool _preSaveChangesIsShared = false;

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            if ( entityState == EntityState.Deleted )
            {
                // If the link section was deleted, the "ModifiedDateTime" of link orders need to be updated.
                // Otherwise, we won't be able to detect that the links have changed due to deleting a record.
                new PersonalLinkSectionOrderService( dbContext as RockContext ).UpdateLinkOrdersModifiedDateTime( _preSaveChangesPersonAliasId );
            }

            if ( _preSaveChangesIsShared || this.IsShared )
            {
                // If this is a shared section, update the SharedPersonalLinkSectionCache
                SharedPersonalLinkSectionCache.UpdateCachedEntity( this.Id, entityState );
                SharedPersonalLinkSectionCache.FlushLastModifiedDateTime();
            }

            // Since this change probably impacts the current person's links, flush the current person's link's ModifiedDateTime 
            PersonalLinkService.PersonalLinksHelper.FlushPersonalLinksSessionDataLastModifiedDateTime();
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns>IEntityCache.</returns>
        public IEntityCache GetCacheObject()
        {
            return SharedPersonalLinkSectionCache.Get( this.Id );
        }

        #endregion ICacheable
    }

    #region Entity Configuration

    /// <summary>
    /// Personal Link Section Configuration class.
    /// </summary>
    public partial class PersonalLinkSectionConfiguration : EntityTypeConfiguration<PersonalLinkSection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalLinkSectionConfiguration"/> class.
        /// </summary>
        public PersonalLinkSectionConfiguration()
        {
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
