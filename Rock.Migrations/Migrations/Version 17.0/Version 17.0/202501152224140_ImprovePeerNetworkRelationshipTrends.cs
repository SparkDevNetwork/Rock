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

    using Rock.Migrations.Migrations;
    using Rock.Plugin.HotFixes;

    /// <summary>
    ///
    /// </summary>
    public partial class ImprovePeerNetworkRelationshipTrends : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Update Person Profile > Peer Network Block

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

            #region Update Peer Network > Peer List Block

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

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
        <span class=""card-title"">Full Peer Network</span>
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
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Update Peer Network > Peer List Block

            #region Update Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spPeerNetwork_UpdateFollowing] (dropping it first).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateFollowing]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateFollowing];" );

            Sql( RockMigrationSQL._202501152224140_ImprovePeerNetworkRelationshipTrends_spPeerNetwork_UpdateFollowing );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Update Peer Network Stored Procedure
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            #region Revert Person Profile > Peer Network Block

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
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
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

            #endregion Revert Person Profile > Peer Network Block

            #region Revert Peer Network > Peer List Block

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , CAST( ROUND( SUM(pn.[RelationshipScore]), 0 ) AS INT ) AS [RelationshipScore]
        , MAX(pn.[RelationshipTrend]) AS [RelationshipTrend]
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
        <span class=""card-title"">Full Peer Network</span>
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
</div>", "0A35B353-E14E-4B9C-8E0C-7E7D0863A67B" );

            #endregion Revert Peer Network > Peer List Block

            #region Revert Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Roll back to the previous version of [spPeerNetwork_UpdateFollowing] (dropping it first).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateFollowing]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateFollowing];" );

            Sql( HotFixMigrationResource._221_PeerNetworkPageAndBlocks_spPeerNetwork_UpdateFollowing );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Revert Peer Network Stored Procedure
        }
    }
}
