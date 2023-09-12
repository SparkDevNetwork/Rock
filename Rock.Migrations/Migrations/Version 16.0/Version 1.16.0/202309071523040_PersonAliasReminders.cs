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
    public partial class PersonAliasReminders : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @EntityTypeId_Person INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7');
DECLARE @EntityTypeId_PersonAlias INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '90F5E87B-F0D5-4617-8AE9-EB57E673F36F');

-- Fix merged Person reminders.
UPDATE
	[Reminder]
SET
	[EntityId] = PA.[PersonId]
FROM
	[Reminder] R
		INNER JOIN	[PersonAlias] PA
			ON R.[EntityId] = PA.[AliasPersonId]
		LEFT JOIN	[ReminderType] RT
			ON R.[ReminderTypeId] = RT.[Id]
WHERE
		RT.[EntityTypeId] = @EntityTypeId_Person
	AND	PA.[PersonId] <> PA.[AliasPersonId];

-- Swap Person Ids for PersonAlias Ids.
UPDATE
	[Reminder]
SET
	[EntityId] = PA.[Id]
FROM
	[Reminder] R
		INNER JOIN	[PersonAlias] PA
			ON R.[EntityId] = PA.[PersonId]
		LEFT JOIN	[ReminderType] RT
			ON R.[ReminderTypeId] = RT.[Id]
WHERE
		RT.[EntityTypeId] = @EntityTypeId_Person
	AND	PA.[PersonId] = PA.[AliasPersonId];

-- Update ReminderType EntityTypeId.
UPDATE
	[ReminderType]
SET
	[EntityTypeId] = @EntityTypeId_PersonAlias
WHERE
	[EntityTypeId] = @EntityTypeId_Person;
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
