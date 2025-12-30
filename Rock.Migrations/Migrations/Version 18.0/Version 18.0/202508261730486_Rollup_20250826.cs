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
    using System.Collections.Generic;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20250826 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixMissingDefaultCacheControlHeaderSettingsOnBinaryFileTypes();
            JPH_UpdatePeerNetworkLavaTemplates_20250812_Up();
            AddStructuredEditorAttachmentToolUp();
            UpdateEmailFormMessageBodyUp();
            MigrateObsidianNewCommunicationEntryWizardRoutes();
            SetAssetManagerPageToFullWorksurfaceUp();
            AddMissingRoutesUp();
            UpdateFollowingSuggestionNotificationUp();
            AddCheckinSourceValuesUp();
            IconUpdatesUp();
            UpdatePageMenuLavaTemplateUp();
            ChopBlocksForV18Up();
            SwapBlockTypesv18();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_UpdatePeerNetworkLavaTemplates_20250812_Down();
            AddStructuredEditorAttachmentToolDown();
            AddCheckinSourceValuesDown();
        }

        #region KH: MigrationRollupsForV17_4_0 (Plugin Migration #261)

        #region NA: (data migration) Fix Missing Default CacheControlHeaderSettings on BinaryFileTypes

        private void FixMissingDefaultCacheControlHeaderSettingsOnBinaryFileTypes()
        {
            Sql( @"
-- Fix missing default cache control header settings on BinaryFileTypes
UPDATE [BinaryFileType] SET [CacheControlHeaderSettings] = '{""RockCacheablityType"":0,""MaxAge"":null,""SharedMaxAge"":null}' WHERE [CacheControlHeaderSettings] IS NULL
" );
        }

        #endregion

        #region JPH: Update Peer Network Lava Templates to use FromIdHash Lava Filter (cherry-picked back from develop)

        /// <summary>
        /// JPH: Update Peer Network Lava templates 20250812 - Up.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250812_Up()
        {
            #region Update Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}
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

            #region Update Peer Network > Peer Map Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'D2D0FF94-1816-4B43-A49D-104CC42A5DC3');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "D2D0FF94-1816-4B43-A49D-104CC42A5DC3", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}

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

            #region Update Peer Network > Peer List Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '46775056-3ADF-43CD-809A-88EE3378C039');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "46775056-3ADF-43CD-809A-88EE3378C039", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personId = 'Global' | PageParameter:'PersonId' | FromIdHash %}

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
        }

        /// <summary>
        /// JPH: Update Peer Network Lava templates 20250812 - Down.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250812_Down()
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

            #endregion Revert Person Profile > Peer Network Block

            #region Revert Peer Network > Peer Map Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = 'D2D0FF94-1816-4B43-A49D-104CC42A5DC3');" );

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

            #endregion Revert Peer Network > Peer Map Block

            #region Revert Peer Network > Peer List Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '46775056-3ADF-43CD-809A-88EE3378C039');" );

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

            #endregion Revert Peer Network > Peer List Block
        }

        #endregion

        #region DH: Add Structured Editor Attachment Tool

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public void AddStructuredEditorAttachmentToolUp()
        {
            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS,
                value: "Default",
                description: @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    attachment: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Attachment,
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    alert: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Alert,
        inlineToolbar: ['bold', 'italic'],
        config: {
            alertTypes: ['info', 'success', 'warning', 'danger']
        }
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                guid: SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public void AddStructuredEditorAttachmentToolDown()
        {
            RockMigrationHelper.UpdateDefinedValue(
                definedTypeGuid: SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS,
                value: "Default",
                description: @"{
    header: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContentEditor.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContentEditor.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    alert: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Alert,
        inlineToolbar: ['bold', 'italic'],
        config: {
            alertTypes: ['info', 'success', 'warning', 'danger']
        }
    },
    warning: Rock.UI.StructuredContentEditor.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    raw: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Raw
    },
    delimiter: Rock.UI.StructuredContentEditor.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContentEditor.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContentEditor.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContentEditor.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                guid: SystemGuid.DefinedValue.STRUCTURE_CONTENT_EDITOR_DEFAULT );
        }

        #endregion

        #endregion

        #region ME: Update EmailForm block HTML

        /*
             8/20/2025 - MSE

             Added explicit type attributes to several Email Form fields so they are 
             properly recognized by the JavaScript functions in EmailForm.ascx.

             This prevents form values from being cleared when the CAPTCHA component causes a 
             partial postback.

             Reason: Ensure form data is preserved after CAPTCHA interaction.
        */
        private void UpdateEmailFormMessageBodyUp()
        {
            Sql( @"
DECLARE @AttributeId [int] = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '1B6714A9-F582-4FE0-83E8-CBCE2A52020A');
UPDATE [Attribute]
SET [DefaultValue] = '{% if CurrentPerson %}
    {{ CurrentPerson.NickName }}, could you please complete the form below.
{% else %}
    Please complete the form below.
{% endif %}

<div class=''form-group''>
    <label for=''firstname''>First Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.NickName }}</p>
        <input type=''hidden'' id=''firstname'' name=''FirstName'' value=''{{ CurrentPerson.NickName }}'' />
    {% else %}
        <input type=''text'' class=''form-control'' id=''firstname'' name=''FirstName'' placeholder=''First Name'' required />
    {% endif %}
</div>

<div class=''form-group''>
    <label for=''lastname''>Last Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=''hidden'' id=''lastname'' name=''LastName'' value=''{{ CurrentPerson.LastName }}'' />
    {% else %}
        <input type=''text'' class=''form-control'' id=''lastname'' name=''LastName'' placeholder=''Last Name'' required />
    {% endif %}
</div>

<div class=''form-group''>
    <label for=''email''>Email</label>
    {% if CurrentPerson %}
        <input type=''email'' class=''form-control'' id=''email'' name=''Email'' value=''{{ CurrentPerson.Email }}'' placeholder=''Email'' required />
    {% else %}
        <input type=''email'' class=''form-control'' id=''email'' name=''Email'' placeholder=''Email'' required />
    {% endif %}
</div>

<div class=''form-group''>
    <label for=''message''>Message</label>
    <textarea id=''message'' rows=''4'' class=''form-control'' name=''Message'' placeholder=''Message'' required></textarea>
</div>

<div class=''form-group''>
    <label for=''attachment''>Attachment</label>
    <input type=''file'' id=''attachment'' name=''attachment'' />
    <input type=''file'' id=''attachment2'' name=''attachment2'' />
</div>'
    , [DefaultPersistedTextValue] = NULL
    , [DefaultPersistedHtmlValue] = NULL
    , [DefaultPersistedCondensedTextValue] = NULL
    , [DefaultPersistedCondensedHtmlValue] = NULL
    , [IsDefaultPersistedValueDirty] = 1
WHERE [Id] = @AttributeId;" );
        }

        #endregion

        #region NA: Data Migration to change CommunicationEntryWizard PageRoutes to use the newer pages

        private void MigrateObsidianNewCommunicationEntryWizardRoutes()
        {
            // Legacy NewCommunication Page, Route value, RouteGuid
            var legacyNewCommPageGuid = "2A22D08D-73A8-4AAF-AC7E-220E8B2E7857";
            var newCommunicationPreviewPageGuid = "9F7AE226-CC95-4E6A-B333-C0294A2024BC";

            // Set the new Obsidian NewCommunicationEntry block/page to take over the original routes:
            RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/{CommunicationId}", "79C0C1A7-41B6-4B40-954D-660A4B39B8CE" );
            RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/", "61BAF008-56D1-4F61-8C42-9BB672580420" );
            RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "communications/{CommunicationId}", "CDBC4305-9DFC-35F8-7305-4ECBED604A0A" );
            RockMigrationHelper.AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "communications/new", "01A3891B-9998-7E30-20DC-58081A239D65" );

            // Update saved attribute values that reference the OLD page,routeGuid to now reference the NEW page,routeGuid.
            // Use the CHECKSUM for speed due to the large number of attribute values in the AttributeValue table.
            Sql( $@"
                -- Route 'Communication/{{CommunicationId}}' 79C0C1A7-41B6-4B40-954D-660A4B39B8CE
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},79C0C1A7-41B6-4B40-954D-660A4B39B8CE'
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{legacyNewCommPageGuid},79C0C1A7-41B6-4B40-954D-660A4B39B8CE' AND ValueChecksum = CHECKSUM(N'{legacyNewCommPageGuid},79C0C1A7-41B6-4B40-954D-660A4B39B8CE')
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Route 'Communication/' 61BAF008-56D1-4F61-8C42-9BB672580420  
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},61BAF008-56D1-4F61-8C42-9BB672580420'
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{legacyNewCommPageGuid},61BAF008-56D1-4F61-8C42-9BB672580420' AND ValueChecksum = CHECKSUM(N'{legacyNewCommPageGuid},61BAF008-56D1-4F61-8C42-9BB672580420')
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Route 'communications/{{CommunicationId}}' CDBC4305-9DFC-35F8-7305-4ECBED604A0A
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},CDBC4305-9DFC-35F8-7305-4ECBED604A0A'
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{legacyNewCommPageGuid},CDBC4305-9DFC-35F8-7305-4ECBED604A0A' AND ValueChecksum = CHECKSUM(N'{legacyNewCommPageGuid},CDBC4305-9DFC-35F8-7305-4ECBED604A0A') 
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Route 'communications/new' 01A3891B-9998-7E30-20DC-58081A239D65
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},01A3891B-9998-7E30-20DC-58081A239D65'
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{legacyNewCommPageGuid},01A3891B-9998-7E30-20DC-58081A239D65' AND ValueChecksum = CHECKSUM(N'{legacyNewCommPageGuid},01A3891B-9998-7E30-20DC-58081A239D65') 
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';
            " );

            // Just in case someone already started using the 'preview' routes, update those to use the
            // original pageRouteGuid as well because the previewRouteGuid will be deleted next.
            // Use the CHECKSUM for speed due to the large number of attribute values in the AttributeValue table.
            Sql( $@"
                -- Replace anything that saved the Route 'Communication/Preview/{{CommunicationId}}' 07E4E970-8C2B-46B3-900C-F12E7EF00E14
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},79C0C1A7-41B6-4B40-954D-660A4B39B8CE' -- original routeGuid
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{newCommunicationPreviewPageGuid},07E4E970-8C2B-46B3-900C-F12E7EF00E14' AND ValueChecksum = CHECKSUM(N'{newCommunicationPreviewPageGuid},07E4E970-8C2B-46B3-900C-F12E7EF00E14') 
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Replace anything that saved the Route 'Communication/Preview/' 0AC9BD59-3786-4D81-89A3-77FC06A618AE
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},61BAF008-56D1-4F61-8C42-9BB672580420' -- original routeGuid
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{newCommunicationPreviewPageGuid},0AC9BD59-3786-4D81-89A3-77FC06A618AE' AND ValueChecksum = CHECKSUM(N'{newCommunicationPreviewPageGuid},0AC9BD59-3786-4D81-89A3-77FC06A618AE')
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Replace anything that saved the Route 'communications/Preview/{{CommunicationId}}' 45409F5C-C031-49BE-A50D-D14E7E378BA3
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},CDBC4305-9DFC-35F8-7305-4ECBED604A0A' -- original routeGuid
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{newCommunicationPreviewPageGuid},45409F5C-C031-49BE-A50D-D14E7E378BA3' AND ValueChecksum = CHECKSUM(N'{newCommunicationPreviewPageGuid},45409F5C-C031-49BE-A50D-D14E7E378BA3') 
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';

                -- Replace anything that saved the Route 'communications/Preview/new' CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E
                UPDATE av
                SET av.[Value] = '{newCommunicationPreviewPageGuid},01A3891B-9998-7E30-20DC-58081A239D65' -- original routeGuid
                   ,av.[IsPersistedValueDirty] = 1
                FROM [AttributeValue] av
                INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                WHERE av.[Value] = '{newCommunicationPreviewPageGuid},CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E' AND ValueChecksum = CHECKSUM(N'{newCommunicationPreviewPageGuid},CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E')
                  AND a.[FieldTypeId] = 8
                  AND a.[EntityTypeId] = 9
                  AND a.[EntityTypeQualifierColumn] = 'BlockTypeId';
            " );


            // Now we can drop the "preview routes" originally added via 202503011520245_AddNewCommunicationPreviewPageAndPersonalizationSegments.cs
            /* 
                ...AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/Preview/{CommunicationId}", "07E4E970-8C2B-46B3-900C-F12E7EF00E14" );
                ...AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communication/Preview", "0AC9BD59-3786-4D81-89A3-77FC06A618AE" );
                ...AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communications/Preview/{CommunicationId}", "45409F5C-C031-49BE-A50D-D14E7E378BA3" );
                ...AddOrUpdatePageRoute( newCommunicationPreviewPageGuid, "Communications/Preview/new", "CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E" );
            */
            Sql( $@"
                DELETE FROM [PageRoute]
                WHERE [Guid] IN ( '07E4E970-8C2B-46B3-900C-F12E7EF00E14'
                                , '0AC9BD59-3786-4D81-89A3-77FC06A618AE'
                                , '45409F5C-C031-49BE-A50D-D14E7E378BA3'
                                , 'CEE3AF81-6CAA-4B6B-8CC8-0B5FC55B1B2E' );
            " );
        }

        #endregion

        #region NA: Migration to set Asset Manager page use "full worksurface"

        private void SetAssetManagerPageToFullWorksurfaceUp()
        {
            // Change Asset Manager's page to use use "Full Worksurface" layout
            RockMigrationHelper.UpdatePageLayout( "D2B919E2-3725-438F-8A86-AC87F81A72EB", "C2467799-BB45-4251-8EE6-F0BF27201535" );
        }

        #endregion

        #region NA: Add Missing Routes

        private void AddMissingRoutesUp()
        {
            // Communication Reports page Route
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.COMMUNICATION_REPORTS, "admin/communications/communication-reports" );

            // Finance Reports page Route
            RockMigrationHelper.AddOrUpdatePageRoute( Rock.SystemGuid.Page.FINANCE_REPORTS, "finance/reports" );
        }

        #endregion

        #region NA: Update from PublicApplicationRoot to InternalApplicationRoot for the Following Suggestion Notification system communication

        private void UpdateFollowingSuggestionNotificationUp()
        {
            Sql( @"
    -- Update from PublicApplicationRoot to InternalApplicationRoot for the Following Suggestion Notification system communication.
    UPDATE [SystemCommunication]
    SET [Body] = REPLACE(
        [Body],
        '{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}FollowingSuggestionList',
        '{{ ''Global'' | Attribute:''InternalApplicationRoot'' }}FollowingSuggestionList'
    )
    WHERE [Guid] = '8F5A9400-AED2-48A4-B5C8-C9B5D5669F4C';
    " );
        }

        #endregion

        #region DH: Add additional check-in source defined values.

        private void AddCheckinSourceValuesUp()
        {
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE,
                "Proximity",
                "The attendance record was created in response to a Bluetooth low energy proximity beacon.",
                SystemGuid.DefinedValue.ATTENDANCE_SOURCE_PROXIMITY,
                true );

            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.ATTENDANCE_SOURCE,
                "Token",
                "The attendance record was created by scanning a physical token, such as NFC or a QR Code.",
                SystemGuid.DefinedValue.ATTENDANCE_SOURCE_TOKEN,
                true );
        }

        private void AddCheckinSourceValuesDown()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_PROXIMITY );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.ATTENDANCE_SOURCE_TOKEN );
        }

        #endregion

        #region JE: More Icon Updates

        private void IconUpdatesUp()
        {
            Sql( @"
UPDATE [__IconTransition] 
SET [TablerFull] = 'ti ti-quote', [TablerClass] = 'ti-quote'
WHERE [FontAwesomeFull] = 'fa fa-quote-left';

INSERT INTO [__IconTransition] ([FontAwesomeClass], [FontAwesomeFull], [TablerClass], [TablerFull])
VALUES
('fa-sticky-note-o','fa fa-sticky-note-o','ti-note','ti ti-note'),
('fa fa-heart-o','fa fa-heart-o','ti-heart','ti ti-heart')
" );
        }

        #endregion

        #region PS: Change the my setting page menu block lava template

        private void UpdatePageMenuLavaTemplateUp()
        {
            /// Updates the Lava template for the page menu in the My Settings page.
            /// This template renders PageListAsSettings.lava, which is similar to PageListAsSettings.lava
            /// except it includes an additional panel header containing the light/dark mode toggle.
            RockMigrationHelper.UpdateBlockAttributeValue(
                "A795BE67-F69B-4D7B-91E1-4F0883F1B718", // Block Guid
                "1322186A-862A-4CF1-B349-28ECB67229BA", // Attribute Guid
                @"{% include ''~~/Assets/Lava/PageListAsSettings.lava'' %}"
            );
        }

        #endregion

        #region KH: Register block attributes for chop job in v18 (18.0.11)

        /// <summary>
        /// Ensure the Entity, BlockType and Block Setting Attribute records exist
        /// before the chop job runs. Any missing attributes would cause the job to fail.
        /// </summary>
        private void RegisterBlockAttributesForChop()
        {

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.BulkImport.BulkImportTool
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.BulkImport.BulkImportTool", "Bulk Import Tool", "Rock.Blocks.BulkImport.BulkImportTool, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "5B41F45E-2E09-4F97-8BEA-683AFFE0EB62" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationDetail", "Communication Detail", "Rock.Blocks.Communication.CommunicationDetail, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "32838848-2423-4BD9-B5EF-5F7E6AC7F5F4" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.CommunicationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.CommunicationList", "Communication List", "Rock.Blocks.Communication.CommunicationList, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "E4BD5CAD-579E-476D-87EC-989DE975BB60" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunitySignup
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Connection.ConnectionOpportunitySignup", "Connection Opportunity Signup", "Rock.Blocks.Connection.ConnectionOpportunitySignup, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "A10BF374-F97E-49FA-955C-3B22A9F31787" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.BinaryFileTypeList", "Binary File Type List", "Rock.Blocks.Core.BinaryFileTypeList, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "94AC60CE-B192-4559-88A0-AF0CC143F631" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ScheduleCategoryExclusionList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ScheduleCategoryExclusionList", "Schedule Category Exclusion List", "Rock.Blocks.Core.ScheduleCategoryExclusionList, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "C08129E7-D22A-4213-8703-0F0C1511EBDD" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Prayer.PrayerCommentList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Prayer.PrayerCommentList", "Prayer Comment List", "Rock.Blocks.Prayer.PrayerCommentList, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "B2F1B644-836D-46A6-86C9-8FBB26D96EA7" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MergeTemplateList", "Merge Template List", "Rock.Blocks.Reporting.MergeTemplateList, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "EDAAAF0C-BA30-40C9-8E7C-9D1118FEFD87" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.MetricValueDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.MetricValueDetail", "Metric Value Detail", "Rock.Blocks.Reporting.MetricValueDetail, Rock.Blocks, Version=18.0.10.0, Culture=neutral, PublicKeyToken=null", false, false, "AF69AA1A-3EEE-4F25-8014-1A02BA82AC32" );

            // Add/Update Obsidian Block Type
            //   Name:Binary File Type List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.BinaryFileTypeList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Binary File Type List", "Displays a list of binary file types.", "Rock.Blocks.Core.BinaryFileTypeList", "Core", "000CA534-6164-485E-B405-BA0FA6AE92F9" );

            // Add/Update Obsidian Block Type
            //   Name:Bulk Import
            //   Category:Bulk Import
            //   EntityType:Rock.Blocks.BulkImport.BulkImportTool
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Bulk Import", "Block to import Slingshot files into Rock using BulkImport", "Rock.Blocks.BulkImport.BulkImportTool", "Bulk Import", "66F5882F-163C-4616-9B39-2F063611DB22" );

            // Add/Update Obsidian Block Type
            //   Name:Communication Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication Detail", "Used for displaying details of an existing communication that has already been created.", "Rock.Blocks.Communication.CommunicationDetail", "Communication", "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" );

            // Add/Update Obsidian Block Type
            //   Name:Communication List
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.CommunicationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Communication List", "Lists the status of all previously created communications.", "Rock.Blocks.Communication.CommunicationList", "Communication", "C3544F53-8E2D-43D6-B165-8FEFC541A4EB" );

            // Add/Update Obsidian Block Type
            //   Name:Connection Opportunity Signup
            //   Category:Connection
            //   EntityType:Rock.Blocks.Connection.ConnectionOpportunitySignup
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Connection Opportunity Signup", "Block used to sign up for a connection opportunity.", "Rock.Blocks.Connection.ConnectionOpportunitySignup", "Connection", "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" );

            // Add/Update Obsidian Block Type
            //   Name:Merge Template List
            //   Category:Core
            //   EntityType:Rock.Blocks.Reporting.MergeTemplateList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Merge Template List", "Displays a list of all merge templates.", "Rock.Blocks.Reporting.MergeTemplateList", "Core", "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52" );

            // Add/Update Obsidian Block Type
            //   Name:Metric Value Detail
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.MetricValueDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Metric Value Detail", "Displays the details of a particular metric value.", "Rock.Blocks.Reporting.MetricValueDetail", "Reporting", "B52E7CAE-C5CC-41CB-A5EC-1CF027074A2C" );

            // Add/Update Obsidian Block Type
            //   Name:Prayer Comment List
            //   Category:Core
            //   EntityType:Rock.Blocks.Prayer.PrayerCommentList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Prayer Comment List", "Displays a list of prayer comments for the configured top-level group category.", "Rock.Blocks.Prayer.PrayerCommentList", "Core", "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C" );

            // Add/Update Obsidian Block Type
            //   Name:Schedule Category Exclusion List
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ScheduleCategoryExclusionList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Schedule Category Exclusion List", "List of dates that schedules are not active for an entire category.", "Rock.Blocks.Core.ScheduleCategoryExclusionList", "Core", "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "9788BE43-E6FD-4AC3-8E46-1B514871DD84" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "462EEE93-F09F-49C2-A6E9-82700D2DDC03" );

            // Attribute for BlockType
            //   BlockType: Binary File Type List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "000CA534-6164-485E-B405-BA0FA6AE92F9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the binary file type details.", 0, @"", "657F5CDE-0727-4EE2-81A4-3052FF6E39AE" );

            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Financial Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66F5882F-163C-4616-9B39-2F063611DB22", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Financial Record Import Batch Size", "FinancialRecordImportBatchSize", "Financial Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 100,000 records.)", 1, @"100000", "199F5F94-363D-4A32-BEC2-8A5A7C70390E" );

            // Attribute for BlockType
            //   BlockType: Bulk Import
            //   Category: Bulk Import
            //   Attribute: Person Record Import Batch Size
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "66F5882F-163C-4616-9B39-2F063611DB22", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Record Import Batch Size", "PersonRecordImportBatchSize", "Person Record Import Batch Size", @"If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 25,000 records.)", 0, @"25000", "FA2FAD54-1A96-4DC0-8715-7F96D9FE880D" );

            // Attribute for BlockType
            //   BlockType: Communication Detail
            //   Category: Communication
            //   Attribute: Enable Personal Templates
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2B63C6ED-20D5-467E-9A6A-C608E1D953E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Personal Templates", "EnablePersonalTemplates", "Enable Personal Templates", @"Should support for personal templates be enabled? These are templates that a user can create and are personal to them. If enabled, they will be able to create a new template based on the current communication.", 0, @"False", "6A35D776-6CCF-43B5-B92A-0371EA9FF5E3" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B3DD2485-725A-44B2-9B30-CA47F8ADDF03" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "5192EB07-ED42-4D0E-8BA4-0081711227FC" );

            // Attribute for BlockType
            //   BlockType: Communication List
            //   Category: Communication
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C3544F53-8E2D-43D6-B165-8FEFC541A4EB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the communication details.", 0, @"", "5209A318-9C53-43E4-9511-AAC595FC3684" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Comment Field Label
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Comment Field Label", "CommentFieldLabel", "Comment Field Label", @"The label to apply to the comment field.", 10, @"Comments", "8F837F49-47E8-44D6-B57F-EC168299B230" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Opportunity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "B188B729-FE6D-498B-8871-65AB8FD1E11E", "Connection Opportunity", "ConnectionOpportunity", "Connection Opportunity", @"If a Connection Opportunity is set, only details for it will be displayed (regardless of the querystring parameters).", 6, @"", "2C9B7B54-103D-4D9F-815C-8409B60CC9EE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "Connection Status", @"The connection status to use for new individuals (default: 'Web Prospect').", 4, @"368DD475-242C-49C4-A42C-7278BE690CC2", "D159BE3D-9D5C-4154-95F1-D0657C6AFBAE" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification step will not be performed.", 11, @"False", "11EEBBDE-AC6D-4B1B-9CDD-1FCB06A4D7F1" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Home Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Home Phone", "DisplayHomePhone", "Display Home Phone", @"Whether to display the home phone field.", 0, @"True", "900E5756-877B-4FDA-B4A4-3C4407DFFB8D" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Display Mobile Phone
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Display Mobile Phone", "DisplayMobilePhone", "Display Mobile Phone", @"Whether to display the mobile phone field.", 1, @"True", "0D72F824-7F5F-4343-943E-0E4A43A35DB0" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Enable Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Campus Context", "EnableCampusContext", "Enable Campus Context", @"If the page has a campus context its value will be used as a filter.", 3, @"True", "7B5C0BC7-BA15-4A22-A710-C2236DCCDEC9" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "Entity Type", @"The type of entity that will provide context for this block", 0, @"", "B33B0C55-2C58-45F7-AE04-508500E200B8" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Exclude Attribute Categories", "ExcludeAttributeCategories", "Exclude Attribute Categories", @"Attributes in these Categories will not be displayed.", 8, @"", "9BC3C014-C30B-4330-B64D-9FC4B7A6D807" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Exclude Non-Public Connection Request Attributes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Exclude Non-Public Connection Request Attributes", "ExcludeNonPublicAttributes", "Exclude Non-Public Connection Request Attributes", @"Attributes without 'Public' checked will not be displayed.", 9, @"True", "3D77869F-91F3-43AC-B1CE-B6CBF3679326" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Include Attribute Categories
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "775899FB-AC17-4C2C-B809-CF3A1D2AA4E1", "Include Attribute Categories", "IncludeAttributeCategories", "Include Attribute Categories", @"Attributes in these Categories will be displayed.", 7, @"", "72071F7E-9EB2-4BC2-A187-9670C05970B4" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"Lava template to use to display the response message.", 2, @"{% include '~~/Assets/Lava/OpportunityResponseMessage.lava' %}", "211FD620-27C9-44E0-85DB-67F21E1F20F2" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity Signup
            //   Category: Connection
            //   Attribute: Record Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "Record Status", @"The record status to use for new individuals (default: 'Pending').", 5, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "5885E4B7-6137-4E3C-8963-755F8ED7F318" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "FE1D374F-2FB2-49D2-B4A6-02DA844ACB85" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "1E1BCA3D-0F6B-49E4-B698-0A8AFC899BD8" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page that will show the merge template details.", 0, @"", "71E56082-AABE-47AD-95A6-85A13218DA6D" );

            // Attribute for BlockType
            //   BlockType: Merge Template List
            //   Category: Core
            //   Attribute: Merge Templates Ownership
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "740F7DE3-D5F5-4EEB-BEEE-99C3BFB23B52", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Merge Templates Ownership", "MergeTemplatesOwnership", "Merge Templates Ownership", @"Set this to limit to merge templates depending on ownership type.", 0, @"1", "0845BD0A-8A74-49EA-95AD-84EE380A1F97" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Category Selection
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category Selection", "PrayerRequestCategory", "Category Selection", @"A top level category. Only prayer requests comments under this category will be shown.", 1, @"", "370DB580-B342-48A5-8FED-4C92EF121974" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "19B8566A-34D0-4CA9-8868-928D17538CED" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "0FF4FDB9-C084-455D-8475-1B6B9EF8FF7A" );

            // Attribute for BlockType
            //   BlockType: Prayer Comment List
            //   Category: Core
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F997DA7-AC42-41C9-97F1-2069BB9D9E5C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "5A1D49FC-BA17-45FD-A5E1-D1715294DB37" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"Optional Category to use (if not specified, query will be determined by query string).", 0, @"", "0E7C9636-EFD8-4291-9E2E-E04956C78D78" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "E0598054-8D1D-44CD-A0C4-55FCD45887B9" );

            // Attribute for BlockType
            //   BlockType: Schedule Category Exclusion List
            //   Category: Core
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6BC7DA76-1A19-4685-B50A-DFD7EAA5CE33", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "3F1FB398-7962-41C9-87BA-F9994FE5E854" );
        }

        private void ChopBlockTypesv18_0()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Chop Block Types 18.0 (18.0.11)",
                blockTypeReplacements: new Dictionary<string, string> {
                // blocks chopped in v18.0.11
{ "0926B82C-CBA2-4943-962E-F788C8A80037", "000ca534-6164-485e-b405-ba0fa6ae92f9" }, // Binary File Type List ( Core )
{ "508DA252-F94C-4641-8579-458D8FCE14B2", "b52e7cae-c5cc-41cb-a5ec-1cf027074a2c" }, // Metric Value Detail ( Reporting )
{ "56ABBD0F-8F62-4094-88B3-161E71F21419", "c3544f53-8e2d-43d6-b165-8fefc541a4eb" }, // Communication List ( Communication )
{ "ACF84335-34A1-4DD6-B242-20119B8D0967", "6bc7da76-1a19-4685-b50a-dfd7eaa5ce33" }, // Schedule Category Exclusion List ( Core )
{ "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "35D5EF65-0B0D-4E99-82B5-3F5FC2E0344F" }, // Connection Opportunity Signup ( Connection )
{ "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" }, // Communication Detail ( Communication )
{ "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5", "66f5882f-163c-4616-9b39-2f063611db22" }, // Bulk Import ( Bulk Import )
{ "DA102F02-6DBB-42E6-BFEE-360E137B1411", "740f7de3-d5f5-4eeb-beee-99c3bfb23b52" }, // Merge Template List ( Core )
{ "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "3f997da7-ac42-41c9-97f1-2069bb9d9e5c" }, // Prayer Comment List ( Core )
                // blocks chopped in v18.0.10
{ "3131C55A-8753-435F-85F3-DF777EFBD1C8", "8adb5c0d-9a4f-4396-ab0f-deb552c094e1" }, // Benevolence Request List ( Finance )
{ "41AE0574-BE1E-4656-B45D-2CB734D1BE30", "6e9672e6-ee42-4aac-b0a9-b041c3b8368c" }, // Nameless Person List ( CRM )
{ "63ADDB5A-75D6-4E86-A031-98B3451C49A3", "6c824483-6624-460b-9dd8-e127b25ca65d" }, // Web Farm Node Log List ( WebFarm )
{ "78A31D91-61F6-42C3-BB7D-676EDC72F35F", "1c3d7f3d-e8c7-4f27-871c-7ec20483b416" }, // Block Type List ( CMS )
{ "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "6554ADE3-2FC8-482B-BA63-2C3EABC11D32" }, // Group Schedule Toolbox ( Group Scheduling )
{ "95F38562-6CEF-4798-8A4F-05EBCDFB07E0", "6bba1fc0-ac56-4e58-9e99-eb20da7aa415" }, // Web Farm Node Detail ( WebFarm )
{ "A3E648CC-0F19-455F-AF1D-B70A8205802D", "6c329001-9c04-4090-bed0-12e3f6b88fb6" }, // Block Type Detail ( CMS )
{ "C4191011-0391-43DF-9A9D-BE4987C679A4", "e1dce349-2f5b-46ed-9f3d-8812af857f69" }, // Bank Account List ( Finance )
{ "C93D614A-6EBC-49A1-A80D-F3677D2B86A0", "52df00e5-bc19-43f2-8533-a386db53c74f" }, // Campus List ( Core )
{ "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "e20b2fe2-2708-4e9a-b9fb-b370e8b0e702" }, // Saved Account List ( Finance )
                    // blocks chopped in v18.0.8
{ "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2", "770d3039-3f07-4d6f-a64e-c164acce93e1" }, // Signal Type List ( Core )
{ "B3F280BD-13F4-4195-A68A-AC4A64F574A5", "633a75a7-7186-4cfd-ab80-6f2237f0bdd8" }, // AI Provider List ( Core )
{ "88820905-1B5A-4B82-8E56-F9A0736A0E98", "13f49f94-d9bc-434a-bb20-a6ba87bbe81f" }, // AI Provider Detail ( Core )
{ "D3B7C96B-DF1F-40AF-B09F-AB468E0E726D", "120552e2-5c36-4220-9a73-fbbbd75b0964" }, // Audit List ( Core )
                    // blocks chopped in v18.0
{ "1D7B8095-9E5B-4A9A-A519-69E1746140DD", "e44cac85-346f-41a4-884b-a6fb5fc64de1" }, // Page Short Link Click List ( CMS )
{ "4C4A46CD-1622-4642-A655-11585C5D3D31", "eddfcaff-70aa-4791-b051-6567b37518c4" }, // Achievement Type Detail ( Achievements )
{ "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "fbe75c18-7f71-4d23-a546-7a17cf944ba6" }, // Achievement Attempt Detail ( Engagement )
{ "9C1236AE-4FF0-480C-A7DF-0E5277CA75FB", "b294c1b9-8368-422c-8054-9672c7f41477" }, // Achievement Attempt List ( Achievements )
{ "C26C7979-81C1-4A20-A167-35415CD7FED3", "09FD3746-48D1-4B94-AAA9-6896443AA43E" }, // Lava Shortcode List ( CMS )
{ "D2F918CF-F63C-4643-A02A-FD3DE7C5CFFD", "4acfbf3f-3d49-4ae3-b468-529f79da9898" }, // Achievement Type List ( Streaks )
{ "D6D87CCC-DB6D-4138-A4B5-30F0707A5300", "d25ff675-07c8-4e2d-a3fa-38ba3468b4ae" }, // Page Short Link List ( CMS )
                },
                migrationStrategy: "Chop",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_CHOP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string> {
                { "CEDC742C-0AB3-487D-ABC2-77A0A443AEBF", "SeriesColors" }, // Communication Detail ( Communication )
                { "7F9CEA6F-DCE5-4F60-A551-924965289F1D", "FutureWeeksToShow,SignupInstructions,EnableSignup,DeclineReasonPage" }, // Group Schedule Toolbox    
                { "C7FCE3B7-704B-43C0-AF96-5A70EB7F70D9", "SetPageTitle,EnableDebug,2E6540EA-63F0-40FE-BE50-F2A84735E600,8522BADD-2871-45A5-81DD-C76DA07E2E7E" }, // Connection Opportunity Signup
                { "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "GroupCategoryId" }, // Prayer Comment List ( Core )
                { "3131C55A-8753-435F-85F3-DF777EFBD1C8", "DetailPage,CaseWorkerGroup" }, // Benevolence Request List ( Finance )
                { "7E4663CD-2176-48D6-9CC2-2DBC9B880C23", "StreakPage" } // Achievement Attempt Detail ( Engagement )
            } );
        }

        private void ChopBlocksForV18Up()
        {
            RegisterBlockAttributesForChop();
            //ChopBlockTypesv18_0();
        }

        #endregion

        #region KH: Swap Blocks for v18.0.11

        private void SwapBlockTypesv18()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                "Swap Block Types - 18.0.11",
                blockTypeReplacements: new Dictionary<string, string> {
                    // Swap Block Types - 18.0.11
                    { "13165D92-9CCD-4071-8484-3956169CB640", "535500a7-967f-4da3-8fca-cb844203cb3d" }, // File Asset Manager
                    { "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "0252E237-0684-4426-9E5C-D454A13E152A" }, // Registration Entry
                },
                migrationStrategy: "Swap",
                jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_180_SWAP_OBSIDIAN_BLOCKS,
                blockAttributeKeysToIgnore: new Dictionary<string, string>  {
                { "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "ConfirmAccountTemplate" } // Registration Entry
            } );
        }

        #endregion
    }
}
