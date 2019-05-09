
BEGIN TRANSACTION

DECLARE 
	@NumberOfGroups int = 3, -- number of fake groups to create
	@maxAttendanceCount int = 25, -- max number of fake attendance slots to fill

	@locationTypeValueId int,
	@campusId int = 1, -- main campus
	@groupTypeId int,
	@parentGroupId int,
	@groupCounter int = 0,
	@locationCounter int = 0,
	@groupName nvarchar(100),
	@groupGuid nvarchar(max),
	@groupDescription nvarchar(max),
	@groupId int = null,
	@attendanceCounter int = 0,
	@AttendanceCodeId int,
	@rndLocationNumber int,
	@rndScheduleNumber int,
	@PersonAliasId int,
	@DeviceId int,
	@lastAttendanceOccurrenceId int

SET @groupTypeId = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4' ) -- Serving Team group type
SET @parentGroupId = (SELECT [Id] FROM [Group] WHERE [Guid] = '31730962-4C7B-425B-BD73-4185331F37EF' ) -- Serving Teams (parent group)
SET @locationTypeValueId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '107C6DA1-266D-4E1C-A443-1CD37064601D' ) -- Room type locations

DECLARE @attendanceCodeIds table ( id Int );
DECLARE @locationId Table(Id int)
DECLARE @insertedGroupLocations Table(Id int, rowNum int);
DECLARE @scheduleId Table(Id int,rowNum int)
DECLARE @allLocationsUsed Table(locationId int,rowNum int);
DECLARE @personAliasIds table ( id Int);
DECLARE
     @attendanceTable TABLE(
	    [PersonAliasId] [int] NULL,
	    [DeviceId] [int] NULL,
	    [AttendanceCodeId] [int] NULL,
		[Guid] [uniqueidentifier] NOT NULL
    );

SET @lastAttendanceOccurrenceId = (SELECT Max(ID) FROM AttendanceOccurrence)
 
INSERT INTO @attendanceCodeIds select top 10 id from AttendanceCode
INSERT INTO @personAliasIds select top 100 Id from PersonAlias
-- BUILD A LIST OF ATTENDANCES 
	WHILE @attendanceCounter < @maxAttendanceCount
	BEGIN 
	    set @PersonAliasId =  (select top 1 Id from @personAliasIds order by newid())
		set @DeviceId =  (select top 1 Id from Device where DeviceTypeValueId = 41 order by newid())
		set @AttendanceCodeId = (select top 1 Id from @attendanceCodeIds order by newid())
	

		INSERT INTO @attendanceTable
		(
			[PersonAliasId]
			,[DeviceId] 
			,[AttendanceCodeId]
			,[Guid])
		VALUES
		(
			@PersonAliasId
			,@DeviceId
			,@AttendanceCodeId
			,NEWID()
		);

		SET @attendanceCounter += 1;
	END

	-- location of type Room
	INSERT INTO @allLocationsUsed
	SELECT Id,ROW_NUMBER() OVER(ORDER BY ID) 
			FROM [Location]
		WHERE LocationTypeValueId = @locationTypeValueId 

	-- Create N number of Groups
	PRINT 'INSERTING GROUPS'
	WHILE @groupCounter < @NumberOfGroups
	BEGIN
		-- clear table each iteration
		DELETE FROM @locationId
		DELETE FROM @insertedGroupLocations

		select @groupGuid = NEWID();
		select @groupName = 'Group ' + REPLACE(str(@groupCounter, 2), ' ', '0');
		select @groupDescription = 'Description of ' + @groupName;

		PRINT 'INSERT ' + @groupName
		-- INSERT GROUP
		INSERT INTO [dbo].[Group] ([IsSystem],[ParentGroupId],[GroupTypeId],[CampusId],[Name],[Description],[IsSecurityRole],[IsActive],[Order],[Guid])
							VALUES (0,@parentGroupId,@groupTypeId,@campusId,@groupName,@groupDescription,0,1,0,@groupGuid);
		SET @groupId = @@IDENTITY
		
		PRINT 'UPDATING LOCATION WHERE THE LOCATION TYPE VALUE IS NULL AND NAME IS LIKE ROOM'
		-- Make sure all the location records with type Room have the locationtypValueId  
		UPDATE [Location] SET LocationTypeValueId = @locationTypeValueId  WHERE [NAME] like '%Room%' AND LocationTypeValueId IS NULL
		
		-- returns a random decimal number > 2 and < 4
		set @rndLocationNumber = FLOOR(RAND()*(4-2+1)+2)

		-- Select a random number of location to assign to current group
		INSERT INTO @locationId
        SELECT  TOP(@rndLocationNumber) locationId FROM @allLocationsUsed

		-- get the last group id before  inserting new GroupLocations
		declare @lastGroupLocationId int = (SELECT top 1 ID FROM GroupLocation ORDER BY ID DESC)
		-- create group locations
		PRINT 'INSERTING GROUP LOCATIONS ON GROUP ' + @groupName
		INSERT INTO GroupLocation(GroupId,LocationId,IsMailingLocation,IsMappedLocation, GroupLocationTypeValueId,GUID)
		SELECT @groupId,l.id,0,0,19,NEWID() FROM @locationId l

		-- Select back the currently inserted group locations
		INSERT INTO @insertedGroupLocations select ID,ROW_NUMBER() OVER(ORDER BY ID) as ROW FROM GroupLocation WHERE Id > @lastGroupLocationId
		declare @numberLocations int
		declare @num int = 0

		set @numberLocations = (select count(id) FROM @insertedGroupLocations)	
		-- itterate through Locations and add Group Location Schedules
		PRINT 'INSERT GROUP LOCATION SCHEDULES AND GROUP LOCATION SCHEDULE CONFIGS'
		WHILE(@num < @numberLocations)
		BEGIN
			DELETE FROM @scheduleId -- clear the table from last time
			
			-- returns random decimal number > 2 and < 5
			set @rndScheduleNumber = FLOOR(RAND()*(5-2+1)+2)

			declare @currentLocation int
			SET @currentLocation = (SELECT Id from @insertedGroupLocations il WHERE il.rowNum = @num + 1) 

			-- Add Random Capaicities
		
			-- select a random number of schedules 
			INSERT INTO @scheduleId
			SELECT TOP(@rndScheduleNumber) Id,ROW_NUMBER() OVER(ORDER BY ID) as ROW  FROM Schedule WHERE Id < 8

			-- INSERT GROUPLOCATIONSCHEDULE
			INSERT INTO GroupLocationSchedule(GroupLocationId,ScheduleId)
			SELECT @currentLocation,sc.Id FROM @scheduleId sc 
			WHERE NOT EXISTS(SELECT 1 FROM GroupLocationSchedule gls 
			WHERE gls.GroupLocationId = @currentLocation AND gls.ScheduleId = sc.Id ) -- make sure we dont add same location and schedule combination

			-- INSERT GROUPLOCATIONSCHEDULECONFIGS without capacities
			INSERT INTO GroupLocationScheduleConfig(GroupLocationId,ScheduleId)
			SELECT @currentLocation,sc.Id
			FROM @scheduleId sc 
			WHERE NOT EXISTS(SELECT 1 FROM GroupLocationScheduleConfig glsg
			WHERE glsg.GroupLocationId = @currentLocation AND glsg.ScheduleId = sc.Id ) 

			declare @numGroupLocationConfigs int = (SELECT COUNT(*) FROM GroupLocationScheduleConfig WHERE MinimumCapacity IS NULL AND GroupLocationId = @currentLocation)
			
			declare @grouplocConfigRow int = 0
			
			WHILE @grouplocConfigRow< @numGroupLocationConfigs
			BEGIN 
			 declare @currentScheduleId int = (SELECT Id FROM @scheduleId WHERE rowNum = @grouplocConfigRow + 1 )

			 declare @rndCapacity int;
			 -- return a random number > 2 and < 12
			 set @rndCapacity = FLOOR(RAND()*(12-2+1)+2)

			 -- UPDATE GROUPLOCATION SCHEDULECONFIG WITH CAPACITIES
			 UPDATE GroupLocationScheduleConfig  
				SET MinimumCapacity = @rndCapacity,
					DesiredCapacity = @rndCapacity + 3,
					MaximumCapacity = @rndCapacity + 5 
				WHERE ScheduleId = @currentScheduleId AND GroupLocationId = @currentLocation

				declare @locationsCount int = (SELECT COUNT(*) FROM @allLocationsUsed)
				declare @indexLocation int = 1;

				PRINT'INSERT ATTENDANCE OCCURRENCE FOR EACH LOCATION'
				WHILE(@indexLocation < @locationsCount)
				BEGIN
					
					declare @currentlocationId int = (SELECT locationId FROM @allLocationsUsed WHERE rowNum = @indexLocation + 1)
					declare @occuranceDate date = DATEADD(DAY, @num, SYSDATETIME())

					-- INSERT A ATTENDANCEOCCURRENCE FOR EACH LOCATION
					INSERT INTO AttendanceOccurrence (GroupId,LocationId,ScheduleId,OccurrenceDate,Guid) SELECT @groupId,@currentlocationId,@currentScheduleId,@occuranceDate,NEWID()
					WHERE NOT EXISTS (SELECT Id FROM AttendanceOccurrence WHERE GroupId = @groupId AND LocationId = @currentlocationId AND ScheduleId = @currentScheduleId AND OccurrenceDate = @occuranceDate ) 
					set @indexLocation +=1
				END

			set @grouplocConfigRow +=1

			END
			set @num += 1
		END
		set @groupCounter += 1;
	END;


	-- Iterate through the newly add Attendance Occurances and add Attendees 
	DECLARE @addedOccurrence TABLE(attendanceOccurrencId int,rowNum int);
	INSERT INTO @addedOccurrence 
	SELECT Id,ROW_NUMBER() OVER(ORDER BY ID) FROM AttendanceOccurrence WHERE Id > @lastAttendanceOccurrenceId
	
	DECLARE @countAddedOccurrences int = (SELECT COUNT(*) FROM @addedOccurrence)
	DECLARE @indexOccurrence int = 0;
	WHILE @indexOccurrence < @countAddedOccurrences
	BEGIN
		DECLARE @currentOccurrenceId int = (SELECT attendanceOccurrencId FROM @addedOccurrence WHERE rowNum = @indexOccurrence + 1);
		DECLARE @startdatetime date = (select OccurrenceDate FROM AttendanceOccurrence WHERE id = @currentOccurrenceId)
		
		DECLARE @personAliasCount int = (SELECT Count(id) FROM @personAliasIds);
			
			INSERT INTO Attendance  
			( 
					OccurrenceId
					,PersonAliasId
					,DeviceId
					, AttendanceCodeId
					, StartDateTime
					, CampusId
					, DidAttend
					, [Guid] )
			SELECT @currentOccurrenceId
					,a.PersonAliasId
					,a.DeviceId
					,a.AttendanceCodeId
					,@startDateTime
					,1
					,1
					,a.[Guid]
					 FROM @attendanceTable a 

	  SET @indexOccurrence +=1;
	END

--SELECT * FROM AttendanceOccurrence WHERE OccurrenceDate > DateAdd( d, -7, GetDate() )
--ROLLBACK TRANSACTION
COMMIT
