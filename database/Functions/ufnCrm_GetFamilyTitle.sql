/*
<doc>
	<summary>
 		This function returns either the FullName of the specified Person or a list of names of family members
        In the case of a group (family), it will return the names of the adults of the family. If there are no adults in the family, the names of the non-adults will be listed
        Example1 (specific person): Bob Smith 
        Example2 (family with kids): Bill and Sally Jones
        Example3 (different lastnames): Jim Jackson and Betty Sanders
        Example4 (just kids): Joey, George, and Jenny Swenson
	</summary>

	<returns>
		* Name(s)
	</returns>
    <param name='PersonId' datatype='int'>The Person to get a full name for. NULL means use the GroupId paramter </param>
	<param name='@GroupId' datatype='int'>The Group (family) to get the list of names for</param>
	<param name='@GroupPersonIds' datatype='varchar(max)'>The Persons within the Group (family) to get the list of names for</param>
	<param name='@UseNickName' datatype='bit'>Determines if nickname (1) or firstname (0,default) is used in list of names</param>
	<remarks>
		[ufnCrm_GetFamilyTitle] is used by spFinance_ContributionStatementQuery as part of generating Contribution Statements
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](2, null, default, default) -- Single Person
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, default, default) -- Family
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, '2,3', default) -- Family, limited to the specified PersonIds
	</code>
</doc>
*/

/* #Obsolete# - Family Title can be gotten from Group.GroupSalutation */
ALTER FUNCTION [dbo].[ufnCrm_GetFamilyTitle] (
	@PersonId INT
	,@GroupId INT
	,@GroupPersonIds VARCHAR(max) = NULL
	,@UseNickName BIT = 0
	)
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
	INSERT INTO @PersonNamesTable ([PersonNames])
	SELECT *
	FROM [ufnCrm_GetFamilyTitleIncludeInactive](@PersonId, @GroupId, @GroupPersonIds, @UseNickName, DEFAULT)

	RETURN;
END