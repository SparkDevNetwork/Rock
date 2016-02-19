ALTER FUNCTION [dbo].[_church_ccv_FamilyLastAttended] (@FamilyID int, @WeekendDate datetime)
returns datetime

as
begin

	DECLARE @LastAttended datetime 
	
	SET @LastAttended = (
		SELECT MAX(A.StartDateTime) AS last_attended
		FROM GroupMember FM
		INNER JOIN PersonAlias PA ON PA.PersonId = FM.PersonId
		INNER JOIN Attendance A  ON A.PersonAliasId = PA.Id
		WHERE FM.GroupId = @FamilyID
		AND A.StartDateTime <= @WeekendDate
		AND A.DidAttend = 1
		AND A.GroupId IN (1199220,1199221,1199222,1199223,1199224,
						1199225,1199226,1199227,1199228,1199231,
						1199232,1199233,1199600,1199601,1199602))

		
	RETURN @LastAttended

end