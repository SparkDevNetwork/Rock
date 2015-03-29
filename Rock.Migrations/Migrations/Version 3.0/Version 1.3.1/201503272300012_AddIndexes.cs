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
    public partial class AddIndexes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person");
            AddForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person", "Id");

            CreateIndex( "dbo.Audit", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.Auth", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.Following", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.History", new string[] { "EntityTypeId", "EntityId" } );
            CreateIndex( "dbo.EntitySetItem", new string[] { "EntitySetId", "EntityId" } );
            CreateIndex( "dbo.Note", new string[] { "NoteTypeId", "EntityId" } );
            CreateIndex( "dbo.TaggedItem", new string[] { "TagId", "EntityGuid" } );
            CreateIndex( "dbo.PersonDuplicate", "DuplicatePersonAliasId" );

            Sql( @"
	/*
	<doc>
		<summary>
 			This procedure merges the data from the non-primary person to the primary person.  It
			is used when merging people in Rock and should never be used outside of that process. 
		</summary>

		<returns>
		</returns>
		<param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
		<param name=""New Id"" datatype=""int"">The person id of the primary Person being merged</param>
		<remarks>	
			Uses the following constants:
				* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
				* Group Role - Known Relationship Owner: 7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42
				* Group Role - Implied Relationship Owner: CB9A0E14-6FCF-4C07-A49A-D7873F45E196
		</remarks>
		<code>
		</code>
	</doc>
	*/

	ALTER PROCEDURE [dbo].[spCrm_PersonMerge]
		@OldId int
		, @NewId int

	AS
	BEGIN

		DECLARE @OldGuid uniqueidentifier
		DECLARE @NewGuid uniqueidentifier

		SET @OldGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @OldId )
		SET @NewGuid = ( SELECT [Guid] FROM [Person] WHERE [Id] = @NewId )

		IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
		BEGIN

			DECLARE @PersonEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )
			DECLARE @PersonFieldTypeId INT = ( SELECT [Id] FROM [FieldType] WHERE [Class] = 'Rock.Field.Types.PersonFieldType' )

			-- Move/Update Known Relationships
			EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42'

			-- Move/Update Implied Relationships
			EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196'

			-- Group Member
			-----------------------------------------------------------------------------------------------
			-- Update any group members associated to old person to the new person where the new is not 
			-- already in the group with the same role
			UPDATE GMO
				SET [PersonId] = @NewId
			FROM [GroupMember] GMO
				INNER JOIN [GroupTypeRole] GTR
					ON GTR.[Id] = GMO.[GroupRoleId]
				LEFT OUTER JOIN [GroupMember] GMN
					ON GMN.[GroupId] = GMO.[GroupId]
					AND GMN.[PersonId] = @NewId
					AND (GTR.[MaxCount] <= 1 OR GMN.[GroupRoleId] = GMO.[GroupRoleId])
			WHERE GMO.[PersonId] = @OldId
				AND GMN.[Id] IS NULL

			-- Delete any group members not updated (already existed with new id)
			DELETE [GroupMember]
			WHERE [PersonId] = @OldId

			-- User Login
			-----------------------------------------------------------------------------------------------
			-- Update any user logins associated with old id to be associated with primary person
			UPDATE [UserLogin]
			SET [PersonId] = @NewId
			WHERE [PersonId] = @OldId

			-- Attribute
			-----------------------------------------------------------------------------------------------
			-- Update any attribute value that is associated to the old person to be associated to the new 
			-- person. The merge block should update most attributes, but if old person has any that new
            -- person does, update them to new person
			UPDATE V
				SET [EntityId] = @NewId
			FROM [Attribute] A
				INNER JOIN [Attributevalue] V
					ON V.[AttributeId] = A.[Id]
					AND V.[EntityId] = @OldId
				LEFT OUTER JOIN [Attributevalue] NV
					ON NV.[AttributeId] = A.[Id]
					AND NV.[EntityId] = @NewId
			WHERE A.[EntityTypeId] = @PersonEntityTypeId
				AND NV.[Id] IS NULL

			DELETE V
			FROM [Attribute] A
				INNER JOIN [Attributevalue] V
					ON V.[AttributeId] = A.[Id]
					AND V.[EntityId] = @OldId
			WHERE A.[EntityTypeId] = @PersonEntityTypeId

			-- Audit
			-----------------------------------------------------------------------------------------------
			-- Update any audit records that were associated to the old person to be associated to the new person
			UPDATE [Audit] SET [EntityId] = @NewId
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Auth
			-----------------------------------------------------------------------------------------------
			-- Update any auth records that were associated to the old person to be associated to the new person
			-- There is currently not any UI to set security associated to person, so really shouldn't be
			-- any values here to update
			UPDATE A
				SET [EntityId] = @NewId
			FROM [Auth] A
				LEFT OUTER JOIN [Auth] NA
					ON NA.[EntityTypeId] = A.[EntityTypeId]
					AND NA.[EntityId] = @NewId
					AND NA.[Action] = A.[Action]
			WHERE A.[EntityTypeId] = @PersonEntityTypeId
				AND A.[EntityId] = @OldId
				AND NA.[Id] IS NULL

			DELETE [Auth]
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Entity Set
			-----------------------------------------------------------------------------------------------
			-- Update any entity set items that are associated to the old person to be associated to the new 
			-- person. 
			UPDATE I
				SET [EntityId] = @NewId
			FROM [EntitySet] S
				INNER JOIN [EntitySetItem] I
					ON I.[EntitySetId] = S.[Id]
					AND I.[EntityId] = @OldId
				LEFT OUTER JOIN [EntitySetItem] NI
					ON NI.[EntitySetId] = S.[Id]
					AND NI.[EntityId] = @NewId
			WHERE S.[EntityTypeId] = @PersonEntityTypeId
				AND NI.[Id] IS NULL

			DELETE I
			FROM [EntitySet] S
				INNER JOIN [EntitySetItem] I
					ON I.[EntitySetId] = S.[Id]
					AND I.[EntityId] = @OldId
			WHERE S.[EntityTypeId] = @PersonEntityTypeId

			-- Following
			-----------------------------------------------------------------------------------------------
			-- Update any followings that are associated to the old person to be associated to the new 
			-- person. 
			UPDATE F
				SET [EntityId] = @NewId
			FROM [Following] F
				LEFT OUTER JOIN [Following] NF
					ON NF.[EntityTypeId] = F.[EntityTypeId]
					AND NF.[EntityId] = @NewId
					AND NF.[PersonAliasId] = F.[PersonAliasId]
			WHERE F.[EntityTypeId] = @PersonEntityTypeId
				AND F.[EntityId] = @OldId
				AND NF.[Id] IS NULL

			DELETE [Following]
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- History
			-----------------------------------------------------------------------------------------------
			-- Update any history that is associated to the old person to be associated to the new person
			UPDATE [History] SET [EntityId] = @NewId
			WHERE [EntityTypeId] = @PersonEntityTypeId
			AND [EntityId] = @OldId

			-- Note
			-----------------------------------------------------------------------------------------------
			-- Update any note that is associated to the old person to be associated to the new person
			UPDATE N
				SET [EntityId] = @NewId
			FROM [NoteType] NT
				INNER JOIN [Note] N
					ON N.[NoteTypeId] = NT.[Id]
					AND N.[EntityId] = @OldId
			WHERE NT.[EntityTypeId] = @PersonEntityTypeId
		
			-- Tags
			-----------------------------------------------------------------------------------------------
			-- Update any tags associated to the old person to be associated to the new person as long as 
			-- same tag does not already exist for new person
			UPDATE TIO
				SET [EntityGuid] = @NewGuid
			FROM [Tag] T
				INNER JOIN [TaggedItem] TIO
					ON TIO.[TagId] = T.[Id]
					AND TIO.[EntityGuid] = @OldGuid
				LEFT OUTER JOIN [TaggedItem] TIN
					ON TIN.[TagId] = T.[Id]
					AND TIN.[EntityGuid] = @NewGuid
			WHERE T.[EntityTypeId] = @PersonEntityTypeId
				AND TIN.[Id] IS NULL

			-- Delete any tagged items still associated with old person (new person had same tag)
			DELETE TIO
			FROM [Tag] T
				INNER JOIN [TaggedItem] TIO
					ON TIO.[TagId] = T.[Id]
					AND TIO.[EntityGuid] = @OldGuid
			WHERE T.[EntityTypeId] = @PersonEntityTypeId

			-- Update the Person Alias pointer
			UPDATE [PersonAlias]
			SET [PersonId] = @NewId
			WHERE [PersonId] = @OldId

			-- Remaining Tables
			-----------------------------------------------------------------------------------------------
			-- Update any column on any table that has a foreign key relationship to the Person table's Id
			-- column ( Core tables are handled explicitely above, so this should only include custom tables )

			DECLARE @Sql varchar(max)

			DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
			SELECT 
				' UPDATE ' + tso.name +
				' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
				' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
			FROM sys.foreign_key_columns kc
				INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
				INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
				INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
				INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
				INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
			WHERE so.name = 'Person'
				AND rac.name = 'Id'
				AND tso.name NOT IN (
					 'GroupMember'
					,'PhoneNumber'
					,'UserLogin'
					,'PersonAlias'
				)

			OPEN ForeignKeyCursor

			FETCH NEXT
			FROM ForeignKeyCursor
			INTO @Sql

			WHILE (@@FETCH_STATUS <> -1)
			BEGIN

				IF (@@FETCH_STATUS = 0)
				BEGIN

					EXEC(@Sql)
			
				END
		
				FETCH NEXT
				FROM ForeignKeyCursor
				INTO @Sql

			END

			CLOSE ForeignKeyCursor
			DEALLOCATE ForeignKeyCursor


			-- Person
			-----------------------------------------------------------------------------------------------
			-- Delete the old person record.  By this time it should not have any relationships 
			-- with other tables 

			DELETE Person
			WHERE [Id] = @OldId

		END

	END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.Audit", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.Auth", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.Following", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.History", new string[] { "EntityTypeId", "EntityId" } );
            DropIndex( "dbo.EntitySetItem", new string[] { "EntitySetId", "EntityId" } );
            DropIndex( "dbo.Note", new string[] { "NoteTypeId", "EntityId" } );
            DropIndex( "dbo.TaggedItem", new string[] { "TagId", "EntityGuid" } );
            DropIndex( "dbo.PersonDuplicate", "DuplicatePersonAliasId" );

            DropForeignKey( "dbo.PersonAlias", "PersonId", "dbo.Person" );
            AddForeignKey("dbo.PersonAlias", "PersonId", "dbo.Person", "Id", cascadeDelete: true);
        }
    }
}
