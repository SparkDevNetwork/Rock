/*
<doc>
    <summary>
        This stored procedure retrieves placed and unplaced people for all supported configurations
        of the Group Placement block in Rock RMS. It supports filtering by template, instance, groups,
        and entity sets.
    </summary>

    <returns>
        A result set containing placement people for group placements.
    </returns>

    <param name='@RegistrationTemplatePlacementEntityTypeId' datatype='int'>
		The EntityTypeId for RegistrationTemplatePlacement.
	</param>
    <param name='@RegistrationInstanceEntityTypeId' datatype='int'>
        The EntityTypeId for RegistrationInstance.
    </param>
    <param name='@PersonEntityTypeId' datatype='int'>
        The EntityTypeId for Person.
    </param>
    <param name='@SourceEntityTypeId' datatype='int'>
        Optional. The EntityTypeId for the source entity, such as a source group or entity set.
    </param>
    <param name='@TargetEntityTypeId' datatype='int'>
        The EntityTypeId for the target entity (usually groups).
    </param>
    <param name='@RegistrationTemplateId' datatype='int'>
        Optional. The ID of the registration template.
    </param>
    <param name='@RegistrationInstanceId' datatype='int'>
        Optional. The ID of the registration instance.
    </param>
    <param name='@RegistrationTemplatePlacementId' datatype='int'>
        Optional. The ID of a specific RegistrationTemplatePlacement.
    </param>
    <param name='@SourceEntityId' datatype='int'>
        Optional. The ID of the source entity (e.g., SourceGroupId or EntitySetId).
    </param>
    <param name='@PlacementMode' datatype='varchar(50)'>
        The placement mode: 
        - 'TemplateMode': Filter by template and placements
        - 'InstanceMode': Filter by specific registration instance
        - 'GroupMode': Use a group as the source.
        - 'EntitySetMode': Use an entity set of people as the source
    </param>
    <param name='@IncludedRegistrationInstanceIds' datatype='nvarchar(max)'>
        Optional. A comma-delimited list of RegistrationInstanceIds to include in the results.
    </param>
    <param name='@IncludeFees' datatype='bit'>
        If 1, includes registration fees in the filtering logic.
    </param>
    <param name='@IncludedFeeItemIds' datatype='nvarchar(max)'>
        Optional. A comma-delimited list of specific fee item IDs to include.
    </param>
    <param name='@DestinationGroupTypeId' datatype='int'>
        Optional. The GroupTypeId of groups that are eligible as placement destinations.
    </param>
    <param name='@DestinationGroupIds' datatype='nvarchar(max)'>
        Optional. A comma-delimited list of specific GroupIds that can be used as placement targets.
    </param>
    <param name='@DisplayedCampusGuid' datatype='uniqueidentifier'>
        Optional. If provided, filters people and/or destination groups by campus.
    </param>
    <param name='@PurposeKey' datatype='nvarchar(max)'>
        Optional. A high-level purpose key to scope placements.
    </param>
    <param name='@RegistrationTemplatePurposeKey' datatype='nvarchar(max)'>
        Optional. Purpose key for the RegistrationTemplate to further filter.
    </param>
    <param name='@RegistrationInstancePurposeKey' datatype='nvarchar(max)'>
        Optional. Purpose key for the RegistrationInstance to further filter.
    </param>

    <remarks>
        This procedure is used internally by the Group Placement block to retrieve people who are 
        candidates for group placement or people who have already been placed into any of the Destination
		Groups. The combination of input parameters determines the retrieval strategy.
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

ALTER PROCEDURE [dbo].[spGetGroupPlacementPeople]
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
	@RegistrationInstancePurposeKey NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #DestinationGroups (
        GroupId INT PRIMARY KEY,
        GroupTypeId INT
    );

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
		  );

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
		  );
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
				gm.CreatedDateTime
			FROM GroupMember gm
			INNER JOIN Person p ON gm.PersonId = p.Id
			LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
			WHERE gm.GroupId = @SourceEntityId
			  OR gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)

	END
	ELSE IF @PlacementMode = 'EntitySetMode'
    BEGIN
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
			esi.CreatedDateTime
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
			NULL AS CreatedDateTime
		FROM Person p
		INNER JOIN [GroupMember] gm ON p.Id = gm.PersonId
		INNER JOIN #DestinationGroups g ON gm.GroupId = g.GroupId

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
		) x -- Gets all people. (placed registrants, unplaced registrants, and placed non-registrants)
        INNER JOIN PersonAlias pa ON x.PersonAliasId = pa.Id
		INNER JOIN Person p ON pa.PersonId = p.Id
        LEFT JOIN GroupMember gm ON gm.PersonId = p.Id
            AND gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)
		LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
		LEFT JOIN FeeData fd ON fd.RegistrationRegistrantId = x.RegistrantId
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
		) x -- Gets all people. (placed registrants, unplaced registrants, and placed non-registrants)
        INNER JOIN PersonAlias pa ON x.PersonAliasId = pa.Id
        INNER JOIN Person p ON pa.PersonId = p.Id
        LEFT JOIN GroupMember gm ON gm.PersonId = p.Id
            AND gm.GroupId IN (SELECT GroupId FROM #DestinationGroups)
		LEFT JOIN #DestinationGroups g ON gm.GroupId = g.GroupId
		LEFT JOIN FeeData fd ON fd.RegistrationRegistrantId = X.RegistrantId
		WHERE (
			NOT EXISTS (SELECT 1 FROM IncludedFeeItemIds)
			OR EXISTS (
				SELECT 1
				FROM FeeData fd
				WHERE fd.RegistrationRegistrantId = X.RegistrantId
					AND fd.FeeItemId IN (SELECT Id FROM IncludedFeeItemIds)
			)
		 )
    END

	DROP TABLE #DestinationGroups;
END