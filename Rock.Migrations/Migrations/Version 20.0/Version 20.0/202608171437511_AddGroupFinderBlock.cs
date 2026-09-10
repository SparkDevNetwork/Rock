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
namespace Rock.Migrations
{
    /// <summary>
    /// Adds the Obsidian Group Finder block and the Meeting Style columns behind its meeting-style
    /// filter, and renames the WebForms Group Finder to "Group Finder (Legacy)" so the new block takes
    /// over the name.
    /// </summary>
    public partial class AddGroupFinderBlock : Rock.Migrations.RockMigration
    {
        #region Constants

        /// <summary>
        /// The WebForms Group Finder block type Guid, carried through the rename unchanged.
        /// </summary>
        private const string LegacyGroupFinderBlockTypeGuid = "9F8F2D68-DEEA-4686-810F-AB32923F855E";

        /// <summary>
        /// The path the WebForms block occupied before the rename.
        /// </summary>
        private const string OriginalLegacyBlockPath = "~/Blocks/Groups/GroupFinder.ascx";

        /// <summary>
        /// The path the WebForms block occupies after the rename.
        /// </summary>
        private const string RenamedLegacyBlockPath = "~/Blocks/Groups/GroupFinderLegacy.ascx";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "MeetingStyle", c => c.Int());
            AddColumn("dbo.GroupType", "IsMeetingStyleEnabled", c => c.Boolean(nullable: false));

            // Enable Meeting Style on the built-in Small Group type so the Group Finder's
            // meeting-style filter and the per-group field are usable out of the box.
            Sql( @"
                UPDATE [GroupType]
                SET [IsMeetingStyleEnabled] = 1
                WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'" );

            // Renamed before the block below registers, so "Group Finder" is never on two block types.
            RepointLegacyGroupFinderBlockType( "Group Finder (Legacy)", RenamedLegacyBlockPath );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupFinder
            RockMigrationHelper.UpdateEntityType("Rock.Blocks.Group.GroupFinder", "Group Finder", "Rock.Blocks.Group.GroupFinder, Rock.Blocks, Version=20.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "B6E7A1C2-0D4F-4E90-9C3A-2F1B7A0E5D64");

            // Add/Update Obsidian Block Type
            //   Name:Group Finder
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupFinder
            RockMigrationHelper.AddOrUpdateEntityBlockType("Group Finder", "Block for people to find a group through filters, a card list, and an optional map.", "Rock.Blocks.Group.GroupFinder", "Group", "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupType", "Group Types", @"The group types whose groups the finder offers, and whose attributes the filter and card settings are drawn from.", 0, @"50FCFB30-F51A-49DF-86F4-2B176EA1820B", "54A01BA6-D333-439A-96CF-14AADB8E29F1" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide Campus Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Campus Filters", "HideCampusFilters", "Hide Campus Filters", @"", 10, @"False", "0CA7B6A9-FA36-4D61-8D6A-6F4B95A57CDB" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide Where Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Where Filters", "HideWhereFilters", "Hide Where Filters", @"", 20, @"False", "80B80EA5-148E-475B-A33D-7E93AE998C28" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide When Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide When Filters", "HideWhenFilters", "Hide When Filters", @"", 30, @"False", "0F0EB234-0E9B-4488-87B2-FEB4247EA080" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide What Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide What Filters", "HideWhatFilters", "Hide What Filters", @"", 40, @"False", "D36CCDB4-D949-4D27-A65C-AAD5DD407B49" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"The campus types offered by the campus filter.", 50, @"", "5A7919BC-9FED-4960-BC4A-6C49BEF76D4A" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"The campus statuses offered by the campus filter.", 60, @"", "5E0CB72E-DA91-4470-8704-7CFC9A1D247C" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Enable Proximity Features
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Proximity Features", "EnableProximityFeatures", "Enable Proximity Features", @"Renders an address input and a Use Current Location action. Requires Google APIs to be configured.", 70, @"False", "4A29171C-6DD1-45E3-A0DE-AC168937737C" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Supported Meeting Styles
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Supported Meeting Styles", "SupportedMeetingStyles", "Supported Meeting Styles", @"The meeting styles offered by the Where filter. When none are selected the Meeting Style filter is hidden.", 80, @"", "4E7D7A93-81DA-438A-B5E4-119B47367DC8" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Day of Week Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Day of Week Filter", "DisplayDayOfWeekFilter", "Display Day of Week Filter", @"", 90, @"False", "6487110F-7316-48CD-8CFF-5B7DBC63282C" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Time of Day Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Time of Day Filter", "DisplayTimeOfDayFilter", "Display Time of Day Filter", @"", 100, @"False", "151DD14E-E739-4630-89D0-45AF8D2B4146" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Live Text Search
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Live Text Search", "EnableLiveSearch", "Live Text Search", @"Renders a text field that filters groups by name as the visitor types.", 110, @"False", "029ECA28-EE55-4341-ADD8-36808BE37D57" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Featured Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Featured Attributes", "FeaturedAttributes", "Featured Attributes", @"The group attributes promoted into the What section of the filter bar as pills. Mutually exclusive with Display Attribute Filters, and limited to Single-select, Multi-select, and Boolean field types.", 130, @"", "1543F60A-A9EB-4C63-A0FF-6BAC6FDA2B33" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Attribute Filters
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Display Attribute Filters", "AttributeFilters", "Display Attribute Filters", @"The group attributes an individual can filter results by, rendered in the More Filters modal. Mutually exclusive with Featured Attributes.", 120, @"", "A61B6BF7-983E-41B7-A14C-3ACB18D0E1C2" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Attribute on Card
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Show Attribute on Card", "AttributeColumns", "Show Attribute on Card", @"The group attributes displayed on each result card.", 140, @"", "36955021-CB1D-401C-B66D-3BD1E42CFBD6" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Image
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Image", "ShowImage", "Show Image", @"", 150, @"False", "80D044F6-75B4-4C91-811A-6CAE9D5BA95E" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Map
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map", "ShowMap", "Show Map", @"When enabled the layout switches to a side by side arrangement of cards and a map.", 170, @"False", "05DEFDF0-AEDF-4251-A1F0-B2365EFAE555" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Average Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Average Age", "ShowAverageAge", "Show Average Age", @"", 160, @"False", "75837759-AB39-4E15-97AA-C80A6B95D341" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Register Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Register Page", "RegisterPage", "Register Page", @"The page a visitor is sent to when signing up for a group.", 180, @"", "C1514372-A779-4EBC-8CA7-F11FCD210157" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Marker Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Group Marker Color", "GroupMarkerColor", "Group Marker Color", @"The color of the group markers on the map. This one color drives every state via opacity: a solid 2px border with a light fill when hovered or selected, and a lighter border and fill otherwise.", 175, @"#D70015", "D05EAC1A-D883-4EEC-9F37-4D4511CB4A19" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Current Location Marker Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Current Location Marker Color", "CurrentLocationMarkerColor", "Current Location Marker Color", @"The color of the ""you are here"" proximity marker (the visitor's current location or entered address) on the map.", 176, @"#EE7725", "E8AC38A7-C8FC-4B5F-8588-1423E95A7FAF" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Map Style
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "Map Style", @"The map style applied to the results map. When not set, the block's default style is used.", 177, @"", "FE65EA53-E9B3-49DC-B8C3-73C5C4BD62A6" );

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Card Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Group Card Template", "GroupCardTemplate", "Group Card Template", @"The Lava template that renders the content of each result card, from the card border inward. The block owns the card's border, corner radius, highlighting, and click-to-select behavior; this template controls everything inside, including the padding and the register button. Clear this to reset to the default template.", 190, @"{% comment %}
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
{% endcomment %}
<div class=""group-finder-card-content"">
    {%- if ShowImage -%}
    {%- if ImageUrl and ImageUrl != '' -%}
    <div class=""group-finder-card-media"">
        <img class=""group-finder-card-image"" src=""{{ ImageUrl }}"" alt=""{{ Group.Name | Escape }}"" />
        {%- if GroupTypeName and GroupTypeName != '' -%}<span class=""group-finder-card-badge"">{{ GroupTypeName }}{%- endif -%}</span>
    </div>
    {%- else -%}
    <div class=""group-finder-card-media is-fallback"" style=""--group-finder-fallback-color: {{ GroupTypeColor | Default:'#4fd1c5' }}"">
        {%- if GroupTypeName and GroupTypeName != '' -%}<span class=""group-finder-card-badge"">{{ GroupTypeName }}{%- endif -%}</span>
    </div>
    {%- endif -%}
    {%- endif -%}

    <div class=""group-finder-card-body"">
        {%- if DrivingDistance or StraightLineDistance or AverageAge -%}
        <div class=""group-finder-card-meta"">
            {%- if DriveTime and DriveTime != '' -%}
            <span class=""group-finder-card-distance""><strong>Drive Time:</strong> {{ DriveTime }}{% if DrivingDistance %} ({{ DrivingDistance | Format:'0.0' }} mi){% endif %}</span>
            {%- elsif StraightLineDistance -%}
            <span class=""group-finder-card-distance""><strong>Distance:</strong> ~{{ StraightLineDistance | Format:'0.0' }} mile{% if StraightLineDistance != 1 %}s{% endif %}</span>
            {%- endif -%}
            {%- if AverageAge -%}
            <span class=""group-finder-card-average-age""><strong>Avg Age:</strong> {{ AverageAge }} yrs</span>
            {%- endif -%}
        </div>
        <hr class=""group-finder-card-divider"" />
        {%- endif -%}

        <h3 class=""group-finder-card-title"">{{ Group.Name }}</h3>

        {%- if ScheduleText and ScheduleText != '' -%}
        <div class=""group-finder-card-schedule"">{{ ScheduleText }}</div>
        {%- endif -%}

        {%- if Group.Description and Group.Description != '' -%}
        <p class=""group-finder-card-description"">{{ Group.Description }}</p>
        {%- endif -%}

        {%- assign attributeCount = Attributes | Size -%}
        {%- assign hasCampus = false -%}
        {%- if CampusName and CampusName != '' -%}{%- assign hasCampus = true -%}{%- endif -%}
        {%- if hasCampus or attributeCount > 0 -%}
        <ul class=""group-finder-card-attributes"">
            {%- if CampusName and CampusName != '' -%}
            <li class=""group-finder-card-attribute""><i class=""ti ti-map-pin""></i><span>{{ CampusName }}</li></span>
            {%- endif -%}
            {%- for attribute in Attributes -%}
            <li class=""group-finder-card-attribute"">
                {%- if attribute.IconCssClass and attribute.IconCssClass != '' -%}<i class=""{{ attribute.IconCssClass }}""></i>{%- endif -%}
                <span>{{ attribute.Value }}</span>
            </li>
            {%- endfor -%}
        </ul>
        {%- endif -%}

        {%- if RegisterUrl and RegisterUrl != '' -%}
        <div class=""group-finder-card-footer"">
            <a class=""group-finder-card-action btn btn-primary"" href=""{{ RegisterUrl }}"">Register</a>
        </div>
        {%- endif -%}
    </div>
</div>", "E6A1B1B5-D1A6-4443-9EAE-E47D02AA4F87" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            
            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Card Template
            RockMigrationHelper.DeleteAttribute("E6A1B1B5-D1A6-4443-9EAE-E47D02AA4F87");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Map Style
            RockMigrationHelper.DeleteAttribute("FE65EA53-E9B3-49DC-B8C3-73C5C4BD62A6");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Current Location Marker Color
            RockMigrationHelper.DeleteAttribute("E8AC38A7-C8FC-4B5F-8588-1423E95A7FAF");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Marker Color
            RockMigrationHelper.DeleteAttribute("D05EAC1A-D883-4EEC-9F37-4D4511CB4A19");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Register Page
            RockMigrationHelper.DeleteAttribute("C1514372-A779-4EBC-8CA7-F11FCD210157");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Map
            RockMigrationHelper.DeleteAttribute("05DEFDF0-AEDF-4251-A1F0-B2365EFAE555");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Average Age
            RockMigrationHelper.DeleteAttribute("75837759-AB39-4E15-97AA-C80A6B95D341");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Image
            RockMigrationHelper.DeleteAttribute("80D044F6-75B4-4C91-811A-6CAE9D5BA95E");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Show Attribute on Card
            RockMigrationHelper.DeleteAttribute("36955021-CB1D-401C-B66D-3BD1E42CFBD6");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Featured Attributes
            RockMigrationHelper.DeleteAttribute("1543F60A-A9EB-4C63-A0FF-6BAC6FDA2B33");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Attribute Filters
            RockMigrationHelper.DeleteAttribute("A61B6BF7-983E-41B7-A14C-3ACB18D0E1C2");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Live Text Search
            RockMigrationHelper.DeleteAttribute("029ECA28-EE55-4341-ADD8-36808BE37D57");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Time of Day Filter
            RockMigrationHelper.DeleteAttribute("151DD14E-E739-4630-89D0-45AF8D2B4146");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Display Day of Week Filter
            RockMigrationHelper.DeleteAttribute("6487110F-7316-48CD-8CFF-5B7DBC63282C");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Supported Meeting Styles
            RockMigrationHelper.DeleteAttribute("4E7D7A93-81DA-438A-B5E4-119B47367DC8");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Enable Proximity Features
            RockMigrationHelper.DeleteAttribute("4A29171C-6DD1-45E3-A0DE-AC168937737C");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute("5E0CB72E-DA91-4470-8704-7CFC9A1D247C");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute("5A7919BC-9FED-4960-BC4A-6C49BEF76D4A");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide When Filters
            RockMigrationHelper.DeleteAttribute("0F0EB234-0E9B-4488-87B2-FEB4247EA080");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide What Filters
            RockMigrationHelper.DeleteAttribute("D36CCDB4-D949-4D27-A65C-AAD5DD407B49");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide Where Filters
            RockMigrationHelper.DeleteAttribute("80B80EA5-148E-475B-A33D-7E93AE998C28");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Hide Campus Filters
            RockMigrationHelper.DeleteAttribute("0CA7B6A9-FA36-4D61-8D6A-6F4B95A57CDB");

            // Attribute for BlockType
            //   BlockType: Group Finder
            //   Category: Group
            //   Attribute: Group Types
            RockMigrationHelper.DeleteAttribute("54A01BA6-D333-439A-96CF-14AADB8E29F1");

            // Delete BlockType
            //   Name: Group Finder
            //   Category: Group
            //   Path: -
            //   EntityType: Group Finder
            RockMigrationHelper.DeleteBlockType("3C9A5E71-8B24-4D0E-A6F1-9E7C2B4A0D53");

            // Restored after the block above is deleted, which frees the "Group Finder" name again.
            RepointLegacyGroupFinderBlockType( "Group Finder", OriginalLegacyBlockPath );

            DropColumn("dbo.GroupType", "IsMeetingStyleEnabled");
            DropColumn("dbo.Group", "MeetingStyle");
        }

        #region Private Methods

        /// <summary>
        /// Applies a name and path to the WebForms Group Finder block type, matched on its Guid.
        /// </summary>
        /// <remarks>
        /// A hand-written UPDATE is used rather than <c>UpdateBlockTypeByGuid()</c> for two reasons: that
        /// helper's DELETE is keyed on Path alone, which wipes every entity-based block type when the path
        /// is empty, and its INSERT branch would add a second row if the Guid lookup ever missed. Matching
        /// on Guid moves the existing row instead, which is what keeps configured block instances attached.
        /// </remarks>
        /// <param name="name">The admin-facing block type name to apply.</param>
        /// <param name="path">The block path to apply.</param>
        private void RepointLegacyGroupFinderBlockType( string name, string path )
        {
            Sql( $@"
                UPDATE [BlockType]
                SET [Name] = '{name}'
                    , [Path] = '{path}'
                WHERE [Guid] = '{LegacyGroupFinderBlockTypeGuid}'" );
        }

        #endregion
    }
}
