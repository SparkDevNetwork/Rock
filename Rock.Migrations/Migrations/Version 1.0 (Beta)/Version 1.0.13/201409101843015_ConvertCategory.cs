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
    public partial class ConvertCategory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DefinedType", "CategoryId", c => c.Int());
            AddColumn("dbo.SystemEmail", "CategoryId", c => c.Int());
            CreateIndex("dbo.DefinedType", "CategoryId");
            CreateIndex("dbo.SystemEmail", "CategoryId");
            AddForeignKey("dbo.DefinedType", "CategoryId", "dbo.Category", "Id");
            AddForeignKey( "dbo.SystemEmail", "CategoryId", "dbo.Category", "Id" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.SystemEmail", "System Email", "Rock.Model.SystemEmail, Rock, Version=1.0.12.0, Culture=neutral, PublicKeyToken=null", true, true, "B21FD119-893E-46C0-B42D-E4CDD5C8C49D" );

            Sql( @"
    DECLARE @DefinedTypeEntity int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedType' )
    DECLARE @SystemEmailEntity int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemEmail' )

    INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [Name], [Guid], [Order] )
    SELECT 
	    CAST(MAX(CAST([IsSystem] AS int)) AS bit)
	    , @DefinedTypeEntity
	    , [Category]
	    , NEWID()
	    , 0
    FROM [DefinedType]
    WHERE [Category] NOT IN ( 
	    SELECT DISTINCT [Name] 
	    FROM [Category]
	    WHERE [EntityTypeId] = @DefinedTypeEntity
    )
    GROUP BY [Category]

    INSERT INTO [Category] ( [IsSystem], [EntityTypeId], [Name], [Guid], [Order] )
    SELECT 
	    CAST(MAX(CAST([IsSystem] AS int)) AS bit)
	    , @SystemEmailEntity
	    , [Category]
	    , CASE [Category]
		    WHEN 'Workflow' THEN 'C7B9B5F1-9D90-485F-93E4-5D7D81EC2B12'
		    ELSE NEWID()
	      END
	    , 0
    FROM [SystemEmail]
    WHERE [Category] NOT IN ( 
	    SELECT DISTINCT [Name] 
	    FROM [Category]
	    WHERE [EntityTypeId] = @SystemEmailEntity
    )
    GROUP BY [Category]

    UPDATE T SET [CategoryId] = C.[Id]
    FROM [DefinedType] T
    INNER JOIN [Category] C
	    ON C.[EntityTypeId] = @DefinedTypeEntity
	    AND C.[Name] = T.Category

    UPDATE T SET [CategoryId] = C.[Id]
    FROM [SystemEmail] T
    INNER JOIN [Category] C
	    ON C.[EntityTypeId] = @SystemEmailEntity
	    AND C.[Name] = T.Category
" );

            DropColumn("dbo.DefinedType", "Category");
            DropColumn("dbo.SystemEmail", "Category");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.SystemEmail", "Category", c => c.String(maxLength: 100));
            AddColumn("dbo.DefinedType", "Category", c => c.String(maxLength: 100));

            Sql( @"
    DECLARE @DefinedTypeEntity int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedType' )
    DECLARE @SystemEmailEntity int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemEmail' )

    UPDATE T SET [Category] = C.[Name]
    FROM [DefinedType] T
    INNER JOIN [Category] C
	    ON C.[Id] = T.[CategoryId]

    UPDATE T SET [Category] = C.[Name]
    FROM [SystemEmail] T
    INNER JOIN [Category] C
	    ON C.[Id] = T.[CategoryId]

    DELETE [Category] 
    WHERE [EntityTypeId] IN ( @DefinedTypeEntity, @SystemEmailEntity )
" );

            DropForeignKey("dbo.SystemEmail", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.DefinedType", "CategoryId", "dbo.Category");
            DropIndex("dbo.SystemEmail", new[] { "CategoryId" });
            DropIndex("dbo.DefinedType", new[] { "CategoryId" });
            DropColumn("dbo.SystemEmail", "CategoryId");
            DropColumn("dbo.DefinedType", "CategoryId");
        }
    }
}
