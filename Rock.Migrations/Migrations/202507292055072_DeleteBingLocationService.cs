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
    public partial class DeleteBingLocationService : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @SmartyStreetsEntityTypeId INT;
DECLARE @BingEntityTypeId INT;

-- Get EntityTypeIds
SET @SmartyStreetsEntityTypeId = (
    SELECT [Id]
    FROM [EntityType]
    WHERE [Guid] = '4278E7EF-221B-45E6-B9C6-5D11884389EF'
);

SET @BingEntityTypeId = (
    SELECT [Id]
    FROM [EntityType]
    WHERE [Guid] = 'F9090BDE-83EB-4D93-B5D6-19016A830E2D'
);

-- Check if Bing Active is true, then update SmartyStreets
IF EXISTS (
    SELECT 1
    FROM [Attribute] a
    JOIN [AttributeValue] av ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeId] = @BingEntityTypeId
      AND a.[Key] = 'Active'
      AND av.[Value] = 'True'
)
BEGIN
    -- Set SmartyStreets Active = True
    UPDATE av
    SET av.[Value] = 'True',
        av.[PersistedTextValue] = NULL,
        av.[PersistedHtmlValue] = NULL,
        av.[PersistedCondensedTextValue] = NULL,
        av.[PersistedCondensedHtmlValue] = NULL,
        av.[IsPersistedValueDirty] = 1
    FROM [Attribute] a
    JOIN [AttributeValue] av ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeId] = @SmartyStreetsEntityTypeId
      AND a.[Key] = 'Active';

    -- Set SmartyStreets UseManagedAPIKey = True
    UPDATE av
    SET av.[Value] = 'True',
        av.[PersistedTextValue] = NULL,
        av.[PersistedHtmlValue] = NULL,
        av.[PersistedCondensedTextValue] = NULL,
        av.[PersistedCondensedHtmlValue] = NULL,
        av.[IsPersistedValueDirty] = 1
    FROM [Attribute] a
    JOIN [AttributeValue] av ON a.[Id] = av.[AttributeId]
    WHERE a.[EntityTypeId] = @SmartyStreetsEntityTypeId
      AND a.[Key] = 'UseManagedAPIKey';
END

-- Delete AttributeValues for Bing
DELETE av
FROM [AttributeValue] av
JOIN [Attribute] a ON av.[AttributeId] = a.[Id]
WHERE a.[EntityTypeId] = @BingEntityTypeId;

-- Delete Attributes for Bing
DELETE FROM [Attribute]
WHERE [EntityTypeId] = @BingEntityTypeId;

-- Delete Entity Type for Bing
DELETE FROM [EntityType]
WHERE [Id] = @BingEntityTypeId;
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
