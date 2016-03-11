/*
<doc>
	<summary>
 		This procedure merges relationship data for two people being merged.  It is called from the
		from the [dbo].[spCrm_PersonMerge] procedure to merge Known Relationships and Implied
		Relationships.  It is used when merging people in Rock and should never be used outside 
		of that process. 
	</summary>

	<returns></returns>
	<param name="Old Id" datatype="int">The person id of the non-primary Person being merged</param>
	<param name="New Id" datatype="int">The person id of the rimary Person being merged</param>
	<param name="Owner Role Guid" datatype="uniqueidentifier">The role of the owner in the relationship group</param>
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, '7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42' -- Known Relationships
		EXEC [dbo].[spCrm_PersonMergeRelationships] @OldId, @NewId, 'CB9A0E14-6FCF-4C07-A49A-D7873F45E196' -- Implied Relationships
	</code>
</doc>
*/

ALTER PROCEDURE [dbo].[spCrm_PersonMergeRelationships]
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