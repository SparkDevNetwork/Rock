-- =====================================================================================================
-- Author:        Rock
-- Create Date:   04-04-24
-- Description:   Populates LearningProgram, Courses, Classes, Activities, Students, Facilitators 
--  and other data for a complete LMS test environment.
--
-- Change History:
--                 
-- ======================================================================================================
SET NOCOUNT ON
DECLARE
  @programsToCreate INT = 4,
  @maxCoursesPerProgram INT = 3,
  @maxClassesPerCourse INT = 3,
  @maxStudentsPerClass INT = 35,
  @maxFacilitatorsPerClass INT = 3,
  @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775',
  @programCommunicationGuid UNIQUEIDENTIFIER = '18f6d27b-2546-4b0d-b135-dd5a4311ad81';
BEGIN

    DECLARE
        @programsCreated INT = 0,
        @activeGroupMemberStatus INT = 1,
        @pendingProgramCompletion INT = 1,
        @classCompletionIncomplete INT = 1,
        @classCompletionFail INT = 2,
        @classCompletionPass INT = 3,
        @now DATETIMEOFFSET = SYSDATETIMEOFFSET(),
        @facilitatorRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A'),
        @studentRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2'),
        @programCommunicationId INT = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = @programCommunicationGuid),
        @numberOfGradingSystems INT = (SELECT MAX(Id) FROM LearningGradingSystem); -- Assumes no gaps in identity column values.

    SELECT @programsToCreate ParameterValue, 'programs'
    UNION SELECT @maxCoursesPerProgram ParameterValue, 'max courses per program'
    UNION SELECT @maxClassesPerCourse ParameterValue, 'max classes per course'
    UNION SELECT @maxStudentsPerClass ParameterValue, 'max students per class'
    UNION SELECT @maxFacilitatorsPerClass ParameterValue, 'max facilitators per class'

    DECLARE @lmsGroupType INT = (SELECT TOP 1 Id FROM GroupType WHERE [Guid] = @lmsGroupTypeGuid);

    WHILE @programsCreated < @programsToCreate
    BEGIN
            
        DECLARE @programSample TABLE (
            Id INT NOT NULL IDENTITY(1,1),
            ProgramName NVARCHAR(200),
            IconCssClass NVARCHAR(100),
            HighlightColor NVARCHAR(50),
            Summary NVARCHAR(1000)
        );

        INSERT @programSample (ProgramName, IconCssClass, HighlightColor, Summary)
        SELECT ProgramName, IconCssClass, HighlightColor, Summary 
        FROM (
            SELECT 'Online' ProgramName, 'fa fa-university' IconCssClass, '#357958' HighlightColor, 'Go at your own pace online study' Summary
            UNION SELECT 'Ministry Studies', 'fa fa-bible', '#735f95', 'Enhance your knowledge of Ministry.'
            UNION SELECT 'Extended Education', 'fa fa-graduation-cap', '#265e46', 'Gain the knowledge you need.'
            UNION SELECT 'Prophets', 'fa fa-eye', '#735f95', 'Experience the visions of the prophets.'
            UNION SELECT 'Service to Others', 'fa fa-hands-helping', '#e36c4d', 'Learn how you can help others.'
            UNION SELECT 'Spiritual Growth University', 'fa fa-university', '#357958', 'An immersive journey into deepening faith, understanding scripture, and fostering personal transformation through holistic Christian teachings.'
        ) s
        -- Don't re-use names that are in use already.
        WHERE NOT EXISTS (
            SELECT 1
            FROM [LearningProgram] [ex]
            WHERE [ex].Name = s.ProgramName
        )

        DECLARE @programNameCount INT = (SELECT MAX(Id) FROM @programSample);
            
        DECLARE 
            @programGuid UNIQUEIDENTIFIER = NEWID(),
            @configurationMode INT = FLOOR(RAND(CHECKSUM(NEWID())) * 2) + 1,
            @tracksCompletionStatus BIT = ROUND(RAND(CHECKSUM(NEWID())), 0);

        -- Insert the program using the values from the sample table plus other randomized values.
        INSERT [LearningProgram] ( [Name], [PublicName], [ConfigurationMode], [IsPublic], [IsActive], [IsCompletionStatusTracked], [SystemCommunicationId], [Guid], [Summary], [IconCssClass], [HighlightColor] )
        SELECT p.ProgramName, p.ProgramName, @configurationMode, 1, 1, @tracksCompletionStatus, @programCommunicationId, @programGuid, p.Summary, p.IconCssClass, p.HighlightColor
        FROM @programSample p
        WHERE p.Id = FLOOR(RAND(CHECKSUM(NEWID())) * @programNameCount) + 1
            AND NOT EXISTS (
            SELECT 1
            FROM [LearningProgram] [ex]
            WHERE [ex].[Guid] = @programGuid
        )

        -- Get the newly created ProgramId.
        DECLARE 
            @programId INT = IDENT_CURRENT ('LearningProgram'),
            @coursesCreated INT = 0,
            @coursesToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxCoursesPerProgram) + 1,
            @classesCreated INT = 0,
            @classesToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse) + 1,
            @nextYear INT = YEAR(@now) + 1,
            @semesterGuid UNIQUEIDENTIFIER = NEWID();

        PRINT CONCAT('Program Added: ', @programId)

        -- Add a Semester
        INSERT [LearningSemester] ( [Name], [LearningProgramId], [StartDate], [EndDate], [EnrollmentCloseDate], [Guid] )
        VALUES (CONCAT('SPRING ', @nextYear), @programId, CONCAT(@nextYear,'-01-01'), CONCAT(@nextYear,'-04-15'), CONCAT(YEAR(@now),'-12-31'), @semesterGuid)

        DECLARE @semesterId INT = IDENT_CURRENT ('LearningSemester');

        PRINT CONCAT('Semester Added: ', @semesterId)
        
        /* Add the Course for the program */
        WHILE @coursesCreated < @coursesToCreate
        BEGIN
            DECLARE @courseNumber INT = @coursesCreated + 1;
            DECLARE
                @courseGuid UNIQUEIDENTIFIER = NEWID(),
                @allowHistoricalAccess BIT = ROUND(RAND(CHECKSUM(NEWID())), 0),
                @gradingSystemId INT = FLOOR(RAND(CHECKSUM(NEWID())) * @numberOfGradingSystems) + 1,
                @courseName NVARCHAR(200) = CONCAT('Course ', @courseNumber),
                @courseCode NVARCHAR(10) = CONCAT(
                    (SELECT LEFT(Name, 3) FROM LearningProgram WHERE Id = @programId),
                    @courseNumber, '0', @courseNumber
                ),
                @credits INT = FLOOR(RAND(CHECKSUM(NEWID())) * 4) + 1;
            
            INSERT [LearningCourse] ( [Name], [PublicName], [CourseCode], [LearningProgramId], [Credits], [IsActive], [IsPublic], [Order], [MaxStudents], [SystemCommunicationId], [EnableAnnouncements], [AllowHistoricalAccess], [Guid], [Summary] )
            VALUES (@courseName, @courseName, @courseCode, @programId, @credits, 1, 1, @coursesCreated + 1, @maxStudentsPerClass,  @programCommunicationId, 0, @allowHistoricalAccess, @courseGuid, '')
            
            DECLARE @courseId INT = IDENT_CURRENT ('LearningCourse');

            PRINT CONCAT('Course Added: ', @courseId)
        
            /* Add the Groups, Classes, Students and Facilitators for the Course */
            WHILE @classesCreated < @classesToCreate
            BEGIN
            
                DECLARE 
                    @groupGuid UNIQUEIDENTIFIER = NEWID(),
                    @classNumber INT = @classesCreated + 1;

                -- Add the Group and LearningClass records for the program.
                INSERT [GROUP] ( [IsSystem], [GroupTypeId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid] )
                VALUES (0, @lmsGroupType, CONCAT('Class ', @classNumber), 0, 1, @classNumber, @groupGuid)
            
                DECLARE @groupClassId INT = IDENT_CURRENT ('Group');

                INSERT [LearningClass] ( [LearningCourseId], [LearningGradingSystemId], [LearningSemesterId], [Id] )
                VALUES ( @courseId, @gradingSystemId, @semesterId, @groupClassId)

                DECLARE
                    @participantsCreated INT = 0,
                    @studentsToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxStudentsPerClass) + 1,
                    @facilitatorsCreated INT = 0,
                    @facilitatorsToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxFacilitatorsPerClass) + 1;

                DECLARE @activities TABLE (
                    Name NVARCHAR(200),
                    AssignTo INT,
                    TypeId INT,
                    DueCalculationMethod INT,
                    [DueDateDefault] DATE, 
                    [DueDateOffset] INT,
                    AvailableCalculationMethod INT,
                    [AvailableDateDefault] DATE, 
                    [AvailableDateOffset] INT,
                    Points INT,
                    SortOrder INT,
                    IsStudentCommentingEnabled BIT,
                    [Guid] UNIQUEIDENTIFIER
                )

                DECLARE 
                    @specificDate INT = 1,
                    @classStartOffset INT = 2,
                    @enrollmentOffset INT = 3,
                    @neverDue INT = 4,
                    @availableAfterPreviousComplete INT = 4,
                    @alwaysAvailable INT = 5,
                    @videoActivityGuid UNIQUEIDENTIFIER = NEWID(),
                    @video2ActivityGuid UNIQUEIDENTIFIER = NEWID(),
                    @checkoffActivityGuid UNIQUEIDENTIFIER = NEWID(),
                    @facilitatorActivityGuid UNIQUEIDENTIFIER = NEWID(),
                    @classStart DATE = ISNULL((
                            select TOP 1 s.EffectiveStartDate
                            from GroupLocationSchedule gls
                            JOIN Schedule s on s.Id = gls.ScheduleId
                            JOIN GroupLocation gl ON gl.Id = gls.GroupLocationId
                            WHERE GroupId = @groupClassId
                        ),
                    @now),
                    @dateAfterToday DATE = DATEADD(
                        DAY, 
                        RAND(CHECKSUM(NEWID())) * (1+DATEDIFF(DAY, @now, DATEADD(MONTH, 1, @now))),
                        @now);

                /* Create the Activities for the Class */
                INSERT [LearningActivity] ( [LearningClassId], [Name], [Order], [ActivityComponentId], [AssignTo], [DueDateCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableDateCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], [Guid] )
                SELECT @groupClassId, seed.[Name], seed.[Order], [ActivityComponentId], [AssignTo], [DueCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], seed.[Guid]
                FROM (
                    SELECT 'Video Watch' [Name], 1 [Order], 1 [ActivityComponentId], 1 [AssignTo], @classStartOffset [DueCalculationMethod], NULL [DueDateDefault], 2 [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 5 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @videoActivityGuid [Guid]
                    UNION SELECT 'Check Off' [Name], 2 [Order], 2 [ActivityComponentId], 1 [AssignTo], @neverDue [DueCalculationMethod], NULL [DueDateDefault], NULL [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 10 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @checkoffActivityGuid [Guid]
                    UNION SELECT 'Review Projects' [Name], 3 [Order], 2 [ActivityComponentId], 2 [AssignTo], @neverDue [DueCalculationMethod], NULL [DueDateDefault], NULL [DueDateOffset], @availableAfterPreviousComplete [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 0 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @facilitatorActivityGuid [Guid]
                    UNION SELECT 'Video Watch 2' [Name], 4 [Order], 1 [ActivityComponentId], 1 [AssignTo], @specificDate [DueCalculationMethod], @dateAfterToday [DueDateDefault], NULL [DueDateOffset], @classStartOffset [AvailableCalculationMethod], NULL [AvailableDateDefault], 3 AvailableDateOffset, 5 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @video2ActivityGuid [Guid]
                ) seed
            
                /* Create students and facilitators for the class/group. */
                WHILE @participantsCreated < (@studentsToCreate + @facilitatorsToCreate)
                BEGIN
                    DECLARE @personId INT = (
                                SELECT TOP 1 Id 
                                FROM Person p
                                WHERE NOT EXISTS (
                                    SELECT 1 Id
                                    FROM [GroupMember] [ex]
                                    WHERE [ex].[PersonId] = p.Id
                                        AND [ex].[GroupId] = @groupClassId
                                )
                                -- Order by the random id first, but if it's used we'll take next person.
                                ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * (SELECT MAX(Id) FROM Person))), 1, 0), Id
                            );

                    DECLARE 
                        @groupMemberGuid UNIQUEIDENTIFIER = NEWID(),
                        @programCompletionId INT = (
                            SELECT TOP 1 c.[Id] 
                            FROM [LearningProgramCompletion] [c]
                            JOIN [PersonAlias] p ON p.Id = c.PersonAliasId
                            WHERE [c].LearningProgramId = @programId
                                AND [p].Id = @personId
                        ),
                        @participantRoleId INT = IIF(@facilitatorsToCreate < @facilitatorsCreated, @facilitatorRoleId, @studentRoleId),
                        @randomDate DATETIMEOFFSET = (
                            SELECT DATEADD(
                                    DAY, 
                                    RAND(CHECKSUM(NEWID())) * (1+DATEDIFF(DAY, DATEADD(MONTH, -1, @now), DATEADD(MONTH, 1, @now))),
                                    @now)
                        ),
                        @primaryAliasId INT = (
                            SELECT PrimaryAliasId 
                            FROM Person 
                            WHERE Id = @personId
                        );

                    IF @tracksCompletionStatus = 1 AND @programCompletionId IS NULL
                    BEGIN
                        INSERT [LearningProgramCompletion] ( [LearningProgramId], [PersonAliasId], [StartDate], [StartDateKey], [CompletionStatus], [Guid] )
                        VALUES ( @programId, @primaryAliasId, @randomDate, CONCAT(YEAR(@randomDate), MONTH(@randomDate), DAY(@randomDate)), @pendingProgramCompletion, NEWID() )

                        SET @programCompletionId = IDENT_CURRENT ('LearningProgramCompletion');
                    END 
                    
                    INSERT [GroupMember] ( [IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid], [GroupTypeId], [DateTimeAdded] )
                    VALUES (0, @groupClassId, @personId, @participantRoleId, @activeGroupMemberStatus, @groupMemberGuid, @lmsGroupType, @randomDate)
                    
                    DECLARE @groupMemberId INT = IDENT_CURRENT ('GroupMember');

                    INSERT [LearningParticipant] ( [LearningCompletionStatus], [LearningGradePercent], [LearningClassId],  [LearningProgramCompletionId], [Id] )
                    VALUES (@classCompletionIncomplete, 0, @groupClassId, @programCompletionId, @groupMemberId)

                    IF @participantRoleId = @facilitatorRoleId
                    BEGIN
                        SET @facilitatorsCreated = @facilitatorsCreated + 1;
                    END
                    ELSE 
                    BEGIN
                        
                        INSERT [LearningActivityCompletion] ( [LearningActivityId], [StudentId], [PointsEarned], [IsStudentCompleted], [IsFacilitatorCompleted], [WasCompletedOnTime], [Guid], [AvailableDate], [DueDate] )
                        SELECT [a].[Id], @groupMemberId, 0 [PointsEarned], 0 [IsStudentCompleted], 0 [IsFacilitatorCompleted], 0 [WasCompletedOnTime], NEWID(),
                            CASE 
                                WHEN AvailableDateCalculationMethod = @specificDate THEN AvailableDateDefault 
                                WHEN AvailableDateCalculationMethod = @classStartOffset THEN DATEADD(DAY, AvailableDateOffset, @classStart)
                                WHEN AvailableDateCalculationMethod = @enrollmentOffset THEN DATEADD(DAY, AvailableDateOffset, g.DateTimeAdded)
                                WHEN AvailableDateCalculationMethod = @availableAfterPreviousComplete THEN NULL
                                ELSE @now
                            END AvailableDate,
                            CASE 
                                WHEN DueDateCalculationMethod = @specificDate THEN DueDateDefault 
                                WHEN DueDateCalculationMethod = @classStartOffset THEN DATEADD(DAY, DueDateOffset, @classStart)
                                WHEN DueDateCalculationMethod = @enrollmentOffset THEN DATEADD(DAY, DueDateOffset, g.DateTimeAdded)
                                ELSE NULL
                            END DueDate
                        FROM LearningActivity a
                        JOIN GroupMember g ON g.Id = @groupMemberId
                        WHERE a.LearningClassId = @groupClassId
                    END

                    SET @participantsCreated = @participantsCreated + 1;
                END

                SET @classesCreated = @classesCreated + 1;
            END
            
            SET @coursesCreated = @coursesCreated + 1
        END

        SET @programsCreated = @programsCreated + 1;
    END
END

/*
    -- Script to clean-up based on pipe-delimited list of LearningProgramIds.
    DECLARE @pipeDelimitedProgramIds NVARCHAR(200) = '56|63|64|71|84|85|86';
    
    DELETE ac
    FROM LearningParticipant lp
    JOIN LearningClass lc on lc.Id = lp.LearningClassId
    JOIN LearningActivity a on a.LearningClassId = lp.LearningClassId
    JOIN LearningActivityCompletion ac ON ac.LearningActivityId = a.Id
    JOIN LearningCourse c on c.Id = lc.LearningCourseId
    WHERE c.LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE lp
    FROM LearningParticipant lp
    JOIN [GroupMember] gm on gm.Id = lp.Id
    JOIN LearningClass lc ON lc.Id = lp.LearningClassId
    JOIN LearningCourse c on c.Id = lc.LearningCourseId
    WHERE c.LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE la
    FROM LearningClass lc
    JOIN LearningCourse c on c.Id = lc.LearningCourseId
    JOIN LearningActivity la ON la.LearningClassId = lc.Id
    WHERE c.LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE lc
    FROM LearningClass lc
    JOIN LearningCourse c on c.Id = lc.LearningCourseId
    JOIN LearningSemester s on s.Id = lc.LearningSemesterId
    WHERE c.LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE s
    FROM LearningCourse c
    JOIN LearningClass lc ON lc.LearningCourseId = c.Id
    JOIN LearningSemester s on s.Id = lc.LearningSemesterId
    WHERE c.LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE LearningProgramCompletion
    WHERE LearningProgramId IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

    DELETE 
    FROM LearningProgram
    WHERE Id IN (SELECT TRIM(value) FROM string_split(@pipeDelimitedProgramIds, '|'))

*/