/* ====================================================== */
-- Finance Staff Giving Report

-- @StartDate and @EndDate can be set by Rock using query params.
-- If they are not set, reportStartDate and reportEndDate are the
-- most recent completed week starting on a Monday and ending
-- on a Sunday.

-- Filter HTML Content block at end of this file...

/* ====================================================== */

-- For testing in sql server management studio, uncomment here:
--DECLARE @StartDate AS NVARCHAR(MAX) = '2015-01-01';
--DECLARE @EndDate AS NVARCHAR(MAX) = '2015-12-31';

DECLARE @today AS DATE = GETDATE();
DECLARE @reportEndDate AS DATE = CONVERT(DATE, DATEADD(DAY, 1 - DATEPART(WEEKDAY, @today), @today));
DECLARE @reportStartDate AS DATE = DATEADD(DAY, -6, @reportEndDate);

IF ISDATE(@StartDate) = 1 AND ISDATE(@EndDate) = 1
BEGIN
    SELECT @reportEndDate = @EndDate;
    SELECT @reportStartDate = @StartDate;
END

DECLARE @staffGroupId AS INT = (SELECT Id FROM [Group] WHERE Name = 'RSR - Staff Workers');
DECLARE @familyType AS INT = (SELECT Id FROM [GroupType] WHERE Name = 'Family' AND IsSystem = 1);
DECLARE @adultRole AS INT = (SELECT Id FROM GroupTypeRole WHERE GroupTypeId = @familyType AND Name = 'Adult');

DECLARE @fundNames AS NVARCHAR(MAX) = (
	SELECT 
		CONVERT(NVARCHAR(MAX), '[' + pfa.PublicName + '],')
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
		pfa.Id,
		pfa.PublicName
	ORDER BY 
		pfa.PublicName
	FOR XML PATH(''));

SELECT @fundNames = LEFT(@fundNames, LEN(@fundNames) - 1);

DECLARE @sql AS NVARCHAR(MAX) = CONCAT(N'
	SELECT
		*
	FROM (
		SELECT
			MAX(staff.Id) AS PersonId,
			CONCAT( 
				STUFF((
					SELECT 
						'' and '' + FirstName
					FROM 
						Person personB 
						JOIN GroupMember familyMember ON familyMember.PersonId = personB.Id
						JOIN [Group] family ON familyMember.GroupId = family.Id
					WHERE 
						family.GroupTypeId = ', @familyType, N'
						AND familyMember.GroupRoleId = ', @adultRole, N'
						AND personB.GivingGroupId = givers.GivingGroupId 
					ORDER BY
						personB.Gender
					FOR XML PATH('''')), 1, 5, ''''), 
				'' '', 
				givers.LastName) AS StaffFamily,
			givers.LastName,
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
			staffGM.GroupId = ', @staffGroupId, N'
			AND staffGM.IsSystem = 0
			AND staff.IsSystem = 0
			AND ft.TransactionDateTime >= CONVERT(DATE, ''', @reportStartDate, N''')
			AND ft.TransactionDateTime <= CONVERT(DATE, ''', @reportEndDate, N''')
		GROUP BY
			givers.GivingGroupId,
			givers.LastName,
			pfa.Id,
			pfa.PublicName
	) AS DataTable
	PIVOT (SUM(TotalGiven) FOR Account IN (', @fundNames, N')) AS PivotTable
		ORDER BY
			LastName
');

EXEC SP_EXECUTESQL @sql;
