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
    public partial class Rollup_0626 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block to Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "56F1DC05-3D7D-49B6-9A30-5CF271C687F4", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel Dynamic", "Main", @"", @"", 0, "0C828414-8AEF-4B43-AAEF-B200544A2197" );
            // Add Block to Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlock( true, "2D0D0FB0-68C4-47E1-8BC6-98F931497F5E", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Blog Details", "Main", @"", @"", 0, "70DCBB50-0978-4B24-9382-CDD9BEED5ADB" );
            // Add Block to Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "7669A501-4075-431A-9828-565C47FD21C8", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel View Detail", "Main", @"", @"", 0, "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7" );
            // Add Block to Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlock( true, "BB83C51D-65C7-4F6C-BA24-A496167C9B11", "", "63659EBE-C5AF-4157-804A-55C7D565110E", "Content Channel View Detail", "Main", @"", @"", 0, "71D998C7-9F27-4B8A-937A-64C5EFC4783A" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Workflow Type Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Title Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Description Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Card Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Twitter Image Attribute Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Enabled Lava Commands Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Content Channel Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"8e213bb1-9e6f-40c1-b468-b3f8a60d5d24" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Content Channel Query Parameter Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Lava Template Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "47C56661-FB70-4703-9781-8651B8B49485", @"{% assign detailImageGuid = Item | Attribute:'DetailImage','RawValue' %}
{% if detailImageGuid != '' %}
  <img alt=""{{ Item.Title }}"" src=""/GetImage.ashx?Guid={{ detailImageGuid }}"" class=""title-image img-responsive"">
{% endif %}
<h1>{{ Item.Title }}</h1>{{ Item.Content }}" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Output Cache Duration Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Set Page Title Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Log Interactions Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Write Interaction Only If Individual Logged In Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Launch Workflow Only If Individual Logged In Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel Dynamic, Attribute:Launch Workflow Condition Page: Item Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "0C828414-8AEF-4B43-AAEF-B200544A2197", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Blog Details, Attribute:Launch Workflow Condition Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Blog Details, Attribute:Write Interaction Only If Individual Logged In Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Output Cache Duration Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Log Interactions Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Launch Workflow Only If Individual Logged In Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Blog Details, Attribute:Set Page Title Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Blog Details, Attribute:Lava Template Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "47C56661-FB70-4703-9781-8651B8B49485", @"<h1>{{ Item.Title }}</h1>
{{ Item.Content }}" );
            // Attrib Value for Block:Blog Details, Attribute:Enabled Lava Commands Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Content Channel Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"2b408da7-bdd1-4e71-b6ac-f22d786b605f" );
            // Attrib Value for Block:Blog Details, Attribute:Content Channel Query Parameter Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Image Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Description Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Card Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Blog Details, Attribute:Twitter Title Attribute Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Blog Details, Attribute:Workflow Type Page: Blog Details, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Workflow Type Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Title Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Description Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Card Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Image Attribute Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Query Parameter Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Enabled Lava Commands Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"0a63a427-e6b5-2284-45b3-789b293c02ea" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Lava Template Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "47C56661-FB70-4703-9781-8651B8B49485", @"	{% assign videoLink = Item | Attribute:'VideoLink','RawValue' %}
	{% assign videoEmbed = Item | Attribute:'VideoEmbed' %}
	{% assign audioLink = Item | Attribute:'AudioLink','RawValue' %}

	<artcile class=""message-detail"">

		{% if videoEmbed != '' %}
			{{ videoEmbed }}
		{% endif %}

		<h1>{{ Item.Title }}</h1>

		<p>
			<strong> {{ item | Attribute:'Speaker' }} - {{ Item.StartDateTime | Date:'M/d/yyyy' }}</strong>
		</p>

		<div class=""row"">
			<div class=""col-md-8"">
				{{ Item.Content }}
			</div>
			<div class=""col-md-4"">
				{% if videoLink != '' or audioLink != '' %}
					<div class=""panel panel-default"">
						<div class=""panel-heading"">Downloads &amp; Resources</div>
						<div class=""list-group"">

							{% if videoLink != '' %}
								<a href=""{{ videoLink }}"" class=""list-group-item""><i class=""fa fa-film""></i> Video Download</a>
							{% endif %}

							{% if audioLink != '' %}
								<a href=""{{ audioLink }}"" class=""list-group-item""><i class=""fa fa-volume-up""></i> Audio Download</a>
							{% endif %}

						</div>
					</div>
				{% endif %}
			</div>
		</row>
	</article>" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Condition Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Output Cache Duration Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Set Page Title Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Log Interactions Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Write Interaction Only If Individual Logged In Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Only If Individual Logged In Page: Message Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "71D998C7-9F27-4B8A-937A-64C5EFC4783A", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Only If Individual Logged In Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "EB298724-07D5-42AF-B4BF-82420AF6A657", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Write Interaction Only If Individual Logged In Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "63B254F7-E19C-48FD-A93F-AFEE19C1ED21", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Launch Workflow Condition Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "E5EFC23D-E030-496C-A9A4-D2BF4181CB49", @"1" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Log Interactions Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "3503170E-DD5E-4F51-9699-DCEA80C8C64C", @"False" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Output Cache Duration Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "7A9CBC44-FF60-464D-983A-61BD009F9C95", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Lava Template Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "47C56661-FB70-4703-9781-8651B8B49485", @"<style>
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
		<div class=""series-banner"" style=""background-image: url('/GetImage.ashx?Guid={{ seriesImageGuid }}');"" ></div>

		<h1 class=""series-title"">{{ Item.Title }}</h1>
		<p class=""series-dates"">
			<strong>{{ Item.StartDateTime | Date:'M/d/yyyy' }}
				{% if Item.StartDateTime != Item.ExpireDateTime %}
					- {{ Item.ExpireDateTime | Date:'M/d/yyyy' }}
				{% endif %}
			</strong>
		</p>


		<script>function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }</script>
		<script>function ics_click() { text = `{{ EventItemOccurrence.Schedule.iCalendarContent }}`.replace('END:VEVENT', 'SUMMARY: {{ Event.Name }}\r\nLOCATION: {{ EventItemOccurrence.Location }}\r\nEND:VEVENT'); var element = document.createElement('a'); element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text)); element.setAttribute('download', '{{ Event.Name }}.ics'); element.style.display = 'none'; document.body.appendChild(element); element.click(); document.body.removeChild(element); }</script>
		<ul class=""socialsharing"">
			<li>
				<a href=""http://www.facebook.com/share.php?u=<url>"" onclick=""return fbs_click()"" target=""_blank"" class=""socialicon socialicon-facebook"" title="""" data-original-title=""Share via Facebook"">
					<i class=""fa fa-fw fa-facebook""></i>
				</a>
			</li>
			<li>
				<a href=""http://twitter.com/home?status={{ 'Global' | Page:'Url' | EscapeDataString }}"" class=""socialicon socialicon-twitter"" title="""" data-original-title=""Share via Twitter"">
					<i class=""fa fa-fw fa-twitter""></i>
				</a>
			</li>
			<li>
				<a href=""mailto:?Subject={{ Event.Name | EscapeDataString }}&Body={{ 'Global' | Page:'Url' }}""  class=""socialicon socialicon-email"" title="""" data-original-title=""Share via Email"">
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
{% endif %}" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Set Page Title Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "406D4BB0-9BE3-4047-99C9-EAB5592B0942", @"True" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "E8921151-6392-4FFD-A1F4-67A6AAD69776", @"e2c598f1-d299-1baa-4873-8b679e3c1998" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Content Channel Query Parameter Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "39CC148D-B905-4560-96DD-C5151DC344DE", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Image Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "4CEFDE01-A056-4DBE-BEC2-979DCE0F4D39", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Enabled Lava Commands Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "8E741F29-A5D1-433B-A520-25C65B349216", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Description Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "32DE419C-062E-45FE-9BBE-CAE104A11491", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Card Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "D0C4618E-1F92-4107-A22F-8D638FD73E19", @"none" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Workflow Type Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "61361765-4762-4017-A58D-6CFCDD3CADC1", @"" );
            // Attrib Value for Block:Content Channel View Detail, Attribute:Twitter Title Attribute Page: Series Detail, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7", "CE43C275-44CA-4DA6-92CB-FAAFB1F886CF", @"" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Content Channel View Detail, from Page: Series Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "847E12E0-A7FC-4BD5-BD7E-1E9D435510E7" );
            // Remove Block: Content Channel View Detail, from Page: Message Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "71D998C7-9F27-4B8A-937A-64C5EFC4783A" );
            // Remove Block: Blog Details, from Page: Blog Details, Site: External Website
            RockMigrationHelper.DeleteBlock( "70DCBB50-0978-4B24-9382-CDD9BEED5ADB" );
            // Remove Block: Content Channel Dynamic, from Page: Item Detail, Site: External Website
            RockMigrationHelper.DeleteBlock( "0C828414-8AEF-4B43-AAEF-B200544A2197" );
        }
    }
}
