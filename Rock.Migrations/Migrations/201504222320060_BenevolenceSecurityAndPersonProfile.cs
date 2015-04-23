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
    public partial class BenevolenceSecurityAndPersonProfile : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Benevolence", "Group of individuals who can access the various parts of the benevolence functionality.", Rock.SystemGuid.Group.GROUP_BENEVOLENCE );

            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "Error", "Error", "", "65E99944-C6D1-4820-9BC4-EE3F309D60D6" ); // Site:Rock RMS
            RockMigrationHelper.AddLayout( "C2D29296-6A87-47A9-A753-EE4E9159C4C4", "Homepage", "Homepage", "", "BD8DB1D2-67EA-4295-9EA2-FABB02103CCC" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Benevolence", "", "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence Request Detail", "", "648CA58C-EB12-4479-9994-F064070E3A32", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Campus Context Setter - Device", "Block that can be used to set the campus context for the site based on the location of the device.", "~/Blocks/Core/CampusContextSetter.Device.ascx", "Core", "C5F30BD6-45E5-4898-ADAA-F4BB4E840098" );
            RockMigrationHelper.UpdateBlockType( "Transaction Entry - Kiosk", "Block used to process giving from a kiosk.", "~/Blocks/Finance/TransactionEntry.Kiosk.ascx", "Finance", "9936E9BD-CF2C-4E3E-96AC-6E69E5127AA8" );
            RockMigrationHelper.UpdateBlockType( "Prayer Request Entry - Kiosk", "Allows prayer requests to be added from a kiosk.", "~/Blocks/Prayer/PrayerRequestEntry.Kiosk.ascx", "Prayer", "0AF9B74A-89F5-48E9-9B39-3E96344E6FE3" );
            // Add Block to Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.AddBlock( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "", "3131C55A-8753-435F-85F3-DF777EFBD1C8", "Benevolence Request List", "SectionC1", "", "", 0, "52EDDE7F-6808-4912-A73B-94AE0939DD48" );

            // Add Block to Page: Benevolence Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "648CA58C-EB12-4479-9994-F064070E3A32", "", "34275D0E-BC7E-4A9C-913E-623D086159A1", "Benevolence Request Detail", "Main", "", "", 0, "27515E57-CE16-4853-AA94-E995547BF166" );

            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Backdated Notes", "AllowBackdatedNotes", "", "", 12, @"False", "0D10F570-89BA-4BDE-96FF-65CC9A59CD2A" );

            // Attrib for BlockType: Group List:Display Member Count Column
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Member Count Column", "DisplayMemberCountColumn", "", "Should the Member Count column be displayed? Does not affect lists with a person context.", 7, @"True", "FF814712-D64E-4BC4-9597-7809BF7934EC" );

            // Attrib for BlockType: Group List:Limit to Active Status
            RockMigrationHelper.AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Limit to Active Status", "LimittoActiveStatus", "", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", 10, @"all", "65390B94-C2E8-4346-815B-EEF69A4C362B" );

            // Attrib for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 26, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "2700BC5E-045B-4246-9B39-95D01CBBD33D" );

            // Attrib for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 25, @"368DD475-242C-49C4-A42C-7278BE690CC2", "230F6BB7-FDF7-4EAF-9A57-B01253A630DE" );

            // Attrib Value for Block:Benevolence Request List, Attribute:Detail Page Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52EDDE7F-6808-4912-A73B-94AE0939DD48", "E2C90243-A79A-4DAD-9301-07F6DF095CDB", @"648ca58c-eb12-4479-9994-f064070e3a32" );

            // Attrib Value for Block:Benevolence Request List, Attribute:Case Worker Group Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52EDDE7F-6808-4912-A73B-94AE0939DD48", "576E31E0-EE40-4A89-93AE-5CCF1F45D21F", @"26e7148c-2059-4f45-bcfe-32230a12f0dc" );

            RockMigrationHelper.UpdateFieldType( "Group And Role", "", "Rock", "Rock.Field.Types.GroupAndRoleFieldType", "9AB9C5E7-914F-4BE7-895B-DDE53D2A82F0" );

            // Add/Update PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "Rock.Model.Person", "PersonId", "1863B413-642D-423B-B82B-8A0BCED3839C" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityRoleGroup( Rock.SystemGuid.Group.GROUP_BENEVOLENCE );

            // Attrib for BlockType: Transaction Entry:Connection Status
            RockMigrationHelper.DeleteAttribute( "230F6BB7-FDF7-4EAF-9A57-B01253A630DE" );
            // Attrib for BlockType: Transaction Entry:Record Status
            RockMigrationHelper.DeleteAttribute( "2700BC5E-045B-4246-9B39-95D01CBBD33D" );
            // Attrib for BlockType: Group List:Limit to Active Status
            RockMigrationHelper.DeleteAttribute( "65390B94-C2E8-4346-815B-EEF69A4C362B" );
            // Attrib for BlockType: Group List:Display Member Count Column
            RockMigrationHelper.DeleteAttribute( "FF814712-D64E-4BC4-9597-7809BF7934EC" );
            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.DeleteAttribute( "0D10F570-89BA-4BDE-96FF-65CC9A59CD2A" );
            // Remove Block: Benevolence Request Detail, from Page: Benevolence Request Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "27515E57-CE16-4853-AA94-E995547BF166" );
            // Remove Block: Benevolence Request List, from Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "52EDDE7F-6808-4912-A73B-94AE0939DD48" );
            
            RockMigrationHelper.DeletePage( "648CA58C-EB12-4479-9994-F064070E3A32" ); //  Page: Benevolence Request Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2" ); //  Page: Benevolence, Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.DeleteLayout( "BD8DB1D2-67EA-4295-9EA2-FABB02103CCC" ); //  Layout: Homepage, Site: Rock RMS
            RockMigrationHelper.DeleteLayout( "65E99944-C6D1-4820-9BC4-EE3F309D60D6" ); //  Layout: Error, Site: Rock RMS
            
            // Delete PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "1863B413-642D-423B-B82B-8A0BCED3839C" );

        }
    }
}
