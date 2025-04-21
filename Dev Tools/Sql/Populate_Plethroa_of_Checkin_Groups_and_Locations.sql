DECLARE @KioskCount INT = 50
DECLARE @LocationCount INT = 1250
DECLARE @GroupCount INT = 2500

DECLARE @LocationsPerKiosk INT = 5
DECLARE @LocationsPerGroup INT = 8
DECLARE @SchedulesPerGroupLocation INT = 3
DECLARE @CleanBeforeCreate BIT = 1

DECLARE @ForeignKey NVARCHAR(100) = 'PlethoraCheckinScript'

-- Should not be a need to modify anything below this line.
SET XACT_ABORT ON
SET NOCOUNT ON

BEGIN TRANSACTION

DECLARE @ScriptName NVARCHAR(100) = @ForeignKey
DECLARE @KioskDeviceValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'bc809626-1389-4543-b8bb-6fac79c27afd')
DECLARE @MainCampusId INT = (SELECT TOP 1 [Id] FROM [Campus] ORDER BY [Id])
DECLARE @MainCampusLocationId INT = (SELECT [LocationId] FROM [Campus] WHERE [Id] = @MainCampusId)
DECLARE @CampusBuildingGuid UNIQUEIDENTIFIER = '8ad242b1-e6d9-408f-abf1-f9a36f02aa7a'
DECLARE @ServiceScheduleCategoryId INT = (SELECT [Id] FROM [Category] WHERE [Guid] = '4fecc91b-83f9-4269-ae03-a006f401c47e')

DECLARE @CheckinGroupTypeIds TABLE ([Id] INT)
INSERT INTO @CheckinGroupTypeIds
    SELECT [Id] FROM [GroupType] WHERE [Guid] IN
    (
        'cadb2d12-7836-44bc-8eea-3c6ab22fd5e8' -- Nursery/Preschool Area
        , 'e3c8f7d6-5ceb-43bb-802f-66c3e734049e' -- Elementary Area
        , '7a17235b-69ad-439b-bab0-1a0a472db96f' -- Jr High Area
        , '9a88743b-f336-4404-b877-2a623689195d' -- High School Area
    )

DECLARE @GroupGuid UNIQUEIDENTIFIER
DECLARE @GroupId INT
DECLARE @GroupLocationGuid UNIQUEIDENTIFIER
DECLARE @GroupLocationId INT
DECLARE @CampusBuildingId INT
DECLARE @DeviceId INT

-----------------------------------------------------------
--
-- Generate all the Kiosk devices.
--

IF @CleanBeforeCreate = 1
BEGIN
    DELETE FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId
END

WHILE (SELECT COUNT(*) FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId) < @KioskCount
BEGIN
    INSERT INTO [Device]
    (
        [Guid]
        , [Name]
        , [Description]
        , [DeviceTypeValueId]
        , [PrintFrom]
        , [PrintToOverride]
        , [ForeignKey]
        , [IsActive]
        , [HasCamera]
    )
    VALUES
    (
        NEWID()
        , 'Generated Kiosk ' + CAST((SELECT COUNT(*) + 1 FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId) AS NVARCHAR(10))
        , 'Auto generated device from ' + @ScriptName
        , @KioskDeviceValueId
        , 1
        , 1
        , @ForeignKey
        , 1
        , 0
    )
END

-----------------------------------------------------------
--
-- Generate all the Locations.
--

IF @CleanBeforeCreate = 1
BEGIN
    DELETE FROM [Location] WHERE [ForeignKey] = @ForeignKey
END

IF (SELECT COUNT(*) FROM [Location] WHERE [Guid] = @CampusBuildingGuid) = 0
BEGIN
    INSERT INTO [Location]
    (
        [Guid]
        , [ParentLocationId]
        , [Name]
        , [IsActive]
        , [ForeignKey]
    )
    VALUES
    (
        @CampusBuildingGuid
        , @MainCampusLocationId
        , @ScriptName + ' Building'
        , 1
        , @ForeignKey
    )
END

SET @CampusBuildingId = (SELECT [Id] FROM [Location] WHERE [Guid] = @CampusBuildingGuid)

WHILE (SELECT COUNT(*) FROM [Location] WHERE [ForeignKey] = @ForeignKey) < @LocationCount
BEGIN
    INSERT INTO [Location]
    (
        [Guid]
        , [ParentLocationId]
        , [Name]
        , [IsActive]
        , [ForeignKey]
    )
    VALUES
    (
        NEWID()
        , @CampusBuildingId
        , 'Generated Location ' + CAST((SELECT COUNT(*) + 1 FROM [Location] WHERE [ForeignKey] = @ForeignKey) AS NVARCHAR(10))
        , 1
        , @ForeignKey
    )
END

-----------------------------------------------------------
--
-- Generate all the KioskLocations.
--

IF @CleanBeforeCreate = 1
BEGIN
    DELETE FROM [DeviceLocation] WHERE [DeviceId] IN (SELECT [Id] FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId)
END

SET @DeviceId = (SELECT TOP 1 [Id] FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId ORDER BY [Id])
WHILE @DeviceId IS NOT NULL
BEGIN
    WHILE (SELECT COUNT(*) FROM [DeviceLocation] WHERE [DeviceId] = @DeviceId) < @LocationsPerKiosk
    BEGIN
        INSERT INTO [DeviceLocation]
            (
                [DeviceId]
                , [LocationId]
            )
            VALUES
            (
                @DeviceId
                , (
                    SELECT TOP 1 [Id]
                    FROM [Location]
                    WHERE [ForeignKey] = @ForeignKey
                      AND [ParentLocationId] = @CampusBuildingId
                      AND [Id] NOT IN (SELECT [LocationId] FROM [DeviceLocation] WHERE [DeviceId] = @DeviceId)
                    ORDER BY NEWID()
                )
            )
    END

    -- Next device.
    SET @DeviceId = (SELECT TOP 1 [Id] FROM [Device] WHERE [ForeignKey] = @ForeignKey AND [DeviceTypeValueId] = @KioskDeviceValueId AND [Id] > @DeviceId ORDER BY [Id])
END

-----------------------------------------------------------
--
-- Generate all the check-in groups.
--

IF @CleanBeforeCreate = 1
BEGIN
    DELETE FROM [GroupLocation] WHERE [ForeignKey] = @ForeignKey
    DELETE FROM [Group] WHERE [ForeignKey] = @ForeignKey
END

WHILE (SELECT COUNT(*) FROM [Group] WHERE [ForeignKey] = @ForeignKey) < @GroupCount
BEGIN
    SET @GroupGuid = NEWID()

    INSERT INTO [Group]
    (
        [Guid]
        , [IsSystem]
        , [GroupTypeId]
        , [Name]
        , [Description]
        , [IsSecurityRole]
        , [IsActive]
        , [Order]
        , [ForeignKey]
        , [IsPublic]
        , [IsArchived]
        , [SchedulingMustMeetRequirements]
        , [AttendanceRecordRequiredForCheckIn]
        , [DisableScheduleToolboxAccess]
        , [DisableScheduling]
        , [ElevatedSecurityLevel]
    )
    VALUES
    (
        @GroupGuid
        , 0
        , (SELECT TOP 1 [Id] FROM @CheckinGroupTypeIds ORDER BY NEWID())
        , 'Generated Group ' + CAST((SELECT COUNT(*) + 1 FROM [Group] WHERE [ForeignKey] = @ForeignKey) AS NVARCHAR(10))
        , 'Auto generated device from ' + @ScriptName
        , 0
        , 1
        , 0
        , @ForeignKey
        , 1
        , 0
        , 0
        , 0
        , 0
        , 0
        , 0
    )

    SET @GroupId = (SELECT [Id] FROM [Group] WHERE [Guid] = @GroupGuid)

    -------------------------------------------------------
    --
    -- Generate all the group locations
    --
    WHILE (SELECT COUNT(*) FROM [GroupLocation] WHERE [GroupId] = @GroupId) < @LocationsPerGroup
    BEGIN
        SET @GroupLocationGuid = NEWID()

        INSERT INTO [GroupLocation]
        (
            [Guid]
            , [GroupId]
            , [LocationId]
            , [IsMailingLocation]
            , [IsMappedLocation]
            , [Order]
        )
        VALUES
        (
            @GroupLocationGuid
            , @GroupId
            , (SELECT TOP 1 [Id] FROM [Location] WHERE [ForeignKey] = @ForeignKey ORDER BY NEWID())
            , 0
            , 0
            , 0
        )

        SET @GroupLocationId = (SELECT [Id] FROM [GroupLocation] WHERE [Guid] = @GroupLocationGuid)

        ---------------------------------------------------
        --
        -- Generate all the group location schedules.
        --
        WHILE (SELECT COUNT(*) FROM [GroupLocationSchedule] WHERE [GroupLocationId] = @GroupLocationId) < @SchedulesPerGroupLocation
        BEGIN
            INSERT INTO [GroupLocationSchedule]
            (
                [GroupLocationId]
                , [ScheduleId]
            )
            VALUES
            (
                @GroupLocationId
                , (SELECT TOP 1 [Id] FROM [Schedule] WHERE [CategoryId] = @ServiceScheduleCategoryId AND [Id] NOT IN (SELECT [ScheduleId] FROM [GroupLocationSchedule] WHERE [GroupLocationId] = @GroupLocationId) ORDER BY NEWID())
            )
        END
    END
END

COMMIT TRANSACTION
