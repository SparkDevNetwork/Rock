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
    public partial class BenevolenceUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BenevolenceRequestDocument",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BenevolenceRequestId = c.Int(nullable: false),
                        BinaryFileId = c.Int(nullable: false),
                        Order = c.Int(),
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
                .ForeignKey("dbo.BenevolenceRequest", t => t.BenevolenceRequestId, cascadeDelete: true)
                .ForeignKey("dbo.BinaryFile", t => t.BinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.BenevolenceRequestId)
                .Index(t => t.BinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            AddColumn("dbo.BenevolenceRequest", "ProvidedNextSteps", c => c.String());
            AddColumn("dbo.BenevolenceRequest", "CampusId", c => c.Int());
            CreateIndex("dbo.BenevolenceRequest", "CampusId");
            AddForeignKey("dbo.BenevolenceRequest", "CampusId", "dbo.Campus", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.BenevolenceRequestDocument", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BenevolenceRequestDocument", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BenevolenceRequestDocument", "BinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.BenevolenceRequestDocument", "BenevolenceRequestId", "dbo.BenevolenceRequest");
            DropForeignKey("dbo.BenevolenceRequest", "CampusId", "dbo.Campus");
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignKey" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignGuid" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ForeignId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "Guid" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "BinaryFileId" });
            DropIndex("dbo.BenevolenceRequestDocument", new[] { "BenevolenceRequestId" });
            DropIndex("dbo.BenevolenceRequest", new[] { "CampusId" });
            DropColumn("dbo.BenevolenceRequest", "CampusId");
            DropColumn("dbo.BenevolenceRequest", "ProvidedNextSteps");
            DropTable("dbo.BenevolenceRequestDocument");
        }
    }
}
