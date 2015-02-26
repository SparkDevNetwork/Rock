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
    public partial class AddGradesDefinedValue : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT, string.Empty, string.Empty, "Grade Label", "The label for items that refer to the grade that the person is in.", 0, "Grade", "20A402B4-4098-4040-948E-0C20E44780DD", "core.GradeLabel" );

            RockMigrationHelper.AddDefinedType( "Global", "School Grades", "Used to calculate school classes/grades in a way that can be modified for internationalization.", "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", @"" );

            RockMigrationHelper.AddDefinedTypeAttribute( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Abbreviation", "Abbreviation", "", 31, "", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0" );
            RockMigrationHelper.AddAttributeQualifier( "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", "ispassword", "False", "33ECF11F-F69C-4A20-B5A3-25C563C859E6" );

            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "0", "Senior", "C49BD3AF-FF94-4A7C-99E1-08503A3C746E", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "1", "Junior", "78F7D773-8244-4995-8BC4-AD6F6A7B7820", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "2", "Sophomore", "E04E3F62-EF5C-4860-8F32-1C152CA1700A", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "3", "Freshman", "2A130E04-3712-427A-8BB0-473EB8FF8924", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "4", "8th Grade", "D58D70AF-3CCC-4D4E-BFAF-2014D8579D60", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "5", "7th Grade", "3FE728AC-BE25-409A-98CB-3CFCE5FA063B", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "6", "6th Grade", "2D702ED8-7046-4DA5-AFFA-9633A211F594", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "7", "5th Grade", "3D8CDBC8-8840-4A7E-85D0-B7C29A019EBB", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "8", "4th Grade", "F0F98B9C-E6BE-4C42-B8F4-0D8AB1A18847", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "9", "3rd Grade", "23CC6288-78ED-4849-AFC9-417E0DA5A4A9", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "10", "2nd Grade", "E475D0CA-5979-4C76-8788-D91ADF595E10", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "11", "1st Grade", "6B5CDFBD-9882-4EBB-A01A-7856BCD0CF61", false );
            RockMigrationHelper.AddDefinedValue( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D", "12", "Kindergarten", "0FED3291-51F3-4EED-886D-1D3DF826BEAC", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "0FED3291-51F3-4EED-886D-1D3DF826BEAC", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"K" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6B5CDFBD-9882-4EBB-A01A-7856BCD0CF61", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"1st" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E475D0CA-5979-4C76-8788-D91ADF595E10", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"2nd" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "23CC6288-78ED-4849-AFC9-417E0DA5A4A9", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"3rd" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F0F98B9C-E6BE-4C42-B8F4-0D8AB1A18847", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"4th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3D8CDBC8-8840-4A7E-85D0-B7C29A019EBB", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"5th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2D702ED8-7046-4DA5-AFFA-9633A211F594", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"6th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3FE728AC-BE25-409A-98CB-3CFCE5FA063B", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"7th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D58D70AF-3CCC-4D4E-BFAF-2014D8579D60", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"8th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2A130E04-3712-427A-8BB0-473EB8FF8924", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"9th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "E04E3F62-EF5C-4860-8F32-1C152CA1700A", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"10th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "78F7D773-8244-4995-8BC4-AD6F6A7B7820", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"11th" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C49BD3AF-FF94-4A7C-99E1-08503A3C746E", "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0", @"12th" );


            // Migration Rollups

            Sql( @"
/*
<doc>
	<summary>
		This stored procedure returns the Mailing Addresses and any CustomMessages for the Contribution Statement, but not the actual transactions
		The StatementGenerator utility uses this procedure along with querying transactions thru REST to generate statements
	</summary>

	<returns>
		* PersonId
		* GroupId
		* AddressPersonNames
		* Street1
		* Street2
		* City
		* State
		* PostalCode
		* StartDate
		* EndDate
		* CustomMessage1
		* CustomMessage2
	</returns>
	<param name='StartDate' datatype='datetime'>The starting date of the date range</param>
	<param name='EndDate' datatype='datetime'>The ending date of the date range</param>
	<param name='AccountIds' datatype='varchar(max)'>Comma delimited list of account ids. NULL means all</param>
	<param name='PersonId' datatype='int'>Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name='OrderByPostalCode' datatype='int'>Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 0, 1 -- year 2014 statements for all persons that have a mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1, 1 -- year 2014 statements for all persons regardless of mailing address
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, 2, 1, 1  -- year 2014 statements for Ted Decker
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spFinance_ContributionStatementQuery]
	@StartDate datetime
	, @EndDate datetime
	, @AccountIds varchar(max) 
	, @PersonId int -- NULL means all persons
    , @IncludeIndividualsWithNoAddress bit 
	, @OrderByPostalCode bit
AS
BEGIN
	DECLARE @cGROUPTYPE_FAMILY uniqueidentifier = '790E3215-3B10-442B-AF69-616C0DCB998E'	
	DECLARE @cLOCATION_TYPE_HOME uniqueidentifier = '8C52E53C-2A66-435A-AE6E-5EE307D9A0DC'

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;WITH tranListCTE
	AS
	(
		SELECT  
			[pa].[PersonId] 
		FROM 
			[FinancialTransaction] [ft]
		INNER JOIN 
			[FinancialTransactionDetail] [ftd] ON [ft].[Id] = [ftd].[TransactionId]
		INNER JOIN 
			[PersonAlias] [pa] ON [pa].[id] = [ft].[AuthorizedPersonAliasId]
		WHERE 
			([TransactionDateTime] >= @StartDate and [TransactionDateTime] < @EndDate)
		AND 
			(
				(@AccountIds is null)
				OR
				(ftd.[AccountId] in (select * from ufnUtility_CsvToTable(@AccountIds)))
			)
	)

	SELECT * FROM (
    SELECT 
		  [pg].[PersonId]
		, [pg].[GroupId]
		, [pn].[PersonNames] [AddressPersonNames]
        , case when l.Id is null then 0 else 1 end [HasAddress]
		, [l].[Street1]
		, [l].[Street2]
		, [l].[City]
		, [l].[State]
		, [l].[PostalCode]
		, @StartDate [StartDate]
		, @EndDate [EndDate]
		, null [CustomMessage1]
		, null [CustomMessage2]
	FROM (
		-- Get distinct Giving Groups for Persons that have a specific GivingGroupId and have transactions that match the filter
		-- These are Persons that give as part of a Group.  For example, Husband and Wife
		SELECT DISTINCT
			null [PersonId] 
			, [g].[Id] [GroupId]
		FROM 
			[Person] [p]
		INNER JOIN 
			[Group] [g] ON [p].[GivingGroupId] = [g].[Id]
		WHERE 
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
        AND
			[p].[Id] in (SELECT * FROM tranListCTE)
		UNION
		-- Get Persons and their GroupId(s) that do not have GivingGroupId and have transactions that match the filter.        
		-- These are the persons that give as individuals vs as part of a group. We need the Groups (families they belong to) in order 
		-- to determine which address(es) the statements need to be mailed to 
		SELECT  
			[p].[Id] [PersonId],
			[g].[Id] [GroupId]
		FROM
			[Person] [p]
		JOIN 
			[GroupMember] [gm]
		ON 
			[gm].[PersonId] = [p].[Id]
		JOIN 
			[Group] [g]
		ON 
			[gm].[GroupId] = [g].[Id]
		WHERE
			[p].[GivingGroupId] is null
		AND
			[g].[GroupTypeId] = (SELECT Id FROM GroupType WHERE [Guid] = @cGROUPTYPE_FAMILY)
        AND
		(
			(@personId is null) 
		OR 
			([p].[Id] = @personId)
		)
		AND [p].[Id] IN (SELECT * FROM tranListCTE)
	) [pg]
	CROSS APPLY 
		[ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId]) [pn]
	LEFT OUTER JOIN (
    SELECT l.*, gl.GroupId from
		[GroupLocation] [gl] 
	LEFT OUTER JOIN
		[Location] [l]
	ON 
		[l].[Id] = [gl].[LocationId]
	WHERE 
		[gl].[IsMailingLocation] = 1
	AND
		[gl].[GroupLocationTypeValueId] = (SELECT Id FROM DefinedValue WHERE [Guid] = @cLOCATION_TYPE_HOME)
        ) [l] 
        ON 
		[l].[GroupId] = [pg].[GroupId]
    ) n
    WHERE n.HasAddress = 1 or @IncludeIndividualsWithNoAddress = 1
    ORDER BY
	CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
    
END
" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.GroupType", 0, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "25bb9da3-f680-0d80-45ed-e944eff95353" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.GroupType", 1, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "6ce1f6a9-bb91-8fb3-4316-9dcfc90fd6af" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.GroupType", 2, "View", true, "300BA2C8-49A3-44BA-A82A-82E3FD8C3745", 0, "ee3226ce-b1d5-b995-49f7-6b3f4a1f5fd7" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.GroupType", 3, "View", false, null, 1, "73a2c6c6-d13a-6493-4121-3867ec60492a" );

            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = '{% if Person.FirstTime == true %}F{% endif %}'
    WHERE [Guid] = 'F9D654E2-B715-4C6A-B875-AC690B21B3E8'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // remove Grade Label Global Attribute
            RockMigrationHelper.DeleteAttribute( "20A402B4-4098-4040-948E-0C20E44780DD" );

            // Remove Defined Value "Abbreviation" attribute for School Grades defined values
            RockMigrationHelper.DeleteAttribute( "839B67E3-6A12-46D9-8F7F-5DFCB4F8DBE0" );

            // Remove School Grades DefinedValues/DefinedType
            RockMigrationHelper.DeleteDefinedValue( "0FED3291-51F3-4EED-886D-1D3DF826BEAC" );
            RockMigrationHelper.DeleteDefinedValue( "23CC6288-78ED-4849-AFC9-417E0DA5A4A9" );
            RockMigrationHelper.DeleteDefinedValue( "2A130E04-3712-427A-8BB0-473EB8FF8924" );
            RockMigrationHelper.DeleteDefinedValue( "2D702ED8-7046-4DA5-AFFA-9633A211F594" );
            RockMigrationHelper.DeleteDefinedValue( "3D8CDBC8-8840-4A7E-85D0-B7C29A019EBB" );
            RockMigrationHelper.DeleteDefinedValue( "3FE728AC-BE25-409A-98CB-3CFCE5FA063B" );
            RockMigrationHelper.DeleteDefinedValue( "6B5CDFBD-9882-4EBB-A01A-7856BCD0CF61" );
            RockMigrationHelper.DeleteDefinedValue( "78F7D773-8244-4995-8BC4-AD6F6A7B7820" );
            RockMigrationHelper.DeleteDefinedValue( "C49BD3AF-FF94-4A7C-99E1-08503A3C746E" );
            RockMigrationHelper.DeleteDefinedValue( "D58D70AF-3CCC-4D4E-BFAF-2014D8579D60" );
            RockMigrationHelper.DeleteDefinedValue( "E04E3F62-EF5C-4860-8F32-1C152CA1700A" );
            RockMigrationHelper.DeleteDefinedValue( "E475D0CA-5979-4C76-8788-D91ADF595E10" );
            RockMigrationHelper.DeleteDefinedValue( "F0F98B9C-E6BE-4C42-B8F4-0D8AB1A18847" );
            RockMigrationHelper.DeleteDefinedType( "24E5A79F-1E62-467A-AD5D-0D10A2328B4D" );
        }
    }
}
