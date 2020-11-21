-- add and assign a Group of GroupType 'Campus Team' to all existing Campuses

IF OBJECT_ID('tempdb..#campusTemp') IS NOT NULL
BEGIN
    DROP TABLE #campusTemp;
END

CREATE TABLE #campusTemp (
    [Id] [int] IDENTITY(1,1) NOT NULL
    , [CampusId] [int] NOT NULL
    , [CampusName] [nvarchar](100) NOT NULL
    , CONSTRAINT [pk_campusTemp] PRIMARY KEY CLUSTERED ( [Id] ASC )
);

-- collect all existing Campuses (that don't already have a TeamGroup assigned)
INSERT INTO #campusTemp
SELECT [Id]
    , [Name]
FROM [Campus]
WHERE ([TeamGroupId] IS NULL);

DECLARE @CampusCount [int] = (SELECT COUNT([Id]) FROM #campusTemp);
--PRINT N' >>> Found ' + CAST(@CampusCount AS nvarchar(10)) + N' Campuses without a TeamGroup assigned.'

DECLARE @CampusId [int]
    , @CampusName [nvarchar] (100)
    , @TeamGroupName [nvarchar] (100)
    , @TeamGroupId [int]
    , @GroupTypeId [int] = (SELECT [Id] FROM [GroupType] WHERE ([Guid] = 'BADD7A6C-1FB3-4E11-A721-6D1377C6958C'))
    , @TempId [int] = 1
    , @MaxTempId [int] = (SELECT MAX([Id]) FROM #campusTemp);

-- loop through each Campus and add/assign a new Group
IF (@GroupTypeId IS NOT NULL)
BEGIN
    WHILE (@TempId <= @MaxTempId)
    BEGIN
        SELECT @CampusId = [CampusId]
            , @CampusName = [CampusName]
        FROM #campusTemp
        WHERE ([Id] = @TempId);

        -- ensure we don't exceed the 100 character limit of the [Group].[Name] field
        SET @TeamGroupName = LEFT(@CampusName, 95) + ' Team';

        -- add new Group
        INSERT INTO [Group]
        (
            [IsSystem]
            , [GroupTypeId]
            , [Name]
            , [Description]
            , [IsSecurityRole]
            , [IsActive]
            , [Order]
            , [Guid]
            , [IsPublic]
            , [IsArchived]
            , [SchedulingMustMeetRequirements]
            , [AttendanceRecordRequiredForCheckIn]
        )
        VALUES
        (
            1
            , @GroupTypeId
            , @TeamGroupName
            , 'Are responsible for leading and administering the Campus.'
            , 0
            , 1
            , 0
            , NEWID()
            , 1
            , 0
            , 0
            , 0
        );

        SET @TeamGroupId = (SELECT SCOPE_IDENTITY());

        --PRINT N' >>> Added a new Group with [Id] = ' + CAST(@TeamGroupId AS nvarchar(10)) + N' and [Name] = "' + @TeamGroupName + '".';

        -- assign newly-created Group to current Campus
        UPDATE [Campus]
        SET [TeamGroupId] = @TeamGroupId
        WHERE ([Id] = @CampusId);

        --PRINT N' >>> Assigned newly-added "' + @TeamGroupName + N'" Group ([Group].[Id] = ' + CAST(@TeamGroupId AS nvarchar(10)) + N') to "' + @CampusName + N'" Campus ([Campus].[Id] = ' + CAST(@CampusId as nvarchar(10)) + N').';

        SET @TempId = @TempId + 1;
    END
END

DROP TABLE #campusTemp;
