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
    public partial class Rollup_0526 : Rock.Migrations.RockMigration
    {
        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            UpdateMobileBlockTemplates();
            AddMobileNotesChannel();
            EnableLabelReprintingByDefault();
            AddStructuredContentMessageNote();
            AttendanceSelfEntryBlockSetting();
            BlockTypeServiceMetricsDefaultToZeroUp();
            BlockTypeServiceMetricsMetricDateDeterminedByUp();
            BlockTypeServiceMetricsDefaultToZeroDown();
            BlockTypeServiceMetricsMetricDateDeterminedByDown();
            UpdateServiceJobHistoryCount();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Attempt Detail","Displays the details of the given attempt for editing.","~/Blocks/Streaks/AchievementAttemptDetail.ascx","Streaks","E00619A6-2C1D-46D0-B62A-3C27273C2C30");
            RockMigrationHelper.UpdateBlockType("Achievement Attempt List","Lists all the people that have made an attempt at earning an achievement.","~/Blocks/Streaks/AchievementAttemptList.ascx","Streaks","12FBDD49-B59D-4BAC-BEAF-313EE9212E0B");
            RockMigrationHelper.UpdateBlockType("Achievement Type Detail","Displays the details of the given Achievement Type for editing.","~/Blocks/Streaks/AchievementTypeDetail.ascx","Streaks","B51CBAB6-D660-4A25-9706-A2CCC3652AB5");
            RockMigrationHelper.UpdateBlockType("Achievement Type List","Shows a list of all achievement types.","~/Blocks/Streaks/AchievementTypeList.ascx","Streaks","FD6AF432-4697-43AA-A53B-028CE387CC2A");
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "093A874B-1E87-4CEB-9D56-02FBA90F98F1");
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "5835B9C9-D9A5-4984-9CF1-E0181A8C3318");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5835B9C9-D9A5-4984-9CF1-E0181A8C3318", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "EE6CE54B-81A1-40A4-BA50-CFF08394699A" );
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5835B9C9-D9A5-4984-9CF1-E0181A8C3318", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "805EE8CE-F4A6-4399-9DED-90749D94988B" );
            // Attrib for BlockType: Attempt Detail:Streak Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00619A6-2C1D-46D0-B62A-3C27273C2C30", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Streak Page", "StreakPage", "Streak Page", @"Page used for viewing the streak that these attempts are derived from.", 1, @"", "A607C5D0-D4D6-4348-A41C-C6C33DDA4E21" );
            // Attrib for BlockType: Attempt Detail:Achievement Type Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E00619A6-2C1D-46D0-B62A-3C27273C2C30", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Type Page", "AchievementPage", "Achievement Type Page", @"Page used for viewing the achievement type that this attempt is toward.", 2, @"", "34D8B60F-F295-4173-8AE5-4BBEC99EBF0B" );
            // Attrib for BlockType: Achievement Attempt List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "12FBDD49-B59D-4BAC-BEAF-313EE9212E0B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page navigated to when a grid item is clicked.", 1, @"", "03183447-8F14-4D66-89B8-60FD701F56A3" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "12FBDD49-B59D-4BAC-BEAF-313EE9212E0B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2AE00451-0B20-4981-ABCF-7803224003AB" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "12FBDD49-B59D-4BAC-BEAF-313EE9212E0B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C43AE772-9154-42EA-BC0F-E8317DA5F667" );
            // Attrib for BlockType: Achievement Type List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD6AF432-4697-43AA-A53B-028CE387CC2A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "0FB54253-E88E-4B36-B024-F528B996944A" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD6AF432-4697-43AA-A53B-028CE387CC2A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "05223D41-C2A3-4CA5-965D-A4A4EB742252" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD6AF432-4697-43AA-A53B-028CE387CC2A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6ACA8CC1-684E-43F4-A792-E0B79A67C652" );
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals Younger Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Hide Individuals Younger Than", "HideIndividualsYoungerThan", "Hide Individuals Younger Than", @"The age that should be used as the cut-off for displaying on the attendance list. The value of 14 will hide individuals younger than 14. Individuals without an age will always be shown. Defaults to blank.", 25, @"", "CAE1C16B-2E7B-4002-A0DF-B3843F7B1459" );
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals In Grade Less Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Hide Individuals In Grade Less Than", "HideIndividualsInGradeLessThan", "Hide Individuals In Grade Less Than", @"Individuals in grades lower than this value will not be show on the attendance list. Defaults to empty (not set).", 26, @"", "0DA87F7A-C593-4830-84AA-26A0A2719717" );
            // Attrib for BlockType: Scheduled Transaction Edit:Impersonator can see saved accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5171C4E5-7698-453E-9CC8-088D362296DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Impersonator can see saved accounts", "ImpersonatorCanSeeSavedAccounts", "Impersonator can see saved accounts", @"Should the current user be able to view other people's saved accounts?  IMPORTANT: This should only be enabled on an internal page that is secured to trusted users", 0, @"False", "10B6F8D0-2A9F-4949-A252-CBD710FF6749" );
            // Attrib for BlockType: Service Metrics Entry:Insert 0 for Blank Items
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Insert 0 for Blank Items", "DefaultToZero", "Insert 0 for Blank Items", @"If enabled, a zero will be added to any metrics that are left empty when entering data.", 5, @"false", "1820C4E0-9F01-49F7-A668-BD195BDBA5B8" );
            // Attrib for BlockType: Service Metrics Entry:Metric Date Determined By
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Metric Date Determined By", "MetricDateDeterminedBy", "Metric Date Determined By", @"This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.", 6, @"0", "BD311D1C-6420-4CFD-B89D-8263E4E4B563" );
            // Attrib for BlockType: Streak Detail:Achievement Attempts Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA9857FF-6703-4E4E-A6FF-65C23EBD2216", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievement Attempts Page", "AttemptsPage", "Achievement Attempts Page", @"Page used for viewing a list of achievement attempts for this streak.", 1, @"", "0ED59A74-2A95-42BE-A4DB-9071CC01EE53" );
            // Attrib for BlockType: Streak Type Detail:Achievements Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9D4AF22-7743-478A-9D21-AEA4F1A0C5F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Achievements Page", "AchievementsPage", "Achievements Page", @"Page used for viewing a list of streak type achievement types.", 3, @"", "C0BB9FAB-A1F4-4284-8A94-8D085DFF0988" );
        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Streak Type Detail:Achievements Page
            RockMigrationHelper.DeleteAttribute("C0BB9FAB-A1F4-4284-8A94-8D085DFF0988");
            // Attrib for BlockType: Streak Detail:Achievement Attempts Page
            RockMigrationHelper.DeleteAttribute("0ED59A74-2A95-42BE-A4DB-9071CC01EE53");
            // Attrib for BlockType: Service Metrics Entry:Metric Date Determined By
            RockMigrationHelper.DeleteAttribute("BD311D1C-6420-4CFD-B89D-8263E4E4B563");
            // Attrib for BlockType: Service Metrics Entry:Insert 0 for Blank Items
            RockMigrationHelper.DeleteAttribute("1820C4E0-9F01-49F7-A668-BD195BDBA5B8");
            // Attrib for BlockType: Scheduled Transaction Edit:Impersonator can see saved accounts
            RockMigrationHelper.DeleteAttribute("10B6F8D0-2A9F-4949-A252-CBD710FF6749");
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals In Grade Less Than
            RockMigrationHelper.DeleteAttribute("0DA87F7A-C593-4830-84AA-26A0A2719717");
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals Younger Than
            RockMigrationHelper.DeleteAttribute("CAE1C16B-2E7B-4002-A0DF-B3843F7B1459");
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6ACA8CC1-684E-43F4-A792-E0B79A67C652");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("05223D41-C2A3-4CA5-965D-A4A4EB742252");
            // Attrib for BlockType: Achievement Type List:Detail Page
            RockMigrationHelper.DeleteAttribute("0FB54253-E88E-4B36-B024-F528B996944A");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C43AE772-9154-42EA-BC0F-E8317DA5F667");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("2AE00451-0B20-4981-ABCF-7803224003AB");
            // Attrib for BlockType: Achievement Attempt List:Detail Page
            RockMigrationHelper.DeleteAttribute("03183447-8F14-4D66-89B8-60FD701F56A3");
            // Attrib for BlockType: Attempt Detail:Achievement Type Page
            RockMigrationHelper.DeleteAttribute("34D8B60F-F295-4173-8AE5-4BBEC99EBF0B");
            // Attrib for BlockType: Attempt Detail:Streak Page
            RockMigrationHelper.DeleteAttribute("A607C5D0-D4D6-4348-A41C-C6C33DDA4E21");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.DeleteAttribute("805EE8CE-F4A6-4399-9DED-90749D94988B");
            // Attrib for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.DeleteAttribute("EE6CE54B-81A1-40A4-BA50-CFF08394699A");
            RockMigrationHelper.DeleteBlockType("FD6AF432-4697-43AA-A53B-028CE387CC2A"); // Achievement Type List
            RockMigrationHelper.DeleteBlockType("B51CBAB6-D660-4A25-9706-A2CCC3652AB5"); // Achievement Type Detail
            RockMigrationHelper.DeleteBlockType("12FBDD49-B59D-4BAC-BEAF-313EE9212E0B"); // Achievement Attempt List
            RockMigrationHelper.DeleteBlockType("E00619A6-2C1D-46D0-B62A-3C27273C2C30"); // Attempt Detail
            RockMigrationHelper.DeleteBlockType("5835B9C9-D9A5-4984-9CF1-E0181A8C3318"); // Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("093A874B-1E87-4CEB-9D56-02FBA90F98F1"); // Structured Content View
        }

        /// <summary>
        /// DH: Update mobile block templates for Id => Guid conversion.
        /// </summary>
        private void UpdateMobileBlockTemplates()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6207AF10-B6C9-40B5-8AA5-4C11FA6D0966",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    <StackLayout Orientation=""Horizontal"" Spacing=""20"">
        <StackLayout Orientation=""Vertical"" Spacing=""0"" HorizontalOptions=""FillAndExpand"">
            <Label StyleClass=""h1"" Text=""{{ Group.Name | Escape }} Group"" />
            <Label Text=""{{ Group.Members | Size }} members"" LineHeight=""0.8"" />
        </StackLayout>
        {% if GroupEditPage != '' and AllowedActions.Edit == true %}
        <Rock:Icon IconClass=""Ellipsis-v"" FontSize=""24"" TextColor=""#ccc"" Command=""{Binding ShowActionPanel}"">
            <Rock:Icon.CommandParameter>
                <Rock:ShowActionPanelParameters Title=""Group Actions"" CancelTitle=""Cancel"">
                    <Rock:ActionPanelButton Title=""Edit Group"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupEditPage }}?GroupGuid={{ Group.Guid }}"" />
                </Rock:ShowActionPanelParameters>
            </Rock:Icon.CommandParameter>
        </Rock:Icon>
        {% endif %}
    </StackLayout>

    <BoxView Color=""#ccc"" HeightRequest=""1"" Margin=""0, 30, 0, 10"" />

    <!-- Handle Group Attributes -->
    {% if VisibleAttributes != empty %}
        <Rock:ResponsiveLayout>
        {% for attribute in VisibleAttributes %}
            <Rock:ResponsiveColumn ExtraSmall=""6"">
                <Rock:FieldContainer>
                    <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
                </Rock:FieldContainer>
            </Rock:ResponsiveColumn>
        </Rock:ResponsiveLayout>
        {% endfor %}
    {% endif %}

    <!-- Handle displaying of leaders -->
    {% if ShowLeaderList == true %}
        <Label Text=""Leaders"" StyleClass=""field-title"" Margin=""0, 40, 0, 0"" />
        <Grid RowSpacing=""0"" ColumnSpacing=""20"">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width=""Auto"" />
                <ColumnDefinition Width=""*"" />
            </Grid.ColumnDefinitions>
        {% assign row = 0 %}
        {% assign members = Group.Members | OrderBy:'Person.FullName' %}
        {% for member in members %}
            {% if member.GroupRole.IsLeader == false %}{% continue %}{% endif %}
            <Label Grid.Row=""{{ row }}"" Grid.Column=""0"" Text=""{{ member.Person.FullName }}"" />
            <Label Grid.Row=""{{ row }}"" Grid.Column=""1"" Text=""{{ member.GroupRole.Name }}"" />
            {% assign row = row | Plus:1 %}
        {% endfor %}
        </Grid>
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "F093A516-6D95-429E-8EEB-1DFB0303DF71",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    <Label StyleClass=""h1"" Text=""{{ Member.Group.Name | Escape }} Group"" />
    <Label Text=""{{ Member.Group.Members | Size }} members"" LineHeight=""0.8"" />

    <StackLayout Orientation=""Horizontal"" Spacing=""20"" Margin=""0, 20, 0, 40"">
            <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{% if Member.Person.PhotoId != null %}{{ Member.Person.PhotoUrl | Append:'&width=120' | Escape }}{% else %}{{ Member.Person.PhotoUrl | Escape }}{% endif %}"" WidthRequest=""80"">
                <Rock:CircleTransformation />
            </Rock:Image>
            <StackLayout Spacing=""0"" VerticalOptions=""Center"">
                <Label FontSize=""20"" FontAttributes=""Bold"" Text=""{{ Member.Person.FullName | Escape }}"" />
                {% if Member.Person.BirthDate != null %}
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Age: {{ Member.Person.AgePrecise | Floor }}"" />
                    <Label LineHeight=""0.85"" TextColor=""#888"" Text=""Birthdate: {{ Member.Person.BirthDate | Date:'MMMM' }} {{ Member.Person.BirthDate | Date:'d' | NumberToOrdinal }}"" />
                {% endif %}
            </StackLayout>
    </StackLayout>

    <!-- Handle Member Attributes -->
    {% if VisibleAttributes != empty %}
        {% for attribute in VisibleAttributes %}
        <Rock:FieldContainer Margin=""0, 0, 0, {% if forloop.last %}40{% else %}10{% endif %}"">
            <Rock:Literal Label=""{{ attribute.Name | Escape }}"" Text=""{{ attribute.FormattedValue }}"" />
        </Rock:FieldContainer>
        {% endfor %}
    {% endif %}

    <!-- Contact options -->
    {% assign hasContact = false %}
    {% if Member.Person.Email != '' %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ Member.Person.Email | Escape }}"" />
                <Label Text=""Email"" />
            </StackLayout>
            <Rock:Icon IconClass=""Envelope"" FontSize=""36"" Command=""{Binding SendEmail}"" CommandParameter=""{{ Member.Person.Email | Escape }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Mobile' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Mobile',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Mobile"" />
            </StackLayout>
            <Rock:Icon IconClass=""Comment"" FontSize=""36"" Command=""{Binding SendSms}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% assign phoneNumber = Member.Person | PhoneNumber:'Home' %}
    {% assign phoneNumberLong = Member.Person | PhoneNumber:'Home',true %}
    {% if phoneNumber != '' and phoneNumber != null %}
        {% assign hasContact = true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        <StackLayout Orientation=""Horizontal"" Padding=""12"" Spacing=""20"">
            <StackLayout Spacing=""0"" VerticalOptions=""Center"" HorizontalOptions=""FillAndExpand"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ phoneNumber }}"" />
                <Label Text=""Home"" />
            </StackLayout>
            <Rock:Icon IconClass=""Phone"" FontSize=""36"" Command=""{Binding CallPhoneNumber}"" CommandParameter=""{{ phoneNumberLong }}"" VerticalOptions=""Center"" />
        </StackLayout>
    {% endif %}

    {% if hasContact == true %}
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
    {% endif %}

    {% if GroupMemberEditPage != '' %}
        <Button StyleClass=""btn,btn-primary"" Text=""Edit"" Margin=""0, 40, 0, 0"" WidthRequest=""200"" HorizontalOptions=""Center"" Command=""{Binding PushPage}"" CommandParameter=""{{ GroupMemberEditPage }}?GroupMemberGuid={{ Member.Guid }}"" />
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "674CF1E3-561C-430D-B4A8-39957AC1BCF1",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_MEMBER_LIST,
                "Default",
                @"<StackLayout>
    <Label StyleClass=""h1"" Text=""{{ Title | Escape }}"" />
    <Label Text=""{{ Members | Size }} members"" LineHeight=""0.8"" />

    {% if Members != empty %}
    <StackLayout Spacing=""0"" Margin=""0,20,0,0"">
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        {% for member in Members %}
        <StackLayout Orientation=""Horizontal"" HeightRequest=""60"">
            <StackLayout.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding PushPage}"" CommandParameter=""{{ DetailPage }}?GroupMemberGuid={{ member.Guid }}"" />
            </StackLayout.GestureRecognizers>
            {%- if member.PhotoId != null -%}
                <Rock:Image Source=""{{ member.PhotoUrl | Append:'&width=60' | Escape }}"" WidthRequest=""60"" BackgroundColor=""#ccc"" />
            {%- else -%}
                <Rock:Image Source=""{{ member.PhotoUrl | Escape }}"" WidthRequest=""60"" BackgroundColor=""#ccc"" />
            {%- endif -%}

            <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""Center"">
                <Label FontSize=""16"" FontAttributes=""Bold"" Text=""{{ member.FullName | Escape }}"" />
                <Label LineHeight=""0.85"" FontSize=""12"" TextColor=""#888"" Text=""{{ member.GroupRole | Escape }}"" />
            </StackLayout>
            <Rock:Icon IconClass=""chevron-right"" Margin=""0,0,20,0"" VerticalOptions=""Center"" />
        </StackLayout>
        <BoxView Color=""#ccc"" HeightRequest=""1"" />
        {% endfor %}
    </StackLayout>
    {% endif %}
</StackLayout>",
                STANDARD_ICON_SVG,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// JE: Migrationfor Mobile - Added Notes Channel
        /// </summary>
        private void AddMobileNotesChannel()
        {
            Sql( @"
-- Add new Content Channel Type for Message Notes
IF NOT EXISTS( SELECT * FROM [ContentChannelType] WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb' )
BEGIN
    INSERT INTO [ContentChannelType]
           ([IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority], [IncludeTime], [DisableContentField], [DisableStatus], [ShowInChannelList])
     VALUES
           (1, 'Podcast Message Notes', 1, '48951e97-0e45-4494-b87c-4eb9fca067eb', 1, 0, 0, 1, 1)
END

-- Add new Content Channel for Message Notes
IF NOT EXISTS( SELECT * FROM [ContentChannel] WHERE [Guid] = '888ef5ea-e075-4a56-a61c-13a6dad93d6f' )
BEGIN
    DECLARE @NotesContentChannelTypeId int = (SELECT TOP 1 [Id] FROM [ContentChannelType] WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb')
    DECLARE @NoteStructuredToolValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '31c63fb9-1365-4eef-851d-8ab9a188a06c')

	IF (@NotesContentChannelTypeId IS NOT NULL AND @NoteStructuredToolValueId IS NOT NULL)
	BEGIN
		INSERT INTO [dbo].[ContentChannel]
			([ContentChannelTypeId], [Name], [Description], [IconCssClass], [RequiresApproval], [EnableRss], [ChannelUrl], [ItemUrl], [TimeToLive], [Guid], [ContentControlType], [RootImageDirectory], [ItemsManuallyOrdered], [ChildItemsManuallyOrdered], [IsIndexEnabled], [IsTaggingEnabled], [ItemTagCategoryId], [IsStructuredContent], [StructuredContentToolValueId])
		VALUES
			( @NotesContentChannelTypeId, 'Message Notes', 'Notes for messages that are being podcasted.', 'fa fa-sticky-note', 0, 0, '', '', 0, '888ef5ea-e075-4a56-a61c-13a6dad93d6f', 0, '', 0, 0, 0, 0, null, 1, @NoteStructuredToolValueId)
    END
END

-- Add new note content channel as a child to the existing message notes content channel
DECLARE @MessageContentChannelId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '0a63a427-e6b5-2284-45b3-789b293c02ea')
DECLARE @MessageNotesContentChannelId int = (SELECT TOP 1 [Id] FROM [ContentChannel] WHERE [Guid] = '888ef5ea-e075-4a56-a61c-13a6dad93d6f')


IF (@MessageContentChannelId IS NOT NULL AND @MessageNotesContentChannelId IS NOT NULL)
BEGIN
	IF NOT EXISTS (
			SELECT *
			FROM [ContentChannelAssociation]
			WHERE [ContentChannelId] = @MessageContentChannelId AND [ChildContentChannelId] = @MessageNotesContentChannelId
			)
	BEGIN
		INSERT INTO [ContentChannelAssociation] ([ContentChannelId], [ChildContentChannelId])
		VALUES (@MessageContentChannelId, @MessageNotesContentChannelId)
	END
END

-- Remove the word Podcast from existing channels
UPDATE [ContentChannel]
SET [Name] = 'Messages'
WHERE [Name] = 'Podcast Message' AND [Guid] = '0a63a427-e6b5-2284-45b3-789b293c02ea'

UPDATE [ContentChannel]
SET [Name] = 'Message Series'
WHERE [Name] = 'Podcast Series' AND [Guid] = 'e2c598f1-d299-1baa-4873-8b679e3c1998'" );
        }

        /// <summary>
        /// SK: Enable Label Reprinting by Default in the Check-in System's Welcome.ascx Block
        /// </summary>
        private void EnableLabelReprintingByDefault()
        {

            Sql( $@"
                    DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '563FF060-D7FD-4704-A4AA-8F4B6D4F75CE')

                    IF @AttributeId IS NOT NULL
                    BEGIN
                        -- Update the primary attribute
                        UPDATE [Attribute] SET [DefaultValue] = 'True' 
                        WHERE [Id] = @AttributeId

                        UPDATE [AttributeValue] SET [Value] = 'True' 
                        WHERE [AttributeId] = @AttributeId
                    END" );
        }

        /// <summary>
        /// GJ: Added new Message Note defined value for Structured Content
        /// </summary>
        private void AddStructuredContentMessageNote()
        {
            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Default",
                "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     }, code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }",
                "09B25845-B879-4E69-87E9-003F9380B8DD",
                false);

            RockMigrationHelper.UpdateDefinedValue("E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Message Notes",
                "{     header: {     class: Header,     inlineToolbar: ['link'],     config: {         placeholder: 'Header'     },     shortcut: 'CMD+SHIFT+H'     },     image: {     class: SimpleImage,     inlineToolbar: ['link'],     },     list: {     class: List,     inlineToolbar: true,     shortcut: 'CMD+SHIFT+L'     },     checklist: {     class: Checklist,     inlineToolbar: true,     },     quote: {     class: Quote,     inlineToolbar: true,     config: {         quotePlaceholder: 'Enter a quote',         captionPlaceholder: 'Quote\'s author',     },     shortcut: 'CMD+SHIFT+O'     },     warning: Warning,     marker: {     class:  Marker,     shortcut: 'CMD+SHIFT+M'     },     fillin: {     class:  FillIn,     shortcut: 'CMD+SHIFT+F'     },     code: {     class:  CodeTool,     shortcut: 'CMD+SHIFT+C'     },     note: {     class:  NoteTool,     shortcut: 'CMD+SHIFT+N'     },     delimiter: Delimiter,     inlineCode: {     class: InlineCode,     shortcut: 'CMD+SHIFT+C'     },     linkTool: LinkTool,     embed: Embed,     table: {     class: Table,     inlineToolbar: true,     shortcut: 'CMD+ALT+T'     } }",
                "31C63FB9-1365-4EEF-851D-8AB9A188A06C",
                false);
        }

        /// <summary>
        /// SK: Add block setting to the Attendance Self Entry block.
        /// </summary>
        private void AttendanceSelfEntryBlockSetting()
        {
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals Younger Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Hide Individuals Younger Than", "HideIndividualsYoungerThan", "Hide Individuals Younger Than", @"The age that should be used as the cut-off for displaying on the attendance list. The value of 14 will hide individuals younger than 14. Individuals without an age will always be shown. Defaults to blank.", 25, @"", "1612C057-122C-4110-9E06-2750828653FD" );
            // Attrib for BlockType: Attendance Self Entry:Hide Individuals In Grade Less Than
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Hide Individuals In Grade Less Than", "HideIndividualsInGradeLessThan", "Hide Individuals In Grade Less Than", @"Individuals in grades lower than this value will not be show on the attendance list. Defaults to empty (not set).", 26, @"", "8F12D1F7-0855-43E5-9D6D-F5F8F3F8F13F" );  
        }

        /// <summary>
        /// MB: Add DefaultToZero and MetricDateDeterminedBy attributes to Service Metric Block Type.
        /// </summary>
        private void BlockTypeServiceMetricsDefaultToZeroUp()
        {
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", SystemGuid.FieldType.BOOLEAN, "Insert 0 for Blank Items", "DefaultToZero", "", @"If enabled, a zero will be added to any metrics that are left empty when entering data.", 5, @"false", "4CF6300D-F17F-4F45-AFB3-8DA09DE5759D", false );
        }

        /// <summary>
        /// MB: Add DefaultToZero and MetricDateDeterminedBy attributes to Service Metric Block Type.
        /// </summary>
        private void BlockTypeServiceMetricsMetricDateDeterminedByUp()
        {
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", SystemGuid.FieldType.SINGLE_SELECT, "Metric Date Determined By", "MetricDateDeterminedBy", "", @"This setting determines what date to use when entering the metric. 'Sunday Date' would use the selected Sunday date. 'Day from Schedule' will use the first day configured from the selected schedule.", 6, @"0", "4442352F-9678-4079-B6DB-37D69BAE8A11", false );
        }

        /// <summary>
        /// MB: Add DefaultToZero and MetricDateDeterminedBy attributes to Service Metric Block Type.
        /// </summary>
        private void BlockTypeServiceMetricsDefaultToZeroDown()
        {
            RockMigrationHelper.DeleteAttribute( "4CF6300D-F17F-4F45-AFB3-8DA09DE5759D" );
        }

        /// <summary>
        /// MB: Add DefaultToZero and MetricDateDeterminedBy attributes to Service Metric Block Type.
        /// </summary>
        private void BlockTypeServiceMetricsMetricDateDeterminedByDown()
        {
            RockMigrationHelper.DeleteAttribute( "4442352F-9678-4079-B6DB-37D69BAE8A11" );
        }

        /// <summary>
        /// SK: Update Existing ServiceJob HistoryCount to 500
        /// </summary>
        private void UpdateServiceJobHistoryCount()
        {
            Sql( "UPDATE [ServiceJob] SET [HistoryCount]=500 WHERE [HistoryCount]=0 " );
        }
    }
}
