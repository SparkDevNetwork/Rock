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
    public partial class Rollup_0114 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ChangeExternalLinksToHttps();
            FixMobileBlockTypeCategoriesUp();
            AddGroupTypeAttributePersonSelectLavaTemplateButtonUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddGroupTypeAttributePersonSelectLavaTemplateButtonDown();
        }

        /// <summary>
        /// ED: Change External Application Links to HTTPS
        /// </summary>
        private void ChangeExternalLinksToHttps()
        {
            Sql( @"
                DECLARE @DownloadUrlAttributeId INT = (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')
                UPDATE [dbo].[AttributeValue]
                SET [Value] = REPLACE([Value],'http:','https:')
                WHERE [AttributeId] = @DownloadUrlAttributeId" );
        }

        /// <summary>
        /// DH: Fix Mobile Block Type Categories
        /// </summary>
        private void FixMobileBlockTypeCategoriesUp()
        {
            RockMigrationHelper.UpdateMobileBlockType( "Calendar Event List", "Displays a list of events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarEventList", "Mobile > Events", "A9149623-6A82-4F25-8F4D-0961557BE78C" );
            RockMigrationHelper.UpdateMobileBlockType( "Calendar View", "Views events from a calendar.", "Rock.Blocks.Types.Mobile.Events.CalendarView", "Mobile > Events", "14B447B3-6117-4142-92E7-E3F289106140" );
            RockMigrationHelper.UpdateMobileBlockType( "Communication List Subscribe", "Allows the user to subscribe or unsubscribe from specific communication lists.", "Rock.Blocks.Types.Mobile.Events.CommunicationListSubscribe", "Mobile > Communication", "D0C51784-71ED-46F3-86AB-972148B78BE8" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Attendance Entry", "Allows the user to mark attendance for a group.", "Rock.Blocks.Types.Mobile.Groups.GroupAttendanceEntry", "Mobile > Groups", "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Edit", "Edits the basic settings of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupEdit", "Mobile > Groups", "FEC66374-E38F-4651-BAA6-AC658409D9BD" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member Edit", "Edits a member of a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberEdit", "Mobile > Groups", "514B533A-8970-4628-A4C8-35388CD869BC" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member List", "Allows the user to view a list of members in a group.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberList", "Mobile > Groups", "5A6D2ADB-03A7-4B55-8EAA-26A37116BFF1" );
            RockMigrationHelper.UpdateMobileBlockType( "Group Member View", "Allows the user to view the details about a specific group member.", "Rock.Blocks.Types.Mobile.Groups.GroupMemberView", "Mobile > Groups", "6B3C23EA-A1C2-46FA-9F04-5B0BD004ED8B" );
            RockMigrationHelper.UpdateMobileBlockType( "Group View", "Allows the user to view the details about a group.", "Rock.Blocks.Types.Mobile.Groups.GroupView", "Mobile > Groups", "3F34AE03-9378-4363-A232-0318139C3BD3" );
            RockMigrationHelper.UpdateMobileBlockType( "Hero", "Displays an image with text overlay on the page.", "Rock.Blocks.Types.Mobile.Cms.Hero", "Mobile > Cms", "A8597994-BD47-4A15-8BB1-4B508977665F" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Request Details", "Edits an existing prayer request or creates a new one.", "Rock.Blocks.Types.Mobile.Prayer.PrayerRequestDetails", "Mobile > Prayer", "EBB91B46-292E-4784-9E37-38781C714008" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Session", "Allows the user to read through and pray for prayer requests.", "Rock.Blocks.Types.Mobile.Events.PrayerSession", "Mobile > Prayer", "420DEA5F-9ABC-4E59-A9BD-DCA972657B84" );
            RockMigrationHelper.UpdateMobileBlockType( "Prayer Session Setup", "Displays a page to configure and prepare a prayer session.", "Rock.Blocks.Types.Mobile.Events.PrayerSessionSetup", "Mobile > Prayer", "4A3B0D13-FC32-4354-A224-9D450F860BE9" );
        }

        private void AddGroupTypeAttributePersonSelectLavaTemplateButtonUp()
        {
            // Entity: Rock.Model.GroupType Attribute: Person Select Additional Information Template
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "GroupTypePurposeValueId", "142", "Person Select Additional Information Template", "Person Select Template", @"The lava template used to append additional information to each person button on the Person Select & Multi-Person Select Check-in blocks.", 1040, @"", "55ED9569-8ED8-45A4-AD54-E23F968494EB", "core_checkin_PersonSelectAdditionalInformationLavaTemplate");

            // Qualifier for attribute: core_PersonSelectAdditionalInformationTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "55ED9569-8ED8-45A4-AD54-E23F968494EB", "editorHeight", @"", "3AF279D6-0BED-4646-9484-63A4A65ECCE5");
            // Qualifier for attribute: core_PersonSelectAdditionalInformationTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "55ED9569-8ED8-45A4-AD54-E23F968494EB", "editorMode", @"3", "459B5BCD-9162-4886-A376-1EF8A08D1BE3");
            // Qualifier for attribute: core_PersonSelectAdditionalInformationTemplate
            RockMigrationHelper.UpdateAttributeQualifier( "55ED9569-8ED8-45A4-AD54-E23F968494EB", "editorTheme", @"0", "7B9B5D2C-E704-4C89-9894-D5AF103B07DD");
        }

        private void AddGroupTypeAttributePersonSelectLavaTemplateButtonDown()
        {
            RockMigrationHelper.DeleteAttribute( "55ED9569-8ED8-45A4-AD54-E23F968494EB"); // Rock.Model.GroupType: Person Select Additional Information Template
        }
    }
}
