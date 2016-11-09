IF EXISTS (
        SELECT *
        FROM [sysobjects]
        WHERE [id] = OBJECT_ID(N'[dbo].[spAnalytics_ETL_Person]')
            AND OBJECTPROPERTY([id], N'IsProcedure') = 1
        )
    DROP PROCEDURE [dbo].spAnalytics_ETL_Person
GO

--
CREATE PROCEDURE [dbo].spAnalytics_ETL_Person
AS
BEGIN
	DECLARE @EtlDateTime DATETIME = SysDateTime();
    DECLARE @CurrentEffectiveDatePrefix CHAR(8) = (convert(CHAR(8), @EtlDateTime, 112))
        ,@EtlDate DATE = convert(DATE, @EtlDateTime)
        ,@MaxExpireDate DATE = DateFromParts(9999, 1, 1)

    -- insert any records into [AnalyticsDimPersonHistorical] from the source Person table that haven't been added yet
    INSERT INTO [dbo].[AnalyticsDimPersonHistorical] (
        [PersonKey]
        ,[PersonId]
        ,[CurrentRowIndicator]
        ,[EffectiveDate]
        ,[ExpireDate]
        ,[PrimaryFamilyId]
        ,[RecordTypeValueId]
        ,[RecordStatusValueId]
        ,[RecordStatusLastModifiedDateTime]
        ,[RecordStatusReasonValueId]
        ,[ConnectionStatusValueId]
        ,[ReviewReasonValueId]
        ,[IsDeceased]
        ,[TitleValueId]
        ,[FirstName]
        ,[NickName]
        ,[MiddleName]
        ,[LastName]
        ,[SuffixValueId]
        ,[PhotoId]
        ,[BirthDay]
        ,[BirthMonth]
        ,[BirthYear]
        ,[Gender]
        ,[MaritalStatusValueId]
        ,[AnniversaryDate]
        ,[GraduationYear]
        ,[GivingGroupId]
        ,[GivingId]
        ,[GivingLeaderId]
        ,[Email]
        ,[IsEmailActive]
        ,[EmailNote]
        ,[EmailPreference]
        ,[ReviewReasonNote]
        ,[InactiveReasonNote]
        ,[SystemNote]
        ,[ViewedCount]
        ,[Guid]
        )
    SELECT -- {{ EffectiveDate in YYYYMMDD }} + '_' + {{ PersonId }}
        CONCAT (
            @CurrentEffectiveDatePrefix
            ,'_'
            ,p.Id
            ) [PersonKey]
        ,p.Id [PersonId]
        ,1 [CurrentRowIndicator]
        ,@EtlDate [EffectiveDate]
        ,@MaxExpireDate [ExpireDate]
        ,family.GroupId [PrimaryFamilyId]
        ,p.RecordTypeValueId
        ,RecordStatusValueId
        ,RecordStatusLastModifiedDateTime
        ,RecordStatusReasonValueId
        ,ConnectionStatusValueId
        ,ReviewReasonValueId
        ,IsDeceased
        ,TitleValueId
        ,FirstName
        ,NickName
        ,MiddleName
        ,LastName
        ,SuffixValueId
        ,PhotoId
        ,BirthDay
        ,BirthMonth
        ,BirthYear
        ,Gender
        ,MaritalStatusValueId
        ,AnniversaryDate
        ,GraduationYear
        ,GivingGroupId
        ,GivingId
        ,GivingLeaderId
        ,Email
        ,IsEmailActive
        ,EmailNote
        ,EmailPreference
        ,ReviewReasonNote
        ,InactiveReasonNote
        ,SystemNote
        ,ViewedCount
        ,NEWID()
    FROM Person p
    OUTER APPLY (
        SELECT top 1 gm.GroupId [GroupId]
        FROM [GroupMember] gm
        JOIN [Group] g ON gm.GroupId = g.Id
        WHERE g.GroupTypeId = 10
            AND gm.PersonId = p.Id
			order by g.IsActive desc, g.Id desc
        ) family
    WHERE p.Id NOT IN (
            SELECT PersonId
            FROM [AnalyticsDimPersonHistorical]
            WHERE CurrentRowIndicator = 1
            )

    -- Set Records as History if PersonId no longer exists in Person table
    UPDATE [AnalyticsDimPersonHistorical]
    SET [CurrentRowIndicator] = 0
        ,[ExpireDate] = @EtlDate
    WHERE PersonId NOT IN (
            SELECT Id
            FROM Person
            )
END
