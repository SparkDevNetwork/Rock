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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.CommunicationListSubscribe;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Block that allows a person to manage the communication lists that they are subscribed to.
    /// </summary>
    [DisplayName( "Communication List Subscribe" )]
    [Category( "Communication" )]
    [Description( "Block that allows a person to manage the communication lists that they are subscribed to." )]
    [IconCssClass( "ti ti-speakerphone" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware( typeof( Campus ), typeof( Person ) )]

    #region Block Attributes

    [GroupCategoryField(
        "Communication List Categories",
        Description = "Select the categories of the communication lists to display, or select none to show all that the user is authorized to view.",
        AllowMultiple = true,
        GroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST,
        IsRequired = false,
        Key = AttributeKey.CommunicationListCategories,
        Order = 1 )]

    [BooleanField(
        "Show Medium Preference",
        Description = "Show the user's current medium preference for each list and allow them to change it.",
        DefaultBooleanValue = true,
        Key = AttributeKey.ShowMediumPreference,
        Order = 2 )]

    [BooleanField(
        "Filter Groups By Campus Context",
        Description = "When enabled, will filter the listed Communication Lists by the campus context of the page. Groups with no campus will always be shown.",
        IsRequired = false,
        Key = AttributeKey.FilterGroupsByCampusContext,
        Order = 3 )]

    [BooleanField(
        "Always Include Subscribed Lists",
        Description = "When filtering is enabled this setting will include lists that the person is subscribed to even if they don't match the current campus context. (note this would still filter by the category though, so lists not in the configured category would not show even if subscribed to them)",
        IsRequired = false,
        DefaultBooleanValue = true,
        Key = AttributeKey.AlwaysIncludeSubscribedLists,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B1C36EA1-6B94-4C3C-98CD-D6A9DAA9B7A4" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "002A8BFF-9C9F-4D25-A0A9-94CB209AFBF7" )]
    [Rock.SystemGuid.BlockTypeGuid( "52E0AA5B-B08B-42E4-8180-DD7925BAA57F" )]
    public class CommunicationListSubscribe : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string CommunicationListCategories = "CommunicationListCategories";
            public const string ShowMediumPreference = "ShowMediumPreference";
            public const string FilterGroupsByCampusContext = "FilterGroupsByCampusContext";
            public const string AlwaysIncludeSubscribedLists = "AlwaysIncludeSubscribedLists";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<CommunicationListSubscribeBag, CommunicationListSubscribeOptionsBag>();

            var person = GetPerson();

            box.Bag = new CommunicationListSubscribeBag
            {
                CommunicationLists = GetCommunicationListItems( person )
            };

            box.Options.IsShowMediumPreferenceEnabled = GetAttributeValue( AttributeKey.ShowMediumPreference ).AsBoolean();

            return box;
        }

        /// <summary>
        /// Gets the person whose subscriptions should be managed.
        /// </summary>
        /// <returns>The resolved person, or null if no person could be determined.</returns>
        private Person GetPerson()
        {
            var person = RequestContext.GetContextEntity<Person>();
            if ( person != null )
            {
                return person;
            }

            var key = PageParameter( PageParameterKey.PersonId );
            if ( key.IsNotNullOrWhiteSpace() )
            {
                return new PersonService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return RequestContext.CurrentPerson;
        }

        /// <summary>
        /// Gets the list of communication list items that the person can subscribe to or unsubscribe from.
        /// Filters by synced groups, categories, authorization, and campus context as configured.
        /// </summary>
        /// <param name="person">The person whose subscriptions should be loaded.</param>
        /// <returns>A list of communication list items, or an empty list if no person is provided.</returns>
        private List<CommunicationListItemBag> GetCommunicationListItems( Person person )
        {
            if ( person == null )
            {
                return new List<CommunicationListItemBag>();
            }

            var communicationListGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            var communicationListGroupTypeDefaultRoleId = GroupTypeCache.Get( communicationListGroupTypeId ).DefaultGroupRoleId;

            // Get all group IDs the person is a member of.
            var memberOfList = new GroupMemberService( RockContext )
                .GetByPersonId( person.Id )
                .Select( a => a.GroupId )
                .ToList();

            // Get synced groups where the default role is synced and the person is NOT a member.
            // These groups should not be shown as subscribable.
            var commGroupSyncsForDefaultRole = new GroupSyncService( RockContext ).Queryable()
                .Where( a => a.Group.GroupTypeId == communicationListGroupTypeId )
                .Where( a => a.GroupTypeRoleId == communicationListGroupTypeDefaultRoleId )
                .Where( a => !memberOfList.Contains( a.GroupId ) )
                .Select( a => a.GroupId )
                .ToList();

            var communicationLists = new GroupService( RockContext ).Queryable()
                .Where( a => a.GroupTypeId == communicationListGroupTypeId && !commGroupSyncsForDefaultRole.Contains( a.Id ) && a.IsPublic )
                .IsActive()
                .ToList();

            var categoryGuids = GetAttributeValue( AttributeKey.CommunicationListCategories ).SplitDelimitedValues().AsGuidList();
            var viewableCommunicationLists = new List<Rock.Model.Group>();

            foreach ( var communicationList in communicationLists )
            {
                communicationList.LoadAttributes( RockContext );

                if ( !categoryGuids.Any() )
                {
                    // If no categories were specified, only show lists that the person has VIEW auth.
                    if ( communicationList.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        viewableCommunicationLists.Add( communicationList );
                    }
                }
                else
                {
                    var categoryGuid = communicationList.GetAttributeValue( "Category" ).AsGuidOrNull();
                    if ( categoryGuid.HasValue && categoryGuids.Contains( categoryGuid.Value ) )
                    {
                        var category = CategoryCache.Get( categoryGuid.Value );
                        if ( category != null && category.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                        {
                            viewableCommunicationLists.Add( communicationList );
                        }
                    }
                }
            }

            // Sort by PublicName attribute, falling back to group name.
            viewableCommunicationLists = viewableCommunicationLists
                .OrderBy( a =>
                {
                    var name = a.GetAttributeValue( "PublicName" );
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        name = a.Name;
                    }

                    return name;
                } )
                .ToList();

            var groupIds = viewableCommunicationLists.Select( a => a.Id ).ToList();

            // Load the person's group member records for these communication lists.
            var personCommunicationListsMember = new GroupMemberService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( a => groupIds.Contains( a.GroupId ) && a.PersonId == person.Id )
                .GroupBy( a => a.GroupId )
                .ToList()
                .ToDictionary( k => k.Key, v => v.FirstOrDefault() );

            // Apply campus context filtering if enabled.
            var filterByCampus = GetAttributeValue( AttributeKey.FilterGroupsByCampusContext ).AsBoolean();
            if ( filterByCampus )
            {
                var contextCampus = RequestContext.GetContextEntity<Campus>();
                if ( contextCampus != null )
                {
                    var alwaysIncludeSubscribed = GetAttributeValue( AttributeKey.AlwaysIncludeSubscribedLists ).AsBoolean();

                    viewableCommunicationLists = viewableCommunicationLists
                        // We're going to do two steps of filtering here.
                        .Where( x =>
                        {
                            var groupMember = personCommunicationListsMember.GetValueOrNull( x.Id );
                            var isSubscribed = groupMember != null && groupMember.GroupMemberStatus == GroupMemberStatus.Active;

                            return ( x.CampusId == null || x.CampusId == contextCampus.Id )
                                || ( alwaysIncludeSubscribed && isSubscribed );
                        } )
                        .ToList();
                }
            }

            // Build the list items.
            return viewableCommunicationLists.Select( group =>
            {
                var groupMember = personCommunicationListsMember.GetValueOrNull( group.Id );
                var isSubscribed = groupMember != null && groupMember.GroupMemberStatus == GroupMemberStatus.Active;

                // Determine communication preference: group member preference takes precedence over person preference.
                var communicationType = person.CommunicationPreference == CommunicationType.SMS ? CommunicationType.SMS : CommunicationType.Email;
                var groupMemberHasSmsOrEmailPreference = groupMember != null && ( groupMember.CommunicationPreference == CommunicationType.SMS || groupMember.CommunicationPreference == CommunicationType.Email );
                if ( groupMemberHasSmsOrEmailPreference )
                {
                    communicationType = groupMember.CommunicationPreference;
                }

                var publicName = group.GetAttributeValue( "PublicName" );

                return new CommunicationListItemBag
                {
                    CommunicationListGuid = group.Guid,
                    DisplayName = publicName.IsNotNullOrWhiteSpace() ? publicName : group.Name,
                    Description = group.Description,
                    IsSubscribed = isSubscribed,
                    CommunicationPreference = ( Rock.Enums.Communication.CommunicationType ) communicationType
                };
            } ).ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Updates the subscription status for a communication list.
        /// If subscribing and the person is not a member, a new group member record is created.
        /// If the person is already a member, their status is toggled between Active and Inactive.
        /// </summary>
        /// <param name="bag">The <see cref="CommunicationListSubscribeRequestBag"/> containing the list and subscription status.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult UpdateSubscription( CommunicationListSubscribeRequestBag bag )
        {
            var person = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( "Unable to determine the person." );
            }

            var groupMemberService = new GroupMemberService( RockContext );
            var group = new GroupService( RockContext ).Get( bag.CommunicationListGuid );
            if ( group == null )
            {
                return ActionBadRequest( "Communication list not found." );
            }

            var existingGroupMembers = groupMemberService.Queryable()
                .Where( a => a.GroupId == group.Id && a.PersonId == person.Id )
                .ToList();

            if ( existingGroupMembers.Any() )
            {
                foreach ( var existingGroupMember in existingGroupMembers )
                {
                    // Update existing group member record.
                    if ( bag.IsSubscribed )
                    {
                        if ( existingGroupMember.GroupMemberStatus == GroupMemberStatus.Inactive )
                        {
                            existingGroupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            if ( existingGroupMember.Note == "Unsubscribed" )
                            {
                                existingGroupMember.Note = string.Empty;
                            }   
                        }
                    }
                    else
                    {
                        if ( existingGroupMember.GroupMemberStatus == GroupMemberStatus.Active )
                        {
                            existingGroupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                            if ( string.IsNullOrWhiteSpace( existingGroupMember.Note ) )
                            {
                                existingGroupMember.Note = "Unsubscribed";
                            }
                        }
                    }
                }
            }
            else if ( bag.IsSubscribed )
            {
                // Person is not a member — create a new group member record.
                var groupMember = new GroupMember
                {
                    PersonId = person.Id,
                    GroupId = group.Id,
                    GroupMemberStatus = GroupMemberStatus.Active
                };

                var defaultGroupRoleId = GroupTypeCache.Get( group.GroupTypeId )?.DefaultGroupRoleId;
                if ( !defaultGroupRoleId.HasValue )
                {
                    return ActionBadRequest( "Unable to add to group. Group has no default group role." );
                }

                groupMember.GroupRoleId = defaultGroupRoleId.Value;

                // Set the communication preference to match the person's preference.
                groupMember.CommunicationPreference = person.CommunicationPreference == CommunicationType.SMS
                    ? CommunicationType.SMS
                    : CommunicationType.Email;

                if ( !groupMember.IsValidGroupMember( RockContext ) )
                {
                    var errorMessages = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( " " );
                    return ActionBadRequest( $"Unable to add to group. {errorMessages}" );
                }

                groupMemberService.Add( groupMember );
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        /// <summary>
        /// Updates the communication preference (Email or SMS) for a person's membership
        /// in a communication list.
        /// </summary>
        /// <param name="bag">The <see cref="CommunicationListPreferenceRequestBag"/> containing the list and preference.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction]
        public BlockActionResult UpdateCommunicationPreference( CommunicationListPreferenceRequestBag bag )
        {
            var person = GetPerson();
            if ( person == null )
            {
                return ActionBadRequest( "Unable to determine the person." );
            }

            var group = new GroupService( RockContext ).Get( bag.CommunicationListGuid );
            if ( group == null )
            {
                return ActionBadRequest( "Communication list not found." );
            }

            var groupMemberRecordsForPerson = new GroupMemberService( RockContext ).Queryable()
                .Where( a => a.GroupId == group.Id && a.PersonId == person.Id )
                .ToList();

            foreach ( var groupMember in groupMemberRecordsForPerson )
            {
                groupMember.CommunicationPreference = ( Rock.Model.CommunicationType ) bag.CommunicationType;
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion Block Actions
    }
}