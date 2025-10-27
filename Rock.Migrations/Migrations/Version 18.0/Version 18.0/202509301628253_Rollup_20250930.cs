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
    public partial class Rollup_20250930 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            NA_DeactivateDuplicateAppleDeviceModelDefinedValues_Up();
            JPH_UpdatePeerNetworkLavaTemplates_20250910_Up();
            NA_MigrateModelMapToTabler_Up();
            NA_ChangeConnectionStatusColorAttributeToColorPicker_Up();
            FixMissingCodeOnNoteLabelUp();
            AddMissingSecurityToFinancialAccountUp();
            RemoveCampusContextBlocksOnStepsPages_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_UpdatePeerNetworkLavaTemplates_20250910_Down();
            AddMissingSecurityToFinancialAccountDown();
        }

        #region NA: Migration to Inactivate duplicate DefinedValues for Apple Device Models

        private void NA_DeactivateDuplicateAppleDeviceModelDefinedValues_Up()
        {
            Sql( @"
                -- Step 1: Retrieve the Apple Device DefinedTypeId
                DECLARE @AppleDeviceDefinedTypeId INT = (
                    SELECT [Id] 
                    FROM [DefinedType] 
                    WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C'
                );

                -- Step 2: Identify the original (lowest Id) for each duplicate group
                WITH DuplicatesCTE AS (
                    SELECT 
                        [Value],
                        [Description],
                        MIN([Id]) AS OriginalId
                    FROM [DefinedValue]
                    WHERE [DefinedTypeId] = @AppleDeviceDefinedTypeId
		                AND [IsActive] = 1
                    GROUP BY [Value], [Description]
                    HAVING COUNT(*) > 1
                )

                -- Step 3: Update IsActive to 0 for true duplicates (those with higher Ids)
                UPDATE dv
                SET dv.[IsActive] = 0
                FROM [DefinedValue] dv
                INNER JOIN DuplicatesCTE dct ON 
                    dv.[Value] = dct.[Value] AND 
                    dv.[Description] = dct.[Description] AND 
                    dv.[Id] > dct.OriginalId
                WHERE dv.[DefinedTypeId] = @AppleDeviceDefinedTypeId;
                " );
        }

        #endregion

        #region JPH: Update Peer Network Lava Templates.. Again

        /// <summary>
        /// JPH: Update Peer Network Lava templates 20250910 - Up.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250910_Up()
        {
            #region Update Person Profile > Peer Network Block

            // Delete all old templates tied to this block (to account for possible versioning).
            Sql( "DELETE FROM [HtmlContent] WHERE [BlockId] = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '6094C135-10E2-4AF4-A46B-1FC6D073A854');" );

            RockMigrationHelper.UpdateHtmlContentBlock( "6094C135-10E2-4AF4-A46B-1FC6D073A854", @"/-
    IMPORTANT
    Do not change this Lava template as it will be updated in future releases as we
    continue to innovate on this feature. These updates will wipe out your changes.
-/

{% assign personIdParam = 'Global' | PageParameter:'PersonId' %}

//- Check for person ID format of integer ID or IdKey or Guid.
{% assign personId = personIdParam | FromIdHash %}
{% if personId == null %}
    {% assign personEntityTypeId = '72657ED8-D16E-492E-AC12-144C5E7567E7' | GuidToId:'EntityType' %}
    {% assign personId = personIdParam | GuidToId:personEntityTypeId %}
{% endif %}

{% if personId == null %}
    {% return %}
{% endif %}

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

{% assign personIdParam = 'Global' | PageParameter:'PersonId' %}

//- Check for person ID format of integer ID or IdKey or Guid.
{% assign personId = personIdParam | FromIdHash %}
{% if personId == null %}
    {% assign personEntityTypeId = '72657ED8-D16E-492E-AC12-144C5E7567E7' | GuidToId:'EntityType' %}
    {% assign personId = personIdParam | GuidToId:personEntityTypeId %}
{% endif %}

{% if personId == null %}
    {% return %}
{% endif %}

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

{% assign personIdParam = 'Global' | PageParameter:'PersonId' %}

//- Check for person ID format of integer ID or IdKey or Guid.
{% assign personId = personIdParam | FromIdHash %}
{% if personId == null %}
    {% assign personEntityTypeId = '72657ED8-D16E-492E-AC12-144C5E7567E7' | GuidToId:'EntityType' %}
    {% assign personId = personIdParam | GuidToId:personEntityTypeId %}
{% endif %}

{% if personId == null %}
    {% return %}
{% endif %}

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
        /// JPH: Update Peer Network Lava templates 20250910 - Down.
        /// </summary>
        private void JPH_UpdatePeerNetworkLavaTemplates_20250910_Down()
        {
            #region Revert Person Profile > Peer Network Block

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

            #endregion Revert Person Profile > Peer Network Block

            #region Revert Peer Network > Peer Map Block

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

            #endregion Revert Peer Network > Peer Map Block

            #region Revert Peer Network > Peer List Block

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

            #endregion Revert Peer Network > Peer List Block
        }

        #endregion

        #region NA: Data Migration for ModelMap to Tabler

        private void NA_MigrateModelMapToTabler_Up()
        {
            RockMigrationHelper.AddBlockAttributeValue( "2583DE89-F028-4ACE-9E1F-2873340726AC", "75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A", @"Check-in^ti ti-door-enter|CMS^ti ti-code|Communication^ti ti-message-circle|Connection^ti ti-plug|Core^ti ti-settings|CRM^ti ti-user|Engagement^ti ti-user-cog|Event^ti ti-calendar-event|Finance^ti ti-cash-banknote|Group^ti ti-users|LMS^ti ti-school|Meta^ti ti-table|Other^ti ti-help-circle|Prayer^ti ti-cloud-upload|Reporting^ti ti-report-analytics|Security^ti ti-shield-lock|WebFarm^ti ti-server|Workflow^ti ti-settings-cog" );
        }

        #endregion

        #region NA: Change Connection Status Color Attribute to a Color Picker

        private void NA_ChangeConnectionStatusColorAttributeToColorPicker_Up()
        {
            Sql( @"-- Change the Connection Status 'Color' Attribute from a Text field type to Color (FieldTypeId 4)
UPDATE [Attribute] SET FieldTypeId = 4 WHERE [Guid] = '23777A50-E000-4F29-994F-26635A357160' 

-- add a new [AttributeQualifier] of 'selectiontype' (Key) and 'Color Picker' (Value)
IF NOT EXISTS (
    SELECT 1
    FROM AttributeQualifier
    WHERE AttributeId = 1121
      AND [Key] = 'selectiontype'
)
BEGIN
    INSERT INTO AttributeQualifier ([IsSystem], [AttributeId], [Key], [Value], [Guid])
    VALUES (1, 1121, 'selectiontype', 'Color Picker', '19BAC142-F565-42FA-8D95-B2757701709A');
END" );
        }

        #endregion

        #region DH: Fix Missing Code On Note Label

        private void FixMissingCodeOnNoteLabelUp()
        {
            var noteLabelGuid = "2272b545-e254-4377-8b06-3e465a6c8545";
            var content = "{\"Width\":4.0,\"Height\":2.0,\"Fields\":[{\"Guid\":\"7eadf522-72dc-47d3-a57d-652f9e23df83\",\"FieldType\":2,\"FieldSubType\":0,\"ConditionalVisibility\":{\"Guid\":\"0cdb8d71-abb5-4c04-a355-e33347bf1065\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"c86c1225-533e-4794-a665-e5b743283417\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"f832ab6f-b684-4eea-8db4-c54b895c79ed\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.12499999999999996,\"Top\":1.3750000000000004,\"Width\":0.25000000000000006,\"Height\":0.24999999999999939,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"isFilled\":\"true\",\"borderThickness\":\"0\",\"cornerRadius\":\"0\",\"icon\":\"\",\"isColorInverted\":\"false\"}},{\"Guid\":\"1a8c534c-2411-4fae-8e93-88ed886aedd4\",\"FieldType\":2,\"FieldSubType\":0,\"ConditionalVisibility\":{\"Guid\":\"a12b940b-592a-4198-893b-f65ef2cafb82\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"1fe3dd4f-b376-4040-bb55-3c2aad9f3721\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"dbd192c9-0aa1-46ec-92ab-a3da8e056d31\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.125,\"Top\":1.0,\"Width\":0.25000000000000039,\"Height\":0.250000000000001,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"isFilled\":\"true\",\"borderThickness\":\"0\",\"cornerRadius\":\"0\",\"icon\":\"\",\"isColorInverted\":\"false\"}},{\"Guid\":\"22da9ae2-db12-4e71-bc73-099fe512da7a\",\"FieldType\":2,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":0.0,\"Top\":0.0,\"Width\":4.0,\"Height\":0.625,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"isFilled\":\"true\",\"borderThickness\":\"0\",\"cornerRadius\":\"0\"}},{\"Guid\":\"f12e8270-7c96-4a58-be85-2990fa96a6bd\",\"FieldType\":0,\"FieldSubType\":2,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.3749999999999982,\"Top\":0.084932659932660068,\"Width\":1.4999999999999971,\"Height\":0.49999999999999994,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"40\",\"horizontalAlignment\":\"2\",\"isBold\":\"false\",\"isColorInverted\":\"true\",\"isCondensed\":\"false\",\"placeholderText\":\"CODE\",\"staticText\":\"Text\",\"sourceKey\":\"b66a29e1-d84c-4c4e-9c39-e22cf4cc9b9c\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\"}},{\"Guid\":\"45512e17-13c4-4544-86ce-f277256c43de\",\"FieldType\":0,\"FieldSubType\":1,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":0.12499999999999994,\"Top\":0.66792929292929348,\"Width\":1.7500000000000002,\"Height\":0.19444444444444464,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"14\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"placeholderText\":\"Full Name\",\"staticText\":\"Text\",\"sourceKey\":\"ea92317c-65b9-4d8e-b4c4-7fc7ab9ad932\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"bfc1d8b3-d28c-48ad-97de-fae6c7004f0f\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"14=12;18=8\",\"isDynamicText\":\"false\",\"dynamicTextTemplate\":\"\"}},{\"Guid\":\"a88fc3b1-06b6-44fa-9189-09f61d4886bd\",\"FieldType\":0,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.1249999999999964,\"Top\":0.64983164983164865,\"Width\":1.7500000000000036,\"Height\":0.23224607976095626,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"9\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"placeholderText\":\"Location: Schedule\",\"staticText\":\"Text\",\"isDynamicText\":\"true\",\"dynamicTextTemplate\":\"{{ Location.Name }}: {{ ScheduleNames | Join:', ' }}\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\",\"sourceKey\":\"\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\",\"isBlack\":\"false\",\"thickness\":\"1\"}},{\"Guid\":\"b23fd993-cdec-41a0-ae7c-8780c7d303a4\",\"FieldType\":4,\"FieldSubType\":0,\"ConditionalVisibility\":{\"Guid\":\"317c4041-e309-4717-ab5e-5ad6b0af639e\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"03910d60-acbf-4539-9114-b5926c1c1d3b\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"dbd192c9-0aa1-46ec-92ab-a3da8e056d31\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.12499999999999999,\"Top\":1.0000000000000007,\"Width\":0.25,\"Height\":0.24999999999999922,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"icon\":\"briefcase-medical\",\"isColorInverted\":\"true\",\"isDynamicText\":\"false\",\"staticText\":\"\",\"dynamicTextTemplate\":\"\",\"placeholderText\":\"\",\"fontSize\":\"12\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isCondensed\":\"false\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\",\"isBlack\":\"false\",\"isFilled\":\"false\",\"sourceKey\":\"\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\"}},{\"Guid\":\"f5badab7-a275-4635-9969-717e08a8b4bb\",\"FieldType\":0,\"FieldSubType\":1,\"ConditionalVisibility\":{\"Guid\":\"11d1bb70-989f-4626-b954-268a65929e45\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"dc15bb94-2fb1-4d72-a6bb-eb83f6af7db8\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"dbd192c9-0aa1-46ec-92ab-a3da8e056d31\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.43702127093002174,\"Top\":0.99999999999999967,\"Width\":1.4379787290699764,\"Height\":0.37499999999999967,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"8\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"placeholderText\":\"Allergy\",\"staticText\":\"Text\",\"sourceKey\":\"attribute:person:dbd192c9-0aa1-46ec-92ab-a3da8e056d31\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\",\"icon\":\"\"}},{\"Guid\":\"185dc295-f249-46b3-9df9-8708a30afde0\",\"FieldType\":1,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.0,\"Top\":0.80723905723905742,\"Width\":0.0,\"Height\":1.1927609427609427,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"thickness\":\"2\",\"isDynamicText\":\"false\",\"staticText\":\"\",\"dynamicTextTemplate\":\"\",\"placeholderText\":\"\",\"fontSize\":\"12\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\"}},{\"Guid\":\"e049a34f-b46a-4935-9f79-71aff738d919\",\"FieldType\":4,\"FieldSubType\":0,\"ConditionalVisibility\":{\"Guid\":\"e3e236c7-49fd-4eac-abd4-c2cd022a74de\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"73d4620e-210e-4154-91bf-b6af15da7774\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"f832ab6f-b684-4eea-8db4-c54b895c79ed\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.125,\"Top\":1.3750000000000002,\"Width\":0.25,\"Height\":0.25,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"icon\":\"star-of-life\",\"isColorInverted\":\"true\",\"sourceKey\":\"\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\",\"placeholderText\":\"\",\"fontSize\":\"12\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isCondensed\":\"false\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\",\"isBlack\":\"false\",\"isFilled\":\"false\"}},{\"Guid\":\"7708ca94-d24d-479f-8cc9-b71c21f3dad1\",\"FieldType\":0,\"FieldSubType\":1,\"ConditionalVisibility\":{\"Guid\":\"50951a2e-9767-4706-bf13-a8d129cee0fd\",\"ExpressionType\":1,\"Rules\":[{\"Guid\":\"ba0bc5de-486f-40e0-b838-1cb9c6a053ac\",\"ComparisonType\":64,\"Value\":\"\",\"Path\":\"Person\",\"SourceType\":0,\"AttributeGuid\":\"f832ab6f-b684-4eea-8db4-c54b895c79ed\",\"PropertyName\":null}],\"Groups\":null},\"IsIncludedOnPreview\":true,\"Left\":0.43702127093002124,\"Top\":1.375000000000002,\"Width\":1.4379787290699788,\"Height\":0.37499999999999994,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"8\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"placeholderText\":\"Legal Notes\",\"staticText\":\"Text\",\"sourceKey\":\"attribute:person:f832ab6f-b684-4eea-8db4-c54b895c79ed\",\"collectionFormat\":\"0\",\"formatterOptionKey\":\"\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\",\"icon\":\"\",\"isBlack\":\"false\",\"thickness\":\"1\"}},{\"Guid\":\"947d5f66-f42a-4c30-aa44-55514fabd11f\",\"FieldType\":0,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.1249999999999987,\"Top\":0.90934869060853984,\"Width\":0.49999999999999889,\"Height\":0.10496107370122436,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"fontSize\":\"8\",\"horizontalAlignment\":\"0\",\"isBold\":\"false\",\"isColorInverted\":\"false\",\"isCondensed\":\"false\",\"placeholderText\":\"Text\",\"staticText\":\"Notes:\",\"isDynamicText\":\"false\",\"dynamicTextTemplate\":\"\",\"maxLength\":\"null\",\"adaptiveFontSize\":\"\"}},{\"Guid\":\"918cdbcc-c39f-44ab-a4c5-b19e197408b0\",\"FieldType\":1,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.125,\"Top\":1.9351010101010111,\"Width\":1.75,\"Height\":0.0,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"thickness\":\"2\"}},{\"Guid\":\"7a5ddaa6-7c08-47c4-9c87-ffdd998ae7e2\",\"FieldType\":1,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.125,\"Top\":1.6927609427609425,\"Width\":1.75,\"Height\":0.0,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"thickness\":\"2\"}},{\"Guid\":\"dda3c620-4aff-4ab6-86c5-5d2e0fd8fb71\",\"FieldType\":1,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.125,\"Top\":1.1822390572390571,\"Width\":1.75,\"Height\":0.0,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"thickness\":\"2\"}},{\"Guid\":\"67686cee-bbcf-46af-93f1-21a8755aa250\",\"FieldType\":1,\"FieldSubType\":0,\"ConditionalVisibility\":null,\"IsIncludedOnPreview\":true,\"Left\":2.125,\"Top\":1.4351010101010098,\"Width\":1.75,\"Height\":0.0,\"RotationAngle\":0.0,\"CustomData\":null,\"ConfigurationValues\":{\"isBlack\":\"true\",\"thickness\":\"2\"}}]}";

            Sql( $@"
UPDATE [CheckInLabel]
    SET [Content] = '{content.Replace( "'", "''" )}'
WHERE [Guid] = '{noteLabelGuid}'" );
        }

        #endregion

        #region DH: Add Missing Security To Financial Account

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        private void AddMissingSecurityToFinancialAccountUp()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialAccount",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                0,
                "e6c7454a-b8d7-4e57-9319-b7f324c52106" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        private void AddMissingSecurityToFinancialAccountDown()
        {
            RockMigrationHelper.DeleteSecurityAuth( "e6c7454a-b8d7-4e57-9319-b7f324c52106" );
        }


        #endregion

        #region KH: Remove Campus Context Blocks on Steps Pages

        private void RemoveCampusContextBlocksOnStepsPages_Up()
        {
            // Remove Campus Context block from Steps pages
            RockMigrationHelper.DeleteBlock( "6B2FD4C7-0EF7-4906-97D0-8DF69A3F45A7" ); // Step Program Page - Campus Context
            RockMigrationHelper.DeleteBlock( "81D440A0-D25E-4EA8-A9C2-926AE742CBF8" ); // Step Type Page - Campus Context
        }

        #endregion
    }
}
