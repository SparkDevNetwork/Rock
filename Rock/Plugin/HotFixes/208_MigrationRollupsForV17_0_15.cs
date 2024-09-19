// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 208, "1.16.4" )]
    public class MigrationRollupsForV17_0_15 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            PrayerAutomationCompletionsUp();
            UpdateGivingHouseholdsMetricsUp();
            AddExceptionLogFilterGlobalAttributeUp();
            AddModalLayoutUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            
        }

        #region JC: Prayer Automation Completions

        private void PrayerAutomationCompletionsUp()
        {
            Rock.Web.SystemSettings.SetValue( "core_PrayerRequestAICompletions", PrayerRequestAICompletionTemplate().ToJson() );
        }

        private PrayerRequest.PrayerRequestAICompletions PrayerRequestAICompletionTemplate()
        {
            return new PrayerRequest.PrayerRequestAICompletions
            {
                PrayerRequestFormatterTemplate = @"
{%- comment -%}
This is the lava template for the Text formatting AI automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:

PrayerRequest - The PrayerRequest entity object.
EnableFixFormattingAndSpelling - True if the AI Automation AttributeValue for TextEnhancement is equal to MinorFormattingAndSpelling; otherwise false.
EnableEnhancedReadability - True if the AI Automation AttributeValue for TextEnhancement is equal to EnhanceReadability; otherwise false.    
EnableRemovalLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to LastNamesOnly; otherwise false.
EnableRemovalFirstAndLastNames - True if the AI Automation AttributeValue for NameRemoval is equal to FirstAndLastNames; otherwise false.
{%- endcomment -%}

Refer to the Prayer Request below, delimited by ```Prayer Request```. Return only the modified text without any additional comments.

{%- if EnableRemovalLastNames == true and EnableRemovalFirstAndLastNames == false %}
Remove surname and family names, but leave first names in their original form from the text below.
{%- endif -%}
{%- if EnableRemovalFirstAndLastNames == true %}
Remove names, both first and last, from the text below. If the text uses a pronoun or possessive pronoun continue to use that; otherwise use generic words like: ""an individual"", ""some individuals"", ""a family"" etc.
{%- endif -%}
{%- if EnableFixFormattingAndSpelling == true and EnableEnhancedReadability == false %}
Fix any formatting and spelling mistakes, but do not change the text.
{% endif -%}
{%- if EnableEnhancedReadability == true %}
Make the request more readable and polished. Do not change words if they significantly alter the perceived meaning.
If the request is not in English and a translation is included - leave the translation in it's original form.
{% endif -%}

```Prayer Request```
{{ PrayerRequest.Text }}
```Prayer Request```
",
                PrayerRequestAnalyzerTemplate = @"
{%- comment -%}
This is the lava template for the AI analysis automation that occurs in the PrayerRequest PostSave SaveHook.
Available Lava Fields:
PrayerRequest - The PrayerRequest entity object.
ParentCategoryId - The integer identifier of the parent category (for the PrayerRequest).
AutoCategorize - True if the AI automation is configured to auto-categorize the prayer request.
ClassifySentiment - True if the AI automation is configured to classify the sentiment of the prayer request.
CheckAppropriateness - True if the AI automation is configured to check if prayer request is appropriate for public viewing.
Categories - The child categories of the currently selected Prayer Request category or children of ""All Church"" if no category is selected.
SentimentEmotions - The Sentiment Emotions DefinedType with DefinedValues from Cache (well-known Guid: C9751C20-DA81-4521-81DE-0099D6F598BA).
{%- endcomment -%}
{%- if AutoCategorize == true and Categories != empty %}
Choose the Id of the category that most closely matches the main theme of the prayer request.
{%- assign categoriesJson = '[' -%}
{%- for category in Categories -%}
    {%- capture categoriesJsonRow -%}
        {
            ""Id"": {{ category.Id }},
            ""CategoryName"": {{category.Name | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign categoriesJson = categoriesJson | Append:categoriesJsonRow -%}
{%- endfor -%}
{%- assign categoriesJson = categoriesJson | Append: ']' %}
```Categories```
{{ categoriesJson | FromJSON | ToJSON }}
```Categories```
{% endif -%}

{%- if ClassifySentiment == true %}
Choose the Id of the sentiment that most closely matches the prayer request text.
{%- assign sentimentsJson = '[' -%}
{%- for definedValue in SentimentEmotions.DefinedValues -%}
    {%- capture sentimentsJsonRow -%}
        {
            ""Id"": {{ definedValue.Id }},
            ""Sentiment"": {{ definedValue.Value | ToJSON }}
        }{% unless forloop.last %},{% endunless %}
    {%- endcapture -%}
    {%- assign sentimentsJson = sentimentsJson | Append:sentimentsJsonRow -%}
{%- endfor -%}
{%- assign sentimentsJson = sentimentsJson | Append: ']' %}
```Sentiments```
{{ sentimentsJson | FromJSON | ToJSON }}
```Sentiments```
{% endif -%}
{%- if CheckAppropriateness == true -%}
Determine if the prayer request text is appropriate for public viewing being sensitive to privacy and legal concerns.
First names alone are ok, but pay attention to other details which might make it easy to uniquely identify an individual within a community.
{%- endif %}

```Prayer Request```
{{PrayerRequest.Text}}
```Prayer Request```
Respond with ONLY a VALID JSON object in the format below. Do not use backticks ```.
{
""sentimentId"": <The Id of the Sentiment from the list delimited by ```Sentiments``` that most closely matches the main theme of the prayer request text>,
""categoryId"": <The Id of the Category from the list delimited by ```Categories``` that most closely matches the main theme of the prayer request text>,
""isAppropriateForPublic"": <boolean value indicating whether the prayer request text is appropriate for public viewing>
}
"
            };
        }

        #endregion

        #region: KA Update Giving Households Metric

        private void UpdateGivingHouseholdsMetricsUp()
        {
            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
	[PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= 0 THEN 1 ELSE 0 END) AS [TotalGivingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY PrimaryCampusId;'
WHERE [Guid] = 'B5BFAB51-9B46-4E7E-992E-B0119E4D25EC'
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT
    SUM(CASE WHEN [GivingAmount] >= [FamiliesMedianTithe] THEN 1 ELSE 0 END) AS [TotalTithingHouseholds]
    , PrimaryCampusId AS [CampusId]
FROM CTE
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY PrimaryCampusId;'
WHERE [Guid] = '2B798177-E8F4-46DB-A1D7-308D63CA519A'
" );

            Sql( @"
UPDATE [dbo].[Metric]
SET [SourceSql] = 'DECLARE @StartDate int = FORMAT( DATEADD( d, -365, GETDATE()), ''yyyyMMdd'' )
DECLARE @EndDate int = FORMAT( GETDATE(), ''yyyyMMdd'' )
-- Only Include Person Type Records
DECLARE @PersonRecordTypeId INT = ( SELECT [Id] FROM [dbo].[DefinedValue] WHERE [Guid] = ''36CF10D6-C695-413D-8E7C-4546EFEF385E'' )

;WITH CTE AS (
    SELECT
    [PrimaryCampusId]
    , [PrimaryFamilyId]
    , SUM(ftd.[Amount]) AS [GivingAmount]
    , (SELECT TOP 1 
            LEFT([PostalCode], 5)
        FROM [dbo].[Location] l
        INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1
      ) AS [PostalCode]
    , (SELECT [FamiliesMedianIncome] * .1 FROM [dbo].[AnalyticsSourcePostalCode] WHERE [PostalCode] = (SELECT TOP 1 LEFT([PostalCode], 5) FROM [dbo].[Location] l INNER JOIN [dbo].[GroupLocation] gl ON gl.[LocationId] = l.[Id] AND gl.[GroupId] = [PrimaryFamilyId] AND gl.[IsMappedLocation] = 1) ) AS [FamiliesMedianTithe]
	FROM
		[Person] p
		INNER JOIN [dbo].[PersonAlias] pa ON pa.[PersonId] = p.[Id]
		INNER JOIN [dbo].[FinancialTransaction] ft ON ft.[AuthorizedPersonAliasId] = pa.[Id]
		INNER JOIN [dbo].[FinancialTransactionDetail] ftd ON ftd.[TransactionId] = ft.[Id]
		INNER JOIN [dbo].[FinancialAccount] fa ON fa.[Id] = ftd.[AccountId] 
		INNER JOIN [dbo].[AnalyticsSourceDate] asd ON asd.[DateKey] = ft.[TransactionDateKey]
	WHERE
		fa.[IsTaxDeductible] = 1
		AND asd.[DateKey] > = @StartDate AND asd.[DateKey] <= @EndDate
-- Only include person type records.
AND p.[RecordTypeValueId] = @PersonRecordTypeId
	GROUP BY [PrimaryCampusId], [PrimaryFamilyId]
)
SELECT 
    CAST(COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FLOAT) / COUNT(*) * 100 AS PercentageAboveMedianTithe,
    [PrimaryCampusId],
    COUNT(*) AS TotalFamilies,
    COUNT(CASE WHEN [GivingAmount] > [FamiliesMedianTithe] THEN 1 END) AS FamiliesAboveMedianTithe
FROM 
    CTE
-- Only include families that have a postal code and/or we have a [FamiliesMedianIncome] value
WHERE (PostalCode IS NOT NULL AND PostalCode != '''') and ([FamiliesMedianTithe] is NOT NULL and [FamiliesMedianTithe] > 0)
GROUP BY [PrimaryCampusId];'
WHERE [Guid] = 'F4951A42-9F71-4CB1-A46E-2A7ED84CD923'
" );
        }

        #endregion

        #region PA: Added Exception Log Filter Global Attribute

        private void AddExceptionLogFilterGlobalAttributeUp()
        {
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "", string.Empty, "Exception Log Filter", "Before logging an unhandled exception, Rock can evaluate the current client's HTTP Server variables and ignore/skip any that are from clients that have server variable values containing the values configured here. (Example: key: HTTP_USER_AGENT value: Googlebot)", 0, "", Guid.NewGuid().ToString(),
                Rock.SystemKey.GlobalAttributeKey.EXCEPTION_LOG_FILTER, false );
        }

        #endregion

        #region PA: Added Modal Layout and move Shortlink Block Obsidian to it

        private void AddModalLayoutUp()
        {
            RockMigrationHelper.AddLayout( SystemGuid.Site.SITE_ROCK_INTERNAL, "Modal", "Modal", "", SystemGuid.Layout.MODAL );

            RockMigrationHelper.UpdatePageLayout( "A9188D7A-80D9-4865-9C77-9F90E992B65C", SystemGuid.Layout.MODAL );
        }

        #endregion

        #region PA: Register block attributes for chop job in v1.17.0.28

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        private void RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.LocationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.LocationList", "Location List", "Rock.Blocks.Core.LocationList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "62622112-8375-44CF-957B-2B8FB4922C2B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupRequirementTypeList", "Group Requirement Type List", "Rock.Blocks.Group.GroupRequirementTypeList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "C1D4FEC2-F868-4FE7-899F-62CCFBEB29C6" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupRequirementTypeDetail", "Group Requirement Type Detail", "Rock.Blocks.Group.GroupRequirementTypeDetail, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "8A95BCF0-63CB-4CD6-99C9-E812D9AFAE99" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Crm.SignalTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Crm.SignalTypeDetail", "Signal Type Detail", "Rock.Blocks.Crm.SignalTypeDetail, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "E7B94691-BB91-4995-B2A0-3C3724224250" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.FollowingSuggestionTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.FollowingSuggestionTypeList", "Following Suggestion Type List", "Rock.Blocks.Core.FollowingSuggestionTypeList, Rock.Blocks, Version=1.17.0.27, Culture=neutral, PublicKeyToken=null", false, false, "BD0594CB-7A1B-40D2-A3C7-D27CB7481511" );


            // Add/Update Obsidian Block Type
            //   Name:Location List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.LocationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Location List", "Displays a list of locations.", "Rock.Blocks.Core.LocationList", "Core", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" );

            // Add/Update Obsidian Block Type
            //   Name:Group Requirement Type List
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Requirement Type List", "List of Group Requirement Types.", "Rock.Blocks.Group.GroupRequirementTypeList", "Group", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" );

            // Add/Update Obsidian Block Type
            //   Name:Group Requirement Type Detail
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupRequirementTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Requirement Type Detail", "Displays the details of the given group requirement type for editing.", "Rock.Blocks.Group.GroupRequirementTypeDetail", "Group", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" );

            // Add/Update Obsidian Block Type
            //   Name:Person Signal Type Detail
            //   Category:CRM
            //   EntityType:Rock.Blocks.Crm.SignalTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Person Signal Type Detail", "Shows the details of a particular person signal type.", "Rock.Blocks.Crm.SignalTypeDetail", "CRM", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" );

            // Add/Update Obsidian Block Type
            //   Name:Suggestion List
            //   Category:Follow
            //   EntityType:Rock.Blocks.Core.FollowingSuggestionTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Suggestion List", "Block for viewing list of following events.", "Rock.Blocks.Core.FollowingSuggestionTypeList", "Follow", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" );


            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the location details.", 0, @"", "EF5E30A6-8855-471F-8B82-25353D65C56A" );

            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "528C7391-5B8B-4BBA-8A5E-CAC5D3153FE0" );

            // Attribute for BlockType
            //   BlockType: Location List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "EC607613-E22D-4DB0-B5C5-C9107D9F4A37" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the group requirement type details.", 0, @"", "70790688-B494-4D58-92FC-2727BFA48A51" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6FECF863-2F24-4E6E-9D09-246AC035A752" );

            // Attribute for BlockType
            //   BlockType: Group Requirement Type List
            //   Category: Group
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DA7834C6-C5C6-470B-B1C8-9AFA492151F8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1A9B8368-70D4-4E26-A803-555C293AC335" );
            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the following suggestion type details.", 0, @"", "E137AADA-0039-4A32-BCA6-093505BC521E" );

            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "6D5576D1-7465-4AD2-AC21-BB1983B380E8" );

            // Attribute for BlockType
            //   BlockType: Suggestion List
            //   Category: Follow
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "C01EEADC-F78F-4442-A1C6-E2AC53D2B049" );
        }

        // PA: Chop blocks for v1.17.0.28
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.28",
                blockTypeReplacements: new Dictionary<string, string> {
{ "08189564-1245-48F8-86CC-560F4DD48733", "D0203B97-5856-437E-8700-8846309F8EED" }, // Location Detail
{ "5144ED5B-89A9-4D77-B0E5-695070BE0C8E", "8A5AF4F4-32A2-426F-8363-57AC4F02A6F6" }, // Location List
{ "92E4BFE8-DF80-49D7-819D-417E579E282D", "C0CFDAB7-BB29-499E-BD0A-468B0856C037" }, // Registration List Lava
{ "1270E3F7-5ACB-4044-94CD-E2B4368FF391", "DA7834C6-C5C6-470B-B1C8-9AFA492151F8" }, // Group Requirement Type List
{ "68FC983E-05F0-4067-83AC-97DD226F5071", "C17B6D03-FDF3-4DD7-B9A9-3D6159A838F5" }, // Group Requirement Type Detail
{ "E9AB79D9-429F-410D-B4A8-327829FC7C63", "8B65EE51-4075-4FC0-B1A9-F56C7153AA77" }, // Person Signal Type Detail
{ "23818F47-D81E-4B6E-B89B-045B1FAD4C2B", "0C01496C-B4FD-4335-A11F-9B3D14D3C0E1" }, // Suggestion List

                    // blocks chopped in v1.17.0.27
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "92E4BFE8-DF80-49D7-819D-417E579E282D", "LimitToOwed,MaxResults" }
            } );
        }

        #endregion

        #region PA: Swap blocks for v1.17.0.28

        private void SwapBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                    "Swap Block Types - 1.17.0.28",
                    blockTypeReplacements: new Dictionary<string, string> {
    { "37D43C21-1A4D-4B13-9555-EF0B7304EB8A", "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" }, // Group Scheduler
                    },
                    migrationStrategy: "Swap",
                    jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                    blockAttributeKeysToIgnore: new Dictionary<string, string>{
    { "37D43C21-1A4D-4B13-9555-EF0B7304EB8A",  "FutureWeeksToShow" }
                } );
        }

        #endregion
    }
}
