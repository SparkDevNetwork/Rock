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
    /// Adds Group RSVP pages, routes and blocks to Rock core.
    /// </summary>
    public partial class AddRsvpPages : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Decline Reasons defined type/values.
            RockMigrationHelper.AddDefinedType( "Group", "Group RSVP Decline Reason", "Possible reasons someone might decline an invitation.", SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON );
            // Nick:  This list probably needs to be updated with appropriate default values.
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON, "Illness", "", SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_ILLNESS, true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON, "Vacation", "", SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_VACATION, true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON, "Schedule Conflict", "", SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_SCHEDULE_CONFLICT, true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON, "No Childcare Available", "", SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_CHILDCARE, true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON, "Other", "", SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_OTHER, true );

            // Add Group RSVP List Page.
            RockMigrationHelper.AddPage( true, SystemGuid.Page.GROUP_VIEWER, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Group RSVP List", "", SystemGuid.Page.GROUP_RSVP_LIST );
            // Add RSVP List Block to RSVP List Page.
            RockMigrationHelper.UpdateBlockType( "RSVP List", "Lists RSVP Occurrences.", "~/Blocks/Rsvp/RsvpList.ascx", "RSVP", SystemGuid.BlockType.RSVP_LIST );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.GROUP_RSVP_LIST.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.RSVP_LIST.AsGuid(), "RSVP List", "Main", @"", @"", 0, SystemGuid.Block.RSVP_LIST );

            // Add Group RSVP Detail Page.
            RockMigrationHelper.AddPage( true, SystemGuid.Page.GROUP_RSVP_LIST, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "Group RSVP Detail", "", SystemGuid.Page.GROUP_RSVP_DETAIL );
            // Add RSVP Detail Block to RSVP Detail Page.
            RockMigrationHelper.UpdateBlockType( "RSVP Detail", "Shows detailed RSVP information for a specific occurrence and allows editing RSVP details.", "~/Blocks/Rsvp/RsvpDetail.ascx", "RSVP", SystemGuid.BlockType.RSVP_DETAIL );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.GROUP_RSVP_DETAIL.AsGuid(), null, SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), SystemGuid.BlockType.RSVP_DETAIL.AsGuid(), "RSVP Detail", "Main", @"", @"", 0, SystemGuid.Block.RSVP_DETAIL );

            // Add Group RSVP Response Page (External Site).
            RockMigrationHelper.AddPage( true, SystemGuid.Page.SUPPORT_PAGES_EXTERNAL_SITE, SystemGuid.Layout.FULL_WIDTH, "Group RSVP Response", "", SystemGuid.Page.GROUP_RSVP_RESPONSE );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.GROUP_RSVP_RESPONSE, "RSVP", SystemGuid.PageRoute.RSVP );
            // Add RSVP Response Block to RSVP Detail Page.
            RockMigrationHelper.UpdateBlockType( "RSVP Response", "Allows invited people to RSVP for one or more Attendance Occurrences.", "~/Blocks/Rsvp/RsvpResponse.ascx", "RSVP", SystemGuid.BlockType.RSVP_RESPONSE );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.GROUP_RSVP_RESPONSE.AsGuid(), null, SystemGuid.Site.EXTERNAL_SITE.AsGuid(), SystemGuid.BlockType.RSVP_RESPONSE.AsGuid(), "RSVP Response", "Main", @"", @"", 0, SystemGuid.Block.RSVP_RESPONSE );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_ILLNESS );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_VACATION );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_SCHEDULE_CONFLICT );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_CHILDCARE );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.GROUP_RSVP_DECLINE_REASON_OTHER );
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.GROUP_RSVP_DECLINE_REASON );

            RockMigrationHelper.DeleteBlock( SystemGuid.Block.RSVP_RESPONSE );
            RockMigrationHelper.DeletePageRoute( SystemGuid.PageRoute.RSVP );
            RockMigrationHelper.DeletePage( SystemGuid.Page.GROUP_RSVP_RESPONSE );

            RockMigrationHelper.DeleteBlock( SystemGuid.Block.RSVP_DETAIL );
            RockMigrationHelper.DeletePage( SystemGuid.Page.GROUP_RSVP_DETAIL );

            RockMigrationHelper.DeleteBlock( SystemGuid.Block.RSVP_LIST );
            RockMigrationHelper.DeletePage( SystemGuid.Page.GROUP_RSVP_LIST );
        }
    }
}
