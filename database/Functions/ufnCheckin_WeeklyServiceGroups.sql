/*
<doc>
	<summary>
 		This function returns all groups that are used to denote groups that are for tracking
		attendance for weekly services.
	</summary>

	<returns>
		* Id
		* Name
		* Description
		* CampusId
		* IsActive
		* Guid
		* Order
	</returns>
	<remarks>
		Uses the function dbo.ufnCheckin-WeeklyServiceGroupTypes() to get 
		group types that are used to track weekly services.
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroups]()
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroups]()
RETURNS TABLE AS

RETURN ( 
	SELECT 
		[Id] 
		, [Name] 
		, [Description]
		, [CampusId]
		, [IsActive]
		, [Guid]
		, [Order]
	FROM
		[Group] g
	WHERE
		[GroupTypeId] in (SELECT [Id] FROM dbo.ufnCheckin_WeeklyServiceGroupTypes())
)