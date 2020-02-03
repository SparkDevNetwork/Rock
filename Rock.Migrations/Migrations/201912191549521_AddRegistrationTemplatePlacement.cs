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
    public partial class AddRegistrationTemplatePlacement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.RegistrationTemplatePlacement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        RegistrationTemplateId = c.Int(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        IconCssClass = c.String(maxLength: 100),
                        AllowMultiplePlacements = c.Boolean(nullable: false),
                        IsInternal = c.Boolean(nullable: false),
                        Cost = c.Decimal(precision: 18, scale: 2),
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
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.RegistrationTemplate", t => t.RegistrationTemplateId)
                .Index(t => t.RegistrationTemplateId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.GroupType", "AllowAnyChildGroupType", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RegistrationTemplatePlacement", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.RegistrationTemplatePlacement", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplatePlacement", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.RegistrationTemplatePlacement", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RegistrationTemplatePlacement", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplatePlacement", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplatePlacement", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplatePlacement", new[] { "GroupTypeId" });
            DropIndex("dbo.RegistrationTemplatePlacement", new[] { "RegistrationTemplateId" });
            DropColumn("dbo.GroupType", "AllowAnyChildGroupType");
            DropTable("dbo.RegistrationTemplatePlacement");
        }
    }
}
