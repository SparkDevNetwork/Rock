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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 56, "1.8.3" )]
    public class MigrationRollupsForV8_4 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //CorrectContentChannelSeries();
            //UpdateInteractionForeignKeyIndex();
            //FixShortcodeFunctionality();
            //AddDefaultBackgroundCheckSystemSetting();
            //EnableNCOA();
            //FixLavaIfStatementIssues();
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Fixes the lava *If* statement issues.
        /// </summary>
        private void FixLavaIfStatementIssues()
        {
            Sql( @"
-- Fix Body AttributeValue for Complete Notify Originator for the Person Data Error workflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Workflow | Attribute:''ReportedBy'' }},</p>
<p>The data error that you reported for {{ Workflow.Name }} has been completed.<p>

<h4>Details:</h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

{% assign Resolution = Workflow | Attribute:''Resolution'' %}
{% if Resolution != empty %}

    <h4>Resolution:</h4>
    <p>{{ Resolution }}</p>

{% endif %}

{{ ''Global'' | Attribute:''EmailFooter'' }}

'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}

<p>{{ Workflow | Attribute:''ReportedBy'' }},</p>
<p>The data error that you reported for {{ Workflow.Name }} has been completed.<p>

<h4>Details:</h4>
<p>{{ Workflow | Attribute:''Details'' }}</p>

{% if Workflow | Attribute:''Resolution'' != Empty %}

    <h4>Resolution:</h4>
    <p>{{ Workflow | Attribute:''Resolution'' }}</p>

{% endif %}

{{ ''Global'' | Attribute:''EmailFooter'' }}

';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Approved Email Requester for the Position Approval workflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for the posting process.
</p>
{% assign ApprovalNotes = Workflow | Attribute:''ApprovalNotes'' %}
{% if ApprovalNotes != empty %}
    <b>Approval Notes:</b>
    <p>
        {{ ApprovalNotes }}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for the posting process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------


-- Fix Body AttributeValue for Approved Email HR for the Position Approval workflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for posting the position.
</p>
{% assign ApprovalNotes = Workflow | Attribute:''ApprovalNotes'' %}
{% if ApprovalNotes != empty %}
    <b>Approval Notes:</b>
    <p>
        {{ ApprovalNotes }}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}' 
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position has been 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for posting the position.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Denied Email Requester for the Position Approval workflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position was not approved by
    {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for this process.
</p>
{% assign ApprovalNotes = Workflow | Attribute:''ApprovalNotes'' %}
{% if ApprovalNotes != empty %}
    <b>Approval Notes:</b>
    <p>
        {{ ApprovalNotes }}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    {{Person.NickName}}, your request for the {{Workflow | Attribute:''PositionTitle''}} position was not approved by
    {{Workflow | Attribute:''Approver''}}. HR will be getting with you soon to arrange next steps
    for this process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------

-- Fix Subject AttributeValue for Approved Email Requester for the Position Approval workflow
-- Fix Subject AttributeValue for Approved Email HR for the Position Approval workflow
-- Fix Subject AttributeValue for Denied Email Requester for the Position Approval workflow
-- Fix Subject AttributeValue for Denied Email HR for the Position Approval workflow
UPDATE [AttributeValue]
SET [Value] = 'UPDATE: {{Workflow | Attribute:''PositionTitle''}}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '5D9B13B6-CD96-4C7C-86FA-4512B9D28386')
	AND [Value] = 'UPDATE: {{Workflow.PositionTitle}}';
----------------------------------------------------------------------------------------------------------------

-- Fix Body AttributeValue for Denied Email HR for the Position Approval workflow
UPDATE [AttributeValue]
SET [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position was not 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for this process.
</p>
{% assign ApprovalNotes = Workflow | Attribute:''ApprovalNotes'' %}
{% if ApprovalNotes != empty %}
    <b>Approval Notes:</b>
    <p>
        {{ ApprovalNotes }}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}'
WHERE AttributeId in (select Id from Attribute where [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
	AND [Value] = '{{ ''Global'' | Attribute:''EmailHeader'' }}
<p>
    The request for the {{Workflow | Attribute:''PositionTitle''}} position was not 
    approved by {{Workflow | Attribute:''Approver''}}. Please follow up with {{Workflow | Attribute:''Requester''}} with
    next steps for this process.
</p>
{% if Workflow | Attribute:''ApprovalNotes'' != null %}
    <b>Approval Notes:</b>
    <p>
        {{Workflow | Attribute:''ApprovalNotes''}}
    </p>
{% endif %}
{{ ''Global'' | Attribute:''EmailFooter'' }}';
----------------------------------------------------------------------------------------------------------------
" );




            Sql( @"UPDATE [WorkflowActionForm]
SET [Header] = '{% assign WarnOfRecent = Workflow | Attribute:''WarnOfRecent'' %}<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the form below to start the background
    request process.
</p>
{% if WarnOfRecent == ''Yes'' %}
    <div class=''alert alert-warning''>
        Notice: It''s been less than a year since this person''s last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />'
WHERE [Guid] = '644D005C-CC28-4050-994C-C6E53A930F69'" );
        }

        /// <summary>
        /// Enables the NCOA Job
        /// </summary>
        private void EnableNCOA()
        {
            // GP: Enable NCOA
            Sql( @"
DELETE FROM [dbo].[Auth] WHERE [Guid] = '8628653e-f4d8-42b4-4793-38647fada3f1'
UPDATE [dbo].[ServiceJob]
   SET [IsActive] = 1, [Description] = 'Job that gets National Change of Address (NCOA) data'
 WHERE Guid = 'D2D6EA6C-F94A-39A0-481B-A23D08B887D6'
" );
        }


        /// <summary>
        /// ED: Add DetailPage block attribute to ContentChannelViewDetail and correct example
        /// </summary>
        private void CorrectContentChannelSeries()
        {
            string oldValue = @"<style>
 .series-banner {
  height: 220px;
  background-size: cover;
  background-position: center center;
  background-repeat: no-repeat;
 }
 @media (min-width: 992px) {
  .series-banner {
   height: 420px;
  }
 }
 .series-title{
  margin-bottom: 4px;
 }
 .series-dates {
  opacity: .6;
 }
 .messages-title {
  font-size: 24px;
 }
 .messages {
  font-size: 18px;
 }
</style>
{% if Item  %}
 <article class=""series-detail"">
  {% assign seriesImageGuid = Item | Attribute:''SeriesImage'',''RawValue'' %}
  <div class=""series-banner"" style=""background-image: url(''/GetImage.ashx?Guid={{ seriesImageGuid }}'');"" ></div>
  <h1 class=""series-title"">{{ Item.Title }}</h1>
  <p class=""series-dates"">
   <strong>{{ Item.StartDateTime | Date:''M/d/yyyy'' }}
    {% if Item.StartDateTime != Item.ExpireDateTime %}
     - {{ Item.ExpireDateTime | Date:''M/d/yyyy'' }}
    {% endif %}
   </strong>
  </p>

  <script>function fbs_click() { u = location.href; t = document.title; window.open(''http://www.facebook.com/sharer.php?u='' + encodeURIComponent(u) + ''&t='' + encodeURIComponent(t), ''sharer'', ''toolbar=0,status=0,width=626,height=436''); return false; }</script>
  <script>function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace(''END:VEVENT'', ''SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT''); var element = document.createElement(''a''); element.setAttribute(''href'', ''data:text/plain;charset=utf-8,'' + encodeURIComponent(text)); element.setAttribute(''download'', ''{{ Event.Name }}.ics''); element.style.display = ''none''; document.body.appendChild(element); element.click(); document.body.removeChild(element); }</script>
  <ul class=""socialsharing"">
   <li>
    <a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
     <i class=""fa fa-fw fa-facebook""></i>
    </a>
   </li>
   <li>
    <a href=""http://twitter.com/home?status={{ ''Global'' | Page:''Url'' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
     <i class=""fa fa-fw fa-twitter""></i>
    </a>
   </li>
   <li>
    <a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ ''Global'' | Page:''Url'' }}""  class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
     <i class=""fa fa-fw fa-envelope-o""></i>
    </a>
   </li>
  </ul>
  <div class=""margin-t-lg"">
   {{ Item.Content }}
  </div>
  <h4 class=""messages-title margin-t-lg"">In This Series</h4>
  <ol class=""messages"">
   {% for message in Item.ChildItems %}
    <li>
     <a href=""/page/461?Item={{ message.ChildContentChannelItem.Id }}"">
      {{ message.ChildContentChannelItem.Title }}
     </a>
        </li>
   {% endfor %}
  </ol>
 </article>
{% else %}
 <h1>Could not find series.</h1>
{% endif %}";
            string newValue = @"<style>
 .series-banner {
  height: 220px;
  background-size: cover;
  background-position: center center;
  background-repeat: no-repeat;
 }
 @media (min-width: 992px) {
  .series-banner {
   height: 420px;
  }
 }
 .series-title{
  margin-bottom: 4px;
 }
 .series-dates {
  opacity: .6;
 }
 .messages-title {
  font-size: 24px;
 }
 .messages {
  font-size: 18px;
 }
</style>
{% if Item  %}
 <article class=""series-detail"">
  {% assign seriesImageGuid = Item | Attribute:''SeriesImage'',''RawValue'' %}
  <div class=""series-banner"" style=""background-image: url(''/GetImage.ashx?Guid={{ seriesImageGuid }}'');"" ></div>
  <h1 class=""series-title"">{{ Item.Title }}</h1>
  <p class=""series-dates"">
   <strong>{{ Item.StartDateTime | Date:''M/d/yyyy'' }}
    {% if Item.StartDateTime != Item.ExpireDateTime %}
     - {{ Item.ExpireDateTime | Date:''M/d/yyyy'' }}
    {% endif %}
   </strong>
  </p>

  <script>function fbs_click() { u = location.href; t = document.title; window.open(''http://www.facebook.com/sharer.php?u='' + encodeURIComponent(u) + ''&t='' + encodeURIComponent(t), ''sharer'', ''toolbar=0,status=0,width=626,height=436''); return false; }</script>
  <script>function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace(''END:VEVENT'', ''SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT''); var element = document.createElement(''a''); element.setAttribute(''href'', ''data:text/plain;charset=utf-8,'' + encodeURIComponent(text)); element.setAttribute(''download'', ''{{ Event.Name }}.ics''); element.style.display = ''none''; document.body.appendChild(element); element.click(); document.body.removeChild(element); }</script>
  <ul class=""socialsharing"">
   <li>
    <a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
     <i class=""fa fa-fw fa-facebook""></i>
    </a>
   </li>
   <li>
    <a href=""http://twitter.com/home?status={{ ''Global'' | Page:''Url'' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
     <i class=""fa fa-fw fa-twitter""></i>
    </a>
   </li>
   <li>
    <a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ ''Global'' | Page:''Url'' }}""  class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
     <i class=""fa fa-fw fa-envelope-o""></i>
    </a>
   </li>
  </ul>
  <div class=""margin-t-lg"">
   {{ Item.Content }}
  </div>
  <h4 class=""messages-title margin-t-lg"">In This Series</h4>
  <ol class=""messages"">
   {% for message in Item.ChildItems %}
    <li>
     <a href=""/page/{{ DetailPage }}?Item={{ message.ChildContentChannelItem.Id }}"">
      {{ message.ChildContentChannelItem.Title }}
     </a>
        </li>
   {% endfor %}
  </ol>
 </article>
{% else %}
 <h1>Could not find series.</h1>
{% endif %}";
            RockMigrationHelper.UpdateBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "47C56661-FB70-4703-9781-8651B8B49485", newValue, oldValue );
            // Attrib for BlockType: Content Channel View Detail:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "63659EBE-C5AF-4157-804A-55C7D565110E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"Page used to view a content item.", 1, @"", "769F832A-778B-454D-A9A6-0CC49E748547" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Detail Page Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "769F832A-778B-454D-A9A6-0CC49E748547", @"461" );
        }

        /// <summary>
        /// MP: Update Interaction.ForeignKey Index
        /// </summary>
        private void UpdateInteractionForeignKeyIndex()
        {
            Sql( @"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV84DataMigrations'
   AND[Guid] = '79FBDA04-ADFD-40D4-824F-E07D660F7858'
  )
BEGIN
 INSERT INTO[ServiceJob](
  [IsSystem]
  ,[IsActive]
  ,[Name]
  ,[Description]
  ,[Class]
  ,[CronExpression]
  ,[NotificationStatus]
  ,[Guid]
  )
 VALUES(
  0
  ,1
  ,'Data Migrations for v8.4'
  ,'This job will take care of any data migrations that need to occur after updating to v8.4. After all the operations are done, this job will delete itself.'
  ,'Rock.Jobs.PostV84DataMigrations'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'79FBDA04-ADFD-40D4-824F-E07D660F7858'
  );
        END" );
        }

        /// <summary>
        /// GJ: Fix Shortcode Functionality
        /// </summary>
        private void FixShortcodeFunctionality()
        {
            // GJ: Fix Wordcloud Shortcode
            Sql( @"UPDATE [LavaShortCode] 
SET [Markup] = '{% javascript id:''d3-layout-cloud'' url:''~/Scripts/d3-cloud/d3.layout.cloud.js'' %}{% endjavascript %}
{% javascript id:''d3-min'' url:''~/Scripts/d3-cloud/d3.min.js'' %}{% endjavascript %}

<div id=""{{ uniqueid }}"" style=""width: {{ width }}; height: {{ height }};""></div>

{%- assign anglecount = anglecount | Trim -%}
{%- assign anglemin = anglemin | Trim -%}
{%- assign anglemax = anglemax | Trim -%}

{% javascript disableanonymousfunction:''true'' %}
    $( document ).ready(function() {
        Rock.controls.wordcloud.initialize({
            inputTextId: ''hf-{{ uniqueid }}'',
            visId: ''{{ uniqueid }}'',
            width: ''{{ width }}'',
            height: ''{{ height }}'',
            fontName: ''{{ fontname }}'',
            maxWords: {{ maxwords }},
            scaleName: ''{{ scalename }}'',
            spiralName: ''{{ spiralname}}'',
            colors: [ ''{{ colors | Replace:'','',""'',''"" }}''],
            {%- if anglecount != '''' %}
            anglecount: {{ anglecount }}{%- if anglemin != '''' or anglemax != '''' -%},{%- endif -%}
            {%- endif -%}
            {%- if anglemin != '''' %}
            anglemin: {{ anglemin }}{%- if anglemax != '''' -%},{%- endif -%}
            {%- endif -%}
            {%- if anglemax != '''' %}
            anglemax: {{ anglemax }}
            {%- endif -%}
        });
    });
{% endjavascript %}

<input type=""hidden"" id=""hf-{{ uniqueid }}"" value=""{{ blockContent }}"" />' WHERE [Guid] = 'CA9B54BF-EF0A-4B08-884F-7042A6B3EAF4'" );

            // GJ: Fix Parallax Shortcode
            Sql( @"UPDATE [LavaShortCode] 
SET [Markup] = '{%- javascript url:''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax.min.js'' id:''jarallax-shortcode'' -%}{%- endjavascript -%}
{% if videourl != '''' -%}
    {%- javascript url:''https://cdnjs.cloudflare.com/ajax/libs/jarallax/1.9.2/jarallax-video.min.js'' id:''jarallax-video-shortcode'' -%}{%- endjavascript -%}
{% endif -%}

{% assign id = uniqueid -%} 
{% assign bodyZindex = zindex | Plus:1 -%}

{% assign speed = speed | AsInteger %}

{% if speed > 0 -%}
    {% assign speed = speed | Times:''.01'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% elseif speed == 0 -%}
    {% assign speed = 1 -%}
{% else -%}
    {% assign speed = speed | Times:''.02'' -%}
    {% assign speed = speed | Plus:''1'' -%}
{% endif -%}


 
{% if videourl != ''''- %}
    <div id=""{{ id }}"" class=""jarallax"" data-jarallax-video=""{{ videourl }}"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
{% else- %} 
    <div id=""{{ id }}"" data-jarallax class=""jarallax"" data-type=""{{ type }}"" data-speed=""{{ speed }}"" data-img-position=""{{ position }}"" data-object-position=""{{ position }}"" data-background-position=""{{ position }}"" data-zindex=""{{ bodyZindex }}"" data-no-android=""{{ noandroid }}"" data-no-ios=""{{ noios }}"">
        <img class=""jarallax-img"" src=""{{ image }}"" alt="""">
{% endif -%}

        {% if blockContent != '''' -%}
            <div class=""parallax-content"">
                {{ blockContent }}
            </div>
        {% else- %}
            {{ blockContent }}
        {% endif -%}
    </div>

{% stylesheet %}
#{{ id }} {
    /* eventually going to change the height using media queries with mixins using sass, and then include only the classes I want for certain parallaxes */
    min-height: {{ height }};
    background: transparent;
    position: relative;
    z-index: 0;
}

#{{ id }} .jarallax-img {
    position: absolute;
    object-fit: cover;
    /* support for plugin https://github.com/bfred-it/object-fit-images */
    font-family: ''object-fit: cover;'';
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: -1;
}

#{{ id }} .parallax-content{
    display: inline-block;
    margin: {{ contentpadding }};
    color: {{ contentcolor }};
    text-align: {{ contentalign }};
	width: 100%;
}
{% endstylesheet %}' WHERE [Guid] = '4b6452ef-6fea-4a66-9fb9-1a7cce82e7a4'" );

        }

        /// <summary>
        /// NA: Add default background check provider system setting
        /// </summary>
        private void AddDefaultBackgroundCheckSystemSetting()
        {
            RockMigrationHelper.UpdateSystemSetting( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, "" );
        }

        /// <summary>
        /// MP: Improve performance of Family Analytics Job
        /// </summary>
        private void ImproveFamilyAnalyticsJobPerf()
        {
            Sql( HotFixMigrationResource._056_MigrationRollupsForV8_4_spCrm_FamilyAnalyticsEraDataset );
        }

    }
}
