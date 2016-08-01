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
    public partial class DigitalSignature2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.SignatureDocumentType", newName: "SignatureDocumentTemplate");
            RenameColumn(table: "dbo.SignatureDocument", name: "SignatureDocumentTypeId", newName: "SignatureDocumentTemplateId");
            RenameColumn(table: "dbo.RegistrationTemplate", name: "RequiredSignatureDocumentTypeId", newName: "RequiredSignatureDocumentTemplateId");
            RenameColumn(table: "dbo.Group", name: "RequiredSignatureDocumentTypeId", newName: "RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.Group", name: "IX_RequiredSignatureDocumentTypeId", newName: "IX_RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.RegistrationTemplate", name: "IX_RequiredSignatureDocumentTypeId", newName: "IX_RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.SignatureDocument", name: "IX_SignatureDocumentTypeId", newName: "IX_SignatureDocumentTemplateId");
            AddColumn("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", c => c.Int());
            AddColumn("dbo.SignatureDocument", "LastInviteDate", c => c.DateTime());
            AddColumn("dbo.SignatureDocument", "InviteCount", c => c.Int(nullable: false));
            CreateIndex("dbo.SignatureDocumentTemplate", "InviteSystemEmailId");
            AddForeignKey("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "dbo.SystemEmail", "Id");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromName");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromAddress");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateSubject");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateBody");
            DropColumn("dbo.SignatureDocument", "RequestDate");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.SignatureDocument", "RequestDate", c => c.DateTime());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateBody", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateSubject", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromAddress", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromName", c => c.String());
            DropForeignKey("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "dbo.SystemEmail");
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "InviteSystemEmailId" });
            DropColumn("dbo.SignatureDocument", "InviteCount");
            DropColumn("dbo.SignatureDocument", "LastInviteDate");
            DropColumn("dbo.SignatureDocumentTemplate", "InviteSystemEmailId");
            RenameIndex(table: "dbo.SignatureDocument", name: "IX_SignatureDocumentTemplateId", newName: "IX_SignatureDocumentTypeId");
            RenameIndex(table: "dbo.RegistrationTemplate", name: "IX_RequiredSignatureDocumentTemplateId", newName: "IX_RequiredSignatureDocumentTypeId");
            RenameIndex(table: "dbo.Group", name: "IX_RequiredSignatureDocumentTemplateId", newName: "IX_RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.Group", name: "RequiredSignatureDocumentTemplateId", newName: "RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.RegistrationTemplate", name: "RequiredSignatureDocumentTemplateId", newName: "RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.SignatureDocument", name: "SignatureDocumentTemplateId", newName: "SignatureDocumentTypeId");
            RenameTable(name: "dbo.SignatureDocumentTemplate", newName: "SignatureDocumentType");
        }
    }
}
