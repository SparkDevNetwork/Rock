TRUNCATE TABLE AnalyticsDimDate

SELECT DATENAME(DAY, SYSDATETIME())

-- see https://www.mssqltips.com/sqlservertip/4054/creating-a-date-dimension-or-calendar-table-in-sql-server/ for some background
DECLARE @BeginDate DATE = DateFromParts(1850, 1, 1)
    ,@InsertDate DATE
    ,@EndDate DATE = DateFromParts(2100, 1, 1)

BEGIN
    SET @InsertDate = @BeginDate

    BEGIN TRANSACTION

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
                ,[DayOfWeekName]
                ,[DayOfWeekAbbreviated]
                ,[DayNumberInCalendarMonth]
                ,[DayNumberInCalendarYear]
                ,[DayNumberInFiscalMonth]
                ,[DayNumberInFiscalYear]
                ,[LastDayInMonthIndictor]
                ,[WeekNumberInMonth]
                ,[SundayDate]
                ,[GivingMonth]
                ,[GivingMonthName]
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
                ,DATEPART(WEEKDAY, @InsertDate)
                ,DATENAME(WEEKDAY, @InsertDate) --<DayOfWeekName, nvarchar(max),>
                ,SUBSTRING(DATENAME(WEEKDAY, @InsertDate), 1, 3) --<DayOfWeekAbbreviated, nvarchar(max),>
                ,DATENAME(DAY, @InsertDate) --<DayNumberInCalendarMonth, int,>
                ,DATENAME(DAYOFYEAR, @InsertDate) --<DayNumberInCalendarYear, int,>
                ,0 --<DayNumberInFiscalMonth, int,>
                ,0 --<DayNumberInFiscalYear, int,>
                ,CASE 
                    WHEN EOMONTH(@InsertDate) = @InsertDate
                        THEN 1
                    ELSE 0
                    END --<LastDayInMonthIndictor, bit,>
                ,0 --<WeekNumberInMonth, int,>
                ,dbo.ufnUtility_GetSundayDate(@InsertDate) -- <SundayDate, date,>
                ,0 --<GivingMonth, int,> UI will ask if [use Month of SundayDate, use actual month]
                ,'' --<GivingMonthName, nvarchar(max),> UI will ask if [use Month of SundayDate, use actual month]
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
                ,0 --datepart(year, @InsertDate) --<FiscalYear, int,>
                ,0 --<HolidayIndicator, bit,>
                ,0 --<WeekHolidayIndicator, bit,>
                ,0 --<EasterIndicator, bit,>
                ,0 --<EasterWeekIndicator, bit,>
                ,0 --<ChristmasIndicator, bit,>
                ,0 --<ChristmasWeekIndicator, bit,>
        END

        SET @InsertDate = DATEADD(day, 1, @InsertDate)
    END

    COMMIT TRANSACTION
END
