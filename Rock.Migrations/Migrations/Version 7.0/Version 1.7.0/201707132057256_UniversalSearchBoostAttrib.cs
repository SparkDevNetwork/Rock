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
    public partial class UniversalSearchBoostAttrib : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add global attribute for boosting universal search models
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.KEY_VALUE_LIST, "", "", "Universal Search Index Boost", "Allows you to boost certain universal search indexes.", 1000, "", "757F912F-55E0-76A9-46D2-345BB61D7B02", "UniversalSearchIndexBoost" );

            // Add Fundraising Page/Block
            RockMigrationHelper.AddPage( true, "4E237286-B715-4109-A578-C1445EC02707", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Fundraising", "", "3E0F2EF9-DC32-4DFD-B213-A410AE5B6AB7", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Fundraising Progress", "Progress for all people in a fundraising opportunity", "~/Blocks/Fundraising/FundraisingProgress.ascx", "Fundraising", "75D2BC14-34DF-42EA-8DBB-3F5294B290A9" );
            // Add Block to Page: Fundraising, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3E0F2EF9-DC32-4DFD-B213-A410AE5B6AB7", "", "75D2BC14-34DF-42EA-8DBB-3F5294B290A9", "Fundraising Progress", "Main", @"", @"", 0, "59A24C5E-8214-4F84-AE4C-648B3C5E3975" );

            // Group List Page setting inside Group Detail Block
            // Attrib for BlockType: Group Detail:Group List Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group List Page", "GroupListPage", "", "The page to display related Group List.", 11, @"", "4F55D7DD-27A4-4ABC-89AD-1160F5697FD2" );

            // Attrib Value for Block:Security Roles Detail, Attribute:Group List Page Page: Security Roles Detail, Site: Rock RMS            
            RockMigrationHelper.AddBlockAttributeValue("B58919B6-0947-4FE6-A9AE-FB28194643E7","4F55D7DD-27A4-4ABC-89AD-1160F5697FD2",@"d9678fef-c086-4232-972c-5dbac14bfee6");

            // Attrib Value for Block:Application Group Detail, Attribute:Group List Page Page: Application Group Detail, Site: Rock RMS            
            RockMigrationHelper.AddBlockAttributeValue("64EEA884-8D04-44A3-9C75-5523A9EB9175","4F55D7DD-27A4-4ABC-89AD-1160F5697FD2",@"ba078bb8-7205-46f4-9530-b2fb9ead3e57");

            // Fundraising Progress setting in Group Detail Block
            // Attrib for BlockType: Group Detail:Fundraising Progress Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Fundraising Progress Page", "FundraisingProgressPage", "", "The page to display fundraising progress for all its members.", 11, @"", "8C2AF872-C75A-41ED-8C22-76B472B18655" );
            // Attrib Value for Block:GroupDetailRight, Attribute:Fundraising Progress Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "8C2AF872-C75A-41ED-8C22-76B472B18655", @"3e0f2ef9-dc32-4dfd-b213-a410ae5b6ab7" );
            // Attrib Value for Block:Security Roles Detail, Attribute:Fundraising Progress Page Page: Security Roles Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "8C2AF872-C75A-41ED-8C22-76B472B18655", @"3e0f2ef9-dc32-4dfd-b213-a410ae5b6ab7" );
            // Attrib Value for Block:Application Group Detail, Attribute:Fundraising Progress Page Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "64EEA884-8D04-44A3-9C75-5523A9EB9175", "8C2AF872-C75A-41ED-8C22-76B472B18655", @"3e0f2ef9-dc32-4dfd-b213-a410ae5b6ab7" );
            // Attrib Value for Block:Group Detail, Attribute:Fundraising Progress Page Page: Org Chart, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C1B5CA27-8B5B-4D7F-9160-916EFAAA7D26", "8C2AF872-C75A-41ED-8C22-76B472B18655", @"3e0f2ef9-dc32-4dfd-b213-a410ae5b6ab7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "757F912F-55E0-76A9-46D2-345BB61D7B02" );

            // Remove Block: Fundraising Progress, from Page: Fundraising, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "59A24C5E-8214-4F84-AE4C-648B3C5E3975" );
            RockMigrationHelper.DeleteBlockType( "75D2BC14-34DF-42EA-8DBB-3F5294B290A9" ); // Fundraising Progress
            RockMigrationHelper.DeletePage( "3E0F2EF9-DC32-4DFD-B213-A410AE5B6AB7" ); //  Page: Fundraising, Layout: Full Width, Site: Rock RMS
            
            // Attrib for BlockType: Group Detail:Group List Page
            RockMigrationHelper.DeleteAttribute( "4F55D7DD-27A4-4ABC-89AD-1160F5697FD2" );

            // Attrib for BlockType: Group Detail:Fundraising Progress Page
            RockMigrationHelper.DeleteAttribute( "8C2AF872-C75A-41ED-8C22-76B472B18655" );
        }
    }
}
