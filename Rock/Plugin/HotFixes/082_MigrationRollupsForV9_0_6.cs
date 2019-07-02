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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 82, "1.9.0" )]
    public class MigrationRollupsForV9_0_6 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            FixIssue3497();
            MotivatorsUpdate();
            SegmentedChartUpdate();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// NA: Re-fix #3497
        /// </summary>
        private void FixIssue3497()
        {
                        // Re-Fixes #3497
            Sql( @"
                IF NOT EXISTS (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'b91ba046-bc1e-400c-b85d-638c1f4e0ce2' )
                BEGIN
                    SET IDENTITY_INSERT [dbo].[DefinedValue] ON
                    INSERT INTO [dbo].[DefinedValue] ([Id], [IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid], [IsActive] )
                    VALUES (66, 1, 4, 2, N'Visitor', N'Used when a person first enters through your first-time visitor process. As they continue to attend they will become an attendee and possibly a member.', N'b91ba046-bc1e-400c-b85d-638c1f4e0ce2', 1)
                    SET IDENTITY_INSERT [dbo].[DefinedValue] OFF
                END" );
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
<div style=""margin: 0;max-width:280px"">
{[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100' chartheight:'150px']}
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

        /// <summary>
        /// GJ: Segmented Chart Update
        /// </summary>
        private void SegmentedChartUpdate()
        {
            Sql( HotFixMigrationResource._082_MigrationRollupsForV9_0_6_SegmentedChartUpdate );
        }
    }
}
