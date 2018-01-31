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
    public partial class TransactionHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: History Log:Heading
            RockMigrationHelper.UpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Heading", "Heading", "", "The Lava template to use for the heading. <span class='tip tip-lava'></span>", 0, @"{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047" );

            // Add Block to Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "97716641-D003-4663-9EA2-D9BB94E7955B", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 1, "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB" );
            RockMigrationHelper.AddBlockAttributeValue( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"2c1cb26b-ab22-42d0-8164-aedee0dae667" );
            RockMigrationHelper.AddBlockAttributeValue( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"Transaction History" );
            RockMigrationHelper.UpdatePageContext( "97716641-D003-4663-9EA2-D9BB94E7955B", "Rock.Model.FinancialTransaction", "TransactionId", "A696D2B5-7E0A-4702-8902-A81781B20F6B" );

            // Add Block to Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 1, "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF" );
            RockMigrationHelper.AddBlockAttributeValue( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"2c1cb26b-ab22-42d0-8164-aedee0dae667" );
            RockMigrationHelper.AddBlockAttributeValue( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"Transaction History" );
            RockMigrationHelper.UpdatePageContext( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "Rock.Model.FinancialTransaction", "TransactionId", "117B901D-1E76-4DE9-857C-2B32209AC35F" );

            RockMigrationHelper.AddPage( true, "63990874-0DFF-45FC-9F09-81B0B0D375B4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Calendar Attributes", "", "9C610805-BE44-42DF-A73F-2C6D0014AD49", "" ); // Site:Rock RMS

            // Add Block to Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9C610805-BE44-42DF-A73F-2C6D0014AD49", "", "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "Calendar Attributes", "Main", @"", @"", 0, "F04979E2-33A2-4C0E-936E-5C8849BB98F4" );

            // Attrib for BlockType: Calendar Types:Calendar Attributes Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "041B5C23-5F1F-4B02-A767-FB7F4B1A5345", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Calendar Attributes Page", "CalendarAttributesPage", "", "Page used to configure attributes for event calendars.", 1, @"", "C3A59187-D823-4C47-BBFF-02F960803ECE" );
            // Attrib Value for Block:Event Calendar List, Attribute:Calendar Attributes Page Page: Calendars, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "367B36C6-2779-451D-BF8C-BC6318D42AA1", "C3A59187-D823-4C47-BBFF-02F960803ECE", @"9c610805-be44-42df-a73f-2c6d0014ad49" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Entity Qualifier Column Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "ECD5B86C-2B48-4548-9FE9-7AC6F6FA8106", @"" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Entity Qualifier Value Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "FCE1E87D-F816-4AD5-AE60-1E71942C547C", @"" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Entity Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"e67d8d6d-4fe6-48d5-a940-a39213047314" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Entity Id Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "CBB56D68-3727-42B9-BF13-0B2B593FB328", @"0" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Allow Setting of Values Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "018C0016-C253-44E4-84DB-D166084C5CAD", @"False" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Configure Type Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "D4132497-18BE-4D1F-8913-468E33DE63C4", @"True" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Enable Show In Grid Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "920FE120-AD75-4D5C-BFE0-FA5745B1118B", @"False" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Category Filter Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "0C2BCD33-05CC-4B03-9F57-C686B8911E64", @"" );
            // Attrib Value for Block:Calendar Attributes, Attribute:Enable Ordering Page: Calendar Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( true, "F04979E2-33A2-4C0E-936E-5C8849BB98F4", "B0518446-18AB-473C-AD35-C873BBEF27F9", @"True" );

            // Attrib for BlockType: Transaction Entry:Enable Anonymous Giving
            RockMigrationHelper.UpdateBlockTypeAttribute( "74EE3481-3E5A-4971-A02E-D463ABB45591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Anonymous Giving", "EnableAnonymousGiving", "", "Should the option to give anonymously be displayed. Giving anonymously will display the transaction as 'Anonymous' in places where it is shown publicly, for example, on a list of fundraising contributors.", 32, @"False", "5A66EB01-712A-4D9A-AE51-3DD93345378A" );

            // Attrib Value for Block:Fundraising Transaction Entry, Attribute:Enable Anonymous Giving Page: Fundraising Transaction Entry, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "1BAD904E-2F79-4488-B8BE-EECD67AE2925", "5A66EB01-712A-4D9A-AE51-3DD93345378A", @"True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB" );
            RockMigrationHelper.DeleteBlock( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF" );
        }
    }
}
