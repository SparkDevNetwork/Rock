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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupTypeMap;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays groups of a given group type on a map.
    /// </summary>
    [DisplayName( "Group Type Map" )]
    [Category( "Groups" )]
    [Description( "Displays groups on a map." )]
    [IconCssClass( "ti ti-map" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [GroupTypeField(
        "Group Type",
        Description = "The type of group to map.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.GroupType )]

    [IntegerField(
        "Map Height",
        Description = "Height of the map in pixels (default value is 600px)",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Order = 2,
        Key = AttributeKey.MapHeight )]

    [LinkedPage(
        "Group Detail Page",
        Description = "Page to use as a link to the group details (optional).",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.GroupDetailPage )]

    [LinkedPage(
        "Person Profile Page",
        Description = "Page to use as a link to the person profile page (optional).",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.PersonProfilePage )]

    [BooleanField(
        "Show Map Info Window",
        Description = "Control whether a info window should be displayed when clicking on a map point.",
        DefaultBooleanValue = true,
        Order = 5,
        Category = AttributeCategory.InfoWindow,
        Key = AttributeKey.ShowMapInfoWindow )]

    [BooleanField(
        "Include Inactive Groups",
        Description = "Determines if inactive groups should be included on the map.",
        DefaultBooleanValue = false,
        Order = 6,
        Key = AttributeKey.IncludeInactiveGroups )]

    [TextField(
        "Attributes",
        Description = "Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').",
        IsRequired = false,
        Order = 7,
        Category = AttributeCategory.InfoWindow,
        Key = AttributeKey.Attributes )]

    [DefinedValueField(
        "Map Style",
        Description = "The map theme that should be used for styling the map.",
        IsRequired = true,
        AllowMultiple = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        DefaultValue = Rock.SystemGuid.DefinedValue.MAP_STYLE_GOOGLE,
        Order = 8,
        Key = AttributeKey.MapStyle )]

    [CodeEditorField(
        "Info Window Contents",
        Description = "Lava template for the info window. To suppress the window provide a blank template.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 600,
        IsRequired = false,
        DefaultValue = DefaultLavaTemplate,
        Order = 9,
        Category = AttributeCategory.InfoWindow,
        Key = AttributeKey.InfoWindowContents )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "B7E2F4A1-6C3D-4E8B-9A5F-2D7C1E0B4A83" )]
    // TODO The Obsidian block uses a temporary block type Guid while it is validated against the
    // WebForms version. It will adopt the WebForms Guid ("2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF")
    // once the WebForms block is retired.
    // NOTE: Will become: [Rock.SystemGuid.BlockTypeGuid( "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF" )]
    [Rock.SystemGuid.BlockTypeGuid( "3F9A6C2D-1B8E-4A7C-B5D0-8E2F6A1C9D04" )]
    public class GroupTypeMap : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string GroupType = "GroupType";
            public const string MapHeight = "MapHeight";
            public const string GroupDetailPage = "GroupDetailPage";
            public const string PersonProfilePage = "PersonProfilePage";
            public const string ShowMapInfoWindow = "ShowMapInfoWindow";
            public const string IncludeInactiveGroups = "IncludeInactiveGroups";
            public const string Attributes = "Attributes";
            public const string MapStyle = "MapStyle";
            public const string InfoWindowContents = "InfoWindowContents";
        }

        private static class PageParameterKey
        {
            public const string GroupTypeId = "GroupTypeId";
        }

        /// <summary>
        /// Block setting categories, used to group related settings into sections in the block
        /// configuration panel.
        /// </summary>
        private static class AttributeCategory
        {
            /// <summary>Settings that control the marker info window (whether it shows, which
            /// attributes it includes, and its Lava template).</summary>
            public const string InfoWindow = "Info Window";
        }

        /// <summary>
        /// Attribute keys on the DefinedValues this block reads.
        /// </summary>
        private static class DefinedValueAttributeKey
        {
            /// <summary>The Map Style's pipe-delimited list of marker colors.</summary>
            public const string MapStyleColors = "Colors";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// Default Lava template for the group info window. Kept identical to the WebForms block
        /// so existing customized templates continue to render against the same merge fields.
        /// </summary>
        private const string DefaultLavaTemplate = @"
<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4>
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.PostalCode}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            {% for PhoneType in GroupMember.PhoneTypes %}
                <br>{{PhoneType.Name}}: {{PhoneType.Number}}
            {% endfor %}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

";

        /// <summary>
        /// The marker color used when the Map Style does not define one.
        /// </summary>
        private const string DefaultMarkerColor = "FE7569";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<GroupTypeMapInitializationBag, GroupTypeMapOptionsBag>
            {
                Options = GetBoxOptions()
            };

            box.Bag = GetBag();

            return box;
        }

        /// <summary>
        /// Builds the block's configured settings for the client. These do not depend on the
        /// resolved group type and do not change for the life of the rendered block.
        /// </summary>
        /// <returns>The options bag describing how the map should be displayed.</returns>
        private GroupTypeMapOptionsBag GetBoxOptions()
        {
            return new GroupTypeMapOptionsBag
            {
                MapHeight = GetAttributeValue( AttributeKey.MapHeight ).AsIntegerOrNull() ?? 600,
                MapStyleValueGuid = GetAttributeValue( AttributeKey.MapStyle ).AsGuidOrNull(),
                IsInfoWindowShown = GetAttributeValue( AttributeKey.ShowMapInfoWindow ).AsBoolean()
            };
        }

        /// <summary>
        /// Builds the runtime data for the client: the resolved group type's mappable groups,
        /// the groups that could not be mapped, and the marker color. When the map cannot be
        /// displayed an <see cref="GroupTypeMapInitializationBag.ErrorMessage"/> is returned
        /// instead.
        /// </summary>
        /// <returns>The populated runtime bag.</returns>
        private GroupTypeMapInitializationBag GetBag()
        {
            var bag = new GroupTypeMapInitializationBag
            {
                MarkerColor = GetMarkerColor(),
                Groups = new List<GroupTypeMapGroupBag>(),
                UnmappedGroups = new List<GroupTypeMapUnmappedGroupBag>()
            };

            // The group type comes from the block setting (a Guid) or, when that is blank, from
            // the GroupTypeId query string parameter (a legacy integer Id).
            var settingGroupTypeValue = GetAttributeValue( AttributeKey.GroupType );
            var queryStringGroupTypeValue = PageParameter( PageParameterKey.GroupTypeId );

            if ( settingGroupTypeValue.IsNullOrWhiteSpace() && queryStringGroupTypeValue.IsNullOrWhiteSpace() )
            {
                bag.ErrorMessage = "<strong>Group Mapper</strong> Please configure a group type to display as a block setting or pass a GroupTypeId as a query parameter.";
                return bag;
            }

            var groupType = ResolveGroupType( settingGroupTypeValue, queryStringGroupTypeValue );
            if ( groupType == null )
            {
                bag.ErrorMessage = "<strong>Group Mapper</strong> Please configure a group type to display and a location type to use.";
                return bag;
            }

            var includeInactiveGroups = GetAttributeValue( AttributeKey.IncludeInactiveGroups ).AsBoolean();

            // Project just the marker essentials plus the first geo-located location for each group.
            var groupsQuery = new GroupService( RockContext ).Queryable()
                .Where( g => g.GroupTypeId == groupType.Id );

            if ( !includeInactiveGroups )
            {
                groupsQuery = groupsQuery.Where( g => g.IsActive == true );
            }

            var groups = groupsQuery
                .Select( g => new
                {
                    GroupId = g.Id,
                    GroupName = g.Name,
                    Location = g.GroupLocations
                        .Where( l => l.Location.GeoPoint != null )
                        .Select( l => new
                        {
                            l.LocationId,
                            Latitude = l.Location.GeoPoint.Latitude,
                            Longitude = l.Location.GeoPoint.Longitude
                        } )
                        .FirstOrDefault()
                } )
                .ToList();

            foreach ( var group in groups )
            {
                if ( group.Location != null && group.Location.Latitude.HasValue && group.Location.Longitude.HasValue )
                {
                    bag.Groups.Add( new GroupTypeMapGroupBag
                    {
                        GroupId = group.GroupId,
                        LocationId = group.Location.LocationId,
                        Name = group.GroupName,
                        Latitude = group.Location.Latitude.Value,
                        Longitude = group.Location.Longitude.Value
                    } );
                }
                else
                {
                    bag.UnmappedGroups.Add( new GroupTypeMapUnmappedGroupBag
                    {
                        Name = group.GroupName,
                        DetailPageUrl = BuildGroupDetailUrl( group.GroupId )
                    } );
                }
            }

            // Mirror the WebForms behavior: when nothing could be mapped, suppress the map and
            // the unmapped list, showing only the "no groups" message.
            if ( !bag.Groups.Any() )
            {
                bag.ErrorMessage = "No groups were able to be mapped. You may want to check your configuration.";
                bag.UnmappedGroups.Clear();
            }

            return bag;
        }

        /// <summary>
        /// Resolves the group type to map from the block setting (Guid) or the GroupTypeId query
        /// string parameter (legacy integer Id). The block setting takes precedence.
        /// </summary>
        /// <param name="settingGroupTypeValue">The raw Group Type block setting value (a Guid).</param>
        /// <param name="queryStringGroupTypeValue">The raw GroupTypeId query string value (an Id).</param>
        /// <returns>The resolved group type, or <c>null</c> when neither value resolves one.</returns>
        private GroupTypeCache ResolveGroupType( string settingGroupTypeValue, string queryStringGroupTypeValue )
        {
            var settingGroupTypeGuid = settingGroupTypeValue.AsGuidOrNull();
            if ( settingGroupTypeGuid.HasValue )
            {
                return GroupTypeCache.Get( settingGroupTypeGuid.Value );
            }

            var queryStringGroupTypeId = queryStringGroupTypeValue.AsIntegerOrNull();
            if ( queryStringGroupTypeId.HasValue )
            {
                return GroupTypeCache.Get( queryStringGroupTypeId.Value );
            }

            return null;
        }

        /// <summary>
        /// Derives the marker color from the Map Style's Colors attribute (a pipe-delimited list),
        /// falling back to a default when none are configured.
        /// </summary>
        /// <returns>The marker color, without a leading "#".</returns>
        private string GetMarkerColor()
        {
            var mapStyleValue = DefinedValueCache.Get( GetAttributeValue( AttributeKey.MapStyle ).AsGuid() );
            if ( mapStyleValue != null )
            {
                var colors = ( mapStyleValue.GetAttributeValue( DefinedValueAttributeKey.MapStyleColors ) ?? string.Empty )
                    .Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .ToList();

                if ( colors.Any() )
                {
                    return colors.First().Replace( "#", string.Empty );
                }
            }

            return DefaultMarkerColor;
        }

        /// <summary>
        /// Builds the Group Detail Page URL for a group, or an empty string when no page is
        /// configured.
        /// </summary>
        /// <param name="groupId">The Id of the group to link to.</param>
        /// <returns>The group detail URL, or an empty string.</returns>
        private string BuildGroupDetailUrl( int groupId )
        {
            var groupPageReference = new PageReference( GetAttributeValue( AttributeKey.GroupDetailPage ) );
            if ( groupPageReference.PageId <= 0 )
            {
                return string.Empty;
            }

            groupPageReference.Parameters = new Dictionary<string, string>
            {
                { "GroupId", groupId.ToString() }
            };

            return groupPageReference.BuildUrl();
        }

        /// <summary>
        /// Builds the Lava merge-field dictionary for a group's info window. Mirrors the shape
        /// the WebForms block produced so customized templates keep working: the group name and
        /// campus, the clicked location, the configured attribute values, and the group members
        /// with their phone numbers.
        /// </summary>
        /// <param name="group">The group whose marker was clicked.</param>
        /// <param name="locationId">The clicked location's Id, used to resolve the address block.</param>
        /// <returns>The merge-field dictionary consumed by the info window template.</returns>
        private Dictionary<string, object> BuildInfoWindowMergeFields( Rock.Model.Group group, int locationId )
        {
            var mergeFields = new Dictionary<string, object>
            {
                { "GroupId", group.Id },
                { "GroupName", group.Name },
                { "GroupCampus", group.Campus?.Name ?? string.Empty },
                { "GroupMemberTerm", group.GroupType?.GroupMemberTerm ?? string.Empty },
                { "GroupDetailPage", BuildGroupDetailUrl( group.Id ) },
                { "PersonProfilePage", BuildPersonProfileUrl() },
                { "GroupLocation", BuildInfoWindowLocation( group, locationId ) },
                { "Attributes", BuildInfoWindowAttributes( group ) },
                { "GroupMembers", BuildInfoWindowMembers( group ) }
            };

            return mergeFields;
        }

        /// <summary>
        /// Builds the Person Profile Page URL stub. The PersonId is intentionally left blank so
        /// the template can append each member's Id (e.g. <c>{{PersonProfilePage}}{{GroupMember.Id}}</c>).
        /// Returns an empty string when no page is configured.
        /// </summary>
        /// <returns>The person profile URL stub, or an empty string.</returns>
        private string BuildPersonProfileUrl()
        {
            var pageParameters = new Dictionary<string, string> { { "PersonId", string.Empty } };
            return this.GetLinkedPageUrl( AttributeKey.PersonProfilePage, pageParameters );
        }

        /// <summary>
        /// Builds the location merge field for the clicked location (the first geo-located
        /// location that produced the marker), or <c>null</c> when it cannot be resolved.
        /// </summary>
        /// <param name="group">The group whose location should be described.</param>
        /// <param name="locationId">The Id of the clicked location.</param>
        /// <returns>A merge-field dictionary describing the location, or <c>null</c>.</returns>
        private Dictionary<string, object> BuildInfoWindowLocation( Rock.Model.Group group, int locationId )
        {
            var groupLocation = group.GroupLocations.FirstOrDefault( gl => gl.LocationId == locationId );
            if ( groupLocation == null )
            {
                return null;
            }

            return new Dictionary<string, object>
            {
                { "Name", groupLocation.GroupLocationTypeValue?.Value ?? string.Empty },
                { "Street1", groupLocation.Location.Street1 },
                { "Street2", groupLocation.Location.Street2 },
                { "City", groupLocation.Location.City },
                { "State", groupLocation.Location.State },
                { "PostalCode", groupLocation.Location.PostalCode },
                { "Latitude", groupLocation.Location.GeoPoint?.Latitude },
                { "Longitude", groupLocation.Location.GeoPoint?.Longitude }
            };
        }

        /// <summary>
        /// Builds the attribute merge fields for the info window, limited to the attribute keys
        /// configured in the Attributes block setting and formatted for display as HTML.
        /// </summary>
        /// <param name="group">The group whose attribute values should be rendered.</param>
        /// <returns>The list of attribute merge-field dictionaries (Key, Name, Value).</returns>
        private List<Dictionary<string, object>> BuildInfoWindowAttributes( Rock.Model.Group group )
        {
            var attributes = new List<Dictionary<string, object>>();

            var attributeKeys = GetAttributeValue( AttributeKey.Attributes ).SplitDelimitedValues().ToList();
            if ( !attributeKeys.Any() )
            {
                return attributes;
            }

            var groupEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Group ) ).Id;

            // Only attribute values that actually exist for this group are shown, matching the
            // WebForms behavior (defaults were not surfaced).
            var attributeValues = new AttributeValueService( RockContext ).Queryable( "Attribute" )
                .Where( v =>
                    v.Attribute.EntityTypeId == groupEntityTypeId
                    && attributeKeys.Contains( v.Attribute.Key )
                    && v.EntityId == group.Id )
                .ToList();

            var orderedAttributeValues = attributeValues
                .Select( v => new
                {
                    AttributeValue = v,
                    AttributeCache = AttributeCache.Get( v.AttributeId )
                } )
                .OrderBy( x => x.AttributeCache?.Order )
                .ThenBy( x => x.AttributeCache?.Name );

            foreach ( var item in orderedAttributeValues )
            {
                var attributeCache = item.AttributeCache;
                var formattedValue = attributeCache != null
                    ? attributeCache.FieldType.Field.FormatValueAsHtml( null, attributeCache.EntityTypeId, group.Id, item.AttributeValue.Value, attributeCache.QualifierValues, false )
                    : item.AttributeValue.Value;

                attributes.Add( new Dictionary<string, object>
                {
                    { "Key", attributeCache?.Key },
                    { "Name", attributeCache?.Name },
                    { "Value", formattedValue }
                } );
            }

            return attributes;
        }

        /// <summary>
        /// Builds the members portion of the info window merge fields, including each member's
        /// phone numbers.
        /// </summary>
        /// <param name="group">The group whose members should be rendered.</param>
        /// <returns>The list of member merge-field dictionaries.</returns>
        private List<Dictionary<string, object>> BuildInfoWindowMembers( Rock.Model.Group group )
        {
            var members = new List<Dictionary<string, object>>();

            foreach ( var member in group.Members )
            {
                var phoneTypes = member.Person.PhoneNumbers
                    .Select( p => new Dictionary<string, object>
                    {
                        { "Name", p.NumberTypeValue?.Value ?? string.Empty },
                        { "Number", p.ToString() }
                    } )
                    .ToList<object>();

                members.Add( new Dictionary<string, object>
                {
                    { "Id", member.Person.Id },
                    { "GuidP", member.Person.Guid },
                    { "NickName", member.Person.NickName },
                    { "LastName", member.Person.LastName },
                    { "RoleName", member.GroupRole?.Name },
                    { "Email", member.Person.Email },
                    { "PhotoGuid", member.Person.Photo != null ? member.Person.Photo.Guid : Guid.Empty },
                    { "PhoneTypes", phoneTypes }
                } );
            }

            return members;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Renders the info window Lava for a clicked group marker. The Lava template and linked
        /// page URLs are resolved from the block's own settings server-side (never trusted from
        /// the client), and VIEW authorization is re-checked before any group data is rendered.
        /// </summary>
        /// <param name="request">Identifies the clicked group and location.</param>
        /// <returns>The rendered info window HTML (empty when the window is suppressed).</returns>
        [BlockAction]
        public BlockActionResult GetInfoWindow( GroupTypeMapInfoWindowRequestBag request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "A request is required." );
            }

            // A blank template, or the Show Map Info Window setting being off, intentionally
            // suppresses the info window.
            if ( !GetAttributeValue( AttributeKey.ShowMapInfoWindow ).AsBoolean() )
            {
                return ActionOk( string.Empty );
            }

            var template = GetAttributeValue( AttributeKey.InfoWindowContents );
            if ( template.IsNullOrWhiteSpace() )
            {
                return ActionOk( string.Empty );
            }

            var group = new GroupService( RockContext ).Get( request.GroupId );
            if ( group == null )
            {
                return ActionBadRequest( "The group could not be found." );
            }

            // Re-check VIEW on every call; the client must never be trusted to have done so.
            if ( !group.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "You are not authorized to view this group." );
            }

            var mergeFields = BuildInfoWindowMergeFields( group, request.LocationId );

            return ActionOk( template.ResolveMergeFields( mergeFields ) );
        }

        #endregion Block Actions
    }
}
