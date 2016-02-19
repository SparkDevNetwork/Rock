/*
<doc>
       <summary>
             (Deprecated) Returns the internal phone number of the person's Neighborhood Pastor
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

ALTER FUNCTION [dbo].[_church_ccv_ufnGetNHPastorPhone]
(
	@PersonId INT
)
RETURNS VARCHAR(50)
AS
BEGIN
	
	DECLARE @NHPastorPhone VARCHAR(50) = ''
	
	SELECT @NHPastorPhone = PH.NumberFormatted
	FROM _church_ccv_Datamart_Person DP
	INNER JOIN [Group] G ON G.Id = DP.NeighborhoodId
	INNER JOIN [GroupMember] AL ON AL.GroupId = G.Id AND AL.GroupRoleId = 45
	INNER JOIN [Person] NP ON NP.Id = AL.PersonId
	INNER JOIN [PhoneNumber] PH ON PH.PersonId = NP.Id AND PH.NumberTypeValueId = 613
	WHERE DP.PersonId = @PersonId
	
	RETURN @NHPastorPhone

END