// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class AddFinancialGateway : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.FinancialTransaction", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "GatewayEntityTypeId" });
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "GatewayEntityTypeId" });
            CreateTable(
                "dbo.FinancialGateway",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        EntityTypeId = c.Int(nullable: false),
                        BatchTimeOffsetTicks = c.Long(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.EntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.EntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.UpdateEntityType( "Rock.Model.FinancialGateway", "122EFE60-84A6-4C7A-A852-30E4BD89A662", true, true );

            Sql( @"
    DECLARE @PayFlowProGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.PayFlowPro.Gateway' )
    DECLARE @TestGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Financial.TestGateway' )
    DECLARE @GatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '122EFE60-84A6-4C7A-A852-30E4BD89A662' )
    DECLARE @PayFlowProGatewayId int
    DECLARE @TestGatewayId int

    INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [IsActive], [BatchTimeOffsetTicks], [Guid] )
    VALUES ( 'Pay Flow Pro', @PayFlowProGatewayEntityTypeId, 0, 0, '420747BE-640C-406E-B382-CEE4BB377F29' )
    SET @PayFlowProGatewayId = SCOPE_IDENTITY()

    INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [IsActive], [BatchTimeOffsetTicks], [Guid] )
    VALUES( 'Test Gateway', @TestGatewayEntityTypeId, 0, 0, '6432D2D2-32FF-443D-B5B3-FB6C8414C3AD' )
    SET @TestGatewayId = SCOPE_IDENTITY()
    
	UPDATE [FinancialTransaction]
    SET [FinancialGatewayId] = (
	    CASE WHEN [GatewayEntityTypeId] = @PayFlowProGatewayEntityTypeId
		    THEN @PayFlowProGatewayId
		    ELSE @TestGatewayId
	    END
    )

    UPDATE [FinancialScheduledTransaction]
    SET [FinancialGatewayId] = (
	    CASE WHEN [GatewayEntityTypeId] = @PayFlowProGatewayEntityTypeId
		    THEN @PayFlowProGatewayId
		    ELSE @TestGatewayId
	    END
    )

    UPDATE [FinancialTransaction]
    SET [FinancialGatewayId] = (
	    CASE WHEN [GatewayEntityTypeId] = @PayFlowProGatewayEntityTypeId
		    THEN @PayFlowProGatewayId
		    ELSE @TestGatewayId
	    END
    )

    UPDATE G SET [IsActive] = ( CASE WHEN AV.[Value] = 'True' THEN 1 ELSE 0 END )
    FROM [Attribute] A
    INNER JOIN [AttributeValue] AV ON AV.[AttributeId] = A.[Id]
    INNER JOIN [FinancialGateway] G ON G.[Id] = ( CASE WHEN A.[EntityTypeId] = @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId ELSE @TestGatewayId END )
    WHERE [A].[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId )
    AND [A].[Key] = 'Active'
    AND [AV].[EntityId] = 0

    UPDATE G SET BatchTimeOffsetTicks = CAST(10000000 as bigint) * DATEDIFF( second, '2015-04-01 00:00:00', CAST( '2015-04-01 ' + '20:00:00' AS DATETIME) )
    FROM [Attribute] A
    INNER JOIN [AttributeValue] AV ON AV.[AttributeId] = A.[Id]
    INNER JOIN [FinancialGateway] G  ON G.[Id] = ( CASE WHEN A.[EntityTypeId] = @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId ELSE @TestGatewayId END )
    WHERE [A].[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId )
    AND [A].[Key] = 'BatchProcessTime'
    AND [AV].[EntityId] = 0

    DELETE [Attribute]
    WHERE [EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId )
    AND [Key] IN ( 'Active', 'BatchProcessTime', 'Order')

    UPDATE AV SET [EntityId] = ( 
	    CASE WHEN A.[EntityTypeId] = @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId ELSE @TestGatewayId END 
    )
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    WHERE A.[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId )

    UPDATE [Attribute] SET
	    [IsSystem] = 1,
	    [EntityTypeId] = @GatewayEntityTypeId,
	    [EntityTypeQualifierColumn] = 'EntityTypeId',
	    [EntityTypeQualifierValue] = CAST( [EntityTypeId] AS VARCHAR )
    WHERE [EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId )
" );

            AddColumn("dbo.FinancialTransaction", "FinancialGatewayId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "FinancialGatewayId", c => c.Int());
            AddColumn("dbo.FinancialPersonSavedAccount", "FinancialGatewayId", c => c.Int());
            CreateIndex("dbo.FinancialTransaction", "FinancialGatewayId");
            CreateIndex("dbo.FinancialScheduledTransaction", "FinancialGatewayId");
            CreateIndex("dbo.FinancialPersonSavedAccount", "FinancialGatewayId");
            AddForeignKey("dbo.FinancialTransaction", "FinancialGatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "FinancialGatewayId", "dbo.FinancialGateway", "Id");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "FinancialGatewayId", "dbo.FinancialGateway", "Id");

            DropColumn( "dbo.FinancialTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", c => c.Int());
            AddColumn("dbo.FinancialTransaction", "GatewayEntityTypeId", c => c.Int());
            DropForeignKey("dbo.FinancialPersonSavedAccount", "FinancialGatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialScheduledTransaction", "FinancialGatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialTransaction", "FinancialGatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.FinancialGateway", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.FinancialGateway", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.FinancialPersonSavedAccount", new[] { "FinancialGatewayId" });
            DropIndex("dbo.FinancialScheduledTransaction", new[] { "FinancialGatewayId" });
            DropIndex("dbo.FinancialGateway", new[] { "Guid" });
            DropIndex("dbo.FinancialGateway", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.FinancialGateway", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.FinancialGateway", new[] { "EntityTypeId" });
            DropIndex("dbo.FinancialTransaction", new[] { "FinancialGatewayId" });
            DropColumn("dbo.FinancialPersonSavedAccount", "FinancialGatewayId");
            DropColumn("dbo.FinancialScheduledTransaction", "FinancialGatewayId");
            DropColumn("dbo.FinancialTransaction", "FinancialGatewayId");
            DropTable("dbo.FinancialGateway");
            CreateIndex("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId");
            CreateIndex("dbo.FinancialTransaction", "GatewayEntityTypeId");
            AddForeignKey("dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
            AddForeignKey("dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id");
        }
    }
}
