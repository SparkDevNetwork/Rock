DECLARE
-- Set these
  @startingDate DATETIME = '1/1/2019',
  @numberOfDaysToCreateSchedules int = 365*3,
  @maxSchedulesPerDay int = 15,  -- it will create a random number of schedules per day up to this number
  @minSchedulesPerDay int = 3

--- leave these alone
DECLARE
  @campusId int = null,
  @numberSchedulesForDay int,
  @scheduleDate DATETIME,
  @scheduleDescription nvarchar(100),
  @scheduleName nvarchar(100),
  @iCalContent nvarchar(max),
  @scheduleGuid nvarchar(max),
  @dayCounter int = 0,
  @scheduleCounter int = 1,
  @scheduleId int,
  @eventItemOccurrenceGuid nvarchar(max),
  @eventItemId int,
  @rndHour int,
  @rndHour2 int,
  @paddedHour varchar(2),
  @paddedHour2 varchar(2),
  @dayOfWeek varchar(2),
  @scheduleCategoryId int,
  @scheduleEventTypeId int = (SELECT ID FROM EntityType WHERE NAME = 'Rock.Model.Schedule'),
  @eventScheduleCategoryId int,
  @randNum int,
  @utcTime varchar(100)

DECLARE @crlf varchar(2) = char(13) + char(10)

BEGIN

BEGIN TRANSACTION

-- Add new schedule category if not exist
SET @eventScheduleCategoryId = (SELECT ID FROM category WHERE [Name] = 'Event Schedules' AND EntityTypeId = @scheduleEventTypeId )
SET @scheduleCategoryId = (SELECT [Id] FROM [Category]  WHERE [Guid] = 'EE1F4D0A-38BD-4EA2-BC46-7FFBE88B34A2')

IF ( @scheduleCategoryId IS NULL )
BEGIN
	INSERT INTO [dbo].[Category]
	( [IsSystem]
	 ,[ParentCategoryId]
	 ,[EntityTypeId]
	 ,[Name]
	 ,[Guid]
	 ,[Order]
	)
	VALUES
	(0
	,@eventScheduleCategoryId
	,@scheduleEventTypeId
	,'Auto Populated Schedules'
	,'EE1F4D0A-38BD-4EA2-BC46-7FFBE88B34A2'
	,1
	)
	SELECT @scheduleCategoryId = @@IDENTITY;
END



while @dayCounter < @numberOfDaysToCreateSchedules
BEGIN
        SET @scheduleDate = DATEADD(DAY, @dayCounter, @startingDate);

        -- return a random number of schedules for this day
		SET @numberSchedulesForDay = FLOOR(RAND()*(@maxSchedulesPerDay-@minSchedulesPerDay+1)+@minSchedulesPerDay)

        SET @scheduleCounter = 1;
        while @scheduleCounter <= @numberSchedulesForDay
        BEGIN

            SELECT @scheduleGuid = NEWID();

		    -- return a random hour number > 8 and < 20
		    SET @rndHour = FLOOR(RAND()*(20-8+1)+8)
		    SET @rndHour2 = @rndHour + 1;

		    SET @dayOfWeek = UPPER( LEFT(DATENAME(weekday, @scheduleDate), 2) );

		    -- pad with leading zero
		    SET @paddedHour = (SELECT RIGHT('0'+CAST(@rndHour AS VARCHAR(2)),2))
		    SET @paddedHour2 = (SELECT RIGHT('0'+CAST(@rndHour2 AS VARCHAR(2)),2))

            SELECT @scheduleName = 'Sched ' +  CONVERT(VARCHAR(8), @scheduleDate, 112) + ' ' + @paddedHour + ' - ' + @paddedHour2;
            SELECT @scheduleDescription = 'Description of ' + @scheduleName;
            SET @utcTime = CONVERT(varchar, GetUTCDate(), 112) + 'T' + REPLACE( CONVERT(varchar, GetUTCDate(), 108), ':','') + 'Z'

		    SET @iCalContent = 'BEGIN:VCALENDAR'+@crlf 
                +'VERSION:2.0'+@crlf 
                +'PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN'+@crlf 
                +'BEGIN:VEVENT'+@crlf 
                +'DTEND:'+ CONVERT(VARCHAR(8), @scheduleDate, 112)+'T'+@paddedHour2+'0000'+@crlf 
                +'DTSTAMP:'+@utcTime + @crlf 
                +'DTSTART:'+CONVERT(VARCHAR(8), @scheduleDate, 112)+'T'+@paddedHour+'0000'+@crlf 
                +'SEQUENCE:0'+@crlf 
                +'UID:'+convert(nvarchar(50), newid())+@crlf 
                +'END:VEVENT'+@crlf 
                +'END:VCALENDAR'


            INSERT INTO [dbo].[Schedule]
               ([Name]
               ,[Description]
               ,[iCalendarContent]
               ,[EffectiveStartDate]
               ,[CategoryId]
               ,[Guid]
               ,[IsActive])
            VALUES
               (@scheduleName
               ,@scheduleDescription
               ,@iCalContent
               ,@scheduleDate
               ,@scheduleCategoryId
		       ,@scheduleGuid
               ,1)
           
            SELECT @scheduleId = @@IDENTITY;

            SELECT @eventItemOccurrenceGuid = NEWID();

		    -- select a random eventItemId
            SELECT @eventItemId = (SELECT TOP 1 [Id] FROM [EventItem] ORDER BY NEWID());
			
		    -- select a random eventItemId
            -- 1 out of 10 times use ALL campus (null)
            SET @randNum = FLOOR(RAND()*(10)+1)
            IF ( @randNum < 10 )
            BEGIN
                SELECT @campusId = (SELECT TOP 1 [Id] FROM [Campus] ORDER BY NEWID());
            END
            ELSE
            BEGIN
                SET @campusId = null
            END

            -- Now add a event item occurrence
             INSERT INTO [dbo].[EventItemOccurrence]
                   ([EventItemId]
                   ,[CampusId]
                   ,[Guid]
                   ,[ScheduleId])
             VALUES
                   (@eventItemId
                   ,@campusId
                   ,@eventItemOccurrenceGuid
                   ,@scheduleId)

            SET @scheduleCounter += 1;
        END -- random schedules per day

    SET @dayCounter += 1;
END; -- next day


--SELECT * FROM Schedule
--SELECT * from EventItemOccurrence
COMMIT TRANSACTION

END