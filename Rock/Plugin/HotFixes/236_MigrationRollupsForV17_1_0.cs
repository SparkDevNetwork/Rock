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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 236, "1.17.0" )]
    public class MigrationRollupsForV17_1_0 : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            UpdateExternalSiteSeriesAndMessagePagesUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            UpdateExternalSiteSeriesAndMessagePagesDown();
        }

        #region DH: Update external site series and message pages and routes.

        /// <summary>
        /// Updates the external site message watch pages and routes.
        /// </summary>
        private void UpdateExternalSiteSeriesAndMessagePagesUp()
        {
            // Update the Series Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '7669a501-4075-431a-9828-565c47fd21c8'" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "39cc148d-b905-4560-96dd-c5151dc344de", "Series" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "47c56661-fb70-4703-9781-8651b8b49485", @"<style>
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
	{% assign seriesImageGuid = Item | Attribute:'SeriesImage','RawValue' %}
	<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"">
	</div>
	<h1 class=""series-title"">
		{{ Item.Title }}
	</h1>
	<p class=""series-dates"">
		<strong>
			{{ Item.StartDateTime | Date:'M/d/yyyy' }}
			{% if Item.StartDateTime != Item.ExpireDateTime %}
			- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
			{% endif %}
		</strong>
	</p>
	<script>
		function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }
	</script>
	<script>
		function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }
	</script>
	<ul class=""socialsharing"">
		<li>
			<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
				<i class=""fa fa-fw fa-facebook"">
				</i>
			</a>
		</li>
		<li>
			<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
				<i class=""fa fa-fw fa-twitter"">
				</i>
			</a>
		</li>
		<li>
			<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}"" class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
				<i class=""fa fa-fw fa-envelope-o"">
				</i>
			</a>
		</li>
	</ul>
	<div class=""margin-t-lg"">
		{{ Item.Content }}
	</div>
	<h4 class=""messages-title margin-t-lg"">
		In This Series
	</h4>
	<ol class=""messages"">
		{% for message in Item.ChildItems %}
		<li>
			<a href=""/watch/{{ Item.Id }}/{{ message.ChildContentChannelItem.Id }}"">
				{{ message.ChildContentChannelItem.Title }}
			</a>
		</li>
		{% endfor %}
	</ol>
</article>
{% else %}
<h1>
	Could not find series.
</h1>
{% endif %}
" );

            // Update the Message Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );
            RockMigrationHelper.AddBlockAttributeValue( "71d998c7-9f27-4b8a-937a-64c5efc4783a", "39cc148d-b905-4560-96dd-c5151dc344de", "Message" );

            // Update the Blog Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '2d0d0fb0-68c4-47e1-8bc6-98f931497f5e'" );

            // Add the new routes, but only if there are not any routes
            // that might conflict.
            Sql( @"
IF NOT EXISTS (SELECT 1 FROM [PageRoute] WHERE [Route] LIKE 'watch/%')
BEGIN
    INSERT INTO [PageRoute] ([IsSystem], [PageId], [Route], [IsGlobal], [Guid]) 
        VALUES
        (1, 460, 'watch/{Series}', 0, 'bef2bf79-7cc0-49eb-93ab-2dcb7b1ce859'),
        (1, 461, 'watch/{Series}/{Message}', 0, '8f2735a3-c434-4e3a-b7b6-a943a4254c5e')
END" );
        }

        /// <summary>
        /// Revert the changes made to the external site series and message pages.
        /// </summary>
        private void UpdateExternalSiteSeriesAndMessagePagesDown()
        {
            // Revert the page routes.
            Sql( "DELETE FROM [PageRoute] WHERE [Guid] IN ('bef2bf79-7cc0-49eb-93ab-2dcb7b1ce859', '8f2735a3-c434-4e3a-b7b6-a943a4254c5e')" );

            // Revert the Blog Detail page.
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = '2d0d0fb0-68c4-47e1-8bc6-98f931497f5e'" );

            // Revert the Message Detail page.
            RockMigrationHelper.AddBlockAttributeValue( "71d998c7-9f27-4b8a-937a-64c5efc4783a", "39cc148d-b905-4560-96dd-c5151dc344de", "Message" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );

            // Revert the Series Detail page.
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "47c56661-fb70-4703-9781-8651b8b49485", @"<style>
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
	{% assign seriesImageGuid = Item | Attribute:'SeriesImage','RawValue' %}
	<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"">
	</div>
	<h1 class=""series-title"">
		{{ Item.Title }}
	</h1>
	<p class=""series-dates"">
		<strong>
			{{ Item.StartDateTime | Date:'M/d/yyyy' }}
			{% if Item.StartDateTime != Item.ExpireDateTime %}
			- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
			{% endif %}
		</strong>
	</p>
	<script>
		function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }
	</script>
	<script>
		function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }
	</script>
	<ul class=""socialsharing"">
		<li>
			<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
				<i class=""fa fa-fw fa-facebook"">
				</i>
			</a>
		</li>
		<li>
			<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
				<i class=""fa fa-fw fa-twitter"">
				</i>
			</a>
		</li>
		<li>
			<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}"" class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
				<i class=""fa fa-fw fa-envelope-o"">
				</i>
			</a>
		</li>
	</ul>
	<div class=""margin-t-lg"">
		{{ Item.Content }}
	</div>
	<h4 class=""messages-title margin-t-lg"">
		In This Series
	</h4>
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
<h1>
	Could not find series.
</h1>
{% endif %}
" );
            RockMigrationHelper.AddBlockAttributeValue( "847e12e0-a7fc-4bd5-bd7e-1e9d435510e7", "39cc148d-b905-4560-96dd-c5151dc344de", "" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 1 WHERE [Guid] = 'bb83c51d-65c7-4f6c-ba24-a496167c9b11'" );
        }

        #endregion

    }
}
