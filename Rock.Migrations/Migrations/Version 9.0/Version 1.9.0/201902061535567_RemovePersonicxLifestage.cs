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
    public partial class RemovePersonicxLifestage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageGroup", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageGroup", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "MetaPersonicxLifestyleGroupId", "dbo.MetaPersonicxLifestageGroup");
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Person", "MetaPersonicxLifestageClusterId", "dbo.MetaPersonicxLifestageCluster");
            DropForeignKey("dbo.Person", "MetaPersonicxLifestageGroupId", "dbo.MetaPersonicxLifestageGroup");
            DropIndex("dbo.Person", new[] { "MetaPersonicxLifestageClusterId" });
            DropIndex("dbo.Person", new[] { "MetaPersonicxLifestageGroupId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "MetaPersonicxLifestyleGroupId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "Guid" });
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "Guid" });
            DropColumn("dbo.Person", "MetaPersonicxLifestageClusterId");
            DropColumn("dbo.Person", "MetaPersonicxLifestageGroupId");
            DropTable("dbo.MetaPersonicxLifestageCluster");
            DropTable("dbo.MetaPersonicxLifestageGroup");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CreateTable(
                "dbo.MetaPersonicxLifestageGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LifestyleGroupCode = c.String(nullable: false, maxLength: 3, fixedLength: true),
                        LifestyleGroupName = c.String(nullable: false, maxLength: 50),
                        Summary = c.String(maxLength: 600),
                        Description = c.String(),
                        DetailsUrl = c.String(maxLength: 120),
                        LifeStage = c.String(maxLength: 120),
                        LifeStageLevel = c.String(maxLength: 50),
                        MaritalStatus = c.String(maxLength: 25),
                        HomeOwnership = c.String(maxLength: 50),
                        Children = c.String(maxLength: 50),
                        Income = c.String(maxLength: 50),
                        IncomeLevel = c.String(maxLength: 50),
                        IncomeRank = c.Int(),
                        Urbanicity = c.String(maxLength: 50),
                        UrbanicityRank = c.Int(),
                        NetWorth = c.String(maxLength: 50),
                        NetWorthLevel = c.String(maxLength: 50),
                        NetworthRank = c.Int(),
                        PercentUS = c.Decimal(precision: 18, scale: 2),
                        PercentOrganization = c.Decimal(precision: 18, scale: 2),
                        OrganizationHouseholdCount = c.Int(),
                        OrganizationIndividualCount = c.Int(),
                        MeanAge = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MetaPersonicxLifestageCluster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MetaPersonicxLifestyleGroupId = c.Int(nullable: false),
                        LifestyleClusterCode = c.String(nullable: false, maxLength: 2, fixedLength: true),
                        LifestyleClusterName = c.String(nullable: false, maxLength: 50),
                        Summary = c.String(maxLength: 1000),
                        Description = c.String(),
                        DetailsUrl = c.String(maxLength: 120),
                        LifeStage = c.String(maxLength: 120),
                        LifeStageLevel = c.String(maxLength: 50),
                        MaritalStatus = c.String(maxLength: 25),
                        HomeOwnership = c.String(maxLength: 50),
                        Children = c.String(maxLength: 50),
                        Income = c.String(maxLength: 50),
                        IncomeLevel = c.String(maxLength: 50),
                        IncomeRank = c.Int(),
                        Urbanicity = c.String(maxLength: 50),
                        UrbanicityRank = c.Int(),
                        NetWorth = c.String(maxLength: 50),
                        NetWorthLevel = c.String(maxLength: 50),
                        NetworthRank = c.Int(),
                        PercentUS = c.Decimal(precision: 18, scale: 2),
                        PercentOrganization = c.Decimal(precision: 18, scale: 2),
                        OrganizationHouseholdCount = c.Int(),
                        OrganizationIndividualCount = c.Int(),
                        MeanAge = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Person", "MetaPersonicxLifestageGroupId", c => c.Int());
            AddColumn("dbo.Person", "MetaPersonicxLifestageClusterId", c => c.Int());
            CreateIndex("dbo.MetaPersonicxLifestageGroup", "Guid", unique: true);
            CreateIndex("dbo.MetaPersonicxLifestageGroup", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MetaPersonicxLifestageGroup", "CreatedByPersonAliasId");
            CreateIndex("dbo.MetaPersonicxLifestageCluster", "Guid", unique: true);
            CreateIndex("dbo.MetaPersonicxLifestageCluster", "ModifiedByPersonAliasId");
            CreateIndex("dbo.MetaPersonicxLifestageCluster", "CreatedByPersonAliasId");
            CreateIndex("dbo.MetaPersonicxLifestageCluster", "MetaPersonicxLifestyleGroupId");
            CreateIndex("dbo.Person", "MetaPersonicxLifestageGroupId");
            CreateIndex("dbo.Person", "MetaPersonicxLifestageClusterId");
            AddForeignKey("dbo.Person", "MetaPersonicxLifestageGroupId", "dbo.MetaPersonicxLifestageGroup", "Id");
            AddForeignKey("dbo.Person", "MetaPersonicxLifestageClusterId", "dbo.MetaPersonicxLifestageCluster", "Id");
            AddForeignKey("dbo.MetaPersonicxLifestageCluster", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MetaPersonicxLifestageCluster", "MetaPersonicxLifestyleGroupId", "dbo.MetaPersonicxLifestageGroup", "Id");
            AddForeignKey("dbo.MetaPersonicxLifestageGroup", "ModifiedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MetaPersonicxLifestageGroup", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.MetaPersonicxLifestageCluster", "CreatedByPersonAliasId", "dbo.PersonAlias", "Id");
        }
    }
}
