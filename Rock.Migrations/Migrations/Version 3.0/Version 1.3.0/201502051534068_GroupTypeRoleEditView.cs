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
    public partial class GroupTypeRoleEditView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            AddColumn("dbo.GroupTypeRole", "CanView", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupTypeRole", "CanEdit", c => c.Boolean(nullable: false));

            Sql( @"
    UPDATE [GroupTypeRole] SET [CanView] = 1 
    WHERE [IsLeader] = 1
    OR [Guid] IN ( 
	    '8438D6C5-DB92-4C99-947B-60E9100F223D',	-- Organization Unit Leader
	    '17E516FC-76A4-4BF4-9B6F-0F859B13F563',	-- Organization Unit Member
	    'F6CECB48-52C1-4D25-9411-F1465755EB70', -- Serving Team Leader
	    '8F63AB81-A2F7-4D69-82E9-158FDD92DB3D', -- Serving Team Member
	    '6D798EFA-0110-41D5-BCE4-30ACEFE4317E', -- Small Group Leader
	    'F0806058-7E5D-4CA9-9C04-3BDF92739462'  -- Small Group Member
    )

    UPDATE [GroupTypeRole] SET [CanEdit] = 1 
    WHERE [IsLeader] = 1
    OR [Guid] IN ( 
	    '8438D6C5-DB92-4C99-947B-60E9100F223D',	-- Organization Unit Leader
	    'F6CECB48-52C1-4D25-9411-F1465755EB70', -- Serving Team Leader
	    '6D798EFA-0110-41D5-BCE4-30ACEFE4317E'  -- Small Group Leader
    )
" );

            #region Migration Rollups

            // NA: Adds RootPage attribute for new SiteMap feature
            RockMigrationHelper.AddBlockTypeAttribute( "2700A1B8-BD1A-40F1-A660-476DA86D0432", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Root Page", "RootPage", "", "Select the root page to use as a starting point for the tree view. Leaving empty will build a tree of all pages.", 0, @"", "355F8C83-F8BF-4F7F-B2E6-D9BE4A7FB22C" );

            // DT: Update BlockType Defined Value List Liquid
            Sql( @"
    UPDATE [BlockType] SET
        [Path] = '~/Blocks/Core/DefinedValueListLava.ascx',
        [Name] = 'Defined Value List Lava',
        [Description] = 'Takes a defined type and returns all defined values and merges them with a lava template.'
    WHERE [Guid] = 'C4ADDDFA-DF16-467E-9285-B1FF0FC066ED'
" );

            // JE: Fix Ad Detail to use lava file not DB template
            Sql( @"
    DECLARE @BlockId int = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '7173AA95-15AF-49C5-933D-004717A3FF3C' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8' )

    IF @BlockId IS NOT NULL AND @AttributeId IS NOT NULL 
    BEGIN
        DECLARE @Value varchar(max) =  ( SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId )
		IF @Value = '
{% for item in Items %}
    {% if item.DetailImage_unformatted != '''' %}
        <img src=""/GetImage.ashx?Guid={{ item.DetailImage_unformatted }}"" class=""title-image"">
    {% endif %}
    <h1>{{ item.Title }}</h1>{{ item.Content }}
{% endfor -%}
'		
        BEGIN
			UPDATE [AttributeValue] 
			SET [Value] = '{% include ''~~/Assets/Lava/AdDetails.lava'' %}'
			WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId 
		END
	END
" );

            // JE: Change Ability Levels to not be system
            Sql( @"
    UPDATE [DefinedValue]
    SET [IsSystem] = 0
    WHERE [DefinedTypeId] = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '7BEEF4D4-0860-4913-9A3D-857634D1BF7C')
" );

            // MP: Update Installer Paths
            // update new location of checkscanner installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.3.0/checkscanner.exe' 
    WHERE [Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180'
" );

            // update new location of jobscheduler installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/jobscheduler/1.3.0/jobscheduler.exe'
    WHERE [Guid] = '7FBC4397-6BFD-451D-A6B9-83D7B7265641'
" );

            // update new location of statementgenerator installer
            Sql( @"
    UPDATE [AttributeValue] 
    SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.3.0/statementgenerator.exe' 
    WHERE [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'
" );

            // TC: Change Attribute named "Last Saved Date" to "DISC Last Saved Date"
            Sql( @"
    UPDATE [Attribute]
    SET [Name] = 'DISC Last Save Date'
    WHERE [Guid] = '990275DB-611B-4D2E-94EA-3FFA1186A5E1'
" );

            // MP: Update spFinance_ContributionStatementQuery stored proc
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
	<param name=""StartDate"" datatype=""datetime"">The starting date of the date range</param>
	<param name=""EndDate"" datatype=""datetime"">The ending date of the date range</param>
	<param name=""AccountIds"" datatype=""varchar(max)"">Comma delimited list of account ids. NULL means all</param>
	<param name=""PersonId"" datatype=""int"">Person the statement if for. NULL means all persons that have transactions for the date range</param>
	<param name=""OrderByPostalCode"" datatype=""int"">Set to 1 to have the results sorted by PostalCode, 0 for no particular order</param>
	<remarks>	
		Uses the following constants:
			* Group Type - Family: 790E3215-3B10-442B-AF69-616C0DCB998E
			* Group Role - Adult: 2639F9A5-2AAE-4E48-A8C3-4FFE86681E42
			* Group Role - Child: C8B1814F-6AA7-4055-B2D7-48FE20429CB9
	</remarks>
	<code>
		EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, null, 1  -- year 2014 statements for all persons
        EXEC [dbo].[spFinance_ContributionStatementQuery] '01-01-2014', '01-01-2015', null, 2, 1  -- year 2014 statements for Ted Decker
	</code>
</doc>
*/
ALTER PROCEDURE [dbo].[spFinance_ContributionStatementQuery]
	@StartDate datetime
	, @EndDate datetime
	, @AccountIds varchar(max) 
	, @PersonId int -- NULL means all persons
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

	SELECT 
		  [pg].[PersonId]
		, [pg].[GroupId]
		, [pn].[PersonNames] [AddressPersonNames]
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
	
	ORDER BY
	CASE WHEN @OrderByPostalCode = 1 THEN PostalCode END
END
" );

            // NA: Fix typo (remove plural) on Global Attribute description
            Sql( @"
    UPDATE [Attribute] 
    SET [Description] = 'Should users be able to select a country when entering addresses?' 
    WHERE [Guid] = '14D56DC9-7A64-4210-97D9-BF1ABE409DC7' 
    AND [Description] = 'Should user''s be able to select a country when entering addresses?'
" );

            // DT: Update the Summary fields on Web Ads and Blogs channel types to allow HTML content
            RockMigrationHelper.AddAttributeQualifier( "AE455FB9-20FD-4767-9A65-4801B00FCD1A", "allowhtml", "True", "aeed0a26-19a2-0685-4c89-9faa0861ccf6" );
            RockMigrationHelper.AddAttributeQualifier( "35993D3B-57D3-4F41-88A5-A83EE380D2DD", "allowhtml", "True", "4a4d0663-1113-b989-4257-939524ce9963" );

            #endregion

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupTypeRole", "CanEdit");
            DropColumn("dbo.GroupTypeRole", "CanView");
        }
    }
}
