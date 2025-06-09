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

    /// <summary>
    ///
    /// </summary>
    public partial class UpdatePeerNetworkLavaTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Update Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}
{% assign displayCount = 20 %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">

            {% for peer in results limit:displayCount %}
                <div class=""row"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ peer.TargetPersonId }}"">
                            {{ peer.TargetName }}
                        </a>
                    </div>
                    <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                    <div class=""col-xs-2"">
                        {% if peer.PointDifference > 0 %}
                            <i class=""fa fa-arrow-up text-success""></i>
                        {% elseif peer.PointDifference < 0 %}
                            <i class=""fa fa-arrow-down text-danger""></i>
                        {% else %}
                            <i class=""fa fa-minus text-muted""></i>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}

            {% assign resultCount = results | Size %}
            {% if resultCount > displayCount %}
                {% assign moreCount = resultCount | Minus:displayCount %}
                <div class=""row mt-2"">
                    <div class=""col-xs-8"">
                        <a href=""/person/{{ personId }}/peer-graph""><small>(and {{ moreCount | Format:'#,##0' }} more)</small></a>
                    </div>
                </div>
            {% endif %}

    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Update Person Profile > Peer Network Block
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            #region Revert Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        TOP 20
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , SUM( pn.[RelationshipScore] ) - SUM( pn.[RelationshipScoreLastUpdateValue] ) AS [PointDifference]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
    ORDER BY [RelationshipScore] DESC
        , tp.[LastName]
        , tp.[NickName];

{% endsql %}

<div class=""card card-profile card-peer-network panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Network</span>

        <div class=""panel-labels"">
            <a href=""/person/{{ personId }}/peer-graph""><span class=""label label-default"">Peer Graph</span></a>
        </div>
    </div>

    <div class=""card-section"">
        <div class=""row"">
            {% for peer in results %}
                <div class=""col-xs-8"">
                    <a href=""/person/{{ peer.TargetPersonId }}"">
                        {{ peer.TargetName }}
                    </a>
                </div>
                <div class=""col-xs-2"">{{ peer.RelationshipScore }}</div>
                <div class=""col-xs-2"">
                    {% if peer.PointDifference > 0 %}
                        <i class=""fa fa-arrow-up text-success""></i>
                    {% elseif peer.PointDifference < 0 %}
                        <i class=""fa fa-arrow-down text-danger""></i>
                    {% else %}
                        <i class=""fa fa-minus text-muted""></i>
                    {% endif %}
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "879C5623-3A45-4D6F-9759-F9A294D7425B" );

            #endregion Update Person Profile > Peer Network Block
        }
    }
}
