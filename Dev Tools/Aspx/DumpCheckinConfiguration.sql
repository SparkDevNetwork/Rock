--
-- This script is designed to build a small export of all check-in configuration
-- data. It would then be used by a small block on a dev server to import the
-- configuration for testing.
--

DECLARE @Json nvarchar(max) = '{}'

-- Export all the Devices.
SET @Json = JSON_MODIFY(@Json, '$.Devices', (
    SELECT
        [D].[Name]
        , [D].[Guid]
        , [DeviceType].[Guid] AS [DeviceTypeValueGuid]
        , [PrinterDevice].[Guid] AS [PrinterDeviceGuid]
        , [D].[IsActive]
    FROM [Device] AS [D]
    INNER JOIN [DefinedValue] AS [DeviceType] ON [DeviceType].[Id] = [D].[DeviceTypeValueId]
    LEFT OUTER JOIN [Device] AS [PrinterDevice] ON [PrinterDevice].[Id] = [D].[PrinterDeviceId]
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the named Locations.
SET @Json = JSON_MODIFY(@Json, '$.Locations', (
    SELECT
        [L].[Guid]
        , [L].[Name]
        , [ParentLocation].[Guid] AS [ParentLocationGuid]
        , [L].[IsActive]
        , [L].[SoftRoomThreshold]
        , [L].[FirmRoomThreshold]
        , [P].[Guid] AS [PrinterDeviceGuid]
    FROM [Location] AS [L]
    LEFT OUTER JOIN [Location] AS [ParentLocation] ON [ParentLocation].[Id] = [L].[ParentLocationId]
    LEFT OUTER JOIN [Device] AS [P] ON [P].[Id] = [L].[PrinterDeviceId]
    WHERE ISNULL([L].[Name], '') != ''
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the DeviceLocations.
SET @Json = JSON_MODIFY(@Json, '$.DeviceLocations', (
    SELECT
        [D].[Guid] AS [DeviceGuid]
        , [L].[Guid] AS [LocationGuid]
    FROM [DeviceLocation] AS [DL]
    INNER JOIN [Device] AS [D] ON [D].[Id] = [DL].[DeviceId]
    INNER JOIN [Location] AS [L] ON [L].[Id] = [DL].[LocationId]
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the Campuses.
SET @Json = JSON_MODIFY(@Json, '$.Campuses', (
    SELECT
        [C].[Guid]
        , [C].[Name]
        , [C].[ShortCode]
        , [L].[Guid] AS [LocationGuid]
        , [C].[IsActive]
        , [C].[Order]
        , [CampusStatusValue].[Guid] AS [CampusStatusValueGuid]
        , [CampusTypeValue].[Guid] AS [CampusTypeValueGuid]
    FROM [Campus] AS [C]
    LEFT OUTER JOIN [Location] AS [L] ON [L].[Id] = [C].[LocationId]
    LEFT OUTER JOIN [DefinedValue] AS [CampusStatusValue] ON [CampusStatusValue].[Id] = [C].[CampusStatusValueId]
    LEFT OUTER JOIN [DefinedValue] AS [CampusTypeValue] ON [CampusTypeValue].[Id] = [C].[CampusTypeValueId]
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the schedule categories.
SET @Json = JSON_MODIFY(@Json, '$.ScheduleCategories', (
    SELECT
        [C].[Guid]
        , [C].[Name]
        , [ParentCategory].[Guid] AS [ParentCategoryGuid]
        , [C].[Order]
    FROM [Category] AS [C]
    LEFT OUTER JOIN [Category] AS [ParentCategory] ON [ParentCategory].[Id] = [C].[ParentCategoryId]
    INNER JOIN [EntityType] AS [ET] On [ET].[Id] = [C].[EntityTypeId]
    WHERE [ET].[Name] = 'Rock.Model.Schedule'
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the named Schedules.
SET @Json = JSON_MODIFY(@Json, '$.Schedules', (
    SELECT
        [S].[Guid]
        , [S].[Name]
        , [S].[iCalendarContent]
        , [S].[CheckInStartOffsetMinutes]
        , [S].[CheckInEndOffsetMinutes]
        , [S].[EffectiveStartDate]
        , [S].[EffectiveEndDate]
        , [Category].[Guid] AS [CategoryGuid]
        , [S].[IsActive]
        , [S].[Order]
    FROM [Schedule] AS [S]
    LEFT OUTER JOIN [Category] ON [Category].[Id] = [S].[CategoryId]
    WHERE ISNULL([S].[Name], '') != ''
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the GroupTypes related to check-in.
DECLARE @GroupTypeIds TABLE ([Id] INT)
INSERT INTO @GroupTypeIds
SELECT
    [GT].[Id]
FROM [GroupType] AS [GT]
INNER JOIN [DefinedValue] AS [GroupTypePurposeValue] ON [GroupTypePurposeValue].[Id] = [GT].[GroupTypePurposeValueId]
WHERE [GroupTypePurposeValue].[Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01'

DECLARE @LastGroupTypeCount INT = 0
WHILE (1 = 1)
BEGIN
    INSERT INTO @GroupTypeIds
    SELECT
        [GTA].[ChildGroupTypeId]
    FROM [GroupTypeAssociation] AS [GTA]
    INNER JOIN [GroupType] AS [ChildGroupType] ON [ChildGroupType].[Id] = [GTA].[ChildGroupTypeId]
    LEFT OUTER JOIN [DefinedValue] AS [ChildGroupTypePurposeValue] ON [ChildGroupTypePurposeValue].[Id] = [ChildGroupType].[GroupTypePurposeValueId]
    WHERE [GTA].[GroupTypeId] IN (SELECT [Id] FROM @GroupTypeIds)
      AND [GTA].[ChildGroupTypeId] NOT IN (SELECT [Id] FROM @GroupTypeIds)
      AND ([ChildGroupType].[GroupTypePurposeValueId] IS NULL OR [ChildGroupTypePurposeValue].[Guid] != '6BCED84C-69AD-4F5A-9197-5C0F9C02DD34')

    if (@@ROWCOUNT = 0)
        BREAK
END

SET @Json = JSON_MODIFY(@Json, '$.GroupTypes', (
    SELECT
        [GT].[Guid]
        , [GT].[Name]
        , [GT].[AttendanceRule]
        , [GT].[AttendancePrintTo]
        , [GT].[Order]
        , [GroupTypePurposeValue].[Guid] AS [GroupTypePurposeValueGuid]
        , [InheritedGroupType].[Guid] AS [InheritedGroupTypeGuid]
        , [GT].[LocationSelectionMode]
        , [GT].[AttendanceCountsAsWeekendService]
        , (
            SELECT
                [A].[Key]
                , [A].[Guid] AS [AttributeGuid]
                , [AV].[Value]
            FROM [AttributeValue] AS [AV]
            INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
            INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
            WHERE [ET].[Name] = 'Rock.Model.GroupType' AND [EntityId] = [GT].[Id]
            ORDER BY [AV].[EntityId], [A].[Key]
            FOR JSON PATH, INCLUDE_NULL_VALUES) AS [AttributeValues]
    FROM [GroupType] AS [GT]
    LEFT OUTER JOIN [GroupType] AS [InheritedGroupType] ON [InheritedGroupType].[Id] = [GT].[InheritedGroupTypeId]
    LEFT OUTER JOIN [DefinedValue] AS [GroupTypePurposeValue] ON [GroupTypePurposeValue].[Id] = [GT].[GroupTypePurposeValueId]
    WHERE [GT].[Id] IN (SELECT [Id] FROM @GroupTypeIds)
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the GroupTypeAssociations.
SET @Json = JSON_MODIFY(@Json, '$.GroupTypeAssociations', (
    SELECT
        [GT].[Guid] AS [GroupTypeGuid]
        , [ChildGroupType].[Guid] AS [ChildGroupTypeGuid]
    FROM [GroupTypeAssociation] AS [GTA]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [GTA].[GroupTypeId]
    INNER JOIN [GroupType] AS [ChildGroupType] ON [ChildGroupType].[Id] = [GTA].[ChildGroupTypeId]
    WHERE [GTA].[ChildGroupTypeId] IN (SELECT [Id] FROM @GroupTypeIds)
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the Groups related to check-in.
SET @Json = JSON_MODIFY(@Json, '$.Groups', (
    SELECT
        [G].[Guid]
        , [G].[Name]
        , [ParentGroup].[Guid] AS [ParentGroupGuid]
        , [GT].[Guid] AS [GroupTypeGuid]
        , [G].[IsActive]
        , [G].[Order]
        , (
            SELECT
                [A].[Key]
                , [A].[Guid] AS [AttributeGuid]
                , [AV].[Value]
            FROM [AttributeValue] AS [AV]
            INNER JOIN [Attribute] AS [A] ON [A].[Id] = [AV].[AttributeId]
            INNER JOIN [EntityType] AS [ET] ON [ET].[Id] = [A].[EntityTypeId]
            WHERE [ET].[Name] = 'Rock.Model.Group' AND [EntityId] = [G].[Id]
            ORDER BY [AV].[EntityId], [A].[Key]
            FOR JSON PATH, INCLUDE_NULL_VALUES) AS [AttributeValues]
    FROM [Group] AS [G]
    INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [G].[GroupTypeId]
    LEFT OUTER JOIN [Group] AS [ParentGroup] ON [ParentGroup].[Id] = [G].[ParentGroupId]
    WHERE [G].[GroupTypeId] IN (SELECT [Id] FROM @GroupTypeIds)
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the named GroupLocations.
SET @Json = JSON_MODIFY(@Json, '$.GroupLocations', (
    SELECT
        [GL].[Guid]
        , [G].[Guid] AS [GroupGuid]
        , [L].[Guid] AS [LocationGuid]
        , [GL].[Order]
    FROM [GroupLocation] AS [GL]
    INNER JOIN [Group] AS [G] ON [G].[Id] = [GL].[GroupId]
    INNER JOIN [Location] AS [L] ON [L].[Id] = [GL].[LocationId]
    WHERE ISNULL([L].[Name], '') != ''
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


-- Export all the GroupLocationSchedules for GroupLocations we have exported.
SET @Json = JSON_MODIFY(@Json, '$.GroupLocationSchedules', (
    SELECT
        [GL].[Guid] AS [GroupLocationGuid]
        , [S].[Guid] AS [ScheduleGuid]
    FROM [GroupLocationSchedule] AS [GLS]
    INNER JOIN [GroupLocation] AS [GL] ON [GL].[Id] = [GLS].[GroupLocationId]
    INNER JOIN [Schedule] AS [S] ON [S].[Id] = [GLS].[ScheduleId]
    WHERE [GL].[Guid] IN (SELECT [Guid] FROM OPENJSON(JSON_QUERY(@Json, '$.GroupLocations')) WITH ([Guid] UNIQUEIDENTIFIER '$.Guid'))
    FOR JSON PATH, INCLUDE_NULL_VALUES
))


SELECT @Json
