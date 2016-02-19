/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spDatamart_Family
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spDatamart_Family]
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[_church_ccv_spDatamart_Family]
AS
BEGIN
    DECLARE @CurrentSaturdayDate DATE;

    SET NOCOUNT ON;

    TRUNCATE TABLE _church_ccv_Datamart_Family;

    SELECT @CurrentSaturdayDate = dbo._church_ccv_ufnGetSaturdayDate(GETDATE());

    WITH CTE
    AS (
        SELECT F.Id AS [FamilyID]
            ,F.NAME
            ,HH.Id [HHPersonId]
            ,HH.FirstName
            ,HH.NickName
            ,HH.LastName
            ,HH.LastName + ', ' + HH.NickName AS [FullName]
            ,HH.Gender
            ,CS.Value AS [ConnectionStatus]
            ,MS.Value AS [MaritalStatus]
            ,FV.ValueAsDateTime AS [FirstVisitDate]
            ,FA.ValueAsDateTime AS [FirstActivity]
            ,dbo._church_ccv_ufnGetAge(HH.BirthDate) AS [Age]
            ,CASE 
                WHEN HH.IsEmailActive = 1
                    THEN HH.Email
                ELSE NULL
                END AS [Email]
            ,HP.NumberFormatted AS [HomePhone]
        FROM [Group] F
        INNER JOIN Person HH ON HH.Id = dbo._church_ccv_ufnGetHeadOfHousehold(F.Id)
        LEFT OUTER JOIN DefinedValue CS ON CS.Id = HH.ConnectionStatusValueId
        LEFT OUTER JOIN DefinedValue MS ON MS.Id = HH.MaritalStatusValueId
        LEFT OUTER JOIN AttributeValue FV ON FV.EntityId = HH.Id
            AND FV.AttributeId = 717
        LEFT OUTER JOIN AttributeValue FA ON FA.EntityId = HH.Id
            AND FA.AttributeId = 1056
        LEFT OUTER JOIN PhoneNumber HP ON HP.PersonId = HH.Id
            AND HP.NumberTypeValueId = 13
        WHERE F.GroupTypeId = 10
        )
        ,Giving
    AS (
        SELECT *
        FROM (
            SELECT FM.GroupId AS [familyid]
                ,YEAR(FT.TransactionDateTime) AS [year]
                ,SUM(FTD.Amount) AS [total]
            FROM FinancialTransactionDetail FTD
            INNER JOIN FinancialTransaction FT ON FT.Id = FTD.TransactionId
            INNER JOIN FinancialAccount FA ON FA.Id = FTD.AccountId
            INNER JOIN PersonAlias PA ON PA.AliasPersonId = FT.AuthorizedPersonAliasId
            INNER JOIN GroupMember FM ON FM.PersonId = PA.PersonId
            WHERE FA.Id IN (
                    745
                    ,498
                    ,609
                    ,690
                    ,708
                    ,727
                    )
                AND YEAR(FT.TransactionDateTime) >= 2007
            GROUP BY FM.GroupId
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
        )
    INSERT INTO _church_ccv_Datamart_Family (
        [FamilyId]
        ,[FamilyName]
        ,[HHPersonId]
        ,[HHFirstName]
        ,[HHNickName]
        ,[HHLastName]
        ,[HHFullName]
        ,[HHGender]
        ,[HHMemberStatus]
        ,[HHMaritalStatus]
        ,[HHFirstVisit]
        ,[HHFirstActivity]
        ,[HHAge]
        ,[NeighborhoodId]
        ,[NeighborhoodName]
        ,[InNeighborhoodGroup]
        ,[IsEra]
        ,[NearestNeighborhoodGroupName]
        ,[NearestNeighborhoodGroupId]
        ,[IsServing]
        ,[Attendance16Week]
        ,[ConnectionStatus]
        ,[Email]
        ,[HomePhone]
        ,[AdultCount]
        ,[ChildCount]
        ,[LocationId]
        ,[Address]
        ,[City]
        ,[State]
        ,[Country]
        ,[PostalCode]
        ,[GeoPoint]
        ,[Latitude]
        ,[Longitude]
        ,[Campus]
        ,[AdultNames]
        ,[ChildNames]
        ,[Giving2015]
        ,[Giving2014]
        ,[Giving2013]
        ,[Giving2012]
        ,[Giving2011]
        ,[Giving2010]
        ,[Giving2009]
        ,[Giving2008]
        ,[Giving2007]
        ,[Guid]
        ,[ForeignId]
        ,[LastAttendedDate]
        )
    SELECT F.Id AS [FamilyID]
        ,F.NAME [FamilyName]
        ,HH.HHPersonId
        ,HH.FirstName
        ,HH.NickName
        ,HH.LastName
        ,HH.FullName
        ,HH.Gender
        ,HH.ConnectionStatus [HHMemberStatus]
        ,HH.MaritalStatus
        ,HH.FirstVisitDate
        ,HH.FirstActivity
        ,HH.Age
		,GFG.Id
		,GFG.Name
        ,CASE 
            WHEN EXISTS (
                    SELECT NG.Id
                    FROM GroupMember NGM
                    LEFT OUTER JOIN [Group] NG ON NG.Id = NGM.GroupId
                        AND NG.GroupTypeId = 49
                    WHERE NGM.PersonId IN (
                            SELECT TOP 1 P.Id
                            FROM Person P
                            INNER JOIN GroupMember GM ON GM.PersonId = P.Id
                            WHERE GM.GroupId = F.Id
                                AND GM.GroupRoleId = 3
                            ORDER BY P.Gender
                                ,P.BirthYear DESC
                            )
                    )
                THEN 1
            ELSE 0
            END [InNeighborhoodGroup]
        ,(
            SELECT max(case RegularAttendee when 1 then 1 else 0 end)
            FROM _church_ccv_Datamart_ERA ERA
            WHERE ERA.WeekendDate = @CurrentSaturdayDate
                AND F.Id = ERA.FamilyId
            ) [IsEra]
        ,--isera
        ng.Name
        ,--nearest group name
		ng.Id
        ,--nearest group id
        CASE 
            WHEN FamilyThatServes.ID IS NULL
                THEN 0
            ELSE 1
            END [IsServing]
        ,--isserving
        (
            SELECT TimesAttendedLast16Weeks
            FROM _church_ccv_Datamart_ERA
            WHERE FamilyId = F.Id
                AND WeekendDate = @CurrentSaturdayDate
            ) [Attendance16wk]
        ,--attendance16wk
        HH.ConnectionStatus
        ,--connectionstatus
        HH.Email
        ,HH.HomePhone
        ,--home phone
        (
            SELECT COUNT(*)
            FROM GroupMember FM
            WHERE FM.GroupId = F.Id
                AND FM.GroupRoleId = 3
            ) [AdultCount]
        ,--adultcount
        (
            SELECT COUNT(*)
            FROM GroupMember FM
            WHERE FM.GroupId = F.Id
                AND FM.GroupRoleId = 4
            ) [ChildCound]
        ,--childcount
        L.Id [LocationId]
        ,(L.Street1 + ' ' + L.Street2) [Street]
        ,L.City
        ,L.[State]
        ,L.Country
        ,L.PostalCode
        ,L.GeoPoint
        ,L.GeoPoint.Lat [Latitude]
        ,L.GeoPoint.Long [Longitude]
        ,C.NAME
        ,LEFT(STUFF((
                SELECT ', ' + P.NickName
                FROM GroupMember FM
                INNER JOIN Person P ON P.Id = FM.PersonId
                WHERE FM.GroupId = F.Id
                    AND FM.GroupRoleId = 3
                ORDER BY P.Gender
                FOR XML PATH('')
                ), 1, 1, ''), 200) [AdultNames]
        ,--adultnames
        LEFT(STUFF((
                SELECT ', ' + P.NickName
                FROM GroupMember FM
                INNER JOIN Person P ON P.Id = FM.PersonId
                WHERE FM.GroupId = F.Id
                    AND FM.GroupRoleId = 4
                ORDER BY P.BirthDate
                FOR XML PATH('')
                ), 1, 1, ''), 2000) [ChildNames]
        ,--childnames
        G.[2015]
        ,G.[2014]
        ,G.[2013]
        ,G.[2012]
        ,G.[2011]
        ,G.[2010]
        ,G.[2009]
        ,G.[2008]
        ,G.[2007]
        ,NEWID() [Guid]
        ,NULL [ForeignId]
        ,(
            SELECT MAX(at.StartDateTime)
            FROM Attendance at
            JOIN PersonAlias pa ON pa.Id = at.PersonAliasId
            WHERE pa.PersonId IN (
                    SELECT PersonId
                    FROM GroupMember
                    WHERE GroupId = f.Id
                    )
                AND isnull(at.DidAttend, 0) = 1
            ) AS [LastAttendedDate]
    FROM CTE HH
    INNER JOIN [Group] F ON F.Id = HH.FamilyId
    INNER JOIN Campus C ON C.Id = F.CampusId
    LEFT OUTER JOIN [GroupLocation] GL ON GL.GroupId = F.Id
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
            ) ng
    LEFT OUTER JOIN Giving G ON G.familyid = F.Id
    LEFT OUTER JOIN (
        SELECT DISTINCT f.Id
        FROM GroupMember FM
        JOIN [Group] f ON fm.GroupId = f.Id
        WHERE f.GroupTypeId = 10
            AND fm.PersonId IN (
                SELECT stm.PersonId
                FROM [Group] ST
                JOIN [GroupMember] STM ON STM.GroupId = st.Id
                    AND ST.GroupTypeId = 23
                )
        ) [FamilyThatServes] ON FamilyThatServes.Id = f.Id
    WHERE F.GroupTypeId = 10
END