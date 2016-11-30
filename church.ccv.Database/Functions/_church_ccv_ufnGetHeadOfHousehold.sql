ALTER FUNCTION [dbo].[_church_ccv_ufnGetHeadOfHousehold](@FamilyID int)
	RETURNS int

AS 

BEGIN
	DECLARE @HeadPersonID int
	SET @HeadPersonID = (SELECT TOP 1 P.Id
				FROM GroupMember FM
				INNER JOIN Person P ON P.Id = FM.PersonId
				INNER JOIN [Group] F ON F.Id = FM.GroupId AND F.GroupTypeId = 10
				WHERE FM.GroupId = @FamilyID
				ORDER BY FM.GroupRoleId, P.Gender)
	RETURN @HeadPersonID
END