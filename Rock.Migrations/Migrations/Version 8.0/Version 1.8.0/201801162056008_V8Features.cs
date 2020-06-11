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
    public partial class V8Features : Rock.Migrations.RockMigration
    {

        #region Constants

        const string ENTITY_TYPE_BADGE_TOP_PERSON_SIGNAL = "1BC1335A-A37E-4C02-83C1-AD2883FD954E";
        const string ENTITY_TYPE_SIGNAL_TYPE_CACHE = "0FCFAEE7-F945-4FC7-AD46-6F3CADB28C9B";
        const string ENTITY_TYPE_REPORTING_SIGNAL_SELECT = "63A79A4D-A3B0-4B97-B5AB-CE93FC3C03C7";
        const string ENTITY_TYPE_REPORTING_HAS_SIGNAL_FILTER = "5DC0EEB7-2B9E-4828-883B-0E7090C992AA";

        const string BADGE_TOP_PERSON_SIGNAL = "B4B336CE-137E-44BE-9123-27740D0064C2";

        const string BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST = "250FF1C3-B2AE-4AFD-BEFA-29C45BEB30D2";
        const string BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL = "E9AB79D9-429F-410D-B4A8-327829FC7C63";
        const string BLOCK_TYPE_SIGNAL_LIST = "813CFCCF-30BF-4A2F-BB55-F240A3B7809F";

        const string PAGE_SIGNAL_TYPE_LIST = "EA6B3CF2-9DE2-4CF0-8EFA-01B76B51C329";
        const string PAGE_SIGNAL_TYPE_DETAIL = "67AF60BC-D814-4DBC-BA64-D12128CCF52C";

        const string BLOCK_SIGNAL_TYPE_LIST = "1134FF9C-8539-462F-95B4-65B89178B8EA";
        const string BLOCK_SIGNAL_TYPE_DETAIL = "72D04752-0024-4ED9-BBD0-0F12714ABB31";
        const string BLOCK_SIGNAL_LIST = "C5DA2773-E12B-4E63-A392-7A775C09BCE5";

        #endregion

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            #region Table Changes

            CreateTable(
                "dbo.PersonSignal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        SignalTypeId = c.Int(nullable: false),
                        OwnerPersonAliasId = c.Int(nullable: false),
                        Note = c.String(),
                        ExpirationDate = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.OwnerPersonAliasId)
                .ForeignKey("dbo.Person", t => t.PersonId, cascadeDelete: true)
                .ForeignKey("dbo.SignalType", t => t.SignalTypeId, cascadeDelete: true)
                .Index(t => t.PersonId)
                .Index(t => t.SignalTypeId)
                .Index(t => t.OwnerPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.SignalType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        SignalColor = c.String(nullable: false, maxLength: 100),
                        SignalIconCssClass = c.String(maxLength: 100),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.Name, unique: true)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.DataViewPersistedValue",
                c => new
                    {
                        DataViewId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataViewId, t.EntityId })
                .ForeignKey("dbo.DataView", t => t.DataViewId, cascadeDelete: true)
                .Index(t => t.DataViewId);
            
            AddColumn("dbo.Person", "TopSignalColor", c => c.String(maxLength: 100));
            AddColumn("dbo.Person", "TopSignalIconCssClass", c => c.String(maxLength: 100));
            AddColumn("dbo.Person", "TopSignalId", c => c.Int());
            AddColumn("dbo.Person", "AgeClassification", c => c.Int(nullable: false));
            AddColumn("dbo.Person", "PrimaryFamilyId", c => c.Int());
            AddColumn("dbo.GroupType", "GroupViewLavaTemplate", c => c.String());
            AddColumn("dbo.GroupType", "AllowSpecificGroupMemberAttributes", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "EnableSpecificGroupRequirements", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AllowGroupSync", c => c.Boolean(nullable: false));
            AddColumn("dbo.GroupType", "AllowSpecificGroupMemberWorkflows", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataView", "PersistedScheduleIntervalMinutes", c => c.Int());
            AddColumn("dbo.DataView", "PersistedLastRefreshDateTime", c => c.DateTime());
            AddColumn("dbo.RegistrationInstance", "RegistrationInstructions", c => c.String());
            AddColumn("dbo.RegistrationTemplate", "RegistrationInstructions", c => c.String());
            CreateIndex("dbo.Person", "PrimaryFamilyId");
            AddForeignKey("dbo.Person", "PrimaryFamilyId", "dbo.Group", "Id");

            #endregion

            #region Group View Lava Template 

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

            #endregion

            #region Person Signals

            //
            // Ensure all the entity types are registered with known Guids.
            //
            RockMigrationHelper.UpdateEntityType( "Rock.Model.SignalType", "Signal Type", "Rock.Model.SignalType, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", true, true, Rock.SystemGuid.EntityType.SIGNAL_TYPE );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.PersonSignal", "Person Signal", "Rock.Model.PersonSignal, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", true, true, Rock.SystemGuid.EntityType.PERSON_SIGNAL );
            RockMigrationHelper.UpdateEntityType( "Rock.Web.Cache.SignalTypeCache", "Signal Type Cache", "Rock.Web.Cache.SignalTypeCache, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_SIGNAL_TYPE_CACHE );
            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.TopPersonSignal", "Top Person Signal", "Rock.PersonProfile.Badge.TopPersonSignal, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_BADGE_TOP_PERSON_SIGNAL );
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataSelect.Person.SignalSelect", "Signal Select", "Rock.Reporting.DataSelect.Person.SignalSelect, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_REPORTING_SIGNAL_SELECT );
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataFilter.Person.HasSignalFilter", "Has Signal Filter", "Rock.Reporting.DataFilter.Person.HasSignalFilter, Rock, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null", false, true, ENTITY_TYPE_REPORTING_HAS_SIGNAL_FILTER );

            //
            // Set security on the reporting entity types so only Rock Administrators can
            // use them by default.
            //
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataSelect.Person.SignalSelect",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                (int)Rock.Model.SpecialRole.AllUsers,
                "F466F3C8-BDAF-439B-B43D-EBE38D3E23B1" );
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataSelect.Person.SignalSelect",
                1,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                (int)Rock.Model.SpecialRole.AllUsers,
                "A3B5206D-E52A-4545-987F-B2FF39AA4818" );

            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataFilter.Person.HasSignalFilter",
                0,
                Rock.Security.Authorization.VIEW,
                true,
                Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
                (int)Rock.Model.SpecialRole.AllUsers,
                "6B7790A1-68C0-4453-B3D5-A58BE3928935" );
            RockMigrationHelper.AddSecurityAuthForEntityType(
                "Rock.Reporting.DataFilter.Person.HasSignalFilter",
                1,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                (int)Rock.Model.SpecialRole.AllUsers,
                "E8A3332F-8D63-43DC-B13E-B3D947D9C9AD" );

            //
            // Register the Top Person Signal badge in the database.
            //
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.UpdatePersonBadge(
                "Top Person Signal",
                "Shows the top person badge and the number of signals.",
                "Rock.PersonProfile.Badge.TopPersonSignal",
                0,
                BADGE_TOP_PERSON_SIGNAL );
#pragma warning restore CS0618 // Type or member is obsolete

            //
            // Add the signal badge to the left badge bar on the person details page.
            //
            Sql( string.Format( @"
    UPDATE AV
    	SET AV.[Value] = AV.[Value] + CASE WHEN AV.[Value] != '' THEN ',' END + '{0}'
        FROM AttributeValue AS AV
        LEFT JOIN [Block] AS B ON B.Id = AV.EntityId
        LEFT JOIN [Attribute] AS A ON A.Id = AV.[AttributeId]
        WHERE B.[Guid] = '98A30DD7-8665-4C6D-B1BB-A8380E862A04'
          AND A.[Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A'
    	  AND AV.[Value] NOT LIKE '%{0}%'
", BADGE_TOP_PERSON_SIGNAL ) );

            //
            // Add the Job for calculating person signals.
            //
            Sql( @"
    INSERT INTO [ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid] )
    VALUES (
         0
        ,1
        ,'Calculate Person Signals'
        ,'Re-calculates all person signals to ensure that the top-most signal is still the current one.'
        ,'Rock.Jobs.CalculatePersonSignals'
        ,'0 15 3 1/1 * ? *'
        ,1
        ,'82B6315E-53D0-43C6-8F09-C5B0E8890B8D');
" );

            //
            // Update all the new Block Types.
            //
            RockMigrationHelper.UpdateBlockType(
                "Person Signal Type List",
                "Shows a list of all signal types.",
                "~/Blocks/CRM/PersonSignalTypeList.ascx",
                "CRM",
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST );
            RockMigrationHelper.AddBlockTypeAttribute(
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST,
                SystemGuid.FieldType.PAGE_REFERENCE,
                "Detail Page",
                "DetailPage",
                string.Empty,
                string.Empty,
                0,
                string.Empty,
                "5ECE9E10-0C47-4CC1-8491-E73873E6BD66",
                true );

            RockMigrationHelper.UpdateBlockType(
                "Person Signal Type Detail",
                "Shows the details of a particular person signal type.",
                "~/Blocks/CRM/PersonSignalTypeDetail.ascx",
                "CRM",
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL );

            RockMigrationHelper.UpdateBlockType(
                "Signal List",
                "Lists all the signals on a person.",
                "~/Blocks/CRM/PersonDetail/SignalList.ascx",
                "CRM > Person Detail",
                BLOCK_TYPE_SIGNAL_LIST );

            //
            // Create the Security > Person Signal Types page.
            //
            RockMigrationHelper.AddPage(
                "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", // Security
                "D65F783D-87A9-4CC9-8110-E83466A0EADB", // Full Width
                "Person Signal Types",
                string.Empty,
                PAGE_SIGNAL_TYPE_LIST,
                "fa fa-flag" );
            RockMigrationHelper.AddBlock(
                PAGE_SIGNAL_TYPE_LIST,
                string.Empty,
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST,
                "Person Signal Type List",
                "Main",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_TYPE_LIST );
            RockMigrationHelper.AddBlockAttributeValue(
                BLOCK_SIGNAL_TYPE_LIST,
                "5ECE9E10-0C47-4CC1-8491-E73873E6BD66",
                PAGE_SIGNAL_TYPE_DETAIL );

            //
            // Create the Security > Person Signal Types > Person Signal Type Detail page.
            //
            RockMigrationHelper.AddPage(
                PAGE_SIGNAL_TYPE_LIST,
                "D65F783D-87A9-4CC9-8110-E83466A0EADB", // Full Width
                "Person Signal Type Detail",
                string.Empty,
                PAGE_SIGNAL_TYPE_DETAIL,
                "fa fa-flag" );
            RockMigrationHelper.AddBlock(
                PAGE_SIGNAL_TYPE_DETAIL,
                string.Empty,
                BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL,
                "Person Signal Type Detail",
                "Main",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_TYPE_DETAIL );

            //
            // Insert the Signal List block at top of the Person Detail > Security page.
            //
            RockMigrationHelper.AddBlock(
                "0E56F56E-FB32-4827-A69A-B90D43CB47F5",
                "",
                BLOCK_TYPE_SIGNAL_LIST,
                "Signal List",
                "SectionC1",
                string.Empty,
                string.Empty,
                0,
                BLOCK_SIGNAL_LIST );
            Sql( string.Format( @"
    UPDATE [B]
    	SET [B].[Order] = [B].[Order] + 1
    	FROM [Block] AS [B]
    	LEFT JOIN [Page] AS [P] ON [P].[Id] = [B].[PageId]
    	WHERE [P].[Guid] = '0E56F56E-FB32-4827-A69A-B90D43CB47F5'
	      AND [B].[Zone] = 'SectionC1'
    	  AND [B].[Guid] != '{0}'
", BLOCK_SIGNAL_LIST ) );

            #endregion

            #region Social Field Types

            // Attrib for BlockType: Person Bio:Social Media Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Social Media Category", "SocialMediaCategory", "", "The Attribute Category to display attributes from", 14, @"DD8F467D-B83C-444F-B04C-C681167046A1", "FD51EC2E-D660-4B79-95C7-39214D4BEA8E" );

            RockMigrationHelper.UpdateFieldType( "Social Media Account", "Used to configure and display the social Network accounts.", "Rock", "Rock.Field.Types.SocialMediaAccountFieldType", Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT );

            Sql( string.Format( @"
    DECLARE @socialAccountFieldType int = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{0}' )
    UPDATE [Attribute] SET 
        [IconCssClass] = '',
        [FieldTypeId] = @socialAccountFieldType
    WHERE [Guid] in ('{1}','{2}','{3}')
", Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT,
            Rock.SystemGuid.Attribute.PERSON_FACEBOOK,
            Rock.SystemGuid.Attribute.PERSON_TWITTER,
            Rock.SystemGuid.Attribute.PERSON_INSTAGRAM ) );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "name", "Facebook", "ECFCD6FC-6E2C-40CC-8F60-DFA256C29C7A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "iconcssclass", "fa fa-facebook", "FC271703-58E5-45EC-8A48-B0D5B58C606A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "color", "#44619d", "4FF8C097-9C2D-4022-BF83-A110A1DEE56C" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "BC8F9FEF-59D6-4BC4-84B3-BC6EC52CECED" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "baseurl", "http://www.facebook.com/", "204A1861-D7DA-4C0C-94E7-81B19DDCD4F4" );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "name", "Twitter", "E8B59009-DD08-4B8E-8A86-8602D45E2BDA" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "iconcssclass", "fa fa-twitter", "5FC5E5F8-EB4B-44F0-943C-0CD487191FF3" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "color", "#55acee", "5F9E35DA-B8E9-4D0A-8FEB-F5B5BB9A43E7" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "6FFF488B-C7A8-410A-ADC2-3D9D21706511" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "baseurl", "http://www.twitter.com/", "57FB78D1-A1F3-48F0-AE38-4EAC65732140" );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "name", "Instagram", "D848B8A8-E8AA-42FE-92A6-218135DDC426" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "iconcssclass", "fa fa-instagram", "FB1E8A30-7299-4208-8AAF-2C5051456D46" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "color", "#39688f", "A8CE78E3-AE6F-4F60-A63E-4E44BD420A4E" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "02820F4F-476A-448F-A869-14206625670C" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "baseurl", "http://www.instagram.com/", "A00E1DE5-F37F-4CBE-8FE4-F89CBC1AE055" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT, Rock.SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL, "Snapchat", "Snapchat", "", "Link to person's Snapchat page", 3, "", Rock.SystemGuid.Attribute.PERSON_SNAPCHAT );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "name", "Snapchat", "19283998-F3D7-41A1-B9F5-36FE17CC4566" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-snapchat-ghost text-shadow", "E9168011-2719-40EB-A082-9337B5F52233" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "color", "#FFFC00", "C396FB2E-E38B-4D9E-9DFC-3BC9F2D04C9A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "texttemplate", "<a href='{{value}}' target='_blank'>{{ value | Url:'segments' | Last }}</a>", "7B3650EF-8F42-40DF-A729-9BEF19941DD8" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "baseurl", "http://www.snapchat.com/", "E2115559-2B43-4630-8A0F-7F1B0141D62C" );

            #endregion

            #region Personal Device Type UI

            RockMigrationHelper.AddDefinedTypeAttribute( "C1848F4C-D6F8-4514-8DB6-CD3C19621025", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon CSS Class", "IconCssClass", "", 1001, "", "DC0E00D2-7694-410E-82C0-E99A097D0A30" );

            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Personal Device", "", "B2786294-99DC-477E-871D-2E28FCE00A98", "fa fa-mobile" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B2786294-99DC-477E-871D-2E28FCE00A98", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Personal Device Interactions", "", "5A31D3D3-91A7-409F-8AFF-C3802AC055EC", "fa fa-mobile" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Personal Device Interactions", "Shows a list of all interactions for a personal device.", "~/Blocks/Crm/PersonalDeviceInteractions.ascx", "CRM", "D6224911-2590-427F-9DCE-6D14E79806BA" );
            RockMigrationHelper.UpdateBlockType( "Personal Devices", "Shows a list of all person devices.", "~/Blocks/Crm/PersonalDevices.ascx", "CRM", "2D90562E-7332-46DB-9100-0C4106151CA1" );

            // Add Block to Page: Personal Device, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "B2786294-99DC-477E-871D-2E28FCE00A98", "", "2D90562E-7332-46DB-9100-0C4106151CA1", "Personal Devices", "Main", @"", @"", 0, "B5A94C63-869C-4B4C-B129-9E098EF5537C" );
            // Add Block to Page: Personal Device Interactions, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "5A31D3D3-91A7-409F-8AFF-C3802AC055EC", "", "D6224911-2590-427F-9DCE-6D14E79806BA", "Personal Device Interactions", "Main", @"", @"", 0, "4276AF44-78F5-4864-A3DF-AF6BBE82F2FF" );

            // Attrib for BlockType: Personal Devices:Lava Template              
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D90562E-7332-46DB-9100-0C4106151CA1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display content", 2, @"
<div class=""panel panel-block"">       
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <i class=""fa fa-mobile""></i>
            {{ Person.FullName }}
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md"">
            {% for item in PersonalDevices %}
                <div class=""col-md-3 col-sm-4"">                  
                    <div class=""well margin-b-none rollover-container"">                        
                        <a class=""pull-right rollover-item btn btn-xs btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm('Are you sure you want to delete this Device?', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:'DeleteDevice' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""margin-v-none"">
                                {% if item.DeviceIconCssClass != '' %}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {% endif %}
                                {% if item.PersonalDevice.NotificationsEnabled == true %}
                                    <i class=""fa fa-comment-o""></i>
                                {% endif %}
                            </h3>
                            <dl>
                                {% if item.PlatformValue != '' %}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{% endif %}
                                {% if item.PersonalDevice.CreatedDateTime != null %}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{% endif %}                              
                                {% if item.PersonalDevice.MACAddress != '' and item.PersonalDevice.MACAddress != null %}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{% endif %}
                            </dl>
                        </div>
                        {% if LinkUrl != '' %}
                            <a href=""{{ LinkUrl | Replace:'[Id]',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}
        </div>
    </div>
</div>", "24CAD424-3DAD-407C-9EA5-90FAD6293F81" );

            // Attrib for BlockType: Personal Devices:Interactions Page              
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D90562E-7332-46DB-9100-0C4106151CA1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Interactions Page", "InteractionsPage", "", "The interactions associated with a specific personal device.", 0, @"", "EE777BF3-9953-4830-A7A9-37CCA6EAF175" );

            // Attrib Value for Block:Personal Devices, Attribute:Lava Template Page: Personal Device, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B5A94C63-869C-4B4C-B129-9E098EF5537C", "24CAD424-3DAD-407C-9EA5-90FAD6293F81", @"
<div class=""panel panel-block"">       
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <i class=""fa fa-mobile""></i>
            {{ Person.FullName }}
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md"">
            {% for item in PersonalDevices %}
                <div class=""col-md-3 col-sm-4"">                  
                    <div class=""well margin-b-none rollover-container"">                        
                        <a class=""pull-right rollover-item btn btn-xs btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm('Are you sure you want to delete this Device?', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:'DeleteDevice' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""margin-v-none"">
                                {% if item.DeviceIconCssClass != '' %}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {% endif %}
                                {% if item.PersonalDevice.NotificationsEnabled == true %}
                                    <i class=""fa fa-comment-o""></i>
                                {% endif %}
                            </h3>
                            <dl>
                                {% if item.PlatformValue != '' %}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{% endif %}
                                {% if item.PersonalDevice.CreatedDateTime != null %}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{% endif %}                              
                                {% if item.PersonalDevice.MACAddress != '' and item.PersonalDevice.MACAddress != null %}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{% endif %}
                            </dl>
                        </div>
                        {% if LinkUrl != '' %}
                            <a href=""{{ LinkUrl | Replace:'[Id]',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}
        </div>
    </div>
</div>" );
            // Attrib Value for Block:Personal Devices, Attribute:Interactions Page Page: Personal Device, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue( "B5A94C63-869C-4B4C-B129-9E098EF5537C", "EE777BF3-9953-4830-A7A9-37CCA6EAF175", @"5a31d3d3-91a7-409f-8aff-c3802ac055ec" );

            RockMigrationHelper.UpdateEntityType( "Rock.PersonProfile.Badge.PersonalDevice", "C92E1D6C-EE4B-4BD6-B5C6-9E6071243341", false, true );
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.UpdatePersonBadge( "Personal Devices", "Badge showing the number of personal devices that have been associated to a person.",
                "Rock.PersonProfile.Badge.PersonalDevice", 0, "307CB56D-140C-4CC9-8B54-DD551CC40174" );
#pragma warning restore CS0618 // Type or member is obsolete

            #endregion

            #region Data Integrity

            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Data Integrity Settings", "", "4818E7C6-4D21-4657-B4E7-464B61160EB2", "fa fa-tachometer" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Data Integrity Settings", "Block used to set values specific to data integrity (NCOA, Data Automation, Etc).", "~/Blocks/Administration/DataIntegritySettings.ascx", "Administration", "BA4292F9-AB6A-4464-9F0B-FC580B92C4BF" );
            RockMigrationHelper.AddBlock( true, "4818E7C6-4D21-4657-B4E7-464B61160EB2", "", "BA4292F9-AB6A-4464-9F0B-FC580B92C4BF", "Data Integrity Settings", "Main", @"", @"", 0, "816CBFA2-B0CD-4EAB-8E42-7F77216815DA" );

            RockMigrationHelper.AddDefinedTypeAttribute( "E17D5988-0372-4792-82CF-9E37C79F7319", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Automated Reactivation", "AllowAutomatedReactivation", "", 1001, "True", "E47870C0-17C7-4556-A922-D7866DFC2C57" );
            RockMigrationHelper.AddAttributeQualifier( "E47870C0-17C7-4556-A922-D7866DFC2C57", "falsetext", "No", "0EBAAE3D-DC46-4834-8EE7-F44CA07D43E6" );
            RockMigrationHelper.AddAttributeQualifier( "E47870C0-17C7-4556-A922-D7866DFC2C57", "truetext", "Yes", "C322A622-C1FE-45C7-87B8-60A357BDC2D8" );

            RockMigrationHelper.UpdateDefinedValue( "E17D5988-0372-4792-82CF-9E37C79F7319", "Does not attend with family", "The individual has not attended with family.", "2BDE800A-C562-4077-9636-5C68770D9676", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "05D35BC4-5816-4210-965F-1BF44F35A16A", "E47870C0-17C7-4556-A922-D7866DFC2C57", @"False", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2BDE800A-C562-4077-9636-5C68770D9676", "E47870C0-17C7-4556-A922-D7866DFC2C57", @"False", false );

            #endregion

            #region Personix

            Sql( @"
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =15
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =11
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category A' WHERE [Id] =19
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =7
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =16
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =8
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =12
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =2
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =14
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category A' WHERE [Id] =20
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =9
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =3
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =18
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =10
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =13
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =4
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =5
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =1
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =17
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category C' WHERE [Id] =21
    UPDATE [MetaPersonicxLifestageGroup] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =6

    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =1
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =2
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =3
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =4
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category A' WHERE [Id] =9
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =8
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =11
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =7
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category A' WHERE [Id] =5
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =10
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =6
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =18
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =12
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =17
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =13
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =32
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =29
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category A' WHERE [Id] =27
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =20
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category A' WHERE [Id] =19
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category A' WHERE [Id] =36
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =15
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =31
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =25
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =23
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =16
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =22
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =14
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =28
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =21
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =26
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =30
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =38
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =49
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =24
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =35
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =34
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =37
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =50
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =46
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =51
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category B' WHERE [Id] =44
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =43
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category B' WHERE [Id] =42
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =40
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =53
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =55
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =48
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category B' WHERE [Id] =47
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =56
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category A', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =33
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =54
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category C' WHERE [Id] =66
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category B', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =41
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =68
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category C' WHERE [Id] =65
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =39
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =69
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Mature', [NetWorthLevel] = 'Category C' WHERE [Id] =64
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =61
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =57
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =62
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =58
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =45
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =60
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =52
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =59
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Middle Age', [NetWorthLevel] = 'Category C' WHERE [Id] =63
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =67
    UPDATE [MetaPersonicxLifestageCluster] SET [IncomeLevel] = 'Category C', [LifeStageLevel] = 'Young Adult', [NetWorthLevel] = 'Category C' WHERE [Id] =70
" );

            #endregion

            #region Label Field Type

            RockMigrationHelper.UpdateFieldType( "Label", "Labels that can be printed during check-in", "Rock", "Rock.Field.Types.LabelFieldType", Rock.SystemGuid.FieldType.LABEL );

            Sql( $@"
    DECLARE @BinaryFileFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '{Rock.SystemGuid.FieldType.BINARY_FILE}' )
    DECLARE @LabelFieldTypeId int = ( SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '{Rock.SystemGuid.FieldType.LABEL}' )
    
    UPDATE A SET [FieldTypeId] = @LabelFieldTypeId
    FROM [Attribute] A 
	INNER JOIN [AttributeQualifier] Q 
		ON Q.[AttributeId] = A.[Id]
		AND Q.[Key] = 'binaryFileType'
		AND Q.[Value] = '{Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL}'
	WHERE A.[FieldTypeId] = @BinaryFileFieldTypeId
" );

            #endregion

            #region Add/Update Persisted Dataviews Job

            Sql( @"
    INSERT INTO [dbo].[ServiceJob] (
         [IsSystem]
        ,[IsActive]
        ,[Name]
        ,[Description]
        ,[Class]
        ,[CronExpression]
        ,[NotificationStatus]
        ,[Guid]
    )
    VALUES (
         0 
        ,1 
        ,'Update Persisted DataViews'
        ,'Job to makes sure that persisted dataviews are updated based on their schedule interval.'
        ,'Rock.Jobs.UpdatePersistedDataviews'
        ,'0 0/1 * 1/1 * ? *'
        ,3
        ,'11900FEC-B5D4-4CF8-8B48-136F5BF06CB0')
" );

            #endregion

            #region Update Group Config

            Sql(
               @"DECLARE @securityRoleId INT, @groupMemberEntityType INT;
        SELECT @securityRoleId = [Id] FROM [GroupType] WHERE [Guid]='AECE949F-704C-483E-A4FB-93D5E4720C4C'

        SELECT @groupMemberEntityType = [Id] FROM [EntityType] WHERE [Guid]='49668B95-FEDC-43DD-8085-D2B0D6343C48'

        UPDATE 
	        [GroupType] 
        SET 
	        [AllowGroupSync] = 1 
        WHERE [Id] IN 
			        (SELECT Distinct [GroupTypeId] FROM [Group] WHERE [SyncDataViewId] IS NOT NULL) OR
			        [Id] IN (SELECT [Id] FROM [GroupType] WHERE [InheritedGroupTypeId]=@securityRoleId ) OR 
			        [Id]=@securityRoleId

        UPDATE 
	        [GroupType] 
        SET 
	        [EnableSpecificGroupRequirements] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId] FROM [Group] A INNER JOIN [GroupRequirement] B ON A.[Id]=B.[GroupId]
	        ) 

        UPDATE 
	        [GroupType]
        SET 
	        [AllowSpecificGroupMemberAttributes] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId]
	        FROM 
		        [Group] A INNER JOIN 
		        [Attribute] B  
	        ON 
		        A.[Id] = CONVERT(INT,B.[EntityTypeQualifierValue]) 
	        WHERE 
		        B.[EntityTypeQualifierColumn]='GroupId' AND
		        B.[EntityTypeId]=@groupMemberEntityType) 



        UPDATE 
	        [GroupType] 
        SET 
	        [AllowSpecificGroupMemberWorkflows] = 1 
        WHERE 
	        [Id] IN  (
	        SELECT A.[GroupTypeId] FROM [Group] A INNER JOIN [GroupMemberWorkflowTrigger] B ON A.[Id]=B.[GroupId]
        ) 
" );

            #endregion

            #region Migration Catch-up

            // Attrib for BlockType: Pledge List:Show Account Summary
            RockMigrationHelper.UpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Account Summary", "ShowAccountSummary", "", @"Should the account summary be displayed at the bottom of the list?", 5, @"False", "B9594D47-4E75-4336-9F92-6C96B8CBEB42" );

            #endregion

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            #region Delete Persisted Dataviews Job

            Sql( "DELETE FROM [ServiceJob] where [Guid] = '11900FEC-B5D4-4CF8-8B48-136F5BF06CB0'" );

            #endregion

            #region Data Integrity

            RockMigrationHelper.DeleteAttribute( "E47870C0-17C7-4556-A922-D7866DFC2C57" ); // AllowAutomatedReactivation	0
            RockMigrationHelper.DeleteDefinedValue( "2BDE800A-C562-4077-9636-5C68770D9676" ); // Does not attend with family	1
            // Remove Block: Data Integrity Settings, from Page: Data Integrity Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "816CBFA2-B0CD-4EAB-8E42-7F77216815DA" );
            RockMigrationHelper.DeleteBlockType( "BA4292F9-AB6A-4464-9F0B-FC580B92C4BF" ); // Data Integrity Settings
            RockMigrationHelper.DeletePage( "4818E7C6-4D21-4657-B4E7-464B61160EB2" ); //  Page: Data Integrity Settings, Layout: Full Width, Site: Rock RMS

            #endregion

            #region Personal Device Type UI

            // Attrib for BlockType: Personal Devices:Interactions Page
            RockMigrationHelper.DeleteAttribute( "EE777BF3-9953-4830-A7A9-37CCA6EAF175" );
            // Attrib for BlockType: Personal Devices:Lava Template              
            RockMigrationHelper.DeleteAttribute( "24CAD424-3DAD-407C-9EA5-90FAD6293F81" );

            // Remove Block: Personal Device Interactions, from Page: Personal Device Interactions, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "4276AF44-78F5-4864-A3DF-AF6BBE82F2FF" );
            // Remove Block: Personal Devices, from Page: Personal Device, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "B5A94C63-869C-4B4C-B129-9E098EF5537C" );

            RockMigrationHelper.DeleteBlockType( "2D90562E-7332-46DB-9100-0C4106151CA1" ); // Personal Devices
            RockMigrationHelper.DeleteBlockType( "D6224911-2590-427F-9DCE-6D14E79806BA" ); // Personal Device Interactions

            RockMigrationHelper.DeletePage( "5A31D3D3-91A7-409F-8AFF-C3802AC055EC" ); //  Page: Personal Device Interactions, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B2786294-99DC-477E-871D-2E28FCE00A98" ); //  Page: Personal Device, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteAttribute( "DC0E00D2-7694-410E-82C0-E99A097D0A30" ); // IconCssClass

            #endregion

            #region Social Field Types

            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_SNAPCHAT );
            RockMigrationHelper.DeleteAttribute( "FD51EC2E-D660-4B79-95C7-39214D4BEA8E" );

            #endregion

            #region Person Signals

            //
            // Delete the Signal List block from the Person Detail > Security page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_LIST );

            //
            // Delete the Security > Person Signal Types > Person Signal Type Detail page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_TYPE_DETAIL );
            RockMigrationHelper.DeletePage( PAGE_SIGNAL_TYPE_DETAIL );

            //
            // Delete the Security > Person Signal Types page.
            //
            RockMigrationHelper.DeleteBlock( BLOCK_SIGNAL_TYPE_LIST );
            RockMigrationHelper.DeletePage( PAGE_SIGNAL_TYPE_LIST );

            //
            // Delete block types.
            //
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_SIGNAL_LIST );
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_PERSON_SIGNAL_TYPE_DETAIL );
            RockMigrationHelper.DeleteAttribute( "5ECE9E10-0C47-4CC1-8491-E73873E6BD66" );
            RockMigrationHelper.DeleteBlockType( BLOCK_TYPE_PERSON_SIGNAL_TYPE_LIST );

            //
            // Remove the job to calculate signals.
            //
            Sql( "DELETE [ServiceJob] WHERE [Guid] = '82B6315E-53D0-43C6-8F09-C5B0E8890B8D'" );

            //
            // Delete the signal badge to the left badge bar on the person details page.
            // Remove in order of ",guid", "guid,", "guid" to keep formatting correct.
            //
            Sql( string.Format( @"
    UPDATE AV
    	SET AV.[Value] = REPLACE(REPLACE(REPLACE(AV.[Value], ',{0}', ''), '{0},', ''), '{0}', '')
        FROM AttributeValue AS AV
        LEFT JOIN [Block] AS B ON B.Id = AV.EntityId
        LEFT JOIN [Attribute] AS A ON A.Id = AV.[AttributeId]
        WHERE B.[Guid] = '98A30DD7-8665-4C6D-B1BB-A8380E862A04'
          AND A.[Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A'
", BADGE_TOP_PERSON_SIGNAL ) );

            #endregion

            #region Database Changes

            DropForeignKey( "dbo.DataViewPersistedValue", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.PersonSignal", "SignalTypeId", "dbo.SignalType");
            DropForeignKey("dbo.SignalType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.SignalType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonSignal", "PersonId", "dbo.Person");
            DropForeignKey("dbo.PersonSignal", "OwnerPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonSignal", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonSignal", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Person", "PrimaryFamilyId", "dbo.Group");
            DropIndex("dbo.DataViewPersistedValue", new[] { "DataViewId" });
            DropIndex("dbo.SignalType", new[] { "Guid" });
            DropIndex("dbo.SignalType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.SignalType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.SignalType", new[] { "Name" });
            DropIndex("dbo.PersonSignal", new[] { "Guid" });
            DropIndex("dbo.PersonSignal", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonSignal", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonSignal", new[] { "OwnerPersonAliasId" });
            DropIndex("dbo.PersonSignal", new[] { "SignalTypeId" });
            DropIndex("dbo.PersonSignal", new[] { "PersonId" });
            DropIndex("dbo.Person", new[] { "PrimaryFamilyId" });
            DropColumn("dbo.RegistrationTemplate", "RegistrationInstructions");
            DropColumn("dbo.RegistrationInstance", "RegistrationInstructions");
            DropColumn("dbo.DataView", "PersistedLastRefreshDateTime");
            DropColumn("dbo.DataView", "PersistedScheduleIntervalMinutes");
            DropColumn("dbo.GroupType", "AllowSpecificGroupMemberWorkflows");
            DropColumn("dbo.GroupType", "AllowGroupSync");
            DropColumn("dbo.GroupType", "EnableSpecificGroupRequirements");
            DropColumn("dbo.GroupType", "AllowSpecificGroupMemberAttributes");
            DropColumn("dbo.GroupType", "GroupViewLavaTemplate");
            DropColumn("dbo.Person", "PrimaryFamilyId");
            DropColumn("dbo.Person", "AgeClassification");
            DropColumn("dbo.Person", "TopSignalId");
            DropColumn("dbo.Person", "TopSignalIconCssClass");
            DropColumn("dbo.Person", "TopSignalColor");
            DropTable("dbo.DataViewPersistedValue");
            DropTable("dbo.SignalType");
            DropTable("dbo.PersonSignal");

            #endregion
        }
    }
}
