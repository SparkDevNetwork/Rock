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

using System.Collections.Generic;

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 207, "1.16.4" )]
    public class MigrationRollupsForV17_0_14 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateHistoryTableIndexUp();
            PrayerAutomationCompletionsUp();
            StandardizePageShortlinkNamesUp();
            ChopBlocksUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            
        }

        #region JC: Update IX_EntityTypeId_EntityId Index

        private void UpdateHistoryTableIndexUp()
        {
            // This job can be run at any time and should improve performance of the HistoryLog web forms block.
            // By default it will run at 2 am unless manually run sooner by an administrator.
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.0 - History Index Update - Add Includes",
                description: "This job updates the IX_EntityTypeId_EntityId index on the dbo.History table to add includes for: " +
                "[RelatedEntityTypeId], [RelatedEntityId], [CategoryId], [CreatedByPersonAliasId], [CreatedDateTime].",
                jobType: "Rock.Jobs.PostUpdateJobs.PostV17UpdateHistoryTableEntityTypeIdIndexPostMigration",
                cronExpression: "0 0 2 ? * * *",
                guid: Rock.SystemGuid.ServiceJob.POST_170_UPDATE_HISTORY_ENTITYTYPEID_INDEX );
        }

        #endregion

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
%- comment -%}
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

        #region PA: Standardize names of Page Shortlink Blocks

        private void StandardizePageShortlinkNamesUp()
        {
            RockMigrationHelper.RenameEntityType( "026C6A93-5295-43E9-B67D-C3708ACB25B9",
                "Rock.Blocks.Cms.PageShortLinkDialog",
                "Short Link Detail",
                "Rock.Blocks.Cms.PageShortLinkDialog, Rock.Blocks, Version=1.16.6.6, Culture=neutral, PublicKeyToken=null",
                false,
                false );

            // Add/Update Obsidian Block Type
            //   Name:Shortened Links (dialog)
            //   Category:Administration
            //   EntityType:Rock.Blocks.Cms.ShortLink
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Page Short Link (dialog)", "Displays a dialog for adding a short link to the current page.", "Rock.Blocks.Cms.PageShortLinkDialog", "Administration", "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" );
        }

        #endregion

        #region PA: Chop blocks for v1.17.0.27

        // PA: Register block attributes for chop job in v1.17.0.27
        private void RegisterBlockAttributesForChop()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Finance.BenevolenceTypeDetail", "Benevolence Type Detail", "Rock.Blocks.Finance.BenevolenceTypeDetail, Rock.Blocks, Version=1.17.0.26, Culture=neutral, PublicKeyToken=null", false, false, "B39BA58D-83DD-46E0-BA47-787C4EB4EB69" );

            // Add/Update Obsidian Block Type
            //   Name:Benevolence Type Detail
            //   Category:Finance
            //   EntityType:Rock.Blocks.Finance.BenevolenceTypeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Benevolence Type Detail", "Block to display the benevolence type detail.", "Rock.Blocks.Finance.BenevolenceTypeDetail", "Finance", "03397615-EF2B-4D33-BD62-A79186F56ACE" );

            // Attribute for BlockType
            //   BlockType: Benevolence Type Detail
            //   Category: Finance
            //   Attribute: Benevolence Type Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "03397615-EF2B-4D33-BD62-A79186F56ACE", "99B090AA-4D7E-46D8-B393-BF945EA1BA8B", "Benevolence Type Attributes", "BenevolenceTypeAttributes", "Benevolence Type Attributes", @"The attributes that should be displayed / edited for benevolence types.", 1, @"", "2ACAFAC1-CF36-4BD4-A7E0-42FD771092E3" );
        }

        // PA: Chop blocks for v1.17.0.27
        private void ChopBlockTypesv17()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types - 1.17.0.27",
                blockTypeReplacements: new Dictionary<string, string> {
{ "052B84EA-0C34-4A07-AC4C-1FBCEC87C223", "E18AB976-6665-48A5-B418-8FAC8F374135" }, // Suggestion Detail
{ "14293AEB-B0F5-434B-844A-66592AE3A416", "7E2DFB55-F1AB-4452-A5DF-6CE65FBFDDAD" }, // Photo Opt-Out
{ "0DE16268-BD5B-4CFC-A7C6-F1E07F47527A", "F61C0FDF-E8A0-457A-B8AF-42CAC8A18718" }, // Benevolence Type List
{ "C96479B6-E309-4B1A-B024-1F1276122A13", "03397615-EF2B-4D33-BD62-A79186F56ACE" }, // Benevolence Type Detail
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: null );
        }

        private void ChopBlocksUp()
        {
            RegisterBlockAttributesForChop();
            ChopBlockTypesv17();
        }

        #endregion
    }
}
