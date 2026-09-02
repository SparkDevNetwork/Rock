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
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock;
using Rock.Attribute;
using Rock.Core.Geography;
using Rock.Core.Geography.Classes;
using Rock.Data;
using Rock.Enums.Geography;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Utility.GroupFinder;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Group.GroupFinder;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Block for people to find a group through a pill filter bar, a grid of group cards, and an optional map.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Finder" )]
    [Category( "Group" )]
    [Description( "Block for people to find a group through filters, a card list, and an optional map." )]
    [IconCssClass( "ti ti-map-search" )]

    #region Block Attributes

    [GroupTypesField( "Group Types",
        Key = AttributeKey.GroupTypes,
        Order = 0,
        Category = AttributeCategory.CustomSetting,
        Description = "The group types whose groups the finder offers, and whose attributes the filter and card settings are drawn from.",
        EnhancedSelection = true,
        DefaultValue = Rock.SystemGuid.GroupType.GROUPTYPE_SMALL_GROUP,
        IsRequired = true )]

    [BooleanField( "Hide Campus Filters",
        Key = AttributeKey.HideCampusFilters,
        Order = 10,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled, the Campus filter is not shown in the filter bar.",
        DefaultBooleanValue = false )]

    [DefinedValueField( "Campus Types",
        Key = AttributeKey.CampusTypes,
        Order = 50,
        Category = AttributeCategory.CustomSetting,
        Description = "The campus types offered by the campus filter.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL )]

    [DefinedValueField( "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Order = 60,
        Category = AttributeCategory.CustomSetting,
        Description = "The campus statuses offered by the campus filter.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN )]

    [CustomDropdownListField( "Distance Calculation",
        Key = AttributeKey.DistanceCalculation,
        Order = 70,
        Category = AttributeCategory.CustomSetting,
        Description = "How far each group is from the visitor, how the visitor shares where they are, and how the distance is measured. Each mode adds to the one before it: My Current Location shares the visitor's location through the browser and measures a direct line, Address or Zip Code adds an address box (which requires geocoding), and Driving Distance adds drive time and driving miles (which calls a routing provider). None offers no location search.",
        ListSource = "None^None,StraightLineCurrentLocation^Straight-Line Distance (My Current Location),StraightLineAddress^Straight-Line Distance (Address or Zip Code),Driving^Driving Distance",
        IsRequired = true,
        DefaultValue = "StraightLineCurrentLocation" )]

    [CustomCheckboxListField( "Supported Meeting Styles",
        Key = AttributeKey.SupportedMeetingStyles,
        Order = 80,
        Category = AttributeCategory.CustomSetting,
        Description = "The meeting styles offered by the Where filter. When none are selected the Meeting Style filter is hidden.",
        ListSource = "InPerson^In-Person,Online^Online,Hybrid^Hybrid",
        IsRequired = false,
        DefaultValue = "InPerson" )]

    [BooleanField( "Display Day of Week Filter",
        Key = AttributeKey.DisplayDayOfWeekFilter,
        Order = 90,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled, a Day of Week filter is shown in the When section.",
        DefaultBooleanValue = true )]

    [BooleanField( "Display Time of Day Filter",
        Key = AttributeKey.DisplayTimeOfDayFilter,
        Order = 100,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled, a Time of Day filter is shown in the When section.",
        DefaultBooleanValue = false )]

    [BooleanField( "Live Text Search",
        Key = AttributeKey.EnableLiveSearch,
        Order = 110,
        Category = AttributeCategory.CustomSetting,
        Description = "Renders a text field that filters groups by name as the visitor types.",
        DefaultBooleanValue = true )]

    [AttributeField( "Display Attribute Filters",
        Key = AttributeKey.DisplayAttributeFilters,
        Order = 120,
        Category = AttributeCategory.CustomSetting,
        Description = "The group attributes an individual can filter results by, rendered in the More Filters modal. Mutually exclusive with Featured Attributes.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        AllowMultiple = true,
        IsRequired = false )]

    [AttributeField( "Featured Attributes",
        Key = AttributeKey.FeaturedAttributes,
        Order = 130,
        Category = AttributeCategory.CustomSetting,
        Description = "The group attributes promoted into the What section of the filter bar as pills. Mutually exclusive with Display Attribute Filters, and limited to Single-select, Multi-select, and Boolean field types.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        AllowMultiple = true,
        IsRequired = false )]

    [AttributeField( "Show Attribute on Card",
        Key = AttributeKey.ShowAttributeOnCard,
        Order = 140,
        Category = AttributeCategory.CustomSetting,
        Description = "The group attributes displayed on each result card.",
        EntityTypeGuid = Rock.SystemGuid.EntityType.GROUP,
        AllowMultiple = true,
        IsRequired = false )]

    [BooleanField( "Show Image",
        Key = AttributeKey.ShowImage,
        Order = 150,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled, the group image is shown on each result card.",
        DefaultBooleanValue = false )]

    [BooleanField( "Show Average Age",
        Key = AttributeKey.ShowAverageAge,
        Order = 160,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled, the average member age is shown on each result card.",
        DefaultBooleanValue = false )]

    [BooleanField( "Show Map",
        Key = AttributeKey.ShowMap,
        Order = 170,
        Category = AttributeCategory.CustomSetting,
        Description = "When enabled the layout switches to a side by side arrangement of cards and a map.",
        DefaultBooleanValue = false )]

    [ColorField( "Group Marker Color",
        Key = AttributeKey.GroupMarkerColor,
        Order = 175,
        Category = AttributeCategory.CustomSetting,
        Description = "The color of the group markers on the map. This one color drives every state via opacity: a solid 2px border with a light fill when hovered or selected, and a lighter border and fill otherwise.",
        DefaultValue = "#2B7FFF" )]

    [ColorField( "Current Location Marker Color",
        Key = AttributeKey.CurrentLocationMarkerColor,
        Order = 176,
        Category = AttributeCategory.CustomSetting,
        Description = "The color of the \"you are here\" proximity marker (the visitor's current location or entered address) on the map.",
        DefaultValue = "#EF4444" )]

    [DefinedValueField( "Map Style",
        Key = AttributeKey.MapStyle,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.MAP_STYLES,
        Order = 177,
        Category = AttributeCategory.CustomSetting,
        Description = "The map style applied to the results map. When not set, the block's default style is used.",
        IsRequired = false )]

    [LinkedPage( "Register Page",
        Key = AttributeKey.RegisterPage,
        Order = 180,
        Category = AttributeCategory.CustomSetting,
        Description = "The page a visitor is sent to when signing up for a group.",
        IsRequired = false )]

    [CodeEditorField( "Group Card Template",
        Key = AttributeKey.GroupCardTemplate,
        Order = 190,
        Category = AttributeCategory.CustomSetting,
        Description = "The Lava template that renders the content of each result card, from the card border inward. The block owns the card's border, corner radius, highlighting, and click-to-select behavior; this template controls everything inside, including the padding and the register button. Clear this to reset to the default template.",
        EditorMode = Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = AttributeDefault.GroupCardTemplate )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "B6E7A1C2-0D4F-4E90-9C3A-2F1B7A0E5D64" )]
    [Rock.SystemGuid.BlockTypeGuid( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53" )]
    public class GroupFinder : RockBlockType, IHasCustomActions
    {
        #region Keys

        /*
            07/09/26 - JMH

            Settings the new block shares with the legacy WebForms GroupFinder
            reuse the legacy attribute Key strings (GroupType, CampusTypes,
            CampusStatuses, AttributeFilters, AttributeColumns, ShowMap,
            RegisterPage). This lets a future replacement of legacy instances
            inherit their configured values. Settings unique to the new block
            get new keys.

            Reason: Enable value inheritance if the legacy block is chopped later.
        */
        private static class AttributeKey
        {
            // Carried from the legacy block (reuse the legacy Key).
            public const string GroupTypes = "GroupType";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string DisplayAttributeFilters = "AttributeFilters";
            public const string ShowAttributeOnCard = "AttributeColumns";
            public const string ShowMap = "ShowMap";
            public const string RegisterPage = "RegisterPage";

            // New to this block.
            public const string HideCampusFilters = "HideCampusFilters";
            public const string DistanceCalculation = "DistanceCalculation";
            public const string SupportedMeetingStyles = "SupportedMeetingStyles";
            public const string DisplayDayOfWeekFilter = "DisplayDayOfWeekFilter";
            public const string DisplayTimeOfDayFilter = "DisplayTimeOfDayFilter";
            public const string EnableLiveSearch = "EnableLiveSearch";
            public const string FeaturedAttributes = "FeaturedAttributes";
            public const string ShowImage = "ShowImage";
            public const string ShowAverageAge = "ShowAverageAge";
            public const string GroupMarkerColor = "GroupMarkerColor";
            public const string CurrentLocationMarkerColor = "CurrentLocationMarkerColor";
            public const string MapStyle = "MapStyle";
            public const string GroupCardTemplate = "GroupCardTemplate";
        }

        private static class AttributeCategory
        {
            /// <summary>
            /// Attributes edited through the custom settings panel rather than the standard block-settings editor.
            /// </summary>
            public const string CustomSetting = "CustomSetting";
        }

        private static class AttributeDefault
        {
            /// <summary>
            /// The default group card template. Reproduces the block's built-in card content and
            /// documents the available merge fields in a leading comment block.
            /// </summary>
            public const string GroupCardTemplate = @"/-
    Available merge fields for the group card:
      Group                - the full group entity (Group.Name, Group.Description, Group.Schedule, Group.GroupType, Group.Attributes, ...)
      GroupTypeName        - group type name, shown as the badge
      GroupTypeColor       - group type color
      ShowImage            - whether the card image area is shown (the ""Show Image"" block setting)
      ImageUrl             - group photo URL, or empty when there is no photo
      ScheduleText         - friendly schedule text (day and time)
      CampusName           - campus name, or empty when filtered to a single campus
      AverageAge           - average member age, or empty when not shown
      StraightLineDistance - approximate straight-line miles (shown with a ~), or empty
      DrivingDistance      - calculated driving miles, or empty when the group cannot be routed
      DrivingMinutes       - calculated driving time in minutes, or empty when the group cannot be routed
      DriveTime            - calculated driving time as a friendly label (""1 hr 20 min""), or empty when the group cannot be routed
      Attributes           - the ""show on card"" attributes, each with Label, Value, and IconCssClass
      RegisterUrl          - the register page URL for this group, or empty when no register page is set
-/
<div class=""groupfinder-card-content"">
    {% if ShowImage %}
    {% if ImageUrl and ImageUrl != '' %}
    <div class=""groupfinder-card-media"">
        <img class=""groupfinder-card-image"" src=""{{ ImageUrl }}"" alt=""{{ Group.Name | Escape }}"" />
        {% if GroupTypeName and GroupTypeName != '' %}<span class=""groupfinder-card-badge"">{{ GroupTypeName | Escape }}</span>{% endif %}
    </div>
    {% else %}
    <div class=""groupfinder-card-media is-fallback"" style=""--groupfinder-fallback-color: {{ GroupTypeColor | Default:'#4fd1c5' }}"">
        {% if GroupTypeName and GroupTypeName != '' %}<span class=""groupfinder-card-badge"">{{ GroupTypeName | Escape }}</span>{% endif %}
    </div>
    {% endif %}
    {% endif %}

    <div class=""groupfinder-card-body"">
        {% if DrivingDistance or StraightLineDistance or AverageAge %}
        <div class=""groupfinder-card-meta"">
            {% if DriveTime and DriveTime != '' %}
            <span class=""groupfinder-card-distance""><strong>Drive Time:</strong> {{ DriveTime }}{% if DrivingDistance %} ({{ DrivingDistance | Format:'0.0' }} mi){% endif %}</span>
            {% elsif StraightLineDistance %}
            <span class=""groupfinder-card-distance""><strong>Distance:</strong> ~{{ StraightLineDistance | Format:'0.0' }} mile{% if StraightLineDistance != 1 %}s{% endif %}</span>
            {% endif %}
            {% if AverageAge %}
            <span class=""groupfinder-card-average-age""><strong>Avg Age:</strong> {{ AverageAge }} yrs</span>
            {% endif %}
        </div>
        <hr class=""groupfinder-card-divider"" />
        {% endif %}

        <h3 class=""groupfinder-card-title"">{{ Group.Name | Escape }}</h3>

        {% if ScheduleText and ScheduleText != '' %}
        <div class=""groupfinder-card-schedule"">{{ ScheduleText | Escape }}</div>
        {% endif %}

        {% if Group.Description and Group.Description != '' %}
        <p class=""groupfinder-card-description"">{{ Group.Description | Escape }}</p>
        {% endif %}

        {% assign attributeCount = Attributes | Size %}
        {% assign hasCampus = false %}
        {% if CampusName and CampusName != '' %}{% assign hasCampus = true %}{% endif %}
        {% if hasCampus or attributeCount > 0 %}
        <ul class=""groupfinder-card-attribute-list"">
            {% if CampusName and CampusName != '' %}
            <li class=""groupfinder-card-attribute""><i class=""ti ti-map-pin""></i><span>{{ CampusName | Escape }}</span></li>
            {% endif %}
            {% for attribute in Attributes %}
            <li class=""groupfinder-card-attribute"">
                {% if attribute.IconCssClass and attribute.IconCssClass != '' %}<i class=""{{ attribute.IconCssClass }}""></i>{% endif %}
                <span>{{ attribute.Value | Escape }}</span>
            </li>
            {% endfor %}
        </ul>
        {% endif %}

        {% if RegisterUrl and RegisterUrl != '' %}
        <div class=""groupfinder-card-footer"">
            <a class=""groupfinder-card-action btn btn-primary"" href=""{{ RegisterUrl }}"">Register</a>
        </div>
        {% endif %}
    </div>
</div>";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The number of groups returned per page. The server skips and takes this many per request and
        /// reports it on the results bag so the client can size the pager.
        /// </summary>
        private const int ResultsPageSize = 12;

        /// <summary>
        /// The most candidate groups considered for a single result set. When a viewport is supplied
        /// this bounds the coarse pre-filter that feeds the in-memory fuzzed-location test; otherwise
        /// it caps the whole set. Larger than a single page so the fuzzed-location test has a full
        /// candidate pool to draw from.
        /// </summary>
        private const int MaxResults = 1000;

        /// <summary>
        /// Meters per degree of latitude (constant); used to convert a metric offset into a coordinate shift.
        /// </summary>
        private const double MetersPerDegreeLatitude = 111320;

        /// <summary>
        /// Meters per mile; used to present the engine's metric distances in miles.
        /// </summary>
        private const double MetersPerMile = 1609.344;

        /// <summary>
        /// The radius, in miles, searched around an origin when there is no larger geocoded viewport to size the area (current location, a campus, a coordinate, or a precise address). Sized to a convenient drive for a weekly group rather than a whole metro.
        /// </summary>
        private const double DefaultSearchRadiusMiles = 5;

        /// <summary>
        /// The value stored for Supported Meeting Styles when an administrator has cleared every style.
        /// </summary>
        /// <remarks>
        /// Rock restores an attribute's DefaultValue whenever its stored value is blank, so a truly empty
        /// save would resurrect the default styles. This sentinel is a non-blank value outside the meeting
        /// style set, so the cleared state persists; the read path strips it back to an empty selection.
        /// </remarks>
        private const string MeetingStylesClearedSentinel = "0";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var meetingStyles = GetSupportedMeetingStyles();
            var featuredAttributeFilters = GetFeaturedAttributeFilters();
            var modalAttributeFilters = GetModalAttributeFilters( featuredAttributeFilters.Select( f => f.AttributeKey ).ToList() );

            // With a distance calculation chosen, the client defaults the Where filter to the visitor's
            // location: their device location if allowed, otherwise this server-side best guess (their
            // profile address, then IP geolocation). Only computed when it is needed to avoid the lookup.
            var distanceCalculation = GetDistanceCalculation();
            var proximityEnabled = distanceCalculation != DistanceCalculationMode.None;
            (double? Latitude, double? Longitude) visitorLocation = ( null, null );
            if ( proximityEnabled )
            {
                visitorLocation = GetVisitorLocationGuess();
            }

            // The block leans on two independent Google keys, so the client degrades gracefully when
            // either is absent: the client map key renders the map (no key -> list-only), and the
            // server key geocodes a typed address (no key -> hide the address box, keep current
            // location, which needs no geocoding). Current-location and all non-proximity filters work
            // regardless.
            var globalAttributes = GlobalAttributesCache.Get();
            var isMapAvailable = globalAttributes.GetValue( "GoogleAPIKey" ).IsNotNullOrWhiteSpace();
            var isGeocodingAvailable = globalAttributes.GetValue( "GoogleApiKeyServer" ).IsNotNullOrWhiteSpace();
            var isMapConfigured = GetAttributeValue( AttributeKey.ShowMap ).AsBoolean();

            // The address box belongs to Straight-Line (Address or Zip Code) and up; current location alone
            // needs no geocoding. The settings panel already probed geocoding before this mode could be
            // saved, so the block trusts the mode here and lets a typed address degrade gracefully if the
            // key has since gone missing.
            var isLocationSearchEnabled = distanceCalculation >= DistanceCalculationMode.StraightLineAddress;

            // With no group types selected the finder has nothing to search, so the block shows only the
            // configuration message and hides the rest (IsUnconfigured drives that on the client).
            var isGroupTypeMissing = GetAttributeValue( AttributeKey.GroupTypes ).IsNullOrWhiteSpace();

            // Each Where/When/What segment carries no setting of its own: it renders when at least one of
            // its filters is configured and disappears when none are. Contents: Where = Meeting Style
            // and/or the proximity location search; When = Day of Week and/or Time of Day; What = Live
            // Search and/or Featured Attributes.
            var isMeetingStyleShown = meetingStyles.Any();
            var isDayOfWeekShown = GetAttributeValue( AttributeKey.DisplayDayOfWeekFilter ).AsBoolean();
            var isTimeOfDayShown = GetAttributeValue( AttributeKey.DisplayTimeOfDayFilter ).AsBoolean();
            var isLiveSearchEnabled = GetAttributeValue( AttributeKey.EnableLiveSearch ).AsBoolean();
            var hasFeaturedAttributes = featuredAttributeFilters.Any();

            var hasWhereFilters = isMeetingStyleShown || proximityEnabled;
            var hasWhenFilters = isDayOfWeekShown || isTimeOfDayShown;
            var hasWhatFilters = isLiveSearchEnabled || hasFeaturedAttributes;

            var isCampusFilterHidden = GetAttributeValue( AttributeKey.HideCampusFilters ).AsBoolean();
            var filterCampuses = GetFilterCampuses();
            var hasCampuses = filterCampuses.Any();

            // The Campus segment is the exception among the segments: hiding it is a deliberate choice (the
            // Hide Campus Filter setting), so an empty-but-not-hidden Campus filter is a misconfiguration worth
            // flagging. The Where/When/What segments hide by simply having no filters configured, which is an
            // intentional way to remove a segment and needs no warning.
            var isCampusFilterEmpty = !isCampusFilterHidden && !hasCampuses;

            var configurationWarning = GetConfigurationWarning( isGroupTypeMissing, isMapConfigured && !isMapAvailable, isLocationSearchEnabled && !isGeocodingAvailable, isCampusFilterEmpty );

            return new GroupFinderInitializationBox
            {
                IsImageShown = GetAttributeValue( AttributeKey.ShowImage ).AsBoolean(),
                IsMapShown = isMapConfigured && isMapAvailable,
                GroupMarkerColor = GetAttributeValue( AttributeKey.GroupMarkerColor ),
                CurrentLocationMarkerColor = GetAttributeValue( AttributeKey.CurrentLocationMarkerColor ),
                MapStyleValueGuid = GetAttributeValue( AttributeKey.MapStyle ),
                VisitorLatitude = visitorLocation.Latitude,
                VisitorLongitude = visitorLocation.Longitude,
                IsProximityEnabled = proximityEnabled,
                IsLocationSearchAvailable = isLocationSearchEnabled,
                ConfigurationWarning = configurationWarning.Message,
                ConfigurationWarningItems = configurationWarning.Items,
                IsUnconfigured = isGroupTypeMissing,
                PageSize = ResultsPageSize,
                IsCampusFilterShown = !isCampusFilterHidden && hasCampuses,
                IsWhereFilterShown = hasWhereFilters,
                IsWhenFilterShown = hasWhenFilters,
                IsWhatFilterShown = hasWhatFilters,
                IsMeetingStyleFilterShown = isMeetingStyleShown,
                IsDayOfWeekFilterShown = isDayOfWeekShown,
                IsTimeOfDayFilterShown = isTimeOfDayShown,
                IsLiveSearchEnabled = isLiveSearchEnabled,
                Campuses = filterCampuses,
                MeetingStyles = meetingStyles
                    .Select( v => new ListItemBag { Value = v, Text = GetMeetingStyleDisplayText( v ) } )
                    .ToList(),
                FeaturedAttributeFilters = featuredAttributeFilters,
                ModalAttributeFilters = modalAttributeFilters,
                IsMoreFiltersShown = modalAttributeFilters.Any(),
                RegisterPageUrl = this.GetLinkedPageUrl( AttributeKey.RegisterPage, new Dictionary<string, string>() ),
                // No groups ship in the box; the client makes the first request on mount (an unfiltered
                // browse of the first page), so the payload stays small and the map can size itself to
                // the returned markers.
                Results = null
            };
        }

        /// <summary>
        /// Gets the configured distance calculation.
        /// </summary>
        /// <remarks>
        /// An unset or unrecognized value reads as <see cref="DistanceCalculationMode.StraightLineCurrentLocation"/>,
        /// the setting's default, since that mode needs nothing configured to work.
        /// </remarks>
        /// <returns>The configured mode.</returns>
        private DistanceCalculationMode GetDistanceCalculation()
        {
            return GetAttributeValue( AttributeKey.DistanceCalculation )
                .ConvertToEnumOrNull<DistanceCalculationMode>() ?? DistanceCalculationMode.StraightLineCurrentLocation;
        }

        /// <summary>
        /// The display label for a Supported Meeting Styles value, taken from the <see cref="MeetingStyle"/> enum's Description.
        /// </summary>
        /// <remarks>
        /// The label is single-sourced from the enum's Description attributes, which the client also reads
        /// (via the generated MeetingStyleDescription). The Supported Meeting Styles attribute's list source
        /// still repeats these labels as a compile-time literal it cannot derive from the enum; keep that one
        /// in step with the enum's Descriptions.
        /// </remarks>
        /// <param name="value">The meeting-style value (a <see cref="MeetingStyle"/> name).</param>
        /// <returns>The display label for the value.</returns>
        private static string GetMeetingStyleDisplayText( string value )
        {
            return Enum.TryParse<MeetingStyle>( value, out var meetingStyle )
                ? meetingStyle.ConvertToString()
                : value;
        }

        /// <summary>
        /// Gets the configured Supported Meeting Styles, with the "cleared" sentinel removed.
        /// </summary>
        /// <remarks>
        /// A blank value would be restored to the attribute's default by Rock, so an administrator who clears
        /// every style is saved <see cref="MeetingStylesClearedSentinel"/> instead. This strips that sentinel
        /// so callers see the real selection, which is empty when the Meeting Style filter has been cleared.
        /// </remarks>
        /// <returns>The selected meeting style values, empty when the filter has been cleared.</returns>
        private List<string> GetSupportedMeetingStyles()
        {
            return GetAttributeValue( AttributeKey.SupportedMeetingStyles )
                .SplitDelimitedValues()
                .Where( v => v != MeetingStylesClearedSentinel )
                .ToList();
        }

        /// <summary>
        /// Builds the message shown above the block: an everyone-visible message when no group types are selected (specific for an administrator, general otherwise), an administrator-only lead plus a bulleted list of issues when a Google key is missing or the Campus filter is empty, or nulls when nothing needs saying.
        /// </summary>
        /// <param name="isGroupTypeMissing">Whether no group types are selected, leaving the finder with nothing to search.</param>
        /// <param name="isMapKeyMissing">Whether the map is enabled but the client Google Maps key is absent.</param>
        /// <param name="isGeocodingKeyMissing">Whether proximity is enabled but the server Google geocoding key is absent.</param>
        /// <param name="isCampusFilterEmpty">Whether the Campus segment is visible but no campuses are available to show.</param>
        /// <returns>The lead message (or <c>null</c> when nothing needs saying) and the bulleted issue list (<c>null</c> unless the not-fully-configured lead is returned).</returns>
        private (string Message, List<string> Items) GetConfigurationWarning( bool isGroupTypeMissing, bool isMapKeyMissing, bool isGeocodingKeyMissing, bool isCampusFilterEmpty )
        {
            var isAdministrator = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            // No group types means the finder has nothing to search, so everyone sees a message (the client
            // hides the rest of the block); an administrator gets a specific, actionable one.
            if ( isGroupTypeMissing )
            {
                var message = isAdministrator
                    ? "Group Finder has no group types selected, so it has nothing to show. Choose at least one group type in the block settings."
                    : "Groups are not available right now. Please check back soon.";
                return ( message, null );
            }

            // The remaining problems are graceful degradations (a hidden map, address search off, or an empty
            // Campus filter) that only an administrator can fix, so a visitor just gets the degraded experience
            // with no message.
            if ( !isAdministrator )
            {
                return ( null, null );
            }

            var issues = new List<string>();
            if ( isMapKeyMissing )
            {
                issues.Add( "The map is hidden because the 'Google API Key' global attribute is not set." );
            }
            if ( isGeocodingKeyMissing )
            {
                issues.Add( "Address search is unavailable because the 'Google API Key Server' global attribute is not set." );
            }
            if ( isCampusFilterEmpty )
            {
                issues.Add( "The Campus filter is enabled but there are no campuses to show. Add an active campus (or adjust the Campus Types and Statuses in the block settings), or hide the Campus filter." );
            }

            if ( issues.Count == 0 )
            {
                return ( null, null );
            }

            return ( "Group Finder is not fully configured. Only administrators see this message:", issues );
        }

        /// <summary>
        /// Gets a best guess of the visitor's location for when device geolocation is unavailable: the signed-in person's mapped home address, then the request's IP geolocation.
        /// </summary>
        /// <returns>The guessed latitude and longitude, or nulls when neither source resolves.</returns>
        private (double? Latitude, double? Longitude) GetVisitorLocationGuess()
        {
            var homePoint = RequestContext.CurrentPerson?.GetHomeLocation( RockContext )?.GeoPoint;
            if ( homePoint?.Latitude != null && homePoint.Longitude != null )
            {
                return ( homePoint.Latitude, homePoint.Longitude );
            }

            var geolocation = RequestContext.ClientInformation?.Geolocation;
            if ( geolocation?.Latitude != null && geolocation.Longitude != null )
            {
                return ( geolocation.Latitude, geolocation.Longitude );
            }

            // Last resort, so the finder still estimates a location when nothing about the visitor is
            // known (no profile address, no IP geolocation): the organization's own location.
            var organizationPoint = GetOrganizationPoint();
            if ( organizationPoint?.Latitude != null && organizationPoint.Longitude != null )
            {
                return ( organizationPoint.Latitude, organizationPoint.Longitude );
            }

            return ( null, null );
        }

        /// <summary>
        /// Gets the organization's own geocoded location as a coarse location estimate: its configured address, then the first active campus with a mapped location.
        /// </summary>
        /// <returns>The organization's geography point, or null when neither the organization address nor any campus is geocoded.</returns>
        private System.Data.Entity.Spatial.DbGeography GetOrganizationPoint()
        {
            var locationService = new LocationService( RockContext );

            // The configured organization address, when it maps to a geocoded location.
            var organizationLocationGuid = GlobalAttributesCache.Value( "OrganizationAddress" ).AsGuidOrNull();
            if ( organizationLocationGuid.HasValue )
            {
                var organizationPoint = locationService.Get( organizationLocationGuid.Value )?.GeoPoint;
                if ( organizationPoint != null )
                {
                    return organizationPoint;
                }
            }

            // Otherwise the first active campus that has a mapped location.
            var campusLocationIds = CampusCache.All()
                .Where( c => c.IsActive != false && c.LocationId.HasValue )
                .Select( c => c.LocationId.Value )
                .ToList();

            return locationService.Queryable()
                .Where( l => campusLocationIds.Contains( l.Id ) && l.GeoPoint != null )
                .Select( l => l.GeoPoint )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the featured attribute filters (a subset of the display attribute filters promoted into the What bar as pills).
        /// </summary>
        /// <returns>The featured attribute pill filters, limited to pill-eligible field types.</returns>
        private List<GroupFinderAttributeFilterBag> GetFeaturedAttributeFilters()
        {
            // Featured and Display are disjoint sets (the settings panel keeps an attribute out of one list
            // once it is in the other), so a featured pill is driven solely by the Featured setting - it is
            // not required to also appear in Display.
            var featuredGuids = GetAttributeValue( AttributeKey.FeaturedAttributes ).SplitDelimitedValues().AsGuidList();

            var filters = new List<GroupFinderAttributeFilterBag>();
            foreach ( var guid in featuredGuids )
            {
                var attribute = AttributeCache.Get( guid );
                var options = attribute != null ? GetPillOptions( attribute ) : null;
                if ( options == null )
                {
                    continue;
                }

                filters.Add( new GroupFinderAttributeFilterBag
                {
                    AttributeKey = attribute.Key,
                    Label = attribute.Name,
                    IconCssClass = attribute.IconCssClass,
                    Options = options
                } );
            }

            return filters;
        }

        /// <summary>
        /// Gets the non-featured attribute filters rendered in the More Filters modal via the standard field-type control.
        /// </summary>
        /// <param name="featuredAttributeKeys">The attribute keys already promoted as featured pills, which are excluded here.</param>
        /// <returns>The modal attribute filters, limited to engine-filterable field types.</returns>
        private List<GroupFinderModalFilterBag> GetModalAttributeFilters( List<string> featuredAttributeKeys )
        {
            var displayGuids = GetAttributeValue( AttributeKey.DisplayAttributeFilters ).SplitDelimitedValues().AsGuidList();

            var filters = new List<GroupFinderModalFilterBag>();
            foreach ( var guid in displayGuids )
            {
                var attribute = AttributeCache.Get( guid );
                if ( attribute == null || featuredAttributeKeys.Contains( attribute.Key ) || !attribute.FieldType.Field.HasFilterControl() )
                {
                    continue;
                }

                filters.Add( new GroupFinderModalFilterBag
                {
                    AttributeKey = attribute.Key,
                    Attribute = PublicAttributeHelper.GetPublicAttributeForEdit( attribute )
                } );
            }

            return filters;
        }

        /// <summary>
        /// Gets the selectable pill options for an attribute, or null when the field type is not pill-eligible.
        /// </summary>
        /// <param name="attribute">The attribute to read options from.</param>
        /// <returns>The pill options (value and text), or null for non-pill field types.</returns>
        private List<ListItemBag> GetPillOptions( AttributeCache attribute )
        {
            var field = attribute.FieldType.Field;

            if ( field is BooleanFieldType )
            {
                return new List<ListItemBag>
                {
                    new ListItemBag { Value = "True", Text = "Yes" },
                    new ListItemBag { Value = "False", Text = "No" }
                };
            }

            if ( field is SelectSingleFieldType || field is SelectMultiFieldType )
            {
                var publicConfig = field.GetPublicConfigurationValues( attribute.ConfigurationValues, Rock.Field.ConfigurationValueUsage.Edit, null );
                return publicConfig.GetValueOrNull( "values" ).FromJsonOrNull<List<ListItemBag>>() ?? new List<ListItemBag>();
            }

            return null;
        }

        /// <summary>
        /// Gets the attributes configured for a filter or card setting, resolved from its stored attribute guids.
        /// </summary>
        /// <param name="attributeKey">The block attribute key whose value is a list of attribute guids.</param>
        /// <returns>The resolved attributes, skipping any guid that no longer resolves.</returns>
        private List<AttributeCache> GetConfiguredAttributes( string attributeKey )
        {
            return GetAttributeValue( attributeKey ).SplitDelimitedValues().AsGuidList()
                .Select( guid => AttributeCache.Get( guid ) )
                .Where( attribute => attribute != null )
                .ToList();
        }

        /// <summary>
        /// Gets the campuses offered by the Campus filter, honoring the configured campus types and statuses.
        /// </summary>
        /// <returns>The campus items, each with its name and unique identifier.</returns>
        private List<ListItemBag> GetFilterCampuses()
        {
            var campusTypeIds = GetAttributeValue( AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList()
                .Select( g => DefinedValueCache.Get( g )?.Id ).Where( id => id.HasValue ).Select( id => id.Value ).ToList();
            var campusStatusIds = GetAttributeValue( AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList()
                .Select( g => DefinedValueCache.Get( g )?.Id ).Where( id => id.HasValue ).Select( id => id.Value ).ToList();

            return CampusCache.All()
                .Where( c => c.IsActive == true )
                .Where( c => !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                .Where( c => !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) )
                .OrderBy( c => c.Order )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the group type ids the finder is configured to search.
        /// </summary>
        /// <returns>The configured group type ids, or an empty list when none are configured.</returns>
        private List<int> GetConfiguredGroupTypeIds()
        {
            return GetAttributeValue( AttributeKey.GroupTypes )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( g => GroupTypeCache.Get( g )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();
        }

        /// <summary>
        /// Builds the engine options from the block settings and the visitor's query.
        /// </summary>
        /// <param name="query">The visitor's filter selections.</param>
        /// <param name="groupTypeIds">The configured group type ids to search.</param>
        /// <returns>The options to pass to the group finder engine.</returns>
        private GroupFinderOptions BuildOptions( GroupFinderQueryBag query, List<int> groupTypeIds )
        {
            var options = new GroupFinderOptions
            {
                GroupTypeIds = groupTypeIds,
                EnablePublicFilter = true,
                ReturnOnlyClosestLocationPerGroup = true,
                Include = "Group.Schedule,Location"
            };

            // The proximity origin (and the boundary it implies) is resolved in GetGroupResults, which
            // needs the geocoded viewport as well as the point; only the raw origin string is carried
            // here for the engine.
            if ( GetDistanceCalculation() != DistanceCalculationMode.None && query.Origin.IsNotNullOrWhiteSpace() )
            {
                options.Origin = query.Origin;
            }

            return options;
        }

        /// <summary>
        /// Resolves the proximity origin the visitor supplied into a point and, for a typed location, the viewport sized to it.
        /// </summary>
        /// <param name="origin">The visitor's origin: either "latitude,longitude" (current location or a searched area), or a typed address, ZIP, or city.</param>
        /// <returns>The resolved point and its viewport (the viewport is null for a coordinate origin or when the provider returned none), or a null point when the origin could not be geocoded.</returns>
        private (GeographyPoint Point, GeographyBounds Viewport) ResolveOrigin( string origin )
        {
            /*
                7/21/26 - JMH

                The origin is either a "latitude,longitude" pair (current location or a searched map
                area) or a typed address, ZIP, or city. GroupFinderHelper.GetOriginPoint is not used
                because it reads any all-digit input (e.g. "85308") as a person id and returns null for a
                ZIP code. Parse the coordinate pair first, then geocode everything else.

                Geocoding is best-effort: a missing or misconfigured Google server key throws from the
                geocoder's constructor, and quota or transient failures return null. Swallow both so the
                search degrades (no origin, hence no results) rather than failing the whole request. The
                client hides the address search when no server key is configured (see
                IsLocationSearchAvailable), so this mainly guards runtime and quota failures.

                Reason: A typed ZIP resolved to no origin, and a geocode failure must not break the search.
            */
            if ( GeographyPoint.TryParse( origin, out var point ) )
            {
                return (point, null);
            }

            try
            {
                var result = Task.Run( () => GeographyHelpers.GeocodeDetailed( origin ) ).Result;
                return (result?.Location, result?.Viewport);
            }
            catch ( Exception ex )
            {
                Logger.LogWarning( ex, "Group Finder could not geocode the origin '{Origin}'.", origin );
                return (null, null);
            }
        }

        /// <summary>
        /// Rebuilds the origin the client already resolved from the values echoed back in the query, so a repeated search of the same origin does not geocode it again.
        /// </summary>
        /// <param name="query">The visitor's query, which carries the point and viewport resolved for the origin on a prior request.</param>
        /// <returns>The resolved point and viewport when the query carries them for the current origin; <c>null</c> when it does not, signaling the caller to geocode.</returns>
        /// <seealso cref="ResolveOrigin(string)"/>
        private (GeographyPoint Point, GeographyBounds Viewport)? GetClientResolvedOrigin( GroupFinderQueryBag query )
        {
            // The echoed values are trusted only when they were resolved for this exact origin; an edited
            // origin must geocode afresh rather than reuse a stale point.
            if ( query.ResolvedOriginKey != query.Origin
                || !query.ResolvedOriginLatitude.HasValue
                || !query.ResolvedOriginLongitude.HasValue )
            {
                return null;
            }

            var point = new GeographyPoint( query.ResolvedOriginLatitude.Value, query.ResolvedOriginLongitude.Value );

            // The viewport is optional: an origin that resolved to a bare point (a coordinate or a precise
            // address) carries none, which floors the search area to the default radius exactly as the
            // original geocode did.
            var hasViewport = query.ResolvedViewportNorth.HasValue
                && query.ResolvedViewportSouth.HasValue
                && query.ResolvedViewportEast.HasValue
                && query.ResolvedViewportWest.HasValue;
            var viewport = hasViewport
                ? new GeographyBounds( query.ResolvedViewportNorth.Value, query.ResolvedViewportSouth.Value, query.ResolvedViewportEast.Value, query.ResolvedViewportWest.Value )
                : null;

            return (point, viewport);
        }

        /// <summary>
        /// Builds a square latitude/longitude bounding box of the given radius around a center point.
        /// </summary>
        /// <param name="center">The center of the box.</param>
        /// <param name="radiusMiles">The radius, in miles, from the center to each edge.</param>
        /// <returns>The bounding box.</returns>
        private static GeographyBounds BuildRadiusBounds( GeographyPoint center, double radiusMiles )
        {
            var radiusMeters = radiusMiles * MetersPerMile;
            var latitudeDelta = radiusMeters / MetersPerDegreeLatitude;
            var metersPerDegreeLongitude = MetersPerDegreeLatitude * Math.Cos( center.Latitude * Math.PI / 180 );
            var longitudeDelta = metersPerDegreeLongitude > 0 ? radiusMeters / metersPerDegreeLongitude : latitudeDelta;

            return new GeographyBounds(
                center.Latitude + latitudeDelta,
                center.Latitude - latitudeDelta,
                center.Longitude + longitudeDelta,
                center.Longitude - longitudeDelta );
        }

        /// <summary>
        /// Combines two bounding boxes into the smallest box that contains both.
        /// </summary>
        /// <param name="first">The first box.</param>
        /// <param name="second">The second box.</param>
        /// <returns>The enclosing box.</returns>
        private static GeographyBounds UnionBounds( GeographyBounds first, GeographyBounds second )
        {
            return new GeographyBounds(
                Math.Max( first.North, second.North ),
                Math.Min( first.South, second.South ),
                Math.Max( first.East, second.East ),
                Math.Min( first.West, second.West ) );
        }

        /// <summary>
        /// Gets the driving distance (miles) and static drive time (minutes) from the origin to each group's fuzzed marker, for the groups the routing provider can route.
        /// </summary>
        /// <remarks>
        /// Groups the provider cannot route (and every group on a total failure) are absent from the result; the caller supplies straight-line distance for those. Requesting distance and static duration keeps the call in the Essentials billing tier (it is not traffic-aware).
        /// </remarks>
        /// <param name="origin">The person's location the distance is measured from.</param>
        /// <param name="groupIds">The result group ids.</param>
        /// <param name="fuzzedByGroup">The fuzzed marker coordinates keyed by group id.</param>
        /// <returns>The driving distance in miles and drive time in minutes keyed by group id, for the routed groups only.</returns>
        private Dictionary<int, (double Miles, double Minutes)> GetDriveMatrixByGroup( GeographyPoint origin, List<int> groupIds, Dictionary<int, (double Latitude, double Longitude)> fuzzedByGroup )
        {
            var results = new Dictionary<int, (double Miles, double Minutes)>();
            var orderedIds = groupIds.Where( id => fuzzedByGroup.ContainsKey( id ) ).ToList();
            if ( !orderedIds.Any() )
            {
                return results;
            }

            var destinations = orderedIds
                .Select( id => new GeographyPoint( fuzzedByGroup[id].Latitude, fuzzedByGroup[id].Longitude ) )
                .ToList();

            /*
                7/22/26 - JMH

                Drive distance and time are measured to the FUZZED marker, not the true location: the
                numbers are estimates either way, and routing to the true point could help reveal it.

                Reason: Accurate-enough drive distance/time without exposing a group's true location.
            */
            try
            {
                // Distance plus static duration stays in the Essentials billing tier (no traffic-aware routing).
                var matrix = Task.Run( () => GeographyHelpers.GetDrivingMatrixAsync( origin, destinations, TravelMode.Drive, RouteMatrixDetail.DistanceAndDuration ) ).Result;
                foreach ( var element in matrix )
                {
                    // A pair Google could not route comes back with no distance (0); leave it absent so the
                    // caller's straight-line value stands for that group instead of a bogus 0 miles.
                    if ( element.DistanceInMeters <= 0 )
                    {
                        continue;
                    }

                    var index = destinations.FindIndex( d => d == element.DestinationPoint );
                    if ( index >= 0 )
                    {
                        results[orderedIds[index]] = ( element.DistanceInMeters / MetersPerMile, element.TravelTimeInMinutes );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logger.LogWarning( ex, "Group Finder could not compute drive distances; straight-line distance stands in." );
            }

            return results;
        }

        /// <summary>
        /// Gets a stable key identifying the distance origin, or null when there is no origin.
        /// </summary>
        /// <remarks>
        /// The coordinates are rounded so that float noise between requests resolves to the same key, letting the client reuse its cached distances until the visitor materially changes the origin.
        /// </remarks>
        /// <param name="origin">The resolved proximity origin, or null.</param>
        /// <returns>The rounded "latitude,longitude" key, or null.</returns>
        private static string GetOriginKey( GeographyPoint origin )
        {
            if ( origin == null )
            {
                return null;
            }

            var latitude = Math.Round( origin.Latitude, 4 );
            var longitude = Math.Round( origin.Longitude, 4 );
            return $"{latitude.ToString( System.Globalization.CultureInfo.InvariantCulture )},{longitude.ToString( System.Globalization.CultureInfo.InvariantCulture )}";
        }

        /// <summary>
        /// Formats a drive time in minutes as a compact label, e.g. "45 min" or "1 hr 20 min".
        /// </summary>
        /// <param name="minutes">The drive time in minutes.</param>
        /// <returns>The compact drive-time label.</returns>
        private static string FormatDriveTime( double minutes )
        {
            /*
                08/03/26 - JMH

                Humanizer's TimeSpan.Humanize emits the full-word, culture-aware form
                ("1 hour, 20 minutes") and offers no compact-abbreviation option, so the
                short "1 hr 20 min" label is built here rather than through the library.

                Reason: Humanizer cannot produce the compact "hr/min" drive-time label.
            */
            var totalMinutes = ( int ) Math.Round( minutes );
            var hours = totalMinutes / 60;
            var remainingMinutes = totalMinutes % 60;

            if ( hours > 0 && remainingMinutes > 0 )
            {
                return $"{hours} hr {remainingMinutes} min";
            }

            return hours > 0 ? $"{hours} hr" : $"{remainingMinutes} min";
        }

        /// <summary>
        /// Gets the great-circle (straight-line) distance in miles between the origin and a point.
        /// </summary>
        /// <param name="origin">The point the distance is measured from.</param>
        /// <param name="point">The destination coordinates.</param>
        /// <returns>The distance in miles.</returns>
        private static double HaversineMiles( GeographyPoint origin, (double Latitude, double Longitude) point )
        {
            const double earthRadiusMiles = 3958.7613;
            var originLatitudeRadians = origin.Latitude * Math.PI / 180;
            var pointLatitudeRadians = point.Latitude * Math.PI / 180;
            var deltaLatitude = ( point.Latitude - origin.Latitude ) * Math.PI / 180;
            var deltaLongitude = ( point.Longitude - origin.Longitude ) * Math.PI / 180;

            var h = Math.Sin( deltaLatitude / 2 ) * Math.Sin( deltaLatitude / 2 )
                + Math.Cos( originLatitudeRadians ) * Math.Cos( pointLatitudeRadians )
                * Math.Sin( deltaLongitude / 2 ) * Math.Sin( deltaLongitude / 2 );

            return earthRadiusMiles * 2 * Math.Asin( Math.Min( 1, Math.Sqrt( h ) ) );
        }

        /// <summary>
        /// Builds the engine filter list from the visitor's query. Day of week, time of day, and the
        /// name search are applied directly on the query in <see cref="GetResults"/> rather than here.
        /// </summary>
        /// <param name="query">The visitor's filter selections.</param>
        /// <param name="featuredAttributes">The configured Featured (pill) attributes, resolved once by the caller.</param>
        /// <returns>The list of engine filters.</returns>
        private List<GroupFinderFilter> BuildFilters( GroupFinderQueryBag query, List<AttributeCache> featuredAttributes )
        {
            var filters = new List<GroupFinderFilter>();

            // Campus: the engine expects a comma separated list of campus ids.
            var campusIds = ( query.CampusGuids ?? new List<string>() )
                .Select( g => CampusCache.Get( g.AsGuid() )?.Id )
                .Where( id => id.HasValue )
                .Select( id => id.Value.ToString() )
                .ToList();
            if ( campusIds.Any() )
            {
                filters.Add( new GroupFinderFilter( "campus", null, null, campusIds.AsDelimited( "," ) ) );
            }

            // Meeting style.
            if ( query.MeetingStyles?.Any() == true )
            {
                filters.Add( new GroupFinderFilter( "meetingstyle", null, null, query.MeetingStyles.AsDelimited( "," ) ) );
            }

            // Featured (pill) attribute selections match their value list via the engine's 'in' operator,
            // which is the correct semantic for the Single-select, Multi-select, and Boolean types pills are
            // restricted to. Display (More Filters) selections are applied separately through each field
            // type's own filter expression (see ApplyDisplayAttributeFilters), so they are skipped here.
            if ( query.AttributeSelections != null )
            {
                var featuredKeys = new HashSet<string>( featuredAttributes.Select( a => a.Key ) );

                foreach ( var selection in query.AttributeSelections )
                {
                    if ( !featuredKeys.Contains( selection.Key ) )
                    {
                        continue;
                    }

                    var values = ( selection.Value ?? new List<string>() ).Where( v => v.IsNotNullOrWhiteSpace() ).ToList();
                    if ( values.Any() )
                    {
                        filters.Add( new GroupFinderFilter( "attribute", selection.Key, "in", values.AsDelimited( "," ) ) );
                    }
                }
            }

            return filters;
        }

        /// <summary>
        /// Applies the More Filters (Display Attribute Filters) selections to the group query by delegating to each attribute's own field type.
        /// </summary>
        /// <remarks>
        /// Each attribute's predicate is built by its field type (honoring the visitor's chosen comparison),
        /// so every filterable field type is supported. Predicates are grouped by the group type they belong
        /// to (AND within a group type, OR across group types), so an attribute present on only one group
        /// type does not exclude the groups of other types.
        /// </remarks>
        /// <param name="groupQuery">The group location query to filter.</param>
        /// <param name="query">The visitor's filter selections.</param>
        /// <returns>The query filtered to groups matching the selected Display attribute filters.</returns>
        private IQueryable<GroupLocation> ApplyDisplayAttributeFilters( IQueryable<GroupLocation> groupQuery, GroupFinderQueryBag query )
        {
            if ( query.AttributeFilterValues == null || !query.AttributeFilterValues.Any() )
            {
                return groupQuery;
            }

            var displayAttributesByKey = GetConfiguredAttributes( AttributeKey.DisplayAttributeFilters )
                .ToDictionary( attribute => attribute.Key, attribute => attribute );
            if ( !displayAttributesByKey.Any() )
            {
                return groupQuery;
            }

            var groupService = new GroupService( RockContext );
            var parameterExpression = groupService.ParameterExpression;
            var predicatesByQualifier = new Dictionary<string, Expression>();

            foreach ( var filterValue in query.AttributeFilterValues )
            {
                if ( !displayAttributesByKey.TryGetValue( filterValue.Key, out var attribute ) )
                {
                    continue;
                }

                // Leave the comparison unset when the client did not send one, rather than forcing EqualTo.
                // Checkbox-style field types (Defined Value, Multi-Select) carry no comparison and expect
                // their natural "contains / any-of" filter, which ORs the selected values. Forcing EqualTo
                // turned a multi-value selection into an exact match of the joined values, so selecting two
                // Topics matched no group (an unintended AND) while one still matched.
                ComparisonType? comparisonType = null;
                if ( query.AttributeComparisons != null && query.AttributeComparisons.TryGetValue( filterValue.Key, out var postedComparison ) )
                {
                    comparisonType = postedComparison;
                }

                // The client sends the field type's own public filter value. The field type converts it to
                // its private filter representation, a JSON [comparisonType, value] pair, which is exactly
                // what the expression builder consumes. Field-type-specific encodings (such as a Defined
                // Value's JSON) are handled by the field type rather than parsed here.
                var publicComparisonValue = new ComparisonValue { ComparisonType = comparisonType, Value = filterValue.Value };
                var filterValues = attribute.FieldType.Field.GetPrivateFilterValue( publicComparisonValue, attribute.ConfigurationValues )
                    .FromJsonOrNull<List<string>>();

                if ( filterValues == null || !filterValues.Any() )
                {
                    continue;
                }

                var entityField = EntityHelper.GetEntityFieldForAttribute( attribute );
                var expression = ExpressionHelper.GetAttributeExpression( groupService, parameterExpression, entityField, filterValues );

                if ( expression == null || expression is NoAttributeFilterExpression )
                {
                    continue;
                }

                var qualifierKey = $"{attribute.EntityTypeQualifierColumn}_{attribute.EntityTypeQualifierValue}";
                predicatesByQualifier[qualifierKey] = predicatesByQualifier.TryGetValue( qualifierKey, out var existing )
                    ? Expression.And( existing, expression )
                    : expression;
            }

            if ( !predicatesByQualifier.Any() )
            {
                return groupQuery;
            }

            Expression combined = null;
            foreach ( var predicate in predicatesByQualifier.Values )
            {
                combined = combined == null ? predicate : Expression.Or( combined, predicate );
            }

            // Pre-filter the groups upstream and constrain the location query to the survivors. Using the
            // unexecuted group-id query as the basis for Contains lets EF generate a subquery rather than a
            // large IN list, and leaves the shared GroupFinderHelper untouched.
            var matchingGroupIds = groupService.Queryable().Where( parameterExpression, combined ).Select( g => g.Id );

            return groupQuery.Where( gl => matchingGroupIds.Contains( gl.GroupId ) );
        }

        /// <summary>
        /// Requires a group to actually have a value set for each selected Featured (pill) attribute.
        /// </summary>
        /// <remarks>
        /// The engine's "in" filter (see <see cref="BuildFilters"/>) intentionally includes groups that have
        /// no value for the attribute, so that an attribute defined on only one of several group types does
        /// not exclude the others. That makes a Featured pill "looser" than a More Filters selection, which
        /// excludes groups without a value. Adding this presence requirement aligns the two: a Featured pill
        /// now returns only groups that have the attribute set to a matching value.
        /// </remarks>
        /// <param name="groupQuery">The group location query to filter.</param>
        /// <param name="query">The visitor's filter selections.</param>
        /// <param name="featuredAttributes">The configured Featured (pill) attributes, resolved once by the caller.</param>
        /// <returns>The query constrained to groups that have each selected Featured attribute set.</returns>
        private IQueryable<GroupLocation> ApplyFeaturedAttributePresence( IQueryable<GroupLocation> groupQuery, GroupFinderQueryBag query, List<AttributeCache> featuredAttributes )
        {
            if ( query.AttributeSelections == null || !query.AttributeSelections.Any() )
            {
                return groupQuery;
            }

            var featuredKeys = new HashSet<string>( featuredAttributes.Select( a => a.Key ) );

            foreach ( var selection in query.AttributeSelections )
            {
                if ( !featuredKeys.Contains( selection.Key ) )
                {
                    continue;
                }

                var values = ( selection.Value ?? new List<string>() ).Where( v => v.IsNotNullOrWhiteSpace() ).ToList();
                if ( !values.Any() )
                {
                    continue;
                }

                var key = selection.Key;
                groupQuery = groupQuery.Where( gl => gl.Group.GroupAttributeValues.Any( av => av.Key == key ) );
            }

            return groupQuery;
        }

        /// <summary>
        /// Projects a group into a result card.
        /// </summary>
        /// <param name="group">The group to project.</param>
        /// <param name="commonMergeFields">The shared merge fields (current person, page parameters, globals) resolved once for the whole page; copied so each card can layer its own fields without mutating the shared set.</param>
        /// <param name="compiledCardTemplate">The card Lava template parsed once for the whole page and rendered per card.</param>
        /// <param name="enabledLavaCommands">The Lava commands enabled while rendering the card template.</param>
        /// <param name="isCampusFiltered">Whether the visitor filtered to a single campus (which is then omitted from the card).</param>
        /// <param name="averageAge">The rounded average age of the group's members, or null when not shown.</param>
        /// <param name="straightLineDistance">The straight-line distance from the origin in miles, or null when proximity is not in use.</param>
        /// <param name="drivingDistance">The driving distance from the origin in miles, or null when it is not available (no origin, or the group could not be routed).</param>
        /// <param name="drivingMinutes">The driving time from the origin in minutes, or null when it is not available (no origin, or the group could not be routed).</param>
        /// <returns>The card bag for the group.</returns>
        private GroupFinderCardBag ToCardBag( Rock.Model.Group group, IDictionary<string, object> commonMergeFields, ILavaTemplate compiledCardTemplate, string[] enabledLavaCommands, bool isCampusFiltered, int? averageAge, double? straightLineDistance, double? drivingDistance, double? drivingMinutes )
        {
            var groupType = GroupTypeCache.Get( group.GroupTypeId );
            var campusName = isCampusFiltered ? null : ( group.CampusId.HasValue ? CampusCache.Get( group.CampusId.Value )?.Name : null );
            var scheduleText = group.Schedule?.FriendlyScheduleText;

            var registerUrl = this.GetLinkedPageUrl( AttributeKey.RegisterPage, new Dictionary<string, string> { { "GroupGuid", group.Guid.ToString() } } );

            var mergeFields = new Dictionary<string, object>( commonMergeFields );
            mergeFields["Group"] = group;
            mergeFields["GroupTypeName"] = groupType?.Name;
            mergeFields["GroupTypeColor"] = groupType?.GroupTypeColor;
            mergeFields["ImageUrl"] = group.PhotoId.HasValue ? group.PhotoUrl : null;
            mergeFields["ScheduleText"] = scheduleText;
            mergeFields["CampusName"] = campusName;
            mergeFields["AverageAge"] = averageAge;
            mergeFields["DrivingDistance"] = drivingDistance;
            mergeFields["DrivingMinutes"] = drivingMinutes;
            mergeFields["DriveTime"] = drivingMinutes.HasValue ? FormatDriveTime( drivingMinutes.Value ) : null;
            mergeFields["StraightLineDistance"] = straightLineDistance;
            mergeFields["RegisterUrl"] = registerUrl;
            mergeFields["Attributes"] = GetCardAttributes( group );

            var renderContext = LavaService.NewRenderContext( mergeFields, enabledLavaCommands );
            var renderResult = LavaService.RenderTemplate( compiledCardTemplate, renderContext );

            return new GroupFinderCardBag
            {
                GroupGuid = group.Guid.ToString(),
                ContentHtml = renderResult.Text
            };
        }

        /// <summary>
        /// Builds a compact signature of a group's fuzzed point for the driving-distance cache. A cached
        /// distance is reused only while this key still matches, so a group whose location changed since
        /// the value was computed is re-routed rather than showing a stale distance and time.
        /// </summary>
        /// <param name="point">The fuzzed point the distance was, or would be, computed from.</param>
        /// <returns>An invariant "latitude,longitude" key rounded to roughly a meter.</returns>
        private static string GetDistanceLocationKey( (double Latitude, double Longitude) point )
        {
            return FormattableString.Invariant( $"{point.Latitude:F5},{point.Longitude:F5}" );
        }

        /// <summary>
        /// Gets the "Show Attribute on Card" attribute values for a group, each with its icon.
        /// </summary>
        /// <param name="group">The group whose attributes are read.</param>
        /// <returns>The card attributes, or an empty list when none are configured.</returns>
        private List<GroupFinderCardAttributeInfo> GetCardAttributes( Rock.Model.Group group )
        {
            var attributeGuids = GetAttributeValue( AttributeKey.ShowAttributeOnCard ).SplitDelimitedValues().AsGuidList();
            if ( !attributeGuids.Any() )
            {
                return new List<GroupFinderCardAttributeInfo>();
            }

            if ( group.Attributes == null )
            {
                group.LoadAttributes( RockContext );
            }

            var cardAttributes = new List<GroupFinderCardAttributeInfo>();
            foreach ( var attributeGuid in attributeGuids )
            {
                var attribute = AttributeCache.Get( attributeGuid );
                if ( attribute == null || !group.Attributes.ContainsKey( attribute.Key ) )
                {
                    continue;
                }

                // Skip attributes the group has no value for, so an unset attribute leaves out its whole
                // row (icon, label, and value) rather than showing a label and icon with nothing beside them.
                // Tested on the raw value so a set-but-false Boolean still shows ("No"); only a truly empty
                // value is dropped.
                if ( group.GetAttributeValue( attribute.Key ).IsNullOrWhiteSpace() )
                {
                    continue;
                }

                cardAttributes.Add( new GroupFinderCardAttributeInfo
                {
                    Label = attribute.Name,
                    // The formatted value, so a Defined Value shows its name and a Boolean shows Yes/No,
                    // rather than the raw stored value (a guid or "True"/"False").
                    Value = group.GetAttributeTextValue( attribute.Key ),
                    IconCssClass = attribute.IconCssClass
                } );
            }

            return cardAttributes;
        }

        /// <summary>
        /// A single card attribute exposed to the group card Lava template: its label, formatted value, and icon.
        /// </summary>
        /// <remarks>
        /// A <see cref="LavaDataObject"/> so the template reaches its members directly (Attributes are
        /// iterated as <c>attribute.Label</c>, <c>attribute.Value</c>, <c>attribute.IconCssClass</c>). It is
        /// server-only and never serialized to the client, which receives only the rendered card HTML.
        /// </remarks>
        private class GroupFinderCardAttributeInfo : LavaDataObject
        {
            /// <summary>
            /// Gets or sets the attribute's display label.
            /// </summary>
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the attribute's formatted value.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the attribute's icon CSS class.
            /// </summary>
            public string IconCssClass { get; set; }
        }

        /// <summary>
        /// Gets the rounded average age of the active members for each of the specified groups, in a single query.
        /// </summary>
        /// <param name="groupIds">The group ids to compute average ages for.</param>
        /// <returns>A map of group id to rounded average age; groups with no member birth dates are absent.</returns>
        private Dictionary<int, int> GetAverageAgesByGroup( List<int> groupIds )
        {
            var today = RockDateTime.Today;

            var memberBirthDates = new GroupMemberService( RockContext )
                .Queryable()
                .Where( m => groupIds.Contains( m.GroupId )
                    && m.GroupMemberStatus == GroupMemberStatus.Active
                    && m.Person.BirthDate.HasValue )
                .Select( m => new { m.GroupId, m.Person.BirthDate } )
                .ToList();

            return memberBirthDates
                .GroupBy( m => m.GroupId )
                .ToDictionary(
                    g => g.Key,
                    g => ( int ) Math.Round( g.Average( m => ( today - m.BirthDate.Value ).TotalDays / 365.25 ) ) );
        }

        #endregion

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "ti ti-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Group/groupFinderCustomSettings.obs"
                } );
            }

            return actions;
        }

        #endregion IHasCustomActions

        #region Custom Settings

        /// <summary>
        /// Gets the current custom settings values and the options needed to render the custom settings panel.
        /// </summary>
        /// <returns>The custom settings box, or a forbidden result when the caller cannot administrate the block.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            var groupTypeGuids = GetAttributeValue( AttributeKey.GroupTypes ).SplitDelimitedValues().AsGuidList();

            var settings = new GroupFinderCustomSettingsBag
            {
                // Group Types and Attributes.
                GroupTypes = groupTypeGuids
                    .Select( g => GroupTypeCache.Get( g ) )
                    .Where( gt => gt != null )
                    .Select( gt => new ListItemBag { Value = gt.Guid.ToString(), Text = gt.Name } )
                    .ToList(),
                FeaturedAttributes = GetAttributeValue( AttributeKey.FeaturedAttributes ).SplitDelimitedValues().AsGuidList(),
                DisplayAttributeFilters = GetAttributeValue( AttributeKey.DisplayAttributeFilters ).SplitDelimitedValues().AsGuidList(),
                ShowAttributeOnCard = GetAttributeValue( AttributeKey.ShowAttributeOnCard ).SplitDelimitedValues().AsGuidList(),

                // Filters.
                IsCampusFilterHidden = GetAttributeValue( AttributeKey.HideCampusFilters ).AsBoolean(),
                CampusTypes = GetAttributeValue( AttributeKey.CampusTypes ).DefinedValueGuidsToListItemBagList(),
                CampusStatuses = GetAttributeValue( AttributeKey.CampusStatuses ).DefinedValueGuidsToListItemBagList(),
                DistanceCalculation = GetAttributeValue( AttributeKey.DistanceCalculation ),
                SupportedMeetingStyles = GetSupportedMeetingStyles(),
                IsDayOfWeekFilterShown = GetAttributeValue( AttributeKey.DisplayDayOfWeekFilter ).AsBoolean(),
                IsTimeOfDayFilterShown = GetAttributeValue( AttributeKey.DisplayTimeOfDayFilter ).AsBoolean(),
                IsLiveSearchEnabled = GetAttributeValue( AttributeKey.EnableLiveSearch ).AsBoolean(),

                // Card and Map.
                IsImageShown = GetAttributeValue( AttributeKey.ShowImage ).AsBoolean(),
                IsAverageAgeShown = GetAttributeValue( AttributeKey.ShowAverageAge ).AsBoolean(),
                IsMapShown = GetAttributeValue( AttributeKey.ShowMap ).AsBoolean(),
                GroupMarkerColor = GetAttributeValue( AttributeKey.GroupMarkerColor ),
                CurrentLocationMarkerColor = GetAttributeValue( AttributeKey.CurrentLocationMarkerColor ),
                MapStyle = GetAttributeValue( AttributeKey.MapStyle ).DefinedValueGuidsToListItemBagList().FirstOrDefault(),
                GroupCardTemplate = GetAttributeValue( AttributeKey.GroupCardTemplate ),

                // Linked Pages.
                RegisterPage = GetAttributeValue( AttributeKey.RegisterPage ).ToPageRouteValueBag()
            };

            var options = GetCustomSettingsAttributeOptions( groupTypeGuids.Select( g => g.ToString() ) );

            return ActionOk( new CustomSettingsBox<GroupFinderCustomSettingsBag, GroupFinderCustomSettingsOptionsBag>
            {
                Settings = settings,
                Options = options,
                SecurityGrantToken = new SecurityGrant().ToToken()
            } );
        }

        /// <summary>
        /// Gets the attribute options rescoped to the supplied group types, so the panel can refresh as the admin changes the Group Types selection.
        /// </summary>
        /// <param name="selectedGroupTypeGuids">The group type guids currently selected in the panel.</param>
        /// <returns>The rescoped attribute options, or a forbidden result when the caller cannot administrate the block.</returns>
        [BlockAction]
        public BlockActionResult GetUpdatedAttributeOptions( List<string> selectedGroupTypeGuids )
        {
            if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to edit block settings." );
            }

            return ActionOk( GetCustomSettingsAttributeOptions( selectedGroupTypeGuids ?? new List<string>() ) );
        }

        /// <summary>
        /// Saves the custom settings values edited in the custom settings panel.
        /// </summary>
        /// <param name="box">The box carrying the edited settings.</param>
        /// <returns>An empty ok result, or a forbidden result when the caller cannot administrate the block.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<GroupFinderCustomSettingsBag, GroupFinderCustomSettingsOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var block = new BlockService( rockContext ).Get( BlockId );
                block.LoadAttributes( rockContext );

                box.IfValidProperty( nameof( box.Settings.GroupTypes ),
                    () => block.SetAttributeValue( AttributeKey.GroupTypes, ( box.Settings.GroupTypes ?? new List<ListItemBag>() ).Select( i => i.Value ).ToList().AsDelimited( "," ) ) );

                box.IfValidProperty( nameof( box.Settings.FeaturedAttributes ),
                    () => block.SetAttributeValue( AttributeKey.FeaturedAttributes, ( box.Settings.FeaturedAttributes ?? new List<Guid>() ).Select( g => g.ToString() ).ToList().AsDelimited( "," ) ) );

                box.IfValidProperty( nameof( box.Settings.DisplayAttributeFilters ),
                    () => block.SetAttributeValue( AttributeKey.DisplayAttributeFilters, ( box.Settings.DisplayAttributeFilters ?? new List<Guid>() ).Select( g => g.ToString() ).ToList().AsDelimited( "," ) ) );

                box.IfValidProperty( nameof( box.Settings.ShowAttributeOnCard ),
                    () => block.SetAttributeValue( AttributeKey.ShowAttributeOnCard, ( box.Settings.ShowAttributeOnCard ?? new List<Guid>() ).Select( g => g.ToString() ).ToList().AsDelimited( "," ) ) );

                box.IfValidProperty( nameof( box.Settings.IsCampusFilterHidden ),
                    () => block.SetAttributeValue( AttributeKey.HideCampusFilters, box.Settings.IsCampusFilterHidden.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.CampusTypes ),
                    () => block.SetAttributeValue( AttributeKey.CampusTypes, box.Settings.CampusTypes.ToCommaDelimitedValuesString() ) );

                box.IfValidProperty( nameof( box.Settings.CampusStatuses ),
                    () => block.SetAttributeValue( AttributeKey.CampusStatuses, box.Settings.CampusStatuses.ToCommaDelimitedValuesString() ) );

                box.IfValidProperty( nameof( box.Settings.DistanceCalculation ),
                    () => block.SetAttributeValue( AttributeKey.DistanceCalculation, box.Settings.DistanceCalculation ) );

                box.IfValidProperty( nameof( box.Settings.SupportedMeetingStyles ), () =>
                {
                    // Persist a sentinel for "none selected" so the cleared state survives; a blank value would
                    // be restored to the attribute's default on the next load. See GetSupportedMeetingStyles.
                    var selectedStyles = box.Settings.SupportedMeetingStyles ?? new List<string>();
                    var storedValue = selectedStyles.Any() ? selectedStyles.AsDelimited( "," ) : MeetingStylesClearedSentinel;
                    block.SetAttributeValue( AttributeKey.SupportedMeetingStyles, storedValue );
                } );

                box.IfValidProperty( nameof( box.Settings.IsDayOfWeekFilterShown ),
                    () => block.SetAttributeValue( AttributeKey.DisplayDayOfWeekFilter, box.Settings.IsDayOfWeekFilterShown.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.IsTimeOfDayFilterShown ),
                    () => block.SetAttributeValue( AttributeKey.DisplayTimeOfDayFilter, box.Settings.IsTimeOfDayFilterShown.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.IsLiveSearchEnabled ),
                    () => block.SetAttributeValue( AttributeKey.EnableLiveSearch, box.Settings.IsLiveSearchEnabled.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.IsImageShown ),
                    () => block.SetAttributeValue( AttributeKey.ShowImage, box.Settings.IsImageShown.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.IsAverageAgeShown ),
                    () => block.SetAttributeValue( AttributeKey.ShowAverageAge, box.Settings.IsAverageAgeShown.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.IsMapShown ),
                    () => block.SetAttributeValue( AttributeKey.ShowMap, box.Settings.IsMapShown.ToString() ) );

                box.IfValidProperty( nameof( box.Settings.GroupMarkerColor ),
                    () => block.SetAttributeValue( AttributeKey.GroupMarkerColor, box.Settings.GroupMarkerColor ) );

                box.IfValidProperty( nameof( box.Settings.CurrentLocationMarkerColor ),
                    () => block.SetAttributeValue( AttributeKey.CurrentLocationMarkerColor, box.Settings.CurrentLocationMarkerColor ) );

                box.IfValidProperty( nameof( box.Settings.MapStyle ),
                    () => block.SetAttributeValue( AttributeKey.MapStyle, box.Settings.MapStyle?.Value ) );

                box.IfValidProperty( nameof( box.Settings.GroupCardTemplate ),
                    () => block.SetAttributeValue( AttributeKey.GroupCardTemplate, box.Settings.GroupCardTemplate ) );

                box.IfValidProperty( nameof( box.Settings.RegisterPage ),
                    () => block.SetAttributeValue( AttributeKey.RegisterPage, box.Settings.RegisterPage.ToCommaDelimitedPageRouteValues() ) );

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        /// <summary>
        /// Builds the attribute options for the custom settings panel, scoped to the supplied group types.
        /// </summary>
        /// <param name="groupTypeGuidStrings">The selected group type guids.</param>
        /// <returns>The available Featured (filterable and pill-eligible), Display (filterable), and Card (all) attribute options.</returns>
        private GroupFinderCustomSettingsOptionsBag GetCustomSettingsAttributeOptions( IEnumerable<string> groupTypeGuidStrings )
        {
            var featured = new List<ListItemBag>();
            var display = new List<ListItemBag>();
            var card = new List<ListItemBag>();

            foreach ( var (attribute, label) in GetGroupTypeAttributes( groupTypeGuidStrings ) )
            {
                var listItem = new ListItemBag { Value = attribute.Guid.ToString(), Text = label };

                card.Add( listItem );

                if ( attribute.FieldType.Field.HasFilterControl() )
                {
                    display.Add( listItem );

                    if ( IsPillEligible( attribute ) )
                    {
                        featured.Add( listItem );
                    }
                }
            }

            // The higher distance-calculation modes depend on Google services that a key can be present for
            // yet not licensed to use, which only a live call reveals. The panel probes them each time it
            // opens so an administrator sees a mode disabled the moment its service stops answering.
            var distanceServices = ProbeDistanceServices();

            return new GroupFinderCustomSettingsOptionsBag
            {
                AvailableFeaturedAttributes = featured,
                AvailableDisplayAttributes = display,
                AvailableCardAttributes = card,
                IsAddressSearchAvailable = distanceServices.IsGeocodingAvailable,
                IsDrivingDistanceAvailable = distanceServices.IsGeocodingAvailable && distanceServices.IsRoutingAvailable
            };
        }

        /*
            Probe endpoints. Geocoding a stable place name and routing between two fixed points exercises each
            service without depending on any organization data. The points are a short, always-routable hop so
            a working Routes response carries a positive distance.
        */
        private const string DistanceProbeAddress = "Washington, DC";
        private static readonly GeographyPoint DistanceProbeOrigin = new GeographyPoint( 38.8977, -77.0365 );
        private static readonly GeographyPoint DistanceProbeDestination = new GeographyPoint( 38.8899, -77.0091 );

        /// <summary>
        /// Probes the Google geographic services the higher distance-calculation modes depend on with a live call to each.
        /// </summary>
        /// <remarks>
        /// A live probe is the only reliable signal, because a key can be present yet not licensed for a given
        /// API, which surfaces only when a real request is made. The two probes run together; each failure (a
        /// missing key throws, a misconfigured key or quota returns nothing) reads as that service unavailable.
        /// </remarks>
        /// <returns>Whether geocoding (which Address mode needs) and routing (which Driving mode needs) each answered.</returns>
        private (bool IsGeocodingAvailable, bool IsRoutingAvailable) ProbeDistanceServices()
        {
            var geocodeProbe = Task.Run( async () =>
            {
                try
                {
                    var result = await GeographyHelpers.GeocodeDetailed( DistanceProbeAddress );
                    return result?.Location != null;
                }
                catch
                {
                    // A missing key throws from the provider constructor; treat it as unavailable.
                    return false;
                }
            } );

            var routeProbe = Task.Run( async () =>
            {
                try
                {
                    var matrix = await GeographyHelpers.GetDrivingMatrixAsync(
                        DistanceProbeOrigin,
                        new List<GeographyPoint> { DistanceProbeDestination },
                        TravelMode.Drive,
                        RouteMatrixDetail.DistanceOnly );

                    return matrix != null && matrix.Any( r => r != null && r.DistanceInMeters > 0 );
                }
                catch
                {
                    // A missing key throws from the provider constructor, and a key without Routes access
                    // throws on the non-OK response; both mean the routing service is unavailable.
                    return false;
                }
            } );

            Task.WaitAll( geocodeProbe, routeProbe );

            return ( geocodeProbe.Result, routeProbe.Result );
        }

        /// <summary>
        /// Gets the distinct group attributes across the supplied group types, each labeled with the group type(s) it belongs to.
        /// </summary>
        /// <param name="groupTypeGuidStrings">The group type guids to load attributes from.</param>
        /// <returns>The distinct attributes with their display labels, ordered by attribute name.</returns>
        private List<(AttributeCache Attribute, string Label)> GetGroupTypeAttributes( IEnumerable<string> groupTypeGuidStrings )
        {
            var groupTypeNamesByAttributeGuid = new Dictionary<Guid, List<string>>();
            var attributesByGuid = new Dictionary<Guid, AttributeCache>();

            // One context is shared across every group type. LoadAttributes on a transient group (Id 0)
            // resolves definitions from AttributeCache and its inherited attributes from GroupTypeCache,
            // and skips the attribute-value query, so this reads the cache rather than the database.
            // Passing the context avoids LoadAttributes allocating a fresh one per group type.
            using ( var rockContext = new RockContext() )
            {
                foreach ( var guidString in groupTypeGuidStrings ?? Enumerable.Empty<string>() )
                {
                    var groupType = GroupTypeCache.Get( guidString.AsGuid() );
                    if ( groupType == null )
                    {
                        continue;
                    }

                    var group = new Rock.Model.Group { GroupTypeId = groupType.Id };
                    group.LoadAttributes( rockContext );

                    foreach ( var attribute in group.Attributes.Select( a => a.Value ) )
                    {
                        if ( !groupTypeNamesByAttributeGuid.TryGetValue( attribute.Guid, out var names ) )
                        {
                            names = new List<string>();
                            groupTypeNamesByAttributeGuid.Add( attribute.Guid, names );
                            attributesByGuid[attribute.Guid] = attribute;
                        }

                        names.Add( groupType.Name );
                    }
                }
            }

            return attributesByGuid.Values
                .Select( attribute => (
                    Attribute: attribute,
                    Label: $"{attribute.Name} ({string.Join( ", ", groupTypeNamesByAttributeGuid[attribute.Guid].OrderBy( name => name ) )})" ) )
                .OrderBy( item => item.Attribute.Name )
                .ToList();
        }

        /// <summary>
        /// Determines whether an attribute can be featured as a pill (Single-select, Multi-select, or Boolean).
        /// </summary>
        /// <param name="attribute">The attribute to test.</param>
        /// <returns><c>true</c> for pill-eligible field types; otherwise <c>false</c>.</returns>
        private static bool IsPillEligible( AttributeCache attribute )
        {
            var field = attribute.FieldType.Field;
            return field is SelectSingleFieldType
                || field is SelectMultiFieldType
                || field is BooleanFieldType;
        }

        #endregion Custom Settings

        #region Block Actions

        /// <summary>
        /// Returns the page of groups matching the supplied filter selections.
        /// </summary>
        /// <param name="query">The visitor's filter selections.</param>
        /// <returns>The matching groups for the page and the total count across all pages.</returns>
        [BlockAction]
        public BlockActionResult GetResults( GroupFinderQueryBag query )
        {
            return ActionOk( GetGroupResults( query ?? new GroupFinderQueryBag() ) );
        }

        /// <summary>
        /// Returns address autocomplete suggestions for the partial address the visitor has typed.
        /// </summary>
        /// <param name="text">The partial address, ZIP, city, or place typed so far.</param>
        /// <returns>The matching suggestion descriptions, or an empty list.</returns>
        [BlockAction]
        public BlockActionResult GetAddressSuggestions( string text )
        {
            // Gate behind a distance calculation that offers the address box, and a minimum length, so a
            // public page cannot drive the paid Places Autocomplete API with empty or one-character queries. The page rate limit and a Google
            // daily quota cap are the outer guards against abuse (see the spec's cost/abuse note).
            if ( GetDistanceCalculation() == DistanceCalculationMode.None
                || text.IsNullOrWhiteSpace()
                || text.Trim().Length < 3 )
            {
                return ActionOk( new List<string>() );
            }

            return ActionOk( Task.Run( () => GeographyHelpers.GetAddressSuggestionsAsync( text.Trim() ) ).Result );
        }

        /// <summary>
        /// Runs the group search for the supplied filter selections and projects the requested page.
        /// </summary>
        /// <param name="query">The filter selections and page number.</param>
        /// <returns>The matching groups for the page and the total count across all pages.</returns>
        private GroupFinderResultsBag GetGroupResults( GroupFinderQueryBag query )
        {
            var groupTypeIds = GetConfiguredGroupTypeIds();
            if ( !groupTypeIds.Any() )
            {
                return new GroupFinderResultsBag
                {
                    TotalCount = 0,
                    PageSize = ResultsPageSize,
                    Cards = new List<GroupFinderCardBag>(),
                    Markers = new List<GroupFinderMapMarkerBag>()
                };
            }

            var helper = new GroupFinderHelper( RockContext );
            var options = BuildOptions( query, groupTypeIds );
            var distanceCalculation = GetDistanceCalculation();
            var proximityEnabled = distanceCalculation != DistanceCalculationMode.None;

            /*
                Two independent locations drive the search:

                  - The SEARCH location selects which groups come back: the visitor's typed
                    address/ZIP/city, their current-location coordinates, an explicit "Search this area"
                    map box, or nothing on the initial browse.
                  - The PROXIMITY location is the person, and drives only distance and sort: the device
                    coordinates the visitor shared, else the server's best guess.

                Both are gated by Enable Proximity Features and coincide only when the visitor searches
                by current location.
            */

            // Resolve the search area (which groups). An explicit map box wins and applies regardless of the
            // distance mode, since "Search this area" filters by the map viewport and never needs the
            // visitor's location; a typed origin or current location (both proximity features) geocodes to its
            // own area; the initial browse has none.
            var hasClientBounds = query.MapBoundsNorth.HasValue && query.MapBoundsSouth.HasValue
                && query.MapBoundsEast.HasValue && query.MapBoundsWest.HasValue;
            GeographyBounds searchBounds = null;

            // When the area comes from a point (current location, a coordinate, or a geocoded place)
            // rather than an explicit map box, the results are also clamped to a true circle of this
            // radius around this center, so a corner of the bounding box is not counted as "nearby".
            GeographyPoint searchRadiusCenter = null;
            double searchRadiusMiles = 0;

            // The origin's raw geocoded viewport, returned to the client so it can echo it back and avoid
            // geocoding the same origin again (see GetClientResolvedOrigin). Null for a non-origin search.
            GeographyBounds resolvedOriginViewport = null;

            if ( hasClientBounds )
            {
                searchBounds = new GeographyBounds( query.MapBoundsNorth.Value, query.MapBoundsSouth.Value, query.MapBoundsEast.Value, query.MapBoundsWest.Value );
            }
            else if ( proximityEnabled && query.Origin.IsNotNullOrWhiteSpace() )
            {
                // Reuse the origin the client already resolved for this exact search instead of geocoding it
                // again. The client echoes back the point and viewport it received for a typed origin while
                // paging, refiltering, or resorting that same origin; a changed origin does not match and
                // geocodes afresh.
                var resolved = GetClientResolvedOrigin( query ) ?? ResolveOrigin( query.Origin );
                resolvedOriginViewport = resolved.Viewport;

                // A typed origin that could not be geocoded returns no results rather than the whole set
                // - the visitor asked for a specific place (see ResolveOrigin).
                if ( resolved.Point == null )
                {
                    return new GroupFinderResultsBag
                    {
                        TotalCount = 0,
                        PageSize = ResultsPageSize,
                        Cards = new List<GroupFinderCardBag>(),
                        Markers = new List<GroupFinderMapMarkerBag>()
                    };
                }

                // Search a useful area around the geocoded point. Google's viewport is sized to the
                // match, which is fine for a city or ZIP but far too small for a precise street address
                // (a few hundred meters), where no group's fuzzed marker would fall inside. Floor it to
                // a default-radius box so an address search still returns nearby groups, while a larger
                // place keeps its bigger viewport.
                var defaultArea = BuildRadiusBounds( resolved.Point, DefaultSearchRadiusMiles );
                searchBounds = resolved.Viewport != null
                    ? UnionBounds( resolved.Viewport, defaultArea )
                    : defaultArea;

                // Clamp to a real circle around the searched point. A precise point (current location,
                // coordinate, street address) clamps to the default radius; a larger geocoded place
                // (city, ZIP) keeps its own extent so its results are not clipped below the viewport.
                searchRadiusCenter = resolved.Point;
                searchRadiusMiles = resolved.Viewport != null
                    ? Math.Max( DefaultSearchRadiusMiles, HaversineMiles( resolved.Point, ( resolved.Viewport.North, resolved.Viewport.East ) ) )
                    : DefaultSearchRadiusMiles;
            }

            var isSearch = searchBounds != null;

            // Resolve the proximity location (the person) for distance and sort. A typed address or a
            // current-location search is measured from the search location itself (searchRadiusCenter, the
            // geocoded point), so the distances match where the visitor searched. An area search or an
            // unfiltered browse has no such point, so it falls back to the device coordinates the visitor
            // shared, else the server's best guess (profile address, then IP, then campus).
            GeographyPoint proximityPoint = null;

            // Whether the origin is a precise point the visitor gave (a search location or their shared
            // device location) rather than the server's coarse guess. Only a precise origin is marked on
            // the map, so the visitor sees the point distances are measured from; a guess (which can be a
            // city off) is not pinned.
            var isPreciseOrigin = false;
            if ( proximityEnabled )
            {
                if ( searchRadiusCenter != null )
                {
                    proximityPoint = searchRadiusCenter;
                    isPreciseOrigin = true;
                }
                else if ( query.ProximityLatitude.HasValue && query.ProximityLongitude.HasValue )
                {
                    proximityPoint = new GeographyPoint( query.ProximityLatitude.Value, query.ProximityLongitude.Value );
                    isPreciseOrigin = true;
                }
                else
                {
                    var guess = GetVisitorLocationGuess();
                    if ( guess.Latitude.HasValue && guess.Longitude.HasValue )
                    {
                        proximityPoint = new GeographyPoint( guess.Latitude.Value, guess.Longitude.Value );
                    }
                }
            }

            var hasProximity = proximityPoint != null;
            var proximityDbPoint = proximityPoint?.ToDatabase();

            // Resolve the Featured attributes once; both the engine filter and the presence requirement need them.
            var featuredAttributes = GetConfiguredAttributes( AttributeKey.FeaturedAttributes );

            var groupQuery = helper.ApplyFilters( helper.GetGroupLocationQueryable( options ), options, BuildFilters( query, featuredAttributes ) );

            // The More Filters (Display) selections delegate to each field type's own filter expression,
            // so they are applied here rather than through the shared engine's string operators.
            groupQuery = ApplyDisplayAttributeFilters( groupQuery, query );

            // Featured pills match values through the engine's "in" (in BuildFilters), which includes groups
            // that have no value for the attribute; require the value to be set so pills exclude unset groups
            // the way More Filters does.
            groupQuery = ApplyFeaturedAttributePresence( groupQuery, query, featuredAttributes );

            // Live text search by group name (not an engine filter).
            if ( GetAttributeValue( AttributeKey.EnableLiveSearch ).AsBoolean() && query.SearchTerm.IsNotNullOrWhiteSpace() )
            {
                var term = query.SearchTerm.Trim();
                groupQuery = groupQuery.Where( gl => gl.Group.Name.Contains( term ) );
            }

            // Day of week (multi-select, matches any selected day).
            var daysOfWeek = ( query.DaysOfWeek ?? new List<string>() ).AsEnumList<DayOfWeek>();
            if ( daysOfWeek.Any() )
            {
                groupQuery = groupQuery.Where( gl => gl.Group.Schedule.WeeklyDayOfWeek.HasValue
                    && daysOfWeek.Contains( gl.Group.Schedule.WeeklyDayOfWeek.Value ) );
            }

            // Time of day reuses the shared TimePeriodOfDay bucketing.
            var timePeriod = query.TimeOfDay.ConvertToEnumOrNull<TimePeriodOfDay>();
            if ( timePeriod.HasValue )
            {
                groupQuery = groupQuery
                    .Where( gl => gl.Group.Schedule.WeeklyTimeOfDay.HasValue )
                    .WhereTimePeriodIsOneOf( new[] { timePeriod.Value }, gl => gl.Group.Schedule.WeeklyTimeOfDay.Value );
            }

            /*
                Restrict the query to the search area when one is set. This database step is a coarse
                pre-filter on the TRUE location, expanded by the maximum fuzz offset, so it returns
                every group whose fuzzed marker could fall in the area (a superset). The exact,
                privacy-safe test then runs on the fuzzed point in memory (see FilterToFuzzedViewport),
                because membership must match the marker actually drawn - filtering the database on the
                true location would let a visitor triangulate it.
            */
            if ( isSearch )
            {
                var latitudeMargin = LocationObfuscator.DefaultCircleRadiusMeters / MetersPerDegreeLatitude;
                var centerLatitude = ( searchBounds.North + searchBounds.South ) / 2;
                var metersPerDegreeLongitude = MetersPerDegreeLatitude * Math.Cos( centerLatitude * Math.PI / 180 );
                var longitudeMargin = metersPerDegreeLongitude > 0
                    ? LocationObfuscator.DefaultCircleRadiusMeters / metersPerDegreeLongitude
                    : latitudeMargin;

                var southBound = searchBounds.South - latitudeMargin;
                var northBound = searchBounds.North + latitudeMargin;
                var westBound = searchBounds.West - longitudeMargin;
                var eastBound = searchBounds.East + longitudeMargin;

                groupQuery = groupQuery.Where( gl => gl.Location.GeoPoint.Latitude >= southBound
                    && gl.Location.GeoPoint.Latitude <= northBound
                    && gl.Location.GeoPoint.Longitude >= westBound
                    && gl.Location.GeoPoint.Longitude <= eastBound );
            }

            // Collapse multiple locations per group to one row. When proximity is on, carry the min
            // straight-line distance from the person (true location) to order the set server-side; the
            // displayed distance is recomputed from the fuzzed marker below.
            var groupAggregates = hasProximity
                ? groupQuery.GroupBy( gl => gl.GroupId ).Select( g => new
                {
                    GroupId = g.Key,
                    GroupName = g.Max( gl => gl.Group.Name ),
                    MinDistanceMeters = ( double? ) g.Min( gl => gl.Location.GeoPoint.Distance( proximityDbPoint ) )
                } )
                : groupQuery.GroupBy( gl => gl.GroupId ).Select( g => new
                {
                    GroupId = g.Key,
                    GroupName = g.Max( gl => gl.Group.Name ),
                    MinDistanceMeters = ( double? ) null
                } );

            // A search recomputes totalCount from the in-memory fuzzed-viewport filter below, so the database
            // COUNT is only needed for the browse; skipping it on a search avoids a wasted round trip.
            var totalCount = isSearch ? 0 : groupAggregates.Count();
            var singleCampusFiltered = ( query.CampusGuids?.Count ?? 0 ) == 1;

            // This is the SQL order, so it decides page membership and must be paged consistently:
            // nearest first by straight-line distance to the person when proximity is known (drive
            // distance cannot be sorted in the database), otherwise by name. Groups whose location is not
            // geocoded have no distance; they sort last (not first, which is where SQL would otherwise put
            // the nulls) so they land on the final page rather than crowding the nearest results. Each
            // returned page is then refined in memory below - by drive distance for a search - which only
            // reorders groups within a page, never across pages, so the global paging order stays consistent.
            var resultAggregates = ( hasProximity
                ? groupAggregates.OrderBy( r => r.MinDistanceMeters == null ? 1 : 0 ).ThenBy( r => r.MinDistanceMeters ).ThenBy( r => r.GroupName )
                : groupAggregates.OrderBy( r => r.GroupName ) )
                .Take( MaxResults )
                .ToList();

            var resultGroupIds = resultAggregates.Select( r => r.GroupId ).ToList();

            // A search keeps only groups whose fuzzed marker falls in the area (the database step was a
            // coarse superset), so totalCount reflects the in-area total; the browse keeps the full match
            // count. Either way the returned set is one page of ResultsPageSize groups: page 0 by
            // default, or a later page when the visitor pages through a result set larger than the cap
            // (a list-only layout, or a map search already at its maximum zoom). totalCount lets the
            // client size the pager.
            var page = Math.Max( 0, query.Page );
            var pageSkip = page * ResultsPageSize;

            if ( isSearch )
            {
                var inAreaGroupIds = FilterToFuzzedViewport( resultGroupIds, hasProximity, proximityDbPoint, searchBounds, searchRadiusCenter, searchRadiusMiles );
                totalCount = inAreaGroupIds.Count;
                resultGroupIds = inAreaGroupIds.Skip( pageSkip ).Take( ResultsPageSize ).ToList();
            }
            else
            {
                resultGroupIds = resultGroupIds.Skip( pageSkip ).Take( ResultsPageSize ).ToList();
            }

            /*
                Distance metrics are shown only for an EXPLICIT origin: a resolved current-location grant or
                a typed address. A guessed origin (the server's best guess with no shared location) still
                orders results by proximity to the guess, but surfaces no distance or drive-time numbers, so
                a visitor who never shared a location is never shown "how far away" they are.

                For an explicit origin, distance is measured from the person to the FUZZED marker (never the
                true location). Straight-line distance is cheap and computed for every group. Driving distance
                AND static drive time come from the Routes Matrix (one call, both fields, still the Essentials
                tier). To avoid looking up a pair the visitor's session already has, the client sends the drive
                distances/times it holds for this origin (KnownDistances), honored only when the request's
                origin key matches. Only the still-unknown groups go to the Routes Matrix; the newly looked-up
                pairs come back so the client can cache them. A group the provider cannot route has no drive
                distance/time (the template falls back to straight-line). The page is ordered by drive time
                where known, then by straight-line distance for the rest, then name.
            */
            var straightLineByGroup = new Dictionary<int, double>();
            var drivingByGroup = new Dictionary<int, (double Miles, double Minutes)>();
            var nameByGroup = resultAggregates.ToDictionary( r => r.GroupId, r => r.GroupName ?? string.Empty );
            var originKey = isPreciseOrigin ? GetOriginKey( proximityPoint ) : null;
            var newDistances = new Dictionary<string, GroupFinderDistanceBag>();

            // Resolved once for the result page: the precise-origin distance block and the map markers both
            // need each group's representative point, so it is computed here and reused rather than queried twice.
            List<(int GroupId, Guid Guid, double Latitude, double Longitude)> representativePoints = null;

            if ( isPreciseOrigin && resultGroupIds.Any() )
            {
                representativePoints = GetRepresentativePoints( resultGroupIds, true, proximityDbPoint );
                var guidByGroupId = representativePoints.ToDictionary( p => p.GroupId, p => p.Guid );
                var fuzzedByGroup = representativePoints.ToDictionary(
                    p => p.GroupId,
                    p => LocationObfuscator.GetFuzzedLocation( p.Guid, p.Latitude, p.Longitude ) );

                // Straight-line distance is always available for every group.
                foreach ( var pair in fuzzedByGroup )
                {
                    straightLineByGroup[pair.Key] = HaversineMiles( proximityPoint, pair.Value );
                }

                /*
                    Drive time and driving miles are the Driving Distance mode's addition, and the only part
                    of the distance work that leaves the database, so nothing here runs in Straight-Line
                    Distance mode. The straight-line numbers above already stand on their own, and the card
                    template falls back to them whenever a drive time is absent.
                */
                if ( distanceCalculation == DistanceCalculationMode.Driving )
                {
                    // The client's cached distances/times count only when they were computed for this same origin.
                    var knownDistances = query.KnownDistancesOriginKey == originKey
                        ? ( query.KnownDistances ?? new Dictionary<string, GroupFinderDistanceBag>() )
                        : new Dictionary<string, GroupFinderDistanceBag>();

                    var idsToLookUp = new List<int>();
                    foreach ( var groupId in resultGroupIds )
                    {
                        if ( !fuzzedByGroup.ContainsKey( groupId ) )
                        {
                            continue;
                        }

                        var groupGuid = guidByGroupId.TryGetValue( groupId, out var guid ) ? guid.ToString() : null;
                        var locationKey = GetDistanceLocationKey( fuzzedByGroup[groupId] );

                        // Reuse a cached distance only when it was computed for this same group location. A
                        // group that moved since (its fuzzed point shifts with its real location) misses the
                        // cache and is re-routed, so the card's distance and drive time track the new location.
                        if ( groupGuid != null
                            && knownDistances.TryGetValue( groupGuid, out var cached )
                            && cached != null
                            && cached.LocationKey == locationKey )
                        {
                            drivingByGroup[groupId] = ( cached.Miles, cached.Minutes );
                        }
                        else
                        {
                            idsToLookUp.Add( groupId );
                        }
                    }

                    if ( idsToLookUp.Any() )
                    {
                        foreach ( var lookedUp in GetDriveMatrixByGroup( proximityPoint, idsToLookUp, fuzzedByGroup ) )
                        {
                            drivingByGroup[lookedUp.Key] = lookedUp.Value;

                            if ( guidByGroupId.TryGetValue( lookedUp.Key, out var guid ) )
                            {
                                newDistances[guid.ToString()] = new GroupFinderDistanceBag
                                {
                                    Miles = lookedUp.Value.Miles,
                                    Minutes = lookedUp.Value.Minutes,
                                    LocationKey = GetDistanceLocationKey( fuzzedByGroup[lookedUp.Key] )
                                };
                            }
                        }
                    }

                    // Order by drive time where it is known (timed groups first, nearest by minutes), then the
                    // rest by straight-line distance, then name.
                    resultGroupIds = resultGroupIds
                        .OrderBy( id => drivingByGroup.ContainsKey( id ) ? 0 : 1 )
                        .ThenBy( id => drivingByGroup.TryGetValue( id, out var d )
                            ? d.Minutes
                            : ( straightLineByGroup.TryGetValue( id, out var straight ) ? straight : double.MaxValue ) )
                        .ThenBy( id => nameByGroup.TryGetValue( id, out var name ) ? name : string.Empty )
                        .ToList();
                }
            }
            else if ( !hasProximity )
            {
                resultGroupIds = resultGroupIds
                    .OrderBy( id => nameByGroup.TryGetValue( id, out var name ) ? name : string.Empty )
                    .ToList();
            }

            // A guessed origin (has proximity but not a precise one) keeps the proximity ordering already
            // applied to the candidate set and shows no distance metrics.

            // Load every result group, then restore the distance/name ordering the ids were sorted in.
            var groupsById = new GroupService( RockContext )
                .Queryable()
                .Where( g => resultGroupIds.Contains( g.Id ) )
                .ToList()
                .ToDictionary( g => g.Id );

            var resultGroups = resultGroupIds
                .Where( id => groupsById.ContainsKey( id ) )
                .Select( id => groupsById[id] )
                .ToList();

            // Batch-load the card attributes for the whole page in one query. Without this each card would
            // lazy-load its own group's attributes (an N+1); the per-group load in GetCardAttributes then
            // becomes a no-op because the values are already present.
            if ( GetAttributeValue( AttributeKey.ShowAttributeOnCard ).SplitDelimitedValues().AsGuidList().Any() )
            {
                resultGroups.LoadAttributes( RockContext );
            }

            // Average ages, if shown, are computed for the whole set in one query rather than per card.
            var averageAges = GetAttributeValue( AttributeKey.ShowAverageAge ).AsBoolean()
                ? GetAverageAgesByGroup( resultGroupIds )
                : new Dictionary<int, int>();

            // The common merge fields (current person, page parameters, globals) are identical for every card,
            // so they are resolved once here and each card layers only its own group-specific fields on top.
            var commonMergeFields = RequestContext.GetCommonMergeFields();
            commonMergeFields["ShowImage"] = GetAttributeValue( AttributeKey.ShowImage ).AsBoolean();

            // Parse the card template once for the whole page. The Lava engine already caches parsed templates,
            // but rendering the compiled template directly skips the per-card cache-key hash and lookup that the
            // string ResolveMergeFields path incurs for every card.
            var cardTemplate = GetAttributeValue( AttributeKey.GroupCardTemplate );
            if ( cardTemplate.IsNullOrWhiteSpace() )
            {
                cardTemplate = AttributeDefault.GroupCardTemplate;
            }

            var parseResult = LavaService.ParseTemplate( cardTemplate );
            var compiledCardTemplate = parseResult.HasErrors
                ? LavaService.ParseTemplate( $"Group card template error: {parseResult.GetLavaException()?.Message}" ).Template
                : parseResult.Template;
            var enabledLavaCommands = GlobalAttributesCache.Value( "DefaultEnabledLavaCommands" ).SplitDelimitedValues();

            var cards = resultGroups
                .Select( g =>
                {
                    var hasDrive = drivingByGroup.TryGetValue( g.Id, out var drive );
                    return ToCardBag(
                        g,
                        commonMergeFields,
                        compiledCardTemplate,
                        enabledLavaCommands,
                        singleCampusFiltered,
                        averageAges.TryGetValue( g.Id, out var age ) ? age : ( int? ) null,
                        straightLineByGroup.TryGetValue( g.Id, out var straightLine ) ? ( double? ) straightLine : null,
                        hasDrive ? ( double? ) drive.Miles : null,
                        hasDrive ? ( double? ) drive.Minutes : null );
                } )
                .ToList();

            return new GroupFinderResultsBag
            {
                TotalCount = totalCount,
                PageSize = ResultsPageSize,
                Cards = cards,
                Markers = GetMapMarkers( resultGroupIds, hasProximity, proximityDbPoint, representativePoints ),
                NewDistances = newDistances,
                OriginKey = originKey,
                OriginLatitude = isPreciseOrigin ? proximityPoint.Latitude : ( double? ) null,
                OriginLongitude = isPreciseOrigin ? proximityPoint.Longitude : ( double? ) null,
                SearchBoundsNorth = searchBounds?.North,
                SearchBoundsSouth = searchBounds?.South,
                SearchBoundsEast = searchBounds?.East,
                SearchBoundsWest = searchBounds?.West,
                ResolvedViewportNorth = resolvedOriginViewport?.North,
                ResolvedViewportSouth = resolvedOriginViewport?.South,
                ResolvedViewportEast = resolvedOriginViewport?.East,
                ResolvedViewportWest = resolvedOriginViewport?.West
            };
        }

        /// <summary>
        /// Builds the fuzzed map markers for the result groups, or an empty list when the map is off.
        /// </summary>
        /// <param name="groupIds">The group ids in the result set.</param>
        /// <param name="hasOrigin">Whether a proximity origin is in use (selects the closest location per group).</param>
        /// <param name="originPoint">The origin geography point, or null when proximity is not in use.</param>
        /// <param name="representativePoints">The result page's representative points when the caller already resolved them (same group ids, origin, and hasOrigin), reused to avoid a second query; null resolves them here.</param>
        /// <returns>One marker per group that has a mappable location, with coordinates fuzzed for privacy.</returns>
        private List<GroupFinderMapMarkerBag> GetMapMarkers( List<int> groupIds, bool hasOrigin, System.Data.Entity.Spatial.DbGeography originPoint, List<(int GroupId, Guid Guid, double Latitude, double Longitude)> representativePoints = null )
        {
            if ( !GetAttributeValue( AttributeKey.ShowMap ).AsBoolean() || !groupIds.Any() )
            {
                return new List<GroupFinderMapMarkerBag>();
            }

            var markers = new List<GroupFinderMapMarkerBag>();
            foreach ( var point in representativePoints ?? GetRepresentativePoints( groupIds, hasOrigin, originPoint ) )
            {
                var fuzzed = LocationObfuscator.GetFuzzedLocation( point.Guid, point.Latitude, point.Longitude );

                markers.Add( new GroupFinderMapMarkerBag
                {
                    GroupGuid = point.Guid.ToString(),
                    Latitude = fuzzed.Latitude,
                    Longitude = fuzzed.Longitude,
                    CircleRadiusMeters = LocationObfuscator.DefaultCircleRadiusMeters
                } );
            }

            return markers;
        }

        /// <summary>
        /// Gets one representative location per group as true (un-fuzzed) coordinates: the closest to the origin when proximity is in use, otherwise a single stable location.
        /// </summary>
        /// <param name="groupIds">The group ids to resolve locations for.</param>
        /// <param name="hasOrigin">Whether a proximity origin is in use (selects the closest location per group).</param>
        /// <param name="originPoint">The origin geography point, or null when proximity is not in use.</param>
        /// <returns>One tuple per group that has a mappable location, carrying its guid and true coordinates.</returns>
        private List<(int GroupId, Guid Guid, double Latitude, double Longitude)> GetRepresentativePoints( List<int> groupIds, bool hasOrigin, System.Data.Entity.Spatial.DbGeography originPoint )
        {
            if ( !groupIds.Any() )
            {
                return new List<(int, Guid, double, double)>();
            }

            var locationQuery = new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => groupIds.Contains( gl.GroupId ) && gl.Location.GeoPoint != null );

            var representativeLocations = hasOrigin
                ? locationQuery.GroupBy( gl => gl.GroupId ).Select( g => g.OrderBy( gl => gl.Location.GeoPoint.Distance( originPoint ) ).FirstOrDefault() )
                : locationQuery.GroupBy( gl => gl.GroupId ).Select( g => g.OrderBy( gl => gl.Id ).FirstOrDefault() );

            return representativeLocations
                .Select( gl => new
                {
                    gl.GroupId,
                    gl.Group.Guid,
                    gl.Location.GeoPoint.Latitude,
                    gl.Location.GeoPoint.Longitude
                } )
                .ToList()
                .Where( p => p.Latitude.HasValue && p.Longitude.HasValue )
                .Select( p => (p.GroupId, p.Guid, p.Latitude.Value, p.Longitude.Value) )
                .ToList();
        }

        /// <summary>
        /// Filters candidate group ids to those whose fuzzed marker falls within the requested map viewport, preserving the candidate order.
        /// </summary>
        /// <remarks>
        /// The privacy fuzz is deterministic per group but is applied in memory (it is not expressible in the database query), so the viewport test matches the marker actually drawn. Deciding membership on the fuzzed point rather than the true one prevents a visitor from triangulating a true location by panning the map.
        /// </remarks>
        /// <param name="candidateGroupIds">The ordered candidate group ids from the coarse database pre-filter.</param>
        /// <param name="hasOrigin">Whether a proximity origin is in use.</param>
        /// <param name="originPoint">The origin geography point, or null when proximity is not in use.</param>
        /// <param name="bounds">The search area the fuzzed marker must fall within.</param>
        /// <returns>The candidate ids, in order, whose fuzzed marker lies within the area.</returns>
        /// <summary>
        /// Filters the candidate groups to those whose fuzzed marker falls in the search area, testing on the fuzzed point rather than the true location so membership matches the drawn marker.
        /// </summary>
        /// <param name="candidateGroupIds">The coarse superset of group ids to test.</param>
        /// <param name="hasOrigin">Whether an origin point is available for representative-point selection.</param>
        /// <param name="originPoint">The origin used to pick each group's representative location.</param>
        /// <param name="bounds">The bounding box the fuzzed marker must fall within.</param>
        /// <param name="radiusCenter">When set (with a positive radius), also requires the fuzzed marker to be within <paramref name="radiusMiles"/> straight-line of this point, turning the box into a true circle. Null skips the circular test (e.g. an explicit map-box search).</param>
        /// <param name="radiusMiles">The straight-line radius, in miles, applied around <paramref name="radiusCenter"/>.</param>
        private List<int> FilterToFuzzedViewport( List<int> candidateGroupIds, bool hasOrigin, System.Data.Entity.Spatial.DbGeography originPoint, GeographyBounds bounds, GeographyPoint radiusCenter = null, double radiusMiles = 0 )
        {
            var pointsByGroup = GetRepresentativePoints( candidateGroupIds, hasOrigin, originPoint )
                .ToDictionary( p => p.GroupId );

            var south = bounds.South;
            var north = bounds.North;
            var west = bounds.West;
            var east = bounds.East;
            var hasRadius = radiusCenter != null && radiusMiles > 0;

            var inViewport = new List<int>();
            foreach ( var groupId in candidateGroupIds )
            {
                if ( !pointsByGroup.TryGetValue( groupId, out var point ) )
                {
                    continue;
                }

                var fuzzed = LocationObfuscator.GetFuzzedLocation( point.Guid, point.Latitude, point.Longitude );
                var inBox = fuzzed.Latitude >= south && fuzzed.Latitude <= north
                    && fuzzed.Longitude >= west && fuzzed.Longitude <= east;

                if ( inBox && ( !hasRadius || HaversineMiles( radiusCenter, fuzzed ) <= radiusMiles ) )
                {
                    inViewport.Add( groupId );
                }
            }

            return inViewport;
        }

        #endregion
    }
}
