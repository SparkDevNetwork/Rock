set nocount on

declare
    @attendanceCounter int = 0,
    @maxAttendanceCount int = 250000, 
    --@maxPersonId int = (select min(id), max(id) from (select top 1000 id  from Person order by Id) x),  /* limit to first 4000 persons in the database */ 
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
    @categoryServiceTimes int = (select id from Category where [Guid] = '4FECC91B-83F9-4269-AE03-A006F401C47E'),
    @randomDateInc decimal = 0.5

declare
    @attendanceGroupIds table ( id Int );

declare
    @personAliasIds table ( id Int );

declare
    @attendanceCodeIds table ( id Int );

declare
     @attendanceTable TABLE(
   	    [LocationId] [int] NULL,
	    [ScheduleId] [int] NULL,
	    [GroupId] [int] NULL,
	    [PersonAliasId] [int] NULL,
	    [DeviceId] [int] NULL,
	    [SearchTypeValueId] [int] NULL,
	    [AttendanceCodeId] [int] NULL,
	    [QualifierValueId] [int] NULL,
	    [StartDateTime] [datetime] NOT NULL,
	    [EndDateTime] [datetime] NULL,
	    [DidAttend] [bit] NOT NULL,
	    [Note] [nvarchar](max) NULL,
	    [Guid] [uniqueidentifier] NOT NULL,
	    [CreatedDateTime] [datetime] NULL,
	    [ModifiedDateTime] [datetime] NULL,
	    [CreatedByPersonAliasId] [int] NULL,
	    [ModifiedByPersonAliasId] [int] NULL,
	    [ForeignId] [nvarchar](50) NULL,
	    [CampusId] [int] NULL
    );

begin

    insert into @attendanceGroupIds select Id from [Group] where GroupTypeId in (select Id from GroupType where TakesAttendance = 1);
    insert into @personAliasIds select top 10000 Id from PersonAlias
    insert into @attendanceCodeIds select top 10 id from AttendanceCode

    set @StartDateTime = SYSDATETIME()

    while @attendanceCounter < @maxAttendanceCount
    begin

        if (@attendanceCounter % 100 = 0) begin
            set @GroupId = (select top 1 Id from @attendanceGroupIds order by newid()) 
            set @PersonAliasId =  (select top 1 Id from @personAliasIds order by newid())
            set @DeviceId =  (select top 1 Id from Device where DeviceTypeValueId = 41 order by newid())
            set @LocationId = (select top 1 Id from Location where ParentLocationId = 3 order by newid())
        end

        if (@attendanceCounter % 10 = 0) begin
            set @randomDateInc = rand()
            set @ScheduleId = (select top 1 Id from Schedule where CategoryId = @categoryServiceTimes order by newid()) 
        end

        set @StartDateTime = DATEADD(ss, -((86000*365/@maxAttendanceCount) * @randomDateInc), @StartDateTime);
        set @DidAttend = (select case when FLOOR(rand() * 50) > 10 then 1 else 0 end) -- select random didattend with ~80% true
        set @CampusId = (select top 1 Id from Campus order by newid()) 
        set @AttendanceCodeId = (select top 1 Id from @attendanceCodeIds order by newid())

        INSERT INTO @attendanceTable
                   ([LocationId]
                    ,[ScheduleId]
                    ,[GroupId]
                    ,[PersonAliasId]
                    ,[DeviceId] 
                    --,[SearchTypeValueId]
                    ,[AttendanceCodeId]
                    --,[QualifierValueId]
                    ,[StartDateTime]
                    --,[EndDateTime]
                    ,[DidAttend]
                    ,[CampusId]
                    ,[Guid])
             VALUES
                   (@LocationId
                    ,@ScheduleId
                    ,@GroupId
                    ,@PersonAliasId
                    ,@DeviceId
                    --,@SearchTypeValueId
                    ,@AttendanceCodeId
                    --,@QualifierValueId
                    ,@StartDateTime
                    --,@EndDateTime
                    ,@DidAttend
                    ,@CampusId
                   ,NEWID()
        )

        set @attendanceCounter += 1;
        
    end

    --truncate table Attendance    
    insert into Attendance 
        ( LocationId, ScheduleId, GroupId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, [Guid] ) 
    select LocationId, ScheduleId, GroupId, PersonAliasId, DeviceId, AttendanceCodeId, StartDateTime, CampusId, DidAttend, [Guid] from @attendanceTable order by StartDateTime
end;


