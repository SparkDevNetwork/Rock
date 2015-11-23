BEGIN TRY
    ALTER TABLE Person

    DROP COLUMN DaysUntilBirthday
END TRY

BEGIN CATCH
END CATCH

ALTER TABLE Person ADD DaysUntilBirthday AS (
    CASE 
        -- if there birthday is Feb 29 and their next birthday is this year and it isn't a leap year, set their birthday to Feb 28 (this year)
        WHEN (
                BirthMonth = 2
                AND BirthDay = 29
                AND datepart(month, sysdatetime()) < 3
                AND (isdate(convert(VARCHAR(4), datepart(year, sysdatetime())) + '-02-29') = 0)
                )
            THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, sysdatetime())) + '-02-28'))
        -- if there birthday is Feb 29 and their next birthday is next year and it isn't a leap year, set their birthday to Feb 28 (next year)
        WHEN (
                BirthMonth = 2
                AND BirthDay = 29
                AND (isdate(convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-02-29') = 0)
                )
            THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-02-28'))
        -- otherwise return the number of days normally if they have a valid BirthMonth and BirthDay 
        ELSE CASE 
                WHEN ((BirthMonth = 2 and BirthDay < 30) or
                isdate(convert(VARCHAR(4), datepart(year, sysdatetime())) + '-' + right('00' + CONVERT([varchar](2), BirthMonth), (2)) + '-' + right('00' + CONVERT([varchar](2), BirthDay), (2))) = 1
                )
                    THEN CASE 
                            WHEN ( (datepart(month, sysdatetime())*100) + datepart(day,sysdatetime()) > ((BirthMonth*100) + BirthDay))
                                -- next birthday is next year
                                THEN datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, dateadd(year, 1, sysdatetime()))) + '-' + right('00' + CONVERT([varchar](2), [birthmonth]), (2)) + '-' + right('00' + CONVERT([varchar](2), [birthday]), (2))))
                                -- next birthday is this year
                                ELSE datediff(day, sysdatetime(), convert(DATE, convert(VARCHAR(4), datepart(year, sysdatetime())) + '-' + right('00' + CONVERT([varchar](2), [birthmonth]), (2)) + '-' + right('00' + CONVERT([varchar](2), [birthday]), (2))))
                            END
                ELSE NULL
                END
        END
    )    