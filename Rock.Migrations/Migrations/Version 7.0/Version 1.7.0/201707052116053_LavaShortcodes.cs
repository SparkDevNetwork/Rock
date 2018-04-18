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
    public partial class LavaShortcodes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.LavaShortcode",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        Documentation = c.String(),
                        IsSystem = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        TagName = c.String(nullable: false, maxLength: 50),
                        Markup = c.String(nullable: false),
                        TagType = c.Int(nullable: false),
                        EnabledLavaCommands = c.String(maxLength: 500),
                        Parameters = c.String(maxLength: 2500),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);


            RockMigrationHelper.AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Lava Shortcodes", "", "6CFF2C81-6303-4477-A7EC-156DDBF8BE64", "fa fa-cube" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "6CFF2C81-6303-4477-A7EC-156DDBF8BE64", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Lava Shortcode Detail", "", "1E30B9E7-0951-45FC-8637-8ADCBE782A30", "fa fa-cube" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Lava Shortcode Detail", "Displays the details of a Lava Shortcode.", "~/Blocks/Cms/LavaShortcodeDetail.ascx", "CMS", "092BFC5F-A291-4472-B737-0C69EA33D08A" );
            RockMigrationHelper.UpdateBlockType( "Lava Shortcode List", "Lists Lava Shortcode in the system.", "~/Blocks/Cms/LavaShortcodeList.ascx", "CMS", "C26C7979-81C1-4A20-A167-35415CD7FED3" );

            // Add Block to Page: Lava Shortcodes, Site: Rock RMS              
            RockMigrationHelper.AddBlock("6CFF2C81-6303-4477-A7EC-156DDBF8BE64","","C26C7979-81C1-4A20-A167-35415CD7FED3","Lava Shortcode List","Main",@"",@"",0,"F7534DE0-DD13-4DEF-818E-8B90BABF0D0B");
            // Add Block to Page: Lava Shortcode Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlock("1E30B9E7-0951-45FC-8637-8ADCBE782A30","","092BFC5F-A291-4472-B737-0C69EA33D08A","Lava Shortcode Detail","Main",@"",@"",0,"D9694212-12B5-43EF-B939-14631968EF84");

            // Attrib for BlockType: Lava Shortcode List:Detail Page              
            RockMigrationHelper.UpdateBlockTypeAttribute("C26C7979-81C1-4A20-A167-35415CD7FED3","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","",0,@"","912DEDE4-E0B1-4F40-BDD2-4D011C2FB2E9");  

            // Attrib Value for Block:Lava Shortcode List, Attribute:Detail Page Page: Lava Shortcodes, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("F7534DE0-DD13-4DEF-818E-8B90BABF0D0B","912DEDE4-E0B1-4F40-BDD2-4D011C2FB2E9",@"1e30b9e7-0951-45fc-8637-8adcbe782a30");  

            RockMigrationHelper.UpdateFieldType( "Lava", "", "Rock", "Rock.Field.Types.LavaFieldType", "27718256-C1EB-4B1F-9B4B-AC53249F78DF" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Lava Shortcode List:Detail Page              
            RockMigrationHelper.DeleteAttribute("912DEDE4-E0B1-4F40-BDD2-4D011C2FB2E9");
            // Remove Block: Lava Shortcode Detail, from Page: Lava Shortcode Detail, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("D9694212-12B5-43EF-B939-14631968EF84");
            // Remove Block: Lava Shortcode List, from Page: Lava Shortcodes, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("F7534DE0-DD13-4DEF-818E-8B90BABF0D0B");
            RockMigrationHelper.DeleteBlockType( "C26C7979-81C1-4A20-A167-35415CD7FED3" ); // Lava Shortcode List
            RockMigrationHelper.DeleteBlockType( "092BFC5F-A291-4472-B737-0C69EA33D08A" ); // Lava Shortcode Detail
            RockMigrationHelper.DeletePage( "1E30B9E7-0951-45FC-8637-8ADCBE782A30" ); //  Page: Lava Shortcode Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6CFF2C81-6303-4477-A7EC-156DDBF8BE64" ); //  Page: Lava Shortcodes, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.LavaShortcode", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.LavaShortcode", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.LavaShortcode", new[] { "Guid" });
            DropIndex("dbo.LavaShortcode", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.LavaShortcode", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.LavaShortcode");
        }
    }
}
