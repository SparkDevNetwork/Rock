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
    public partial class ReplaceFontAwesomeWithTablerIcons : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
-- Initial Clean-up

UPDATE [GroupType]
SET [IconCssClass] = 
  CASE 
    WHEN LEFT([IconCssClass], 8) = 'icon-fw ' 
    THEN RIGHT([IconCssClass], LEN([IconCssClass]) - 8) 
    ELSE [IconCssClass] 
  END

UPDATE [WorkflowType]
SET [IconCssClass] = 
  CASE 
    WHEN LEFT([IconCssClass], 8) = 'icon-fw ' 
    THEN RIGHT([IconCssClass], LEN([IconCssClass]) - 8) 
    ELSE [IconCssClass] 
  END

UPDATE [Category]
SET [IconCssClass] = 
  CASE 
    WHEN LEFT([IconCssClass], 8) = 'icon-fw ' 
    THEN RIGHT([IconCssClass], LEN([IconCssClass]) - 8) 
    ELSE [IconCssClass] 
  END

DECLARE @sql NVARCHAR(MAX) = N'';
DECLARE @tableName NVARCHAR(256);
DECLARE @schemaName NVARCHAR(256);

-- Cursor to find all user tables with a column named 'IconCssClass'
DECLARE icon_cursor CURSOR FOR
SELECT 
    t.name AS TableName,
    s.name AS SchemaName
FROM 
    sys.columns c
JOIN 
    sys.tables t ON c.object_id = t.object_id
JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    c.name = 'IconCssClass';

OPEN icon_cursor;
FETCH NEXT FROM icon_cursor INTO @tableName, @schemaName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build the dynamic update SQL
    SET @sql += '
UPDATE [' + @schemaName + '].[' + @tableName + ']
SET IconCssClass = t.TablerFull
FROM [' + @schemaName + '].[' + @tableName + '] AS target
JOIN [__IconTransition] AS t
    ON target.IconCssClass = t.FontAwesomeFull
WHERE t.TablerFull IS NOT NULL;
';

    FETCH NEXT FROM icon_cursor INTO @tableName, @schemaName;
END

CLOSE icon_cursor;
DEALLOCATE icon_cursor;

-- Execute dynamic SQL
EXEC sp_executesql @sql;
" );
            // And now update Attributes Values and Attribute Default Values
            Sql( @"
-- Updates Attribute Values
UPDATE av
SET av.[Value] = t.[TablerFull]
FROM [dbo].[AttributeValue] av
JOIN [dbo].[__IconTransition] t
    ON av.[ValueChecksum] = CHECKSUM(t.[FontAwesomeFull])
   AND av.[Value] = t.[FontAwesomeFull]
WHERE t.[TablerFull] IS NOT NULL;

-- Update Attribute Default Values
UPDATE a
SET a.[DefaultValue] = t.[TablerFull]
FROM [dbo].[Attribute] a
JOIN [dbo].[__IconTransition] t
    ON a.[DefaultValueChecksum] = CHECKSUM(t.[FontAwesomeFull])
   AND a.[DefaultValue] = t.[FontAwesomeFull]
WHERE t.[TablerFull] IS NOT NULL;
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
