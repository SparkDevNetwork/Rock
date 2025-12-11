/*
<doc>
    <summary>
        This stored procedure retrieves placed and unplaced people for all supported
        configurations of the Group Placement block in Rock RMS. It supports filtering
        by template, instance, groups, entity sets, demographics, campus, DataView
        membership, and fee qualifications.
    </summary>

    <returns>
        A result set containing placement-eligible people along with placement status
        and contextual registration or group information.
    </returns>

    <param name='@RegistrationTemplatePlacementEntityTypeId' datatype='int'>
        The EntityTypeId for RegistrationTemplatePlacement.
    </param>

    <param name='@RegistrationInstanceEntityTypeId' datatype='int'>
        The EntityTypeId for RegistrationInstance.
    </param>

    <param name='@PersonEntityTypeId' datatype='int'>
        The EntityTypeId for Person (used for EntitySetMode).
    </param>

    <param name='@SourceEntityTypeId' datatype='int'>
        Optional. The EntityTypeId for the source entity (group or entity set).
    </param>

    <param name='@TargetEntityTypeId' datatype='int'>
        The EntityTypeId of the target entity, typically Group.
    </param>

    <param name='@RegistrationTemplateId' datatype='int'>
        Optional. The ID of the registration template.
    </param>

    <param name='@RegistrationInstanceId' datatype='int'>
        Optional. The ID of a specific registration instance.
    </param>

    <param name='@RegistrationTemplatePlacementId' datatype='int'>
        Optional. The ID of a RegistrationTemplatePlacement.
    </param>

    <param name='@SourceEntityId' datatype='int'>
        Optional. The ID of the source group or entity set.
    </param>

    <param name='@PlacementMode' datatype='varchar(50)'>
        Determines the retrieval strategy:
        - 'TemplateMode': Placements based on registration template.
        - 'InstanceMode': Placements for a single registration instance.
        - 'GroupMode': People in a source group + destination groups.
        - 'EntitySetMode': People in an entity set + destination groups.
    </param>

    <param name='@IncludedRegistrationInstanceIds' datatype='nvarchar(max)'>
        Optional. Comma-delimited list of RegistrationInstanceIds to include.
    </param>

    <param name='@IncludeFees' datatype='bit'>
        If 1, fee-based filtering is applied.
    </param>

    <param name='@IncludedFeeItemIds' datatype='nvarchar(max)'>
        Optional. Comma-delimited list of specific fee item IDs to include.
    </param>

    <param name='@DestinationGroupTypeId' datatype='int'>
        Optional. Limits eligible destination groups to a specific GroupType.
    </param>

    <param name='@DestinationGroupIds' datatype='nvarchar(max)'>
        Optional. Comma-delimited list of explicit destination GroupIds.
    </param>

    <param name='@DisplayedCampusGuid' datatype='uniqueidentifier'>
        Optional. Filters people or groups to those matching a specific campus.
    </param>

    <param name='@PurposeKey' datatype='nvarchar(max)'>
        Optional. Used when matching RelatedEntities for Group or EntitySet modes.
    </param>

    <param name='@RegistrationTemplatePurposeKey' datatype='nvarchar(max)'>
        Optional. Used for TemplateMode group relationships.
    </param>

    <param name='@RegistrationInstancePurposeKey' datatype='nvarchar(max)'>
        Optional. Used for InstanceMode group relationships.
    </param>

    <param name='@FilterAppliesTo' datatype='int'>
        Determines which subset demographic filters apply to:
        - 0 = Unplaced only
        - 1 = Placed only
        - 2 = Both placed and unplaced
    </param>

    <param name='@Gender' datatype='int'>
        Optional. Rock numeric gender value. Filters placed/unplaced depending on @FilterAppliesTo.
    </param>

    <param name='@CampusGuids' datatype='nvarchar(max)'>
        Optional. Comma-delimited list of campus GUIDs to filter PrimaryCampusId.
    </param>

    <param name='@AgeComparisonType' datatype='int'>
        Optional. Comparison operator from Rock DataFilter ExpressionType.
    </param>

    <param name='@AgeLow' datatype='int'>
        Optional. Lower bound for between-age comparisons.
    </param>

    <param name='@AgeHigh' datatype='int'>
        Optional. Value used for most age comparison types.
    </param>

    <param name='@GradeComparisonType' datatype='int'>
        Optional. Grade-level comparison operator from Rock DataFilter ExpressionType.
    </param>

    <param name='@CurrentSchoolYear' datatype='int'>
        Required for grade comparisons. The current academic year.
    </param>

    <param name='@GradeOffset' datatype='int'>
        Grade offset representing the grade level.
    </param>

    <param name='@NextGradeOffset' datatype='int'>
        Grade offset for the next grade. Used in range comparison logic.
    </param>

    <param name='@PersistedDataViewGuids' datatype='nvarchar(max)'>
        Optional. Comma-delimited list of persisted DataView GUIDs.
        Only people included in at least one of these DataViews are allowed.
    </param>

    <remarks>
        This procedure powers the Group Placement block in Rock RMS. It retrieves
        registrants, group members, entity set members, or combinations thereof,
        depending on the placement mode. It also supports demographic filters,
        campus filters, DataView filters, fee filters, and placement-status filters.
    </remarks>

    <code>
        -- Example usage:
        EXEC dbo.spGetGroupPlacementPeople
            @RegistrationTemplatePlacementEntityTypeId = 591,
            @RegistrationInstanceEntityTypeId = 260,
            @TargetEntityTypeId = 16,
            @RegistrationTemplateId = 3,
            @RegistrationTemplatePlacementId = 1,
            @PlacementMode = 'TemplateMode',
            @IncludeFees = 0,
            @DestinationGroupTypeId = 26,
            @RegistrationTemplatePurposeKey = 'PLACEMENT-TEMPLATE',
            @RegistrationInstancePurposeKey = 'PLACEMENT'
    </code>
</doc>
*/


CREATE PROCEDURE [dbo].[spGetGroupPlacementPeople]
    @RegistrationTemplatePlacementEntityTypeId INT = NULL,
    @RegistrationInstanceEntityTypeId INT = NULL,
	@PersonEntityTypeId INT = NULL,
    @SourceEntityTypeId INT = NULL,
	@TargetEntityTypeId INT,
    @RegistrationTemplateId INT = NULL,
    @RegistrationInstanceId INT = NULL,
    @RegistrationTemplatePlacementId INT = NULL,
	@SourceEntityId INT = NULL,
    @PlacementMode VARCHAR(50),
    @IncludedRegistrationInstanceIds NVARCHAR(MAX) = NULL,
	@IncludeFees BIT = 0,
	@IncludedFeeItemIds NVARCHAR(MAX) = NULL,
	@DestinationGroupTypeId INT = NULL,
	@DestinationGroupIds NVARCHAR(MAX) = NULL,
	@DisplayedCampusGuid UNIQUEIDENTIFIER = NULL,
	@PurposeKey NVARCHAR(MAX) = NULL,
	@RegistrationTemplatePurposeKey NVARCHAR(MAX) = NULL,
	@RegistrationInstancePurposeKey NVARCHAR(MAX) = NULL,
	@FilterAppliesTo INT = 0,
	@Gender INT = NULL,
	@CampusGuids NVARCHAR(MAX) = NULL,
	@AgeComparisonType INT = NULL,
	@AgeLow INT = NULL,
	@AgeHigh INT = NULL,
	@GradeComparisonType INT = NULL,
	@CurrentSchoolYear INT = NULL,
	@GradeOffset INT = NULL,
	@NextGradeOffset INT = NULL,
	@PersistedDataViewGuids NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #DestinationGroups (
        GroupId INT PRIMARY KEY,
        GroupTypeId INT
    );

	DECLARE @IncludedCampuses TABLE (Id INT PRIMARY KEY);

	INSERT INTO @IncludedCampuses (Id)
	SELECT c.Id
	FROM Campus c
	INNER JOIN (
		SELECT TRY_CAST(value AS uniqueidentifier) AS Guid
		FROM STRING_SPLIT(@CampusGuids, ',')
		WHERE value <> ''
	) x ON c.Guid = x.Guid;

	DECLARE @IncludedPersistedDataViews TABLE (Id INT PRIMARY KEY);

	INSERT INTO @IncludedPersistedDataViews (Id)
	SELECT d.Id
	FROM DataView d
	INNER JOIN (
		SELECT TRY_CAST(value AS uniqueidentifier) AS Guid
		FROM STRING_SPLIT(@PersistedDataViewGuids, ',')
		WHERE value <> ''
	) x ON d.Guid = x.Guid;

	DECLARE @Registrants TABLE (
		RegistrationInstanceId INT, 
		RegistrationInstanceName NVARCHAR(200), 
		CreatedDateTime DATETIME, 
		PersonAliasId INT, 
		RegistrantId INT
	);

	IF @PlacementMode = 'GroupMode' OR @PlacementMode = 'EntitySetMode'
	BEGIN
		;WITH IncludedDestinationGroupIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@DestinationGroupIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		)

		INSERT INTO #DestinationGroups (GroupId, GroupTypeId)
		SELECT g.Id, g.GroupTypeId
		FROM [Group] g
		LEFT JOIN Campus c ON g.CampusId = c.Id
		LEFT JOIN RelatedEntity re ON re.TargetEntityId = g.Id
		WHERE (
			(
				re.SourceEntityTypeId = @SourceEntityTypeId
				AND re.TargetEntityTypeId = @TargetEntityTypeId
				AND re.SourceEntityId = @SourceEntityId
				AND re.PurposeKey = @PurposeKey
			)
			OR (
				g.Id IN (SELECT Id FROM IncludedDestinationGroupIds)
			)
		)
		AND g.GroupTypeId = @DestinationGroupTypeId
		AND (
			@DisplayedCampusGuid IS NULL
			OR c.Guid = @DisplayedCampusGuid
			OR g.CampusId IS NULL
		)
		AND g.IsArchived = 0
	END
	ELSE
	BEGIN
		-- Path 1: From RegistrationTemplatePlacement > RelatedEntity > Group
		INSERT INTO #DestinationGroups (GroupId, GroupTypeId)
		SELECT g.Id, g.GroupTypeId
		FROM RegistrationTemplatePlacement rtp
		INNER JOIN RelatedEntity re ON rtp.Id = re.SourceEntityId
		INNER JOIN [Group] g ON re.TargetEntityId = g.Id
		LEFT JOIN Campus c ON g.CampusId = c.Id
		WHERE re.SourceEntityTypeId = @RegistrationTemplatePlacementEntityTypeId
		  AND re.TargetEntityTypeId = @TargetEntityTypeId
		  AND re.PurposeKey = @RegistrationTemplatePurposeKey
		  AND rtp.RegistrationTemplateId = @RegistrationTemplateId
		  AND rtp.Id = @RegistrationTemplatePlacementId
		  AND g.GroupTypeId = @DestinationGroupTypeId
		  AND (
			@DisplayedCampusGuid IS NULL
			OR c.Guid = @DisplayedCampusGuid
			OR g.CampusId IS NULL
		  )
		  AND g.IsArchived = 0;

		-- Path 2: From RegistrationInstance > RelatedEntity > Group
		INSERT INTO #DestinationGroups (GroupId, GroupTypeId)
		SELECT DISTINCT g.Id, g.GroupTypeId
		FROM RegistrationInstance ri
		INNER JOIN RelatedEntity re ON ri.Id = re.SourceEntityId
		INNER JOIN [Group] g ON re.TargetEntityId = g.Id
		LEFT JOIN Campus c ON g.CampusId = c.Id
		WHERE re.SourceEntityTypeId = @RegistrationInstanceEntityTypeId
		  AND re.TargetEntityTypeId = @TargetEntityTypeId
		  AND re.PurposeKey = @RegistrationInstancePurposeKey
		  AND re.QualifierValue = @RegistrationTemplatePlacementId
		  AND ri.RegistrationTemplateId = @RegistrationTemplateId
		  AND (@PlacementMode = 'TemplateMode' OR ri.Id = @RegistrationInstanceId)
		  AND g.GroupTypeId = @DestinationGroupTypeId
		  AND g.Id NOT IN (SELECT GroupId FROM #DestinationGroups)
		  AND (
			@DisplayedCampusGuid IS NULL
			OR c.Guid = @DisplayedCampusGuid
			OR g.CampusId IS NULL
		  )
		  AND g.IsArchived = 0;
	END

	IF @PlacementMode = 'GroupMode'
	BEGIN
		    SELECT 
				gm.GroupId,
				gm.GroupTypeId,
				gm.Id AS GroupMemberId, 
				gm.GroupRoleId, 
				p.Id AS PersonId, 
				p.FirstName, 
				p.NickName, 
				p.LastName, 
				p.Gender, 
				p.PhotoId, 
				p.Age, 
				p.RecordTypeValueId, 
				p.AgeClassification, 
				gm.DateTimeAdded
			FROM GroupMember gm
			INNER JOIN Person p ON gm.PersonId = p.Id
			LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
			CROSS APPLY (
				SELECT CASE
					WHEN @FilterAppliesTo = 0 AND g.GroupId = @SourceEntityId THEN 1 -- unplaced subset
					WHEN @FilterAppliesTo = 1 AND g.GroupId <> @SourceEntityId AND g.GroupId IS NOT NULL THEN 1 -- placed subset
					WHEN @FilterAppliesTo = 2 THEN 1 -- all people
					ELSE 0
				END AS FilterTarget
			) ft
			WHERE gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
			AND (
				gm.GroupId = @SourceEntityId
				OR gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)
			)
			AND (
				ft.FilterTarget = 0        -- person is NOT part of the filtered subset => always allow
				OR @Gender IS NULL         -- no gender filter applied at all
				OR p.Gender = @Gender      -- person IS in the filtered subset => must match gender
			)
			AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedCampuses)
				OR p.PrimaryCampusId IN (SELECT Id FROM @IncludedCampuses)
			)
			AND (
				ft.FilterTarget = 0
				OR @AgeComparisonType IS NULL
				OR (
					@AgeComparisonType NOT IN (32, 64, 4096) -- NOT Is Null / Is Not Null / Between
					AND @AgeHigh IS NULL                     -- AND missing value => skip filter
				)
				OR (
					@AgeComparisonType = 4096
					AND (
						@AgeLow IS NULL 
						OR @AgeHigh IS NULL
					)
				)
				OR (
					CASE @AgeComparisonType
						WHEN 1      THEN CASE WHEN p.Age = @AgeHigh THEN 1 END
						WHEN 2      THEN CASE WHEN p.Age <> @AgeHigh THEN 1 END
						WHEN 128    THEN CASE WHEN p.Age > @AgeHigh THEN 1 END
						WHEN 256    THEN CASE WHEN p.Age >= @AgeHigh THEN 1 END
						WHEN 512    THEN CASE WHEN p.Age < @AgeHigh THEN 1 END
						WHEN 1024   THEN CASE WHEN p.Age <= @AgeHigh THEN 1 END
						WHEN 4096   THEN CASE WHEN p.Age BETWEEN @AgeLow AND @AgeHigh THEN 1 END
						WHEN 32     THEN CASE WHEN p.Age IS NULL THEN 1 END
						WHEN 64     THEN CASE WHEN p.Age IS NOT NULL THEN 1 END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR @GradeComparisonType IS NULL
				OR (
					@GradeComparisonType NOT IN (32, 64)
					AND @GradeOffset IS NULL
				)
				OR (
					CASE @GradeComparisonType
						-- Equal To
						WHEN 1 THEN 
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >  @NextGradeOffset 
								THEN 1 
							END
						-- Not Equal To
						WHEN 2 THEN
							CASE 
								WHEN ( (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									OR (p.GraduationYear - @CurrentSchoolYear) >  @GradeOffset )
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Less Than
						WHEN 512 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @GradeOffset 
								THEN 1 
							END
						-- Less Than Or Equal To
						WHEN 1024 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @NextGradeOffset 
								THEN 1 
							END
						-- Greater Than
						WHEN 128 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END

						-- Greater Than Or Equal To
						WHEN 256 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Is Blank
						WHEN 32 THEN
							CASE 
								WHEN p.GraduationYear IS NULL 
									OR (p.GraduationYear - @CurrentSchoolYear) < 0
								THEN 1 
							END

						-- Is Not Blank
						WHEN 64 THEN
							CASE 
								WHEN p.GraduationYear IS NOT NULL
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0
								THEN 1 
							END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedPersistedDataViews)
				OR EXISTS (
						SELECT 1
						FROM @IncludedPersistedDataViews dv
						INNER JOIN DataViewPersistedValue dpv
							ON dpv.DataViewId = dv.Id
						WHERE dpv.EntityId = p.Id
					)
			)

	END
	ELSE IF @PlacementMode = 'EntitySetMode'
    BEGIN
		SELECT
			ep.*
		FROM (
			-- People in the entity set
			SELECT 
				NULL AS GroupId, 
				NULL AS GroupTypeId, 
				NULL AS GroupMemberId, 
				NULL AS GroupRoleId, 
				p.Id AS PersonId, 
				p.FirstName, 
				p.NickName, 
				p.LastName, 
				p.Gender, 
				p.PhotoId, 
				p.Age, 
				p.RecordTypeValueId, 
				p.AgeClassification, 
				esi.CreatedDateTime,
				NULL AS DateTimeAdded,
				p.PrimaryCampusId,
				p.GraduationYear
			FROM Person p
			INNER JOIN EntitySetItem esi ON p.Id = esi.EntityId
			INNER JOIN EntitySet es ON esi.EntitySetId = es.Id
			WHERE es.EntityTypeId = @PersonEntityTypeId 
				AND es.Id = @SourceEntityId

			UNION

			-- People in the Destination Groups
			SELECT 
				g.GroupId, 
				g.GroupTypeId, 
				gm.Id AS GroupMemberId, 
				gm.GroupRoleId, 
				p.Id AS PersonId, 
				p.FirstName, 
				p.NickName, 
				p.LastName, 
				p.Gender, 
				p.PhotoId, 
				p.Age, 
				p.RecordTypeValueId, 
				p.AgeClassification, 
				NULL AS CreatedDateTime,
				gm.DateTimeAdded,
				p.PrimaryCampusId,
				p.GraduationYear
			FROM Person p
			INNER JOIN [GroupMember] gm ON p.Id = gm.PersonId
			INNER JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
			WHERE gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
		) ep
		CROSS APPLY (
			SELECT CASE
				WHEN @FilterAppliesTo = 0 AND ep.GroupId IS NULL THEN 1 -- unplaced subset
				WHEN @FilterAppliesTo = 1 AND ep.GroupId IS NOT NULL THEN 1 -- placed subset
				WHEN @FilterAppliesTo = 2 THEN 1 -- all people
				ELSE 0
			END AS FilterTarget
		) ft
		WHERE (
				ft.FilterTarget = 0        -- person is NOT part of the filtered subset => always allow
				OR @Gender IS NULL         -- no gender filter applied at all
				OR ep.Gender = @Gender      -- person IS in the filtered subset => must match gender
		)
		AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedCampuses)
				OR ep.PrimaryCampusId IN (SELECT Id FROM @IncludedCampuses)
		  )
		  AND (
				ft.FilterTarget = 0
				OR @AgeComparisonType IS NULL
				OR (
					@AgeComparisonType NOT IN (32, 64, 4096) -- NOT Is Null / Is Not Null / Between
					AND @AgeHigh IS NULL                     -- AND missing value => skip filter
				)
				OR (
					@AgeComparisonType = 4096
					AND (
						@AgeLow IS NULL 
						OR @AgeHigh IS NULL
					)
				)
				OR (
					CASE @AgeComparisonType
						WHEN 1      THEN CASE WHEN ep.Age = @AgeHigh THEN 1 END
						WHEN 2      THEN CASE WHEN ep.Age <> @AgeHigh THEN 1 END
						WHEN 128    THEN CASE WHEN ep.Age > @AgeHigh THEN 1 END
						WHEN 256    THEN CASE WHEN ep.Age >= @AgeHigh THEN 1 END
						WHEN 512    THEN CASE WHEN ep.Age < @AgeHigh THEN 1 END
						WHEN 1024   THEN CASE WHEN ep.Age <= @AgeHigh THEN 1 END
						WHEN 4096   THEN CASE WHEN ep.Age BETWEEN @AgeLow AND @AgeHigh THEN 1 END
						WHEN 32     THEN CASE WHEN ep.Age IS NULL THEN 1 END
						WHEN 64     THEN CASE WHEN ep.Age IS NOT NULL THEN 1 END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR @GradeComparisonType IS NULL
				OR (
					@GradeComparisonType NOT IN (32, 64)
					AND @GradeOffset IS NULL
				)
				OR (
					CASE @GradeComparisonType
						-- Equal To
						WHEN 1 THEN 
							CASE 
								WHEN (ep.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (ep.GraduationYear - @CurrentSchoolYear) >  @NextGradeOffset 
								THEN 1 
							END
						-- Not Equal To
						WHEN 2 THEN
							CASE 
								WHEN ( (ep.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									OR (ep.GraduationYear - @CurrentSchoolYear) >  @GradeOffset )
									AND (ep.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Less Than
						WHEN 512 THEN
							CASE 
								WHEN (ep.GraduationYear - @CurrentSchoolYear) > @GradeOffset 
								THEN 1 
							END
						-- Less Than Or Equal To
						WHEN 1024 THEN
							CASE 
								WHEN (ep.GraduationYear - @CurrentSchoolYear) > @NextGradeOffset 
								THEN 1 
							END
						-- Greater Than
						WHEN 128 THEN
							CASE 
								WHEN (ep.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									AND (ep.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END

						-- Greater Than Or Equal To
						WHEN 256 THEN
							CASE 
								WHEN (ep.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (ep.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Is Blank
						WHEN 32 THEN
							CASE 
								WHEN ep.GraduationYear IS NULL 
									OR (ep.GraduationYear - @CurrentSchoolYear) < 0
								THEN 1 
							END

						-- Is Not Blank
						WHEN 64 THEN
							CASE 
								WHEN ep.GraduationYear IS NOT NULL
									AND (ep.GraduationYear - @CurrentSchoolYear) >= 0
								THEN 1 
							END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedPersistedDataViews)
				OR EXISTS (
						SELECT 1
						FROM @IncludedPersistedDataViews dv
						INNER JOIN DataViewPersistedValue dpv
							ON dpv.DataViewId = dv.Id
						WHERE dpv.EntityId = ep.PersonId
					)
			)

	END
    ELSE IF @PlacementMode = 'TemplateMode'
    BEGIN
		INSERT INTO @Registrants
		SELECT
			ri.Id AS RegistrationInstanceId,
			ri.Name AS RegistrationInstanceName,
			rr.CreatedDateTime,
			rr.PersonAliasId,
			rr.Id AS RegistrantId
		FROM RegistrationTemplate rt
		INNER JOIN RegistrationInstance ri ON rt.Id = ri.RegistrationTemplateId
		INNER JOIN Registration r ON ri.Id = r.RegistrationInstanceId
		INNER JOIN RegistrationRegistrant rr ON r.Id = rr.RegistrationId
		WHERE rt.Id = @RegistrationTemplateId;

		;WITH IncludedInstanceIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@IncludedRegistrationInstanceIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		),
		IncludedFeeItemIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@IncludedFeeItemIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		),
		FeeData AS (
			SELECT
				rrf.RegistrationRegistrantId,
				rtf.Name AS FeeName,
				rtf.FeeType,
				rrf.[Option],
				rrf.Quantity,
				rrf.Cost,
				rtfi.Id AS FeeItemId
			FROM RegistrationRegistrantFee rrf
			INNER JOIN RegistrationTemplateFee rtf ON rtf.Id = rrf.RegistrationTemplateFeeId
			INNER JOIN RegistrationTemplateFeeItem rtfi ON rtfi.Id = rrf.RegistrationTemplateFeeItemId
			WHERE
				rrf.Quantity > 0
				AND @IncludeFees = 1
		)

        SELECT 
			g.GroupId,
			g.GroupTypeId,
			gm.Id AS GroupMemberId,
			gm.GroupRoleId,
			gm.DateTimeAdded,
			p.Id AS PersonId, 
			p.FirstName,
			p.NickName,
			p.LastName,
			p.Gender, 
			p.PhotoId, 
			p.Age, 
			p.RecordTypeValueId, 
			p.AgeClassification,
			x.RegistrantId, 
			x.RegistrationInstanceName,
			x.RegistrationInstanceId,
			x.CreatedDateTime, 
			fd.FeeName, 
			fd.[Option], 
			fd.Quantity, 
			fd.Cost,
			fd.FeeType,
			fd.FeeItemId
        FROM (
			SELECT
				RegistrationInstanceId,
				RegistrationInstanceName,
				CreatedDateTime,
				PersonAliasId,
				RegistrantId
			FROM @Registrants
			UNION
			SELECT
				NULL,
				NULL,
				NULL,
				pa.Id,
				NULL
			FROM GroupMember gm
			JOIN #DestinationGroups dg ON gm.GroupId = dg.GroupId
			JOIN PersonAlias pa ON gm.PersonId = pa.PersonId
			WHERE NOT EXISTS (
				SELECT 1 
				FROM @Registrants r 
				WHERE r.PersonAliasId = pa.Id
			)
			AND gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
		) x -- Gets all people. (placed registrants, unplaced registrants, and placed non-registrants)
        INNER JOIN PersonAlias pa ON x.PersonAliasId = pa.Id
		INNER JOIN Person p ON pa.PersonId = p.Id
        LEFT JOIN GroupMember gm ON gm.PersonId = p.Id
            AND gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)
			AND gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
		LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
		LEFT JOIN FeeData fd ON fd.RegistrationRegistrantId = x.RegistrantId
		CROSS APPLY (
			SELECT CASE
				WHEN @FilterAppliesTo = 0 AND g.GroupId IS NULL THEN 1 -- unplaced subset
				WHEN @FilterAppliesTo = 1 AND g.GroupId IS NOT NULL THEN 1 -- placed subset
				WHEN @FilterAppliesTo = 2 THEN 1 -- all people
				ELSE 0
			END AS FilterTarget
		) ft
        WHERE (
			  g.GroupId IS NOT NULL OR (
				  NOT EXISTS (SELECT 1 FROM IncludedInstanceIds)
				  OR x.RegistrationInstanceId IN (SELECT Id FROM IncludedInstanceIds)
			  )
          )
		  AND (
			NOT EXISTS (SELECT 1 FROM IncludedFeeItemIds)
			OR EXISTS (
				SELECT 1
				FROM FeeData fd
				WHERE fd.RegistrationRegistrantId = x.RegistrantId
					AND fd.FeeItemId IN (SELECT Id FROM IncludedFeeItemIds)
			)
		  )
		  AND (
				ft.FilterTarget = 0        -- person is NOT part of the filtered subset => always allow
				OR @Gender IS NULL         -- no gender filter applied at all
				OR p.Gender = @Gender      -- person IS in the filtered subset => must match gender
		  )
		  AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedCampuses)
				OR p.PrimaryCampusId IN (SELECT Id FROM @IncludedCampuses)
		  )
		  AND (
				ft.FilterTarget = 0
				OR @AgeComparisonType IS NULL
				OR (
					@AgeComparisonType NOT IN (32, 64, 4096) -- NOT Is Null / Is Not Null / Between
					AND @AgeHigh IS NULL                     -- AND missing value => skip filter
				)
				OR (
					@AgeComparisonType = 4096
					AND (
						@AgeLow IS NULL 
						OR @AgeHigh IS NULL
					)
				)
				OR (
					CASE @AgeComparisonType
						WHEN 1      THEN CASE WHEN p.Age = @AgeHigh THEN 1 END
						WHEN 2      THEN CASE WHEN p.Age <> @AgeHigh THEN 1 END
						WHEN 128    THEN CASE WHEN p.Age > @AgeHigh THEN 1 END
						WHEN 256    THEN CASE WHEN p.Age >= @AgeHigh THEN 1 END
						WHEN 512    THEN CASE WHEN p.Age < @AgeHigh THEN 1 END
						WHEN 1024   THEN CASE WHEN p.Age <= @AgeHigh THEN 1 END
						WHEN 4096   THEN CASE WHEN p.Age BETWEEN @AgeLow AND @AgeHigh THEN 1 END
						WHEN 32     THEN CASE WHEN p.Age IS NULL THEN 1 END
						WHEN 64     THEN CASE WHEN p.Age IS NOT NULL THEN 1 END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR @GradeComparisonType IS NULL
				OR (
					@GradeComparisonType NOT IN (32, 64)
					AND @GradeOffset IS NULL
				)
				OR (
					CASE @GradeComparisonType
						-- Equal To
						WHEN 1 THEN 
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >  @NextGradeOffset 
								THEN 1 
							END
						-- Not Equal To
						WHEN 2 THEN
							CASE 
								WHEN ( (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									OR (p.GraduationYear - @CurrentSchoolYear) >  @GradeOffset )
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Less Than
						WHEN 512 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @GradeOffset 
								THEN 1 
							END
						-- Less Than Or Equal To
						WHEN 1024 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @NextGradeOffset 
								THEN 1 
							END
						-- Greater Than
						WHEN 128 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END

						-- Greater Than Or Equal To
						WHEN 256 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Is Blank
						WHEN 32 THEN
							CASE 
								WHEN p.GraduationYear IS NULL 
									OR (p.GraduationYear - @CurrentSchoolYear) < 0
								THEN 1 
							END

						-- Is Not Blank
						WHEN 64 THEN
							CASE 
								WHEN p.GraduationYear IS NOT NULL
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0
								THEN 1 
							END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedPersistedDataViews)
				OR EXISTS (
						SELECT 1
						FROM @IncludedPersistedDataViews dv
						INNER JOIN DataViewPersistedValue dpv
							ON dpv.DataViewId = dv.Id
						WHERE dpv.EntityId = p.Id
					)
			)

    END
    ELSE IF @PlacementMode = 'InstanceMode'
    BEGIN
		INSERT INTO @Registrants
		SELECT
			ri.Id AS RegistrationInstanceId,
			ri.Name AS RegistrationInstanceName,
			rr.CreatedDateTime,
			rr.PersonAliasId,
			rr.Id AS RegistrantId
		FROM RegistrationInstance ri
		INNER JOIN Registration r ON ri.Id = r.RegistrationInstanceId
		INNER JOIN RegistrationRegistrant rr ON r.Id = rr.RegistrationId
		WHERE ri.Id = @RegistrationInstanceId;

		;WITH IncludedFeeItemIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@IncludedFeeItemIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		),
		FeeData AS (
			SELECT
				rrf.RegistrationRegistrantId,
				rtf.Name AS FeeName,
				rtf.FeeType,
				rrf.[Option],
				rrf.Quantity,
				rrf.Cost,
				rtfi.Id AS FeeItemId
			FROM RegistrationRegistrantFee rrf
			INNER JOIN RegistrationTemplateFee rtf ON rtf.Id = rrf.RegistrationTemplateFeeId
			INNER JOIN RegistrationTemplateFeeItem rtfi ON rtfi.Id = rrf.RegistrationTemplateFeeItemId
			WHERE
				rrf.Quantity > 0
				AND @IncludeFees = 1
		)

        SELECT 
			g.GroupId, 
			g.GroupTypeId, 
			gm.Id AS GroupMemberId, 
			gm.GroupRoleId,
			gm.DateTimeAdded,
			p.Id AS PersonId, 
			p.FirstName, 
			p.NickName, 
			p.LastName, 
			p.Gender, 
			p.PhotoId, 
			p.Age, 
			p.RecordTypeValueId, 
			p.AgeClassification, 
			x.RegistrantId, 
			x.RegistrationInstanceName,
			x.RegistrationInstanceId, 
			x.CreatedDateTime, 
			fd.FeeName, 
			fd.[Option], 
			fd.Quantity,
			fd.Cost, 
			fd.FeeType, 
			fd.FeeItemId
        FROM (
			SELECT
				RegistrationInstanceId,
				RegistrationInstanceName,
				CreatedDateTime,
				PersonAliasId,
				RegistrantId
			FROM @Registrants
			UNION
			SELECT
				NULL,
				NULL,
				NULL,
				pa.Id,
				NULL
			FROM GroupMember gm
			JOIN #DestinationGroups dg ON gm.GroupId = dg.GroupId
			JOIN PersonAlias pa ON gm.PersonId = pa.PersonId
			WHERE NOT EXISTS (
				SELECT 1 
				FROM @Registrants r 
				WHERE r.PersonAliasId = pa.Id
			)
			AND gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
		) x -- Gets all people. (placed registrants, unplaced registrants, and placed non-registrants)
        INNER JOIN PersonAlias pa ON x.PersonAliasId = pa.Id
        INNER JOIN Person p ON pa.PersonId = p.Id
        LEFT JOIN GroupMember gm ON gm.PersonId = p.Id
            AND gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)
			AND gm.IsArchived = 0
			AND gm.GroupMemberStatus != 0
		LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
		LEFT JOIN FeeData fd ON fd.RegistrationRegistrantId = X.RegistrantId
		CROSS APPLY (
			SELECT CASE
				WHEN @FilterAppliesTo = 0 AND g.GroupId IS NULL THEN 1 -- unplaced subset
				WHEN @FilterAppliesTo = 1 AND g.GroupId IS NOT NULL THEN 1 -- placed subset
				WHEN @FilterAppliesTo = 2 THEN 1 -- all people
				ELSE 0
			END AS FilterTarget
		) ft
		WHERE (
			NOT EXISTS (SELECT 1 FROM IncludedFeeItemIds)
			OR EXISTS (
				SELECT 1
				FROM FeeData fd
				WHERE fd.RegistrationRegistrantId = X.RegistrantId
					AND fd.FeeItemId IN (SELECT Id FROM IncludedFeeItemIds)
			)
		 )
		 AND (
				ft.FilterTarget = 0        -- person is NOT part of the filtered subset => always allow
				OR @Gender IS NULL         -- no gender filter applied at all
				OR p.Gender = @Gender      -- person IS in the filtered subset => must match gender
		  )
		  AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedCampuses)
				OR p.PrimaryCampusId IN (SELECT Id FROM @IncludedCampuses)
		  )
		  AND (
				ft.FilterTarget = 0
				OR @AgeComparisonType IS NULL
				OR (
					@AgeComparisonType NOT IN (32, 64, 4096) -- NOT Is Null / Is Not Null / Between
					AND @AgeHigh IS NULL                     -- AND missing value => skip filter
				)
				OR (
					@AgeComparisonType = 4096
					AND (
						@AgeLow IS NULL 
						OR @AgeHigh IS NULL
					)
				)
				OR (
					CASE @AgeComparisonType
						WHEN 1      THEN CASE WHEN p.Age = @AgeHigh THEN 1 END
						WHEN 2      THEN CASE WHEN p.Age <> @AgeHigh THEN 1 END
						WHEN 128    THEN CASE WHEN p.Age > @AgeHigh THEN 1 END
						WHEN 256    THEN CASE WHEN p.Age >= @AgeHigh THEN 1 END
						WHEN 512    THEN CASE WHEN p.Age < @AgeHigh THEN 1 END
						WHEN 1024   THEN CASE WHEN p.Age <= @AgeHigh THEN 1 END
						WHEN 4096   THEN CASE WHEN p.Age BETWEEN @AgeLow AND @AgeHigh THEN 1 END
						WHEN 32     THEN CASE WHEN p.Age IS NULL THEN 1 END
						WHEN 64     THEN CASE WHEN p.Age IS NOT NULL THEN 1 END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR @GradeComparisonType IS NULL
				OR (
					@GradeComparisonType NOT IN (32, 64)
					AND @GradeOffset IS NULL
				)
				OR (
					CASE @GradeComparisonType
						-- Equal To
						WHEN 1 THEN 
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >  @NextGradeOffset 
								THEN 1 
							END
						-- Not Equal To
						WHEN 2 THEN
							CASE 
								WHEN ( (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									OR (p.GraduationYear - @CurrentSchoolYear) >  @GradeOffset )
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Less Than
						WHEN 512 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @GradeOffset 
								THEN 1 
							END
						-- Less Than Or Equal To
						WHEN 1024 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) > @NextGradeOffset 
								THEN 1 
							END
						-- Greater Than
						WHEN 128 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @NextGradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END

						-- Greater Than Or Equal To
						WHEN 256 THEN
							CASE 
								WHEN (p.GraduationYear - @CurrentSchoolYear) <= @GradeOffset
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0 
								THEN 1 
							END
						-- Is Blank
						WHEN 32 THEN
							CASE 
								WHEN p.GraduationYear IS NULL 
									OR (p.GraduationYear - @CurrentSchoolYear) < 0
								THEN 1 
							END

						-- Is Not Blank
						WHEN 64 THEN
							CASE 
								WHEN p.GraduationYear IS NOT NULL
									AND (p.GraduationYear - @CurrentSchoolYear) >= 0
								THEN 1 
							END
					END = 1
				)
			)
			AND (
				ft.FilterTarget = 0
				OR NOT EXISTS (SELECT 1 FROM @IncludedPersistedDataViews)
				OR EXISTS (
						SELECT 1
						FROM @IncludedPersistedDataViews dv
						INNER JOIN DataViewPersistedValue dpv
							ON dpv.DataViewId = dv.Id
						WHERE dpv.EntityId = p.Id
					)
			)
    END

	DROP TABLE #DestinationGroups;
END