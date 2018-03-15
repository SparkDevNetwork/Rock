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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class UniversalSearchPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // update the index rebuilder
            Sql( MigrationSQL._201707171725146_UniversalSearchPages_spDbaRebuildIndexes );

            // add pages for api docs
            RockMigrationHelper.AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "API Docs", "", "C132F1D5-9F43-4AEB-9172-CD45138B4CEA", "fa fa-diamond" ); // Site:Rock RMS
            // Add Block to Page: API Docs, Site: Rock RMS              
            RockMigrationHelper.AddBlock("C132F1D5-9F43-4AEB-9172-CD45138B4CEA","","B97FB779-5D3E-4663-B3B5-3C2C227AE14A","Redirect","Main",@"",@"",0,"ECB741BB-E4E8-4029-B83E-2FCC2D95ABE4");
            // Attrib for BlockType: Redirect:Redirect When              
            RockMigrationHelper.UpdateBlockTypeAttribute("B97FB779-5D3E-4663-B3B5-3C2C227AE14A","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Redirect When","RedirectWhen","","When the redirect will occur.",1,@"1","F09F2F0C-9FB0-4BC2-818C-FAD25900CF26");
            // Attrib for BlockType: Redirect:Network              
            RockMigrationHelper.UpdateBlockTypeAttribute("B97FB779-5D3E-4663-B3B5-3C2C227AE14A","9C204CD0-1233-41C5-818A-C5DA439445AA","Network","Network","","The network to compare to in the format of '192.168.0.0/24'. See http://www.ipaddressguide.com/cidr for assistance in calculating CIDR addresses.",2,@"","1A584473-9E6B-4DB1-A4E2-78B62CEBBDC4");
            // Attrib Value for Block:Redirect, Attribute:Redirect When Page: API Docs, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("ECB741BB-E4E8-4029-B83E-2FCC2D95ABE4","F09F2F0C-9FB0-4BC2-818C-FAD25900CF26",@"1");
            // Attrib Value for Block:Redirect, Attribute:Network Page: API Docs, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("ECB741BB-E4E8-4029-B83E-2FCC2D95ABE4","1A584473-9E6B-4DB1-A4E2-78B62CEBBDC4",@"");
            // Attrib Value for Block:Redirect, Attribute:Url Page: API Docs, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("ECB741BB-E4E8-4029-B83E-2FCC2D95ABE4","964D33F4-27D0-4715-86BE-D30CEB895044",@"/api/docs");

            RockMigrationHelper.AddSecurityAuthForBlock( "ECB741BB-E4E8-4029-B83E-2FCC2D95ABE4", 0, Authorization.ADMINISTRATE, false, null, Model.SpecialRole.AllUsers, "3FC5ECB8-6066-8CB8-42D0-15D9DA8D4427" );

            // set attribute on universal search search component
            RockMigrationHelper.UpdateEntityType( "Rock.Search.Other.Universal", "Universal", "Rock.Search.Other.Universal, Rock, Version=1.7.0.33, Culture=neutral, PublicKeyToken=null", false, true, "BD0FAAC1-2313-4D36-8B78-268715320F02" );

            Sql( @"DECLARE @AttributeId int = 0
DECLARE @EntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Search.Other.Universal')
DECLARE @TextFieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')
IF EXISTS (SELECT TOP 1 * FROM [Attribute] a
		INNER JOIN [EntityType] e ON e.[Id] = a.[EntityTypeId]
		WHERE e.[Name] = 'Rock.Search.Other.Universal' AND a.[Key] = 'ResultURL')
	BEGIN
		SELECT @AttributeId = (SELECT TOP 1 a.[Id] FROM [Attribute] a
		INNER JOIN [EntityType] e ON e.[Id] = a.[EntityTypeId]
		WHERE e.[Name] = 'Rock.Search.Other.Universal' AND a.[Key] = 'ResultURL')
	END
ELSE
	BEGIN
		INSERT INTO [Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [Key], [Name], [Description], [Order], [IsGridColumn],[IsMultiValue], [IsRequired], [Guid], [AllowSearch], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory])
		VALUES (0,@TextFieldTypeId, @EntityTypeId, 'ResultURL', 'Result URL', 'The url to redirect user to after they have entered search text.  (use ''{0}'' for the search text)', 1,0,0,1,'537CE5A5-7B05-569E-48BE-EA38A12EBA7A', 0,0,0,0)
	
		SET @AttributeId = SCOPE_IDENTITY()
	END

IF EXISTS (SELECT * FROM [AttributeValue] WHERE [AttributeId] = @AttributeId)
	BEGIN
		UPDATE [AttributeValue]
			SET [Value] = 'universalsearch/{0}?SmartSearch=true'
		WHERE [AttributeId] = @AttributeId
	END
ELSE
	BEGIN
		INSERT INTO [AttributeValue] ([IsSystem], [EntityId], [AttributeId], [Value], [Guid])
		VALUES
		(0, 0, @AttributeId, 'universalsearch/{0}?SmartSearch=true', 'A48B694E-28CF-268B-4EAB-907114EC9F70')
	END" );

            // pages for universal search
            RockMigrationHelper.AddPage( "936C90C4-29CF-4665-A489-7C687217F7B8", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Universal Search", "", "B07F30B3-95C4-40A5-9CF6-455399BEF67A", "fa fa-search" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "B07F30B3-95C4-40A5-9CF6-455399BEF67A", "universalsearch/{Q}", "69B00D34-9E70-4451-8EB8-955B32926380" );// for Page:Universal Search
            RockMigrationHelper.AddPageRoute( "B07F30B3-95C4-40A5-9CF6-455399BEF67A", "universalsearch/{DocumentType}/{DocumentId}", "2AB19664-0B4A-4846-A4EE-316BC0D2DD10" );// for Page:Universal Search
            RockMigrationHelper.AddPageRoute( "B07F30B3-95C4-40A5-9CF6-455399BEF67A", "universalsearch", "43CAE944-6358-423A-8A12-5F2AE26DAAC4" );// for Page:Universal Search

            RockMigrationHelper.UpdateBlockType( "Entity Attribute Values", "View and edit attribute values for an entity.", "~/Blocks/Utility/EntityAttributeValues.ascx", "Utility", "4A89FF55-A6A3-4A9B-8D1D-2ADE092565F5" );
            // Add Block to Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlock("B07F30B3-95C4-40A5-9CF6-455399BEF67A","","FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","Universal Search","Main",@"",@"",0,"309A2477-9A5B-4FD4-A722-735F87861A05");
            // Attrib for BlockType: Universal Search:Use Custom Results              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Use Custom Results","UseCustomResults","","Determines if the custom results should be displayed.",0,@"False","45F64F50-A41C-4247-B0FF-655520B6AC97");
            // Attrib for BlockType: Universal Search:Search Type              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Search Type","SearchType","","The type of search to perform.",0,@"0","152FA041-3DA7-4BA4-A2D5-87BFA1618536");
            // Attrib for BlockType: Universal Search:Search Input Post-HTML              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Search Input Post-HTML","PostHtml","","Custom Lava to place after the search input (for styling).",0,@"","8F8CDF58-C491-4DB7-AE41-2AC91AE77270");
            // Attrib for BlockType: Universal Search:Show Filters              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Filters","ShowFilters","","Toggles the display of the model filter which allows the user to select which models to search on.",0,@"True","8BCE3DFD-B91D-4962-8F5B-B0E56C5CA34B");
            // Attrib for BlockType: Universal Search:Enabled Models              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","9C204CD0-1233-41C5-818A-C5DA439445AA","Enabled Models","EnabledModels","","The models that should be enabled for searching.",0,@"","4B5C7A21-E5C8-4954-93A4-063CE1A9EB9C");
            // Attrib for BlockType: Universal Search:Results Per Page              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Results Per Page","ResultsPerPage","","The number of results to show per page.",0,@"20","026E7BDE-ED30-42A8-824A-420B649EEFA2");
            // Attrib for BlockType: Universal Search:Base Field Filters             
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","9C204CD0-1233-41C5-818A-C5DA439445AA","Base Field Filters","BaseFieldFilters","","These field filters will always be enabled and will not be changeable by the individual. Uses tha same syntax as the lava command.",0,@"","C3DFA7F1-29F0-4D56-91F4-2C41906AB972");
            // Attrib for BlockType: Universal Search:Show Refined Search              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Refined Search","ShowRefinedSearch","","Determines whether the refinded search should be shown.",0,@"True","565A7DF1-FE40-432B-B912-25E8CDDAC318");
            // Attrib for BlockType: Universal Search:Show Scores              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Scores","ShowScores","","Enables the display of scores for help with debugging.",0,@"False","87CB8916-A2D4-443D-8EF4-DA5ABDB367D0");
            // Attrib for BlockType: Universal Search:Lava Result Template              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Result Template","LavaResultTemplate","","Custom Lava results template to use instead of the standard results.",0,@"<ul>{% for result in Results %}      <li><i class='fa {{ result.IconCssClass }}'></i> {{ result.DocumentName }} <small>(Score {{ result.Score }} )</small> </li>  {% endfor %}</ul>","0FC1B4E2-A470-49B4-A08A-67D477D0B3B6");
            // Attrib for BlockType: Universal Search:Custom Results Commands              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D","Custom Results Commands","CustomResultsCommands","","The custom Lava fields to allow.",0,@"","09A3947F-E3C5-417F-8299-C75BC580D0FC");
            // Attrib for BlockType: Universal Search:Search Input Pre-HTML              
            RockMigrationHelper.UpdateBlockTypeAttribute("FDF1BBFF-7A7B-4F4E-BF34-831203B0FEAC","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Search Input Pre-HTML","PreHtml","","Custom Lava to place before the search input (for styling).",0,@"","84E0262A-55F1-46EA-A3E4-EFE40A8504DE");
            // Attrib Value for Block:Universal Search, Attribute:Use Custom Results Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","45F64F50-A41C-4247-B0FF-655520B6AC97",@"False");
            // Attrib Value for Block:Universal Search, Attribute:Search Type Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","152FA041-3DA7-4BA4-A2D5-87BFA1618536",@"2");
            // Attrib Value for Block:Universal Search, Attribute:Search Input Post-HTML Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","8F8CDF58-C491-4DB7-AE41-2AC91AE77270",@"");
            // Attrib Value for Block:Universal Search, Attribute:Show Filters Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","8BCE3DFD-B91D-4962-8F5B-B0E56C5CA34B",@"True");
            // Attrib Value for Block:Universal Search, Attribute:Enabled Models Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","4B5C7A21-E5C8-4954-93A4-063CE1A9EB9C",@"208,16,15");
            // Attrib Value for Block:Universal Search, Attribute:Results Per Page Page: Universal Search, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","026E7BDE-ED30-42A8-824A-420B649EEFA2",@"20");
            // Attrib Value for Block:Universal Search, Attribute:Base Field Filters Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","C3DFA7F1-29F0-4D56-91F4-2C41906AB972",@"");
            // Attrib Value for Block:Universal Search, Attribute:Show Refined Search Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","565A7DF1-FE40-432B-B912-25E8CDDAC318",@"True");
            // Attrib Value for Block:Universal Search, Attribute:Show Scores Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","87CB8916-A2D4-443D-8EF4-DA5ABDB367D0",@"False");
            // Attrib Value for Block:Universal Search, Attribute:Lava Result Template Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","0FC1B4E2-A470-49B4-A08A-67D477D0B3B6",@"<ul>{% for result in Results %}     <li><i class='fa {{ result.IconCssClass }}'></i> {{ result.DocumentName }} <small>(Score {{ result.Score }} )</small> </li> {% endfor %}</ul>");
            // Attrib Value for Block:Universal Search, Attribute:Custom Results Commands Page: Universal Search, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","09A3947F-E3C5-417F-8299-C75BC580D0FC",@"");
            // Attrib Value for Block:Universal Search, Attribute:Search Input Pre-HTML Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("309A2477-9A5B-4FD4-A722-735F87861A05","84E0262A-55F1-46EA-A3E4-EFE40A8504DE",@"");


            // add lava webhook
            RockMigrationHelper.AddDefinedType( "Global", "Lava Webhook", "Webhook to process pre-defined Lava in response to incoming requests. ", SystemGuid.DefinedType.WEBHOOK_TO_LAVA, @"The following merge fields are available for the ''Template'' attribute.

<p>
    <a data-toggle=""collapse"" href=""#collapsefields"" class=""btn btn-action btn-xs"">show/hide fields</a>
</p>

<div id=""collapsefields"" class=""panel-collapse collapse"">
<pre>
{
  ""Url"": ""/test"",
  ""RawUrl"": ""http://localhost:6229/Webhooks/LavaApi.ashx/test?queryParameter=true"",
  ""Method"": ""POST"",
  ""QueryString"": {
    ""queryParameter"": ""true""
  },
  ""RemoteAddress"": ""127.0.0.1"",
  ""RemoteName"": ""127.0.0.1"",
  ""ServerName"": ""localhost"",
  ""RawBody"": ""{""key1"":""value1"", ""key2"":""value2""}"",
  ""Body"": {
    ""key1"": ""value1"",
    ""key2"": ""value2""
  },
  ""Headers"": {
    ""Content-Length"": ""34"",
    ""Content-Type"": ""application/json"",
    ""Accept"": ""*/*"",
    ""Host"": ""localhost:6229"",
    ""User-Agent"": ""curl/7.35.0""
  },
  ""Cookies"": {
    ""sessionToken"": ""abc123""
  }
}
</pre>
</div>" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.SINGLE_SELECT, "Method", "Method", "The HTTP Verb to match against. Leave blank to match all verbs.", 1, "", "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0" );
            RockMigrationHelper.AddAttributeQualifier( "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0", "fieldtype", "ddl", "6939b529-7fcf-49f9-9c37-3a8a1e7af7a3" );
            RockMigrationHelper.AddAttributeQualifier( "d9c92cdb-70ab-4d99-b580-eb55ed9fbee0", "values", "GET,POST,PUT,PATCH,DELETE", "461cfb14-4a2a-4662-98df-89ea7478627a" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.CODE_EDITOR, "Template", "Template", "The lava script to use when processing this request. <span class='tip tip-lava'></span>", 2, "", "4303ae08-2208-4f46-9b98-fd91a710ce1e" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorHeight", "300", "c65b2e5a-27fd-4096-ab6c-cc58289d1258" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorMode", Rock.Web.UI.Controls.CodeEditorMode.Lava.ConvertToInt().ToString(), "4d987d2e-4584-436f-9e5c-ed18d87af827" );
            RockMigrationHelper.AddAttributeQualifier( "4303ae08-2208-4f46-9b98-fd91a710ce1e", "editorTheme", Rock.Web.UI.Controls.CodeEditorTheme.Rock.ConvertToInt().ToString(), "f46b6509-2ef7-4283-b4fd-dd899de65832" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.LAVA_COMMANDS, "Enabled Lava Commands", "EnabledLavaCommands", "Which lava commands are available to be used by the lava template", 3, "", "2df9f53d-926e-4d2a-b755-818edd933781" );

            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.WEBHOOK_TO_LAVA, SystemGuid.FieldType.TEXT, "Response Content Type", "ResponseContentType", "The Content-Type header to include in the response. If no value is provided then 'text/plain' is used. Common values are 'application/json' and 'application/xml'.", 4, "", "73774e76-028f-445c-a078-bfb8885a102c" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "537CE5A5-7B05-569E-48BE-EA38A12EBA7A" );
            
            // Attrib for BlockType: Universal Search:Search Input Pre-HTML              
            RockMigrationHelper.DeleteAttribute("84E0262A-55F1-46EA-A3E4-EFE40A8504DE");
            // Attrib for BlockType: Universal Search:Custom Results Commands              
            RockMigrationHelper.DeleteAttribute("09A3947F-E3C5-417F-8299-C75BC580D0FC");
            // Attrib for BlockType: Universal Search:Lava Result Template              
            RockMigrationHelper.DeleteAttribute("0FC1B4E2-A470-49B4-A08A-67D477D0B3B6");
            // Attrib for BlockType: Universal Search:Show Scores              
            RockMigrationHelper.DeleteAttribute("87CB8916-A2D4-443D-8EF4-DA5ABDB367D0");
            // Attrib for BlockType: Universal Search:Show Refined Search              
            RockMigrationHelper.DeleteAttribute("565A7DF1-FE40-432B-B912-25E8CDDAC318");
            // Attrib for BlockType: Universal Search:Base Field Filters              
            RockMigrationHelper.DeleteAttribute("C3DFA7F1-29F0-4D56-91F4-2C41906AB972");
            // Attrib for BlockType: Universal Search:Results Per Page              
            RockMigrationHelper.DeleteAttribute("026E7BDE-ED30-42A8-824A-420B649EEFA2");
            // Attrib for BlockType: Universal Search:Enabled Models              
            RockMigrationHelper.DeleteAttribute("4B5C7A21-E5C8-4954-93A4-063CE1A9EB9C");
            // Attrib for BlockType: Universal Search:Show Filters              
            RockMigrationHelper.DeleteAttribute("8BCE3DFD-B91D-4962-8F5B-B0E56C5CA34B");
            // Attrib for BlockType: Universal Search:Search Input Post-HTML              
            RockMigrationHelper.DeleteAttribute("8F8CDF58-C491-4DB7-AE41-2AC91AE77270");
            // Attrib for BlockType: Universal Search:Search Type              
            RockMigrationHelper.DeleteAttribute("152FA041-3DA7-4BA4-A2D5-87BFA1618536");
            // Attrib for BlockType: Universal Search:Use Custom Results              
            RockMigrationHelper.DeleteAttribute("45F64F50-A41C-4247-B0FF-655520B6AC97");

            // Remove Block: Universal Search, from Page: Universal Search, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "309A2477-9A5B-4FD4-A722-735F87861A05" );

            RockMigrationHelper.DeleteBlockType( "4A89FF55-A6A3-4A9B-8D1D-2ADE092565F5" ); // Entity Attribute Values
            RockMigrationHelper.DeletePage( "B07F30B3-95C4-40A5-9CF6-455399BEF67A" ); //  Page: Universal Search, Layout: Full Width, Site: Rock RMS
        }
    }
}
