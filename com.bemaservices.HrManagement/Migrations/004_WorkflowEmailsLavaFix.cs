﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 4, "1.9.4" )]
    public class WorkflowEmailsLavaFix : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
         //   PtoAllocationWorkflow();
            PtoRequestWorkflow();
        }

        private void PtoRequestWorkflow()
        {
            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate", "546C6C01-5C8B-449E-A16A-580D92D0317B", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ActivateActivity", "38907A90-1634-4A93-8017-619326A4A582", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.PersistWorkflow", "F1A39347-6FE0-43D4-89FB-544195088ECF", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.Redirect", "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunLava", "BC21E57A-1477-44B3-A7C2-61A806118945", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.ShowHtml", "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E8ABD802-372C-47BE-82B1-96F50DB5169E" ); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "739FD425-5B8C-4605-B775-7E4D9D4C11DB", "Activity", "Activity", "The activity type to activate", 0, @"", "02D5A7A5-8781-46B4-B9FC-AF816829D240" ); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "38907A90-1634-4A93-8017-619326A4A582", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3809A78C-B773-440C-8E3F-A8E81D0DAE08" ); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "HTML", "HTML", "The HTML to show. <span class='tip tip-lava'></span>", 0, @"", "640FBD13-FEEB-4313-B6AC-6E5CF6E005DF" ); // Rock.Workflow.Action.ShowHtml:HTML
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "05673872-1E8D-42CD-9517-7CAFBC6976F9" ); // Rock.Workflow.Action.ShowHtml:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide Status Message", "HideStatusMessage", "Whether or not to hide the built-in status message.", 1, @"False", "46ACD91A-9455-41D2-8849-C2305F364418" ); // Rock.Workflow.Action.ShowHtml:Hide Status Message
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "B3B530F2-602D-44AB-A7AB-F0839F2B0754" ); // Rock.Workflow.Action.ShowHtml:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DD610F3E-2E83-41AE-B63B-9B163B87F82E" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Existing Pto Request", "PTO_REQUEST_ATTRIBUTE_KEY", "The Pto Request to update.", 0, @"", "C957F777-F0FE-4D05-BB22-10D7C7A5C437" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Existing Pto Request
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Allocation|Attribute Value", "ALLOCATION_KEY", "The allocation or an attribute that contains the allocation of the pto request. <span class='tip tip-lava'></span>", 1, @"", "EC01344E-61BF-4E22-88E3-36051BCAABE7" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Allocation|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Approval State|Attribute Value", "APPROVAL_STATE_KEY", "The Approval State or an attribute that contains the Approval State of the pto request. <span class='tip tip-lava'></span>", 7, @"", "080025FD-9E80-4158-8D7F-FBF3ED12A2E1" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Approval State|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Approver|Attribute Value", "APPROVER_KEY", "The approver or an attribute that contains the approver of the pto request. <span class='tip tip-lava'></span>", 6, @"", "A781A20B-4F21-47CA-9BCF-1654565DB5F6" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Approver|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "End Date|Attribute Value", "ENDDATE_KEY", "The end date or an attribute that contains the end date of the pto request. <span class='tip tip-lava'></span>", 3, @"", "8304DE14-DA5C-41FD-BA30-026D91A492C7" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:End Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Exclude Weekends|Attribute Value", "EXCLUDE_WEEKENDS_KEY", "Whether to Include weekends, or an attribute that contains whether or not to incldue weekends. <span class='tip tip-lava'></span>", 8, @"False", "552610AA-C128-4A6F-AAB6-20ACC0C5F060" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Exclude Weekends|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Hours|Attribute Value", "HOURS_KEY", "The hours per day or an attribute that contains the hours per day of the pto request. <span class='tip tip-lava'></span>", 4, @"", "858BFCA2-E793-446E-B146-87D5FC6783A0" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Hours|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Reason|Attribute Value", "PTO_REASON_KEY", "The reason or an attribute that contains the reason of the pto request. <span class='tip tip-lava'></span>", 5, @"", "C6A51AEB-18CB-4591-BDF8-D4017CF38DCF" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Reason|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Start Date|Attribute Value", "STARTDATE_KEY", "The start date or an attribute that contains the start date of the pto request. <span class='tip tip-lava'></span>", 2, @"", "3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Start Date|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "546C6C01-5C8B-449E-A16A-580D92D0317B", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "A9B5EAF8-9CC9-4521-9FC1-480875B11CAA" ); // com.bemaservices.HrManagement.Workflow.Action.PtoRequestUpdate:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "E3667110-339F-4FE3-B6B7-084CF9633580" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Value", "The <span class='tip tip-lava'></span> to run.", 0, @"", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4" ); // Rock.Workflow.Action.RunLava:Lava
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "F1924BDC-9B79-4018-9D4A-C3516C87A514" ); // Rock.Workflow.Action.RunLava:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to store the result in.", 1, @"", "431273C6-342D-4030-ADC7-7CDEDC7F8B27" ); // Rock.Workflow.Action.RunLava:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "The Lava commands that should be enabled for this action.", 2, @"", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5" ); // Rock.Workflow.Action.RunLava:Enabled Lava Commands
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "BC21E57A-1477-44B3-A7C2-61A806118945", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "1B833F48-EFC2-4537-B1E3-7793F6863EAA" ); // Rock.Workflow.Action.RunLava:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "385A255B-9F48-4625-862B-26231DBAC53A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "50B01639-4938-40D2-A791-AA0EB4F86847" ); // Rock.Workflow.Action.PersistWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Persist Immediately", "PersistImmediately", "This action will normally cause the workflow to be persisted (saved) once all the current activities/actions have completed processing. Set this flag to true, if the workflow should be persisted immediately. This is only required if a subsequent action needs a persisted workflow with a valid id.", 0, @"False", "82744A46-0110-4728-BD3D-66C85C5FCB2F" ); // Rock.Workflow.Action.PersistWorkflow:Persist Immediately
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F1A39347-6FE0-43D4-89FB-544195088ECF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "86F795B0-0CB6-4DA4-9CE4-B11D0922F361" ); // Rock.Workflow.Action.PersistWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "1DAA899B-634B-4DD5-A30A-69BAC235B383" ); // Rock.Workflow.Action.Redirect:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Url|Url Attribute", "Url", "The full Url to redirect to, for example: http://www.rockrms.com  <span class='tip tip-lava'></span>", 0, @"", "051BD491-817F-45DD-BBAC-875BA79E3644" ); // Rock.Workflow.Action.Redirect:Url|Url Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Processing Options", "ProcessingOptions", "How should workflow continue processing?", 1, @"0", "581736CE-76CF-46CE-A401-60A9E9EBCC1A" ); // Rock.Workflow.Action.Redirect:Processing Options
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "66A0A14E-45EC-45CD-904E-F0AC4344E1DB" ); // Rock.Workflow.Action.Redirect:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "PTO Requests", "fa fa-clock", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", 0 ); // PTO Requests

            #endregion

            #region PTO Request

            RockMigrationHelper.UpdateWorkflowType( false, true, "PTO Request", "", "CD21D1FD-B9DB-4122-B252-86E8FD85CEEC", "PTO Request", "fa fa-list-ol", 28800, false, 0, "EBF1D986-8BBD-4888-8A7E-43AF5914751C", 0 ); // PTO Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "", 0, @"", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", false ); // PTO Request:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Start Date", "StartDate", "", 1, @"", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", false ); // PTO Request:Start Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "End Date", "EndDate", "", 2, @"", "286C5A76-9113-49C4-A209-078E856BD0B2", false ); // PTO Request:End Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "C3206242-E213-4038-99EB-1A563B375997", "PTO Allocation", "PTOAllocation", "", 3, @"", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", false ); // PTO Request:PTO Allocation
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Approval State", "ApprovalState", "", 4, @"0", "FFC543BE-7B65-425B-A56C-AD441986FA2C", false ); // PTO Request:Approval State
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "90ECF283-5344-4168-9224-E0D26E9B7ECB", "PTO Request", "PTORequest", "", 5, @"", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", false ); // PTO Request:PTO Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Supervisor", "Supervisor", "", 6, @"", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", false ); // PTO Request:Supervisor
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Supervisor Attribute", "SupervisorAttribute", "", 7, @"67afd5a3-28f3-404f-a3b8-88630061f294", "CB12115A-8783-472C-B980-FE404D67F12E", false ); // PTO Request:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "HasViewRights", "HasViewRights", "", 8, @"", "160380E7-EF1F-4D6A-82C3-712FABD0C263", false ); // PTO Request:HasViewRights
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "HasReviewRights", "HasReviewRights", "", 9, @"", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", false ); // PTO Request:HasReviewRights
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Hours / Day", "HoursDay", "", 10, @"", "4180D0D3-A144-4974-B364-34292969C1A9", false ); // PTO Request:Hours / Day
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Reason", "Reason", "", 11, @"", "1136E804-A793-4081-9902-F8E7ED0CDD69", false ); // PTO Request:Reason
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Approver", "Approver", "", 12, @"", "35198559-8801-424C-B410-7145E00D3F67", false ); // PTO Request:Approver
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Remaining Hours", "RemainingHours", "", 13, @"0", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", false ); // PTO Request:Remaining Hours
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "C757A554-3009-4214-B05D-CEA2B2EA6B8F", "Requested Hours", "RequestedHours", "", 14, @"", "52166C99-8A84-437E-8E01-A0282CD6E5BC", false ); // PTO Request:Requested Hours
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Weekends", "ExcludeWeekends", "", 15, @"", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", false ); // PTO Request:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Cancel Request", "CancelRequest", "", 16, @"", "65B0E759-671A-421C-B2FC-E9885BA4D38D", false ); // PTO Request:Cancel Request
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Time Frame Validation Error", "TimeFrameValidationError", "", 17, @"", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", false ); // PTO Request:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "7BD25DC9-F34A-478D-BEF9-0C787F5D39B8", "Human Resources", "HumanResources", "", 18, @"6f8aaba3-5bc8-468b-90dd-f0686f38e373", "A28F65E4-D1C9-41B5-89CB-065F7809B298", false ); // PTO Request:Human Resources
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Requested Hours YTD", "RequestedHoursYTD", "", 19, @"", "A8101AB8-B260-407D-9C8D-EF4063C73922", false ); // PTO Request:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Reason for Cancellation", "CancelReason", "", 20, @"", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", false ); // PTO Request:Reason for Cancellation
            RockMigrationHelper.UpdateAttributeQualifier( "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", "EnableSelfSelection", @"False", "B739C4CC-FB6D-4760-849C-46E11F7023B6" ); // PTO Request:Person:EnableSelfSelection
            RockMigrationHelper.UpdateAttributeQualifier( "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", "datePickerControlType", @"Date Picker", "86EDAA53-9E4C-48F9-A482-FDE3EA070110" ); // PTO Request:Start Date:datePickerControlType
            RockMigrationHelper.UpdateAttributeQualifier( "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", "displayCurrentOption", @"False", "E9877EEB-C546-4EE1-8BF3-4B740736530D" ); // PTO Request:Start Date:displayCurrentOption
            RockMigrationHelper.UpdateAttributeQualifier( "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", "displayDiff", @"False", "612FF8F7-4A70-46AC-B0C5-9B6DB13F36C5" ); // PTO Request:Start Date:displayDiff
            RockMigrationHelper.UpdateAttributeQualifier( "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", "format", @"", "5F442441-0DCB-4E6C-8715-2282A38F4076" ); // PTO Request:Start Date:format
            RockMigrationHelper.UpdateAttributeQualifier( "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", "futureYearCount", @"", "74DE5344-F2F8-4628-8B5C-602C027DBE31" ); // PTO Request:Start Date:futureYearCount
            RockMigrationHelper.UpdateAttributeQualifier( "286C5A76-9113-49C4-A209-078E856BD0B2", "datePickerControlType", @"Date Picker", "ECBEF1BB-049A-4645-A99C-CFA717EC5DD0" ); // PTO Request:End Date:datePickerControlType
            RockMigrationHelper.UpdateAttributeQualifier( "286C5A76-9113-49C4-A209-078E856BD0B2", "displayCurrentOption", @"False", "567DB0DA-970E-4A3A-897A-432CE086C250" ); // PTO Request:End Date:displayCurrentOption
            RockMigrationHelper.UpdateAttributeQualifier( "286C5A76-9113-49C4-A209-078E856BD0B2", "displayDiff", @"False", "C8DE160F-402C-4F31-8E76-372B5F219762" ); // PTO Request:End Date:displayDiff
            RockMigrationHelper.UpdateAttributeQualifier( "286C5A76-9113-49C4-A209-078E856BD0B2", "format", @"", "1FEBAFE7-329F-4B5D-B32D-C2936A178A35" ); // PTO Request:End Date:format
            RockMigrationHelper.UpdateAttributeQualifier( "286C5A76-9113-49C4-A209-078E856BD0B2", "futureYearCount", @"", "EF662E76-D74E-4842-8ECF-5CB5E2AE0CC1" ); // PTO Request:End Date:futureYearCount
            RockMigrationHelper.UpdateAttributeQualifier( "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", "fieldtype", @"ddl", "37D121A3-1B3C-4179-800F-AEE05A0B2EF7" ); // PTO Request:PTO Allocation:fieldtype
            RockMigrationHelper.UpdateAttributeQualifier( "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", "repeatColumns", @"", "458378D9-C68F-413B-8E97-87D2E48E2ED4" ); // PTO Request:PTO Allocation:repeatColumns
            RockMigrationHelper.UpdateAttributeQualifier( "FFC543BE-7B65-425B-A56C-AD441986FA2C", "fieldtype", @"ddl", "DD012574-4434-4B6E-8DB3-47E3B02AF324" ); // PTO Request:Approval State:fieldtype
            RockMigrationHelper.UpdateAttributeQualifier( "FFC543BE-7B65-425B-A56C-AD441986FA2C", "repeatColumns", @"", "56C8921B-D585-491C-8026-F2B5530F24CD" ); // PTO Request:Approval State:repeatColumns
            RockMigrationHelper.UpdateAttributeQualifier( "FFC543BE-7B65-425B-A56C-AD441986FA2C", "values", @"0^Pending, 1^Approved, 2^Denied,3^Cancelled", "A2DEAEE2-BEE6-4BBB-AB7A-81E84FA6FC15" ); // PTO Request:Approval State:values
            RockMigrationHelper.UpdateAttributeQualifier( "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", "EnableSelfSelection", @"False", "B344F7FA-1528-43F8-A9F9-46F072F2DC38" ); // PTO Request:Supervisor:EnableSelfSelection
            RockMigrationHelper.UpdateAttributeQualifier( "CB12115A-8783-472C-B980-FE404D67F12E", "allowmultiple", @"False", "B890FD82-6B8B-45E8-88BA-D289FB080FD4" ); // PTO Request:Supervisor Attribute:allowmultiple
            RockMigrationHelper.UpdateAttributeQualifier( "CB12115A-8783-472C-B980-FE404D67F12E", "entitytype", @"72657ed8-d16e-492e-ac12-144c5e7567e7", "398BAABC-36D4-47C5-9027-385D6B1952DB" ); // PTO Request:Supervisor Attribute:entitytype
            RockMigrationHelper.UpdateAttributeQualifier( "CB12115A-8783-472C-B980-FE404D67F12E", "qualifierColumn", @"", "E1866C99-5032-4FB3-801E-8EBAEC929EDE" ); // PTO Request:Supervisor Attribute:qualifierColumn
            RockMigrationHelper.UpdateAttributeQualifier( "CB12115A-8783-472C-B980-FE404D67F12E", "qualifierValue", @"", "73597A01-B60E-4DB9-82DB-C0B2A4E394F1" ); // PTO Request:Supervisor Attribute:qualifierValue
            RockMigrationHelper.UpdateAttributeQualifier( "160380E7-EF1F-4D6A-82C3-712FABD0C263", "ispassword", @"False", "54B523BC-B104-4EC7-83E9-0EF90FF51F1A" ); // PTO Request:HasViewRights:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "160380E7-EF1F-4D6A-82C3-712FABD0C263", "maxcharacters", @"", "B08FB017-3B2A-4527-85F6-1FA3CC4E4FC5" ); // PTO Request:HasViewRights:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "160380E7-EF1F-4D6A-82C3-712FABD0C263", "showcountdown", @"False", "DD25F828-1D14-4736-BB83-7750C0AE3BE5" ); // PTO Request:HasViewRights:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "14E6B221-3531-4177-A1B1-8DD8B24B80AA", "ispassword", @"False", "6FCF86F8-6BA8-413D-95F6-3E1D2076BFC6" ); // PTO Request:HasReviewRights:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "14E6B221-3531-4177-A1B1-8DD8B24B80AA", "maxcharacters", @"", "CE1CD539-4FE0-404D-A552-C0E6B6AEBF4F" ); // PTO Request:HasReviewRights:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "14E6B221-3531-4177-A1B1-8DD8B24B80AA", "showcountdown", @"False", "9D9A32B9-3819-478A-AF5E-8BFEE8A1A67D" ); // PTO Request:HasReviewRights:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "4180D0D3-A144-4974-B364-34292969C1A9", "fieldtype", @"ddl", "B38109F1-8E46-4AA4-871A-A6678FAF3E87" ); // PTO Request:Hours / Day:fieldtype
            RockMigrationHelper.UpdateAttributeQualifier( "4180D0D3-A144-4974-B364-34292969C1A9", "repeatColumns", @"", "A77DE7CC-AC7F-4264-A082-74A539BE30D6" ); // PTO Request:Hours / Day:repeatColumns
            RockMigrationHelper.UpdateAttributeQualifier( "4180D0D3-A144-4974-B364-34292969C1A9", "values", @"0.5,1.0,1.5,2.0,2.5,3.0,3.5,4.0,4.5,5.0,5.5,6.0,6.5,7.0,7.5,8.0", "0DFD3FDD-7B42-496B-BE85-6A72A2BDA5CE" ); // PTO Request:Hours / Day:values
            RockMigrationHelper.UpdateAttributeQualifier( "1136E804-A793-4081-9902-F8E7ED0CDD69", "allowhtml", @"False", "3CF2C29E-2595-46A8-953E-3BDA18A076CF" ); // PTO Request:Reason:allowhtml
            RockMigrationHelper.UpdateAttributeQualifier( "1136E804-A793-4081-9902-F8E7ED0CDD69", "maxcharacters", @"", "93529A51-6540-4132-B608-71C6F677CE8B" ); // PTO Request:Reason:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "1136E804-A793-4081-9902-F8E7ED0CDD69", "numberofrows", @"", "941A5667-C960-4993-B8C3-662D52CA2FA8" ); // PTO Request:Reason:numberofrows
            RockMigrationHelper.UpdateAttributeQualifier( "1136E804-A793-4081-9902-F8E7ED0CDD69", "showcountdown", @"False", "B1D63D31-5605-4741-B8C5-22AD44937BDF" ); // PTO Request:Reason:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "35198559-8801-424C-B410-7145E00D3F67", "EnableSelfSelection", @"False", "53F2DD76-AF00-47C6-B103-8520DA51367D" ); // PTO Request:Approver:EnableSelfSelection
            RockMigrationHelper.UpdateAttributeQualifier( "5D083A44-57B8-46AF-AE1B-5FFA067DD187", "falsetext", @"No", "DD492B17-9325-4ED3-9861-9FC4B35AE32C" ); // PTO Request:Exclude Weekends:falsetext
            RockMigrationHelper.UpdateAttributeQualifier( "5D083A44-57B8-46AF-AE1B-5FFA067DD187", "truetext", @"Yes", "2E996169-5D81-4B0F-A7F4-C91F025BB5BC" ); // PTO Request:Exclude Weekends:truetext
            RockMigrationHelper.UpdateAttributeQualifier( "65B0E759-671A-421C-B2FC-E9885BA4D38D", "fieldtype", @"ddl", "5B01CF2D-73A4-49B6-B523-0A90C67394F0" ); // PTO Request:Cancel Request:fieldtype
            RockMigrationHelper.UpdateAttributeQualifier( "65B0E759-671A-421C-B2FC-E9885BA4D38D", "repeatColumns", @"", "C5646D95-77D4-4D0E-9B2F-635FCEB44602" ); // PTO Request:Cancel Request:repeatColumns
            RockMigrationHelper.UpdateAttributeQualifier( "65B0E759-671A-421C-B2FC-E9885BA4D38D", "values", @"Yes,No", "C2B8EC4F-6D6C-46D9-9681-D473F98051F3" ); // PTO Request:Cancel Request:values
            RockMigrationHelper.UpdateAttributeQualifier( "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", "ispassword", @"False", "A47EF571-1E85-4FAF-889A-4A13653A5CB6" ); // PTO Request:Time Frame Validation Error:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", "maxcharacters", @"", "B18916CD-5F6A-4C6E-A4BE-095C4E75D5CF" ); // PTO Request:Time Frame Validation Error:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", "showcountdown", @"False", "91951331-8C58-404E-8257-E6948EF42598" ); // PTO Request:Time Frame Validation Error:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "A8101AB8-B260-407D-9C8D-EF4063C73922", "ispassword", @"False", "5F8C211E-E47B-4F8B-976C-6CEC8C240058" ); // PTO Request:Requested Hours YTD:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "A8101AB8-B260-407D-9C8D-EF4063C73922", "maxcharacters", @"", "EEEC5D52-EB87-4B63-B5EE-4B3C38063CB4" ); // PTO Request:Requested Hours YTD:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "A8101AB8-B260-407D-9C8D-EF4063C73922", "showcountdown", @"False", "F5FF7BAF-8889-42AB-9730-D0628DE7B66C" ); // PTO Request:Requested Hours YTD:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", "ispassword", @"False", "88DAF56F-E3DF-4FB4-8029-29EFE18E8FDC" ); // PTO Request:Reason for Cancellation:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", "maxcharacters", @"", "7DD0F3AE-8F8F-4ACD-BA88-E168C06155B7" ); // PTO Request:Reason for Cancellation:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", "showcountdown", @"False", "950C7735-CDFA-43BC-BD8A-C0E7F4B5941A" ); // PTO Request:Reason for Cancellation:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", true, "Start", "", true, 0, "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9" ); // PTO Request:Start
            RockMigrationHelper.UpdateWorkflowActivityType( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", true, "Add / Modify Request", "", false, 1, "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08" ); // PTO Request:Add / Modify Request
            RockMigrationHelper.UpdateWorkflowActivityType( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", true, "Review Request", "", false, 2, "F8AFF1EA-7578-4D62-9445-2F98F40C99F3" ); // PTO Request:Review Request
            RockMigrationHelper.UpdateWorkflowActivityType( "EBF1D986-8BBD-4888-8A7E-43AF5914751C", true, "Cancel Request", "", false, 3, "CEC47883-FCD3-47D3-87B7-E1A826184463" ); // PTO Request:Cancel Request
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selection", "Selection", "", 0, @"", "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587" ); // PTO Request:Add / Modify Request:Selection
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "CEC47883-FCD3-47D3-87B7-E1A826184463", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Selection", "Selection", "", 0, @"", "7B2496AD-98C3-4FB0-979B-6985DB23EEC8" ); // PTO Request:Cancel Request:Selection
            RockMigrationHelper.UpdateAttributeQualifier( "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", "ispassword", @"False", "775A8DB3-BA1F-4AA4-BA96-742BB8E52E91" ); // PTO Request:Selection:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", "maxcharacters", @"", "1AE7EC51-B999-4931-82A5-55D7A98B4767" ); // PTO Request:Selection:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", "showcountdown", @"False", "AF95D6AF-7A06-44CF-99B4-180C28ABC5D6" ); // PTO Request:Selection:showcountdown
            RockMigrationHelper.UpdateAttributeQualifier( "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", "ispassword", @"False", "BED27056-C739-42B4-B86B-4CA46C96EAB3" ); // PTO Request:Selection:ispassword
            RockMigrationHelper.UpdateAttributeQualifier( "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", "maxcharacters", @"", "284C3E69-CA60-49FA-99E5-31B7ECC78553" ); // PTO Request:Selection:maxcharacters
            RockMigrationHelper.UpdateAttributeQualifier( "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", "showcountdown", @"False", "4AA2D669-F103-4B58-97EB-1B04327D0E28" ); // PTO Request:Selection:showcountdown
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Modify Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08^Your information has been submitted successfully.|Cancel Request^638beee0-2f8f-4706-b9a4-5bab70386697^CEC47883-FCD3-47D3-87B7-E1A826184463^|", "", true, "", "74C1F3A4-6198-43A4-B860-BCDFD8F8656A" ); // PTO Request:Start:Initial Form - User
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Review Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^F8AFF1EA-7578-4D62-9445-2F98F40C99F3^Your information has been submitted successfully.|Cancel Request^638beee0-2f8f-4706-b9a4-5bab70386697^CEC47883-FCD3-47D3-87B7-E1A826184463^|", "", true, "", "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7" ); // PTO Request:Start:Initial Form - Reviewer
            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow | Attribute:'TimeFrameValidationError' }}
{% assign remainingHours = Workflow | Attribute:'RemainingHours' | AsDecimal %}
{% if remainingHours < 0 %}
<div class=""alert alert-danger"">PTO Requests cannot exceed allocated hours.  Please modify your request.</div>
{% endif %}", @"", "Submit Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^^|", "", true, "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", "4C57D323-945F-4FA1-86DB-C0B5C459268C" ); // PTO Request:Add / Modify Request:Add Request Form
            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow | Attribute:'TimeFrameValidationError' }}
{% assign remainingHours = Workflow | Attribute:'RemainingHours' | AsDecimal %}
{% if remainingHours < 0 %}
<div class=""alert alert-danger"">PTO Requests cannot exceed allocated hours.  Please modify your request.</div>
{% endif %}", @"", "Submit Changes^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^Your information has been submitted successfully.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^^|", "", true, "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", "DB797225-7327-47F7-91C5-123AC720256C" ); // PTO Request:Add / Modify Request:Modify Request Form
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^^^Your information has been submitted successfully.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1" ); // PTO Request:Review Request:Form
            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Cancel Request & Notify Supervisor^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^|Cancel Request^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^^|", "", true, "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", "6BE418EB-4761-40C6-AC31-B6EFF5D83871" ); // PTO Request:Cancel Request:Cancel Form
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, true, true, false, false, @"", @"", "1617313E-4367-473B-9567-A0A992BCD483" ); // PTO Request:Start:Initial Form - User:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 7, true, true, false, false, @"", @"", "E9CF6608-8E0E-476B-B989-E10FB1BBD545" ); // PTO Request:Start:Initial Form - User:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "286C5A76-9113-49C4-A209-078E856BD0B2", 8, false, true, false, false, @"", @"", "8589A2EE-2FF6-4E78-ADDA-AA0C89E49143" ); // PTO Request:Start:Initial Form - User:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 1, true, true, false, false, @"", @"", "9A2E0FC1-D907-49E1-84C1-6D065101341D" ); // PTO Request:Start:Initial Form - User:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 2, true, true, false, false, @"", @"", "00C7AAA9-6025-436A-AE3F-CCDCBF5B2BB2" ); // PTO Request:Start:Initial Form - User:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 3, false, true, false, false, @"", @"", "F8DFC113-2129-4C93-A690-2D1165D1B8D3" ); // PTO Request:Start:Initial Form - User:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 4, false, true, false, false, @"", @"", "21CD2385-3657-4B3A-ACFB-C700C3E6C441" ); // PTO Request:Start:Initial Form - User:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "CB12115A-8783-472C-B980-FE404D67F12E", 5, false, true, false, false, @"", @"", "557FC75E-7246-48DB-B85F-C1BBFFC42229" ); // PTO Request:Start:Initial Form - User:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 11, false, true, false, false, @"", @"", "9A684554-FD9B-4D01-B9DC-1B52B3780674" ); // PTO Request:Start:Initial Form - User:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 6, false, true, false, false, @"", @"", "968C51F5-DEA3-4232-AA32-75AEB32A4FC5" ); // PTO Request:Start:Initial Form - User:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "4180D0D3-A144-4974-B364-34292969C1A9", 9, true, true, false, false, @"", @"", "2C06ECF7-F252-4A23-94F3-F3E22BE734D0" ); // PTO Request:Start:Initial Form - User:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "1136E804-A793-4081-9902-F8E7ED0CDD69", 10, true, true, false, false, @"", @"", "714A9CCA-D968-4A66-990C-E5B3C189E3EA" ); // PTO Request:Start:Initial Form - User:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "35198559-8801-424C-B410-7145E00D3F67", 12, false, true, false, false, @"", @"", "9A28C1E6-7877-4AD6-A381-2BF24B3C654A" ); // PTO Request:Start:Initial Form - User:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 13, false, true, false, false, @"", @"", "2A4BA2D9-81AB-4594-A6E4-7C257B5B8947" ); // PTO Request:Start:Initial Form - User:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 14, false, true, false, false, @"", @"", "440F3022-30BF-4BFF-8C66-ABE75CF478B4" ); // PTO Request:Start:Initial Form - User:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 15, false, true, false, false, @"", @"", "76FB3970-86FF-4F71-9F32-A6AB0F880E60" ); // PTO Request:Start:Initial Form - User:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "3455E8C7-450D-4469-B0C5-A4FB806E8603" ); // PTO Request:Start:Initial Form - User:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "138E6CE5-044D-4DC3-9BDE-64862C0EEA3F" ); // PTO Request:Start:Initial Form - User:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "33A6DA12-A223-48AB-B599-D78FFD4E972C" ); // PTO Request:Start:Initial Form - User:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "A744895F-7FA9-4A3C-921E-1C7B83F81CDA" ); // PTO Request:Start:Initial Form - User:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 20, false, true, false, false, @"", @"", "7420E7C9-78B3-4FF2-AE44-B336135FAC49" ); // PTO Request:Start:Initial Form - User:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, true, true, false, false, @"", @"", "14CEB0F8-B488-4670-8825-F86F0E5EAB42" ); // PTO Request:Start:Initial Form - Reviewer:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 4, true, true, false, false, @"", @"", "A6379B7B-1DF5-45C5-AA65-3336D9BC641E" ); // PTO Request:Start:Initial Form - Reviewer:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "286C5A76-9113-49C4-A209-078E856BD0B2", 2, false, true, false, false, @"", @"", "A8A32F53-F38A-4D26-A0AB-37D5DBE81214" ); // PTO Request:Start:Initial Form - Reviewer:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 1, true, true, false, false, @"", @"", "6CB51C4D-C3B5-4278-BC41-7F8118009F98" ); // PTO Request:Start:Initial Form - Reviewer:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 3, true, true, false, false, @"", @"", "8D36F857-884E-4A0E-9A25-0CF48DA1250A" ); // PTO Request:Start:Initial Form - Reviewer:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 5, false, true, false, false, @"", @"", "E1A99D81-FEFF-4C2B-AB71-637F4701DAB7" ); // PTO Request:Start:Initial Form - Reviewer:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 6, false, true, false, false, @"", @"", "2CC47489-F5AB-4DCF-AEC7-14F12DE4079A" ); // PTO Request:Start:Initial Form - Reviewer:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "CB12115A-8783-472C-B980-FE404D67F12E", 7, false, true, false, false, @"", @"", "ED159120-EFDC-48E4-A217-4D62A98A88AB" ); // PTO Request:Start:Initial Form - Reviewer:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 8, false, true, false, false, @"", @"", "9035EA86-0FBF-4588-997F-A03D9C161A06" ); // PTO Request:Start:Initial Form - Reviewer:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 9, false, true, false, false, @"", @"", "054BFA95-252D-41CC-B9BC-8BA8E8AAC924" ); // PTO Request:Start:Initial Form - Reviewer:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "4180D0D3-A144-4974-B364-34292969C1A9", 10, true, true, false, false, @"", @"", "9BAD33C1-7D79-40D5-916E-C9AB9B414B2A" ); // PTO Request:Start:Initial Form - Reviewer:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "1136E804-A793-4081-9902-F8E7ED0CDD69", 11, true, true, false, false, @"", @"", "04EB6453-F7F3-4A9E-B074-226AB7D2D060" ); // PTO Request:Start:Initial Form - Reviewer:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "35198559-8801-424C-B410-7145E00D3F67", 12, false, true, false, false, @"", @"", "D2EA145F-FEDF-45EE-89BC-5BFD8074E742" ); // PTO Request:Start:Initial Form - Reviewer:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 13, false, true, false, false, @"", @"", "4A2F62CF-003F-41DD-B9BD-249AFE4AF314" ); // PTO Request:Start:Initial Form - Reviewer:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 14, false, true, false, false, @"", @"", "B42C7B71-F7DB-47DE-AB18-84F987BAF8DF" ); // PTO Request:Start:Initial Form - Reviewer:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 15, false, true, false, false, @"", @"", "63ED71CA-F8BC-43CD-867B-654058AE419C" ); // PTO Request:Start:Initial Form - Reviewer:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "C5B9D0F0-9F10-45B8-94EA-226C9FA45054" ); // PTO Request:Start:Initial Form - Reviewer:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "C56601EE-60AA-48D3-A5FE-0F5255E4E061" ); // PTO Request:Start:Initial Form - Reviewer:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "11A0E019-080F-4D58-BB07-EF5FC3DFCEFD" ); // PTO Request:Start:Initial Form - Reviewer:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "9410E929-B515-4704-A6A7-D1E71E320193" ); // PTO Request:Start:Initial Form - Reviewer:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 20, false, true, false, false, @"", @"", "46BC8747-AD68-400D-9656-B936DE44C88A" ); // PTO Request:Start:Initial Form - Reviewer:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, true, true, false, false, @"", @"", "1DD3103F-7465-4970-9FF1-2B913534CD01" ); // PTO Request:Add / Modify Request:Add Request Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", 20, false, true, false, false, @"", @"", "80FF4AF1-DF02-4533-9B09-F1CAB45C9AA8" ); // PTO Request:Add / Modify Request:Add Request Form:Selection
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 7, true, false, true, false, @"", @"", "E0204296-3FC7-47E1-BF49-E043A9EADE40" ); // PTO Request:Add / Modify Request:Add Request Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "286C5A76-9113-49C4-A209-078E856BD0B2", 8, true, false, false, false, @"", @"", "CB83027E-D256-4801-B9A3-EC4AA68FF93A" ); // PTO Request:Add / Modify Request:Add Request Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 5, true, false, true, false, @"", @"", "389B00A8-F7A2-4C74-A375-F181164BAF7E" ); // PTO Request:Add / Modify Request:Add Request Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 6, true, true, false, false, @"", @"", "8B3F097B-6BA1-4953-94C9-8350FAD78C4B" ); // PTO Request:Add / Modify Request:Add Request Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 1, false, true, false, false, @"", @"", "D85DAE3A-908B-444D-A746-81C5CCC9747E" ); // PTO Request:Add / Modify Request:Add Request Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 2, false, true, false, false, @"", @"", "CD9073A5-EE57-4315-A5E7-D24AC02681E9" ); // PTO Request:Add / Modify Request:Add Request Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "CB12115A-8783-472C-B980-FE404D67F12E", 3, false, true, false, false, @"", @"", "12DD8CA9-05A9-4293-8D5D-34CC6E18B575" ); // PTO Request:Add / Modify Request:Add Request Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 12, false, true, false, false, @"", @"", "A2A60D27-CCEC-4EAD-A6BC-520F1D3E9753" ); // PTO Request:Add / Modify Request:Add Request Form:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 4, false, true, false, false, @"", @"", "3E70A524-35FF-47EF-9441-D25A56EEC0F6" ); // PTO Request:Add / Modify Request:Add Request Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "4180D0D3-A144-4974-B364-34292969C1A9", 9, true, false, true, false, @"", @"", "8C5D4C1C-C2D0-402C-A3F9-9AEF107AFB87" ); // PTO Request:Add / Modify Request:Add Request Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "1136E804-A793-4081-9902-F8E7ED0CDD69", 11, true, false, true, false, @"", @"", "F9086841-D9A4-4B85-8AD3-1A8D136FCCDA" ); // PTO Request:Add / Modify Request:Add Request Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "35198559-8801-424C-B410-7145E00D3F67", 13, false, true, false, false, @"", @"", "3252E791-80B8-40B1-BAA2-80772A6034AE" ); // PTO Request:Add / Modify Request:Add Request Form:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 14, false, true, false, false, @"", @"", "E4674BEA-2E3E-4AED-A7CB-FD64326A6745" ); // PTO Request:Add / Modify Request:Add Request Form:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 15, false, true, false, false, @"", @"", "AF066075-A857-47EF-ACF7-EECDCE007FF2" ); // PTO Request:Add / Modify Request:Add Request Form:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 10, true, false, false, false, @"", @"", "97AA8FAD-0E2F-4A8B-8D83-588DB43F4D5D" ); // PTO Request:Add / Modify Request:Add Request Form:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "0FC5CF68-4BED-4CB4-BD34-08E312E15567" ); // PTO Request:Add / Modify Request:Add Request Form:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "DA50344B-5957-4C18-A5AF-1907CA4D106A" ); // PTO Request:Add / Modify Request:Add Request Form:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "73CCE029-302D-4467-8463-64290F41DAF3" ); // PTO Request:Add / Modify Request:Add Request Form:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "8EBD7ED0-8030-4BA3-8967-A40D0A3A2B68" ); // PTO Request:Add / Modify Request:Add Request Form:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "4C57D323-945F-4FA1-86DB-C0B5C459268C", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 21, false, true, false, false, @"", @"", "6638E414-94F1-4651-AAA0-E763B1105AC8" ); // PTO Request:Add / Modify Request:Add Request Form:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, true, true, false, false, @"", @"", "E659CC7D-4F7D-44AA-9F92-72EE9347480A" ); // PTO Request:Add / Modify Request:Modify Request Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", 20, false, true, false, false, @"", @"", "C21692E0-2508-4256-885E-DA43D6D78B07" ); // PTO Request:Add / Modify Request:Modify Request Form:Selection
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 3, true, false, true, false, @"", @"", "DEAFC992-EBBD-48D8-A028-FF5814C761D0" ); // PTO Request:Add / Modify Request:Modify Request Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "286C5A76-9113-49C4-A209-078E856BD0B2", 4, true, false, false, false, @"", @"", "710FE644-9E58-4E87-9570-9E6577D4FF49" ); // PTO Request:Add / Modify Request:Modify Request Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 1, true, false, false, false, @"", @"", "CE07A790-505A-4CD6-97F1-F527D1405498" ); // PTO Request:Add / Modify Request:Modify Request Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 2, true, true, false, false, @"", @"", "E2E4DCFD-5445-4C6C-9BA2-0A87C10241F5" ); // PTO Request:Add / Modify Request:Modify Request Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 5, false, true, false, false, @"", @"", "F4C948DA-8CD1-4F2D-AAC8-42338C86228A" ); // PTO Request:Add / Modify Request:Modify Request Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 6, false, true, false, false, @"", @"", "AD8E8139-C905-4267-ACFA-7361D9C91D5D" ); // PTO Request:Add / Modify Request:Modify Request Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "CB12115A-8783-472C-B980-FE404D67F12E", 7, false, true, false, false, @"", @"", "CEC8CC6F-3E07-48A6-8AFB-51218C03957E" ); // PTO Request:Add / Modify Request:Modify Request Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 8, false, true, false, false, @"", @"", "2BE0C887-AC0F-4E01-8B37-62AF9760E538" ); // PTO Request:Add / Modify Request:Modify Request Form:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 9, false, true, false, false, @"", @"", "15ADF872-5DDD-4781-AEDF-CC3EA835EE6C" ); // PTO Request:Add / Modify Request:Modify Request Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "4180D0D3-A144-4974-B364-34292969C1A9", 10, true, false, true, false, @"", @"", "7CBBE2D1-4B45-49A8-A3D4-7E41945886AE" ); // PTO Request:Add / Modify Request:Modify Request Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "1136E804-A793-4081-9902-F8E7ED0CDD69", 12, true, false, true, false, @"", @"", "C54F98F1-329C-4007-A51E-48DDCA5AC346" ); // PTO Request:Add / Modify Request:Modify Request Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "35198559-8801-424C-B410-7145E00D3F67", 13, false, true, false, false, @"", @"", "2B721D1E-4F42-423D-B52E-3595FC010043" ); // PTO Request:Add / Modify Request:Modify Request Form:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 14, false, true, false, false, @"", @"", "711F3476-67A0-4EB8-B668-A66CB5A3F67C" ); // PTO Request:Add / Modify Request:Modify Request Form:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 15, false, true, false, false, @"", @"", "DEB47B13-7092-4505-A043-66BD955C348D" ); // PTO Request:Add / Modify Request:Modify Request Form:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 11, true, false, false, false, @"", @"", "3E339374-8F64-4C87-8498-C80B26D939A5" ); // PTO Request:Add / Modify Request:Modify Request Form:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "ED6BC0E0-1FBE-4EE4-8DDA-A3EF83937EF6" ); // PTO Request:Add / Modify Request:Modify Request Form:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "430EC24B-6AF9-4D5C-A8DD-F1E62C7C78FF" ); // PTO Request:Add / Modify Request:Modify Request Form:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "63DD2A75-8844-4E2D-B250-F1ED40334DE0" ); // PTO Request:Add / Modify Request:Modify Request Form:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "9CCBC1F2-E2BE-4E5B-AE4F-96114E5322FE" ); // PTO Request:Add / Modify Request:Modify Request Form:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "DB797225-7327-47F7-91C5-123AC720256C", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 21, false, true, false, false, @"", @"", "E503AB96-448A-4E74-AD57-4D52FCB4F598" ); // PTO Request:Add / Modify Request:Modify Request Form:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, true, true, false, false, @"", @"", "639DE8BC-17DF-421B-BC9A-97CFF0632A55" ); // PTO Request:Review Request:Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 3, true, false, true, false, @"", @"", "87E0E969-37EF-4133-8CED-98708A5EDC2F" ); // PTO Request:Review Request:Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "286C5A76-9113-49C4-A209-078E856BD0B2", 4, true, false, false, false, @"", @"", "D7A03211-4011-431C-B271-343284FDEB6E" ); // PTO Request:Review Request:Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 1, true, false, false, false, @"", @"", "3AE915D1-72CC-4E4E-940D-1C31B095AF46" ); // PTO Request:Review Request:Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 2, true, false, false, false, @"", @"", "1D62A696-F1A3-4003-9052-82EB0DEDF0D0" ); // PTO Request:Review Request:Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 5, false, true, false, false, @"", @"", "39BB0B21-D9CD-475D-B4E4-9591027B4A4C" ); // PTO Request:Review Request:Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 6, false, true, false, false, @"", @"", "D48F6162-852A-431F-B20A-7827EE2202B4" ); // PTO Request:Review Request:Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "CB12115A-8783-472C-B980-FE404D67F12E", 7, false, true, false, false, @"", @"", "02266B98-00C9-476F-8237-F99CDF51358A" ); // PTO Request:Review Request:Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 8, false, true, false, false, @"", @"", "7D61D053-7C10-44DF-A1A8-9EFE4277DC3D" ); // PTO Request:Review Request:Form:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 9, false, true, false, false, @"", @"", "87EA194F-3E3F-401D-A506-CF45C2513094" ); // PTO Request:Review Request:Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "4180D0D3-A144-4974-B364-34292969C1A9", 10, true, false, true, false, @"", @"", "83D532E7-87F9-41DD-BB75-2769214F1607" ); // PTO Request:Review Request:Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "1136E804-A793-4081-9902-F8E7ED0CDD69", 11, true, false, true, false, @"", @"", "D9F3A0ED-E5ED-4B48-963B-121341F1CD93" ); // PTO Request:Review Request:Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "35198559-8801-424C-B410-7145E00D3F67", 12, false, true, false, false, @"", @"", "F6E26CBC-AF24-4D68-BFF8-55E7505D6ABA" ); // PTO Request:Review Request:Form:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 13, false, true, false, false, @"", @"", "3C74260B-2242-47E0-9BD5-A87583864184" ); // PTO Request:Review Request:Form:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 14, false, true, false, false, @"", @"", "5BCC71DD-F857-4894-A77C-E6A62B3368D3" ); // PTO Request:Review Request:Form:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 15, false, true, false, false, @"", @"", "7BDF5100-3BEF-4B78-959F-B8216F5A3C22" ); // PTO Request:Review Request:Form:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "B480E337-1A03-475F-BEBD-56B18C7E6D27" ); // PTO Request:Review Request:Form:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "D0F039CB-B7CD-4DCF-AEB4-BF5D09B4A128" ); // PTO Request:Review Request:Form:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "0047AB1C-9E76-4355-A4F3-08A3C88B0C8B" ); // PTO Request:Review Request:Form:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "26AF0A47-AFB4-47CE-8A8E-CE1018FE4133" ); // PTO Request:Review Request:Form:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 20, false, true, false, false, @"", @"", "0C4F0056-2469-453F-9FA5-648D481183DA" ); // PTO Request:Review Request:Form:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 0, false, true, false, false, @"", @"", "2999F51E-4240-4C99-9D2C-523707230B25" ); // PTO Request:Cancel Request:Cancel Form:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", 21, false, true, false, false, @"", @"", "D00E97B0-5616-4117-B8D5-0AEA9969C881" ); // PTO Request:Cancel Request:Cancel Form:Selection
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "F6FB28D8-AE58-445E-9373-46CDA04E6CC5", 1, false, true, false, false, @"", @"", "8FC4F958-DEA2-4C29-A896-8C18E70532DC" ); // PTO Request:Cancel Request:Cancel Form:Start Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "286C5A76-9113-49C4-A209-078E856BD0B2", 2, false, true, false, false, @"", @"", "7ECDA271-8D85-417B-A552-D9F7EC8ED52B" ); // PTO Request:Cancel Request:Cancel Form:End Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "C9E17918-4ECF-4029-A2F3-BF95D4EED4E3", 3, false, true, false, false, @"", @"", "7B6D91C4-7136-4A23-A0F1-34CB0A438C38" ); // PTO Request:Cancel Request:Cancel Form:PTO Allocation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "FFC543BE-7B65-425B-A56C-AD441986FA2C", 4, false, true, false, false, @"", @"", "35A53C27-8248-4676-BD59-DE2982820D90" ); // PTO Request:Cancel Request:Cancel Form:Approval State
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 5, false, true, false, false, @"", @"", "B2C302C1-E3A6-4D58-A120-2BD7943A0B31" ); // PTO Request:Cancel Request:Cancel Form:PTO Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 6, false, true, false, false, @"", @"", "06C43392-5970-40C5-ACFD-27020A5F2423" ); // PTO Request:Cancel Request:Cancel Form:Supervisor
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "CB12115A-8783-472C-B980-FE404D67F12E", 7, false, true, false, false, @"", @"", "C528D8F3-2213-4C25-8769-D783E6125AA4" ); // PTO Request:Cancel Request:Cancel Form:Supervisor Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 8, false, true, false, false, @"", @"", "FA0EE01C-0C72-4865-BB1A-5FE8D5502A90" ); // PTO Request:Cancel Request:Cancel Form:HasViewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 9, false, true, false, false, @"", @"", "DAAA3448-34CE-4077-8003-B0D3339932E9" ); // PTO Request:Cancel Request:Cancel Form:HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "4180D0D3-A144-4974-B364-34292969C1A9", 10, false, true, false, false, @"", @"", "ACE0449C-B8D8-4FFA-B18C-031D8643DE5A" ); // PTO Request:Cancel Request:Cancel Form:Hours / Day
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "1136E804-A793-4081-9902-F8E7ED0CDD69", 11, false, true, false, false, @"", @"", "FBD5DBC9-43E4-414A-A27E-8DBF2364FF05" ); // PTO Request:Cancel Request:Cancel Form:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "35198559-8801-424C-B410-7145E00D3F67", 12, false, true, false, false, @"", @"", "A8210068-01D4-46BD-B566-7C0BC0E07C3F" ); // PTO Request:Cancel Request:Cancel Form:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 13, false, true, false, false, @"", @"", "F82B187A-7D90-4027-A6D7-21C4716CBA19" ); // PTO Request:Cancel Request:Cancel Form:Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "52166C99-8A84-437E-8E01-A0282CD6E5BC", 14, false, true, false, false, @"", @"", "B62EA12E-88E7-4163-9291-357A6F5F4070" ); // PTO Request:Cancel Request:Cancel Form:Requested Hours
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "5D083A44-57B8-46AF-AE1B-5FFA067DD187", 15, false, true, false, false, @"", @"", "2740CB59-7480-4CB6-8E5A-68AEB2EDC547" ); // PTO Request:Cancel Request:Cancel Form:Exclude Weekends
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 16, false, true, false, false, @"", @"", "2CD83F6B-F138-4E2C-B6A5-43E1EA3D04A1" ); // PTO Request:Cancel Request:Cancel Form:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 17, false, true, false, false, @"", @"", "1F1B6A44-E566-4E44-AD2F-8FE3C98FA9C2" ); // PTO Request:Cancel Request:Cancel Form:Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "A28F65E4-D1C9-41B5-89CB-065F7809B298", 18, false, true, false, false, @"", @"", "59D4B795-2C6E-455B-B4BD-0992F8C5F425" ); // PTO Request:Cancel Request:Cancel Form:Human Resources
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "A8101AB8-B260-407D-9C8D-EF4063C73922", 19, false, true, false, false, @"", @"", "0F13D7DC-2864-42F3-BC14-D2D882F36472" ); // PTO Request:Cancel Request:Cancel Form:Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "FA2F1F80-D035-41BD-BADF-98ADBB72D7C2", 20, true, false, true, false, @"", @"", "94AB6A21-62C7-4E2E-9EF4-C51571AE5489" ); // PTO Request:Cancel Request:Cancel Form:Reason for Cancellation
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Person", 0, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 64, "", "066D899E-645D-40F8-BFD8-AFDFEEFCE183" ); // PTO Request:Start:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Person To Current Person If Blank", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "FF9B21ED-F47A-4961-A7DB-7CF8D90D96C3", 32, "", "56AA96EB-57C8-43E8-9BF3-3CF2534834D6" ); // PTO Request:Start:Set Person To Current Person If Blank
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Supervisor", 2, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "C56BECDD-24BB-40F4-908D-4ECE52F8FBBD" ); // PTO Request:Start:Set Supervisor
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set HasReviewRights", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "FBBB0090-31C2-44D2-8B9D-B68C75B2157C" ); // PTO Request:Start:Set HasReviewRights
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set HasViewRights", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "9DEAF62B-1C6C-494D-B6DC-ED84DD6A18B2" ); // PTO Request:Start:Set HasViewRights
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Error message if not Authorized", 5, "497746B4-8B0C-4D7B-9AF7-B42CEDD6C37C", true, true, "", "160380E7-EF1F-4D6A-82C3-712FABD0C263", 8, "false", "FFA96459-CDD8-4DF1-BE36-A05500F5DE93" ); // PTO Request:Start:Error message if not Authorized
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Activate Add Activity if no existing PTO Request", 6, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 32, "", "494C7681-CFFA-47A3-8A9D-C2779A8FF4D4" ); // PTO Request:Start:Activate Add Activity if no existing PTO Request
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Pto Allocation", 7, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "3DA9A3CA-23AB-429F-A363-C9FD8F298966" ); // PTO Request:Start:Set Pto Allocation
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Start Date", 8, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "941EE98E-3732-4037-A9C7-F3D95519E1EA" ); // PTO Request:Start:Set Start Date
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Reason", 9, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "7E6DA347-7333-4723-9DB6-F5E5B69F64B8" ); // PTO Request:Start:Set Reason
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Hours", 10, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 64, "", "790F40BC-0D26-42E6-B6EA-6761273653BC" ); // PTO Request:Start:Set Hours
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Set Approval State", 11, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "E2FCD026-7AC1-45F0-93BD-110920E2369C" ); // PTO Request:Start:Set Approval State
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Activate Cancel Request Activity", 12, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "65B0E759-671A-421C-B2FC-E9885BA4D38D", 1, "Yes", "91B7FD94-E1D7-4A65-A25F-FE795424D635" ); // PTO Request:Start:Activate Cancel Request Activity
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Initial Form - User", 13, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "74C1F3A4-6198-43A4-B860-BCDFD8F8656A", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 1, "false", "309F9CA5-6ADD-48F4-B615-5F367FE691FA" ); // PTO Request:Start:Initial Form - User
            RockMigrationHelper.UpdateWorkflowActionType( "BD2A46DC-C0CC-4145-9D7A-1D3B55E3AAA9", "Initial Form - Reviewer", 14, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "3F5CD9A8-105D-4F6C-8E91-0F319929C8A7", "14E6B221-3531-4177-A1B1-8DD8B24B80AA", 1, "true", "D1D1DC50-7186-458C-B105-55B9F8CEF26C" ); // PTO Request:Start:Initial Form - Reviewer
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Add Request Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "4C57D323-945F-4FA1-86DB-C0B5C459268C", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 32, "", "0B3A4061-593E-4403-9B6D-7BDC66C5AE84" ); // PTO Request:Add / Modify Request:Add Request Form
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Modify Request Form", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "DB797225-7327-47F7-91C5-123AC720256C", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 64, "", "6F4A6BBB-3C80-42A3-910F-0106CB5B3BFD" ); // PTO Request:Add / Modify Request:Modify Request Form
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Persist Workflow", 2, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "9404EB9E-0E26-43E8-ACEE-FDD62434FDDB" ); // PTO Request:Add / Modify Request:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Redirect", 3, "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", true, true, "", "8ECFAEAF-9381-4D3F-9A46-3EC7292AD587", 8, "Cancel", "0D0031DF-A108-4745-840C-3B1C2D5C25CE" ); // PTO Request:Add / Modify Request:Redirect
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Build Time Frame Validation Error", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "728BB3A2-A5E1-4CE7-A23A-8C500C7B2112" ); // PTO Request:Add / Modify Request:Build Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Show Time Frame Validation Error", 5, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "B1DA7119-6B5E-4A1A-A08B-B9E0BD9FABF8", 64, "", "52E2A6AA-7974-431E-BA54-229BD93D8DB0" ); // PTO Request:Add / Modify Request:Show Time Frame Validation Error
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Set Requested Hours YTD", 6, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "88F975B5-7EAD-4823-A9B5-7EF55AEF1068" ); // PTO Request:Add / Modify Request:Set Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Set Requested Hours", 7, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "6299C5B3-7233-4CD3-9FD6-91A1B286C5CC" ); // PTO Request:Add / Modify Request:Set Requested Hours
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Check Remaining Hours", 8, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "4556A8F7-7B85-4246-892D-BF7EA5CB4FB8" ); // PTO Request:Add / Modify Request:Check Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Show Overdraft Error", 9, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "1A8D6FA2-5AA0-4548-83EF-4F4D65670347", 512, "0", "3CD27A70-082A-4EC0-8BB2-42E82F202326" ); // PTO Request:Add / Modify Request:Show Overdraft Error
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Modify Request", 10, "546C6C01-5C8B-449E-A16A-580D92D0317B", true, false, "", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 64, "", "1B6922B4-16A3-4CE4-848C-4BA8672073F3" ); // PTO Request:Add / Modify Request:Modify Request
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Add Request", 11, "546C6C01-5C8B-449E-A16A-580D92D0317B", true, false, "", "9FE62EB5-6604-416F-899B-F836C1DEC7A5", 32, "", "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB" ); // PTO Request:Add / Modify Request:Add Request
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Send Email to Supervisor if one exists", 12, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 64, "", "E6B880BD-3903-41D5-8734-91DBDB91EE09" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Send Email to HR if no supervisor exists", 13, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "203AB4E9-065C-4D49-8DA5-9C7C61F49A01", 32, "", "C889FAA5-3FC6-4920-BC4D-8C70E17061F4" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Redirect", 14, "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", true, false, "", "", 1, "", "81BDBD72-79B6-4AEE-92C3-135472ED8C53" ); // PTO Request:Add / Modify Request:Redirect
            RockMigrationHelper.UpdateWorkflowActionType( "BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08", "Complete Workflow", 15, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "FC3DA720-67E3-443D-B0CF-7983FD102BB4" ); // PTO Request:Add / Modify Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "FC39F7C5-82C5-43C7-A0F0-0C4D1CA417A1", "", 1, "", "A5DB1A4C-0B69-4394-B7E3-0728C75C6C66" ); // PTO Request:Review Request:Form
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Persist Workflow", 1, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "C8056574-6838-4C42-AD9E-021F1ECEBA70" ); // PTO Request:Review Request:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Set Approver", 2, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "22BDBE2C-2670-49FA-AF2E-E0420FB24714" ); // PTO Request:Review Request:Set Approver
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Update Request", 3, "546C6C01-5C8B-449E-A16A-580D92D0317B", true, false, "", "", 1, "", "77664272-44CB-40F0-BED2-0A11ADB5C125" ); // PTO Request:Review Request:Update Request
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Redirect", 4, "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", true, false, "", "", 1, "", "F560F977-B36C-4429-A11D-8D7CF429745B" ); // PTO Request:Review Request:Redirect
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Set Requested Hours YTD", 5, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "5DBCAEEF-AA8C-43FC-BC4B-D65284956AE0" ); // PTO Request:Review Request:Set Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Set Requested Hours", 6, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "6D5A6A43-F2E5-40F9-ACEE-54F14C904787" ); // PTO Request:Review Request:Set Requested Hours
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Set Remaining Hours", 7, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "A25B9E13-E915-4F7E-B99F-1D114A3CDC87" ); // PTO Request:Review Request:Set Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Send Email to Person", 8, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "", 1, "", "B35D6DAB-82EE-4646-A96A-797124CDEA1A" ); // PTO Request:Review Request:Send Email to Person
            RockMigrationHelper.UpdateWorkflowActionType( "F8AFF1EA-7578-4D62-9445-2F98F40C99F3", "Complete Workflow", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "BA66A6A3-DF0F-4A53-A565-C52AAF7ED5B3" ); // PTO Request:Review Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Cancel Form", 0, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "6BE418EB-4761-40C6-AC31-B6EFF5D83871", "", 1, "", "A56DFEBF-5DB6-4A9F-B0D7-C8B7560770CD" ); // PTO Request:Cancel Request:Cancel Form
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Persist Workflow", 1, "F1A39347-6FE0-43D4-89FB-544195088ECF", true, false, "", "", 1, "", "6C9AFB68-FB82-4DB9-B28D-72992CEAEDD8" ); // PTO Request:Cancel Request:Persist Workflow
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Set Approval State to Cancelled", 2, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "5797F050-5A1E-4EBC-A800-D63215386BC5" ); // PTO Request:Cancel Request:Set Approval State to Cancelled
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Set Requested Hours YTD", 3, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "D5381E17-4D23-498B-98BB-8C7BCB5EFF88" ); // PTO Request:Cancel Request:Set Requested Hours YTD
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Set Requested Hours", 4, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "7CA58F02-2006-4F02-9566-17FA43FF4E42" ); // PTO Request:Cancel Request:Set Requested Hours
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Set Remaining Hours", 5, "BC21E57A-1477-44B3-A7C2-61A806118945", true, false, "", "", 1, "", "AC83A176-224C-435F-A030-667AC09D7322" ); // PTO Request:Cancel Request:Set Remaining Hours
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Send Email to Supervisor", 6, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", 8, "Notify", "8A500384-1F40-443E-9387-04AB9BB8BDE9" ); // PTO Request:Cancel Request:Send Email to Supervisor
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Cancel Request", 7, "546C6C01-5C8B-449E-A16A-580D92D0317B", true, false, "", "7B2496AD-98C3-4FB0-979B-6985DB23EEC8", 8, "Request", "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB" ); // PTO Request:Cancel Request:Cancel Request
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Redirect", 8, "F4FB0FB4-B2B3-4FC4-BEEA-E9B846A63293", true, false, "", "", 1, "", "077BFC61-E634-4827-8769-8D1ABDB0AD46" ); // PTO Request:Cancel Request:Redirect
            RockMigrationHelper.UpdateWorkflowActionType( "CEC47883-FCD3-47D3-87B7-E1A826184463", "Complete Workflow", 9, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "B277B0BC-474C-4934-80B3-01383BB2D95E" ); // PTO Request:Cancel Request:Complete Workflow
            RockMigrationHelper.AddActionTypeAttributeValue( "066D899E-645D-40F8-BFD8-AFDFEEFCE183", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.PtoAllocation.PersonAlias.Guid}}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Person:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "066D899E-645D-40F8-BFD8-AFDFEEFCE183", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "066D899E-645D-40F8-BFD8-AFDFEEFCE183", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"ff9b21ed-f47a-4961-a7db-7cf8d90d96c3" ); // PTO Request:Start:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "066D899E-645D-40F8-BFD8-AFDFEEFCE183", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Person:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "56AA96EB-57C8-43E8-9BF3-3CF2534834D6", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // PTO Request:Start:Set Person To Current Person If Blank:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "56AA96EB-57C8-43E8-9BF3-3CF2534834D6", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"ff9b21ed-f47a-4961-a7db-7cf8d90d96c3" ); // PTO Request:Start:Set Person To Current Person If Blank:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "C56BECDD-24BB-40F4-908D-4ECE52F8FBBD", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign person = Workflow | Attribute:'Person','Object' %}
{% assign supervisorAttribute = Workflow | Attribute:'SupervisorAttribute','Object' %}
{% assign supervisor = person | Attribute:supervisorAttribute.Key, 'Object' %}
{{{supervisor.PrimaryAlias.Guid}}" ); // PTO Request:Start:Set Supervisor:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "C56BECDD-24BB-40F4-908D-4ECE52F8FBBD", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Supervisor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C56BECDD-24BB-40F4-908D-4ECE52F8FBBD", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"203ab4e9-065c-4d49-8da5-9c7c61f49a01" ); // PTO Request:Start:Set Supervisor:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FBBB0090-31C2-44D2-8B9D-B68C75B2157C", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign viewer = CurrentPerson %}
{% assign canReview = false %}
{% group where:'Guid == ""6F8AABA3-5BC8-468B-90DD-F0686F38E373""' %}
    {% capture groupIdString %}{{group.Id}}{% endcapture %}
    {% assign groupMembers = viewer | Group:groupIdString %}
    {% for groupMember in groupMembers %}
        {% assign canReview = true %}
    {% endfor %}
{% endgroup %}

{% group where:'Guid == """"628C51A8-4613-43ED-A18D-4A6FB999273E""""' %}
    {% capture groupIdString %}{{group.Id}}{% endcapture %}
    {% assign groupMembers = viewer | Group:groupIdString %}
    {% for groupMember in groupMembers %}
        {% assign canReview = true %}
    {% endfor %}
{% endgroup %}

{% if canReview == false %}
    {% assign person = Workflow | Attribute:'Person','Object' %}
    {% assign supervisorAttribute = Workflow | Attribute:'SupervisorAttribute','Object' %}
    {% assign supervisor = person | Attribute:supervisorAttribute.Key, 'Object' %}
    {% if supervisor.Id == viewer.Id %}
        {% assign canReview = true %}
    {% endif %}
{% endif %}

{{canReview}}" ); // PTO Request:Start:Set HasReviewRights:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "FBBB0090-31C2-44D2-8B9D-B68C75B2157C", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set HasReviewRights:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FBBB0090-31C2-44D2-8B9D-B68C75B2157C", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"14e6b221-3531-4177-a1b1-8dd8b24b80aa" ); // PTO Request:Start:Set HasReviewRights:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "FBBB0090-31C2-44D2-8B9D-B68C75B2157C", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set HasReviewRights:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "9DEAF62B-1C6C-494D-B6DC-ED84DD6A18B2", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign viewer = CurrentPerson %}
{% assign canView = false %}
{% group where:'Guid == ""6F8AABA3-5BC8-468B-90DD-F0686F38E373""' %}
    {% capture groupIdString %}{{group.Id}}{% endcapture %}
    {% assign groupMembers = viewer | Group:groupIdString %}
    {% for groupMember in groupMembers %}
        {% assign canView = true %}
    {% endfor %}
{% endgroup %}

{% group where:'Guid == """"628C51A8-4613-43ED-A18D-4A6FB999273E""""' %}
    {% capture groupIdString %}{{group.Id}}{% endcapture %}
    {% assign groupMembers = viewer | Group:groupIdString %}
    {% for groupMember in groupMembers %}
        {% assign canView = true %}
    {% endfor %}
{% endgroup %}

{% if canView == false %}
    {% assign person = Workflow | Attribute:'Person','Object' %}
    {% assign supervisorAttribute = Workflow | Attribute:'SupervisorAttribute','Object' %}
    {% assign supervisor = person | Attribute:supervisorAttribute.Key, 'Object' %}
    {% if supervisor.Id == viewer.Id %}
        {% assign canView = true %}
    {% endif %}
{% endif %}

{% if canView == false %}
    {% assign person = Workflow | Attribute:'Person','Object' %}
    {% if person.Id == viewer.Id %}
        {% assign canView = true %}
    {% endif %}
{% endif %}

{{canView}}" ); // PTO Request:Start:Set HasViewRights:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "9DEAF62B-1C6C-494D-B6DC-ED84DD6A18B2", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set HasViewRights:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9DEAF62B-1C6C-494D-B6DC-ED84DD6A18B2", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"160380e7-ef1f-4d6a-82c3-712fabd0c263" ); // PTO Request:Start:Set HasViewRights:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "9DEAF62B-1C6C-494D-B6DC-ED84DD6A18B2", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set HasViewRights:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "FFA96459-CDD8-4DF1-BE36-A05500F5DE93", "640FBD13-FEEB-4313-B6AC-6E5CF6E005DF", @"<div class='alert alert-warning'>
    You are not authorized to view this PTO Request.
</div>" ); // PTO Request:Start:Error message if not Authorized:HTML
            RockMigrationHelper.AddActionTypeAttributeValue( "FFA96459-CDD8-4DF1-BE36-A05500F5DE93", "05673872-1E8D-42CD-9517-7CAFBC6976F9", @"False" ); // PTO Request:Start:Error message if not Authorized:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FFA96459-CDD8-4DF1-BE36-A05500F5DE93", "46ACD91A-9455-41D2-8849-C2305F364418", @"True" ); // PTO Request:Start:Error message if not Authorized:Hide Status Message
            RockMigrationHelper.AddActionTypeAttributeValue( "494C7681-CFFA-47A3-8A9D-C2779A8FF4D4", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // PTO Request:Start:Activate Add Activity if no existing PTO Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "494C7681-CFFA-47A3-8A9D-C2779A8FF4D4", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08" ); // PTO Request:Start:Activate Add Activity if no existing PTO Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "3DA9A3CA-23AB-429F-A363-C9FD8F298966", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.PtoAllocation.Guid}}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Pto Allocation:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "3DA9A3CA-23AB-429F-A363-C9FD8F298966", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Pto Allocation:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3DA9A3CA-23AB-429F-A363-C9FD8F298966", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3" ); // PTO Request:Start:Set Pto Allocation:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "3DA9A3CA-23AB-429F-A363-C9FD8F298966", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Pto Allocation:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "941EE98E-3732-4037-A9C7-F3D95519E1EA", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.RequestDate}}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Start Date:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "941EE98E-3732-4037-A9C7-F3D95519E1EA", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Start Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "941EE98E-3732-4037-A9C7-F3D95519E1EA", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"f6fb28d8-ae58-445e-9373-46cda04e6cc5" ); // PTO Request:Start:Set Start Date:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "941EE98E-3732-4037-A9C7-F3D95519E1EA", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Start Date:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "7E6DA347-7333-4723-9DB6-F5E5B69F64B8", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.Reason}}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Reason:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "7E6DA347-7333-4723-9DB6-F5E5B69F64B8", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Reason:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7E6DA347-7333-4723-9DB6-F5E5B69F64B8", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1136e804-a793-4081-9902-f8e7ed0cdd69" ); // PTO Request:Start:Set Reason:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "7E6DA347-7333-4723-9DB6-F5E5B69F64B8", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Reason:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "790F40BC-0D26-42E6-B6EA-6761273653BC", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {{ptoRequest.Hours| Format:'#0.0' }}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "790F40BC-0D26-42E6-B6EA-6761273653BC", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "790F40BC-0D26-42E6-B6EA-6761273653BC", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"4180d0d3-a144-4974-b364-34292969c1a9" ); // PTO Request:Start:Set Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "790F40BC-0D26-42E6-B6EA-6761273653BC", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "E2FCD026-7AC1-45F0-93BD-110920E2369C", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoRequestGuid = Workflow | Attribute:'PTORequest','RawValue' %}
{% ptorequest where:'Guid == ""{{ptoRequestGuid}}""'%}
    {% for ptoRequest in ptorequestItems %}
        {% case ptoRequest.PtoRequestApprovalState %}
        {% when 'Pending' %}
            0
        {% when 'Approved' %}
            1
        {% else %}
            2
        {% endcase %}
    {% endfor %}
{% endptorequest %}" ); // PTO Request:Start:Set Approval State:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "E2FCD026-7AC1-45F0-93BD-110920E2369C", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Start:Set Approval State:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E2FCD026-7AC1-45F0-93BD-110920E2369C", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"ffc543be-7b65-425b-a56c-ad441986fa2c" ); // PTO Request:Start:Set Approval State:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "E2FCD026-7AC1-45F0-93BD-110920E2369C", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Start:Set Approval State:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "91B7FD94-E1D7-4A65-A25F-FE795424D635", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // PTO Request:Start:Activate Cancel Request Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "91B7FD94-E1D7-4A65-A25F-FE795424D635", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"CEC47883-FCD3-47D3-87B7-E1A826184463" ); // PTO Request:Start:Activate Cancel Request Activity:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "309F9CA5-6ADD-48F4-B615-5F367FE691FA", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Start:Initial Form - User:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D1D1DC50-7186-458C-B105-55B9F8CEF26C", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Start:Initial Form - Reviewer:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0B3A4061-593E-4403-9B6D-7BDC66C5AE84", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Add / Modify Request:Add Request Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6F4A6BBB-3C80-42A3-910F-0106CB5B3BFD", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Add / Modify Request:Modify Request Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9404EB9E-0E26-43E8-ACEE-FDD62434FDDB", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // PTO Request:Add / Modify Request:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "9404EB9E-0E26-43E8-ACEE-FDD62434FDDB", "82744A46-0110-4728-BD3D-66C85C5FCB2F", @"True" ); // PTO Request:Add / Modify Request:Persist Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "0D0031DF-A108-4745-840C-3B1C2D5C25CE", "1DAA899B-634B-4DD5-A30A-69BAC235B383", @"False" ); // PTO Request:Add / Modify Request:Redirect:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "0D0031DF-A108-4745-840C-3B1C2D5C25CE", "051BD491-817F-45DD-BBAC-875BA79E3644", @"/Person/{{ Workflow | Attribute:'Person','Id' }}/HR" ); // PTO Request:Add / Modify Request:Redirect:Url|Url Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "0D0031DF-A108-4745-840C-3B1C2D5C25CE", "581736CE-76CF-46CE-A401-60A9E9EBCC1A", @"0" ); // PTO Request:Add / Modify Request:Redirect:Processing Options
            RockMigrationHelper.AddActionTypeAttributeValue( "728BB3A2-A5E1-4CE7-A23A-8C500C7B2112", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{%- assign allocation = Workflow | Attribute:'PTOAllocation','RawValue' -%}
{%- assign endDate = Workflow | Attribute:'EndDate' | AsDateTime -%}
{%- ptoallocation where:'Guid == ""{{ allocation }}""' -%}
{%- if endDate > ptoallocation.EndDate -%}
<div class=""alert alert-danger"">PTO Requests cannot span multiple allocations.  Please select an end date that is within your allocation's timeframe.</div>
{%- endif -%}
{%- endptoallocation -%}" ); // PTO Request:Add / Modify Request:Build Time Frame Validation Error:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "728BB3A2-A5E1-4CE7-A23A-8C500C7B2112", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Add / Modify Request:Build Time Frame Validation Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "728BB3A2-A5E1-4CE7-A23A-8C500C7B2112", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"b1da7119-6b5e-4a1a-a08b-b9e0bd9fabf8" ); // PTO Request:Add / Modify Request:Build Time Frame Validation Error:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "728BB3A2-A5E1-4CE7-A23A-8C500C7B2112", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Add / Modify Request:Build Time Frame Validation Error:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "52E2A6AA-7974-431E-BA54-229BD93D8DB0", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // PTO Request:Add / Modify Request:Show Time Frame Validation Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "52E2A6AA-7974-431E-BA54-229BD93D8DB0", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08" ); // PTO Request:Add / Modify Request:Show Time Frame Validation Error:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "88F975B5-7EAD-4823-A9B5-7EF55AEF1068", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ ptoAllocationGuid }}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
    {% endfor %}
{% endptoallocation %}
{{ takenHours }}" ); // PTO Request:Add / Modify Request:Set Requested Hours YTD:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "88F975B5-7EAD-4823-A9B5-7EF55AEF1068", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Add / Modify Request:Set Requested Hours YTD:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "88F975B5-7EAD-4823-A9B5-7EF55AEF1068", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"a8101ab8-b260-407d-9c8d-ef4063c73922" ); // PTO Request:Add / Modify Request:Set Requested Hours YTD:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "88F975B5-7EAD-4823-A9B5-7EF55AEF1068", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Add / Modify Request:Set Requested Hours YTD:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "6299C5B3-7233-4CD3-9FD6-91A1B286C5CC", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign startDate = Workflow | Attribute:'StartDate' %}
{% assign endDate = Workflow | Attribute:'EndDate' %}
{% assign hoursPerDay = Workflow | Attribute:'HoursDay' %}
{% assign excludeWeekends = Workflow | Attribute:'ExcludeWeekends' %}
{% assign requestDate = startDate %}
{% assign totalRequestHours = 0 %}

{% if endDate != empty %}
    {% assign dayCount = startDate | DateDiff:endDate, 'd' %}
    {% for i in (0...dayCount) %}
        {% assign requestDate = startDate | DateAdd:i,'d' %}
        {% assign dayCounts = true %}
        
        {% if excludeWeekends == 'True' %}
            {% assign dayOfWeek = requestDate | Date:'dddd' %}
            {% if dayOfWeek == 'Sunday' or dayOfWeek == 'Saturday' %}
                {% assign dayCounts = false %}
            {% endif %}
        {% endif %}
        
        {% if dayCounts %}
            {% assign totalRequestHours = totalRequestHours | Plus:hoursPerDay %}
        {% endif %}
    {% endfor %}
{% else %}
    {% assign totalRequestHours = hoursPerDay %}
{% endif %}

{{totalRequestHours }}" ); // PTO Request:Add / Modify Request:Set Requested Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "6299C5B3-7233-4CD3-9FD6-91A1B286C5CC", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Add / Modify Request:Set Requested Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6299C5B3-7233-4CD3-9FD6-91A1B286C5CC", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"52166c99-8a84-437e-8e01-a0282cd6e5bc" ); // PTO Request:Add / Modify Request:Set Requested Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4556A8F7-7B85-4246-892D-BF7EA5CB4FB8", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign requestHours = Workflow | Attribute:'RequestedHours' %}

{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
        {% assign remainingHours = totalHours | Minus:takenHours %}
    {% endfor %}
{% endptoallocation %}
{% assign remainingHours = remainingHours | Minus:requestHours %}

{{ remainingHours }}" ); // PTO Request:Add / Modify Request:Check Remaining Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "4556A8F7-7B85-4246-892D-BF7EA5CB4FB8", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Add / Modify Request:Check Remaining Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4556A8F7-7B85-4246-892D-BF7EA5CB4FB8", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1a8d6fa2-5aa0-4548-83ef-4f4d65670347" ); // PTO Request:Add / Modify Request:Check Remaining Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4556A8F7-7B85-4246-892D-BF7EA5CB4FB8", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Add / Modify Request:Check Remaining Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "3CD27A70-082A-4EC0-8BB2-42E82F202326", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // PTO Request:Add / Modify Request:Show Overdraft Error:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "3CD27A70-082A-4EC0-8BB2-42E82F202326", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"BFBA23D1-FA40-43FF-9FA8-7C9B4342ED08" ); // PTO Request:Add / Modify Request:Show Overdraft Error:Activity
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "DD610F3E-2E83-41AE-B63B-9B163B87F82E", @"False" ); // PTO Request:Add / Modify Request:Modify Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "C957F777-F0FE-4D05-BB22-10D7C7A5C437", @"9fe62eb5-6604-416f-899b-f836c1dec7a5" ); // PTO Request:Add / Modify Request:Modify Request:Existing Pto Request
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "EC01344E-61BF-4E22-88E3-36051BCAABE7", @"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3" ); // PTO Request:Add / Modify Request:Modify Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D", @"f6fb28d8-ae58-445e-9373-46cda04e6cc5" ); // PTO Request:Add / Modify Request:Modify Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "8304DE14-DA5C-41FD-BA30-026D91A492C7", @"286c5a76-9113-49c4-a209-078e856bd0b2" ); // PTO Request:Add / Modify Request:Modify Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "858BFCA2-E793-446E-B146-87D5FC6783A0", @"4180d0d3-a144-4974-b364-34292969c1a9" ); // PTO Request:Add / Modify Request:Modify Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "C6A51AEB-18CB-4591-BDF8-D4017CF38DCF", @"1136e804-a793-4081-9902-f8e7ed0cdd69" ); // PTO Request:Add / Modify Request:Modify Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "080025FD-9E80-4158-8D7F-FBF3ED12A2E1", @"0" ); // PTO Request:Add / Modify Request:Modify Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "1B6922B4-16A3-4CE4-848C-4BA8672073F3", "552610AA-C128-4A6F-AAB6-20ACC0C5F060", @"5d083a44-57b8-46af-ae1b-5ffa067dd187" ); // PTO Request:Add / Modify Request:Modify Request:Exclude Weekends|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "DD610F3E-2E83-41AE-B63B-9B163B87F82E", @"False" ); // PTO Request:Add / Modify Request:Add Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "EC01344E-61BF-4E22-88E3-36051BCAABE7", @"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3" ); // PTO Request:Add / Modify Request:Add Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D", @"f6fb28d8-ae58-445e-9373-46cda04e6cc5" ); // PTO Request:Add / Modify Request:Add Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "8304DE14-DA5C-41FD-BA30-026D91A492C7", @"286c5a76-9113-49c4-a209-078e856bd0b2" ); // PTO Request:Add / Modify Request:Add Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "858BFCA2-E793-446E-B146-87D5FC6783A0", @"4180d0d3-a144-4974-b364-34292969c1a9" ); // PTO Request:Add / Modify Request:Add Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "C6A51AEB-18CB-4591-BDF8-D4017CF38DCF", @"1136e804-a793-4081-9902-f8e7ed0cdd69" ); // PTO Request:Add / Modify Request:Add Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "080025FD-9E80-4158-8D7F-FBF3ED12A2E1", @"0" ); // PTO Request:Add / Modify Request:Add Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "CDF5DB63-354C-4123-AF0B-A4AA9C4AFCEB", "552610AA-C128-4A6F-AAB6-20ACC0C5F060", @"5d083a44-57b8-46af-ae1b-5ffa067dd187" ); // PTO Request:Add / Modify Request:Add Request:Exclude Weekends|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "0C4C13B8-7076-4872-925A-F950886B5E16", @"203ab4e9-065c-4d49-8da5-9c7c61f49a01" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{ Workflow | Attribute:'Type' }} Time Off Request for {{Workflow | Attribute:'Person'}} ({{Workflow | Attribute:'StartDate'}} - {{Workflow | Attribute:'EndDate'}})" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}
{% assign selection = Activity | Attribute:'Selection'%}

{{ 'Global' | Attribute:'EmailHeader' }}
{% if selection == 'Submit Changes' %}
    An updated {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% elseif selection == 'Submit Request' %}
    A new {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% else %}
    A {{ Workflow | Attribute:'Type' }} Time Off Request has been CANCELLED by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% endif %}

    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "0C4C13B8-7076-4872-925A-F950886B5E16", @"a28f65e4-d1c9-41b5-89cb-065f7809b298" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{ Workflow | Attribute:'Type' }} Time Off Request for {{Workflow | Attribute:'Person'}} ({{Workflow | Attribute:'StartDate'}} - {{Workflow | Attribute:'EndDate'}})" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}
{% assign selection = Activity | Attribute:'Selection' %}

{{ 'Global' | Attribute:'EmailHeader' }}

{% if selection == 'Submit Changes' %}
    An updated {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% elseif selection == 'Submit Request' %}
    A new {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% else %}
    A {{ Workflow | Attribute:'Type' }} Time Off Request has been CANCELLED by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% endif %}

    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "81BDBD72-79B6-4AEE-92C3-135472ED8C53", "1DAA899B-634B-4DD5-A30A-69BAC235B383", @"False" ); // PTO Request:Add / Modify Request:Redirect:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "81BDBD72-79B6-4AEE-92C3-135472ED8C53", "051BD491-817F-45DD-BBAC-875BA79E3644", @"/Person/{{ Workflow | Attribute:'Person','Id' }}/HR" ); // PTO Request:Add / Modify Request:Redirect:Url|Url Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "81BDBD72-79B6-4AEE-92C3-135472ED8C53", "581736CE-76CF-46CE-A401-60A9E9EBCC1A", @"0" ); // PTO Request:Add / Modify Request:Redirect:Processing Options
            RockMigrationHelper.AddActionTypeAttributeValue( "FC3DA720-67E3-443D-B0CF-7983FD102BB4", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Request:Add / Modify Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "FC3DA720-67E3-443D-B0CF-7983FD102BB4", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // PTO Request:Add / Modify Request:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A5DB1A4C-0B69-4394-B7E3-0728C75C6C66", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Review Request:Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C8056574-6838-4C42-AD9E-021F1ECEBA70", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // PTO Request:Review Request:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C8056574-6838-4C42-AD9E-021F1ECEBA70", "82744A46-0110-4728-BD3D-66C85C5FCB2F", @"True" ); // PTO Request:Review Request:Persist Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "22BDBE2C-2670-49FA-AF2E-E0420FB24714", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // PTO Request:Review Request:Set Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "22BDBE2C-2670-49FA-AF2E-E0420FB24714", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"35198559-8801-424c-b410-7145e00d3f67" ); // PTO Request:Review Request:Set Approver:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "DD610F3E-2E83-41AE-B63B-9B163B87F82E", @"False" ); // PTO Request:Review Request:Update Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "C957F777-F0FE-4D05-BB22-10D7C7A5C437", @"9fe62eb5-6604-416f-899b-f836c1dec7a5" ); // PTO Request:Review Request:Update Request:Existing Pto Request
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "EC01344E-61BF-4E22-88E3-36051BCAABE7", @"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3" ); // PTO Request:Review Request:Update Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D", @"f6fb28d8-ae58-445e-9373-46cda04e6cc5" ); // PTO Request:Review Request:Update Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "8304DE14-DA5C-41FD-BA30-026D91A492C7", @"286c5a76-9113-49c4-a209-078e856bd0b2" ); // PTO Request:Review Request:Update Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "858BFCA2-E793-446E-B146-87D5FC6783A0", @"4180d0d3-a144-4974-b364-34292969c1a9" ); // PTO Request:Review Request:Update Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "C6A51AEB-18CB-4591-BDF8-D4017CF38DCF", @"1136e804-a793-4081-9902-f8e7ed0cdd69" ); // PTO Request:Review Request:Update Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "A781A20B-4F21-47CA-9BCF-1654565DB5F6", @"35198559-8801-424c-b410-7145e00d3f67" ); // PTO Request:Review Request:Update Request:Approver|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "080025FD-9E80-4158-8D7F-FBF3ED12A2E1", @"ffc543be-7b65-425b-a56c-ad441986fa2c" ); // PTO Request:Review Request:Update Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "77664272-44CB-40F0-BED2-0A11ADB5C125", "552610AA-C128-4A6F-AAB6-20ACC0C5F060", @"False" ); // PTO Request:Review Request:Update Request:Exclude Weekends|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "F560F977-B36C-4429-A11D-8D7CF429745B", "1DAA899B-634B-4DD5-A30A-69BAC235B383", @"False" ); // PTO Request:Review Request:Redirect:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F560F977-B36C-4429-A11D-8D7CF429745B", "051BD491-817F-45DD-BBAC-875BA79E3644", @"/Person/{{ Workflow | Attribute:'Person','Id' }}/HR" ); // PTO Request:Review Request:Redirect:Url|Url Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F560F977-B36C-4429-A11D-8D7CF429745B", "581736CE-76CF-46CE-A401-60A9E9EBCC1A", @"0" ); // PTO Request:Review Request:Redirect:Processing Options
            RockMigrationHelper.AddActionTypeAttributeValue( "5DBCAEEF-AA8C-43FC-BC4B-D65284956AE0", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ ptoAllocationGuid }}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
    {% endfor %}
{% endptoallocation %}
{{ takenHours }}" ); // PTO Request:Review Request:Set Requested Hours YTD:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "5DBCAEEF-AA8C-43FC-BC4B-D65284956AE0", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Review Request:Set Requested Hours YTD:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5DBCAEEF-AA8C-43FC-BC4B-D65284956AE0", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"a8101ab8-b260-407d-9c8d-ef4063c73922" ); // PTO Request:Review Request:Set Requested Hours YTD:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5DBCAEEF-AA8C-43FC-BC4B-D65284956AE0", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Review Request:Set Requested Hours YTD:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "6D5A6A43-F2E5-40F9-ACEE-54F14C904787", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign startDate = Workflow | Attribute:'StartDate' %}
{% assign endDate = Workflow | Attribute:'EndDate' %}
{% assign hoursPerDay = Workflow | Attribute:'HoursDay' %}
{% assign excludeWeekends = Workflow | Attribute:'ExcludeWeekends' %}
{% assign requestDate = startDate %}
{% assign totalRequestHours = 0 %}

{% if endDate != empty %}
    {% assign dayCount = startDate | DateDiff:endDate, 'd' %}
    {% for i in (0...dayCount) %}
        {% assign requestDate = startDate | DateAdd:i,'d' %}
        {% assign dayCounts = true %}
        
        {% if excludeWeekends == 'True' %}
            {% assign dayOfWeek = requestDate | Date:'dddd' %}
            {% if dayOfWeek == 'Sunday' or dayOfWeek == 'Saturday' %}
                {% assign dayCounts = false %}
            {% endif %}
        {% endif %}
        
        {% if dayCounts %}
            {% assign totalRequestHours = totalRequestHours | Plus:hoursPerDay %}
        {% endif %}
    {% endfor %}
{% else %}
    {% assign totalRequestHours = hoursPerDay %}
{% endif %}

{{totalRequestHours }}" ); // PTO Request:Review Request:Set Requested Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "6D5A6A43-F2E5-40F9-ACEE-54F14C904787", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Review Request:Set Requested Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6D5A6A43-F2E5-40F9-ACEE-54F14C904787", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"52166c99-8a84-437e-8e01-a0282cd6e5bc" ); // PTO Request:Review Request:Set Requested Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A25B9E13-E915-4F7E-B99F-1D114A3CDC87", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign requestHours = Workflow | Attribute:'RequestedHours' %}

{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
        {% assign remainingHours = totalHours | Minus:takenHours %}
    {% endfor %}
{% endptoallocation %}
{% assign remainingHours = remainingHours | Minus:requestHours %}

{{ remainingHours }}" ); // PTO Request:Review Request:Set Remaining Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "A25B9E13-E915-4F7E-B99F-1D114A3CDC87", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Review Request:Set Remaining Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "A25B9E13-E915-4F7E-B99F-1D114A3CDC87", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1a8d6fa2-5aa0-4548-83ef-4f4d65670347" ); // PTO Request:Review Request:Set Remaining Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A25B9E13-E915-4F7E-B99F-1D114A3CDC87", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Review Request:Set Remaining Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "B35D6DAB-82EE-4646-A96A-797124CDEA1A", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // PTO Request:Review Request:Send Email to Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B35D6DAB-82EE-4646-A96A-797124CDEA1A", "0C4C13B8-7076-4872-925A-F950886B5E16", @"ff9b21ed-f47a-4961-a7db-7cf8d90d96c3" ); // PTO Request:Review Request:Send Email to Person:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "B35D6DAB-82EE-4646-A96A-797124CDEA1A", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{Workflow | Attribute:'ApprovalState'}}: {{ Workflow | Attribute:'Type' }} Time Off Request for {{Workflow | Attribute:'Person'}} ({{Workflow | Attribute:'StartDate'}} - {{Workflow | Attribute:'EndDate'}})" ); // PTO Request:Review Request:Send Email to Person:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "B35D6DAB-82EE-4646-A96A-797124CDEA1A", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}

{{ 'Global' | Attribute:'EmailHeader' }}

    Your {{ Workflow | Attribute:'Type' }} Time Off Request has been {{Workflow | Attribute:'ApprovalState'}} by {{ Workflow | Attribute:'Approver' }}. <br /> <br />

    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Review Request:Send Email to Person:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "B35D6DAB-82EE-4646-A96A-797124CDEA1A", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // PTO Request:Review Request:Send Email to Person:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "BA66A6A3-DF0F-4A53-A565-C52AAF7ED5B3", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Request:Review Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "BA66A6A3-DF0F-4A53-A565-C52AAF7ED5B3", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // PTO Request:Review Request:Complete Workflow:Status|Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "A56DFEBF-5DB6-4A9F-B0D7-C8B7560770CD", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // PTO Request:Cancel Request:Cancel Form:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6C9AFB68-FB82-4DB9-B28D-72992CEAEDD8", "50B01639-4938-40D2-A791-AA0EB4F86847", @"False" ); // PTO Request:Cancel Request:Persist Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6C9AFB68-FB82-4DB9-B28D-72992CEAEDD8", "82744A46-0110-4728-BD3D-66C85C5FCB2F", @"True" ); // PTO Request:Cancel Request:Persist Workflow:Persist Immediately
            RockMigrationHelper.AddActionTypeAttributeValue( "5797F050-5A1E-4EBC-A800-D63215386BC5", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // PTO Request:Cancel Request:Set Approval State to Cancelled:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "5797F050-5A1E-4EBC-A800-D63215386BC5", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"ffc543be-7b65-425b-a56c-ad441986fa2c" ); // PTO Request:Cancel Request:Set Approval State to Cancelled:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "5797F050-5A1E-4EBC-A800-D63215386BC5", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"3" ); // PTO Request:Cancel Request:Set Approval State to Cancelled:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "D5381E17-4D23-498B-98BB-8C7BCB5EFF88", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ ptoAllocationGuid }}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
    {% endfor %}
{% endptoallocation %}
{{ takenHours }}" ); // PTO Request:Cancel Request:Set Requested Hours YTD:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "D5381E17-4D23-498B-98BB-8C7BCB5EFF88", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Cancel Request:Set Requested Hours YTD:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D5381E17-4D23-498B-98BB-8C7BCB5EFF88", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"a8101ab8-b260-407d-9c8d-ef4063c73922" ); // PTO Request:Cancel Request:Set Requested Hours YTD:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D5381E17-4D23-498B-98BB-8C7BCB5EFF88", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Cancel Request:Set Requested Hours YTD:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "7CA58F02-2006-4F02-9566-17FA43FF4E42", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign startDate = Workflow | Attribute:'StartDate' %}
{% assign endDate = Workflow | Attribute:'EndDate' %}
{% assign hoursPerDay = Workflow | Attribute:'HoursDay' %}
{% assign excludeWeekends = Workflow | Attribute:'ExcludeWeekends' %}
{% assign requestDate = startDate %}
{% assign totalRequestHours = 0 %}

{% if endDate != empty %}
    {% assign dayCount = startDate | DateDiff:endDate, 'd' %}
    {% for i in (0...dayCount) %}
        {% assign requestDate = startDate | DateAdd:i,'d' %}
        {% assign dayCounts = true %}
        
        {% if excludeWeekends == 'True' %}
            {% assign dayOfWeek = requestDate | Date:'dddd' %}
            {% if dayOfWeek == 'Sunday' or dayOfWeek == 'Saturday' %}
                {% assign dayCounts = false %}
            {% endif %}
        {% endif %}
        
        {% if dayCounts %}
            {% assign totalRequestHours = totalRequestHours | Plus:hoursPerDay %}
        {% endif %}
    {% endfor %}
{% else %}
    {% assign totalRequestHours = hoursPerDay %}
{% endif %}

{{totalRequestHours }}" ); // PTO Request:Cancel Request:Set Requested Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "7CA58F02-2006-4F02-9566-17FA43FF4E42", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Cancel Request:Set Requested Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "7CA58F02-2006-4F02-9566-17FA43FF4E42", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"52166c99-8a84-437e-8e01-a0282cd6e5bc" ); // PTO Request:Cancel Request:Set Requested Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AC83A176-224C-435F-A030-667AC09D7322", "F1F6F9D6-FDC5-489C-8261-4B9F45B3EED4", @"{% assign requestHours = Workflow | Attribute:'RequestedHours' %}

{% assign ptoAllocationGuid = Workflow | Attribute:'PTOAllocation','RawValue' %}
{% if ptoAllocationGuid == empty %}
    {% assign ptoRequest = Workflow | Attribute:'PTORequest','Object' %}
    {% assign ptoAllocationGuid = ptoRequest.PtoAllocation.Guid %}
{% endif %}

{% ptoallocation where:'Guid == ""{{ptoAllocationGuid}}""' %}
    {% for ptoAllocation in ptoallocationItems %}
        {% assign totalHours = ptoAllocation.Hours %}
        {% assign takenHours = 0.0 %}
        {% for request in ptoAllocation.PtoRequests %}
            {% if request.PtoRequestApprovalState != 0 %}
                {% assign takenHours = takenHours | Plus:request.Hours %}
            {% endif %}
        {% endfor %}
        {% assign remainingHours = totalHours | Minus:takenHours %}
    {% endfor %}
{% endptoallocation %}
{% assign remainingHours = remainingHours | Minus:requestHours %}

{{ remainingHours }}" ); // PTO Request:Cancel Request:Set Remaining Hours:Lava
            RockMigrationHelper.AddActionTypeAttributeValue( "AC83A176-224C-435F-A030-667AC09D7322", "F1924BDC-9B79-4018-9D4A-C3516C87A514", @"False" ); // PTO Request:Cancel Request:Set Remaining Hours:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "AC83A176-224C-435F-A030-667AC09D7322", "431273C6-342D-4030-ADC7-7CDEDC7F8B27", @"1a8d6fa2-5aa0-4548-83ef-4f4d65670347" ); // PTO Request:Cancel Request:Set Remaining Hours:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "AC83A176-224C-435F-A030-667AC09D7322", "F3E380BF-AAC8-4015-9ADC-0DF56B5462F5", @"RockEntity" ); // PTO Request:Cancel Request:Set Remaining Hours:Enabled Lava Commands
            RockMigrationHelper.AddActionTypeAttributeValue( "8A500384-1F40-443E-9387-04AB9BB8BDE9", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // PTO Request:Cancel Request:Send Email to Supervisor:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8A500384-1F40-443E-9387-04AB9BB8BDE9", "0C4C13B8-7076-4872-925A-F950886B5E16", @"203ab4e9-065c-4d49-8da5-9c7c61f49a01" ); // PTO Request:Cancel Request:Send Email to Supervisor:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "8A500384-1F40-443E-9387-04AB9BB8BDE9", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"{{Workflow | Attribute:'ApprovalState'}}: {{ Workflow | Attribute:'Type' }} Time Off Request for {{Workflow | Attribute:'Person'}} ({{Workflow | Attribute:'StartDate'}} - {{Workflow | Attribute:'EndDate'}})" ); // PTO Request:Cancel Request:Send Email to Supervisor:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "8A500384-1F40-443E-9387-04AB9BB8BDE9", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}

    A {{ Workflow | Attribute:'Type' }} Time Off Request has been CANCELLED by {{ CurrentPerson.FullName }}. <br /> <br />


 <strong>Date Cancelled:</strong> {{ 'Now' | Date: 'dddd MMM dd, yyyy' }}<br />
 <strong>Reason for Cancellation:</strong> {{ Workflow | Attribute:'CancelReason' }}<br />
    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Cancel Request:Send Email to Supervisor:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "8A500384-1F40-443E-9387-04AB9BB8BDE9", "1BDC7ACA-9A0B-4C8A-909E-8B4143D9C2A3", @"True" ); // PTO Request:Cancel Request:Send Email to Supervisor:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "DD610F3E-2E83-41AE-B63B-9B163B87F82E", @"False" ); // PTO Request:Cancel Request:Cancel Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "C957F777-F0FE-4D05-BB22-10D7C7A5C437", @"9fe62eb5-6604-416f-899b-f836c1dec7a5" ); // PTO Request:Cancel Request:Cancel Request:Existing Pto Request
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "EC01344E-61BF-4E22-88E3-36051BCAABE7", @"c9e17918-4ecf-4029-a2f3-bf95d4eed4e3" ); // PTO Request:Cancel Request:Cancel Request:Allocation|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "3C5F03BD-2CDD-41D7-9ED1-5AAC62AF733D", @"f6fb28d8-ae58-445e-9373-46cda04e6cc5" ); // PTO Request:Cancel Request:Cancel Request:Start Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "8304DE14-DA5C-41FD-BA30-026D91A492C7", @"286c5a76-9113-49c4-a209-078e856bd0b2" ); // PTO Request:Cancel Request:Cancel Request:End Date|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "858BFCA2-E793-446E-B146-87D5FC6783A0", @"4180d0d3-a144-4974-b364-34292969c1a9" ); // PTO Request:Cancel Request:Cancel Request:Hours|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "C6A51AEB-18CB-4591-BDF8-D4017CF38DCF", @"1136e804-a793-4081-9902-f8e7ed0cdd69" ); // PTO Request:Cancel Request:Cancel Request:Reason|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "080025FD-9E80-4158-8D7F-FBF3ED12A2E1", @"ffc543be-7b65-425b-a56c-ad441986fa2c" ); // PTO Request:Cancel Request:Cancel Request:Approval State|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "ED092B4A-AB43-4836-A3C7-28CFF9F47DFB", "552610AA-C128-4A6F-AAB6-20ACC0C5F060", @"5d083a44-57b8-46af-ae1b-5ffa067dd187" ); // PTO Request:Cancel Request:Cancel Request:Exclude Weekends|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "077BFC61-E634-4827-8769-8D1ABDB0AD46", "1DAA899B-634B-4DD5-A30A-69BAC235B383", @"False" ); // PTO Request:Cancel Request:Redirect:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "077BFC61-E634-4827-8769-8D1ABDB0AD46", "051BD491-817F-45DD-BBAC-875BA79E3644", @"/Person/{{ Workflow | Attribute:'Person','Id' }}/HR" ); // PTO Request:Cancel Request:Redirect:Url|Url Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "077BFC61-E634-4827-8769-8D1ABDB0AD46", "581736CE-76CF-46CE-A401-60A9E9EBCC1A", @"0" ); // PTO Request:Cancel Request:Redirect:Processing Options
            RockMigrationHelper.AddActionTypeAttributeValue( "B277B0BC-474C-4934-80B3-01383BB2D95E", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // PTO Request:Cancel Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B277B0BC-474C-4934-80B3-01383BB2D95E", "385A255B-9F48-4625-862B-26231DBAC53A", @"Completed" ); // PTO Request:Cancel Request:Complete Workflow:Status|Status Attribute

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @"
			UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) )
			FROM [AttributeQualifier] [aq]
			INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId]
			INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId]
			INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value]
			WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType'
			AND [aq].[key] = 'definedtypeguid'
		" );

            #endregion
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
