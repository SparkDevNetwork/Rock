ALTER VIEW [dbo].[_church_ccv_v_ERALoss]
AS
SELECT        
CW.FamilyId, 
DATEADD(week, 1,
    (SELECT MAX(WeekendDate) AS Expr1
     FROM _church_ccv_Datamart_ERA
     WHERE (FamilyId = CW.FamilyId) AND (RegularAttendee = 1))) AS LossDate
FROM _church_ccv_Datamart_ERA AS CW 
INNER JOIN GroupMember FM ON FM.PersonId = dbo._church_ccv_ufnGetHeadOfHousehold(CW.FamilyId) 
INNER JOIN Person P ON P.Id = FM.PersonId AND P.RecordStatusValueId = 3
WHERE (CW.WeekendDate = dbo._church_ccv_ufnGetSaturdayDate(GETDATE())) AND (CW.RegularAttendee = 0) AND (FM.GroupRoleId = 3)