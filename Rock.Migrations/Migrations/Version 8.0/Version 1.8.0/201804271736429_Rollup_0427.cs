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
    public partial class Rollup_0427 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateInteractionsForiegnKey();
            RockMigrationHelper.UpdateSystemSettingIfNullOrBlank( "core_GenderAutoFillConfidence", "99.9" );
            NewBlockSettingAttributes();
            RemoveExtraSpacesFromGroupTypeLavaTemplate();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        public void CreateInteractionsForiegnKey()
        {
            // (schedule for 9pm to avoid conflict with AppPoolRecycle)
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV80DataMigrations' AND [Guid] = 'AF760EF9-66BD-4A4D-AF95-749AA789ACAA' )
                BEGIN
                INSERT INTO [ServiceJob] (
                [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid] )
                VALUES (
                0
                    , 1
                    , 'Data Migrations for v8.0'
                    , 'This job will take care of any data migrations that need to occur after updating to v8.0. After all the operations are done, this job will delete itself.'
                    , 'Rock.Jobs.PostV80DataMigrations'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , 'AF760EF9-66BD-4A4D-AF95-749AA789ACAA' );
            END" );
        }

        public void NewBlockSettingAttributes()
        {
            // Attrib for BlockType: Attendance Analytics:Show Bulk Update Option
            RockMigrationHelper.UpdateBlockTypeAttribute( "3CD3411C-C076-4344-A9D5-8F3B4F01E31D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Bulk Update Option", "ShowBulkUpdateOption", "", @"Should the Bulk Update option be allowed from the attendance grid?", 11, @"True", "2435DFE0-0188-4BBD-8822-63965B89418A" );
            // Attrib for BlockType: Person Select (Family Check-in):Next Button Text
            RockMigrationHelper.UpdateBlockTypeAttribute( "92DCF018-F551-4890-8BA1-511D97BF6B8A", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Next Button Text", "NextButtonText", "", @"", 12, @"Next", "0ECA773C-F4B4-42BC-BC6B-E992BE1E0D03" );
            // Attrib for BlockType: Person Profile:Show Related People
            RockMigrationHelper.UpdateBlockTypeAttribute( "48BBB7A7-1E1D-461E-9B64-E9CAD815E9E1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Related People", "ShowRelatedPeople", "", @"Should anyone who is allowed to check-in the current person also be displayed with the family members?", 1, @"False", "98D6ABD9-60ED-41C0-AFC8-67F31D41CC92" );
            // Attrib for BlockType: Personal Device Interactions:Currently Present Interval
            RockMigrationHelper.UpdateBlockTypeAttribute( "D6224911-2590-427F-9DCE-6D14E79806BA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Currently Present Interval", "CurrentlyPresentInterval", "", @"The number of minutes to use to determine is someone is still present. For example if set to 5 the system will consider the device present if their interaction records end date/time is within the last 5 minutes.", 0, @"5", "648B7897-0F45-474A-B43C-0C0298EF0B0D" );
            // Attrib for BlockType: Transaction Matching:Transaction Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Detail Page", "TransactionDetailPage", "", @"Select the page to return to, if this block was being used to edit a single transaction.", 4, @"", "E2BCD400-752C-47DE-B77A-75EC5F73FE24" );
            // Attrib for BlockType: Prayer Request List:Show Public Only
            RockMigrationHelper.UpdateBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Public Only", "ShowPublicOnly", "", @"If enabled, it will limit the list only to the prayer requests that are public.", 5, @"False", "C131C26B-1309-459D-8A94-0A3A6B01B3B6" );
            // Attrib for BlockType: Interaction Channel List:Interaction Channels
            RockMigrationHelper.UpdateBlockTypeAttribute( "FBC2066B-8E7C-43CB-AFD2-FA9408F6699D", "D5781EB0-3A2A-4FBB-AF8E-E14664147003", "Interaction Channels", "InteractionChannels", "", @"Select interaction channel to limit the display. No selection will show all.", 3, @"", "B8D9C666-CCC6-4BD4-A96D-4C2F3991C4FD" );
            RockMigrationHelper.UpdateFieldType( "Interaction Channels", "", "Rock", "Rock.Field.Types.InteractionChannelsFieldType", "D5781EB0-3A2A-4FBB-AF8E-E14664147003" );
        }

        public void RemoveExtraSpacesFromGroupTypeLavaTemplate()
        {
            string lavaTemplate = @"{% if Group.GroupType.GroupCapacityRule != 'None' and  Group.GroupCapacity != '' %}
		{% assign warningLevel = ''warning'' %}

		{% if Group.GroupType.GroupCapacityRule == 'Hard' %}
			{% assign warningLevel = 'danger' %}
		{% endif %}

		{% assign activeMemberCount = countActive | Plus:1 %} {% comment %}the counter is zero based{% endcomment %}
		{% assign overageAmount = activeMemberCount | Minus:Group.GroupCapacity %}

		{% if overageAmount > 0 %}
			<div class=""alert alert-{{ warningLevel }} margin-t-sm"">This group is over capacity by {{ overageAmount }} {{ 'individual' | PluralizeForQuantity:overageAmount }}.</div>
		{% endif %}
	{% endif %}
	
	
	
{% if Group.Description != '' -%}
    <p class='description'>{{ Group.Description }}</p>
{% endif -%}

<div class=""row"">
   <div class=""col-md-6"">
        <dl>
            {% if Group.ParentGroup != null %}
            <dt> Parent Group </ dt>
               <dd>{{ Group.ParentGroup.Name }}</dd>
            {% endif %}
            {% if Group.RequiredSignatureDocumentTemplate != null %}
            <dt> Required Signed Document </dt>
               <dd>{{ Group.RequiredSignatureDocumentTemplate.Name }}</ dd >
            {% endif %}
            {% if Group.Schedule != null %}

            <dt> Schedule </dt>
            <dd>{{ Group.Schedule.ToString() }}</ dd >
            {% endif %}
            {% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}
        </dl>
        <dl>
        {% for attribute in Group.AttributeValues %}
        <dt>{{ attribute.AttributeName }}:</dt>

<dd>{{ attribute.ValueFormatted }} </dd>
        {% endfor %}
        </dl>
    </div>

    <div class=""col-md-6 location-maps"">
	{% assign googleAPIKey = 'Global' | Attribute: 'GoogleAPIKey' %}
	{% assign staticMapStyle = MapStyle | Attribute: 'StaticMapStyle' %}

	{% if Group.GroupLocations != null %}
	{% assign groupLocations = Group.GroupLocations %}
	{% assign locationCount = groupLocations | Size %}
	    {% if locationCount > 0 and googleAPIKey != null and googleAPIKey !='' and staticMapStyle != null and staticMapStyle != '' %}
		{% for groupLocation in groupLocations %}
	    	{% if groupLocation.Location.GeoPoint != null and groupLocation.Location.GeoPoint != '' %}
	    	{% capture markerPoints %}{{ groupLocation.Location.Latitude }},{{ groupLocation.Location.Longitude }}{% endcapture %}
	    	{% assign mapLink = staticMapStyle | Replace:'{MarkerPoints}', markerPoints   %}
	    	{% assign mapLink = mapLink | Replace:'{PolygonPoints}','' %}
	    	{% assign mapLink = mapLink | Append:'&sensor=false&size=450x250&zoom=13&format=png&key=' %}
            {% assign mapLink = mapLink | Append: googleAPIKey %}
	    	<div class=""group-location-map"">
	    	    {% if groupLocation.GroupLocationTypeValue != null %}
	    	    <h4> {{ groupLocation.GroupLocationTypeValue.Value }} </h4>
	    	    {% endif %}
	    	    <a href = '{{ GroupMapUrl }}'>
	    	    <img class='img-thumbnail' src='{{ mapLink }}'/>
	    	    </a>
	    	    {% if groupLocation.Location.FormattedAddress != null and groupLocation.Location.FormattedAddress != '' and ShowLocationAddresses == true %}
	    	    {{ groupLocation.Location.FormattedAddress }}
	    	    {% endif %}
	    	 </div>
		    {% endif %}
		    {% if groupLocation.Location.GeoFence != null and groupLocation.Location.GeoFence != ''  %}

		    {% assign mapLink = staticMapStyle | Replace:'{MarkerPoints}','' %}
		    {% assign googlePolygon = 'enc:' | Append: groupLocation.Location.GooglePolygon %}
	    	{% assign mapLink = mapLink | Replace:'{PolygonPoints}', googlePolygon  %}
	    	{% assign mapLink = mapLink | Append:'&sensor=false&size=350x200&format=png&key=' %}
	    	{% assign mapLink = mapLink | Append: googleAPIKey %}
		    <div class='group-location-map'>
		        {% if groupLocation.GroupLocationTypeValue != null %}
		        <h4> {{ groupLocation.GroupLocationTypeValue.Value }} </h4>
		        {% endif %}
		    <a href = '{{ GroupMapUrl }}'><img class='img-thumbnail' src='{{ mapLink }}'/></a>
		    </div>	
		    {% endif %}
		{% endfor %}
		{% endif %}
	{% endif %}
	{% if Group.Linkages != null %}
	{% assign linkages = Group.Linkages %}
	{% assign linkageCount = linkages | Size %}
	{% if linkageCount > 0 %}
	{% assign countRegistration = 0 %}
	{% assign countLoop = 0 %}
	{% assign countEventItemOccurances = 0 %}
	{% assign countContentItems = 0 %}
	{% for linkage in linkages %}
		{% if linkage.RegistrationInstanceId != null and linkage.RegistrationInstanceId != '' %}
			{% if countRegistration == 0 %}
			<strong> Registrations</strong>
			<ul class=""list-unstyled"">
			{% endif %}
			<li><a href = '{{ RegistrationInstancePage }}?RegistrationInstanceId={{ linkage.RegistrationInstanceId }}'>{% if linkage.EventItemOccurrence != null %} {{ linkage.EventItemOccurrence.EventItem.Name }} ({% if linkage.EventItemOccurrence.Campus != null %} {{ linkage.EventItemOccurrence.Campus.Name }}  {% else %}  All Campuses {% endif %}) {% endif %} - {{ linkage.RegistrationInstance.Name }}</a></li>
			{% assign countRegistration = countRegistration | Plus: 1 %}
		{% endif %}
		{% assign countLoop = countLoop | Plus: 1 %}
		{% if countRegistration > 0 and countLoop == linkageCount  %}
		</ul>
		{% endif %}
	{% endfor %}
	{% assign countLoop = 0 %}
	{% for linkage in linkages %}
		{% if linkage.EventItemOccurrence != null and linkage.EventItemOccurrence.EventItem != null %}
			{% if countEventItemOccurances == 0 %}
			<strong> Event Item Occurrences</strong>
			<ul class=""list-unstyled"">
			{% endif %}
			<li><a href = '{{ EventItemOccurrencePage }}?EventItemOccurrenceId={{ linkage.EventItemOccurrence.Id }}'>{% if linkage.EventItemOccurrence != null %} {{ linkage.EventItemOccurrence.EventItem.Name }} -{% if linkage.EventItemOccurrence.Campus != null %} {{ linkage.EventItemOccurrence.Campus.Name }}  {% else %}  All Campuses {% endif %} {% endif %}</a></li>
			{% assign countEventItemOccurances = countEventItemOccurances | Plus: 1 %}
		{% endif %}
		{% assign countLoop = countLoop | Plus: 1 %}
		{% if countEventItemOccurances > 0  and countLoop == linkageCount %}
			</ul>
		{% endif %}
	{% endfor %}
	{% assign countLoop = 0 %}
	{% for linkage in linkages %}
		{% if linkage.EventItemOccurrence != null and linkage.EventItemOccurrence.EventItem != null %}
			{% assign contentChannelItemsCount = linkage.EventItemOccurrence.ContentChannelItems | Size %}
			{% if contentChannelItemsCount > 0 %}
			{% assign contentChannelItems = linkage.EventItemOccurrence.ContentChannelItems %}
				{% for contentChannelItem in contentChannelItems %}
				{% if contentChannelItem.ContentChannelItem != null  %}
					{% if countContentItems == 0 %}
					<strong> Content Items</strong>
					<ul class=""list-unstyled"">
					{% endif %}
					<li><a href = '{{ ContentItemPage }}?ContentItemId={{ contentChannelItem.ContentChannelItemId }}'>{{ contentChannelItem.ContentChannelItem.Title }} <small>({{ contentChannelItem.ContentChannelItem.ContentChannelType.Name }})</small></a></li>
					{% assign countContentItems = countContentItems | Plus: 1 %}
				{% endif %}
				{% endfor %}
			{% endif %}
    	{% endif %}
    	{% assign countLoop = countLoop | Plus: 1 %}
    	{% if countContentItems > 0 and countLoop == linkageCount %}
			</ul>
		{% endif %}
	{% endfor %}
	{% endif %}
{% endif %}
	</div>
</div>";
            Sql( string.Format( @"UPDATE [GroupType] SET [GroupViewLavaTemplate] = '{0}'", lavaTemplate.Replace( "'", "''" ) ) );

            RockMigrationHelper.UpdateSystemSetting( "core_templates_GroupViewTemplate", lavaTemplate );
        }
    }
}
