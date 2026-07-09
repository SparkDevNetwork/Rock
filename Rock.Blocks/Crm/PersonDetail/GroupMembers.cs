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
using Rock.Drawing.Avatar;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMembers;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Allows you to view the other members of a group person belongs to (e.g. Family groups).
    /// </summary>

    [DisplayName( "Group Members" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to view the other members of a group person belongs to (e.g. Family groups)." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [GroupTypeField(
        "Group Type",
        Key = AttributeKey.GroupType,
        Description = "The group type to display groups for (default is Family)",
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY,
        Order = 0 )]

    [BooleanField(
        "Auto Create Group",
        Key = AttributeKey.AutoCreateGroup,
        Description = "If person doesn't belong to a group of this type, should one be created for them (default is Yes).",
        DefaultBooleanValue = true,
        Order = 1 )]

    [LinkedPage(
        "Group Edit Page",
        Key = AttributeKey.GroupEditPage,
        Description = "Page used to edit the members of the selected group.",
        IsRequired = true,
        Order = 2 )]

    [LinkedPage(
        "Location Detail Page",
        Key = AttributeKey.LocationDetailPage,
        Description = "Page used to edit the settings for a particular location.",
        IsRequired = false,
        Order = 3 )]

    [BooleanField(
        "Show County",
        Key = AttributeKey.ShowCounty,
        Description = "Should County be displayed when editing an address?.",
        DefaultBooleanValue = false,
        Order = 4 )]

    [CodeEditorField(
        "Group Header Lava",
        Key = AttributeKey.GroupHeaderLava,
        Description = "Lava to put at the top of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        Order = 5 )]

    [CodeEditorField(
        "Group Footer Lava",
        Key = AttributeKey.GroupFooterLava,
        Description = "Lava to put at the bottom of the block. Merge fields include Page, CurrentPerson, Group (the family) and GroupMembers.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        Order = 6 )]

    [EnumField(
        "Avatar Style",
        Key = AttributeKey.AvatarStyle,
        Description = "Allows control of the person photo avatar to use either an icon to represent the person's gender and age classification, or first and last name initials when the person does not have a photo.",
        IsRequired = true,
        EnumSourceType = typeof( AvatarStyle ),
        DefaultEnumValue = ( int ) AvatarStyle.Icon,
        Order = 7 )]

    #endregion Block Attributes

    [Rock.Web.UI.ContextAware( typeof( Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "80B4C887-FC4F-4F0E-8DDC-985B4A4B51E8" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "63652754-FFD6-4293-8AC6-BB649FD93D72" )]
    [Rock.SystemGuid.BlockTypeGuid( "7BFD4000-ED0E-41B8-8DD5-C36973C36E1F" )]
    public class GroupMembers : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string AutoCreateGroup = "AutoCreateGroup";
            public const string GroupEditPage = "GroupEditPage";
            public const string LocationDetailPage = "LocationDetailPage";
            public const string ShowCounty = "ShowCounty";
            public const string GroupHeaderLava = "GroupHeaderLava";
            public const string GroupFooterLava = "GroupFooterLava";
            public const string AvatarStyle = "AvatarStyle";
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
            var box = new CustomBlockBox<GroupMembersBag, GroupMembersOptionsBag>();

            var person = GetPerson();

            if ( person == null || person.Id == 0 || person.IsNameless() )
            {
                box.Bag = new GroupMembersBag { Groups = new List<GroupBag>() };
                box.Options = new GroupMembersOptionsBag();

                return box;
            }

            var groupType = GetGroupType();
            var groups = GetOrderedGroups( person, groupType );
            var isEditAuthorized = IsEditAuthorized();

            box.Bag = new GroupMembersBag
            {
                Groups = GetGroupBags( person, groupType, groups )
            };

            box.Options = new GroupMembersOptionsBag
            {
                IsEditAllowed = isEditAuthorized,
                IsAddressVerificationAvailable = Rock.Address.VerificationContainer.Instance.Components.Any( c => c.Value.Value.IsActive ),

                // Only show the reorder handle when there is more than one
                // group to order.
                IsReorderVisible = groups.Count > 1
            };

            return box;
        }

        /// <summary>
        /// Gets the person whose groups are displayed, either from the block
        /// context or the page parameter.
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

            if ( personKey.IsNotNullOrWhiteSpace() )
            {
                return new PersonService( RockContext ).Get( personKey, !PageCache.Layout.Site.DisablePredictableIds );
            }

            return null;
        }

        /// <summary>
        /// Gets the group type whose groups are displayed, falling back to
        /// the Family group type when not configured.
        /// </summary>
        /// <returns>The group type to display groups for.</returns>
        private GroupTypeCache GetGroupType()
        {
            var groupType = GroupTypeCache.Get( GetAttributeValue( AttributeKey.GroupType ).AsGuid() );

            return groupType ?? GroupTypeCache.GetFamilyGroupType();
        }

        /// <summary>
        /// Determines whether the group type is the Family group type.
        /// </summary>
        /// <param name="groupType">The group type to check.</param>
        /// <returns><c>true</c> when the group type is the Family group type.</returns>
        private static bool IsFamilyGroupType( GroupTypeCache groupType )
        {
            return groupType.Guid == Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid();
        }

        /// <summary>
        /// Determines whether the current person is authorized to edit. This
        /// gates the reorder and address verification actions, both of which
        /// modify data.
        /// </summary>
        /// <returns><c>true</c> when the current person is authorized to edit the block.</returns>
        private bool IsEditAuthorized()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the groups of the specified type that the person belongs to,
        /// in display order, creating one when the person has none and the
        /// block is configured to auto create.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="groupType">The group type to display groups for.</param>
        /// <returns>The ordered list of groups.</returns>
        private List<Model.Group> GetOrderedGroups( Person person, GroupTypeCache groupType )
        {
            var memberService = new GroupMemberService( RockContext );

            /*
                7/2/2026 - MSE

                When a person belongs to multiple families, make sure GroupMember.GroupOrder
                is set for all of them before displaying anything. Other features (like the
                primary family calculation) rely on GroupOrder, so normalizing it here keeps
                those results consistent.

                Reason: GroupOrder drives primary family selection and must be deterministic.
            */
            if ( IsFamilyGroupType( groupType ) )
            {
                var groupMemberGroups = memberService.Queryable( true )
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == groupType.Id )
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                    .ToList();

                if ( groupMemberGroups.Count > 1 && memberService.SetGroupMemberGroupOrder( groupMemberGroups ) )
                {
                    RockContext.SaveChanges();
                }
            }

            var groups = memberService.Queryable( true )
                .Where( m =>
                    m.PersonId == person.Id &&
                    m.Group.GroupTypeId == groupType.Id )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                .Select( m => m.Group )
                .AsNoTracking()
                .ToList();

            var isAutoCreateEnabled = GetAttributeValue( AttributeKey.AutoCreateGroup ).AsBoolean( true );

            if ( !groups.Any() && isAutoCreateEnabled && groupType.DefaultGroupRoleId.HasValue )
            {
                // Ensure the person belongs to at least one group of this type.
                var groupService = new GroupService( RockContext );
                var group = new Model.Group
                {
                    Name = person.LastName,
                    GroupTypeId = groupType.Id
                };

                groupService.Add( group );
                RockContext.SaveChanges();

                var groupMember = new GroupMember
                {
                    PersonId = person.Id,
                    GroupRoleId = groupType.DefaultGroupRoleId.Value,
                    GroupId = group.Id
                };

                group.Members.Add( groupMember );
                RockContext.SaveChanges();

                groups.Add( groupService.GetInclude( group.Id, g => g.GroupType ) );
            }

            return groups;
        }

        /// <summary>
        /// Builds the display bags for the specified groups.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="groupType">The group type to display groups for.</param>
        /// <param name="groups">The ordered groups to build bags for.</param>
        /// <returns>A list of bags describing each group card.</returns>
        private List<GroupBag> GetGroupBags( Person person, GroupTypeCache groupType, List<Model.Group> groups )
        {
            var groupIds = groups.Select( g => g.Id ).ToList();

            // Fetch the members and addresses of every displayed group in one
            // query each so no per-group queries run in the loop below.
            var membersByGroupId = new GroupMemberService( RockContext )
                .Queryable( "GroupRole,Person", true )
                .Where( m => groupIds.Contains( m.GroupId ) && m.PersonId != person.Id )
                .AsNoTracking()
                .ToList()
                .GroupBy( m => m.GroupId )
                .ToDictionary( g => g.Key, g => g.OrderBy( m => m.GroupRole.Order ).ToList() );

            var addressesByGroupId = new GroupLocationService( RockContext )
                .Queryable( "Location" )
                .Where( l => groupIds.Contains( l.GroupId ) )
                .AsNoTracking()
                .ToList()
                .GroupBy( l => l.GroupId )
                .ToDictionary( g => g.Key, g => g.OrderBy( l => GetAddressTypeOrder( l.GroupLocationTypeValueId ) ).ToList() );

            var headerLavaTemplate = GetAttributeValue( AttributeKey.GroupHeaderLava );
            var footerLavaTemplate = GetAttributeValue( AttributeKey.GroupFooterLava );
            var avatarStyle = GetAttributeValue( AttributeKey.AvatarStyle ).ConvertToEnum<AvatarStyle>( AvatarStyle.Icon );
            var isShowingCounty = GetAttributeValue( AttributeKey.ShowCounty ).AsBoolean();
            var isFamilyGroupType = IsFamilyGroupType( groupType );

            var bags = new List<GroupBag>();

            foreach ( var group in groups )
            {
                var members = membersByGroupId.TryGetValue( group.Id, out var groupMembers )
                    ? groupMembers
                    : new List<GroupMember>();

                var addresses = addressesByGroupId.TryGetValue( group.Id, out var groupLocations )
                    ? groupLocations
                    : new List<GroupLocation>();

                var (headerHtml, footerHtml) = ResolveGroupLava( headerLavaTemplate, footerLavaTemplate, group, members );

                var bag = new GroupBag
                {
                    IdKey = group.IdKey,
                    TitleHtml = group.Name.FormatAsHtmlTitle(),
                    GroupTypeName = groupType.Name,
                    GroupEditPageUrl = GetGroupEditPageUrl( person, group ),
                    HeaderHtml = headerHtml,
                    FooterHtml = footerHtml,
                    Members = GetMemberBags( person, members, isFamilyGroupType, avatarStyle ),
                    Addresses = GetAddressBags( person, addresses, isShowingCounty )
                };

                SetGroupAttributeBags( bag, group );

                bags.Add( bag );
            }

            return bags;
        }

        /// <summary>
        /// Resolves the group header and footer Lava templates, building the
        /// merge fields once and sharing them across both.
        /// </summary>
        /// <param name="headerTemplate">The header Lava template to resolve.</param>
        /// <param name="footerTemplate">The footer Lava template to resolve.</param>
        /// <param name="group">The group made available to the templates.</param>
        /// <param name="members">The group members made available to the templates.</param>
        /// <returns>The resolved header and footer HTML, each <c>null</c> when its template is not configured.</returns>
        private (string HeaderHtml, string FooterHtml) ResolveGroupLava( string headerTemplate, string footerTemplate, Model.Group group, List<GroupMember> members )
        {
            var hasHeader = headerTemplate.IsNotNullOrWhiteSpace();
            var hasFooter = footerTemplate.IsNotNullOrWhiteSpace();

            if ( !hasHeader && !hasFooter )
            {
                return (null, null);
            }

            // Build the merge fields once and reuse them for both templates.
            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Group", group );
            mergeFields.Add( "GroupMembers", members );

            var headerHtml = hasHeader ? headerTemplate.ResolveMergeFields( mergeFields ) : null;
            var footerHtml = hasFooter ? footerTemplate.ResolveMergeFields( mergeFields ) : null;

            return (headerHtml, footerHtml);
        }

        /// <summary>
        /// Builds the member tile bags for one group, in display order.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="members">The members of the group, excluding the person being viewed.</param>
        /// <param name="isFamilyGroupType">Whether the displayed group type is Family.</param>
        /// <param name="avatarStyle">The avatar style used for members without a photo.</param>
        /// <returns>A list of bags describing each member tile.</returns>
        private List<GroupMemberBag> GetMemberBags( Person person, List<GroupMember> members, bool isFamilyGroupType, AvatarStyle avatarStyle )
        {
            List<GroupMember> orderedMembers;

            if ( isFamilyGroupType )
            {
                var adultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();

                // Adult males first, then adult females, then children, each
                // oldest first.
                orderedMembers = members
                    .Where( m => m.GroupRole.Guid == adultRoleGuid && m.Person.Gender == Gender.Male )
                    .OrderByDescending( m => m.Person.Age )
                    .Concat( members
                        .Where( m => m.GroupRole.Guid == adultRoleGuid && m.Person.Gender != Gender.Male )
                        .OrderByDescending( m => m.Person.Age ) )
                    .Concat( members
                        .Where( m => m.GroupRole.Guid != adultRoleGuid )
                        .OrderByDescending( m => m.Person.Age ) )
                    .ToList();
            }
            else
            {
                orderedMembers = members
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.LastName )
                    .ThenBy( m => m.Person.NickName )
                    .ToList();
            }

            return orderedMembers
                .Where( m => m.Person != null )
                .Select( m => new GroupMemberBag
                {
                    PersonIdKey = m.Person.IdKey,
                    DisplayName = GetMemberDisplayName( person, m.Person, isFamilyGroupType ),
                    Age = m.Person.Age,
                    PhotoUrl = GetMemberPhotoUrl( m.Person, avatarStyle ),
                    IsDeceased = m.Person.IsDeceased
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the display name for a member tile. The last name is included
        /// only when it differs from the viewed person or the group type is
        /// not Family.
        /// </summary>
        /// <param name="viewedPerson">The person whose groups are displayed.</param>
        /// <param name="memberPerson">The member whose name is displayed.</param>
        /// <param name="isFamilyGroupType">Whether the displayed group type is Family.</param>
        /// <returns>The display name of the member.</returns>
        private static string GetMemberDisplayName( Person viewedPerson, Person memberPerson, bool isFamilyGroupType )
        {
            if ( viewedPerson.LastName != memberPerson.LastName || !isFamilyGroupType )
            {
                return $"{memberPerson.NickName} {memberPerson.LastName}";
            }

            return memberPerson.NickName;
        }

        /// <summary>
        /// Gets the photo URL for a member tile, applying the configured
        /// avatar style for members without a photo.
        /// </summary>
        /// <param name="memberPerson">The member whose photo is displayed.</param>
        /// <param name="avatarStyle">The avatar style used for members without a photo.</param>
        /// <returns>The photo URL of the member.</returns>
        private static string GetMemberPhotoUrl( Person memberPerson, AvatarStyle avatarStyle )
        {
            var photoUrl = Person.GetPersonPhotoUrl( memberPerson, 400 );

            if ( avatarStyle == AvatarStyle.Icon )
            {
                photoUrl += "&Style=icon&BackgroundColor=E4E4E7&ForegroundColor=A1A1AA";
            }

            return photoUrl;
        }

        /// <summary>
        /// Loads the group's attribute values that the current person is
        /// authorized to view and splits them into the always-visible and
        /// show-more lists.
        /// </summary>
        /// <param name="bag">The group bag to set the attribute lists on.</param>
        /// <param name="group">The group whose attributes are displayed.</param>
        private void SetGroupAttributeBags( GroupBag bag, Model.Group group )
        {
            group.LoadAttributes( RockContext );

            var authorizedAttributes = group.GetAuthorizedAttributes( Authorization.VIEW, RequestContext.CurrentPerson )
                .Select( a => a.Value )
                .OrderBy( a => a.Order )
                .ToList();

            var gridAttributes = new List<GroupAttributeBag>();
            var moreAttributes = new List<GroupAttributeBag>();

            foreach ( var attribute in authorizedAttributes )
            {
                var value = attribute.DefaultValue;

                if ( group.AttributeValues.ContainsKey( attribute.Key ) && group.AttributeValues[attribute.Key] != null )
                {
                    value = group.AttributeValues[attribute.Key].ValueFormatted;
                }

                if ( value.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var attributeBag = new GroupAttributeBag
                {
                    Name = attribute.Name,
                    FormattedValue = value
                };

                if ( attribute.IsGridColumn )
                {
                    gridAttributes.Add( attributeBag );
                }
                else
                {
                    moreAttributes.Add( attributeBag );
                }
            }

            bag.GridAttributes = gridAttributes;
            bag.MoreAttributes = moreAttributes;
        }

        /// <summary>
        /// Builds the address row bags for one group, in address type order.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="groupLocations">The locations of the group.</param>
        /// <param name="isShowingCounty">Whether the county is included in the formatted address.</param>
        /// <returns>A list of bags describing each address row.</returns>
        private List<GroupAddressBag> GetAddressBags( Person person, List<GroupLocation> groupLocations, bool isShowingCounty )
        {
            var bags = new List<GroupAddressBag>();

            foreach ( var groupLocation in groupLocations )
            {
                if ( groupLocation.Location == null )
                {
                    continue;
                }

                var addressTypeValue = groupLocation.GroupLocationTypeValueId.HasValue
                    ? DefinedValueCache.Get( groupLocation.GroupLocationTypeValueId.Value )
                    : null;

                bags.Add( new GroupAddressBag
                {
                    LocationId = groupLocation.Location.Id,
                    AddressTypeName = FormatAddressType( addressTypeValue?.Value ),
                    FormattedHtml = FormatAddress( groupLocation.Location, isShowingCounty ),
                    IconCssClass = addressTypeValue?.GetAttributeValue( "IconCSSClass" ) ?? "ti ti-map-pin",
                    MapUrl = "https://maps.google.com/maps?q=" + Uri.EscapeDataString( groupLocation.Location.GetFullStreetAddress() ),
                    LocationSettingsPageUrl = GetLocationSettingsPageUrl( person, groupLocation.Location )
                } );
            }

            return bags;
        }

        /// <summary>
        /// Gets the display order of an address type from the cache, used to
        /// sort the addresses on a group card. Addresses with no type sort last.
        /// </summary>
        /// <param name="groupLocationTypeValueId">The defined value identifier of the address type.</param>
        /// <returns>The address type order, or <see cref="int.MaxValue"/> when there is no type.</returns>
        private static int GetAddressTypeOrder( int? groupLocationTypeValueId )
        {
            if ( !groupLocationTypeValueId.HasValue )
            {
                return int.MaxValue;
            }

            return DefinedValueCache.Get( groupLocationTypeValueId.Value )?.Order ?? int.MaxValue;
        }

        /// <summary>
        /// Formats the address type label, appending "Address" when the type
        /// name doesn't already end with it.
        /// </summary>
        /// <param name="addressType">The address type name.</param>
        /// <returns>The formatted address type label.</returns>
        private static string FormatAddressType( string addressType )
        {
            var type = addressType.IsNotNullOrWhiteSpace() ? addressType : "Unknown";

            return type.EndsWith( "Address", StringComparison.CurrentCultureIgnoreCase ) ? type : $"{type} Address";
        }

        /// <summary>
        /// Formats the address HTML, optionally including the county.
        /// </summary>
        /// <param name="location">The location to format.</param>
        /// <param name="isShowingCounty">Whether the county is included.</param>
        /// <returns>The formatted address HTML.</returns>
        private static string FormatAddress( Location location, bool isShowingCounty )
        {
            if ( !isShowingCounty )
            {
                return location.FormattedHtmlAddress;
            }

            return $"{location.Street1}<br/>{location.Street2}<br/>{location.City}{( location.County.IsNotNullOrWhiteSpace() ? ", " + location.County : string.Empty )}, {location.State} {location.PostalCode}"
                .ReplaceWhileExists( "  ", " " )
                .ReplaceWhileExists( "<br/><br/>", "<br/>" );
        }

        /// <summary>
        /// Gets the URL of the page used to edit the members of the group.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="group">The group to edit.</param>
        /// <returns>The group edit page URL.</returns>
        private string GetGroupEditPageUrl( Person person, Model.Group group )
        {
            return this.GetLinkedPageUrl( AttributeKey.GroupEditPage, new Dictionary<string, string>
            {
                ["PersonId"] = person.Id.ToString(),
                ["GroupId"] = group.Id.ToString()
            } );
        }

        /// <summary>
        /// Gets the URL of the page used to edit the settings of the location.
        /// </summary>
        /// <param name="person">The person whose groups are displayed.</param>
        /// <param name="location">The location to edit.</param>
        /// <returns>The location settings page URL.</returns>
        private string GetLocationSettingsPageUrl( Person person, Location location )
        {
            return this.GetLinkedPageUrl( AttributeKey.LocationDetailPage, new Dictionary<string, string>
            {
                ["LocationId"] = location.Id.ToString(),
                ["PersonId"] = person.Id.ToString()
            } );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Moves one of the person's groups to a new position in the display
        /// order and persists the new order.
        /// </summary>
        /// <param name="groupIdKey">The IdKey identifier of the group being moved.</param>
        /// <param name="newIndex">The zero-based index the group was moved to.</param>
        [BlockAction]
        public BlockActionResult ReorderGroup( string groupIdKey, int newIndex )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 )
            {
                return ActionBadRequest( "Person was not found." );
            }

            var groupId = IdHasher.Instance.GetId( groupIdKey );

            if ( !groupId.HasValue )
            {
                return ActionBadRequest( "Group was not found." );
            }

            var groupType = GetGroupType();
            var memberService = new GroupMemberService( RockContext );
            var groupMemberGroups = memberService.Queryable( true )
                .Where( m =>
                    m.PersonId == person.Id &&
                    m.Group.GroupTypeId == groupType.Id )
                .OrderBy( m => m.GroupOrder ?? int.MaxValue ).ThenBy( m => m.Id )
                .ToList();

            var groupMember = groupMemberGroups.FirstOrDefault( a => a.GroupId == groupId.Value );

            if ( groupMember == null )
            {
                return ActionNotFound( "Group was not found." );
            }

            newIndex = Math.Max( 0, Math.Min( newIndex, groupMemberGroups.Count - 1 ) );

            memberService.ReorderGroupMemberGroup( groupMemberGroups, groupMemberGroups.IndexOf( groupMember ), newIndex );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Re-verifies the address of the specified location and returns the
        /// refreshed group data, since verification can standardize the
        /// address.
        /// </summary>
        /// <param name="locationId">The identifier of the location to verify.</param>
        /// <returns>The refreshed group data.</returns>
        [BlockAction]
        public BlockActionResult VerifyLocation( int locationId )
        {
            var person = GetPerson();

            if ( person == null || person.Id == 0 || person.IsNameless() )
            {
                return ActionBadRequest( "Person was not found." );
            }

            var groupType = GetGroupType();
            var groups = GetOrderedGroups( person, groupType );
            var groupIds = groups.Select( g => g.Id ).ToList();

            // Only allow verifying a location shown on this person's group cards.
            var isLocationOnPersonGroup = new GroupLocationService( RockContext )
                .Queryable()
                .Any( gl => groupIds.Contains( gl.GroupId ) && gl.LocationId == locationId );

            if ( !isLocationOnPersonGroup )
            {
                return ActionNotFound( "Location was not found." );
            }

            var locationService = new LocationService( RockContext );
            var location = locationService.Get( locationId );

            if ( location == null )
            {
                return ActionNotFound( "Location was not found." );
            }

            locationService.Verify( location, true );
            RockContext.SaveChanges();

            return ActionOk( new GroupMembersBag
            {
                Groups = GetGroupBags( person, groupType, groups )
            } );
        }

        #endregion Block Actions
    }
}
