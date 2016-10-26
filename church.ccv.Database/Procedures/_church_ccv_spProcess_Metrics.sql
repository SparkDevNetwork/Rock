CREATE PROC [dbo].[_church_ccv_spProcess_Metrics]
AS
/*-----------------------------------------------------------------
	Variables
-----------------------------------------------------------------*/
DECLARE @MinHistoryDate DATETIME = '1/1/2000'
DECLARE @MetricId INTEGER
DECLARE @MetricPartitionId INTEGER

/*-----------------------------------------------------------------
	Attendance 
-----------------------------------------------------------------*/
/*------------------------------------
	Children's (Weekly)
------------------------------------*/
SET @MetricId = 34

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId] -- Temporarily put EntityId into the ForeignId field so that we can populate MetricValuePartition after this
    )
SELECT 0
    ,COUNT(*)
    ,@MetricId
    ,dbo.ufnUtility_GetSundayDate(A.StartDateTime)
    ,GETDATE()
    ,NEWID()
    ,A.CampusId
FROM Attendance A
INNER JOIN [Group] G ON G.Id = A.GroupId
INNER JOIN [GroupType] GT ON GT.Id = G.GroupTypeId
    AND GT.Id IN (
        19
        ,20
        ,35
        ,36
        ,46
        )
WHERE A.DidAttend = 1
    AND dbo.ufnUtility_GetSundayDate(A.StartDateTime) <= GETDATE()
GROUP BY dbo.ufnUtility_GetSundayDate(A.StartDateTime)
    ,A.CampusId
ORDER BY dbo.ufnUtility_GetSundayDate(A.StartDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	Total Attendance (Weekly)
------------------------------------*/
SET @MetricId = 28

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    CreatedDateTime
    ,ModifiedDateTime
    ,MetricValueType
    ,YValue
    ,MetricValueDateTime
    ,MetricId
    ,Guid
    )
SELECT GETDATE()
    ,GETDATE()
    ,0
    ,SUM(MV.YValue)
    ,dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime)
    ,@MetricId
    ,NEWID()
FROM MetricValue MV
INNER JOIN Metric M ON M.Id = MV.MetricId
WHERE M.Id IN (
        1
        ,2
        ,3
        ,34
        )
GROUP BY dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(MV.MetricValueDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*-----------------------------------------------------------------
	Giving
-----------------------------------------------------------------*/
/*------------------------------------
	Giving Kiosks (Weekly)
------------------------------------*/
SET @MetricId = 14

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    )
SELECT 0
    ,SUM(TD.Amount)
    ,@MetricId
    ,dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
    ,GETDATE()
    ,NEWID()
FROM FinancialBatch B
INNER JOIN FinancialTransaction T ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD ON TD.TransactionId = T.Id
INNER JOIN PersonAlias PA ON PA.AliasPersonId = T.AuthorizedPersonAliasId
INNER JOIN Person P ON P.Id = PA.PersonId
WHERE B.NAME LIKE 'Kiosk Giving%'
    AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
    AND TD.Amount <> 0
    AND B.BatchEndDateTime > @MinHistoryDate
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	Giving Kiosks - Participants (Weekly)
------------------------------------*/
SET @MetricId = 15

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    )
SELECT 0
    ,ISNULL(COUNT(DISTINCT P.Id), 0)
    ,@MetricId
    ,dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
    ,GETDATE()
    ,NEWID()
FROM FinancialBatch B
INNER JOIN FinancialTransaction T ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD ON TD.TransactionId = T.Id
INNER JOIN PersonAlias PA ON PA.AliasPersonId = T.AuthorizedPersonAliasId
INNER JOIN Person P ON P.Id = PA.PersonId
WHERE B.NAME LIKE 'Kiosk Giving%'
    AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
    AND TD.Amount <> 0
    AND B.BatchEndDateTime > @MinHistoryDate
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	Online Giving - One Time (Weekly)
------------------------------------*/
SET @MetricId = 13

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    )
SELECT 0
    ,SUM(TD.Amount)
    ,@MetricId
    ,dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
    ,GETDATE()
    ,NEWID()
FROM FinancialBatch B
INNER JOIN FinancialTransaction T ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD ON TD.TransactionId = T.Id
INNER JOIN FinancialAccount FA ON FA.Id = TD.AccountId
--INNER JOIN [Arena].dbo.ctrb_contribution C
--	ON C.foreign_key = T.Id
--INNER JOIN [Arena].dbo.pmnt_transaction PT
--	ON PT.transaction_id = C.transaction_id 
--	AND PT.repeating_payment_id IS NULL
WHERE B.NAME LIKE 'Online Giving%'
    AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
    AND TD.Amount <> 0
    AND T.ScheduledTransactionId IS NULL
    AND B.BatchEndDateTime >= @MinHistoryDate
    AND FA.ID IN (
        498
        ,609
        ,690
        ,708
        ,727
        )
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	Online Giving - Recurring (Weekly)
------------------------------------*/
SET @MetricId = 12

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    )
SELECT 0
    ,SUM(TD.Amount)
    ,@MetricId
    ,dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
    ,GETDATE()
    ,NEWID()
FROM FinancialBatch B
INNER JOIN FinancialTransaction T ON T.BatchId = B.Id
INNER JOIN FinancialTransactionDetail TD ON TD.TransactionId = T.Id
INNER JOIN FinancialAccount FA ON FA.Id = TD.AccountId
--INNER JOIN [Arena].dbo.ctrb_contribution C
--	ON C.foreign_key = T.Id
--INNER JOIN [Arena].dbo.pmnt_transaction PT
--	ON PT.transaction_id = C.transaction_id 
--	AND PT.repeating_payment_id IS NOT NULL
WHERE B.NAME LIKE 'Online Giving%'
    AND dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime) < getdate()
    AND TD.Amount <> 0
    AND T.ScheduledTransactionId IS NOT NULL
    AND B.BatchEndDateTime >= '1/1/2000'
    AND FA.ID IN (
        498
        ,609
        ,690
        ,708
        ,727
        )
GROUP BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)
ORDER BY dbo.ufnUtility_GetSundayDate(B.BatchEndDateTime)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*-----------------------------------------------------------------
	Ministries
-----------------------------------------------------------------*/
/*------------------------------------
	Baptisms (Monthly)
------------------------------------*/
SET @MetricId = 16

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
SELECT 0
    ,COUNT(*)
    ,@MetricId
    ,CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)
    ,GETDATE()
    ,NEWID()
    ,C.Id
FROM Person P
INNER JOIN AttributeValue AV ON AV.AttributeId = 174
    AND AV.EntityId = P.Id
INNER JOIN AttributeValue AV1 ON AV1.AttributeId = 714
    AND AV1.EntityId = P.Id
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId
    AND F.GroupTypeId = 10
INNER JOIN [Campus] C ON C.Id = F.CampusId
WHERE AV.Value IS NOT NULL
    AND P.ConnectionStatusValueId IN (
        65
        ,146
        )
    AND AV.ValueAsDateTime >= @MinHistoryDate
    AND CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR) <= GETDATE()
    AND AV1.Value = 'True'
GROUP BY CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)
    ,C.Id
ORDER BY CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	First Time Visitors (Weekly)
------------------------------------*/

SET @MetricId = 22

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricValueId in (select Id from MetricValue where MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE())))

DELETE
FROM MetricValue
WHERE MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
    SELECT 0 AS [MetricValueType]
        ,COUNT(*) AS [YValue]
        ,@MetricId AS [MetricId]
        ,dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime]
        ,GETDATE() AS [CreatedDateTime]
        ,NEWID() AS [Guid]
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId 
            END AS [EntityId]
    FROM Person P
    INNER JOIN AttributeValue AV ON AV.AttributeId = 717
        AND AV.EntityId = P.Id
    INNER JOIN GroupMember FM ON FM.PersonId = P.Id
    INNER JOIN [Group] F ON F.Id = FM.GroupId
        AND F.GroupTypeId = 10
    WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))
    GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime)
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId
            END

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId
AND MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

/*------------------------------------
	First Time Visitors > Adults (Weekly)
------------------------------------*/
SET @MetricId = 68

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricValueId in (select Id from MetricValue where MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE())))

DELETE
FROM MetricValue
WHERE MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
    SELECT 0 AS [MetricValueType]
        ,COUNT(*) AS [YValue]
        ,@MetricId AS [MetricId]
        ,dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime]
        ,GETDATE() AS [CreatedDateTime]
        ,NEWID() AS [Guid]
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId
            END AS [EntityId]
    FROM Person P
    INNER JOIN AttributeValue AV ON AV.AttributeId = 717
        AND AV.EntityId = P.Id
    INNER JOIN GroupMember FM ON FM.PersonId = P.Id
    INNER JOIN [Group] F ON F.Id = FM.GroupId
        AND F.GroupTypeId = 10
    WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))
        AND FM.GroupRoleId = 3
    GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime)
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId
            END
    
INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId
AND MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

/*------------------------------------
	First Time Visitors > Children (Weekly)
------------------------------------*/
SET @MetricId = 69

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricValueId in (select Id from MetricValue where MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE())))

DELETE
FROM MetricValue
WHERE MetricId = @MetricId and MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
    SELECT 0 AS [MetricValueType]
        ,COUNT(*) AS [YValue]
        ,@MetricId AS [MetricId]
        ,dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) AS [MetricValueDateTime]
        ,GETDATE() AS [CreatedDateTime]
        ,NEWID() AS [Guid]
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId
            END AS [EntityId]
    FROM Person P
    INNER JOIN AttributeValue AV ON AV.AttributeId = 717
        AND AV.EntityId = P.Id
    INNER JOIN GroupMember FM ON FM.PersonId = P.Id
    INNER JOIN [Group] F ON F.Id = FM.GroupId
        AND F.GroupTypeId = 10
    WHERE dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime) = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))
        AND FM.GroupRoleId = 4
    GROUP BY dbo.ufnUtility_GetSundayDate(AV.ValueAsDateTime)
        ,CASE 
            WHEN F.CampusId IS NULL
                THEN 1
            ELSE F.CampusId
            END
    
INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId
AND MetricValueDateTime = dbo.ufnUtility_GetSundayDate(DATEADD(WEEK, - 1, GETDATE()))

/*------------------------------------
	Transfers (Monthly)
------------------------------------*/
SET @MetricId = 17

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
SELECT 0
    ,COUNT(*)
    ,@MetricId
    ,CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)
    ,GETDATE()
    ,NEWID()
    ,C.Id
FROM Person P
INNER JOIN AttributeValue AV ON AV.AttributeId = 1031
    AND AV.EntityId = P.Id
INNER JOIN AttributeValue AV1 ON AV1.AttributeId = 1032
    AND AV1.EntityId = P.Id
INNER JOIN DefinedValue DV ON CAST(DV.[Guid] AS VARCHAR(100)) = AV1.Value
    AND DV.Id = 8840
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId
    AND F.GroupTypeId = 10
INNER JOIN [Campus] C ON C.Id = F.CampusId
WHERE AV.Value IS NOT NULL
    AND AV.ValueAsDateTime >= @MinHistoryDate
    AND CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR) <= GETDATE()
GROUP BY CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)
    ,C.Id
ORDER BY CAST(MONTH(AV.ValueAsDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(AV.ValueAsDateTime) AS VARCHAR)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId

/*------------------------------------
	Starting Point (Monthly)
------------------------------------*/
SET @MetricId = 23

SELECT TOP 1 @MetricPartitionId = Id
FROM MetricPartition
WHERE MetricId = @MetricId

DELETE
FROM MetricValuePartition
WHERE MetricPartitionId IN (
        SELECT Id
        FROM MetricPartition
        WHERE MetricId IN (@MetricId)
        )

DELETE
FROM MetricValue
WHERE MetricId IN (@MetricId)

INSERT INTO MetricValue (
    [MetricValueType]
    ,[YValue]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[CreatedDateTime]
    ,[Guid]
    ,[ForeignId]
    )
SELECT 0
    ,COUNT(*)
    ,23
    ,CAST(MONTH(A.StartDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(A.StartDateTime) AS VARCHAR)
    ,GETDATE()
    ,NEWID()
    ,CASE 
        WHEN F.CampusId IS NULL
            THEN 1
        ELSE F.CampusId
        END
FROM Attendance A
INNER JOIN [Group] G ON G.Id = A.GroupId
INNER JOIN Person P ON P.Id = dbo.ufnUtility_GetPersonIdFromPersonAlias(A.PersonAliasId)
INNER JOIN GroupMember FM ON FM.PersonId = P.Id
INNER JOIN [Group] F ON F.Id = FM.GroupId
    AND F.GroupTypeId = 10
INNER JOIN [GroupType] GT ON GT.Id = G.GroupTypeId
    AND GT.Id IN (57)
WHERE A.DidAttend = 1
    AND A.StartDateTime >= @MinHistoryDate
    AND CAST(MONTH(A.StartDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(A.StartDateTime) AS VARCHAR) <= GETDATE()
GROUP BY CAST(MONTH(A.StartDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(A.StartDateTime) AS VARCHAR)
    ,F.CampusId
ORDER BY CAST(MONTH(A.StartDateTime) AS VARCHAR) + '/1/' + CAST(YEAR(A.StartDateTime) AS VARCHAR)

INSERT INTO [dbo].[MetricValuePartition] (
    [MetricPartitionId]
    ,[MetricValueId]
    ,[EntityId]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[Guid]
    )
SELECT @MetricPartitionId
    ,ID
    ,ForeignId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,NEWID()
FROM MetricValue
WHERE MetricId = @MetricId