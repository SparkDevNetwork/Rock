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
    public partial class AddCcAndBccToSendEmailAction : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_TEXT_OR_ATTRIBUTE, "From Email Address|From Attribute", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_FROM_EMAIL_ADDRESS );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_TEXT_OR_ATTRIBUTE, "Send To Email Addresses|To Attribute", "To", "The email addresses or an attribute that contains the person, email address, group or security role that the email should be sent to. <span class='tip tip-lava'></span>", 1, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_SEND_TO_EMAIL_ADDRESSES );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Addresses' is a group or security role.", 2, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_SEND_TO_GROUP_ROLE );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.TEXT, "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_SUBJECT );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.CODE_EDITOR, "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_BODY );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_TEXT_OR_ATTRIBUTE, "CC Email Addresses|CC Attribute", "CC", "The email addresses or an attribute that contains the person, email address, group or security role that the email should be CC'd (carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>", 5, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_CC_EMAIL_ADDRESSES );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_TEXT_OR_ATTRIBUTE, "BCC Email Addresses|BCC Attribute", "BCC", "The email addresses or an attribute that contains the person, email address, group or security role that the email should be BCC'd (blind carbon copied) to. Any address in this field will be copied on the email sent to every recipient. <span class='tip tip-lava'></span>", 6, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_BCC_EMAIL_ADDRESSES );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_ONE );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 8, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_TWO );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.WORKFLOW_ATTRIBUTE, "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 9, "", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_ATTACHMENT_THREE );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( SystemGuid.EntityType.SEND_EMAIL, SystemGuid.FieldType.BOOLEAN, "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile?", 10, "False", SystemGuid.Attribute.WORKFLOW_ACTION_SEND_EMAIL_SAVE_COMMUNICATION_HISTORY );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Nothing to do here.
        }
    }
}
