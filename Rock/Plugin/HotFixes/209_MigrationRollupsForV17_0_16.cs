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

using System;
using System.Collections.Generic;

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 209, "1.16.4" )]
    public class MigrationRollupsForV17_0_16 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateStarkListAndStarkDetailBlockNameUp();
            AddObsidianControlGalleryToCMSUp();
            SwapBlocksUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateStarkListAndStarkDetailBlockNameDown();
            AddObsidianControlGalleryToCMSDown();
        }

        #region KA: Migration to append 'Legacy' to StarkDetail and StarkList block name

        private void UpdateStarkListAndStarkDetailBlockNameUp()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark Detail (Legacy)'
WHERE GUID = 'D6B14847-B652-49E2-9D4B-658D502F0AEC'
" );

            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark List (Legacy)'
WHERE GUID = 'E333D1CC-CB55-4E73-8568-41DAD296971C'
" );
        }

        private void UpdateStarkListAndStarkDetailBlockNameDown()
        {
            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark Detail'
WHERE GUID = 'D6B14847-B652-49E2-9D4B-658D502F0AEC'
" );

            Sql( @"
UPDATE BlockType
SET [Name] = 'Stark List'
WHERE GUID = 'E333D1CC-CB55-4E73-8568-41DAD296971C'
" );
        }

        #endregion

        #region KA: Migration to add Obsidian control gallery page to CMS

        private void AddObsidianControlGalleryToCMSUp()
        {
            // Add Page to the CMS Page
            RockMigrationHelper.AddPage( "B4A24AB7-9369-4055-883F-4F4892C39AE3", "C2467799-BB45-4251-8EE6-F0BF27201535", "Obsidian Control Gallery", "", guid: "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", iconCssClass: "fa fa-magic", insertAfterPageGuid: "706C0584-285F-4014-BA61-EC42C8F6F76B" );

            // Add custom route
            RockMigrationHelper.AddOrUpdatePageRoute( "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", "admin/cms/control-gallery-obsidian", "B50D4D65-A7BA-40E3-AD87-00F5CEC6A874" );

            // Add Obsidian Control Gallery Block to page
            RockMigrationHelper.AddBlock( "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886", "", "6FAB07FF-D4C6-412B-B13F-7B881ECBFAD0", "Obsidian Control Gallery", "Main", "", "", 0, "6150D7FB-08FC-4A4E-BCBF-359F779A5A93" );
        }

        private void AddObsidianControlGalleryToCMSDown()
        {
            RockMigrationHelper.DeleteBlock( "6150D7FB-08FC-4A4E-BCBF-359F779A5A93" );
            RockMigrationHelper.DeletePageRoute( "B50D4D65-A7BA-40E3-AD87-00F5CEC6A874" );
            RockMigrationHelper.DeletePage( "0CD9A871-2F7D-4713-8FBB-0D8EDDE02886" );
        }

        #endregion

        #region PA: Swap blocks for v1.17.0.29

        private void SwapBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Block Types - 1.17.0.29",
                blockTypeReplacements: new Dictionary<string, string> {
{ "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" }, // Group Scheduler
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string>{
{ "37D43C21-1A4D-4B13-9555-EF0B7304EB8A",  "FutureWeeksToShow" }
            } );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.29

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        private void RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.NoteTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.NoteTypeList", "Note Type List", "Rock.Blocks.Core.NoteTypeList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "CA07CFE0-AC86-4AD5-A4E2-03A90B0281F5" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.AttributeMatrixTemplateDetail", "Attribute Matrix Template Detail", "Rock.Blocks.Core.AttributeMatrixTemplateDetail, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "86759D9B-281C-4C1B-95E6-D4305731C03B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.PersonalLinkList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.PersonalLinkList", "Personal Link List", "Rock.Blocks.Cms.PersonalLinkList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "06F055E8-D396-4AD6-B542-342EE5907D74" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Bus.ConsumerList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Bus.ConsumerList", "Consumer List", "Rock.Blocks.Bus.ConsumerList, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "444BD66E-A715-4367-A3A6-5C0BBD6E93B4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.FinancialStatementTemplateDetail", "Financial Statement Template Detail", "Rock.Blocks.Finance.FinancialStatementTemplateDetail, Rock.Blocks, Version=1.17.0.28, Culture=neutral, PublicKeyToken=null", false, false, "FEEA3B29-3FCE-4216-AB28-E1F69C67A574" );



            // Add/Update Obsidian Block Type
            //   Name:Note Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.NoteTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Note Type List", "Displays a list of note types.", "Rock.Blocks.Core.NoteTypeList", "Core", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" );

            // Add/Update Obsidian Block Type
            //   Name:Attribute Matrix Template Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.AttributeMatrixTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Attribute Matrix Template Detail", "Displays the details of a particular attribute matrix template.", "Rock.Blocks.Core.AttributeMatrixTemplateDetail", "Core", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" );

            // Add/Update Obsidian Block Type
            //   Name:Personal Link List
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.PersonalLinkList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Personal Link List", "Displays a list of personal links.", "Rock.Blocks.Cms.PersonalLinkList", "CMS", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" );

            // Add/Update Obsidian Block Type
            //   Name:Consumer List
            //   Category:Bus
            //   EntityType:Rock.Blocks.Bus.ConsumerList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Consumer List", "Displays a list of consumers.", "Rock.Blocks.Bus.ConsumerList", "Bus", "63F5509A-3D71-4F0F-A074-FA5869856038" );

            // Add/Update Obsidian Block Type
            //   Name:Financial Statement Template Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.FinancialStatementTemplateDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Financial Statement Template Detail", "Displays the details of the statement template.", "Rock.Blocks.Finance.FinancialStatementTemplateDetail", "Finance", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" );


            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the note type details.", 0, @"", "EDE4E14E-C251-4B42-BF04-9D4B017950FB" );

            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "7BF8AE56-BE37-4F6B-BC33-35E78DCE9C1F" );

            // Attribute for BlockType
            //   BlockType: Note Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "E8A3D7B0-09BF-4C66-A439-89B16D105A0A" );

            // Attribute for BlockType
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "F8B7B83F-2380-4198-A08D-6603AEBABB8D" );

            // Attribute for BlockType
            //   BlockType: Personal Link List
            //   Category: CMS
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "F3E689B0-6401-45E3-AECB-4657C752268E" );

        }

        // PA: Chop blocks for v1.17.0.29
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.29",
                blockTypeReplacements: new Dictionary<string, string> {
{ "616D1A98-067D-43B8-B7F5-41FB12FB894E", "53A34D60-31B8-4D22-BC42-E3B669ED152B" }, // Auth Client List
{ "312EAD0E-4068-4211-8410-2EB45B7D8BAB", "8246EF8B-27E9-449E-9CAB-1C267B31DBC2" }, // Auth Client Detail
{ "7EFD5D30-2FF0-4C75-86A2-984A8F45D8A5", "63F5509A-3D71-4F0F-A074-FA5869856038" }, // Consumer List
{ "D4262E61-9CB2-4FF0-A7CA-90BAD1141BF5", "96C5DF9E-6F5C-4E55-92F1-61FE16A18563" }, // Attribute Matrix Template Detail
{ "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "23E3CA31-6A1F-43CB-AC06-374BD9CB9FA5" }, // Note Type List
{ "5DA1D088-2142-4645-AF9C-EF52DA5B4EEA", "9E901A5A-82C2-4788-9623-3720FFC4DAEC" }, // Note Type Detail
{ "3C9D442B-D066-43FA-9380-98C60936992E", "662AF7BB-5B61-43C6-BDA6-A6E7AAB8FC00" }, // Media Folder Detail
{ "ACAF8CEB-18CD-4BAE-BF6A-12C08CF6D61F", "6035AC10-07A5-4EDD-A1E9-10862FC41494" }, // Persisted Dataset Detail
{ "E7546752-C3DC-4B96-88D9-A431F2D1C989", "6C9E7EBF-8F27-48EF-94C4-900AC3A2C167" }, // Personal Link List
{ "78DB9184-97CF-4FC4-BD71-8F3ABE4100BA", "3D13455F-7E5C-46F7-975A-4A5CE12BD330" }, // Financial Statement Template Detail
{ "49F3D87E-BD8D-43D4-8217-340F3DFF4562", "CDAB601D-1369-44CB-A146-4E80C7D66BCD" }, // Apple TV App Detail
{ "5D58BF6A-3914-420C-9013-53CE8A15E390", "A8062FE5-5BCD-48AC-8C37-2124462656A7" }, // Workflow Trigger Detail
{ "005E5980-E2D2-4958-ACB6-BECBC6D1F5C4", "F140B415-9BB3-4492-844E-5A529517A484" }, // Tag Report


                    // blocks chopped in v1.17.0.28
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List
                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }, // Registration List Lava
                { "BEC5B592-9E9E-4C55-BD0D-2B8065A1802E", "EntityType" }, // Note Type List
            } );
        }

        #endregion
    }
}
