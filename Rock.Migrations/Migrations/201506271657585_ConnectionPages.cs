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
    public partial class ConnectionPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Page: Connections
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connections", "", "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "My Connection Opportunities", "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user.", "~/Blocks/Involvement/MyConnectionOpportunities.ascx", "Involvement", "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" );
            RockMigrationHelper.AddBlock( "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "", "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "My Connection Opportunities", "Main", "", "", 0, "80710A2C-9B90-40AE-B887-B885AAA43538" );

            RockMigrationHelper.AddBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Configuration Page", "ConfigurationPage", "", "Page used to modify and create connection opportunities.", 0, @"", "70C77D07-4D28-4E71-BD0F-3DF0F61CE12D" );
            RockMigrationHelper.AddBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of an requests.", 0, @"", "E2A64E17-7310-4ED4-85F4-65D0ED97A513" );
            RockMigrationHelper.AddBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Connection Type Id", "ConnectionTypeId", "", "The Id of the connection type that determines the opportunities listed.", 0, @"1", "06DD6554-AD0E-40E5-9F36-925107FC5642" );

            RockMigrationHelper.AddBlockAttributeValue( "80710A2C-9B90-40AE-B887-B885AAA43538", "70C77D07-4D28-4E71-BD0F-3DF0F61CE12D", @"9cc19684-7ad2-4d4e-a7c4-10dae56e7fa6" ); // Configuration Page
            RockMigrationHelper.AddBlockAttributeValue( "80710A2C-9B90-40AE-B887-B885AAA43538", "E2A64E17-7310-4ED4-85F4-65D0ED97A513", @"50f04e77-8d3b-4268-80ab-bc15dd6cb262" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "80710A2C-9B90-40AE-B887-B885AAA43538", "06DD6554-AD0E-40E5-9F36-925107FC5642", @"1" ); // Connection Type Id

            // Page: Connection Request Detail
            RockMigrationHelper.AddPage( "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Request Detail", "", "50F04E77-8D3B-4268-80AB-BC15DD6CB262", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Connection Request Detail", "Displays the details of the given connection request for editing state, status, etc.", "~/Blocks/Involvement/ConnectionRequestDetail.ascx", "Involvement", "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2" );
            RockMigrationHelper.AddBlock( "50F04E77-8D3B-4268-80AB-BC15DD6CB262", "", "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "Connection Request Detail", "Main", "", "", 0, "94187C5A-7F6A-4D45-B5C2-C3C8673E8817" );

            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manual Workflow Page", "ManualWorkflowPage", "", "Page used to manually start a workflow.", 0, @"", "6C0260B6-F7FD-4C78-AEFB-836EC1E96022" );
            RockMigrationHelper.AddBlockTypeAttribute( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Configuration Page", "WorkflowConfigurationPage", "", "Page used to view and edit configuration of a workflow.", 0, @"", "5280472E-303C-446F-BD88-2490417220AD" );

            // Page: Connection Types
            RockMigrationHelper.AddPage( "530860ED-BC73-4A43-8E7C-69533EF2B6AD", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Types", "", "9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Connection Type List", "Block to display the connection types.", "~/Blocks/Involvement/ConnectionTypeList.ascx", "Involvement", "D25F0658-3038-45B0-A6AA-DFFC4053EE13" );
            RockMigrationHelper.AddBlock( "9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "", "D25F0658-3038-45B0-A6AA-DFFC4053EE13", "Connection Type List", "Main", "", "", 0, "C3333691-9476-4DF6-A07C-C985857EB976" );

            RockMigrationHelper.AddBlockTypeAttribute( "D25F0658-3038-45B0-A6AA-DFFC4053EE13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "Page used to view details of a connection type.", 0, @"", "B491987B-7141-457F-927E-46501E50382A" );

            RockMigrationHelper.AddBlockAttributeValue( "C3333691-9476-4DF6-A07C-C985857EB976", "B491987B-7141-457F-927E-46501E50382A", @"deff1afe-2c33-4e56-b0f5-be3b75224186" ); // Detail Page

            // Page: Connection Type Detail
            RockMigrationHelper.AddPage( "9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Type Detail", "", "DEFF1AFE-2C33-4E56-B0F5-BE3B75224186", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Connection Opportunity List", "Lists all the opportunities for a given connection type.", "~/Blocks/Involvement/ConnectionOpportunityList.ascx", "Involvement", "481AE184-4654-48FB-A2B4-90F6604B59B8" );
            RockMigrationHelper.UpdateBlockType( "Connection Type Detail", "Displays the details of the given Connection Type for editing.", "~/Blocks/Involvement/ConnectionTypeDetail.ascx", "Involvement", "6CB76282-DD57-4AC1-85EF-05A5E65CF6D6" );
            RockMigrationHelper.AddBlock( "DEFF1AFE-2C33-4E56-B0F5-BE3B75224186", "", "6CB76282-DD57-4AC1-85EF-05A5E65CF6D6", "Connection Type Detail", "Main", "", "", 0, "0D66ADEF-07B2-4F23-8AF3-9D6B6420CEA4" );
            RockMigrationHelper.AddBlock( "DEFF1AFE-2C33-4E56-B0F5-BE3B75224186", "", "481AE184-4654-48FB-A2B4-90F6604B59B8", "Connection Opportunity List", "Main", "", "", 1, "5A078DC0-9E85-4429-BC72-29003B81D8B5" );

            RockMigrationHelper.AddBlockTypeAttribute( "481AE184-4654-48FB-A2B4-90F6604B59B8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "4C367E94-C1A2-4AD0-BC0F-364805B11BF2" );

            RockMigrationHelper.AddBlockAttributeValue( "5A078DC0-9E85-4429-BC72-29003B81D8B5", "4C367E94-C1A2-4AD0-BC0F-364805B11BF2", @"0e5797ff-a507-4e02-891f-b80af353e585" ); // Detail Page

            // Page: Connection Opportunity Detail
            RockMigrationHelper.AddPage( "DEFF1AFE-2C33-4E56-B0F5-BE3B75224186", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connection Opportunity Detail", "", "0E5797FF-A507-4E02-891F-B80AF353E585", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Connection Opportunity Detail", "Displays the details of the given connection opportunity.", "~/Blocks/Involvement/ConnectionOpportunityDetail.ascx", "Involvement", "216E2EE6-4E2D-4D0F-AA36-AB808F565C48" );
            RockMigrationHelper.AddBlock( "0E5797FF-A507-4E02-891F-B80AF353E585", "", "216E2EE6-4E2D-4D0F-AA36-AB808F565C48", "Connection Opportunity Detail", "Main", "", "", 0, "D9C657FB-1426-44FA-9B1E-88CECDB52A7F" );

            RockMigrationHelper.AddBlockTypeAttribute( "216E2EE6-4E2D-4D0F-AA36-AB808F565C48", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 2, @"True", "7EA52884-F69A-4A43-BE58-1B6931AF18CA" );

            RockMigrationHelper.AddBlockAttributeValue( "D9C657FB-1426-44FA-9B1E-88CECDB52A7F", "7EA52884-F69A-4A43-BE58-1B6931AF18CA", @"True" ); // Show Edit
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "7EA52884-F69A-4A43-BE58-1B6931AF18CA" );
            RockMigrationHelper.DeleteBlock( "D9C657FB-1426-44FA-9B1E-88CECDB52A7F" );
            RockMigrationHelper.DeleteBlockType( "216E2EE6-4E2D-4D0F-AA36-AB808F565C48" );
            RockMigrationHelper.DeletePage( "0E5797FF-A507-4E02-891F-B80AF353E585" ); //  Page: Connection Opportunity Detail

            RockMigrationHelper.DeleteAttribute( "4C367E94-C1A2-4AD0-BC0F-364805B11BF2" );
            RockMigrationHelper.DeleteBlock( "5A078DC0-9E85-4429-BC72-29003B81D8B5" );
            RockMigrationHelper.DeleteBlock( "0D66ADEF-07B2-4F23-8AF3-9D6B6420CEA4" );
            RockMigrationHelper.DeleteBlockType( "6CB76282-DD57-4AC1-85EF-05A5E65CF6D6" );
            RockMigrationHelper.DeleteBlockType( "481AE184-4654-48FB-A2B4-90F6604B59B8" );
            RockMigrationHelper.DeletePage( "DEFF1AFE-2C33-4E56-B0F5-BE3B75224186" ); //  Page: Connection Type Detail

            RockMigrationHelper.DeleteAttribute( "B491987B-7141-457F-927E-46501E50382A" );
            RockMigrationHelper.DeleteBlock( "C3333691-9476-4DF6-A07C-C985857EB976" );
            RockMigrationHelper.DeleteBlockType( "D25F0658-3038-45B0-A6AA-DFFC4053EE13" );
            RockMigrationHelper.DeletePage( "9CC19684-7AD2-4D4E-A7C4-10DAE56E7FA6" ); //  Page: Connection Types

            RockMigrationHelper.DeleteAttribute( "5280472E-303C-446F-BD88-2490417220AD" );
            RockMigrationHelper.DeleteAttribute( "6C0260B6-F7FD-4C78-AEFB-836EC1E96022" );
            RockMigrationHelper.DeleteBlock( "94187C5A-7F6A-4D45-B5C2-C3C8673E8817" );
            RockMigrationHelper.DeleteBlockType( "A7961C9C-2EF5-44DF-BEA5-C334B42A90E2" );
            RockMigrationHelper.DeletePage( "50F04E77-8D3B-4268-80AB-BC15DD6CB262" ); //  Page: Connection Request Detail

            RockMigrationHelper.DeleteAttribute( "06DD6554-AD0E-40E5-9F36-925107FC5642" );
            RockMigrationHelper.DeleteAttribute( "E2A64E17-7310-4ED4-85F4-65D0ED97A513" );
            RockMigrationHelper.DeleteAttribute( "70C77D07-4D28-4E71-BD0F-3DF0F61CE12D" );
            RockMigrationHelper.DeleteBlock( "80710A2C-9B90-40AE-B887-B885AAA43538" );
            RockMigrationHelper.DeleteBlockType( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A" );
            RockMigrationHelper.DeletePage( "530860ED-BC73-4A43-8E7C-69533EF2B6AD" ); //  Page: Connections
        }
    }
}
