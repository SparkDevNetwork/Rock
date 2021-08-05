IF NOT EXISTS (
        SELECT *
        FROM StreakType
        WHERE [Guid] = 'B9FADD97-38A4-4141-B6DB-48154563A2A9'
        )
BEGIN
    INSERT INTO [dbo].[StreakType] (
        [Name]
        , [Description]
        , [StructureType]
        , [StructureEntityId]
        , [EnableAttendance]
        , [RequiresEnrollment]
        , [OccurrenceFrequency]
        , [StartDate]
        , [OccurrenceMap]
        , [IsActive]
        , [Guid]
        , [FirstDayOfWeek]
        )
    VALUES (
        'Weekly Attendance' --<Name, nvarchar(250),>
        , '' -- <Description, nvarchar(max),>
        , 0 --<StructureType, int,>
        , NULL -- <StructureEntityId, int,>
        , 1 --<EnableAttendance, bit,>
        , 0 --<RequiresEnrollment, bit,>
        , 1 --<OccurrenceFrequency, int,>
        , GetDate() -- <StartDate, date,>
        , 0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000003 --<OccurrenceMap, varbinary(max),>
        , 1 --<IsActive, bit,>
        , 'B9FADD97-38A4-4141-B6DB-48154563A2A9' --<Guid, uniqueidentifier,>
        , NULL -- <FirstDayOfWeek, int,>
        )
END
