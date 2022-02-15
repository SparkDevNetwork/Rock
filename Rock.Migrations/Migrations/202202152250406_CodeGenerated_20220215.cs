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
    public partial class CodeGenerated_20220215 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            
           // Add/Update BlockType 
           //   Name: Form Submission List
           //   Category: WorkFlow > FormBuilder
           //   Path: ~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Form Submission List","Shows a list forms submitted for a given FormBuilder form.","~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx","WorkFlow > FormBuilder","A23592BB-25F7-4A81-90CD-46700724110A");

           // Add/Update BlockType 
           //   Name: Form Template List
           //   Category: WorkFlow > FormBuilder
           //   Path: ~/Blocks/WorkFlow/FormBuilder/FormTemplateList.ascx
           //   EntityType: -
            RockMigrationHelper.UpdateBlockType("Form Template List","Shows a list forms submitted for a given FormBuilder form.","~/Blocks/WorkFlow/FormBuilder/FormTemplateList.ascx","WorkFlow > FormBuilder","1DEFF313-39CF-400F-895A-82ADB9F192BD");

            // Add/Update Obsidian Block Type
            //   Name:Attributes
            //   Category:Obsidian > Core
            //   EntityType:Rock.Blocks.Core.Attributes
            RockMigrationHelper.UpdateMobileBlockType("Attributes", "Allows for the managing of attributes.", "Rock.Blocks.Core.Attributes", "Obsidian > Core", "5B31B69F-306B-4540-8261-EBB1F353F0EF");

            // Add/Update Obsidian Block Type
            //   Name:Form Builder Detail
            //   Category:Obsidian > Workflow > Form Builder
            //   EntityType:Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail
            RockMigrationHelper.UpdateMobileBlockType("Form Builder Detail", "Edits the details of a workflow Form Builder action.", "Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail", "Obsidian > Workflow > Form Builder", "47B4706C-9B47-4FA8-8C26-C8932B7A6BB3");

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registrant List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6EADE364-ECAF-436B-9A6D-E81055073105" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registrant List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "805AA404-B0C5-4957-9932-4B6C54C28438" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity", "Entity", "Entity", @"Entity Name", 0, @"", "7170401B-6CF6-45A8-99D2-B9BC1A33953E" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Column", "EntityQualifierColumn", "Entity Qualifier Column", @"The entity column to evaluate when determining if this attribute applies to the entity", 1, @"", "2D8FA715-481D-4BE9-AA41-1080CECB14CD" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Qualifier Value", "EntityQualifierValue", "Entity Qualifier Value", @"The entity column value to evaluate.  Attributes will only apply to entities with this value", 2, @"", "F57E4EA2-455F-4EC5-BE02-84887D7AA289" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Setting of Values", "AllowSettingofValues", "Allow Setting of Values", @"Should UI be available for setting values of the specified Entity ID?", 3, @"false", "C45BFCEF-D991-4B59-BE2C-A5B9F5E79485" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Entity Id", "EntityId", "Entity Id", @"The entity id that values apply to", 4, @"0", "6D1127DF-60AB-4EBD-B1BB-1FEC94704C70" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "Enable Show In Grid", @"Should the 'Show In Grid' option be displayed when editing attributes?", 5, @"false", "04AC320F-D51C-44FE-A254-3DDECE5FE208" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Category Filter", "CategoryFilter", "Category Filter", @"A comma separated list of category GUIDs to limit the display of attributes to.", 6, @"", "694939C1-CBE0-4638-9139-C7308CA9C7C8" );

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5B31B69F-306B-4540-8261-EBB1F353F0EF", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Columns on Grid", "HideColumnsOnGrid", "Hide Columns on Grid", @"The grid columns that should be hidden.", 7, @"", "AB1F7C2E-4F8D-4004-9917-1751BD1A2B85" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > Form Builder
            //   Attribute: Submissions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47B4706C-9B47-4FA8-8C26-C8932B7A6BB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Submissions Page", "SubmissionsPage", "Submissions Page", @"The page that contains the Submissions block to view submissions existing submissions for this form.", 0, @"", "639E43DA-A3F7-4C47-98E0-FAB4297768EF" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > Form Builder
            //   Attribute: Analytics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "47B4706C-9B47-4FA8-8C26-C8932B7A6BB3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Page", "AnalyticsPage", "Analytics Page", @"The page that contains the Analytics block to view statistics on existing submissions for this form.", 1, @"", "1BC86F28-0F82-4348-866E-39C634D970A4" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to display details about a workflow.", 0, @"BA547EED-5537-49CF-BD4E-C583D760788C", "3A20F8BE-0AAF-4BEC-9261-0D56E84F709A" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Person Profile Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "Person Profile Page", @"Page to display person details.", 1, @"08DBD8A5-2C35-4146-B4A8-0F7652348B25", "2FD30DAC-B9CC-48D3-B1C3-3878640A46EC" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: FormBuilder Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "FormBuilder Detail Page", "FormBuilderDetailPage", "FormBuilder Detail Page", @"Page to edit using the form builder.", 2, @"", "C9119D72-5963-4198-BD09-CE530930D8E0" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Detail Page", "AnalyticsDetailPage", "Analytics Detail Page", @"Page used to view the analytics for this form.", 3, @"", "FA6590E1-7143-4858-892F-327908D69C76" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DEFF313-39CF-400F-895A-82ADB9F192BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to display details about a workflow.", 0, @"BA547EED-5537-49CF-BD4E-C583D760788C", "D91A7A0C-58D7-45F9-8DC2-952718486E72" );

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: FormBuilder Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DEFF313-39CF-400F-895A-82ADB9F192BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "FormBuilder Detail Page", "FormBuilderDetailPage", "FormBuilder Detail Page", @"Page to edit using the form builder.", 1, @"", "1E8014D4-B6F1-4BA7-B6F4-372B1F2E9C18" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: FormBuilder Detail Page
            RockMigrationHelper.DeleteAttribute("1E8014D4-B6F1-4BA7-B6F4-372B1F2E9C18");

            // Attribute for BlockType
            //   BlockType: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("D91A7A0C-58D7-45F9-8DC2-952718486E72");

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Detail Page
            RockMigrationHelper.DeleteAttribute("FA6590E1-7143-4858-892F-327908D69C76");

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: FormBuilder Detail Page
            RockMigrationHelper.DeleteAttribute("C9119D72-5963-4198-BD09-CE530930D8E0");

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Person Profile Page
            RockMigrationHelper.DeleteAttribute("2FD30DAC-B9CC-48D3-B1C3-3878640A46EC");

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("3A20F8BE-0AAF-4BEC-9261-0D56E84F709A");

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > Form Builder
            //   Attribute: Analytics Page
            RockMigrationHelper.DeleteAttribute("1BC86F28-0F82-4348-866E-39C634D970A4");

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: Obsidian > Workflow > Form Builder
            //   Attribute: Submissions Page
            RockMigrationHelper.DeleteAttribute("639E43DA-A3F7-4C47-98E0-FAB4297768EF");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Hide Columns on Grid
            RockMigrationHelper.DeleteAttribute("AB1F7C2E-4F8D-4004-9917-1751BD1A2B85");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Category Filter
            RockMigrationHelper.DeleteAttribute("694939C1-CBE0-4638-9139-C7308CA9C7C8");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Enable Show In Grid
            RockMigrationHelper.DeleteAttribute("04AC320F-D51C-44FE-A254-3DDECE5FE208");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Id
            RockMigrationHelper.DeleteAttribute("6D1127DF-60AB-4EBD-B1BB-1FEC94704C70");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Allow Setting of Values
            RockMigrationHelper.DeleteAttribute("C45BFCEF-D991-4B59-BE2C-A5B9F5E79485");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Value
            RockMigrationHelper.DeleteAttribute("F57E4EA2-455F-4EC5-BE02-84887D7AA289");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity Qualifier Column
            RockMigrationHelper.DeleteAttribute("2D8FA715-481D-4BE9-AA41-1080CECB14CD");

            // Attribute for BlockType
            //   BlockType: Attributes
            //   Category: Obsidian > Core
            //   Attribute: Entity
            RockMigrationHelper.DeleteAttribute("7170401B-6CF6-45A8-99D2-B9BC1A33953E");

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registrant List
            //   Category: Event
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute("805AA404-B0C5-4957-9932-4B6C54C28438");

            // Attribute for BlockType
            //   BlockType: Registration Instance - Registrant List
            //   Category: Event
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute("6EADE364-ECAF-436B-9A6D-E81055073105");

            // Delete BlockType 
            //   Name: Form Template List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormTemplateList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("1DEFF313-39CF-400F-895A-82ADB9F192BD");

            // Delete BlockType 
            //   Name: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType("A23592BB-25F7-4A81-90CD-46700724110A");

            // Delete BlockType 
            //   Name: Form Builder Detail
            //   Category: Obsidian > Workflow > Form Builder
            //   Path: -
            //   EntityType: Form Builder Detail
            RockMigrationHelper.DeleteBlockType("47B4706C-9B47-4FA8-8C26-C8932B7A6BB3");

            // Delete BlockType 
            //   Name: Attributes
            //   Category: Obsidian > Core
            //   Path: -
            //   EntityType: Attributes
            RockMigrationHelper.DeleteBlockType("5B31B69F-306B-4540-8261-EBB1F353F0EF");
        }
    }
}
