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
using System.Collections.Generic;

namespace Rock.Tests.Performance.Modules.Lava
{
    public static class TestData
    {
        public static List<TestDataTemplateItem> GetLavaTemplates()
        {
            var templates = new List<TestDataTemplateItem>();

            // ** Requires a populated Spark database to execute.
            //templates.Add( new TestDataTemplateItem { TestName = "Spark Release Notes", TemplateText = _template1 } );
            templates.Add( new TestDataTemplateItem { TestName = "Hello World", TemplateText = _basicHelloWorld } );
            templates.Add( new TestDataTemplateItem { TestName = "Organisations Map", TemplateText = _template2 } );
            templates.Add( new TestDataTemplateItem { TestName = "RockU", TemplateText = _template3 } );

            // Replace placeholder character for quoted string delimiters (`).
            for ( int i = 0; i < templates.Count; i++ )
            {
                templates[i].TemplateText = templates[i].TemplateText.Replace( "`", @"""" );
            }

            return templates;
        }

        /// <summary>
        /// Lava Template: Hello World
        /// </summary>
        private const string _basicHelloWorld = @"
{% assign message = 'Hello World!' %}
{{ message }}
";

        /// <summary>
        /// Spark Release Notes
        /// </summary>
        private const string _template1 = @"
Release Notes:



<script>
    $(function () {
      $('[data-toggle=`tooltip`]').tooltip()
    })
</script>
<ul>
{% sql %}
SELECT
      [RockVersionValueId]
      ,[RockPreAlphaVersionValueId]
      ,[RockDomainValueId]
      ,[ReleaseNoteTypeValueId]
      ,[IssueFixes]
      ,[ReleaseNote]
      , v.[Value] AS [Version]
      , rd.[Value] AS [ReleaseDate]
      , cv.[Value] AS [CondensedVersion]
      , d.[Value] AS [Domain]
      , t.[Value] AS [NoteType]
      , ic.[Value] AS [NoteTypeIcon]
      , vn.[Value] AS [VersionNote]
      , ia.[Value] AS [InAlpha]
      , ib.[Value] AS [InBeta]
      , ip.[Value] AS [InProduction]
      , wv.[Value] AS [WistiaVideo]
  FROM [_org_sparkdevnetwork_Core_ReleaseNotes] r
  INNER JOIN [DefinedValue] v ON v.[Id] = r.[RockVersionValueId]
  LEFT OUTER JOIN [AttributeValue] rd ON rd.[EntityId] = v.[Id] AND rd.[AttributeId] = 13229
  LEFT OUTER JOIN [AttributeValue] cv ON cv.[EntityId] = v.[Id] AND cv.[AttributeId] = 1936
  LEFT OUTER JOIN [AttributeValue] vn ON vn.[EntityId] = v.[Id] AND vn.[AttributeId] = 14459
  LEFT OUTER JOIN [AttributeValue] ia ON ia.[EntityId] = v.[Id] AND ia.[AttributeId] = 15006
  LEFT OUTER JOIN [AttributeValue] ib ON ib.[EntityId] = v.[Id] AND ib.[AttributeId] = 15005
  LEFT OUTER JOIN [AttributeValue] ip ON ip.[EntityId] = v.[Id] AND ip.[AttributeId] = 15004
  LEFT OUTER JOIN [AttributeValue] wv ON wv.[EntityId] = v.[Id] AND wv.[AttributeId] = 22582
  INNER JOIN [DefinedValue] d ON d.[Id] = r.[RockDomainValueId]
  LEFT OUTER JOIN [AttributeValue] di ON di.[EntityId] = d.[Id] AND di.[AttributeId] = 13229
  INNER JOIN [DefinedValue] t ON t.[Id] = r.[ReleaseNoteTypeValueId]
  LEFT OUTER JOIN [AttributeValue] ic ON ic.[EntityId] = t.[Id] AND ic.[AttributeId] = 13257
  WHERE (
      rd.[Value] != '' OR
      ib.[Value] = 'True' OR
      v.[Value] = '{{ PageParameter.Version }}' AND
      [ReleaseNoteSystemValueId] = 6300 -- Rock Core release notes
  )
  ORDER BY v.[Order] desc, d.[Order]



 {% endsql %}

 {% assign currentRelease = `` %}
 {% assign currentDomain = `` %}
 {% assign headingStyle = `` %}

 {% for item in results %}

    {% if currentRelease != item.RockVersionValueId %}

        {% if currentRelease != `` %}
            </ul>
            {% assign headingStyle = `margin-top: 85px !important;` %}
        {% endif %}

        <a name=`{{ item.CondensedVersion }}`></a>
        <h2 class=`margin-t-lg` style=`{{ headingStyle }}`>Rock {{ item.CondensedVersion }}
        {% if item.ReleaseDate and item.ReleaseDate != `` %}
            <small>Released {{ item.ReleaseDate | Date:'MMMM d, yyyy' }}</small>
        {% else %}

        {% endif %}

        {% if item.InProduction and item.InProduction == `True` %}
        {% elseif item.InBeta and item.InBeta == `True` %}
                <small>(Currently in Beta)</small>
        {% elseif item.InAlpha and item.InAlpha == `True` %}
                <small>(Currently in Alpha)</small>
        {% endif %}

        </h2>
        <hr style=`margin-top: 0; margin-bottom: 24px;`>
        {% if item.WistiaVideo and item.WistiaVideo != '' %}
        <div class=`alert alert-info`>
            Watch the <a href=`https://www.rockrms.com/releasenotes/{{ item.Version }}/video`>{{ item.CondensedVersion }} release video</a> to learn about this  release of Rock RMS.
        </div>
        {% endif %}

        {% if item.VersionNote != '' %}
            {{ item.VersionNote }}
        {% endif %}

        {% assign currentRelease = item.RockVersionValueId %}
        {% assign currentDomain = `` %}
    {% endif %}

    {% if currentDomain != item.RockDomainValueId %}

        {% if currentDomain != `` %}
            </ul>
        {% endif %}

        <h3 class=`margin-t-sm` id=`{{ item.CondensedVersion }}-{{ item.Domain | ToCssClass }}`>{{ item.Domain }} <small style=`font-size:14px`>{{ item.CondensedVersion }}</small></h3>
        {% assign currentDomain = item.RockDomainValueId %}

        <ul class=`fa-ul margin-b-lg`>
    {% endif %}

        <li class=`margin-b-sm`>
            <i class='fa-li {{ item.NoteTypeIcon }}' data-toggle=`tooltip` data-placement=`top` title=`{{ item.NoteType }}`></i>
            {{ item.ReleaseNote }}

            {% if item.IssueFixes and item.IssueFixes != `` %}
                {% assign fixes = item.IssueFixes | Split:',' %}
                Fixes:
                {% for fix in fixes %}
                    <a href=`https://github.com/SparkDevNetwork/Rock/issues/{{ fix }}`>#{{ fix }} </a>
                {% endfor %}
            {% endif %}
        </li>

 {% endfor %}

 </ul>

";

        /// <summary>
        /// Organisations Map
        /// </summary>
        private const string _template2 = @"
Organizations Map:


{% capture mapContent %}
{%- cache key:'organization-map-content' duration:'3600' twopass:'false' -%}
{% group where:'GroupTypeId == 24 && IsVendor != `Yes` && DisplayOnWebsite != `No` && ImplementationStatus == `PRODUCTION` || ImplementationStatus == `IMPLEMENTING`' %}
        {%- for group in groupItems -%}
            {%- for GroupLocation in group.GroupLocations -%}
                {%- if GroupLocation.Location.GeoPoint and GroupLocation.Location.GeoPoint != '' -%}
                     {%- assign icon = 'https://raw.githubusercontent.com/Concept211/Google-Maps-Markers/master/images/marker_orange.png' -%}
                     {%- assign status = group | Attribute:'ImplementationStatus','RawValue' -%}
                     {%- if status == `IMPLEMENTING` -%}
                        {%- assign icon = 'https://raw.githubusercontent.com/Concept211/Google-Maps-Markers/master/images/marker_red.png' -%}
                     {%- endif -%}
                     [[ marker location:'{{ GroupLocation.Location.GeoPoint }}' title:'{{ group.Name }}' icon:'{{ icon }}']]<strong>{{ group.Name }}</strong><br>{{ GroupLocation.Location.City }}, {{ GroupLocation.Location.State }}<br>{{ group | Attribute:'ImplementationStatus' }}
                    [[ endmarker ]]
                {%- endif -%}
            {%- endfor -%}
        {%- endfor -%}
{% endgroup %}
{%- endcache -%}
{% endcapture %}
{[ googlemap ]}
{{ mapContent }}
{[ endgooglemap ]}



<style>



.map-container {
    width: 100%;
}



.map-container div {
    position: relative;
}



.legendcontainer{
    margin: 20px;
    height:50px;
}
.marker{
    padding:10px;
    display:flex;
    align-items: center;
    float:left;
}
.markercomponent1 {
    float: left;
    position: relative;
}



.markercomponent2 {
    float: right;
    width: 100%;
    text-indent: 10px;
    font-size: 15px;
}
.map-container{
    box-shadow:0px 0px 10px 2px #a9a9a9;
}
.marker img {
    height:30px;
}
</style>



<div class=`legendcontainer row`>
    <div class=`col-lg-3 col-md-5 marker`>
        <div class=`markercomponent1`><img src=`https://raw.githubusercontent.com/Concept211/Google-Maps-Markers/master/images/marker_red.png`></div>
        <div class=`markercomponent2`>Working on implementing</div>
    </div>
    <div class=`col-lg-3 col-md-5 marker`>
        <div class=`markercomponent1`><img src=`https://raw.githubusercontent.com/Concept211/Google-Maps-Markers/master/images/marker_orange.png`></div>
        <div class=`markercomponent2`>Running in production</div>
    </div>
</div>

";

        /// <summary>
        /// RockU 
        /// </summary>
        private const string _template3 = @"
RockU:



<style>
.card {
    height: 100%;
    min-height: 250px;
    background-color: #fff;
    border-radius: 2px;
}
</style>



{% assign isLoggedIn = false | AsBoolean %}
{% if CurrentPerson %}
{% assign isLoggedIn = true %}
{% endif %}
{% if isLoggedIn %}
  {%- sql return:'watched' -%}
    SELECT
        [Interaction].[InteractionComponentId],
        [InteractionComponent].Name,
        MAX( [Interaction].[InteractionLength] ) AS Complete
    FROM [Interaction]
    INNER JOIN [InteractionComponent] ON [Interaction].[InteractionComponentId] = [InteractionComponent].[Id]
    INNER JOIN [PersonAlias] ON [PersonAlias].[Id] = [Interaction].[PersonAliasId]
    WHERE
        [InteractionComponent].[InteractionChannelId] = 23
        AND [PersonAlias].[PersonId] = {{ CurrentPerson.Id }}
    GROUP BY
    [Interaction].[InteractionComponentId],
    [InteractionComponent].Name
    ORDER BY [InteractionComponent].Name
  {%- endsql -%}
{% endif %}
<script>
  $(function() {
    var playbackProgress = localStorage.getItem('playbackProgress');
    if (playbackProgress) {
        playbackProgress = JSON.parse(playbackProgress);
        $.each( playbackProgress, function( key, value ) {
          console.log( key + `: ` + value );
          var vidLink = $('#' + key);
          console.log(vidLink.data('duration') + ' ' + value);
          var watched = Math.floor(vidLink.data('duration') * value * 0.01);
          // vidLink.attr('data-sec-watched',watched)
          if(value > 85) {
              vidLink.find('.video-complete-icon').show();
          }
        });
    }

    $('.domainlist-item').each(function(){
        var sum = 0;
        $(this).find(`.video-entry`).each(function(){
            sum += $(this).data('sec-watched');
        });
        var progressBar = $(this).find('.progress-bar').first();
        var percent = Math.floor((sum / progressBar.data('seconds-total')) * 100);
        var percent = percent + '%';
        progressBar.attr('data-seconds-watched',sum);
        progressBar.css({'width': percent});
    });
  });
</script>



<div class=`row domainlist`>



  {%- assign sixtydaysago = 'Now' | Date:'yyyy-MM-ddTHH:mm:sszzz' | DateAdd:-60 -%}
  {% definedvalue where:'DefinedTypeId == 93' sort:'Order' %}
    {% for domain in definedvalueItems %}
      {% if domain.Id != 5859 %}
          {% assign minTotal = 0 | AsInteger %}
          {% assign secTotal = 0 | AsInteger %}

          <div class=`col-md-4 col-sm-6 mb-5`>
            <div class=`card card-accent card-hover domainlist-item`>
              <a href=`/rocku/{{ domain.Value | Remove:'(' | Remove:')' | ToCssClass }}` name=`{{ domain.Value | Replace:' ','' | Downcase }}` class=`stretched-link`></a>
              <div class=`card-body`>
                  <div class=`info-icon`>
                    <i class='{{ domain | Attribute:'IconCSSClass' }}'></i>
                  </div>
                  <h4 class=`mt-1`>{{ domain.Value }}</h4>
                  <p>{{ domain.Description }}</p>
                  {% contentchannelitem where:'ContentChannelId == 33 && Domain == `{{ domain.Guid }}`' sort:'Order' %}
                    <ul class=`list-group hidden`>
                      {% for video in contentchannelitemItems %}
                        {% assign wistiaid = video | Attribute:'WistiaEmbedCode' | Prepend:'wistia_' %}
                        {% assign length =  video | Attribute:'Length' %}
                        {% assign lsize = length | Size | Minus:3 %}
                        {% assign min = length | Slice:0,lsize %}
                        {% assign sec = length | Slice:-2,2 %}

                        {% assign minTotal = minTotal | Plus:min %}
                        {% assign secTotal = secTotal | Plus:sec %}
                        {% assign duration = min | Times:60 | Plus:sec %}
                        {% assign percentWatched = 0 %}

                        {% if isLoggedIn %}
                            {% assign percentWatched = watched | Where:'Name',video.Title | Select:'Complete' %}
                            {% assign percentWatched = percentWatched | First | AsDecimal %}

                            {% if percentWatched > 0.85 %}{% assign percentWatched = 1 %}{% endif %}
                            {% if percentWatched > 0 %}
                            {% assign secWatched = duration | Times:percentWatched | Floor %}
                            {% else %}
                            {% assign secWatched = '0' | ToString %}
                            {% endif %}
                        {% endif %}

                        {% capture link %}/rocku/{{ domain.Value | Remove:'(' | Remove:')' | ToCssClass }}/{{ video.PrimarySlug }}{% endcapture %}
                        <li id=`{{ wistiaid }}` class=`list-group-item video-entry` data-duration=`{{ duration }}` data-sec-watched=`{{ secWatched }}` data-wistia-id=`{{ wistiaid }}`>
                        </li>
                      {% endfor %}

                    </ul>
                  {% endcontentchannelitem %}



              </div>
              <div class=`card-footer`>
                  <span class=`pull-left`><span class=`badge px-3` style=`color:#3D3D3D;font-weight:400;`>{{ contentchannelitemItems | Size }} Videos</span></span>
                  {% if isLoggedIn %} <div class=`pull-right small text-muted text-right`>
                      <div class=`progress` style=`width:93px;height:6px;background:#fff;box-shadow:none;border:1px solid #E2E2E2;border-radius:3px;margin:0;`>
                        <div class=`progress-bar` role=`progressbar` aria-valuemin=`0` aria-valuemax=`100` data-seconds-total=`{{ minTotal | Times:60 | Plus:secTotal }}` style=`width: 0%;`></div>
                      </div>
                      {% capture regex %}\!H\h m\m{% endcapture %}
                      {{ 'Now' | ToMidnight | DateAdd:minTotal,'m' | DateAdd:secTotal,'s' | Date:regex | Replace:'!0h ','' | Remove:'!' }}
                      {% comment %}{{ 'Now' | ToMidnight | DateAdd:minTotal,'m' | DateAdd:secTotal,'s' | Date:'\!H\h m\m' | Replace:'!0h ','' | Remove:'!' }}{% endcomment %}
                  </div> {% endif %}
              </div>
            </div>
          </div>
      {% endif %}
    {% endfor %}
  {% enddefinedvalue %}



</div>

";
    }

    public class TestDataTemplateItem
    {
        public string TestName { get; set; }
        public string TemplateText { get; set; }
    }
}
