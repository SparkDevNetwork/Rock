/*
<doc>
	<summary>
 		This stored procedure builds the data mart table _church_ccv_spProcess_ERAAttributes
	</summary>
	
	<remarks>	
	</remarks>
	<code>
		EXEC [dbo].[_church_ccv_spProcess_ERAAttributes]
	</code>
</doc>
*/
CREATE PROC [dbo].[_church_ccv_spProcess_ERAAttributes]
AS
DELETE AttributeValue
WHERE AttributeId IN (
        2533
        ,2534
        ,2535
        ,2536
        ,2537
        ,2538
        ,2539
        ,2540
        )

DECLARE @CurrentWeekend DATETIME

SET @CurrentWeekend = dbo._church_ccv_ufnGetSaturdayDate(GETDATE())

-- Currently an eRA
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2533
    ,CASE MAX(CAST(CW.RegularAttendee AS INT))
		WHEN 1 THEN 'True'
		ELSE 'False'
	 END
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.RegularAttendee IS NOT NULL 
GROUP BY FM.PersonId

-- Date family became 'lost'
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2540
    ,DATEADD(week, 1,(SELECT MAX(WeekendDate)
	 FROM _church_ccv_Datamart_ERA ERA
	 INNER JOIN GroupMember FM1 ON FM1.GroupId = ERA.FamilyId
	 WHERE FM1.PersonId = FM.PersonId
		AND ERA.RegularAttendee = 1))
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.RegularAttendee = 0
	AND CW.WeekendDate IS NOT NULL 
GROUP BY FM.PersonId

-- Last 16 wk Attendance
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2534
    ,MAX(CW.TimesAttendedLast16Weeks)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.TimesAttendedLast16Weeks IS NOT NULL 
GROUP BY FM.PersonId

-- First Attended
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2535
    ,MAX(CW.FirstAttended)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.FirstAttended IS NOT NULL 
GROUP BY FM.PersonId

-- Last Attended
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2536
    ,MAX(CW.LastAttended)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.LastAttended IS NOT NULL 
GROUP BY FM.PersonId

-- Last 6 wk Given
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2537
    ,MAX(CW.TimesGaveLast6Weeks)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.TimesGaveLast6Weeks IS NOT NULL 
GROUP BY FM.PersonId

-- Times Given
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2538
    ,MAX(CW.TimesGaveLastYear)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.TimesGaveLastYear IS NOT NULL 
GROUP BY FM.PersonId

-- Last Gave
INSERT INTO AttributeValue (
    EntityId
    ,AttributeId
    ,Value
    ,IsSystem
    ,[Guid]
	,[CreatedDateTime]
    )
SELECT FM.PersonId
    ,2539
    ,MAX(CW.LastGave)
    ,0
    ,NEWID()
	,GETDATE()
FROM _church_ccv_Datamart_ERA CW
INNER JOIN GroupMember FM ON FM.GroupId = CW.FamilyId
WHERE CW.WeekendDate = @CurrentWeekend
	AND CW.LastGave IS NOT NULL 
GROUP BY FM.PersonId