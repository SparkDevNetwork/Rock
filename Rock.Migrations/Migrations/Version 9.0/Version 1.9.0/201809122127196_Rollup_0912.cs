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
    public partial class Rollup_0912 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CorrectContentChannelSeries();

            // Attrib for BlockType: Calendar Lava:Show Year View
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Year View", "ShowYearView", "", @"Determines whether the year view option is shown", 13, @"False", "DB36FF5C-AD9B-437D-8A82-1627A04FC6B0" );
            // Attrib for BlockType: Calendar Lava:Show All View
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All View", "ShowAllView", "", @"Determines whether the all view option is shown (Limited to 2 years)", 14, @"False", "B416602C-5825-44A9-8436-DE81C47D8D59" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Calendar Lava:Show All View
            RockMigrationHelper.DeleteAttribute( "B416602C-5825-44A9-8436-DE81C47D8D59" );
            // Attrib for BlockType: Calendar Lava:Show Year View
            RockMigrationHelper.DeleteAttribute( "DB36FF5C-AD9B-437D-8A82-1627A04FC6B0" );
        }

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
    }
}
