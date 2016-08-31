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
    public partial class DuplicateCheckScan : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.FinancialTransaction", "CheckMicrHash", c => c.String(maxLength: 128));
            CreateIndex("dbo.FinancialTransaction", "CheckMicrHash");

            /* Add Group Member Link blocktype */

            RockMigrationHelper.UpdateBlockType( "Group Member Link", "Block adds or updates a person into the configured group with the configured status and role, and sets group member attribute values that are given as name-value pairs in the querystring.", "~/Blocks/Groups/GroupMemberLink.ascx", "Groups", "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF" );

            // Attrib for BlockType: Group Member Link:Error Message
            RockMigrationHelper.AddBlockTypeAttribute( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Error Message", "ErrorMessage", "", "The text to display when a valid person key is NOT provided", 0, @"There was a problem with your registration.  Please try to register again.", "5B2DF4FE-4231-43DB-B1E4-CB88F4E81219" );

            // Attrib for BlockType: Group Member Link:Group
            RockMigrationHelper.AddBlockTypeAttribute( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", "The group this block will be adding or updating people into.", 2, @"", "24E0AF57-2C84-4625-98E9-BA2241C41E14" );

            // Attrib for BlockType: Group Member Link:Group Member Status
            RockMigrationHelper.AddBlockTypeAttribute( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", "The group member status you want to set for the person.", 3, @"2", "DE9E9CA9-8A01-46AB-8A16-CA6967C88626" );

            // Attrib for BlockType: Group Member Link:Success Message
            RockMigrationHelper.AddBlockTypeAttribute( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Success Message", "SuccessMessage", "", "The text (HTML) to display when a person is successfully added to the group.", 0, @"
<h1>You're in!</h1>
<p>You have been added to the group.</p>", "2C5C3A4A-DD79-47CD-9E30-0531266C7F75" );

            // Attrib for BlockType: Group Member Link:Success Page
            RockMigrationHelper.AddBlockTypeAttribute( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Success Page", "SuccessPage", "", "The page to redirect to if the person was registered successfully. (If set, this overrides the Success Message setting.)", 0, @"", "7CC07346-E82F-42C9-8DDA-5AB22D58B1F7" );

            Sql( @"
  UPDATE [EntityType]
	SET [FriendlyName] = 'Send SMS'
	WHERE [Name] = 'Rock.Workflow.Action.SendSms'

  UPDATE [EntityType]
	SET [FriendlyName] = 'Set Workflow Name'
	WHERE [Name] = 'Rock.Workflow.Action.SetName'

  UPDATE [EntityType]
	SET [FriendlyName] = 'Assign Activity From Attribute Value'
	WHERE [Name] = 'Rock.Workflow.Action.AssignActivityToAttributeValue'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.FinancialTransaction", new[] { "CheckMicrHash" });
            DropColumn("dbo.FinancialTransaction", "CheckMicrHash");

            // Attrib for BlockType: Group Member Link:Success Page
            RockMigrationHelper.DeleteAttribute( "7CC07346-E82F-42C9-8DDA-5AB22D58B1F7" );
            // Attrib for BlockType: Group Member Link:Success Message
            RockMigrationHelper.DeleteAttribute( "2C5C3A4A-DD79-47CD-9E30-0531266C7F75" );
            // Attrib for BlockType: Group Member Link:Group Member Status
            RockMigrationHelper.DeleteAttribute( "DE9E9CA9-8A01-46AB-8A16-CA6967C88626" );
            // Attrib for BlockType: Group Member Link:Group
            RockMigrationHelper.DeleteAttribute( "24E0AF57-2C84-4625-98E9-BA2241C41E14" );
            // Attrib for BlockType: Group Member Link:Error Message
            RockMigrationHelper.DeleteAttribute( "5B2DF4FE-4231-43DB-B1E4-CB88F4E81219" );

            RockMigrationHelper.DeleteBlockType( "9AAA967C-D0B0-4E42-89DE-2AE6AAFC17EF" ); // Group Member Link
        }
    }
}
