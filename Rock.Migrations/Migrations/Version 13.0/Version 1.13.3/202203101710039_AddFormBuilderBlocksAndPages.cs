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
    public partial class AddFormBuilderBlocksAndPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // ADD BLOCK TYPES
            // -----------------------------------------------

            /*
                 3/10/2022 - N.A.

                 The Obsidian block Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail (A61C5E3C-2267-4CF7-B305-D8AF0DB9660B) was attempted to be
                 added via \Version 1.13.2\202203012130384_CodeGenerated_20220301.cs and 202202152250406_CodeGenerated_20220215.cs however
                 the underlying EntityType was not registered yet -- so the BlockType was never registered.

                 Reason: The EntityType must be added before you can use UpdateMobileBlockType().
            */
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail", "Form Builder Detail", "Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail, Rock, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.OBSIDIAN_FORM_BUILDER_DETAIL_BLOCK_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Workflow.FormBuilder.FormTemplateDetail", "Form Template Detail", "Rock.Blocks.Workflow.FormBuilder.FormTemplateDetail, Rock, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.OBSIDIAN_FORM_TEMPLATE_DETAIL_BLOCK_TYPE );

            // Add/Update Obsidian Block Type
            //   Name:Form Builder Detail
            //   Category:Obsidian > Workflow > FormBuilder
            //   EntityType:Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail
            RockMigrationHelper.UpdateMobileBlockType( "Form Builder Detail", "Edits the details of a workflow Form Builder action.", "Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail", "Obsidian > Workflow > FormBuilder", "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B" );

            // Add/Update Obsidian Block Type
            //   Name: Form Template Detail
            //   Category:Obsidian > Workflow > FormBuilder
            //   Path: -
            //   EntityType:Rock.Blocks.Workflow.FormBuilder.FormTemplateDetail
            RockMigrationHelper.UpdateMobileBlockType( "Form Template Detail", "Edits the details of a workflow Form Builder Template.", "Rock.Blocks.Workflow.FormBuilder.FormTemplateDetail", "Obsidian > Workflow > FormBuilder", "A522F0A4-39D4-4047-A012-EF42F7D2759D" );

            // Add/Update BlockType 
            //   Name: Form Analytics
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormAnalytics.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Form Analytics", "Shows the interaction and analytics data for the given WorkflowTypeId.", "~/Blocks/WorkFlow/FormBuilder/FormAnalytics.ascx", "WorkFlow > FormBuilder", "778EFA7B-56BC-4ABB-B86D-FFD87B97691F" );

            // Add/Update BlockType 
            //   Name: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Form Submission List", "Shows a list forms submitted for a given FormBuilder form.", "~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx", "WorkFlow > FormBuilder", "A23592BB-25F7-4A81-90CD-46700724110A" );

            // Add/Update BlockType 
            //   Name: Form List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "Form List", "Shows the List of existing forms for the selected category.", "~/Blocks/WorkFlow/FormBuilder/FormList.ascx", "WorkFlow > FormBuilder", "B7C76420-9B34-422A-B161-87BDB45DD50C" );


            // ADD BLOCK TYPE ATTRIBUTE VALUES
            // -----------------------------------------------

            // Attribute for BlockType
            //   BlockType: Form Template List (originally added via \Version 1.13.2\202202152250406_CodeGenerated_20220215.cs)
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "1DEFF313-39CF-400F-895A-82ADB9F192BD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"Page to display details about a workflow.", 0, @"A522F0A4-39D4-4047-A012-EF42F7D2759D", "D91A7A0C-58D7-45F9-8DC2-952718486E72" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Form Builder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Form Builder Page", "FormBuilderPage", "Form Builder Page", @"The page that has the form builder editor.", 2, @"", "BEC4A715-3C80-4ED9-8C64-1761E9E4CF20" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A23592BB-25F7-4A81-90CD-46700724110A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Page", "AnalyticsPage", "Analytics Page", @" The page that shows the analytics for this form.", 3, @"C72FFD9E-514B-433C-901D-D36D15FD5D55", "6BFA165A-3705-4C17-9B52-B3088CA8EB71" );

            // Attribute for BlockType
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "778EFA7B-56BC-4ABB-B86D-FFD87B97691F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Submissions Page", "SubmissionsPage", "Submissions Page", @"The page that shows the submissions for this form.", 0, @"1212C6F8-31AD-4AB4-B202-E0D5DD797468", "9009BE92-3CF7-49B6-B2F7-0EDF727ADA14" );

            // Attribute for BlockType
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Form Builder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "778EFA7B-56BC-4ABB-B86D-FFD87B97691F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Form Builder Page", "FormBuilderPage", "Form Builder Page", @"The page that has the form builder editor.", 1, @"", "A752B415-A311-482E-8167-AD9C8B7E0FE5" );

            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7C76420-9B34-422A-B161-87BDB45DD50C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Submissions Page", "SubmissionsPage", "Submissions Page", @"The page that shows the submissions for this form.", 0, @"1212C6F8-31AD-4AB4-B202-E0D5DD797468", "3377A7F4-F0E7-4D01-A2DC-5DE5FA283DCD" );

            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Form Builder Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7C76420-9B34-422A-B161-87BDB45DD50C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Form Builder Page", "FormBuilderPage", "Form Builder Page", @"The page that has the form builder editor.", 1, @"", "887FC589-D5E9-46A8-9223-72689A2E84AB" );

            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B7C76420-9B34-422A-B161-87BDB45DD50C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Page", "AnalyticsPage", "Analytics Page", @" The page that shows the analytics for this form.", 2, @"C72FFD9E-514B-433C-901D-D36D15FD5D55", "56544897-0D3D-48B4-9E4A-95EE766DBEA2" );


            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Submissions Page", "SubmissionsPage", "Submissions Page", @"The page that contains the Submissions block to view submissions existing submissions for this form.", 0, @"1212C6F8-31AD-4AB4-B202-E0D5DD797468", "4762FA04-D260-4B88-883E-CD27B57464A1" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Analytics Page", "AnalyticsPage", "Analytics Page", @"The page that contains the Analytics block to view statistics on existing submissions for this form.", 1, @"C72FFD9E-514B-433C-901D-D36D15FD5D55", "4F58C338-58D1-4F4F-BEDC-F8663899689F" );


            // ADD PAGES
            // -----------------------------------------------

            // Add Page               
            //  Internal Name: Form Builder Templates              
            //  Site: Rock RMS              
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Form Builder Templates", "", "316E8E0C-9714-4DAF-896F-1154D52D095B", "fa fa-poll-h" );

            // Add Page               
            //  Internal Name: Form Template Details              
            //  Site: Rock RMS              
            RockMigrationHelper.AddPage( true, "316E8E0C-9714-4DAF-896F-1154D52D095B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Form Template Details", "", "65DAD8F8-0A5A-4C97-B275-089CD7C35E9C", "" );

            // Add Page 
            //  Internal Name: Form Builder (aka Create New Form or Form List)
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "CDB27DB2-977C-415A-AED5-D0751DFD5DF2", "C2467799-BB45-4251-8EE6-F0BF27201535", "Form Builder", "", "4F77819C-8F69-4418-933E-08F63E7FC4F9", "" );

            // Add Page 
            //  Internal Name: Form Submissions
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4F77819C-8F69-4418-933E-08F63E7FC4F9", "C2467799-BB45-4251-8EE6-F0BF27201535", "Form Submissions", "", "1212C6F8-31AD-4AB4-B202-E0D5DD797468", "" );

            // Add Page 
            //  Internal Name: Form Builder Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4F77819C-8F69-4418-933E-08F63E7FC4F9", "C2467799-BB45-4251-8EE6-F0BF27201535", "Form Builder Detail", "", "5A5BA13E-8489-42D0-B1CA-06F21BDEAB14", "" );

            // Add Page 
            //  Internal Name: Form Analytics
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4F77819C-8F69-4418-933E-08F63E7FC4F9", "C2467799-BB45-4251-8EE6-F0BF27201535", "Form Analytics", "", "C72FFD9E-514B-433C-901D-D36D15FD5D55", "" );


            // ADD ROUTES
            // -----------------------------------------------
#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route              
            //   Page:Form Builder Templates              
            //   Route:admin/general/form-templates              
            RockMigrationHelper.AddPageRoute( "316E8E0C-9714-4DAF-896F-1154D52D095B", "admin/general/form-templates", "2E460922-4C66-4648-908B-AC15C82EDA0A" );

            // xxxxxxxxx TODO verify or correct this line
            // Add Page Route              
            //   Page:Form Template Details              
            //   Route:admin/general/form-template/{FormTemplateId}              
            RockMigrationHelper.AddPageRoute( "65DAD8F8-0A5A-4C97-B275-089CD7C35E9C", "admin/general/form-template/{FormTemplateId}", "06091772-CB8A-46E4-AF6B-BDB931DB62AC" );
#pragma warning restore CS0618 // Type or member is obsolete


            // ADD BLOCKS
            // -----------------------------------------------

            // Add Block               
            //  Block Name: Form Template List              
            //  Page Name: Form Builder Templates              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "316E8E0C-9714-4DAF-896F-1154D52D095B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "1DEFF313-39CF-400F-895A-82ADB9F192BD".AsGuid(), "Form Template List", "Main", @"", @"", 0, "79EF82C6-8D5B-47A9-9FB8-5B5E3E8CA718" );

            // Add Block               
            //  Block Name: Form Template Detail              
            //  Page Name: Form Template Details              
            //  Layout: -              
            //  Site: Rock RMS              
            RockMigrationHelper.AddBlock( true, "65DAD8F8-0A5A-4C97-B275-089CD7C35E9C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A522F0A4-39D4-4047-A012-EF42F7D2759D".AsGuid(), "Form Template Detail", "Main", @"", @"", 0, "BFDEA6F3-75D5-4DF7-9679-1055956F8F00" );

            // Add Block 
            //  Block Name: Form List
            //  Page Name: Form Builder
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4F77819C-8F69-4418-933E-08F63E7FC4F9".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "B7C76420-9B34-422A-B161-87BDB45DD50C".AsGuid(), "Form List", "Main", @"", @"", 0, "F92FD4B9-7C1E-45AD-87FA-4FF876A553DF" );

            // Add Block 
            //  Block Name: Form Submission List
            //  Page Name: Form Submissions
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "1212C6F8-31AD-4AB4-B202-E0D5DD797468".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A23592BB-25F7-4A81-90CD-46700724110A".AsGuid(), "Form Submission List", "Main", @"", @"", 0, "E1779CFA-5D54-435E-AC7D-C66BBBF2962C" );

            // Add Block -- Obsidian BlockType Rock.Blocks.Workflow.FormBuilder.FormBuilderDetail (A61C5E3C-2267-4CF7-B305-D8AF0DB9660B) was added via \Version 1.13.2\202203012130384_CodeGenerated_20220301.cs
            //  Block Name: Form Builder Detail
            //  Page Name: Form Builder Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5A5BA13E-8489-42D0-B1CA-06F21BDEAB14".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B".AsGuid(), "Form Builder Detail", "Main", @"", @"", 0, "A7963754-6E17-44D1-A8D9-52FFE21C08B6" );

            // Add Block 
            //  Block Name: Form Analytics
            //  Page Name: Form Analytics
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C72FFD9E-514B-433C-901D-D36D15FD5D55".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "778EFA7B-56BC-4ABB-B86D-FFD87B97691F".AsGuid(), "Form Analytics", "Main", @"", @"", 0, "9ABE7E5F-CC65-4A5B-ADC4-ECE44B9E8506" );


            // ADD BLOCK SETTINGS
            // -----------------------------------------------

            // Add Block Attribute Value
            //   Block: Form Template List
            //   BlockType: Form Template List
            //   Category: Workflow > FormBuilder
            //   Block Location: Page=Form Builder Templates, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 65dad8f8-0a5a-4c97-b275-089cd7c35e9c,06091772-cb8a-46e4-af6b-bdb931db62ac */
            RockMigrationHelper.AddBlockAttributeValue( "79EF82C6-8D5B-47A9-9FB8-5B5E3E8CA718", "D91A7A0C-58D7-45F9-8DC2-952718486E72", @"65dad8f8-0a5a-4c97-b275-089cd7c35e9c,06091772-cb8a-46e4-af6b-bdb931db62ac" ); // TODO correct this default page/route xxxxxxxxxxxxxx

            Sql( @"
-- Form Builder Templates
UPDATE [Page]
SET [DisplayInNavWhen] = 2
WHERE [Guid] = '316E8E0C-9714-4DAF-896F-1154D52D095B'

-- Form Builder (main page)
UPDATE [Page]
SET [DisplayInNavWhen] = 2
WHERE [Guid] = '4F77819C-8F69-4418-933E-08F63E7FC4F9'
" );

            // Add Block Attribute Value
            //   Block: Form List
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Builder, Site=Rock RMS
            //   Attribute: Submissions Page
            /*   Attribute Value: 1212c6f8-31ad-4ab4-b202-e0d5dd797468 */
            RockMigrationHelper.AddBlockAttributeValue( "F92FD4B9-7C1E-45AD-87FA-4FF876A553DF", "3377A7F4-F0E7-4D01-A2DC-5DE5FA283DCD", @"1212c6f8-31ad-4ab4-b202-e0d5dd797468" );

            // Add Block Attribute Value
            //   Block: Form List
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Builder, Site=Rock RMS
            //   Attribute: Form Builder Page
            /*   Attribute Value: 5a5ba13e-8489-42d0-b1ca-06f21bdeab14 */
            RockMigrationHelper.AddBlockAttributeValue( "F92FD4B9-7C1E-45AD-87FA-4FF876A553DF", "887FC589-D5E9-46A8-9223-72689A2E84AB", @"5a5ba13e-8489-42d0-b1ca-06f21bdeab14" );

            // Add Block Attribute Value
            //   Block: Form List
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Builder, Site=Rock RMS
            //   Attribute: Analytics Page
            /*   Attribute Value: c72ffd9e-514b-433c-901d-d36d15fd5d55 */
            RockMigrationHelper.AddBlockAttributeValue( "F92FD4B9-7C1E-45AD-87FA-4FF876A553DF", "56544897-0D3D-48B4-9E4A-95EE766DBEA2", @"c72ffd9e-514b-433c-901d-d36d15fd5d55" );

            // Add Block Attribute Value
            //   Block: Form Builder Detail
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Builder Detail, Site=Rock RMS
            //   Attribute: Submissions Page
            /*   Attribute Value: 1212c6f8-31ad-4ab4-b202-e0d5dd797468 */
            RockMigrationHelper.AddBlockAttributeValue( "A7963754-6E17-44D1-A8D9-52FFE21C08B6", "4762FA04-D260-4B88-883E-CD27B57464A1", @"1212c6f8-31ad-4ab4-b202-e0d5dd797468" );

            // Add Block Attribute Value
            //   Block: Form Builder Detail
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Builder Detail, Site=Rock RMS
            //   Attribute: Analytics Page
            /*   Attribute Value: c72ffd9e-514b-433c-901d-d36d15fd5d55 */
            RockMigrationHelper.AddBlockAttributeValue( "A7963754-6E17-44D1-A8D9-52FFE21C08B6", "4F58C338-58D1-4F4F-BEDC-F8663899689F", @"c72ffd9e-514b-433c-901d-d36d15fd5d55" );

            // Add Block Attribute Value
            //   Block: Form Submission List
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Submissions, Site=Rock RMS
            //   Attribute: Form Builder Page
            /*   Attribute Value: 5a5ba13e-8489-42d0-b1ca-06f21bdeab14 */
            RockMigrationHelper.AddBlockAttributeValue( "E1779CFA-5D54-435E-AC7D-C66BBBF2962C", "BEC4A715-3C80-4ED9-8C64-1761E9E4CF20", @"5a5ba13e-8489-42d0-b1ca-06f21bdeab14" );

            // Add Block Attribute Value
            //   Block: Form Submission List
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Submissions, Site=Rock RMS
            //   Attribute: Analytics Page
            /*   Attribute Value: c72ffd9e-514b-433c-901d-d36d15fd5d55 */
            RockMigrationHelper.AddBlockAttributeValue( "E1779CFA-5D54-435E-AC7D-C66BBBF2962C", "6BFA165A-3705-4C17-9B52-B3088CA8EB71", @"c72ffd9e-514b-433c-901d-d36d15fd5d55" );

            // Add Block Attribute Value
            //   Block: Form Submission List
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Submissions, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ba547eed-5537-49cf-bd4e-c583d760788c */
            RockMigrationHelper.AddBlockAttributeValue( "E1779CFA-5D54-435E-AC7D-C66BBBF2962C", "3A20F8BE-0AAF-4BEC-9261-0D56E84F709A", @"ba547eed-5537-49cf-bd4e-c583d760788c" );

            // Add Block Attribute Value
            //   Block: Form Submission List
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Block Location: Page=Form Submissions, Site=Rock RMS
            //   Attribute: Person Profile Page
            /*   Attribute Value: 08dbd8a5-2c35-4146-b4a8-0f7652348b25 */
            RockMigrationHelper.AddBlockAttributeValue( "E1779CFA-5D54-435E-AC7D-C66BBBF2962C", "2FD30DAC-B9CC-48D3-B1C3-3878640A46EC", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );

            // Add Block Attribute Value
            //   Block: Form Analytics
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormAnalytics
            //   Block Location: Page=Form Analytics, Site=Rock RMS
            //   Attribute: Submissions Page
            /*   Attribute Value: 1212c6f8-31ad-4ab4-b202-e0d5dd797468 */
            RockMigrationHelper.AddBlockAttributeValue( "9ABE7E5F-CC65-4A5B-ADC4-ECE44B9E8506", "9009BE92-3CF7-49B6-B2F7-0EDF727ADA14", @"1212c6f8-31ad-4ab4-b202-e0d5dd797468" );

            // Add Block Attribute Value
            //   Block: Form Analytics
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormAnalytics
            //   Block Location: Page=Form Analytics, Site=Rock RMS
            //   Attribute: Form Builder Page
            /*   Attribute Value: 5a5ba13e-8489-42d0-b1ca-06f21bdeab14 */
            RockMigrationHelper.AddBlockAttributeValue( "9ABE7E5F-CC65-4A5B-ADC4-ECE44B9E8506", "A752B415-A311-482E-8167-AD9C8B7E0FE5", @"5a5ba13e-8489-42d0-b1ca-06f21bdeab14" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.DeleteAttribute( "56544897-0D3D-48B4-9E4A-95EE766DBEA2" );

            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Form Builder Page
            RockMigrationHelper.DeleteAttribute( "887FC589-D5E9-46A8-9223-72689A2E84AB" );

            // Attribute for BlockType
            //   BlockType: Form List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.DeleteAttribute( "3377A7F4-F0E7-4D01-A2DC-5DE5FA283DCD" );

            // Attribute for BlockType
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormAnalytics
            //   Attribute: Form Builder Page
            RockMigrationHelper.DeleteAttribute( "A752B415-A311-482E-8167-AD9C8B7E0FE5" );

            // Attribute for BlockType
            //   BlockType: Form Analytics
            //   Category: WorkFlow > FormAnalytics
            //   Attribute: Submissions Page
            RockMigrationHelper.DeleteAttribute( "9009BE92-3CF7-49B6-B2F7-0EDF727ADA14" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.DeleteAttribute( "6BFA165A-3705-4C17-9B52-B3088CA8EB71" );

            // Attribute for BlockType
            //   BlockType: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Form Builder Page
            RockMigrationHelper.DeleteAttribute( "BEC4A715-3C80-4ED9-8C64-1761E9E4CF20" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Analytics Page
            RockMigrationHelper.DeleteAttribute( "4F58C338-58D1-4F4F-BEDC-F8663899689F" );

            // Attribute for BlockType
            //   BlockType: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Attribute: Submissions Page
            RockMigrationHelper.DeleteAttribute( "4762FA04-D260-4B88-883E-CD27B57464A1" );

            // Remove Block
            //  Name: Form Template Detail, from Page: Form Template Details, Site: Rock RMS
            //  from Page: Form Template Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "BFDEA6F3-75D5-4DF7-9679-1055956F8F00" );

            // Remove Block
            //  Name: Form Template List, from Page: Form Builder Templates, Site: Rock RMS
            //  from Page: Form Builder Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "79EF82C6-8D5B-47A9-9FB8-5B5E3E8CA718" );

            // Remove Block
            //  Name: Form Analytics, from Page: Form Analytics, Site: Rock RMS
            //  from Page: Form Analytics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "9ABE7E5F-CC65-4A5B-ADC4-ECE44B9E8506" );

            // Remove Block
            //  Name: Form Submission List, from Page: Form Submissions, Site: Rock RMS
            //  from Page: Form Submissions, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E1779CFA-5D54-435E-AC7D-C66BBBF2962C" );

            // Remove Block
            //  Name: Form Builder Detail, from Page: Form Builder Detail, Site: Rock RMS
            //  from Page: Form Builder Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "A7963754-6E17-44D1-A8D9-52FFE21C08B6" );

            // Remove Block
            //  Name: Form List, from Page: Form Builder, Site: Rock RMS
            //  from Page: Form Builder, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F92FD4B9-7C1E-45AD-87FA-4FF876A553DF" );

            // Delete BlockType 
            //   Name: Form Template Detail
            //   Category: Obsidian > Workflow > FormBuilder
            //   Path: -
            //   EntityType:Rock.Blocks.Workflow.FormBuilder.FormTemplateDetail
            RockMigrationHelper.DeleteBlockType( "A522F0A4-39D4-4047-A012-EF42F7D2759D" );

            // Delete BlockType 
            //   Name: Form Builder Detail
            //   Category: WorkFlow > FormBuilder
            //   Path: -
            //   EntityType: Form Builder Detail
            RockMigrationHelper.DeleteBlockType( "A61C5E3C-2267-4CF7-B305-D8AF0DB9660B" );

            // Delete BlockType 
            //   Name: Form List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "B7C76420-9B34-422A-B161-87BDB45DD50C" );

            // Delete BlockType 
            //   Name: Form Analytics
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormAnalytics.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "778EFA7B-56BC-4ABB-B86D-FFD87B97691F" );

            // Delete BlockType 
            //   Name: Form Submission List
            //   Category: WorkFlow > FormBuilder
            //   Path: ~/Blocks/WorkFlow/FormBuilder/FormSubmissionList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "A23592BB-25F7-4A81-90CD-46700724110A" );

            // Delete Page 
            //  Internal Name: Form Template Details
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "65DAD8F8-0A5A-4C97-B275-089CD7C35E9C" );

            // Delete Page 
            //  Internal Name: Form Builder Templates
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "316E8E0C-9714-4DAF-896F-1154D52D095B" );

            // Delete Page 
            //  Internal Name: Form Analytics
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "C72FFD9E-514B-433C-901D-D36D15FD5D55" );

            // Delete Page 
            //  Internal Name: Form Builder Detail
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "5A5BA13E-8489-42D0-B1CA-06F21BDEAB14" );

            // Delete Page 
            //  Internal Name: Form Submissions
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "1212C6F8-31AD-4AB4-B202-E0D5DD797468" );

            // Delete Page 
            //  Internal Name: Form Builder
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "4F77819C-8F69-4418-933E-08F63E7FC4F9" );
        }
    }
}