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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 225, "1.17.0" )]
    public class ImprovePeerNetworkCalculations : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            ImprovePeerNetworkCalculationsUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            ImprovePeerNetworkCalculationsDown();
        }

        /// <summary>
        /// JPH: Improve Peer Network calculations - up.
        /// </summary>
        private void ImprovePeerNetworkCalculationsUp()
        {
            #region Update Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spPeerNetwork_UpdateGroupConnections] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateGroupConnections]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            Sql( HotFixMigrationResource._225_ImprovePeerNetworkCalculations_spPeerNetwork_UpdateGroupConnections );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Update Peer Network Stored Procedure

            #region Update Peer Network > Peer Map Block

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
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
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }}
    GROUP BY tp.[NickName]
        , tp.[LastName]
        , tp.[Id]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption];

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Update Peer Network > Peer Map Block
        }

        /// <summary>
        /// JPH: Improve Peer Network calculations - down.
        /// </summary>
        private void ImprovePeerNetworkCalculationsDown()
        {
            #region Revert Peer Network > Peer Map Block

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' %}

{% sql %}

    SELECT 
        tp.[NickName] + ' ' + tp.[LastName] AS [TargetName]
        , tp.[Id] AS [TargetPersonId]
        , pn.[RelationshipScore]
        , pn.[RelationshipTrend]
        , pn.[RelationshipTypeValueId]
        , pn.[RelatedEntityId]
        , pn.[Caption]
    FROM [PeerNetwork] pn
        INNER JOIN [Person] tp ON tp.[Id] = pn.[TargetPersonId]
    WHERE [SourcePersonId] = {{ personId }};

{% endsql %}

<div class=""card card-profile card-peer-map panel-widget"">

    <div class=""card-header"">
        <span class=""card-title"">Peer Map</span>
    </div>

    <div class=""card-section p-0"">
        <div style=""height: 800px"">
            {[ networkgraph height:'100%' minimumnodesize:'10' highlightcolor:'#bababa' ]}

                {% assign followingConnectionsValueId = '84E0360E-0828-E5A5-4BCC-F3113BE338A1' | GuidToId:'DefinedValue' %}
                {% assign following = results | Where:'RelationshipTypeValueId', followingConnectionsValueId %}

                [[ node id:'F-MASTER' label:'Following' color:'#36cf8c' ]][[ endnode ]]

                {% for followed in following %}
                    [[ node id:'F-{{ followed.TargetPersonId }}' label:'{{ followed.TargetName }}' color:'#88ebc0' size:'10' ]][[ endnode ]]
                    [[ edge source:'F-MASTER' target:'F-{{ followed.TargetPersonId }}' color:'#c4c4c4' ]][[ endedge ]]
                {% endfor %}

                {% assign groupConnectionsValueId = 'CB51DC46-FBDB-43DA-B7F3-60E7C6E70F40' | GuidToId:'DefinedValue' %}
                {% assign groups = results | Where:'RelationshipTypeValueId', groupConnectionsValueId | GroupBy:'RelatedEntityId' %}

                {% for group in groups %}
                    {% assign parts = group | PropertyToKeyValue %}

                    {% assign groupName = parts.Value | First | Property:'Caption' %}
                    [[ node id:'G-{{ parts.Key }}' label:""{{ groupName }}"" color:'#4e9fd9' ]][[ endnode ]]

                    {% for member in parts.Value %}
                        [[ node id:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' label:'{{ member.TargetName }}' color:'#a6d5f7' ]][[ endnode ]]

                        [[ edge source:'GM-{{ parts.Key }}-{{ member.TargetPersonId }}' target:'G-{{ parts.Key }}' color:'#c4c4c4' ]][[ endedge ]]
                    {% endfor %}

                {% endfor %}

            {[ endnetworkgraph ]}
        </div>
    </div>
</div>", "A311EB92-5BB5-407D-AF6C-74BC9FB9FA64" );

            #endregion Revert Peer Network > Peer Map Block

            #region Revert Peer Network Stored Procedure

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Roll back to the pervious version of [spPeerNetwork_UpdateGroupConnections] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spPeerNetwork_UpdateGroupConnections]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spPeerNetwork_UpdateGroupConnections];" );

            Sql( HotFixMigrationResource._221_PeerNetworkPageAndBlocks_spPeerNetwork_UpdateGroupConnections );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion Revert Peer Network Stored Procedure
        }
    }
}
