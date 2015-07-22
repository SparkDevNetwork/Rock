// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class MyDashboard : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Following By Entity:Link URL
            RockMigrationHelper.AddBlockTypeAttribute( "36B56055-7AA2-4169-82DD-CCFBD2C7B4CC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Link URL", "LinkUrl", "", "The address to use for the link. The text '[Id]' will be replaced with the Id of the entity '[Guid]' will be replaced with the Guid.", 1, @"/samplepage/[Id]", "9981ABB3-7130-41DB-87AF-973722FBD54E" );

            // Attrib for BlockType: Following By Entity:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "36B56055-7AA2-4169-82DD-CCFBD2C7B4CC", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show merge data to help you see what's available to you.", 3, @"False", "D7094B16-B237-4D97-B1FC-F680C0701583" );

			
			// Add new block types
			RockMigrationHelper.UpdateBlockType("Following By Entity","Takes a entity type and displays a person's following items for that entity using a Lava template.","~/Blocks/Core/FollowingByEntityLava.ascx","Core","36B56055-7AA2-4169-82DD-CCFBD2C7B4CC");
            RockMigrationHelper.UpdateBlockType("Event Calendar Item Personalized Registration","Simplifies the registration process for a given person and event calendar item.","~/Blocks/Event/EventCalendarItemPersonalizedRegistration.ascx","Event","1A1FFACC-D74C-4061-B6A7-34150C462DB7");
  
			// Add My Dashboard Page
            RockMigrationHelper.AddPage("20F97A93-7949-4C2A-8A5E-C756FE8585CA","22D220B5-0D34-429A-B9E3-59D80AE423E7","My Dashboard","","AE1818D8-581C-4599-97B9-509EA450376A","fa fa-tachometer"); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute("AE1818D8-581C-4599-97B9-509EA450376A","MyDashboard");// for Page:My Dashboard
            
            // hide page from nav
            Sql( @"  UPDATE [Page]
  SET [DisplayInNavWhen] = 2
  WHERE [Guid] = 'AE1818D8-581C-4599-97B9-509EA450376A'" );

			// Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock("AE1818D8-581C-4599-97B9-509EA450376A","","36B56055-7AA2-4169-82DD-CCFBD2C7B4CC","Following Groups","Sidebar1","","",0,"AA11F703-FF26-4DA3-8CAE-E95989013135"); 

            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock("AE1818D8-581C-4599-97B9-509EA450376A","","4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","My  Assigned Workflows","Main","","",1,"415575C3-70AC-4A7A-8936-B98464C5557F"); 

            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock("AE1818D8-581C-4599-97B9-509EA450376A","","4F217A7F-A34E-489E-AE0E-2B7EDCF69CD1","My Open Workflows","Sidebar1","","",1,"8111124F-8201-4F54-8A2C-CDC9D7CEA1BC"); 

            // Add Block to Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlock("AE1818D8-581C-4599-97B9-509EA450376A","","19B61D65-37E3-459F-A44F-DEF0089118A3","Intro","Feature","","",0,"6DE44644-65FE-4321-A09D-36B329D6AE04"); 

            // Add/Update HtmlContent for Block: Intro
            RockMigrationHelper.UpdateHtmlContentBlock("6DE44644-65FE-4321-A09D-36B329D6AE04",@"<h1>{{ CurrentPerson.NickName | Possessive}} Homepage</h1>","F0BCB32C-CEB0-41CC-B43A-26FC66CCBD36");


            // disable cache on html block
            RockMigrationHelper.AddBlockAttributeValue( "6DE44644-65FE-4321-A09D-36B329D6AE04", "4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4", "0" );

            
            // Attrib for BlockType: Group Member List:Require Note on Alternate Placement
            RockMigrationHelper.AddBlockTypeAttribute("88B7EFA9-7419-4D05-9F88-38B936E61EDD","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Require Note on Alternate Placement","RequireAlternatePlacementNote","","Flag that indicates whether a note is required to save the alternate placement.",3,@"False","94174D1A-1C58-4456-A931-A07A8FB19154");

            // Attrib for BlockType: Following By Entity:Max Results
            RockMigrationHelper.AddBlockTypeAttribute("36B56055-7AA2-4169-82DD-CCFBD2C7B4CC","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Max Results","MaxResults","","The maximum number of results to display.",4,@"100","D2BB9E7D-9DCC-4432-88F5-D5A3058F6DEB");

            // Attrib for BlockType: Following By Entity:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute("36B56055-7AA2-4169-82DD-CCFBD2C7B4CC","3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB","Entity Type","EntityType","","The type of entity to show following for.",0,@"","C896AA82-F8D7-4A94-9730-D4030DA3DBFA");

            // Attrib for BlockType: Login Status:Logged In Page List
            RockMigrationHelper.AddBlockTypeAttribute("04712F3D-9667-4901-A49D-4507573EF7AD","73B02051-0D38-4AD9-BF81-A2D477DE4F70","Logged In Page List","LoggedInPageList","","List of pages to show in the dropdown when the user is logged in. The link field takes Lava with the CurrentPerson merge fields. Place the text 'divider' in the title field to add a divider.",0,@"","1B0E8904-196B-433E-B1CC-937AD3CA5BF2");

            // Attrib for BlockType: Following By Entity:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute("36B56055-7AA2-4169-82DD-CCFBD2C7B4CC","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","","Lava template to use to display content",2,@"<div class=""panel panel-block""> 
    <div class=""panel-heading"">
       <h4 class=""panel-title"">Followed {{ EntityType | Pluralize }}</h4>
    </div>
    <div class=""panel-body"">

        <ul>
        {% for item in FollowingItems %}
            {% if LinkUrl != '' %}
                <li><a href=""{{ LinkUrl | Replace:'[Id]',item.Id }}"">{{ item.Name }}</a></li>
            {% else %}
                <li>{{ item.Name }}</li>
            {% endif %}
        {% endfor %}
        </ul>
        
    </div>
</div>","58CA8CF4-6C86-4D27-92BE-C687E74D014F");

            
			
			// Add My Dashboard to the internal login statuses
			
			// Attrib Value for Block:Login Status, Attribute:Logged In Page List , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("19C2140D-498A-4675-B8A2-18B281736F6E","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"My Dashboard^~/MyDashboard|");

            // Attrib Value for Block:Login Status, Attribute:Logged In Page List , Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("82AF461F-022D-4ADB-BB12-F220CD605459","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"My Dashboard^~/MyDashboard|");

            // Attrib Value for Block:Login Status, Attribute:Logged In Page List , Layout: Left Sidebar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("791A6AA0-D498-4795-BB5F-21609175826F","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"My Dashboard^~/MyDashboard|");

            // Attrib Value for Block:Login Status, Attribute:Logged In Page List , Layout: Right Sidebar, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("2356DEDC-803F-4782-A8E9-D0D88393EC2E","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"My Dashbaord^~/MyDashboard|");

            // Attrib Value for Block:Login Status, Attribute:Logged In Page List , Layout: Homepage, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("5A5C6063-EA0D-4EDD-A394-4B1B772F2041","1B0E8904-196B-433E-B1CC-937AD3CA5BF2",@"");

            // Attrib Value for Block:Following Groups, Attribute:Max Results Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AA11F703-FF26-4DA3-8CAE-E95989013135","D2BB9E7D-9DCC-4432-88F5-D5A3058F6DEB",@"100");

            // Attrib Value for Block:Following Groups, Attribute:Entity Type Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AA11F703-FF26-4DA3-8CAE-E95989013135","C896AA82-F8D7-4A94-9730-D4030DA3DBFA",@"9bbfda11-0d22-40d5-902f-60adfbc88987");

            // Attrib Value for Block:Following Groups, Attribute:Link URL Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AA11F703-FF26-4DA3-8CAE-E95989013135","9981ABB3-7130-41DB-87AF-973722FBD54E",@"/page/113?GroupId=[Id]");

            // Attrib Value for Block:Following Groups, Attribute:Lava Template Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AA11F703-FF26-4DA3-8CAE-E95989013135","58CA8CF4-6C86-4D27-92BE-C687E74D014F",@"<div class=""panel panel-block""> 
    <div class=""panel-heading"">
       <h4 class=""panel-title""><i class=""fa fa-users""></i> Followed {{ EntityType | Pluralize }}</h4>
    </div>
    <div class=""panel-body"">

        <ul class=""list-unstyled"">

        {% assign itemsSorted = FollowingItems | Sort:'GroupTypeId' %}
        {% assign currentType = '' %}

        {% for item in itemsSorted %}
            {% if currentType != item.GroupTypeId %}
                {% if currentType != '' %}
                    </ul>
                {% endif %}
                
                <strong>{{ item.GroupType.Name }}</strong>
                <ul class=""list-unstyled margin-b-md"">
                {% assign currentType = item.GroupTypeId %}
            {% endif %}
            
            <li><i class=""{{ item.GroupType.IconCssClass }} icon-fw""></i> <a href=""{{ LinkUrl | Replace:'[Id]',item.Id }}"">{{ item.Name }}</a></li>
        {% endfor %}
        
        {% if currentType != '' %}
            </ul>
        {% endif %}
        
    </div>
</div>");

            // Attrib Value for Block:Following Groups, Attribute:Enable Debug Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("AA11F703-FF26-4DA3-8CAE-E95989013135","D7094B16-B237-4D97-B1FC-F680C0701583",@"False");

            // Attrib Value for Block:My  Assigned Workflows, Attribute:Contents Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("415575C3-70AC-4A7A-8936-B98464C5557F","D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2",@"{% if Actions.size > 0 %}
    <div class='panel panel-block'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My Assigned Tasks</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
");

            // Attrib Value for Block:My  Assigned Workflows, Attribute:Include Child Categories Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("415575C3-70AC-4A7A-8936-B98464C5557F","5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC",@"True");

            // Attrib Value for Block:My  Assigned Workflows, Attribute:Categories Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("415575C3-70AC-4A7A-8936-B98464C5557F","28DF6F17-AF47-49DF-824F-9E7C8B94AD5D",@"");

            // Attrib Value for Block:My  Assigned Workflows, Attribute:Role Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("415575C3-70AC-4A7A-8936-B98464C5557F","C8816900-0772-4E15-8D41-D20874F560BE",@"0");

            // Attrib Value for Block:My Open Workflows, Attribute:Role Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8111124F-8201-4F54-8A2C-CDC9D7CEA1BC","C8816900-0772-4E15-8D41-D20874F560BE",@"1");

            // Attrib Value for Block:My Open Workflows, Attribute:Categories Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8111124F-8201-4F54-8A2C-CDC9D7CEA1BC","28DF6F17-AF47-49DF-824F-9E7C8B94AD5D",@"");

            // Attrib Value for Block:My Open Workflows, Attribute:Include Child Categories Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8111124F-8201-4F54-8A2C-CDC9D7CEA1BC","5AD3495C-AFBF-4262-BD3D-AC16FD8CF3EC",@"True");

            // Attrib Value for Block:My Open Workflows, Attribute:Contents Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8111124F-8201-4F54-8A2C-CDC9D7CEA1BC","D2149BA3-7AE8-4FE8-AF7C-4EF40DBEB4B2",@"{% if Actions.size > 0 %}
    <div class='panel panel-block'> 
        <div class='panel-heading'>
            <h4 class='panel-title'>My Open Requests</h4>
        </div>
        <div class='panel-body'>
            <ul class='fa-ul'>
                {% for action in Actions %}
                    <li>
                        <i class='fa-li {{ action.Activity.Workflow.WorkflowType.IconCssClass }}'></i>
                        <a href='~/{% if Role == '0' %}WorkflowEntry/{{ action.Activity.Workflow.WorkflowTypeId }}{% else %}Workflow{% endif %}/{{ action.Activity.Workflow.Id }}'>{{ action.Activity.Workflow.WorkflowType.Name }}: {{ action.Activity.Workflow.Name }}{% if role == '0' %} ({{ action.Activity.ActivityType.Name }}){% endif %}</a>
                    </li>
                {% endfor %}
            </ul>
        </div>
    </div>
{% endif %}
");

            RockMigrationHelper.AddDefinedType( "Check-in", "Family Manager Configuration", "Configuration templates for the Family Manager application.", "251D752B-0595-C3A6-4E2A-AD0264DAFCCD", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "251D752B-0595-C3A6-4E2A-AD0264DAFCCD", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Configuration Template", "ConfigurationTemplate", "JSON configuration file.", 0, "", "ACC82748-157F-BF8E-4E34-FFD3C05269B3" );
            RockMigrationHelper.AddAttributeQualifier( "ACC82748-157F-BF8E-4E34-FFD3C05269B3", "editorHeight", "600", "8B8E9AC2-6F82-85B5-4EBF-88F2ACFA0634" );
            RockMigrationHelper.AddAttributeQualifier( "ACC82748-157F-BF8E-4E34-FFD3C05269B3", "editorMode", "4", "A5D7AA2D-533C-3DB5-4A46-0879A47F9B5E" );
            RockMigrationHelper.AddAttributeQualifier( "ACC82748-157F-BF8E-4E34-FFD3C05269B3", "editorTheme", "0", "9BD724FA-E098-0F91-4EF4-E6117E808242" );
            RockMigrationHelper.AddDefinedValue( "251D752B-0595-C3A6-4E2A-AD0264DAFCCD", "Rock Default", "The default theme for the Family Manager application.", "8BB6A099-1592-4E7F-AE8E-7FDEE63E040C", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BB6A099-1592-4E7F-AE8E-7FDEE63E040C", "ACC82748-157F-BF8E-4E34-FFD3C05269B3", @"{
	""familyAttributes"": [
	],
	""personAttributes"": [
		{""attributeId"": 2073, ""required"": true },
		{""attributeId"": 174, ""required"": false },
		{""attributeId"": 714, ""required"": true },
		{""attributeId"": 1117, ""required"": false },
		{""attributeId"": 1032, ""required"": true },
		{""attributeId"": 1059, ""required"": false }
	],
	
	""requireFamilyAddress"": false,
	""requireChildBirthdate"": true,
	""requireAdultBirthdate"": false,
	""requireChildGrade"": true,
	""firstVisitPrompt"": false,
	
	""visualSettings"": 
	{
	    ""BackgroundColor"" : ""#dcd5cbff"",
		""LogoURL"" : ""~/Assets/FamilyManagerThemes/RockDefault/rock-logo.png"",
		""AdultMaleNoPhoto"" : ""~/Assets/FamilymanagerThemes/RockDefault/photo-adult-male.png"",
		""AdultFemaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-adult-female.png"",
		""ChildMaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-child-male.png"",
		""ChildFemaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-child-female.png"",
		
		""TopHeaderBGColor"" : ""#282526FF"",
		""TopHeaderTextColor"" : ""#d3cec5FF"",
		
		""FooterBGColor"" : ""#c2b8a7FF"",
		""FooterTextColor"" : ""#847f77FF"",
	
		""SelectedPersonColor"" : ""#ee7624FF"",
		
		""LargeFontSize"" : ""18"",
		""MediumFontSize"" : ""20"",
		""SmallFontSize"" : ""14"",
				
		""DefaultButtonStyle"" :
		{
			""BackgroundColor"" : ""#d6d6d6FF"",
			""TextColor"" : ""#524c43FF"",
			""BorderColor"" : ""#c2b8a7FF"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4""
		 },
		 ""PrimaryButtonStyle"" : 
		 {
		 	""BackgroundColor"" : ""#ee7624FF"",
			""TextColor"" : ""#FFFFFFFF"",
			""BorderColor"" : ""#ae4f0dff"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4""
		 },
		 ""FamilyCellStyle"":
		 {
		 	""BackgroundColor"" : ""#00000000"",
			""AddFamilyButtonBGColor"" : ""#c2b8a7FF"",
			""AddFamilyButtonTextColor"" : ""#554e51FF"",
			""EntryBGColor"" : ""#c2b8a7FF"",
			""EntryTextColor"" : ""#554e51FF"",
		 },
		 ""ToggleStyle"" :
		 {
		 	""ActiveColor"" : ""#ee7624FF"",
			 ""InActiveColor"" : ""#777777FF"",
			 ""TextColor"" : ""#FFFFFFFF"",
			 ""CornerRadius"" : ""4""
		 },
		 ""DatePickerStyle"" :
		 {
			""BackgroundColor"" : ""#c2b8a7FF"",
			""TextColor"" : ""#3b3b3bFF"",
			""Size"" : ""22"",
			""Font"" : ""OpenSans-Regular""
		 },
		 ""LabelStyle"" :
		 {
		 	""TextColor"" : ""#6a6a6aFF"",
		 },
		 ""SwitchStyle"" :
		 {
		 	""OnColor"" : ""#FFFFFFFF""
		 },
		 ""TextFieldStyle"" :
		 {
		 	""TextColor"" : ""#555555FF"",
			""PlaceHolderColor"" : ""#AAAAAAFF"",
			""BorderColor"" : ""#ccccccFF"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4"",
		 },
		 ""SearchResultStyle"":
		 {
		 	""BackgroundColor"" : ""#c2b8a7FF"",
			""TextColor"" : ""#3b3b3bFF""
		 }
	}
}" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // delete family manager
            RockMigrationHelper.DeleteAttribute( "ACC82748-157F-BF8E-4E34-FFD3C05269B3" );
            RockMigrationHelper.DeleteDefinedValue( "8BB6A099-1592-4E7F-AE8E-7FDEE63E040C" );
            RockMigrationHelper.DeleteDefinedType( "251D752B-0595-C3A6-4E2A-AD0264DAFCCD" );

            // Attrib for BlockType: Login Status:Logged In Page List
            RockMigrationHelper.DeleteAttribute( "1B0E8904-196B-433E-B1CC-937AD3CA5BF2" );
            // Attrib for BlockType: Group Member List:Require Note on Alternate Placement
            RockMigrationHelper.DeleteAttribute( "94174D1A-1C58-4456-A931-A07A8FB19154" );
            // Attrib for BlockType: Following By Entity:Enable Debug
            RockMigrationHelper.DeleteAttribute( "D7094B16-B237-4D97-B1FC-F680C0701583" );
            // Attrib for BlockType: Following By Entity:Lava Template
            RockMigrationHelper.DeleteAttribute( "58CA8CF4-6C86-4D27-92BE-C687E74D014F" );
            // Attrib for BlockType: Following By Entity:Link URL
            RockMigrationHelper.DeleteAttribute( "9981ABB3-7130-41DB-87AF-973722FBD54E" );
            // Attrib for BlockType: Following By Entity:Entity Type
            RockMigrationHelper.DeleteAttribute( "C896AA82-F8D7-4A94-9730-D4030DA3DBFA" );
            // Attrib for BlockType: Following By Entity:Max Results
            RockMigrationHelper.DeleteAttribute( "D2BB9E7D-9DCC-4432-88F5-D5A3058F6DEB" );
            // Remove Block: Intro, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "6DE44644-65FE-4321-A09D-36B329D6AE04" );
            // Remove Block: My Open Workflows, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8111124F-8201-4F54-8A2C-CDC9D7CEA1BC" );
            // Remove Block: My  Assigned Workflows, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "415575C3-70AC-4A7A-8936-B98464C5557F" );
            // Remove Block: Following Groups, from Page: My Dashboard, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AA11F703-FF26-4DA3-8CAE-E95989013135" );
            RockMigrationHelper.DeleteBlockType( "1A1FFACC-D74C-4061-B6A7-34150C462DB7" ); // Event Calendar Item Personalized Registration
            RockMigrationHelper.DeleteBlockType( "36B56055-7AA2-4169-82DD-CCFBD2C7B4CC" ); // Following By Entity
            RockMigrationHelper.DeletePage( "AE1818D8-581C-4599-97B9-509EA450376A" ); //  Page: My Dashboard, Layout: Right Sidebar, Site: Rock RMS
        }
    }
}
