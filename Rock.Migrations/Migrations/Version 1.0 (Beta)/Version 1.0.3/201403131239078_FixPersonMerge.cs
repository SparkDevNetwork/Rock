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
    public partial class FixPersonMerge : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add the GroupList's DisplaySystemColumn attribute
            UpdateBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display System Column", "DisplaySystemColumn", "", "Should the System column be displayed?", 6, @"True", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F" ); 
            AddBlockAttributeValue( "1CBE10C7-5E64-4385-BEE3-81DCA43DC47F", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"False" );

            // Update the QuickLinks content
            Sql( @"
    UPDATE [dbo].[HtmlContent]
        SET [Content] = N'<ul class=""list-group"">
     <li class=""list-group-item"">
      <a href=""http://www.rockrms.com/"">Rock RMS Website</a></li>
     <li class=""list-group-item"">
      <a href=""./page/1"">External Website</a></li>
    </ul>'
    WHERE [Guid] = N'007ea905-d5d3-4dc5-ad0b-2c1e3935e452' 
        AND [ModifiedByPersonAliasId] IS NULL
" );


            Sql( @"
/*
<doc>
	<summary>
 		This procedure merges relationship data for two people being merged.  It is called from the
		from the [dbo].[spCrm_PersonMerge] procedure to merge Known Relationships and Implied
		Relationships.  It is used when merging people in Rock and should never be used outside 
		of that process. 
	</summary>

	<returns></returns>
	<param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
	<param name=""New Id"" datatype=""int"">The person id of the rimary Person being merged</param>
	<param name=""Owner Role Guid"" datatype=""uniqueidentifier"">The role of the owner in the relationship group</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42' -- Known Relationships
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196' -- Implied Relationships
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].[spCrm_PersonMergeRelationships]
	@OldId int
	, @NewId int
	, @OwnerRoleGuid uniqueidentifier

AS
BEGIN

	DECLARE @GroupTypeId int
	DECLARE @GroupRoleId int

	DECLARE @NewMemberId int
	DECLARE @NewGroupId int

	DECLARE @OldMemberId int
	DECLARE @OldGroupId int
	
	-- Find the old person's owner group member record
	SELECT TOP 1
		@OldMemberId = GM.[Id],
		@OldGroupId = GM.[GroupId],
		@GroupTypeId = GTR.[GroupTypeId],
		@GroupRoleId = GTR.[Id]
	FROM [GroupMember] GM
		INNER JOIN [GroupTypeRole] GTR ON GTR.[Id] = GM.GroupRoleId AND GTR.[Guid] = @OwnerRoleGuid
	WHERE GM.[PersonId] = @OldId
		
	IF @GroupTypeId IS NOT NULL
	BEGIN

		-- Find the new person's owner group member record
		SELECT TOP 1
			@NewMemberId = GM.[Id],
			@NewGroupId = GM.[GroupId]
		FROM [GroupMember] GM
			INNER JOIN [GroupTypeRole] GTR ON GTR.[Id] = GM.GroupRoleId AND GTR.[Guid] = @OwnerRoleGuid
		WHERE GM.[PersonId] = @NewId
	
		IF @NewGroupId IS NULL 
		BEGIN

			-- If new person doesn't have a relation group member record, update
			-- the old person to use the new person id (and be done)
			UPDATE [GroupMember]
			SET [PersonId] = @NewId
			WHERE [Id] = @OldMemberID

		END
		ELSE
		BEGIN

			-- Move the relationships from old person's relationship group
			-- to new person's group when new person doesn't already have
			-- same relationship
			UPDATE GMO
				SET [GroupId] = @NewGroupId
			FROM [GroupMember] GMO
				LEFT OUTER JOIN [GroupMember] GMN ON GMN.[GroupId] = @NewGroupId 
					AND GMN.[PersonId] = GMO.[PersonId] AND GMN.[GroupRoleId] = GMO.[GroupRoleId]
			WHERE GMO.[GroupId] = @OldGroupId
				AND GMO.[Id] <> @OldMemberId
				AND GMN.[Id] IS NULL

			-- Delete the old person's relationship group if it still exists
			DELETE [Group] 
			WHERE [Id] = @OldGroupId	
	
		END

	END

END
" );

            Sql( @"
/*
<doc>
	<summary>
 		This procedure merges the data from the non-primary person to the primary to the primary person.  It
		is used when merging people in Rock and should never be used outside of that process. 
	</summary>

	<returns>
	</returns>
	<param name=""Old Id"" datatype=""int"">The person id of the non-primary Person being merged</param>
	<param name=""New Id"" datatype=""int"">The person id of the rimary Person being merged</param>
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

		-- Authorization
		-----------------------------------------------------------------------------------------------
		-- Update any authorizations associated to old person that do not already have a matching 
		-- authorization for the new person
		UPDATE AO
			SET [PersonId] = @NewId
		FROM [Auth] AO
			LEFT OUTER JOIN [Auth] AN
				ON AN.[PersonId] = @NewId
				AND AN.[EntityTypeId] = AO.[EntityTypeId]
				AND AN.[EntityId] = AO.[EntityId]
				AND AN.[Action] = AO.[Action]
				AND AN.[AllowOrDeny] = AO.[AllowOrDeny]
				AND AN.[SpecialRole] = AO.[SpecialRole]
		WHERE AO.[PersonId] = @OldId
			AND AN.[Id] IS NULL

		-- Delete any authorizations not updated to new person
		DELETE [Auth]
		WHERE [PersonId] = @OldId

		-- Category
		-----------------------------------------------------------------------------------------------
		-- Currently UI does not allow categorizing people, but if it does in the future, would need 
		-- to add script to handle merge


		-- Communication Recipient
		-----------------------------------------------------------------------------------------------
		-- Update any communication recipients associated to old person to the new person where the new
		-- person does not already have the recipient record
		UPDATE CRO
			SET [PersonId] = @NewId
		FROM [CommunicationRecipient] CRO
			LEFT OUTER JOIN [CommunicationRecipient] CRN
				ON CRN.[CommunicationId] = CRO.[CommunicationId]
				AND CRN.[PersonId] = @NewId
		WHERE CRO.[PersonId] = @OldId
			AND CRN.[Id] IS NULL

		-- Delete any remaining recipents that were not updated
		DELETE [CommunicationRecipient]
		WHERE [PersonId] = @OldId

		SELECT DISTINCT G.[ID]
		INTO #FamilyIDs
		FROM [GroupMember] GM
			INNER JOIN [Group] G ON G.[Id] = GM.[GroupId]
			INNER JOIN [GroupType] GT ON GT.[Id] = G.[GroupTypeId]
		WHERE GM.[PersonId] = @OldId
			AND GT.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'

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


		-- History
		-----------------------------------------------------------------------------------------------
		-- Update any history that is associated to the old person to be associated to the new person
		UPDATE [History] SET [EntityId] = @NewId
		WHERE [EntityTypeId] = @PersonEntityTypeId

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

		-- If old person and new person have tags with the same name for the same entity type,
		-- update the old person's tagged items to use the new person's tag
		UPDATE TIO
			SET [TagId] = TIN.[Id]
		FROM [Tag] T
			INNER JOIN [Tag] TN
				ON TN.[EntityTypeId] = T.[EntityTypeId]
				AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				AND TN.[Name] = T.[Name]
				AND TN.[OwnerId] = @NewId
			INNER JOIN [TaggedItem] TIO
				ON TIO.[TagId] = T.[Id]
			LEFT OUTER JOIN [TaggedItem] TIN
				ON TIN.[TagId] = TN.[Id]
		WHERE T.[OwnerId] = @OldId
			AND TIN.[Id] IS NULL

		-- Delete any of the old person's tags that have the same name and are associated to same 
		-- entity type as a tag used bo the new person
		DELETE T
		FROM [Tag] T
			INNER JOIN [Tag] TN
				ON TN.[EntityTypeId] = T.[EntityTypeId]
				AND TN.[EntityTypeQualifierColumn] = T.[EntityTypeQualifierColumn]
				AND TN.[EntityTypeQualifierValue] = T.[EntityTypeQualifierValue]
				AND TN.[Name] = T.[Name]
				AND TN.[OwnerId] = @NewId
		WHERE T.[OwnerId] = @OldId


		-- Remaining Tables
		-----------------------------------------------------------------------------------------------
		-- Update any column on any table that has a foreign key relationship to the Person table's Id
		-- column  

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
					'Auth'
				,'CommunicationRecipient'
				,'GroupMember'
				,'PhoneNumber'
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
        }
    }
}
