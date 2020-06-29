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
    public partial class Rollup_0303 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
            Issue3478();
            AddUpgradeSQLServerMessageUp();
            FixAgeCalcErrorUp();
            AttendanceAnalyticsSortAttendeesByCampus();
            UpdateSampleDataUrl();
            RemoveGlobalAttributeRouteDomainMatching();
            RemoveObsoleteServiceJobs();
            PageIconMigration();
            AddPersistedDatasetPagesUp();
            AddBusinessMergePageUp();
            AddConnectionRequestWorkflowTriggersJobUp();
            RemoveIsDeceasedFilterFromADataView();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddConnectionRequestWorkflowTriggersJobDown();
            AddBusinessMergePageDown();
            AddPersistedDatasetPagesDown();
            FixAgeCalcErrorDown();
            AddUpgradeSQLServerMessageDown();
            CodeGenMigrationsDown();
        }

        /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            RockMigrationHelper.UpdateBlockType("Page Parameter Filter","Filter block that passes the filter values as query string parameters.","~/Blocks/Reporting/PageParameterFilter.ascx","Reporting","6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7");
            // Attrib for BlockType: Attributes:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5431F4A8-553A-431E-8561-7C7FF4120B4D" );
            // Attrib for BlockType: Attributes:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C0FED8AD-799C-4E9A-B5A7-C4CD06F041E7" );
            // Attrib for BlockType: Components:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "21F5F466-59BC-40B2-8D73-7314D936C3CB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4AF30CE4-FE81-4612-AC64-5CB9902B8583" );
            // Attrib for BlockType: Components:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "21F5F466-59BC-40B2-8D73-7314D936C3CB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C29E9E43-B246-4CBB-9A8A-274C8C377FDF" );
            // Attrib for BlockType: Group List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5E4612EB-E1DA-4606-97CC-09D11331FE84" );
            // Attrib for BlockType: Group List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "D7AE5225-28BF-4954-9D04-782AC76CBF4E" );
            // Attrib for BlockType: Campus List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "894B4118-83B9-449B-BF75-3D510C68829A" );
            // Attrib for BlockType: Campus List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "29B903D8-3C98-442F-BC6B-E6C2F29F388C" );
            // Attrib for BlockType: Scheduled Job List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6D3F924E-BDD0-4C78-981E-B698351E75AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2E3161E9-5C64-4D17-8D6C-18A295D40C92" );
            // Attrib for BlockType: Scheduled Job List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6D3F924E-BDD0-4C78-981E-B698351E75AD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "FB603A69-2773-4A7B-A12A-3C2CF3979DA8" );
            // Attrib for BlockType: Site List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "348F6509-2AFD-4BC2-8B60-C508E2935A6E" );
            // Attrib for BlockType: Site List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "441D5A71-C250-4FF5-90C3-DEEAD3AC028D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EB67C256-C509-4327-B2E2-FD3CA37CA59C" );
            // Attrib for BlockType: Group Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "556EAD07-E0E9-4959-8D0E-6584CD6FE2F3" );
            // Attrib for BlockType: Group Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "80306BB1-FE4B-436F-AC7A-691CF0BC0F5E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A60FF939-4225-4E46-9B8C-246C7D4EB601" );
            // Attrib for BlockType: Defined Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F21610E0-E22C-48D9-AC0E-CCE1149267E5" );
            // Attrib for BlockType: Defined Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5470C9C4-09C1-439F-AA56-3524047497EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "66FA42CD-1FD5-4F9D-B5E9-A7A2FC105607" );
            // Attrib for BlockType: Route List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E92E3C51-EB14-414D-BC68-9061FEB92A22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0E5DC54A-9090-42F0-9B8A-17E59E33ACD9" );
            // Attrib for BlockType: Route List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E92E3C51-EB14-414D-BC68-9061FEB92A22", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0DD924BF-99DF-4463-8E64-70E10DF0CF89" );
            // Attrib for BlockType: Workflow List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "620EDCED-E6FA-41CF-9733-550389C7B555" );
            // Attrib for BlockType: Workflow List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4D534CC7-BEA0-45C5-A5B3-AD070F7FADF6" );
            // Attrib for BlockType: Group Member List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D4D7DCB4-F481-48F2-9992-E35C6F4E8C0B" );
            // Attrib for BlockType: Group Member List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2053BCCE-A4CA-49F0-BC94-4EA0FFA04E91" );
            // Attrib for BlockType: Block Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "06939EBC-FA3B-42C8-B2EE-933D0A6697D6" );
            // Attrib for BlockType: Block Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "214DB447-5CDD-4A46-BF6B-1DE0BB109180" );
            // Attrib for BlockType: Prayer Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "05400FF0-BD51-4C64-9E96-45C0DA682090" );
            // Attrib for BlockType: Prayer Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1C62F958-E94C-4A61-B513-DA0E239B7519" );
            // Attrib for BlockType: Prayer Comment List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "CC9B09CB-9C14-44FD-99D1-AA4AEF1D4455" );
            // Attrib for BlockType: Prayer Comment List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "74323918-73DC-4D03-87BB-7D0D158DA8F8" );
            // Attrib for BlockType: Binary File Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0926B82C-CBA2-4943-962E-F788C8A80037", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "310285D8-3021-43C8-9713-EAA70FE1B22C" );
            // Attrib for BlockType: Binary File Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0926B82C-CBA2-4943-962E-F788C8A80037", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "53944C14-9BCF-4B37-8012-D48C6B15FAC9" );
            // Attrib for BlockType: Workflow Trigger List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72F48121-2CE2-4696-840C-CF404EAF7EEE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "EFB628F1-6F2F-469C-9D79-7D017E90BDDF" );
            // Attrib for BlockType: Workflow Trigger List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72F48121-2CE2-4696-840C-CF404EAF7EEE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8B830DF5-2B92-456E-9735-0EDAF3029A63" );
            // Attrib for BlockType: Device List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "32183AD6-01CB-4533-858B-1BDA5120AAD5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "491465C5-9110-4930-9E8B-9094174459EC" );
            // Attrib for BlockType: Device List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "32183AD6-01CB-4533-858B-1BDA5120AAD5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EDD0BF18-5923-4F91-86A2-23F81D7208A1" );
            // Attrib for BlockType: Binary File List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "26541C8A-9E54-4723-A739-21FAA5191014", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A2983BB0-1FAD-4D5F-B04E-F29EC28761E9" );
            // Attrib for BlockType: Binary File List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "26541C8A-9E54-4723-A739-21FAA5191014", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3AA43986-A640-4EC8-8C96-72D57987DC44" );
            // Attrib for BlockType: Pledge List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "04C7A203-7125-452E-91C2-D11DD4A782CC" );
            // Attrib for BlockType: Pledge List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EB314955-A5AE-492F-B350-A2E4046B6DDA" );
            // Attrib for BlockType: Batch List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "1C297F0E-DA0D-45FB-9FA3-57B5346DEC9D" );
            // Attrib for BlockType: Batch List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "68CA768F-4159-4C43-B4CE-DEDF320CD242" );
            // Attrib for BlockType: Transaction List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "64389018-EB2C-4592-B764-02BEDCC33A91" );
            // Attrib for BlockType: Transaction List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E04320BC-67C3-452D-9EF6-D74D8C177154", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "12749FBF-9A92-40FA-A7A4-85BECA1ED52A" );
            // Attrib for BlockType: Communication List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56ABBD0F-8F62-4094-88B3-161E71F21419", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "EE559663-3A16-4821-BA46-FD395AF7EDA1" );
            // Attrib for BlockType: Communication List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "56ABBD0F-8F62-4094-88B3-161E71F21419", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "37C90DE1-0EB3-4EC7-8569-AE0D93F0C440" );
            // Attrib for BlockType: Exception List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "C697EF19-61C8-45C6-B687-D22C851A3294" );
            // Attrib for BlockType: Exception List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6302B319-9830-4BE3-A402-17801C88F7E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8F28C102-0321-4B65-A9D5-E1A77872128A" );
            // Attrib for BlockType: Schedule List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "41AB15D5-7A4C-4D28-8C9F-671BFB0169CA" );
            // Attrib for BlockType: Schedule List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C1B934D1-2139-471E-B2B8-B22FF4499B2F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "295A1429-9581-49CF-87D9-9FA912707646" );
            // Attrib for BlockType: Scheduled Transaction List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "45F47A89-F906-4143-A895-5D53F66B164A" );
            // Attrib for BlockType: Scheduled Transaction List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6AF171B4-4FFA-45BA-8C93-DC5F4CA18032" );
            // Attrib for BlockType: Schedule Builder:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A7142744-9EFA-4E13-90DD-534678638F5E" );
            // Attrib for BlockType: Schedule Builder:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8EEB4117-F746-409D-B00A-5786A3FDCBD1" );
            // Attrib for BlockType: Tag List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "23D81607-52D7-461A-A7F2-9B13B40572C7" );
            // Attrib for BlockType: Tag List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1704B351-3416-497F-9626-0D94B3A968C7" );
            // Attrib for BlockType: Defined Value List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0A3F078E-8A2A-4E9D-9763-3758E123E042" );
            // Attrib for BlockType: Defined Value List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "80765648-83B0-4B75-A296-851384C41CAB" );
            // Attrib for BlockType: Layout List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5996BF81-F2E2-4702-B401-B0B1B6667DAE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "13F0BA8A-F85D-41FC-9497-720215592553" );
            // Attrib for BlockType: Layout List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5996BF81-F2E2-4702-B401-B0B1B6667DAE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6FF16D9F-AAAD-461A-B501-013DEB53B434" );
            // Attrib for BlockType: External Application List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "850A0541-D31A-4559-94D1-9DAD5F52EFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7D46D41C-F552-47D2-ADAC-ABE4E01BF1EC" );
            // Attrib for BlockType: External Application List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "850A0541-D31A-4559-94D1-9DAD5F52EFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7CC2907E-1AD5-4EBD-8009-A1D8F4A7B9F4" );
            // Attrib for BlockType: Account List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8A5BDA9F-8FCE-4A1E-837C-1816FB14D67B" );
            // Attrib for BlockType: Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7F18F54E-3B2A-42A1-8449-1A3909136FE6" );
            // Attrib for BlockType: REST Action List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5A8BCD0B-3332-425E-B389-D512122D1B5C" );
            // Attrib for BlockType: REST Action List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "20AD75DD-0DF3-49E9-9DB1-8537C12B1664", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F0AFBA55-DFE0-4039-8875-6FF1F59F5E68" );
            // Attrib for BlockType: REST Controller List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A0F1FBFF-1272-408B-879C-7F86DC355D77" );
            // Attrib for BlockType: REST Controller List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7BF616C1-CE1D-4EF0-B56F-B9810B811192", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4D3C450B-8F97-48A8-BF7B-13B5C78A6E7A" );
            // Attrib for BlockType: Audit Information List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "102FE63E-815F-4845-B3B3-7238E2445BF5" );
            // Attrib for BlockType: Audit Information List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BB859A0A-A29A-49AE-8581-AF52F91DF221" );
            // Attrib for BlockType: Categories:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7F8257E2-0B7E-4447-BCB0-5D82DC13F356" );
            // Attrib for BlockType: Categories:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "620FC4A2-6587-409F-8972-22065919D9AC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "547BF211-7B74-4CE2-A72B-561B4715D230" );
            // Attrib for BlockType: User Login List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B3FE8591-ECF3-4493-8184-945EAF517929" );
            // Attrib for BlockType: User Login List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE06640D-C1BA-4ACE-AF03-8D733FD3247C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E6463229-119B-448A-841A-632B1F260E33" );
            // Attrib for BlockType: Page List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "64AA17D1-4027-4779-BFF7-9C874ED20316" );
            // Attrib for BlockType: Page List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9847FE8-5279-4CC4-BD69-8A71B2F1ED70", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "665625F7-0528-4EF4-83C4-949B58A45E47" );
            // Attrib for BlockType: Attendance History:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "21FFA70E-18B3-4148-8FC4-F941100B49B8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "1D637F4D-9B35-4F0B-95BA-F77EDF43D927" );
            // Attrib for BlockType: Attendance History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "21FFA70E-18B3-4148-8FC4-F941100B49B8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "92E5A8CD-D3DF-4126-9D7E-370ABF314AAF" );
            // Attrib for BlockType: Layout Block List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "ECFA346C-DB15-4128-A30A-F687A19DAF82" );
            // Attrib for BlockType: Layout Block List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1580D3A8-3EC5-4A5A-899E-8D43264AB017" );
            // Attrib for BlockType: Badge List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8CCD577-2200-44C5-9073-FD16F174D364", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4A4B6713-D6DB-4CD0-B929-B83A4865BD24" );
            // Attrib for BlockType: Badge List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D8CCD577-2200-44C5-9073-FD16F174D364", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "89E483A8-7873-4392-B488-275AB6ECE2BE" );
            // Attrib for BlockType: System Email List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2645A264-D5E5-43E8-8FE2-D351F3D5435B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A329DEB6-8AC0-4A5C-98BB-9696402E7EF5" );
            // Attrib for BlockType: System Email List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2645A264-D5E5-43E8-8FE2-D351F3D5435B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C0C0DDF6-499F-40C2-99E9-BB3FBA80F875" );
            // Attrib for BlockType: Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EACDBBD4-C355-4D38-B604-779BC55D3876", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "54149758-CE20-4172-8EBB-01ED8A94D8D5" );
            // Attrib for BlockType: Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EACDBBD4-C355-4D38-B604-779BC55D3876", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "15968808-AE85-45DD-93A5-863E8709B46B" );
            // Attrib for BlockType: Location List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0DAAA063-8FC3-4599-9C6E-1054D41A2F27" );
            // Attrib for BlockType: Location List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C455CED6-1D5E-4E69-A577-DFEAD64268B8" );
            // Attrib for BlockType: Metric Value List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E40A1526-04D0-42A0-B275-D1AE161E2E57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "78D52A97-B7EC-46F5-B333-CD162C401BD5" );
            // Attrib for BlockType: Metric Value List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E40A1526-04D0-42A0-B275-D1AE161E2E57", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E159BE04-8B85-493C-89F2-FDD14FA33EF9" );
            // Attrib for BlockType: Business List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1ACCF349-73A5-4568-B801-2A6A620791D9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "31FBEC02-1CF4-45F4-8CA2-8DA2F46F80A0" );
            // Attrib for BlockType: Business List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1ACCF349-73A5-4568-B801-2A6A620791D9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "972BE9E4-9DB1-4C2B-B4E4-91ADDFA65DFB" );
            // Attrib for BlockType: Rest Key List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "AA6FCF53-6036-46DB-BB9C-89B607D7E676" );
            // Attrib for BlockType: Rest Key List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C4FBF612-C1F6-428B-97FD-8AB0B8EA31FC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F4E145B2-C0DF-4156-9163-1DF95DB96600" );
            // Attrib for BlockType: Person Following List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD548744-DC6D-4870-9FED-BB9EA24E709B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4E36F54E-1CDA-4C49-AE4B-4E84BE95B5F7" );
            // Attrib for BlockType: Person Following List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BD548744-DC6D-4870-9FED-BB9EA24E709B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "8EC047EE-13F6-4C82-88D7-C942144B34E0" );
            // Attrib for BlockType: Stark List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E333D1CC-CB55-4E73-8568-41DAD296971C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7CA449C6-6556-4F34-8E14-48286CF80BFE" );
            // Attrib for BlockType: Stark List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E333D1CC-CB55-4E73-8568-41DAD296971C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AC6B87B6-961B-4597-938B-03D64F6FA674" );
            // Attrib for BlockType: Bank Account List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C4191011-0391-43DF-9A9D-BE4987C679A4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F5C7D70E-B4AB-418C-AC3E-0E9D4AAF56FE" );
            // Attrib for BlockType: Bank Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C4191011-0391-43DF-9A9D-BE4987C679A4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "256D8C3E-2CC1-488E-AAC2-D3DCAE46A701" );
            // Attrib for BlockType: Person Duplicate Detail:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "467BAFEF-C367-400E-9FA3-04A3D4328959" );
            // Attrib for BlockType: Person Duplicate Detail:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A65CF2F8-93A4-4AC6-9018-D7C6996D9017", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "139D6001-F973-491B-B265-A4D33139BE44" );
            // Attrib for BlockType: Person Duplicate List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "240AAB19-5481-460F-8256-89016927F680" );
            // Attrib for BlockType: Person Duplicate List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "12D89810-23EB-4818-99A2-E076097DD979", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EF0750F8-7B01-4FB1-A054-5BA77723F8FF" );
            // Attrib for BlockType: Report List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37D29989-F7CA-4D51-925A-378DB73ED53D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "81362512-D452-422E-8791-E4FC359EDA37" );
            // Attrib for BlockType: Report List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "37D29989-F7CA-4D51-925A-378DB73ED53D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "91425E91-07F2-406B-AAA9-0A613F7C1229" );
            // Attrib for BlockType: Content Channel Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A580027F-56DB-43B0-AAD6-7C2B8A952012", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "925AE569-047F-4494-AA7D-0D901A4355BA" );
            // Attrib for BlockType: Content Channel Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A580027F-56DB-43B0-AAD6-7C2B8A952012", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "7260D29D-D4F0-4687-917E-3B74F231EBD5" );
            // Attrib for BlockType: Content Channel List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "991507B6-D222-45E5-BA0D-B61EA72DFB64", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6B3781C1-0FD3-4C1E-B2D6-D5EFF1494F79" );
            // Attrib for BlockType: Content Channel List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "991507B6-D222-45E5-BA0D-B61EA72DFB64", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "006256B7-0DF0-45D4-A16C-459956741047" );
            // Attrib for BlockType: Content Channel Item List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BC0E5B7E-65A6-451A-AF03-4A24C47FAF18" );
            // Attrib for BlockType: Content Channel Item List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B995BE3F-A9EB-4A18-AE24-E93A8796AEDE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F1FEB0B3-AF7B-4FAA-A435-7066D8D7491B" );
            // Attrib for BlockType: Group Attendance List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "264678DD-59ED-482E-9DAB-7F1CBC1F9C67" );
            // Attrib for BlockType: Group Attendance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C547728-38C2-420A-8602-3CDAAC369247", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "AC59375E-DEE0-4465-B1DF-E25C8283E5AD" );
            // Attrib for BlockType: Benevolence Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "23E44310-68C4-45F9-8426-8D15BDCD4B14" );
            // Attrib for BlockType: Benevolence Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3131C55A-8753-435F-85F3-DF777EFBD1C8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DB8E2D32-BF2B-439D-A463-173139CE02AA" );
            // Attrib for BlockType: Merge Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA102F02-6DBB-42E6-BFEE-360E137B1411", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BB37E693-418D-45F3-B011-91E445B4781C" );
            // Attrib for BlockType: Merge Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA102F02-6DBB-42E6-BFEE-360E137B1411", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6C212B29-668A-4BF5-9322-8E107D306207" );
            // Attrib for BlockType: Gateway List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "32E89BAE-C085-40B3-B872-B62E25A62BDB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8DC98507-027B-4837-AE45-E035F227926D" );
            // Attrib for BlockType: Gateway List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "32E89BAE-C085-40B3-B872-B62E25A62BDB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C41B4A8D-7C61-49CD-AE42-9244303B3D7A" );
            // Attrib for BlockType: Group Requirement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B8CF900E-2007-4A38-BCF5-F33D35AFD38E" );
            // Attrib for BlockType: Group Requirement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2B9D97F9-A4FA-4324-850A-36E9C25B11AD" );
            // Attrib for BlockType: Calendar Event Item List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC8DFDC5-C177-4208-8ABA-1F85010FBBFF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2DDBEA1F-9401-456D-966F-40D11701B8A7" );
            // Attrib for BlockType: Calendar Event Item List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EC8DFDC5-C177-4208-8ABA-1F85010FBBFF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "64AABE0F-6D06-45CA-9E34-9F19589076AB" );
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "AE9062AA-2DB2-4C04-B598-51A4A2C0808B" );
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "4AD1343D-3116-49D4-9DBA-12F464D98323" );
            // Attrib for BlockType: Registration Instance List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "632F63A9-5629-4731-BE6A-AB534EDD9BC9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "A27BBCBB-01DF-4C38-A247-1F6E59FF363E" );
            // Attrib for BlockType: Registration Instance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "632F63A9-5629-4731-BE6A-AB534EDD9BC9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1A3EC3E1-99BF-4452-9854-7BBBE8BD4856" );
            // Attrib for BlockType: Connection Opportunity List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "481AE184-4654-48FB-A2B4-90F6604B59B8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B6B24DAD-B1A3-416C-96D1-78C79776C69D" );
            // Attrib for BlockType: Connection Opportunity List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "481AE184-4654-48FB-A2B4-90F6604B59B8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C3178F42-7D35-4817-9C5C-9C479209C5E9" );
            // Attrib for BlockType: Communication Recipient List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBEA5996-5695-4A42-A21C-29E11E711BE8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "4886BD88-A730-4959-81F2-E35F9149EE08" );
            // Attrib for BlockType: Communication Recipient List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EBEA5996-5695-4A42-A21C-29E11E711BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "86BE8D5C-9FB9-4FEA-8A15-A10E00E8A77A" );
            // Attrib for BlockType: Event List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2BF48893-C1C7-4291-A9FA-E5BAC98D1395" );
            // Attrib for BlockType: Event List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C3CEBAD-3CCB-493B-9CBA-9D0D33852050", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BADF8FC4-20C4-42C7-83CE-4B0BF1620DC7" );
            // Attrib for BlockType: Suggestion List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5F10E479-AC38-44D8-A14D-0A865D8A4C70" );
            // Attrib for BlockType: Suggestion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "93A21076-F296-4AAD-AA76-AD31CA6AE220" );
            // Attrib for BlockType: Person Suggestion List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "844BC0F6-735B-454A-AA46-CA3243C89F88" );
            // Attrib for BlockType: Person Suggestion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "68F09AD7-43A1-45B2-9C59-EF2FF6956E3A" );
            // Attrib for BlockType: Saved Account List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "8AC39C3F-BE58-457C-B173-91AC09EFB796" );
            // Attrib for BlockType: Saved Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A4F145C2-4C67-4EA8-AA8B-091FE4197719" );
            // Attrib for BlockType: Person Merge Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CBFB5FC-0174-489A-8B95-90BB8FAA2144", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "2CF7290D-9E7E-4852-A7FB-EAF32EF8F2BE" );
            // Attrib for BlockType: Person Merge Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4CBFB5FC-0174-489A-8B95-90BB8FAA2144", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A45B5C82-5ED2-4243-B7D4-A8F769D5D9F0" );
            // Attrib for BlockType: Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6F25C9B9-D086-43BF-96AF-AD506F1B76C2" );
            // Attrib for BlockType: Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "CFC25BAE-F723-4486-8DBE-25C4B57CAAAF" );
            // Attrib for BlockType: Theme List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "95B2F79C-4515-4578-8DCF-FA16274CE085" );
            // Attrib for BlockType: Theme List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1685A29A-84FD-42FD-95D9-75D12DF20787" );
            // Attrib for BlockType: Signature Document List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B8F18655-89CC-441D-BF1C-0483A68E3711" );
            // Attrib for BlockType: Signature Document List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "88974107-DBD5-4854-8736-87236F90B888" );
            // Attrib for BlockType: Signature Document Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E413152-B790-4EC2-84A9-9B48D2717D63", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F5E0243B-A838-4DC8-808E-FB14D04DB887" );
            // Attrib for BlockType: Signature Document Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E413152-B790-4EC2-84A9-9B48D2717D63", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E56ED66E-CDF6-4B2D-95C2-5CDCBA207867" );
            // Attrib for BlockType: Schedule Category Exclusion List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACF84335-34A1-4DD6-B242-20119B8D0967", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5C6A65D7-2130-4E93-9A4E-DAEB64927ADE" );
            // Attrib for BlockType: Schedule Category Exclusion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "ACF84335-34A1-4DD6-B242-20119B8D0967", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "B39137C5-96B3-49F4-8CA1-02D167D4BD0E" );
            // Attrib for BlockType: Registration Instance Active List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE8CAFA-587B-4EF2-A457-18047AC6BA39", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "DCF84C23-7FCF-43C2-A5C8-F257F66CD6EA" );
            // Attrib for BlockType: Registration Instance Active List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE8CAFA-587B-4EF2-A457-18047AC6BA39", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "031E2D2D-D83F-491C-8D71-39ECE0AE12D2" );
            // Attrib for BlockType: Transaction Entity Matching:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9EEEB6FE-F8D8-4E6F-B808-B456EB64F319" );
            // Attrib for BlockType: Transaction Entity Matching:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A58BCB1E-01D9-4F60-B925-D831A9537051", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "A6400FEC-91A2-4485-8671-A0C41F0F8857" );
            // Attrib for BlockType: Attribute Matrix Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "069554B7-983E-4653-9A28-BA39659C6D63", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "1534EC4C-13DC-4437-A521-89F6D9F6902C" );
            // Attrib for BlockType: Attribute Matrix Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "069554B7-983E-4653-9A28-BA39659C6D63", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "041103E4-97E3-4081-9CC7-F159163F9742" );
            // Attrib for BlockType: Short Link List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "81D15628-1205-4EF0-898E-3A666D2969BB" );
            // Attrib for BlockType: Short Link List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6FD28CC2-5F0F-4247-9F80-2D5C995F444E" );
            // Attrib for BlockType: Short Link Click List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "BEC135D8-2F46-4BED-9776-1B14B5BF5861" );
            // Attrib for BlockType: Short Link Click List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F3F9BF73-F7AC-4E46-8F62-0A414D692D6D" );
            // Attrib for BlockType: Bulk Import:Person Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Record Import Batch Size", "PersonRecordImportBatchSize", "Person Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use.  If you run into memory utilization problems while importing a large number of records, consider decreasing this value.  (A value less than 1 will result in the default of 25,000 records.)", 0, @"25000", "8408F00B-628B-4B33-832D-5CAE86830BF6" );
            // Attrib for BlockType: Bulk Import:Financial Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Financial Record Import Batch Size", "FinancialRecordImportBatchSize", "Financial Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use.  If you run into memory utilization problems while importing a large number of records, consider decreasing this value.  (A value less than 1 will result in the default of 100,000 records.)", 1, @"100000", "6C8F24D5-AAC2-4413-8318-4ACFD472AA22" );
            // Attrib for BlockType: Group Archived List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "53FC8986-0C52-49E7-94C3-730A61AB2383" );
            // Attrib for BlockType: Group Archived List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "CDEA3BBA-F7BA-41AD-9D8A-BE294A0CAFA3" );
            // Attrib for BlockType: Group History:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E916D65E-5D30-4086-9A11-8E891CCD930E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "90329282-C6B9-4E79-8449-68EF14E9CD1C" );
            // Attrib for BlockType: Group History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E916D65E-5D30-4086-9A11-8E891CCD930E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F4625352-BC87-4E91-9463-5C5C56651983" );
            // Attrib for BlockType: Group Member History:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E6D24F04-2611-4135-8229-4EA204667B6E" );
            // Attrib for BlockType: Group Member History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA6EA2E7-6504-41FE-AB55-0B1E7D04B226", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "290D46BC-8CA9-49A2-9AD2-3006EE3E3719" );
            // Attrib for BlockType: Note Watch List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "5EA1F4F4-0F89-4828-884A-0E474C6A7B25" );
            // Attrib for BlockType: Note Watch List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E8718774-3DCA-4AFF-9F4C-FBA50A00BB57", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "783430C1-5D77-434D-A84A-BAE3ED3DAFDF" );
            // Attrib for BlockType: Scheduled Job History:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "718BF0EE-39C7-4BAD-A252-22DBAA029B16" );
            // Attrib for BlockType: Scheduled Job History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6E289D5-610D-4D85-83BE-B70D5B5E2EEB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "06AF1836-4B09-408B-B790-5FB7AE32A53C" );
            // Attrib for BlockType: Asset Storage Provider List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F4C6D279-2130-44E8-963F-151C7550ECB4" );
            // Attrib for BlockType: Asset Storage Provider List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7A8599B0-6B69-4E1F-9D12-CA9874E8E5D8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6ED2ECB6-4EF2-435F-928D-F89B6911569E" );
            // Attrib for BlockType: Group Member Schedule Template List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F3087F02-3692-44E0-86D3-9E9AE19BBDC5" );
            // Attrib for BlockType: Group Member Schedule Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E9E1CAA0-4959-4BE6-837A-65B299B7F086" );
            // Attrib for BlockType: Step Participant List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9954A558-E9B7-427E-AFAA-49A7E41EAFF3" );
            // Attrib for BlockType: Step Participant List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2E4A1578-145E-4052-9B56-1739F7366827", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F36995B7-4D56-4CB6-A634-C4E3CDF33EF6" );
            // Attrib for BlockType: Streak List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "46A5143E-8DE7-4E3D-96B3-674E8FD12949", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "0F86054E-BF1C-4718-91C0-60D560860745" );
            // Attrib for BlockType: Streak List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "46A5143E-8DE7-4E3D-96B3-674E8FD12949", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "91F60C5F-C2C6-44BA-B362-67BA1D719E1C" );
            // Attrib for BlockType: Persisted Dataset List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9BD5E3D0-F520-47E3-9E6E-943009B3A629" );
            // Attrib for BlockType: Persisted Dataset List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "50ADE904-BB5C-40F9-A97D-ED8FF530B5A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "968BB23B-E11D-47BB-8AAA-0B5731415E6D" );
            // Attrib for BlockType: Nameless Person List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41AE0574-BE1E-4656-B45D-2CB734D1BE30", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "66BF8444-0D05-4E35-83C6-DDBBF896969F" );
            // Attrib for BlockType: Nameless Person List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "41AE0574-BE1E-4656-B45D-2CB734D1BE30", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "CDA5752D-7295-4D3A-ABFD-45BAE7F7359E" );
            // Attrib for BlockType: RSVP List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CE8B41-FD1B-43F2-8C8E-4E878470E8BD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E2FD92F5-999D-4E04-A015-3B3D96347FC2" );
            // Attrib for BlockType: RSVP List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "16CE8B41-FD1B-43F2-8C8E-4E878470E8BD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "CAF68E58-7B3D-4E7E-B468-AB6E54FA8B94" );
            // Attrib for BlockType: Assessment History:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7EB1E42-FEA7-4735-83FE-A618BD2616BF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "EC1A4270-F1C1-4EAD-8E5E-9371D75C8366" );
            // Attrib for BlockType: Assessment History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E7EB1E42-FEA7-4735-83FE-A618BD2616BF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DB5BD1D2-DC7C-47CE-8289-A2CF46C40B9B" );
            // Attrib for BlockType: System Communication List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13BD5FCC-8F03-46B4-B193-E9C0987D2F20", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B4198A78-48FB-4449-82F0-40D3602A98E4" );
            // Attrib for BlockType: System Communication List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13BD5FCC-8F03-46B4-B193-E9C0987D2F20", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "38B7D0F1-2BA8-40C3-AA8F-EAF0E368C9DD" );
            // Attrib for BlockType: Attendance List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "678ED4B6-D76F-4D43-B069-659E352C9BD8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "D8E9C105-8700-472B-9C2D-21DDD0AC4DEA" );
            // Attrib for BlockType: Attendance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "678ED4B6-D76F-4D43-B069-659E352C9BD8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "DD2B83AE-CE8A-49AE-BF4E-B9ED29DA3838" );
            // Attrib for BlockType: Documents:Document Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0", "1FD31CDC-E5E2-431B-8D53-72FC0430044D", "Document Types", "DocumentTypes", "Document Types", @"The document types that should be displayed.", 2, @"", "79244903-8CF1-4D5C-A1DD-4BD9EB9C8CB9" );
            // Attrib for BlockType: SMS Pipeline List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB6FD0BF-FDCE-48DA-919C-240F029518A2", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7BD8918A-85B4-4A30-AA33-FF09C9DB5C34" );
            // Attrib for BlockType: SMS Pipeline List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB6FD0BF-FDCE-48DA-919C-240F029518A2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "2B9D87E2-96D4-4E63-84B0-53E9D7EF391F" );
            // Attrib for BlockType: Checkr Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9A2EE19D-5094-4200-8131-426E71DD5356" );
            // Attrib for BlockType: Checkr Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "BD59FDAE-C639-4FAD-B122-5A14A94BF9D6" );
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "70A23BEA-B109-4661-8101-954B9B11E0AD" );
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EF7175E6-73ED-481B-A5F2-7F4C1CFBD920" );
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "966CC5E7-0C0D-463D-9F84-8792B379295D" );
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "29681509-9BBF-4398-96B4-AF6B44E4F9AB" );
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "Title", @"The main title to display over the image. <span class='tip tip-lava'></span>", 0, @"", "2D8BA33B-2ACF-4C6C-A139-B09AED805059" );
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle", "Subtitle", "Subtitle", @"The subtitle to display over the image. <span class='tip tip-lava'></span>", 1, @"", "CCDCD9A8-ABC6-4896-AAC0-3748CDF25B50" );
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Phone", "BackgroundImagePhone", "Background Image - Phone", @"Recommended size is at least 1024px wide and double the height specified below.", 2, @"", "925839DE-19DB-4BFC-9523-5965697DE89A" );
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "Background Image - Tablet", "BackgroundImageTablet", "Background Image - Tablet", @"Recommended size is at least 2048px wide and double the height specified below.", 3, @"", "0A0FD796-89CE-4318-BE10-0126863E9B3C" );
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Phone", "ImageHeightPhone", "Height - Phone", @"", 4, @"200", "F77E4433-8C14-470A-B4B5-B08B7CEA4777" );
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Height - Tablet", "ImageHeightTablet", "Height - Tablet", @"", 5, @"350", "5AC6AFCA-4166-4822-979C-2EC9F89FEE4C" );
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Text Align", "HorizontalTextAlign", "Text Align", @"", 6, @"Center", "4256E5A9-5427-42A8-BA4F-F53C20E20CEE" );
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Color", "TitleColor", "Title Color", @"Will override the theme's hero title (.hero-title) color.", 7, @"", "03C5FA82-04E3-48C3-9E23-73AE373A6FFA" );
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subtitle Color", "SubtitleColor", "Subtitle Color", @"Will override the theme's hero subtitle (.hero-subtitle) color.", 8, @"", "4A6CFA0C-1D90-4920-9EB9-A9EABD43F489" );
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9A93E34E-838B-4B34-A1DE-5E2F520BF416", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Padding", "Padding", "Padding", @"The padding around the inside of the image.", 9, @"20", "D5B41DA8-40EA-4C50-B8BB-26E7BE9C3C1D" );
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE030FA7-B08C-41D6-84DC-3514A4084B2F", "EC0D9528-1A22-404E-A776-566404987363", "Calendar", "Calendar", "Calendar", @"The calendar to pull events from", 0, @"", "73A105A8-B5CB-4D8D-A01D-2DB0F84106C7" );
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE030FA7-B08C-41D6-84DC-3514A4084B2F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to push onto the navigation stack when viewing details of an event.", 1, @"", "FA912251-F785-4B1C-85D0-4718B617226D" );
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE030FA7-B08C-41D6-84DC-3514A4084B2F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Audience Filter", "AudienceFilter", "Audience Filter", @"Determines which audiences should be displayed in the filter.", 2, @"", "E7C4A192-71E3-4AAA-B71B-E6E76F2C3B2C" );
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE030FA7-B08C-41D6-84DC-3514A4084B2F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Event Summary", "EventSummary", "Event Summary", @"The XAML to use when rendering the event summaries below the calendar.", 3, @"<Frame HasShadow=""false"" StyleClass=""calendar-event"">
    <StackLayout Spacing=""0"">
        <Label StyleClass=""calendar-event-title"" Text=""{Binding Name}"" />
        {% if Item.EndDateTime == null %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% else %}
            <Label StyleClass=""calendar-event-text"" Text=""{{ Item.StartDateTime | Date:'h:mm tt' }} - {{ Item.EndDateTime | Date:'h:mm tt' }}"" LineBreakMode=""NoWrap"" />
        {% endif %}
        <StackLayout Orientation=""Horizontal"">
            <Label HorizontalOptions=""FillAndExpand"" StyleClass=""calendar-event-audience"" Text=""{{ Item.Audiences | Select:'Name' | Join:', ' }}"" />
            <Label StyleClass=""calendar-event-campus"" Text=""{{ Item.Campus }}"" HorizontalTextAlignment=""End"" LineBreakMode=""NoWrap"" />
        </StackLayout>
    </StackLayout>
</Frame>
", "A5574D5F-ADEE-4527-9BA0-52423FCA3895" );
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EE030FA7-B08C-41D6-84DC-3514A4084B2F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Filter", "ShowFilter", "Show Filter", @"If enabled then the user will be able to apply custom filtering.", 4, @"True", "F8385920-C7C9-4490-A34B-FAA77879829E" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1918EDFB-8617-4420-9E60-C8575ACD4501", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Forward to Allow", "NumberOfDaysForwardToAllow", "Number of Days Forward to Allow", @"", 0, @"0", "C25E2A17-E0E3-41B6-B51A-9CAB7C2F8D0C" );
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1918EDFB-8617-4420-9E60-C8575ACD4501", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Days Back to Allow", "NumberOfDaysBackToAllow", "Number of Days Back to Allow", @"", 1, @"30", "A59B1827-0172-4950-B2C6-3FA8482AB4FA" );
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1918EDFB-8617-4420-9E60-C8575ACD4501", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Save Redirect Page", "SaveRedirectPage", "Save Redirect Page", @"If set, redirect user to this page on save. If not set, page is popped off the navigation stack.", 2, @"", "ABB015E9-3E12-4DB9-8DA9-356F60B23E54" );
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1918EDFB-8617-4420-9E60-C8575ACD4501", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Save Button", "ShowSaveButton", "Show Save Button", @"If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).", 3, @"False", "89171507-C193-4566-9241-FF954915CD59" );
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1918EDFB-8617-4420-9E60-C8575ACD4501", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Any Date Selection", "AllowAnyDateSelection", "Allow Any Date Selection", @"If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.", 4, @"False", "B2EA9ABA-5AC5-4610-B1CB-7E8C04360DA5" );
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Name", "ShowGroupName", "Show Group Name", @"", 0, @"True", "8764E3E1-7C21-4047-BDC2-81EA1C7CB12F" );
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Name Edit", "EnableGroupNameEdit", "Enable Group Name Edit", @"", 1, @"True", "48169F08-DE9F-46BE-8D29-A8C2C2CBF22B" );
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "Show Description", @"", 2, @"True", "62689D78-E402-4EAA-B18D-7BE9ECCB8056" );
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Description Edit", "EnableDescriptionEdit", "Enable Description Edit", @"", 3, @"True", "C92909C2-B40C-4246-9346-660C9FFB6298" );
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "ShowCampus", "Show Campus", @"", 4, @"True", "EC39E792-4E77-4903-8D07-3DB24FD05FBA" );
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Edit", "EnableCampusEdit", "Enable Campus Edit", @"", 5, @"True", "A9415559-E738-437D-B184-1266151D1572" );
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Group Capacity", "ShowGroupCapacity", "Show Group Capacity", @"", 6, @"True", "68574A24-D407-4CE1-B2F9-5AFF055BDD7E" );
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Group Capacity Edit", "EnableGroupCapacityEdit", "Enable Group Capacity Edit", @"", 7, @"True", "FD309D1F-E025-462F-A9F6-DABD6EC5BC6F" );
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Active Status", "ShowActiveStatus", "Show Active Status", @"", 8, @"True", "4CE968AE-9D61-4299-AFF4-39CF397E2A17" );
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Active Status Edit", "EnableActiveStatusEdit", "Enable Active Status Edit", @"", 9, @"True", "F34EC0A6-9840-493B-BC12-7D986BCD0F13" );
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Status", "ShowPublicStatus", "Show Public Status", @"", 10, @"True", "C86EDAE4-1413-491B-91B2-2202672F1F49" );
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Public Status Edit", "EnablePublicStatusEdit", "Enable Public Status Edit", @"", 11, @"True", "336727E1-31DE-4BE7-B7E7-4DA6474FF329" );
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 12, @"", "DBE479D1-1F45-49DA-B890-985DE513067C" );
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B6B9CBCC-BDA2-4389-83A7-0C334D449591", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"The group detail page to return to, if not set then the edit page is popped off the navigation stack.", 13, @"", "6DEE9631-DC19-4995-B9D2-EF7E54D3887A" );
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48079AB0-41FF-4F48-9CD7-43600251061F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Role Change", "AllowRoleChange", "Allow Role Change", @"", 0, @"True", "1D874785-E718-4ED8-964B-C2F08FB98F38" );
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48079AB0-41FF-4F48-9CD7-43600251061F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Member Status Change", "AllowMemberStatusChange", "Allow Member Status Change", @"", 1, @"True", "75193A26-9094-40A3-94DE-FAFED396C389" );
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48079AB0-41FF-4F48-9CD7-43600251061F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Note Edit", "AllowNoteEdit", "Allow Note Edit", @"", 2, @"True", "C1E2ABB4-A46F-41A9-B9D7-77A0F93C572D" );
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48079AB0-41FF-4F48-9CD7-43600251061F", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Attribute Category", "AttributeCategory", "Attribute Category", @"Category of attributes to show and allow editing on.", 3, @"", "7A0DA3CB-1A29-4006-A07C-F0AFEED2C31F" );
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "48079AB0-41FF-4F48-9CD7-43600251061F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Member Detail Page", "MemberDetailsPage", "Member Detail Page", @"The group member page to return to, if not set then the edit page is popped off the navigation stack.", 4, @"", "BA971602-6478-41BB-B9BE-5A4043BFA2EE" );
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "43A7FC7E-D9B4-4214-AE9A-3C3C87F2C9AD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"The page that will display the group member details when selecting a member.", 0, @"", "B2A330CA-0FA2-49DF-8559-5C17DB1201A9" );
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "43A7FC7E-D9B4-4214-AE9A-3C3C87F2C9AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title Template", "TitleTemplate", "Title Template", @"The value to use when rendering the title text. <span class='tip tip-lava'></span>", 1, @"{{ Group.Name }} Group Roster", "79791B63-A93D-4227-9FE2-0BF0591986FE" );
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "43A7FC7E-D9B4-4214-AE9A-3C3C87F2C9AD", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "CD11FD90-1A4D-4F9A-A930-8F6901D08432" );
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "43A7FC7E-D9B4-4214-AE9A-3C3C87F2C9AD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Additional Fields", "AdditionalFields", "Additional Fields", @"", 3, @"", "2F5C5913-86E1-46A1-9A2B-81A8E2477501" );
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B4E29B40-5368-46F5-BA73-7C9AD9CD2FD3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Edit Page", "GroupMemberEditPage", "Group Member Edit Page", @"The page that will allow editing of a group member.", 0, @"", "F89684CC-23DA-4583-875D-C8C59A803C26" );
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B4E29B40-5368-46F5-BA73-7C9AD9CD2FD3", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 1, @"", "9DF8F030-18ED-4E22-9329-6CDA752B42F3" );
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1C8868F-40A7-4D66-B9C5-FA3441CDC4F8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Edit Page", "GroupEditPage", "Group Edit Page", @"The page that will allow editing of the group.", 0, @"", "ECF02A04-C7CC-4B09-BB52-E4F83CE0B53B" );
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1C8868F-40A7-4D66-B9C5-FA3441CDC4F8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Leader List", "ShowLeaderList", "Show Leader List", @"Specifies if the leader list should be shown, this value is made available to the Template as ShowLeaderList.", 1, @"True", "215EA7BF-EB51-49B2-9299-0C72923D60D6" );
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E1C8868F-40A7-4D66-B9C5-FA3441CDC4F8", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the content.", 2, @"", "BC2408E8-B8EB-4F4F-81C1-87A586FC8AA8" );
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category", "EnableCategory", "Show Category", @"If disabled, then the user will not be able to select a category and the default category will be used exclusively.", 0, @"True", "9199A2A8-B775-4303-9924-1AD7B1D7F6D9" );
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"A top level category. This controls which categories the person can choose from when entering their prayer request.", 1, @"", "90F80215-926D-4255-8F15-A70F886F9D35" );
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Default Category", "DefaultCategory", "Default Category", @"The default category to use for all new prayer requests.", 2, @"", "59ABFB3E-50A6-430B-A039-D8782A913ADA" );
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Auto Approve", "EnableAutoApprove", "Enable Auto Approve", @"If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.", 0, @"True", "0EB397D8-E06F-49F4-8166-4B1F1BC496EE" );
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Expires After (Days)", "ExpiresAfterDays", "Expires After (Days)", @"Number of days until the request will expire (only applies when auto-approved is enabled).", 1, @"14", "5164FE14-55AC-4358-A844-25E060314376" );
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Urgent Flag", "EnableUrgentFlag", "Show Urgent Flag", @"If enabled, requestors will be able to flag prayer requests as urgent.", 2, @"False", "82F38CE9-6E17-4522-AD69-B0115FE850B8" );
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Display Flag", "EnablePublicDisplayFlag", "Show Public Display Flag", @"If enabled, requestors will be able set whether or not they want their request displayed on the public website.", 3, @"False", "9E7858D0-EE65-48CE-82EE-96F586C54E68" );
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Default To Public", "DefaultToPublic", "Default To Public", @"If enabled, all prayers will be set to public by default.", 4, @"False", "45D6191C-7810-4162-B6C1-3CA961D904C3" );
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Character Limit", "CharacterLimit", "Character Limit", @"If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.", 5, @"250", "B8B03429-0539-4FE6-9939-72E83E33CE31" );
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Campus", "EnableCampus", "Show Campus", @"Should the campus field be displayed? If there is only one active campus then the campus field will not show.", 6, @"True", "D3A8E136-B07C-4603-A8CF-1BF083707EF1" );
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Campus", "RequireCampus", "Require Campus", @"Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.", 7, @"False", "1372D8D2-0A20-4A3A-9718-DEF45AD877D5" );
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Require Last Name", "RequireLastName", "Require Last Name", @"Require that a last name be entered. First name is always required.", 8, @"True", "2219A0C8-8A25-4A77-BC99-B234BD39EB13" );
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Person Matching", "EnablePersonMatching", "Enable Person Matching", @"If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.", 9, @"False", "F0023ACE-B747-4F91-8DD2-9B981E7F8E8C" );
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Completion Action", "CompletionAction", "Completion Action", @"What action to perform after saving the prayer request.", 0, @"0", "4762659A-7181-4726-B1A3-7DCE765FE250" );
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Xaml", "CompletionXaml", "Completion Xaml", @"The XAML markup that will be used if the. <span class='tip tip-lava'></span>", 1, @"<Rock:NotificationBox NotificationType=""Success"">
    Thank you for allowing us to pray for you.
</Rock:NotificationBox>", "5A61D74C-1478-4B5B-BEED-626E55FED732" );
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EDCAB491-74E4-48C8-BDD9-9A229795951C", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", @"An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.", 2, @"", "65A6CE53-8D80-44CB-A2E7-EF238EE1D251" );
            // Attrib for BlockType: Page Parameter Filter:Show Block Title
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Block Title", "ShowBlockTitle", "Show Block Title", @"Determines if the Block Title should be displayed", 1, @"True", "D0F41DA4-F5D3-4D43-99C5-B80954F3755B" );
            // Attrib for BlockType: Page Parameter Filter:Block Title Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Text", "BlockTitleText", "Block Title Text", @"The text to display as the block title.", 2, @"BlockTitle", "03C296C8-B892-4731-AD2D-24B8A1614181" );
            // Attrib for BlockType: Page Parameter Filter:Block Title Icon CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Block Title Icon CSS Class", "BlockTitleIconCSSClass", "Block Title Icon CSS Class", @"The css class name to use for the block title icon.", 3, @"fa fa-filter", "6677A0F0-254A-4431-B336-D2266C8FE62F" );
            // Attrib for BlockType: Page Parameter Filter:Filters Per Row
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Filters Per Row", "FiltersPerRow", "Filters Per Row", @"The number of filters to have per row.  Maximum is 12.", 4, @"2", "F625D9F9-E549-4CD3-8E1D-273907AB7E70" );
            // Attrib for BlockType: Page Parameter Filter:Show Reset Filters Button
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Reset Filters Button", "ShowResetFiltersButton", "Show Reset Filters Button", @"Determines if the Reset Filters button should be displayed", 5, @"True", "C9C347F0-76FA-4610-85B0-5385F57AE725" );
            // Attrib for BlockType: Page Parameter Filter:Filter Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Filter Button Text", "FilterButtonText", "Filter Button Text", @"Sets the button text for the filter button.", 6, @"Filter", "731BB3C6-CDD4-4D69-9FA7-4592380E57CA" );
            // Attrib for BlockType: Page Parameter Filter:Filter Button Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Filter Button Size", "FilterButtonSize", "Filter Button Size", @"", 7, @"3", "B62222F0-2EF5-476F-82E6-AAE96001130A" );
            // Attrib for BlockType: Page Parameter Filter:Redirect Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Redirect Page", "RedirectPage", "Redirect Page", @"If set, the filter button will redirect to the selected page.", 8, @"", "54A62107-9A51-4F02-BD5D-639FC2684CC4" );
            // Attrib for BlockType: Page Parameter Filter:Does Selection Cause Postback
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Does Selection Cause Postback", "DoesSelectionCausePostback", "Does Selection Cause Postback", @"If set, selecting a filter will force a PostBack, recalculating the available selections. Useful for SQL values.", 9, @"False", "467B5EFC-A379-48DB-A2DA-EF390D4A4481" );

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Workflow Trigger List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("8B830DF5-2B92-456E-9735-0EDAF3029A63");
            // Attrib for BlockType: Workflow Trigger List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("EFB628F1-6F2F-469C-9D79-7D017E90BDDF");
            // Attrib for BlockType: Workflow List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("4D534CC7-BEA0-45C5-A5B3-AD070F7FADF6");
            // Attrib for BlockType: Workflow List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("620EDCED-E6FA-41CF-9733-550389C7B555");
            // Attrib for BlockType: Stark List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("AC6B87B6-961B-4597-938B-03D64F6FA674");
            // Attrib for BlockType: Stark List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("7CA449C6-6556-4F34-8E14-48286CF80BFE");
            // Attrib for BlockType: Streak List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("91F60C5F-C2C6-44BA-B362-67BA1D719E1C");
            // Attrib for BlockType: Streak List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("0F86054E-BF1C-4718-91C0-60D560860745");
            // Attrib for BlockType: Achievement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("EF7175E6-73ED-481B-A5F2-7F4C1CFBD920");
            // Attrib for BlockType: Achievement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("70A23BEA-B109-4661-8101-954B9B11E0AD");
            // Attrib for BlockType: Achievement Attempt List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("29681509-9BBF-4398-96B4-AF6B44E4F9AB");
            // Attrib for BlockType: Achievement Attempt List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("966CC5E7-0C0D-463D-9F84-8792B379295D");
            // Attrib for BlockType: Step Participant List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F36995B7-4D56-4CB6-A634-C4E3CDF33EF6");
            // Attrib for BlockType: Step Participant List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("9954A558-E9B7-427E-AFAA-49A7E41EAFF3");
            // Attrib for BlockType: Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("CFC25BAE-F723-4486-8DBE-25C4B57CAAAF");
            // Attrib for BlockType: Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("6F25C9B9-D086-43BF-96AF-AD506F1B76C2");
            // Attrib for BlockType: Checkr Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("BD59FDAE-C639-4FAD-B122-5A14A94BF9D6");
            // Attrib for BlockType: Checkr Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("9A2EE19D-5094-4200-8131-426E71DD5356");
            // Attrib for BlockType: User Login List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("E6463229-119B-448A-841A-632B1F260E33");
            // Attrib for BlockType: User Login List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B3FE8591-ECF3-4493-8184-945EAF517929");
            // Attrib for BlockType: Rest Key List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F4E145B2-C0DF-4156-9163-1DF95DB96600");
            // Attrib for BlockType: Rest Key List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("AA6FCF53-6036-46DB-BB9C-89B607D7E676");
            // Attrib for BlockType: RSVP List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("CAF68E58-7B3D-4E7E-B468-AB6E54FA8B94");
            // Attrib for BlockType: RSVP List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("E2FD92F5-999D-4E04-A015-3B3D96347FC2");
            // Attrib for BlockType: Report List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("91425E91-07F2-406B-AAA9-0A613F7C1229");
            // Attrib for BlockType: Report List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("81362512-D452-422E-8791-E4FC359EDA37");
            // Attrib for BlockType: Metric Value List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("E159BE04-8B85-493C-89F2-FDD14FA33EF9");
            // Attrib for BlockType: Metric Value List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("78D52A97-B7EC-46F5-B333-CD162C401BD5");
            // Attrib for BlockType: Prayer Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1C62F958-E94C-4A61-B513-DA0E239B7519");
            // Attrib for BlockType: Prayer Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("05400FF0-BD51-4C64-9E96-45C0DA682090");
            // Attrib for BlockType: Prayer Comment List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("74323918-73DC-4D03-87BB-7D0D158DA8F8");
            // Attrib for BlockType: Prayer Comment List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("CC9B09CB-9C14-44FD-99D1-AA4AEF1D4455");
            // Attrib for BlockType: Group Member Schedule Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("E9E1CAA0-4959-4BE6-837A-65B299B7F086");
            // Attrib for BlockType: Group Member Schedule Template List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("F3087F02-3692-44E0-86D3-9E9AE19BBDC5");
            // Attrib for BlockType: Group Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("A60FF939-4225-4E46-9B8C-246C7D4EB601");
            // Attrib for BlockType: Group Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("556EAD07-E0E9-4959-8D0E-6584CD6FE2F3");
            // Attrib for BlockType: Group Requirement Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("2B9D97F9-A4FA-4324-850A-36E9C25B11AD");
            // Attrib for BlockType: Group Requirement Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B8CF900E-2007-4A38-BCF5-F33D35AFD38E");
            // Attrib for BlockType: Group Member List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("2053BCCE-A4CA-49F0-BC94-4EA0FFA04E91");
            // Attrib for BlockType: Group Member List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("D4D7DCB4-F481-48F2-9992-E35C6F4E8C0B");
            // Attrib for BlockType: Group Member History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("290D46BC-8CA9-49A2-9AD2-3006EE3E3719");
            // Attrib for BlockType: Group Member History:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("E6D24F04-2611-4135-8229-4EA204667B6E");
            // Attrib for BlockType: Group List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("D7AE5225-28BF-4954-9D04-782AC76CBF4E");
            // Attrib for BlockType: Group List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5E4612EB-E1DA-4606-97CC-09D11331FE84");
            // Attrib for BlockType: Group History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F4625352-BC87-4E91-9463-5C5C56651983");
            // Attrib for BlockType: Group History:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("90329282-C6B9-4E79-8449-68EF14E9CD1C");
            // Attrib for BlockType: Group Attendance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("AC59375E-DEE0-4465-B1DF-E25C8283E5AD");
            // Attrib for BlockType: Group Attendance List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("264678DD-59ED-482E-9DAB-7F1CBC1F9C67");
            // Attrib for BlockType: Group Archived List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("CDEA3BBA-F7BA-41AD-9D8A-BE294A0CAFA3");
            // Attrib for BlockType: Group Archived List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("53FC8986-0C52-49E7-94C3-730A61AB2383");
            // Attrib for BlockType: Suggestion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("93A21076-F296-4AAD-AA76-AD31CA6AE220");
            // Attrib for BlockType: Suggestion List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5F10E479-AC38-44D8-A14D-0A865D8A4C70");
            // Attrib for BlockType: Person Suggestion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("68F09AD7-43A1-45B2-9C59-EF2FF6956E3A");
            // Attrib for BlockType: Person Suggestion List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("844BC0F6-735B-454A-AA46-CA3243C89F88");
            // Attrib for BlockType: Person Following List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("8EC047EE-13F6-4C82-88D7-C942144B34E0");
            // Attrib for BlockType: Person Following List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("4E36F54E-1CDA-4C49-AE4B-4E84BE95B5F7");
            // Attrib for BlockType: Event List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("BADF8FC4-20C4-42C7-83CE-4B0BF1620DC7");
            // Attrib for BlockType: Event List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("2BF48893-C1C7-4291-A9FA-E5BAC98D1395");
            // Attrib for BlockType: Transaction List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("12749FBF-9A92-40FA-A7A4-85BECA1ED52A");
            // Attrib for BlockType: Transaction List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("64389018-EB2C-4592-B764-02BEDCC33A91");
            // Attrib for BlockType: Transaction Entity Matching:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("A6400FEC-91A2-4485-8671-A0C41F0F8857");
            // Attrib for BlockType: Transaction Entity Matching:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("9EEEB6FE-F8D8-4E6F-B808-B456EB64F319");
            // Attrib for BlockType: Scheduled Transaction List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6AF171B4-4FFA-45BA-8C93-DC5F4CA18032");
            // Attrib for BlockType: Scheduled Transaction List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("45F47A89-F906-4143-A895-5D53F66B164A");
            // Attrib for BlockType: Saved Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("A4F145C2-4C67-4EA8-AA8B-091FE4197719");
            // Attrib for BlockType: Saved Account List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("8AC39C3F-BE58-457C-B173-91AC09EFB796");
            // Attrib for BlockType: Pledge List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("EB314955-A5AE-492F-B350-A2E4046B6DDA");
            // Attrib for BlockType: Pledge List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("04C7A203-7125-452E-91C2-D11DD4A782CC");
            // Attrib for BlockType: Gateway List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C41B4A8D-7C61-49CD-AE42-9244303B3D7A");
            // Attrib for BlockType: Gateway List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("8DC98507-027B-4837-AE45-E035F227926D");
            // Attrib for BlockType: Business List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("972BE9E4-9DB1-4C2B-B4E4-91ADDFA65DFB");
            // Attrib for BlockType: Business List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("31FBEC02-1CF4-45F4-8CA2-8DA2F46F80A0");
            // Attrib for BlockType: Benevolence Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("DB8E2D32-BF2B-439D-A463-173139CE02AA");
            // Attrib for BlockType: Benevolence Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("23E44310-68C4-45F9-8426-8D15BDCD4B14");
            // Attrib for BlockType: Batch List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("68CA768F-4159-4C43-B4CE-DEDF320CD242");
            // Attrib for BlockType: Batch List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("1C297F0E-DA0D-45FB-9FA3-57B5346DEC9D");
            // Attrib for BlockType: Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("7F18F54E-3B2A-42A1-8449-1A3909136FE6");
            // Attrib for BlockType: Account List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("8A5BDA9F-8FCE-4A1E-837C-1816FB14D67B");
            // Attrib for BlockType: Registration Instance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1A3EC3E1-99BF-4452-9854-7BBBE8BD4856");
            // Attrib for BlockType: Registration Instance List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A27BBCBB-01DF-4C38-A247-1F6E59FF363E");
            // Attrib for BlockType: Registration Instance Active List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("031E2D2D-D83F-491C-8D71-39ECE0AE12D2");
            // Attrib for BlockType: Registration Instance Active List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("DCF84C23-7FCF-43C2-A5C8-F257F66CD6EA");
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("4AD1343D-3116-49D4-9DBA-12F464D98323");
            // Attrib for BlockType: Calendar Event Item Occurrence List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("AE9062AA-2DB2-4C04-B598-51A4A2C0808B");
            // Attrib for BlockType: Calendar Event Item List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("64AABE0F-6D06-45CA-9E34-9F19589076AB");
            // Attrib for BlockType: Calendar Event Item List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("2DDBEA1F-9401-456D-966F-40D11701B8A7");
            // Attrib for BlockType: Bank Account List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("256D8C3E-2CC1-488E-AAC2-D3DCAE46A701");
            // Attrib for BlockType: Bank Account List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("F5C7D70E-B4AB-418C-AC3E-0E9D4AAF56FE");
            // Attrib for BlockType: Person Merge Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("A45B5C82-5ED2-4243-B7D4-A8F769D5D9F0");
            // Attrib for BlockType: Person Merge Request List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("2CF7290D-9E7E-4852-A7FB-EAF32EF8F2BE");
            // Attrib for BlockType: Person Duplicate List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("EF0750F8-7B01-4FB1-A054-5BA77723F8FF");
            // Attrib for BlockType: Person Duplicate List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("240AAB19-5481-460F-8256-89016927F680");
            // Attrib for BlockType: Person Duplicate Detail:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("139D6001-F973-491B-B265-A4D33139BE44");
            // Attrib for BlockType: Person Duplicate Detail:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("467BAFEF-C367-400E-9FA3-04A3D4328959");
            // Attrib for BlockType: Nameless Person List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("CDA5752D-7295-4D3A-ABFD-45BAE7F7359E");
            // Attrib for BlockType: Nameless Person List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("66BF8444-0D05-4E35-83C6-DDBBF896969F");
            // Attrib for BlockType: Documents:Document Types
            RockMigrationHelper.DeleteAttribute("79244903-8CF1-4D5C-A1DD-4BD9EB9C8CB9");
            // Attrib for BlockType: Badge List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("89E483A8-7873-4392-B488-275AB6ECE2BE");
            // Attrib for BlockType: Badge List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("4A4B6713-D6DB-4CD0-B929-B83A4865BD24");
            // Attrib for BlockType: Assessment History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("DB5BD1D2-DC7C-47CE-8289-A2CF46C40B9B");
            // Attrib for BlockType: Assessment History:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("EC1A4270-F1C1-4EAD-8E5E-9371D75C8366");
            // Attrib for BlockType: Tag List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1704B351-3416-497F-9626-0D94B3A968C7");
            // Attrib for BlockType: Tag List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("23D81607-52D7-461A-A7F2-9B13B40572C7");
            // Attrib for BlockType: Signature Document Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("E56ED66E-CDF6-4B2D-95C2-5CDCBA207867");
            // Attrib for BlockType: Signature Document Template List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("F5E0243B-A838-4DC8-808E-FB14D04DB887");
            // Attrib for BlockType: Signature Document List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("88974107-DBD5-4854-8736-87236F90B888");
            // Attrib for BlockType: Signature Document List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B8F18655-89CC-441D-BF1C-0483A68E3711");
            // Attrib for BlockType: Schedule List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("295A1429-9581-49CF-87D9-9FA912707646");
            // Attrib for BlockType: Schedule List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("41AB15D5-7A4C-4D28-8C9F-671BFB0169CA");
            // Attrib for BlockType: Scheduled Job List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("FB603A69-2773-4A7B-A12A-3C2CF3979DA8");
            // Attrib for BlockType: Scheduled Job List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("2E3161E9-5C64-4D17-8D6C-18A295D40C92");
            // Attrib for BlockType: Scheduled Job History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("06AF1836-4B09-408B-B790-5FB7AE32A53C");
            // Attrib for BlockType: Scheduled Job History:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("718BF0EE-39C7-4BAD-A252-22DBAA029B16");
            // Attrib for BlockType: Schedule Category Exclusion List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("B39137C5-96B3-49F4-8CA1-02D167D4BD0E");
            // Attrib for BlockType: Schedule Category Exclusion List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5C6A65D7-2130-4E93-9A4E-DAEB64927ADE");
            // Attrib for BlockType: REST Controller List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("4D3C450B-8F97-48A8-BF7B-13B5C78A6E7A");
            // Attrib for BlockType: REST Controller List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A0F1FBFF-1272-408B-879C-7F86DC355D77");
            // Attrib for BlockType: REST Action List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F0AFBA55-DFE0-4039-8875-6FF1F59F5E68");
            // Attrib for BlockType: REST Action List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5A8BCD0B-3332-425E-B389-D512122D1B5C");
            // Attrib for BlockType: Note Watch List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("783430C1-5D77-434D-A84A-BAE3ED3DAFDF");
            // Attrib for BlockType: Note Watch List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5EA1F4F4-0F89-4828-884A-0E474C6A7B25");
            // Attrib for BlockType: Merge Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6C212B29-668A-4BF5-9322-8E107D306207");
            // Attrib for BlockType: Merge Template List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("BB37E693-418D-45F3-B011-91E445B4781C");
            // Attrib for BlockType: Location List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C455CED6-1D5E-4E69-A577-DFEAD64268B8");
            // Attrib for BlockType: Location List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("0DAAA063-8FC3-4599-9C6E-1054D41A2F27");
            // Attrib for BlockType: Exception List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("8F28C102-0321-4B65-A9D5-E1A77872128A");
            // Attrib for BlockType: Exception List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("C697EF19-61C8-45C6-B687-D22C851A3294");
            // Attrib for BlockType: Device List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("EDD0BF18-5923-4F91-86A2-23F81D7208A1");
            // Attrib for BlockType: Device List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("491465C5-9110-4930-9E8B-9094174459EC");
            // Attrib for BlockType: Defined Value List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("80765648-83B0-4B75-A296-851384C41CAB");
            // Attrib for BlockType: Defined Value List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("0A3F078E-8A2A-4E9D-9763-3758E123E042");
            // Attrib for BlockType: Defined Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("66FA42CD-1FD5-4F9D-B5E9-A7A2FC105607");
            // Attrib for BlockType: Defined Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("F21610E0-E22C-48D9-AC0E-CCE1149267E5");
            // Attrib for BlockType: Components:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C29E9E43-B246-4CBB-9A8A-274C8C377FDF");
            // Attrib for BlockType: Components:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("4AF30CE4-FE81-4612-AC64-5CB9902B8583");
            // Attrib for BlockType: Categories:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("547BF211-7B74-4CE2-A72B-561B4715D230");
            // Attrib for BlockType: Categories:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("7F8257E2-0B7E-4447-BCB0-5D82DC13F356");
            // Attrib for BlockType: Campus List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("29B903D8-3C98-442F-BC6B-E6C2F29F388C");
            // Attrib for BlockType: Campus List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("894B4118-83B9-449B-BF75-3D510C68829A");
            // Attrib for BlockType: Binary File Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("53944C14-9BCF-4B37-8012-D48C6B15FAC9");
            // Attrib for BlockType: Binary File Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("310285D8-3021-43C8-9713-EAA70FE1B22C");
            // Attrib for BlockType: Binary File List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("3AA43986-A640-4EC8-8C96-72D57987DC44");
            // Attrib for BlockType: Binary File List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A2983BB0-1FAD-4D5F-B04E-F29EC28761E9");
            // Attrib for BlockType: Audit Information List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("BB859A0A-A29A-49AE-8581-AF52F91DF221");
            // Attrib for BlockType: Audit Information List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("102FE63E-815F-4845-B3B3-7238E2445BF5");
            // Attrib for BlockType: Attributes:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C0FED8AD-799C-4E9A-B5A7-C4CD06F041E7");
            // Attrib for BlockType: Attributes:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("5431F4A8-553A-431E-8561-7C7FF4120B4D");
            // Attrib for BlockType: Attribute Matrix Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("041103E4-97E3-4081-9CC7-F159163F9742");
            // Attrib for BlockType: Attribute Matrix Template List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("1534EC4C-13DC-4437-A521-89F6D9F6902C");
            // Attrib for BlockType: Asset Storage Provider List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6ED2ECB6-4EF2-435F-928D-F89B6911569E");
            // Attrib for BlockType: Asset Storage Provider List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("F4C6D279-2130-44E8-963F-151C7550ECB4");
            // Attrib for BlockType: Connection Opportunity List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C3178F42-7D35-4817-9C5C-9C479209C5E9");
            // Attrib for BlockType: Connection Opportunity List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B6B24DAD-B1A3-416C-96D1-78C79776C69D");
            // Attrib for BlockType: Template List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("15968808-AE85-45DD-93A5-863E8709B46B");
            // Attrib for BlockType: Template List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("54149758-CE20-4172-8EBB-01ED8A94D8D5");
            // Attrib for BlockType: System Email List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("C0C0DDF6-499F-40C2-99E9-BB3FBA80F875");
            // Attrib for BlockType: System Email List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A329DEB6-8AC0-4A5C-98BB-9696402E7EF5");
            // Attrib for BlockType: System Communication List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("38B7D0F1-2BA8-40C3-AA8F-EAF0E368C9DD");
            // Attrib for BlockType: System Communication List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("B4198A78-48FB-4449-82F0-40D3602A98E4");
            // Attrib for BlockType: SMS Pipeline List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("2B9D87E2-96D4-4E63-84B0-53E9D7EF391F");
            // Attrib for BlockType: SMS Pipeline List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("7BD8918A-85B4-4A30-AA33-FF09C9DB5C34");
            // Attrib for BlockType: Communication Recipient List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("86BE8D5C-9FB9-4FEA-8A15-A10E00E8A77A");
            // Attrib for BlockType: Communication Recipient List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("4886BD88-A730-4959-81F2-E35F9149EE08");
            // Attrib for BlockType: Communication List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("37C90DE1-0EB3-4EC7-8569-AE0D93F0C440");
            // Attrib for BlockType: Communication List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("EE559663-3A16-4821-BA46-FD395AF7EDA1");
            // Attrib for BlockType: Theme List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1685A29A-84FD-42FD-95D9-75D12DF20787");
            // Attrib for BlockType: Theme List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("95B2F79C-4515-4578-8DCF-FA16274CE085");
            // Attrib for BlockType: Site List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("EB67C256-C509-4327-B2E2-FD3CA37CA59C");
            // Attrib for BlockType: Site List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("348F6509-2AFD-4BC2-8B60-C508E2935A6E");
            // Attrib for BlockType: Short Link List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6FD28CC2-5F0F-4247-9F80-2D5C995F444E");
            // Attrib for BlockType: Short Link List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("81D15628-1205-4EF0-898E-3A666D2969BB");
            // Attrib for BlockType: Short Link Click List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F3F9BF73-F7AC-4E46-8F62-0A414D692D6D");
            // Attrib for BlockType: Short Link Click List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("BEC135D8-2F46-4BED-9776-1B14B5BF5861");
            // Attrib for BlockType: Persisted Dataset List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("968BB23B-E11D-47BB-8AAA-0B5731415E6D");
            // Attrib for BlockType: Persisted Dataset List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("9BD5E3D0-F520-47E3-9E6E-943009B3A629");
            // Attrib for BlockType: Route List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("0DD924BF-99DF-4463-8E64-70E10DF0CF89");
            // Attrib for BlockType: Route List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("0E5DC54A-9090-42F0-9B8A-17E59E33ACD9");
            // Attrib for BlockType: Page List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("665625F7-0528-4EF4-83C4-949B58A45E47");
            // Attrib for BlockType: Page List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("64AA17D1-4027-4779-BFF7-9C874ED20316");
            // Attrib for BlockType: Layout List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("6FF16D9F-AAAD-461A-B501-013DEB53B434");
            // Attrib for BlockType: Layout List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("13F0BA8A-F85D-41FC-9497-720215592553");
            // Attrib for BlockType: Layout Block List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("1580D3A8-3EC5-4A5A-899E-8D43264AB017");
            // Attrib for BlockType: Layout Block List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("ECFA346C-DB15-4128-A30A-F687A19DAF82");
            // Attrib for BlockType: Content Channel Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("7260D29D-D4F0-4687-917E-3B74F231EBD5");
            // Attrib for BlockType: Content Channel Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("925AE569-047F-4494-AA7D-0D901A4355BA");
            // Attrib for BlockType: Content Channel List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("006256B7-0DF0-45D4-A16C-459956741047");
            // Attrib for BlockType: Content Channel List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("6B3781C1-0FD3-4C1E-B2D6-D5EFF1494F79");
            // Attrib for BlockType: Content Channel Item List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("F1FEB0B3-AF7B-4FAA-A435-7066D8D7491B");
            // Attrib for BlockType: Content Channel Item List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("BC0E5B7E-65A6-451A-AF03-4A24C47FAF18");
            // Attrib for BlockType: Schedule Builder:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("8EEB4117-F746-409D-B00A-5786A3FDCBD1");
            // Attrib for BlockType: Schedule Builder:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("A7142744-9EFA-4E13-90DD-534678638F5E");
            // Attrib for BlockType: Attendance List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("DD2B83AE-CE8A-49AE-BF4E-B9ED29DA3838");
            // Attrib for BlockType: Attendance List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("D8E9C105-8700-472B-9C2D-21DDD0AC4DEA");
            // Attrib for BlockType: Attendance History:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("92E5A8CD-D3DF-4126-9D7E-370ABF314AAF");
            // Attrib for BlockType: Attendance History:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("1D637F4D-9B35-4F0B-95BA-F77EDF43D927");
            // Attrib for BlockType: Bulk Import:Financial Record Import Batch Size
            RockMigrationHelper.DeleteAttribute("6C8F24D5-AAC2-4413-8318-4ACFD472AA22");
            // Attrib for BlockType: Bulk Import:Person Record Import Batch Size
            RockMigrationHelper.DeleteAttribute("8408F00B-628B-4B33-832D-5CAE86830BF6");
            // Attrib for BlockType: External Application List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("7CC2907E-1AD5-4EBD-8009-A1D8F4A7B9F4");
            // Attrib for BlockType: External Application List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("7D46D41C-F552-47D2-ADAC-ABE4E01BF1EC");
            // Attrib for BlockType: Page Parameter Filter:Does Selection Cause Postback
            RockMigrationHelper.DeleteAttribute("467B5EFC-A379-48DB-A2DA-EF390D4A4481");
            // Attrib for BlockType: Page Parameter Filter:Redirect Page
            RockMigrationHelper.DeleteAttribute("54A62107-9A51-4F02-BD5D-639FC2684CC4");
            // Attrib for BlockType: Page Parameter Filter:Filter Button Size
            RockMigrationHelper.DeleteAttribute("B62222F0-2EF5-476F-82E6-AAE96001130A");
            // Attrib for BlockType: Page Parameter Filter:Filter Button Text
            RockMigrationHelper.DeleteAttribute("731BB3C6-CDD4-4D69-9FA7-4592380E57CA");
            // Attrib for BlockType: Page Parameter Filter:Show Reset Filters Button
            RockMigrationHelper.DeleteAttribute("C9C347F0-76FA-4610-85B0-5385F57AE725");
            // Attrib for BlockType: Page Parameter Filter:Filters Per Row
            RockMigrationHelper.DeleteAttribute("F625D9F9-E549-4CD3-8E1D-273907AB7E70");
            // Attrib for BlockType: Page Parameter Filter:Block Title Icon CSS Class
            RockMigrationHelper.DeleteAttribute("6677A0F0-254A-4431-B336-D2266C8FE62F");
            // Attrib for BlockType: Page Parameter Filter:Block Title Text
            RockMigrationHelper.DeleteAttribute("03C296C8-B892-4731-AD2D-24B8A1614181");
            // Attrib for BlockType: Page Parameter Filter:Show Block Title
            RockMigrationHelper.DeleteAttribute("D0F41DA4-F5D3-4D43-99C5-B80954F3755B");
            // Attrib for BlockType: Prayer Request Details:Workflow
            RockMigrationHelper.DeleteAttribute("65A6CE53-8D80-44CB-A2E7-EF238EE1D251");
            // Attrib for BlockType: Prayer Request Details:Completion Xaml
            RockMigrationHelper.DeleteAttribute("5A61D74C-1478-4B5B-BEED-626E55FED732");
            // Attrib for BlockType: Prayer Request Details:Completion Action
            RockMigrationHelper.DeleteAttribute("4762659A-7181-4726-B1A3-7DCE765FE250");
            // Attrib for BlockType: Prayer Request Details:Enable Person Matching
            RockMigrationHelper.DeleteAttribute("F0023ACE-B747-4F91-8DD2-9B981E7F8E8C");
            // Attrib for BlockType: Prayer Request Details:Require Last Name
            RockMigrationHelper.DeleteAttribute("2219A0C8-8A25-4A77-BC99-B234BD39EB13");
            // Attrib for BlockType: Prayer Request Details:Require Campus
            RockMigrationHelper.DeleteAttribute("1372D8D2-0A20-4A3A-9718-DEF45AD877D5");
            // Attrib for BlockType: Prayer Request Details:Show Campus
            RockMigrationHelper.DeleteAttribute("D3A8E136-B07C-4603-A8CF-1BF083707EF1");
            // Attrib for BlockType: Prayer Request Details:Character Limit
            RockMigrationHelper.DeleteAttribute("B8B03429-0539-4FE6-9939-72E83E33CE31");
            // Attrib for BlockType: Prayer Request Details:Default To Public
            RockMigrationHelper.DeleteAttribute("45D6191C-7810-4162-B6C1-3CA961D904C3");
            // Attrib for BlockType: Prayer Request Details:Show Public Display Flag
            RockMigrationHelper.DeleteAttribute("9E7858D0-EE65-48CE-82EE-96F586C54E68");
            // Attrib for BlockType: Prayer Request Details:Show Urgent Flag
            RockMigrationHelper.DeleteAttribute("82F38CE9-6E17-4522-AD69-B0115FE850B8");
            // Attrib for BlockType: Prayer Request Details:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("5164FE14-55AC-4358-A844-25E060314376");
            // Attrib for BlockType: Prayer Request Details:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("0EB397D8-E06F-49F4-8166-4B1F1BC496EE");
            // Attrib for BlockType: Prayer Request Details:Default Category
            RockMigrationHelper.DeleteAttribute("59ABFB3E-50A6-430B-A039-D8782A913ADA");
            // Attrib for BlockType: Prayer Request Details:Parent Category
            RockMigrationHelper.DeleteAttribute("90F80215-926D-4255-8F15-A70F886F9D35");
            // Attrib for BlockType: Prayer Request Details:Show Category
            RockMigrationHelper.DeleteAttribute("9199A2A8-B775-4303-9924-1AD7B1D7F6D9");
            // Attrib for BlockType: Group View:Template
            RockMigrationHelper.DeleteAttribute("BC2408E8-B8EB-4F4F-81C1-87A586FC8AA8");
            // Attrib for BlockType: Group View:Show Leader List
            RockMigrationHelper.DeleteAttribute("215EA7BF-EB51-49B2-9299-0C72923D60D6");
            // Attrib for BlockType: Group View:Group Edit Page
            RockMigrationHelper.DeleteAttribute("ECF02A04-C7CC-4B09-BB52-E4F83CE0B53B");
            // Attrib for BlockType: Group Member View:Template
            RockMigrationHelper.DeleteAttribute("9DF8F030-18ED-4E22-9329-6CDA752B42F3");
            // Attrib for BlockType: Group Member View:Group Member Edit Page
            RockMigrationHelper.DeleteAttribute("F89684CC-23DA-4583-875D-C8C59A803C26");
            // Attrib for BlockType: Group Member List:Additional Fields
            RockMigrationHelper.DeleteAttribute("2F5C5913-86E1-46A1-9A2B-81A8E2477501");
            // Attrib for BlockType: Group Member List:Template
            RockMigrationHelper.DeleteAttribute("CD11FD90-1A4D-4F9A-A930-8F6901D08432");
            // Attrib for BlockType: Group Member List:Title Template
            RockMigrationHelper.DeleteAttribute("79791B63-A93D-4227-9FE2-0BF0591986FE");
            // Attrib for BlockType: Group Member List:Group Member Detail Page
            RockMigrationHelper.DeleteAttribute("B2A330CA-0FA2-49DF-8559-5C17DB1201A9");
            // Attrib for BlockType: Group Member Edit:Member Detail Page
            RockMigrationHelper.DeleteAttribute("BA971602-6478-41BB-B9BE-5A4043BFA2EE");
            // Attrib for BlockType: Group Member Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("7A0DA3CB-1A29-4006-A07C-F0AFEED2C31F");
            // Attrib for BlockType: Group Member Edit:Allow Note Edit
            RockMigrationHelper.DeleteAttribute("C1E2ABB4-A46F-41A9-B9D7-77A0F93C572D");
            // Attrib for BlockType: Group Member Edit:Allow Member Status Change
            RockMigrationHelper.DeleteAttribute("75193A26-9094-40A3-94DE-FAFED396C389");
            // Attrib for BlockType: Group Member Edit:Allow Role Change
            RockMigrationHelper.DeleteAttribute("1D874785-E718-4ED8-964B-C2F08FB98F38");
            // Attrib for BlockType: Group Edit:Group Detail Page
            RockMigrationHelper.DeleteAttribute("6DEE9631-DC19-4995-B9D2-EF7E54D3887A");
            // Attrib for BlockType: Group Edit:Attribute Category
            RockMigrationHelper.DeleteAttribute("DBE479D1-1F45-49DA-B890-985DE513067C");
            // Attrib for BlockType: Group Edit:Enable Public Status Edit
            RockMigrationHelper.DeleteAttribute("336727E1-31DE-4BE7-B7E7-4DA6474FF329");
            // Attrib for BlockType: Group Edit:Show Public Status
            RockMigrationHelper.DeleteAttribute("C86EDAE4-1413-491B-91B2-2202672F1F49");
            // Attrib for BlockType: Group Edit:Enable Active Status Edit
            RockMigrationHelper.DeleteAttribute("F34EC0A6-9840-493B-BC12-7D986BCD0F13");
            // Attrib for BlockType: Group Edit:Show Active Status
            RockMigrationHelper.DeleteAttribute("4CE968AE-9D61-4299-AFF4-39CF397E2A17");
            // Attrib for BlockType: Group Edit:Enable Group Capacity Edit
            RockMigrationHelper.DeleteAttribute("FD309D1F-E025-462F-A9F6-DABD6EC5BC6F");
            // Attrib for BlockType: Group Edit:Show Group Capacity
            RockMigrationHelper.DeleteAttribute("68574A24-D407-4CE1-B2F9-5AFF055BDD7E");
            // Attrib for BlockType: Group Edit:Enable Campus Edit
            RockMigrationHelper.DeleteAttribute("A9415559-E738-437D-B184-1266151D1572");
            // Attrib for BlockType: Group Edit:Show Campus
            RockMigrationHelper.DeleteAttribute("EC39E792-4E77-4903-8D07-3DB24FD05FBA");
            // Attrib for BlockType: Group Edit:Enable Description Edit
            RockMigrationHelper.DeleteAttribute("C92909C2-B40C-4246-9346-660C9FFB6298");
            // Attrib for BlockType: Group Edit:Show Description
            RockMigrationHelper.DeleteAttribute("62689D78-E402-4EAA-B18D-7BE9ECCB8056");
            // Attrib for BlockType: Group Edit:Enable Group Name Edit
            RockMigrationHelper.DeleteAttribute("48169F08-DE9F-46BE-8D29-A8C2C2CBF22B");
            // Attrib for BlockType: Group Edit:Show Group Name
            RockMigrationHelper.DeleteAttribute("8764E3E1-7C21-4047-BDC2-81EA1C7CB12F");
            // Attrib for BlockType: Group Attendance Entry:Allow Any Date Selection
            RockMigrationHelper.DeleteAttribute("B2EA9ABA-5AC5-4610-B1CB-7E8C04360DA5");
            // Attrib for BlockType: Group Attendance Entry:Show Save Button
            RockMigrationHelper.DeleteAttribute("89171507-C193-4566-9241-FF954915CD59");
            // Attrib for BlockType: Group Attendance Entry:Save Redirect Page
            RockMigrationHelper.DeleteAttribute("ABB015E9-3E12-4DB9-8DA9-356F60B23E54");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Back to Allow
            RockMigrationHelper.DeleteAttribute("A59B1827-0172-4950-B2C6-3FA8482AB4FA");
            // Attrib for BlockType: Group Attendance Entry:Number of Days Forward to Allow
            RockMigrationHelper.DeleteAttribute("C25E2A17-E0E3-41B6-B51A-9CAB7C2F8D0C");
            // Attrib for BlockType: Calendar View:Show Filter
            RockMigrationHelper.DeleteAttribute("F8385920-C7C9-4490-A34B-FAA77879829E");
            // Attrib for BlockType: Calendar View:Event Summary
            RockMigrationHelper.DeleteAttribute("A5574D5F-ADEE-4527-9BA0-52423FCA3895");
            // Attrib for BlockType: Calendar View:Audience Filter
            RockMigrationHelper.DeleteAttribute("E7C4A192-71E3-4AAA-B71B-E6E76F2C3B2C");
            // Attrib for BlockType: Calendar View:Detail Page
            RockMigrationHelper.DeleteAttribute("FA912251-F785-4B1C-85D0-4718B617226D");
            // Attrib for BlockType: Calendar View:Calendar
            RockMigrationHelper.DeleteAttribute("73A105A8-B5CB-4D8D-A01D-2DB0F84106C7");
            // Attrib for BlockType: Hero:Padding
            RockMigrationHelper.DeleteAttribute("D5B41DA8-40EA-4C50-B8BB-26E7BE9C3C1D");
            // Attrib for BlockType: Hero:Subtitle Color
            RockMigrationHelper.DeleteAttribute("4A6CFA0C-1D90-4920-9EB9-A9EABD43F489");
            // Attrib for BlockType: Hero:Title Color
            RockMigrationHelper.DeleteAttribute("03C5FA82-04E3-48C3-9E23-73AE373A6FFA");
            // Attrib for BlockType: Hero:Text Align
            RockMigrationHelper.DeleteAttribute("4256E5A9-5427-42A8-BA4F-F53C20E20CEE");
            // Attrib for BlockType: Hero:Height - Tablet
            RockMigrationHelper.DeleteAttribute("5AC6AFCA-4166-4822-979C-2EC9F89FEE4C");
            // Attrib for BlockType: Hero:Height - Phone
            RockMigrationHelper.DeleteAttribute("F77E4433-8C14-470A-B4B5-B08B7CEA4777");
            // Attrib for BlockType: Hero:Background Image - Tablet
            RockMigrationHelper.DeleteAttribute("0A0FD796-89CE-4318-BE10-0126863E9B3C");
            // Attrib for BlockType: Hero:Background Image - Phone
            RockMigrationHelper.DeleteAttribute("925839DE-19DB-4BFC-9523-5965697DE89A");
            // Attrib for BlockType: Hero:Subtitle
            RockMigrationHelper.DeleteAttribute("CCDCD9A8-ABC6-4896-AAC0-3748CDF25B50");
            // Attrib for BlockType: Hero:Title
            RockMigrationHelper.DeleteAttribute("2D8BA33B-2ACF-4C6C-A139-B09AED805059");
            // Attrib for BlockType: Block Type List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("214DB447-5CDD-4A46-BF6B-1DE0BB109180");
            // Attrib for BlockType: Block Type List:core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("06939EBC-FA3B-42C8-B2EE-933D0A6697D6");
            RockMigrationHelper.DeleteBlockType("6F8D3F32-ED4C-46A9-A8AD-8740BC3495D7"); // Page Parameter Filter
            RockMigrationHelper.DeleteBlockType("EDCAB491-74E4-48C8-BDD9-9A229795951C"); // Prayer Request Details
            RockMigrationHelper.DeleteBlockType("E1C8868F-40A7-4D66-B9C5-FA3441CDC4F8"); // Group View
            RockMigrationHelper.DeleteBlockType("B4E29B40-5368-46F5-BA73-7C9AD9CD2FD3"); // Group Member View
            RockMigrationHelper.DeleteBlockType("43A7FC7E-D9B4-4214-AE9A-3C3C87F2C9AD"); // Group Member List
            RockMigrationHelper.DeleteBlockType("48079AB0-41FF-4F48-9CD7-43600251061F"); // Group Member Edit
            RockMigrationHelper.DeleteBlockType("B6B9CBCC-BDA2-4389-83A7-0C334D449591"); // Group Edit
            RockMigrationHelper.DeleteBlockType("1918EDFB-8617-4420-9E60-C8575ACD4501"); // Group Attendance Entry
            RockMigrationHelper.DeleteBlockType("EE030FA7-B08C-41D6-84DC-3514A4084B2F"); // Calendar View
            RockMigrationHelper.DeleteBlockType("9A93E34E-838B-4B34-A1DE-5E2F520BF416"); // Hero
        }
    
        /// <summary>
        /// MB: [Migration] Adds Upgrade to SQL Server 2014 message.
        /// </summary>
        private void AddUpgradeSQLServerMessageUp()
        {
            Sql( @"
                IF NOT EXISTS(SELECT 1 FROM [dbo].[DefinedValue] WHERE [Guid] = '0b16bd4b-f55b-4adb-a744-fc4751731a7d')
                BEGIN
                -- Get Defined Type Id
                DECLARE @DefinedTypeId AS INT
                SELECT @DefinedTypeId = Id
                FROM DefinedType
                WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D'

                -- Get Completed Attribute Id
                DECLARE @CompletedAttributeId AS INT
                SELECT @CompletedAttributeId = Id
                FROM Attribute
                WHERE [Guid] = 'FBB2E564-29A3-4756-A255-38565B486000'

                -- Make room at the top of the list.
                UPDATE DefinedValue
                SET [Order] = [Order] + 1
                WHERE DefinedTypeId = @DefinedTypeId

                -- Insert new item into list.
                INSERT INTO [dbo].[DefinedValue]
                    ([IsSystem]
                    ,[DefinedTypeId]
                    ,[Order]
                    ,[Value]
                    ,[Description]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime]
                    ,[IsActive])
                VALUES
                    (1
                    ,@DefinedTypeId
                    ,0
                    ,'Upgrade to SQL Server 2014 or Later'
                    ,'Please remember that starting with v11 Rock will no longer support SQL Server 2012 (see this <a href=''https://community.rockrms.com/connect/ending-support-for-sql-server-2012''>link</a> for more details).'
                    ,'0B16BD4B-F55B-4ADB-A744-FC4751731A7D'
                    ,GETDATE()
                    ,GETDATE()
                    ,1)

                DECLARE @DefinedValueId AS INT
                SET @DefinedValueId = @@IDENTITY

                DECLARE @IsCompleted AS NVARCHAR(20)
                SET @IsCompleted = 'False'

                -- Calculate IsCompleted Value
                DECLARE @SqlServerVersion AS NVARCHAR(20) 
                SET @SqlServerVersion = CONVERT(NVARCHAR(20), SERVERPROPERTY('productversion'))
                SET @SqlServerVersion = SUBSTRING(@SqlServerVersion, 1, CHARINDEX('.', @SqlServerVersion) - 1)

                IF CAST(@SqlServerVersion AS INT) > 11
                BEGIN
	                SET @IsCompleted = 'True'
                END

                -- Insert Completed Attribute Value
                INSERT INTO [dbo].[AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Value]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime])
                VALUES
                    (0
                    ,@CompletedAttributeId
                    ,@DefinedValueId
                    ,@IsCompleted
                    ,'125EB24D-7BFC-4440-AD8C-014FB49EB95E'
                    ,GETDATE()
                    ,GETDATE())
            END"
            );
        }

        /// <summary>
        /// MB: [Migration] Adds Upgrade to SQL Server 2014 message.
        /// </summary>
        private void AddUpgradeSQLServerMessageDown()
        {
            Sql( @"
                DECLARE @DefinedTypeId AS INT
                SELECT @DefinedTypeId = Id
                FROM DefinedType
                WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D'
                
                IF EXISTS(SELECT 1 FROM [dbo].[DefinedValue] WHERE [Guid] = '0b16bd4b-f55b-4adb-a744-fc4751731a7d')
                BEGIN
                    DELETE [dbo].[AttributeValue]
                    WHERE [Guid] = '125EB24D-7BFC-4440-AD8C-014FB49EB95E'

                    DELETE [dbo].[DefinedValue]
                    WHERE [Guid] = '0B16BD4B-F55B-4ADB-A744-FC4751731A7D'

                    UPDATE DefinedValue
                    SET [Order] = [Order] - 1
                    WHERE DefinedTypeId = @DefinedTypeId
                END"
            );
        }

        /// <summary>
        /// MB: [Migration] Fixes Age Calculation error ISSUE #3881
        /// </summary>
        private void FixAgeCalcErrorUp()
        {
            Sql( @"/*
<doc>
	<summary>
 		This function returns the age given a birthdate.
	</summary>

	<returns>
		The age based on birthdate
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAge]( '2000-01-01')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAge](@BirthDate date) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Age INT
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = GETDATE()

	-- Year 0001 is a special year, which denotes no year selected therefore we shouldn't calculate the age.
	IF @BirthDate IS NOT NULL AND DATEPART( year, @BirthDate ) > 1
	BEGIN

		SET @Age = DATEPART( year, @CurrentDate ) - DATEPART( year, @BirthDate )
		IF @BirthDate > DATEADD( year, 0 - @Age, @CurrentDate )
		BEGIN
			SET @Age = @Age - 1
		END

	END
		
	RETURN @Age

END" );
        }

        /// <summary>
        /// MB: [Migration] Fixes Age Calculation error ISSUE #3881
        /// </summary>
        private void FixAgeCalcErrorDown()
        {
            Sql( @"/*
<doc>
	<summary>
 		This function returns the age given a birthdate.
	</summary>

	<returns>
		The age based on birthdate
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAge]( '2000-01-01')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAge](@BirthDate datetime) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Age INT

	IF @BirthDate IS NOT NULL
	BEGIN

		SET @Age = DATEPART( year, GETDATE() ) - DATEPART( year, @BirthDate )
		IF @BirthDate > DATEADD( year, 0 - @Age, GETDATE() )
		BEGIN
			SET @Age = @Age - 1
		END

	END
		
	RETURN @Age

END" );
        }
        /// <summary>
        /// SK:  [Migration] Fixed Attendance Analytics block to sort attendees grid by Campus
        /// </summary>
        private void AttendanceAnalyticsSortAttendeesByCampus()
        {
            Sql( MigrationSQL._202003031854249_Rollup_0303_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( MigrationSQL._202003031854249_Rollup_0303_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
        }

        /// <summary>
        /// SK: Updated Sample Data Url
        /// </summary>
        private void UpdateSampleDataUrl()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_7_0.xml" );
        }

        /// <summary>
        /// ED/NA: Remove reverted Global Attribute : Enable Route Domain Matching
        /// </summary>
        private void RemoveGlobalAttributeRouteDomainMatching()
        {
            // This was reverted by MP https://github.com/sparkdevnetwork/rock/commit/7d5461970d2296170e57e676477dafd322b308c6 but could have been inserted from pre-alpha-release.
            // Remove the reverted Global Attribute : Enable Route Domain Matching
            RockMigrationHelper.DeleteAttribute( "0B7DD63E-AD00-445E-8E9D-047956FEAFB3" );    // Global Attribute : Enable Route Domain Matching
        }

        /// <summary>
        /// MB: [Migration] Fixes Issue 3478
        /// </summary>
        private void Issue3478()
        {
            Sql( MigrationSQL._202003031854249_Rollup_0303_spFinance_PledgeAnalyticsQuery );
        }

        /// <summary>
        /// Removes the obsolete service jobs.
        /// </summary>
        private void RemoveObsoleteServiceJobs()
        {
            Sql( @"DELETE FROM [ServiceJob] WHERE [Class] IN ( 'Rock.Jobs.MigrateCommunicationMediumData', 'Rock.Jobs.MigrateHistorySummaryData', 'Rock.Jobs.MigrateInteractionsData' )" );
        }

        /// <summary>
        /// GJ: Page Icon Migration
        /// </summary>
        private void PageIconMigration()
        {
            RockMigrationHelper.UpdatePageIcon( "CB1C42A2-285C-4BC5-BB2C-DC442C8A97C2", "fa fa-file-signature" );
            RockMigrationHelper.UpdatePageIcon( "7096FA12-07A5-489C-83B0-EE55494A3484", "fa fa-file-signature" );
            RockMigrationHelper.UpdatePageIcon( "FAA6A2F2-4CFD-4B97-A0C2-8F4F9CE841F3", "fa fa-file-signature" );

            RockMigrationHelper.UpdatePageIcon( "3149959B-EFAC-4C2D-B0E8-8CF4FA1BB2FF", "fa fa-praying-hands" );
            RockMigrationHelper.UpdatePageIcon( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "fa fa-praying-hands" );
            RockMigrationHelper.UpdatePageIcon( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48", "fa fa-praying-hands" );
            RockMigrationHelper.UpdatePageIcon( "FA2A1171-9308-41C7-948C-C9EBEA5BD668", "fa fa-praying-hands" );

            RockMigrationHelper.UpdatePageIcon( "26547B83-A92D-4D7E-82ED-691F403F16B6", "fa fa-icons" );
            RockMigrationHelper.UpdatePageIcon( "D376EFD7-5B0D-44BF-A44D-03C466D2D30D", "fa fa-icons" );

            RockMigrationHelper.UpdatePageIcon( "1FD5698F-7279-463F-9637-9A80DB86BB86", "fa fa-search-location" );

            RockMigrationHelper.UpdatePageIcon( "BB2AF2B3-6D06-48C6-9895-EDF2BA254533", "fa fa-font-awesome" );
        }

        /// <summary>
        /// SK: Add Persisted Dataset Pages
        /// </summary>
        public void AddPersistedDatasetPagesUp()
        {
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Persisted Datasets", "", "37C20B91-737B-42D1-907D-9868104DBA7B", "fa  fa-database" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( true, "37C20B91-737B-42D1-907D-9868104DBA7B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Persisted Dataset Detail", "", "0ED8A471-B177-4AC3-933E-DFAB965E2E0D", "" ); // Site:Rock RMS
            // Add Block to Page: Persisted Datasets Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "37C20B91-737B-42D1-907D-9868104DBA7B".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"50ADE904-BB5C-40F9-A97D-ED8FF530B5A6".AsGuid(), "Persisted Dataset List","Main",@"",@"",0,"A6D89633-F251-4D12-A284-9B8BB75E9621");
            // Add Block to Page: Persisted Dataset Detail Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "0ED8A471-B177-4AC3-933E-DFAB965E2E0D".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F".AsGuid(), "Persisted Dataset Detail","Main",@"",@"",0,"918F7A4E-0953-47C4-8446-2CEABCF6048C");
            // Attrib Value for Block:Persisted Dataset List, Attribute:Max Preview Size (MB) Page: Persisted Datasets, Site: Rock RMS          
            RockMigrationHelper.AddBlockAttributeValue("A6D89633-F251-4D12-A284-9B8BB75E9621","CFA5BA3D-E6B9-4602-A1B2-CF54A2C75400",@"1");
            // Attrib Value for Block:Persisted Dataset List, Attribute:Detail Page Page: Persisted Datasets, Site: Rock RMS      
            RockMigrationHelper.AddBlockAttributeValue("A6D89633-F251-4D12-A284-9B8BB75E9621","C6F5517D-9902-4A72-87D6-513604DD1819",@"0ed8a471-b177-4ac3-933e-dfab965e2e0d");
            // Attrib Value for Block:Persisted Dataset List, Attribute:core.CustomGridEnableStickyHeaders Page: Persisted Datasets, Site: Rock RMS      
            RockMigrationHelper.AddBlockAttributeValue("A6D89633-F251-4D12-A284-9B8BB75E9621","A7A23287-39D3-4E5A-8AC1-4EB4AFFA7F14",@"False");  
        }

        /// <summary>
        /// SK: Add Persisted Dataset Pages
        /// </summary>
        public void AddPersistedDatasetPagesDown()
        {
            // Remove Block: Persisted Dataset Detail, from Page: Persisted Dataset Detail, Site: Rock RMS 
            RockMigrationHelper.DeleteBlock("918F7A4E-0953-47C4-8446-2CEABCF6048C");
            // Remove Block: Persisted Dataset List, from Page: Persisted Datasets, Site: Rock RMS        
            RockMigrationHelper.DeleteBlock("A6D89633-F251-4D12-A284-9B8BB75E9621");
            RockMigrationHelper.DeletePage( "0ED8A471-B177-4AC3-933E-DFAB965E2E0D" ); //  Page: Persisted Dataset Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "37C20B91-737B-42D1-907D-9868104DBA7B" ); //  Page: Persisted Datasets, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// SK: Add Merge Business to Business List
        /// </summary>
        public void AddBusinessMergePageUp()
        {
            RockMigrationHelper.AddPage( true, "F4DF4899-2D44-4997-BA9B-9D2C64958A20", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Business Merge", "", "0B863363-CCA3-4EDE-9ABA-7ED22A88F503", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "0B863363-CCA3-4EDE-9ABA-7ED22A88F503", "BusinessMerge/{Set}", "B789A41F-CB35-41AF-A330-3A90604A42B9" );// for Page:Business Merge
            // Add Block to Page: Business Merge Site: Rock RMS  
            RockMigrationHelper.AddBlock( true, "0B863363-CCA3-4EDE-9ABA-7ED22A88F503".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"9B274A75-1D9B-4533-9849-7892F10A7672".AsGuid(), "Business Merge","Main",@"",@"",0,"BFABA40E-2A01-4C42-90D5-1FF1E8530C7F");
            // Attrib for BlockType: Business List:Business Merge Page    
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1ACCF349-73A5-4568-B801-2A6A620791D9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Business Merge Page", "BusinessMergePage", "Business Merge Page", @"", 1, @"", "856EBEE0-EB5E-4231-843B-D72A185B9A5F" );
            // Attrib Value for Block:Business List, Attribute:Business Merge Page Page: Businesses, Site: Rock RMS        
            RockMigrationHelper.AddBlockAttributeValue("04E68378-E2F6-465B-925A-D8B124858C44","856EBEE0-EB5E-4231-843B-D72A185B9A5F",@"0b863363-cca3-4ede-9aba-7ed22a88f503,b789a41f-cb35-41af-a330-3a90604a42b9");  
            RockMigrationHelper.AddBlockAttributeValue("BFABA40E-2A01-4C42-90D5-1FF1E8530C7F","50133AA7-FEEE-4E7A-A593-8BE8EC9C425D",@"d2b43273-c64f-4f57-9aae-9571e1982bac,873f057a-e32b-de81-4a48-8a1c6eef3c07");  
        }
    
        /// <summary>
        /// SK: Add Merge Business to Business List
        /// </summary>
        public void AddBusinessMergePageDown()
        {
            // Attrib for BlockType: Business List:Business Merge Page            
            RockMigrationHelper.DeleteAttribute("856EBEE0-EB5E-4231-843B-D72A185B9A5F");
            RockMigrationHelper.DeleteBlock("BFABA40E-2A01-4C42-90D5-1FF1E8530C7F");
            RockMigrationHelper.DeletePage( "0B863363-CCA3-4EDE-9ABA-7ED22A88F503" ); //  Page: Business Merge, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// SK: Added Connection Request Workflow Triggers Job
        /// </summary>
        public void AddConnectionRequestWorkflowTriggersJobUp()
        {
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ConnectionRequestWorkflowTriggers' AND [Guid] = '92F0EEB7-EAC7-436E-ACC6-199BD2349E52' )
                    BEGIN
                      INSERT INTO [ServiceJob] 
		                    ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus], [Guid] )
                      VALUES 
		                    (0, 1, 'Connection Request Workflow Triggers',  '', 'Rock.Jobs.ConnectionRequestWorkflowTriggers','0 0 7 * * ?', 1, '92F0EEB7-EAC7-436E-ACC6-199BD2349E52')
                    END" );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.ConnectionRequestWorkflowTriggers", "Number of Days to Look Back", "Number of Days to Look Back", @"This is the number of days that the workflow should look back to find connection requests with past future follow-up dates.", 0, @"1", "35D43311-1202-4A5E-B49E-436CFA689B3D", "NumberOfDaysToLookBack" ); 
        }
    
        /// <summary>
        /// SK: Added Connection Request Workflow Triggers Job
        /// </summary>
        public void AddConnectionRequestWorkflowTriggersJobDown()
        {
            RockMigrationHelper.DeleteAttribute( "35D43311-1202-4A5E-B49E-436CFA689B3D" );

            Sql(@"
                IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ConnectionRequestWorkflowTriggers' AND [Guid] = '92F0EEB7-EAC7-436E-ACC6-199BD2349E52' )
                BEGIN
                    DELETE [ServiceJob]  WHERE [Guid] = '92F0EEB7-EAC7-436E-ACC6-199BD2349E52';
                END" );  
        }
    
        /// <summary>
        /// SK: Remove IsDeceased filter from "Adult Members & Attendees" data view
        /// </summary>
        public void RemoveIsDeceasedFilterFromADataView()
        {
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Selection" );
            string selection = @"[    ""IsDeceased"",    ""False""  ]";
            string selection1 = @"[""Property_IsDeceased"",""1"",""False""]";
            Sql( $@"
            IF EXISTS ( SELECT * FROM [DataViewFilter] where [Guid]='53B219C5-A25D-42C3-9345-DFDFF6331CF1' AND ({targetColumn} LIKE '%{selection}%' OR {targetColumn} LIKE '%{selection1}%'))
            BEGIN
                DELETE FROM [DataViewFilter] where [Guid]='53B219C5-A25D-42C3-9345-DFDFF6331CF1'
            END"
            );
        }
    }
}
