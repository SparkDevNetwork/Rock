// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Plugin;

namespace com.bemaservices.PastoralCare.Migrations
{
    [MigrationNumber( 2, "1.8.3" )]
    public class Pages : Migration
    {
        public override void Up()
        {
            // Page: Pastoral Care
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Pastoral Care", "", "EB812DF7-163B-418F-8C39-9C808A2CDF71", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Care Item List", "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user.", "~/Plugins/com_bemaservices/PastoralCare/CareItemList.ascx", "BEMA Services > Pastoral Care", "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A" );
            // Add Block to Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "EB812DF7-163B-418F-8C39-9C808A2CDF71", "", "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "Care Item List", "Main", "", "", 0, "C5AD1E56-608F-4D53-AC97-F0749B2362FF" );
            // Attrib for BlockType: Care Item List:Configuration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "", "Page used to modify and create connection careTypes.", 0, @"", "056A2CCA-86F7-4480-B8C6-6CB14827340E" );
            // Attrib for BlockType: Care Item List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "838211A9-6A9D-4523-8913-EEE15BDD05FA" );
            // Attrib for BlockType: Care Item List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of an careItems.", 1, @"", "4FA7FEE2-8203-4001-89EA-3C796424469F" );
            // Attrib Value for Block:Care Item List, Attribute:Configuration Page Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C5AD1E56-608F-4D53-AC97-F0749B2362FF", "056A2CCA-86F7-4480-B8C6-6CB14827340E", @"fc1531f6-5a3c-4f05-8e92-b2b66688b492" );
            // Attrib Value for Block:Care Item List, Attribute:Detail Page Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C5AD1E56-608F-4D53-AC97-F0749B2362FF", "4FA7FEE2-8203-4001-89EA-3C796424469F", @"9c2e82d3-5f23-4854-ad27-b8ea07183edf" );

            // Page: Care Types
            RockMigrationHelper.AddPage( "EB812DF7-163B-418F-8C39-9C808A2CDF71", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Care Types", "", "FC1531F6-5A3C-4F05-8E92-B2B66688B492", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Care Type List", "Block to display the care types.", "~/Plugins/com_bemaservices/PastoralCare/CareTypeList.ascx", "BEMA Services > Pastoral Care", "252EF3E6-876A-40BA-9F7A-0EEEC9A50200" );
            // Add Block to Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FC1531F6-5A3C-4F05-8E92-B2B66688B492", "", "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "Care Type List", "Main", "", "", 0, "7FD75626-52D5-4206-A1ED-265233EB19EE" );
            // Attrib for BlockType: Care Type List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of a care type.", 0, @"", "009755D4-2EA8-49E1-B179-622AFFFDBAAB" );
            // Attrib Value for Block:Care Type List, Attribute:Detail Page Page: Care Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "7FD75626-52D5-4206-A1ED-265233EB19EE", "009755D4-2EA8-49E1-B179-622AFFFDBAAB", @"9d03672e-a36a-402a-857a-a52fb66362b2" );

            // Page: Care Type Detail
            RockMigrationHelper.AddPage( "FC1531F6-5A3C-4F05-8E92-B2B66688B492", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Care Type Detail", "", "9D03672E-A36A-402A-857A-A52FB66362B2", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Care Type Detail", "Displays the details of the given Care Type for editing.", "~/Plugins/com_bemaservices/PastoralCare/CareTypeDetail.ascx", "BEMA Services > Pastoral Care", "7B44CB2C-67D2-4AD0-9CC5-975692CB6802" );
            // Add Block to Page: Care Type Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9D03672E-A36A-402A-857A-A52FB66362B2", "", "7B44CB2C-67D2-4AD0-9CC5-975692CB6802", "Care Type Detail", "Main", "", "", 0, "3019A517-A6FB-464F-96D0-82B554224C22" );

            // Page: Care Item Detail
            RockMigrationHelper.AddPage( "EB812DF7-163B-418F-8C39-9C808A2CDF71", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Care Item Detail", "", "9C2E82D3-5F23-4854-AD27-B8EA07183EDF", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Care Item Detail", "Displays the details of the given care item for editing.", "~/Plugins/com_bemaservices/PastoralCare/CareItemDetail.ascx", "BEMA Services > Pastoral Care", "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2" );
            // Add Block to Page: Care Item Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "9C2E82D3-5F23-4854-AD27-B8EA07183EDF", "", "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "Care Item Detail", "Main", "", "", 0, "9B2D7E4D-6CA3-4FB8-BB6E-26726FC8E5A4" );
            // Attrib for BlockType: Care Item Detail:Badges
            RockMigrationHelper.UpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "", "The person badges to display in this block.", 0, @"", "7ACA8972-EB2A-400C-8607-D7E185F85359" );
            // Attrib for BlockType: Care Item Detail:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 0, @"", "4BD88C6A-8D22-4466-B5C2-9DEF44170354" );
            // Attrib Value for Block:Care Item Detail, Attribute:Badges Page: Care Item Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9B2D7E4D-6CA3-4FB8-BB6E-26726FC8E5A4", "7ACA8972-EB2A-400C-8607-D7E185F85359", @"66972bff-42cd-49ab-9a7a-e1b9deca4ebf,b4b336ce-137e-44be-9123-27740d0064c2" );
            // Attrib Value for Block:Care Item Detail, Attribute:Person Profile Page Page: Care Item Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9B2D7E4D-6CA3-4FB8-BB6E-26726FC8E5A4", "4BD88C6A-8D22-4466-B5C2-9DEF44170354", @"df6eafc1-f139-4bf4-9249-df146c14f93d" );

            // Page: Pastoral Care
            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Pastoral Care", "", "DF6EAFC1-F139-4BF4-9249-DF146C14F93D", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "DF6EAFC1-F139-4BF4-9249-DF146C14F93D", "Person/{PersonId}/PastoralCare" );
            RockMigrationHelper.UpdateBlockType( "Care Item List", "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user.", "~/Plugins/com_bemaservices/PastoralCare/CareItemList.ascx", "BEMA Services > Pastoral Care", "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A" );
            // Add Block to Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "DF6EAFC1-F139-4BF4-9249-DF146C14F93D", "", "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "Care Item List", "SectionC1", "", "", 0, "F94C50E0-1E98-4F8B-9F39-6D052582DBC5" );
            // Attrib for BlockType: Care Item List:Configuration Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "", "Page used to modify and create connection careTypes.", 0, @"", "056A2CCA-86F7-4480-B8C6-6CB14827340E" );
            // Attrib for BlockType: Care Item List:Entity Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "838211A9-6A9D-4523-8913-EEE15BDD05FA" );
            // Attrib for BlockType: Care Item List:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of an careItems.", 1, @"", "4FA7FEE2-8203-4001-89EA-3C796424469F" );
            // Attrib Value for Block:Care Item List, Attribute:Configuration Page Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F94C50E0-1E98-4F8B-9F39-6D052582DBC5", "056A2CCA-86F7-4480-B8C6-6CB14827340E", @"" );
            // Attrib Value for Block:Care Item List, Attribute:Detail Page Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F94C50E0-1E98-4F8B-9F39-6D052582DBC5", "4FA7FEE2-8203-4001-89EA-3C796424469F", @"9c2e82d3-5f23-4854-ad27-b8ea07183edf" );
            // Attrib Value for Block:Care Item List, Attribute:Entity Type Page: Pastoral Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F94C50E0-1E98-4F8B-9F39-6D052582DBC5", "838211A9-6A9D-4523-8913-EEE15BDD05FA", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Add/Update PageContext for Page:Pastoral Care, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "DF6EAFC1-F139-4BF4-9249-DF146C14F93D", "Rock.Model.Person", "PersonId", "C54A3089-F8A9-416F-B752-04408B58A479" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "838211A9-6A9D-4523-8913-EEE15BDD05FA" );
            RockMigrationHelper.DeleteAttribute( "4FA7FEE2-8203-4001-89EA-3C796424469F" );
            RockMigrationHelper.DeleteAttribute( "056A2CCA-86F7-4480-B8C6-6CB14827340E" );
            RockMigrationHelper.DeleteBlock( "F94C50E0-1E98-4F8B-9F39-6D052582DBC5" );
            RockMigrationHelper.DeleteBlockType( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A" );
            RockMigrationHelper.DeletePage( "DF6EAFC1-F139-4BF4-9249-DF146C14F93D" ); //  Page: Pastoral Care

            // Delete PageContext for Page:Pastoral Care, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "C54A3089-F8A9-416F-B752-04408B58A479" );

            RockMigrationHelper.DeleteAttribute( "4BD88C6A-8D22-4466-B5C2-9DEF44170354" );
            RockMigrationHelper.DeleteAttribute( "7ACA8972-EB2A-400C-8607-D7E185F85359" );
            RockMigrationHelper.DeleteBlock( "9B2D7E4D-6CA3-4FB8-BB6E-26726FC8E5A4" );
            RockMigrationHelper.DeleteBlockType( "C2A7DC2E-81A9-4CE8-86D1-A90F87DF36E2" );
            RockMigrationHelper.DeletePage( "9C2E82D3-5F23-4854-AD27-B8EA07183EDF" ); //  Page: Care Item Detail

            RockMigrationHelper.DeleteBlock( "3019A517-A6FB-464F-96D0-82B554224C22" );
            RockMigrationHelper.DeleteBlockType( "7B44CB2C-67D2-4AD0-9CC5-975692CB6802" );
            RockMigrationHelper.DeletePage( "9D03672E-A36A-402A-857A-A52FB66362B2" ); //  Page: Care Type Detail

            RockMigrationHelper.DeleteAttribute( "009755D4-2EA8-49E1-B179-622AFFFDBAAB" );
            RockMigrationHelper.DeleteBlock( "7FD75626-52D5-4206-A1ED-265233EB19EE" );
            RockMigrationHelper.DeleteBlockType( "252EF3E6-876A-40BA-9F7A-0EEEC9A50200" );
            RockMigrationHelper.DeletePage( "FC1531F6-5A3C-4F05-8E92-B2B66688B492" ); //  Page: Care Types

            RockMigrationHelper.DeleteAttribute( "838211A9-6A9D-4523-8913-EEE15BDD05FA" );
            RockMigrationHelper.DeleteAttribute( "4FA7FEE2-8203-4001-89EA-3C796424469F" );
            RockMigrationHelper.DeleteAttribute( "056A2CCA-86F7-4480-B8C6-6CB14827340E" );
            RockMigrationHelper.DeleteBlock( "C5AD1E56-608F-4D53-AC97-F0749B2362FF" );
            RockMigrationHelper.DeleteBlockType( "A7A6FFAB-FD70-4BDB-8948-CAE86727BE7A" );
            RockMigrationHelper.DeletePage( "EB812DF7-163B-418F-8C39-9C808A2CDF71" ); //  Page: Pastoral Care


        }
    }
}
