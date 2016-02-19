ALTER FUNCTION [dbo].[_church_ccv_ufnFamilyFirstAttended] (@FamilyID int)
returns datetime

as
begin

	DECLARE @FirstAttended datetime 
	
	SET @FirstAttended = (
		SELECT MIN(A.StartDateTime) AS first_attended
		FROM GroupMember FM
		INNER JOIN PersonAlias PA ON PA.PersonId = FM.PersonId
		INNER JOIN Attendance A  ON A.PersonAliasId = PA.Id
		WHERE FM.GroupId = @FamilyID
		AND A.DidAttend = 1
		AND A.GroupId IN (1199220,1199221,1199222,1199223,1199224,
						1199225,1199226,1199227,1199228,1199231,
						1199232,1199233,1199600,1199601,1199602))

	IF @FirstAttended IS NOT NULL
		SET @FirstAttended = dbo._church_ccv_ufnGetSaturdayDate(@FirstAttended)
		
	RETURN @FirstAttended

end