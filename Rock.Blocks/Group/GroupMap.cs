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
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupMap;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a group (and any child groups) on a map.
    /// </summary>
    [DisplayName( "Group Map" )]
    [Category( "Groups" )]
    [Description( "Displays a group (and any child groups) on a map." )]
    [IconCssClass( "ti ti-map" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Group Page",
        Description = "The page to display group details.",
        IsRequired = true,
        DefaultValue = "",
        Order = 0,
        Key = AttributeKey.GroupPage )]

    [LinkedPage(
        "Person Profile Page",
        Description = "The page to display person details.",
        IsRequired = true,
        DefaultValue = "",
        Order = 1,
        Key = AttributeKey.PersonProfilePage )]

    [LinkedPage(
        "Map Page",
        Description = "The page to display group map (typically this page).",
        IsRequired = true,
        DefaultValue = "",
        Order = 2,
        Key = AttributeKey.MapPage )]

    [DefinedValueField(
        "Map Style",
        Description = "The map theme that should be used for styling the map.",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE,
        Order = 3,
        Key = AttributeKey.MapStyle )]

    [IntegerField(
        "Map Height",
        Description = "Height of the map in pixels (default value is 600px)",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Order = 4,
        Key = AttributeKey.MapHeight )]

    [TextField(
        "Polygon Colors",
        Description = "Comma-Delimited list of colors to use when displaying multiple polygons (e.g. #f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc).",
        IsRequired = true,
        DefaultValue = "#f37833,#446f7a,#afd074,#649dac,#f8eba2,#92d0df,#eaf7fc",
        Order = 5,
        Key = AttributeKey.PolygonColors )]

    [BooleanField(
        "Show Campuses Filter",
        Description = "",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.ShowCampusesFilter )]

    [DefinedValueField(
        "Campus Types",
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 7,
        Key = AttributeKey.CampusTypes )]

    [DefinedValueField(
        "Campus Statuses",
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 8,
        Key = AttributeKey.CampusStatuses )]

    [BooleanField(
        "Show Child Groups as Default",
        Description = "Defaults to showing all child groups if no user preference is set",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ShowChildGroupsAsDefault )]

    [CodeEditorField(
        "Info Window Contents",
        Description = "Lava template for the info window. To suppress the window provide a blank template.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 600,
        IsRequired = false,
        DefaultValue = DefaultLavaTemplate,
        Order = 10,
        Key = AttributeKey.InfoWindowContents )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E6A4C4B0-9F2D-4B7E-8C3A-1D5F6E7A8B90" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "CAFCF3E4-7EB6-4651-BDEC-F79B29C8AAAA" )]
     [Rock.SystemGuid.BlockTypeGuid( "967F0D2B-DB76-486A-B034-D22B9D9240D3" )]
    public class GroupMap : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupPage = "GroupPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string MapPage = "MapPage";
            public const string MapStyle = "MapStyle";
            public const string MapHeight = "MapHeight";
            public const string PolygonColors = "PolygonColors";
            public const string ShowCampusesFilter = "ShowCampusesFilter";
            public const string ShowChildGroupsAsDefault = "ShowChildGroupsAsDefault";
            public const string InfoWindowContents = "InfoWindowContents";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
        }

        private static class PersonPreferenceKey
        {
            public const string ShowChildGroups = "ShowChildGroups";
            public const string GroupTypeIds = "GroupTypeIds";
        }

        private static class NavigationUrlKey
        {
            public const string GroupPage = "GroupPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string MapPage = "MapPage";
        }

        /// <summary>
        /// Attribute keys on the DefinedValues this block reads.
        /// </summary>
        private static class DefinedValueAttributeKey
        {
            /// <summary>The Map Style's pipe-delimited list of marker colors.</summary>
            public const string MapStyleColors = "Colors";

            /// <summary>The Person Connection Status's layer color.</summary>
            public const string ConnectionStatusColor = "Color";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// Default Lava template for the group map info window.
        /// </summary>
        private const string DefaultLavaTemplate = @"
<div style='width: 300px; font-size: var(--font-size-small); line-height: var(--line-height-normal); color: var(--color-interface-strongest);'>
    {% if Campus.Name and Campus.Name != '' %}
        <span class='label label-campus'>{{ Campus.Name }}</span>
    {% endif %}

    <div style='margin: var(--spacing-xsmall) 0 var(--spacing-tiny); font-size: var(--font-size-h5); font-weight: var(--font-weight-bold);'>{{ GroupName }}</div>

    {% if Location.Address and Location.Address != '' %}
        <div style='margin-bottom: var(--spacing-small); color: var(--color-interface-medium);'>{{ Location.Address }}</div>
    {% endif %}

    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
        <div style='display: flex; gap: var(--spacing-xsmall);'>
            <a href='{{ DetailPageUrl }}' class='btn btn-primary' style='flex: 1;'>View Group Details</a>
            <a href='{{ MapPageUrl }}' class='btn btn-default' style='flex: 1;'>View Map</a>
        </div>
    {% endif %}

    {% if Members.size > 0 %}
        <div style='margin: var(--spacing-small) 0; border-top: 1px solid var(--color-interface-soft);'></div>

        <div style='display: flex; align-items: center; gap: var(--spacing-xsmall); margin-bottom: var(--spacing-tiny); font-size: var(--font-size-xsmall); font-weight: var(--font-weight-bold); letter-spacing: .04em; text-transform: uppercase; color: var(--color-interface-medium);'>Members <span style='display: inline-flex; align-items: center; justify-content: center; min-width: 18px; height: 18px; padding: 0 var(--spacing-tiny); border-radius: 999px; background-color: var(--color-interface-soft); color: var(--color-interface-strong); font-size: var(--font-size-xsmall); font-weight: var(--font-weight-semibold);'>{{ Members.size }}</span></div>

        {% for GroupMember in Members %}
            <div style='display: flex; gap: var(--spacing-xsmall); padding: var(--spacing-xsmall) 0;{% unless forloop.first %} border-top: 1px solid var(--color-interface-softer);{% endunless %}'>
                <img src='{{ GroupMember.PhotoUrl }}&maxheight=80&maxwidth=80' alt='' style='flex: 0 0 auto; width: 40px; height: 40px; border-radius: 50%; object-fit: cover; background-color: var(--color-interface-soft);'>

                <div style='min-width: 0;'>
                    <div>
                        <a href='{{ GroupMember.ProfilePageUrl }}' style='font-weight: var(--font-weight-bold); color: var(--color-link); text-decoration: none;'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a>
                            <span style='display: inline-flex; align-items: center; margin-left: var(--spacing-tiny); padding: 1px var(--spacing-tiny); border: 1px solid var(--color-interface-soft); border-radius: 999px; font-size: var(--font-size-xsmall); font-weight: var(--font-weight-bold); letter-spacing: .03em; text-transform: uppercase; color: var(--color-interface-medium); vertical-align: middle;'>{{ GroupMember.Role }}</span>
                    </div>

                    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus and GroupMember.ConnectionStatus != '' %}
                        <div style='color: var(--color-interface-medium); margin: 2px 0 var(--spacing-tiny);'>{{ GroupMember.ConnectionStatus }}</div>
                    {% endif %}

                    {% if GroupMember.Email and GroupMember.Email != '' %}
                        <div style='display: flex; align-items: center; gap: var(--spacing-tiny); margin-top: var(--spacing-tiny);'><i class='ti ti-mail' style='width: 14px; text-align: center; color: var(--color-interface-medium);'></i>{{ GroupMember.Email }}</div>
                    {% endif %}

                    {% for Phone in GroupMember.PhoneTypes %}
                        <div style='display: flex; align-items: center; gap: var(--spacing-tiny); margin-top: var(--spacing-tiny);'><i class='ti ti-phone' style='width: 14px; text-align: center; color: var(--color-interface-medium);'></i><span>{{ Phone.Name }} {{ Phone.Number }}</span></div>
                    {% endfor %}
                </div>
            </div>
        {% endfor %}
    {% endif %}
</div>
";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupMapInitializationBag, GroupMapOptionsBag>
            {
                Options = GetBoxOptions(),
                NavigationUrls = GetBoxNavigationUrls()
            };

            box.Bag = GetBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client.
        /// </summary>
        /// <returns>The options bag describing how the map should be displayed and scoped.</returns>
        private GroupMapOptionsBag GetBoxOptions()
        {
            var polygonColors = GetAttributeValue( AttributeKey.PolygonColors )
                .Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                .ToList();

            return new GroupMapOptionsBag
            {
                MapHeight = GetAttributeValue( AttributeKey.MapHeight ).AsIntegerOrNull() ?? 600,
                MapStyleValueGuid = GetAttributeValue( AttributeKey.MapStyle ).AsGuidOrNull(),
                PolygonColors = polygonColors,
                IsCampusFilterShown = GetAttributeValue( AttributeKey.ShowCampusesFilter ).AsBoolean(),
                Campuses = GetCampuses(),
                GroupTypes = GetLocationGroupTypes(),
                ConnectionStatuses = GetConnectionStatuses()
            };
        }

        /// <summary>
        /// Builds the navigation URLs for the configured linked pages.
        /// </summary>
        /// <returns>A map of navigation key to URL.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.GroupPage] = this.GetLinkedPageUrl( AttributeKey.GroupPage ),
                [NavigationUrlKey.PersonProfilePage] = this.GetLinkedPageUrl( AttributeKey.PersonProfilePage ),
                [NavigationUrlKey.MapPage] = this.GetLinkedPageUrl( AttributeKey.MapPage )
            };
        }

        /// <summary>
        /// Builds the runtime data for the client: the resolved group, marker colors, the initial
        /// map view, and the person's saved filter preferences.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private GroupMapInitializationBag GetBag()
        {
            var bag = new GroupMapInitializationBag
            {
                SelectedGroupTypeIds = new List<int>(),
                SelectedGroupTypeGuids = new List<Guid>()
            };

            // Resolve the group from the page parameter (Id, IdKey, or Guid).
            var groupKey = PageParameter( PageParameterKey.GroupId );
            var group = GroupCache.Get( groupKey, !PageCache.Layout.Site.DisablePredictableIds );

            if ( group == null )
            {
                bag.ErrorMessage = "A Group ID is required to display the map.";
                return bag;
            }

            bag.GroupId = group.Id;
            bag.GroupName = group.Name;

            SetMarkerColors( bag );

            // Load the person's saved filter preferences, falling back to the block defaults.
            var preferences = GetBlockPersonPreferences();

            var selectedGroupTypes = GetSelectedGroupTypes( preferences );
            bag.SelectedGroupTypeIds = selectedGroupTypes.Select( groupType => groupType.Id ).ToList();
            bag.SelectedGroupTypeGuids = selectedGroupTypes.Select( groupType => groupType.Guid ).ToList();

            bag.IsShowChildGroupsEnabled = preferences.GetValue( PersonPreferenceKey.ShowChildGroups ).AsBooleanOrNull()
                ?? GetAttributeValue( AttributeKey.ShowChildGroupsAsDefault ).AsBoolean();

            return bag;
        }

        /// <summary>
        /// Gets the campuses available in the campus filter, narrowed by the block's configured
        /// Campus Types and Campus Statuses.
        /// </summary>
        /// <returns>The list of campuses as list items (Guid value, name text).</returns>
        private List<ListItemBag> GetCampuses()
        {
            var campusTypeIds = GetAttributeValues( AttributeKey.CampusTypes )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var campusStatusIds = GetAttributeValues( AttributeKey.CampusStatuses )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            return CampusCache.All()
                .Where( c => ( !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                    && ( !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) ) )
                .Select( c => new ListItemBag { Value = c.Id.ToString(), Text = c.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the group types the options panel offers for scoping the child-group layer:
        /// those shown in navigation and the group list that can have a location.
        /// </summary>
        /// <returns>The list of group types as list items (Guid value, name text).</returns>
        private List<ListItemBag> GetLocationGroupTypes()
        {
            return new GroupTypeService( RockContext ).Queryable()
                .Where( a => a.ShowInNavigation
                    && a.ShowInGroupList
                    && a.LocationSelectionMode != GroupLocationPickerMode.None )
                .OrderBy( a => a.Name )
                .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                .ToList();
        }

        /// <summary>
        /// Gets the person connection statuses that have a color, each rendered as a togglable
        /// families layer.
        /// </summary>
        /// <returns>The list of connection statuses with their layer colors.</returns>
        private List<GroupMapConnectionStatusBag> GetConnectionStatuses()
        {
            var connectionStatusType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() );
            if ( connectionStatusType == null )
            {
                return new List<GroupMapConnectionStatusBag>();
            }

            return connectionStatusType.DefinedValues
                .OrderBy( v => v.Order )
                .ThenBy( v => v.Value )
                .Select( v => new GroupMapConnectionStatusBag
                {
                    Id = v.Id,
                    Name = v.Value.Pluralize(),
                    Color = ( v.GetAttributeValue( DefinedValueAttributeKey.ConnectionStatusColor ) ?? string.Empty ).Replace( "#", string.Empty )
                } )
                .Where( s => s.Color.IsNotNullOrWhiteSpace() )
                .ToList();
        }

        /// <summary>
        /// Derives the group / child group / member marker colors from the Map Style's Colors
        /// attribute (a pipe-delimited list), falling back to a default when none are configured.
        /// </summary>
        /// <param name="bag">The bag to populate with the three marker colors.</param>
        private void SetMarkerColors( GroupMapInitializationBag bag )
        {
            var markerColors = new List<string>();

            var mapStyleValue = DefinedValueCache.Get( GetAttributeValue( AttributeKey.MapStyle ).AsGuid() );
            if ( mapStyleValue != null )
            {
                markerColors = ( mapStyleValue.GetAttributeValue( DefinedValueAttributeKey.MapStyleColors ) ?? string.Empty )
                    .Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList();
            }

            if( !markerColors.Any() )
            {
                markerColors.Add( "FE7569" );
            }

            bag.GroupColor = markerColors[0].Replace( "#", string.Empty );
            bag.ChildGroupColor = ( markerColors.Count > 1 ? markerColors[1] : markerColors[0] ).Replace( "#", string.Empty );
            bag.MemberColor = ( markerColors.Count > 2 ? markerColors[2] : markerColors[0] ).Replace( "#", string.Empty );
        }

        /// <summary>
        /// Gets the group types the person has selected to scope the child-groups layer. New
        /// preferences store the group type Guid; legacy (WebForms) preferences stored the integer
        /// Id, so both forms are resolved to a cached group type here. The client needs the Guids
        /// to pre-select the settings picker and the Ids for the child-groups map endpoint.
        /// </summary>
        /// <param name="preferences">The block person preferences.</param>
        /// <returns>The selected group types (empty when none are saved or resolvable).</returns>
        private List<GroupTypeCache> GetSelectedGroupTypes( PersonPreferenceCollection preferences )
        {
            var selectedGroupTypes = preferences.GetValue( PersonPreferenceKey.GroupTypeIds );
            if ( selectedGroupTypes.IsNullOrWhiteSpace() )
            {
                return new List<GroupTypeCache>();
            }

            return selectedGroupTypes
                .Split( ',' )
                .Select( key =>
                {
                    var guid = key.AsGuidOrNull();
                    return guid.HasValue ? GroupTypeCache.Get( guid.Value ) : GroupTypeCache.Get( key.AsInteger() );
                } )
                .Where( groupType => groupType != null )
                .ToList();
        }

        /// <summary>
        /// Builds the Lava merge-field dictionary for the info window: the group, its campus and
        /// group type, the clicked location's address, and the group's members with their linked
        /// profile URLs. Mirrors the shape the legacy GetMapInfoWindow endpoint produced.
        /// </summary>
        /// <param name="group">The group whose marker was clicked.</param>
        /// <param name="locationId">The clicked location's Id, used to resolve the address block.</param>
        /// <returns>The merge-field dictionary consumed by the info window template.</returns>
        private Dictionary<string, object> BuildInfoWindowMergeFields( Rock.Model.Group group, int locationId )
        {
            var groupDetailUrl = new PageReference( GetAttributeValue( AttributeKey.GroupPage ),
                new Dictionary<string, string> { { PageParameterKey.GroupId, group.IdKey } } ).BuildUrl();

            // The PersonId is intentionally left blank so the template can append each member's IdKey.
            var personProfileUrl = new PageReference( GetAttributeValue( AttributeKey.PersonProfilePage ),
                new Dictionary<string, string> { { "PersonId", string.Empty } } ).BuildUrl();

            var groupMapUrl = new PageReference( GetAttributeValue( AttributeKey.MapPage ),
                new Dictionary<string, string> { { PageParameterKey.GroupId, group.IdKey } } ).BuildUrl();

            var mergeFields = new Dictionary<string, object>
            {
                { "GroupId", group.Id },
                { "GroupName", group.Name },
                { "DetailPageUrl", groupDetailUrl },
                { "MapPageUrl", groupMapUrl },
                { "Campus", new Dictionary<string, object> { { "Name", group.Campus?.Name ?? string.Empty } } },
                {
                    "GroupType", new Dictionary<string, object>
                    {
                        { "Id", group.GroupType.Id },
                        { "Guid", group.GroupType.Guid.ToString().ToUpper() },
                        { "GroupTerm", group.GroupType.GroupTerm },
                        { "GroupMemberTerm", group.GroupType.GroupMemberTerm }
                    }
                }
            };

            var groupLocation = group.GroupLocations.FirstOrDefault( gl => gl.LocationId == locationId );
            if ( groupLocation != null )
            {
                mergeFields["Location"] = new Dictionary<string, object>
                {
                    { "Type", DefinedValueCache.GetValue( groupLocation.GroupLocationTypeValueId ) ?? string.Empty },
                    { "Address", groupLocation.Location.GetFullStreetAddress().ConvertCrLfToHtmlBr() },
                    { "Street1", groupLocation.Location.Street1 },
                    { "Street2", groupLocation.Location.Street2 },
                    { "City", groupLocation.Location.City },
                    { "State", groupLocation.Location.State },
                    { "PostalCode", groupLocation.Location.PostalCode },
                    { "Country", groupLocation.Location.Country }
                };
            }

            mergeFields["Members"] = BuildInfoWindowMembers( group, personProfileUrl );

            return mergeFields;
        }

        /// <summary>
        /// Builds the members portion of the info window merge fields, ordered by role then by
        /// birth date ascending (oldest member first).
        /// </summary>
        /// <param name="group">The group whose members should be rendered.</param>
        /// <param name="personProfileUrl">The person profile URL stub each member Id is appended to.</param>
        /// <returns>The list of member merge-field dictionaries.</returns>
        private List<Dictionary<string, object>> BuildInfoWindowMembers( Rock.Model.Group group, string personProfileUrl )
        {
            var members = new List<Dictionary<string, object>>();

            // Only active, non-archived members are shown, matching the group members map layer
            // (the /Members/Active endpoint) so the roster agrees with the pins on the map.
            var activeMembers = group.Members
                .Where( m => !m.IsArchived && m.GroupMemberStatus == GroupMemberStatus.Active )
                .OrderBy( m => m.GroupRole.Order )
                .ThenBy( m => m.Person.BirthDate );

            foreach ( var member in activeMembers )
            {
                var phoneNumbers = member.Person.PhoneNumbers
                    .Select( p => new Dictionary<string, object>
                    {
                        { "Name", DefinedValueCache.GetValue( p.NumberTypeValueId ) ?? string.Empty },
                        { "Number", p.ToString() }
                    } )
                    .ToList();

                members.Add( new Dictionary<string, object>
                {
                    { "PersonId", member.Person.Id },
                    { "ProfilePageUrl", personProfileUrl + member.Person.IdKey },
                    { "Role", member.GroupRole.Name },
                    { "NickName", member.Person.NickName },
                    { "LastName", member.Person.LastName },
                    { "PhotoUrl", member.Person.PhotoUrl },
                    { "PhotoId", member.Person.PhotoId },
                    { "ConnectionStatus", DefinedValueCache.GetValue( member.Person.ConnectionStatusValueId ) ?? string.Empty },
                    { "Email", member.Person.Email },
                    { "PhoneTypes", phoneNumbers }
                } );
            }

            return members;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Saves the person's options-panel preferences (whether to show child groups and which
        /// group types scope that layer). Mirrors the WebForms "Apply" button.
        /// </summary>
        /// <param name="showChildGroups">Whether the child-groups layer is shown.</param>
        /// <param name="groupTypeGuids">The group type Guids selected to scope the child-groups layer.</param>
        /// <returns>The resolved group type Ids for the newly saved selection.</returns>
        [BlockAction]
        public BlockActionResult SavePreferences( bool showChildGroups, List<Guid> groupTypeGuids )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( PersonPreferenceKey.ShowChildGroups, showChildGroups.ToTrueFalse() );
            preferences.SetValue( PersonPreferenceKey.GroupTypeIds, groupTypeGuids != null ? string.Join( ",", groupTypeGuids ) : string.Empty );
            preferences.Save();

            var groupTypeIds = ( groupTypeGuids ?? new List<Guid>() )
                .Select( guid => GroupTypeCache.GetId( guid ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            return ActionOk( groupTypeIds );
        }

        /// <summary>
        /// Renders the info window Lava for a clicked map item. The Lava template and linked-page
        /// URLs are resolved from the block's own settings server-side (never trusted from the
        /// client), and VIEW authorization is re-checked before any group data is rendered.
        /// </summary>
        /// <param name="request">Identifies the clicked group and location.</param>
        /// <returns>The rendered info window HTML (empty when the template is intentionally blank).</returns>
        [BlockAction]
        public BlockActionResult GetInfoWindow( GroupMapInfoWindowRequestBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "A request is required." );
            }

            var group = new GroupService( RockContext )
                .Queryable()
                .Include( g => g.GroupType )
                .Include( g => g.Campus )
                .Include( g => g.GroupLocations.Select( gl => gl.Location ) )
                .Include( g => g.Members.Select( m => m.Person.PhoneNumbers ) )
                .Include( g => g.Members.Select( m => m.GroupRole ) )
                .FirstOrDefault( g => g.Id == request.GroupId );

            if ( group == null )
            {
                return ActionBadRequest( "The group could not be found." );
            }

            // Re-check VIEW on every call; the client must never be trusted to have done so.
            if ( !group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this group." );
            }

            // A blank template intentionally suppresses the info window.
            var template = GetAttributeValue( AttributeKey.InfoWindowContents );
            if ( template.IsNullOrWhiteSpace() )
            {
                return ActionOk( string.Empty );
            }

            var mergeFields = BuildInfoWindowMergeFields( group, request.LocationId );

            return ActionOk( template.ResolveMergeFields( mergeFields ) );
        }

        #endregion Block Actions
    }
}
