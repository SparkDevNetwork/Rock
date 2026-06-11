/*
<doc>
	<summary>
		This procedure merges the data from the non-primary person to the primary person.  It
		is used when merging people in Rock and should never be used outside of that process. 
	</summary>

	<returns>
	</returns>
	<param name="Old Id" datatype="int">The person id of the non-primary Person being merged</param>
	<param name="New Id" datatype="int">The person id of the primary Person being merged</param>
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
	
CREATE PROCEDURE [dbo].[spCrm_PersonMerge]
	  @OldId int
	, @NewId int

AS
BEGIN

	DECLARE @OldGuid uniqueidentifier
	DECLARE @NewGuid uniqueidentifier
	DECLARE @GroupMemberStatusInactive INT = 0
	DECLARE @GroupMemberStatusActive INT = 1
	DECLARE @GroupMemberStatusPending INT = 2

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
		DECLARE @LessActiveGroupMembersIdsToDelete TABLE (id INT);
		DECLARE @GroupMembersIdsToArchive TABLE (id INT);

		-- In the case when the old person and the new person are in the same group with the same role,
		-- delete the groupmember record for the new person if it is 'less active' (Active > Pending > Inactive) then the old person. 
		-- That will get that record out of the way so that the 'old' group member record can be assigned to the new person
		-- We'll also consider 'less active' as having fewer LearningClassActivityCompletion records for the same class then the old person.
		INSERT INTO @LessActiveGroupMembersIdsToDelete
		SELECT gmn.id
		FROM [GroupMember] GMO
		INNER JOIN [GroupTypeRole] GTR ON GTR.[Id] = GMO.[GroupRoleId]
		INNER JOIN [GroupMember] GMN ON GMN.[GroupId] = GMO.[GroupId]
			AND GMN.[PersonId] = @NewId
			AND (
				GTR.[MaxCount] <= 1
				OR GMN.[GroupRoleId] = GMO.[GroupRoleId]
				)
		-- Count completions for the "old" group member in this class
		OUTER APPLY (
			SELECT COUNT(lcac.[Id]) AS OldCompletionCount
			FROM [dbo].[LearningParticipant] AS lp
			LEFT JOIN [dbo].[LearningClassActivityCompletion] AS lcac
				ON lcac.[StudentId] = lp.[Id]
			WHERE lp.[Id] = gmo.[Id]              -- LP is the TPT extension of GroupMember
				AND lp.[LearningClassId] = gmo.[GroupId] -- same class as the group
		) AS oldc
		-- Count completions for the "new" group member in this class
		OUTER APPLY (
			SELECT COUNT(lcac2.[Id]) AS NewCompletionCount
			FROM [dbo].[LearningParticipant] AS lp2
			LEFT JOIN [dbo].[LearningClassActivityCompletion] AS lcac2
				ON lcac2.[StudentId] = lp2.[Id]
			WHERE lp2.[Id] = gmn.[Id]
				AND lp2.[LearningClassId] = gmn.[GroupId]
		) AS newc
		WHERE GMO.[PersonId] = @OldId
			AND (
					(
					-- old person' group member status is Active but new person's status is not, so delete the new person's groupmember record so that we can set the old record to the new person id
					gmn.GroupMemberStatus != @GroupMemberStatusActive
					AND gmo.GroupMemberStatus = @GroupMemberStatusActive
					)
				OR (
					-- old person's group member status is Pending but new person's group member status is Inactive, so delete the new person's groupmember record so that we can set the old record to the new person id
					gmn.GroupMemberStatus = @GroupMemberStatusInactive
					AND gmo.GroupMemberStatus = @GroupMemberStatusPending
					)
				 OR (
					  -- the old person has more LearningClassActivityCompletions for the class than the new person, so delete the new person's groupmember record.
					  ISNULL(oldc.[OldCompletionCount], 0) > ISNULL(newc.NewCompletionCount, 0)
					)
				)

		/**********************************************************************************************
		 * Handle LearningClassActivityCompletion and LearningParticipant records
		 **********************************************************************************************/
		DELETE
		FROM [LearningClassActivityCompletion]
		WHERE StudentId IN (
				SELECT [Id]
				FROM @LessActiveGroupMembersIdsToDelete
		)

		DELETE
		FROM LearningParticipant
		WHERE Id IN (
				SELECT [Id]
				FROM @LessActiveGroupMembersIdsToDelete
				)

		/**********************************************************************************************
		 * Handle RegistrationRegistrant records
		 **********************************************************************************************/
		-- NULL out RegistrationRegistrant Records for the @LessActiveGroupMembersIdsToDelete
		UPDATE [RegistrationRegistrant]
		SET [GroupMemberId] = NULL
		WHERE [GroupMemberId] IN (
				SELECT [Id]
				FROM @LessActiveGroupMembersIdsToDelete
				)

		/*********************************************************************************************
		 * Handle GroupMemberAssignment records
		 **********************************************************************************************/
			DECLARE @Pairs TABLE (
				OldGMId           INT NOT NULL,
				KeepGMId          INT NOT NULL,
				GroupId           INT NOT NULL,
				FinalWinnerGMId   INT NOT NULL,
				FinalLoserGMId    INT NOT NULL,
				PRIMARY KEY (FinalLoserGMId, GroupId)
			);

			-- Because our initial 'Keep' record might be in the @LessActiveGroupMembersIdsToDelete
			-- list, we need to adjust which one we'll keep based on that. 
			INSERT INTO @Pairs (OldGMId, KeepGMId, GroupId, FinalWinnerGMId, FinalLoserGMId)
			SELECT
				GMOld.Id,
				GMKeep.Id,
				GMOld.GroupId,
				CASE WHEN LA.Id IS NOT NULL THEN GMOld.Id ELSE GMKeep.Id END,
				CASE WHEN LA.Id IS NOT NULL THEN GMKeep.Id ELSE GMOld.Id END
			FROM dbo.GroupMember AS GMOld
			JOIN dbo.GroupMember AS GMKeep ON GMKeep.GroupId = GMOld.GroupId
			 AND GMKeep.GroupRoleId = GMOld.GroupRoleId
			 AND GMKeep.PersonId    = @NewId
			LEFT JOIN @LessActiveGroupMembersIdsToDelete AS LA ON LA.Id = GMKeep.Id
			WHERE GMOld.PersonId = @OldId;

			-----------------------------------------------------------------------------
			--   @Assigns: losing assignments and their projected winner.
			-----------------------------------------------------------------------------
			DECLARE @Assigns TABLE (
				GroupMemberAssignmentId INT NOT NULL PRIMARY KEY,
				GroupId                 INT NOT NULL,
				LocationId              INT NULL,
				ScheduleId              INT NULL,
				NewGroupMemberId        INT NOT NULL
			);

			INSERT INTO @Assigns (GroupMemberAssignmentId, GroupId, LocationId, ScheduleId, NewGroupMemberId)
			SELECT
				GMA.Id,
				GMA.GroupId,
				GMA.LocationId,
				GMA.ScheduleId,
				P.FinalWinnerGMId
			FROM dbo.GroupMemberAssignment AS GMA
			JOIN @Pairs AS P ON GMA.GroupMemberId = P.FinalLoserGMId
			   AND GMA.GroupId = P.GroupId;

			/*
				12/2/2025 - N.A.

				Why does this appear to be so complicated?

				Additional cleanup steps are required when updating GroupMember assignments due to 
				the IX_GroupMemberIdLocationIdScheduleId index since you cannot have two 
				GroupMemberAssignments for the same groupmember, location and schedule.

				Specifically:
				1) A 'losing' row might match an existing 'winner' row after remapping, violating
				   the unique index. Therefore, the losing row must be deleted before the update.
			*/

			-- (1) DELETE losing assignments that would collide with EXISTING winner rows.
			DELETE GMA
			FROM dbo.GroupMemberAssignment AS GMA
			JOIN @Assigns AS A ON A.GroupMemberAssignmentId = GMA.Id
			WHERE EXISTS (
				SELECT 1
				FROM dbo.GroupMemberAssignment AS W
				WHERE W.GroupMemberId = A.NewGroupMemberId
				   AND W.GroupId       = A.GroupId
				   AND ( (W.LocationId = A.LocationId) OR (W.LocationId IS NULL AND A.LocationId IS NULL) )
				   AND ( (W.ScheduleId = A.ScheduleId) OR (W.ScheduleId IS NULL AND A.ScheduleId IS NULL) )
			);

			-- Update assignments from Loser -> Winner (@Pairs / @Assigns are scoped by GroupId)
			UPDATE GMA
			SET GMA.GroupMemberId = A.NewGroupMemberId
			FROM dbo.GroupMemberAssignment AS GMA
			JOIN @Assigns AS A ON A.GroupMemberAssignmentId = GMA.Id;

		/**********************************************************************************************
		 * Handle GroupMemberHistory records
		 **********************************************************************************************/
		-- If there is GroupMemberHistory, we can't delete, so create a list of GroupMemberIds that we'll archive instead of delete
		INSERT INTO @GroupMembersIdsToArchive 
			SELECT [Id]	FROM @LessActiveGroupMembersIdsToDelete WHERE Id IN (SELECT GroupMemberId FROM GroupMemberHistorical)

		DELETE FROM @LessActiveGroupMembersIdsToDelete 
			WHERE Id IN (SELECT Id FROM @GroupMembersIdsToArchive)

		/**********************************************************************************************
		 * Handle the final surviving GroupMember records
		 **********************************************************************************************/
		-- Delete the @LessActiveGroupMembersIdsToDelete for any GroupMember records that don't have GroupMemberHistory
		DELETE
		FROM GroupMember
		WHERE Id IN (
				SELECT [Id]
				FROM @LessActiveGroupMembersIdsToDelete
				)
		
		-- Update any group members associated to old person to the new person where the new is not 
		-- already in the group with the same role (except for groupmember records that we are going to archive)
		UPDATE GMO
			SET [PersonId] = @NewId
		FROM [GroupMember] GMO
			INNER JOIN [GroupTypeRole] GTR
				ON GTR.[Id] = GMO.[GroupRoleId]
			LEFT OUTER JOIN [GroupMember] GMN
				ON GMN.[GroupId] = GMO.[GroupId]
				AND GMN.[PersonId] = @NewId
				AND GMN.[IsArchived] = 0
				AND (GTR.[MaxCount] <= 1 OR GMN.[GroupRoleId] = GMO.[GroupRoleId])
		WHERE GMO.[PersonId] = @OldId
			AND GMN.[Id] IS NULL
			and GMO.Id NOT IN (SELECT [Id] FROM @GroupMembersIdsToArchive)

		/**********************************************************************************************
		 * Handle RegistrationRegistrant records
		 **********************************************************************************************/
		-- Update any registrant groups that point to a group member about to be deleted 
		UPDATE [RegistrationRegistrant]
		SET [GroupMemberId] = NULL 
		WHERE [GroupMemberId] IN (
			SELECT [Id]
			FROM [GroupMember]
			WHERE [PersonId] = @OldId
		)

		/**********************************************************************************************
		 * Handle GroupMemberAssignment records (again)
		 *    NOTE: There should not be any if we did everything correctly earlier/above.
		 **********************************************************************************************/
		-- Delete any Group Assignments that point to a group member about to be deleted
		DELETE FROM [GroupMemberAssignment]
		WHERE [GroupMemberId] IN (
			SELECT [Id]
			FROM [GroupMember]
			WHERE [PersonId] = @OldId
		)

		-- If there is GroupMemberHistory, we can't delete, so add any other GroupMemberIds for the old PersonId to our @GroupMembersIdsToArchive list
		INSERT INTO @GroupMembersIdsToArchive 
			SELECT [Id]	FROM [GroupMember] WHERE [PersonId] = @OldId AND Id IN (SELECT GroupMemberId FROM GroupMemberHistorical)

		UPDATE [GroupMember] 
			SET [IsArchived] = 1, [PersonId] = @NewId
			WHERE [Id] IN (SELECT [Id] FROM @GroupMembersIdsToArchive)

        -- Delete LMS LearningParticipant rows whose parent GroupMember belongs to @OldId
        DELETE lp
        FROM [LearningParticipant] AS lp
        INNER JOIN [GroupMember] AS gm
            ON gm.[Id] = lp.[Id]         -- TPT key equality (LP inherits GM)
        WHERE gm.[PersonId] = @OldId;

		-- Delete any group members not updated (already existed with new id)
		DELETE [GroupMember]
		WHERE [PersonId] = @OldId

		-- User Login
		-----------------------------------------------------------------------------------------------
		-- Update any user logins associated with old id to be associated with primary person
		UPDATE [UserLogin]
		SET [PersonId] = @NewId
		WHERE [PersonId] = @OldId

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

		-- Document
		-----------------------------------------------------------------------------------------------
		-- Update any documents that are associated to the old person to be associated to the new person
		UPDATE [dbo].[Document]
		SET [EntityId] = @NewId
		WHERE [Id] IN (
			SELECT d.[Id]
			FROM [Document] d
			JOIN [DocumentType] dt ON dt.[Id] = d.[DocumentTypeId]
			WHERE dt.[EntityTypeId] = @PersonEntityTypeId
				AND d.[EntityId] = @OldId)

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

		-- Transaction Detail
		-----------------------------------------------------------------------------------------------
		-- Update any financial transaction ( or scheduled transaction ) details that are associated to the old person to be associated to the new person
		UPDATE [FinancialTransactionDetail] SET [EntityId] = @NewId
		WHERE [EntityTypeId] = @PersonEntityTypeId
		AND [EntityId] = @OldId

		UPDATE [FinancialScheduledTransactionDetail] SET [EntityId] = @NewId
		WHERE [EntityTypeId] = @PersonEntityTypeId
		AND [EntityId] = @OldId
		
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

		UPDATE [History] SET [RelatedEntityId] = @NewId
		WHERE [RelatedEntityTypeId] = @PersonEntityTypeId
		AND [RelatedEntityId] = @OldId

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

		-- Attribute Value
		-----------------------------------------------------------------------------------------------
		-- Update any attribute values associated with old id to be associated with primary person
		UPDATE [AttributeValue]
		SET [ValueAsPersonId] = @NewId
		WHERE [ValueAsPersonId] = @OldId

		-- Update the Person Alias pointer
		UPDATE [PersonAlias]
		SET [PersonId] = @NewId
		WHERE [PersonId] = @OldId

		-- Delete any duplicate previous names
		DELETE PN
		FROM [PersonPreviousName] PN
		INNER JOIN [PersonAlias] PA ON PA.[Id] = PN.[PersonAliasId]
		WHERE PA.[PersonId] = @NewId
		AND PN.[Id] NOT IN (
			SELECT MIN(PN2.[Id]) AS [Id]
			FROM [PersonPreviousName] PN2
			INNER JOIN [PersonAlias] PA2 ON PA2.[Id] = PN2.[PersonAliasId]
			WHERE PA2.[PersonId] = @NewId
			GROUP BY PN2.[LastName]
		)

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
				,'AttributeValue'
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

        -- Reset FirstTime Attendance for all but the to the oldest first time record.
		DECLARE @Records AS TABLE(Id INT, StartDateTime DATETIME)

		INSERT INTO @Records
		SELECT Attendance.Id, Attendance.StartDateTime
		FROM Attendance
		INNER JOIN PersonAlias ON PersonAlias.Id = Attendance.PersonAliasId
		WHERE PersonId = @NewId AND IsFirstTime = 1

		DECLARE @FirstTimeRecordId AS INT
		SELECT TOP 1 @FirstTimeRecordId = a.Id
		FROM @Records a
		ORDER BY a.StartDateTime ASC

		UPDATE Attendance
		SET IsFirstTime = 0
		FROM Attendance
		INNER JOIN PersonAlias ON PersonAlias.Id = Attendance.PersonAliasId
		WHERE Attendance.Id IN (
			SELECT a.Id
			FROM @Records a
		)
		AND Attendance.Id != @FirstTimeRecordId

	END

END