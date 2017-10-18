Declare @SundayDate datetime = null

----------------------------------------------------------------------------
-- GET THE CATEGORY IDS
----------------------------------------------------------------------------
-- These are the root category Ids
DECLARE @ParentWeekendCategoryId INT = 435;
DECLARE @ParentWorshipCategoryId INT = 443;
DECLARE @ParentChildrenCategoryId INT = 440;
DECLARE @ParentStudentsCategoryId INT = 444;
DECLARE @ParentScheduleCategoryId INT = 50;
Declare @ParentStudentsScheduleCategoryId INT = 496;

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
and ( s.CategoryId in (select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentScheduleCategoryId)) or s.CategoryId in (select Id from dbo.ufnChurchMetrics_GetDescendantCategoriesFromRoot(@ParentStudentsScheduleCategoryId)))
and mv.YValue is not null

----------------------------------------------------------------------------
-- GET THE DATE RANGES
----------------------------------------------------------------------------

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

----------------------------------------------------------------------------
-- DETERMINE IF CURRENT DATE TIME IS THE WEEKEND
----------------------------------------------------------------------------
declare @CurrentDateTime datetime = GetDate(); 
declare @CurrentWeekDay integer = DatePart(weekday, @CurrentDateTime);
declare @CurrentHour integer = DatePart(hour, @CurrentDateTime);
declare @CurrentSundayDate date = dbo.ufnUtility_GetSundayDate(@CurrentDateTime);
declare @IsServicesOngoing bit;

if @CurrentSundayDate = convert(date, @ThisWeekEnd)
Begin
	If @CurrentWeekDay = 7 and @CurrentHour >= 16
		 set @IsServicesOngoing = 1
	Else If @CurrentWeekDay = 1 and @CurrentHour < 13
		 set @IsServicesOngoing = 1
	Else set @IsServicesOngoing = 0
End
Else
set @IsServicesOngoing = 0

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
----------------------------------------------------------------------------
-- GRAB THE TOTAL WEEKEND ATTENDANCE
----------------------------------------------------------------------------
Select *
From 
(
	Select titleTable.AttendanceType as 'AttendanceType'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		From @MetricValues
		group by Rollup(RootMetricCategoryId)
	) as titleTable
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(RootMetricCategoryId)
	) as currentWeekService
	on titleTable.AttendanceType = currentWeekService.AttendanceType
	left join  
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues mv
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as lastWeekService
	on titleTable.AttendanceType = lastWeekService.AttendanceType
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(RootMetricCategoryId)
	) as lastYearWeekService
	on titleTable.AttendanceType = lastYearWeekService.AttendanceType

	-- Concat all the Notes
	-- This Week Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @MetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @MetricValues p1
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(RootMetricCategoryId)
	) as thisweekNotes
	on titleTable.AttendanceType = thisweekNotes.AttendanceType

	-- Last Week Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @MetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @MetricValues p1
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		group by Rollup(RootMetricCategoryId)
	) as lastweekNotes
	on titleTable.AttendanceType = lastweekNotes.AttendanceType

	-- Last Year Notes
	left join 
	(
		select CASE GROUPING(RootMetricCategoryId) WHEN 1 THEN 'Total' ELSE Max(RootMetricCategoryName) END as 'AttendanceType'
		,
			( SELECT Note + ',' 
           FROM @MetricValues p2
          WHERE p2.RootMetricCategoryId = p1.RootMetricCategoryId
		  AND MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		  and p2.Note is not null
		  and p2.Note != ''
          ORDER BY MetricValueDateTime
            FOR XML PATH('') ) AS Notes
		From @MetricValues p1
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		group by Rollup(RootMetricCategoryId)
	) as lastyearNotes
	on titleTable.AttendanceType = lastyearNotes.AttendanceType

) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order]

----------------------------------------------------------------------------
-- GRAB THE SERVICE BREAKDOWN
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Schedule as 'Schedule'
	,SUBSTRING(titleTable.iCalendarContent, CHARINDEX('DTSTART:', titleTable.iCalendarContent) + 16, (CHARINDEX('RRULE:', titleTable.iCalendarContent)-(CHARINDEX('DTSTART:', titleTable.iCalendarContent)+16))) as 'Time'
	,SUBSTRING(titleTable.iCalendarContent, CHARINDEX('BYDAY=', titleTable.iCalendarContent) + 6, (CHARINDEX('SEQUENCE:', titleTable.iCalendarContent)-(CHARINDEX('BYDAY=', titleTable.iCalendarContent)+6))) as 'Date'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(ScheduleName)
	) as currentWeekService
	on titleTable.Schedule = currentWeekService.Schedule
	left join  
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastWeekService
	on titleTable.Schedule = lastWeekService.Schedule
	left join 
	(
		select CASE GROUPING(ScheduleName) WHEN 1 THEN 'Total' ELSE Max(ScheduleName) END as 'Schedule'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(ScheduleName)
	) as lastYearWeekService
	on titleTable.Schedule = lastYearWeekService.Schedule
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
order by [Date], [Time]
----------------------------------------------------------------------------
-- GRAB THE CAMPUS ROLLUP
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		From @MetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @MetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE WORSHIP CENTER
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		From @WorshipMetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @WorshipMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE CHILDREN'S
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @ChildrensMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE STUDENTS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @StudentsMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE BAPTISMS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @BaptismsMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE DISCOVER CENTRAL
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @ThisMonthStart and MetricValueDateTime <= @ThisMonthEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @LastMonthStart and MetricValueDateTime <= @LastMonthEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @DiscoverCentralMetricValues
		where MetricValueDateTime >= @LastYearMonthStart and MetricValueDateTime <= @LastYearMonthEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus
----------------------------------------------------------------------------
-- GRAB THE FIRST TIME GUESTS
----------------------------------------------------------------------------
Select *
From (
	Select titleTable.Campus as 'Campus'
	, titleTable.[Order] as 'Order'
	, Format(currentWeekService.Attendance,'N0')  as 'ThisWeek'
	, Format(lastWeekService.Attendance,'N0')  as 'LastWeek'
	, Format(lastYearWeekService.Attendance,'N0')  as 'LastYear'
	, (currentWeekService.Attendance - lastYearWeekService.Attendance) / lastYearWeekService.Attendance As 'Growth'
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
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @ThisWeekStart and MetricValueDateTime <= @ThisWeekEnd
		group by Rollup(CampusId)
	) as currentWeekService
	on titleTable.Campus = currentWeekService.Campus
	left join  
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @LastWeekStart and MetricValueDateTime <= @LastWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastWeekService
	on titleTable.Campus = lastWeekService.Campus
	left join 
	(
		select CASE GROUPING(CampusId) WHEN 1 THEN 'Total' ELSE Max(CampusName) END as 'Campus'
		,Sum(Attendance) as 'Attendance'
		From @FirstTimeGuestMetricValues
		where MetricValueDateTime >= @LastYearWeekStart and MetricValueDateTime <= @LastYearWeekEnd
		and (@IsServicesOngoing = 0 or MetricKeyString in (select MetricKeyString from @CurrentMetrics))
		group by Rollup(CampusId)
	) as lastYearWeekService
	on titleTable.Campus = lastYearWeekService.Campus
	) innerTable
where innerTable.ThisWeek is not null
or innerTable.LastWeek is not null
or innerTable.LastYear is not null
Order By [Order], Campus