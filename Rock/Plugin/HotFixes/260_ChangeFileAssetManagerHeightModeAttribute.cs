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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 260, "17.3" )]
    public class ChangeFileAssetManagerHeightModeAttribute : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            const string fileAssetManagerBlockTypeGuid = "535500A7-967F-4DA3-8FCA-CB844203CB3D";
            const string heightModeAttributeGuid = "67ECB409-F5C5-4487-A60B-FD572B99D95B";

            // Attribute for BlockType
            //   BlockType: File Asset Manager
            //   Category: CMS
            //   Attribute: Height Mode
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( fileAssetManagerBlockTypeGuid, SystemGuid.FieldType.SINGLE_SELECT, "Height Mode", "HeightMode", "Height Mode", @"Static lets you set a CSS height below to determine the height of the block. Flexible will grow with the content. Full Worksurface is designed to fill up a full worksurface page layout.", 2, @"static", heightModeAttributeGuid );


            Sql( @"
DECLARE @BlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')
DECLARE @BlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE Guid = '535500A7-967F-4DA3-8FCA-CB844203CB3D')

DECLARE @HeightModeAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'HeightMode' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @IsStaticHeightAttributeId INT = (SELECT [Id] FROM [Attribute] 
WHERE [KEY] = 'IsStaticHeight' 
AND [EntityTypeId] = @BlockEntityTypeId 
AND [EntityTypeQualifierColumn] = 'BlockTypeId' 
AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR))

DECLARE @BlockId INT
DECLARE @IsStaticHeight VARCHAR(50)
DECLARE @TheValue VARCHAR(50)

DECLARE block_cursor CURSOR FOR
SELECT [Id] FROM [Block] WHERE BlockTypeId = @BlockTypeId

OPEN block_cursor
FETCH NEXT FROM block_cursor INTO @BlockId

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @IsStaticHeight = [Value] FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @IsStaticHeightAttributeId

    SET @TheValue = CASE
        WHEN @IsStaticHeight = 'True' THEN 'static'
        WHEN @IsStaticHeight = 'False' THEN 'flexible'
        ELSE 'static'
    END

    IF EXISTS (SELECT 1 FROM [AttributeValue] WHERE [EntityId] = @BlockId AND [AttributeId] = @HeightModeAttributeId)  
    BEGIN  
        UPDATE [AttributeValue]   
        SET [Value] = @TheValue 
        WHERE [EntityId] = @BlockId AND [AttributeId] = @HeightModeAttributeId;  
    END  
    ELSE  
    BEGIN  
        INSERT INTO [AttributeValue] (
            [IsSystem],
            [AttributeId],
            [EntityId],
            [Value],
            [Guid])
        VALUES(
            1,
            @HeightModeAttributeId,
            @BlockId,
            @TheValue,
            NEWID()) 
    END

    FETCH NEXT FROM block_cursor INTO @BlockId
END

CLOSE block_cursor
DEALLOCATE block_cursor
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