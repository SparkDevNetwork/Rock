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

using Rock.Model;


namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 212, "1.16.6" )]
    public class PrayerCategoryPageBlockSettings : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            PrayerAutomationCompletionsUp();
            CategoryDetailAttributes();
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

        /// <summary>
        /// Updates the PrayerRequest AI Completion Templates to include instructions
        /// to not edit any text when no text enhancement is requested, but name removal is.
        /// </summary>
        /// <returns>The prompts to use for Prayer Request AI completions.</returns>
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
{%- if EnableFixFormattingAndSpelling == false and EnableEnhancedReadability == false %}
Do not modify any other text such as spelling mistakes or grammatical errors.
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

        /// <summary>
        /// Ensures the Category Detail and Category Tree View blocks have
        /// the proper block settings on the Prayer Request Categories Page.
        /// </summary>
        private void CategoryDetailAttributes()
        {
            Sql( $@"
DECLARE @prayerRequestEntityTypeGuid NVARCHAR(40) = '{SystemGuid.EntityType.PRAYER_REQUEST}'
DECLARE @blockEntityTypeId INT = (SELECT [Id] FROM [dbo].[EntityType] WHERE [Guid] = '{SystemGuid.EntityType.BLOCK}');
DECLARE @prayerCategoriesPageGuid NVARCHAR(40) = '{SystemGuid.Page.PRAYER_CATEGORIES}';
DECLARE @categoryDetailBlockTypeGuid NVARCHAR(40) = '{SystemGuid.BlockType.OBSIDIAN_CATEGORY_DETAIL}';

-- Get the AttributeId of the EntityType block setting for the Obsidian CategoryDetail block.
DECLARE @entityTypeAttributedId INT = (
	SELECT TOP 1 a.[Id]
	FROM [dbo].[block] b
	JOIN [dbo].[BlockType] bt ON bt.Id = b.BlockTypeId
	JOIN [dbo].[Page] p ON p.Id = b.PageId
	JOIN [dbo].[Attribute] a ON a.EntityTypeId = @blockEntityTypeId
		AND a.[key] = 'EntityType'
		AND a.EntityTypeQualifierColumn = 'BlockTypeId'
		AND a.EntityTypeQualifierValue = CONVERT(VARCHAR(50), bt.Id)
	WHERE bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Get the Id of the Category Detail block on the Prayer Categories page.
DECLARE @categoryDetailBlockId INT = (
	SELECT TOP 1 b.[Id]
	from [Page] p
	JOIN [Block] b ON b.[PageId] = p.[Id]
	JOIN [BlockType] bt ON bt.[Id] = b.BlockTypeId
	WHERE p.[Guid] = @prayerCategoriesPageGuid
		AND bt.[Guid] = @categoryDetailBlockTypeGuid
);

-- Update the Value of the EntityType attribute to use the GUID if it's numeric.
UPDATE av SET
	[Value] = @prayerRequestEntityTypeGuid
FROM [dbo].[AttributeValue] av
WHERE av.AttributeId = @entityTypeAttributedId
	AND ISNUMERIC(av.[Value]) = 1

DECLARE @now DATETIMEOFFSET = SYSDATETIMEOFFSET();

-- Add AttributeValues for the Category Detail block instance.
INSERT [AttributeValue] (
	[IsSystem],
	[AttributeId],
	[EntityId],
	[Value],
	[Guid],
	[IsPersistedValueDirty],
	[CreatedDateTime],
	[ModifiedDateTime]
)
SELECT 0, @entityTypeAttributedId, @categoryDetailBlockId, @prayerRequestEntityTypeGuid, NEWID(), 1, @now, @now
WHERE NOT EXISTS (
	    SELECT *
	    FROM [dbo].[AttributeValue] ex
	    WHERE ex.[AttributeId] = @entityTypeAttributedId
		    AND ex.[EntityId] = @categoryDetailBlockId
)
" );
        }

        #endregion
    }
}
