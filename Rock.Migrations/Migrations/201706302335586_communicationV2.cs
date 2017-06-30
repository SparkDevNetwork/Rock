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
    public partial class communicationV2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommunicationAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BinaryFileId = c.Int(nullable: false),
                        CommunicationId = c.Int(nullable: false),
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
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.Communication", t => t.CommunicationId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CommunicationId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.CommunicationRecipient", "MediumEntityTypeId", c => c.Int());
            AddColumn("dbo.Communication", "Name", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "CommunicationType", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "ListGroupId", c => c.Int());
            AddColumn("dbo.Communication", "Segments", c => c.String());
            AddColumn("dbo.Communication", "SegmentCriteria", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "FromName", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "FromEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "ReplyToEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "CCEmails", c => c.String());
            AddColumn("dbo.Communication", "BCCEmails", c => c.String());
            AddColumn("dbo.Communication", "Message", c => c.String());
            AddColumn("dbo.Communication", "MessageMetaData", c => c.String());
            AddColumn("dbo.Communication", "SMSFromDefinedValueId", c => c.Int());
            AddColumn("dbo.Communication", "SMSMessage", c => c.String());
            AddColumn("dbo.Communication", "PushTitle", c => c.String(maxLength: 100));
            AddColumn("dbo.Communication", "PushMessage", c => c.String());
            AddColumn("dbo.Communication", "PushSound", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "ImageFileId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "FromName", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "FromEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "ReplyToEmail", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "CCEmails", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "BCCEmails", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "Message", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "MessageMetaData", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "FromNumber", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "SMSMessage", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "Title", c => c.String(maxLength: 100));
            AddColumn("dbo.CommunicationTemplate", "PushMessage", c => c.String());
            AddColumn("dbo.CommunicationTemplate", "PushSound", c => c.String(maxLength: 100));
            CreateIndex("dbo.CommunicationRecipient", "MediumEntityTypeId");
            CreateIndex("dbo.Communication", "SMSFromDefinedValueId");
            AddForeignKey("dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue", "Id");
            AddForeignKey("dbo.CommunicationRecipient", "MediumEntityTypeId", "dbo.EntityType", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CommunicationRecipient", "MediumEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.CommunicationAttachment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationAttachment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationAttachment", "CommunicationId", "dbo.Communication");
            DropForeignKey("dbo.CommunicationAttachment", "BinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.CommunicationAttachment", new[] { "Guid" });
            DropIndex("dbo.CommunicationAttachment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "CommunicationId" });
            DropIndex("dbo.CommunicationAttachment", new[] { "BinaryFileId" });
            DropIndex("dbo.Communication", new[] { "SMSFromDefinedValueId" });
            DropIndex("dbo.CommunicationRecipient", new[] { "MediumEntityTypeId" });
            DropColumn("dbo.CommunicationTemplate", "PushSound");
            DropColumn("dbo.CommunicationTemplate", "PushMessage");
            DropColumn("dbo.CommunicationTemplate", "Title");
            DropColumn("dbo.CommunicationTemplate", "SMSMessage");
            DropColumn("dbo.CommunicationTemplate", "FromNumber");
            DropColumn("dbo.CommunicationTemplate", "MessageMetaData");
            DropColumn("dbo.CommunicationTemplate", "Message");
            DropColumn("dbo.CommunicationTemplate", "BCCEmails");
            DropColumn("dbo.CommunicationTemplate", "CCEmails");
            DropColumn("dbo.CommunicationTemplate", "ReplyToEmail");
            DropColumn("dbo.CommunicationTemplate", "FromEmail");
            DropColumn("dbo.CommunicationTemplate", "FromName");
            DropColumn("dbo.CommunicationTemplate", "ImageFileId");
            DropColumn("dbo.Communication", "PushSound");
            DropColumn("dbo.Communication", "PushMessage");
            DropColumn("dbo.Communication", "PushTitle");
            DropColumn("dbo.Communication", "SMSMessage");
            DropColumn("dbo.Communication", "SMSFromDefinedValueId");
            DropColumn("dbo.Communication", "MessageMetaData");
            DropColumn("dbo.Communication", "Message");
            DropColumn("dbo.Communication", "BCCEmails");
            DropColumn("dbo.Communication", "CCEmails");
            DropColumn("dbo.Communication", "ReplyToEmail");
            DropColumn("dbo.Communication", "FromEmail");
            DropColumn("dbo.Communication", "FromName");
            DropColumn("dbo.Communication", "SegmentCriteria");
            DropColumn("dbo.Communication", "Segments");
            DropColumn("dbo.Communication", "ListGroupId");
            DropColumn("dbo.Communication", "CommunicationType");
            DropColumn("dbo.Communication", "Name");
            DropColumn("dbo.CommunicationRecipient", "MediumEntityTypeId");
            DropTable("dbo.CommunicationAttachment");
        }
    }
}
