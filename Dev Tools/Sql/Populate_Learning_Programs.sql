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
  @programsToCreate INT = 2,
  @maxCoursesPerProgram INT = 3,
  @maxClassesPerCourse INT = 3,
  @maxStudentsPerClass INT = 35,
  @maxFacilitatorsPerClass INT = 3,
  @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775',
  @programCommunicationGuid UNIQUEIDENTIFIER = '18f6d27b-2546-4b0d-b135-dd5a4311ad81';


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
    @maxUserId INT = (SELECT MAX(Id) FROM Person),
    @programCommunicationId INT = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = @programCommunicationGuid),
    @numberOfGradingSystems INT = (SELECT MAX(Id) FROM LearningGradingSystem); -- Assumes no gaps in identity column values.

SELECT @programsToCreate ParameterValue, 'programs' [Description], 3 [DisplayOrder]
UNION SELECT @maxCoursesPerProgram ParameterValue, 'max courses per program', 10
UNION SELECT @maxClassesPerCourse ParameterValue, 'max classes per course', 20
UNION SELECT @maxStudentsPerClass ParameterValue, 'max students per class', 30
UNION SELECT @maxFacilitatorsPerClass ParameterValue, 'max facilitators per class', 40
ORDER BY DisplayOrder

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
    -- Avoid reusing names.
    ORDER BY CASE WHEN NOT EXISTS (
        SELECT 1
        FROM [LearningProgram] [ex]
        WHERE [ex].Name = s.ProgramName
    ) THEN 0 ELSE 1 END

    DECLARE @programNameCount INT = (SELECT MAX(Id) FROM @programSample);
            
    DECLARE 
        @programGuid UNIQUEIDENTIFIER = NEWID(),
        @configurationMode INT = FLOOR(RAND(CHECKSUM(NEWID())) * 2) + 1,
        @tracksCompletionStatus BIT = ROUND(RAND(CHECKSUM(NEWID())), 0);

    -- Insert the program using the values from the sample table plus other randomized values.
    INSERT [LearningProgram] ( [Name], [PublicName], [ConfigurationMode], [IsPublic], [IsActive], [IsCompletionStatusTracked], [SystemCommunicationId], [Guid], [Summary], [IconCssClass], [HighlightColor] )
    SELECT TOP 1 p.ProgramName, p.ProgramName, @configurationMode, 1, 1, @tracksCompletionStatus, @programCommunicationId, @programGuid, p.Summary, p.IconCssClass, p.HighlightColor
    FROM @programSample p
    -- Avoid reusing names.
    ORDER BY CASE WHEN NOT EXISTS (
        SELECT 1
        FROM [LearningProgram] [ex]
        WHERE [ex].Name = p.ProgramName
    ) THEN 0 ELSE 1 END

    -- Get the newly created ProgramId.
    DECLARE @programId INT = SCOPE_IDENTITY();
    DECLARE
        @coursesCreated INT = 0,
        @coursesToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxCoursesPerProgram) + 1,
        @classesToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse) + 1,
        @nextYear INT = YEAR(@now) + 1,
        @semesterGuid UNIQUEIDENTIFIER = NEWID();

    PRINT CONCAT('Program Added with Id: ', @programId)

    /* Add a Semester */
    INSERT [LearningSemester] ( [Name], [LearningProgramId], [StartDate], [EndDate], [EnrollmentCloseDate], [Guid] )
    VALUES (CONCAT('SPRING ', @nextYear), @programId, CONCAT(@nextYear,'-01-01'), CONCAT(@nextYear,'-04-15'), CONCAT(YEAR(@now),'-12-31'), @semesterGuid)

    DECLARE @semesterId INT = SCOPE_IDENTITY();

    PRINT CONCAT('Semester Added with Id: ', @semesterId)
        
    /* Add the Course for the program */
    WHILE @coursesCreated < @coursesToCreate
    BEGIN
        DECLARE 
            @courseNumber INT = @coursesCreated + 1,
            @classesCreated INT = 0;

        DECLARE
            @courseGuid UNIQUEIDENTIFIER = NEWID(),
            @allowHistoricalAccess BIT = ROUND(RAND(CHECKSUM(NEWID())), 0),
            @gradingSystemId INT = FLOOR(RAND(CHECKSUM(NEWID())) * @numberOfGradingSystems) + 1,
            @courseName NVARCHAR(200) = CONCAT('Course ', @courseNumber),
            @courseCode NVARCHAR(10) = CONCAT(
                (
                    SELECT STRING_AGG(val, '')
                    FROM (
                        SELECT LEFT(value, 1) val
                        FROM string_split((SELECT [Name] FROM LearningProgram WHERE Id = @programId), ' ')
                    ) a
                ),
                @courseNumber, '0', @courseNumber
            ),
            @credits INT = FLOOR(RAND(CHECKSUM(NEWID())) * 4) + 1;
            
        INSERT [LearningCourse] ( [Name], [PublicName], [CourseCode], [LearningProgramId], [Credits], [IsActive], [IsPublic], [Order], [MaxStudents], [SystemCommunicationId], [EnableAnnouncements], [AllowHistoricalAccess], [Guid], [Summary] )
        VALUES (@courseName, @courseName, @courseCode, @programId, @credits, 1, 1, @coursesCreated + 1, @maxStudentsPerClass,  @programCommunicationId, 0, @allowHistoricalAccess, @courseGuid, '')
            
        DECLARE @courseId INT = SCOPE_IDENTITY();

        PRINT CONCAT('Course Added with Id: ', @courseId)
        
        /* Add the Groups, Classes, Students and Facilitators for the Course */
        WHILE @classesCreated < @classesToCreate
        BEGIN
            
            DECLARE 
                @groupGuid UNIQUEIDENTIFIER = NEWID(),
                @classNumber INT = @classesCreated + 1;

            /* Add the Group and LearningClass records for the program. */
            INSERT [GROUP] ( [IsSystem], [GroupTypeId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid] )
            VALUES (0, @lmsGroupType, CONCAT('Class ', @classNumber), 0, 1, @classNumber, @groupGuid)
            
            DECLARE @groupClassId INT = SCOPE_IDENTITY();

            INSERT [LearningClass] ( [LearningCourseId], [LearningGradingSystemId], [LearningSemesterId], [Id] )
            VALUES ( @courseId, @gradingSystemId, @semesterId, @groupClassId)

            PRINT CONCAT('Class Added with Id: ', @groupClassId);
            
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
            INSERT [LearningActivity] ( [LearningClassId], [Name], [Order], [ActivityComponentId], [AssignTo], [DueDateCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableDateCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], [Guid], [Description] )
            SELECT @groupClassId, seed.[Name], seed.[Order], [ActivityComponentId], [AssignTo], [DueCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], seed.[Guid], [Description]
            FROM (
                SELECT 'Video Watch' [Name], 1 [Order], 1 [ActivityComponentId], 1 [AssignTo], @classStartOffset [DueCalculationMethod], NULL [DueDateDefault], 2 [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 5 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @videoActivityGuid [Guid], '{"time":1714410593015,"blocks":[{"id":"nUecbc6V6Z","type":"paragraph","data":{"text":"Watch the video paying special attention as an assessment will be provided after completion."}}],"version":"2.28.0"}' [Description]
                UNION SELECT 'Check Off' [Name], 2 [Order], 2 [ActivityComponentId], 1 [AssignTo], @neverDue [DueCalculationMethod], NULL [DueDateDefault], NULL [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 10 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @checkoffActivityGuid [Guid], '{"time":1714410593015,"blocks":[{"id":"nUecbc6V6Z","type":"paragraph","data":{"text":"Certify that you''ve read and agree to the statement."}}],"version":"2.28.0"}'
                UNION SELECT 'Review Projects' [Name], 3 [Order], 2 [ActivityComponentId], 2 [AssignTo], @neverDue [DueCalculationMethod], NULL [DueDateDefault], NULL [DueDateOffset], @availableAfterPreviousComplete [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 0 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @facilitatorActivityGuid [Guid], '{"time":1714410593015,"blocks":[{"id":"nUecbc6V6Z","type":"paragraph","data":{"text":"Review the student submitted project for accuracy."}}],"version":"2.28.0"}'
                UNION SELECT 'Video Watch 2' [Name], 4 [Order], 1 [ActivityComponentId], 1 [AssignTo], @specificDate [DueCalculationMethod], @dateAfterToday [DueDateDefault], NULL [DueDateOffset], @classStartOffset [AvailableCalculationMethod], NULL [AvailableDateDefault], 3 AvailableDateOffset, 5 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @video2ActivityGuid [Guid], '{"time":1714410593015,"blocks":[{"id":"nUecbc6V6Z","type":"paragraph","data":{"text":"Watch the optional video."}}],"version":"2.28.0"}'
            ) seed
            
            /* Create students and facilitators for the class/group. */
            WHILE @participantsCreated < (@studentsToCreate + @facilitatorsToCreate)
            BEGIN
                DECLARE @personId INT = (
                    SELECT TOP 1 Id 
                    FROM Person p
                    WHERE p.PrimaryAliasId IS NOT NULL 
                        AND NOT EXISTS (
                            SELECT Id
                            FROM [GroupMember] [ex]
                            WHERE [ex].[PersonId] = p.Id
                                AND [ex].[GroupId] = @groupClassId
                        )
                    -- Order by the random id first, but if it's used we'll take next person.
                    ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxUserId)), 0, 1), Id
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
                    @participantRoleId INT = IIF(@facilitatorsCreated < @facilitatorsToCreate, @facilitatorRoleId, @studentRoleId),
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
                    
                    /* Add a Program Completion */
                    INSERT [LearningProgramCompletion] ( [LearningProgramId], [PersonAliasId], [StartDate], [StartDateKey], [CompletionStatus], [Guid] )
                    VALUES ( @programId, @primaryAliasId, @randomDate, CONCAT(YEAR(@randomDate), MONTH(@randomDate), DAY(@randomDate)), @pendingProgramCompletion, NEWID() )

                    SET @programCompletionId = SCOPE_IDENTITY();
                END 
                    
                /* Add a Participant as a GroupMember first (the table with the identity column) */
                INSERT [GroupMember] ( [IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid], [GroupTypeId], [DateTimeAdded] )
                VALUES (0, @groupClassId, @personId, @participantRoleId, @activeGroupMemberStatus, @groupMemberGuid, @lmsGroupType, @randomDate)
                    
                DECLARE @groupMemberId INT = SCOPE_IDENTITY();
                
                /* Add a Participant */
                INSERT [LearningParticipant] ( [LearningCompletionStatus], [LearningGradePercent], [LearningClassId],  [LearningProgramCompletionId], [Id] )
                VALUES (@classCompletionIncomplete, 0, @groupClassId, @programCompletionId, @groupMemberId)

                IF @participantRoleId = @facilitatorRoleId
                BEGIN
                    SET @facilitatorsCreated = @facilitatorsCreated + 1;
                END
                ELSE 
                BEGIN
                    /* Add some variability to the completions */
                    DECLARE @studentComment NVARCHAR(300) = IIF( @participantsCreated % 3 = 0, 'I was hoping to discuss this further', NULL);
                    DECLARE @facilitatorComment NVARCHAR(300) = IIF( @participantsCreated % 4 = 0, 'Excellent Work!', NULL);
                    
                    /* Add a Student Activity Completion */
                    INSERT [LearningActivityCompletion] ( [LearningActivityId], [StudentId], [IsStudentCompleted], [IsFacilitatorCompleted], [Guid]
                        , [AvailableDate], [DueDate], [StudentComment], [FacilitatorComment], [CompletedDate], [WasCompletedOnTime], [PointsEarned], [CompletedByPersonAliasId] )
                    SELECT [a].[Id], 
                        @groupMemberId, 
                        IIF(@groupMemberId % 5 IN (0, 1), 1, 0) [IsStudentCompleted],
                        0 [IsFacilitatorCompleted],
                        NEWID() [Guid],
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
                        END DueDate,
                        IIF(a.IsStudentCommentingEnabled = 1, @studentComment, NULL) StudentComment,
                        @facilitatorComment FacilitatorComment,

                        /* Every 5 activities mark one completed on time and one late  */
                        CASE 
                            WHEN @groupMemberId % 5 = 0 THEN  
                                /* DueDate - 24 hours and some additional random hours */
                                DATEADD(HOUR, -(24 + (FLOOR(RAND(CHECKSUM(NEWID())) * 20 + 7))), CASE 
                                    WHEN DueDateCalculationMethod = @specificDate THEN DueDateDefault 
                                    WHEN DueDateCalculationMethod = @classStartOffset THEN DATEADD(DAY, DueDateOffset, @classStart)
                                    WHEN DueDateCalculationMethod = @enrollmentOffset THEN DATEADD(DAY, DueDateOffset, g.DateTimeAdded)
                                    ELSE NULL
                                END)
                            WHEN @groupMemberId % 5 = 1 THEN  
                                /* DueDate + 1 */
                                DATEADD(HOUR, 24 + FLOOR(RAND(CHECKSUM(NEWID())) * 20 + 7), CASE 
                                    WHEN DueDateCalculationMethod = @specificDate THEN DueDateDefault 
                                    WHEN DueDateCalculationMethod = @classStartOffset THEN DATEADD(DAY, DueDateOffset, @classStart)
                                    WHEN DueDateCalculationMethod = @enrollmentOffset THEN DATEADD(DAY, DueDateOffset, g.DateTimeAdded)
                                    ELSE NULL
                            END)
                        END CompletedDate,
                    IIF( @groupMemberId % 5 = 0, 1, 0) WasCompletedOnTime,
                    IIF( @groupMemberId % 5 = 0, FLOOR(RAND(CHECKSUM(NEWID())) * a.Points), 0) PointedEarned,
                    IIF(@groupMemberId % 5 IN (0, 1), @primaryAliasId, NULL) CompletedByPersonAliasId

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

SELECT 
    p.[Name] [Program], 
    s.Name [Semester], 
    c.Name [Course], 
    COUNT(DISTINCT cl.Id) Classes, 
    COUNT(DISTINCT a.Id) Activities, 
    SUM(IIF(gm.GroupRoleId = @studentRoleId, 1, 0)) Students,
    SUM(IIF(gm.GroupRoleId = @facilitatorRoleId, 1, 0)) Facilitators
FROM LearningProgram p
JOIN LearningCourse c ON p.Id = c.LearningProgramId
LEFT JOIN LearningSemester s on s.LearningProgramId = p.Id
LEFT JOIN LearningClass cl on cl.LearningCourseId = c.Id
    AND cl.LearningSemesterId = s.Id
LEFT JOIN LearningActivity a ON a.LearningClassId = cl.Id
LEFT JOIN LearningParticipant lp on lp.LearningClassId = cl.Id
LEFT JOIN GroupMember gm ON gm.Id = lp.Id
GROUP BY p.[Name], s.Name, c.Name

/*
    -- Script to clean-up based on pipe-delimited list of LearningProgramIds.
        ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    ~~~~~~~~~~  Delete All Learning Programs by default         ~~~~~~~
    ~~~~~~~~~~  Verify the variable @pipeDelimitedProgramIds!   ~~~~~~~
        ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    DECLARE @pipeDelimitedProgramIds NVARCHAR(200) = (SELECT string_agg(Id, '|') FROM LearningProgram);
    
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
