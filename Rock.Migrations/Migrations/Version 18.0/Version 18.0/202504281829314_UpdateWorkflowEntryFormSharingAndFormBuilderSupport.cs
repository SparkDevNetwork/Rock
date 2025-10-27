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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateWorkflowEntryFormSharingAndFormBuilderSupport : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JMH_AutoGenerateWorkflowTypeSlugsUp();
            JMH_AddWorkflowEntryEnableForFormSharingSettingUp();
            JMH_AddWorkflowEntryUseFormNameForPageTitleUp();
            JMH_AddFormBuilderCommunicationTemplateCategoryUp();
            JMH_AddFormSubmissionResultSystemCommunicationUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JMH_AddFormSubmissionResultSystemCommunicationDown();
            JMH_AddFormBuilderCommunicationTemplateCategoryDown();
            JMH_AddWorkflowEntryUseFormNameForPageTitleDown();
            JMH_AddWorkflowEntryEnableForFormSharingSettingDown();
            JMH_AutoGenerateWorkflowTypeSlugsDown();
        }
        
        private void JMH_AutoGenerateWorkflowTypeSlugsUp()
        {
            Sql( RockMigrationSQL._202504281829314_UpdateWorkflowEntryFormSharingAndFormBuilderSupport_GenerateWorkflowTypeSlugs );
            AlterColumn( "dbo.WorkflowType", "Slug", c => c.String( maxLength: 400, nullable: false ) );
        }
        
        private void JMH_AutoGenerateWorkflowTypeSlugsDown()
        {
            AlterColumn( "dbo.WorkflowType", "Slug", c => c.String( maxLength: 400, nullable: true ) );
        }
        
        private void JMH_AddWorkflowEntryEnableForFormSharingSettingUp()
        {
            // Attribute for BlockType
            //   BlockType: Workflow Entry (WebForms)
            //   Category: WorkFlow
            //   Attribute: Enable for Form Sharing
            this.RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable for Form Sharing", "EnableForFormSharing", "Enable for Form Sharing", @"Marks this block instance as available for form sharing. When enabled, the Form Builder can display this block as a shareable link option.", 9, @"False", "904C07BA-DC75-4684-9B3D-25EAE6DFEE73" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry (Obsidian)
            //   Category: Workflow
            //   Attribute: Enable for Form Sharing
            this.RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable for Form Sharing", "EnableForFormSharing", "Enable for Form Sharing", @"Marks this block instance as available for form sharing. When enabled, the Form Builder can display this block as a shareable link option.", 15, @"False", "40EE92DE-61F9-474E-BC49-ACA04A38C113" );

            // Update the Generic Workflow Entry page's Workflow Entry (WebForms) "Enable for Form Sharing" block setting.
            // No need to handle the Obsidian block setting here. The WebForms block setting value will be copied
            // if it gets chopped/swapped with the Obsidian version later.
            this.RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.EXTERNAL_WORKFLOW_ENTRY, "904C07BA-DC75-4684-9B3D-25EAE6DFEE73", "True" );
        }
        
        private void JMH_AddWorkflowEntryUseFormNameForPageTitleUp()
        {
            // Attribute for BlockType
            //   BlockType: Workflow Entry (WebForms)
            //   Category: Workflow
            //   Attribute: Use Form Name for Page Title
            this.RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Form Name for Page Title", "UseFormNameForPageTitle", "Use Form Name for Page Title", @"When enabled, the page title will be overridden with the name of the form associated with this workflow.", 10, @"False", "41C8F578-62D4-46B0-BC1F-8352B5ADD3BF" );

            // Attribute for BlockType
            //   BlockType: Workflow Entry (Obsidian)
            //   Category: Workflow
            //   Attribute: Use Form Name for Page Title
            this.RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9116AAD8-CF16-4BCE-B0CF-5B4D565710ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Form Name for Page Title", "UseFormNameForPageTitle", "Use Form Name for Page Title", @"When enabled, the page title will be overridden with the name of the form associated with this workflow.", 16, @"False", "06B761D3-2F95-4F2E-983D-3460E4845F98" );
        }
        
        private void JMH_AddFormBuilderCommunicationTemplateCategoryUp()
        {
            this.RockMigrationHelper.UpdateCategory( 
                SystemGuid.EntityType.SYSTEM_COMMUNICATION, 
                "Form Builder", 
                string.Empty, 
                "System Communications triggered by forms created with the Form Builder. These may include confirmation messages, internal alerts, or follow-up instructions automatically sent when a form is submitted.",
                SystemGuid.Category.SYSTEM_COMMUNICATION_FORM_BUILDER );
        }

        private void JMH_AddFormSubmissionResultSystemCommunicationUp()
        {
            this.RockMigrationHelper.UpdateSystemCommunication(
                "Form Builder",
                "Form Submission Result",
                string.Empty, // from address
                string.Empty, // from name
                string.Empty, // to
                string.Empty, // cc
                string.Empty, // bcc
                "Form Submission Result: {{ Workflow.Name }}", // subject
                @"{{ 'Global' | Attribute:'EmailHeader' }}

{% assign attributes = Action.FormAttributes %}

{% if Person.NickName %}
    <p>Hello {{ Person.NickName }},</p>
{% else %}
    <p>Hello,</p>
{% endif %}

<p>Here is a summary of the information submitted by {{ Workflow.InitiatorPersonAlias.Person.FullName }}:</p>

<ul>
    {% for attribute in attributes %}
        <li><strong>{{ attribute.Name }}:</strong> {{ attribute.Value }}</li>
    {% endfor %}
</ul>

<p>Thank you!</p>


{{ 'Global' | Attribute:'EmailFooter' }}",
                "D5414D4F-538E-4578-8330-D6CA8FD5F664" );
        }
        
        private void JMH_AddWorkflowEntryEnableForFormSharingSettingDown()
        {
            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Workflow
            //   Attribute: Enable for Form Sharing
            RockMigrationHelper.DeleteAttribute("40EE92DE-61F9-474E-BC49-ACA04A38C113");

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Enable for Form Sharing
            RockMigrationHelper.DeleteAttribute("904C07BA-DC75-4684-9B3D-25EAE6DFEE73");
        }
        
        private void JMH_AddWorkflowEntryUseFormNameForPageTitleDown()
        {
            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: Workflow
            //   Attribute: Use Form Name for Page Title
            RockMigrationHelper.DeleteAttribute("41C8F578-62D4-46B0-BC1F-8352B5ADD3BF");

            // Attribute for BlockType
            //   BlockType: Workflow Entry
            //   Category: WorkFlow
            //   Attribute: Use Form Name for Page Title
            RockMigrationHelper.DeleteAttribute("06B761D3-2F95-4F2E-983D-3460E4845F98");
        }
        
        private void JMH_AddFormBuilderCommunicationTemplateCategoryDown()
        {
            this.RockMigrationHelper.DeleteCategory( SystemGuid.Category.SYSTEM_COMMUNICATION_FORM_BUILDER );
        }

        private void JMH_AddFormSubmissionResultSystemCommunicationDown()
        {
            this.RockMigrationHelper.DeleteSystemCommunication( "D5414D4F-538E-4578-8330-D6CA8FD5F664" );
        }
    }
}
