﻿// <copyright>
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
    [MigrationNumber( 108, "1.10.0" )]
    public class UpdateSpiritualGifts : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"IF NOT EXISTS (
    SELECT [Id]
    FROM [ServiceJob]
    WHERE ([Class] = 'Rock.Jobs.PostV103DataMigrationsSpiritualGifts')
        AND ([Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_103_SPIRITUAL_GIFTS}')
)
BEGIN
    INSERT INTO [ServiceJob] (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
    )
    VALUES (
        1
        , 1
        , 'Rock Update Helper v10.3 - Spiritual Gifts Update'
        , 'This job will take care of any data migrations to Spiritual Gifts Assessment results that need to occur after updating to v10.3. After all the operations are done, this job will delete itself.'
        , 'Rock.Jobs.PostV103DataMigrationsSpiritualGifts'
        , '0 0 2 1/1 * ? *'
        , 1
        , '{SystemGuid.ServiceJob.DATA_MIGRATIONS_103_SPIRITUAL_GIFTS}'
    );
END" );

            RockMigrationHelper.UpdateBlockTypeAttribute( SystemGuid.BlockType.GIFTS_ASSESSMENT, SystemGuid.FieldType.CODE_EDITOR, "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 0, @"
<div>
    <h2 class='h2'>Dominant Gifts</h2>
    <div>
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
                    {% if DominantGifts != empty %}
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
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Dominant Gifts
                            </td>
                        </tr>
                    {% endif %}
                </tbody>
            </table>
        </div>
    </div>
</div>
<div>
    <h2 class='h2'>Supportive Gifts</h2>
    <div>
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
                    {% if SupportiveGifts != empty %}
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
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Supportive Gifts
                            </td>
                        </tr>
                    {% endif %}
                </tbody>
            </table>
        </div>
    </div>
</div>
<div>
    <h2 class='h2'>Other Gifts</h2>
    <div>
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
                    {% if OtherGifts != empty %}
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
                    {% else %}
                        <tr>
                            <td colspan='2'>
                                You did not have any Other Gifts
                            </td>
                        </tr>
                    {% endif %}
                </tbody>
            </table>
        </div>
    </div>
</div>
{% if GiftScores != null and GiftScores != empty %}
    <div>
        <h2 class='h2'>Ranked Gifts</h2>
        <p>
            The following graph shows your spiritual gifts ranked from top to bottom.
        </p>
        <div class='panel panel-default'>
            <div class='panel-body'>
                {[ chart type:'horizontalBar' ]}
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
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
