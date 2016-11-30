ALTER FUNCTION [dbo].[_church_ccv_ufn_GetHeadOfHousehold](@FamilyID int)
	RETURNS int

AS 

BEGIN
	DECLARE @HeadPersonID int
	SET @HeadPersonID = (SELECT TOP 1 P.Id
				FROM GroupMember FM
				INNER JOIN Person P ON P.Id = FM.PersonId
				WHERE FM.GroupId = @FamilyID
				ORDER BY FM.GroupRoleId, P.Gender)
	RETURN @HeadPersonID
END