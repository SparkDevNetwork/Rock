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
IF OBJECT_ID('tempdb..#classes') IS NOT NULL
    DROP TABLE #classes;

IF OBJECT_ID('tempdb..#participants') IS NOT NULL
    DROP TABLE #participants;

IF OBJECT_ID('tempdb..#numbers') IS NOT NULL
    DROP TABLE #numbers;

with numbersToAHundred as (
    select 1 as [Number]
    union all
    select [Number] + 1
    from [numbersToAHundred]
    where [Number] < 100
)

SELECT *
INTO #numbers
FROM numbersToAHundred

DECLARE
    @now DATETIMEOFFSET = SYSDATETIME(),
    @passFailGradingSystemId INT = 1,
    @letterGradeGradingSystemId INT = 2,
    @maxClassesPerCourse INT = 3,
    @maxStudentsPerClass INT = 35,
    @maxFacilitatorsPerClass INT = 3,
    @lmsGroupTypeGuid uniqueidentifier = '4BBC41E2-0A37-4289-B7A7-756B9FE8F775',
    @programCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870',
    @introToNewTestamentCourtseDescription NVARCHAR(MAX) = '{"time":1716417751238,"blocks":[{"id":"OUS0UaeEE7","type":"header","data":{"text":"Course Description:","level":2}},{"id":"Ei258NjQ45","type":"paragraph","data":{"text":"\"Introduction to the New Testament\" invites students on a journey through the sacred Christian scriptures, exploring the origins, context, and central messages of the New Testament. Spanning from the life and teachings of Jesus Christ, as depicted in the Gospels, to the establishment and growth of the early Christian church in the Epistles, and culminating with the visionary Apocalypse of John, this course provides a comprehensive overview of the New Testament''s twenty-seven books."}},{"id":"kXMdRcD45a","type":"paragraph","data":{"text":"Throughout the semester, students will:"}},{"id":"aTBcx6k2Ju","type":"list","data":{"style":"unordered","items":[{"content":"Gain an understanding of the historical and socio-cultural settings in which the New Testament was written. ","items":[]},{"content":"Engage in close readings of key passages, deepening their appreciation for the literary and theological richness of these ancient texts. ","items":[]},{"content":"Familiarize themselves with critical issues of the New Testament scholarship, including questions of authorship, dating, and textual criticism. ","items":[]},{"content":"Reflect on the enduring significance of the New Testament, its role in shaping Christian theology, and its relevance to contemporary religious and ethical discussions.","items":[]}]}},{"id":"0bk3M95tHi","type":"header","data":{"text":"Course Structure:","level":3}},{"id":"kgRg6FX27t","type":"paragraph","data":{"text":"The course is divided into three primary sections:"}},{"id":"l6KZ4lc3j7","type":"list","data":{"style":"ordered","items":[{"content":"The Gospels and Acts: A study of Jesus'' life, teachings, death, resurrection, and the early days of the Christian movement.","items":[]},{"content":"The Pauline and General Epistles: Exploration of letters addressing theology, pastoral concerns, and general guidance for early Christian communities.","items":[]},{"content":"Apocalyptic and Johannine Literature: An examination of the Gospel of John, the three Johannine epistles, and the Book of Revelation, with a focus on their distinct theological perspectives and literary styles.","items":[]}]}},{"id":"3W9lneh-jF","type":"header","data":{"text":"Required Texts:","level":3}},{"id":"XLWptJG_e1","type":"paragraph","data":{"text":"\"The New Oxford Annotated Bible: New Revised Standard Version with the Apocrypha.\" Oxford University Press.Ehrman, Bart D. \"The New Testament: A Historical Introduction to the Early Christian Writings.\" Oxford University Press.&nbsp; &nbsp;"}}],"version":"2.28.0"}'
    ;

DECLARE
    @academicCalendarMode INT = 0,
    @onDemandMode INT = 1,
    @programsCreated INT = 0,
    @nextYear INT = YEAR(@now) + 1,
    @activeGroupMemberStatus INT = 1,
    @pendingProgramCompletion INT = 0,
    @classCompletionIncomplete INT = 0,
    @classCompletionFail INT = 1,
    @classCompletionPass INT = 2,
    @facilitatorRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A'),
    @studentRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2'),
    @maxUserId INT = (SELECT MAX(Id) FROM Person),
    @programCommunicationId INT = (SELECT [Id] FROM [SystemCommunication] WHERE [Guid] = @programCommunicationGuid);

SELECT @maxClassesPerCourse ParameterValue, 'max classes per course' [Description], 3 [DisplayOrder]
UNION SELECT @maxStudentsPerClass ParameterValue, 'max students per class', 30
UNION SELECT @maxFacilitatorsPerClass ParameterValue, 'max facilitators per class', 40
ORDER BY DisplayOrder

/* 
    Create the sample programs. 
    Because we'll be referencing these later we'll store them in a table variable.
*/
DECLARE @programSample TABLE (
    ProgramName NVARCHAR(200) NOT NULL,
    IconCssClass NVARCHAR(100),
    HighlightColor NVARCHAR(50),
    TracksCompletionStatus BIT NOT NULL, 
    ConfigurationMode BIT NOT NULL,
    SystemCommunicationId INT NOT NULL,
    Summary NVARCHAR(1000),
    [Guid] uniqueidentifier NOT NULL
);

INSERT @programSample (ProgramName, IconCssClass, HighlightColor, Summary, TracksCompletionStatus, ConfigurationMode, SystemCommunicationId, [Guid])
SELECT ProgramName, IconCssClass, HighlightColor, Summary, TracksCompletionStatus, ConfigurationMode, CommunicationId, [Guid]
FROM (
    SELECT  'Spiritual Growth University' ProgramName, 'fa fa-university' IconCssClass, '#357958' HighlightColor, 0 TracksCompletionStatus, @academicCalendarMode ConfigurationMode, @programCommunicationId CommunicationId, 'An immersive journey into deepening faith, understanding scripture, and fostering personal transformation through holistic Christian teachings.' Summary, '75305EA6-ACE9-4E49-AA70-191FC21C3C83' [Guid]
    UNION SELECT 'Biblical Studies', 'fa fa-book', '#265e46', 1, @academicCalendarMode, @programCommunicationId, 'Embark on our Biblical Studies series, an immersive exploration into the scriptures, offering profound insights and understanding of Christianity''s foundational texts.', '21AD5771-424C-43D9-9D7B-0B1A7C38E90A'
    UNION SELECT 'Foster Care Training', 'fa fa-hands-helping', '#e36c4d', 0, @onDemandMode, @programCommunicationId, 'Dive into our Foster Care training series, designed to equip potential foster parents with the essential skills and knowledge to provide a nurturing environment for children in need.', '28B3E301-7AA7-40B5-8945-E6406740E48B'
) s

INSERT [LearningProgram] ( [Name], [PublicName], [ConfigurationMode], [IsPublic], [IsActive], [IsCompletionStatusTracked], [SystemCommunicationId], [Guid], [Summary], [IconCssClass], [HighlightColor] )
SELECT p.[ProgramName], p.[ProgramName], [ConfigurationMode], 1, 1, [TracksCompletionStatus], [SystemCommunicationId], p.[Guid], p.[Summary], p.[IconCssClass], p.[HighlightColor]
FROM @programSample p
WHERE NOT EXISTS (
    SELECT 1
    FROM LearningProgram ex
    WHERE ex.[Guid] = p.[Guid]
)

/* Add A semesters for each program */
INSERT [LearningSemester] ( [Name], [LearningProgramId], [StartDate], [EndDate], [EnrollmentCloseDate], [Guid] )
SELECT IIF(
            ps.[ConfigurationMode] = @onDemandMode,
            'Default',
            CONCAT('SPRING ', @nextYear)
        )
        , p.Id, 
        CONCAT(@nextYear,'-01-01'), 
        CONCAT(@nextYear,'-04-15'), 
        CONCAT(YEAR(@now),'-12-31'), 
        NEWID()
FROM @programSample ps
JOIN [LearningProgram] p on p.[Guid] = ps.[Guid]
WHERE NOT EXISTS (
    SELECT 1
    FROM [LearningSemester] [ex]
    WHERE [ex].[LearningProgramId] = p.[Id]
)

/* 
    Create the sample courses. 
    Because we'll be referencing these later we'll store them in a table variable.
*/
DECLARE @courseSample TABLE (
    Id INT NOT NULL IDENTITY(1,1),
    CourseName NVARCHAR(200),
    Summary NVARCHAR(1000),
    CourseCode NVARCHAR(12),
    CourseOrder INT NOT NULL DEFAULT(0),
    [Description] NVARCHAR(MAX),
    ProgramName NVARCHAR(200),
    Credits INT,
    MaxStudents INT,
    GradingSystemId INT NOT NULL, 
    SystemCommunicationId INT,
    AllowHistoricalAccess BIT NOT NULL,
    CourseGuid UNIQUEIDENTIFIER NOT NULL,
    ClassesToCreate INT
);

INSERT @courseSample ([CourseName], [Summary], [CourseCode], [CourseOrder], [ProgramName], [Credits], [GradingSystemId], [MaxStudents], [SystemCommunicationId], [AllowHistoricalAccess], [Description], [CourseGuid], [ClassesToCreate])
SELECT [CourseName], [Summary], [CourseCode], [CourseOrder], [ProgramName], [Credits], [GradingSystemId], [MaxStudents], [SystemCommunicationId], [AllowHistoricalAccess], [Description], [CourseGuid], [ClassesToCreate]
FROM (
    SELECT 'Biblical Studies' [ProgramName], 'Introduction to the Old Testament' [CourseName], 'An exploration of the Hebrew Bible''s historical, literary, and theological dimensions, from Genesis to Malachi.' [Summary], 'BBL-101' [CourseCode], 1 [CourseOrder], 3 [Credits], @letterGradeGradingSystemId [GradingSystemId], 24 [MaxStudents], @programCommunicationId [SystemCommunicationId], 0 [AllowHistoricalAccess], @introToNewTestamentCourtseDescription [Description], 'dc3602fe-c0b4-4f02-8fc6-f38d08874543' [CourseGuid], FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse) [ClassesToCreate]
    UNION SELECT 'Biblical Studies', 'Introduction to the New Testament', 'A study of the Christian New Testament''s origin, context, and message, focusing on the life of Jesus and the writings of the Apostles.', 'BBL-202', 2, 3, @letterGradeGradingSystemId, 30, @programCommunicationId, 0, @introToNewTestamentCourtseDescription, '19f4dc3c-77f1-4d43-b160-0eae28880ace', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)
    UNION SELECT 'Biblical Studies', 'Pauline Epistles', 'A deep dive into the letters of the Apostle Paul, analyzing their theological significance, historical context, and impact on early Christianity.', 'BBL-303', 3, 3, @letterGradeGradingSystemId, 30, @programCommunicationId, 0, @introToNewTestamentCourtseDescription, '34a2ca32-59f7-47fe-baad-8f23b91c1b75', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)
    UNION SELECT 'Biblical Studies', 'Biblical Hermeneutics', 'An introduction to the principles and methods of interpreting Scripture, addressing various interpretative strategies and their implications.', 'BBL-404', 4, 3, @letterGradeGradingSystemId, 30, @programCommunicationId, 0, @introToNewTestamentCourtseDescription, '4dd98a23-b8c7-49fe-a60f-e8b3339777c1', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)

    UNION SELECT 'Spiritual Growth University', 'Languages of the Bible', 'Clearly understand how the phraseology of the Bible compares to today by studying the languages the Bible was written in.', 'SPG-101', 1, 3, @passFailGradingSystemId, 20, @programCommunicationId, 1, @introToNewTestamentCourtseDescription, '243f9177-302d-4ed2-870a-0da659d0b572', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)
    UNION SELECT 'Spiritual Growth University', 'History and Archaeology of Ancient Israel', 'An examination of archaeological findings and historical records that provide context for the Old Testament narratives.', 'SPG-202', 2, 3, @passFailGradingSystemId, 30, @programCommunicationId, 1, @introToNewTestamentCourtseDescription, '90058d6c-daec-47f5-88ad-c1bdffda3c43', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)
        
    UNION SELECT 'Foster Care Training', 'What to Expect When Fostering', 'Prepare yourself for fostering before you start so you''re ready to handle any situation.', 'FCT-101', 1, 3, @passFailGradingSystemId, 15, NULL, 1, @introToNewTestamentCourtseDescription, '257a48bd-50db-4208-a754-395bddfcf5e3', FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse)
) s

 INSERT [LearningCourse] ( [Name], [PublicName], [CourseCode], [LearningProgramId], [Credits], [IsActive], [IsPublic], [Order], [MaxStudents], [EnableAnnouncements], [AllowHistoricalAccess], [Guid], [Summary], [Description] )
 SELECT c.[CourseName], c.[CourseName], c.[CourseCode], p.[Id], c.[Credits], 1, 1, c.[CourseOrder], c.[MaxStudents], 0, c.[AllowHistoricalAccess], c.[CourseGuid], c.[Summary], c.[Description]
 FROM @courseSample c
 JOIN @programSample ps ON ps.[ProgramName] = c.[ProgramName]
 JOIN LearningProgram p on p.[Guid] = ps.[Guid]
 WHERE NOT EXISTS (
    SELECT 1
    FROM [LearningCourse] ex
    WHERE ex.[Guid] = c.CourseGuid
 )

DECLARE 
    -- Courses
    @courseSampleSize INT = (SELECT COUNT(*) FROM @courseSample),
    @lmsGroupType INT = (SELECT TOP 1 Id FROM GroupType WHERE [Guid] = @lmsGroupTypeGuid),
    @classesCreated INT = 0,
    @classesToCreate INT = FLOOR(RAND(CHECKSUM(NEWID())) * @maxClassesPerCourse) + 1,
    
    -- Activities
    @assessmentComponentId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A585C101-02E8-4953-BF77-C783C7CFDFDC'),
    @assessment1Description NVARCHAR(MAX) = '{"time":1714411974206,"blocks":[{"id":"nUecbc6V6Z","type":"paragraph","data":{"text":"My Content Here"}}],"version":"2.28.0"}',
    @assessment1Settings NVARCHAR(MAX) = '{"items":[{"uniqueId":"B6D5C8A6-3C4E-4F5F-9ADA-C092BDF8BC3F","typeName":"Multiple Choice","question":"How many apostles are there?","helpText":"","answers":["7","12","20"],"correctAnswer":"12","response":"","order":1},{"uniqueId":"D1524351-462B-49DC-9330-F11C8C31619B","typeName":"Section","title":"Short Answer Section","summary":"Please answer with as much detail as you can.","response":"","order":4},{"uniqueId":"F62B3AA3-A126-4A4B-8DE6-26E817EB13CF","answerBoxRows":5,"helpText":"","maxCharacterCount":120,"pointsEarned":0,"question":"Are there any other numbers in the Bible that match the number of apostles?","questionWeight":20,"response":"","typeName":"Short Answer","order":5},{"answerBoxRows":2,"helpText":"Possibly one of the Gospels?","maxCharacterCount":50,"pointsEarned":0,"question":"Which apostle was the one Jesus loved?","questionWeight":30,"response":"","typeName":"Short Answer","order":6,"uniqueId":"B5658589-B322-4193-85B5-F71767ACD7AB"},{"uniqueId":"ae4fe271-abd2-4779-9ebf-c9481c209d46","typeName":"Multiple Choice","question":"Who was sold into slavery in Egypt?","helpText":"","answers":["Joseph","Jonathan","Johab","Jorge"],"correctAnswer":"Joseph","response":"","order":2},{"uniqueId":"64a74114-92fa-4099-8dea-9e010c912d48","typeName":"Multiple Choice","question":"Who led the Israelites after Moses died?","helpText":"","answers":["Joshua","Johab","Jonah","Lot"],"correctAnswer":"Joshua","order":3}],"assessmentTerm":"Assessment","header":"{\"time\":1716326144116,\"blocks\":[{\"id\":\"-SQIQa_9xh\",\"type\":\"paragraph\",\"data\":{\"text\":\"Please complete to the best of your ability.\"}}],\"version\":\"2.28.0\"}","multipleChoiceWeight":50,"showMissedQuestionsOnResults":true,"showResultsOnCompletion":true}',
    @pointAssessmentComponentId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'A6E91C3C-4A4C-4FC2-816A-A6B1E6422381'),
    @presentationDescription NVARCHAR(MAX) = '',
    @presentationSettings NVARCHAR(MAX) = '{"instructions":"{\"time\":1719941564985,\"blocks\":[{\"id\":\"uiHbfAQlzA\",\"type\":\"paragraph\",\"data\":{\"text\":\"Be sure to speak loud enough for all to hear and remember to make eye contact with people in the audience.\"}}],\"version\":\"2.28.0\"}","rubric":"{\"time\":1715703275889,\"blocks\":[{\"id\":\"D2cSRK74X8\",\"type\":\"paragraph\",\"data\":{\"text\":\"Grading Rubric:\"}},{\"id\":\"qZ7h7XNvN2\",\"type\":\"table\",\"data\":{\"withHeadings\":true,\"content\":[[\"Item\",\"Score\"],[\"Eye Contact\",\"5\"],[\"Speaking Quality\",\"5\"],[\"Content\",\"20\"],[\"Slides\",\"20\"]]}}],\"version\":\"2.28.0\"}"}',
    @checkOffComponentId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '7FAE61A2-5F08-4FD9-8BB7-FF7FAB410AC5'),
    @checkOffDescription NVARCHAR(MAX) = '',
    @checkOffSettings NVARCHAR(MAX) = '{"content":"","confirmationText":"I Certify that I have prepared my final assessment on my own without any significant contributions from others.","isConfirmationRequired":true}',
    @fileUploadComponentId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = 'DEB298FE-383B-46BD-A974-AFD92C09843A'),
    @fileUploadDescription NVARCHAR(MAX) = '{"time":1719854012944,"blocks":[{"id":"XD223JYWoi","type":"paragraph", "data": {"text":"Please upload your mid term paper. Changes will be accepted up until the due date."}}]}',
    @fileUploadSettings NVARCHAR(MAX) = '';
    

/* Get a list of random sized classes for each course. */
SELECT 
    CONCAT(cs.CourseCode, ' - Class: ', ROW_NUMBER() OVER (PARTITION BY c.Id ORDER BY n.Number)) [ClassName], 
    c.Id [CourseId], 
    c.[LearningProgramId], 
    s.Id [SemesterId], 
    cs.[GradingSystemId],
    NEWID() [ClassGuid], 
    n.Number, 
    (SELECT FLOOR(RAND(CHECKSUM(NEWID())) * @maxStudentsPerClass)) [Students], 
    (SELECT FLOOR(RAND(CHECKSUM(NEWID())) * @maxFacilitatorsPerClass) ) [Facilitators],
    ps.[TracksCompletionStatus]
INTO #classes
FROM @courseSample cs
JOIN LearningCourse c on c.[Guid] = cs.[CourseGuid]
JOIN @programSample ps ON ps.[ProgramName] = cs.[ProgramName]
JOIN (
    -- Just allow one semester per Program
    SELECT MAX([Id]) Id, LearningProgramId
    FROM LearningSemester ls
    GROUP BY LearningProgramId
) s ON s.LearningProgramId = c.LearningProgramId
JOIN #numbers n ON n.[Number] <= FLOOR(RAND(CHECKSUM(NEWID())) * @courseSampleSize)

/* Add the Group and LearningClass records for the classes. */
INSERT [GROUP] ( [IsSystem], [GroupTypeId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid] )
SELECT 0, @lmsGroupType, [ClassName], 0, 1, [Number], [ClassGuid]
FROM #classes

/* Add the Classes. */
INSERT [LearningClass] ( [LearningCourseId], [LearningGradingSystemId], [LearningSemesterId], [Id] )
SELECT c.CourseId,c.GradingSystemId, c.SemesterId, g.Id
FROM #classes c
JOIN [Group] g ON g.[Guid] = c.ClassGuid;


/* Add a syllabus ContentPage to all created classes. */ 
DECLARE @contentPageContent NVARCHAR(MAX) = '{"time":1719854012944,"blocks":[{"id":"XF223JYWoi","type":"header","data":{"text":"Course Overview:","level":3}},{"id":"HgLH4Gl6Fu","type":"paragraph","data":{"text":"&nbsp;\"Introduction to the New Testament\" offers students an immersive journey into the sacred Christian scriptures. This course will explore the historical, literary, and theological dimensions of the New Testament, spanning from the Gospels'' accounts of Jesus Christ to the visionary writings in Revelation."}},{"id":"CmjigRceFH","type":"header","data":{"text":"Course Objectives:","level":3}},{"id":"OzQA9hniFU","type":"list","data":{"style":"unordered","items":[{"content":"Understand the historical context of the New Testament texts.","items":[]},{"content":"Analyze key passages for their theological and literary significance.","items":[]},{"content":"Recognize the various genres and writing styles within the New Testament.","items":[]},{"content":"Discuss the impact of the New Testament on Christian theology and practices.","items":[]}]}},{"id":"DzcbG7ZJu7","type":"header","data":{"text":"&nbsp;Course Outline:","level":3}},{"id":"6BUqLsXVIi","type":"list","data":{"style":"ordered","items":[{"content":"Introduction and Background","items":[{"content":"The historical and cultural world of the New Testament","items":[]},{"content":"Canonization:  How the New Testament came to be","items":[]}]},{"content":"The Gospels","items":[{"content":"Synoptic Gospels: Matthew, Mark, and Luke","items":[]},{"content":"The Gospel of John: A unique perspective","items":[]}]},{"content":"Acts of the Apostles","items":[{"content":"The early Christian community and missionary journeys","items":[]}]},{"content":"Pauline Epistles","items":[{"content":"The theology and context of Paul''s letters","items":[]}]},{"content":"General Epistles","items":[{"content":"From Hebrews to Jude: Diverse voices in early Christianity","items":[]}]},{"content":"Revelation and Apocalyptic Literature","items":[{"content":"Symbols, prophecy, and the end times","items":[]}]}]}},{"id":"D2VZID577K","type":"header","data":{"text":"Assessment Methods:","level":3}},{"id":"vQR2ojuKof","type":"list","data":{"style":"unordered","items":[{"content":"Class Participation: 10%","items":[]},{"content":"Quizzes: 20%","items":[]},{"content":"Midterm Exam: 25%","items":[]},{"content":"Research Paper (8-10 pages): 20%","items":[]},{"content":"Final Exam: 25%","items":[]}]}},{"id":"yfICZbb2kJ","type":"header","data":{"text":"Required Texts:","level":3}},{"id":"7zAgNkxyqw","type":"list","data":{"style":"unordered","items":[{"content":"\"The New Oxford Annotated Bible: New Revised Standard Version with the Apocrypha.\" Oxford University Press.","items":[]},{"content":"Ehrman, Bart D. \"The New Testament: A Historical Introduction to the Early Christian Writings.\" Oxford University Press.","items":[]}]}},{"id":"QN-4zlZCdB","type":"header","data":{"text":"Academic Integrity:","level":3}},{"id":"dyYzZMyyFq","type":"paragraph","data":{"text":"Plagiarism and any form of academic dishonesty will not be tolerated. Students found in violation will face disciplinary actions as per institutional policies."}},{"id":"7FjzGXVR0r","type":"header","data":{"text":"Note:&nbsp;","level":3}},{"id":"NyiUxJs3OD","type":"paragraph","data":{"text":"The instructor reserves the right to make changes to the syllabus as necessary. Any changes will be communicated in a timely manner.&nbsp;&nbsp;"}}],"version":"2.28.0"}';

INSERT [LearningClassContentPage] ([LearningClassId], [Title], [Content], [Guid])
SELECT g.Id, 'Syllabus', @contentPageContent, NEWID()
FROM #classes c
JOIN [Group] g ON g.[Guid] = c.[ClassGuid]
WHERE NOT EXISTS (
    SELECT *
    FROM [dbo].[LearningClassContentPage] [ex]
    WHERE [ex].[Title] = 'Syllabus'
        AND [ex].LearningClassId = g.Id
)

/* Create the activity templates */
 DECLARE @activities TABLE (
    Name NVARCHAR(200),
    [AssignTo] INT,
    [TypeId] INT,
    [DueCalculationMethod] INT,
    [DueDateDefault] DATE, 
    [DueDateOffset] INT,
    [AvailableCalculationMethod] INT,
    [AvailableDateDefault] DATE, 
    [AvailableDateOffset] INT,
    [Points] INT,
    [SortOrder] INT,
    [IsStudentCommentingEnabled] BIT,
    [Guid] UNIQUEIDENTIFIER
)

DECLARE 
    @specificDate INT = 0,
    @classStartOffset INT = 1,
    @enrollmentOffset INT = 2,
    @neverDue INT = 3,
    @alwaysAvailable INT = 3,
    @availableAfterPreviousComplete INT = 4;

/* Create the Activities for the Class */
INSERT [LearningActivity] ( [LearningClassId], [Name], [Order], [ActivityComponentId], [AssignTo], [DueDateCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableDateCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], [Guid], [Description], [ActivityComponentSettingsJson] )
SELECT g.[Id], seed.[Name], seed.[Order], [ActivityComponentId], [AssignTo], [DueCalculationMethod], [DueDateDefault], [DueDateOffset], [AvailableCalculationMethod], [AvailableDateDefault], [AvailableDateOffset], [Points], [IsStudentCommentingEnabled], [SendNotificationCommunication], NEWID() [Guid], seed.[Description], [SettingsJson]
FROM (
    SELECT 'Assessment 1' [Name], 1 [Order], @assessmentComponentId [ActivityComponentId], 0 [AssignTo], @classStartOffset [DueCalculationMethod], NULL [DueDateDefault], 2 [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 10 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @assessment1Description [Description], @assessment1Settings [SettingsJson]
    UNION SELECT 'Check Off' [Name], 2 [Order], @checkOffComponentId [ActivityComponentId], 0 [AssignTo], @neverDue [DueCalculationMethod], NULL [DueDateDefault], NULL [DueDateOffset], @alwaysAvailable [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 0 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @checkOffDescription, @checkOffSettings
    UNION SELECT 'Mid Term' [Name], 3 [Order], @fileUploadComponentId [ActivityComponentId], 0 [AssignTo], @classStartOffset [DueCalculationMethod], NULL [DueDateDefault], 12 [DueDateOffset], @availableAfterPreviousComplete [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 30 [Points], 1 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @fileUploadDescription, @fileUploadSettings
    UNION SELECT 'Final Project' [Name], 4 [Order], @pointAssessmentComponentId [ActivityComponentId], 1 [AssignTo], @classStartOffset [DueCalculationMethod], NULL [DueDateDefault], 24 [DueDateOffset], @availableAfterPreviousComplete [AvailableCalculationMethod], NULL [AvailableDateDefault], NULL AvailableDateOffset, 50 [Points], 0 [IsStudentCommentingEnabled], 0 [SendNotificationCommunication], @presentationDescription, @presentationSettings
) seed
JOIN [Group] g ON 1 = 1
JOIN #classes c ON g.[Guid] = c.[ClassGuid];

/* Add Group Member records to a temp table. */
SELECT
    g.[Id] [GroupId],
    c.[ClassGuid],
    p.[Id] [PersonId],
    -- Create the facilitators first (including at least 1).
    IIF( n.Number = 1 OR n.Number <= Facilitators, @facilitatorRoleId, @studentRoleId ) RoleType,
    NEWID() GroupMemberGuid,  
    DATEADD(
        DAY, 
        RAND(CHECKSUM(NEWID())) * (1+DATEDIFF(DAY, DATEADD(MONTH, -1, @now), DATEADD(MONTH, 1, @now))),
        @now
    ) [RandomDateTimeAdded],
    s.StartDate [SemesterStartDate]
INTO #participants
FROM #classes c
JOIN #numbers n ON (n.Number * c.Number) <= (c.Students + Facilitators)
JOIN [Group] g ON g.[Guid] = c.[ClassGuid]
JOIN [LearningSemester] s ON s.[Id] = c.[SemesterId]
JOIN (
    SELECT Id, ROW_NUMBER() OVER ( ORDER BY IIF(Id > (FLOOR(RAND(CHECKSUM(NEWID())) * @maxUserId)), 0, 1) ) PersonRowNumber
    FROM Person
    WHERE PrimaryAliasId IS NOT NULL
) p ON p.PersonRowNumber = n.Number

/* Add a Participant as a GroupMember first (the table owning the identity column) */
INSERT [GroupMember] ( [IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid], [GroupTypeId], [DateTimeAdded] )
SELECT
    0, 
    p.[GroupId], 
    p.[PersonId],
    p.RoleType,
    @activeGroupMemberStatus,
    p.GroupMemberGuid, 
    @lmsGroupType, 
   p.RandomDateTimeAdded
FROM #participants p
WHERE NOT EXISTS (
    SELECT 1
    FROM [GroupMember] ex
    WHERE ex.[Guid] = p.GroupMemberGuid
)


/* Add LearningProgramCompletion Records for any programs that support it. */
INSERT [LearningProgramCompletion] ( [LearningProgramId], [PersonAliasId], [StartDate], [StartDateKey], [CompletionStatus], [Guid] )
SELECT 
    [LearningProgramId],
    [PrimaryAliasId], 
    EarliestSemesterStartDate, 
    CONCAT(
        YEAR(EarliestSemesterStartDate), 
        MONTH(EarliestSemesterStartDate), 
        DAY(EarliestSemesterStartDate)
    ) [StartDateKey], 
    @pendingProgramCompletion, 
    NEWID()
FROM (
    SELECT DISTINCT c.[LearningProgramId], pers.PrimaryAliasId, MIN(SemesterStartDate) EarliestSemesterStartDate
    FROM #participants p
    JOIN Person pers on pers.[Id] = p.[PersonId]
    JOIN #classes c ON p.[ClassGuid] = c.[ClassGuid]
    WHERE c.[TracksCompletionStatus] = 1
    GROUP BY c.[LearningProgramId], pers.PrimaryAliasId
) distinctPrograms

/* Add a Participant */
INSERT [LearningParticipant] ( [LearningCompletionStatus], [LearningGradePercent], [LearningClassId], [LearningProgramCompletionId], [Id] )
SELECT 
    @classCompletionIncomplete, 
    0, 
    p.[GroupId], 
    (
        SELECT TOP 1 pc.Id 
        FROM [LearningProgramCompletion] pc 
        WHERE pc.PersonAliasId = pers.PrimaryAliasId 
        AND pc.LearningProgramId = c.LearningProgramId
    ) [LearningProgramCompletionId], 
    gm.Id
FROM #participants p
JOIN [GroupMember] gm ON gm.[Guid] = p.[GroupMemberGuid]
JOIN [Person] pers on pers.[Id] = p.[PersonId]
JOIN #classes c on c.[ClassGuid] = p.[ClassGuid]

/* Add the Student Activity Completions */
INSERT [LearningActivityCompletion] ( [LearningActivityId], [StudentId], [IsStudentCompleted], [IsFacilitatorCompleted], [Guid]
    , [AvailableDateTime], [DueDate], [StudentComment], [FacilitatorComment], [CompletedDateTime], [WasCompletedOnTime], [PointsEarned], [CompletedByPersonAliasId] )
SELECT [a].[Id], 
    gm.[Id], 
    IIF(gm.[Id] % 5 IN (0, 1), 1, 0) [IsStudentCompleted],
    0 [IsFacilitatorCompleted],
    NEWID() [Guid],
    CASE 
        WHEN [AvailableDateCalculationMethod] = @specificDate THEN [AvailableDateDefault] 
        WHEN [AvailableDateCalculationMethod] = @classStartOffset THEN DATEADD(DAY, [AvailableDateOffset], [RandomDateTimeAdded])
        WHEN [AvailableDateCalculationMethod] = @enrollmentOffset THEN DATEADD(DAY, [AvailableDateOffset], [DateTimeAdded])
        WHEN [AvailableDateCalculationMethod] = @availableAfterPreviousComplete THEN NULL
        ELSE @now
    END [AvailableDate],
    CASE 
        WHEN [DueDateCalculationMethod] = @specificDate THEN [DueDateDefault] 
        WHEN [DueDateCalculationMethod] = @classStartOffset THEN DATEADD(DAY, [DueDateOffset], [RandomDateTimeAdded])
        WHEN [DueDateCalculationMethod] = @enrollmentOffset THEN DATEADD(DAY, [DueDateOffset], [DateTimeAdded])
        ELSE NULL
    END [DueDate],
    IIF(
        a.[IsStudentCommentingEnabled] = 1, 
        IIF( gm.[Id] % 3 = 0, 'I was hoping to discuss this further', NULL), 
        NULL
    ) StudentComment,
    IIF( gm.[Id] % 4 = 0 AND gm.[Id] % 5 IN (0, 1), 'Excellent Work!', NULL) FacilitatorComment,

    /* Every 5 activities mark one completed on time and one late  */
    CASE 
        WHEN gm.[Id] % 5 = 0 THEN  
            /* DueDate - 24 hours and some additional random hours */
            DATEADD(HOUR, -(24 + (FLOOR(RAND(CHECKSUM(NEWID())) * 20 + 7))), CASE 
                WHEN [DueDateCalculationMethod] = @specificDate THEN DueDateDefault 
                WHEN [DueDateCalculationMethod] = @classStartOffset THEN DATEADD(DAY, [DueDateOffset], [RandomDateTimeAdded])
                WHEN [DueDateCalculationMethod] = @enrollmentOffset THEN DATEADD(DAY, [DueDateOffset], [DateTimeAdded])
                ELSE NULL
            END)
        WHEN gm.[Id] % 5 = 1 THEN  
            /* DueDate + 1 */
            DATEADD(HOUR, 24 + FLOOR(RAND(CHECKSUM(NEWID())) * 20 + 7), CASE 
                WHEN [DueDateCalculationMethod] = @specificDate THEN [DueDateDefault] 
                WHEN [DueDateCalculationMethod] = @classStartOffset THEN DATEADD(DAY, [DueDateOffset], [RandomDateTimeAdded])
                WHEN [DueDateCalculationMethod] = @enrollmentOffset THEN DATEADD(DAY, [DueDateOffset], [DateTimeAdded])
                ELSE NULL
        END)
    END [CompletedDate],
    IIF( gm.[Id] % 5 = 0, 1, 0) [WasCompletedOnTime],
    IIF( gm.[Id] % 5 = 0, FLOOR(RAND(CHECKSUM(NEWID())) * a.[Points]), 0) [PointedEarned],
    IIF( gm.[Id] % 5 IN (0, 1), pers.[PrimaryAliasId], NULL) [CompletedByPersonAliasId]
FROM [LearningActivity] a
JOIN #participants p on p.[GroupId] = a.[LearningClassId]
JOIN Person pers ON pers.Id = p.PersonId
JOIN [GroupMember] gm ON gm.[Guid] = p.[GroupMemberGuid]
JOIN #classes c ON c.[ClassGuid] = p.[ClassGuid]

/* Redeclare the variables here in case we want to run the query without the inserts above. */
DECLARE
    @queryFacilitatorRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '80F802CE-2F59-4AB1-ABD8-CFD7A009A00A'),
    @queryStudentRoleId INT = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = 'FA3ACAC2-0377-484C-B888-974CA3BF2FF2');
SELECT 
    p.[Name] [Program], 
    s.Name [Semester], 
    c.Name [Course], 
    COUNT(DISTINCT cl.Id) Classes, 
    COUNT(DISTINCT a.Id) Activities, 
    SUM(IIF(gm.GroupRoleId = @queryStudentRoleId, 1, 0)) Students,
    SUM(IIF(gm.GroupRoleId = @queryFacilitatorRoleId, 1, 0)) Facilitators
FROM LearningProgram p
JOIN LearningCourse c ON p.Id = c.LearningProgramId
LEFT JOIN LearningSemester s on s.LearningProgramId = p.Id
LEFT JOIN LearningClass cl on cl.LearningCourseId = c.Id
    AND cl.LearningSemesterId = s.Id
LEFT JOIN LearningActivity a ON a.LearningClassId = cl.Id
LEFT JOIN LearningParticipant lp on lp.LearningClassId = cl.Id
LEFT JOIN GroupMember gm ON gm.Id = lp.Id
GROUP BY p.[Name], s.Name, c.Name, c.[Order]
ORDER BY p.Name, c.[Order]

/*
    -- Script to clean-up based on pipe-delimited list of LearningProgramIds.
    --    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    --~~~~~~~~~~  Delete All Learning Programs by default         ~~~~~~~
    --~~~~~~~~~~  Verify the variable @pipeDelimitedProgramIds!   ~~~~~~~
    --    ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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
    
    DELETE lcp
    FROM LearningClass lc
    JOIN LearningClassContentPage lcp ON lcp.LearningClassId = lc.Id
    JOIN LearningCourse c on c.Id = lc.LearningCourseId
    JOIN LearningSemester s on s.Id = lc.LearningSemesterId
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
