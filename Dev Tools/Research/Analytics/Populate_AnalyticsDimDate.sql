-- see https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/ for some background
DECLARE @BeginDate DATE = DateFromParts(1850, 1, 1)
    ,@InsertDate DATE
    ,@EndDate DATE = DateFromParts(2100, 1, 1)

BEGIN
    SET @InsertDate = @BeginDate

    WHILE (@InsertDate < @EndDate)
    BEGIN
        IF NOT EXISTS (
                SELECT 1
                FROM AnalyticsDimDate
                WHERE [Date] = @InsertDate
                )
        BEGIN
            INSERT INTO AnalyticsDimDate (
                [DateKey]
                ,[Date]
                ,[FullDateDescription]
                ,[DayOfWeek]
                ,[DayOfWeekAbbreviated]
                ,[DayNumberInCalendarMonth]
                ,[DayNumberInCalendarYear]
                ,[DayNumberInFiscalMonth]
                ,[DayNumberInFiscalYear]
                ,[LastDayInMonthIndictor]
                ,[WeekNumberInMonth]
                ,[SundayDate]
                ,[GivingMonth]
                ,[CalendarWeekNumberInYear]
                ,[CalendarInMonthName]
                ,[CalendarInMonthNameAbbrevated]
                ,[CalendarMonthNumberInYear]
                ,[CalendarYearMonth]
                ,[CalendarQuarter]
                ,[CalendarYearQuarter]
                ,[CalendarYear]
                ,[FiscalWeek]
                ,[FiscalWeekNumberInYear]
                ,[FiscalMonth]
                ,[FiscalMonthAbbrevated]
                ,[FiscalMonthNumberInYear]
                ,[FiscalMonthYear]
                ,[FiscalQuarter]
                ,[FiscalYearQuarter]
                ,[FiscalHalfYear]
                ,[FiscalYear]
                ,[HolidayIndicator]
                ,[WeekHolidayIndicator]
                ,[EasterIndicator]
                ,[EasterWeekIndicator]
                ,[ChristmasIndicator]
                ,[ChristmasWeekIndicator]
                )
            SELECT convert(INT, (convert(CHAR(8), @InsertDate, 112)))
                ,@InsertDate
                ,NULL -- FullDateDescription
                ,DATENAME(WEEKDAY, @InsertDate) --<DayOfWeek, nvarchar(max),>
                ,'' --<DayOfWeekAbbreviated, nvarchar(max),>
                ,DATENAME(DAY, @InsertDate) --<DayNumberInCalendarMonth, int,>
                ,DATENAME(DAYOFYEAR, @InsertDate) --<DayNumberInCalendarYear, int,>
                ,0 --<DayNumberInFiscalMonth, int,>
                ,0 --<DayNumberInFiscalYear, int,>
                ,case when EOMONTH(@InsertDate) = @InsertDate then 1 else 0 end  --<LastDayInMonthIndictor, bit,>
                ,0 --<WeekNumberInMonth, int,>
                ,dbo.ufnUtility_GetSundayDate(@InsertDate) -- <SundayDate, date,>
                ,0 --<GivingMonth, nvarchar(max),>
                ,0 --<CalendarWeekNumberInYear, int,>
                ,'' --<CalendarInMonthName, nvarchar(max),>
                ,'' --<CalendarInMonthNameAbbrevated, nvarchar(max),>
                ,0 --<CalendarMonthNumberInYear, int,>
                ,'' --<CalendarYearMonth, nvarchar(max),>
                ,'' --<CalendarQuarter, nvarchar(max),>
                ,'' --<CalendarYearQuarter, nvarchar(max),>
                ,datepart(year, @InsertDate) --<CalendarYear, int,>
                ,'' --<FiscalWeek, int,>
                ,'' --<FiscalWeekNumberInYear, int,>
                ,'' --<FiscalMonth, nvarchar(max),>
                ,'' --<FiscalMonthAbbrevated, nvarchar(max),>
                ,'' --<FiscalMonthNumberInYear, int,>
                ,'' --<FiscalMonthYear, nvarchar(max),>
                ,'' --<FiscalQuarter, nvarchar(max),>
                ,'' --<FiscalYearQuarter, nvarchar(max),>
                ,'' --<FiscalHalfYear, nvarchar(max),>
                ,datepart(year, @InsertDate) --<FiscalYear, int,>
                ,0 --<HolidayIndicator, bit,>
                ,0 --<WeekHolidayIndicator, bit,>
                ,0 --<EasterIndicator, bit,>
                ,0 --<EasterWeekIndicator, bit,>
                ,0 --<ChristmasIndicator, bit,>
                ,0 --<ChristmasWeekIndicator, bit,>
        END

        SET @InsertDate = DATEADD(day, 1, @InsertDate)
    END
END
