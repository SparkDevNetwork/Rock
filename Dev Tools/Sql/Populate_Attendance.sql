set nocount on

-- configuration
declare
    @populateStartDateTime datetime = DateAdd(month, -0, GetDate()),
    @populateEndDateTime datetime  = DateAdd(week, 1, GetDate()),
    @populateGroupScheduling int = 0, -- set this to true if the attendance records should be for scheduling attendences
    @maxAttendanceCount int = 25000, 
    @personSampleSize int = 10000 -- number of people to use when randomly assigning a person to each attendance. You might want to set this lower or higher depending on what type of data you want
    
declare
    @attendanceCounter int = 0,
    @LocationId int,
    @ScheduleId int,
    @PersonAliasId int,
    @GroupId int,
    @DeviceId int, 
    @SearchTypeValueId int,
    @AttendanceCodeId int,
    @QualifierValueId int,
    @DidAttend int,
    @CampusId int,
	@OccurrenceDate date,
	@AttendanceOccurrenceId int,
    @categoryServiceTimes int = (select id from Category where [Guid] = '4FECC91B-83F9-4269-AE03-A006F401C47E'),
    @randomDateInc decimal = 0.5,
    @randomSeed int = 1
    

declare
  @StartDateTime datetime = @populateStartDateTime,
  @attendancesPerDay int = @maxAttendanceCount/DateDiff(day, @populateStartDateTime, @populateEndDateTime)

declare
    @attendanceGroupIds table ( id Int );


declare
    @locationIds table ( id Int );

declare
	@campusIds table ( id int);

declare
     @attendanceTable TABLE(
	    [PersonAliasId] [int] NULL,
	    [DeviceId] [int] NULL,
	    [AttendanceCodeId] [int] NULL,
	    [QualifierValueId] [int] NULL,
	    [StartDateTime] [datetime] NOT NULL,
	    [DidAttend] [bit] NOT NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CampusId] [int] NULL,
		[OccurrenceId] [int],
        [RSVP] int,
        [RequestedToAttend] bit null,
        [ScheduledToAttend] bit null,
        [SundayDate] [date]
    );

begin
    if (@populateGroupScheduling = 1)
    begin
        insert into @attendanceGroupIds select Id from [Group] g where GroupTypeId in (select Id from GroupType where IsSchedulingEnabled = 1) and g.DisableScheduling = 0;
    end else begin
        insert into @attendanceGroupIds select Id from [Group] where GroupTypeId in (select Id from GroupType where TakesAttendance = 1 or IsSchedulingEnabled = 1);
    end
    
	insert into @locationIds select Id from [Location] where ParentLocationId = 3 or [Name] like 'A/V Booth'
	insert into @campusIds select Id from Campus;

    
    IF CURSOR_STATUS('global','personAliasIdCursor')>=-1
    BEGIN
     DEALLOCATE personAliasIdCursor;
    END

    -- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each attendance
	declare personAliasIdCursor cursor LOCAL FAST_FORWARD for 
        select top (@personSampleSize) Id from PersonAlias pa where pa.PersonId 
        not in (select Id from Person where IsDeceased = 1  and RecordStatusValueId != 3) order by newid();
	open personAliasIdCursor;

    IF CURSOR_STATUS('global','scheduleIdCursor')>=-1
    BEGIN
     DEALLOCATE scheduleIdCursor;
    END

    declare scheduleIdCursor cursor LOCAL FAST_FORWARD for select Id from Schedule s where s.CategoryId = @categoryServiceTimes and s.IsActive = 1;
	open scheduleIdCursor;

    IF CURSOR_STATUS('global','attendanceCodeIdCursor')>=-1
    BEGIN
     DEALLOCATE attendanceCodeIdCursor;
    END

    declare attendanceCodeIdCursor cursor LOCAL FAST_FORWARD for select Id from AttendanceCode
	open attendanceCodeIdCursor;

    while @StartDateTime <= @populateEndDateTime
    begin
		
		if (@attendanceCounter % 100 = 0) begin
            set @GroupId = (select top 1 Id from @attendanceGroupIds order by newid()) 
            set @DeviceId =  (select top 1 Id from Device where DeviceTypeValueId = 41 order by newid())
            set @LocationId = (select top 1 Id from @locationIds order by newid())
            set @CampusId = (select top 1 Id from @campusIds order by newid()) 
        end
		
		fetch next from personAliasIdCursor into @PersonAliasId;

		if (@@FETCH_STATUS != 0) begin
		   close personAliasIdCursor;
		   open personAliasIdCursor;
		   fetch next from personAliasIdCursor into @PersonAliasId;
		end

        fetch next from attendanceCodeIdCursor into @AttendanceCodeId;

		if (@@FETCH_STATUS != 0) begin
		   close attendanceCodeIdCursor;
		   open attendanceCodeIdCursor;
		   fetch next from attendanceCodeIdCursor into @AttendanceCodeId;
		end

        if (@attendanceCounter % 10 = 0) begin
            fetch next from scheduleIdCursor into @ScheduleId;
		    if (@@FETCH_STATUS != 0) begin
		       close scheduleIdCursor;
		       open scheduleIdCursor;
		       fetch next from scheduleIdCursor into @ScheduleId;
		    end
        end

        set @StartDateTime = DATEADD(ss, (86000/@attendancesPerDay), @StartDateTime)
		set @OccurrenceDate = convert(date, @StartDateTime);
        set @randomSeed = CHECKSUM(newid())
        set @DidAttend = (select case when FLOOR(rand(@randomSeed) * 50) > 10 then 1 else 0 end) -- select random didattend with ~80% true

        set @randomSeed = CHECKSUM(newid());

		set @AttendanceOccurrenceId = (select top 1 id from AttendanceOccurrence where GroupId = @GroupId and ScheduleId = @ScheduleId and LocationId = @LocationId and OccurrenceDate = @OccurrenceDate);
		if (@AttendanceOccurrenceId is null) begin
			insert into AttendanceOccurrence(LocationId, ScheduleId, GroupId, OccurrenceDate, SundayDate, [Guid]) values (@LocationId, @ScheduleId, @GroupId, @OccurrenceDate, dbo.ufnUtility_GetSundayDate(@OccurrenceDate), newid());
			set @AttendanceOccurrenceId = @@IDENTITY;
		end

        set @randomSeed = CHECKSUM(newid());

		if ( FLOOR(rand(@randomSeed) * 10) = 5) begin
			-- randomly set CampusId to null since some types of attendance don't have a campus (like neighborhood groups )
			set @CampusId = null
		end


        declare @rsvp int = 3;
        declare @requestedToAttend bit = null
        declare @scheduledToAttend bit = null;
        if (@populateGroupScheduling = 1) begin
            set @rsvp = round(rand(CHECKSUM(newid())) * 3, 0);
            set @requestedToAttend = 1;
            if (@rsvp = 1) begin
                set @scheduledToAttend = 1;
            end else begin
                set @scheduledToAttend = 0;
            end
        end
        

        

		INSERT INTO @attendanceTable
                   ([PersonAliasId]
                    ,[DeviceId] 
                    ,[AttendanceCodeId]
                    ,[StartDateTime]
                    ,[SundayDate]
                    ,[DidAttend]
                    ,[CampusId]
					,[OccurrenceId]
                    ,[RSVP]
                    ,[RequestedToAttend]
                    ,[ScheduledToAttend]
                    ,[Guid])
             VALUES
                   (
                    @PersonAliasId
                    ,@DeviceId
                    ,@AttendanceCodeId
                    ,@StartDateTime
                    ,dbo.ufnUtility_GetSundayDate(@StartDateTime)
                    ,@DidAttend
                    ,@CampusId
					,@AttendanceOccurrenceId
                    ,@rsvp
                    ,@requestedToAttend
                    ,@scheduledToAttend
                   ,NEWID()
				   
        )

		if (@attendanceCounter % 10000 = 0) begin
		print @attendanceCounter
		end
        
		set @attendanceCounter += 1;
    end

	close personAliasIdCursor;
    close scheduleIdCursor;
    close attendanceCodeIdCursor;

    insert into Attendance 
        ( OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] ) 
    select OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] 
        from @attendanceTable order by StartDateTime

	select count(*) [AttendanceCount] from Attendance
    select count(*) [AttendanceOccurrenceCount] from AttendanceOccurrence
end;





