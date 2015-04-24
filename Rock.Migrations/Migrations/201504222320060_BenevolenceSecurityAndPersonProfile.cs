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
    using Rock.Security;

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

            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Benevolence", "", "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Benevolence Request Detail", "", "648CA58C-EB12-4479-9994-F064070E3A32", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Transaction Yearly Summary Lava", "Presents a summary of financial transactions broke out by year and account using lava", "~/Blocks/Finance/TransactionYearlySummaryLava.ascx", "Finance", "535307C8-77D1-44F8-AD4D-1577572B6D26" );

            // Add Block to Page: Benevolence, Site: Rock RMS (Person Profile)
            RockMigrationHelper.AddBlock( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "", "3131C55A-8753-435F-85F3-DF777EFBD1C8", "Benevolence Request List", "SectionC1", "", "", 0, "52EDDE7F-6808-4912-A73B-94AE0939DD48" );

            // Add Block to Page: Benevolence Request Detail, Site: Rock RMS (Person Profile)
            RockMigrationHelper.AddBlock( "648CA58C-EB12-4479-9994-F064070E3A32", "", "34275D0E-BC7E-4A9C-913E-623D086159A1", "Benevolence Request Detail", "Main", "", "", 0, "27515E57-CE16-4853-AA94-E995547BF166" );

            // Attrib Value for Block:Benevolence Request List, Attribute:Detail Page Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52EDDE7F-6808-4912-A73B-94AE0939DD48", "E2C90243-A79A-4DAD-9301-07F6DF095CDB", @"648ca58c-eb12-4479-9994-f064070e3a32" );

            // Attrib Value for Block:Benevolence Request List, Attribute:Case Worker Group Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "52EDDE7F-6808-4912-A73B-94AE0939DD48", "576E31E0-EE40-4A89-93AE-5CCF1F45D21F", @"26e7148c-2059-4f45-bcfe-32230a12f0dc" );

            // Add/Update PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "Rock.Model.Person", "PersonId", "1863B413-642D-423B-B82B-8A0BCED3839C" );

            RockMigrationHelper.AddPageRoute( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "Person/{PersonId}/Benevolence" ); // for Page:Benevolence

            // Add Block to Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "535307C8-77D1-44F8-AD4D-1577572B6D26", "Transaction Yearly Summary", "SectionC1", "", "", 0, "EF8BB598-E991-421F-96A1-3019B3D855A6" );

            // reorder Person Contributions blocks
            Sql( @"
update Block set [Order] = 0 where [Guid] = 'EF8BB598-E991-421F-96A1-3019B3D855A6'
update Block set [Order] = 1 where [Guid] = 'B33DF8C4-29B2-4DC5-B182-61FC255B01C0'
update Block set [Order] = 2 where [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'
update Block set [Order] = 3 where [Guid] = '7C698D61-81C9-4942-BFE3-9839130C1A3E'
" );


            // Attrib for BlockType: Transaction Yearly Summary Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "535307C8-77D1-44F8-AD4D-1577572B6D26", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 2, @"False", "22C2445C-CE91-4791-A00C-68BB604C55CB" );

            // Attrib for BlockType: Transaction Yearly Summary Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "535307C8-77D1-44F8-AD4D-1577572B6D26", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the transaction summary.", 1, @"{% include '~~/Assets/Lava/TransactionYearlySummary.lava' %}", "D33B39C7-F81C-4D0C-8A63-E0489FEA4DF4" );

            // Add/Update PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2", "Rock.Model.Person", "PersonId", "E79EDE83-3AD6-4AD0-81BB-1C2E3EEB2DC3" );

            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.SCHEDULED_TRANSACTIONS, 0, Authorization.VIEW, false, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "D4ADD68A-CA20-4609-A4D6-D64761997A2C" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.PLEDGE_LIST, 0, Authorization.VIEW, false, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "1B8CB618-C716-4472-817C-8A6D2692DDF1" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.TRANSACTIONS, 0, Authorization.VIEW, false, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "7E60D020-5ABE-43ED-BB21-D3F1E90DC4D5" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BATCHES, 0, Authorization.VIEW, false, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "A3DAC27E-2625-4FF7-BDBC-B12FD140D810" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.ADMINISTRATION_FINANCE, 0, Authorization.VIEW, false, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "8ACEAD60-1954-4F59-9321-D91B3EEC4E08" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.FINANCE, 2, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "815CB9D8-C466-489A-90A7-5C4040A1431C" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityRoleGroup( Rock.SystemGuid.Group.GROUP_BENEVOLENCE );

            // Remove Block: Benevolence Request Detail, from Page: Benevolence Request Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "27515E57-CE16-4853-AA94-E995547BF166" );
            // Remove Block: Benevolence Request List, from Page: Benevolence, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "52EDDE7F-6808-4912-A73B-94AE0939DD48" );

            RockMigrationHelper.DeletePage( "648CA58C-EB12-4479-9994-F064070E3A32" ); //  Page: Benevolence Request Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "15FA4176-1C8E-409D-8B47-85ADA35DE5D2" ); //  Page: Benevolence, Layout: PersonDetail, Site: Rock RMS

            // Delete PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "1863B413-642D-423B-B82B-8A0BCED3839C" );

            // Remove Block: Transaction Yearly Summary, from Page: Contributions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EF8BB598-E991-421F-96A1-3019B3D855A6" );
            // Attrib for BlockType: Transaction Yearly Summary Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "D33B39C7-F81C-4D0C-8A63-E0489FEA4DF4" );
            // Attrib for BlockType: Transaction Yearly Summary Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "22C2445C-CE91-4791-A00C-68BB604C55CB" );
            // Delete PageContext for Page:Benevolence, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "E79EDE83-3AD6-4AD0-81BB-1C2E3EEB2DC3" );

            RockMigrationHelper.DeleteBlockType( "535307C8-77D1-44F8-AD4D-1577572B6D26" ); // Transaction Yearly Summary Lava

            RockMigrationHelper.DeleteBlockType( "535307C8-77D1-44F8-AD4D-1577572B6D26" ); // Transaction Yearly Summary Lava

        }
    }
}
