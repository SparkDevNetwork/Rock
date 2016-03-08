/*
<doc>
	<summary>
 		This function returns all group types that are used to denote 
		groups that are for tracking attendance for weekly services
	</summary>

	<returns>
		* GroupTypeId
		* Guid
		* Name
	</returns>
	<remarks>
		Uses the following constants:
			* Defined Value - Check-in Filter: 6BCED84C-69AD-4F5A-9197-5C0F9C02DD34
			* Defined Value - Check-in Template: 4A406CB0-495B-4795-B788-52BDFDE00B01
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
	</code>
</doc>
*/


ALTER FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
RETURNS TABLE AS

RETURN ( WITH
	cteServiceGroupTypes ([Id], [Guid], [Name])
	AS (

		SELECT [Id], [Guid], [Name]
		FROM [GroupType]
		WHERE [GroupTypePurposeValueId] in (SELECT [Id] FROM [DefinedValue] WHERE [Guid] in ('6BCED84C-69AD-4F5A-9197-5C0F9C02DD34', '4A406CB0-495B-4795-B788-52BDFDE00B01'))

		UNION ALL

		SELECT g.[Id], g.[Guid], g.[Name]
		FROM [GroupType] g
			INNER JOIN cteServiceGroupTypes r ON g.[InheritedGroupTypeId] = r.[Id]

	)

 SELECT DISTINCT * FROM cteServiceGroupTypes )