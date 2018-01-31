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
    public partial class MetaModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // JE: Add Page Icon to Fundraising Progress Page
            Sql( @"
  UPDATE [Page]
  SET [IconCssClass] = 'fa fa-certificate'
  WHERE [Guid] = '3E0F2EF9-DC32-4DFD-B213-A410AE5B6AB7'
" );

            CreateTable(
                "dbo.MetaFirstNameGenderLookup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        MaleCount = c.Int(),
                        FemaleCount = c.Int(),
                        Country = c.String(maxLength: 10),
                        Language = c.String(maxLength: 10),
                        TotalCount = c.Int(),
                        FemalePercent = c.Decimal(precision: 18, scale: 2),
                        MalePercent = c.Decimal(precision: 18, scale: 2),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.MetaLastNameLookup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LastName = c.String(nullable: false, maxLength: 10),
                        Count = c.Int(),
                        Rank = c.Int(),
                        CountIn100k = c.Decimal(precision: 18, scale: 2),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.MetaNickNameLookup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        NickName = c.String(nullable: false, maxLength: 50),
                        Gender = c.String(maxLength: 1, fixedLength: true, unicode: false),
                        Count = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.MetaPersonicxLifestageCluster",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MetaPersonicxLifestyleGroupId = c.Int(nullable: false),
                        LifestyleClusterCode = c.String(nullable: false, maxLength: 2, fixedLength: true),
                        LifestyleClusterName = c.String(nullable: false, maxLength: 50),
                        Summary = c.String(maxLength: 600),
                        Description = c.String(),
                        DetailsUrl = c.String(maxLength: 120),
                        LifeStage = c.String(maxLength: 120),
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
                .ForeignKey("dbo.MetaPersonicxLifestageGroup", t => t.MetaPersonicxLifestyleGroupId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.MetaPersonicxLifestyleGroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
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
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "MetaPersonicxLifestyleGroupId", "dbo.MetaPersonicxLifestageGroup");
            DropForeignKey("dbo.MetaPersonicxLifestageGroup", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageGroup", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MetaPersonicxLifestageCluster", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "Guid" });
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageGroup", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "Guid" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MetaPersonicxLifestageCluster", new[] { "MetaPersonicxLifestyleGroupId" });
            DropIndex("dbo.MetaNickNameLookup", new[] { "Guid" });
            DropIndex("dbo.MetaLastNameLookup", new[] { "Guid" });
            DropIndex("dbo.MetaFirstNameGenderLookup", new[] { "Guid" });
            DropTable("dbo.MetaPersonicxLifestageGroup");
            DropTable("dbo.MetaPersonicxLifestageCluster");
            DropTable("dbo.MetaNickNameLookup");
            DropTable("dbo.MetaLastNameLookup");
            DropTable("dbo.MetaFirstNameGenderLookup");
        }
    }
}
