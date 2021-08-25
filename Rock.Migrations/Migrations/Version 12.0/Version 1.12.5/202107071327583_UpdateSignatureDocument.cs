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
    public partial class UpdateSignatureDocument : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.SignatureDocumentTemplate", "LavaTemplate", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.SignatureDocumentTemplate", "DocumentTerm", c => c.String(maxLength: 100));
            AddColumn("dbo.SignatureDocumentTemplate", "SignatureType", c => c.Int(nullable: false));
            AddColumn("dbo.SignatureDocumentTemplate", "CompletionSystemCommunicationId", c => c.Int());
            AddColumn("dbo.SignatureDocument", "SignedDocumentText", c => c.String());
            AddColumn("dbo.SignatureDocument", "SignedName", c => c.String(maxLength: 250));
            AddColumn("dbo.SignatureDocument", "SignedClientIp", c => c.String(maxLength: 128));
            AddColumn("dbo.SignatureDocument", "SignedClientUserAgent", c => c.String());
            AddColumn("dbo.SignatureDocument", "SignedDateTime", c => c.DateTime());
            AddColumn("dbo.SignatureDocument", "SignedByEmail", c => c.String(maxLength: 75));
            AddColumn("dbo.SignatureDocument", "CompletionEmailSentDateTime", c => c.DateTime());
            AddColumn("dbo.SignatureDocument", "SignatureData", c => c.String());
            AddColumn("dbo.SignatureDocument", "SignatureVerficationHash", c => c.String(maxLength: 40));
            AddColumn("dbo.SignatureDocument", "EntityTypeId", c => c.Int());
            AddColumn("dbo.SignatureDocument", "EntityId", c => c.Int());
            CreateIndex("dbo.SignatureDocumentTemplate", "CompletionSystemCommunicationId");
            CreateIndex("dbo.SignatureDocument", "EntityTypeId");
            AddForeignKey("dbo.SignatureDocumentTemplate", "CompletionSystemCommunicationId", "dbo.SystemCommunication", "Id");
            AddForeignKey("dbo.SignatureDocument", "EntityTypeId", "dbo.EntityType", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.SignatureDocument", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.SignatureDocumentTemplate", "CompletionSystemCommunicationId", "dbo.SystemCommunication");
            DropIndex("dbo.SignatureDocument", new[] { "EntityTypeId" });
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "CompletionSystemCommunicationId" });
            DropColumn("dbo.SignatureDocument", "EntityId");
            DropColumn("dbo.SignatureDocument", "EntityTypeId");
            DropColumn("dbo.SignatureDocument", "SignatureVerficationHash");
            DropColumn("dbo.SignatureDocument", "SignatureData");
            DropColumn("dbo.SignatureDocument", "CompletionEmailSentDateTime");
            DropColumn("dbo.SignatureDocument", "SignedByEmail");
            DropColumn("dbo.SignatureDocument", "SignedDateTime");
            DropColumn("dbo.SignatureDocument", "SignedClientUserAgent");
            DropColumn("dbo.SignatureDocument", "SignedClientIp");
            DropColumn("dbo.SignatureDocument", "SignedName");
            DropColumn("dbo.SignatureDocument", "SignedDocumentText");
            DropColumn("dbo.SignatureDocumentTemplate", "CompletionSystemCommunicationId");
            DropColumn("dbo.SignatureDocumentTemplate", "SignatureType");
            DropColumn("dbo.SignatureDocumentTemplate", "DocumentTerm");
            DropColumn("dbo.SignatureDocumentTemplate", "IsActive");
            DropColumn("dbo.SignatureDocumentTemplate", "LavaTemplate");
        }
    }
}
