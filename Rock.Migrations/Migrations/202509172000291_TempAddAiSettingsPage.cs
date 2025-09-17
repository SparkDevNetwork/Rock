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
    ///
    /// </summary>
    public partial class TempAddAiSettingsPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Page 
            //  Internal Name: Settings
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "9F7B9158-3A73-429A-A817-5909D2AED13C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Settings", "", "FC231EB9-6FD6-40B1-A678-FB4E85C70CD1", "ti ti-settings" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.AI.AISettings
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.AI.AISettings", "AI Settings", "Rock.Blocks.AI.AISettings, Rock.Blocks, Version=18.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "D385228B-6D65-442B-8874-BF7D1F0FE84F" );

            // Add/Update Obsidian Block Type
            //   Name:AI Settings
            //   Category:AI
            //   EntityType:Rock.Blocks.AI.AISettings
            RockMigrationHelper.AddOrUpdateEntityBlockType( "AI Settings", "Configures system settings related to the AI features in Rock.", "Rock.Blocks.AI.AISettings", "AI", "7B2A9827-8584-46AE-B2FD-E52A7F131FBF" );

            // Add Block 
            //  Block Name: AI Settings
            //  Page Name: Settings
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FC231EB9-6FD6-40B1-A678-FB4E85C70CD1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "7B2A9827-8584-46AE-B2FD-E52A7F131FBF".AsGuid(), "AI Settings", "Main", @"", @"", 0, "98E0B611-316B-4401-9F23-7B6E7359B8DA" );

            // Update page order so the new Settings page is first.
            Sql( "UPDATE [Page] SET [Order] = [Order] + 1 WHERE [ParentPageId] = (SELECT [Id] FROM [Page] WHERE [Guid] = '9F7B9158-3A73-429A-A817-5909D2AED13C')" );
            Sql( "UPDATE [Page] SET [Order] = 0 WHERE [Guid] = 'FC231EB9-6FD6-40B1-A678-FB4E85C70CD1'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block
            //  Name: AI Settings, from Page: Settings, Site: Rock RMS
            //  from Page: Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "98E0B611-316B-4401-9F23-7B6E7359B8DA" );

            // Delete BlockType 
            //   Name: AI Settings
            //   Category: AI
            //   Path: -
            //   EntityType: AI Settings
            RockMigrationHelper.DeleteBlockType( "7B2A9827-8584-46AE-B2FD-E52A7F131FBF" );

            // Delete Page 
            //  Internal Name: Settings
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "FC231EB9-6FD6-40B1-A678-FB4E85C70CD1" );
        }
    }
}
