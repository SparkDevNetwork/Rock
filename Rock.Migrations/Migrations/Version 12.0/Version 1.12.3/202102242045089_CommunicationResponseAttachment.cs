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
    public partial class CommunicationResponseAttachment : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommunicationResponseAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BinaryFileId = c.Int(nullable: false),
                        CommunicationResponseId = c.Int(nullable: false),
                        CommunicationType = c.Int(nullable: false),
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
                .ForeignKey("dbo.CommunicationResponse", t => t.CommunicationResponseId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CommunicationResponseId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CommunicationResponseAttachment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationResponseAttachment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationResponseAttachment", "CommunicationResponseId", "dbo.CommunicationResponse");
            DropForeignKey("dbo.CommunicationResponseAttachment", "BinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.CommunicationResponseAttachment", new[] { "Guid" });
            DropIndex("dbo.CommunicationResponseAttachment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationResponseAttachment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationResponseAttachment", new[] { "CommunicationResponseId" });
            DropIndex("dbo.CommunicationResponseAttachment", new[] { "BinaryFileId" });
            DropTable("dbo.CommunicationResponseAttachment");
        }
    }
}
