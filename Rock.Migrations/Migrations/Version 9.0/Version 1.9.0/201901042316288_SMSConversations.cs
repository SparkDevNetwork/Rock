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
    /// Adds the Entity and and data neccessary for the SMS Conversations block.
    /// </summary>
    public partial class SMSConversations : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateCommunicationResponseUp();
            AddCommunicationRecipientSentMessageUp();
            UpdateDefinedType_COMMUNICATION_SMS_FROM_Up();
            AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Up();
            PagesAndBlocksUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesAndBlocksDown();
            AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Down();
            UpdateDefinedType_COMMUNICATION_SMS_FROM_Down();
            AddCommunicationRecipientSentMessageDown();
            CreateCommunicationResponseDown();
        }

        private void CreateCommunicationResponseUp()
        {
            CreateTable(
                "dbo.CommunicationResponse",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MessageKey = c.String(nullable: false, maxLength: 1000),
                        FromPersonAliasId = c.Int(),
                        ToPersonAliasId = c.Int(),
                        IsRead = c.Boolean(nullable: false),
                        RelatedSmsFromDefinedValueId = c.Int(),
                        RelatedCommunicationId = c.Int(),
                        RelatedTransportEntityTypeId = c.Int(nullable: false),
                        RelatedMediumEntityTypeId = c.Int(nullable: false),
                        Response = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.FromPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Communication", t => t.RelatedCommunicationId)
                .ForeignKey("dbo.EntityType", t => t.RelatedTransportEntityTypeId)
                .ForeignKey("dbo.EntityType", t => t.RelatedMediumEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ToPersonAliasId)
                .Index(t => t.FromPersonAliasId)
                .Index(t => t.ToPersonAliasId)
                .Index(t => t.RelatedCommunicationId)
                .Index(t => t.RelatedTransportEntityTypeId)
                .Index(t => t.RelatedMediumEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }

        private void CreateCommunicationResponseDown()
        {
            DropForeignKey("dbo.CommunicationResponse", "ToPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationResponse", "RelatedMediumEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.CommunicationResponse", "RelatedTransportEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.CommunicationResponse", "RelatedCommunicationId", "dbo.Communication");
            DropForeignKey("dbo.CommunicationResponse", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationResponse", "FromPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationResponse", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.CommunicationResponse", new[] { "Guid" });
            DropIndex("dbo.CommunicationResponse", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationResponse", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationResponse", new[] { "RelatedMediumEntityTypeId" });
            DropIndex("dbo.CommunicationResponse", new[] { "RelatedTransportEntityTypeId" });
            DropIndex("dbo.CommunicationResponse", new[] { "RelatedCommunicationId" });
            DropIndex("dbo.CommunicationResponse", new[] { "ToPersonAliasId" });
            DropIndex("dbo.CommunicationResponse", new[] { "FromPersonAliasId" });
            DropTable("dbo.CommunicationResponse");
        }

        private void AddCommunicationRecipientSentMessageUp()
        {
            AddColumn("dbo.CommunicationRecipient", "SentMessage", c => c.String());
        }

        private void AddCommunicationRecipientSentMessageDown()
        {
            DropColumn("dbo.CommunicationRecipient", "SentMessage");
        }

        /// <summary>
        /// Updates the defined type communication SMS from up.
        /// Updates properties of known defined value with guid 611BDE1F-7405-4D16-8626-CCFEDB0E62BE.
        /// </summary>
        private void UpdateDefinedType_COMMUNICATION_SMS_FROM_Up()
        {
            RockMigrationHelper.AddDefinedType( 
                "Communication", 
                "SMS Phone Numbers",
                "SMS numbers that can be used to send text messages from. This is usually a phone number or short code that has been set up with your SMS provider. Providing a response recipient will send replies straight to the individual's mobile phone unless 'Enable Mobile Conversations' is set to No. Leaving this field blank will enable responses to be processed from within Rock.",
                Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM );
        }

        private void UpdateDefinedType_COMMUNICATION_SMS_FROM_Down()
        {
            RockMigrationHelper.AddDefinedType(
                "Communication",
                "SMS From Values",
                "Values that can be used to send text messages from.  This is usually a phone number or short code that has been set up with your SMS provider.",
                Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM );
        }

        private void AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Up()
        {
            // Edit DT Attribute Response Recipient and make optional
            RockMigrationHelper.UpdateDefinedTypeAttribute( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
                Rock.SystemGuid.FieldType.PERSON,
                "Response Recipient",
                "ResponseRecipient",
                "The person who should receive responses to the SMS number. This person must have a phone number with SMS enabled or no response will be sent.",
                24,
                true,
                string.Empty,
                false,
                false,
                "E9E82709-5506-4339-8F6A-C2259329A71F" );


            // Add DT Attribute Enable Mobile Conversations and set to true
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Enable Mobile Conversations",
                "EnableMobileConversations",
                "When enabled, SMS conversations would be processed by sending messages to the Response Recipient's mobile phone. Otherwise, the conversations would be handled using the SMS Conversations page.",
                1019,
                "True",
                "60E05E00-E1A3-46A2-A56D-FE208D91FE4F" );

            // Add DT Attribute Value for Enable Mobile Conversations Attribute and set to true
            RockMigrationHelper.AddDefinedValueAttributeValue( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "60E05E00-E1A3-46A2-A56D-FE208D91FE4F", "1" );

            // Add DT Attribute LaunchWorkflowOnResponseReceived 
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
                Rock.SystemGuid.FieldType.WORKFLOW_TYPE,
                "Launch Workflow On Response Received",
                "LaunchWorkflowOnResponseReceived",
                "The workflow type to launch when a response is received. Additionally if the workflow type has any of the following attribute keys defined, those attribute values will also be set: FromPhone, Message, SMSFromDefinedValue, FromPerson, ToPerson.",
                1020,
                string.Empty,
                "49C7A5A3-D711-4E41-86E4-06408ED6C1BD" );
        }

        private void AddUpdateDefinedValuesFor_COMMUNICATION_SMS_FROM_Down()
        {
            // Edit DT Attribute Response Recipient and make optional
            RockMigrationHelper.UpdateDefinedTypeAttribute( Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM,
                Rock.SystemGuid.FieldType.PERSON,
                "Response Recipient",
                "ResponseRecipient",
                "The person who should receive responses to the SMS number. This person must have a phone number with SMS enabled or no response will be sent.",
                24,
                true,
                string.Empty,
                false,
                true,
                "E9E82709-5506-4339-8F6A-C2259329A71F" );

            // Add DT Attribute Enable Mobile Conversations and set to true
            RockMigrationHelper.DeleteAttribute( "60E05E00-E1A3-46A2-A56D-FE208D91FE4F" );

            // Add DT Attribute LaunchWorkflowOnResponseReceived 
            RockMigrationHelper.DeleteAttribute( "49C7A5A3-D711-4E41-86E4-06408ED6C1BD" );

        }

        private void PagesAndBlocksUp()
        {
            RockMigrationHelper.AddPage( true, "7F79E512-B9DB-4780-9887-AD6D63A39050","D65F783D-87A9-4CC9-8110-E83466A0EADB","SMS Conversations","","275A5175-60E0-40A2-8C63-4E9D9CD39036","fa fa-comments"); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "SMS Conversations", "Block for having SMS Conversations between an SMS enabled phone and a Rock SMS Phone number that has 'Enable Mobile Conversations' set to false.", "~/Blocks/Communication/SmsConversations.ascx", "Communication", "3497603B-3BE6-4262-B7E9-EC01FC7140EB");
            // Add Block to Page: SMS Conversations Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "275A5175-60E0-40A2-8C63-4E9D9CD39036".AsGuid(),null,Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(),"3497603B-3BE6-4262-B7E9-EC01FC7140EB".AsGuid(), "Sms Conversations","Main",@"",@"",0,"24CAFC0B-3C23-45EB-B69C-BEC68BB21B97"); 
        }

        private void PagesAndBlocksDown()
        {
            // Remove Block: Sms Conversations, from Page: SMS Conversations, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("24CAFC0B-3C23-45EB-B69C-BEC68BB21B97");
            RockMigrationHelper.DeleteBlockType("3497603B-3BE6-4262-B7E9-EC01FC7140EB"); // Communication > Sms Conversations
            RockMigrationHelper.DeletePage("275A5175-60E0-40A2-8C63-4E9D9CD39036"); //  Page: SMS Conversations, Layout: Full Width, Site: Rock RMS
        }
    }
}
