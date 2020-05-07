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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 114, "1.10.0" )]
    public class MigrationRollupsFor10_3_3 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateGroupMemberForSystemCommunication();
            //UpdateAssessmentResultsLavaBarCharts();
            //RemovePagenamefromSystemCommunicationDetailBreadcrumbs();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: Create Similar Migration for SystemCommunication record too
        /// </summary>
        private void UpdateGroupMemberForSystemCommunication()
        {
            string newValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            string oldValue = "{%- assign mobilePhone = pendingIndividual.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            // Use NormalizeColumnCRLF when attempting to do a WHERE clause or REPLACE using multi line strings!
            var targetColumn = RockMigrationHelper.NormalizeColumnCRLF( "Body" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '18521B26-1C7D-E287-487D-97D176CA4986'" );

            newValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 12 | Select:'NumberFormatted' -%}".Replace( "'", "''" );
            oldValue = "{%- assign mobilePhone =absentMember.PhoneNumbers | Where:'NumberTypeValueId', 136 | Select:'NumberFormatted' -%}".Replace( "'", "''" );

            Sql( $@"UPDATE [dbo].[SystemCommunication] 
                    SET [Body] = REPLACE({targetColumn}, '{oldValue}', '{newValue}')
                    WHERE {targetColumn} LIKE '%{oldValue}%'
                            AND [Guid] = '8747131E-3EDA-4FB0-A484-C2D2BE3918BA'" );
        }

        /// <summary>
        /// JH: Update bar charts within Assessment results Lava to use zero for the x and y minimum values.
        /// </summary>
        private void UpdateAssessmentResultsLavaBarCharts()
        {
            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.CONFLICT_PROFILE, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<h2>Conflict Engagement Profile Results</h2>
<p>
   {{ Person.NickName }}, here are your conflict engagement results.
   You will rank high, medium or low in each of the following five modes.
</p>

{[ chart type:'bar' yaxismin:'0' ]}
    [[ dataitem label:'Winning' value:'{{Winning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
    [[ dataitem label:'Resolving' value:'{{Resolving}}' fillcolor:'#5585B7' ]] [[ enddataitem ]]
    [[ dataitem label:'Compromising' value:'{{Compromising}}' fillcolor:'#6399D1' ]] [[ enddataitem ]]
    [[ dataitem label:'Avoiding' value:'{{Avoiding}}' fillcolor:'#94DB84' ]] [[ enddataitem ]]
    [[ dataitem label:'Yielding' value:'{{Yielding}}' fillcolor:'#A1ED90' ]] [[ enddataitem ]]
{[ endchart ]}

<h3>Conflict Engagement Modes</h3>

<h4>Winning</h4>
<p>
    Winning means you prefer competing over cooperating. You believe you have the right answer and you desire to
  prove you are right, whatever it takes. This may include standing up for your own rights, beliefs or position.
</p>

<h4>Resolving</h4>
<p>
    Resolving means you attempt to work with the other person in depth to find the best solution, regardless of
    who appears to get the most immediate benefit. This involves digging beneath the presenting issue to find a
    solution that offers benefit to both parties and can take more time than other approaches.
</p>

<h4>Compromising</h4>
<p>
    Compromising means you find a middle ground in the conflict. This often involves meeting in the middle or finding
    some mutually agreeable point between both positions. This is useful for quick solutions.
</p>

<h4>Avoiding</h4>
<p>
    Avoiding means not pursuing your own rights or those of the other person. You typically do not address the
    conflict at all, if possible. This may be diplomatically sidestepping an issue or staying away from a
    threatening situation.
</p>

<h4>Yielding</h4>
<p>
    Yielding means neglecting your own interests while giving in to those of the other person. This is
    self-sacrificing and maybe charitable; serving or choosing to obey another when you prefer not to.
</p>

<h3>Conflict Engagement Themes</h3>

<p>Often people find that they have a combined approach and gravitate toward one of the following themes.</p>

{[ chart type:'pie' ]}
    [[ dataitem label:'Solving' value:'{{EngagementProfileSolving}}' fillcolor:'#4E79A7' ]] [[ enddataitem ]]
    [[ dataitem label:'Accommodating' value:'{{EngagementProfileAccommodating}}' fillcolor:'#8CD17D' ]] [[ enddataitem ]]
    [[ dataitem label:'Winning' value:'{{EngagementProfileWinning}}' fillcolor:'#E15759' ]] [[ enddataitem ]]
{[ endchart ]}

<h4>Solving</h4>
<p>
    Solving describes those who seek to use both Resolving and Compromising modes for solving conflict. By combining
    these two modes, they seek to solve problems as a team. Their leadership styles are highly cooperative and
    empowering for the benefit of the entire group.
</p>

<h4>Accommodating</h4>
<p>
    Accommodating combines Avoiding and Yielding modes for solving conflict. They are most effective in roles
    where allowing others to have their way is better for the team, such as support roles or roles where an
    emphasis on the contribution of others is significant.
</p>

<h4>Winning</h4>
<p>
    Winning is not a combination of modes, but a theme that is based entirely on the Winning model alone for
    solving conflict. This theme is important for times when quick decisions need to be made and is helpful
    for roles such as sole-proprietor.
</p>", "1A855117-6489-4A15-846A-5A99F54E9747" );

            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.GIFTS_ASSESSMENT, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"{% if DominantGifts != empty %}
    <div>
        <h2 class='h2'>Dominant Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for dominantGift in DominantGifts %}
                        <tr>
                            <td>
                                {{ dominantGift.Value }}
                            </td>
                            <td>
                                {{ dominantGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if SupportiveGifts != empty %}
    <div>
        <h2 class='h2'>Supportive Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for supportiveGift in SupportiveGifts %}
                        <tr>
                            <td>
                                {{ supportiveGift.Value }}
                            </td>
                            <td>
                                {{ supportiveGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if OtherGifts != empty %}
    <div>
        <h2 class='h2'>Other Gifts</h2>
        <div class='table-responsive'>
            <table class='table'>
                <thead>
                    <tr>
                        <th>
                            Spiritual Gift
                        </th>
                        <th>
                            You are uniquely wired to:
                        </th>
                    </tr>
                </thead>
                <tbody>
                    {% for otherGift in OtherGifts %}
                        <tr>
                            <td>
                                {{ otherGift.Value }}
                            </td>
                            <td>
                                {{ otherGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                </tbody>
            </table>
        </div>
    </div>
{% endif %}
{% if GiftScores != null and GiftScores != empty %}
    <!-- The following empty h2 element is to mantain vertical spacing between sections. -->
    <h2 class='h2'></h2>
    <div>
        <p>
            The following graph shows your spiritual gifts ranked from top to bottom.
        </p>
        <div class='panel panel-default'>
            <div class='panel-heading'>
                <h2 class='panel-title'><b>Ranked Gifts</b></h2>
            </div>
            <div class='panel-body'>
                {[ chart type:'horizontalBar' xaxistype:'linearhorizontal0to100' ]}
                    {% assign sortedScores = GiftScores | OrderBy:'Percentage desc,SpiritualGiftName' %}
                    {% for score in sortedScores %}
                        [[ dataitem label:'{{ score.SpiritualGiftName }}' value:'{{ score.Percentage }}' fillcolor:'#709AC7' ]]
                        [[ enddataitem ]]
                    {% endfor %}
                {[ endchart ]}
            </div>
        </div>
    </div>
{% endif %}", "85256610-56EB-4E6F-B62B-A5517B54B39E" );

            // correct the ordering of Gifts Assessment Attributes
            Sql( "UPDATE [Attribute] SET [Order] = 0 WHERE ([Guid] = '86C9E794-B678-4453-A831-FE348A440646');" ); // Instructions
            // ResultsMessage [Order] has already been set above
            Sql( "UPDATE [Attribute] SET [Order] = 2 WHERE ([Guid] = '85107259-0A30-4F1A-A651-CBED5243B922');" ); // SetPageTitle
            Sql( "UPDATE [Attribute] SET [Order] = 3 WHERE ([Guid] = 'DA7752F5-9F21-4391-97F3-BB7D35F885CE');" ); // SetPageIcon
            Sql( "UPDATE [Attribute] SET [Order] = 4 WHERE ([Guid] = '861F4601-82B7-46E3-967F-2E03D769E2D2');" ); // NumberOfQuestions

            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.MOTIVATORS, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<p>
    {{ Person.NickName }}, here are your motivators results. We’ve listed your Top 5 Motivators, your
    growth propensity score, along with a complete listing of all 22 motivators and your results
    for each.
</p>
<h2>Growth Propensity</h2>
<p>
    Growth Propensity measures your perceived mindset on a continuum between a growth mindset and
    fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.
</p>
<div style='margin: 0;max-width:280px'>
    {[ chart type:'gauge' backgroundcolor:'#f13c1f,#f0e3ba,#0e9445,#3f56a1' gaugelimits:'0,2,17,85,100' chartheight:'150px']}
        [[ dataitem value:'{{ GrowthScore }}' fillcolor:'#484848' ]] [[ enddataitem ]]
    {[ endchart ]}
</div>
<h2>Individual Motivators</h2>
<p>
    There are 22 possible motivators in this assessment. While your Top 5 Motivators may be most helpful in understanding your results in a snapshot, you may also find it helpful to see your scores on each for a complete picture.
</p>
<!-- Theme Chart -->
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Composite Score</b></h2>
    </div>
    <div class='panel-body'>
        {[chart type:'horizontalBar' chartheight:'200px' xaxistype:'linearhorizontal0to100' ]}
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
        <br>
        {{ motivatorThemeScore.DefinedValue.Description }}
        <br>
        {{ motivatorThemeScore.DefinedValue | Attribute:'Summary' }}
    </p>
{% endfor %}
<p>
    The following graph shows your motivators ranked from top to bottom.
</p>
<div class='panel panel-default'>
    <div class='panel-heading'>
        <h2 class='panel-title'><b>Ranked Motivators</b></h2>
    </div>
    <div class='panel-body'>
        {[ chart type:'horizontalBar' xaxistype:'linearhorizontal0to100' ]}
            {% for motivatorScore in MotivatorScores %}
                {% assign theme = motivatorScore.DefinedValue | Attribute:'Theme' %}
                {% if theme and theme != empty %}
                    [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]]
                    [[enddataitem]]
                {% endif %}
            {% endfor %}
        {[endchart]}
    </div>
</div>", "BA51DFCD-B174-463F-AE3F-6EEE73DD9338" );
        }

        /// <summary>
        /// JH: Remove Page name from SystemCommunicationDetail Page's Breadcrumbs and fix typo in (legacy) System Email Detail Block's PreHtml field
        /// </summary>
        private void RemovePagenamefromSystemCommunicationDetailBreadcrumbs()
        {
            // remove Page name from System Communication Detail Page's breadcrumb trail
            Sql( $@"UPDATE [Page]
SET [BreadCrumbDisplayName] = 0
WHERE ([Guid] = '{SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL}');" );

            // fix typo in (legacy) System Email Detail Block's PreHtml field
            Sql( @"UPDATE [Block]
SET [PreHtml] = REPLACE([PreHtml], 'should now managed', 'should now be managed')
WHERE ([BlockTypeId] IN (SELECT [Id]
                         FROM [BlockType]
                         WHERE ([Path] = '~/Blocks/Communication/SystemEmailDetail.ascx') OR
                            ([Path] = '~/Blocks/Communication/SystemEmailList.ascx')));" );
        }
    }
}
