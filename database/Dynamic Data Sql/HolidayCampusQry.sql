Declare @Holiday nvarchar(50) = 'Christmas'
Declare @CampusId int = 1
Declare @SundayDate datetime = null

----------------------------------------------------------------------------
-- GET THE CATEGORY IDS
----------------------------------------------------------------------------
-- These are the root category Ids
DECLARE @ParentWeekendCategoryId INT = 435;
DECLARE @ParentWorshipCategoryId INT = 443;
DECLARE @ParentChildrenCategoryId INT = 440;
DECLARE @ParentStudentsCategoryId INT = 444;

DECLARE @ParentScheduleCategoryId INT;
If @Holiday = 'Christmas'
	Set @ParentScheduleCategoryId = 448	
Else If @Holiday = 'Easter'
	Set @ParentScheduleCategoryId = 284
Else If @Holiday = 'Thanksgiving'
	Set @ParentScheduleCategoryId = 457
Else Set @ParentScheduleCategoryId = 448
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
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWorshipCategoryId)) Then @ParentWorshipCategoryId
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentStudentsCategoryId)) Then @ParentStudentsCategoryId
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentChildrenCategoryId)) Then @ParentChildrenCategoryId
	end
	,Case
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWorshipCategoryId)) Then 'Worship Center'
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentChildrenCategoryId)) Then 'Children'
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentStudentsCategoryId)) Then 'Students'
	end
	,Case
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWorshipCategoryId)) Then 1
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentChildrenCategoryId)) Then 2
		When  mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentStudentsCategoryId)) Then 3
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
Where ( mc.CategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWeekendCategoryId)) or mv.MetricId =27 or mv.MetricId = 73 or mv.MetricId = 74)
and ( s.CategoryId in (select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentScheduleCategoryId)))
and mv.YValue is not null
and c.id = @CampusId

----------------------------------------------------------------------------
-- GET THE DATE RANGES
----------------------------------------------------------------------------

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
----------------------------------------------------------------------------
-- SPLIT INTO INDIVIDUAL TABLES
----------------------------------------------------------------------------
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

Insert Into @WorshipMetricValues
Select * From @MetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWorshipCategoryId)) 

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

Insert Into @ChildrensMetricValues
Select * From @MetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentChildrenCategoryId))

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

Insert Into @StudentsMetricValues
Select * From @MetricValues
Where  MetricCategoryId in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentStudentsCategoryId))  

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

Insert Into @BaptismsMetricValues
Select *
From @MetricValues
where MetricId = 27

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

Insert Into @DiscoverCentralMetricValues
Select *
From @MetricValues
where MetricId = 73

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

Insert Into @FirstTimeGuestMetricValues
Select *
From @MetricValues
where MetricId = 74

Delete
From @MetricValues
Where MetricCategoryId not in ( Select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentWeekendCategoryId))
----------------------------------------------------------------------------
-- GRAB THE EXISTING CURRENT METRIC VALUES
----------------------------------------------------------------------------
Declare @CurrentMetrics table(
KeyString nvarchar(max)
);

Insert Into @CurrentMetrics
Select STR(MetricId)+'-'+STR(CampusId)+'-'+ScheduleName
From @MetricValues
where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
----------------------------------------------------------------------------
-- GRAB THE TOTAL WEEKEND ATTENDANCE
----------------------------------------------------------------------------
Select *
From 
(
	Select titleTable.AttendanceType as 'AttendanceType'
	, titleTable.[Order] as 'Order'
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
	From 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN Max(RootMetricCategoryOrder)+1 ELSE Max(RootMetricCategoryOrder) END as 'Order'
		From @MetricValues
		group by Rollup(RootMetricCategoryId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(RootMetricCategoryId)
	) as currentYearService
	on titleTable.AttendanceType = currentYearService.AttendanceType
	left join  
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues mv
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as lastYearService
	on titleTable.AttendanceType = lastYearService.AttendanceType
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as TwoYearService
	on titleTable.AttendanceType = TwoYearService.AttendanceType
) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order]

----------------------------------------------------------------------------
-- GRAB THE SERVICE BREAKDOWN
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
	From 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,CASE GROUPING(ScheduleName) WHEN 1 THEN @DummyTotalICalContent ELSE Max(ScheduleICalendarContent) End as 'iCalendarContent'		
		From @MetricValues
		group by Rollup(ScheduleName)
	) as titleTable
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
order by iCalendarContent

----------------------------------------------------------------------------
-- GRAB THE WORSHIP CENTER
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		From @WorshipMetricValues
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule
----------------------------------------------------------------------------
-- GRAB THE CHILDREN'S
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule
----------------------------------------------------------------------------
-- GRAB THE STUDENTS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule
----------------------------------------------------------------------------
-- GRAB THE BAPTISMS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule
----------------------------------------------------------------------------
-- GRAB THE DISCOVER CENTRAL
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule
----------------------------------------------------------------------------
-- GRAB THE FIRST TIME GUESTS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	, titleTable.[Order] as 'Order'
	,titleTable.iCalendarContent
	,Format(currentYearService.Attendance,'N0')  as 'ThisYearAttendance'
	,DatePart(Year, @ThisYearStart ) as 'ThisYear'
	, Format(lastYearService.Attendance,'N0')  as 'LastYearAttendance'
	,DatePart(Year, @LastYearStart ) as 'LastYear'
	, Format(TwoYearService.Attendance,'N0')  as 'TwoYearAttendance'
	,DatePart(Year, @TwoYearStart ) as 'TwoYear'
	, (currentYearService.Attendance - lastYearService.Attendance) / lastYearService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisYearStart and MetricValueDateTime <= @ThisYearEnd
		group by Rollup(ScheduleName)
	) as currentYearService
	on titleTable.Schedule = currentYearService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @LastYearStart and MetricValueDateTime <= @LastYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearService
	on titleTable.Schedule = lastYearService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @TwoYearStart and MetricValueDateTime <= @TwoYearEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select *from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as TwoYearService
	on titleTable.Schedule = TwoYearService.Schedule
	) innerTable
where innerTable.ThisYearAttendance is not null
or innerTable.LastYearAttendance is not null
or innerTable.TwoYearAttendance is not null
Order By [Order], Schedule