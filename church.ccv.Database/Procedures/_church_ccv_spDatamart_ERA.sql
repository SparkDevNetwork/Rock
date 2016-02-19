/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spDatamart_ERA
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spDatamart_ERA]
	</code>
</doc>
*/
CREATE PROC [dbo].[_church_ccv_spDatamart_ERA] @CurrentDate DATETIME
AS
BEGIN
    DECLARE @SaturdayDate DATETIME

    SET @SaturdayDate = dbo._church_ccv_ufnGetSaturdayDate(@CurrentDate)

    DELETE [dbo].[_church_ccv_Datamart_ERA]
    WHERE WeekendDate = @SaturdayDate;

    /****************************************************************************
		
		ATTENDANCE 	 

		Calculate the number of times each family has attended in the last 16
		weeks and when they last attended based on children's attendance records.
		
	*****************************************************************************/
    INSERT INTO [dbo].[_church_ccv_Datamart_ERA] (
        WeekendDate
        ,FamilyId
        ,TimesAttendedLast16Weeks
        )
    SELECT @SaturdayDate
        ,FM.GroupId
        ,COUNT(DISTINCT dbo._church_ccv_ufnGetSaturdayDate(A.StartDateTime))
    FROM Attendance A
    INNER JOIN GroupMember FM ON FM.PersonId = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId)
    INNER JOIN [Group] F ON F.Id = FM.GroupId
        AND F.GroupTypeId = 10
    WHERE A.DidAttend = 1
        AND A.GroupId IN (
            1199220
            ,1199221
            ,1199222
            ,1199223
            ,1199224
            ,1199225
            ,1199226
            ,1199227
            ,1199228
            ,1199231
            ,1199232
            ,1199233
            ,1199600
            ,1199601
            ,1199602
            )
        AND A.StartDateTime <= DATEADD(DAY, - 1, @SaturdayDate)
        AND A.StartDateTime >= DATEADD(DAY, - 1, DATEADD(week, - 15, @SaturdayDate))
    GROUP BY FM.GroupId

    /****************************************************************************
		
		GIVING 	 

		Calculate the number of times each family has given in the last 6 weeks,
		total times they've given and when the last time they gave was
		
	*****************************************************************************/
    DECLARE @FamilyContributionCount TABLE (
        FamilyId [int] NOT NULL
        ,total_count [int] NULL
        ,year_count [int] NULL
        ,recent_count [int] NULL
        ,last_gave [datetime] NULL
        );

    WITH CTE
    AS (
        SELECT dbo._church_ccv_ufnGetSaturdayDate(FT.TransactionDateTime) AS WeekendDate
            ,FM.GroupId
            ,CASE 
                WHEN dbo._church_ccv_ufnGetSaturdayDate(FT.TransactionDateTime) >= DATEADD(WEEK, - 52, @SaturdayDate)
                    THEN 1
                ELSE 0
                END AS last_year
            ,CASE 
                WHEN dbo._church_ccv_ufnGetSaturdayDate(FT.TransactionDateTime) >= DATEADD(WEEK, - 5, @SaturdayDate)
                    THEN 1
                ELSE 0
                END AS recent
        FROM FinancialTransaction FT
        INNER JOIN FinancialTransactionDetail FTD ON FTD.TransactionId = FT.Id
        INNER JOIN GroupMember FM ON FM.PersonId = dbo.ufnUtility_GetPersonIdFromPersonAlias(FT.AuthorizedPersonAliasId)
        INNER JOIN [Group] F ON F.Id = FM.GroupId
            AND F.GroupTypeId = 10
        WHERE FTD.Amount > 0
			AND FTD.AccountId IN (498,609,690,708,727,745)
        )
    -- Insert Total Count into temporary table	
    INSERT INTO @FamilyContributionCount
    SELECT GroupId
        ,COUNT(*)
        ,SUM(last_year)
        ,SUM(recent)
        ,MAX(WeekendDate)
    FROM CTE
    GROUP BY GroupId

    --Update Existing Records
    UPDATE ERA
    SET TimesGaveLastYear = FCC.year_count
        ,TimesGaveLast6Weeks = FCC.recent_count
        ,TimesGaveTotal = FCC.total_count
        ,LastGave = FCC.last_gave
    FROM @FamilyContributionCount FCC
    INNER JOIN _church_ccv_Datamart_ERA ERA ON ERA.FamilyId = FCC.FamilyId
        AND ERA.WeekendDate = @SaturdayDate

    -- Create New Records
    INSERT INTO _church_ccv_Datamart_ERA (
        WeekendDate
        ,FamilyId
        ,TimesGaveLastYear
        ,TimesGaveLast6Weeks
        ,TimesGaveTotal
        ,LastGave
        )
    SELECT @SaturdayDate
        ,FCC.FamilyId
        ,FCC.year_count
        ,FCC.recent_count
        ,FCC.total_count
        ,FCC.last_gave
    FROM @FamilyContributionCount FCC
    LEFT OUTER JOIN _church_ccv_Datamart_ERA ERA ON ERA.FamilyId = FCC.FamilyId
        AND ERA.WeekendDate = @SaturdayDate
    WHERE ERA.FamilyId IS NULL

    -- Calculate Attendance
    UPDATE CW
    SET CW.FirstAttended = dbo._church_ccv_ufnFamilyFirstAttended(CW.FamilyId)
        ,CW.LastAttended = dbo._church_ccv_ufnFamilyLastAttended(CW.FamilyId, CW.WeekendDate)
    FROM _church_ccv_Datamart_ERA CW
    WHERE CW.WeekendDate = @SaturdayDate

    UPDATE CW
    SET CW.RegularAttendee = CASE 
            WHEN ISNULL(LW.RegularAttendee, 0) = 0
                THEN
                    -- In ?
                    CASE 
                        WHEN (
                                ISNULL(CW.TimesGaveLastYear, 0) >= 3
                                AND ISNULL(CW.TimesGaveLast6Weeks, 0) >= 1
                                )
                            OR ISNULL(CW.TimesAttendedLast16Weeks, 0) >= 8
                            THEN 1
                        ELSE 0
                        END
            ELSE
                -- Out ?
                CASE 
                    WHEN DATEADD(week, 8, ISNULL(CW.LastGave, '1/1/1900')) < CW.WeekendDate
                        AND DATEADD(week, 4, ISNULL(CW.LastAttended, '1/1/1900')) < CW.WeekendDate
                        AND ISNULL(CW.TimesAttendedLast16Weeks, 0) < 8
                        THEN 0
                    ELSE 1
                    END
            END
    FROM _church_ccv_Datamart_ERA CW
    LEFT OUTER JOIN _church_ccv_Datamart_ERA LW ON CW.FamilyId = LW.FamilyId
        AND CW.WeekendDate = DATEADD(week, 1, LW.WeekendDate)
    WHERE CW.WeekendDate = @SaturdayDate

    -- Calculate Giving/Checking component of regular attendees
    UPDATE _church_ccv_Datamart_ERA
    SET RegularAttendeeG = CASE 
            WHEN DATEADD(week, 8, LastGave) >= WeekendDate
                THEN 1
            ELSE 0
            END
        ,RegularAttendeeC = CASE 
            WHEN DATEADD(week, 4, LastAttended) >= WeekendDate
                OR TimesAttendedLast16Weeks >= 8
                THEN 1
            ELSE 0
            END
    WHERE WeekendDate = @SaturdayDate
        AND RegularAttendee = 1
END