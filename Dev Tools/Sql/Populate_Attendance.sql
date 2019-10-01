set nocount on

declare
    @attendanceCounter int = 0,
    @maxAttendanceCount int = 250000, 
    @personSampleSize int = 10000, -- number of people to use when randomly assigning a person to each attendance. You might want to set this lower or higher depending on what type of data you want
    @LocationId int,
    @ScheduleId int,
    @PersonAliasId int,
    @GroupId int,
    @DeviceId int, 
    @SearchTypeValueId int,
    @AttendanceCodeId int,
    @QualifierValueId int,
    @StartDateTime datetime,
    @EndDateTime datetime,
    @DidAttend int,
    @CampusId int,
	@OccurrenceDate date,
	@AttendanceOccurrenceId int,
    @categoryServiceTimes int = (select id from Category where [Guid] = '4FECC91B-83F9-4269-AE03-A006F401C47E'),
    @randomDateInc decimal = 0.5,
	@yearsBack int = 10

declare
  @daysBack int = @yearsBack * 366

declare
    @attendanceGroupIds table ( id Int );

declare
    @personAliasIds table ( id Int );

declare
    @scheduleIds table ( id Int );

declare
    @locationIds table ( id Int );

declare
	@campusIds table ( id int);

declare
    @attendanceCodeIds table ( id Int );

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
		[OccurrenceId] [int]
    );

begin

    insert into @attendanceGroupIds select Id from [Group] where GroupTypeId in (select Id from GroupType where TakesAttendance = 1 or IsSchedulingEnabled = 1);
    insert into @personAliasIds select top (@personSampleSize) Id from PersonAlias
    insert into @attendanceCodeIds select top 10 id from AttendanceCode
	insert into @scheduleIds select Id from Schedule where CategoryId = @categoryServiceTimes
	insert into @locationIds select Id from [Location] where ParentLocationId = 3 or [Name] like 'A/V Booth'
	insert into @campusIds select Id from Campus;
	set @StartDateTime = DATEADD(DAY, -@daysBack, SYSDATETIME())

	-- put all personIds in randomly ordered cursor to speed up getting a random personAliasId for each attendance
	declare personAliasIdCursor cursor for select Id from @personAliasIds order by newid();
	open personAliasIdCursor;

    while @attendanceCounter < @maxAttendanceCount
    begin
		
		if (@attendanceCounter % 100 = 0) begin
            set @GroupId = (select top 1 Id from @attendanceGroupIds order by newid()) 
            set @DeviceId =  (select top 1 Id from Device where DeviceTypeValueId = 41 order by newid())
            set @LocationId = (select top 1 Id from @locationIds order by newid())
        end
		
		fetch next from personAliasIdCursor into @PersonAliasId;

		if (@@FETCH_STATUS != 0) begin
		   close personAliasIdCursor;
		   open personAliasIdCursor;
		   fetch next from personAliasIdCursor into @PersonAliasId;
		end

        if (@attendanceCounter % 10 = 0) begin
            set @randomDateInc = rand()
            set @ScheduleId = (select top 1 Id from @scheduleIds order by newid()) 
        end

		set @StartDateTime = DATEADD(ss, (86000*@daysBack/@maxAttendanceCount), @StartDateTime);
		set @OccurrenceDate = convert(date, @StartDateTime);
        set @DidAttend = (select case when FLOOR(rand() * 50) > 10 then 1 else 0 end) -- select random didattend with ~80% true
        set @CampusId = (select top 1 Id from @campusIds order by newid()) 
        set @AttendanceCodeId = (select top 1 Id from @attendanceCodeIds order by newid())

		set @AttendanceOccurrenceId = (select top 1 id from AttendanceOccurrence where GroupId = @GroupId and ScheduleId = @ScheduleId and LocationId = @LocationId and OccurrenceDate = @OccurrenceDate);
		if (@AttendanceOccurrenceId is null) begin
			insert into AttendanceOccurrence(LocationId, ScheduleId, GroupId, OccurrenceDate, [Guid]) values (@LocationId, @ScheduleId, @GroupId, @OccurrenceDate, newid());
			set @AttendanceOccurrenceId = @@IDENTITY;
		end

		if ( FLOOR(rand() * 10) = 5) begin
			-- randomly set CampusId to null since some types of attendance don't have a campus (like neighborhood groups )
			set @CampusId = null
		end

        

		INSERT INTO @attendanceTable
                   ([PersonAliasId]
                    ,[DeviceId] 
                    ,[AttendanceCodeId]
                    ,[StartDateTime]
                    ,[DidAttend]
                    ,[CampusId]
					,[OccurrenceId]
                    ,[Guid])
             VALUES
                   (
                    @PersonAliasId
                    ,@DeviceId
                    ,@AttendanceCodeId
                    ,@StartDateTime
                    ,@DidAttend
                    ,@CampusId
					,@AttendanceOccurrenceId
                   ,NEWID()
				   
        )

		if (@attendanceCounter % 1000 = 0) begin
		print @attendanceCounter
		end

        
		set @attendanceCounter += 1;
    end

	close personAliasIdCursor;


    insert into Attendance 
        ( OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, [Guid] ) 
    select OccurrenceId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, [Guid] from @attendanceTable order by StartDateTime

	select count(*) from Attendance
end;





