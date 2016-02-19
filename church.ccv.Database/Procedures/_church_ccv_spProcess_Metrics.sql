CREATE PROC [dbo].[_church_ccv_spProcess_Metrics]

AS

/*-----------------------------------------------------------------
	Variables
-----------------------------------------------------------------*/
DECLARE @MinHistoryDate datetime = '1/1/2000'



/*-----------------------------------------------------------------
	Attendance 
-----------------------------------------------------------------*/

/*------------------------------------
	Children's (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (34)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT 
	0,
	COUNT(*),
	0,
	34,
	dbo.ufnUtility_GetSundayDate(A.StartDateTime),
	GETDATE(),
	NEWID(),
	A.CampusId
FROM Attendance A
INNER JOIN [Group] G ON G.Id = A.GroupId
INNER JOIN [GroupType] GT ON GT.Id = G.GroupTypeId 
	AND GT.Id IN (19,20,35,36,46)
WHERE A.DidAttend = 1
	AND dbo.ufnUtility_GetSundayDate(A.StartDateTime) <= GETDATE()
GROUP BY dbo.ufnUtility_GetSundayDate(A.StartDateTime), A.CampusId
ORDER BY dbo.ufnUtility_GetSundayDate(A.StartDateTime)


/*------------------------------------
	Total Attendance (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId = 28

INSERT INTO 
MetricValue (CreatedDateTime,
ModifiedDateTime,
MetricValueType,
[Order],
YValue,
MetricValueDateTime,
MetricId,
Guid)
SELECT
	GETDATE(),
	GETDATE(),
	0,
	0,
	SUM(MV.YValue),
	dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime),
	28,
	NEWID()
FROM MetricValue MV
INNER JOIN Metric M
	ON M.Id = MV.MetricId
WHERE M.Id IN (1, 2, 3, 34)
GROUP BY dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime)



/*-----------------------------------------------------------------
	Giving
-----------------------------------------------------------------*/


/*------------------------------------
	Giving Kiosks (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (14)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	SUM(TD.Amount),
	0,
	14,
	dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime),
	GETDATE(),
	NEWID(),
	NULL
FROM FinancialBatch B
INNER JOIN FinancialTransaction T
	ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD
	ON TD.TransactionId = T.Id
INNER JOIN PersonAlias PA 
	ON PA.AliasPersonId = T.AuthorizedPersonAliasId
INNER JOIN Person P 
	ON P.Id = PA.PersonId
WHERE B.Name LIKE 'Kiosk Giving%'
AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
AND TD.Amount <> 0
AND B.BatchEndDateTime > @MinHistoryDate
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)


/*------------------------------------
	Giving Kiosks - Participants (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (15)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	ISNULL(COUNT(DISTINCT P.Id), 0),
	0,
	15,
	dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime),
	GETDATE(),
	NEWID(),
	NULL	
FROM FinancialBatch B
INNER JOIN FinancialTransaction T
	ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD
	ON TD.TransactionId = T.Id
INNER JOIN PersonAlias PA 
	ON PA.AliasPersonId = T.AuthorizedPersonAliasId
INNER JOIN Person P 
	ON P.Id = PA.PersonId
WHERE B.Name LIKE 'Kiosk Giving%'
AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
AND TD.Amount <> 0
AND B.BatchEndDateTime > @MinHistoryDate
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)


/*------------------------------------
	Online Giving - One Time (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (13)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	SUM(TD.Amount),
	0,
	13,
	dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime),
	GETDATE(),
	NEWID(),
	NULL
FROM FinancialBatch B
INNER JOIN FinancialTransaction T
	ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD
	ON TD.TransactionId = T.Id
INNER JOIN FinancialAccount FA
	ON FA.Id = TD.AccountId
--INNER JOIN [Arena].dbo.ctrb_contribution C
--	ON C.foreign_key = T.Id
--INNER JOIN [Arena].dbo.pmnt_transaction PT
--	ON PT.transaction_id = C.transaction_id 
--	AND PT.repeating_payment_id IS NULL
WHERE B.Name LIKE 'Online Giving%'
AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
AND TD.Amount <> 0
AND T.ScheduledTransactionId IS NULL
AND B.BatchEndDateTime >= @MinHistoryDate
AND FA.ID IN (498,609,690,708,727)
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)


/*------------------------------------
	Online Giving - Recurring (Weekly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (12)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	SUM(TD.Amount),
	0,
	12,
	dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime),
	GETDATE(),
	NEWID(),
	NULL
FROM FinancialBatch B
INNER JOIN FinancialTransaction T
	ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD
	ON TD.TransactionId = T.Id
INNER JOIN FinancialAccount FA
	ON FA.Id = TD.AccountId
--INNER JOIN [Arena].dbo.ctrb_contribution C
--	ON C.foreign_key = T.Id
--INNER JOIN [Arena].dbo.pmnt_transaction PT
--	ON PT.transaction_id = C.transaction_id 
--	AND PT.repeating_payment_id IS NOT NULL
WHERE B.Name LIKE 'Online Giving%'
AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
AND TD.Amount <> 0
AND T.ScheduledTransactionId IS NOT NULL
AND B.BatchEndDateTime >= '1/1/2000'
AND FA.ID IN (498,609,690,708,727)
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)



/*-----------------------------------------------------------------
	Ministries
-----------------------------------------------------------------*/

/*------------------------------------
	Baptisms (Monthly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (16)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	COUNT(*),
	0,
	16,
	CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar),
	GETDATE(),
	NEWID(),
	C.Id
FROM Person P
INNER JOIN AttributeValue AV ON AV.AttributeId = 174 AND AV.EntityId = P.Id
INNER JOIN AttributeValue AV1 ON AV1.AttributeId = 714 AND AV1.EntityId = P.Id
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
INNER JOIN [Campus] C ON C.Id = F.CampusId
WHERE AV.Value IS NOT NULL
	AND P.ConnectionStatusValueId IN (65,146)
	AND AV.ValueAsDateTime >= @MinHistoryDate
	AND CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar) <= GETDATE()
	AND AV1.Value = 'True'
GROUP BY CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar), C.Id
ORDER BY CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar)


/*------------------------------------
	First Time Visitors (Weekly)
------------------------------------*/
--DELETE FROM MetricValue
--WHERE MetricId IN (22)

MERGE MetricValue AS MV
USING ( 
	SELECT
		0 AS [MetricValueType],
		COUNT(*) AS [YValue],
		0 AS [Order],
		22 AS [MetricId],
		dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime],
		GETDATE() AS [CreatedDateTime],
		NEWID() AS [Guid],
		CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END AS [EntityId]
	FROM Person P
	INNER JOIN AttributeValue AV ON AV.AttributeId = 717 AND AV.EntityId = P.Id
	INNER JOIN GroupMember FM ON FM.PersonId = P.Id
	INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
	WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, -1, GETDATE()))
	GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime), CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END
) AS T
ON ( T.EntityId = MV.EntityId AND T.MetricValueDateTime = MV.MetricValueDateTime AND T.MetricId = MV.MetricId )
WHEN NOT MATCHED BY TARGET 
	THEN
		INSERT (MetricValueType, YValue, [Order], MetricId, MetricValueDateTime, CreatedDateTime, [Guid], EntityId)
		VALUES (T.MetricValueType, T.YValue, T.[Order], T.MetricId, T.MetricValueDateTime, T.CreatedDateTime, T.[Guid], T.EntityId) 
WHEN MATCHED 
	THEN UPDATE SET MV.YValue = T.YValue;

--INSERT INTO MetricValue
--(
--	[MetricValueType],
--	[YValue],
--	[Order],
--	[MetricId],
--	[MetricValueDateTime],
--	[CreatedDateTime],
--	[Guid],
--	[EntityId]
--)
--SELECT
--	0,
--	COUNT(*),
--	0,
--	22,
--	dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime),
--	GETDATE(),
--	NEWID(),
--	NULL
--FROM Person P
--INNER JOIN AttributeValue AV ON AV.AttributeId = 717 AND AV.EntityId = P.Id
--WHERE AV.ValueAsDateTime >= @MinHistoryDate
--GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime)
--ORDER BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime)


/*------------------------------------
	First Time Visitors > Adults (Weekly)
------------------------------------*/
MERGE MetricValue AS MV
USING ( 
	SELECT
		0 AS [MetricValueType],
		COUNT(*) AS [YValue],
		0 AS [Order],
		68 AS [MetricId],
		dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime],
		GETDATE() AS [CreatedDateTime],
		NEWID() AS [Guid],
		CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END AS [EntityId]
	FROM Person P
	INNER JOIN AttributeValue AV ON AV.AttributeId = 717 AND AV.EntityId = P.Id
	INNER JOIN GroupMember FM ON FM.PersonId = P.Id
	INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
	WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, -1, GETDATE()))
		AND FM.GroupRoleId = 3
	GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime), CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END
) AS T
ON ( T.EntityId = MV.EntityId AND T.MetricValueDateTime = MV.MetricValueDateTime AND T.MetricId = MV.MetricId )
WHEN NOT MATCHED BY TARGET 
	THEN
		INSERT (MetricValueType, YValue, [Order], MetricId, MetricValueDateTime, CreatedDateTime, [Guid], EntityId)
		VALUES (T.MetricValueType, T.YValue, T.[Order], T.MetricId, T.MetricValueDateTime, T.CreatedDateTime, T.[Guid], T.EntityId) 
WHEN MATCHED 
	THEN UPDATE SET MV.YValue = T.YValue;


/*------------------------------------
	First Time Visitors > Children (Weekly)
------------------------------------*/
MERGE MetricValue AS MV
USING ( 
	SELECT
		0 AS [MetricValueType],
		COUNT(*) AS [YValue],
		0 AS [Order],
		69 AS [MetricId],
		dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime],
		GETDATE() AS [CreatedDateTime],
		NEWID() AS [Guid],
		CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END AS [EntityId]
	FROM Person P
	INNER JOIN AttributeValue AV ON AV.AttributeId = 717 AND AV.EntityId = P.Id
	INNER JOIN GroupMember FM ON FM.PersonId = P.Id
	INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
	WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, -1, GETDATE()))
		AND FM.GroupRoleId = 4
	GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime), CASE WHEN F.CampusId IS NULL THEN 1 ELSE F.CampusId END
) AS T
ON ( T.EntityId = MV.EntityId AND T.MetricValueDateTime = MV.MetricValueDateTime AND T.MetricId = MV.MetricId )
WHEN NOT MATCHED BY TARGET 
	THEN
		INSERT (MetricValueType, YValue, [Order], MetricId, MetricValueDateTime, CreatedDateTime, [Guid], EntityId)
		VALUES (T.MetricValueType, T.YValue, T.[Order], T.MetricId, T.MetricValueDateTime, T.CreatedDateTime, T.[Guid], T.EntityId) 
WHEN MATCHED 
	THEN UPDATE SET MV.YValue = T.YValue;

/*------------------------------------
	Transfers (Monthly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (17)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	COUNT(*),
	0,
	17,
	CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar),
	GETDATE(),
	NEWID(),
	C.Id
FROM Person P
INNER JOIN AttributeValue AV ON AV.AttributeId = 1031 AND AV.EntityId = P.Id
INNER JOIN AttributeValue AV1 ON AV1.AttributeId = 1032 AND AV1.EntityId = P.Id
INNER JOIN DefinedValue DV ON CAST(DV.[Guid] AS VARCHAR(100)) = AV1.Value AND DV.Id = 8840
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
INNER JOIN [Campus] C ON C.Id = F.CampusId
WHERE AV.Value IS NOT NULL
	AND AV.ValueAsDateTime >= @MinHistoryDate
	AND CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar) <= GETDATE()
GROUP BY CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar), C.Id
ORDER BY CAST(MONTH(DATEADD(MONTH, 1, AV.ValueAsDateTime)) AS varchar) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS varchar)


/*------------------------------------
	Starting Point (Monthly)
------------------------------------*/
DELETE FROM MetricValue
WHERE MetricId IN (23)

INSERT INTO MetricValue
(
	[MetricValueType],
	[YValue],
	[Order],
	[MetricId],
	[MetricValueDateTime],
	[CreatedDateTime],
	[Guid],
	[EntityId]
)
SELECT
	0,
	COUNT(*),
	0,
	23,
	CAST(MONTH(DATEADD(MONTH, 1, A.StartDateTime)) AS varchar) + '/1/' + CAST(YEAR(A.StartDateTime) AS varchar),
	GETDATE(),
	NEWID(),
	CASE 
		WHEN F.CampusId IS NULL THEN 1
		ELSE F.CampusId
	END
FROM Attendance A
INNER JOIN [Group] G ON G.Id = A.GroupId
INNER JOIN Person P ON P.Id = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId)
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
INNER JOIN [GroupType] GT ON GT.Id = G.GroupTypeId 
	AND GT.Id IN (57)
WHERE A.DidAttend = 1
	AND A.StartDateTime >= @MinHistoryDate
	AND CAST(MONTH(DATEADD(MONTH, 1, A.StartDateTime)) AS varchar) + '/1/' + CAST(YEAR(A.StartDateTime) AS varchar) <= GETDATE()
GROUP BY CAST(MONTH(DATEADD(MONTH, 1, A.StartDateTime)) AS varchar) + '/1/' + CAST(YEAR(A.StartDateTime) AS varchar), F.CampusId
ORDER BY CAST(MONTH(DATEADD(MONTH, 1, A.StartDateTime)) AS varchar) + '/1/' + CAST(YEAR(A.StartDateTime) AS varchar)