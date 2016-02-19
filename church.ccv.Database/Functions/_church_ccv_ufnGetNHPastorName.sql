/*
<doc>
       <summary>
             (Deprecated) Returns the name of the person's Neighborhood Pastor
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

ALTER FUNCTION [dbo].[_church_ccv_ufnGetNHPastorName]
(
	@PersonId INT
)
RETURNS VARCHAR(30)
AS
BEGIN
	
	DECLARE @NHPastorName VARCHAR(30) = ''
	
	SELECT @NHPastorName = NP.NickName + ' ' + NP.LastName
	FROM _church_ccv_Datamart_Person DP
	INNER JOIN [Group] G ON G.Id = DP.NeighborhoodId
	INNER JOIN [GroupMember] AL ON AL.GroupId = G.Id AND AL.GroupRoleId = 45
	INNER JOIN [Person] NP ON NP.Id = AL.PersonId
	WHERE DP.PersonId = @PersonId
	
	RETURN @NHPastorName

END