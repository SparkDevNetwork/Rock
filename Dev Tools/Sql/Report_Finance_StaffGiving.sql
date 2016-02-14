/* ====================================================== */
-- Finance Staff Giving Report

-- @StartDate and @EndDate can be set by Rock using query params.
-- If they are not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

DECLARE @staffGroupId AS INT = (SELECT Id FROM [Group] WHERE Name = 'RSR - Staff Workers');

SELECT
	staff.Id AS PersonId,
	staff.LastName,
	staff.FirstName,
	pfa.PublicName AS Account,
	SUM(ftd.Amount) AS TotalGiven
FROM
	GroupMember staffGM
	JOIN Person staff ON staff.Id = staffGM.PersonId
	JOIN Person givers ON givers.GivingGroupId = staff.GivingGroupId
	JOIN PersonAlias giverAliases ON giverAliases.PersonId = givers.Id
	LEFT JOIN FinancialTransaction ft ON ft.AuthorizedPersonAliasId = giverAliases.Id
	LEFT JOIN FinancialTransactionDetail ftd ON ftd.TransactionId = ft.Id
	LEFT JOIN FinancialAccount fa ON fa.Id = ftd.AccountId
	LEFT JOIN FinancialAccount pfa ON fa.ParentAccountId = pfa.Id
WHERE
	staffGM.GroupId = @staffGroupId
	AND staffGM.IsSystem = 0
	AND staff.IsSystem = 0
	AND ft.TransactionDateTime >= @reportStartDate
	AND ft.TransactionDateTime <= @reportEndDate
GROUP BY
	staff.Id,
	pfa.Id,
	pfa.PublicName,
	staff.FirstName,
	staff.LastName
ORDER BY
	staff.LastName,
	staff.FirstName,
	pfa.PublicName;