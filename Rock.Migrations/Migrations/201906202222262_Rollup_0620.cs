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
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_0620 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddMotivatorsEngagement();
            MoveLegacyDiscAttributesToPADCategory();
            CopyLegacyDiscPersonalityTypeToAssessmentDiscScore();
            UpdateDiscProfileAttributeCategory();
            UdpateLegacyBlockName();
            CalendarAttributeStyling();
            RemoveInvalidFirstNamesFromMetaFirstNameGenderLookup();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        private void AddMotivatorsEngagement()
        {
            var categories = new System.Collections.Generic.List<string> { "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" };
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( SystemGuid.FieldType.DECIMAL, categories, "Motivator Engaging", "Motivator Engaging", "core_MotivatorEngaging", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
            AddDenyToAllSecurityToAttribute( SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
        }

        /// <summary>
        /// Adds the deny to all security to attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddDenyToAllSecurityToAttribute( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                System.Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                System.Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// Move three Legacy DISC attributes to the new, Personality Assessment Data category
        /// </summary>
        private void MoveLegacyDiscAttributesToPADCategory()
        {
            Sql( @"
                DECLARE @DISCCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = '0B187C81-2106-4875-82B6-FBF1277AE23B' ) -- DISC
                DECLARE @HiddenCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = 'B08A3096-FCFA-4DA0-B95D-1F3F11CC9969' ) -- Personality Assessment Data

                DECLARE @LastSaveDateAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '990275DB-611B-4D2E-94EA-3FFA1186A5E1' ) -- key LastSaveDate
                DECLARE @LastDiscRequestDateAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '41B73365-A984-879E-4749-7DB4FC720138' ) -- key LastDiscRequestDate
                DECLARE @PersonalityTypeAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = 'C7B529C6-B6C8-45B5-B892-5D9821CEDDCD' ) -- key PersonalityType

                UPDATE [AttributeCategory]
                SET [CategoryId] = @HiddenCategoryId
                WHERE [AttributeId] IN ( @LastSaveDateAttributeId, @LastDiscRequestDateAttributeId, @PersonalityTypeAttributeId )
	                AND [CategoryId] = @DISCCategoryId" );
        }

        private void UpdateDiscProfileAttributeCategory()
        {
            Sql( @"
                DECLARE @DISCCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = '0B187C81-2106-4875-82B6-FBF1277AE23B' ) -- DISC
                DECLARE @HiddenCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = 'B08A3096-FCFA-4DA0-B95D-1F3F11CC9969' ) -- Personality Assessment Data
                DECLARE @DiscProfileAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D' ) -- key LastSaveDate

                UPDATE [AttributeCategory]
                SET [CategoryId] = @DISCCategoryID
                WHERE [AttributeId] = @DiscProfileAttributeId
	                AND [CategoryId] = @HiddenCategoryId" );
        }

        /// <summary>
        /// Copy values from old PersonalityType to new core_DISCDISCProfile type attribute
        /// </summary>
        private void CopyLegacyDiscPersonalityTypeToAssessmentDiscScore()
        {
            Sql( @"
                DECLARE @DefinedTypeId INT = ( SELECT TOP 1 ID FROM [DefinedType] WHERE [Guid] = 'F06DDAD8-6058-4182-AD0A-B523BB7A2D78' )
                DECLARE @OldDISCPersonalityAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = 'C7B529C6-B6C8-45B5-B892-5D9821CEDDCD' ) -- key PersonalityType
                DECLARE @NewDISCPersonalityAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D' ) -- key core_DISCDISCProfile

                -- Delete any blank values that exist
                DELETE FROM [AttributeValue] 
                WHERE [AttributeId] = @NewDISCPersonalityAttributeId
	                AND ( [Value] IS NULL OR [Value] = '')

                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                SELECT 0, @NewDISCPersonalityAttributeId, [EntityId], dv.[Guid], NEWID()
                FROM [AttributeValue] av
                -- join on the defined value table to find the one that matches... in order to get the DV's GUID
                INNER JOIN [DefinedValue] dv ON dv.[Value] = av.[Value] and dv.[DefinedTypeId] = @DefinedTypeId
                WHERE av.[AttributeId] = @OldDISCPersonalityAttributeId AND av.[Value] <> ''
                AND NOT EXISTS (
	                SELECT * FROM [AttributeValue] c
                    WHERE c.[AttributeId] = @NewDISCPersonalityAttributeId AND av.[EntityId] = c.[EntityId])" );
        }

        /// <summary>
        /// NA: Update legacy block name 'Page Xslt Transformation'
        /// </summary>
        private void UdpateLegacyBlockName()
        {
            Sql( @"
                DECLARE @BlockTypeId INT = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = 'CACB9D1A-A820-4587-986A-D66A69EE9948')
                UPDATE [Block] SET [Name] = 'Page Menu' WHERE [BlockTypeId] = @BlockTypeId AND [Name] = 'Page Xslt Transformation'" );
        }

        /// <summary>
        /// GJ: Calendar Attribute Styling
        /// </summary>
        private void CalendarAttributeStyling()
        {
            Sql( @"UPDATE [dbo].[Block]
                SET [PreHtml] = '<style>
    .panel-parent .panel-block .panel-heading { display: none; }
</style>
<div class=""panel panel-block panel-parent"" style=""margin-bottom: 15px;"">
<div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa-calendar""></i>
            Calendar Attributes
        </h1>
    </div>'
                WHERE [Guid] = N'F04979E2-33A2-4C0E-936E-5C8849BB98F4';

                UPDATE [dbo].[Block]
                SET [PreHtml] = '<div class=""panel panel-block panel-parent"" style=""margin-bottom: 15px;"">
    <div class=""panel-heading"">
        <h1 class=""panel-title"">
            <i class=""fa fa-fw fa fa-clock-o""></i>
            Event Occurrence Attributes
        </h1>
    </div>'
                WHERE [Guid] = N'DF4472A7-AC11-4245-B9D0-FBB8547B60B4';" );

        }

        /// <summary>
        /// JE: Remove errant firstnames from MetaFirstNameGenderLookup
        /// </summary>
        private void RemoveInvalidFirstNamesFromMetaFirstNameGenderLookup()
        {
            Sql( @"
                DELETE FROM [MetaFirstNameGenderLookup]
                WHERE [FirstName] IN ('Test', 'Test Kid', 'Family', 'Family:', 'Child')" );
        }

        /// <summary>
        /// GJ: Motivators Update
        /// </summary>
        private void MotivatorsUpdate()
        {
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<p>
   {{ Person.NickName }}, here are your motivators results. We’ve listed your Top 5 Motivators, your
   growth propensity score, along with a complete listing of all 22 motivators and your results
   for each.
</p>
<h2>Growth Propensity</h2>
<p>
    Growth Propensity measures your perceived mindset on a continuum between a growth mindset and
    fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.
</p>
<div style=""margin: 0 auto;max-width:40%"">
{[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100']}
    [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
{[ endchart ]}
</div>
<h2>Individual Motivators</h2>
<p>
    There are 22 possible motivators in this assessment. While your Top 5 Motivators may be most helpful in understanding your results in a snapshot, you may also find it helpful to see your scores on each for a complete picture.
</p>
<!--  Theme Chart -->
<div class=""panel panel-default"">
    <div class=""panel-heading"">
    <h2 class=""panel-title""><b>Composite Score</b></h2>
    </div>
    <div class=""panel-body"">
    {[chart type:'horizontalBar' chartheight:'200px' ]}
    {% for motivatorThemeScore in MotivatorThemeScores %}
        [[dataitem label:'{{ motivatorThemeScore.DefinedValue.Value }}' value:'{{ motivatorThemeScore.Value }}' fillcolor:'{{ motivatorThemeScore.DefinedValue | Attribute:'Color' }}' ]]
        [[enddataitem]]
    {% endfor %}
    {[endchart]}
    </div>
</div>
<p>
    This graph is based on the average composite score for each Motivator Theme.
</p>
{% for motivatorThemeScore in MotivatorThemeScores %}
    <p>
        <b>{{ motivatorThemeScore.DefinedValue.Value }}</b>
        </br>
        {{ motivatorThemeScore.DefinedValue.Description }}
        </br>
        {{ motivatorThemeScore.DefinedValue | Attribute:'Summary' }}
    </p>
{% endfor %}
<p>
   The following graph shows your motivators ranked from top to bottom.
</p>
  <div class=""panel panel-default"">
    <div class=""panel-heading"">
      <h2 class=""panel-title""><b>Ranked Motivators</b></h2>
    </div>
    <div class=""panel-body"">
      {[ chart type:'horizontalBar' ]}
        {% for motivatorScore in MotivatorScores %}
        {% assign theme = motivatorScore.DefinedValue | Attribute:'Theme' %}
            {% if theme and theme != empty %}
                [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]]
                [[enddataitem]]
            {% endif %}
        {% endfor %}
        {[endchart]}
    </div>
  </div>
", "BA51DFCD-B174-463F-AE3F-6EEE73DD9338" );
        }
    }
}
