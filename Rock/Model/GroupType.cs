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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Security;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{

    /// <summary>
    /// Represents a type or category of <see cref="Rock.Model.Group">Groups</see> in Rock.  A GroupType is also used to configure how Groups that belong to a GroupType will operate
    /// and how they will interact with other components of Rock.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupType" )]
    [DataContract]
    public partial class GroupType : Model<GroupType>, IOrdered, ICacheable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupType"/> class.
        /// </summary>
        public GroupType()
        {
            ShowInGroupList = true;
            ShowInNavigation = true;
            GroupTerm = "Group";
            GroupMemberTerm = "Member";
            AdministratorTerm = "Administrator";
        }

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType is part of the Rock core system/framework.  This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this GroupType is part of the Rock core system/framework.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the GroupType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the GroupType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description of the GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the GroupType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the term that a <see cref="Rock.Model.Group"/> belonging to this <see cref="Rock.Model.GroupType"/> is called.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the term that a <see cref="Rock.Model.Group"/> belonging to this <see cref="Rock.Model.GroupType"/> is called.
        /// </value>
        /// <remarks>
        /// Examples of GroupTerms include: group, community, class, family, etc.
        /// </remarks>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string GroupTerm { get; set; }

        /// <summary>
        /// Gets or sets the term that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> that belongs to this GroupType is called.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the term that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this 
        /// GroupType is called.
        /// </value>
        /// <example>
        /// Examples of GroupMemberTerms include: member, attendee, team member, student, etc.
        /// </example>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string GroupMemberTerm { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.GroupTypeRole"/> that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this GroupType is given by default.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> that a <see cref="Rock.Model.GroupMember"/> of a <see cref="Rock.Model.Group"/> belonging to this GroupType is given by default.
        /// </value>
        [DataMember]
        public int? DefaultGroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if <see cref="Rock.Model.Group">Groups</see> of this type are allowed to have multiple locations.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if a <see cref="Rock.Model.Group"/> of this GroupType are allowed to have multiple locations; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultipleLocations { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a <see cref="Rock.Model.Group"/> of this GroupType will be shown in the group list.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if a <see cref="Rock.Model.Group"/> of this GroupType will be shown in the GroupList; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowInGroupList { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this GroupType and its <see cref="Rock.Model.Group">Groups</see> are shown in Navigation.
        /// If false, this GroupType will be hidden navigation controls, such as TreeViews and Menus
        /// </summary>
        /// <remarks>
        ///  Navigation controls include objects lie menus and treeview controls.
        /// </remarks>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this GroupType and Groups should be displayed in Navigation controls.
        /// </value>
        [DataMember]
        public bool ShowInNavigation { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS class name of a font based icon.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a <see cref="Rock.Model.Group" /> of this GroupType supports taking attendance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean" /> representing if a <see cref="Rock.Model.Group" /> of this GroupType supports taking attendance.
        /// </value>
        [DataMember]
        public bool TakesAttendance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [attendance counts as weekend service].
        /// </summary>
        /// <value>
        /// <c>true</c> if [attendance counts as weekend service]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AttendanceCountsAsWeekendService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if an attendance reminder should be sent to group leaders.
        /// </summary>
        /// <value>
        /// <c>true</c> if [send attendance reminder]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendAttendanceReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Person's connection status as a column in the Group Member Grid
        /// </summary>
        /// <value>
        /// <c>true</c> if [show connection status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Person's martial status as a column in the Group Member Grid
        /// </summary>
        /// <value>
        /// <c>true</c> if [show marital status]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowMaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is managed a <see cref="Rock.Model.Group"/> of this GroupType
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.AttendanceRule"/> that indicates how attendance is managed for a <see cref="Rock.Model.Group"/> of this GroupType.
        /// </value>
        /// <example>
        /// The available options are:
        /// AttendanceRule.None -> A <see cref="Rock.Model.Person"/> does not have to previously belong to the <see cref="Rock.Model.Group"/> that they are checking into, and they will not be automatically added.
        /// AttendanceRule.AddOnCheckin -> If a <see cref="Rock.Model.Person"/> does not belong to the <see cref="Rock.Model.Group"/> that they are checking into, they will be automatically added with the default
        /// <see cref="Rock.Model.GroupTypeRole"/> upon check in.
        /// </example>
        [DataMember]
        public AttendanceRule AttendanceRule { get; set; }

        /// <summary>
        /// Gets or sets the group capacity rule.
        /// </summary>
        /// <value>
        /// The group capacity rule.
        /// </value>
        [DataMember]
        public GroupCapacityRule GroupCapacityRule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PrintTo"/> indicating the type of  location of where attendee labels for <see cref="Rock.Model.Group">Groups</see> of this GroupType should print.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PrintTo"/> enum value indicating how and where attendee labels for <see cref="Rock.Model.Group">Groups</see> of this GroupType should print.
        /// </value>
        /// <remarks>
        /// The available options include:
        /// PrintTo.Default -> print to the default printer.
        /// PrintTo.Kiosk -> print to the printer associated with the kiosk.
        /// PrintTo.Location -> print to the location
        /// </remarks>
        [DataMember]
        public PrintTo AttendancePrintTo { get; set; }

        /// <summary>
        /// Gets or sets the order for this GroupType. This is used for display and priority purposes, the lower the number the higher the priority, or the higher the GroupType is displayed. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display/priority order for this GroupType.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Id of the GroupType to inherit settings and properties from. This is essentially copying the values, but they can be overridden.
        /// </summary>
        /// <value>A <see cref="System.Int32"/> representing the Id of a GroupType to inherit properties and values from.</value>
        [DataMember]
        public int? InheritedGroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the allowed schedule types.
        /// </summary>
        /// <value>
        /// The allowed schedule types.
        /// </value>
        [DataMember]
        public ScheduleType AllowedScheduleTypes { get; set; }

        /// <summary>
        /// Gets or sets selection mode that the Location Picker should use when adding locations to groups of this type
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Web.UI.Controls.LocationPickerMode"/> to use when adding location(s) to <see cref="Rock.Model.Group">Groups</see>
        /// of this GroupType. This can be one or more of the following values
        /// </value>
        /// <remarks>
        /// Available options include one or more of the following:
        ///     GroupLocationPickerMode.Location -> A named location.
        ///     GroupLocationPickerMode.Address -> Selection by address (i.e. 7007 W Happy Valley Rd Peoria, AZ 85383)
        ///     GroupLocationPickerMode.Point -> A geographic point (i.e. 38.229336, -85.542045)
        ///     GroupLocationPickerMode.Polygon -> A geographic polygon.
        ///     GroupLocationPickerMode.GroupMember -> A group members's address
        /// </remarks>
        [DataMember]
        public GroupLocationPickerMode LocationSelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the enable location schedules.
        /// </summary>
        /// <value>
        /// The enable location schedules.
        /// </value>
        [DataMember]
        public bool? EnableLocationSchedules { get; set; }

        /// <summary>
        /// Gets or sets Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.GROUPTYPE_PURPOSE )]
        public int? GroupTypePurposeValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ignore person inactivated.
        /// By default group members are inactivated in their group whenever the person
        /// is inactivated. If this value is set to true, members in groups of this type
        /// will not be marked inactive when the person is inactivated
        /// </summary>
        /// <value>
        /// <c>true</c> if [ignore person inactivated]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IgnorePersonInactivated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is index enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is index enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsIndexEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [groups require campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [groups require campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool GroupsRequireCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [group attendance requires location].
        /// </summary>
        /// <value>
        /// <c>true</c> if [group attendance requires location]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool GroupAttendanceRequiresLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [group attendance requires schedule].
        /// </summary>
        /// <value>
        /// <c>true</c> if [group attendance requires schedule]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool GroupAttendanceRequiresSchedule { get; set; }

        /// <summary>
        /// Gets or sets a lava template that can be used for generating  view details for Group.
        /// </summary>
        /// <value>
        /// The Group View Lava Template.
        /// </value>
        [DataMember]
        public string GroupViewLavaTemplate
        {
            get
            {
                if ( _groupViewLavaTemplate.IsNullOrWhiteSpace() )
                {
                    return _defaultLavaTemplate;
                }
                else
                {
                    return _groupViewLavaTemplate;
                }
            }
            set
            {
                _groupViewLavaTemplate = value;
            }
        }
        private string _groupViewLavaTemplate;
        private string _defaultLavaTemplate = @"{% if Group.GroupType.GroupCapacityRule != 'None' and  Group.GroupCapacity != '' %}
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
            <dd>{{ Group.Schedule.FriendlyScheduleText }}</ dd >
            {% endif %}
            {% if Group.GroupCapacity != null and Group.GroupCapacity != '' %}

            <dt> Capacity </dt>

            <dd>{{ Group.GroupCapacity }}</dd>
            {% endif %}
        {% if Group.GroupType.ShowAdministrator and Group.GroupAdministratorPersonAlias != null and Group.GroupAdministratorPersonAlias != '' %}
            <dt> {{ Group.GroupType.AdministratorTerm }}</dt>
            <dd>{{ Group.GroupAdministratorPersonAlias.Person.FullName }}</dd>
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
	{% assign countEventItemOccurrences = 0 %}
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
			{% if countEventItemOccurrences == 0 %}
			<strong> Event Item Occurrences</strong>
			<ul class=""list-unstyled"">
			{% endif %}
			<li><a href = '{{ EventItemOccurrencePage }}?EventItemOccurrenceId={{ linkage.EventItemOccurrence.Id }}'>{% if linkage.EventItemOccurrence != null %} {{ linkage.EventItemOccurrence.EventItem.Name }} -{% if linkage.EventItemOccurrence.Campus != null %} {{ linkage.EventItemOccurrence.Campus.Name }}  {% else %}  All Campuses {% endif %} {% endif %}</a></li>
			{% assign countEventItemOccurrences = countEventItemOccurrences | Plus: 1 %}
		{% endif %}
		{% assign countLoop = countLoop | Plus: 1 %}
		{% if countEventItemOccurrences > 0  and countLoop == linkageCount %}
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

        /// <summary>
        /// Gets or sets a flag indicating if specific groups are allowed to have their own member attributes.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this specific group are allowed to have their own member attributes, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if group requirements section is enabled for group of this type.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if group requirements section is enabled for group of this type, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableSpecificGroupRequirements { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type are allowed to be sync'ed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type are allowed to be sync'ed, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowGroupSync { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if groups of this type should be allowed to have Group Member Workflows.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if groups of this type should be allowed to have group member workflows, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowSpecificGroupMemberWorkflows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether group history should be enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable group history]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableGroupHistory { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether group tag should be enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable group tag]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableGroupTag { get; set; } = false;

        /// <summary>
        /// The color used to visually distinguish groups on lists.
        /// </summary>
        /// <value>
        /// The group type color.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string GroupTypeColor { get; set; }

        /// <summary>
        /// Gets or sets the DefinedType that Groups of this type will use for the Group.StatusValue
        /// </summary>
        /// <value>
        /// The group status defined type identifier.
        /// </value>
        [DataMember]
        public int? GroupStatusDefinedTypeId { get; set; }

        #endregion Entity Properties

        #region Group Scheduling Related

        /// <summary>
        /// Gets or sets a value indicating whether scheduling is enabled for groups of this type
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is scheduling enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSchedulingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the system email to use when a person is scheduled or when the schedule has been updated
        /// </summary>
        /// <value>
        /// The scheduled system email identifier.
        /// </value>
        [DataMember]
        public int? ScheduleConfirmationSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the system email to use when sending a schedule reminder
        /// </summary>
        /// <value>
        /// The schedule reminder system email identifier.
        /// </value>
        [DataMember]
        public int? ScheduleReminderSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowType to execute when a person indicates they won't be able to attend at their scheduled time
        /// </summary>
        /// <value>
        /// The schedule cancellation workflow type identifier.
        /// </value>
        [DataMember]
        public int? ScheduleCancellationWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a confirmation email.
        /// </summary>
        /// <value>
        /// The schedule confirmation email offset days.
        /// </value>
        [DataMember]
        public int? ScheduleConfirmationEmailOffsetDays { get; set; } = 4;

        /// <summary>
        /// Gets or sets the number of days prior to the schedule to send a reminder email. See also <seealso cref="GroupMember.ScheduleReminderEmailOffsetDays"/>.
        /// </summary>
        /// <value>
        /// The schedule reminder email offset days.
        /// </value>
        [DataMember]
        public int? ScheduleReminderEmailOffsetDays { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether a person must specify a reason when declining/cancelling.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [requires reason if decline schedule]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresReasonIfDeclineSchedule { get; set; }

        /// <summary>
        /// Gets or sets the administrator term for the group of this GroupType.
        /// </summary>
        /// <value>
        /// The administrator term for the group of this GroupType.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string AdministratorTerm { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether administrator for the group of this GroupType will be shown.
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if administrator for the group of this GroupType will be shown; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool ShowAdministrator { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A collection containing a collection of the <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        [LavaInclude]
        public virtual ICollection<Group> Groups
        {
            get { return _groups ?? ( _groups = new Collection<Group>() ); }
            set { _groups = value; }
        }
        private ICollection<Group> _groups;

        /// <summary>
        /// Gets or sets the collection of GroupTypes that inherit from this GroupType.
        /// </summary>
        /// <value>
        /// A collection of the GroupTypes that inherit from this groupType.
        /// </value>
        [DataMember, LavaIgnore]
        public virtual ICollection<GroupType> ChildGroupTypes
        {
            get { return _childGroupTypes ?? ( _childGroupTypes = new Collection<GroupType>() ); }
            set { _childGroupTypes = value; }
        }
        private ICollection<GroupType> _childGroupTypes;

        /// <summary>
        /// Gets or sets a collection containing the GroupTypes that this GroupType inherits from.
        /// </summary>
        /// <value>
        /// A collection containing the GroupTypes that this GroupType inherits from.
        /// </value>
        public virtual ICollection<GroupType> ParentGroupTypes
        {
            get { return _parentGroupTypes ?? ( _parentGroupTypes = new Collection<GroupType>() ); }
            set { _parentGroupTypes = value; }
        }
        private ICollection<GroupType> _parentGroupTypes;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.GroupTypeRole">GroupRoles</see> that this GroupType utilizes.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.GroupTypeRole"/>GroupRoles that are associated with this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeRole> Roles
        {
            get { return _roles ?? ( _roles = new Collection<GroupTypeRole>() ); }
            set { _roles = value; }
        }
        private ICollection<GroupTypeRole> _roles;

        /// <summary>
        /// Gets or sets the group member workflow triggers.
        /// </summary>
        /// <value>
        /// The group member workflow triggers.
        /// </value>
        public virtual ICollection<GroupMemberWorkflowTrigger> GroupMemberWorkflowTriggers
        {
            get { return _triggers ?? ( _triggers = new Collection<GroupMemberWorkflowTrigger>() ); }
            set { _triggers = value; }
        }
        private ICollection<GroupMemberWorkflowTrigger> _triggers;
        
        /// <summary>
        /// Gets or sets the group schedule exclusions.
        /// </summary>
        /// <value>
        /// The group schedule exclusions.
        /// </value>
        public virtual ICollection<GroupScheduleExclusion> GroupScheduleExclusions
        {
            get { return _groupScheduleExclusions ?? ( _groupScheduleExclusions = new Collection<GroupScheduleExclusion>() ); }
            set { _groupScheduleExclusions = value; }
        }
        private ICollection<GroupScheduleExclusion> _groupScheduleExclusions;

        /// <summary>
        /// Gets or sets a collection of the <see cref="Rock.Model.GroupTypeLocationType">GroupTypeLocationTypes</see> that are associated with this GroupType.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.GroupTypeLocationType">GroupTypeLocationTypes</see> that are associated with this GroupType.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupTypeLocationType> LocationTypes
        {
            get { return _locationTypes ?? ( _locationTypes = new Collection<GroupTypeLocationType>() ); }
            set { _locationTypes = value; }
        }
        private ICollection<GroupTypeLocationType> _locationTypes;


        /// <summary>
        /// Gets or sets the default <see cref="Rock.Model.GroupTypeRole"/> for <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to a 
        /// <see cref="Rock.Model.Group"/> of this GroupType.
        /// </summary>
        /// <value>
        /// The default <see cref="Rock.Model.GroupTypeRole"/> for <see cref="Rock.Model.GroupMember">GroupMembers</see> who belong to a <see cref="Rock.Model.Group"/>
        /// of this GroupType.
        /// </value>
        [DataMember]
        public virtual GroupTypeRole DefaultGroupRole { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> that represents the purpose of the GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> that represents the the purpose of the GroupType.
        /// </value>
        [DataMember]
        public virtual DefinedValue GroupTypePurposeValue { get; set; }

        /// <summary>
        /// Gets or sets the system email to use when a person is scheduled or when the schedule has been updated
        /// </summary>
        /// <value>
        /// The scheduled system email.
        /// </value>
        [DataMember]
        public virtual SystemEmail ScheduleConfirmationSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the system email to use when sending a Schedule Reminder
        /// </summary>
        /// <value>
        /// The schedule reminder system email.
        /// </value>
        [DataMember]
        public virtual SystemEmail ScheduleReminderSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the WorkflowType to execute when a person indicates they won't be able to attend at their scheduled time
        /// </summary>
        /// <value>
        /// The type of the schedule cancellation workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType ScheduleCancellationWorkflowType { get; set; }

        /// <summary>
        /// Gets a count of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        [LavaInclude]
        public virtual int GroupCount
        {
            get
            {
                return GroupQuery.Count();
            }
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </summary>
        /// <value>
        /// A queryable collection of <see cref="Rock.Model.Group">Groups</see> that belong to this GroupType.
        /// </value>
        public virtual IQueryable<Group> GroupQuery
        {
            get
            {
                var groupService = new GroupService( new RockContext() );
                var qry = groupService.Queryable().Where( a => a.GroupTypeId.Equals( this.Id ) );
                return qry;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that this GroupType is inheriting settings and properties from. 
        /// This is similar to a parent or a template GroupType.
        /// </summary>
        /// <value>The <see cref="Rock.Model.GroupType"/> that this GroupType is inheriting settings and properties from.</value>
        [LavaInclude]
        public virtual GroupType InheritedGroupType { get; set; }

        /// <summary>
        /// Gets or sets the group requirements for groups of this Group Type (NOTE: Groups also can have additional GroupRequirements )
        /// </summary>
        /// <value>
        /// The group requirements.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupRequirement> GroupRequirements
        {
            get { return _groupsRequirements ?? ( _groupsRequirements = new Collection<GroupRequirement>() ); }
            set { _groupsRequirements = value; }
        }
        private ICollection<GroupRequirement> _groupsRequirements;

        /// <summary>
        /// Gets or sets the DefinedType that Groups of this type will use for the Group.StatusValue
        /// </summary>
        /// <value>
        /// The type of the group status defined.
        /// </value>
        public DefinedType GroupStatusDefinedType { get; set; }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions {
            get {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.MANAGE_MEMBERS, "The roles and/or users that have access to manage the group members." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                    _supportedActions.Add( Authorization.SCHEDULE, "The roles and/or users that may perform scheduling." );
                }
                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    // make sure it isn't getting saved with a recursive parent hierarchy
                    var parentIds = new List<int>();
                    parentIds.Add( this.Id );
                    var parent = this.InheritedGroupTypeId.HasValue ? ( this.InheritedGroupType ?? new GroupTypeService( new RockContext() ).Get( this.InheritedGroupTypeId.Value ) ) : null;
                    while ( parent != null )
                    {
                        if ( parentIds.Contains( parent.Id ) )
                        {
                            this.ValidationResults.Add( new ValidationResult( "Parent Group Type cannot be a child of this Group Type (recursion)" ) );
                            return false;
                        }
                        else
                        {
                            parentIds.Add( parent.Id );
                            parent = parent.InheritedGroupType;
                        }
                    }

                    if ( string.IsNullOrEmpty( GroupViewLavaTemplate ) )
                    {
                        this.ValidationResults.Add( new ValidationResult( "Lava template for group view is mandatory." ) );
                        return false;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, EntityState state )
        {
            if (state == EntityState.Deleted)
            {
                ChildGroupTypes.Clear();

                // manually delete any grouprequirements of this grouptype since it can't be cascade deleted
                var groupRequirementService = new GroupRequirementService( dbContext as RockContext );
                var groupRequirements = groupRequirementService.Queryable().Where( a => a.GroupTypeId.HasValue && a.GroupTypeId == this.Id ).ToList();
                if ( groupRequirements.Any() )
                {
                    groupRequirementService.DeleteRange( groupRequirements );
                }
            }

            // clean up the index
            if ( state == EntityState.Deleted && IsIndexEnabled )
            {
                this.DeleteIndexedDocumentsByGroupType( this.Id );
            }
            else if ( state == EntityState.Modified )
            {
                // check if indexing is enabled
                var changeEntry = dbContext.ChangeTracker.Entries<GroupType>().Where( a => a.Entity == this ).FirstOrDefault();
                if ( changeEntry != null )
                {
                    var originalIndexState = (bool)changeEntry.OriginalValues["IsIndexEnabled"];

                    if ( originalIndexState == true && IsIndexEnabled == false )
                    {
                        // clear out index items
                        this.DeleteIndexedDocumentsByGroupType( Id );
                    }
                    else if ( IsIndexEnabled == true )
                    {
                        // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed
                        BulkIndexDocumentsByGroupType( Id );
                    }
                }
            }

            base.PreSaveChanges( dbContext, state );
        }

        /// <summary>
        /// Gets a list of GroupType Ids, including our own Id, that identifies the
        /// inheritence tree.
        /// </summary>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <returns>A list of GroupType Ids, including our own Id, that identifies the inheritance tree.</returns>
        public List<int> GetInheritedGroupTypeIds( Rock.Data.RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            //
            // Can't use GroupTypeCache here since it loads attributes and could
            // result in a recursive stack overflow situation when we are called
            // from a GetInheritedAttributes() method.
            //
            var groupTypeService = new GroupTypeService( rockContext );
            var groupTypeIds = new List<int>();

            var groupType = this;

            //
            // Loop until we find a recursive loop or run out of parent group types.
            //
            while ( groupType != null && !groupTypeIds.Contains( groupType.Id ) )
            {
                groupTypeIds.Insert( 0, groupType.Id );

                if ( groupType.InheritedGroupTypeId.HasValue )
                {
                    groupType = groupType.InheritedGroupType ?? groupTypeService
                        .Queryable().AsNoTracking().FirstOrDefault( t => t.Id == ( groupType.InheritedGroupTypeId ?? 0 ) );
                }
                else
                {
                    groupType = null;
                }
            }

            return groupTypeIds;
        }

        /// <summary>
        /// Gets a list of all attributes defined for the GroupTypes specified that
        /// match the entityTypeQualifierColumn and the GroupType Ids.
        /// </summary>
        /// <param name="rockContext">The database context to operate in.</param>
        /// <param name="entityTypeId">The Entity Type Id for which Attributes to load.</param>
        /// <param name="entityTypeQualifierColumn">The EntityTypeQualifierColumn value to match against.</param>
        /// <returns>A list of attributes defined in the inheritance tree.</returns>
        public List<AttributeCache> GetInheritedAttributesForQualifier( Rock.Data.RockContext rockContext, int entityTypeId, string entityTypeQualifierColumn )
        {
            var groupTypeIds = GetInheritedGroupTypeIds( rockContext );

            var inheritedAttributes = new Dictionary<int, List<AttributeCache>>();
            groupTypeIds.ForEach( g => inheritedAttributes.Add( g, new List<AttributeCache>() ) );

            //
            // Walk each group type and generate a list of matching attributes.
            //
            foreach ( var entityAttributes in AttributeCache.GetByEntity( entityTypeId ) )
            {
                // group type ids exist and qualifier is for a group type id
                if ( string.Compare( entityAttributes.EntityTypeQualifierColumn, entityTypeQualifierColumn, true ) == 0 )
                {
                    int groupTypeIdValue = int.MinValue;
                    if ( int.TryParse( entityAttributes.EntityTypeQualifierValue, out groupTypeIdValue ) && groupTypeIds.Contains( groupTypeIdValue ) )
                    {
                        foreach ( int attributeId in entityAttributes.AttributeIds )
                        {
                            inheritedAttributes[groupTypeIdValue].Add( AttributeCache.Get( attributeId ) );
                        }
                    }
                }
            }

            //
            // Walk the generated list of attribute groups and put them, ordered, into a list
            // of inherited attributes.
            //
            var attributes = new List<AttributeCache>();
            foreach ( var attributeGroup in inheritedAttributes )
            {
                foreach ( var attribute in attributeGroup.Value.OrderBy( a => a.Order ) )
                {
                    attributes.Add( attribute );
                }
            }

            return attributes;
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            return GetInheritedAttributesForQualifier( rockContext, TypeId, "Id" );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name of the GroupType that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the name of the GroupType that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Index Methods

        /// <summary>
        /// Queues groups of this type to have their indexes deleted
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void DeleteIndexedDocumentsByGroupType( int groupTypeId )
        {
            var groupIds = new GroupService( new RockContext() ).Queryable()
                .Where( i => i.GroupTypeId == groupTypeId )
                .Select( a => a.Id ).ToList();

            int groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            foreach ( var groupId in groupIds )
            {
                var transaction = new DeleteIndexEntityTransaction { EntityId = groupId, EntityTypeId = groupEntityTypeId };
                transaction.Enqueue();
            }
        }

        /// <summary>
        /// Queues groups of this type to have their indexes updated
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void BulkIndexDocumentsByGroupType( int groupTypeId )
        {
            var groupIds = new GroupService( new RockContext() ).Queryable()
                .Where( i => i.GroupTypeId == groupTypeId )
                .Select( a => a.Id ).ToList();

            int groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>().Value;

            foreach ( var groupId in groupIds )
            {
                var transaction = new IndexEntityTransaction { EntityId = groupId, EntityTypeId = groupEntityTypeId };
                transaction.Enqueue();
            }
        }
        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return GroupTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            var parentGroupTypeIds = new GroupTypeService( dbContext as RockContext ).GetParentGroupTypes( this.Id ).Select( a => a.Id ).ToList();
            if ( parentGroupTypeIds?.Any() == true )
            {
                foreach ( var parentGroupTypeId in parentGroupTypeIds )
                {
                    GroupTypeCache.UpdateCachedEntity( parentGroupTypeId, EntityState.Detached );
                }
            }

            GroupTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Group Type Configuration class.
    /// </summary>
    public partial class GroupTypeConfiguration : EntityTypeConfiguration<GroupType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupTypeConfiguration"/> class.
        /// </summary>
        public GroupTypeConfiguration()
        {
            this.HasMany( p => p.ChildGroupTypes ).WithMany( c => c.ParentGroupTypes ).Map( m => { m.MapLeftKey( "GroupTypeId" ); m.MapRightKey( "ChildGroupTypeId" ); m.ToTable( "GroupTypeAssociation" ); } );
            this.HasOptional( p => p.DefaultGroupRole ).WithMany().HasForeignKey( p => p.DefaultGroupRoleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.GroupStatusDefinedType ).WithMany().HasForeignKey( p => p.GroupStatusDefinedTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.InheritedGroupType ).WithMany().HasForeignKey( p => p.InheritedGroupTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ScheduleConfirmationSystemEmail ).WithMany().HasForeignKey( p => p.ScheduleConfirmationSystemEmailId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ScheduleReminderSystemEmail ).WithMany().HasForeignKey( p => p.ScheduleReminderSystemEmailId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ScheduleCancellationWorkflowType ).WithMany().HasForeignKey( p => p.ScheduleCancellationWorkflowTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Represents and indicates the  attendance rule to use when a <see cref="Rock.Model.Person"/> checks in to a <see cref="Rock.Model.Group"/> of this <see cref="Rock.Model.GroupType"/>
    /// </summary>
    public enum AttendanceRule
    {
        /// <summary>
        /// None, person does not need to belong to the group, and they will not automatically 
        /// be added to the group
        /// </summary>
        None = 0,

        /// <summary>
        /// Person will be added to the group whenever they check-in
        /// </summary>
        AddOnCheckIn = 1,

        /// <summary>
        /// User must already belong to the group before they will be allowed to check-in
        /// </summary>
        AlreadyBelongs = 2
    }

    /// <summary>
    /// Group Capacity Rule
    /// </summary>
    public enum GroupCapacityRule
    {
        /// <summary>
        /// The group does not have capacity limitations
        /// </summary>
        None = 0,

        /// <summary>
        /// The group can not go over capacity
        /// </summary>
        Hard = 1,

        /// <summary>
        /// A warning will be shown if a group is going to go over capacity
        /// </summary>
        Soft = 2
    }

    /// <summary>
    /// Represents and indicates the type of location picker to use when setting a location for a group and/or when searching for group(s)
    /// </summary>
    [Flags]
    public enum GroupLocationPickerMode
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// An Address
        /// </summary>
        Address = 1,

        /// <summary>
        /// A Named location (Building, Room)
        /// </summary>
        Named = 2,

        /// <summary>
        /// A Geographic point (Latitude/Longitude)
        /// </summary>
        Point = 4,

        /// <summary>
        /// A Geographic Polygon
        /// </summary>
        Polygon = 8,

        /// <summary>
        /// A Group Member's address
        /// </summary>
        GroupMember = 16,

        /// <summary>
        /// All
        /// </summary>
        All = Address | Named | Point | Polygon | GroupMember

    }
    #endregion

}
