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
    public partial class BackgroundCheck : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    --Fix the case on check-in security code merge field
    UPDATE [AttributeValue] 
    SET    [Value] = '{{ Person.SecurityCode }}' 
    WHERE  [Guid] = 'a1ea9242-ef24-41f7-9094-4da8beb9f3b7' 
    AND [Value] = '{{ person.SecurityCode }}'
" );

            RockMigrationHelper.AddBlockType( "Defined Value List Liquid", "Takes a defined type and returns all defined values and merges them with a liquid template.", "~/Blocks/Core/DefinedValueListLiquid.ascx", "Core", "C4ADDDFA-DF16-467E-9285-B1FF0FC066ED" );
            RockMigrationHelper.AddBlockTypeAttribute( "C4ADDDFA-DF16-467E-9285-B1FF0FC066ED", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "The defined type to load values for merge fields.", 0, "", "8F50A283-4AAD-41DD-94AB-F0CF543453AA", true );
            RockMigrationHelper.AddBlockTypeAttribute( "C4ADDDFA-DF16-467E-9285-B1FF0FC066ED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Liquid Template", "LiquidTemplate", "", "Liquid template to use to display content.", 1, @"{% for definedValue in DefinedValues %}
    {{ definedValue.Value }}
{% endfor %}", "FF11C16B-27E1-463B-8B68-E9A854DD28A0", true );
            RockMigrationHelper.AddBlockTypeAttribute( "C4ADDDFA-DF16-467E-9285-B1FF0FC066ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show merge data to help you see what's available to you.", 2, "", "B16D998A-DBAF-48C3-B8A2-ED32ACF21F10", false );

            Sql( @" DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '6FF59F53-28EA-4BFE-AFE1-A459CC588495')
  
  /* info button */
  DECLARE @DefinedValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '3C026B37-29D4-47CB-BB6E-DA43AFE779FE')

  UPDATE [AttributeValue]
  SET [Value] = '<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#20a9c7"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, ''Helvetica Neue'', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #1b8fa8; 
						display: inline-block;"">
							{{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>'
  WHERE [AttributeId] = @AttributeId AND [EntityId] = @DefinedValueId


  /* warning button */
SET @DefinedValueId = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'F03C9591-C497-4E27-A714-6A482E745141')

  UPDATE [AttributeValue]
  SET [Value] = '<div class=""btn-tabled"" style=""float: left; margin: 0 6px 12px 0"">
<table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
  <tr>
    <td>
      <table border=""0"" cellspacing=""0"" cellpadding=""0"">
        <tr>
          <td align=""center"" style=""
			-webkit-border-radius: 4px; 
			-moz-border-radius: 4px; 
			border-radius: 4px;"" 
			bgcolor=""#efc137"">
								
				<a href=""{{ ButtonLink }}"" onclick=""{{ ButtonClick }}"" data-loading-text=""<i class=''fa fa-refresh fa-spin''></i> {{ ButtonText }}""
					style=""	
						font-size: 14px; 
						font-family: OpenSans, ''Helvetica Neue'', Helvetica, Arial, sans-serif; 
						color: #ffffff; 
						text-decoration: none; 
						-webkit-border-radius: 4px; 
						-moz-border-radius: 4px; 
						border-radius: 4px; 
						padding: 6px 12px; 
						border: 1px solid #edb716; 
						display: inline-block;"">
							{{ ButtonText }}
				</a>
			</td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</div>'
  WHERE [AttributeId] = @AttributeId AND [EntityId] = @DefinedValueId
 " );

            // Update the EntityType qualifier on Attribute attributes to use Guid instead of Id
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(ET.[Guid] AS VARCHAR(50))
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = '99B090AA-4D7E-46D8-B393-BF945EA1BA8B'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'entitytype'
    INNER JOIN [EntityType] ET ON ET.[Id] = CAST(AQ.[Value] AS int)
    WHERE ISNUMERIC(AQ.Value) = 1
" );

            // Update the BinaryFileType qualifier on Binary file attributes to use Guid instead of Id
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(BFT.[Guid] AS VARCHAR(50))
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [BinaryFileType] BFT ON BFT.[Id] = CAST(AQ.[Value] AS int)
    WHERE ISNUMERIC(AQ.Value) = 1
" );
            // Add the security role and application group used by background check workflow
            RockMigrationHelper.AddSecurityRoleGroup( "Safety & Security Team", "Group of people who are responsible for the safety and security of staff and members.", "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB" );
            RockMigrationHelper.UpdateGroup( null, "3981CF6D-7D15-4B57-AACE-C0E25D28BD49", "Background Check Administrators", "Group of people who are responsible for approving and reviewing background checks.", null, 0, "A6BCC49E-103F-46B0-8BAC-84EA03FF04D5", false, false, true );

            // Add Background check binary file type
            RockMigrationHelper.UpdateBinaryFileType( "0AA42802-04FD-4AEC-B011-FEB127FC85CD", "Background Check", "The background check result", "fa fa-check-square-o", "5C701472-8A6B-4BBE-AEC6-EC833C859F2D", false, true );

            // Add new person attribute category for background check data
            RockMigrationHelper.UpdatePersonAttributeCategory( "Safety & Security", "fa fa-medkit", "Information related to safety and security of organization", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2" );

            RockMigrationHelper.UpdatePersonAttribute( "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Checked", "BackgroundChecked", "", "Does person have a valid background check on record", 0, "", "DAF87B87-3D1E-463D-A197-52227FE4EA28" );
            RestrictAttributeToSecurityRole( "DAF87B87-3D1E-463D-A197-52227FE4EA28", "Edit" );

            RockMigrationHelper.UpdatePersonAttribute( "6B6AA175-4758-453F-8D83-FCD8044B5F36", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Date", "BackgroundCheckDate", "", "Date person last passed/failed a background check", 0, "", "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F" );
            RockMigrationHelper.AddAttributeQualifier( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "format", "", "2DF6459A-CBAE-4F8D-9870-873EFF77ACD6" );
            RockMigrationHelper.AddAttributeQualifier( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "displayDiff", "False", "7EF8C117-2743-4420-8BC7-7B8F5149A00F" );
            RestrictAttributeToSecurityRole( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "View" );
            RestrictAttributeToSecurityRole( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F", "Edit" );

            RockMigrationHelper.UpdatePersonAttribute( "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Result", "BackgroundCheckResult", "", "Result of last background check", 0, "", "44490089-E02C-4E54-A456-454845ABBC9D" );
            RockMigrationHelper.AddAttributeQualifier( "44490089-E02C-4E54-A456-454845ABBC9D", "values", "Pass,Fail", "65F25E53-576A-445B-B67D-5CE2373A2D85" );
            RockMigrationHelper.AddAttributeQualifier( "44490089-E02C-4E54-A456-454845ABBC9D", "fieldtype", "ddl", "62A411F9-5CEC-492D-9BAB-98F23B4DF44C" );
            RestrictAttributeToSecurityRole( "44490089-E02C-4E54-A456-454845ABBC9D", "View" );
            RestrictAttributeToSecurityRole( "44490089-E02C-4E54-A456-454845ABBC9D", "View" );

            RockMigrationHelper.UpdatePersonAttribute( "6F9E2DD0-E39E-4602-ADF9-EB710A75304A", "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Background Check Document", "BackgroundCheckDocument", "", "The last background check", 0, "", "F3931952-460D-43E0-A6E0-EB6B5B1F9167" );
            RockMigrationHelper.AddAttributeQualifier( "F3931952-460D-43E0-A6E0-EB6B5B1F9167", "binaryFileType", "5c701472-8a6b-4bbe-aec6-ec833c859f2d", "43248635-B07D-49D8-885D-DF46C041CC04" );
            RestrictAttributeToSecurityRole( "F3931952-460D-43E0-A6E0-EB6B5B1F9167", "View" );
            RestrictAttributeToSecurityRole( "44490089-E02C-4E54-A456-454845ABBC9D", "View" );

            
            // Add Block to Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlock( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Attribute Values", "SectionB2", "", "", 2, "8DA21ED3-E4BC-483C-8B22-A041FEEE8FF4" );
            // Attrib for BlockType: Attribute Values:Attribute Order
            RockMigrationHelper.AddBlockTypeAttribute( "D70A59DC-16BE-43BE-9880-59598FA7A94C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Order", "AttributeOrder", "", "The order to use for displaying attributes.  Note: this value is set through the block's UI and does not need to be set here.", 1, @"", "235C6D48-E1D1-410C-8006-1EA412BC12EF" );
            // Attrib Value for Block:Attribute Values, Attribute:Category Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8DA21ED3-E4BC-483C-8B22-A041FEEE8FF4", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"4d1e1eba-abf2-4a7c-8adf-65cb5aae94e2" );

            // Site:Component Page
            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Background Check Providers", "", "53F1B7D9-806A-4541-93BC-4CCF5DFF90B3", "fa fa-check-square-o", "6F8EC649-FDED-4805-B7AF-42A6901C197F" );
            // Add Block to Page: Background Check Providers, Site: Rock RMS
            RockMigrationHelper.AddBlock( "53F1B7D9-806A-4541-93BC-4CCF5DFF90B3", "", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Components", "Main", "", "", 0, "217A55AA-9E19-4B3C-9774-9678E330DD5C" );
            // Attrib Value for Block:Components, Attribute:Component Container Page: Background Check Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "217A55AA-9E19-4B3C-9774-9678E330DD5C", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.Security.BackgroundCheckContainer, Rock" );
            // Attrib Value for Block:Components, Attribute:Support Ordering Page: Background Check Providers, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "217A55AA-9E19-4B3C-9774-9678E330DD5C", "A4889D7B-87AA-419D-846C-3E618E79D875", @"False" );

            // Update the order of attributes to display correctly.
            Sql( @"
    DECLARE @BlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '8DA21ED3-E4BC-483C-8B22-A041FEEE8FF4')
    DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '235C6D48-E1D1-410C-8006-1EA412BC12EF')
    DECLARE @TheValue NVARCHAR(MAX) = 
        ( SELECT TOP 1 CAST([Id] AS VARCHAR) FROM [Attribute] WHERE [Guid] = 'DAF87B87-3D1E-463D-A197-52227FE4EA28' ) + '|' +
        ( SELECT TOP 1 CAST([Id] AS VARCHAR) FROM [Attribute] WHERE [Guid] = '3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F' ) + '|' +
        ( SELECT TOP 1 CAST([Id] AS VARCHAR) FROM [Attribute] WHERE [Guid] = '44490089-E02C-4E54-A456-454845ABBC9D' ) + '|' +
        ( SELECT TOP 1 CAST([Id] AS VARCHAR) FROM [Attribute] WHERE [Guid] = 'F3931952-460D-43E0-A6E0-EB6B5B1F9167' )

    -- Delete existing attribute value first (might have been created by Rock system)
    DELETE [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId
    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
    VALUES ( 1, @AttributeId, @BlockId, @TheValue, NEWID() )
" );

            RockMigrationHelper.UpdateEntityType( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "C16856F4-3C6B-4AFB-A0B8-88A303508206", false, true );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "User Name", "", "Protect My Ministry User Name", 0, "", "8510691C-BCD0-44FA-80B7-1605B2AE5EE7" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "9F7CBDDA-D9DC-4452-935A-F394C7B0434F" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "", "4A904806-606D-410A-B095-97257A7A8863" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "36167F3E-8CB2-44F9-9022-102F171FBC9A", "", "", "Password", "", "Protect My Ministry Password", 1, "", "DE76945E-3C90-4971-986F-A086DADA2CC9" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Test Mode", "", "Should requests be sent in 'test' mode?", 2, "", "6C3B2C5C-B8C0-4740-8F77-521C7D267EA2" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "", "", "Request URL", "", "The Protect My Ministry URL to send requests to.", 3, "", "941D2262-303E-4EC2-8588-7AE3C1DDA2AF" );
            RockMigrationHelper.AddEntityAttribute( "Rock.Security.BackgroundCheck.ProtectMyMinistry", "C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2", "", "", "Return URL", "", "The Web Hook URL for Protect My Ministry to send results to (e.g. 'http://www.yourchurch.com/Webhooks/ProtectMyMinistry.ashx').", 4, "", "B875B054-6046-496A-A521-D0D3DD62BDD0" );
            RockMigrationHelper.AddAttributeValue( "4A904806-606D-410A-B095-97257A7A8863", 0, "True", "7A1CCF58-7739-49AB-A0A4-D8E8C0B7A4DA" );
            RockMigrationHelper.AddAttributeValue( "9F7CBDDA-D9DC-4452-935A-F394C7B0434F", 0, "", "9A42913F-26FB-471D-AA53-161E2999D439" );
            RockMigrationHelper.AddAttributeValue( "8510691C-BCD0-44FA-80B7-1605B2AE5EE7", 0, "", "DCA0E366-9F17-4E03-8C2A-6A48D789F370" );
            RockMigrationHelper.AddAttributeValue( "DE76945E-3C90-4971-986F-A086DADA2CC9", 0, "", "18472FC3-F4A9-4B4A-AD0C-295F3879ED61" );
            RockMigrationHelper.AddAttributeValue( "6C3B2C5C-B8C0-4740-8F77-521C7D267EA2", 0, "False", "40A59878-1417-4811-BC5C-6C39729C1A98" );
            RockMigrationHelper.AddAttributeValue( "941D2262-303E-4EC2-8588-7AE3C1DDA2AF", 0, "https://services.priorityresearch.com/webservice/default.cfm", "1B20D927-6812-4489-937D-7CA322B7777C" );
            RockMigrationHelper.AddAttributeValue( "B875B054-6046-496A-A521-D0D3DD62BDD0", 0, "http://www.yourchurch.com/Webhooks/ProtectMyMinistry.ashx", "B3A0CD1B-E2A9-47FA-B05B-D171405AFDEA" );

            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteActivity","0D5E33A5-8700-4168-A42E-74D78B62D717",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.Delay","D22E73F7-86E2-46CA-AD5B-7770A866726B",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.LogError","E1F593B9-FF5A-4064-845D-331BC491674A",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersonTagAdd","AD415046-96F9-47C8-8E27-3CD97F572994",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.PersonTagRemove","BD876B64-8A93-4E31-B562-B8519FC622C6",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmailTemplate","4487702A-BEAF-4E5A-92AD-71A1AD48DFCE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendSms","F22FA171-B5E7-497F-9AE6-F7B98A377D0E",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToInitiator","4EEAC6FA-B838-410B-A8DD-21A364029F5D",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.WriteToLog","B442940A-0C8B-4F44-8359-1E0AF3AAAB4C",false,true);
            
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.ActivateActivity","38907A90-1634-4A93-8017-619326A4A582",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.AssignActivityFromAttributeValue","F100A31F-E93A-4C7A-9E55-0FAF41A101C4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.AssignActivityToGroup","DB2D8C44-6E57-4B45-8973-5DE327D61554",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.BackgroundCheckRequest","C4DAE3D6-931F-497F-AC00-60BAFA87B758",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.DeleteWorkflow","0E79AF40-4FB0-49D7-AB0E-E95BD828C62D",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunSQL","A41216D6-6FB0-4019-B222-2C29B4519CF4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmail","66197B01-D1F0-4924-A315-47AD54E030DE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToCurrentPerson","24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToEntity","972F19B9-598B-474B-97A4-50E56E7B59D2",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeValue","C789E457-0783-44B3-9D8F-2EBAB5F11110",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetPersonAttribute","320622DA-52E0-41AE-AF90-2BF78B488552",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetStatus","96D371A7-A291-4F8F-8B38-B8F72CE5407E",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetWorkflowName","36005473-BD5D-470B-B28D-98E6D7ED808D",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("0E79AF40-4FB0-49D7-AB0E-E95BD828C62D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","361A1EC8-FFD0-4880-AF68-91DC0E0D7CDC"); // Rock.Workflow.Action.DeleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("0E79AF40-4FB0-49D7-AB0E-E95BD828C62D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","79D23F8B-0DC8-4B48-8A86-AEA48B396C82"); // Rock.Workflow.Action.DeleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","DE9CB292-4785-4EA3-976D-3826F91E9E98"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The attribute to set to the currently logged in person.",0,@"","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("320622DA-52E0-41AE-AF90-2BF78B488552","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184"); // Rock.Workflow.Action.SetPersonAttribute:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("320622DA-52E0-41AE-AF90-2BF78B488552","33E6DF69-BDFA-407A-9744-C175B60643AE","Person","Person","Workflow attribute that contains the person to update.",0,@"","E456FB6F-05DB-4826-A612-5B704BC4EA13"); // Rock.Workflow.Action.SetPersonAttribute:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("320622DA-52E0-41AE-AF90-2BF78B488552","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Value|Attribute Value","Value","The value or attribute value to set the person attribute to. <span class='tip tip-lava'></span>",2,@"","94689BDE-493E-4869-A614-2D54822D747C"); // Rock.Workflow.Action.SetPersonAttribute:Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("320622DA-52E0-41AE-AF90-2BF78B488552","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Person Attribute","PersonAttribute","The person attribute that should be updated with the provided value.",1,@"","8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762"); // Rock.Workflow.Action.SetPersonAttribute:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("320622DA-52E0-41AE-AF90-2BF78B488552","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B"); // Rock.Workflow.Action.SetPersonAttribute:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0A800013-51F7-4902-885A-5BE215D67D3D"); // Rock.Workflow.Action.SetWorkflowName:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","NameValue","The value to use for the workflow's name. <span class='tip tip-lava'></span>",1,@"","93852244-A667-4749-961A-D47F88675BE4"); // Rock.Workflow.Action.SetWorkflowName:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("36005473-BD5D-470B-B28D-98E6D7ED808D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","5D95C15A-CCAE-40AD-A9DD-F929DA587115"); // Rock.Workflow.Action.SetWorkflowName:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E8ABD802-372C-47BE-82B1-96F50DB5169E"); // Rock.Workflow.Action.ActivateActivity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","739FD425-5B8C-4605-B775-7E4D9D4C11DB","Activity","Activity","The activity type to activate",0,@"","02D5A7A5-8781-46B4-B9FC-AF816829D240"); // Rock.Workflow.Action.ActivateActivity:Activity
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("38907A90-1634-4A93-8017-619326A4A582","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3809A78C-B773-440C-8E3F-A8E81D0DAE08"); // Rock.Workflow.Action.ActivateActivity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Body","Body","The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",3,@"","4D245B9E-6B03-46E7-8482-A51FBA190E4D"); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36197160-7D3D-490D-AB42-7E29105AFE91"); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","From Email Address|Attribute Value","From","The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",0,@"","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC"); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Address|Attribute Value","To","The email address or an attribute that contains the person or email address that email should be sent to",1,@"","0C4C13B8-7076-4872-925A-F950886B5E16"); // Rock.Workflow.Action.SendEmail:Send To Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Subject","Subject","The subject that should be used when sending email. <span class='tip tip-lava'></span>",2,@"","5D9B13B6-CD96-4C7C-86FA-4512B9D28386"); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D1269254-C15A-40BD-B784-ADCC231D3950"); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("96D371A7-A291-4F8F-8B38-B8F72CE5407E","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36CE41F4-4C87-4096-B0C6-8269163BCC0A"); // Rock.Workflow.Action.SetStatus:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("96D371A7-A291-4F8F-8B38-B8F72CE5407E","9C204CD0-1233-41C5-818A-C5DA439445AA","Status","Status","The status to set workflow to. <span class='tip tip-lava'></span>",0,@"","91A9F4BE-4A8E-430A-B466-A88DB2D33B34"); // Rock.Workflow.Action.SetStatus:Status
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("96D371A7-A291-4F8F-8B38-B8F72CE5407E","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","AE8C180C-E370-414A-B10D-97891B95D105"); // Rock.Workflow.Action.SetStatus:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","9392E3D7-A28B-4CD8-8B03-5E147B102EF1"); // Rock.Workflow.Action.SetAttributeToEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Use Id instead of Guid","UseId","Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).",1,@"False","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B"); // Rock.Workflow.Action.SetAttributeToEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",0,@"","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7"); // Rock.Workflow.Action.SetAttributeToEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","AD4EFAC4-E687-43DF-832F-0DC3856ABABB"); // Rock.Workflow.Action.SetAttributeToEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","SQLQuery","SQLQuery","The SQL query to run. <span class='tip tip-lava'></span>",0,@"","F3B9908B-096F-460B-8320-122CF046D1F9"); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","A18C3143-0586-4565-9F36-E603BC674B4E"); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","33E6DF69-BDFA-407A-9744-C175B60643AE","Result Attribute","ResultAttribute","An optional attribute to set to the scaler result of SQL query.",1,@"","56997192-2545-4EA1-B5B2-313B04588984"); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","FA7C685D-8636-41EF-9998-90FFF3998F76"); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","6BEBD4BE-EDC7-4757-B597-445FC60DB6ED"); // Rock.Workflow.Action.BackgroundCheckRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","33E6DF69-BDFA-407A-9744-C175B60643AE","Billing Code Attribute","BillingCodeAttribute","The attribute that contains the billing code to use when submitting background check.",4,@"","232B2F98-3B2F-4C53-81FC-061A92675C41"); // Rock.Workflow.Action.BackgroundCheckRequest:Billing Code Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The Person attribute that contains the person who the background check should be submitted for.",1,@"","077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13"); // Rock.Workflow.Action.BackgroundCheckRequest:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","33E6DF69-BDFA-407A-9744-C175B60643AE","Request Type Attribute","RequestTypeAttribute","The attribute that contains the type of background check to submit (Specific to provider).",3,@"","EC759165-949E-4966-BAFD-68A656A4EBF7"); // Rock.Workflow.Action.BackgroundCheckRequest:Request Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","33E6DF69-BDFA-407A-9744-C175B60643AE","SSN Attribute","SSNAttribute","The attribute that contains the Social Security Number of the person who the background check should be submitted for ( Must be an 'Encrypted Text' attribute )",2,@"","2631E72B-1D9B-40E8-B857-8B1D41943451"); // Rock.Workflow.Action.BackgroundCheckRequest:SSN Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","A7486B0E-4CA2-4E00-A987-5544C7DABA76","Background Check Provider","Provider","The Background Check provider to use",0,@"","6E2366B4-9F0E-454A-9DB1-E06263749C12"); // Rock.Workflow.Action.BackgroundCheckRequest:Background Check Provider
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C4DAE3D6-931F-497F-AC00-60BAFA87B758","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","3936E931-CC27-4C38-9AA5-AAA502057333"); // Rock.Workflow.Action.BackgroundCheckRequest:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","D7EAA859-F500-4521-9523-488B12EAA7D2"); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",0,@"","44A0B977-4730-4519-8FF6-B0A01A95B212"); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","Value","The text or attribute to set the value from. <span class='tip tip-lava'></span>",1,@"","E5272B11-A2B8-49DC-860D-8D574E2BC15C"); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","57093B41-50ED-48E5-B72B-8829E62704C8"); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("DB2D8C44-6E57-4B45-8973-5DE327D61554","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","C0D75D1A-16C5-4786-A1E0-25669BEE8FE9"); // Rock.Workflow.Action.AssignActivityToGroup:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("DB2D8C44-6E57-4B45-8973-5DE327D61554","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","041B7B51-A694-4AF5-B455-64D0DE7160A2"); // Rock.Workflow.Action.AssignActivityToGroup:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("DB2D8C44-6E57-4B45-8973-5DE327D61554","CC34CE2C-0B0E-4BB3-9549-454B2A7DF218","Group","Group","Select group type, then group, to set the group to assign this activity to.",0,@"","BBFAD050-5968-4D11-8887-2FF877D8C8AB"); // Rock.Workflow.Action.AssignActivityToGroup:Group
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The person or group attribute value to assign this activity to.",0,@"","FBADD25F-D309-4512-8430-3CC8615DD60E"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("F100A31F-E93A-4C7A-9E55-0FAF41A101C4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA"); // Rock.Workflow.Action.AssignActivityFromAttributeValue:Order

            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","Safety & Security","fa fa-medkit","","6F8A431C-BEBD-4D33-AAD6-1D70870329C2",0); // Safety & Security
            RestrictCategoryToSecurityRole( "6F8A431C-BEBD-4D33-AAD6-1D70870329C2", "View" );
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "FB420F14-3D9D-4304-878F-124902E2CEAB", "6f8a431c-bebd-4d33-aad6-1d70870329c2", true );

            RockMigrationHelper.UpdateWorkflowType(false,true,"Background Check","Used to request a background check be performed on a person","6F8A431C-BEBD-4D33-AAD6-1D70870329C2","Request","fa fa-check-square-o",0,true,0,"16D12EF7-C546-4039-9036-B73D118EDC90"); // Background Check
            
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","1B71FEF4-201F-4D53-8C60-2DF21F1985ED","Campus","Campus","If included, the campus name will be used as the Billing Reference Code for the request (optional)",7,@"","A5255A04-4832-45C3-A727-00395D082D23"); // Background Check:Campus
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Checked Attribute","CheckedAttribute","The person attribute that indicates if person has a valid background check (passed)", 0,@"daf87b87-3d1e-463d-a197-52227fe4ea28","79EF6D66-CB5E-4514-8652-120F362A7EF7" ); // Background Check:Checked Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Date Attribute","DateAttribute","The person attribute the stores the date background check was completed",1,@"3daff000-7f74-47d7-8cb0-e4a4e6c81f5f","7306A2BC-F6EB-4A81-921F-30CEA98AD489"); // Background Check:Date Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Type","PackageType","Value should be the type of background check to request from the vendor.",9,@"","A4CB9461-D77F-40E0-8DFF-C7838D78F2EC"); // Background Check:Type
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","The person who request should be initiated for",5,@"","AF3F0233-9786-422D-83C8-A7565D99A01D"); // Background Check:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Reason","Reason","A brief description of the reason that a background check is being requested",10,@"","6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E"); // Background Check:Reason
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","C403E219-A56B-439E-9D50-9302DFE760CF","Report","Report","The downloaded background check report",14,@"","E6E5CF21-5A49-4630-9E18-531FF354380E"); // Background Check:Report
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","C0D0D7E2-C3B0-4004-ABEA-4BBFAD10D5D2","Report Link","ReportLink","The location (URL) of the background report result",12,@"","3211A344-7959-4A87-8557-C33B2554208A"); // Background Check:Report Link
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","9C204CD0-1233-41C5-818A-C5DA439445AA","Report Recommendation","ReportRecommendation","Providers recommendation ( if any )",13,@"","F54BC71C-5C93-4EA9-88DF-4B89F457BA5D"); // Background Check:Report Recommendation
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Report Status","ReportStatus","The result status of the background check",11,@"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3"); // Background Check:Report Status
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requester","Requester","The person initiating the request",4,@"","BBD98D86-166C-4904-AD22-B58EB9B006D9"); // Background Check:Requester
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Result Attribute","ResultAttribute","The person attribute that stores the background check document",3,@"f3931952-460d-43e0-a6e0-eb6b5b1f9167","79061845-E1AA-4FEB-8387-921ECAAEEF6F"); // Background Check:Result Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","36167F3E-8CB2-44F9-9022-102F171FBC9A","SSN","SSN","The SSN of the person that the request is for",8,@"","1FAB9A4C-C5A2-4938-B9BD-80935F0A598C"); // Background Check:SSN
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","99B090AA-4D7E-46D8-B393-BF945EA1BA8B","Status Attribute","StatusAttribute","The person attribute that stores the background check status",2,@"44490089-e02c-4e54-a456-454845abbc9d","8C47E696-3749-4C49-942C-12AE9FA1AE1C"); // Background Check:Status Attribute
            RockMigrationHelper.UpdateWorkflowTypeAttribute("16D12EF7-C546-4039-9036-B73D118EDC90","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Warn Of Recent","WarnOfRecent","Flag indicating if user should be warned that person has a recent background check already.",6,@"False","43FB2810-DFF3-40E5-9EC6-FC047A6061A1"); // Background Check:Warn Of Recent
            RockMigrationHelper.AddAttributeQualifier("79EF6D66-CB5E-4514-8652-120F362A7EF7","allowmultiple",@"False","F3020AB7-0057-490C-A2CD-7193C6605D2F"); // Background Check:Checked Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier("79EF6D66-CB5E-4514-8652-120F362A7EF7","entitytype",@"72657ed8-d16e-492e-ac12-144c5e7567e7","8D2F28B8-8748-4A66-AE7C-31081E6D0283"); // Background Check:Checked Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier("7306A2BC-F6EB-4A81-921F-30CEA98AD489","allowmultiple",@"False","ACEE0C26-5AFA-4E3A-A42F-41830EAF87BE"); // Background Check:Date Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier("7306A2BC-F6EB-4A81-921F-30CEA98AD489","entitytype",@"72657ed8-d16e-492e-ac12-144c5e7567e7","2E5ECA21-13AE-4662-9B17-3D409615F415"); // Background Check:Date Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier("A4CB9461-D77F-40E0-8DFF-C7838D78F2EC","fieldtype",@"ddl","3DBE544E-3688-449C-84DA-058D818DC4D2"); // Background Check:Type:fieldtype
            RockMigrationHelper.AddAttributeQualifier("A4CB9461-D77F-40E0-8DFF-C7838D78F2EC","values",@"Basic,Plus","69720270-E209-4110-B2CB-AE887662AB61"); // Background Check:Type:values
            RockMigrationHelper.AddAttributeQualifier("6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E","numberofrows",@"4","88C3F07A-BDDA-4083-8B20-C687A62A79C7"); // Background Check:Reason:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "E6E5CF21-5A49-4630-9E18-531FF354380E", "binaryFileType", @"5C701472-8A6B-4BBE-AEC6-EC833C859F2D", "90C87FF1-F201-4C0B-9D48-F53B81BB57D9" ); // Background Check:Report:binaryFileType
            RockMigrationHelper.AddAttributeQualifier("F54BC71C-5C93-4EA9-88DF-4B89F457BA5D","ispassword",@"False","C4E82826-62B7-4CF6-9ED3-291E1FFA659B"); // Background Check:Report Recommendation:ispassword
            RockMigrationHelper.AddAttributeQualifier("A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3","fieldtype",@"ddl","44DE1B1C-27B0-4F19-969B-4D318BE53F61"); // Background Check:Report Status:fieldtype
            RockMigrationHelper.AddAttributeQualifier("A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3","values",@"Pass,Fail,Review","68C6E385-2CA2-4F6E-9109-D999A746E100"); // Background Check:Report Status:values
            RockMigrationHelper.AddAttributeQualifier("79061845-E1AA-4FEB-8387-921ECAAEEF6F","allowmultiple",@"False","00822305-717A-4A6F-A84F-0E0A6E9345D1"); // Background Check:Result Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier("79061845-E1AA-4FEB-8387-921ECAAEEF6F","entitytype",@"72657ed8-d16e-492e-ac12-144c5e7567e7","1EC3600E-AC87-425F-9B97-39A6B3A830BA"); // Background Check:Result Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier("1FAB9A4C-C5A2-4938-B9BD-80935F0A598C","ispassword",@"False","2FC752E9-43BD-4A83-A690-184B893039F2"); // Background Check:SSN:ispassword
            RockMigrationHelper.AddAttributeQualifier("8C47E696-3749-4C49-942C-12AE9FA1AE1C","allowmultiple",@"False","B0748167-67E5-4913-8C97-698BB123D1D6"); // Background Check:Status Attribute:allowmultiple
            RockMigrationHelper.AddAttributeQualifier("8C47E696-3749-4C49-942C-12AE9FA1AE1C","entitytype",@"72657ed8-d16e-492e-ac12-144c5e7567e7","6864671E-0488-48DA-BA2E-2CF52AB35263"); // Background Check:Status Attribute:entitytype
            RockMigrationHelper.AddAttributeQualifier("43FB2810-DFF3-40E5-9EC6-FC047A6061A1","falsetext",@"No","73A34812-F82F-4679-998A-73DC97F3B141"); // Background Check:Warn Of Recent:falsetext
            RockMigrationHelper.AddAttributeQualifier("43FB2810-DFF3-40E5-9EC6-FC047A6061A1","truetext",@"Yes","9EABFAB2-6FE5-472A-B5A8-C74A0825C27C"); // Background Check:Warn Of Recent:truetext
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Initial Request","Saves the person and requester and prompts for additional information needed to perform the request ( SSN, Campus, Type, etc).",true,0,"2950B120-7BB5-46B5-93D0-26D3936F1894"); // Background Check:Initial Request
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Process Result","Evaluates the result of the background check received from the provider",false,4,"0840A659-3935-4DD2-AC5D-1D022ABFB45C"); // Background Check:Process Result
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Cancel Request","Cancels the request prior to submitting to provider and deletes the workflow.",false,7,"F47C3F69-4485-4A6A-BFCE-C44FE628DF3E"); // Background Check:Cancel Request
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Approve Request","Assigns the activity to security team and waits for their approval before submitting the request.",false,1,"342BCBFC-2CA7-426E-ABBB-A7C461A05736"); // Background Check:Approve Request
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Review Denial","Provides the requester a way to add additional information for the security team to approve request.",false,2,"A9CD5D05-B60B-4A19-93FF-1DE6FB5A2B3A"); // Background Check:Review Denial
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Review Result","Allows for review of the results from provider.",false,5,"2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F"); // Background Check:Review Result
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Complete Request","Notifies requester of result and updates person's record with result",false,6,"F95F8B4B-3ACD-4906-81F5-EBF589F87831"); // Background Check:Complete Request
            RockMigrationHelper.UpdateWorkflowActivityType("16D12EF7-C546-4039-9036-B73D118EDC90",true,"Submit Request","Submits the background request to the selected provider for processing.",false,3,"4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956"); // Background Check:Submit Request
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("342BCBFC-2CA7-426E-ABBB-A7C461A05736","9C204CD0-1233-41C5-818A-C5DA439445AA","Approval Status","ApprovalStatus","The status of the appoval (Approve,Deny)",2,@"","5B0B5806-F163-430B-A89D-8A16F1BC2FBF"); // Background Check:Approve Request:Approval Status
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("342BCBFC-2CA7-426E-ABBB-A7C461A05736","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Approver","Approver","Person who approved or denied this request",1,@"","6F334E7A-6FAB-41A2-82AB-2A2B92E84014"); // Background Check:Approve Request:Approver
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("342BCBFC-2CA7-426E-ABBB-A7C461A05736","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Note","Note","Any notes that approver wants to provide to submitter for review",0,@"","6F21C558-7C30-45AC-AA80-0FA12F26DDA1"); // Background Check:Approve Request:Note
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","9C204CD0-1233-41C5-818A-C5DA439445AA","Review Result","ReviewResult","The result of the review (Pass,Fail)",0,@"","E0166CA1-200E-4506-A3E7-B10FF0471C90"); // Background Check:Review Result:Review Result
            RockMigrationHelper.AddAttributeQualifier("5B0B5806-F163-430B-A89D-8A16F1BC2FBF","ispassword",@"False","67EA5AF8-6E4F-496A-93CE-A7E124BF6BE1"); // Background Check:Approval Status:ispassword
            RockMigrationHelper.AddAttributeQualifier("6F21C558-7C30-45AC-AA80-0FA12F26DDA1","numberofrows",@"5","E41C9615-067C-4135-A4F9-684F16ACA7BC"); // Background Check:Note:numberofrows
            RockMigrationHelper.AddAttributeQualifier("E0166CA1-200E-4506-A3E7-B10FF0471C90","ispassword",@"False","5D0E45CF-0DB1-40ED-A399-F93355EC14B4"); // Background Check:Review Result:ispassword
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the form below to start the background
    request process.
</p>
{% if Workflow.WarnOfRecent == 'Yes' %}
    <div class='alert alert-warning'>
        Notice: It's been less than a year since this person's last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^Your request has been submitted successfully.|Cancel^8cf6e927-4fa5-4241-991c-391038b79631^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|","",false,"","328B74E5-6058-4C4E-9EF8-EC10985F18A8"); // Background Check:Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request has been submitted for your review.
    If you approve the request it will be sent to the background check provider for processing. If you
    deny the request, it will be sent back to the requester. If you deny the request, please add notes
    explaining why the request was denied.
</div>",@"","Approve^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been submitted to provider for processing.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The requester will be notified that this request has been denied (along with the reason why).|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"5B0B5806-F163-430B-A89D-8A16F1BC2FBF","DFAEC61D-D109-468D-984E-C9FA4C5321CB"); // Background Check:Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, this request has come back from the approval process with the following results.
</p>

<div class=""well"">
    <strong>Summary of Security Notes:</strong><br />
    <table class="" table table-condensed table-light margin-b-md"">
    	{% for activity in Workflow.Activities %}
    		{% if activity.ActivityType.Name == 'Approve Request' %}
    			<tr>
    				<td width=""220"">{{activity.CompletedDateTime}}</td>
    				<td width=""220"">{{activity.Approver}}</td>
    				<td>{{activity.Note}}</td>
    			</tr>
    		{% endif %}
    	{% endfor %}
    </table>
    
</div>
<hr />",@"","Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^The request has been submitted again to the security team for approval.|Cancel Request^8cf6e927-4fa5-4241-991c-391038b79631^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","91C87731-05BC-44FA-AB84-881F73EDDA20"); // Background Check:Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionForm(@"<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request was submitted and completed, but requires
    your review. Please pass or fail this request. The requester will be notified and the person's record 
    will be updated to indicate the result you select.
</div>
<hr>",@"","Pass^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been marked as passed. Requester will be notified.|Fail^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The request has been marked as failed. Requester will be notified.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"E0166CA1-200E-4506-A3E7-B10FF0471C90","F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD"); // Background Check:Review Result:Review Results
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","7306A2BC-F6EB-4A81-921F-30CEA98AD489",0,false,true,false,"20AF900B-699F-4F1A-B506-86A51F08E794"); // Background Check:Initial Request:Get Details:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","8C47E696-3749-4C49-942C-12AE9FA1AE1C",1,false,true,false,"DFDB674F-FE75-4875-83CA-959927C8ADD7"); // Background Check:Initial Request:Get Details:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","79061845-E1AA-4FEB-8387-921ECAAEEF6F",2,false,true,false,"56E0C69F-C0BE-4BB4-B470-53123BB5D509"); // Background Check:Initial Request:Get Details:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","BBD98D86-166C-4904-AD22-B58EB9B006D9",3,false,true,false,"5A461898-D784-445F-BF68-1B8ECDE0AE8E"); // Background Check:Initial Request:Get Details:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","AF3F0233-9786-422D-83C8-A7565D99A01D",4,true,true,false,"9AE55225-8596-40CE-8E61-C3BEED939328"); // Background Check:Initial Request:Get Details:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","43FB2810-DFF3-40E5-9EC6-FC047A6061A1",5,false,true,false,"8566F913-FC7B-4527-B530-E3364789B9EB"); // Background Check:Initial Request:Get Details:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","1FAB9A4C-C5A2-4938-B9BD-80935F0A598C",6,true,false,true,"B6F9D3E9-5641-444A-9075-DB6D06573575"); // Background Check:Initial Request:Get Details:SSN
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","A5255A04-4832-45C3-A727-00395D082D23",7,true,false,true,"18346714-25CC-4673-A4AF-BDDA3C2F1BDA"); // Background Check:Initial Request:Get Details:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","A4CB9461-D77F-40E0-8DFF-C7838D78F2EC",8,false,true,false,"9C4942B8-07A0-4101-B319-2E7E6B293961"); // Background Check:Initial Request:Get Details:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",10,false,true,false,"E866E672-F52E-4296-A0A5-D74CE03C66B6"); // Background Check:Initial Request:Get Details:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","3211A344-7959-4A87-8557-C33B2554208A",11,false,true,false,"82EAEBB6-0577-4E3E-9881-E346F7F2DAD3"); // Background Check:Initial Request:Get Details:Report Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","E6E5CF21-5A49-4630-9E18-531FF354380E",12,false,true,false,"96A08805-E371-4143-A731-5D9503684265"); // Background Check:Initial Request:Get Details:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E",9,true,false,false,"5D19CED3-63DE-4EA7-9D2E-6FAF98822C37"); // Background Check:Initial Request:Get Details:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","F54BC71C-5C93-4EA9-88DF-4B89F457BA5D",13,false,true,false,"8E1A8CCB-6F50-481C-8CD8-AEAF7220E069"); // Background Check:Initial Request:Get Details:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("328B74E5-6058-4C4E-9EF8-EC10985F18A8","79EF6D66-CB5E-4514-8652-120F362A7EF7",14,false,true,false,"F5680DF1-251F-4923-92D9-670FB0F173B2"); // Background Check:Initial Request:Get Details:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","79EF6D66-CB5E-4514-8652-120F362A7EF7",17,false,true,false,"968B334A-39A0-43BE-843B-D00E5CD67B94"); // Background Check:Approve Request:Approve or Deny:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","F54BC71C-5C93-4EA9-88DF-4B89F457BA5D",16,false,true,false,"4756F965-614B-4E18-8184-D67A65D5AF0C"); // Background Check:Approve Request:Approve or Deny:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","6F334E7A-6FAB-41A2-82AB-2A2B92E84014",14,false,true,false,"D47F30C9-1EEC-42C6-8398-F14B88B98643"); // Background Check:Approve Request:Approve or Deny:Approver
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","5B0B5806-F163-430B-A89D-8A16F1BC2FBF",15,false,true,false,"6295CE8A-5539-4E32-AE2E-67E68CA5DB2E"); // Background Check:Approve Request:Approve or Deny:Approval Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","7306A2BC-F6EB-4A81-921F-30CEA98AD489",0,false,true,false,"77B1822E-4CBB-4281-A69B-9ED6D0ADFCD3"); // Background Check:Approve Request:Approve or Deny:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","8C47E696-3749-4C49-942C-12AE9FA1AE1C",1,false,true,false,"CEBFE00A-ACA2-4BA2-8779-F429AE1ACAB1"); // Background Check:Approve Request:Approve or Deny:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","79061845-E1AA-4FEB-8387-921ECAAEEF6F",2,false,true,false,"0FCEAA23-0F8C-411F-843C-609CCEDE17F1"); // Background Check:Approve Request:Approve or Deny:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","BBD98D86-166C-4904-AD22-B58EB9B006D9",3,true,true,false,"D64B71B4-8D90-491F-8829-5D44F428E052"); // Background Check:Approve Request:Approve or Deny:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","AF3F0233-9786-422D-83C8-A7565D99A01D",4,true,true,false,"C57CA5F5-99C6-4A31-BA04-69912774AB55"); // Background Check:Approve Request:Approve or Deny:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","43FB2810-DFF3-40E5-9EC6-FC047A6061A1",5,false,true,false,"ED232AFF-67BC-485A-89E3-966C24148A7F"); // Background Check:Approve Request:Approve or Deny:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","A5255A04-4832-45C3-A727-00395D082D23",6,true,true,false,"3E522C0A-418C-499A-AE19-B8F6F6CF677E"); // Background Check:Approve Request:Approve or Deny:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","1FAB9A4C-C5A2-4938-B9BD-80935F0A598C",7,false,true,false,"55D7536B-764D-4215-9E8A-34700CF54DC5"); // Background Check:Approve Request:Approve or Deny:SSN
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","A4CB9461-D77F-40E0-8DFF-C7838D78F2EC",9,true,false,true,"5E7C0A87-D6FD-4B11-ABB4-C9404C5009A5"); // Background Check:Approve Request:Approve or Deny:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E",8,true,true,false,"751DE27D-D9EE-46FB-8F6B-970669349BD9"); // Background Check:Approve Request:Approve or Deny:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",11,false,true,false,"B8D079CB-65B4-4642-96A3-EF1BD6532F7A"); // Background Check:Approve Request:Approve or Deny:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","3211A344-7959-4A87-8557-C33B2554208A",12,false,true,false,"2F55F04B-5AA0-41EE-A21C-64D798212A3E"); // Background Check:Approve Request:Approve or Deny:Report Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","E6E5CF21-5A49-4630-9E18-531FF354380E",13,false,true,false,"3053746F-E930-4F73-8EA8-900BCCE593DE"); // Background Check:Approve Request:Approve or Deny:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("DFAEC61D-D109-468D-984E-C9FA4C5321CB","6F21C558-7C30-45AC-AA80-0FA12F26DDA1",10,true,false,false,"99F44F64-9439-4E5E-B4A8-1B6A6CA564C9"); // Background Check:Approve Request:Approve or Deny:Note
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","7306A2BC-F6EB-4A81-921F-30CEA98AD489",0,false,true,false,"61CBAC9B-98FA-416E-889A-59DD85549106"); // Background Check:Review Denial:Review:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","8C47E696-3749-4C49-942C-12AE9FA1AE1C",1,false,true,false,"E25631A4-D314-4F86-9DDA-797804DF30E4"); // Background Check:Review Denial:Review:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","79061845-E1AA-4FEB-8387-921ECAAEEF6F",2,false,true,false,"A82DFD64-026B-465E-A1C3-421DAFA00B0B"); // Background Check:Review Denial:Review:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","BBD98D86-166C-4904-AD22-B58EB9B006D9",3,false,true,false,"BA7A5ACB-025E-4FED-B755-AF649CBEF55B"); // Background Check:Review Denial:Review:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","43FB2810-DFF3-40E5-9EC6-FC047A6061A1",4,false,true,false,"1215C927-1D29-4761-B2DA-1E57ABC93D47"); // Background Check:Review Denial:Review:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","AF3F0233-9786-422D-83C8-A7565D99A01D",5,true,true,false,"34FB3DDF-172A-4D17-80C7-8AEAD1EB7E12"); // Background Check:Review Denial:Review:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","A5255A04-4832-45C3-A727-00395D082D23",6,true,false,true,"2B83EB67-9E45-4311-A277-73CCBD278F30"); // Background Check:Review Denial:Review:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","1FAB9A4C-C5A2-4938-B9BD-80935F0A598C",7,true,false,false,"B0F5E672-581C-444F-BB70-1041EC9E14A2"); // Background Check:Review Denial:Review:SSN
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","A4CB9461-D77F-40E0-8DFF-C7838D78F2EC",8,true,false,true,"C9F759CA-BC14-4F2B-8601-78390D3D4BE2"); // Background Check:Review Denial:Review:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E",9,true,false,false,"1407C7F1-D151-44F3-A3FB-9A55D73777F0"); // Background Check:Review Denial:Review:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",10,false,true,false,"AB2BE181-32CF-43C0-B7B5-7EC059D5958B"); // Background Check:Review Denial:Review:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","3211A344-7959-4A87-8557-C33B2554208A",11,false,true,false,"8ED478F3-39B9-438A-BD84-EF5BDC29026A"); // Background Check:Review Denial:Review:Report Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","E6E5CF21-5A49-4630-9E18-531FF354380E",12,false,true,false,"E913C350-849A-4D6B-968C-E5CBBA422180"); // Background Check:Review Denial:Review:Report
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","F54BC71C-5C93-4EA9-88DF-4B89F457BA5D",13,false,true,false,"697B5D4A-13AB-4C2D-97A2-412E110D35B9"); // Background Check:Review Denial:Review:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("91C87731-05BC-44FA-AB84-881F73EDDA20","79EF6D66-CB5E-4514-8652-120F362A7EF7",14,false,true,false,"B25B92CB-7534-4F8D-BDEB-D7D9331E2838"); // Background Check:Review Denial:Review:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","79EF6D66-CB5E-4514-8652-120F362A7EF7",15,false,true,false,"0E48A145-F60F-4E28-BEC2-B7C67B4701E2"); // Background Check:Review Result:Review Results:Checked Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","F54BC71C-5C93-4EA9-88DF-4B89F457BA5D",11,true,true,false,"B9AF465A-FAEF-4673-8150-935303853F61"); // Background Check:Review Result:Review Results:Report Recommendation
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","E0166CA1-200E-4506-A3E7-B10FF0471C90",14,false,true,false,"A83CB9FE-582E-4FD2-9B43-7CED099C2583"); // Background Check:Review Result:Review Results:Review Result
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","7306A2BC-F6EB-4A81-921F-30CEA98AD489",0,false,true,false,"9E768826-ED9F-4DCD-8583-2EA6F50B449E"); // Background Check:Review Result:Review Results:Date Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","8C47E696-3749-4C49-942C-12AE9FA1AE1C",1,false,true,false,"5377809D-8FBB-4785-8716-759A6CBE2B76"); // Background Check:Review Result:Review Results:Status Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","79061845-E1AA-4FEB-8387-921ECAAEEF6F",2,false,true,false,"DF40215E-1492-4A2D-8E3E-4FE467E43CE0"); // Background Check:Review Result:Review Results:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","BBD98D86-166C-4904-AD22-B58EB9B006D9",3,true,true,false,"DF66AC42-6A3E-4CB7-8868-E6AB3107AD2F"); // Background Check:Review Result:Review Results:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","AF3F0233-9786-422D-83C8-A7565D99A01D",4,true,true,false,"D51C8B06-B45B-4FCC-AE6D-4A65F334F934"); // Background Check:Review Result:Review Results:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","43FB2810-DFF3-40E5-9EC6-FC047A6061A1",5,false,true,false,"36A00CEE-9DCD-42D8-B74E-12F794A02564"); // Background Check:Review Result:Review Results:Warn Of Recent
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","A5255A04-4832-45C3-A727-00395D082D23",6,true,true,false,"8B5401AA-6D7C-4012-9D4D-0432F22F2D5A"); // Background Check:Review Result:Review Results:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","1FAB9A4C-C5A2-4938-B9BD-80935F0A598C",7,false,true,false,"FB94F7FB-56C6-4A9D-8C1B-B0D0F02F28CA"); // Background Check:Review Result:Review Results:SSN
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","A4CB9461-D77F-40E0-8DFF-C7838D78F2EC",8,false,true,false,"BA8B1932-4E70-4915-AD7F-69463B2D1FC1"); // Background Check:Review Result:Review Results:Type
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","6D5417F5-C6DB-4FCF-90B3-7BEFB4A73F8E",9,true,true,false,"E8C2ED86-AC77-4FC7-AB9C-3BF6682D9A6B"); // Background Check:Review Result:Review Results:Reason
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",10,false,true,false,"E5D546D0-C315-465C-B125-EC00E964D95B"); // Background Check:Review Result:Review Results:Report Status
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","3211A344-7959-4A87-8557-C33B2554208A",12,true,true,false,"5F630999-3176-446E-A715-443C04DBF0C4"); // Background Check:Review Result:Review Results:Report Link
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","E6E5CF21-5A49-4630-9E18-531FF354380E",13,true,true,false,"559A864B-C7F2-4D0B-9F1C-555F263FB22D"); // Background Check:Review Result:Review Results:Report
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Set Person",1,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","B9F9636B-CD44-48DB-BE3D-B830CDED59EB"); // Background Check:Initial Request:Set Person
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Set Requester",3,"24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",true,false,"","",1,"","3136A135-4836-4C09-BD81-326CA21C6AA5"); // Background Check:Initial Request:Set Requester
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Set Warning",4,"A41216D6-6FB0-4019-B222-2C29B4519CF4",true,false,"","",1,"","81D1FB3E-5017-4A53-A4EF-F6618F782935"); // Background Check:Initial Request:Set Warning
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Set Name",2,"36005473-BD5D-470B-B28D-98E6D7ED808D",true,false,"","",1,"","6A779AB3-3223-411B-9AEE-87A5EE1EDF12"); // Background Check:Initial Request:Set Name
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Get Details",5,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"328B74E5-6058-4C4E-9EF8-EC10985F18A8","",1,"","A3EAF2A3-97FB-47A6-9844-F7F0755FC5BE"); // Background Check:Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionType("2950B120-7BB5-46B5-93D0-26D3936F1894","Set Status",0,"96D371A7-A291-4F8F-8B38-B8F72CE5407E",true,false,"","",1,"","10E8C6CB-A72C-4254-A1F5-E43B6C7B404B"); // Background Check:Initial Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType("0840A659-3935-4DD2-AC5D-1D022ABFB45C","Activate Review",2,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",2,"Pass","77C9D555-9141-418D-91AB-FF71AE5AA778"); // Background Check:Process Result:Activate Review
            RockMigrationHelper.UpdateWorkflowActionType("0840A659-3935-4DD2-AC5D-1D022ABFB45C","Activate Complete",3,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",1,"Pass","B982C744-0965-4FE8-ACB2-08FE8AD467FF"); // Background Check:Process Result:Activate Complete
            RockMigrationHelper.UpdateWorkflowActionType("0840A659-3935-4DD2-AC5D-1D022ABFB45C","Save Date",0,"320622DA-52E0-41AE-AF90-2BF78B488552",true,false,"","",1,"","A8AADFA0-45F6-4DDF-A549-D29D5F424EB3"); // Background Check:Process Result:Save Date
            RockMigrationHelper.UpdateWorkflowActionType("0840A659-3935-4DD2-AC5D-1D022ABFB45C","Save Report",1,"320622DA-52E0-41AE-AF90-2BF78B488552",true,false,"","",1,"","933E6BF0-D87B-417F-B574-66D50EBD0F90"); // Background Check:Process Result:Save Report
            RockMigrationHelper.UpdateWorkflowActionType("F47C3F69-4485-4A6A-BFCE-C44FE628DF3E","Delete Workflow",0,"0E79AF40-4FB0-49D7-AB0E-E95BD828C62D",true,false,"","",1,"","078B2C46-E1D1-4C49-ACEF-86A488AA42BA"); // Background Check:Cancel Request:Delete Workflow
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Assign to Security",1,"DB2D8C44-6E57-4B45-8973-5DE327D61554",true,false,"","",1,"","207A7FE1-BBB8-453D-8221-A968E637D92D"); // Background Check:Approve Request:Assign to Security
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Approve or Deny",2,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"DFAEC61D-D109-468D-984E-C9FA4C5321CB","",1,"","CFF91283-688F-42C4-8576-7A7DD6630A6F"); // Background Check:Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Set Approver",3,"24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",true,false,"","",1,"","C90454AC-98DD-4A7B-9C82-A9A810A13450"); // Background Check:Approve Request:Set Approver
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Submit Request",4,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","5B0B5806-F163-430B-A89D-8A16F1BC2FBF",1,"Approve","C8246CA7-39F7-450F-8181-D78EE436F26E"); // Background Check:Approve Request:Submit Request
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Deny Request",5,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","5B0B5806-F163-430B-A89D-8A16F1BC2FBF",1,"Deny","1B5EF444-4596-4152-BC5D-E73989CF0ACD"); // Background Check:Approve Request:Deny Request
            RockMigrationHelper.UpdateWorkflowActionType("342BCBFC-2CA7-426E-ABBB-A7C461A05736","Set Status",0,"96D371A7-A291-4F8F-8B38-B8F72CE5407E",true,false,"","",1,"","3339AC58-8DD6-4679-88D2-3003EF406606"); // Background Check:Approve Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType("A9CD5D05-B60B-4A19-93FF-1DE6FB5A2B3A","Set Status",0,"96D371A7-A291-4F8F-8B38-B8F72CE5407E",true,false,"","",1,"","B03F7802-1327-4D25-BC2F-2244B97BA2F9"); // Background Check:Review Denial:Set Status
            RockMigrationHelper.UpdateWorkflowActionType("A9CD5D05-B60B-4A19-93FF-1DE6FB5A2B3A","Assign to Requester",1,"F100A31F-E93A-4C7A-9E55-0FAF41A101C4",true,false,"","",1,"","54032934-011A-4400-A811-33AAED0ABB85"); // Background Check:Review Denial:Assign to Requester
            RockMigrationHelper.UpdateWorkflowActionType("A9CD5D05-B60B-4A19-93FF-1DE6FB5A2B3A","Review",2,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"91C87731-05BC-44FA-AB84-881F73EDDA20","",1,"","18DAF6A7-B404-4C52-9474-98A31CB8F788"); // Background Check:Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionType("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","Set Status",0,"96D371A7-A291-4F8F-8B38-B8F72CE5407E",true,false,"","",1,"","7C04FC25-FE24-48FB-AB30-49DE6C9D645E"); // Background Check:Review Result:Set Status
            RockMigrationHelper.UpdateWorkflowActionType("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","Update Result",3,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","",1,"","8F3B54C0-6E8D-40A4-8C44-0A8D9D573BAD"); // Background Check:Review Result:Update Result
            RockMigrationHelper.UpdateWorkflowActionType("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","Activate Complete",4,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","",1,"","82ECDA63-CAB1-4E85-96B3-E38BBDF0E593"); // Background Check:Review Result:Activate Complete
            RockMigrationHelper.UpdateWorkflowActionType("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","Review Results",2,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,false,"F59CF4C6-8AA6-4AEF-BC21-DE48FA5DA6DD","",1,"","5E132218-D054-43F3-B92F-C6FB2A77B03D"); // Background Check:Review Result:Review Results
            RockMigrationHelper.UpdateWorkflowActionType("2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F","Assign Activity",1,"DB2D8C44-6E57-4B45-8973-5DE327D61554",true,false,"","",1,"","7AF392FC-23AD-4B1A-8A43-C387B88D3EA6"); // Background Check:Review Result:Assign Activity
            RockMigrationHelper.UpdateWorkflowActionType("F95F8B4B-3ACD-4906-81F5-EBF589F87831","Notify Requester",3,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","",1,"","51EE1160-1D54-4A03-9DAD-44D78B9C0585"); // Background Check:Complete Request:Notify Requester
            RockMigrationHelper.UpdateWorkflowActionType("F95F8B4B-3ACD-4906-81F5-EBF589F87831","Complete Workflow",4,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,false,"","",1,"","57F7B0D3-2EF3-4B76-B2D5-5A00DE6A4EB5"); // Background Check:Complete Request:Complete Workflow
            RockMigrationHelper.UpdateWorkflowActionType("F95F8B4B-3ACD-4906-81F5-EBF589F87831","Update Attribute Status",0,"320622DA-52E0-41AE-AF90-2BF78B488552",true,false,"","",1,"","03849FE9-0DF7-45D0-8FC4-B4E88359396F"); // Background Check:Complete Request:Update Attribute Status
            RockMigrationHelper.UpdateWorkflowActionType("F95F8B4B-3ACD-4906-81F5-EBF589F87831","Background Check Passed",1,"320622DA-52E0-41AE-AF90-2BF78B488552",true,false,"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",1,"Pass","CBD767B5-23CF-460F-B3B6-4D848DDE95D7"); // Background Check:Complete Request:Background Check Passed
            RockMigrationHelper.UpdateWorkflowActionType("F95F8B4B-3ACD-4906-81F5-EBF589F87831","Background Check Failed",2,"320622DA-52E0-41AE-AF90-2BF78B488552",true,false,"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",1,"Fail","2C90CDF2-6F01-42AC-96BB-1C7433A30537"); // Background Check:Complete Request:Background Check Failed
            RockMigrationHelper.UpdateWorkflowActionType("4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956","Submit Request",1,"C4DAE3D6-931F-497F-AC00-60BAFA87B758",true,false,"","",1,"","5AA61E74-F2E6-43D8-8C71-D75C73067D6A"); // Background Check:Submit Request:Submit Request
            RockMigrationHelper.UpdateWorkflowActionType("4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956","Set Status",0,"96D371A7-A291-4F8F-8B38-B8F72CE5407E",true,false,"","",1,"","1772E29C-7ADB-4165-8B78-FD43219FFB61"); // Background Check:Submit Request:Set Status
            RockMigrationHelper.UpdateWorkflowActionType("4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956","Process Result",2,"38907A90-1634-4A93-8017-619326A4A582",true,true,"","A2B5C4A7-F550-4DF0-B1A0-A7DEF556D6C3",64,"","0891AE4E-0E74-4C87-8138-AFB76C9BE2FB"); // Background Check:Submit Request:Process Result
            RockMigrationHelper.AddActionTypeAttributeValue("10E8C6CB-A72C-4254-A1F5-E43B6C7B404B","91A9F4BE-4A8E-430A-B466-A88DB2D33B34",@"Initial Entry"); // Background Check:Initial Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue("10E8C6CB-A72C-4254-A1F5-E43B6C7B404B","AE8C180C-E370-414A-B10D-97891B95D105",@""); // Background Check:Initial Request:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("10E8C6CB-A72C-4254-A1F5-E43B6C7B404B","36CE41F4-4C87-4096-B0C6-8269163BCC0A",@"False"); // Background Check:Initial Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B9F9636B-CD44-48DB-BE3D-B830CDED59EB","9392E3D7-A28B-4CD8-8B03-5E147B102EF1",@"False"); // Background Check:Initial Request:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B9F9636B-CD44-48DB-BE3D-B830CDED59EB","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Initial Request:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("B9F9636B-CD44-48DB-BE3D-B830CDED59EB","AD4EFAC4-E687-43DF-832F-0DC3856ABABB",@""); // Background Check:Initial Request:Set Person:Order
            RockMigrationHelper.AddActionTypeAttributeValue("B9F9636B-CD44-48DB-BE3D-B830CDED59EB","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B",@"True"); // Background Check:Initial Request:Set Person:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue("6A779AB3-3223-411B-9AEE-87A5EE1EDF12","5D95C15A-CCAE-40AD-A9DD-F929DA587115",@""); // Background Check:Initial Request:Set Name:Order
            RockMigrationHelper.AddActionTypeAttributeValue("6A779AB3-3223-411B-9AEE-87A5EE1EDF12","0A800013-51F7-4902-885A-5BE215D67D3D",@"False"); // Background Check:Initial Request:Set Name:Active
            RockMigrationHelper.AddActionTypeAttributeValue("6A779AB3-3223-411B-9AEE-87A5EE1EDF12","93852244-A667-4749-961A-D47F88675BE4",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Initial Request:Set Name:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("3136A135-4836-4C09-BD81-326CA21C6AA5","DE9CB292-4785-4EA3-976D-3826F91E9E98",@"False"); // Background Check:Initial Request:Set Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("3136A135-4836-4C09-BD81-326CA21C6AA5","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112",@"bbd98d86-166c-4904-ad22-b58eb9b006d9"); // Background Check:Initial Request:Set Requester:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("3136A135-4836-4C09-BD81-326CA21C6AA5","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8",@""); // Background Check:Initial Request:Set Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("81D1FB3E-5017-4A53-A4EF-F6618F782935","F3B9908B-096F-460B-8320-122CF046D1F9",@"SELECT ISNULL( (
    SELECT 
        CASE WHEN DATEADD(year, 1, AV.[ValueAsDateTime]) > GETDATE() THEN 'True' ELSE 'False' END
    FROM [AttributeValue] AV
        INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
        INNER JOIN [PersonAlias] P ON P.[PersonId] = AV.[EntityId]
    WHERE AV.[ValueAsDateTime] IS NOT NULL
        AND A.[Guid] = '{{ Workflow.DateAttribute_unformatted }}'
        AND P.[Guid] = '{{ Workflow.Person_unformatted }}'
), 'False')"); // Background Check:Initial Request:Set Warning:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue("81D1FB3E-5017-4A53-A4EF-F6618F782935","A18C3143-0586-4565-9F36-E603BC674B4E",@"False"); // Background Check:Initial Request:Set Warning:Active
            RockMigrationHelper.AddActionTypeAttributeValue("81D1FB3E-5017-4A53-A4EF-F6618F782935","FA7C685D-8636-41EF-9998-90FFF3998F76",@""); // Background Check:Initial Request:Set Warning:Order
            RockMigrationHelper.AddActionTypeAttributeValue("81D1FB3E-5017-4A53-A4EF-F6618F782935","56997192-2545-4EA1-B5B2-313B04588984",@"43fb2810-dff3-40e5-9ec6-fc047a6061a1"); // Background Check:Initial Request:Set Warning:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("A3EAF2A3-97FB-47A6-9844-F7F0755FC5BE","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Background Check:Initial Request:Get Details:Active
            RockMigrationHelper.AddActionTypeAttributeValue("A3EAF2A3-97FB-47A6-9844-F7F0755FC5BE","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Background Check:Initial Request:Get Details:Order
            RockMigrationHelper.AddActionTypeAttributeValue("3339AC58-8DD6-4679-88D2-3003EF406606","91A9F4BE-4A8E-430A-B466-A88DB2D33B34",@"Waiting for Submit Approval"); // Background Check:Approve Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue("3339AC58-8DD6-4679-88D2-3003EF406606","AE8C180C-E370-414A-B10D-97891B95D105",@""); // Background Check:Approve Request:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("3339AC58-8DD6-4679-88D2-3003EF406606","36CE41F4-4C87-4096-B0C6-8269163BCC0A",@"False"); // Background Check:Approve Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("207A7FE1-BBB8-453D-8221-A968E637D92D","BBFAD050-5968-4D11-8887-2FF877D8C8AB",@"3981cf6d-7d15-4b57-aace-c0e25d28bd49|a6bcc49e-103f-46b0-8bac-84ea03ff04d5"); // Background Check:Approve Request:Assign to Security:Group
            RockMigrationHelper.AddActionTypeAttributeValue("207A7FE1-BBB8-453D-8221-A968E637D92D","041B7B51-A694-4AF5-B455-64D0DE7160A2",@""); // Background Check:Approve Request:Assign to Security:Order
            RockMigrationHelper.AddActionTypeAttributeValue("207A7FE1-BBB8-453D-8221-A968E637D92D","C0D75D1A-16C5-4786-A1E0-25669BEE8FE9",@"False"); // Background Check:Approve Request:Assign to Security:Active
            RockMigrationHelper.AddActionTypeAttributeValue("CFF91283-688F-42C4-8576-7A7DD6630A6F","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Background Check:Approve Request:Approve or Deny:Active
            RockMigrationHelper.AddActionTypeAttributeValue("CFF91283-688F-42C4-8576-7A7DD6630A6F","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Background Check:Approve Request:Approve or Deny:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C90454AC-98DD-4A7B-9C82-A9A810A13450","DE9CB292-4785-4EA3-976D-3826F91E9E98",@"False"); // Background Check:Approve Request:Set Approver:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C90454AC-98DD-4A7B-9C82-A9A810A13450","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112",@"6f334e7a-6fab-41a2-82ab-2a2b92e84014"); // Background Check:Approve Request:Set Approver:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("C90454AC-98DD-4A7B-9C82-A9A810A13450","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8",@""); // Background Check:Approve Request:Set Approver:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C8246CA7-39F7-450F-8181-D78EE436F26E","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Approve Request:Submit Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C8246CA7-39F7-450F-8181-D78EE436F26E","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Approve Request:Submit Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue("C8246CA7-39F7-450F-8181-D78EE436F26E","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"4F9B0DBB-77CD-4BB9-BF6D-2C357D64A956"); // Background Check:Approve Request:Submit Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("1B5EF444-4596-4152-BC5D-E73989CF0ACD","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Approve Request:Deny Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("1B5EF444-4596-4152-BC5D-E73989CF0ACD","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Approve Request:Deny Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue("1B5EF444-4596-4152-BC5D-E73989CF0ACD","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"A9CD5D05-B60B-4A19-93FF-1DE6FB5A2B3A"); // Background Check:Approve Request:Deny Request:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("B03F7802-1327-4D25-BC2F-2244B97BA2F9","91A9F4BE-4A8E-430A-B466-A88DB2D33B34",@"Waiting for More Details"); // Background Check:Review Denial:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue("B03F7802-1327-4D25-BC2F-2244B97BA2F9","AE8C180C-E370-414A-B10D-97891B95D105",@""); // Background Check:Review Denial:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("B03F7802-1327-4D25-BC2F-2244B97BA2F9","36CE41F4-4C87-4096-B0C6-8269163BCC0A",@"False"); // Background Check:Review Denial:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("54032934-011A-4400-A811-33AAED0ABB85","E0F7AB7E-7761-4600-A099-CB14ACDBF6EF",@"False"); // Background Check:Review Denial:Assign to Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("54032934-011A-4400-A811-33AAED0ABB85","FBADD25F-D309-4512-8430-3CC8615DD60E",@"bbd98d86-166c-4904-ad22-b58eb9b006d9"); // Background Check:Review Denial:Assign to Requester:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("54032934-011A-4400-A811-33AAED0ABB85","7A6B605D-7FB1-4F48-AF35-5A0683FB1CDA",@""); // Background Check:Review Denial:Assign to Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("18DAF6A7-B404-4C52-9474-98A31CB8F788","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Background Check:Review Denial:Review:Active
            RockMigrationHelper.AddActionTypeAttributeValue("18DAF6A7-B404-4C52-9474-98A31CB8F788","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Background Check:Review Denial:Review:Order
            RockMigrationHelper.AddActionTypeAttributeValue("1772E29C-7ADB-4165-8B78-FD43219FFB61","91A9F4BE-4A8E-430A-B466-A88DB2D33B34",@"Waiting for Result"); // Background Check:Submit Request:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue("1772E29C-7ADB-4165-8B78-FD43219FFB61","AE8C180C-E370-414A-B10D-97891B95D105",@""); // Background Check:Submit Request:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("1772E29C-7ADB-4165-8B78-FD43219FFB61","36CE41F4-4C87-4096-B0C6-8269163BCC0A",@"False"); // Background Check:Submit Request:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","6E2366B4-9F0E-454A-9DB1-E06263749C12",@"c16856f4-3c6b-4afb-a0b8-88a303508206"); // Background Check:Submit Request:Submit Request:Background Check Provider
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","3936E931-CC27-4C38-9AA5-AAA502057333",@""); // Background Check:Submit Request:Submit Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","6BEBD4BE-EDC7-4757-B597-445FC60DB6ED",@"False"); // Background Check:Submit Request:Submit Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Submit Request:Submit Request:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","2631E72B-1D9B-40E8-B857-8B1D41943451",@"1fab9a4c-c5a2-4938-b9bd-80935f0a598c"); // Background Check:Submit Request:Submit Request:SSN Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","EC759165-949E-4966-BAFD-68A656A4EBF7",@"a4cb9461-d77f-40e0-8dff-c7838d78f2ec"); // Background Check:Submit Request:Submit Request:Request Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("5AA61E74-F2E6-43D8-8C71-D75C73067D6A","232B2F98-3B2F-4C53-81FC-061A92675C41",@"a5255a04-4832-45c3-a727-00395d082d23"); // Background Check:Submit Request:Submit Request:Billing Code Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("0891AE4E-0E74-4C87-8138-AFB76C9BE2FB","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Submit Request:Process Result:Active
            RockMigrationHelper.AddActionTypeAttributeValue("0891AE4E-0E74-4C87-8138-AFB76C9BE2FB","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Submit Request:Process Result:Order
            RockMigrationHelper.AddActionTypeAttributeValue("0891AE4E-0E74-4C87-8138-AFB76C9BE2FB","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"0840A659-3935-4DD2-AC5D-1D022ABFB45C"); // Background Check:Submit Request:Process Result:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("A8AADFA0-45F6-4DDF-A549-D29D5F424EB3","E456FB6F-05DB-4826-A612-5B704BC4EA13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Process Result:Save Date:Person
            RockMigrationHelper.AddActionTypeAttributeValue("A8AADFA0-45F6-4DDF-A549-D29D5F424EB3","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B",@""); // Background Check:Process Result:Save Date:Order
            RockMigrationHelper.AddActionTypeAttributeValue("A8AADFA0-45F6-4DDF-A549-D29D5F424EB3","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184",@"False"); // Background Check:Process Result:Save Date:Active
            RockMigrationHelper.AddActionTypeAttributeValue("A8AADFA0-45F6-4DDF-A549-D29D5F424EB3","8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762",@"3daff000-7f74-47d7-8cb0-e4a4e6c81f5f"); // Background Check:Process Result:Save Date:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("A8AADFA0-45F6-4DDF-A549-D29D5F424EB3","94689BDE-493E-4869-A614-2D54822D747C",@"{{ 'Now' | Date:'yyyy-MM-dd' }}T00:00:00"); // Background Check:Process Result:Save Date:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("933E6BF0-D87B-417F-B574-66D50EBD0F90","E456FB6F-05DB-4826-A612-5B704BC4EA13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Process Result:Save Report:Person
            RockMigrationHelper.AddActionTypeAttributeValue("933E6BF0-D87B-417F-B574-66D50EBD0F90","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B",@""); // Background Check:Process Result:Save Report:Order
            RockMigrationHelper.AddActionTypeAttributeValue("933E6BF0-D87B-417F-B574-66D50EBD0F90","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184",@"False"); // Background Check:Process Result:Save Report:Active
            RockMigrationHelper.AddActionTypeAttributeValue("933E6BF0-D87B-417F-B574-66D50EBD0F90","8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762",@"f3931952-460d-43e0-a6e0-eb6b5b1f9167"); // Background Check:Process Result:Save Report:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("933E6BF0-D87B-417F-B574-66D50EBD0F90","94689BDE-493E-4869-A614-2D54822D747C",@"e6e5cf21-5a49-4630-9e18-531ff354380e"); // Background Check:Process Result:Save Report:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("77C9D555-9141-418D-91AB-FF71AE5AA778","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Process Result:Activate Review:Active
            RockMigrationHelper.AddActionTypeAttributeValue("77C9D555-9141-418D-91AB-FF71AE5AA778","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Process Result:Activate Review:Order
            RockMigrationHelper.AddActionTypeAttributeValue("77C9D555-9141-418D-91AB-FF71AE5AA778","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"2BE84594-D7F5-4D9D-BD25-D9E4EE51E63F"); // Background Check:Process Result:Activate Review:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("B982C744-0965-4FE8-ACB2-08FE8AD467FF","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Process Result:Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B982C744-0965-4FE8-ACB2-08FE8AD467FF","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Process Result:Activate Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue("B982C744-0965-4FE8-ACB2-08FE8AD467FF","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"F95F8B4B-3ACD-4906-81F5-EBF589F87831"); // Background Check:Process Result:Activate Complete:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("7C04FC25-FE24-48FB-AB30-49DE6C9D645E","91A9F4BE-4A8E-430A-B466-A88DB2D33B34",@"Waiting for Review"); // Background Check:Review Result:Set Status:Status
            RockMigrationHelper.AddActionTypeAttributeValue("7C04FC25-FE24-48FB-AB30-49DE6C9D645E","AE8C180C-E370-414A-B10D-97891B95D105",@""); // Background Check:Review Result:Set Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("7C04FC25-FE24-48FB-AB30-49DE6C9D645E","36CE41F4-4C87-4096-B0C6-8269163BCC0A",@"False"); // Background Check:Review Result:Set Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("7AF392FC-23AD-4B1A-8A43-C387B88D3EA6","BBFAD050-5968-4D11-8887-2FF877D8C8AB",@"3981cf6d-7d15-4b57-aace-c0e25d28bd49|a6bcc49e-103f-46b0-8bac-84ea03ff04d5"); // Background Check:Review Result:Assign Activity:Group
            RockMigrationHelper.AddActionTypeAttributeValue("7AF392FC-23AD-4B1A-8A43-C387B88D3EA6","041B7B51-A694-4AF5-B455-64D0DE7160A2",@""); // Background Check:Review Result:Assign Activity:Order
            RockMigrationHelper.AddActionTypeAttributeValue("7AF392FC-23AD-4B1A-8A43-C387B88D3EA6","C0D75D1A-16C5-4786-A1E0-25669BEE8FE9",@"False"); // Background Check:Review Result:Assign Activity:Active
            RockMigrationHelper.AddActionTypeAttributeValue("5E132218-D054-43F3-B92F-C6FB2A77B03D","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Background Check:Review Result:Review Results:Active
            RockMigrationHelper.AddActionTypeAttributeValue("5E132218-D054-43F3-B92F-C6FB2A77B03D","C178113D-7C86-4229-8424-C6D0CF4A7E23",@""); // Background Check:Review Result:Review Results:Order
            RockMigrationHelper.AddActionTypeAttributeValue("8F3B54C0-6E8D-40A4-8C44-0A8D9D573BAD","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Background Check:Review Result:Update Result:Active
            RockMigrationHelper.AddActionTypeAttributeValue("8F3B54C0-6E8D-40A4-8C44-0A8D9D573BAD","44A0B977-4730-4519-8FF6-B0A01A95B212",@"a2b5c4a7-f550-4df0-b1a0-a7def556d6c3"); // Background Check:Review Result:Update Result:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("8F3B54C0-6E8D-40A4-8C44-0A8D9D573BAD","57093B41-50ED-48E5-B72B-8829E62704C8",@""); // Background Check:Review Result:Update Result:Order
            RockMigrationHelper.AddActionTypeAttributeValue("8F3B54C0-6E8D-40A4-8C44-0A8D9D573BAD","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"e0166ca1-200e-4506-a3e7-b10ff0471c90"); // Background Check:Review Result:Update Result:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("82ECDA63-CAB1-4E85-96B3-E38BBDF0E593","E8ABD802-372C-47BE-82B1-96F50DB5169E",@"False"); // Background Check:Review Result:Activate Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue("82ECDA63-CAB1-4E85-96B3-E38BBDF0E593","3809A78C-B773-440C-8E3F-A8E81D0DAE08",@""); // Background Check:Review Result:Activate Complete:Order
            RockMigrationHelper.AddActionTypeAttributeValue("82ECDA63-CAB1-4E85-96B3-E38BBDF0E593","02D5A7A5-8781-46B4-B9FC-AF816829D240",@"F95F8B4B-3ACD-4906-81F5-EBF589F87831"); // Background Check:Review Result:Activate Complete:Activity
            RockMigrationHelper.AddActionTypeAttributeValue("03849FE9-0DF7-45D0-8FC4-B4E88359396F","E456FB6F-05DB-4826-A612-5B704BC4EA13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Complete Request:Update Attribute Status:Person
            RockMigrationHelper.AddActionTypeAttributeValue("03849FE9-0DF7-45D0-8FC4-B4E88359396F","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B",@""); // Background Check:Complete Request:Update Attribute Status:Order
            RockMigrationHelper.AddActionTypeAttributeValue("03849FE9-0DF7-45D0-8FC4-B4E88359396F","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184",@"False"); // Background Check:Complete Request:Update Attribute Status:Active
            RockMigrationHelper.AddActionTypeAttributeValue("03849FE9-0DF7-45D0-8FC4-B4E88359396F","8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762",@"44490089-e02c-4e54-a456-454845abbc9d"); // Background Check:Complete Request:Update Attribute Status:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("03849FE9-0DF7-45D0-8FC4-B4E88359396F","94689BDE-493E-4869-A614-2D54822D747C",@"a2b5c4a7-f550-4df0-b1a0-a7def556d6c3"); // Background Check:Complete Request:Update Attribute Status:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("CBD767B5-23CF-460F-B3B6-4D848DDE95D7","E456FB6F-05DB-4826-A612-5B704BC4EA13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Complete Request:Background Check Passed:Person
            RockMigrationHelper.AddActionTypeAttributeValue("CBD767B5-23CF-460F-B3B6-4D848DDE95D7","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B",@""); // Background Check:Complete Request:Background Check Passed:Order
            RockMigrationHelper.AddActionTypeAttributeValue("CBD767B5-23CF-460F-B3B6-4D848DDE95D7","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184",@"False"); // Background Check:Complete Request:Background Check Passed:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "CBD767B5-23CF-460F-B3B6-4D848DDE95D7", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"DAF87B87-3D1E-463D-A197-52227FE4EA28" ); // Background Check:Complete Request:Background Check Passed:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("CBD767B5-23CF-460F-B3B6-4D848DDE95D7","94689BDE-493E-4869-A614-2D54822D747C",@"True"); // Background Check:Complete Request:Background Check Passed:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("2C90CDF2-6F01-42AC-96BB-1C7433A30537","E456FB6F-05DB-4826-A612-5B704BC4EA13",@"af3f0233-9786-422d-83c8-a7565d99a01d"); // Background Check:Complete Request:Background Check Failed:Person
            RockMigrationHelper.AddActionTypeAttributeValue("2C90CDF2-6F01-42AC-96BB-1C7433A30537","3F3BF3E6-AD53-491E-A40F-441F2AFCBB5B",@""); // Background Check:Complete Request:Background Check Failed:Order
            RockMigrationHelper.AddActionTypeAttributeValue("2C90CDF2-6F01-42AC-96BB-1C7433A30537","E5BAC4A6-FF7F-4016-BA9C-72D16CB60184",@"False"); // Background Check:Complete Request:Background Check Failed:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "2C90CDF2-6F01-42AC-96BB-1C7433A30537", "8F4BB00F-7FA2-41AD-8E90-81F4DFE2C762", @"DAF87B87-3D1E-463D-A197-52227FE4EA28" ); // Background Check:Complete Request:Background Check Failed:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("2C90CDF2-6F01-42AC-96BB-1C7433A30537","94689BDE-493E-4869-A614-2D54822D747C",@"False"); // Background Check:Complete Request:Background Check Failed:Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC",@""); // Background Check:Complete Request:Notify Requester:From Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Background Check:Complete Request:Notify Requester:Active
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","D1269254-C15A-40BD-B784-ADCC231D3950",@""); // Background Check:Complete Request:Notify Requester:Order
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","0C4C13B8-7076-4872-925A-F950886B5E16",@"bbd98d86-166c-4904-ad22-b58eb9b006d9"); // Background Check:Complete Request:Notify Requester:Send To Email Address|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"Background Check for {{ Workflow.Person }}"); // Background Check:Complete Request:Notify Requester:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("51EE1160-1D54-4A03-9DAD-44D78B9C0585","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The background check for {{ Workflow.Person }} has been completed.</p>
<p>Result: {{ Workflow.ReportStatus | Upcase }}<p/>

{{ GlobalAttribute.EmailFooter }}"); // Background Check:Complete Request:Notify Requester:Body
            RockMigrationHelper.AddActionTypeAttributeValue("57F7B0D3-2EF3-4B76-B2D5-5A00DE6A4EB5","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Background Check:Complete Request:Complete Workflow:Active
            RockMigrationHelper.AddActionTypeAttributeValue("57F7B0D3-2EF3-4B76-B2D5-5A00DE6A4EB5","25CAD4BE-5A00-409D-9BAB-E32518D89956",@""); // Background Check:Complete Request:Complete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue("078B2C46-E1D1-4C49-ACEF-86A488AA42BA","79D23F8B-0DC8-4B48-8A86-AEA48B396C82",@""); // Background Check:Cancel Request:Delete Workflow:Order
            RockMigrationHelper.AddActionTypeAttributeValue("078B2C46-E1D1-4C49-ACEF-86A488AA42BA","361A1EC8-FFD0-4880-AF68-91DC0E0D7CDC",@"False"); // Background Check:Cancel Request:Delete Workflow:Active

            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "16D12EF7-C546-4039-9036-B73D118EDC90", appendToExisting: true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Attribute Values:Attribute Order
            RockMigrationHelper.DeleteAttribute( "235C6D48-E1D1-410C-8006-1EA412BC12EF" );
            // Remove Block: Attribute Values, from Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "8DA21ED3-E4BC-483C-8B22-A041FEEE8FF4" );

            // Remove Block: Components, from Page: Background Check Providers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "217A55AA-9E19-4B3C-9774-9678E330DD5C" );
            //  Page: Background Check Providers, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "53F1B7D9-806A-4541-93BC-4CCF5DFF90B3" );

            // Delete the security role and application group used by background check workflow
            RockMigrationHelper.DeleteGroup( "A6BCC49E-103F-46B0-8BAC-84EA03FF04D5" );
            RockMigrationHelper.DeleteSecurityRoleGroup( "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB" );

            // Delete the background check person attributes/category
            RockMigrationHelper.DeleteAttribute( "DAF87B87-3D1E-463D-A197-52227FE4EA28" );
            RockMigrationHelper.DeleteAttribute( "F3931952-460D-43E0-A6E0-EB6B5B1F9167" );
            RockMigrationHelper.DeleteAttribute( "44490089-E02C-4E54-A456-454845ABBC9D" );
            RockMigrationHelper.DeleteAttribute( "3DAFF000-7F74-47D7-8CB0-E4A4E6C81F5F" );
            RockMigrationHelper.DeleteCategory( "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2" );

            // Update the BinaryFileType qualifier on Binary file attributes to use Id instead of Guid 
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(BFT.[Id] AS VARCHAR)
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [BinaryFileType] BFT ON CAST(BFT.[Guid] AS varchar(50)) = AQ.[Value]
" );

            // Update the EntityType qualifier on Attribute attributes to use Id instead of Guid 
            Sql( @"
    UPDATE AQ
    SET [Value] = CAST(ET.[Id] AS VARCHAR)
    FROM [Attribute] A
    INNER JOIN [FieldType] FT ON FT.[Id] = A.[FieldTypeId] AND FT.[Guid] = 'C403E219-A56B-439E-9D50-9302DFE760CF'
    INNER JOIN [AttributeQualifier] AQ ON AQ.[AttributeId] = A.[Id] AND AQ.[Key] = 'binaryFileType'
    INNER JOIN [EntityType] ET ON CAST(ET.[Guid] AS varchar(50)) = AQ.[Value]
" );

        }

        private void RestrictAttributeToSecurityRole ( string attributeGuid, string action )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 0, action, true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 1, action, true, "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB", 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 2, action, false, null, 1, Guid.NewGuid().ToString() );
        }

        private void RestrictCategoryToSecurityRole( string categoryGuid, string action )
        {
            RockMigrationHelper.AddSecurityAuthForCategory( categoryGuid, 0, action, true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForCategory( categoryGuid, 1, action, true, "32E80B6C-A1EB-40FD-BEC3-E11DE8FF75AB", 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForCategory( categoryGuid, 2, action, false, null, 1, Guid.NewGuid().ToString() );
        }
    }
}
