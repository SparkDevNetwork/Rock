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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Crm.PersonDetail.Relationships;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Renders the related members of a person's relationship group, such as
    /// the Known Relationships group, and allows them to be managed.
    /// </summary>

    [DisplayName( "Relationships" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view relationships of a particular person." )]
    [IconCssClass( "ti ti-users" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupRoleField( null,
        "Group Type/Role Filter",
        Description = "The Group Type and role to display other members from.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.GroupTypeRoleFilter )]

    [BooleanField(
        "Show Role",
        Key = AttributeKey.ShowRole,
        Description = "Should the member's role be displayed with their name",
        Order = 1 )]

    [BooleanField(
        "Create Group",
        Key = AttributeKey.CreateGroup,
        Description = "Should group be created if a group/role cannot be found for the current person.",
        DefaultBooleanValue = true,
        Order = 2 )]

    [IntegerField(
        "Max Relationships To Display",
        Key = AttributeKey.MaxRelationshipsToDisplay,
        Description = "The maximum number of relationships to display.",
        IsRequired = false,
        DefaultIntegerValue = 50,
        Order = 3 )]

    #endregion Block Attributes

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "2F665370-0EEB-4C26-925D-B8847AFF693D" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "16196C62-44AB-48E0-AA0D-FD486251DD58" )]
    [Rock.SystemGuid.BlockTypeGuid( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD" )]
    public class Relationships : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupTypeRoleFilter = "GroupType/RoleFilter";
            public const string ShowRole = "ShowRole";
            public const string CreateGroup = "CreateGroup";
            public const string MaxRelationshipsToDisplay = "MaxRelationshipsToDisplay";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The attribute key that marks a group type role as having an
        /// inverse relationship role.
        /// </summary>
        private const string InverseRelationshipAttributeKey = "InverseRelationship";

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return new RelationshipsOptionsBag { IsVisible = false };
            }

            var ownerRole = GetOwnerRole();
            var group = GetRelationshipGroup( person, ownerRole, GetAttributeValue( AttributeKey.CreateGroup ).AsBoolean() );

            var options = new RelationshipsOptionsBag
            {
                IsVisible = true,
                Title = group != null ? group.Name.Pluralize() : BlockCache.Name,
                CanEdit = CanEditRelationships( group, ownerRole ),
                IsRoleNameVisible = GetAttributeValue( AttributeKey.ShowRole ).AsBoolean(),
                GroupTypeGuid = ownerRole?.GroupTypeId != null ? GroupTypeCache.Get( ownerRole.GroupTypeId.Value )?.Guid : null,
                OwnerRoleGuid = ownerRole?.Guid,
                Relationships = new List<RelationshipBag>()
            };

            if ( group != null )
            {
                if ( group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                {
                    options.Relationships = GetRelationshipBags( group.Id, person.Id );
                }
                else
                {
                    options.AccessWarningMessage = $"You do not have security rights to view {group.Name.Pluralize()}.";
                }
            }

            return options;
        }

        /// <summary>
        /// Gets the person whose relationships are displayed, either from the
        /// page context or the page parameter.
        /// </summary>
        /// <returns>The resolved person or <c>null</c>.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();

            if ( person != null )
            {
                return person;
            }

            var personKey = PageParameter( PageParameterKey.PersonId );

            if ( personKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the configured owner role. The owner is the member that all
        /// other members of the relationship group relate back to.
        /// </summary>
        /// <returns>The owner role or <c>null</c> when not configured.</returns>
        private GroupTypeRoleCache GetOwnerRole()
        {
            var ownerRoleGuid = GetAttributeValue( AttributeKey.GroupTypeRoleFilter ).AsGuidOrNull();

            return ownerRoleGuid.HasValue ? GroupTypeRoleCache.Get( ownerRoleGuid.Value ) : null;
        }

        /// <summary>
        /// Determines whether the owner role supports inverse relationships,
        /// meaning a matching relationship is maintained on the related
        /// person's group whenever a relationship is added, changed or
        /// removed.
        /// </summary>
        /// <param name="ownerRole">The owner role to inspect.</param>
        /// <returns><c>true</c> when inverse relationships are maintained.</returns>
        private bool IsInverseRelationshipsOwner( GroupTypeRoleCache ownerRole )
        {
            if ( ownerRole == null || !ownerRole.Attributes.ContainsKey( InverseRelationshipAttributeKey ) )
            {
                return false;
            }

            return ownerRole.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() || ownerRole.IsLeader;
        }

        /// <summary>
        /// Determines whether the current person may add, edit, and remove
        /// relationships in the group.
        /// </summary>
        /// <param name="group">The relationship group.</param>
        /// <param name="ownerRole">The configured owner role.</param>
        /// <returns><c>true</c> when relationships can be managed.</returns>
        private bool CanEditRelationships( Model.Group group, GroupTypeRoleCache ownerRole )
        {
            /*
                7/7/26 - MSE

                Add, edit, and remove all require the owner role to support
                inverse relationships (e.g. Known Relationships). The legacy
                block only enforced this for the add button; a 2014 change
                accidentally dropped the check from the per-row edit and
                remove buttons, letting them appear for view-only
                configurations such as Peer Network, whose members are
                recalculated by a job.

                Reason: Restore the intended inverse-owner gate for all relationship edits.
            */
            if ( !IsInverseRelationshipsOwner( ownerRole ) )
            {
                return false;
            }

            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                && group?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) == true;
        }

        /// <summary>
        /// Gets the person's relationship group, optionally creating it when
        /// it does not exist yet.
        /// </summary>
        /// <param name="person">The person who owns the group.</param>
        /// <param name="ownerRole">The configured owner role.</param>
        /// <param name="createIfMissing">Whether to create the group when the person does not have one.</param>
        /// <returns>The relationship group or <c>null</c>.</returns>
        private Model.Group GetRelationshipGroup( Person person, GroupTypeRoleCache ownerRole, bool createIfMissing )
        {
            if ( ownerRole == null )
            {
                return null;
            }

            var memberService = new GroupMemberService( RockContext );
            var group = memberService.Queryable( true )
                .Where( m =>
                    m.PersonId == person.Id &&
                    m.GroupRoleId == ownerRole.Id )
                .Select( m => m.Group )
                .FirstOrDefault();

            if ( group != null || !createIfMissing )
            {
                return group;
            }

            var groupType = ownerRole.GroupTypeId.HasValue ? GroupTypeCache.Get( ownerRole.GroupTypeId.Value ) : null;

            if ( groupType == null )
            {
                return null;
            }

            group = new Model.Group
            {
                Name = groupType.Name,
                GroupTypeId = groupType.Id
            };

            new GroupService( RockContext ).Add( group );

            // Save the group first so its identifier exists when the owner
            // member is added, which keeps the history records correct.
            RockContext.SaveChanges();

            var ownerMember = new GroupMember
            {
                PersonId = person.Id,
                GroupRoleId = ownerRole.Id,
                GroupId = group.Id
            };

            memberService.Add( ownerMember );
            RockContext.SaveChanges();

            return group;
        }

        /// <summary>
        /// Gets the relationships to display for the group, excluding the
        /// person who owns the group.
        /// </summary>
        /// <param name="groupId">The identifier of the relationship group.</param>
        /// <param name="personId">The identifier of the person who owns the group.</param>
        /// <returns>A list of <see cref="RelationshipBag"/>.</returns>
        private List<RelationshipBag> GetRelationshipBags( int groupId, int personId )
        {
            var maxRelationshipsToDisplay = GetAttributeValue( AttributeKey.MaxRelationshipsToDisplay ).AsIntegerOrNull();

            IQueryable<GroupMember> memberQry = new GroupMemberService( RockContext )
                .GetByGroupId( groupId, true )
                .Where( m => m.PersonId != personId )
                .OrderBy( m => m.Person.LastName )
                .ThenBy( m => m.Person.FirstName );

            if ( maxRelationshipsToDisplay.HasValue )
            {
                memberQry = memberQry.Take( maxRelationshipsToDisplay.Value );
            }

            var members = memberQry
                .Select( m => new
                {
                    m.Id,
                    m.Person,
                    PrimaryAliasGuid = m.Person.Aliases
                        .Where( a => a.AliasPersonId == m.PersonId )
                        .Select( a => ( Guid? ) a.Guid )
                        .FirstOrDefault(),
                    RoleName = m.GroupRole.Name,
                    RoleGuid = m.GroupRole.Guid
                } )
                .ToList();

            return members
                .Select( m => new RelationshipBag
                {
                    IdKey = IdHasher.Instance.GetHash( m.Id ),
                    Person = new ListItemBag
                    {
                        Value = m.PrimaryAliasGuid?.ToString(),
                        Text = m.Person.FullName
                    },
                    PersonId = m.Person.Id,
                    RoleName = m.RoleName,
                    Role = new ListItemBag
                    {
                        Value = m.RoleGuid.ToString(),
                        Text = m.RoleName
                    },
                    IsDeceased = m.Person.IsDeceased
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Adds a new relationship or updates an existing one, keeping the
        /// inverse relationship on the related person in sync.
        /// </summary>
        /// <param name="bag">The relationship to save.</param>
        /// <returns>The refreshed list of relationships.</returns>
        [BlockAction]
        public BlockActionResult SaveRelationship( SaveRelationshipRequestBag bag )
        {
            var personAliasGuid = bag?.Person?.Value.AsGuidOrNull();
            var roleGuid = bag?.Role?.Value.AsGuidOrNull();

            if ( !personAliasGuid.HasValue || !roleGuid.HasValue )
            {
                return ActionBadRequest( "A person and relationship type are required." );
            }

            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "Person not found." );
            }

            var ownerRole = GetOwnerRole();
            var group = GetRelationshipGroup( person, ownerRole, false );

            if ( group == null )
            {
                return ActionBadRequest( "The relationship group could not be found." );
            }

            if ( !CanEditRelationships( group, ownerRole ) )
            {
                return ActionForbidden( "Not authorized to edit relationships." );
            }

            var role = GroupTypeRoleCache.Get( roleGuid.Value );

            if ( role == null || role.GroupTypeId != ownerRole.GroupTypeId || role.Id == ownerRole.Id )
            {
                return ActionBadRequest( "Invalid relationship type." );
            }

            // The person picker emits a person alias unique identifier, so
            // resolve it to the person it belongs to.
            var relatedPersonId = new PersonAliasService( RockContext ).GetPersonId( personAliasGuid.Value );

            if ( !relatedPersonId.HasValue )
            {
                return ActionBadRequest( "The selected person could not be found." );
            }

            var memberService = new GroupMemberService( RockContext );
            GroupMember groupMember = null;

            if ( bag.GroupMemberIdKey.IsNotNullOrWhiteSpace() )
            {
                groupMember = memberService.Get( bag.GroupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( groupMember == null || groupMember.GroupId != group.Id )
                {
                    return ActionBadRequest( "Relationship not found." );
                }
            }

            var isInverseRelationshipsOwner = IsInverseRelationshipsOwner( ownerRole );

            // Capture the inverse of the existing relationship before it
            // changes so an orphaned inverse can be cleaned up afterwards.
            GroupMember formerInverseGroupMember = null;

            if ( isInverseRelationshipsOwner && groupMember != null )
            {
                formerInverseGroupMember = memberService.GetInverseRelationship( groupMember, false );
            }

            if ( groupMember == null )
            {
                groupMember = new GroupMember
                {
                    GroupId = group.Id
                };

                memberService.Add( groupMember );
            }

            groupMember.PersonId = relatedPersonId.Value;
            groupMember.GroupRoleId = role.Id;

            RockContext.SaveChanges();

            if ( isInverseRelationshipsOwner )
            {
                var inverseGroupMember = memberService.GetInverseRelationship( groupMember, GetAttributeValue( AttributeKey.CreateGroup ).AsBoolean() );

                if ( inverseGroupMember != null )
                {
                    RockContext.SaveChanges();

                    if ( formerInverseGroupMember != null && formerInverseGroupMember.Id != inverseGroupMember.Id )
                    {
                        memberService.Delete( formerInverseGroupMember );
                        RockContext.SaveChanges();
                    }
                }
            }

            return ActionOk( GetRelationshipBags( group.Id, person.Id ) );
        }

        /// <summary>
        /// Removes a relationship along with its inverse relationship on the
        /// related person.
        /// </summary>
        /// <param name="groupMemberIdKey">The identifier key of the group member to remove.</param>
        /// <returns>The refreshed list of relationships.</returns>
        [BlockAction]
        public BlockActionResult DeleteRelationship( string groupMemberIdKey )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "Person not found." );
            }

            var ownerRole = GetOwnerRole();
            var group = GetRelationshipGroup( person, ownerRole, false );

            if ( group == null )
            {
                return ActionBadRequest( "The relationship group could not be found." );
            }

            if ( !CanEditRelationships( group, ownerRole ) )
            {
                return ActionForbidden( "Not authorized to edit relationships." );
            }

            var memberService = new GroupMemberService( RockContext );
            var groupMember = memberService.Get( groupMemberIdKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( groupMember == null || groupMember.GroupId != group.Id )
            {
                return ActionBadRequest( "Relationship not found." );
            }

            if ( IsInverseRelationshipsOwner( ownerRole ) )
            {
                var inverseGroupMember = memberService.GetInverseRelationship( groupMember, false );

                if ( inverseGroupMember != null )
                {
                    memberService.Delete( inverseGroupMember );
                }
            }

            memberService.Delete( groupMember );
            RockContext.SaveChanges();

            return ActionOk( GetRelationshipBags( group.Id, person.Id ) );
        }

        #endregion Block Actions
    }
}
