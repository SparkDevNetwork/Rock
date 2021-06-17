set nocount on

-- configuration
declare
    @populateStartDateTimeLastHour datetime = DateAdd(hour, -1, GetDate()),
    @populateStartDateTimeLast12Months datetime = DateAdd(MONTH, -12, GetDate()),
    @populateStartDateTimeLast5Years datetime = DateAdd(YEAR, -5, GetDate())

declare
    -- set this to @populateStartDateTimeLastHour or @populateStartDateTimeLast12Months (or custom), depending on what you need
    @populateStartDateTime datetime = @populateStartDateTimeLast5Years,
    @populateEndDateTime datetime  = DateAdd(hour, 0, GetDate()),
    
    @limitToChildren bit = 1, -- set this to true to only add attendance for children
    
    @populateGroupScheduling int = 0, -- set this to true if the attendance records should be for scheduling attendences
    @maxAttendanceCount int = 5000, 
    @personSampleSize int = 10000, -- number of people to use when randomly assigning a person to each attendance. You might want to set this lower or higher depending on what type of data you want
    @checkinAreaGroupTypeId int = (SELECT Id FROM GroupType WHERE [Guid] = 'FEDD389A-616F-4A53-906C-63D8255631C5') -- Weekly Service Checkin
    
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
    @randomSeed int = 1,
    @millisecondsPerDay int = 86400000

declare
  @CheckinDateTime datetime = @populateStartDateTime,
  @PresentDateTime datetime = @populateStartDateTime,
  @CheckoutDateTime datetime,
  @attendancesPerDay int = @maxAttendanceCount/(DateDiff(day, @populateStartDateTime, @populateEndDateTime) +1)

  if (@attendancesPerDay = 0) begin
    set @attendancesPerDay = 1;
  end

declare
  @millsecondsIncrement int = null

  if (DateDiff(DAY, @populateStartDateTime, @populateEndDateTime) < 2) begin
    set @millsecondsIncrement = (DateDiff(ms, @populateStartDateTime, @populateEndDateTime))/@attendancesPerDay
  end else begin
    set @millsecondsIncrement = @millisecondsPerDay/@attendancesPerDay
  end

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
        [PresentDateTime] [datetime] NULL,
        [EndDateTime] [datetime] null,
	    [DidAttend] [bit] NOT NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CampusId] [int] NULL,
		[OccurrenceId] [int],
        [RSVP] int,
        [RequestedToAttend] bit null,
        [ScheduledToAttend] bit null
    );

begin
    if (@populateGroupScheduling = 1)
    begin
        insert into @attendanceGroupIds select Id from [Group] g where GroupTypeId in (select Id from GroupType where IsSchedulingEnabled = 1) and g.DisableScheduling = 0;
    end else begin
        -- weekly service checkin
        ;WITH CTE ([RecursionLevel], [GroupTypeId], [ChildGroupTypeId])
        AS (
	        SELECT 0 AS [RecursionLevel], [GroupTypeId], [ChildGroupTypeId]
	        FROM [GroupTypeAssociation]
	        WHERE [GroupTypeId] = @checkinAreaGroupTypeId	        
			        
	        UNION ALL
	        SELECT acte.[RecursionLevel] + 1 AS [RecursionLevel], [a].[GroupTypeId], [a].[ChildGroupTypeId]
	        FROM [GroupTypeAssociation] [a]
	        JOIN CTE acte
		        ON acte.[ChildGroupTypeId] = [a].[GroupTypeId]
	        WHERE acte.[ChildGroupTypeId] <> acte.[GroupTypeId] AND [a].[ChildGroupTypeId] <> acte.[GroupTypeId] -- and the child group type can't be a parent group type
	        )
            INSERT INTO @attendanceGroupIds
            select Id from [Group] gp where gp.GroupTypeId in (
            SELECT Id
            FROM [GroupType]
            WHERE [Id] IN (
		            SELECT [ChildGroupTypeId]
		            FROM CTE
		            ))
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
        not in (select Id from Person where (IsDeceased = 1  and RecordStatusValueId != 3) or (@limitToChildren = 1 and  AgeClassification != 2)) order by newid();
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

    while @CheckinDateTime <= @populateEndDateTime
    begin
        
        set @GroupId = (select top 1 Id from @attendanceGroupIds order by newid()) 
        set @DeviceId =  (select top 1 Id from Device where DeviceTypeValueId = 41 order by newid())
        set @LocationId = (select top 1 Id from @locationIds order by newid())
        set @CampusId = (select top 1 Id from @campusIds order by newid()) 
		
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
        
        set @CheckinDateTime = DATEADD(ms, @millsecondsIncrement, @CheckinDateTime)
        set @PresentDateTime = @CheckinDateTime
        set @CheckoutDateTime = null
		set @OccurrenceDate = convert(date, @CheckinDateTime);
        set @randomSeed = CHECKSUM(newid())
        set @DidAttend = (select case when FLOOR(rand(@randomSeed) * 50) > 10 then 1 else 0 end) -- select random didattend with ~80% true

        set @randomSeed = CHECKSUM(newid());

		set @AttendanceOccurrenceId = (select top 1 id from AttendanceOccurrence where GroupId = @GroupId and ScheduleId = @ScheduleId and LocationId = @LocationId and OccurrenceDate = @OccurrenceDate);
		if (@AttendanceOccurrenceId is null) begin
			insert into AttendanceOccurrence(LocationId, ScheduleId, GroupId, OccurrenceDate, SundayDate, OccurrenceDateKey, [Guid]) 
            values (@LocationId, @ScheduleId, @GroupId, @OccurrenceDate, dbo.ufnUtility_GetSundayDate(@OccurrenceDate), CONVERT(INT, (CONVERT(CHAR(8), @OccurrenceDate, 112))), newid());
			set @AttendanceOccurrenceId = @@IDENTITY;
		end

        set @randomSeed = CHECKSUM(newid());

		if ( FLOOR(rand(@randomSeed) * 10) = 5) begin
			-- randomly set CampusId to null since some types of attendance don't have a campus (like neighborhood groups )
			set @CampusId = null
		end

        if ( FLOOR(rand(@randomSeed) * 3) = 2) begin
			-- randomly set @PresentDateTime null to indicate checked-in but not marked present (when EnablePresence)
			set @PresentDateTime = null
		end

        if ( FLOOR(rand(@randomSeed) * 8) = 2) begin
			-- randomly set @PresentDateTime null to indicate checked-in but not marked present (when EnablePresence)
			set @CheckoutDateTime = DATEADD(mi, 35, @CheckinDateTime)
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
                    ,[PresentDateTime]
                    ,[EndDateTime]
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
                    ,@CheckinDateTime
                    ,@PresentDateTime
                    ,@CheckoutDateTime
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
          insert into Attendance 
            ( OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, PresentDateTime, EndDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] ) 
            select OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, [at].StartDateTime, [at].PresentDateTime, [at].EndDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] 
            from @attendanceTable [at] order by at.StartDateTime
            delete from @attendanceTable
		end
        
		set @attendanceCounter += 1;

        if (@attendanceCounter > @maxAttendanceCount) begin
          break;
        end
    end

	close personAliasIdCursor;
    close scheduleIdCursor;
    close attendanceCodeIdCursor;

    insert into Attendance 
        ( OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, PresentDateTime, EndDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] ) 
    select OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, [at].StartDateTime, [at].PresentDateTime, [at].EndDateTime, CampusId, DidAttend, RSVP, ScheduledToAttend, RequestedToAttend, [Guid] 
        from @attendanceTable [at] order by at.StartDateTime

    update AttendanceOccurrence set OccurrenceDateKey = CONVERT(INT, (CONVERT(CHAR(8), OccurrenceDate, 112)) ) where OccurrenceDateKey = 0

	select count(*) [AttendanceCount] from Attendance
    select count(*) [AttendanceOccurrenceCount] from AttendanceOccurrence
end;





