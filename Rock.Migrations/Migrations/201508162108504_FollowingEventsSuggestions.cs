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
    public partial class FollowingEventsSuggestions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FollowingSuggested",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        SuggestionTypeId = c.Int(nullable: false),
                        LastPromotedDateTime = c.DateTime(),
                        StatusChangedDateTime = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .ForeignKey("dbo.FollowingSuggestionType", t => t.SuggestionTypeId, cascadeDelete: true)
                .Index(t => t.EntityTypeId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.SuggestionTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.FollowingSuggestionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        ReasonNote = c.String(nullable: false, maxLength: 50),
                        ReminderDays = c.Int(),
                        EntityTypeId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        EntityNotificationFormatLava = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.FollowingEventSubscription",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventTypeId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.FollowingEventType", t => t.EventTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => t.EventTypeId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.FollowingEventType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(),
                        FollowedEntityTypeId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        SendOnWeekends = c.Boolean(nullable: false),
                        LastCheckDateTime = c.DateTime(),
                        IsNoticeRequired = c.Boolean(nullable: false),
                        EntityNotificationFormatLava = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.EntityType", t => t.FollowedEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.FollowedEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.GroupType", "IgnorePersonInactivated", c => c.Boolean(nullable: false));

            RockMigrationHelper.UpdateFieldType( "Event Item", "", "Rock", "Rock.Field.Types.EventItemFieldType", "23713932-F558-45F7-BB00-2A550852F70D" );

            // Rename person following block
            RockMigrationHelper.RenameBlockType( "~/Blocks/Crm/PersonFollowingList.ascx", "~/Blocks/Follow/PersonFollowingList.ascx", "Follow" );

            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Events", "", "283D2756-7686-4ED5-AE44-4B8811E3956F", "fa fa-flag" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "283D2756-7686-4ED5-AE44-4B8811E3956F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Event", "", "C68D4CA0-9B2D-4B85-AC5B-361126E787CC", "fa fa-flag" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Suggestions", "", "3FD46CEF-113E-4A19-B9B7-D9A1BCA9C043", "fa fa-flag-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "3FD46CEF-113E-4A19-B9B7-D9A1BCA9C043", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Suggestion", "", "9593F41C-23A2-4F65-BBD4-634A06380E2E", "fa fa-flag-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Settings", "", "18B8EB25-B9F2-48C6-B047-51A512A8F1C9", "fa fa-flag" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "AE1818D8-581C-4599-97B9-509EA450376A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Following Suggestions", "", "50BAAD66-46AB-4968-AFD6-254C536ACEC8", "fa fa-flag-o" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Event List", "Block for viewing list of following events.", "~/Blocks/Follow/EventList.ascx", "Follow", "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050" );
            RockMigrationHelper.UpdateBlockType( "Event Detail", "Block for editing following event types.", "~/Blocks/Follow/EventDetail.ascx", "Follow", "762BC126-1A2E-4483-A63B-3AB4939D19F1" );
            RockMigrationHelper.UpdateBlockType( "Suggestion List", "Block for viewing list of following events.", "~/Blocks/Follow/SuggestionList.ascx", "Follow", "23818F47-D81E-4B6E-B89B-045B1FAD4C2B" );
            RockMigrationHelper.UpdateBlockType( "Suggestion Detail", "Block for editing the following suggestion types.", "~/Blocks/Follow/SuggestionDetail.ascx", "Follow", "052B84EA-0C34-4A07-AC4C-1FBCEC87C223" );
            RockMigrationHelper.UpdateBlockType( "Event Subscription", "Block for users to select which following events they would like to subscribe to.", "~/Blocks/Follow/EventSubscription.ascx", "Follow", "F72A4100-001E-47F9-9406-5529F2A45131" );
            RockMigrationHelper.UpdateBlockType( "Person Suggestion Notice", "Block for displaying a button and count of suggested people that can be used to navigate to person suggestion list block.", "~/Blocks/Follow/PersonSuggestionNotice.ascx", "Follow", "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4" );
            RockMigrationHelper.UpdateBlockType( "Person Suggestion List", "Block for displaying people that have been suggested to current person to follow.", "~/Blocks/Follow/PersonSuggestionList.ascx", "Follow", "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA" );
          
            // Add Block to Page: Following Events, Site: Rock RMS
            RockMigrationHelper.AddBlock( "283D2756-7686-4ED5-AE44-4B8811E3956F", "", "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "Event List", "Main", "", "", 0, "0E56374A-F605-4DDF-88D2-FFA3A0FB96EA" );
            // Add Block to Page: Following Event, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C68D4CA0-9B2D-4B85-AC5B-361126E787CC", "", "762BC126-1A2E-4483-A63B-3AB4939D19F1", "Event Detail", "Main", "", "", 0, "058C5DEF-4DB2-4185-B351-2C2AA5A68FB2" );
            // Add Block to Page: Following Suggestions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "3FD46CEF-113E-4A19-B9B7-D9A1BCA9C043", "", "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "Suggestion List", "Main", "", "", 0, "B410A264-83FC-4568-8F7B-D42988D6A03E" );
            // Add Block to Page: Following Suggestion, Site: Rock RMS
            RockMigrationHelper.AddBlock( "9593F41C-23A2-4F65-BBD4-634A06380E2E", "", "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "Suggestion Detail", "Main", "", "", 0, "4183C0E6-0F3F-47F4-AB83-E4EC1A4838C9" );
            // Add Block to Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( "18B8EB25-B9F2-48C6-B047-51A512A8F1C9", "", "F72A4100-001E-47F9-9406-5529F2A45131", "Event Subscription", "Main", "", "", 0, "BEBABF81-AD85-4F3E-9F12-2DF30CBAF744" );
            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock( "AE1818D8-581C-4599-97B9-509EA450376A", "", "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4", "Person Suggestion Notice", "Sidebar1", "", "", 0, "D54B70C3-B964-459D-B5B6-39BB49AE4E7A" );
            // Add Block to Page: Following Suggestions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "50BAAD66-46AB-4968-AFD6-254C536ACEC8", "", "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA", "Person Suggestion List", "Main", "", "", 0, "E0C3AA84-BF75-420D-8A1F-0E681273DF25" );

            // Attrib for BlockType: Event List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "ADFB8E1D-B068-4351-B378-738C8839CF16" );
            // Attrib for BlockType: Suggestion List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "BEC12CBB-F21F-4CFA-B047-7B53E2071AAE" );
            // Attrib for BlockType: Person Suggestion Notice:List Page
            RockMigrationHelper.AddBlockTypeAttribute( "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "List Page", "ListPage", "", "", 0, @"", "419C144C-B688-4D7A-B14D-0D90156BB0C0" );

            // Attrib Value for Block:Event List, Attribute:Detail Page Page: Following Events, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "0E56374A-F605-4DDF-88D2-FFA3A0FB96EA", "ADFB8E1D-B068-4351-B378-738C8839CF16", @"c68d4ca0-9b2d-4b85-ac5b-361126e787cc" );
            // Attrib Value for Block:Suggestion List, Attribute:Detail Page Page: Following Suggestions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B410A264-83FC-4568-8F7B-D42988D6A03E", "BEC12CBB-F21F-4CFA-B047-7B53E2071AAE", @"9593f41c-23a2-4f65-bbd4-634a06380e2e" );
            // Attrib Value for Block:Person Suggestion Notice, Attribute:List Page Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D54B70C3-B964-459D-B5B6-39BB49AE4E7A", "419C144C-B688-4D7A-B14D-0D90156BB0C0", @"50baad66-46ab-4968-afd6-254c536acec8" );
            
            // Following component types
            RockMigrationHelper.UpdateEntityType( "Rock.Model.FollowingEventType", "8A0D208B-762D-403A-A972-3A0F079866D4", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.FollowingSuggestionType", "CC7DF118-86A1-4F90-82D8-0DAE9CD37343", true, true );

            // Birthday event
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Event.PersonBirthday", "532A7405-A3FB-4147-BE67-3B75A230AADE", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingEventType", "Rock.Follow.Event.PersonBirthday", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", 
                "Lead Days", "The number of days prior to birthday that notification should be sent.", 0, "5", "F5B35909-5A4A-4203-84A8-7F493E56548B" );

            // In Group Together suggestion
            RockMigrationHelper.UpdateEntityType( "Rock.Follow.Suggestion.InGroupTogether", "20AC7F2A-D42F-438D-93D7-46E3C6769B8F", false, true );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "18E29E23-B43B-4CF7-AE41-C85672C09F50",
                "Group Type", "The type of group.", 0, "", "AB754108-EB85-422B-A898-90C47495174A" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8",
                "Follower Group Type (optional)", "If specified, only people with this role will be be notified (Make sure to select same group type as above).",
                1, "", "4C538BAC-943D-4DBF-BB09-1842C2E40515", "FollowerGroupType" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.FollowingSuggestionType", "Rock.Follow.Suggestion.InGroupTogether", "3BB25568-E793-4D12-AE80-AC3FDA6FD8A8",
                "Followed Group Type (optional)", "If specified, only people with this role will be suggested to the follower (Make sure to select same group type as above).",
                2, "", "8BC2A430-86E5-41AE-99CD-AD3586ED59E6", "FollowedGroupType");

            // Add following category and update order of other system email categories
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Event Registration", "fa fa-clipboard", "", "4A7D0D1F-E160-445E-9D29-AEBD140DA242", 0 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Finance", "fa fa-money", "", "673D13E6-0161-4AC2-B265-DF3783DE3B41", 1 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Following", "fa fa-flag", "", "55BC5EFF-3090-4B1A-AA18-276CD90689EB", 2 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Groups", "fa fa-users", "", "B31064D2-F2EF-43AA-8BEA-14DF257CBC59", 3 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Security", "fa fa-lock", "", "AEB302FF-A40B-4EDF-9F4E-7E4292C03A47", 4 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "System", "fa fa-wrench", "", "9AF1FA93-089B-44B7-83A3-48E0031CCC1D", 5 );
            RockMigrationHelper.UpdateCategoryByName( "B21FD119-893E-46C0-B42D-E4CDD5C8C49D", "Workflow", "fa fa-cogs", "", "C7B9B5F1-9D90-485F-93E4-5D7D81EC2B12", 6 );

            // Add system emails for event/suggestion notifications
            RockMigrationHelper.UpdateSystemEmail( "Following", "Following Event Notification", "", "", "", "", "", "Following Notices", @"
{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    Listed below are events that you've opted to be notified about.
</p>

{% for event in EventTypes %}
    <h4>{{ event.EventType.Description }}</h4>
    <table cellpadding=""25"">
    {% for notice in event.Notices %}
        {{ notice }}
    {% endfor %}
    </table>
{% endfor %}

{{ 'Global' | Attribute:'EmailFooter' }}
", "CA7576CD-0A10-4ADA-A068-62EE598178F5" );
            RockMigrationHelper.UpdateSystemEmail( "Following", "Following Suggestion Notification", "", "", "", "", "", "Following Suggestions", @"
{{ 'Global' | Attribute:'EmailHeader' }}

<p>
    {{ Person.NickName }},
</p>

<p>
    You may <a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}Page/426"">want to follow</a>...
</p>

{% for suggestion in Suggestions %}
    <h4>{{ suggestion.SuggestionType.Description }}</h4>
    <table cellpadding=""25"">
    {% for notice in suggestion.Notices %}
        {{ notice }}
    {% endfor %}
    </table>
{% endfor %}

{{ 'Global' | Attribute:'EmailFooter' }}
", "8F5A9400-AED2-48A4-B5C8-C9B5D5669F4C" );

            // Add attributes for event/suggestion jobs (added in sql)
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendFollowingEvents", "Following Event Notification Email Template",
                "", "The system email used to send following event notifications", 0, "", "75E0D938-0CA0-4121-B013-D5B7C03BFBB8", "EmailTemplate" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Class", "Rock.Jobs.SendFollowingEvents", "Eligible Followers",
                "", "The group that contains individuals who should receive following event notification", 1, "", "446B2177-76DF-4082-A89E-E18A1B26CCF9" );

            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendFollowingSuggestions", "Following Suggestion Notification Email Template",
                "", "The system email used to send following suggestion notifications", 0, "", "E40511EA-3AAD-4C4B-9AB4-33745AD1A00A", "EmailTemplate" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Model.ServiceJob", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Class", "Rock.Jobs.SendFollowingSuggestions", "Eligible Followers",
                "", "The group that contains individuals who should receive following suggestion notification", 1, "", "B1E268B0-F890-433A-9FED-331CF4D4FD2E" );

            Sql( MigrationSQL._201508162108504_FollowingEventsSuggestions );

            // JE: Event Reg Security Updates
            // add security to the event reg pages/blocks
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Event Registration Administration", "Gives access to create and administrate event registration templates and instances.", Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.EVENT_REGISTRATION, 0, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS, 0, "4C63D597-1C2E-D7BA-4C71-34724D5C9670" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.EVENT_REGISTRATION, 0, Rock.Security.Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS, 0, "514591FB-4972-93AE-438C-A410739BDBD1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuthForPage( "4C63D597-1C2E-D7BA-4C71-34724D5C9670" );
            RockMigrationHelper.DeleteSecurityAuthForPage( "514591FB-4972-93AE-438C-A410739BDBD1" );
            RockMigrationHelper.DeleteGroup( Rock.SystemGuid.Group.GROUP_EVENT_REGISTRATION_ADMINISTRATORS );


            // Attrib for BlockType: Person Suggestion Notice:List Page
            RockMigrationHelper.DeleteAttribute( "419C144C-B688-4D7A-B14D-0D90156BB0C0" );
            // Attrib for BlockType: Suggestion List:Detail Page
            RockMigrationHelper.DeleteAttribute( "BEC12CBB-F21F-4CFA-B047-7B53E2071AAE" );
            // Attrib for BlockType: Event List:Detail Page
            RockMigrationHelper.DeleteAttribute( "ADFB8E1D-B068-4351-B378-738C8839CF16" );

            // Remove Block: Person Suggestion List, from Page: Following Suggestions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E0C3AA84-BF75-420D-8A1F-0E681273DF25" );
            // Remove Block: Person Suggestion Notice, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "D54B70C3-B964-459D-B5B6-39BB49AE4E7A" );
            // Remove Block: Event Subscription, from Page: Following Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BEBABF81-AD85-4F3E-9F12-2DF30CBAF744" );
            // Remove Block: Suggestion Detail, from Page: Following Suggestion, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4183C0E6-0F3F-47F4-AB83-E4EC1A4838C9" );
            // Remove Block: Suggestion List, from Page: Following Suggestions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B410A264-83FC-4568-8F7B-D42988D6A03E" );
            // Remove Block: Event Detail, from Page: Following Event, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "058C5DEF-4DB2-4185-B351-2C2AA5A68FB2" );
            // Remove Block: Event List, from Page: Following Events, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0E56374A-F605-4DDF-88D2-FFA3A0FB96EA" );

            RockMigrationHelper.DeleteBlockType( "983B9EBE-BDD9-49A6-87FF-7E1A585E97E4" ); // Person Suggestion Notice
            RockMigrationHelper.DeleteBlockType( "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA" ); // Person Suggestion List
            RockMigrationHelper.DeleteBlockType( "F72A4100-001E-47F9-9406-5529F2A45131" ); // Event Subscription
            RockMigrationHelper.DeleteBlockType( "23818F47-D81E-4B6E-B89B-045B1FAD4C2B" ); // Suggestion List
            RockMigrationHelper.DeleteBlockType( "052B84EA-0C34-4A07-AC4C-1FBCEC87C223" ); // Suggestion Detail
            RockMigrationHelper.DeleteBlockType( "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050" ); // Event List
            RockMigrationHelper.DeleteBlockType( "762BC126-1A2E-4483-A63B-3AB4939D19F1" ); // Event Detail

            RockMigrationHelper.DeletePage( "50BAAD66-46AB-4968-AFD6-254C536ACEC8" ); //  Page: Following Suggestions, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "18B8EB25-B9F2-48C6-B047-51A512A8F1C9" ); //  Page: Following Settings, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "9593F41C-23A2-4F65-BBD4-634A06380E2E" ); //  Page: Following Suggestion, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "3FD46CEF-113E-4A19-B9B7-D9A1BCA9C043" ); //  Page: Following Suggestions, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "C68D4CA0-9B2D-4B85-AC5B-361126E787CC" ); //  Page: Following Event, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "283D2756-7686-4ED5-AE44-4B8811E3956F" ); //  Page: Following Events, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.FollowingEventSubscription", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEventSubscription", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEventSubscription", "EventTypeId", "dbo.FollowingEventType");
            DropForeignKey("dbo.FollowingEventType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEventType", "FollowedEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingEventType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingEventType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingEventSubscription", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggested", "SuggestionTypeId", "dbo.FollowingSuggestionType");
            DropForeignKey("dbo.FollowingSuggestionType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggestionType", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingSuggestionType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggested", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggested", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FollowingSuggested", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FollowingSuggested", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FollowingEventType", new[] { "ForeignId" });
            DropIndex("dbo.FollowingEventType", new[] { "Guid" });
            DropIndex("dbo.FollowingEventType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingEventType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingEventType", new[] { "FollowedEntityTypeId" });
            DropIndex("dbo.FollowingEventType", new[] { "EntityTypeId" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "ForeignId" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "Guid" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "PersonAliasId" });
            DropIndex("dbo.FollowingEventSubscription", new[] { "EventTypeId" });
            DropIndex("dbo.FollowingSuggestionType", new[] { "ForeignId" });
            DropIndex("dbo.FollowingSuggestionType", new[] { "Guid" });
            DropIndex("dbo.FollowingSuggestionType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggestionType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggestionType", new[] { "EntityTypeId" });
            DropIndex("dbo.FollowingSuggested", new[] { "ForeignId" });
            DropIndex("dbo.FollowingSuggested", new[] { "Guid" });
            DropIndex("dbo.FollowingSuggested", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggested", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FollowingSuggested", new[] { "SuggestionTypeId" });
            DropIndex("dbo.FollowingSuggested", new[] { "PersonAliasId" });
            DropIndex("dbo.FollowingSuggested", new[] { "EntityTypeId" });
            DropColumn("dbo.GroupType", "IgnorePersonInactivated");
            DropTable("dbo.FollowingEventType");
            DropTable("dbo.FollowingEventSubscription");
            DropTable("dbo.FollowingSuggestionType");
            DropTable("dbo.FollowingSuggested");
        }
    }
}
