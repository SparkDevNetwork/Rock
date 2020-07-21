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
    [MigrationNumber( 129, "1.11.0" )]
    public class MigrationRollupsFor11_1_0 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdatePodcastTwitterUrl();
            //UpdateRockMobileTemplate();
            //UpdateChannelTypeNamesForMessages();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// GJ: Fix Podcast Series Incorrect Twitter URL
        /// </summary>
        private void UpdatePodcastTwitterUrl()
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
				<a href=""http://twitter.com/share?url={{ ''Global'' | Page:''Url'' | EscapeDataString }}"" target=""_blank"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
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
            RockMigrationHelper.UpdateBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "47C56661-FB70-4703-9781-8651B8B49485", newValue, oldValue );
        }

        /// <summary>
        /// JE: Update Rock Mobile Template
        /// </summary>
        private void UpdateRockMobileTemplate()
        {
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "6593D4EB-2B7A-4C24-8D30-A02991D26BC0",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CALENDAR_EVENT_ITEM_OCCURRENCE_VIEW,
                "Default",
                @"<StackLayout Spacing=""0"">
    
    {% if Event.Photo.Guid %}
        <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}/GetImage.ashx?Guid={{ Event.Photo.Guid }}"" 
            Aspect=""AspectFill"" 
            Ratio=""4:2""
            Margin=""0,0,0,16"">
            <Rock:RoundedTransformation CornerRadius=""12""  />
        </Rock:Image>
    {% endif %}
    
    <Label StyleClass=""h1"" 
        Text=""{{ Event.Name | Escape }}""  />
    
    <Rock:FieldContainer FieldLayout=""Individual"">
        {% assign scheduledDates = EventItemOccurrence.Schedule.iCalendarContent | DatesFromICal:'all' %}
        {% assign scheduleListing = '' %}
        {% for scheduledDate in scheduledDates %}
            {% if forloop.index <= 5 %}
                {% assign scheduleDateTime = scheduledDate | Date:'dddd, MMMM d, yyyy @ h:mm tt' %}
                {% assign scheduleListing = scheduleListing | Append:scheduleDateTime | Append:'&#xa;'  %}
            {% endif %}

        {% endfor %}
        
        <Rock:Literal Label=""Date / Time"" Text=""{{ scheduleListing | ReplaceLast:'&#xa;', '' }}"" />
    
        {% if EventItemOccurrence.Location != '' %}
            <Rock:Literal Label=""Location"" Text=""{{ EventItemOccurrence.Location }}"" />
        {% endif %}
    </Rock:FieldContainer>
    
    <Rock:Html StyleClass=""text"">
        {{ Event.Description | Escape }}
    </Rock:Html>
    
    {% if EventItemOccurrence.Note != '' %}
        <Label Text=""Note"" StyleClass=""text, font-weight-bold"" />
        <Rock:Html StyleClass=""text"">{{ EventItemOccurrence.Note | Escape }}</Rock:Html>
    {% endif %}

    
    {% if EventItemOccurrence.ContactPersonAliasId != null or EventItemOccurrence.ContactEmail != '' or EventItemOccurrence.ContactPhone != '' %}
        {% if EventItemOccurrence.ContactPersonAliasId != null %}
            <Label Text=""Contact"" StyleClass=""title"" />
            <Label Text=""{{ EventItemOccurrence.ContactPersonAlias.Person.FullName }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactEmail != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactEmail }}"" />
        {% endif %}
        {% if EventItemOccurrence.ContactPhone != '' %}
            <Label Text=""{{ EventItemOccurrence.ContactPhone }}"" />
        {% endif %}
    {% endif %}
    
    
    {% assign showRegistration = false %}
    {% assign eventItemOccurrenceLinkages = EventItemOccurrence.Linkages %}
    
    {% assign eventItemOccurrenceLinkagesCount = eventItemOccurrenceLinkages | Size %}
    {% if eventItemOccurrenceLinkagesCount > 0 %}
        {% for eventItemOccurrenceLinkage in eventItemOccurrenceLinkages %}
            {% assign daysTillStartDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime,'m' %}
            {% assign daysTillEndDate = 'Now' | DateDiff:eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime,'m' %}
            {% assign showRegistration = true %}
            {% assign registrationMessage = '' %}
    
            {% if daysTillStartDate and daysTillStartDate > 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} opens on {{ eventItemOccurrenceLinkage.RegistrationInstance.StartDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if daysTillEndDate and daysTillEndDate < 0 %}
                {% assign showRegistration = false %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% capture registrationMessage %}Registration closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% else %}
                  {% capture registrationMessage %}Registration for {{ eventItemOccurrenceLinkage.PublicName }} closed on {{ eventItemOccurrenceLinkage.RegistrationInstance.EndDateTime | Date:'dddd, MMMM d, yyyy' }}{% endcapture %}
                {% endif %}
            {% endif %}
    
            {% if showRegistration == true %}
                {% assign statusLabel = RegistrationStatusLabels[eventItemOccurrenceLinkage.RegistrationInstanceId] %}
                {% if eventItemOccurrenceLinkagesCount == 1 %}
                  {% assign registrationButtonText = statusLabel %}
                {% else %}
                  {% assign registrationButtonText = statusLabel | Plus:' for ' | Plus:eventItemOccurrenceLinkage.PublicName %}
                {% endif %}
    
                {% if statusLabel == 'Full' %}
                    {% if eventItemOccurrenceLinkagesCount == 1 %}
                      {% assign registrationButtonText = 'Registration Full' %}
                    {% else %}
                      {% assign registrationButtonText = eventItemOccurrenceLinkage.PublicName | Plus: ' (Registration Full) ' %}
                    {% endif %}
                    <Label StyleClass=""text"">{{ registrationButtonText }}</Label>
                {% else %}
                    {% if eventItemOccurrenceLinkage.UrlSlug != '' %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""Slug"" Value=""{{eventItemOccurrenceLinkage.UrlSlug}}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% else %}
                        <Button Text=""{{ registrationButtonText | Escape }}"" Command=""{Binding OpenExternalBrowser}"" StyleClass=""btn, btn-primary, mt-24"">
                            <Button.CommandParameter>
                                <Rock:OpenExternalBrowserParameters Url=""{{ RegistrationUrl }}"">
                                    <Rock:Parameter Name=""RegistrationInstanceId"" Value=""{{ eventItemOccurrenceLinkage.RegistrationInstanceId }}"" />
                                    <Rock:Parameter Name=""EventOccurrenceId"" Value=""{{ eventItemOccurrenceLinkage.EventItemOccurrenceId }}"" />
                                </Rock:OpenExternalBrowserParameters>
                            </Button.CommandParameter>
                        </Button>
                    {% endif %}
                {% endif %}
            {% else %}
              <Label StyleClass=""font-weight-bold"" Text=""Registration Information"" />
              <Label StyleClass=""text"" Text=""{{ registrationMessage | Escape }}"" />
            {% endif %}
        {% endfor %}
    {% endif %}
</StackLayout>");
        }

        /// <summary>
        /// JE: Update Content Channel Type Names for Messages
        /// </summary>
        private void UpdateChannelTypeNamesForMessages()
        {
            Sql( @"
                UPDATE [ContentChannelType]
                SET [Name] = 'Message Series' 
                WHERE [Guid] = 'dc1a1ef5-fa05-f4b2-4753-0ce971b65f7c'

                UPDATE [ContentChannelType]
                SET [Name] = 'Messages' 
                WHERE [Guid] = '484f9962-9dc6-2987-4ada-aabd122387fc'

                UPDATE [ContentChannelType]
                SET [Name] = 'Message Notes' 
                WHERE [Guid] = '48951e97-0e45-4494-b87c-4eb9fca067eb'" );
        }

    }
}
