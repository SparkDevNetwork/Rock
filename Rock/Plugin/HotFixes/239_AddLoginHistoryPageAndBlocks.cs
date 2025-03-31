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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 239, "1.17.0" )]
    public class AddLoginHistoryPageAndBlocks : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            JPH_UnsecureOidcGivePermissionPage_20250326_Up();
            JPH_AddLoginHistoryPageAndBlocks_20250326_Up();
            JPH_MigrateLoginHistory_20250326_Up();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            JPH_UnsecureOidcGivePermissionPage_20250326_Down();
            JPH_AddLoginHistoryPageAndBlocks_20250326_Down();
        }

        /// <summary>
        /// JPH: Unsecure OIDC Give Permission page - up.
        /// </summary>
        private void JPH_UnsecureOidcGivePermissionPage_20250326_Up()
        {
            // This OIDC page will handle authentication manually, as we need to first load the page unsecured and check
            // some OIDC-specific stuff. We'll redirect to the login page if needed.
            RockMigrationHelper.DeleteSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, 0, "View", true, null, 1, "ED4B5C98-8442-4CCA-8C17-2CD17AF10016" );
        }

        /// <summary>
        /// JPH: Unsecure OIDC Give Permission page - down.
        /// </summary>
        private void JPH_UnsecureOidcGivePermissionPage_20250326_Down()
        {
            // Reset the security back to only allow authenticated individuals.
            RockMigrationHelper.DeleteSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.OIDC_GIVE_PERMISSION, 0, "View", true, null, 2, "A9705A6C-A339-4BE3-835A-1A0CE3CBE194" );
        }

        /// <summary>
        /// JPH: Add Login History page and blocks - up.
        /// </summary>
        private void JPH_AddLoginHistoryPageAndBlocks_20250326_Up()
        {
            // Add Page 
            //  Internal Name: Login History
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Login History", "", "A2495383-5717-451D-95C2-0F14B6787B3A", "fa fa-history" );

            // Add Page Route
            //   Page:Login History
            //   Route:admin/security/login-history
            RockMigrationHelper.AddOrUpdatePageRoute( "A2495383-5717-451D-95C2-0F14B6787B3A", "admin/security/login-history", "7F103C07-FF6F-422C-B202-67E3B6FA8C09" );

            // ----------------------------------

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.LoginHistory", "User Login Activity", "Rock.Blocks.Security.LoginHistory, Rock.Blocks, Version=17.0.36.0, Culture=neutral, PublicKeyToken=null", false, false, "63507646-F14D-4F2C-A5C4-FA28B3DEB8F0" );

            // Add/Update Obsidian Block Type
            //   Name:Login History
            //   Category:Security
            //   EntityType:Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Login History", "A block for viewing login activity for all or a single person.", "Rock.Blocks.Security.LoginHistory", "Security", "6C02377F-DD74-4B2C-9BAD-1A010A12A714" );

            // Attribute for BlockType
            //   BlockType: Login History
            //   Category: Security
            //   Attribute: Enable Person Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C02377F-DD74-4B2C-9BAD-1A010A12A714", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Context", "EnablePersonContext", "Enable Person Context", @"If enabled and the page has a person context, its value will be used to limit the grid results to only this person, and the ""Person"" column will be hidden.", 0, @"False", "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Login History
            //  Page Name: Login History
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A2495383-5717-451D-95C2-0F14B6787B3A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C02377F-DD74-4B2C-9BAD-1A010A12A714".AsGuid(), "Login History", "Main", @"", @"", 0, "38F796C4-EA54-4B51-A16D-2845DDD82263" );

            // ----------------------------------

            // Add Block 
            //  Block Name: Login History
            //  Page Name: History
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "6C02377F-DD74-4B2C-9BAD-1A010A12A714".AsGuid(), "Login History", "SectionC1", @"", @"", 4, "427BC41B-E803-41FD-B09A-91EA85F492F1" );

            // Add Block Attribute Value
            //   Block: Login History
            //   BlockType: Login History
            //   Category: Security
            //   Block Location: Page=History, Site=Rock RMS
            //   Attribute: Enable Person Context
            /*   Attribute Value: True */
            RockMigrationHelper.AddBlockAttributeValue( "427BC41B-E803-41FD-B09A-91EA85F492F1", "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6", @"True" );
        }

        /// <summary>
        /// JPH: Add Login History page and blocks - down.
        /// </summary>
        private void JPH_AddLoginHistoryPageAndBlocks_20250326_Down()
        {
            // Remove Block
            //  Name: Login History, from Page: History, Site: Rock RMS
            //  from Page: History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "427BC41B-E803-41FD-B09A-91EA85F492F1" );

            // ----------------------------------

            // Remove Block
            //  Name: Login History, from Page: Login History, Site: Rock RMS
            //  from Page: Login History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "38F796C4-EA54-4B51-A16D-2845DDD82263" );

            // ----------------------------------

            // Attribute for BlockType
            //   BlockType: Login History
            //   Category: Security
            //   Attribute: Enable Person Context
            RockMigrationHelper.DeleteAttribute( "6C4AD3C4-8635-422D-9F02-8F20E8E64EC6" );

            // Delete BlockType 
            //   Name: Login History
            //   Category: Security
            //   Path: -
            //   EntityType: User Login Activity
            RockMigrationHelper.DeleteBlockType( "6C02377F-DD74-4B2C-9BAD-1A010A12A714" );

            // Delete Block EntityType: Rock.Blocks.Security.LoginHistory
            RockMigrationHelper.DeleteEntityType( "63507646-F14D-4F2C-A5C4-FA28B3DEB8F0" );

            // ----------------------------------

            // Delete Page 
            //  Internal Name: Login History
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "A2495383-5717-451D-95C2-0F14B6787B3A" );
        }

        /// <summary>
        /// JPH: Add a post update job to migrate login history.
        /// </summary>
        private void JPH_MigrateLoginHistory_20250326_Up()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.1 - Migrate Login History",
                description: "This job will migrate login history from the History table to the HistoryLogin table.",
                jobType: "Rock.Jobs.PostV171MigrateLoginHistory",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_171_MIGRATE_LOGIN_HISTORY );
        }
    }
}
