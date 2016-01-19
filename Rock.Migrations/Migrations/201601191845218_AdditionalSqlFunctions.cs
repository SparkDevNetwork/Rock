// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AdditionalSqlFunctions : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"IF object_id('[dbo].[ufnCrm_GetFamilyTitleFromGivingId]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetFamilyTitleFromGivingId]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function returns the household name from a giving id.
	</summary>

	<returns>
		String of household name. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetFamilyTitleFromGivingId]('G63') -- Decker's (married) Returns 'Ted & Cindy Decker'
		SELECT [dbo].[ufnCrm_GetFamilyTitleFromGivingId]('G64') -- Jones' (single) Returns 'Ben Jones'
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetFamilyTitleFromGivingId](@GivingId varchar(31) ) 

RETURNS nvarchar(250) AS
BEGIN
	DECLARE @UnitType char(1)
	DECLARE @UnitId int
	DECLARE @Result varchar(250)

	SET @UnitType = LEFT(@GivingId, 1)
	SET @UnitId = CAST(SUBSTRING(@GivingId, 2, LEN(@GivingId)) AS INT)

	IF @UnitType = 'P'
		SET @Result = (SELECT TOP 1 [NickName] + ' ' + [LastName] FROM [Person] WHERE [GivingId] = @GivingId)
	ELSE
		SET @Result = (SELECT * FROM dbo.ufnCrm_GetFamilyTitle(null, @UnitId, default, 1))

	RETURN @Result
END
GO

IF object_id('[dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
<doc>
	<summary>
 		This function returns the head of house for the giving id provided
	</summary>

	<returns>
		Person Id of the head of household. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId]('G63') -- Decker's (married) 
		SELECT [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId]('G64') -- Jones' (single)
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId](@GivingId varchar(31) ) 

RETURNS int AS
BEGIN
	DECLARE @UnitType char(1)
	DECLARE @UnitId int
	DECLARE @Result int

	SET @UnitType = LEFT(@GivingId, 1)
	SET @UnitId = CAST(SUBSTRING(@GivingId, 2, LEN(@GivingId)) AS INT)

	IF @UnitType = 'P' -- person
		SET @Result = @UnitId
	ELSE -- family
		SET @Result =	(
							SELECT TOP 1 p.[Id] 
							FROM 
								[Person] p
								INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
								INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
							WHERE 
								gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
								AND gm.[GroupId] = @UnitId
							ORDER BY p.[Gender]
						)

	RETURN @Result
END
GO

IF object_id('[dbo].[ufnCrm_GetSpousePersonIdFromPersonId]') IS NOT NULL
BEGIN
  DROP FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId]
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
<doc>
	<summary>
 		This function returns the head of house for the giving id provided
	</summary>

	<returns>
		Person Id of the head of household. 
	</returns>
	<remarks>
		

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](3) -- Ted Decker (married) 
		SELECT [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](7) -- Ben Jones (single)
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId](@PersonId int ) 

RETURNS int AS
BEGIN
	
	RETURN (SELECT TOP 1 p.[Id] 
				FROM 
					[Person] p
					INNER JOIN [GroupMember] gm ON gm.[PersonId] = p.[Id]
					INNER JOIN [GroupTypeRole] gtr ON gtr.[Id] = gm.[GroupRoleId]
					INNER JOIN [Group] g ON g.[Id] = gm.[GroupId]
					INNER JOIN [GroupType] gt ON gt.[Id] = g.[GroupTypeId]
				WHERE 
					gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
					AND gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- family
					AND g.[Id] IN (SELECT g2.[Id] 
									FROM [GroupMember] gm2
										INNER JOIN [GroupTypeRole] gtr2 ON gtr2.[Id] = gm2.[GroupRoleId]
										INNER JOIN [Group] g2 ON g.[Id] = gm2.[GroupId]
										INNER JOIN [GroupType] gt2 ON gt2.[Id] = g2.[GroupTypeId]
									WHERE gm2.[PersonId] = @PersonId
										AND gtr.[Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' -- adult
										AND gt.[Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' -- family
									)
		
					AND gm.[PersonId] != @PersonId
				ORDER BY p.[Gender])

END
GO" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                    DROP FUNCTION [dbo].[ufnCrm_GetSpousePersonIdFromPersonId]
                    GO
                    DROP FUNCTION [dbo].[ufnCrm_GetFamilyTitleFromGivingId]
                    GO
                    DROP FUNCTION [dbo].[ufnCrm_GetHeadOfHousePersonIdFromGivingId]
                    GO
                " );
        }
    }
}
