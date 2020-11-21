DECLARE @WeeklySundayScheduleGuid UNIQUEIDENTIFIER = '04A17BB3-B3E8-4E4B-B575-22CB7E81D5F4',
	@EveryOtherWeekSundayScheduleGuid UNIQUEIDENTIFIER = '07F1D164-F8D7-4FA4-B98B-B7480CC40457',
	@FirstAndThirdSundayScheduleGuid UNIQUEIDENTIFIER = '8342535C-A48B-4AE6-8522-33C33BFBBFFC',
	@SecondAndFourthSundayScheduleGuid UNIQUEIDENTIFIER = '34448AF0-ECA2-40E5-89C9-66C1739A8828',
	@WeeklySundayScheduleId INT,
	@EveryOtherWeekSundayScheduleId INT,
	@FirstAndThirdSundayScheduleId INT,
	@SecondAndFourthSundayScheduleId INT

BEGIN
	UPDATE [GroupMember] SET [ScheduleTemplateId] = NULL WHERE [ScheduleTemplateId] IS NOT NULL
	
	DELETE
	FROM [GroupMemberScheduleTemplate]
	WHERE [ScheduleId] IN (
			SELECT Id
			FROM [Schedule]
			WHERE [Guid] IN (@WeeklySundayScheduleGuid, @EveryOtherWeekSundayScheduleGuid, @FirstAndThirdSundayScheduleGuid, @SecondAndFourthSundayScheduleGuid)
			)

	DELETE
	FROM [Schedule]
	WHERE [Id] IN (
			SELECT Id
			FROM [Schedule]
			WHERE [Guid] IN (@WeeklySundayScheduleGuid, @EveryOtherWeekSundayScheduleGuid, @FirstAndThirdSundayScheduleGuid, @SecondAndFourthSundayScheduleGuid)
			)

	INSERT INTO [Schedule] (
		[iCalendarContent],
		[Guid],
		[IsActive]
		)
	VALUES (
		'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20181212T145101
DTSTAMP:20181212T215151Z
DTSTART:20181212T145100
RRULE:FREQ=WEEKLY;BYDAY=SU
SEQUENCE:0
UID:3c256da2-e84c-4085-b211-973a93e40d55
END:VEVENT
END:VCALENDAR',
		@WeeklySundayScheduleGuid,
		1
		),
		(
		'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20181204T132401
DTSTAMP:20181204T202430Z
DTSTART:20181204T132400
RRULE:FREQ=WEEKLY;INTERVAL=2;BYDAY=SU
SEQUENCE:0
UID:bb091012-a097-445d-a243-ba19b4f25dcf
END:VEVENT
END:VCALENDAR',
		@EveryOtherWeekSundayScheduleGuid,
		1
		),
		(
		'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20181204T132301
DTSTAMP:20181204T202333Z
DTSTART:20181204T132300
RRULE:FREQ=MONTHLY;BYDAY=1SU,3SU
SEQUENCE:0
UID:b58ea771-49eb-45bb-a01e-7ff858e06b08
END:VEVENT
END:VCALENDAR',
		@FirstAndThirdSundayScheduleGuid,
		1
		),
		(
		'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20181204T132301
DTSTAMP:20181204T202356Z
DTSTART:20181204T132300
RRULE:FREQ=MONTHLY;BYDAY=2SU,4SU
SEQUENCE:0
UID:cb78d006-08a9-492f-9284-0e40d96f7891
END:VEVENT
END:VCALENDAR',
		@SecondAndFourthSundayScheduleGuid,
		1
		)

	SET @WeeklySundayScheduleId = (
			SELECT TOP 1 Id
			FROM [Schedule]
			WHERE [Guid] = @WeeklySundayScheduleGuid
			);
	SET @EveryOtherWeekSundayScheduleId = (
			SELECT TOP 1 Id
			FROM [Schedule]
			WHERE [Guid] = @EveryOtherWeekSundayScheduleGuid
			);
	SET @FirstAndThirdSundayScheduleId = (
			SELECT TOP 1 Id
			FROM [Schedule]
			WHERE [Guid] = @FirstAndThirdSundayScheduleGuid
			);
	SET @SecondAndFourthSundayScheduleId = (
			SELECT TOP 1 Id
			FROM [Schedule]
			WHERE [Guid] = @SecondAndFourthSundayScheduleGuid
			);

	INSERT INTO GroupMemberScheduleTemplate (
		[Name],
		[ScheduleId],
		[Guid]
		)
	VALUES (
		'Every Week',
		@WeeklySundayScheduleId,
		'7aa70d4a-ae06-4402-8b63-0e7136a44560'
		)

	INSERT INTO GroupMemberScheduleTemplate (
		[Name],
		[ScheduleId],
		[Guid]
		)
	VALUES (
		'Every Other Week',
		@EveryOtherWeekSundayScheduleId,
		'51bdc208-9f97-452e-ba75-d69f37640d49'
		)

	INSERT INTO GroupMemberScheduleTemplate (
		[Name],
		[ScheduleId],
		[Guid]
		)
	VALUES (
		'1st and 3rd Week',
		@FirstAndThirdSundayScheduleId,
		'fbf8aa40-3378-4a13-b11c-749fc0d60960'
		)

	INSERT INTO GroupMemberScheduleTemplate (
		[Name],
		[ScheduleId],
		[Guid]
		)
	VALUES (
		'2nd and 4th Week',
		@SecondAndFourthSundayScheduleId,
		'73cdbe0a-0921-4284-ae7b-076635040c7b'
		)
END
