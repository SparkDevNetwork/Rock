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
    public partial class UpdateScheduleToolboxActionsLabelLava : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @BlockTypeEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block');
DECLARE @ObsidianGrpSchedToolboxBlockTypeId INT = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '6554ADE3-2FC8-482B-BA63-2C3EABC11D32');

UPDATE	[Attribute]
SET	  [DefaultValue] = '<label>Actions</label>'
	, [DefaultPersistedTextValue] = NULL
	, [DefaultPersistedHtmlValue] = NULL
	, [DefaultPersistedCondensedTextValue] = NULL
	, [DefaultPersistedCondensedHtmlValue] = NULL
	, [IsDefaultPersistedValueDirty] = 1
WHERE	[Key] = 'ActionHeaderLavaTemplate'
	AND	[EntityTypeId] = @BlockTypeEntityTypeId
	AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
	AND	[EntityTypeQualifierValue] = @ObsidianGrpSchedToolboxBlockTypeId
	AND [DefaultValue] = '<h4>Actions</h4>';

UPDATE	[AttributeValue]
SET	  [Value] = '<label>Actions</label>'
	, [PersistedTextValue] = NULL
    , [PersistedHtmlValue] = NULL
    , [PersistedCondensedTextValue] = NULL
    , [PersistedCondensedHtmlValue] = NULL
    , [IsPersistedValueDirty] = 1
WHERE	[Value] = '<h4>Actions</h4>'
	AND	[AttributeId] IN
	(
		SELECT	[Id]
		FROM	[Attribute]
		WHERE	[Key] = 'ActionHeaderLavaTemplate'
			AND	[EntityTypeId] = @BlockTypeEntityTypeId
			AND	[EntityTypeQualifierColumn] = 'BlockTypeId'
			AND	[EntityTypeQualifierValue] = @ObsidianGrpSchedToolboxBlockTypeId
	);
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
