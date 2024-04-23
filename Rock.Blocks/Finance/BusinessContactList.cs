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
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BusinessContactList;
using Rock.Web.Cache;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of group members.
    /// </summary>
    [DisplayName( "Business Contact List" )]
    [Category( "Finance" )]
    [Description( "Displays the list of contacts for a business." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Person Profile Page",
        Description = "The page used to view the details of a business contact.",
        Key = AttributeKey.PersonProfilePage )]

    [SystemGuid.EntityTypeGuid( "49ef69c9-b893-4684-be71-8d8bc8905b06" )]
    [SystemGuid.BlockTypeGuid( "5e72c18d-f459-4226-820b-b47f88efeb0f" )]
    [CustomizedGrid]
    public class BusinessContactList : RockEntityListBlockType<Person>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class NavigationUrlKey
        {
            public const string PersonProfilePage = "PersonProfilePage";
        }

        private static class PageParameterKey
        {
            public const string BusinessId = "BusinessId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BusinessContactListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BusinessContactListOptionsBag GetBoxOptions()
        {
            var options = new BusinessContactListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, "PersonId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Person> GetListQueryable( RockContext rockContext )
        {
            var businessId = GetBusinessId();
            IEnumerable<Person> businesses = new List<Person>();

            if ( businessId.HasValue )
            {
                var groupRoleBusinessGuid = SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
                var groupRoleOwnerGuidGuid = SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

                businesses = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( g => g.GroupRole.Guid.Equals( groupRoleBusinessGuid ) && g.PersonId == businessId )
                    .SelectMany( g => g.Group.Members.Where( m => m.GroupRole.Guid.Equals( groupRoleOwnerGuidGuid ) )
                    .Select( m => m.Person ) );
            }

            return businesses.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<Person> GetGridBuilder()
        {
            return new GridBuilder<Person>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.FullName );
        }

        /// <summary>
        /// Gets the business identifier.
        /// </summary>
        /// <returns></returns>
        private int? GetBusinessId()
        {
            var businessId = PageParameter( PageParameterKey.BusinessId ).AsIntegerOrNull();

            if ( !businessId.HasValue )
            {
                businessId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.BusinessId ) );
            }

            return businessId;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {GroupMember.FriendlyTypeName}." );
            }

            var businessId = GetBusinessId();

            if ( !businessId.HasValue )
            {
                return ActionBadRequest( "Invalid BusinessId." );
            }

            var entityService = new GroupMemberService( RockContext );

            var businessContactId = Rock.Utility.IdHasher.Instance.GetId( key );
            var businessContact = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS_CONTACT.AsGuid();
            var business = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_BUSINESS.AsGuid();
            var ownerGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();

            var groupMembers = entityService
                .Queryable()
                .Where( m =>
                (
                    // The contact person in the business's known relationships
                    m.PersonId == businessContactId &&
                    m.GroupRole.Guid.Equals( businessContact ) &&
                    m.Group.Members.Any( o => o.PersonId == businessId && o.GroupRole.Guid.Equals( ownerGuid ) )
                ) ||
                (
                    // The business in the person's know relationships
                    m.PersonId == businessId &&
                    m.GroupRole.Guid.Equals( business ) &&
                    m.Group.Members.Any( o => o.PersonId == businessContactId && o.GroupRole.Guid.Equals( ownerGuid ) )
                ) );

            foreach ( var groupMember in groupMembers )
            {
                entityService.Delete( groupMember );
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="bag">The bag that contains all the information required to save the business contact.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( BusinessContactBag bag )
        {
            var businessId = GetBusinessId();
            var personGuid = bag.Contact.Value.AsGuidOrNull();

            if ( !businessId.HasValue )
            {
                return ActionBadRequest( "Invalid BusinessId." );
            }

            if ( !personGuid.HasValue )
            {
                return ActionBadRequest( "Invalid Contact." );
            }

            var personService = new PersonService( RockContext );
            var business = personService.Get( businessId.Value );
            var contactId = new PersonAliasService( RockContext ).GetSelect( personGuid.Value, p => p.PersonId );

            if ( contactId > 0 )
            {
                personService.AddContactToBusiness( business.Id, contactId );
                RockContext.SaveChanges();

                return ActionOk();
            }
            else
            {
                return ActionBadRequest( "Invalid Contact." );
            }
        }

        #endregion
    }
}
