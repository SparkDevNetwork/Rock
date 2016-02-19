/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spDatamart_Person
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spDatamart_Person]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[_church_ccv_spDatamart_Person]
AS
BEGIN
    SET NOCOUNT ON;

    TRUNCATE TABLE _church_ccv_Datamart_Person;

    DECLARE @entityTypeIdPerson INT = (
            SELECT TOP 1 Id
            FROM EntityType
            WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'
            )
        ,@entityTypeIdNote INT = (
            SELECT TOP 1 Id
            FROM EntityType
            WHERE [Guid] = '53DC1E78-14A5-44DE-903F-6A2CB02164E7'
            );

   WITH Giving
    AS (
        SELECT *
        FROM (
            SELECT dbo.ufnUtility_GetPersonIdFromPersonAlias(FT.AuthorizedPersonAliasId) AS [PersonId]
                ,YEAR(FT.TransactionDateTime) AS [year]
                ,SUM(FTD.Amount) AS [total]
            FROM FinancialTransactionDetail FTD
            INNER JOIN FinancialTransaction FT ON FT.Id = FTD.TransactionId
            INNER JOIN FinancialAccount FA ON FA.Id = FTD.AccountId
            WHERE FA.Id IN (
                    745
                    ,498
                    ,609
                    ,690
                    ,708
                    ,727
                    )
                AND YEAR(FT.TransactionDateTime) >= 2007
            GROUP BY dbo.ufnUtility_GetPersonIdFromPersonAlias(FT.AuthorizedPersonAliasId)
                ,YEAR(FT.TransactionDateTime)
            ) AS s
        PIVOT(SUM([total]) FOR [year] IN (
                    [2015]
                    ,[2014]
                    ,[2013]
                    ,[2012]
                    ,[2011]
                    ,[2010]
                    ,[2009]
                    ,[2008]
                    ,[2007]
                    )) AS s
        ),
		-- todo: Find a better way to handle people who are in multiple families.
		FAMILY AS
		(
			SELECT
				P1.Id AS [PersonId],
				(SELECT max(F.Id)
				 FROM GroupMember FM
				 INNER JOIN [Group] F ON F.Id = FM.GroupId
					AND F.GroupTypeId = 10
				 WHERE P1.Id = FM.PersonId) AS [FamilyId],
				 (SELECT max(F.CampusId)
				 FROM GroupMember FM
				 INNER JOIN [Group] F ON F.Id = FM.GroupId
					AND F.GroupTypeId = 10
				 WHERE P1.Id = FM.PersonId) AS [CampusId],
				 (SELECT max(FM.GroupRoleId)
				 FROM GroupMember FM
				 INNER JOIN [Group] F ON F.Id = FM.GroupId
					AND F.GroupTypeId = 10
				 WHERE P1.Id = FM.PersonId) AS [GroupRoleId]
			FROM Person P1
			WHERE P1.RecordStatusValueId = 3
		)
    INSERT INTO _church_ccv_Datamart_Person (
        [PersonId]
        ,[FamilyId]
        ,[PersonGuid]
        ,[FirstName]
        ,[NickName]
        ,[MiddleName]
        ,[LastName]
        ,[FullName]
        ,[Age]
        ,[Grade]
        ,[BirthDate]
        ,[Gender]
        ,[MaritalStatus]
        ,[FamilyRole]
        ,[Campus]
        ,[CampusId]
        ,[ConnectionStatus]
		,[ConnectionStatusValueId]
        ,[AnniversaryDate]
        ,[AnniversaryYears]
        ,[NeighborhoodId]
        ,[NeighborhoodName]
        ,[TakenStartingPoint]
        ,[StartingPointDate]
        ,[InNeighborhoodGroup]
        ,[NeighborhoodGroupId]
        ,[NeighborhoodGroupName]
        ,[NearestGroupId]
        ,[NearestGroupName]
        ,[FirstVisitDate]
        ,[IsStaff]
        ,[PhotoUrl]
        ,[IsServing]
        ,[IsEra]
        ,[ServingAreas]
        ,[CreatedDateTime]
        ,[ModifiedDateTime]
        ,[CreatedByPersonAliasId]
        ,[ModifiedByPersonAliasId]
        ,[SpouseName]
        ,[IsHeadOfHousehold]
        ,[Address]
        ,[City]
        ,[State]
        ,[PostalCode]
        ,[Email]
        ,[HomePhone]
        ,[CellPhone]
        ,[WorkPhone]
        ,[IsBaptized]
        ,[BaptismDate]
        ,[LastContributionDate]
        ,[Giving2015]
        ,[Giving2014]
        ,[Giving2013]
        ,[Giving2012]
        ,[Giving2011]
        ,[Giving2010]
        ,[Giving2009]
        ,[Giving2008]
        ,[Giving2007]
        ,[GeoPoint]
        ,[Latitude]
        ,[Longitude]
        ,[Guid]
        ,[ForeignId]
        ,[ViewedCount]
        ,[LastAttendedDate]
        ,[LastPublicNote]
        )
    SELECT P.[Id] AS [PersonId]
        ,F.[FamilyId] AS [FamilyId]
        ,P.[Guid] AS [PersonGuid]
        ,P.[FirstName]
        ,P.[NickName]
        ,P.[MiddleName]
        ,P.[LastName]
        ,P.[LastName] + ', ' + P.[NickName] AS [FullName]
        ,dbo._church_ccv_ufnGetAge(P.BirthDate) AS [Age]
        ,dbo._church_ccv_ufnGetGrade(P.GraduationYear) AS [Grade]
        ,P.[BirthDate]
        ,CASE 
            WHEN P.[Gender] = 1
                THEN 'Male'
            ELSE 'Female'
            END AS [Gender]
        ,CAST(MS.[Value] AS VARCHAR(15)) AS [MaritalStatus]
        ,FR.NAME [FamilyRole]
        ,C.NAME [CampusName]
        ,F.CampusId [CampusId]
        ,CS.Value [ConnectionStatus]
		,CS.Id [ConnectionStatusValueId]
        ,P.AnniversaryDate
        ,YEAR(GETDATE()) - YEAR(P.AnniversaryDate) AS [AnniversaryYears]
		,gfg.Id [NeighborhoodId]
		,gfg.Name [NeighborhoodName]
        ,CASE 
            WHEN (
                    SELECT max(A.Id)
					FROM Attendance A
					INNER JOIN [GroupMember] SPM ON A.GroupId = SPM.GroupId
					INNER JOIN [Group] SP ON SP.Id = SPM.GroupId
						AND SP.GroupTypeId = 57
					WHERE SPM.PersonId = P.Id
						AND A.DidAttend = 1
						AND A.StartDateTime <= GETDATE()
                    ) IS NOT NULL
                THEN 1
            ELSE 0
            END AS [TakenStartingPoint]
        ,(
            SELECT MAX(A.StartDateTime)
            FROM Attendance A
            INNER JOIN [GroupMember] SPM ON A.GroupId = SPM.GroupId
            INNER JOIN [Group] SP ON SP.Id = SPM.GroupId
                AND SP.GroupTypeId = 57
            WHERE SPM.PersonId = P.Id
				AND A.DidAttend = 1
				AND A.StartDateTime <= GETDATE()
            ) AS [StartingPointDate]
        ,CASE 
            WHEN (
                    SELECT max(NG.Id)
                    FROM GroupMember NGM
                    LEFT OUTER JOIN [Group] NG ON NG.Id = NGM.GroupId
                        AND NG.GroupTypeId = 49
                    WHERE NGM.PersonId = P.Id
                        AND NG.IsActive = 1
                    ) IS NOT NULL
                THEN 1
            ELSE 0
            END AS [InNeighborhoodGroup]
        ,ng.Id  AS [NeighborhoodGroupId]
        ,ng.Name AS [NeighborhoodGroupName]
        ,nrg.ID as [NearestGroupId]
        ,nrg.Name AS [NearestGroupName]
        ,FVV.ValueAsDateTime AS [FirstVisitDate]
        ,CASE 
            WHEN (
                    SELECT max(OCM.PersonId)
                    FROM GroupMember OCM
                    INNER JOIN [Group] OC ON OC.Id = OCM.GroupId
                        AND OC.GroupTypeId = 52
                    WHERE OCM.PersonId = P.Id
						AND OCM.GroupMemberStatus = 1
                    ) IS NOT NULL
                THEN 1
            ELSE 0
            END AS [IsStaff]
        ,'http://rock.ccvonline.com/GetImage.ashx?id=' + CAST(P.PhotoId AS VARCHAR(10)) AS [PhotoUrl]
        ,CASE 
            WHEN (
                    SELECT max(STM.PersonId)
                    FROM GroupMember STM
                    INNER JOIN [Group] ST ON ST.Id = STM.GroupId
                        AND ST.GroupTypeId = 23
                    WHERE STM.PersonId = P.Id
						AND STM.GroupMemberStatus = 1
                    ) IS NOT NULL
                THEN 1
            ELSE 0
            END AS [IsServing]
        ,CASE 
			WHEN ERAV.Value IS NULL THEN 'False'
			ELSE ERAV.Value 
		 END AS [IsEra]
        ,LEFT(STUFF((
                    SELECT ', ' + G.NAME
                    FROM [Group] G
                    INNER JOIN [GroupMember] GM ON GM.GroupId = G.Id
                    WHERE G.GroupTypeId = 23
                        AND GM.PersonId = P.Id
                        AND G.IsActive = 1
						AND GM.GroupMemberStatus = 1
                    FOR XML PATH('')
                    ), 1, 1, ''), 1000) AS [ServingAreas]
        ,P.[CreatedDateTime]
        ,P.[ModifiedDateTime]
        ,P.[CreatedByPersonAliasId]
        ,P.[ModifiedByPersonAliasId]
        ,(
            SELECT max(S.FirstName + ' ' + S.LastName)
            FROM Person S
            INNER JOIN GroupMember GM ON GM.PersonId = S.Id
            INNER JOIN [Group] G ON G.Id = GM.GroupId
            WHERE G.Id = F.FamilyId
                AND S.Gender != P.Gender
                AND F.GroupRoleId = 3
                AND GM.GroupRoleId = 3
            ) AS [SpouseName]
        ,CASE 
            WHEN (
                    SELECT TOP 1 S.Id
                    FROM Person S
                    INNER JOIN GroupMember GM ON GM.PersonId = S.Id
                    INNER JOIN [Group] G ON G.Id = GM.GroupId
                    WHERE G.Id = F.FamilyId
                        AND F.GroupRoleId = 3
                        AND GM.GroupRoleId = 3
                    ORDER BY GM.GroupRoleId
                        ,S.Gender
                    ) = P.Id
                THEN 1
            ELSE 0
            END AS [IsHeadOfHousehold]
        ,L.[Street1] + ' ' + L.[Street2] AS [Address]
        ,L.[City]
        ,L.[State]
        ,L.[PostalCode]
        ,CASE 
            WHEN P.IsEmailActive = 1
                THEN P.Email
            WHEN P.IsEmailActive = 0
                THEN ''
            END AS [Email]
        ,HP.NumberFormatted AS [HomePhone]
        ,CP.NumberFormatted AS [CellPhone]
        ,WP.NumberFormatted AS [WorkPhone]
        ,CASE 
            WHEN BD.ValueAsDateTime IS NOT NULL
                THEN 1
            ELSE 0
            END AS [IsBaptized]
        ,BD.ValueAsDateTime AS [BaptismDate]
        ,(
            SELECT MAX(FT.TransactionDateTime)
            FROM FinancialTransactionDetail FTD
            INNER JOIN FinancialTransaction FT ON FT.Id = FTD.TransactionId
            INNER JOIN FinancialAccount FA ON FA.Id = FTD.AccountId
            INNER JOIN PersonAlias PA ON PA.Id = FT.AuthorizedPersonAliasId
            WHERE FA.Id IN (
                    498
                    ,609
                    ,690
                    ,708
                    ,727
                    )
                AND PA.PersonId = P.Id
            ) AS [LastContributionDate]
        ,G.[2015] AS [Giving2015]
        ,G.[2014] AS [Giving2014]
        ,G.[2013] AS [Giving2013]
        ,G.[2012] AS [Giving2012]
        ,G.[2011] AS [Giving2011]
        ,G.[2010] AS [Giving2010]
        ,G.[2009] AS [Giving2009]
        ,G.[2008] AS [Giving2008]
        ,G.[2007] AS [Giving2007]
        ,L.GeoPoint
        ,L.GeoPoint.Lat AS [Latitude]
        ,L.GeoPoint.Long AS [Longitude]
        ,NEWID() AS [Guid]
        ,NULL AS [ForeignId]
        ,P.[ViewedCount]
        ,(
            SELECT MAX(at.StartDateTime)
            FROM Attendance at
            JOIN PersonAlias pa ON pa.Id = at.PersonAliasId
            WHERE pa.PersonId = p.Id
                AND isnull(at.DidAttend, 0) = 1
            ) AS [LastAttendedDate]
        ,(
            SELECT max([n].[Text])
            FROM [Note] [n]
            WHERE [n].NoteTypeId IN (
                    SELECT Id
                    FROM NoteType
                    WHERE EntityTypeId = @entityTypeIdPerson
                    )
                AND [n].Id NOT IN (
                    SELECT EntityId
                    FROM Auth
                    WHERE EntityTypeId = @entityTypeIdNote
                        AND AllowOrDeny = 'D'
                        AND [SpecialRole] = 1
               )
                AND isnull([n].[Text], '') != ''
                AND [n].EntityId = p.Id
            ) AS [LastPublicNote]
    FROM Person P
    LEFT OUTER JOIN DefinedValue MS ON MS.Id = P.MaritalStatusValueId
    INNER JOIN DefinedValue CS ON CS.Id = P.ConnectionStatusValueId
    LEFT OUTER JOIN AttributeValue BD ON BD.EntityId = P.Id
        AND BD.AttributeId = 174
    LEFT OUTER JOIN PhoneNumber HP ON HP.PersonId = P.Id
        AND HP.NumberTypeValueId = 13
    LEFT OUTER JOIN PhoneNumber CP ON CP.PersonId = P.Id
        AND CP.NumberTypeValueId = 12
    LEFT OUTER JOIN PhoneNumber WP ON WP.PersonId = P.Id
        AND WP.NumberTypeValueId = 136
    INNER JOIN FAMILY F ON F.PersonId = P.Id
    INNER JOIN [Campus] C ON C.Id = F.CampusId
    INNER JOIN GroupTypeRole FR ON FR.Id = F.GroupRoleId
        AND FR.GroupTypeId = 10
    LEFT OUTER JOIN [GroupLocation] GL ON GL.GroupId = F.FamilyId
        AND GL.GroupLocationTypeValueId = 19
    LEFT OUTER JOIN [Location] L ON L.Id = GL.LocationId
	OUTER APPLY (SELECT TOP 1 gg.Id, gg.Name FROM dbo.ufnGroup_GeofencingGroups(L.Id, 48) gg where gg.CampusId = f.CampusId) gfg
	OUTER APPLY ( SELECT TOP 1 G.ID, G.Name
            FROM dbo._church_ccv_Datamart_NearestGroup NG
            INNER JOIN GroupLocation GL ON GL.LocationId = NG.GroupLocationId
            INNER JOIN [Group] G ON G.Id = GL.GroupId
                AND G.GroupTypeId = 49
            WHERE NG.FamilyLocationId = L.Id
            ORDER BY NG.Distance
            ) nrg
    OUTER APPLY (
            SELECT TOP 1 NG.Id, NG.Name
            FROM GroupMember NGM
            LEFT OUTER JOIN [Group] NG ON NG.Id = NGM.GroupId
                AND NG.GroupTypeId = 49
            WHERE NGM.PersonId = P.Id
                AND NG.IsActive = 1
				AND NGM.GroupMemberStatus = 1
            ) ng
    LEFT OUTER JOIN AttributeValue ERAV ON ERAV.EntityId = P.Id
        AND ERAV.AttributeId = 2533
    LEFT OUTER JOIN AttributeValue FVV ON FVV.EntityId = P.Id
        AND FVV.AttributeId = 717
    LEFT OUTER JOIN Giving G ON G.PersonId = P.Id
    WHERE P.RecordTypeValueId = 1
        AND P.RecordStatusValueId = 3
END