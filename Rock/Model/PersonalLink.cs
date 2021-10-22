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
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Personal Link Entity.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PersonalLink" )]
    [DataContract]
    public partial class PersonalLink : Model<PersonalLink>, IOrdered, ICacheable
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
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        [MaxLength( 2048 )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the personal link section.
        /// </summary>
        /// <value>
        /// The personal link section.
        /// </value>
        [DataMember]
        public int SectionId { get; set; }

        #endregion

        #region IOrdered

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        #endregion IOrdered

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
        /// Gets or sets the personal link section.
        /// </summary>
        /// <value>
        /// The personal link section.
        /// </value>
        [LavaVisible]
        public virtual PersonalLinkSection Section { get; set; }

        #endregion

        #region overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{this.Name} ({this.Url})";
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized for <paramref name="action"/>.
        /// In the case of non-shared link, security it limited to the person who owns that section.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns><c>true</c> if the specified action is authorized; otherwise, <c>false</c>.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            // if it is non-shared personal link, than only the person that owns the link is authorized for that link. Everybody else has NO access (including admins).
            if ( this.PersonAlias != null )
            {
                return this.PersonAlias.PersonId == person.Id;
            }

            return base.IsAuthorized( action, person );
        }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        /// <value>The parent authority.</value>
        public override ISecured ParentAuthority
        {
            get
            {
                return this.Section;
            }
        }

        #endregion

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
            }
            else
            {
                _preSaveChangesPersonAliasId = this.PersonAliasId;
            }

            var section = SharedPersonalLinkSectionCache.Get( this.SectionId );
            _preSaveChangesIsShared = section?.IsShared ?? false;
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
                // If the link was deleted, the "ModifiedDateTime" of link orders need to be updated.
                // Otherwise, we won't be able to detect that the links have changed due to deleting a record.
                new PersonalLinkSectionOrderService( dbContext as RockContext ).UpdateLinkOrdersModifiedDateTime( _preSaveChangesPersonAliasId );
            }

            var section = SharedPersonalLinkSectionCache.Get( this.SectionId );

            if ( _preSaveChangesIsShared || ( section?.IsShared == true ) )
            {
                // If this is a shared link, update the SharedPersonalLinkSectionCache
                SharedPersonalLinkSectionCache.UpdateCachedEntity( this.Id, entityState );
                SharedPersonalLinkSectionCache.FlushLastModifiedDateTime();
            }

            // Since this change probably impacts the current person's links, update the current person's link's ModifiedDateTime. 
            PersonalLinkService.PersonalLinksHelper.FlushPersonalLinksSessionDataLastModifiedDateTime();
        }

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns>IEntityCache.</returns>
        public IEntityCache GetCacheObject()
        {
            return null;
        }

        #endregion ICacheable
    }

    #region Entity Configuration

    /// <summary>
    /// Personal Link Configuration class.
    /// </summary>
    public partial class PersonalLinkConfiguration : EntityTypeConfiguration<PersonalLink>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalLinkConfiguration"/> class.
        /// </summary>
        public PersonalLinkConfiguration()
        {
            this.HasRequired( r => r.Section ).WithMany( r => r.PersonalLinks ).HasForeignKey( r => r.SectionId ).WillCascadeOnDelete( true );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}