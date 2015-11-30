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
    public partial class GeoSpatialIndexes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Could take a minute or so on a large database
            Sql( @"
IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE NAME = 'IX_GeoPoint'
            AND object_id = OBJECT_ID('Location')
        )
BEGIN
    DROP INDEX [IX_GeoPoint] ON [dbo].[Location]
END

CREATE SPATIAL INDEX [IX_GeoPoint] ON [dbo].[Location]
(
	[GeoPoint]
)USING  GEOGRAPHY_GRID 


IF EXISTS (
        SELECT *
        FROM sys.indexes
        WHERE NAME = 'IX_GeoFence'
            AND object_id = OBJECT_ID('Location')
        )
BEGIN
    DROP INDEX [IX_GeoFence] ON [dbo].[Location]
END

CREATE SPATIAL INDEX [IX_GeoFence] ON [dbo].[Location]
(
	[GeoFence]
)USING  GEOGRAPHY_GRID 

" );

            // Migration Rollups

            //// MP: SiteMap -> PageMap
            // do a DELETE in case the new blocktype already got registered as a new block instead of a renamed block
            Sql( @"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/PageMap.ascx'" );
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/Cms/PageMap.ascx', [Name] = 'Page Map' WHERE [Guid] = '2700a1b8-bd1a-40f1-a660-476da86d0432'" );

            // JE: Allow Rock Admins to see the Data Integrity Report Category
            Sql( @"
  DECLARE @AdminGroupId int = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E')
  DECLARE @CategoryEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '1D68154E-EC76-44C8-9813-7736B27AECF9')
  DECLARE @DataIntegrityCategory int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'D738D12D-BC3B-47B0-8A90-F7924D137595')

  INSERT INTO [Auth] 
	([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid])
  VALUES
	(@CategoryEntityTypeId, @DataIntegrityCategory, 0, 'View', 'A', 0, @AdminGroupId, 'DAC8AC6E-B27B-7BA3-4129-7B04345E1911')
" );

            // JE: Pending group member system email
            Sql( MigrationSQL._201506242325188_GeoSpatialIndexes_AddSystemEmail );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
