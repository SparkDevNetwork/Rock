WITH Attend AS (
	SELECT
        FM.GroupId,
		MAX(A.StartDateTime) AS [LastAttend]
    FROM Attendance A
	INNER JOIN PersonAlias PA
		ON PA.Id = A.PersonAliasId
    INNER JOIN GroupMember FM 
		ON FM.PersonId = PA.PersonId
    INNER JOIN [Group] F 
		ON F.Id = FM.GroupId
        AND F.GroupTypeId = 10
    WHERE A.DidAttend = 1
        AND A.GroupId IN (
            1199220,1199221,1199222,1199223,1199224,1199225
            ,1199226,1199227,1199228,1199231,1199232,1199233
            ,1199600,1199601,1199602,1199229,1199230,1199596
			,1199597,1199598,1199599
            )
    GROUP BY FM.GroupId
	HAVING MAX(A.StartDateTime) <= DATEADD(MONTH, -8, GETDATE())
),

Giving AS (
	SELECT 
		F.Id AS [GroupId],
		MAX(FT.TransactionDateTime) AS [LastContribution]
	FROM FinancialTransaction FT
	INNER JOIN FinancialTransactionDetail FTD 
		ON FTD.TransactionId = FT.Id
	INNER JOIN PersonAlias PA 
		ON PA.Id = FT.AuthorizedPersonAliasId
	INNER JOIN GroupMember FM 
		ON FM.PersonId = PA.PersonId
	INNER JOIN [Group] F 
		ON F.Id = FM.GroupId
		AND F.GroupTypeId = 10
	WHERE FTD.Amount > 0
		AND FTD.AccountId IN (SELECT * FROM dbo._church_ccv_ufnUtility_GetGeneralFinanceAccountIds())
	GROUP BY F.Id
	HAVING MAX(FT.TransactionDateTime) <= DATEADD(MONTH, -8, GETDATE())
),

GroupsSection AS (
	SELECT DISTINCT
		F.Id AS [GroupId],
		MAX(GM.Id) [GroupMemberId]
	FROM [Group] F
	INNER JOIN GroupMember FM
		ON FM.GroupId = F.Id
	INNER JOIN GroupMember GM
		ON GM.PersonId = FM.PersonId
	INNER JOIN [Group] G
		ON G.Id = GM.GroupId
	INNER JOIN [GroupType] GT
		ON GT.Id = G.GroupTypeId
	WHERE G.GroupTypeId NOT IN (12,10,49,23,73)	
		AND F.GroupTypeId = 10
	GROUP BY F.Id 
	HAVING MAX(GM.CreatedDateTime) <= DATEADD(MONTH, -8, GETDATE())
),

Groups AS (
	SELECT 
		GS.GroupId,
		G.Name + ' (' + GT.Name + ')' AS [LastGroup],
		GM.CreatedDateTime AS [LastGroupDate]
	FROM GroupsSection GS
	INNER JOIN GroupMember GM
		ON GM.Id = GS.GroupMemberId
	INNER JOIN [Group] G
		ON G.Id = GM.GroupId
	INNER JOIN GroupType GT
		ON GT.Id = G.GroupTypeId
),

HistorySection AS (
	SELECT DISTINCT
		F.Id AS [GroupId],
		MAX(H.Id) [HistoryId]
	FROM [Group] F
	INNER JOIN GroupMember FM
		ON FM.GroupId = F.Id
	INNER JOIN History H
		ON H.EntityId = FM.PersonId
		WHERE H.EntityTypeId = 15
	GROUP BY F.Id 
	HAVING MAX(H.CreatedDateTime) <= DATEADD(MONTH, -8, GETDATE())
),

Histories AS (
	SELECT 
		HS.GroupId,
		H.Caption + ' - ' + H.Summary AS [LastHistory],
		H.CreatedDateTime AS [LastHistoryDate]
	FROM HistorySection HS
	INNER JOIN History H
		ON H.Id = HS.HistoryId
)

SELECT
	P.Id,
	P.LastName + ', ' + P.NickName AS [Name],
	C.Name AS [Campus],
	DV.Value AS [Connection Status],
	GG.LastGroup AS [Last Group (By Family)],
	CAST(GG.LastGroupDate AS DATE) AS [Last Group Date],
	GH.LastHistory AS [Last History (By Family)],
	CAST(GH.LastHistoryDate AS DATE) AS [Last History Date],
	CAST(AV.Value AS DATE) AS [ERA Lost Date (By Family)],
	P.ModifiedDateTime AS [Modified Date Time]
FROM Person P
INNER JOIN GroupMember GM
	ON GM.PersonId = P.Id
INNER JOIN [Group] G
	ON G.Id = GM.GroupId
	AND G.GroupTypeId = 10
INNER JOIN Campus C
	ON C.Id = G.CampusId
LEFT OUTER JOIN Attend AA
	ON AA.GroupId = G.Id
LEFT OUTER JOIN Giving GC
	ON GC.GroupId = G.Id
LEFT OUTER JOIN Groups GG
	ON GG.GroupId = G.Id
LEFT OUTER JOIN Histories GH
	ON GH.GroupId = G.Id
LEFT OUTER JOIN AttributeValue AV
	ON AV.EntityId = P.Id
INNER JOIN Attribute A 
	ON A.Id = AV.AttributeId
	AND A.[Key] = 'eRALost'
INNER JOIN DefinedValue DV
	ON DV.Id = P.ConnectionStatusValueId
WHERE P.RecordStatusValueId = 3
	AND P.ConnectionStatusValueId IN (65,66,146)
	AND (AA.GroupId IS NOT NULL OR
		 GC.GroupId IS NOT NULL OR
		 GG.GroupId IS NOT NULL OR
		 GH.GroupId IS NOT NULL)
ORDER BY P.ModifiedDateTime, P.LastName, P.NickName