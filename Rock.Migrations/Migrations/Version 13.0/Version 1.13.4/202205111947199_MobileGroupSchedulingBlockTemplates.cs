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
    public partial class MobileGroupSchedulingBlockTemplates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference",
                "Group Schedule Preference",
                "Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE );

            RockMigrationHelper.UpdateMobileBlockType( "Schedule Preferences",
                "Allows an individual to update their schedule preferences.",
                "Rock.Blocks.Types.Mobile.Groups.GroupSchedulePreference",
                "Mobile > Groups",
                Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Groups > Group Schedule Preference > Landing Page",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "C3A98DBE-E977-499C-B823-0B3676731E48",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE,
                "Default",
                @"<StackLayout>
<StackLayout Padding=""16"" HorizontalOptions=""Center"" VerticalOptions=""Center"">
    
    <Label StyleClass=""h3"" 
        Text=""Serving Areas""
        />

    <Label StyleClass=""""
        Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    {% if SchedulingGroupList == empty %}
        <Rock:NotificationBox Text=""You are currently not enrolled in any groups with scheduling options. Contact a church administrator if you are interested!"" />
    {% endif %}
    
    {% for group in SchedulingGroupList %}
    <Grid> 
        <Button StyleClass=""btn,btn-primary,group-selection-button"" Text=""{{ group.Name }}"" HorizontalOptions=""FillAndExpand""
            Command=""{Binding SchedulePreference.PushToPreferencePage}"" 
            CommandParameter=""{{ group.Guid }}"" /> 
    </Grid> 
    {% endfor %} 

</StackLayout>
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupScheduleToolbox",
                "Group Schedule Toolbox",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleToolbox, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX );

            RockMigrationHelper.UpdateMobileBlockType( "Schedule Toolbox",
                "Allows an individual to manage their serving requests.",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleToolbox",
                "Mobile > Groups",
                Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Groups > Group Schedule Toolbox > Decline Modal",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Groups > Group Schedule Toolbox",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX,
                "Default",
                @"<StackLayout StyleClass=""schedule-toolbox"">
        {% if ScheduleList == empty %}
        
        <Rock:NotificationBox Text=""You currently have no pending or confirmed schedules. Reach out to a church administrator if you are interested!"" />
    
        {% endif %}

        {% for attendance in ScheduleList %}

        {% assign status = attendance.GroupScheduleType %}
        
        <Grid ColumnSpacing=""12"" Padding=""8"">
            <StackLayout Spacing=""4"" Grid.Column=""0""  StyleClass=""schedule-toolbox-container"">
                <Label  StyleClass=""detail-title""
                        Text=""{{ attendance.OccurrenceStartDate | Date:'sd' }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />

            <Label StyleClass=""detail""
                        Text=""{{ attendance.Group.Name | Escape }} - {{ attendance.Location.Name }}""
                        MaxLines=""2""
                        LineBreakMode=""TailTruncation"" />

            <Label StyleClass=""detail""
                        Text=""{{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
            </StackLayout>

        {% if status == 'Upcoming' %}
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-confirmations-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">

            <Label StyleClass=""confirmed-text""
                    HorizontalOptions=""Start"" VerticalOptions=""Center""
                    Padding=""8""
                    Text=""Confirmed"" />
            <Rock:Icon IconClass=""Ellipsis-v"" 
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                            CancelTitle=""Exit"">
                            <Rock:ActionPanelButton Title=""Cancel Confirmation"" 
                                Command=""{Binding ScheduleToolbox.SetPending}"" 
                                CommandParameter=""{{ attendance.Guid }}"" />
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
            </StackLayout>
        {% endif %}

        {% if status == 'Unavailable' %}
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-confirmations-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">

            <Label StyleClass=""declined-text""
                    HorizontalOptions=""Start"" VerticalOptions=""Center""
                    Padding=""8""
                    Text=""Declined"" />
            <Rock:Icon IconClass=""Ellipsis-v"" 
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | | Date:'sd' }} - {{ attendance.Schedule.NextStartDateTime | Date:'dddd h:mmtt' }}"" 
                            CancelTitle=""Exit"">
                            <Rock:ActionPanelButton Title=""Cancel Declination"" 
                                Command=""{Binding ScheduleToolbox.SetPending}"" 
                                CommandParameter=""{{ attendance.Guid }}"" />
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
            </StackLayout>
        {% endif %}

        {% if status == 'Pending' %}
                
            <StackLayout Spacing=""8"" Grid.Column=""1"" StyleClass=""schedule-toolbox-pending-container""
                    VerticalOptions=""Center""
                    HorizontalOptions=""End""
                    Orientation=""Horizontal"">
                    <Button StyleClass=""btn,btn-success,accept-button"" 
                        Text=""Accept""
                        Command=""{Binding ScheduleToolbox.ConfirmAttend}""
                        CommandParameter=""{{ attendance.Guid }}""
                        HorizontalOptions=""Fill"" />

                    <Button StyleClass=""btn,btn-outline-danger,decline-button"" 
                        Text=""Decline""              
                        Command=""{Binding ScheduleToolbox.PushScheduleConfirmModal}""
                        CommandParameter=""{{ attendance.Guid }}""
                        HorizontalOptions=""Fill""  />
            </StackLayout>

        {% endif %}
        </Grid>
        <Rock:Divider />
        {% endfor %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "92D39913-7D69-4B73-8FF9-72AC161BE381",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL,
                "Default",
                @"<StackLayout Spacing=""0"">
    <Frame HasShadow=""False"">
        <Grid RowDefinitions=""1*, 1*, 1*"">
            <Label StyleClass=""h2, schedule-toolbox-confirm-title""
                Text=""Can't make it, {{ Attendance.PersonAlias.Person.NickName }}? "" />
            <Label StyleClass=""schedule-toolbox-confirm-title"" Grid.Row=""1""
                Text=""Thanks for letting us know. We’ll try to schedule another person for {{ Attendance.StartDateTime | Date:'dddd @ h:mmtt'}}."" />
        </Grid>
    </Frame>
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupScheduleUnavailability",
                "Group Schedule Unavailability",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleUnavailability, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY );

            RockMigrationHelper.UpdateMobileBlockType( "Schedule Unavailability",
                "Allows an individual to update the dates in which they are unavailable.",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleUnavailability",
                "Mobile > Groups",
                Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Groups > Group Schedule Unavailability",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "1A699B18-AB29-4CD5-BC02-AF55159D5DA6",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY,
                "Default",
                @"<StackLayout StyleClass=""schedule-toolbox"">
    {% if ScheduleExclusionsList == empty %}
        
    <Rock:NotificationBox Text=""You currently have no blackout dates inputted."" />
    
    {% endif %}
    
    {% for attendance in ScheduleExclusionsList %}

        {% if attendance.Group %}
            {% assign GroupName = attendance.Group.Name %}
        {% else %} 
            {% assign GroupName = ""All Groups"" %}
        {% endif %}
        
        <Grid ColumnSpacing=""12"" Padding=""4"" Margin=""8"">
            <StackLayout Spacing=""4"" Grid.Column=""0""  StyleClass=""schedule-toolbox-container"">
                <Label  StyleClass=""detail-title""
                        Text=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.OccurrenceEndDate | Date:'sd' }} ""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />

                <Label StyleClass=""detail""
                        Text=""{{ attendance.PersonAlias.Person.FullName }}""
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />

                <Label StyleClass=""detail""
                        Text=""{{ GroupName }}"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />

                {% if attendance.Title %}
                <Label StyleClass=""detail""
                        Text=""&quot;{{ attendance.Title }}&quot;"" 
                        MaxLines=""1""
                        LineBreakMode=""TailTruncation"" />
                {% endif %}
            </StackLayout>

            <Rock:Icon IconClass=""times"" 
                    StyleClass=""""
                    HorizontalOptions=""End""
                    VerticalOptions=""Center""
                    Padding=""8""
                    Command=""{Binding ShowActionPanel}"">
                    <Rock:Icon.CommandParameter>
                        <Rock:ShowActionPanelParameters 
                            Title=""{{ attendance.OccurrenceStartDate | Date:'sd' }} - {{ attendance.OccurrenceEndDate | Date:'sd' }}"" 
                            CancelTitle=""Exit"">

                            <Rock:ShowActionPanelParameters.DestructiveButton>
                                <Rock:ActionPanelButton Title=""Cancel My Request"" 
                                    Command=""{Binding ScheduleUnavailability.DeleteScheduledUnavailability}""
                                    CommandParameter=""{{ attendance.Guid }}""
                                    />
                            </Rock:ShowActionPanelParameters.DestructiveButton>
                        </Rock:ShowActionPanelParameters>
                    </Rock:Icon.CommandParameter>
                </Rock:Icon>
        </Grid>
        <Rock:Divider />

        {% endfor %}
    <Button Margin=""8"" HorizontalOptions=""End"" StyleClass=""btn,btn-primary,schedule-unavailabilty-button""
                  Text=""Schedule Unavailability""
                  Command=""{Binding ScheduleUnavailability.PushScheduleUnavailabilityModal}"">
    </Button>
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Groups.GroupScheduleSignUp",
                "Group Schedule Sign Up",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleSignUp, Rock, Version=1.13.0.23, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP );

            RockMigrationHelper.UpdateMobileBlockType( "Schedule Sign Up",
                "Allows an individual to sign up for additional serving dates.",
                "Rock.Blocks.Types.Mobile.Groups.GroupScheduleSignUp",
                "Mobile > Groups",
                Rock.SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP );

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Groups > Group Schedule Sign Up > Landing Page",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "C4BFED3A-C2A1-4A68-A646-44C3B499C75A",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE,
                "Default",
                @"<StackLayout>
<StackLayout Padding=""16"" HorizontalOptions=""Center"" VerticalOptions=""Center"">
    <Label StyleClass=""h3"" 
        Text=""Serving Areas""
        />
    <Label StyleClass=""""
        Text=""You are registered to serve in the following areas. Please select an area to update your scheduling preferences."" />
    {% if SchedulingGroupList == empty %}
        <Rock:NotificationBox Text=""You are currently not enrolled in any groups with scheduling options. Contact a church administrator if you are interested!"" />
    {% endif %}
    
    {% for group in SchedulingGroupList %}
    <Grid> 
        <Button StyleClass=""btn,btn-primary,group-selection-button"" Text=""{{ group.Name }}"" HorizontalOptions=""FillAndExpand""

            Command=""{Binding ScheduleSignup.PushToSignupPage}""
            CommandParameter=""{{ group.Guid }}"" />
    </Grid> 
    {% endfor %} 

</StackLayout>
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );

            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "C3A98DBE-E977-499C-B823-0B3676731E48" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_PREFERENCE_LANDING_PAGE );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_PREFERENCE );

            RockMigrationHelper.DeleteTemplateBlockTemplate( "92D39913-7D69-4B73-8FF9-72AC161BE381" );
            RockMigrationHelper.DeleteTemplateBlockTemplate( "CD2629E5-8EB0-4D52-ACAB-8EDF9AF84814" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_TOOLBOX_DECLINE_MODAL );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_TOOLBOX );

            RockMigrationHelper.DeleteTemplateBlockTemplate( "1A699B18-AB29-4CD5-BC02-AF55159D5DA6" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_UNAVAILABILITY );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_UNAVAILABILITY );

            RockMigrationHelper.DeleteTemplateBlockTemplate( "C4BFED3A-C2A1-4A68-A646-44C3B499C75A" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_GROUP_SCHEDULE_SIGNUP_LANDING_PAGE );
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP );
            RockMigrationHelper.DeleteEntityType( SystemGuid.EntityType.MOBILE_GROUPS_GROUP_SCHEDULE_SIGNUP );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
        Sql( @"
        UPDATE [dbo].[__MigrationHistory]
        SET [Model] = 0x
        WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
