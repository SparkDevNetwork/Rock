Declare @IsHoliday bit = 0;
Declare @IsCampus bit = 0;
Declare @Holiday nvarchar(50) = 'Christmas';
Declare @CampusId int = 1;
Declare @SundayDate datetime = null;

----------------------------------------------------------------------------
-- GET THE METRIC CATEGORY IDS
----------------------------------------------------------------------------
-- These are the root category Ids
DECLARE @WeekendMetricCategoryId INT = 435;
DECLARE @WorshipMetricCategoryId INT = 443;
DECLARE @ChildrenMetricCategoryId INT = 440;
DECLARE @StudentsMetricCategoryId INT = 444;

Declare @OtherMetricCategoryId INT = 446 -- This holds Baptisms and First Time Guests

DECLARE @WorshipNightMetricCategoryId INT = 513;


Declare @MetricCategoryIds table(
MetricCategoryId int
);

Insert into @MetricCategoryIds Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WeekendMetricCategoryId);
Insert into @MetricCategoryIds Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@OtherMetricCategoryId);

----------------------------------------------------------------------------
-- GET THE SCHEDULE CATEGORY IDS
----------------------------------------------------------------------------
DECLARE @WeekendScheduleCategoryId INT = 50;
Declare @StudentsScheduleCategoryId INT = 496;
Declare @OtherEventsCategoryId INT = 138;

DECLARE @HolidayScheduleCategoryId INT;
If @Holiday = 'Christmas'
	Set @HolidayScheduleCategoryId = 448	
Else If @Holiday = 'Easter'
	Set @HolidayScheduleCategoryId = 284
Else If @Holiday = 'Thanksgiving'
	Set @HolidayScheduleCategoryId = 457
Else Set @HolidayScheduleCategoryId = 448

Declare @ScheduleCategoryIds table(
ScheduleCategoryId int
);

if @IsHoliday = 1
insert into @ScheduleCategoryIds select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@HolidayScheduleCategoryId)
else
begin
	insert into @ScheduleCategoryIds select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WeekendScheduleCategoryId)
	insert into @ScheduleCategoryIds select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@StudentsScheduleCategoryId)
	insert into @ScheduleCategoryIds select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@OtherEventsCategoryId)
end

----------------------------------------------------------------------------
-- GET THE GENERIC METRICVALUE JOINED TABLE
----------------------------------------------------------------------------
Declare @DummyTotalICalContent nvarchar(max) = 'DTSTART:20130501T235959
RRULE:FREQ=WEEKLY;BYDAY=ZZ
SEQUENCE:0';

Declare @MetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

insert into @MetricValues
Select mv.Id
	,m.Id
	,mc.CategoryId
	,Case
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipMetricCategoryId)) Then @WorshipMetricCategoryId
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@StudentsMetricCategoryId)) Then @StudentsMetricCategoryId
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ChildrenMetricCategoryId)) Then @ChildrenMetricCategoryId
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipNightMetricCategoryId)) Then @WorshipNightMetricCategoryId
	end
	,Case
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipMetricCategoryId)) Then 'Worship Center'
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ChildrenMetricCategoryId)) Then 'Children'
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@StudentsMetricCategoryId)) Then 'Students'
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipNightMetricCategoryId)) Then 'Worship Night'
	end
	,Case
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipMetricCategoryId)) Then 1
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipNightMetricCategoryId)) Then 2
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ChildrenMetricCategoryId)) Then 3
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@StudentsMetricCategoryId)) Then 4
	end
	,mv.YValue
	,mv.MetricValueDateTime
	,s.Id
	,s.Name
	,s.iCalendarContent
	,c.Id
	,c.Name
	,mv.Note
	,STR(m.Id)+'-'+STR(c.Id)+'-'+s.Name
From MetricValue mv
join Metric m on mv.MetricId = m.Id
join MetricValuePartition mvpC on mvpC.MetricValueId = mv.Id
join MetricPartition mpC on mvpC.MetricPartitionId = mpC.Id and mpC.EntityTypeId in (select top 1 Id from EntityType where Name='Rock.Model.Campus')
join Campus c on mvpC.EntityId = c.Id
	join MetricValuePartition mvpS on mvpS.MetricValueId = mv.Id
join MetricPartition mpS on mvpS.MetricPartitionId = mpS.Id and mpS.EntityTypeId in (select top 1 Id from EntityType where Name='Rock.Model.Schedule')
join Schedule s on mvpS.EntityId = s.Id
join MetricCategory mc on mc.MetricId = m.Id
Where  mc.CategoryId in (Select * from @MetricCategoryIds )
and s.CategoryId in (Select * from @ScheduleCategoryIds)
and mv.YValue is not null
and ( @IsCampus = 0 or c.id = @CampusId)

----------------------------------------------------------------------------
-- GET THE DATE RANGES
----------------------------------------------------------------------------

-- Weekend Services
declare @ThisWeekEnd datetime ;
If ( @SundayDate is null or @SundayDate = 'null') 
	set @ThisWeekEnd  = (
		select Max(MetricValueDateTime) 
		from  @MetricValues
		)
else set @ThisWeekEnd  = @SundayDate;
declare @ThisMonthStart datetime= DATEADD(mm, DATEDIFF(mm, 0, @ThisWeekEnd -1), 0)
declare @ThisMonthEnd datetime = DATEADD(day, -1,DATEADD(mm, 1, @ThisMonthStart));
declare @ThisWeekStart datetime= DATEADD(wk, DATEDIFF(wk, 0, @ThisWeekEnd -1), 0)
Set @ThisWeekEnd = DATEADD(day, 6, @ThisWeekStart);
declare @LastWeekEnd datetime= DATEADD(day, -1, @ThisWeekStart);
declare @LastWeekStart datetime= DATEADD(day, -6, @LastWeekEnd);
declare @LastYearWeekStart datetime= DATEADD(wk, -52, @ThisWeekStart);
declare @LastYearWeekEnd datetime= DATEADD(day, 6, @LastYearWeekStart);

declare @LastMonthStart datetime= DATEADD(mm, -1, @ThisMonthStart);
declare @LastMonthEnd datetime= DATEADD(day, -1,DATEADD(mm, 1, @LastMonthStart));
declare @LastYearMonthStart datetime= DATEADD(wk, -52, @ThisMonthStart);
declare @LastYearMonthEnd datetime= DATEADD(day, -1,DATEADD(mm, 1, @LastYearMonthStart));

declare @ThisMinistryYearStart datetime = DateAdd(mm, 7, DATEADD(yy,DATEDIFF(yy,0,@ThisWeekEnd),0));
if(@ThisWeekEnd < @ThisMinistryYearStart)
	set @ThisMinistryYearStart = DateAdd(yy,-1, @ThisMinistryYearStart);

declare @LastMinistryYearStart datetime = DateAdd(yy,-1, @ThisMinistryYearStart);

-- Holiday Services
declare @ThisYearEnd datetime = (select Max(MetricValueDateTime) from @MetricValues)
declare @ThisYearStart datetime= DATEADD(day, -30, @ThisYearEnd)
Set @ThisYearEnd = DateAdd(day, 60, @ThisYearStart);
declare @LastYearEnd datetime= DateAdd(year, -1, @ThisYearEnd)
declare @LastYearStart datetime= DateAdd(year, -1, @ThisYearStart);
declare @TwoYearStart datetime= DateAdd(year, -2, @ThisYearStart);
declare @TwoYearEnd datetime= DateAdd(year, -2, @ThisYearEnd);

----------------------------------------------------------------------------
-- DETERMINE IF CURRENT DATE TIME IS THE WEEKEND
----------------------------------------------------------------------------


declare @CurrentDateTime datetime = GetDate();
declare @IsServicesOngoing bit = 0;

-- Weekend Services
If @IsHoliday = 0
begin

declare @currentPeriodDay integer = DatePart(weekday, @CurrentDateTime);
declare @CurrentHour integer = DatePart(hour, @CurrentDateTime);
declare @CurrentSundayDate date = dbo.ufnUtility_GetSundayDate(@CurrentDateTime);

if @CurrentSundayDate = convert(date, @ThisWeekEnd)
Begin
	If @currentPeriodDay = 7 and @CurrentHour >= 16
		 set @IsServicesOngoing = 1
	Else If @currentPeriodDay = 1 and @CurrentHour < 13
		 set @IsServicesOngoing = 1
	Else set @IsServicesOngoing = 0
End

end

-- Holiday Services

if @IsHoliday = 1
begin 

declare @ScheduleStartTimes table(
	StartDateTime datetime
);

insert into @ScheduleStartTimes
Select Distinct 
Convert(datetime, STUFF(STUFF(STUFF(splitTable.Date+''+splitTable.Time,13,0,':'),11,0,':'),9,0,' ')) as 'Datetime'
from (
Select Distinct
	SUBSTRING(ScheduleICalendarContent, CHARINDEX('DTSTART:', ScheduleICalendarContent) + 8, (CHARINDEX('SEQUENCE:', ScheduleICalendarContent)-(CHARINDEX('DTSTART:', ScheduleICalendarContent)+17))) as 'Date'
	,SUBSTRING(ScheduleICalendarContent, CHARINDEX('DTSTART:', ScheduleICalendarContent) + 17, (CHARINDEX('SEQUENCE:', ScheduleICalendarContent)-(CHARINDEX('DTSTART:', ScheduleICalendarContent)+19))) as 'Time'
	,ScheduleICalendarContent
	 from @MetricValues
 ) as splitTable

Declare @StartOfServices datetime = (select Min(StartDateTime) from @ScheduleStartTimes);
Declare @EndOfServices datetime =DateAdd(hour, 1, (select Max(StartDateTime) from @ScheduleStartTimes));

If (@CurrentDateTime >= @StartOfServices and @CurrentDateTime < @EndOfServices)
	Set @IsServicesOngoing = 1;

end

----------------------------------------------------------------------------
-- Build Individual Tables
----------------------------------------------------------------------------

Declare @TotalMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @WorshipMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @ChildrensMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @StudentsMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @BaptismsMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @DiscoverCentralMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

Declare @FirstTimeGuestMetricValues table(
	Id int,
	MetricId int,
	MetricCategoryId int,
	RootMetricCategoryId int,
	RootMetricCategoryName nvarchar(50),
	RootMetricCategoryOrder int,
	Attendance float,
	MetricValueDateTime datetime,
	ScheduleId Int,
	ScheduleName nvarchar(50),
	ScheduleICalendarContent nvarchar(max),
	CampusId int,
	CampusName nvarchar(50),
	Note nvarchar(max),
	MetricKeyString nvarchar(max)
);

----------------------------------------------------------------------------
-- Populate Individual Tables
----------------------------------------------------------------------------

Insert Into @TotalMetricValues
Select *
From @MetricValues
where MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WeekendMetricCategoryId))
and (
		( @IsHoliday = 0 and
			(
				( MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				(MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd)
			)
		)
		or
		( @IsHoliday = 1 and
			(
				( MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd) or
				( MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd) or
				(MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
			)
		)
	)

Insert Into @WorshipMetricValues
Select * From @TotalMetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@WorshipMetricCategoryId)) 

Insert Into @ChildrensMetricValues
Select * From @TotalMetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ChildrenMetricCategoryId))

Insert Into @StudentsMetricValues
Select * From @TotalMetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@StudentsMetricCategoryId))  

Insert Into @BaptismsMetricValues
Select * From @MetricValues
where MetricId = 27

Insert Into @DiscoverCentralMetricValues
Select * From @MetricValues
where MetricId = 73

Insert Into @FirstTimeGuestMetricValues
Select * From @MetricValues
where MetricId = 74

----------------------------------------------------------------------------
-- GRAB THE EXISTING CURRENT METRIC VALUES
----------------------------------------------------------------------------
Declare @CurrentMetrics table(
KeyString nvarchar(max)
);

Insert Into @CurrentMetrics
Select STR(MetricId)+'-'+STR(CampusId)+'-'+ScheduleName
From @MetricValues
where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd

-----------------------------------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------------------------------------
------
------ Build Data Tables
------
-----------------------------------------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------------------------------------

----------------------------------------------------------------------------
-- GRAB THE TOTAL WEEKEND ATTENDANCE
----------------------------------------------------------------------------
Select *
From 
(
	Select titleTable.AttendanceType as 'AttendanceType'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	--, thisweekNotes.Notes as 'ThisWeekNotes'
	, CASE thisweekNotes.Notes WHEN null THEN '' WHEN '' THEN '' ELSE SUBSTRING(thisweekNotes.Notes, 1, LEN(thisweekNotes.Notes) - 1 ) END AS 'ThisWeekNotes'
	--, lastweekNotes.Notes as 'LastWeekNotes'
	, CASE lastweekNotes.Notes WHEN null THEN '' WHEN '' THEN '' ELSE SUBSTRING(lastweekNotes.Notes, 1, LEN(lastweekNotes.Notes) - 1 ) END AS 'LastWeekNotes'
	--, lastyearNotes.Notes as 'LastYearNotes'
	, CASE lastyearNotes.Notes WHEN null THEN '' WHEN '' THEN '' ELSE SUBSTRING(lastyearNotes.Notes, 1, LEN(lastyearNotes.Notes) - 1 ) END AS 'LastYearNotes'
	From 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN Max(RootMetricCategoryOrder)+1 ELSE Max(RootMetricCategoryOrder) END as 'Order'
		From @TotalMetricValues
		group by Rollup(RootMetricCategoryId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(RootMetricCategoryId)
	) as currentPeriod
	on titleTable.AttendanceType = currentPeriod.AttendanceType
	left join  
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues mv
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as lastPeriod
	on titleTable.AttendanceType = lastPeriod.AttendanceType
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as secondLastPeriod
	on titleTable.AttendanceType = secondLastPeriod.AttendanceType

	-- Concat all the Notes
	-- This Week Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @TotalMetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @TotalMetricValues p1
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(RootMetricCategoryId)
	) as thisweekNotes
	on titleTable.AttendanceType = thisweekNotes.AttendanceType

	-- Last Week Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @TotalMetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @TotalMetricValues p1
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		group by Rollup(RootMetricCategoryId)
	) as lastweekNotes
	on titleTable.AttendanceType = lastweekNotes.AttendanceType

	-- Last Year Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @TotalMetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @TotalMetricValues p1
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		group by Rollup(RootMetricCategoryId)
	) as lastyearNotes
	on titleTable.AttendanceType = lastyearNotes.AttendanceType

) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
Order By [Order]

----------------------------------------------------------------------------
-- GRAB THE SERVICE BREAKDOWN ( Weekend Services )
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'
	,Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (lastPeriod.Attendance != 0 and lastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'		
		From @TotalMetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
order by [Date], [Time], iCalendarContent

----------------------------------------------------------------------------
-- GRAB THE CAMPUS ROLLUP
----------------------------------------------------------------------------
If @IsCampus = 0
Begin

Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	,Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (lastPeriod.Attendance != 0 and lastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,CASE GROUPING(CampusId) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @TotalMetricValues
		group by Rollup(CampusId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(CampusId)
	) as currentPeriod
	on titleTable.Campus = currentPeriod.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastPeriod
	on titleTable.Campus = lastPeriod.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @TotalMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(CampusId)
	) as secondLastPeriod
	on titleTable.Campus = secondLastPeriod.Campus
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
Order By [Order], Campus

End

----------------------------------------------------------------------------
-- GRAB THE WORSHIP CENTER ( All Church )
----------------------------------------------------------------------------
If @IsCampus = 0
Begin
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'	
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,CASE GROUPING(CampusId) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @TotalMetricValues
		group by Rollup(CampusId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(CampusId)
	) as currentPeriod
	on titleTable.Campus = currentPeriod.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastPeriod
	on titleTable.Campus = lastPeriod.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as secondLastPeriod
	on titleTable.Campus = secondLastPeriod.Campus
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
Order By [Order], Campus
end

----------------------------------------------------------------------------
-- GRAB THE WORSHIP CENTER ( Campus Specific )
----------------------------------------------------------------------------
If @IsCampus = 1
Begin
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'	
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'
	
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @TotalMetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
order by [Date], [Time], iCalendarContent
end
----------------------------------------------------------------------------
-- GRAB THE CHILDREN'S ( All Campus)
----------------------------------------------------------------------------
If @IsCampus = 0
Begin
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,CASE GROUPING(CampusId) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(CampusId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(CampusId)
	) as currentPeriod
	on titleTable.Campus = currentPeriod.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastPeriod
	on titleTable.Campus = lastPeriod.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as secondLastPeriod
	on titleTable.Campus = secondLastPeriod.Campus
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
Order By [Order], Campus
End

----------------------------------------------------------------------------
-- GRAB THE CHILDREN'S ( Campus Specific)
----------------------------------------------------------------------------
If @IsCampus = 1
Begin
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'
	
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
order by [Date], [Time], iCalendarContent
End
----------------------------------------------------------------------------
-- GRAB THE STUDENTS ( All Campus )
----------------------------------------------------------------------------
If @IsHoliday = 0 and @IsCampus = 0
Begin

Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,CASE GROUPING(CampusId) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(CampusId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(CampusId)
	) as currentPeriod
	on titleTable.Campus = currentPeriod.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastPeriod
	on titleTable.Campus = lastPeriod.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as secondLastPeriod
	on titleTable.Campus = secondLastPeriod.Campus
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
Order By [Order], Campus

end
----------------------------------------------------------------------------
-- GRAB THE STUDENTS ( Campus Specific )
----------------------------------------------------------------------------
If @IsHoliday = 0 and @IsCampus = 1
Begin

Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'CurrentPeriod'
	, Format(lastPeriod.Attendance,'N0')  as 'LastPeriod'
	, Format(secondLastPeriod.Attendance,'N0')  as 'SecondLastPeriod'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd)
				)
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where (
				( @IsHoliday = 0 and MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd) or
				( @IsHoliday = 1 and MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd)
				)
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.CurrentPeriod is not null
or innerTable.LastPeriod is not null
or innerTable.SecondLastPeriod is not null
order by [Date], [Time], iCalendarContent

end

----------------------------------------------------------------------------
-- GRAB THE BAPTISMS ( All Church )
----------------------------------------------------------------------------
If @IsHoliday = 0 and @IsCampus = 0
begin

Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'ThisWeek'
	, Format(thisYearToDate.Attendance,'N0')  as 'ThisYearToDate'
	, Format(lastYearToDate.Attendance,'N0')  as 'LastYearToDate'
	, Case When (lastYearToDate.Attendance != 0 and lastYearToDate.Attendance is not null) then (thisYearToDate.Attendance - lastYearToDate.Attendance) / lastYearToDate.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,CASE GROUPING(CampusId) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(CampusId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentPeriod
	on titleTable.Campus = currentPeriod.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @ThisMinistryYearStart and MetricValueDateTime <= @ThisWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as thisYearToDate
	on titleTable.Campus = thisYearToDate.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastMinistryYearStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearToDate
	on titleTable.Campus = lastYearToDate.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.ThisYearToDate is not null
or innerTable.LastYearToDate is not null
Order By [Order], Campus

end

----------------------------------------------------------------------------
-- GRAB THE BAPTISMS ( Campus Specific)
----------------------------------------------------------------------------
If @IsHoliday = 0 and @IsCampus = 1
begin

Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'ThisWeek'
	, Format(thisYearToDate.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearToDate.Attendance,'N0')  as 'LastYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'
	, Case When (lastYearToDate.Attendance != 0 and lastYearToDate.Attendance is not null) then (thisYearToDate.Attendance - lastYearToDate.Attendance) / lastYearToDate.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as thisYearToDate
	on titleTable.Schedule = thisYearToDate.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearToDate
	on titleTable.Schedule = lastYearToDate.Schedule
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
order by [Date], [Time], iCalendarContent

end

----------------------------------------------------------------------------
-- GRAB THE DISCOVER CENTRAL
----------------------------------------------------------------------------
If @IsHoliday = 0 and @IsCampus = 1
Begin

Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'ThisWeek'
	, Format(lastPeriod.Attendance,'N0')  as 'LastWeek'
	, Format(secondLastPeriod.Attendance,'N0')  as 'LastYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'	
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @ThisMonthStart and MetricValueDateTime <= @ThisMonthEnd
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @LastMonthStart and MetricValueDateTime <= @LastMonthEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @LastYearMonthStart and MetricValueDateTime <= @LastYearMonthEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
order by [Date], [Time], iCalendarContent
----------------------------------------------------------------------------
-- GRAB THE FIRST TIME GUESTS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	, Format(currentPeriod.Attendance,'N0')  as 'ThisWeek'
	, Format(lastPeriod.Attendance,'N0')  as 'LastWeek'
	, Format(secondLastPeriod.Attendance,'N0')  as 'LastYear'
	,titleTable.iCalendarContent
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) Else '' end as 'Time'
	,Case when titleTable.iCalendarContent like '%RRULE%' then SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6)))Else '' end as 'Date'	
	, Case When (secondLastPeriod.Attendance != 0 and secondLastPeriod.Attendance is not null) then (currentPeriod.Attendance - secondLastPeriod.Attendance) / secondLastPeriod.Attendance  else null end As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN 1 ELSE 0 END as 'Order'
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(ScheduleName)
	) as currentPeriod
	on titleTable.Schedule = currentPeriod.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastPeriod
	on titleTable.Schedule = lastPeriod.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select * from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as secondLastPeriod
	on titleTable.Schedule = secondLastPeriod.Schedule
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
order by [Date], [Time], iCalendarContent

end








