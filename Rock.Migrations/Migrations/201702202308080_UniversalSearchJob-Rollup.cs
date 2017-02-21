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
    public partial class UniversalSearchJobRollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add universal search job
            Sql( @"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.IndexEntities')
BEGIN
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
        ,0
        ,'Universal Search Re-Index'
        ,'Reindexes entities for universal search.'
        ,'Rock.Jobs.IndexEntities'
        ,'0 0 5 1/1 * ? *'
        ,1
        ,'D4B9EFE0-18D4-37AE-4C58-F757E9E2F121');
END" );

            // Respect Campus Context in GroupContextSetter.ascx
            RockMigrationHelper.AddBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205",
               "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Respect Campus Context", "RespectCampusContext", "",
               "Filter groups by the Campus Context block if it exists", 6, @"False",
               "525A2D95-7ACC-4129-9805-DCA4E45A7C63" );

            // core.PersonPickerFetchCount
            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.INTEGER, null, null, "Person Picker Fetch Count", "The maximum number of people to include in a person picker search result", 0, "60", "4515337B-309A-43DE-B4AE-3E19338CE5B2", "core.PersonPickerFetchCount" );

            // Hide Business TransactionLinks when Editing
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 12, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Transaction Links, Attribute:Is Secondary Block Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4", @"True" );

            Sql( @"Update [Block] set PreHtml = '<div class=''col-md-4''>
<div class=""panel panel-block js-hide-nocontent""><div class=""panel-body"">', PostHtml='<script>
    $(document).ready(function(){
        Sys.Application.add_load(function(){
            // find the js-hide-nocontent node from the pre-Html
            var $hideNoContent = $(''.js-hide-nocontent'');
            
            // if it has an HtmlContentBlock on it, but it hasn''t rendered anything, hide the panel panel-block divs
            var $htmlContent = $hideNoContent.find(''.html-content-view'');
            if (!$htmlContent.html()) {
                $hideNoContent.hide();
            } else {
                $hideNoContent.show();
            }
        });
    });
    
</script>' where [Guid] = '84F800D3-C32E-4A16-9F84-081F8CB4DCBF'" );

            // Update to universal search template
            Sql( @"UPDATE [EntityType]
  SET [IndexResultTemplate] = '{% if IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.PersonIndex"" %}

    {% assign url = ""~/Person/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Person-Url and DisplayOptions.Person-Url != null and DisplayOptions.Person-Url != '''' %}
        {% assign url = DisplayOptions.Person-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <div class=""photo-round photo-round-sm"" style=""margin: 0 auto;"" data-original=""{{ IndexDocument.PhotoUrl |  ResolveRockUrl }}&maxwidth=200&maxheight=200&w=100 }}"" style=""background-image: url({{ ""~/Assets/Images/person-no-photo-male.svg"" |  ResolveRockUrl }}); display: block;""></div>
        </div>
        <div class=""col-md-3 col-sm-10"">
            <strong>{{ IndexDocument.NickName}} {{ IndexDocument.LastName}} {{ IndexDocument.Suffix }}</strong> 
            <br>
            {% if IndexDocument.Email != null and IndexDocument.Email != '''' %}
                {{ IndexDocument.Email }} <br>
            {% endif %}
    
            {% if IndexDocument.StreetAddress != '' and IndexDocument.StreetAddress != null %}
                {{ IndexDocument.StreetAddress }}<br>
            {% endif %}
            
            {% if IndexDocument.City != '' and IndexDocument.City != null %}
                {{ IndexDocument.City }}, {{ IndexDocument.State }} {{ IndexDocument.PostalCode }}
            {% endif %}
        </div>
        <div class=""col-md-2"">
            Connection Status: <br> 
            {{ IndexDocument.ConnectionStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Age: <br> 
            {{ IndexDocument.Age }}
        </div>
        <div class=""col-md-2"">
            Record Status: <br> 
            {{ IndexDocument.RecordStatusValueId | FromCache:''DefinedValue'' | Property:''Value'' }}
        </div>
        <div class=""col-md-2"">
            Campus: <br> 
            {{ IndexDocument.CampusId | FromCache:''Campus'' | Property:''Name'' }}
        </div>
    </div>

{% elseif IndexDocument.IndexModelType == ""Rock.UniversalSearch.IndexModels.BusinessIndex"" %}
    {% assign url = ""~/Business/"" | ResolveRockUrl %}
    
    {% if DisplayOptions.Business-Url and DisplayOptions.Business-Url != null and DisplayOptions.Business-Url != '''' %}
        {% assign url = DisplayOptions.Business-Url | ResolveRockUrl %}
    {% endif %}
    
    
    <div class=""row model-cannavigate"" data-href=""{{ url }}{{ IndexDocument.Id }}"">
        <div class=""col-sm-1 text-center"">
            <i class=""{{ IndexDocument.IconCssClass }} fa-2x""></i>
        </div>
        <div class=""col-sm-11"">
            <strong>{{ IndexDocument.Name}}</strong> 

            {% if IndexDocument.Contacts != null and IndexDocument.Contacts != '''' %}
                <br>Contacts: {{ IndexDocument.Contacts }}
            {% endif %}
        </div>
    </div>
{% endif %}'
  WHERE [Name] = 'Rock.Model.Person'" );

            // Add new 'EditFinancials' security
            Sql( @"DECLARE @EntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block' )
DECLARE @PersonEditBlockId INT = ( SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '59C7EA79-2073-4EA9-B439-7E74F06E8F5B' )

IF NOT EXISTS ( 
	SELECT * 
	FROM [Auth] 
	WHERE [EntityTypeId] = @EntityTypeId 
	AND [EntityId] = @PersonEditBlockId
	AND [Action] = 'EditFinancials' )
BEGIN

	DECLARE @Order INT = 0

	DECLARE @FinanceWorkerGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9' )
	IF @FinanceWorkerGroupId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Auth] ( [EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
		VALUES ( @EntityTypeId, @PersonEditBlockId, @Order, 'EditFinancials', 'A', 0, @FinanceWorkerGroupId, NEWID() )
	END

	DECLARE @FinanceAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '6246A7EF-B7A3-4C8C-B1E4-3FF114B84559' )
	IF @FinanceAdminGroupId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Auth] ( [EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
		VALUES ( @EntityTypeId, @PersonEditBlockId, @Order, 'EditFinancials', 'A', 0, @FinanceAdminGroupId, NEWID() )
	END

	DECLARE @RockAdminGroupId INT = ( SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' )
	IF @RockAdminGroupId IS NOT NULL
	BEGIN
		INSERT INTO [dbo].[Auth] ( [EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[GroupId],[Guid])
		VALUES ( @EntityTypeId, @PersonEditBlockId, @Order, 'EditFinancials', 'A', 0, @RockAdminGroupId, NEWID() )
	END

	INSERT INTO [dbo].[Auth] ( [EntityTypeId],[EntityId],[Order],[Action],[AllowOrDeny],[SpecialRole],[Guid])
	VALUES ( @EntityTypeId, @PersonEditBlockId, @Order, 'EditFinancials', '@', 1, NEWID() )

END" );

            // Add Active Registration Instances
            RockMigrationHelper.UpdateBlockType( "Registration Instance Active List", "Block to display active Registration Instances.", "~/Blocks/Event/RegistrationInstanceActiveList.ascx", "Event", "CFE8CAFA-587B-4EF2-A457-18047AC6BA39" );
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "CFE8CAFA-587B-4EF2-A457-18047AC6BA39", "Registration Instance Active List", "Main", "", "", 3, "682AC7FB-84ED-4F6F-866C-60C3A2E92AAE" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // universal search job
            Sql( @"DELETE FROM [ServiceJob] WHERE [Guid] = 'D4B9EFE0-18D4-37AE-4C58-F757E9E2F121'" );
            
            // core.PersonPickerFetchCount
            RockMigrationHelper.DeleteAttribute( "4515337B-309A-43DE-B4AE-3E19338CE5B2" );

            // Hide Business TransactionLinks when Editing
            RockMigrationHelper.DeleteAttribute( "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );

            // Add Active Registration Instances
            RockMigrationHelper.DeleteBlock( "682AC7FB-84ED-4F6F-866C-60C3A2E92AAE" );
            RockMigrationHelper.DeleteBlockType( "CFE8CAFA-587B-4EF2-A457-18047AC6BA39" );
        }
    }
}
