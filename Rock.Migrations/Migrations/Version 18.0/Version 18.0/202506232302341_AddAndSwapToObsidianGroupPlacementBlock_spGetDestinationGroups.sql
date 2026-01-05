/*
<doc>
    <summary>
        Retrieves destination groups for use in the Group Placement block, supporting multiple modes
        of operation including TemplateMode, InstanceMode, GroupMode, and EntitySetMode. This 
		procedure determines which groups are available for placement, optionally filtered by related 
		entity mappings, group type, campus, or purpose keys.
    </summary>

    <returns>
        A result set of available destination groups with metadata including capacity, campus, 
        parent group, group type, and whether the group is shared.
    </returns>

    <param name='@RegistrationTemplatePlacementEntityTypeId' datatype='int'>
        The EntityTypeId for RegistrationTemplatePlacement used in related entity filters.
    </param>
    <param name='@RegistrationInstanceEntityTypeId' datatype='int'>
        The EntityTypeId for RegistrationInstance used in related entity filters.
    </param>
    <param name='@SourceEntityTypeId' datatype='int'>
        Optional. The EntityTypeId for the source entity (such as a SourceGroup or EntitySet).
    </param>
    <param name='@TargetEntityTypeId' datatype='int'>
        The EntityTypeId for the target entity (typically Group).
    </param>
    <param name='@RegistrationTemplateId' datatype='int'>
        Optional. The ID of the registration template used to scope placements.
    </param>
    <param name='@RegistrationInstanceId' datatype='int'>
        Optional. The ID of the specific registration instance.
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
        Optional. A comma-separated list of RegistrationInstance IDs to include.
    </param>
    <param name='@DestinationGroupTypeId' datatype='int'>
        Optional. Filters destination groups by GroupTypeId.
    </param>
    <param name='@DestinationGroupIds' datatype='nvarchar(max)'>
        Optional. A comma-separated list of GroupIds that are eligible for placement.
    </param>
    <param name='@DisplayedCampusGuid' datatype='uniqueidentifier'>
        Optional. Filters destination groups by Campus. If provided, only groups from that campus
        or with no campus are included.
    </param>
    <param name='@PurposeKey' datatype='nvarchar(max)'>
        Optional. The purpose key used to identify relevant related entities for placement in GroupMode/EntitySetMode.
    </param>
    <param name='@RegistrationTemplatePurposeKey' datatype='nvarchar(max)'>
        Optional. The purpose key for related entities scoped to RegistrationTemplatePlacement.
    </param>
    <param name='@RegistrationInstancePurposeKey' datatype='nvarchar(max)'>
        Optional. The purpose key for related entities scoped to RegistrationInstance.
    </param>

    <remarks>
        This procedure supports dynamic group selection strategies based on how group placements 
        are configured—whether tied directly to a group, an entity set, a template, or specific 
		registration instances.
    </remarks>

    <code>
        -- Example usage:
		EXEC dbo.spGetDestinationGroups
			@RegistrationTemplatePlacementEntityTypeId = 591,
			@RegistrationInstanceEntityTypeId = 260,
			@TargetEntityTypeId = 16,
			@RegistrationTemplateId = 3,
			@RegistrationTemplatePlacementId = 1,
			@PlacementMode = 'TemplateMode',
			@IncludedRegistrationInstanceIds = @IncludedIds,
			@DestinationGroupTypeId = 26,
			@RegistrationTemplatePurposeKey = 'PLACEMENT-TEMPLATE',
			@RegistrationInstancePurposeKey = 'PLACEMENT'
    </code>
</doc>
*/


CREATE PROCEDURE [dbo].[spGetDestinationGroups]
    @RegistrationTemplatePlacementEntityTypeId INT = NULL,
    @RegistrationInstanceEntityTypeId INT = NULL,
    @SourceEntityTypeId INT = NULL,
	@TargetEntityTypeId INT,
    @RegistrationTemplateId INT = NULL,
    @RegistrationInstanceId INT = NULL,
    @RegistrationTemplatePlacementId INT = NULL,
	@SourceEntityId INT = NULL,
    @PlacementMode VARCHAR(50),
    @IncludedRegistrationInstanceIds NVARCHAR(MAX) = NULL,
	@DestinationGroupTypeId INT = NULL,
	@DestinationGroupIds NVARCHAR(MAX) = NULL,
	@DisplayedCampusGuid UNIQUEIDENTIFIER = NULL,
	@PurposeKey NVARCHAR(MAX) = NULL,
	@RegistrationTemplatePurposeKey NVARCHAR(MAX) = NULL,
	@RegistrationInstancePurposeKey NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

	IF @PlacementMode = 'GroupMode' OR @PlacementMode = 'EntitySetMode'
	BEGIN
		;WITH IncludedDestinationGroupIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@DestinationGroupIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		)

		SELECT 
			g.Id AS GroupId, 
			g.Name AS GroupName, 
			g.Guid AS GroupGuid,
			g.ParentGroupId,
			g.CampusId,
			g.GroupCapacity, 
			g.GroupTypeId, 
			g.[Order] AS GroupOrder,
			CAST(0 AS BIT) AS IsShared,
			NULL AS RegistrationInstanceId
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

		;WITH IncludedIds AS (
			SELECT TRY_CAST(value AS INT) AS Id
			FROM STRING_SPLIT(@IncludedRegistrationInstanceIds, ',')
			WHERE TRY_CAST(value AS INT) IS NOT NULL
		)

		-- Pull destination groups from Template and Instance paths
		SELECT 
			g.Id AS GroupId, 
			g.Name AS GroupName, 
			g.Guid AS GroupGuid,
			g.ParentGroupId,
			g.CampusId,
			g.GroupCapacity, 
			g.GroupTypeId, 
			g.[Order] AS GroupOrder,
			CAST(1 AS BIT) AS IsShared,
			NULL AS RegistrationInstanceId
		FROM RegistrationTemplatePlacement rtp
		INNER JOIN RelatedEntity re ON rtp.Id = re.SourceEntityId
		INNER JOIN [Group] g ON re.TargetEntityId = g.Id
		LEFT JOIN Campus c ON g.CampusId = c.Id
		WHERE re.SourceEntityTypeId = @RegistrationTemplatePlacementEntityTypeId
		  AND re.PurposeKey = @RegistrationTemplatePurposeKey
		  AND re.TargetEntityTypeId = @TargetEntityTypeId
		  AND rtp.RegistrationTemplateId = @RegistrationTemplateId
		  AND rtp.Id = @RegistrationTemplatePlacementId
		  AND g.GroupTypeId = @DestinationGroupTypeId
		  AND (
			@DisplayedCampusGuid IS NULL
			OR c.Guid = @DisplayedCampusGuid
			OR g.CampusId IS NULL
		  )

		UNION

		SELECT 
			g.Id AS GroupId, 
			g.Name AS GroupName, 
			g.Guid AS GroupGuid,
			g.ParentGroupId,
			g.CampusId,
			g.GroupCapacity, 
			g.GroupTypeId, 
			g.[Order] AS GroupOrder,
			CAST(0 AS BIT) AS IsShared,
			ri.Id AS RegistrationInstanceId
		FROM RegistrationInstance ri
		INNER JOIN RelatedEntity re ON ri.Id = re.SourceEntityId
		INNER JOIN [Group] g ON re.TargetEntityId = g.Id
		LEFT JOIN Campus c ON g.CampusId = c.Id
		WHERE re.SourceEntityTypeId = @RegistrationInstanceEntityTypeId
		  AND re.TargetEntityTypeId = @TargetEntityTypeId
		  AND re.QualifierValue = @RegistrationTemplatePlacementId
		  AND re.PurposeKey = @RegistrationInstancePurposeKey
		  AND ri.RegistrationTemplateId = @RegistrationTemplateId
		  AND g.GroupTypeId = @DestinationGroupTypeId
		  AND (
				@PlacementMode = 'TemplateMode' 
				OR ri.Id = @RegistrationInstanceId
			  )
		  AND (
				@PlacementMode != 'TemplateMode'
				OR NOT EXISTS (SELECT 1 FROM IncludedIds)
				OR ri.Id IN (SELECT Id FROM IncludedIds)
			  )
		  AND (
			@DisplayedCampusGuid IS NULL
			OR c.Guid = @DisplayedCampusGuid
			OR g.CampusId IS NULL
		  );
	END
END
