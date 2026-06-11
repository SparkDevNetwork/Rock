/*
<doc>
    <summary>
        Evaluates requirement status for applicable Group Members and updates 
        or inserts corresponding GroupMemberRequirement records. Also returns 
        any records that should trigger automatic 'Not Met' or 'Warning' workflows.
    </summary>

    <param name="GroupRequirementId" datatype="int">
        The Id of the GroupRequirement definition being evaluated.
    </param>

    <param name="GroupId" datatype="int">
        The Id of the Group whose members should be evaluated.
    </param>

    <param name="AppliesToPersonIds" datatype="dbo.IdList">
        A table-valued list of PersonIds for whom the requirement applies.
        (Used only when the requirement is restricted by DataView or other rules.)
    </param>

    <param name="WarningPersonIds" datatype="dbo.IdList">
        A table-valued list of PersonIds whose evaluation has resulted in
        "Meets with Warning".
    </param>	

    <param name="MeetsPersonIds" datatype="dbo.IdList">
        A table-valued list of PersonIds whose evaluation has resulted in
        "Meets Requirement".
    </param>

    <returns>
        Returns rows identifying GroupMemberRequirements requiring workflow
        initiation, including Person info and counts of INSERT/UPDATE/DELETE
        operations that occurred during MERGE processing.
    </returns>

    <remarks>
        This procedure mirrors Rock RMS requirement-evaluation logic including:
        - Role filtering
        - Age classification filtering
        - DataView-based applicability
        - Requirement expiration behavior
        - Due Date computation (static, attribute-based, or join-date-based)
        - Resolution of Meets / Warning / Not Met statuses
        - MERGE-based upsert logic for GroupMemberRequirement rows
        - Workflow-trigger conditions
    </remarks>
</doc>
*/

CREATE PROCEDURE [dbo].[spUpdateGroupMemberRequirements]
    @GroupRequirementId INT,
    @GroupId INT,
	@AppliesToPersonIds dbo.IdList READONLY,
	@WarningPersonIds dbo.IdList READONLY,
	@MeetsPersonIds dbo.IdList READONLY
AS
BEGIN
    SET NOCOUNT ON;

	--------------------------------------------------------------------------
    -- Load metadata for the Group Requirement (role filters, dataview, etc.)
    --------------------------------------------------------------------------
	DECLARE @GroupRoleId INT;
	DECLARE @AppliesToAgeClassification INT;
	DECLARE @AppliesToDataViewId INT;

	SELECT 
	    @GroupRoleId = gr.GroupRoleId,
		@AppliesToAgeClassification = gr.AppliesToAgeClassification,
		@AppliesToDataViewId = gr.AppliesToDataViewId
	FROM GroupRequirement gr
	WHERE gr.Id = @GroupRequirementId;

	--------------------------------------------------------------------------
    -- Build a working set of applicable GroupMember Ids. 
    -- Filters out archived, inactive, role-restricted, or age-restricted members.
    --------------------------------------------------------------------------
	CREATE TABLE #ApplicableGroupMemberIds (Id INT NOT NULL PRIMARY KEY);

	INSERT INTO #ApplicableGroupMemberIds (Id)
	SELECT gm.Id
	FROM GroupMember gm
	INNER JOIN Person p
		ON gm.PersonId = p.Id
	WHERE gm.GroupId = @GroupId
		AND gm.GroupMemberStatus != 0		-- Active members only
		AND gm.IsArchived = 0				-- Exclude archived records
		AND (@GroupRoleId IS NULL OR gm.GroupRoleId = @GroupRoleId)
		AND (@AppliesToAgeClassification = 0 OR p.AgeClassification = @AppliesToAgeClassification);

	--------------------------------------------------------------------------
    -- Apply expiration rules. If requirements can expire, keep only group members
    -- whose previously-met requirement has expired. Otherwise, remove those who
    -- have met it.
    --------------------------------------------------------------------------
	DECLARE @CanExpire BIT;
	DECLARE @ExpireInDays INT;

	SELECT 
		@CanExpire = grt.CanExpire,
		@ExpireInDays = grt.ExpireInDays
	FROM GroupRequirement gr
	INNER JOIN GroupRequirementType grt ON gr.GroupRequirementTypeId = grt.Id
	WHERE gr.Id = @GroupRequirementId;

	IF @CanExpire = 1 AND @ExpireInDays IS NOT NULL
	BEGIN
		-- Remove members whose "Met" status is still valid and has NOT expired.
		DELETE ag
		FROM #ApplicableGroupMemberIds ag
		INNER JOIN GroupMemberRequirement gmr
			ON gmr.GroupMemberId = ag.Id
			AND gmr.GroupRequirementId = @GroupRequirementId
		WHERE 
			gmr.RequirementMetDateTime IS NOT NULL
			AND gmr.RequirementWarningDateTime IS NULL
			AND DATEDIFF(DAY, gmr.RequirementMetDateTime, SYSDATETIME()) < @ExpireInDays;
	END
	ELSE
	BEGIN
		-- Requirements never expire -> remove anyone who has already met it.
		DELETE ag
		FROM #ApplicableGroupMemberIds ag
		INNER JOIN GroupMemberRequirement gmr
			ON gmr.GroupMemberId = ag.Id
		   AND gmr.GroupRequirementId = @GroupRequirementId
		WHERE gmr.RequirementMetDateTime IS NOT NULL;
	END

	--------------------------------------------------------------------------
    -- Apply DataView filtering. Keep only members whose PersonId appears in
    -- @AppliesToPersonIds when a DataView is configured for the requirement.
    --------------------------------------------------------------------------
	IF @AppliesToDataViewId IS NOT NULL
	BEGIN
		DELETE ag
		FROM #ApplicableGroupMemberIds ag
		INNER JOIN GroupMember gm ON ag.Id = gm.Id
		LEFT JOIN @AppliesToPersonIds dv
			   ON gm.PersonId = dv.Id
		WHERE dv.Id IS NULL;  -- Person NOT in the DataView -> exclude.
	END

    --------------------------------------------------------------------------
    -- Prepare a table variable to capture MERGE output and audit what happened.
    --------------------------------------------------------------------------
    DECLARE @Results TABLE
    (
		ActionTaken NVARCHAR(10),
        GroupMemberRequirementId INT,
        GroupMemberId INT,
        GroupRequirementId INT,
        MeetsGroupRequirement INT,
        WarningWorkflowId INT,
        DoesNotMeetWorkflowId INT
    );

	--------------------------------------------------------------------------
    -- Compute Due Date rules for each member: static date, attribute-driven,
    -- or join-date-driven. Used later to determine NotMet vs Warning.
    --------------------------------------------------------------------------
	DECLARE 
		@DueDateType INT,
		@DueDateOffset INT,
		@StaticDueDate DATETIME,
		@AttributeDueDate DATETIME;

	SELECT
		@DueDateType = grt.DueDateType,
		@DueDateOffset = grt.DueDateOffsetInDays,
		@StaticDueDate = gr.DueDateStaticDate,
		@AttributeDueDate = TRY_CAST(av.Value AS DATETIME2)
	FROM GroupRequirement gr
	INNER JOIN GroupRequirementType grt 
		ON gr.GroupRequirementTypeId = grt.Id
	LEFT JOIN AttributeValue av 
		ON av.AttributeId = gr.DueDateAttributeId 
		AND av.EntityId = @GroupId
	WHERE gr.Id = @GroupRequirementId;

	--------------------------------------------------------------------------
    -- Build CTE for computing Possible Due Dates for each member.
    --------------------------------------------------------------------------
	;WITH PossibleDueDate AS (
		SELECT
			gm.Id AS GroupMemberId,
			CASE @DueDateType
				WHEN 1 THEN @StaticDueDate
				WHEN 2 THEN 
					CASE 
						WHEN @AttributeDueDate IS NOT NULL 
						THEN DATEADD(DAY, ISNULL(@DueDateOffset, 0), @AttributeDueDate)
					END
				WHEN 3 THEN 
					CASE 
						WHEN gm.DateTimeAdded IS NOT NULL
						THEN DATEADD(DAY, ISNULL(@DueDateOffset, 0), gm.DateTimeAdded)
					END
				WHEN 0 THEN NULL
			END AS DueDate
		FROM GroupMember gm
		WHERE gm.GroupId = @GroupId
	),

    --------------------------------------------------------------------------
    -- Determine Meets / Warning / Not Met:
    --
    -- Meets (0)
    -- MeetsWithWarning (2)
    -- NotMet (1)
    --
    -- Logic:
    --  - Meets + Warning -> MeetsWithWarning
    --  - Meets only -> Meets
    --  - Not Met but requirement is NOT yet due -> MeetsWithWarning
    --  - Otherwise -> NotMet
    --------------------------------------------------------------------------
	RequirementResults AS (
		SELECT
			gm.Id AS GroupMemberId,
			CASE
				WHEN mp.Id IS NOT NULL AND wp.Id IS NOT NULL THEN 2
				WHEN mp.Id IS NOT NULL THEN 0
				WHEN pd.DueDate > SYSDATETIME() THEN 2
				ELSE 1
			END AS MeetsGroupRequirement,
			SYSDATETIME() AS LastRequirementCheckDateTime
		FROM GroupMember gm
		INNER JOIN [PossibleDueDate] pd
			ON pd.GroupMemberId = gm.Id
		INNER JOIN [Group] g
			ON gm.GroupId = g.Id
		INNER JOIN GroupRequirement gr
			ON ((gr.GroupId IS NOT NULL AND gr.GroupId = g.Id)
			  OR (gr.GroupTypeId IS NOT NULL AND gr.GroupTypeId = g.GroupTypeId))
			AND gr.Id = @GroupRequirementId
		INNER JOIN #ApplicableGroupMemberIds ag
			ON gm.Id = ag.Id
		LEFT JOIN @MeetsPersonIds mp
			ON gm.PersonId = mp.Id
		LEFT JOIN @WarningPersonIds wp
			ON gm.PersonId = wp.Id
	)

    --------------------------------------------------------------------------
    -- MERGE: Update existing requirement rows or insert new ones.
    -- Handles timestamp updates and resetting workflow Ids when requirements
    -- transition into "Meets".
    --------------------------------------------------------------------------
    MERGE INTO [GroupMemberRequirement] AS [target]
    USING RequirementResults AS [source]
        ON [target].[GroupRequirementId] = @GroupRequirementId
        AND [target].[GroupMemberId] = [source].[GroupMemberId]

    WHEN MATCHED THEN
        UPDATE SET
            [target].[RequirementMetDateTime] =
                CASE WHEN [source].[MeetsGroupRequirement] = 0 THEN SYSDATETIME() ELSE NULL END,
            [target].[RequirementWarningDateTime] =
                CASE WHEN [source].[MeetsGroupRequirement] = 2 THEN SYSDATETIME() ELSE NULL END,
            [target].[RequirementFailDateTime] =
                CASE WHEN [source].[MeetsGroupRequirement] = 1 THEN SYSDATETIME() ELSE NULL END,
            [target].[LastRequirementCheckDateTime] = [source].[LastRequirementCheckDateTime],
			[target].[WarningWorkflowId] = 
				CASE WHEN [source].[MeetsGroupRequirement] = 0 THEN NULL ELSE [target].[WarningWorkflowId] END,		-- If the group member meets the requirement then clear their "warning" workflow Id.
			[target].[DoesNotMeetWorkflowId] = 
				CASE WHEN [source].[MeetsGroupRequirement] = 0 THEN NULL ELSE [target].[DoesNotMeetWorkflowId] END	-- If the group member meets the requirement then clear their "does not meet" workflow Id.

    WHEN NOT MATCHED BY TARGET THEN
        INSERT
        (
            GroupMemberId,
            GroupRequirementId,
            RequirementMetDateTime,
            RequirementWarningDateTime,
            RequirementFailDateTime,
            LastRequirementCheckDateTime,
            Guid,
            CreatedDateTime,
            ModifiedDateTime
        )
        VALUES
        (
            [source].[GroupMemberId],
            @GroupRequirementId,
            CASE WHEN [source].[MeetsGroupRequirement] = 0 THEN SYSDATETIME() END,
            CASE WHEN [source].[MeetsGroupRequirement] = 2 THEN SYSDATETIME() END,
            CASE WHEN [source].[MeetsGroupRequirement] = 1 THEN SYSDATETIME() END,
            [source].[LastRequirementCheckDateTime],
            NEWID(),
            [source].[LastRequirementCheckDateTime],
            [source].[LastRequirementCheckDateTime]
        )

    OUTPUT
		$action,
        inserted.Id AS GroupMemberRequirementId,
        inserted.GroupMemberId,
        inserted.GroupRequirementId,
        source.MeetsGroupRequirement,
        inserted.WarningWorkflowId,
        inserted.DoesNotMeetWorkflowId
    INTO @Results;

	--------------------------------------------------------------------------
    -- Delete requirement records for members that no longer apply.
    -- Only delete rows where the requirement is not met.
    --------------------------------------------------------------------------
	DELETE gmreq
	OUTPUT
		'DELETE',
		deleted.Id,
		deleted.GroupMemberId,
		deleted.GroupRequirementId,
		NULL,
		deleted.WarningWorkflowId,
		deleted.DoesNotMeetWorkflowId
	INTO @Results
	FROM GroupMemberRequirement gmreq
	INNER JOIN GroupMember gm
		ON gmreq.GroupMemberId = gm.Id
	LEFT JOIN #ApplicableGroupMemberIds src
		ON gmreq.GroupMemberId = src.Id
	WHERE src.Id IS NULL
	  AND gm.GroupId = @GroupId
	  AND gmreq.RequirementMetDateTime IS NULL
	  AND gmreq.GroupRequirementId = @GroupRequirementId;

    --------------------------------------------------------------------------
    -- Determine which records should trigger workflows.
    --------------------------------------------------------------------------
	;WITH WorkflowRows AS (
		SELECT
			r.GroupMemberRequirementId,
			r.GroupMemberId,
			gm.PersonId,
			p.NickName,
			p.LastName,
			p.SuffixValueId,
			p.RecordTypeValueId,
			CASE 
				WHEN r.MeetsGroupRequirement = 1 THEN 'NotMet'
				WHEN r.MeetsGroupRequirement = 2 THEN 'Warning'
			END AS WorkflowType
		FROM @Results r
		INNER JOIN GroupMember gm ON r.GroupMemberId = gm.Id
		INNER JOIN Person p ON gm.PersonId = p.Id
		INNER JOIN GroupRequirement gr ON r.GroupRequirementId = gr.Id
		INNER JOIN GroupRequirementType grt ON gr.GroupRequirementTypeId = grt.Id
		WHERE
			(
				r.MeetsGroupRequirement = 1
				AND grt.ShouldAutoInitiateDoesNotMeetWorkflow = 1
				AND grt.DoesNotMeetWorkflowTypeId IS NOT NULL
				AND r.DoesNotMeetWorkflowId IS NULL
			)
			OR
			(
				r.MeetsGroupRequirement = 2
				AND grt.ShouldAutoInitiateWarningWorkflow = 1
				AND grt.WarningWorkflowTypeId IS NOT NULL
				AND r.WarningWorkflowId IS NULL
			)
	)
	SELECT
		wr.GroupMemberRequirementId,
		wr.GroupMemberId,
		wr.PersonId,
		wr.NickName,
		wr.LastName,
		wr.SuffixValueId,
		wr.RecordTypeValueId,
		wr.WorkflowType,
		InsertedCount = COUNT(CASE WHEN r.ActionTaken = 'INSERT' THEN 1 END),
		UpdatedCount  = COUNT(CASE WHEN r.ActionTaken = 'UPDATE' THEN 1 END),
		DeletedCount  = COUNT(CASE WHEN r.ActionTaken = 'DELETE' THEN 1 END)
	FROM @Results r
	FULL OUTER JOIN WorkflowRows wr
		ON 1 = 1
	GROUP BY
		wr.GroupMemberRequirementId,
		wr.GroupMemberId,
		wr.PersonId,
		wr.NickName,
		wr.LastName,
		wr.SuffixValueId,
		wr.RecordTypeValueId,
		wr.WorkflowType;

	DROP TABLE IF EXISTS #ApplicableGroupMemberIds;
END
