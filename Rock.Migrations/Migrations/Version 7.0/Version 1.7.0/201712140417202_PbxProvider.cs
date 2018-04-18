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
    public partial class PbxProvider : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM, "PBX CDR", "Used for tracking phone calls coming from phone systems.", SystemGuid.DefinedValue.PBX_CDR_MEDIUM_VALUE );

            RockMigrationHelper.AddPage( true, "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Phone Systems", "", "A33C221B-F361-437A-BDC1-E46BB3B532EF", "fa fa-phone-square" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Person Preferences", "Allows the person to set their personal preferences.", "~/Blocks/Crm/PersonPreferences.ascx", "CRM", "D2049782-C286-4EE1-94E8-039111E16794" );
            // Add Block to Page: My Settings, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "CF54E680-2E02-4F16-B54B-A2F2D29CD932","","D2049782-C286-4EE1-94E8-039111E16794","Person Preferences","Feature",@"",@"",0,"D7578388-BDEB-4E5F-9F35-DF98925027D7");
            // Add Block to Page: Phone Systems, Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "A33C221B-F361-437A-BDC1-E46BB3B532EF","","21F5F466-59BC-40B2-8D73-7314D936C3CB","Components","Main",@"",@"",0,"231B1FBC-7399-4453-A012-66E9A2783DE3");

            // Attrib for BlockType: Person Bio:Enable Call Origination              
            RockMigrationHelper.UpdateBlockTypeAttribute("0F5922BB-CD68-40AC-BF3C-4AAB1B98760C","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Call Origination","EnableCallOrigination","",@"Should click-to-call links be added to phone numbers.",14,@"True","5E066824-0ECE-46D1-B1B3-98E9D8D2FD5B");  
            // Attrib Value for Block:Bio, Attribute:Enable Call Origination , Layout: PersonDetail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("B5C1FDB6-0224-43E4-8E26-6B2EAF86253A","5E066824-0ECE-46D1-B1B3-98E9D8D2FD5B",@"True");  
            // Attrib Value for Block:Components, Attribute:Component Container Page: Phone Systems, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("231B1FBC-7399-4453-A012-66E9A2783DE3","259AF14D-0214-4BE4-A7BF-40423EA07C99",@"Rock.Pbx.PbxContainer, Rock");
            // Attrib Value for Block:Components, Attribute:Support Ordering Page: Phone Systems, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("231B1FBC-7399-4453-A012-66E9A2783DE3","A4889D7B-87AA-419D-846C-3E618E79D875",@"True");
            // Attrib Value for Block:Components, Attribute:core.CustomGridColumnsConfig Page: Phone Systems, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("231B1FBC-7399-4453-A012-66E9A2783DE3","DACB8566-FD46-4905-BCAD-460BEECCFF5A",@"");

            // Font Awesome Page/Block
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Font Awesome Settings", "", "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", "fa fa-flag" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Font Awesome Settings", "Block that can be used to configure Font Awesome", "~/Blocks/Cms/FontAwesomeSettings.ascx", "CMS", "5150C021-3BA5-4CCC-B943-52AABD49D481" );
            // Add Block to Page: Font Awesome Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", "", "5150C021-3BA5-4CCC-B943-52AABD49D481", "Font Awesome Settings", "Main", @"", @"", 0, "F820E008-7754-4A92-ADB9-80D1BF662E2B" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Font Awesome Settings, from Page: Font Awesome Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F820E008-7754-4A92-ADB9-80D1BF662E2B" );
            RockMigrationHelper.DeleteBlockType( "5150C021-3BA5-4CCC-B943-52AABD49D481" ); // Font Awesome Settings
            RockMigrationHelper.DeletePage( "BB2AF2B3-6D06-48C6-9895-EDF2BA254533" ); //  Page: Font Awesome Settings, Layout: Full Width, Site: Rock RMS

            // Attrib for BlockType: Person Bio:Enable Call Origination              
            RockMigrationHelper.DeleteAttribute("5E066824-0ECE-46D1-B1B3-98E9D8D2FD5B");

            // Remove Block: Person Preferences, from Page: My Settings, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("D7578388-BDEB-4E5F-9F35-DF98925027D7");
            // Remove Block: Components, from Page: Phone Systems, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("231B1FBC-7399-4453-A012-66E9A2783DE3");
            RockMigrationHelper.DeleteBlockType( "D2049782-C286-4EE1-94E8-039111E16794" ); // Person Preferences
            RockMigrationHelper.DeletePage( "A33C221B-F361-437A-BDC1-E46BB3B532EF" ); //  Page: Phone Systems, Layout: Full Width, Site: Rock RMS
        }
    }
}
