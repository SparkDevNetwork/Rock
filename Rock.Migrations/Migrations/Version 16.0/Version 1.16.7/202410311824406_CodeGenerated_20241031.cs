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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20241031 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Mobile.CheckIn.CheckIn
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Mobile.CheckIn.CheckIn", "Check In", "Rock.Blocks.Mobile.CheckIn.CheckIn, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "BC0A4B6C-9F6D-4D39-8FFE-B6F9FA4B2F49" );

            // Add/Update Mobile Block Type
            //   Name:Check-in
            //   Category:Mobile > Check-in
            //   EntityType:Rock.Blocks.Mobile.CheckIn.CheckIn
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in", "Check yourself or family members in/out.", "Rock.Blocks.Mobile.CheckIn.CheckIn", "Mobile > Check-in", "85A9DDF0-D199-4D7B-887C-9AE8B3508444" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.CheckIn.CheckInScheduleBuilder
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.CheckInScheduleBuilder", "Check In Schedule Builder", "Rock.Blocks.CheckIn.CheckInScheduleBuilder, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "28B9DAB2-C58A-4459-9EE7-8D1895C09592" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.LavaShortcodeList", "Lava Shortcode List", "Rock.Blocks.Cms.LavaShortcodeList, Rock.Blocks, Version=1.16.6.9, Culture=neutral, PublicKeyToken=null", false, false, "B02078CC-FA42-4249-ABE0-7E166C63D2B6" );

            // Add/Update Obsidian Block Type
            //   Name:Schedule Builder
            //   Category:Check-in
            //   EntityType:Rock.Blocks.CheckIn.CheckInScheduleBuilder
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule Builder", "Helps to build schedules used for check-in.", "Rock.Blocks.CheckIn.CheckInScheduleBuilder", "Check-in", "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" );

            // Add/Update Obsidian Block Type
            //   Name:Lava Shortcode List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.LavaShortcodeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Lava Shortcode List", "Lists Lava Shortcode in the system.", "Rock.Blocks.Cms.LavaShortcodeList", "CMS", "09FD3746-48D1-4B94-AAA9-6896443AA43E" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Phone Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Phone Types", "PhoneTypes", "Phone Types", @"The phone numbers to display for editing (for non logged in individuals).", 13, @"", "D020C28F-4BFC-4A2E-8F9B-AFC4B62D32D1" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Required Phone Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0455ECBD-D54D-4485-BF4D-F469048AE10F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Required Phone Types", "PhoneTypesRequired", "Required Phone Types", @"The phone numbers that are required (for non logged in individuals).", 14, @"", "7FF00D9A-997D-483E-ADD5-D06A72DF8F0F" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B5B5BDF6-E715-40E1-835E-49CD5A163FF9" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5F3527FD-1611-460D-8E71-87DF9E33BC22" );

            // Attribute for BlockType
            //   BlockType: Lava Shortcode List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "09FD3746-48D1-4B94-AAA9-6896443AA43E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "58055646-80F7-47CA-9DDB-6C1A0BDE39C1" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Loading Screen Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Loading Screen Template", "LoadingScreenTemplate", "Loading Screen Template", @"The template to use for the loading screen. Lava is disabled.", 0, @"<VerticalStackLayout Spacing=""24""
    VerticalOptions=""Center""
    HorizontalOptions=""Center"">
    
    <ActivityIndicator IsVisible=""true"" IsRunning=""true"" />
        
    <Label Text=""Hang on while we fetch your details!""
        StyleClass=""text-interface-strongest, bold, subheadline"" />
        
</VerticalStackLayout>", "74FAF302-CE8C-4DF8-8DD2-85755A63B04B" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Completion Screen Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Screen Template", "CompletionScreenTemplate", "Completion Screen Template", @"The template to use for the success screen.", 1, @"{% assign hasNewAchievements = false %}

{% for attendance in RecordedAttendances %}
    {% assign newAchievementSize = attendance.JustCompletedAchievements | Size %}
    {% if newAchievementSize > 0 %}
        {% assign hasNewAchievements = true %}
    {% endif %}
{% endfor %}

<Grid>
    <StackLayout Spacing=""24"">
    
        //- Header Row
        <VerticalStackLayout>
            <Label Text=""Check-In Complete""
                StyleClass=""title1, bold, text-interface-strongest"" />
                
            <Label Text=""Below are the details of your check-in""
                StyleClass=""footnote, text-interface-strong"" />
        </VerticalStackLayout>
        
        //- Achievement Bar
        {% if hasNewAchievements %}
            <VerticalStackLayout>
                {% for attendance in RecordedAttendances.JustCompletedAchievements %}
                    <Grid ColumnDefinitions=""Auto, *"">
                        <Label Text=""test"" />
                    </Grid>        
                {% endfor %}
            </VerticalStackLayout>
        {% endif %}
    
        //- Attendance Details
        <VerticalStackLayout Spacing=""24"">
            {% for savedAttendance in RecordedAttendances %}
                <Rock:StyledBorder StyleClass=""p-16, bg-interface-softest, border, border-interface-soft, rounded"">
                    <VerticalStackLayout Spacing=""8"">
    
                        //- Avatar and person name
                        <HorizontalStackLayout Spacing=""16""
                            HorizontalOptions=""Center"">
                            <Rock:Avatar Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ savedAttendance.Attendance.Person.PhotoUrl | Escape }}"" 
                                HeightRequest=""32""
                                WidthRequest=""32"" />
        
                            <Label Text=""{{ savedAttendance.Attendance.Person.FullName | Escape }}""
                                StyleClass=""title3, bold, text-interface-stronger""
                                VerticalOptions=""Center"" />
                        </HorizontalStackLayout>
    
                        //- Checked into group
                        <Grid RowSpacing=""4""
                            RowDefinitions=""Auto, Auto"">
                            <Label Text=""Checked into""
                                StyleClass=""footnote, text-interface-strong"" />
                            
                            <Rock:StyledBorder StyleClass=""bg-primary-strong, px-8, py-4, rounded""
                                Grid.Row=""1"">
                                <Grid ColumnDefinitions=""*, Auto""
                                    VerticalOptions=""Center"">
                                    <Label Text=""{{ savedAttendance.Attendance.Location.Name | Escape }}""
                                        StyleClass=""body, bold, text-primary-soft"" />
                                    
                                    <Label Text=""{{ savedAttendance.Attendance.Schedule.Name | Escape }}""
                                        Grid.Column=""1""
                                        StyleClass=""body, text-primary-soft""/>
                                </Grid>
                            </Rock:StyledBorder>
                        </Grid>
                    </VerticalStackLayout>
                </Rock:StyledBorder>
            {% endfor %} 
        </VerticalStackLayout>
    </StackLayout>
    
    <Rock:ConfettiView IsAnimationEnabled=""True"" InputTransparent=""true"" />
</Grid>", "DC375A74-280B-4BB8-8F8D-49A51A101152" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Login Screen Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Login Screen Template", "LoginScreenTemplate", "Login Screen Template", @"The template to use for the screen that displays if a person is not logged in. Lava is disabled.", 2, @"", "96F9F7D5-E6BA-40A7-9316-37C87FD51CB2" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Allow Add Family Member
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Add Family Member", "AllowAddFamilyMember", "Allow Add Family Member", @"Whether or not this block should display the option to add a family member.", 3, @"False", "E018C9A6-96BD-4670-B10C-E06CD552D089" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Add Person Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Add Person Attributes", "AddPersonAttributes", "Add Person Attributes", @"The attributes to display when adding a person.", 4, @"", "A13FD9B1-D734-4156-806F-C8B8146E3872" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Configuration Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Configuration Template", "ConfigurationTemplate", "Configuration Template", @"The check-in configuration to use for this block.", 0, @"", "5BB516A9-2512-479D-9979-50DFAA705960" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Areas
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Areas", "Areas", "Areas", @"The areas to use for this block.", 0, @"", "6F90A16E-BB57-47D0-9230-07B633CAA65F" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Kiosk
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "85A9DDF0-D199-4D7B-887C-9AE8B3508444", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Kiosk", "Kiosk", "Kiosk", @"The kiosk to use for this block.", 0, @"", "EBA83553-0D5C-487A-9CF5-48E88EA67F8B" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Kiosk
            RockMigrationHelper.DeleteAttribute( "EBA83553-0D5C-487A-9CF5-48E88EA67F8B" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Areas
            RockMigrationHelper.DeleteAttribute( "6F90A16E-BB57-47D0-9230-07B633CAA65F" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Configuration Template
            RockMigrationHelper.DeleteAttribute( "5BB516A9-2512-479D-9979-50DFAA705960" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Add Person Attributes
            RockMigrationHelper.DeleteAttribute( "A13FD9B1-D734-4156-806F-C8B8146E3872" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Allow Add Family Member
            RockMigrationHelper.DeleteAttribute( "E018C9A6-96BD-4670-B10C-E06CD552D089" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Login Screen Template
            RockMigrationHelper.DeleteAttribute( "96F9F7D5-E6BA-40A7-9316-37C87FD51CB2" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Completion Screen Template
            RockMigrationHelper.DeleteAttribute( "DC375A74-280B-4BB8-8F8D-49A51A101152" );

            // Attribute for BlockType
            //   BlockType: Check-in
            //   Category: Mobile > Check-in
            //   Attribute: Loading Screen Template
            RockMigrationHelper.DeleteAttribute( "74FAF302-CE8C-4DF8-8DD2-85755A63B04B" );

            // Attribute for BlockType
            //   BlockType: Lava Shortcode List
            //   Category: CMS
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "58055646-80F7-47CA-9DDB-6C1A0BDE39C1" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "5F3527FD-1611-460D-8E71-87DF9E33BC22" );

            // Attribute for BlockType
            //   BlockType: Schedule Builder
            //   Category: Check-in
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B5B5BDF6-E715-40E1-835E-49CD5A163FF9" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Required Phone Types
            RockMigrationHelper.DeleteAttribute( "7FF00D9A-997D-483E-ADD5-D06A72DF8F0F" );

            // Attribute for BlockType
            //   BlockType: Pledge Entry
            //   Category: Finance
            //   Attribute: Phone Types
            RockMigrationHelper.DeleteAttribute( "D020C28F-4BFC-4A2E-8F9B-AFC4B62D32D1" );

            // Delete BlockType 
            //   Name: Check-in
            //   Category: Mobile > Check-in
            //   Path: -
            //   EntityType: Check In
            RockMigrationHelper.DeleteBlockType( "85A9DDF0-D199-4D7B-887C-9AE8B3508444" );

            // Delete BlockType 
            //   Name: Lava Shortcode List
            //   Category: CMS
            //   Path: -
            //   EntityType: Lava Shortcode List
            RockMigrationHelper.DeleteBlockType( "09FD3746-48D1-4B94-AAA9-6896443AA43E" );

            // Delete BlockType 
            //   Name: Schedule Builder
            //   Category: Check-in
            //   Path: -
            //   EntityType: Check In Schedule Builder
            RockMigrationHelper.DeleteBlockType( "03C8EA07-DAF5-4B5A-9BB6-3A1AF99BB135" );
        }
    }
}
