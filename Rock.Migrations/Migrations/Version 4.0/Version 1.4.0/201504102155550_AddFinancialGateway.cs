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
            DropForeignKey( "dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType" );
            DropIndex( "dbo.FinancialTransaction", new[] { "GatewayEntityTypeId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "GatewayEntityTypeId" } );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "GatewayEntityTypeId" } );
            CreateTable(
                "dbo.FinancialGateway",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Description = c.String(),
                    EntityTypeId = c.Int( nullable: false ),
                    BatchTimeOffsetTicks = c.Long( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.FinancialTransaction", "FinancialGatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialScheduledTransaction", "FinancialGatewayId", c => c.Int() );
            AddColumn( "dbo.FinancialPersonSavedAccount", "FinancialGatewayId", c => c.Int() );
            CreateIndex( "dbo.FinancialTransaction", "FinancialGatewayId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "FinancialGatewayId" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "FinancialGatewayId" );
            AddForeignKey( "dbo.FinancialTransaction", "FinancialGatewayId", "dbo.FinancialGateway", "Id" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "FinancialGatewayId", "dbo.FinancialGateway", "Id" );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "FinancialGatewayId", "dbo.FinancialGateway", "Id" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.FinancialGateway", "122EFE60-84A6-4C7A-A852-30E4BD89A662", true, true );
            RockMigrationHelper.UpdateFieldType( "Financial Gateway", "", "Rock", "Rock.Field.Types.FinancialGatewayFieldType", "7B34F9D8-6BBA-423E-B50E-525ABB3A1013" );

            Sql( @"
    DECLARE @PayFlowProGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.PayFlowPro.Gateway' )
    DECLARE @TestGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Financial.TestGateway' )
    DECLARE @CyberSourceGatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.CyberSource.Gateway' )
    DECLARE @GatewayEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '122EFE60-84A6-4C7A-A852-30E4BD89A662' )
    DECLARE @GatewayFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '7B34F9D8-6BBA-423E-B50E-525ABB3A1013' )
    DECLARE @PayFlowProGatewayId int
    DECLARE @TestGatewayId int
    DECLARE @CyberSourceGatewayId int

    IF @PayFlowProGatewayEntityTypeId IS NOT NULL
    BEGIN
        INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [IsActive], [BatchTimeOffsetTicks], [Guid] )
        VALUES ( 'Pay Flow Pro', @PayFlowProGatewayEntityTypeId, 0, 0, '420747BE-640C-406E-B382-CEE4BB377F29' )
        SET @PayFlowProGatewayId = SCOPE_IDENTITY()
    END

    IF @TestGatewayEntityTypeId IS NOT NULL
    BEGIN
        INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [IsActive], [BatchTimeOffsetTicks], [Guid] )
        VALUES( 'Test Gateway', @TestGatewayEntityTypeId, 0, 0, '6432D2D2-32FF-443D-B5B3-FB6C8414C3AD' )
        SET @TestGatewayId = SCOPE_IDENTITY()
    END

    IF @CyberSourceGatewayEntityTypeId IS NOT NULL
    BEGIN
        INSERT INTO [FinancialGateway] ( [Name], [EntityTypeId], [IsActive], [BatchTimeOffsetTicks], [Guid] )
        VALUES ( 'Cyber Source', @CyberSourceGatewayEntityTypeId, 0, 0, '457B3B47-88EF-45F8-92FE-4F7F2C60718A' )
        SET @CyberSourceGatewayId = SCOPE_IDENTITY()
    END
    
	UPDATE [FinancialTransaction]
    SET [FinancialGatewayId] = (
	    CASE [GatewayEntityTypeId] 
            WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId
            WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId
		    ELSE @TestGatewayId
	    END
    )

    UPDATE [FinancialScheduledTransaction]
    SET [FinancialGatewayId] = (
	    CASE [GatewayEntityTypeId] 
            WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId
            WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId
		    ELSE @TestGatewayId
	    END
    )
    WHERE [GatewayEntityTypeId] IS NOT NULL

    UPDATE [FinancialTransaction]
    SET [FinancialGatewayId] = (
	    CASE [GatewayEntityTypeId] 
            WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId
            WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId
		    ELSE @TestGatewayId
	    END
    )
    WHERE [GatewayEntityTypeId] IS NOT NULL

    UPDATE G SET [IsActive] = ( CASE WHEN AV.[Value] = 'True' THEN 1 ELSE 0 END )
    FROM [Attribute] A
    INNER JOIN [AttributeValue] AV 
        ON AV.[AttributeId] = A.[Id]
    INNER JOIN [FinancialGateway] G 
        ON G.[Id] = ( 
            CASE A.[EntityTypeId] 
                WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId 
                WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId 
                ELSE @TestGatewayId END 
            )
    WHERE [A].[EntityTypeId] IS NOT NULL
    AND [A].[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId, @CyberSourceGatewayEntityTypeId )
    AND [A].[Key] = 'Active'
    AND [AV].[EntityId] = 0

    UPDATE G SET BatchTimeOffsetTicks = CAST(10000000 as bigint) * DATEDIFF( second, '2015-04-01 00:00:00', CAST( '2015-04-01 ' + '20:00:00' AS DATETIME) )
    FROM [Attribute] A
    INNER JOIN [AttributeValue] AV 
        ON AV.[AttributeId] = A.[Id]
    INNER JOIN [FinancialGateway] G 
        ON G.[Id] = ( 
            CASE A.[EntityTypeId] 
                WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId 
                WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId 
                ELSE @TestGatewayId END 
            )
    WHERE [A].[EntityTypeId] IS NOT NULL
    AND [A].[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId, @CyberSourceGatewayEntityTypeId )
    AND [A].[Key] = 'BatchProcessTime'
    AND [AV].[EntityId] = 0

    DELETE [Attribute]
    WHERE [EntityTypeId] IS NOT NULL
    AND [EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId, @CyberSourceGatewayEntityTypeId )
    AND [Key] IN ( 'Active', 'BatchProcessTime', 'Order')

    UPDATE AV SET [EntityId] = ( 
        CASE A.[EntityTypeId] 
            WHEN @PayFlowProGatewayEntityTypeId THEN @PayFlowProGatewayId 
            WHEN @CyberSourceGatewayEntityTypeId THEN @CyberSourceGatewayId 
            ELSE @TestGatewayId END 
        )
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    WHERE A.[EntityTypeId] IS NOT NULL
    AND A.[EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId, @CyberSourceGatewayEntityTypeId )

    UPDATE [Attribute] SET
	    [IsSystem] = 1,
	    [EntityTypeId] = @GatewayEntityTypeId,
	    [EntityTypeQualifierColumn] = 'EntityTypeId',
	    [EntityTypeQualifierValue] = CAST( [EntityTypeId] AS VARCHAR )
    WHERE [EntityTypeId] IS NOT NULL
    AND [EntityTypeId] IN ( @PayFlowProGatewayEntityTypeId, @TestGatewayEntityTypeId, @CyberSourceGatewayEntityTypeId )

    UPDATE A SET 
	    [FieldTypeId] = @GatewayFieldTypeId,
	    [DefaultValue] = '6432D2D2-32FF-443D-B5B3-FB6C8414C3AD'
    FROM [Attribute] A
    INNER JOIN [FieldType] F ON F.[Id] = A.[FieldTypeId]
    WHERE F.[Guid] = 'A7486B0E-4CA2-4E00-A987-5544C7DABA76' 
    AND [Key] IN ( 'CCGateway', 'ACHGateway' )

    UPDATE AV SET [Value] = ( 
        CASE T.[Name] 
            WHEN 'Rock.PayFlowPro.Gateway' THEN '420747BE-640C-406E-B382-CEE4BB377F29' 
            WHEN 'Rock.CyberSource.Gateway' THEN '457B3B47-88EF-45F8-92FE-4F7F2C60718A' 
            ELSE '6432D2D2-32FF-443D-B5B3-FB6C8414C3AD' END 
        )
    FROM [Attribute] A 
    INNER JOIN [AttributeValue] AV
	    ON AV.[AttributeId] = A.[Id]
    LEFT OUTER JOIN [EntityType] T	
	    ON CAST( T.[Guid] as varchar(60) ) = AV.[Value]
    WHERE A.[FieldTypeId] = @GatewayFieldTypeId 
    AND A.[Key] IN ( 'CCGateway', 'ACHGateway' )

" );

            DropColumn( "dbo.FinancialTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId" );
            DropColumn( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId" );

            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Financial Gateways", "", "F65AA215-8B46-4E34-B709-FA956BF62C30", "fa fa-credit-card" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "F65AA215-8B46-4E34-B709-FA956BF62C30", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Gateway Detail", "", "24DE6092-CE91-468C-8E49-94DB3875B9B7", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Gateway Detail", "Displays the details of the given financial gateway.", "~/Blocks/Finance/GatewayDetail.ascx", "Finance", "B4D8CBCA-00F6-4D81-B8B6-170373D28128" );
            RockMigrationHelper.UpdateBlockType( "Gateway List", "Block for viewing list of financial gateways.", "~/Blocks/Finance/GatewayList.ascx", "Finance", "32E89BAE-C085-40B3-B872-B62E25A62BDB" );
            // Add Block to Page: Financial Gateways, Site: Rock RMS
            RockMigrationHelper.AddBlock( "F65AA215-8B46-4E34-B709-FA956BF62C30", "", "32E89BAE-C085-40B3-B872-B62E25A62BDB", "Gateway List", "Main", "", "", 0, "80D93241-AB5D-46EC-BC12-0E32C3CA784C" );
            // Add Block to Page: Gateway Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "24DE6092-CE91-468C-8E49-94DB3875B9B7", "", "B4D8CBCA-00F6-4D81-B8B6-170373D28128", "Gateway Detail", "Main", "", "", 0, "CB7AF6BA-4A89-4653-B4A2-A73999BC7236" );
            // Attrib for BlockType: Gateway List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "32E89BAE-C085-40B3-B872-B62E25A62BDB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "CBDD5D38-07EE-4ED5-8904-AF18F4161037" );
            // Attrib Value for Block:Gateway List, Attribute:Detail Page Page: Financial Gateways, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "80D93241-AB5D-46EC-BC12-0E32C3CA784C", "CBDD5D38-07EE-4ED5-8904-AF18F4161037", @"24de6092-ce91-468c-8e49-94db3875b9b7" );

            Sql( @"
    DECLARE @Order int = ( SELECT TOP 1 [Order] FROM [Page] WHERE [Guid] = '6F8EC649-FDED-4805-B7AF-42A6901C197F' )
    UPDATE [Page] SET [Order] = @Order WHERE [Guid] = 'F65AA215-8B46-4E34-B709-FA956BF62C30'
" );

            RockMigrationHelper.DeleteBlock( "8C707818-ECB1-4E40-8F2C-6E9802E6BA73" );
            RockMigrationHelper.DeletePage( "6F8EC649-FDED-4805-B7AF-42A6901C197F" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Gateway List:Detail Page
            RockMigrationHelper.DeleteAttribute( "CBDD5D38-07EE-4ED5-8904-AF18F4161037" );
            // Remove Block: Gateway Detail, from Page: Gateway Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "CB7AF6BA-4A89-4653-B4A2-A73999BC7236" );
            // Remove Block: Gateway List, from Page: Financial Gateways, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "80D93241-AB5D-46EC-BC12-0E32C3CA784C" );
            RockMigrationHelper.DeleteBlockType( "32E89BAE-C085-40B3-B872-B62E25A62BDB" ); // Gateway List
            RockMigrationHelper.DeleteBlockType( "B4D8CBCA-00F6-4D81-B8B6-170373D28128" ); // Gateway Detail
            RockMigrationHelper.DeletePage( "24DE6092-CE91-468C-8E49-94DB3875B9B7" ); //  Page: Gateway Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "F65AA215-8B46-4E34-B709-FA956BF62C30" ); //  Page: Financial Gateways, Layout: Full Width, Site: Rock RMS

            AddColumn( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", c => c.Int() );
            AddColumn( "dbo.FinancialTransaction", "GatewayEntityTypeId", c => c.Int() );
            DropForeignKey( "dbo.FinancialPersonSavedAccount", "FinancialGatewayId", "dbo.FinancialGateway" );
            DropForeignKey( "dbo.FinancialScheduledTransaction", "FinancialGatewayId", "dbo.FinancialGateway" );
            DropForeignKey( "dbo.FinancialTransaction", "FinancialGatewayId", "dbo.FinancialGateway" );
            DropForeignKey( "dbo.FinancialGateway", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.FinancialGateway", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.FinancialGateway", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.FinancialPersonSavedAccount", new[] { "FinancialGatewayId" } );
            DropIndex( "dbo.FinancialScheduledTransaction", new[] { "FinancialGatewayId" } );
            DropIndex( "dbo.FinancialGateway", new[] { "Guid" } );
            DropIndex( "dbo.FinancialGateway", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.FinancialGateway", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.FinancialGateway", new[] { "EntityTypeId" } );
            DropIndex( "dbo.FinancialTransaction", new[] { "FinancialGatewayId" } );
            DropColumn( "dbo.FinancialPersonSavedAccount", "FinancialGatewayId" );
            DropColumn( "dbo.FinancialScheduledTransaction", "FinancialGatewayId" );
            DropColumn( "dbo.FinancialTransaction", "FinancialGatewayId" );
            DropTable( "dbo.FinancialGateway" );
            CreateIndex( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId" );
            CreateIndex( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId" );
            CreateIndex( "dbo.FinancialTransaction", "GatewayEntityTypeId" );
            AddForeignKey( "dbo.FinancialPersonSavedAccount", "GatewayEntityTypeId", "dbo.EntityType", "Id" );
            AddForeignKey( "dbo.FinancialScheduledTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id" );
            AddForeignKey( "dbo.FinancialTransaction", "GatewayEntityTypeId", "dbo.EntityType", "Id" );
        }
    }
}
