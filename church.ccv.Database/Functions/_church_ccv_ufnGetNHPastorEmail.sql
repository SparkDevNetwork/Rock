/*
<doc>
       <summary>
             (Deprecated) Returns the email address of the person's Neighborhood Pastor
       </summary>

       <returns>
       </returns>
       <param name="PersonId" datatype="int">The person id</param>
       <remarks>     
       </remarks>
       <code>
       </code>
</doc>
*/

ALTER FUNCTION [dbo].[_church_ccv_ufnGetNHPastorEmail]
(
	@PersonId INT
)
RETURNS VARCHAR(50)
AS
BEGIN
	
	DECLARE @NHPastorEmail VARCHAR(50) = ''
	
	SELECT @NHPastorEmail = NP.Email
	FROM _church_ccv_Datamart_Person DP
	INNER JOIN [Group] G ON G.Id = DP.NeighborhoodId
	INNER JOIN [GroupMember] AL ON AL.GroupId = G.Id AND AL.GroupRoleId = 45
	INNER JOIN [Person] NP ON NP.Id = AL.PersonId
	WHERE DP.PersonId = @PersonId
	
	RETURN @NHPastorEmail

END