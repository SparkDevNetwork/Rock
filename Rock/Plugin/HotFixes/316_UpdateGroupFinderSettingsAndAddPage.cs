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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Brings the Group Finder block's stored settings in line with its code: the reviewed defaults, and
    /// the removal of the three Hide Filter settings it no longer declares. Also adds a Group Finder page
    /// to the external site's Connect section so churches find the new block already placed.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 316, "20.0" )]
    public class UpdateGroupFinderSettingsAndAddPage : Migration
    {
        #region Constants

        private const string GroupFinderBlockTypeGuid = "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53";

        // The external site's Connect page, whose child pages are its side navigation, and the Full Width
        // layout the new page uses.
        private const string ConnectPageGuid = "7625A63E-6650-4886-B605-53C2234FA5E1";
        private const string ExternalFullWidthLayoutGuid = "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD";

        private const string GroupFinderPageGuid = "BB8FC981-77EA-4416-B672-6B3969384043";
        private const string GroupFinderPageRouteGuid = "727A4B84-9D40-46CC-A5C5-200ED7C41273";
        private const string GroupFinderBlockGuid = "63733A54-060F-4C8A-8166-87F9E1E4E835";

        // Field types used by the settings this migration restates.
        private const string BooleanFieldTypeGuid = "1EDAFDED-DFE6-4334-B019-6EECBA89E05A";
        private const string DefinedValueFieldTypeGuid = "59D5A94C-94A0-4630-B80A-BB25697D74C7";
        private const string CheckboxListFieldTypeGuid = "BD0D9B57-2A41-4490-89FF-F01DAB7D4904";
        private const string ColorFieldTypeGuid = "D747E6AE-C383-4E22-8846-71518E3DD06F";
        private const string CodeEditorFieldTypeGuid = "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5";

        // The Hide Where/When/What settings the block dropped: each segment now shows itself whenever any
        // of its own filters is configured, so there is nothing left for these to control.
        private const string HideWhereFiltersAttributeGuid = "80B80EA5-148E-475B-A33D-7E93AE998C28";
        private const string HideWhenFiltersAttributeGuid = "0F0EB234-0E9B-4488-87B2-FEB4247EA080";
        private const string HideWhatFiltersAttributeGuid = "D36CCDB4-D949-4D27-A65C-AAD5DD407B49";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateSettingDefaults();
            RemoveHideFilterSettings();
            AddGroupFinderPage();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Only the page and its block are removed. The setting definitions are owned by the block's
            // own attribute declarations, which Rock re-applies from code on startup, so reverting them
            // here would be undone on the next run.
            RockMigrationHelper.DeleteBlock( GroupFinderBlockGuid );
            RockMigrationHelper.DeletePage( GroupFinderPageGuid );
        }

        /// <summary>
        /// Restates the settings whose defaults changed in the design review, so a database carrying the
        /// original values matches the block's declarations.
        /// </summary>
        private void UpdateSettingDefaults()
        {
            // Campus Types: Physical.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, DefinedValueFieldTypeGuid, "Campus Types", "CampusTypes", "Campus Types", @"The campus types offered by the campus filter.", 50, Rock.SystemGuid.DefinedValue.CAMPUS_TYPE_PHYSICAL, "5A7919BC-9FED-4960-BC4A-6C49BEF76D4A" );

            // Campus Statuses: Open.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, DefinedValueFieldTypeGuid, "Campus Statuses", "CampusStatuses", "Campus Statuses", @"The campus statuses offered by the campus filter.", 60, Rock.SystemGuid.DefinedValue.CAMPUS_STATUS_OPEN, "5E0CB72E-DA91-4470-8704-7CFC9A1D247C" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, BooleanFieldTypeGuid, "Enable Proximity Features", "EnableProximityFeatures", "Enable Proximity Features", @"Renders an address input and a Use Current Location action. Requires Google APIs to be configured.", 70, @"True", "4A29171C-6DD1-45E3-A0DE-AC168937737C" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, CheckboxListFieldTypeGuid, "Supported Meeting Styles", "SupportedMeetingStyles", "Supported Meeting Styles", @"The meeting styles offered by the Where filter. When none are selected the Meeting Style filter is hidden.", 80, @"InPerson", "4E7D7A93-81DA-438A-B5E4-119B47367DC8" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, BooleanFieldTypeGuid, "Display Day of Week Filter", "DisplayDayOfWeekFilter", "Display Day of Week Filter", @"When enabled, a Day of Week filter is shown in the When section.", 90, @"True", "6487110F-7316-48CD-8CFF-5B7DBC63282C" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, BooleanFieldTypeGuid, "Live Text Search", "EnableLiveSearch", "Live Text Search", @"Renders a text field that filters groups by name as the visitor types.", 110, @"True", "029ECA28-EE55-4341-ADD8-36808BE37D57" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, ColorFieldTypeGuid, "Group Marker Color", "GroupMarkerColor", "Group Marker Color", @"The color of the group markers on the map. This one color drives every state via opacity: a solid 2px border with a light fill when hovered or selected, and a lighter border and fill otherwise.", 175, @"#2B7FFF", "D05EAC1A-D883-4EEC-9F37-4D4511CB4A19" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, ColorFieldTypeGuid, "Current Location Marker Color", "CurrentLocationMarkerColor", "Current Location Marker Color", @"The color of the ""you are here"" proximity marker (the visitor's current location or entered address) on the map.", 176, @"#EF4444", "E8AC38A7-C8FC-4B5F-8588-1423E95A7FAF" );

            // The card template, restated for its Lava syntax alone: plain tags rather than whitespace-
            // trimming ones, and a /- -/ comment for the merge field reference.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( GroupFinderBlockTypeGuid, CodeEditorFieldTypeGuid, "Group Card Template", "GroupCardTemplate", "Group Card Template", @"The Lava template that renders the content of each result card, from the card border inward. The block owns the card's border, corner radius, highlighting, and click-to-select behavior; this template controls everything inside, including the padding and the register button. Clear this to reset to the default template.", 190, @"/-
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
<div class=""group-finder-card-content"">
    {% if ShowImage %}
    {% if ImageUrl and ImageUrl != '' %}
    <div class=""group-finder-card-media"">
        <img class=""group-finder-card-image"" src=""{{ ImageUrl }}"" alt=""{{ Group.Name | Escape }}"" />
        {% if GroupTypeName and GroupTypeName != '' %}<span class=""group-finder-card-badge"">{{ GroupTypeName | Escape }}</span>{% endif %}
    </div>
    {% else %}
    <div class=""group-finder-card-media is-fallback"" style=""--group-finder-fallback-color: {{ GroupTypeColor | Default:'#4fd1c5' }}"">
        {% if GroupTypeName and GroupTypeName != '' %}<span class=""group-finder-card-badge"">{{ GroupTypeName | Escape }}</span>{% endif %}
    </div>
    {% endif %}
    {% endif %}

    <div class=""group-finder-card-body"">
        {% if DrivingDistance or StraightLineDistance or AverageAge %}
        <div class=""group-finder-card-meta"">
            {% if DriveTime and DriveTime != '' %}
            <span class=""group-finder-card-distance""><strong>Drive Time:</strong> {{ DriveTime }}{% if DrivingDistance %} ({{ DrivingDistance | Format:'0.0' }} mi){% endif %}</span>
            {% elsif StraightLineDistance %}
            <span class=""group-finder-card-distance""><strong>Distance:</strong> ~{{ StraightLineDistance | Format:'0.0' }} mile{% if StraightLineDistance != 1 %}s{% endif %}</span>
            {% endif %}
            {% if AverageAge %}
            <span class=""group-finder-card-average-age""><strong>Avg Age:</strong> {{ AverageAge }} yrs</span>
            {% endif %}
        </div>
        <hr class=""group-finder-card-divider"" />
        {% endif %}

        <h3 class=""group-finder-card-title"">{{ Group.Name | Escape }}</h3>

        {% if ScheduleText and ScheduleText != '' %}
        <div class=""group-finder-card-schedule"">{{ ScheduleText | Escape }}</div>
        {% endif %}

        {% if Group.Description and Group.Description != '' %}
        <p class=""group-finder-card-description"">{{ Group.Description | Escape }}</p>
        {% endif %}

        {% assign attributeCount = Attributes | Size %}
        {% assign hasCampus = false %}
        {% if CampusName and CampusName != '' %}{% assign hasCampus = true %}{% endif %}
        {% if hasCampus or attributeCount > 0 %}
        <ul class=""group-finder-card-attributes"">
            {% if CampusName and CampusName != '' %}
            <li class=""group-finder-card-attribute""><i class=""ti ti-map-pin""></i><span>{{ CampusName | Escape }}</span></li>
            {% endif %}
            {% for attribute in Attributes %}
            <li class=""group-finder-card-attribute"">
                {% if attribute.IconCssClass and attribute.IconCssClass != '' %}<i class=""{{ attribute.IconCssClass }}""></i>{% endif %}
                <span>{{ attribute.Value | Escape }}</span>
            </li>
            {% endfor %}
        </ul>
        {% endif %}

        {% if RegisterUrl and RegisterUrl != '' %}
        <div class=""group-finder-card-footer"">
            <a class=""group-finder-card-action btn btn-primary"" href=""{{ RegisterUrl }}"">Register</a>
        </div>
        {% endif %}
    </div>
</div>", "E6A1B1B5-D1A6-4443-9EAE-E47D02AA4F87" );
        }

        /// <summary>
        /// Deletes the Hide Where, Hide When, and Hide What settings, along with any values saved for them.
        /// </summary>
        private void RemoveHideFilterSettings()
        {
            RockMigrationHelper.DeleteAttribute( HideWhereFiltersAttributeGuid );
            RockMigrationHelper.DeleteAttribute( HideWhenFiltersAttributeGuid );
            RockMigrationHelper.DeleteAttribute( HideWhatFiltersAttributeGuid );
        }

        /// <summary>
        /// Adds the Group Finder page and its block to the external site's Connect section.
        /// </summary>
        /// <remarks>
        /// The page is added with no security of its own, so it inherits Connect's, and the block is added
        /// with no attribute values, so it runs on the block's own defaults. Both are deliberate: a church
        /// should be able to see the new finder and decide what to configure. AddPage with no
        /// insertAfterPageGuid orders the page after its siblings, putting it at the foot of the section's
        /// navigation.
        /// </remarks>
        private void AddGroupFinderPage()
        {
            RockMigrationHelper.AddPage( true, ConnectPageGuid, ExternalFullWidthLayoutGuid, "Group Finder", "Find a group to join.", GroupFinderPageGuid, "ti ti-map-search" );
            RockMigrationHelper.AddPageRoute( GroupFinderPageGuid, "GroupFinder", GroupFinderPageRouteGuid );
            RockMigrationHelper.AddBlock( true, GroupFinderPageGuid, null, GroupFinderBlockTypeGuid, "Group Finder", "Main", @"", @"", 0, GroupFinderBlockGuid );
        }
    }
}
