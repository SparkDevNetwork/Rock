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
    public partial class AddGroupViewLavaTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "GroupViewLavaTemplate", c => c.String());

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
	
	
	
<p class='description'>{{ Group.Description }}</p>

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
			<li><a href = '{{ RegistrationInstancePage }}? RegistrationInstanceId = {{ linkage.RegistrationInstanceId }}'>{% if linkage.EventItemOccurrence != null %} {{ linkage.EventItemOccurrence.EventItem.Name }} ({% if linkage.EventItemOccurrence.Campus != null %} {{ linkage.EventItemOccurrence.Campus.Name }}  {% else %}  All Campuses {% endif %}) {% endif %} - {{ linkage.RegistrationInstance.Name }}</a></li>
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
			<li><a href = '{{ EventItemOccurrencePage }}? EventItemOccurrenceId = {{ linkage.EventItemOccurrence.Id }}'>{% if linkage.EventItemOccurrence != null %} {{ linkage.EventItemOccurrence.EventItem.Name }} -{% if linkage.EventItemOccurrence.Campus != null %} {{ linkage.EventItemOccurrence.Campus.Name }}  {% else %}  All Campuses {% endif %} {% endif %}</a></li>
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
					<li><a href = '{{ ContentItemPage }}? ContentItemId = {{ contentChannelItem.ContentChannelItemId }}'>{{ contentChannelItem.ContentChannelItem.Title }} <small>({{ contentChannelItem.ContentChannelItem.ContentChannelType.Name }})</small></a></li>
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
            Sql( string.Format( @"UPDATE [GroupType] SET [GroupViewLavaTemplate] = '{0}'", lavaTemplate.Replace("'","''" ) ));

            Rock.Web.SystemSettings.SetValue( "core_templates_GroupViewTemplate", lavaTemplate );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.GroupType", "GroupViewLavaTemplate");
        }
    }
}
