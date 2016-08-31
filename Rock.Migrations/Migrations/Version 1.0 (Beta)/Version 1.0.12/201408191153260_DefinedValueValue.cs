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
    public partial class DefinedValueValue : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            #region Update Context attributes

            // Site Context Setter
            RockMigrationHelper.AddBlockTypeAttribute( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Context Scope", "ContextScope", "", "The scope of context to set", 0, "Site", "A1F4E771-8A8C-46E6-9DDD-1603590B67E8", true );
            RockMigrationHelper.AddAttributeQualifier( "A1F4E771-8A8C-46E6-9DDD-1603590B67E8", "values", "Site,Page", "841B71F5-052D-4D0D-BDC1-78FDED2227D4" );
            RockMigrationHelper.AddAttributeQualifier( "A1F4E771-8A8C-46E6-9DDD-1603590B67E8", "fieldtype", "rb", "9D2B777B-BE4C-4C03-A170-26AB7D3B1AF8" );
            RockMigrationHelper.AddBlockAttributeValue( "8B940F43-C38A-4086-80D8-7C33961518E3", "A1F4E771-8A8C-46E6-9DDD-1603590B67E8", "Site" );

            // Group Context Setter
            RockMigrationHelper.AddBlockTypeAttribute( "62F749F7-67DF-4A84-B7DD-84CA8E10E205", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Context Scope", "ContextScope", "", "The scope of context to set", 0, "Site", "DE231BEB-5F3F-43DB-8A14-6F339462335F", true );
            RockMigrationHelper.AddAttributeQualifier( "DE231BEB-5F3F-43DB-8A14-6F339462335F", "values", "Site,Page", "891A143C-4F59-467A-9684-F4775C8FC6AA" );
            RockMigrationHelper.AddAttributeQualifier( "DE231BEB-5F3F-43DB-8A14-6F339462335F", "fieldtype", "rb", "057E56B1-ACF6-4A9A-A389-C597A083079F" );

            #endregion

            #region Update to "Actions" setting on person bio block

            RockMigrationHelper.UpdateFieldType( "Workflow Types", "Field Type used to display a workflow type picker", "Rock", "Rock.Field.Types.WorkflowTypesFieldType", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "", "The label badges to display in this block.", 0, "", "8E11F65B-7272-4E9F-A4F1-89CE08E658DE" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1D50399B-0D6E-480B-A71A-E7BD37DD83F0", "Workflow Actions", "WorkflowActions", "", "The workflows to make available as actions.", 1, "", "7197A0FB-B330-43C4-8E62-F3C14F649813" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Additional Custom Actions", "Actions", "", @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/LaunchWorkflow/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>
", 2, "", "35F69669-48DE-4182-B828-4EC9C1C31B08" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Business Detail Page", "BusinessDetailPage", "", "The page to redirect user to if a business is is requested.", 3, "", "509F3D63-6218-49BD-B0A9-CE49B6FCB2CF" );
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "35F69669-48DE-4182-B828-4EC9C1C31B08", "" );
            RockMigrationHelper.AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "7197A0FB-B330-43C4-8E62-F3C14F649813", "221bf486-a82c-40a7-85b7-bb44da45582f" );

            #endregion

            #region Rename DefinedValue Name column

            RenameColumn( "dbo.DefinedValue", "Name", "Value" );

            RockMigrationHelper.UpdateBlockTypeAttribute( "967F0D2B-DB76-486A-B034-D22B9D9240D3", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 6, @"
<div style='width:250px'>

    <div class='clearfix'>
        <h4 class='pull-left' style='margin-top: 0;'>{{ GroupName }}</h4> 
        <span class='label label-campus pull-right'>{{ Campus.Name }}</span>
    </div>
    
    <div class='clearfix'>
		{% if Location.Street1 and Location.Street1 != '' %}
			<strong>{{ Location.Type }}</strong>
			<br>{{ Location.Street1 }}
			<br>{{ Location.City }}, {{ Location.State }} {{ Location.Zip }}
		{% endif %}
		{% if Members.size > 0 %}
			<br>
			<br><strong>{{ GroupType.GroupMemberTerm }}s</strong><br>
			{% for GroupMember in Members -%}
				<div class='clearfix'>
					{% if GroupMember.PhotoUrl != '' %}
						<div class='pull-left' style='padding: 0 5px 2px 0'>
							<img src='{{ GroupMember.PhotoUrl }}&maxheight=50&maxwidth=50'>
						</div>
					{% endif %}
					<a href='{{ GroupMember.ProfilePageUrl }}'>{{ GroupMember.NickName }} {{ GroupMember.LastName }}</a> - {{ GroupMember.Role }}
                    {% if groupTypeGuid != '790E3215-3B10-442B-AF69-616C0DCB998E' and GroupMember.ConnectionStatus != '' %}
				        <br>{{ GroupMember.ConnectionStatus }}
					{% endif %}
					{% if GroupMember.Email != '' %}
						<br>{{ GroupMember.Email }}
					{% endif %}
					{% for Phone in GroupMember.Person.PhoneNumbers %}
						<br>{{ Phone.NumberTypeValue.Value }}: {{ Phone.NumberFormatted }}
					{% endfor %}
				</div>
				<br>
			{% endfor -%}
		{% endif %}
    </div>
    
    {% if GroupType.Guid != '790E3215-3B10-442B-AF69-616C0DCB998E' %}
		<br>
		<a class='btn btn-xs btn-action' href='{{ DetailPageUrl }}'>View {{ GroupType.GroupTerm }}</a>
		<a class='btn btn-xs btn-action' href='{{ MapPageUrl }}'>View Map</a>
	{% endif %}

</div>
", "92B339D5-D8AF-4810-A7F8-09373DC5D0DE" );

            Sql( @"
                DECLARE @LiquidAttributeId int = (SELECT [ID] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3' )
                UPDATE [AttributeValue] 
                    SET [Value] = REPLACE([Value], 'Value.Name', 'Value.Value')
                WHERE [AttributeId] = @LiquidAttributeId
" );

            #endregion

            #region Remove Barcode checkin search type

            RockMigrationHelper.DeleteDefinedValue( "9A66BFCD-0F16-4EAE-BE35-B3FAF4B817BE" );

            #endregion

            #region Migration Rollups

            // add page icon to communication template detail page
            Sql( @"UPDATE [Page]
  SET [IconCssClass] = 'fa fa-envelope'
  WHERE [Guid] = '588c72a8-7dec-405f-ba4a-fe64f87cb817'" );

            // add two admin checklist items
            Sql( @"

INSERT INTO [DefinedValue] 
	(IsSystem
	,DefinedTypeId
	,[Order]
	,[Value]
	,[Guid]
	,[Description])
VALUES
	(0
	,35
	,4
	,'Update Email Templates'
	,'9fe1d521-a810-4fbb-8f52-6efb43fdbc70'
	,'<p>Update the default email template under: <span class=""navigation-tip"">Admin Tools &gt; Communication Settings &gt; Communication Templates &gt; Default Organization Template</span>.</p>
    <p>You''ll also want to update the default email header and footer under: <span class=""navigation-tip"">Admin Tools &gt; General Settings &gt; Global Attributes &gt; Email Header/Footer</span>.</p> 
  ')

  INSERT INTO [DefinedValue] 
	(IsSystem
	,DefinedTypeId
	,[Order]
	,[Value]
	,[Guid]
	,[Description])
VALUES
	(0
	,35
	,4
	,'Update System Workflows'
	,'e5b3bc7c-e1d2-406a-b05e-c74b0b3edac9'
	,'<p>Rock ships with several default workflows (e.g. ''Report Data Error'', ''IT Requests'', ''Facility Requests''). These workflows should be configured to meet the needs of your organization (proper assignees configured.)</p> 
  ')



" );

            // update the default system template
            Sql( @"
UPDATE [CommunicationTemplate]
  SET [ChannelDataJson] = '{
  ""HtmlMessage"": ""<meta content=\""text/html; charset=utf-8\"" http-equiv=\""Content-Type\"" />\n<meta content=\""width=device-width\"" name=\""viewport\"" />\n<style type=\""text/css\"">td, h1, h2, h3, h4, h5, h6, p, a {\n    font-family: ''Helvetica'', ''Arial'', sans-serif; \n    line-height: 1.2; \n}\n\nh1, h2, h3, h4 {\n    color: #777;\n}\n\np {\n    color: #777;\n}\n\nem {\n    color: #777;\n}\n\na {\n    color: #2ba6cb; \n    text-decoration: none;\n}\n\n.btn a {\n    line-height: 1;\n    margin: 0;\n    padding: 0;\n}\n\na:hover {\ncolor: #2795b6 !important;\n}\na:active {\ncolor: #2795b6 !important;\n}\na:visited {\ncolor: #2ba6cb !important;\n}\nh1 a:active {\ncolor: #2ba6cb !important;\n}\nh2 a:active {\ncolor: #2ba6cb !important;\n}\nh3 a:active {\ncolor: #2ba6cb !important;\n}\nh4 a:active {\ncolor: #2ba6cb !important;\n}\nh5 a:active {\ncolor: #2ba6cb !important;\n}\nh6 a:active {\ncolor: #2ba6cb !important;\n}\nh1 a:visited {\ncolor: #2ba6cb !important;\n}\nh2 a:visited {\ncolor: #2ba6cb !important;\n}\nh3 a:visited {\ncolor: #2ba6cb !important;\n}\nh4 a:visited {\ncolor: #2ba6cb !important;\n}\nh5 a:visited {\ncolor: #2ba6cb !important;\n}\nh6 a:visited {\ncolor: #2ba6cb !important;\n}\ntable.button:hover td {\nbackground: #2795b6 !important;\n}\ntable.button:visited td {\nbackground: #2795b6 !important;\n}\ntable.button:active td {\nbackground: #2795b6 !important;\n}\ntable.button:hover td a {\ncolor: #fff !important;\n}\ntable.button:visited td a {\ncolor: #fff !important;\n}\ntable.button:active td a {\ncolor: #fff !important;\n}\ntable.button:hover td {\nbackground: #2795b6 !important;\n}\ntable.tiny-button:hover td {\nbackground: #2795b6 !important;\n}\ntable.small-button:hover td {\nbackground: #2795b6 !important;\n}\ntable.medium-button:hover td {\nbackground: #2795b6 !important;\n}\ntable.large-button:hover td {\nbackground: #2795b6 !important;\n}\ntable.button:hover td a {\ncolor: #ffffff !important;\n}\ntable.button:active td a {\ncolor: #ffffff !important;\n}\ntable.button td a:visited {\ncolor: #ffffff !important;\n}\ntable.tiny-button:hover td a {\ncolor: #ffffff !important;\n}\ntable.tiny-button:active td a {\ncolor: #ffffff !important;\n}\ntable.tiny-button td a:visited {\ncolor: #ffffff !important;\n}\ntable.small-button:hover td a {\ncolor: #ffffff !important;\n}\ntable.small-button:active td a {\ncolor: #ffffff !important;\n}\ntable.small-button td a:visited {\ncolor: #ffffff !important;\n}\ntable.medium-button:hover td a {\ncolor: #ffffff !important;\n}\ntable.medium-button:active td a {\ncolor: #ffffff !important;\n}\ntable.medium-button td a:visited {\ncolor: #ffffff !important;\n}\ntable.large-button:hover td a {\ncolor: #ffffff !important;\n}\ntable.large-button:active td a {\ncolor: #ffffff !important;\n}\ntable.large-button td a:visited {\ncolor: #ffffff !important;\n}\ntable.secondary:hover td {\nbackground: #d0d0d0 !important; color: #555;\n}\ntable.secondary:hover td a {\ncolor: #555 !important;\n}\ntable.secondary td a:visited {\ncolor: #555 !important;\n}\ntable.secondary:active td a {\ncolor: #555 !important;\n}\ntable.success:hover td {\nbackground: #457a1a !important;\n}\ntable.alert:hover td {\nbackground: #970b0e !important;\n}\ntable.facebook:hover td {\nbackground: #2d4473 !important;\n}\ntable.twitter:hover td {\nbackground: #0087bb !important;\n}\ntable.google-plus:hover td {\nbackground: #CC0000 !important;\n}\n@media only screen and (max-width: 600px) {\n  table[class=\""body\""] img {\n    width: auto !important; height: auto !important;\n  }\n  table[class=\""body\""] center {\n    min-width: 0 !important;\n  }\n  table[class=\""body\""] .container {\n    width: 95% !important;\n  }\n  table[class=\""body\""] .row {\n    width: 100% !important; display: block !important;\n  }\n  table[class=\""body\""] .wrapper {\n    display: block !important; padding-right: 0 !important;\n  }\n  table[class=\""body\""] .columns {\n    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;\n  }\n  table[class=\""body\""] .column {\n    table-layout: fixed !important; float: none !important; width: 100% !important; padding-right: 0px !important; padding-left: 0px !important; display: block !important;\n  }\n  table[class=\""body\""] .wrapper.first .columns {\n    display: table !important;\n  }\n  table[class=\""body\""] .wrapper.first .column {\n    display: table !important;\n  }\n  table[class=\""body\""] table.columns td {\n    width: 100% !important;\n  }\n  table[class=\""body\""] table.column td {\n    width: 100% !important;\n  }\n  table[class=\""body\""] td.offset-by-one {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-two {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-three {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-four {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-five {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-six {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-seven {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-eight {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-nine {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-ten {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] td.offset-by-eleven {\n    padding-left: 0 !important;\n  }\n  table[class=\""body\""] .expander {\n    width: 9999px !important;\n  }\n  table[class=\""body\""] .right-text-pad {\n    padding-left: 10px !important;\n  }\n  table[class=\""body\""] .text-pad-right {\n    padding-left: 10px !important;\n  }\n  table[class=\""body\""] .left-text-pad {\n    padding-right: 10px !important;\n  }\n  table[class=\""body\""] .text-pad-left {\n    padding-right: 10px !important;\n  }\n  table[class=\""body\""] .hide-for-small {\n    display: none !important;\n  }\n  table[class=\""body\""] .show-for-desktop {\n    display: none !important;\n  }\n  table[class=\""body\""] .show-for-small {\n    display: inherit !important;\n  }\n  table[class=\""body\""] .hide-for-desktop {\n    display: inherit !important;\n  }\n  table[class=\""body\""] .right-text-pad {\n    padding-left: 10px !important;\n  }\n  table[class=\""body\""] .left-text-pad {\n    padding-right: 10px !important;\n  }\n}\n</style>\n<table class=\""body\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; height: 100%; width: 100%; color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\"">\n\t<tbody>\n\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t<td align=\""center\"" class=\""center\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;\"" valign=\""top\"">\n\t\t\t<center style=\""width: 100%; min-width: 580px;\"">\n\t\t\t<table bgcolor=\""#5e5e5e\"" class=\""row header\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; background: #5e5e5e; padding: 0px;\"">\n\t\t\t\t<tbody>\n\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t<td align=\""center\"" class=\""center\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: center; padding: 0;\"" valign=\""top\"">\n\t\t\t\t\t\t<center style=\""width: 100%; min-width: 580px;\"">\n\t\t\t\t\t\t<table class=\""container\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""wrapper last\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t<table class=\""twelve columns\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""six sub-columns\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; min-width: 0px; width: 50% !important; padding: 0px 10px 10px 0px;\"" valign=\""top\""><img align=\""left\"" src=\""/assets/images/email-header.jpg\"" style=\""outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; width: auto; max-width: 100%; float: left; clear: both; display: block;\"" /></td>\n\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""right\"" class=\""six sub-columns last\"" style=\""text-align: right; vertical-align: middle; word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; min-width: 0px; width: 50% !important; padding: 0px 0px 10px;\"" valign=\""middle\"">&nbsp;</td>\n\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""expander\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;\"" valign=\""top\"">&nbsp;</td>\n\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t</table>\n\t\t\t\t\t\t</center>\n\t\t\t\t\t\t</td>\n\t\t\t\t\t</tr>\n\t\t\t\t</tbody>\n\t\t\t</table>\n\n\t\t\t<table class=\""container\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: inherit; width: 580px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t<tbody>\n\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t<td align=\""left\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0;\"" valign=\""top\"">\n\t\t\t\t\t\t<table class=\""row\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;\"">\n\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""wrapper last\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t<table class=\""twelve columns\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t<p align=\""left\"" class=\""lead\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;\"">&nbsp;</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<p>{{ Person.NickName }},</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<p>--&gt; Insert Your Communication Text Here &lt;--</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<p>{{ Communication.ChannelData.FromName }}<br />\n\t\t\t\t\t\t\t\t\t\t\t\t<a href=\""mailto:{{ Communication.ChannelData.FromAddress }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ Communication.ChannelData.FromAddress }}</a></p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<p>&nbsp;</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<table class=\""row footer\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" bgcolor=\""#ebebeb\"" class=\""wrapper\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 20px 0px 0px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<table class=\""six columns\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""left-text-pad\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px 10px;\"" valign=\""top\""><!-- recommend social network links here --></td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""expander\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;\"" valign=\""top\"">&nbsp;</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" bgcolor=\""#ebebeb\"" class=\""wrapper last\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; background: #ebebeb; padding: 10px 0px 0px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<table class=\""six columns\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 280px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""last right-text-pad\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<h5 align=\""left\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 1.3; word-break: normal; font-size: 24px; margin: 0; padding: 0 0 10px;\"">Contact Info:</h5>\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<p align=\""left\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;\"">{{ GlobalAttribute.OrganizationAddress }}</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<p align=\""left\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 12px 0 0 0; padding: 0 0 10px;\"">{{ GlobalAttribute.OrganizationPhone }}</p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<p align=\""left\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;\""><a href=\""mailto:{{ GlobalAttribute.OrganizationEmail }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ GlobalAttribute.OrganizationEmail }}</a></p>\n\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<p align=\""left\"" style=\""color: #222222; font-family: ''Helvetica'', ''Arial'', sans-serif; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px; margin: 0; padding: 0 0 10px;\""><a href=\""{{ GlobalAttribute.PublicApplicationRoot }}\"" style=\""color: #2ba6cb; text-decoration: none;\"">{{ GlobalAttribute.OrganizationWebsite }}</a></p>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""expander\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;\"" valign=\""top\"">&nbsp;</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t</table>\n\n\t\t\t\t\t\t\t\t\t\t\t\t<table class=\""row\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 100%; position: relative; display: block; padding: 0px;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""wrapper last\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; position: relative; padding: 10px 0px 0px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<table class=\""twelve columns\"" style=\""border-spacing: 0; border-collapse: collapse; vertical-align: top; text-align: left; width: 580px; margin: 0 auto; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<tr align=\""left\"" style=\""vertical-align: top; text-align: left; padding: 0;\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""center\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; padding: 0px 0px 10px;\"" valign=\""top\"">\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<center style=\""width: 100%; min-width: 580px;\""><!-- recommend privacy - terms - unsubscribe here -->[[ UnsubscribeOption ]]</center>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t<td align=\""left\"" class=\""expander\"" style=\""word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; padding: 0;\"" valign=\""top\"">&nbsp;</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t\t\t\t<!-- container end below --></td>\n\t\t\t\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t\t\t\t</table>\n\t\t\t\t\t\t\t\t\t</td>\n\t\t\t\t\t\t\t\t</tr>\n\t\t\t\t\t\t\t</tbody>\n\t\t\t\t\t\t</table>\n\t\t\t\t\t\t</td>\n\t\t\t\t\t</tr>\n\t\t\t\t</tbody>\n\t\t\t</table>\n\t\t\t</center>\n\t\t\t</td>\n\t\t</tr>\n\t</tbody>\n</table>\n""
}'
WHERE [ModifiedDateTime] IS NULL
  AND [Guid] = 'afe2add1-5278-441e-8e84-1dc743d99824'
" );
            #endregion
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameColumn( "dbo.DefinedValue", "Value", "Name" );
        }
    }
}
