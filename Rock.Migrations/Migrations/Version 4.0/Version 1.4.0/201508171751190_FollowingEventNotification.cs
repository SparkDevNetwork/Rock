// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class FollowingEventNotification : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FollowingEventNotification",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FollowingEventTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        LastNotified = c.DateTime(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.FollowingEventType", t => t.FollowingEventTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.FollowingEventTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);

            // Anniversary event
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonAnniversary", "17DFDE21-0C1E-426F-8516-4BBA9ED28385", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonAnniversary", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "Lead Days", "The number of days prior to birthday that notification should be sent.", 0, "5", "4E9F3547-5EF8-471E-A537-25846F26A00F" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonAnniversary", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "Nth Year", "Only be notified for anniversaries that are a multiple of this number (i.e. a value of 5 will notify you on the person's 5th, 10th, 15th, etc. anniversary).",
                1, "5", "73A36175-E82E-4315-92FD-A58C1499232E" );

            // Baptized event
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonBaptized", "A156E5A0-FEE8-4730-8AC7-B3239B35F9F2", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonBaptized", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "Max Days Back", "Maximum number of days back to consider", 0, "30", "F68163EC-73E8-4785-A7E9-A11CAD464D05" );
            
            // First Attended Group Type event
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonFirstAttendedGroupType", "F74232DD-62B6-4F04-BF5F-9E5CF159CD8B", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonFirstAttendedGroupType", "18E29E23-B43B-4CF7-AE41-C85672C09F50",
                "Group Type", "The group type to evaluate if person has just attended for the first time", 0, "", "5FB3A5E7-519A-4FE0-BCF4-237D94848950" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonFirstAttendedGroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "Max Days Back", "Maximum number of days back to consider", 1, "30", "DBB890DA-BD04-4841-8D90-1F5BF8EAD805" );

            // First Joined Group Type event
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonFirstJoinedGroupType", "4CDE3741-D284-4B32-9F8A-DFB63C600594", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonFirstJoinedGroupType", "18E29E23-B43B-4CF7-AE41-C85672C09F50",
                "Group Type", "The group type to evaluate if person has just joined for the first time", 0, "", "BC421FBC-3D0E-4C87-A533-EF678D156516" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonFirstJoinedGroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF",
                "MaxDaysBack", "Maximum number of days back to consider", 1, "30", "3C024CDE-3481-410E-8392-566B4F78CC4E" );

            // In Group Together suggestion
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "18E29E23-B43B-4CF7-AE41-C85672C09F50",
                "Group Type", "The type of group.", 
                0, "", "AB754108-EB85-422B-A898-90C47495174A" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "F4399CEF-827B-48B2-A735-F7806FCFE8E8",
                "Group (optional)", "A specific group to evaluate (Make sure to select group with same group type as above).",
                1, "", "A2E16D4F-C86F-4269-ABEC-8E97AE678101", "Group" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8",
                "Security Role (optional)", "If specified, only people with this role will be be notified (Make sure to select same group type as above).",
                2, "", "FC85D434-8AFF-4302-879B-0AF4AF7D6349", "SecurityRole" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8",
                "Follower Group Type (optional)", "If specified, only people with this role will be be notified (Make sure to select same group type as above).",
                3, "", "4C538BAC-943D-4DBF-BB09-1842C2E40515", "FollowerGroupType" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8",
                "Followed Group Type (optional)", "If specified, only people with this role will be suggested to the follower (Make sure to select same group type as above).",
                4, "", "8BC2A430-86E5-41AE-99CD-AD3586ED59E6", "FollowedGroupType" );

            Sql( MigrationSQL._201508171751190_FollowingEventNotification );

            // MP: Admin Checklist Item for External Applications
            // delete the previous 'Update External Applications' checklist item since the v4 one replaces it
            RockMigrationHelper.DeleteDefinedValue( "CC9B250C-859D-3899-4730-9C5EFAEC504C" );
            RockMigrationHelper.AddDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D", "Update External Applications", @"The following external applications have been updated in v4.0. You can download the installers under <span class=""navigation-tip"">Admins Tools > Power Tools > External Applications</span>.
<ul>
<li>Rock Check Scanner</li>
<li>Rock Statement Generator</li>
<li>Rock Job Scheduler Service</li>
<li>Rock Windows Check-in Client</li>
</ul>", "62A45DA7-8C68-4283-B9AA-2E1BE38610AF" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FollowingEventNotification", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEventNotification", "FollowingEventTypeId", "dbo.FollowingEventType");
            DropForeignKey("dbo.FollowingEventNotification", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FollowingEventNotification", new[] { "ForeignId" });
            DropIndex("dbo.FollowingEventNotification", new[] { "Guid" });
            DropIndex("dbo.FollowingEventNotification", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingEventNotification", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingEventNotification", new[] { "FollowingEventTypeId" });
            DropTable("dbo.FollowingEventNotification");
        }
    }
}
