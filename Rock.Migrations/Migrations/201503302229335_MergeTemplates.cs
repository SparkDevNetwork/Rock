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
    public partial class MergeTemplates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.MergeTemplate",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    TemplateBinaryFileId = c.Int( nullable: false ),
                    MergeTemplateTypeEntityTypeId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                    PersonAliasId = c.Int(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Category", t => t.CategoryId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.MergeTemplateTypeEntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .ForeignKey( "dbo.BinaryFile", t => t.TemplateBinaryFileId )
                .Index( t => t.TemplateBinaryFileId )
                .Index( t => t.MergeTemplateTypeEntityTypeId )
                .Index( t => t.CategoryId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.EntitySetItem", "AdditionalMergeValuesJson", c => c.String() );

            RockMigrationHelper.UpdateBinaryFileType( Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE, "Merge Template", "Usually either a MS Word Docx or Html file", "fa fa-files-o", Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, false, true );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 0, "Edit", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, Model.SpecialRole.None, "D5D0FB59-993D-4B70-851C-18787AE8DE09" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 1, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Model.SpecialRole.None, "00615556-29EB-4085-A774-F1D203ECE9D0" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 2, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Model.SpecialRole.None, "E7300CED-B48F-4803-B27C-8851B2905668" );

            RockMigrationHelper.UpdateEntityType( "Rock.MergeTemplates.HtmlMergeTemplateType", "Html", "Rock.MergeTemplates.HtmlMergeTemplateType, Rock, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null", false, true, "5fbff041-9edc-41a3-8a92-d7ac4ff88221" );
            RockMigrationHelper.UpdateEntityType( "Rock.MergeTemplates.WordDocumentMergeTemplateType", "Word Document", "Rock.MergeTemplates.WordDocumentMergeTemplateType, Rock, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null", false, true, "7b86e093-3eb8-46ca-8ca7-068d699e7811" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.MergeTemplate", "Merge Template", "Rock.Model.MergeTemplate, Rock, Version=1.3.0.0, Culture=neutral, PublicKeyToken=null", true, true, Rock.SystemGuid.EntityType.MERGE_TEMPLATE );

            RockMigrationHelper.AddEntityAttribute( "Rock.MergeTemplates.HtmlMergeTemplateType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "E4A988D8-181A-417B-B13C-4FD69093D687" );
            RockMigrationHelper.AddAttributeValue( "E4A988D8-181A-417B-B13C-4FD69093D687", 0, "True", "CB5374AA-5BEF-4FA1-9801-128A4ADC3BF1" );
            
            RockMigrationHelper.AddEntityAttribute( "Rock.MergeTemplates.WordDocumentMergeTemplateType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "True", "5515CA06-7FA3-4D63-A3CF-B0AE9108CF33" );
            RockMigrationHelper.AddAttributeValue( "5515CA06-7FA3-4D63-A3CF-B0AE9108CF33", 0, "True", "A6F63120-4120-4116-9E4D-D82302A753D5" );

            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.MERGE_TEMPLATE, "Personal", "fa fa-user", "Personal Merge Templates", Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE, int.MaxValue );

            Sql( @"/*
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
	<remarks>
		[ufnCrm_GetFamilyTitle] is used by spFinance_ContributionStatementQuery as part of generating Contribution Statements
	</remarks>
	<code>
		SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](2, null, default) -- Single Person
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, default) -- Family
        SELECT * FROM [dbo].[ufnCrm_GetFamilyTitle](null, 44, '2,3') -- Family, limited to the specified PersonIds
	</code>
</doc>
*/
ALTER FUNCTION [dbo].[ufnCrm_GetFamilyTitle] (
    @PersonId INT
    ,@GroupId INT
    ,@GroupPersonIds VARCHAR(max) = NULL
    ,@UseNickName BIT = 0
    )
RETURNS @PersonNamesTable TABLE (PersonNames VARCHAR(max))
AS
BEGIN
    DECLARE @PersonNames VARCHAR(max);
    DECLARE @AdultLastNameCount INT;
    DECLARE @GroupFirstOrNickNames VARCHAR(max) = '';
    DECLARE @GroupLastName VARCHAR(max);
    DECLARE @GroupAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupNonAdultFullNames VARCHAR(max) = '';
    DECLARE @GroupMemberTable TABLE (
        LastName VARCHAR(max)
        ,FirstOrNickName VARCHAR(max)
        ,FullName VARCHAR(max)
        ,Gender INT
        ,GroupRoleGuid UNIQUEIDENTIFIER
        );
    DECLARE @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT UNIQUEIDENTIFIER = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42';

    IF (@PersonId IS NOT NULL)
    BEGIN
        -- just getting the Person Names portion of the address for an individual person
        SELECT @PersonNames = CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '')
        FROM [Person] [p]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        WHERE [p].[Id] = @PersonId;
    END
    ELSE
    BEGIN
        -- populate a table variable with the data we'll need for the different cases
        -- if GroupPersonIds is set (comma-delimited) only a subset of the family members should be combined
        INSERT INTO @GroupMemberTable
        SELECT [p].[LastName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END [FirstName]
            ,CASE @UseNickName
                WHEN 1
                    THEN ISNULL([p].[NickName], '')
                ELSE ISNULL([p].[FirstName], '')
                END + ' ' + ISNULL([p].[LastName], '') + ' ' + ISNULL([dv].[Value], '') [FullName]
            ,[p].Gender
            ,[gr].[Guid]
        FROM [GroupMember] [gm]
        JOIN [Person] [p] ON [p].[Id] = [gm].[PersonId]
        LEFT OUTER JOIN [DefinedValue] [dv] ON [dv].[Id] = [p].[SuffixValueId]
        JOIN [GroupTypeRole] [gr] ON [gm].[GroupRoleId] = [gr].[Id]
        WHERE [GroupId] = @GroupId
            AND (
                ISNULL(@GroupPersonIds, '') = ''
                OR (
                    p.[Id] IN (
                        SELECT *
                        FROM ufnUtility_CsvToTable(@GroupPersonIds)
                        )
                    )
                )

        -- determine adultCount and if we can use the same lastname for all adults, and get lastname while we are at it
        SELECT @AdultLastNameCount = count(DISTINCT [LastName])
            ,@GroupLastName = max([LastName])
        FROM @GroupMemberTable
        WHERE [GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT;

        IF @AdultLastNameCount > 0
        BEGIN
            -- get the FirstNames and Adult FullNames for use in the cases of families with Adults
            SELECT @GroupFirstOrNickNames = @GroupFirstOrNickNames + [FirstOrNickName] + ' & '
                ,@GroupAdultFullNames = @GroupAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable g
            WHERE g.[GroupRoleGuid] = @cGROUPTYPEROLE_FAMILY_MEMBER_ADULT
            ORDER BY g.Gender
                ,g.FirstOrNickName

            -- cleanup the trailing ' &'s
            IF len(@GroupFirstOrNickNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupFirstOrNickNames = SUBSTRING(@GroupFirstOrNickNames, 0, len(@GroupFirstOrNickNames) - 1)
            END

            IF len(@GroupAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupAdultFullNames = SUBSTRING(@GroupAdultFullNames, 0, len(@GroupAdultFullNames) - 1)
            END
        END

        IF @AdultLastNameCount = 0
        BEGIN
            -- get the NonAdultFullNames for use in the case of families without adults 
            SELECT @GroupNonAdultFullNames = @groupNonAdultFullNames + [FullName] + ' & '
            FROM @GroupMemberTable
            ORDER BY [FullName]

            IF len(@GroupNonAdultFullNames) > 2
            BEGIN
                -- trim the extra ' &' off the end 
                SET @GroupNonAdultFullNames = SUBSTRING(@GroupNonAdultFullNames, 0, len(@GroupNonAdultFullNames) - 1)
            END
        END

        IF (@AdultLastNameCount = 1)
        BEGIN
            -- just one lastname and at least one adult. Get the Person Names portion of the address in the format <MaleAdult> & <FemaleAdult> <LastName>
            SET @PersonNames = @GroupFirstOrNickNames + ' ' + @GroupLastName;
        END
        ELSE IF (@AdultLastNameCount = 0)
        BEGIN
            -- no adults in family, list all members of the family in 'Fullname & FullName & ...' format
            SET @PersonNames = @GroupNonAdultFullNames;
        END
        ELSE
        BEGIN
            -- multiple adult lastnames
            SET @PersonNames = @GroupAdultFullNames;
        END
    END

    INSERT INTO @PersonNamesTable ([PersonNames])
    VALUES (@PersonNames);

    RETURN
END
" );

            Sql( @"/*
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
		[ufnCrm_GetFamilyTitle]([pg].[PersonId], [pg].[GroupId], default, default) [pn]
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

            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Merge Templates", "", "679AF013-0093-435E-AA49-E73B99EB9710", "fa fa-files-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Merge Template Types", "", "42717D07-3744-4187-89EC-F01EDD0FF5AD", "fa fa-files-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "936C90C4-29CF-4665-A489-7C687217F7B8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Merge Template Entry", "", "B864FBFD-3699-4DB5-A8A9-12B2FEA86C7A", "" ); // Site:Rock RMS

            RockMigrationHelper.AddPageRoute( "B864FBFD-3699-4DB5-A8A9-12B2FEA86C7A", "MergeTemplate/{Set}" );
            RockMigrationHelper.UpdateBlockType( "Merge Template Detail", "Block for administrating a Merge Template", "~/Blocks/Core/MergeTemplateDetail.ascx", "Core", "820DE5F9-8391-4A2A-AA87-24156882BD5F" );
            RockMigrationHelper.UpdateBlockType( "Merge Template Entry", "Used for merging data into output documents, such as Word, Html, using a pre-defined template.", "~/Blocks/Core/MergeTemplateEntry.ascx", "Core", "8C6280DA-9BB4-47C8-96BA-3878B8B85466" );
            RockMigrationHelper.UpdateBlockType( "Person Update - Kiosk", "Block used to update a person's information from a kiosk.", "~/Blocks/Crm/PersonUpdate.Kiosk.ascx", "CRM", "61C5C8F2-6F76-4583-AB97-228878A6AB65" );
            // Add Block to Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( "679AF013-0093-435E-AA49-E73B99EB9710", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Merge Template Category Tree View", "Sidebar1", "", "", 0, "2F302012-6DEC-4963-B519-F8567CBAE67B" );

            // Add Block to Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( "679AF013-0093-435E-AA49-E73B99EB9710", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Merge Template Category Detail", "Main", "", "", 0, "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37" );

            // Add Block to Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( "679AF013-0093-435E-AA49-E73B99EB9710", "", "820DE5F9-8391-4A2A-AA87-24156882BD5F", "Merge Template Detail", "Main", "", "", 1, "72291AAD-E4B8-47CB-AA8E-28FFA2243FF5" );

            // Add Block to Page: Merge Template Types, Site: Rock RMS
            RockMigrationHelper.AddBlock( "42717D07-3744-4187-89EC-F01EDD0FF5AD", "", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Merge Template Types", "Main", "", "", 0, "4120986C-B653-4C10-B54F-542A3168D211" );

            // Add Block to Page: Merge Template Entry, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B864FBFD-3699-4DB5-A8A9-12B2FEA86C7A", "", "8C6280DA-9BB4-47C8-96BA-3878B8B85466", "Merge Template Entry", "Main", "", "", 0, "3CF8137C-23C8-472A-936A-EF8086ACB65C" );

            // Attrib for BlockType: Category Tree View:Exclude Categories
            RockMigrationHelper.AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Exclude Categories", "ExcludeCategories", "", "Select any category that you need to exclude from the tree view", 0, @"", "61398707-FCCE-4AFD-8374-110BCA275F34" );

            // Attrib for BlockType: Category Tree View:Root Category
            RockMigrationHelper.AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Root Category", "RootCategory", "", "Select the root category to use as a starting point for the tree view.", 0, @"", "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD" );

            // Attrib for BlockType: Merge Template Detail:Merge Templates Ownership
            RockMigrationHelper.AddBlockTypeAttribute( "820DE5F9-8391-4A2A-AA87-24156882BD5F", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "", "Set this to restrict if the merge template must be a Personal or Global merge template.", 0, @"Global", "97860FA4-F59F-473D-A456-550FA40B937C" );

            // Attrib for BlockType: Category Detail:Exclude Categories
            RockMigrationHelper.AddBlockTypeAttribute( "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Exclude Categories", "ExcludeCategories", "", "Select any category that you need to exclude from the parent category picker", 0, @"", "AB542995-876F-4B8F-8417-11D83369289E" );

            // Attrib for BlockType: Category Detail:Root Category
            RockMigrationHelper.AddBlockTypeAttribute( "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Root Category", "RootCategory", "", "Select the root category to use as a starting point for the parent category picker.", 0, @"", "C3B72ADF-93D7-42CF-A103-8A7661A6926B" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Entity Type Friendly Name Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "07213E2C-C239-47CA-A781-F7A908756DC2", @"" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Show Unnamed Entity Items Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Exclude Categories Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "61398707-FCCE-4AFD-8374-110BCA275F34", @"a9f2f544-660b-4176-acad-88898416a66e" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Root Category Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD", @"" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Entity Type Qualifier Property Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "2D5FF74A-D316-4924-BCD2-6AA338D8DAAC", @"" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Entity type Qualifier Value Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "F76C5EEF-FD45-4BD6-A903-ED5AB53BB928", @"" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Page Parameter Key Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "AA057D3E-00CC-42BD-9998-600873356EDB", @"MergeTemplateId" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Entity Type Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"cd1db988-6891-4b0f-8d1b-b0a311a3bc3e" );

            // Attrib Value for Block:Merge Template Category Tree View, Attribute:Detail Page Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2F302012-6DEC-4963-B519-F8567CBAE67B", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"679af013-0093-435e-aa49-e73b99eb9710" );

            // Attrib Value for Block:Merge Template Category Detail, Attribute:Exclude Categories Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37", "AB542995-876F-4B8F-8417-11D83369289E", @"a9f2f544-660b-4176-acad-88898416a66e" );

            // Attrib Value for Block:Merge Template Category Detail, Attribute:Root Category Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37", "C3B72ADF-93D7-42CF-A103-8A7661A6926B", @"" );

            // Attrib Value for Block:Merge Template Category Detail, Attribute:Entity Type Qualifier Property Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37", "620957FF-BC28-4A89-A74F-C917DA5CFD47", @"" );

            // Attrib Value for Block:Merge Template Category Detail, Attribute:Entity Type Qualifier Value Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37", "985128EE-D40C-4598-B14B-7AD728ECCE38", @"" );

            // Attrib Value for Block:Merge Template Category Detail, Attribute:Entity Type Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", @"cd1db988-6891-4b0f-8d1b-b0a311a3bc3e" );

            // Attrib Value for Block:Merge Template Detail, Attribute:Merge Templates Ownership Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72291AAD-E4B8-47CB-AA8E-28FFA2243FF5", "97860FA4-F59F-473D-A456-550FA40B937C", @"0" );

            // Attrib Value for Block:Merge Template Types, Attribute:Support Ordering Page: Merge Template Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4120986C-B653-4C10-B54F-542A3168D211", "A4889D7B-87AA-419D-846C-3E618E79D875", @"True" );

            // Attrib Value for Block:Merge Template Types, Attribute:Component Container Page: Merge Template Types, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4120986C-B653-4C10-B54F-542A3168D211", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.MergeTemplates.MergeTemplateTypeContainer, Rock" );


            
            //// Personal Merge Template Pages and Blocks

            RockMigrationHelper.AddPage( "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Merge Templates", "", "23F81A62-617A-498B-AAAC-D748F721176A", "fa fa-files-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "23F81A62-617A-498B-AAAC-D748F721176A", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Merge Template Detail", "", "F29C7AF7-6436-4C4B-BD17-330A487A4BF4", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Merge Template List", "Displays a list of all merge templates.", "~/Blocks/Core/MergeTemplateList.ascx", "Core", "DA102F02-6DBB-42E6-BFEE-360E137B1411" );
            // Add Block to Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlock( "23F81A62-617A-498B-AAAC-D748F721176A", "", "DA102F02-6DBB-42E6-BFEE-360E137B1411", "Merge Template List", "Main", "", "", 0, "CCB05F67-1BB6-4443-B4ED-3E7431D2B489" );

            // Add Block to Page: Merge Template Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "F29C7AF7-6436-4C4B-BD17-330A487A4BF4", "", "820DE5F9-8391-4A2A-AA87-24156882BD5F", "Merge Template Detail", "Main", "", "", 0, "73C7AF82-38D0-4C90-AD55-B8C38D751190" );
            
            // Attrib for BlockType: Merge Template List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "DA102F02-6DBB-42E6-BFEE-360E137B1411", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "21DA6B32-216E-4E03-AB54-5F9CD9978975" );

            // Attrib for BlockType: Merge Template List:Merge Templates Ownership
            RockMigrationHelper.AddBlockTypeAttribute( "DA102F02-6DBB-42E6-BFEE-360E137B1411", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "", "Set this to limit to merge templates depending on ownership type", 0, @"Personal", "85C47082-F88D-46DB-9858-52FF064ED744" );

            // Attrib Value for Block:Merge Template List, Attribute:Detail Page Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CCB05F67-1BB6-4443-B4ED-3E7431D2B489", "21DA6B32-216E-4E03-AB54-5F9CD9978975", @"f29c7af7-6436-4c4b-bd17-330a487a4bf4" );

            // Attrib Value for Block:Merge Template List, Attribute:Merge Templates Ownership Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CCB05F67-1BB6-4443-B4ED-3E7431D2B489", "85C47082-F88D-46DB-9858-52FF064ED744", @"1" );

            // Attrib Value for Block:Merge Template Detail, Attribute:Merge Templates Ownership Page: Merge Template Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "73C7AF82-38D0-4C90-AD55-B8C38D751190", "97860FA4-F59F-473D-A456-550FA40B937C", @"1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.MergeTemplate", "TemplateBinaryFileId", "dbo.BinaryFile" );
            DropForeignKey( "dbo.MergeTemplate", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MergeTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MergeTemplate", "MergeTemplateTypeEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.MergeTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.MergeTemplate", "CategoryId", "dbo.Category" );
            DropIndex( "dbo.MergeTemplate", new[] { "Guid" } );
            DropIndex( "dbo.MergeTemplate", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.MergeTemplate", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.MergeTemplate", new[] { "PersonAliasId" } );
            DropIndex( "dbo.MergeTemplate", new[] { "CategoryId" } );
            DropIndex( "dbo.MergeTemplate", new[] { "MergeTemplateTypeEntityTypeId" } );
            DropIndex( "dbo.MergeTemplate", new[] { "TemplateBinaryFileId" } );
            DropColumn( "dbo.EntitySetItem", "AdditionalMergeValuesJson" );
            DropTable( "dbo.MergeTemplate" );

            // Attrib for BlockType: Category Detail:Root Category
            RockMigrationHelper.DeleteAttribute( "C3B72ADF-93D7-42CF-A103-8A7661A6926B" );
            // Attrib for BlockType: Category Detail:Exclude Categories
            RockMigrationHelper.DeleteAttribute( "AB542995-876F-4B8F-8417-11D83369289E" );
            // Attrib for BlockType: Merge Template Detail:Merge Templates Ownership
            RockMigrationHelper.DeleteAttribute( "97860FA4-F59F-473D-A456-550FA40B937C" );
            // Attrib for BlockType: Category Tree View:Root Category
            RockMigrationHelper.DeleteAttribute( "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD" );
            // Attrib for BlockType: Category Tree View:Exclude Categories
            RockMigrationHelper.DeleteAttribute( "61398707-FCCE-4AFD-8374-110BCA275F34" );
            // Remove Block: Merge Template Entry, from Page: Merge Template Entry, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3CF8137C-23C8-472A-936A-EF8086ACB65C" );
            // Remove Block: Merge Template Types, from Page: Merge Template Types, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "4120986C-B653-4C10-B54F-542A3168D211" );
            // Remove Block: Merge Template Detail, from Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "72291AAD-E4B8-47CB-AA8E-28FFA2243FF5" );
            // Remove Block: Merge Template Category Detail, from Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "EC3C1F43-260F-44DC-B0FC-9C6CA9F64F37" );
            // Remove Block: Merge Template Category Tree View, from Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2F302012-6DEC-4963-B519-F8567CBAE67B" );
            RockMigrationHelper.DeleteBlockType( "8C6280DA-9BB4-47C8-96BA-3878B8B85466" ); // Merge Template Entry
            RockMigrationHelper.DeleteBlockType( "820DE5F9-8391-4A2A-AA87-24156882BD5F" ); // Merge Template Detail
            RockMigrationHelper.DeletePage( "B864FBFD-3699-4DB5-A8A9-12B2FEA86C7A" ); //  Page: Merge Template Entry, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "42717D07-3744-4187-89EC-F01EDD0FF5AD" ); //  Page: Merge Template Types, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "679AF013-0093-435E-AA49-E73B99EB9710" ); //  Page: Merge Templates, Layout: Left Sidebar, Site: Rock RMS

            //// Personal Merge Template Pages and Blocks

            // Attrib for BlockType: Merge Template List:Merge Templates Ownership
            RockMigrationHelper.DeleteAttribute( "85C47082-F88D-46DB-9858-52FF064ED744" );
            // Attrib for BlockType: Merge Template List:Detail Page
            RockMigrationHelper.DeleteAttribute( "21DA6B32-216E-4E03-AB54-5F9CD9978975" );
            
            // Remove Block: Merge Template Detail, from Page: Merge Template Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "73C7AF82-38D0-4C90-AD55-B8C38D751190" );
            // Remove Block: Merge Template List, from Page: Merge Templates, Site: Rock RMS
            RockMigrationHelper.DeleteBlockType( "DA102F02-6DBB-42E6-BFEE-360E137B1411" ); // Merge Template List
            RockMigrationHelper.DeletePage( "F29C7AF7-6436-4C4B-BD17-330A487A4BF4" ); //  Page: Merge Template Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "23F81A62-617A-498B-AAAC-D748F721176A" ); //  Page: Merge Templates, Layout: Full Width, Site: Rock RMS
            
        }
    }
}
