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
    public partial class AddFinancialStatementTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.FinancialStatementTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        ReportTemplate = c.String(),
                        FooterTemplate = c.String(),
                        ReportSettings = c.String(),
                        LogoBinaryFileId = c.Int(),
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
                .ForeignKey("dbo.BinaryFile", t => t.LogoBinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.LogoBinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.FinancialStatementTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialStatementTemplate", "LogoBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.FinancialStatementTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialStatementTemplate", new[] { "Guid" });
            DropIndex("dbo.FinancialStatementTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FinancialStatementTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FinancialStatementTemplate", new[] { "LogoBinaryFileId" });
            DropTable("dbo.FinancialStatementTemplate");
        }
    }
}
