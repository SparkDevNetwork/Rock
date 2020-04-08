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
