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
    /// </summary>
    public partial class SetupSystemOrphanedPagesPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @PageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '51457268-8F59-4E2A-8E22-2B57F2E3607B' );
DECLARE @BlockTypeId INT = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'CACB9D1A-A820-4587-986A-D66A69EE9948' ); -- Page Menu

-- Only add the Page Menu block if it is not already on the Orphaned Pages page in the Main zone.
IF @PageId IS NOT NULL AND @BlockTypeId IS NOT NULL
    AND NOT EXISTS (
        SELECT 1 FROM [Block]
        WHERE [PageId] = @PageId AND [BlockTypeId] = @BlockTypeId AND [Zone] = 'Main' )
BEGIN
    DECLARE @BlockGuid UNIQUEIDENTIFIER = '8599BD7E-B382-4E03-B5EC-A931FE8AF18F'; -- Page Menu block on Orphaned Pages page
    DECLARE @Order INT = ( SELECT ISNULL( MAX( [Order] ) + 1, 0 ) FROM [Block] WHERE [PageId] = @PageId AND [Zone] = 'Main' );

    INSERT INTO [Block] ( [IsSystem], [PageId], [BlockTypeId], [Zone], [Order], [Name], [OutputCacheDuration], [Guid] )
    VALUES ( 1, @PageId, @BlockTypeId, 'Main', @Order, 'Page Menu', 0, @BlockGuid );

    DECLARE @BlockId INT = ( SELECT [Id] FROM [Block] WHERE [Guid] = @BlockGuid );

    -- Template: only store a block value when it differs from the block type's default.
    DECLARE @TemplateAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '1322186A-862A-4CF1-B349-28ECB67229BA' );
    DECLARE @TemplateDesired NVARCHAR(MAX) = '{% include ''~~/Assets/Lava/PageListAsBlocks.lava'' %}';
    IF @TemplateAttributeId IS NOT NULL
        AND ISNULL( ( SELECT [DefaultValue] FROM [Attribute] WHERE [Id] = @TemplateAttributeId ), '' ) <> @TemplateDesired
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @TemplateAttributeId, @BlockId, @TemplateDesired, NEWID() );
    END

    -- Number of Levels: only store a block value when '1' differs from the block type's default.
    DECLARE @LevelsAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6C952052-BC79-41BA-8B88-AB8EA3E99648' );
    DECLARE @LevelsDesired NVARCHAR(MAX) = '1';
    IF @LevelsAttributeId IS NOT NULL
        AND ISNULL( ( SELECT [DefaultValue] FROM [Attribute] WHERE [Id] = @LevelsAttributeId ), '' ) <> @LevelsDesired
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
        VALUES ( 1, @LevelsAttributeId, @BlockId, @LevelsDesired, NEWID() );
    END
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
