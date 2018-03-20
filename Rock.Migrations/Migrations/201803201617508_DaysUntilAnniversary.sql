BEGIN TRY
    ALTER TABLE Person

    DROP COLUMN DaysUntilAnniversary
END TRY

BEGIN CATCH
END CATCH

ALTER TABLE Person ADD DaysUntilAnniversary AS (
    CASE 
        -- if there anniversary is Feb 29 and their next anniversary is this year and it isn't a leap year, set their anniversary to Feb 28 (this year)
        WHEN (
                DATEPART(MONTH, AnniversaryDate) = 2
                AND DATEPART(DAY, AnniversaryDate) = 29
                AND datepart(month, sysdatetime()) < 3
                AND (isdate(convert(VARCHAR(4), datepart(year, sysdatetime())) + '-02-29') = 0)
                )
            THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, sysdatetime())) + '-02-28'))
        -- if there anniversary is Feb 29 and their next anniversary is next year and it isn't a leap year, set their anniversary to Feb 28 (next year)
        WHEN (
                DATEPART(MONTH, AnniversaryDate) = 2
                AND DATEPART(DAY, AnniversaryDate) = 29
                AND (isdate(convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-02-29') = 0)
                )
            THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-02-28'))
        -- otherwise return the number of days normally if they have a valid Anniversary Month and Anniversary Day 
        ELSE CASE 
                WHEN ((DATEPART(MONTH, AnniversaryDate) = 2 and DATEPART(DAY, AnniversaryDate) < 30) or
                isdate(convert(VARCHAR(4), datepart(year, sysdatetime())) + '-' + right('00' + CONVERT([varchar](2), DATEPART(MONTH, AnniversaryDate)), (2)) + '-' + right('00' + CONVERT([varchar](2), DATEPART(DAY, AnniversaryDate)), (2))) = 1
                )
                    THEN CASE 
                            WHEN ( (datepart(month, sysdatetime())*100) + datepart(day,sysdatetime()) > ((DATEPART(MONTH, AnniversaryDate)*100) + DATEPART(DAY, AnniversaryDate)))
                                -- next anniversary is next year
                                THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-' + right('00' + CONVERT([varchar](2), DATEPART(MONTH, AnniversaryDate)), (2)) + '-' + right('00' + CONVERT([varchar](2), DATEPART(DAY, AnniversaryDate)), (2))))
                                -- next anniversary is this year
                                ELSE datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, sysdatetime())) + '-' + right('00' + CONVERT([varchar](2), DATEPART(MONTH, AnniversaryDate)), (2)) + '-' + right('00' + CONVERT([varchar](2), DATEPART(DAY, AnniversaryDate)), (2))))
                            END
                ELSE NULL
                END
        END
    )    