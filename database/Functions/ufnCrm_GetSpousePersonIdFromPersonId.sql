/*
<doc>
	<summary>
 		This function returns the most likely spouse for the person id provided
	</summary>

	<returns>
		Person Id of the most likely spouse. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](3) -- Ted Decker (married) 
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](7) -- Ben Jones (single)
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId]( @PersonId int ) 

RETURNS int AS
BEGIN
	
	RETURN (SELECT TOP 1 S.ID
		from [Group] F
		join GroupType GT on F.GroupTypeId = GT.ID
		join GroupMember FM on FM.GroupId = F.ID
		join Person P on P.ID = FM.PersonId
		join GroupTypeRole R on R.ID = FM.GroupRoleId
		join GroupMember FM2 on FM2.GroupID = F.ID
		join Person S on S.ID = FM2.PersonID
		join GroupTypeRole R2 on R2.ID = FM2.GroupRoleId
		where GT.Guid = '790E3215-3B10-442B-AF69-616C0DCB998E' -- Family
		and P.Id = @PersonID
		and R.Guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Person must be an Adult
		and R2.Guid = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- Potential spouse must be an Adult
		and P.MaritalStatusValueId = 143 -- Person must be Married
		and S.MaritalStatusValueId = 143 -- Potential spouse must be Married
		and FM.PersonID != FM2.PersonID -- Cannot be married to yourself
		and (P.Gender != S.Gender OR P.Gender = 0 OR S.Gender = 0) -- Genders cannot match if both are known
		order by ABS(DATEDIFF(day, ISNULL(P.BirthDate, '1/1/0001'), ISNULL(S.BirthDate, '1/1/0001'))) -- If multiple results, choose nearest in age
			, S.Id -- Sort by ID so that the same result is always returned
	)

END