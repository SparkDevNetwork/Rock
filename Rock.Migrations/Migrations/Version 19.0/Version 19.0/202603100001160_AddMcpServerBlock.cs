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
    public partial class AddMcpServerBlock : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.UserLogin", "ApiKeyPurpose", c => c.Int() );
            AddColumn( "dbo.UserLogin", "Description", c => c.String( maxLength: 250 ) );

            // Set all existing API Keys to have a purpose of "General Use" (1)
            Sql( "UPDATE dbo.UserLogin SET ApiKeyPurpose = 1 WHERE [ApiKey] IS NOT NULL AND RTRIM([ApiKey]) <> ''" );


            // Add Page 
            //  Internal Name: MCP Servers
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "MCP Servers", "", "87BD0803-9532-49DA-B584-D9568A2AD796", "ti ti-robot" );

            // Add Page Route
            //   Page:MCP Servers
            //   Route:my/mcp-servers
            RockMigrationHelper.AddOrUpdatePageRoute( "87BD0803-9532-49DA-B584-D9568A2AD796", "my/mcp-servers", "1566561F-A051-48D9-805B-D099C535F145" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.McpServerList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.McpServerList", "Mcp Server List", "Rock.Blocks.Core.McpServerList, Rock.Blocks, Version=19.0.6.0, Culture=neutral, PublicKeyToken=null", false, false, "F0B14291-8035-4986-A4D8-DC1AE08E4F7B" );

            // Add/Update Obsidian Block Type
            //   Name:MCP Server List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.McpServerList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "MCP Server List", "Displays a list of MCP Servers.", "Rock.Blocks.Core.McpServerList", "Core", "54B23A63-87C0-4955-B915-C91F23C36D48" );

            // Add Block 
            //  Block Name: MCP Server List
            //  Page Name: MCP Server List
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "87BD0803-9532-49DA-B584-D9568A2AD796".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "54B23A63-87C0-4955-B915-C91F23C36D48".AsGuid(), "MCP Server List", "Main", @"", @"", 0, "30A085FD-A7FD-4B8C-933D-5DA6B13881F2" );
            
            // Set MCP Servers page cacheability to "no-store" since the page can contain sensitive information.
            var cacheControlNoStore = new Rock.Utility.RockCacheability
            {
                RockCacheablityType = Rock.Utility.RockCacheablityType.NoStore
            };
            Sql( $@"
UPDATE [dbo].[Page]
   SET [CacheControlHeaderSettings] = '{cacheControlNoStore.ToJson()}'
 WHERE [Guid] = '87BD0803-9532-49DA-B584-D9568A2AD796'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block
            //  Name: MCP Server List, from Page: MCP Servers, Site: Rock RMS
            //  from Page: MCP Servers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("30A085FD-A7FD-4B8C-933D-5DA6B13881F2");

            // Delete BlockType 
            //   Name: MCP Server List
            //   Category: Core
            //   Path: -
            //   EntityType: Mcp Server List
            RockMigrationHelper.DeleteBlockType("54B23A63-87C0-4955-B915-C91F23C36D48");

            // Delete Page 
            //  Internal Name: MCP Servers
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage("87BD0803-9532-49DA-B584-D9568A2AD796");

            DropColumn( "dbo.UserLogin", "Description" );
            DropColumn( "dbo.UserLogin", "ApiKeyPurpose" );
        }
    }
}
