Declare @Now DateTime = GetDate()
DECLARE @ThisMonthStart DATETIME= DATEADD(mm, DATEDIFF(mm, 0, @Now ), 0)
DECLARE @ThisMonthEnd DATETIME = DATEADD(DAY, -1,DATEADD(mm, 1, @ThisMonthStart));

DECLARE @ThisYearStart DATETIME= DATEADD(yy, DATEDIFF(yy, 0, @Now), 0)
DECLARE @ThisYearEnd DATETIME= DATEADD(yy, DATEDIFF(yy, 0, @Now) + 1, -1);

Declare @PreAdoptionAttributeId int = 24986
Declare @PreAdoptionDateAttributeId int = 24939
Declare @PreAdoptionAmountAttributeId int = 24940

Declare @PostAdoptionAssistanceAttributeId int = 35310
Declare @FosterCareAssistanceAttributeId int = 37883
Declare @CommunityAssistanceAttributeId int = 37884
Declare @FinancialAssistanceDateAttributeId int = 34778
Declare @FinancialAssistanceAmountAttributeId int = 34781

-----------------------------------------------------------------------------
-- Dates (Table 1)
-----------------------------------------------------------------------------
Select @ThisMonthStart,@ThisMonthEnd,@ThisYearStart,@ThisYearEnd

-----------------------------------------------------------------------------
-- Funding Received ( Table 2)
-----------------------------------------------------------------------------
Declare @FundingRecievedValueTable table(
	Value float,
	DateApproved datetime
)

Insert into @FundingRecievedValueTable
Select ftd.Amount,
	ft.TransactionDateTime
From FinancialAccount fa
Join FinancialTransactionDetail ftd on ftd.AccountId = fa.Id
Join FinancialTransaction ft on ftd.TransactionId = ft.Id
Where fa.Id in (805,1539,1548)
--and ft.TransactionDateTime >= @ThisYearStart


Select
	'Funding Recieved' as 'Title',
	'Funding Received from the Congregation' as 'Subtitle',
	IsNull(MTD,0) as 'LastValue',
	IsNull(YTD,0) as 'CumulativeValue',
	ISNULL(Total,0) as 'TotalValue'
From (
	Select Sum( Value) as 'MTD' 
	From @FundingRecievedValueTable
	Where DateApproved >= @ThisMonthStart and DateApproved <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	Select Sum( Value) as 'YTD' 
	From @FundingRecievedValueTable
	Where DateApproved >= @ThisYearStart and DateApproved <= @ThisYearEnd
	) as ytdTable
cross join 
    (
    Select Sum( Value) as 'Total' 
	From @FundingRecievedValueTable
	) as totalTable

-----------------------------------------------------------------------------
-- Pre Adoption Funding Given ( Table 3)
-----------------------------------------------------------------------------

SELECT 
    'Pre Adoption Funding Given' as 'Title',
	'Funding Given to Legacy 685 Families' as 'Subtitle',
	IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisMonthStart AND givingDate.ValueAsDateTime <= @ThisMonthEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisMonth],
		IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisYearStart AND givingDate.ValueAsDateTime <= @ThisYearEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisYear],
		IsNull(SUM(amount.ValueAsNumeric),0) AS [TotalValue]
From AttributeValue av
Join Attribute a on av.AttributeId = a.Id
Join AttributeMatrix am on convert(nvarchar(max),am.Guid) = av.Value
Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
Join AttributeValue givingDate on givingDate.EntityId = ami.Id and givingDate.AttributeId = @PreAdoptionDateAttributeId
Join AttributeValue amount on amount.EntityId = ami.Id and amount.AttributeId = @PreAdoptionAmountAttributeId
Where a.Id = @PreAdoptionAttributeId

-----------------------------------------------------------------------------
-- Post Adoption Assistance Given ( Table 4)
-----------------------------------------------------------------------------
SELECT 
    'Post Adoption Funding Given' as 'Title',
	'Funding Given to Legacy 685 Families' as 'Subtitle',
	IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisMonthStart AND givingDate.ValueAsDateTime <= @ThisMonthEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisMonth],
		IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisYearStart AND givingDate.ValueAsDateTime <= @ThisYearEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisYear],
		IsNull(SUM(amount.ValueAsNumeric),0) AS [TotalValue]
From AttributeValue av
Join Attribute a on av.AttributeId = a.Id
Join AttributeMatrix am on convert(nvarchar(max),am.Guid) = av.Value
Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
Join AttributeValue givingDate on givingDate.EntityId = ami.Id and givingDate.AttributeId = @FinancialAssistanceDateAttributeId
Join AttributeValue amount on amount.EntityId = ami.Id and amount.AttributeId = @FinancialAssistanceAmountAttributeId
Where a.Id = @PostAdoptionAssistanceAttributeId

-----------------------------------------------------------------------------
-- Foster Care Assistance Given ( Table 5)
-----------------------------------------------------------------------------
SELECT 
    'Foster Care Funding Given' as 'Title',
	'Funding Given to Legacy 685 Families' as 'Subtitle',
	IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisMonthStart AND givingDate.ValueAsDateTime <= @ThisMonthEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisMonth],
		IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisYearStart AND givingDate.ValueAsDateTime <= @ThisYearEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisYear],
		IsNull(SUM(amount.ValueAsNumeric),0) AS [TotalValue]
From AttributeValue av
Join Attribute a on av.AttributeId = a.Id
Join AttributeMatrix am on convert(nvarchar(max),am.Guid) = av.Value
Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
Join AttributeValue givingDate on givingDate.EntityId = ami.Id and givingDate.AttributeId = @FinancialAssistanceDateAttributeId
Join AttributeValue amount on amount.EntityId = ami.Id and amount.AttributeId = @FinancialAssistanceAmountAttributeId
Where a.Id = @FosterCareAssistanceAttributeId

-----------------------------------------------------------------------------
-- Community Assistance Given ( Table 6)
-----------------------------------------------------------------------------
SELECT 
    'Community Funding Given' as 'Title',
	'Funding Given to Legacy 685 Families' as 'Subtitle',
	IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisMonthStart AND givingDate.ValueAsDateTime <= @ThisMonthEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisMonth],
		IsNull(SUM( CASE WHEN givingDate.ValueAsDateTime >= @ThisYearStart AND givingDate.ValueAsDateTime <= @ThisYearEnd THEN amount.ValueAsNumeric ELSE 0.0 END ),0) AS [ThisYear],
		IsNull(SUM(amount.ValueAsNumeric),0) AS [TotalValue]
From AttributeValue av
Join Attribute a on av.AttributeId = a.Id
Join AttributeMatrix am on convert(nvarchar(max),am.Guid) = av.Value
Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
Join AttributeValue givingDate on givingDate.EntityId = ami.Id and givingDate.AttributeId = @FinancialAssistanceDateAttributeId
Join AttributeValue amount on amount.EntityId = ami.Id and amount.AttributeId = @FinancialAssistanceAmountAttributeId
Where a.Id = @CommunityAssistanceAttributeId

-----------------------------------------------------------------------------
-- Total Donors ( Table 7)
-----------------------------------------------------------------------------
Select
	'Total # Donors' as 'Title',
	'Total Count of Donors Given to Legacy 685' as 'Subtitle',
	IsNull(MTD,0) as 'LastValue',
	IsNull(YTD,0) as 'CumulativeValue',
	IsNull(Total,0) as 'TotalValue'
From (
	SELECT DISTINCT
		COUNT(FT.[AuthorizedPersonAliasId]) AS [MTD]
	FROM [FinancialTransaction] FT
	INNER JOIN [FinancialTransactionDetail] FTD ON FT.[Id] = FTD.[TransactionId]
	WHERE FTD.[AccountId] = 805
	AND FT.[TransactionDateTime] >= @ThisMonthStart AND FT.[TransactionDateTime] <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	SELECT DISTINCT
		COUNT(FT.[AuthorizedPersonAliasId]) AS [YTD]
	FROM [FinancialTransaction] FT
	INNER JOIN [FinancialTransactionDetail] FTD ON FT.[Id] = FTD.[TransactionId]
	WHERE FTD.[AccountId] = 805
	AND FT.[TransactionDateTime] >= @ThisYearStart AND FT.[TransactionDateTime] <= @ThisYearEnd
	) as ytdTable
cross join
	(
	SELECT DISTINCT
		COUNT(FT.[AuthorizedPersonAliasId]) AS [Total]
	FROM [FinancialTransaction] FT
	INNER JOIN [FinancialTransactionDetail] FTD ON FT.[Id] = FTD.[TransactionId]
	WHERE FTD.[AccountId] = 805
	) as totalTable

-----------------------------------------------------------------------------
-- Total Seeking Assistance (Table 8)
-----------------------------------------------------------------------------
Select
	'Total Seeking Assistance' as 'Title',
	'Total Count of Families Seeking Assistance' as 'Subtitle',
	IsNull(MTD,0) as 'LastValue',
	IsNull(YTD,0) as 'CumulativeValue',
	IsNull(Total,0) as 'TotalValue'
From (
	SELECT
		COUNT(DISTINCT AV.EntityId) AS MTD
	FROM [AttributeValue] AV
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = AM.[Guid]
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV1 ON AMI.[Id] = AV1.[EntityId] AND AV1.[AttributeId] = 29117 -- Assistance Type
	LEFT OUTER JOIN [AttributeValue] AV2 ON AMI.[Id] = AV2.[EntityId] AND AV2.[AttributeId] = 24938 -- Document
	LEFT OUTER JOIN [AttributeValue] AV3 ON AMI.[Id] = AV3.[EntityId] AND AV3.[AttributeId] = 24939 -- Date Funding Approved
	LEFT OUTER JOIN [AttributeValue] AV4 ON AMI.[Id] = AV4.[EntityId] AND AV4.[AttributeId] = 24940 -- Amount
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV5.[EntityId] AND AV6.[AttributeId] = 25439 -- Country
	LEFT OUTER JOIN [AttributeValue] AV7 ON AMI.[Id] = AV7.[EntityId] AND AV7.[AttributeId] = 29118 -- Loan Repay Date

	WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV3.[Value], '') = ''
	AND (
		ISNULL(AV1.[Value], '') != '' OR 
		ISNULL(AV2.[Value], '') != '' OR
		ISNULL(AV4.[Value], '') != '' OR
		ISNULL(AV5.[Value], '') != '' OR
		ISNULL(AV6.[Value], '') != '' OR
		ISNULL(AV7.[Value], '') != ''
	)
	AND AMI.[CreatedDateTime] >= @ThisMonthStart AND AMI.[CreatedDateTime] <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	SELECT
		COUNT(DISTINCT AV.EntityId) AS YTD
	FROM [AttributeValue] AV
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = AM.[Guid]
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV1 ON AMI.[Id] = AV1.[EntityId] AND AV1.[AttributeId] = 29117 -- Assistance Type
	LEFT OUTER JOIN [AttributeValue] AV2 ON AMI.[Id] = AV2.[EntityId] AND AV2.[AttributeId] = 24938 -- Document
	LEFT OUTER JOIN [AttributeValue] AV3 ON AMI.[Id] = AV3.[EntityId] AND AV3.[AttributeId] = 24939 -- Date Funding Approved
	LEFT OUTER JOIN [AttributeValue] AV4 ON AMI.[Id] = AV4.[EntityId] AND AV4.[AttributeId] = 24940 -- Amount
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV5.[EntityId] AND AV6.[AttributeId] = 25439 -- Country
	LEFT OUTER JOIN [AttributeValue] AV7 ON AMI.[Id] = AV7.[EntityId] AND AV7.[AttributeId] = 29118 -- Loan Repay Date

	WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV3.[Value], '') = ''
	AND (
		ISNULL(AV1.[Value], '') != '' OR 
		ISNULL(AV2.[Value], '') != '' OR
		ISNULL(AV4.[Value], '') != '' OR
		ISNULL(AV5.[Value], '') != '' OR
		ISNULL(AV6.[Value], '') != '' OR
		ISNULL(AV7.[Value], '') != ''
	)
	AND AMI.[CreatedDateTime] >= @ThisYearStart AND AMI.[CreatedDateTime] <= @ThisYearEnd
	) as ytdTable
cross join
	(
	SELECT
		COUNT(DISTINCT AV.EntityId) AS Total
	FROM [AttributeValue] AV
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = AM.[Guid]
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV1 ON AMI.[Id] = AV1.[EntityId] AND AV1.[AttributeId] = 29117 -- Assistance Type
	LEFT OUTER JOIN [AttributeValue] AV2 ON AMI.[Id] = AV2.[EntityId] AND AV2.[AttributeId] = 24938 -- Document
	LEFT OUTER JOIN [AttributeValue] AV3 ON AMI.[Id] = AV3.[EntityId] AND AV3.[AttributeId] = 24939 -- Date Funding Approved
	LEFT OUTER JOIN [AttributeValue] AV4 ON AMI.[Id] = AV4.[EntityId] AND AV4.[AttributeId] = 24940 -- Amount
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV5.[EntityId] AND AV6.[AttributeId] = 25439 -- Country
	LEFT OUTER JOIN [AttributeValue] AV7 ON AMI.[Id] = AV7.[EntityId] AND AV7.[AttributeId] = 29118 -- Loan Repay Date

WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV3.[Value], '') = ''
	AND (
		ISNULL(AV1.[Value], '') != '' OR 
		ISNULL(AV2.[Value], '') != '' OR
		ISNULL(AV4.[Value], '') != '' OR
		ISNULL(AV5.[Value], '') != '' OR
		ISNULL(AV6.[Value], '') != '' OR
		ISNULL(AV7.[Value], '') != ''
	)
	) as totalTable

-----------------------------------------------------------------------------
-- Total Families (Table 9)
-----------------------------------------------------------------------------
Declare @FamilyTable table(
	FamilyId int,
	FundedDateTime datetime
	)
Insert into @FamilyTable
Select G.Id, AV1.ValueAsDateTime
FROM [AttributeValue] AV
INNER JOIN [Group] G ON AV.[EntityId] = G.[Id]
INNER JOIN [AttributeMatrix] AM ON AV.[Value] = AM.[Guid]
INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
Inner Join [AttributeValue] AV1 ON AV1.EntityId = AMI.[Id] and AV1.AttributeId = 24939
WHERE AV.[AttributeId] = 24986
AND ISNULL(AV.[Value], '') != ''

SELECT 
    'Total Families' AS [Title],
    'Total Families helped through Legacy 685' AS [Subtitle],
	(Select Count(Distinct FamilyId) From @FamilyTable Where FundedDateTime >= @ThisMonthStart AND FundedDateTime <= @ThisMonthEnd) as [ThisMonth],
	(Select Count(Distinct FamilyId) From @FamilyTable Where FundedDateTime >= @ThisYearStart AND FundedDateTime <= @ThisYearEnd) as [ThisYear],
	(Select Count(Distinct FamilyId) From @FamilyTable) as [Total]

-----------------------------------------------------------------------------
-- Total Children (Table 10)
-----------------------------------------------------------------------------

Declare @ChildrenTable table(
	FamilyId int,
	NumberOfChildren int,
	FundedDateTime datetime
	)
Insert into @ChildrenTable
Select G.Id,AV2.ValueAsNumeric, AV1.ValueAsDateTime
FROM [AttributeValue] AV
INNER JOIN [Group] G ON AV.[EntityId] = G.[Id]
INNER JOIN [AttributeMatrix] AM ON AV.[Value] = AM.[Guid]
INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
Inner Join [AttributeValue] AV1 ON AV1.EntityId = AMI.[Id] and AV1.AttributeId = 24939
Inner Join [AttributeValue] AV2 ON AV2.EntityId = AMI.[Id] and AV2.AttributeId = 24941
WHERE AV.[AttributeId] = 24986
AND ISNULL(AV.[Value], '') != ''

SELECT 
    'Total Children' AS [Title],
    'Total Children helped through Legacy 685' AS [Subtitle],
	(
		Select IsNull(Sum(IsNull(NumberOfChildren,0)),0)
		From (
			Select Distinct FamilyId, NumberOfChildren
			From @ChildrenTable 
			Where FundedDateTime >= @ThisMonthStart 
			AND FundedDateTime <= @ThisMonthEnd
		) t1
	) as [ThisMonth],
	(
		Select IsNull(Sum(IsNull(NumberOfChildren,0)),0)
		From (
			Select Distinct FamilyId, NumberOfChildren
			From @ChildrenTable 
			Where FundedDateTime >= @ThisYearStart 
			AND FundedDateTime <= @ThisYearEnd
		) t1
	) as [ThisYear],
	(
		Select IsNull(Sum(IsNull(NumberOfChildren,0)),0)
		From (
			Select Distinct FamilyId, NumberOfChildren
			From @ChildrenTable
		) t1
	) as [Total]

-----------------------------------------------------------------------------
-- Total Countries (Table 11)
-----------------------------------------------------------------------------

SELECT      
	'Total Countries' AS [Title],     
	'Total Countries helped through Legacy 685' AS [Subtitle],
	[Month].[Count] AS [ThisMonth],
	[Year].[Count] AS [ThisYear],
	Total.[Count] AS Total
FROM 
(
	SELECT count (DISTINCT DV6.[Description]) AS [Count]
	FROM [AttributeValue] AV
	INNER JOIN [Group] G ON AV.[EntityId] = G.[Id]
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = CAST(AM.[Guid] as varchar(40))
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV6.[EntityId] AND AV6.[AttributeId] = 25439 -- Countries
	LEFT OUTER JOIN [DefinedValue] DV6 ON AV6.[Value] = CAST(DV6.[Guid] as varchar(40))
	LEFT OUTER JOIN [AttributeValue] AV7 ON AV.[EntityId] = AV7.[EntityId] AND AV7.[AttributeId] = 24782 -- Connection Status
	WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV5.[Value], '') != '' 
	AND ISNULL(AV6.[Value], '') != '' 
	AND AV6.[CreatedDateTime] >= @ThisMonthStart AND AV6.[CreatedDateTime] <= @ThisMonthEnd
) AS [Month]
CROSS APPLY (
	SELECT count (DISTINCT DV6.[Description]) AS [Count]
	FROM [AttributeValue] AV
	INNER JOIN [Group] G ON AV.[EntityId] = G.[Id]
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = CAST(AM.[Guid] as varchar(40))
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV6.[EntityId] AND AV6.[AttributeId] = 25439 -- Countries
	LEFT OUTER JOIN [DefinedValue] DV6 ON AV6.[Value] = CAST(DV6.[Guid] as varchar(40))
	LEFT OUTER JOIN [AttributeValue] AV7 ON AV.[EntityId] = AV7.[EntityId] AND AV7.[AttributeId] = 24782 -- Connection Status
	WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV5.[Value], '') != '' 
	AND ISNULL(AV6.[Value], '') != '' 
	AND AV6.[CreatedDateTime] >= @ThisYearStart AND AV6.[CreatedDateTime] <= @ThisYearEnd
) AS [Year]
CROSS APPLY (
	SELECT count (DISTINCT DV6.[Description]) AS [Count]
	FROM [AttributeValue] AV
	INNER JOIN [Group] G ON AV.[EntityId] = G.[Id]
	INNER JOIN [AttributeMatrix] AM ON AV.[Value] = CAST(AM.[Guid] as varchar(40))
	INNER JOIN [AttributeMatrixItem] AMI ON AM.[Id] = AMI.[AttributeMatrixId]
	LEFT OUTER JOIN [AttributeValue] AV5 ON AMI.[Id] = AV5.[EntityId] AND AV5.[AttributeId] = 24941 -- Children Count
	LEFT OUTER JOIN [AttributeValue] AV6 ON AMI.[Id] = AV6.[EntityId] AND AV6.[AttributeId] = 25439 -- Countries
	LEFT OUTER JOIN [DefinedValue] DV6 ON AV6.[Value] = CAST(DV6.[Guid] as varchar(40))
	LEFT OUTER JOIN [AttributeValue] AV7 ON AV.[EntityId] = AV7.[EntityId] AND AV7.[AttributeId] = 24782 -- Connection Status
	WHERE AV.[AttributeId] = 24986
	AND ISNULL(AV.[Value], '') != ''
	AND ISNULL(AV5.[Value], '') != '' 
	AND ISNULL(AV6.[Value], '') != '' 
) AS [Total]

-----------------------------------------------------------------------------
-- Connect Attendance (Table 12)
-----------------------------------------------------------------------------
Select
	'Connect Attendance' as 'Title',
	'Attendance in Connect Groups' as 'Subtitle',
	IsNull(MTD,0) as 'LastValue',
	IsNull(YTD,0) as 'CumulativeValue'
From 
(
    SELECT 
        SUM( [Count]) AS [MTD]
    FROM 
    (
    	SELECT	
    		COUNT(AO.[Id]) AS [Count]
    		--G.[Name],
    		--AO.[SundayDate],
    		--(SELECT CONCAT(FirstName, ' ', LastName) FROM [Person] WHERE [Id] = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId) ) AS Person
    	FROM [AttendanceOccurrence] AO
    	INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    	INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    	WHERE GroupId IN (
    		SELECT [Id]
    		FROM [Group]
    		WHERE [GroupTypeId] = 424
    	)
    	AND A.[DidAttend] = 1
    	AND AO.[SundayDate] >= @ThisMonthStart and AO.[SundayDate] <= @ThisMonthEnd
    	UNION ALL
    	SELECT	
    		COUNT(AO.[Id]) AS [Count]
    		--G.[Name],
    		--AO.[SundayDate],
    		--(SELECT CONCAT(FirstName, ' ', LastName) FROM [Person] WHERE [Id] = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId) ) AS Person
    	FROM [AttendanceOccurrence] AO
    	INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    	INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    	WHERE GroupId = 207950
    	AND A.[DidAttend] = 1
    	AND AO.[SundayDate] >= @ThisMonthStart and AO.[SundayDate] <= @ThisMonthEnd
    ) val1
) as mtdTable
cross join
	(
    SELECT 
        SUM( [Count]) AS [YTD]
    FROM 
    (
    	SELECT	
    		COUNT(AO.[Id]) AS [Count]
    		--G.[Name],
    		--AO.[SundayDate],
    		--(SELECT CONCAT(FirstName, ' ', LastName) FROM [Person] WHERE [Id] = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId) ) AS Person
    	FROM [AttendanceOccurrence] AO
    	INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    	INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    	WHERE GroupId IN (
    		SELECT [Id]
    		FROM [Group]
    		WHERE [GroupTypeId] = 424
    	)
    	AND A.[DidAttend] = 1
    	AND AO.[SundayDate] >= @ThisYearStart and AO.[SundayDate] <= @ThisYearEnd
    	UNION ALL
    	SELECT	
    		COUNT(AO.[Id]) AS [Count]
    		--G.[Name],
    		--AO.[SundayDate],
    		--(SELECT CONCAT(FirstName, ' ', LastName) FROM [Person] WHERE [Id] = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId) ) AS Person
    	FROM [AttendanceOccurrence] AO
    	INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    	INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    	WHERE GroupId = 207950
    	AND A.[DidAttend] = 1
    	AND AO.[SundayDate] >= @ThisYearStart and AO.[SundayDate] <= @ThisYearEnd
    ) val1
) as ytdTable
-----------------------------------------------------------------------------
-- Equip Attendance (Table 13)
-----------------------------------------------------------------------------
Select
	'Equip Attendance' as 'Title',
	'Attendance in Equip Groups' as 'Subtitle',
	MTD as 'LastValue',
	YTD as 'CumulativeValue'
From (
	SELECT	
    	COUNT(AO.[Id]) AS MTD
    FROM [AttendanceOccurrence] AO
    INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    WHERE GroupId IN (
    	SELECT [Id]
    	FROM [Group]
    	WHERE [GroupTypeId] = 425
    )
    AND A.[DidAttend] = 1
	And AO.[SundayDate] >= @ThisMonthStart and AO.[SundayDate] <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	SELECT	
    	COUNT(AO.[Id]) AS YTD
    FROM [AttendanceOccurrence] AO
    INNER JOIN [Attendance] A ON AO.[Id] = A.[OccurrenceId]
    INNER JOIN [Group] G ON AO.[GroupId] = G.[Id]
    WHERE GroupId IN (
    	SELECT [Id]
    	FROM [Group]
    	WHERE [GroupTypeId] = 425
    )
    AND A.[DidAttend] = 1
	And AO.[SundayDate] >= @ThisYearStart and AO.[SundayDate] <= @ThisYearEnd
	) as ytdTable
-----------------------------------------------------------------------------
-- Touches (Table 14)
-----------------------------------------------------------------------------
Declare @GroupAttributeKey nvarchar(max) = 'AdoptionAssistance'
Declare @ColumnName nvarchar(max) = '# of Children'
Declare @ApprovedColumnName nvarchar(50) = 'Date Approved'

Declare @TouchesTable table(
	GroupId nvarchar(max),
	MetricDateTime datetime
)

----- ADD FAMILIES WHO ATTENDED A CONNECT GROUP
Insert Into @TouchesTable
Select g.Id, StartDateTime
	From Attendance A
	INNER JOIN AttendanceOccurrence AO ON A.[OccurrenceId] = AO.[Id]
	Join [Group] g on AO.GroupId = g.Id
	Where g.GroupTypeId = 424
	And StartDateTime >= @ThisYearStart

----- ADD FAMILIES WHO ATTENDED AN EQUIP GROUP
Insert Into @TouchesTable
Select g.Id, StartDateTime
	From Attendance A
	INNER JOIN AttendanceOccurrence AO ON A.[OccurrenceId] = AO.[Id]
	Join [Group] g on AO.GroupId = g.Id
	Where g.GroupTypeId = 425
	And StartDateTime >= @ThisYearStart

----- ADD FAMILIES WHO HAD PRE ADOPTION ASSISTANCE
	Insert into @TouchesTable
	Select distinct groupAttributeValue.EntityId,
	approvedColumn.ValueAsDateTime as 'DateApproved'
	-- Get the matrixes
	From AttributeValue groupAttributeValue
	Join Attribute groupAttribute on groupAttributeValue.AttributeId = groupAttribute.Id and groupAttribute.[Key] = @GroupAttributeKey
	Join AttributeMatrix am on groupAttributeValue.Value = Convert(nvarchar(max),am.[Guid])

	-- Get the Columns
	Join AttributeMatrixTemplate amt on am.AttributeMatrixTemplateId = amt.Id
	Join Attribute ValueColumn 
		on ValueColumn.EntityTypeQualifierColumn = 'AttributeMatrixTemplateId' 
		and ValueColumn.EntityTypeQualifierValue = convert(nvarchar(max),amt.Id)
		and ValueColumn.[Name] = @ColumnName
	Join Attribute DateApprovedColumn 
		on DateApprovedColumn.EntityTypeQualifierColumn = 'AttributeMatrixTemplateId' 
		and DateApprovedColumn.EntityTypeQualifierValue = convert(nvarchar(max),amt.Id)
		and DateApprovedColumn.[Name] = @ApprovedColumnName

	-- Get the Row Values
	Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
	Join AttributeValue itemColumn on itemColumn.AttributeId = ValueColumn.Id and itemColumn.EntityId = ami.Id
	Join AttributeValue approvedColumn on approvedColumn.AttributeId = DateApprovedColumn.Id and approvedColumn.EntityId = ami.Id
	Where approvedColumn.ValueAsDateTime >= @ThisYearStart

----- ADD FAMILIES WHO HAD FINANCIAL ASSISTANCE
	Insert into @TouchesTable
	Select distinct groupAttributeValue.EntityId,
	approvedColumn.ValueAsDateTime as 'DateApproved'
	-- Get the matrixes
	From AttributeValue groupAttributeValue
	Join Attribute groupAttribute on groupAttributeValue.AttributeId = groupAttribute.Id and groupAttribute.[Key] in ('Legacy685Documents','FosterCareAssistance','CommunityAssistance')
	Join AttributeMatrix am on groupAttributeValue.Value = Convert(nvarchar(max),am.[Guid])
	-- Get the Columns
	Join AttributeMatrixTemplate amt on am.AttributeMatrixTemplateId = amt.Id
	Join Attribute DateApprovedColumn 
		on DateApprovedColumn.EntityTypeQualifierColumn = 'AttributeMatrixTemplateId' 
		and DateApprovedColumn.EntityTypeQualifierValue = convert(nvarchar(max),amt.Id)
		and DateApprovedColumn.[Name] = @ApprovedColumnName

	-- Get the Row Values
	Join AttributeMatrixItem ami on ami.AttributeMatrixId = am.Id
	Join AttributeValue approvedColumn on approvedColumn.AttributeId = DateApprovedColumn.Id and approvedColumn.EntityId = ami.Id
	Where approvedColumn.ValueAsDateTime >= @ThisYearStart

----- ADD FAMILIES WHO WERE TOUCHED BY A PATHWAY
Insert into @TouchesTable
Select g.Id, n.CreatedDateTime
	From Note n
	Join Person p on n.EntityId = p.Id
	Join GroupMember gm on p.Id = gm.PersonId
	Join [Group] g on gm.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Where ( NoteTypeId = 27 or NoteTypeId = 28)
	And gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
	And n.CreatedDateTime >= @ThisYearStart


----- ADD FAMILIES WHO WERE TOUCHED BY AN INTERACTION
Insert into @TouchesTable
Select g.Id, n.CreatedDateTime
	From Note n
	Join Person p on n.EntityId = p.Id
	Join GroupMember gm on p.Id = gm.PersonId
	Join [Group] g on gm.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Join GroupMember legacyGm on p.Id = legacyGm.PersonId
	Join [Group] legacyG on legacyGm.GroupId = legacyG.Id and legacyG.Id = 486871
	Where ( NoteTypeId = 24 or NoteTypeId = 25)
	And gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
	And n.CreatedDateTime >= @ThisYearStart

Select
	'Touches' as 'Title',
	'Total Families touched by Legacy 685' as 'Subtitle',
	MTD as 'LastValue',
	YTD as 'CumulativeValue'
From (
	Select Count(distinct GroupId) as 'MTD' 
	From @TouchesTable
	Where MetricDateTime >= @ThisMonthStart and MetricDateTime <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	Select Count(distinct GroupId) as 'YTD' 
	From @TouchesTable
	Where MetricDateTime >= @ThisYearStart and MetricDateTime <= @ThisYearEnd
	) as ytdTable
-----------------------------------------------------------------------------
-- Pathways (Table 15)
-----------------------------------------------------------------------------

SELECT
    'Pathways' as 'Title',
	'Total pathway' as 'Subtitle',
	COUNT(CASE WHEN cr.[CreatedDateTime] BETWEEN @ThisMonthStart AND @ThisMonthEnd THEN cr.[Id] END ) as 'LastValue',
	COUNT( cr.[Id] ) as 'CumulativeValue'
FROM [ConnectionRequest] cr
JOIN [ConnectionOpportunity] co ON cr.[ConnectionOpportunityId] = co.[Id]
JOIN [PersonAlias] paP ON cr.PersonAliasId = paP.[Id]
JOIN [Person] pP ON paP.[PersonId] = pP.[Id]
LEFT JOIN [PersonAlias] paC ON cr.[ConnectorPersonAliasId] = paC.[Id]
LEFT JOIN [Person] pC ON paC.[PersonId] = pC.[Id]
LEFT JOIN [PersonAlias] paCr ON cr.[CreatedByPersonAliasId] = paCr.[Id]
LEFT JOIN [Person] pCr ON paCr.[PersonId] = pCr.[Id]
WHERE co.[ConnectionTypeId] IN (6, 8, 11)

-----------------------------------------------------------------------------
-- Interactions (Table 16)
-----------------------------------------------------------------------------
Select
	'Interactions' as 'Title',
	'Total Families touched by an interaction' as 'Subtitle',
	MTD as 'LastValue',
	YTD as 'CumulativeValue'
From (
	Select count(distinct g.Id) as 'MTD' 
	From Note n
	Join Person p on n.EntityId = p.Id
	Join GroupMember gm on p.Id = gm.PersonId
	Join [Group] g on gm.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Join GroupMember legacyGm on p.Id = legacyGm.PersonId
	--Join [Group] legacyG on legacyGm.GroupId = legacyG.Id and legacyG.Id = 486871
	Where (NoteTypeId = 30)
	And gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
	And n.CreatedDateTime >= @ThisMonthStart and n.CreatedDateTime <= @ThisMonthEnd
	) as mtdTable
cross join
	(
	Select count(distinct g.Id) as 'YTD' 
	From Note n
	Join Person p on n.EntityId = p.Id
	Join GroupMember gm on p.Id = gm.PersonId
	Join [Group] g on gm.GroupId = g.Id
	Join GroupType gt on g.GroupTypeId = gt.Id
	Join GroupMember legacyGm on p.Id = legacyGm.PersonId
	--Join [Group] legacyG on legacyGm.GroupId = legacyG.Id and legacyG.Id = 486871
	Where (NoteTypeId = 30)
	And gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
	And n.CreatedDateTime >= @ThisYearStart and n.CreatedDateTime <= @ThisYearEnd
	) as ytdTable