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
    public partial class Rollup_20221103 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MoveSignatureDocumentsBlockUp();
            TvAndMobileLoggingDomainsUp();
            GroupRequirementChangesForFundraisingOpportunitiesUp();
            AddChartGridlinecolorProperty();
            ChartShortcodeToolTipNumbersUp();
            SetFinancialAchievementsEnabledUp();
            PanelShortcodeUpdate();
            AxisLabelsOnChartShortcode();
            UpdateCheckScannerUrlUp();
            ContentCollectionViewBlockDefaultItemTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            TvAndMobileLoggingDomainsDown();
            GroupRequirementChangesForFundraisingOpportunitiesDown();
            UpdateCheckScannerUrlDown();
        }

        /// <summary>
        /// DL: Move Signature Documents Block
        /// </summary>
        private void MoveSignatureDocumentsBlockUp()
        {
            Sql( @"
                -- Get the Person/History/Documents block instance.
                DECLARE @blockId int =
                (
                  SELECT b.Id
                  FROM [Block] b
                  INNER JOIN [BlockType] bt ON b.BlockTypeId = bt.Id
                  INNER JOIN [Page] p ON b.PageId = p.Id
                  WHERE bt.Guid = '256F6FDB-B241-4DE6-9C38-0E9DA0270A22'
                  AND p.Guid = 'BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418'
                )
 
                -- Reparent the block to the Person/Documents page.
                DECLARE @documentsPageId int =
                ( 
                  SELECT [Id]
                  FROM [Page]
                  WHERE [Guid] = '6155FBC2-03E9-48C1-B2E7-554CBB7589A5'
                )
 
                IF @blockId IS NOT NULL
                BEGIN 
                    UPDATE [Block]
                    SET [PageId] = @documentsPageId,
                    [Name] = 'Electronic Signature Documents'
                    WHERE [Id] = @blockId
                END
 
                -- Reparent the Document Detail page from Person/History to Person/Documents.
                DECLARE @documentDetailPageGuid uniqueidentifier =
                (
                  SELECT av.Value
                  FROM [AttributeValue] av
                  INNER JOIN [Attribute] a ON av.AttributeId = a.Id
                  WHERE a.[Key] = 'DetailPage'
                  AND [EntityTypeId] = (SELECT Id FROM [EntityType] WHERE [Guid] = 'D89555CA-9AE4-4D62-8AF1-E5E463C1EF65') -- BLOCK
                  AND [EntityId] = @blockId
                )
 
                DECLARE @documentDetailPageId int = (SELECT [Id] FROM [Page] WHERE [Guid] = @documentDetailPageGuid);
 
                IF @documentDetailPageId IS NOT NULL
                BEGIN 
                    UPDATE [Page]
                    SET [ParentPageId] = @documentsPageId
                    WHERE [Id] = @documentDetailPageId
 
                    -- Update the system route for the relocated Document Detail Page.
                    UPDATE [PageRoute]
                    SET [Route] = 'person/{PersonId}/persondocs/signature/{SignatureDocumentId}'
                    WHERE [PageId] = @documentDetailPageId
                    AND [IsSystem] = 1
                END" );
        }

        /// <summary>
        /// Tvs the and mobile logging domains up.
        /// </summary>
        public void TvAndMobileLoggingDomainsUp()
        {
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LOGGING_DOMAINS, "AppleTv", "Used for logging with development related to Apple Tv apps.", SystemGuid.DefinedValue.LOGGING_DOMAIN_APPLE_TV );
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LOGGING_DOMAINS, "Mobile", "Used for logging with development related to mobile apps.", SystemGuid.DefinedValue.LOGGING_DOMAIN_MOBILE );
        }

        /// <summary>
        /// Tvs the and mobile logging domains down.
        /// </summary>
        public void TvAndMobileLoggingDomainsDown()
        {
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.LOGGING_DOMAIN_APPLE_TV );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.LOGGING_DOMAIN_MOBILE );
        }

        /// <summary>
        /// CR: Group Type Participation Attribute, Fundraising Entry Header, and GroupMemberReq ManuallyCompleted Data Update
        /// </summary>
        private void GroupRequirementChangesForFundraisingOpportunitiesUp()
        {
            // Get the GroupTypeId for Fundraising Opportunity.
            var groupReq_GroupTypeIdSql = $"SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY}'";
            var groupReq_GroupTypeId = SqlScalar( groupReq_GroupTypeIdSql ).ToIntSafe();
            if ( groupReq_GroupTypeId > 0 )
            {
                RockMigrationHelper.AddOrUpdateEntityAttribute(
                    "Rock.Model.Group", SystemGuid.FieldType.SINGLE_SELECT, "GroupTypeId",
                    groupReq_GroupTypeId.ToString(),
                    "Participation Type",
                    "Participation Type",
                    @"The type of participation in this group.", 18, @"1", SystemGuid.Attribute.PARTICIPATION_TYPE, "ParticipationType" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "values", @"1^Individual,2^Family", "4B50E240-74D5-4953-B9B3-99DC4EEB5C84" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "fieldtype", @"ddl", "93A56DAE-2672-43F4-8AC9-C144B0AB84B3" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "repeatColumns", @"", "04D5573C-4671-45B7-A591-13EC6EA0FF99" );
            }

            string newTransactionHeader =
                @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = FundraisingGoal %}
{% assign amountRaised = AmountRaised %}
{% assign participationType = PageParameter[''ParticipationMode''] %}
{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}
{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}
{% assign amountRemaining = fundraisingGoal | Minus:AmountRaised %}

<div class=''row''>
    <div class=''col-md-6''>
      <dl>
        <dt>Fundraising Opportunity</dt>
        <dd>{{ groupMember.Group | Attribute:''OpportunityTitle'' }}</dd>
        <dt>Fundraising Goal</dt>
        <dd>
            {{ fundraisingGoal | FormatAsCurrency }}
        </dd>
      </dl>
      <p></p>
    </div>
    <div class=''col-md-6''>
        <dl>
            <dt>Participant</dt>
            <dd>{% if participationType == ''2'' %}
                   {{ groupMember.Person.PrimaryFamily.Name }}
                 {% else %}
                   {{ groupMember.Person.FullName }}
                 {% endif %}</dd>
            <dt>Amount Remaining</dt>
            <dd>{{ amountRemaining | FormatAsCurrency }}</dd>
        </dl>
    </div>
</div>";

            Sql( $@"
                UPDATE [AttributeValue]
                SET [Value] = '{newTransactionHeader}'
                WHERE [Id] IN (SELECT av.[Id]
                    FROM [dbo].[AttributeValue] av
                    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                    WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')" );
        }

        /// <summary>
        /// CR: Group Type Participation Attribute, Fundraising Entry Header, and GroupMemberReq ManuallyCompleted Data Update
        /// </summary>
        private void GroupRequirementChangesForFundraisingOpportunitiesDown()
        {
            string oldTransactionHeader =
    @"{% assign groupMember = TransactionEntity %}
{% assign fundraisingGoal = groupMember | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% if fundraisingGoal == '''' %}
  {% assign fundraisingGoal = groupMember.Group | Attribute:''IndividualFundraisingGoal'',''RawValue'' %}
{% endif %}

{% comment %}
-- convert fundraisingGoal to a numeric by using Plus
{% endcomment %}

{% assign fundraisingGoal = fundraisingGoal | Plus:0.00 %}

{% assign amountRemaining = fundraisingGoal | Minus:TransactionEntityTransactionsTotal %}

<div class=''row''>
    <div class=''col-md-6''>
      <dl>
        <dt>Fundraising Opportunity</dt>
        <dd>{{ groupMember.Group | Attribute:''OpportunityTitle'' }}</dd>
        <dt>Fundraising Goal</dt>
        <dd>
            {{ fundraisingGoal | FormatAsCurrency }}
        </dd>
      </dl>
      <p></p>
    </div>
    <div class=''col-md-6''>
        <dl>
            <dt>Participant</dt>
            <dd>{{ groupMember.Person.FullName }}</dd>
            <dt>Amount Remaining</dt>
            <dd>{{ amountRemaining | FormatAsCurrency }}</dd>
        </dl>
    </div>
</div>";

            Sql( $@"
                UPDATE [AttributeValue]
                SET [Value] = '{oldTransactionHeader}'
                WHERE [Id] IN (SELECT av.[Id]
                    FROM [dbo].[AttributeValue] av
                    INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                    WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')" );

            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PARTICIPATION_TYPE );
        }

        /// <summary>
        /// GJ: Add gridlinecolor property to chart shortcode
        /// </summary>
        public void AddChartGridlinecolorProperty()
        {
            Sql( MigrationSQL._202209291606147_Rollup_20220929_AddChartGridlinecolorProperty );
        }

        /// <summary>
        /// CR: (Rollup) Update Chart LavaShortcode to show tooltip values 
        /// </summary>
        private void ChartShortcodeToolTipNumbersUp()
        {
            Sql( @"
UPDATE [LavaShortcode] SET [Markup]=N'{% javascript url:''~/Scripts/moment.min.js'' id:''moment''%}{% endjavascript %}
{% javascript url:''~/Scripts/Chartjs/Chart.min.js'' id:''chartjs''%}{% endjavascript %}

{%- if type == ''gauge'' or type == ''tsgauge'' -%}
    {%- assign type = ''tsgauge'' -%}
    {% javascript url:''~/Scripts/Chartjs/Gauge.js'' id:''gaugejs''%}{% endjavascript %}
{%- endif -%}

{%- if type == ''stackedbar'' -%}
    {%- assign type = ''bar'' -%}
    {%- assign xaxistype = ''stacked'' -%}
{%- endif -%}

{% assign id = uniqueid %}
{% assign curvedlines = curvedlines | AsBoolean %}

{%- if type == ''tsgauge'' -%}
  {% assign backgroundColor = backgroundcolor | Split:'','' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
  {% assign gaugeLimits = gaugelimits | Split:'','' | Join:'','' | Prepend:''['' | Append:'']'' %}
  {%- assign tooltipshow = false -%}
  {%- capture seriesData -%}
    {
        backgroundColor: {{ backgroundColor }},
        borderWidth: {{ borderwidth }},
        gaugeData: {
            value: {{dataitems[0].value}},
            {% if dataitems[0].label != '''' %}
                label: ''{{dataitems[0].label}}'',
            {% endif %}
            valueColor: ""{{dataitems[0].fillcolor | Default:''#000000''}}""
        },
        gaugeLimits: {{ gaugeLimits }}
    }
  {%- endcapture -%}
{% else %}
  {% assign dataitemCount = dataitems | Size -%}
  {% if dataitemCount > 0 -%}
      {% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}
      {% assign firstDataItem = dataitems | First  %}

      {% capture seriesData -%}
      {
          fill: {{ filllinearea }},
          backgroundColor: {% if firstDataItem.fillcolor %}{{ fillColors }}{% else %}''{{ fillcolor }}''{% endif %},
          borderColor: {% if firstDataItem.bordercolor %}{{ borderColors }}{% else %}''{{ bordercolor }}''{% endif %},
          borderWidth: {{ borderwidth }},
          pointRadius: {{ pointradius }},
          pointBackgroundColor: ''{{ pointcolor }}'',
          pointBorderColor: ''{{ pointbordercolor }}'',
          pointBorderWidth: {{ pointborderwidth }},
          pointHoverBackgroundColor: ''{{ pointhovercolor }}'',
          pointHoverBorderColor: ''{{ pointhoverbordercolor }}'',
          pointHoverRadius: ''{{ pointhoverradius }}'',
          {% if borderdash != '''' -%} borderDash: {{ borderdash }},{% endif -%}
          {% if curvedlines == false -%} lineTension: 0,{% endif -%}
          data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }},
      }
      {% endcapture -%}
      {% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}
  {% else -%}
      {% if labels == '''' -%}
          <div class=""alert alert-warning"">
              When using datasets you must provide labels on the shortcode to define each unit of measure.
              {% raw %}{[ chart labels:''Red, Green, Blue'' ... ]}{% endraw %}
          </div>
      {% else %}
          {% assign labelItems = labels | Split:'','' -%}
          {% assign labels = ''""'' -%}
          {% for labelItem in labelItems -%}
              {% assign labelItem = labelItem | Trim %}
              {% assign labels = labels | Append:labelItem | Append:''"",""'' %}
          {% endfor -%}
          {% assign labels = labels | ReplaceLast:''"",""'',''""'' %}
      {% endif -%}
      {% assign seriesData = '''' -%}
      {% for dataset in datasets -%}
          {% if dataset.label -%} {% assign datasetLabel = dataset.label %} {% else -%} {% assign datasetLabel = '' '' %} {% endif -%}
          {% if dataset.fillcolor -%} {% assign datasetFillColor = dataset.fillcolor %} {% else -%} {% assign datasetFillColor = fillcolor %} {% endif -%}
          {% if dataset.filllinearea -%} {% assign datasetFillLineArea = dataset.filllinearea %} {% else -%} {% assign datasetFillLineArea = filllinearea %} {% endif -%}
          {% if dataset.bordercolor -%} {% assign datasetBorderColor = dataset.bordercolor %} {% else -%} {% assign datasetBorderColor = bordercolor %} {% endif -%}
          {% if dataset.borderwidth -%} {% assign datasetBorderWidth = dataset.borderwidth %} {% else -%} {% assign datasetBorderWidth = borderwidth %} {% endif -%}
          {% if dataset.pointradius -%} {% assign datasetPointRadius = dataset.pointradius %} {% else -%} {% assign datasetPointRadius = pointradius %} {% endif -%}
          {% if dataset.pointcolor -%} {% assign datasetPointColor = dataset.pointcolor %} {% else -%} {% assign datasetPointColor = pointcolor %} {% endif -%}
          {% if dataset.pointbordercolor -%} {% assign datasetPointBorderColor = dataset.pointbordercolor %} {% else -%} {% assign datasetPointBorderColor = pointbordercolor %} {% endif -%}
          {% if dataset.pointborderwidth -%} {% assign datasetPointBorderWidth = dataset.pointborderwidth %} {% else -%} {% assign datasetPointBorderWidth = pointborderwidth %} {% endif -%}
          {% if dataset.pointhovercolor -%} {% assign datasetPointHoverColor = dataset.pointhovercolor %} {% else -%} {% assign datasetPointHoverColor = pointhovercolor %} {% endif -%}
          {% if dataset.pointhoverbordercolor -%} {% assign datasetPointHoverBorderColor = dataset.pointhoverbordercolor %} {% else -%} {% assign datasetPointHoverBorderColor = pointhoverbordercolor %} {% endif -%}
          {% if dataset.pointhoverradius -%} {% assign datasetPointHoverRadius = dataset.pointhoverradius %} {% else -%} {% assign datasetPointHoverRadius = pointhoverradius %} {% endif -%}

          {%- capture itemData -%}
              {
                  label: ''{{ datasetLabel }}'',
                  fill: {{ datasetFillLineArea }},
                  backgroundColor: ''{{ datasetFillColor }}'',
                  borderColor: ''{{ datasetBorderColor }}'',
                  borderWidth: {{ datasetBorderWidth }},
                  pointRadius: {{ datasetPointRadius }},
                  pointBackgroundColor: ''{{ datasetPointColor }}'',
                  pointBorderColor: ''{{ datasetPointBorderColor }}'',
                  pointBorderWidth: {{ datasetPointBorderWidth }},
                  pointHoverBackgroundColor: ''{{ datasetPointHoverColor }}'',
                  pointHoverBorderColor: ''{{ datasetPointHoverBorderColor }}'',
                  pointHoverRadius: ''{{ datasetPointHoverRadius }}'',
                  {%- if dataset.borderdash and dataset.borderdash != '''' -%} borderDash: {{ dataset.borderdash }},{%- endif -%}
                  {%- if dataset.curvedlines and dataset.curvedlines == ''false'' -%} lineTension: 0,{%- endif -%}
                  data: [{{ dataset.data }}]
              },
          {% endcapture -%}

          {% assign seriesData = seriesData | Append:itemData -%}
      {% endfor -%}
      {% assign seriesData = seriesData | ReplaceLast:'','', '''' -%}
  {% endif -%}
{%- endif -%}

<div class=""chart-container"" style=""position: relative; height:{{ chartheight }}; width:{{ chartwidth }}"">
    <canvas id=""chart-{{ id }}""></canvas>
</div>

<script>

    var options = {
    maintainAspectRatio: false,
    {%- if type != ''tsgauge'' -%}
        legend: {
            position: ''{{ legendposition }}'',
            display: {{ legendshow }}
        },
        tooltips: {
            enabled: {{ tooltipshow }}
            {% if tooltipshow %}
            , backgroundColor: ''{{ tooltipbackgroundcolor }}''
            , bodyFontColor: ''{{ tooltipfontcolor }}''
            , titleFontColor: ''{{ tooltipfontcolor }}''
                {% if valueformat != '''' and valueformat != ''none'' %}
                , callbacks: {
                    label: function(tooltipItem, data) {
                        {% if type == ''pie'' %}
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return data.labels[tooltipItem.index] + "": "" + Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                            {% endcase %}
                        {% else %}
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(tooltipItem.yLabel);
                                {% when ''number'' %}
                                    return Intl.NumberFormat().format(data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index]);
                                {% else %}
                                    return Intl.NumberFormat().format(tooltipItem.yLabel);
                            {% endcase %}
                        {% endif %}
                    }
                }
                {% endif %}
            {% endif %}
        }
        {%- else -%}
        events: [],
        showMarkers: false
        {%- endif -%}
        {% if xaxistype == ''time'' %}
            ,scales: {
                xAxes: [{
                    type: ""time"",
                    display: true,
                    scaleLabel: {
                        display: true,
                        labelString: ''Date''
                    }
                    {% if gridlinecolor != '''' %}
                    , gridLines: {
                        color: ""{{ gridlinecolor }}""
                    }
                    {% endif %}
                }],
                yAxes: [{
                    display: true,
                    {% if gridlinecolor != '''' %}
                    gridLines: {
                        color: ""{{ gridlinecolor }}""
                    },
                    {% endif %}
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                    {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                    {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                    {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                    }
                }]
            }
        {% elseif xaxistype == ''linearhorizontal0to100'' %}
            ,scales: {
                xAxes: [{
                        ticks: {
                            min: 0,
                            max: 100
                        }
                        {% if gridlinecolor != '''' %}
                        , gridLines: {
                            color: ""{{ gridlinecolor }}""
                        }
                        {% endif %}
                }],
                yAxes: [{
                    gridLines: {
                        display: false
                    }
                }]
            }
        {% elseif xaxistype == ''stacked'' %}
        {% if yaxislabels != '''' %}
            {%- assign yaxislabels = yaxislabels | Split:'','' -%}
            {%- assign yaxislabelcount = yaxislabels | Size -%}
        {% else %}
            {%- assign yaxislabelcount = 0 -%}
        {% endif %}
        ,scales: {
            xAxes: [{
                stacked: true,
                {% if gridlinecolor != '''' %}
                gridLines: {
                    color: ""{{ gridlinecolor }}""
                }
                {% endif %}
            }],
            yAxes: [{
                stacked: true
                {% if gridlinecolor != '''' %}
                , gridLines: {
                    color: ""{{ gridlinecolor }}""
                }
                {% endif %}
                {%- if yaxislabelcount > 0 or yaxismin != '''' or yaxismax != '''' or yaxisstepsize != '''' -%}
                , ticks: {
                {% if yaxismin != '''' %}min: {{ yaxismin }}{% endif %}
                {% if yaxismax != '''' %},max: {{ yaxismax }}{% endif %}
                {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                {% if yaxislabelcount > 0 %}
                    ,
                    callback: function(label, index, labels) {
                        switch (label) {
                            {%- for yaxislabel in yaxislabels -%}
                                {%- assign axislabel = yaxislabel | Split:''^'' -%}
                                case {{ axislabel[0] }}: return ''{{axislabel[1]}}'';
                            {%- endfor -%}
                        }
                    }
                {% endif %}
                },
        {% endif %}
            }]
    }
        {%- elseif type != ''pie'' and type != ''tsgauge'' -%}
            ,scales: {
                yAxes: [{
                    {% if gridlinecolor != '''' %}
                    gridLines: {
                        color: ""{{ gridlinecolor }}""
                    },
                    {% endif %}
                    {% if label != null and label != '''' %}
                    scaleLabel: {
                        display: true,
                        labelString: ''{{ label }}''
                    },
                    {% endif %}
                    ticks: {
                        {% if valueformat != '''' and valueformat != ''none'' %}
                        callback: function(label, index, labels) {
                            {% case valueformat %}
                                {% when ''currency'' %}
                                    return Intl.NumberFormat(undefined, { style: ''currency'', currency: ''{{ ''Global'' | Attribute:''OrganizationStandardCurrencyCode'' }}'' }).format(label);
                                {% else %}
                                    return Intl.NumberFormat().format(label);
                            {% endcase %}
                        },
                        {% endif %}
                        {% if yaxismin != '''' %}min: {{ yaxismin }}{%- endif %}
                        {% if yaxismax != '''' %},max: {{ yaxismax }}{%- endif %}
                        {% if yaxisstepsize != '''' %}, stepSize: {{ yaxisstepsize }}{% endif %}
                        },
                }]
                {% if gridlinecolor != '''' %}
                , xAxes: [{
                    gridLines: {
                        color: ""{{ gridlinecolor }}""
                    }
                }]
                {% endif %}
            }
        {% endif %}
    };
    {%- if type == ''tsgauge'' -%}
        var data = {
            datasets: [{{ seriesData }}]
        };
    {%- else -%}
        var data = {
            labels: [{{ labels }}],
            datasets: [{{ seriesData }}],
            borderWidth: {{ borderwidth }}
        };
    {% endif %}

    Chart.defaults.global.defaultFontColor = ''{{ fontcolor }}'';
    Chart.defaults.global.defaultFontFamily = ""{{ fontfamily }}"";

    var ctx = document.getElementById(''chart-{{ id }}'').getContext(''2d'');
    var chart = new Chart(ctx, {
        type: ''{{ type }}'',
        data: data,
        options: options
    });
</script>
' WHERE ([Guid]='43819A34-4819-4507-8FEA-2E406B5474EA');
                " );
        }

        /// <summary>
        /// CR: AchievementsEnabledForFinancialTransactions
        /// </summary>
        private void SetFinancialAchievementsEnabledUp()
        {
            Sql( $@"UPDATE [EntityType] SET [IsAchievementsEnabled] = 1 WHERE [Guid] = '{ Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION }'" );
        }

        /// <summary>
        /// GJ: Panel Shortcode Update
        /// </summary>
        private void PanelShortcodeUpdate()
        {
            Sql( @"UPDATE [LavaShortcode] SET [Documentation]=N'<p>
The panel shortcode allows you to easily add a 
<a href=""https://community.rockrms.com/styling/components/panels"" target=""_blank"">Bootstrap panel</a> to your markup. This is a pretty simple shortcode, but it does save you some time.
</p>

<p>Basic Usage:<br>  
</p><pre>{[ panel title:''Important Stuff'' icon:''fa fa-star'' ]}<br>  
This is a super simple panel.<br> 
{[ endpanel ]}</pre>

<p></p><p>
As you can see the body of the shortcode is placed in the body of the panel. Optional parameters include:
</p>

<ul>
<li><strong>title</strong> – The title to show in the heading. If no title is provided then the panel title section will not be displayed.</li>
<li><strong>icon </strong> – The icon to use with the title.</li>
<li><strong>footer</strong> – If provided the text will be placed in the panel’s footer.</li>
<li><strong>type</strong> (default) – Change the type of panel displayed. Options include: default, primary, success, info, warning, danger, block and widget.</li>
<li><strong>style</strong> – The inline style containing CSS styling declarations for a panel element. Example: <code>style:''height:100%''</code></li>
</ul>', [IsSystem]='1', [IsActive]='1', [TagName]=N'panel', [Markup]=N'<div class=""panel panel-{{ type }}"" {% if style != '''' %}style=""{{ style }}""{% endif %}>
  {% if title != '''' %}
      <div class=""panel-heading"">
        <h3 class=""panel-title"">
            {% if icon != '''' %}
                <i class=""{{ icon }}""></i> 
            {% endif %}
            {{ title }}</h3>
      </div>
  {% endif -%}
  <div class=""panel-body"">
    {{ blockContent  }}
  </div>
  {% if footer != '''' %}
    <div class=""panel-footer"">{{ footer }}</div>
  {% endif %}
</div>', [TagType]='2', [EnabledLavaCommands]=N'', [Parameters]=N'type^default|icon^|title^|footer^|style^' WHERE ([Guid]='ADB1F75D-500D-4305-9805-99AF04A2CD88');" );
        }

        /// <summary>
        /// GJ: Show/Hide axis labels on Chart shortcode
        /// </summary>
        private void AxisLabelsOnChartShortcode()
        {
            Sql( MigrationSQL._202211031956456_Rollup_20221103_AxisLabelsOnChartShortcode );
        }

        /// <summary>
        /// MP: Update Check Scanner URL.
        /// </summary>
        private void UpdateCheckScannerUrlUp()
        {
            Sql( @"UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.14.1/checkscanner.msi'
                WHERE ([Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180');" );
        }

        /// <summary>
        /// MP: Update Check Scanner Down.
        /// </summary>
        private void UpdateCheckScannerUrlDown()
        {
            Sql( @"UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/checkscanner/1.10.4/checkscanner.msi'
                WHERE ([Guid] = '82960DBD-2EAA-47DF-B9AC-86F7A2FCA180');" );
        }

        /// <summary>
        /// GJ: Update Content Collection View Block Default Item Template
        /// </summary>
        private void ContentCollectionViewBlockDefaultItemTemplate()
        {
            // Attribute for BlockType
            //   BlockType: Content Library View
            //   Category: CMS
            //   Attribute: Item Template
RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CC387575-3530-4CD6-97E0-1F449DCA1869", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Item Template", "ItemTemplate", "Item Template", @"The lava template to use to render a single result.", 0, @"<div class=""result-item"">
    <h4 class=""mt-0"">{{ Item.Name }}</h4>
    <div class=""mb-3"">
    {{ Item.Content | StripHtml | Truncate:300 }}
    </div>
    <a href=""#"" class=""stretched-link"">Read More</a>
</div>", "CE511E4F-E2DE-4C62-877E-DCE1323F1FC9" );
        }
    }
}
