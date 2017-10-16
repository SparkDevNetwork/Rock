IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_Family]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_Family
GO

-- EXECUTE [dbo].[spAnalytics_ETL_Family] 
CREATE PROCEDURE [dbo].spAnalytics_ETL_Family
AS
BEGIN
    DECLARE @EtlDate DATE = convert(DATE, SysDateTime())
        ,@MaxExpireDate DATE = DateFromParts(9999, 1, 1)
        ,@RecordStatusActiveId INT = (
            SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'
            )
        ,@GroupTypeFamilyId INT = (
            SELECT TOP 1 Id
            FROM GroupType
            WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E'
            )
        ,@GroupRoleAdultId INT = (
            SELECT TOP 1 Id
            FROM GroupTypeRole
            WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42'
            )
        ,@GroupRoleChildId INT = (
            SELECT TOP 1 Id
            FROM GroupTypeRole
            WHERE [Guid] = 'C8B1814F-6AA7-4055-B2D7-48FE20429CB9'
            )
        ,@AttributeIdCore_CurrentlyAnEra INT = (
            SELECT TOP 1 Id
            FROM Attribute
            WHERE [Key] = 'core_CurrentlyAnEra'
            )
        ,@GroupLocationTypeFamilyHomeId INT = (
            SELECT TOP 1 Id
            FROM DefinedValue
            WHERE [Guid] = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'
            )
		,@RowsUpdated INT = 0
		,@RowsInserted INT = 0

    -- throw it all into a temp table so we can Insert and Update only where needed
    CREATE TABLE #AnalyticsSourceFamily (
        [FamilyId] [int] NOT NULL
        ,[Name] [nvarchar](100) NULL
        ,[FamilyTitle] [nvarchar](250) NULL
        ,[CampusId] [int] NULL
        ,[ConnectionStatus] [nvarchar](250) NULL
        ,[IsFamilyActive] [bit] NOT NULL
        ,[AdultCount] [int] NOT NULL
        ,[ChildCount] [int] NOT NULL
        ,[HeadOfHouseholdPersonKey] [int] NULL
        ,[IsEra] [bit] NOT NULL
        ,[MailingAddressLocationId] [int] NULL
        ,[MappedAddressLocationId] [int] NULL
        ,PRIMARY KEY CLUSTERED ([FamilyId])
        )

    INSERT INTO #AnalyticsSourceFamily (
        [FamilyId]
        ,[Name]
        ,[FamilyTitle]
        ,[CampusId]
        ,[ConnectionStatus]
        ,[IsFamilyActive]
        ,[AdultCount]
        ,[ChildCount]
        ,[HeadOfHouseholdPersonKey]
        ,[IsEra]
        ,[MailingAddressLocationId]
        ,[MappedAddressLocationId]
        )
    SELECT g.Id [FamilyId]
        ,g.NAME
        ,SUBSTRING(ft.PersonNames, 1, 250) [FamilyTitle]
        ,g.CampusId [CampusId]
        ,(
            SELECT TOP 1 (
                    CASE 
                        WHEN dv.Value IS NULL
                            THEN ''
                        ELSE dv.Value
                        END
                    )
            FROM [GroupMember] gm
            JOIN [Person] p ON gm.PersonId = p.Id
            JOIN [DefinedValue] dv ON p.ConnectionStatusValueId = dv.Id
            WHERE gm.GroupId = g.Id
            ORDER BY dv.[Order]
            ) [ConnectionStatus] -- ConnectionStatus of “Most Connected family member” (based on DefinedValue.Order where First is most connected)
        ,(
            SELECT CASE count(*)
                    WHEN 0
                        THEN 0
                    ELSE 1
                    END
            FROM [GroupMember] gm
            JOIN [Person] p ON gm.PersonId = p.Id
            WHERE gm.GroupId = g.Id
                AND p.RecordStatusValueId = @RecordStatusActiveId
            ) [IsFamilyActive]
        ,(
            SELECT COUNT(*)
            FROM GroupMember FM
            WHERE FM.GroupId = g.Id
                AND FM.GroupRoleId = @GroupRoleAdultId
            ) [AdultCount]
        ,(
            SELECT COUNT(*)
            FROM GroupMember FM
            WHERE FM.GroupId = g.Id
                AND FM.GroupRoleId = @GroupRoleChildId
            ) [ChildCount]
        ,hhpc.Id [HeadOfHouseholdPersonKey]
        ,(
            SELECT CASE max(convert(INT, CASE 
     WHEN av.ValueAsBoolean IS NULL
                                    THEN 0
                                ELSE av.ValueAsBoolean
                                END))
                    WHEN 1
                        THEN 1
                    ELSE 0
                    END
            FROM AttributeValue av
            JOIN GroupMember gm ON gm.GroupId = g.Id
            WHERE av.AttributeId = @AttributeIdCore_CurrentlyAnEra
                AND av.EntityId = gm.PersonId
            ) [IsEra]
        ,(
            SELECT max(gl.LocationId)
            FROM GroupLocation gl
            WHERE gl.GroupId = g.Id
                AND gl.GroupLocationTypeValueId = @GroupLocationTypeFamilyHomeId
                AND gl.IsMailingLocation = 1
            ) [MailingAddressLocationId]
        ,(
            SELECT max(gl.LocationId)
            FROM GroupLocation gl
            WHERE gl.GroupId = g.Id
                AND gl.GroupLocationTypeValueId = @GroupLocationTypeFamilyHomeId
                AND gl.IsMappedLocation = 1
            ) [MappedAddressLocationId]
    FROM [Group] g
    LEFT OUTER JOIN AnalyticsDimPersonCurrent hhpc ON hhpc.PersonId = (
            SELECT TOP 1 P.Id
            FROM GroupMember FM
            INNER JOIN Person P ON P.Id = FM.PersonId
            INNER JOIN [Group] F ON F.Id = FM.GroupId
                AND F.GroupTypeId = @GroupTypeFamilyId
            WHERE FM.GroupId = g.Id
            ORDER BY FM.GroupRoleId
                ,P.Gender
            ) --HeadOfHousehold
    CROSS APPLY dbo.ufnCrm_GetFamilyTitle(NULL, g.Id, NULL, 0) ft
    WHERE g.GroupTypeId = @GroupTypeFamilyId
    ORDER BY g.Id

	-- Mark Rows as History if any of the important field values change
	UPDATE fh
    SET  CurrentRowIndicator = 0,
        [ExpireDate] = @EtlDate
    FROM AnalyticsSourceFamilyHistorical fh
    JOIN #AnalyticsSourceFamily t ON t.FamilyId = fh.FamilyId
        AND fh.CurrentRowIndicator = 1
    WHERE 
        fh.[FamilyTitle] != t.FamilyTitle
        OR fh.[CampusId] != t.CampusId
        OR fh.[ConnectionStatus] != t.ConnectionStatus
        OR fh.[IsFamilyActive] != t.IsFamilyActive
        OR fh.[AdultCount] != t.AdultCount
        OR fh.[ChildCount] != t.ChildCount
        OR fh.[HeadOfHouseholdPersonKey] != t.HeadOfHouseholdPersonKey
        OR fh.[IsEra] != t.IsEra
        OR fh.[MailingAddressLocationId] != t.MailingAddressLocationId
        OR fh.[MappedAddressLocationId] != t.MappedAddressLocationId
AND fh.FamilyId NOT IN ( -- Ensure that there isn't already a History Record for the current EtlDate 
    SELECT FamilyId
    FROM AnalyticsSourceFamilyHistorical x
    WHERE CurrentRowIndicator = 0
        AND [ExpireDate] = @EtlDate
    )
	
	-- Insert Families that don't have a "CurrentRowIndicator" Row yet (either it was marked as history, or they are a new family)
    INSERT INTO AnalyticsSourceFamilyHistorical (
        [FamilyId]
        ,[CurrentRowIndicator]
        ,[EffectiveDate]
        ,[ExpireDate]
        ,[Name]
        ,[FamilyTitle]
        ,[CampusId]
        ,[ConnectionStatus]
        ,[IsFamilyActive]
        ,[AdultCount]
        ,[ChildCount]
        ,[HeadOfHouseholdPersonKey]
        ,[IsEra]
        ,[MailingAddressLocationId]
        ,[MappedAddressLocationId]
        ,[Guid]
        )
    SELECT [FamilyId]
        ,1 [CurrentRowIndicator]
        ,@EtlDate [EffectiveDate]
        ,@MaxExpireDate [ExpireDate]
        ,[Name]
        ,[FamilyTitle]
        ,[CampusId]
        ,[ConnectionStatus]
        ,[IsFamilyActive]
        ,[AdultCount]
        ,[ChildCount]
        ,[HeadOfHouseholdPersonKey]
        ,[IsEra]
        ,[MailingAddressLocationId]
        ,[MappedAddressLocationId]
        ,NEWID()
    FROM #AnalyticsSourceFamily s
    WHERE s.FamilyId NOT IN (
            SELECT FamilyId
            FROM AnalyticsSourceFamilyHistorical
 WHERE CurrentRowIndicator = 1
            )

    set @RowsInserted = @@ROWCOUNT;

    UPDATE fh
    SET fh.NAME = t.NAME
        ,fh.[FamilyTitle] = t.FamilyTitle
        ,fh.[CampusId] = t.CampusId
        ,fh.[ConnectionStatus] = t.ConnectionStatus
        ,fh.[IsFamilyActive] = t.IsFamilyActive
        ,fh.[AdultCount] = t.AdultCount
        ,fh.[ChildCount] = t.ChildCount
        ,fh.[HeadOfHouseholdPersonKey] = t.HeadOfHouseholdPersonKey
        ,fh.[IsEra] = t.IsEra
        ,fh.[MailingAddressLocationId] = t.MailingAddressLocationId
        ,fh.[MappedAddressLocationId] = t.MappedAddressLocationId
    FROM AnalyticsSourceFamilyHistorical fh
    JOIN #AnalyticsSourceFamily t ON t.FamilyId = fh.FamilyId
        AND fh.CurrentRowIndicator = 1
    WHERE fh.NAME != t.NAME
        OR fh.[FamilyTitle] != t.FamilyTitle
        OR fh.[CampusId] != t.CampusId
        OR fh.[ConnectionStatus] != t.ConnectionStatus
        OR fh.[IsFamilyActive] != t.IsFamilyActive
        OR fh.[AdultCount] != t.AdultCount
        OR fh.[ChildCount] != t.ChildCount
        OR fh.[HeadOfHouseholdPersonKey] != t.HeadOfHouseholdPersonKey
        OR fh.[IsEra] != t.IsEra
        OR fh.[MailingAddressLocationId] != t.MailingAddressLocationId
        OR fh.[MappedAddressLocationId] != t.MappedAddressLocationId

    set @RowsUpdated = @@ROWCOUNT;

    -- delete any Family records that no longer exist the [Group] table (or are no longer GroupType of family)
    DELETE
    FROM AnalyticsSourceFamilyHistorical
    WHERE FamilyId NOT IN (
            SELECT Id
            FROM [Group]
            WHERE GroupTypeId = @GroupTypeFamilyId
            )

    /*
    Explicitly clean up temp tables before the proc exits (vs. have SQL Server do it for us after the proc is done)
    */
    DROP TABLE #AnalyticsSourceFamily;

	select @RowsInserted [RowsInserted], @RowsUpdated [RowsUpdated]
END