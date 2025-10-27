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
    public partial class CodeGenerated_20250930 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Administration.ExternalApplicationList
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Administration.ExternalApplicationList", "External Application List", "Rock.Blocks.Administration.ExternalApplicationList, Rock.Blocks, Version=18.0.11.0, Culture=neutral, PublicKeyToken=null", false, false, "0EB5D32A-D26F-4B4F-A336-D7206539895D" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Cms.EmailForm
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Cms.EmailForm", "Email Form", "Rock.Blocks.Cms.EmailForm, Rock.Blocks, Version=18.0.11.0, Culture=neutral, PublicKeyToken=null", false, false, "F24352C3-1D96-4BF2-9317-6BA8EBE61633" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Reporting.Insights
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Reporting.Insights", "Insights", "Rock.Blocks.Reporting.Insights, Rock.Blocks, Version=18.0.11.0, Culture=neutral, PublicKeyToken=null", false, false, "6739DD77-A510-4826-8263-2C2E53D31DF9" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Security.Oidc.AuthScopeDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Security.Oidc.AuthScopeDetail", "Auth Scope Detail", "Rock.Blocks.Security.Oidc.AuthScopeDetail, Rock.Blocks, Version=18.0.11.0, Culture=neutral, PublicKeyToken=null", false, false, "6FA3255D-BC1D-43AC-B34A-1E4F4B40EAB6" );

            // Add/Update Obsidian Block Type
            //   Name:External Application List
            //   Category:Administration
            //   EntityType:Rock.Blocks.Administration.ExternalApplicationList
            RockMigrationHelper.AddOrUpdateEntityBlockType( "External Application List", "Will list all of the defined type values with the type of External Application.  This provides a way for users to select any one of these files.", "Rock.Blocks.Administration.ExternalApplicationList", "Administration", "2A18F4BF-633F-47CE-A228-3F908AA5A189" );

            // Add/Update Obsidian Block Type
            //   Name:Email Form
            //   Category:CMS
            //   EntityType:Rock.Blocks.Cms.EmailForm
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Email Form", "Block that takes an HTML form and emails the contents to an address of your choosing.", "Rock.Blocks.Cms.EmailForm", "CMS", "956174B7-109C-4821-841A-AC1830B97A13" );

            // Add/Update Obsidian Block Type
            //   Name:Insights
            //   Category:Reporting
            //   EntityType:Rock.Blocks.Reporting.Insights
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Insights", "Shows insights regarding demongraphics, information, and records.", "Rock.Blocks.Reporting.Insights", "Reporting", "551DB463-A013-476C-A619-57CC234DC410" );

            // Add/Update Obsidian Block Type
            //   Name:OpenID Connect Scope Detail
            //   Category:Security > OIDC
            //   EntityType:Rock.Blocks.Security.Oidc.AuthScopeDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "OpenID Connect Scope Detail", "Displays the details of the given OpenID Connect Scope.", "Rock.Blocks.Security.Oidc.AuthScopeDetail", "Security > OIDC", "1A2520F9-4990-485A-8A4D-38CF1440D71D" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Enable List View Display Options
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CF372F6E-7131-4FF7-8BCD-6053DBB67D34", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable List View Display Options", "EnableListViewDisplayOptions", "Enable List View Display Options", @"Allows selecting a display mode of Grid or Cards.", 3, @"false", "9F0A2FE5-87EF-4595-B9E5-A9744E71596E" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "34F4C462-4417-4DF7-AA6B-B9285154AF36" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "429A817E-1379-4BCC-AEFE-01D9C75273E5", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "6303114C-065F-4DEA-A4F4-2F3B47970678" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "B8B03F0D-4DC2-40F7-BA53-D991D01A6D8A" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3EFB4302-9AB4-420F-A818-48B1B06AD109", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "74904791-47DC-470D-89F6-D3813582185F" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Included Inactive List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Included Inactive List", "IncludedInactiveList", "Included Inactive List", @"Should the inactive communication list be displayed as an option for subscribing?", 6, @"True", "08F71A8F-6437-4441-9100-593862211951" );

            // Attribute for BlockType
            //   BlockType: External Application List
            //   Category: Administration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A18F4BF-633F-47CE-A228-3F908AA5A189", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "1C8C5247-820B-4D0A-816E-0DE81CE55AF2" );

            // Attribute for BlockType
            //   BlockType: External Application List
            //   Category: Administration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "2A18F4BF-633F-47CE-A228-3F908AA5A189", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "49D305A5-01F0-45A5-BF6D-069414AFA6A5" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Recipient Email(s)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Recipient Email(s)", "RecipientEmail", "Recipient Email(s)", @"Email addresses (comma delimited) to send the contents to.", 0, @"", "B603278B-0932-4778-BD04-0723584C9A9B" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: CC Email(s)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CC Email(s)", "CCEmail", "CC Email(s)", @"CC Email addresses (comma delimited) to send the contents to.", 1, @"", "E663BF32-4B73-4A49-971F-FBCEB7DCB822" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: BCC Email(s)
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "BCC Email(s)", "BCCEmail", "BCC Email(s)", @"BCC Email addresses (comma delimited) to send the contents to.", 2, @"", "E4E64ABC-CD08-49C7-AC8C-975841FD72AF" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Subject
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "Subject", @"The subject line for the email.", 3, @"", "08143969-556B-49B3-AF5F-89017DC3FE41" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: From Email
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "From Email", "FromEmail", "From Email", @"The email to use for the from address.", 4, @"", "DDEDEB55-A046-47DC-AF64-51AB9B492B4B" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: From Name
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "From Name", "FromName", "From Name", @"The name to use for the from address.", 5, @"", "10245E56-BDB8-400E-BA0B-C869FC6C64EC" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: HTML Form
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "HTML Form", "HTMLForm", "HTML Form", @"The HTML for the form the user will complete.", 6, @"{% if CurrentPerson %}
    {{ CurrentPerson.NickName }}, could you please complete the form below.
{% else %}
    Please complete the form below.
{% endif %}
<div class=""form-group"">
    <label for=""firstname"">First Name</label>
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.NickName }}</p>
        <input type=""hidden"" id=""firstname"" name=""FirstName"" value=""{{ CurrentPerson.NickName }}"" />
    {% else %}
        <input type=""text"" class=""form-control"" id=""firstname"" name=""FirstName"" placeholder=""First Name"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""lastname"">Last Name</label>
   
    {% if CurrentPerson %}
        <p>{{ CurrentPerson.LastName }}</p>
        <input type=""hidden"" id=""lastname"" name=""LastName"" value=""{{ CurrentPerson.LastName }}"" />
    {% else %}
        <input type=""text"" class=""form-control"" id=""lastname"" name=""LastName"" placeholder=""Last Name"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""email"">Email</label>
    {% if CurrentPerson %}
        <input type=""email"" class=""form-control"" id=""email"" name=""Email"" value=""{{ CurrentPerson.Email }}"" placeholder=""Email"" required />
    {% else %}
        <input type=""email"" class=""form-control"" id=""email"" name=""Email"" placeholder=""Email"" required />
    {% endif %}
</div>
<div class=""form-group"">
    <label for=""message"">Message</label>
    <textarea id=""message"" rows=""4"" class=""form-control"" name=""Message"" placeholder=""Message"" required></textarea>
</div>
<div class=""form-group"">
    <label for=""attachment"">Attachment</label>
    <input type=""file"" id=""attachment"" name=""attachment"" /> <br />
    <input type=""file"" id=""attachment2"" name=""attachment2"" />
</div>
", "0D47BD35-4A97-43E2-974E-F267B0A98EEC" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Message Body
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Message Body", "MessageBody", "Message Body", @"The email message body.", 7, @"{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    A email form has been submitted. Please find the information below:
</p>
{% for field in FormFields %}
    {% assign fieldParts = field | PropertyToKeyValue %}
    <strong>{{ fieldParts.Key | Humanize | Capitalize }}</strong>: {{ fieldParts.Value }} <br/>
{% endfor %}
<p>&nbsp;</p>
{{ 'Global' | Attribute:'EmailFooter' }}", "9E93D2CE-0877-444D-AA25-98D25D66775A" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Response Message
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Response Message", "ResponseMessage", "Response Message", @"The message the user will see when they submit the form if no response page if provided. Lava merge fields are available for you to use in your message.", 8, @"<div class=""alert alert-info"">
    Thank you for your response. We appreciate your feedback!
</div>", "06DEF1E1-DE38-488B-A317-150B07C516E8" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Response Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Response Page", "ResponsePage", "Response Page", @"The page the use will be taken to after submitting the form. Use the 'Response Message' field if you just need a simple message.", 9, @"", "AE777577-85FE-4F47-9090-9F37315E69D1" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button Text
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button Text", "SubmitButtonText", "Submit Button Text", @"The text to display for the submit button.", 10, @"Submit", "CDD5380B-B7D3-4BC2-A28A-373770CAB61D" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button Wrap CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button Wrap CSS Class", "SubmitButtonWrapCssClass", "Submit Button Wrap CSS Class", @"CSS class to add to the div wrapping the button.", 11, @"", "6E7BE51B-AAFF-4829-81C8-067001659F10" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button CSS Class
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Submit Button CSS Class", "SubmitButtonCssClass", "Submit Button CSS Class", @"The CSS class add to the submit button.", 12, @"btn btn-primary", "02A92FD5-DD08-4A62-99EC-39C7112EA0F4" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Enable Debug
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "Enable Debug", @"Shows the fields available to merge in lava.", 13, @"False", "85E37189-13FD-4140-94E1-C1F4FB07CDE6" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Save Communication History
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Save Communication History", @"Should a record of this communication be saved to the recipient's profile", 14, @"False", "33325DC1-EA98-42C7-9E13-4D36C597EE42" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Captcha Support", "DisableCaptchaSupport", "Disable Captcha Support", @"If set to 'Yes' the CAPTCHA verification will be skipped. 

Note: If the CAPTCHA site key and/or secret key are not configured in the system settings, this option will be forced as 'Yes', even if 'No' is visually selected.", 15, @"False", "3F1B3615-15FD-44FF-A0E4-66CA8E5EAD17" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "956174B7-109C-4821-841A-AC1830B97A13", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled for this HTML block.", 16, @"", "DA083BEB-06A7-409E-A1AB-C22137BE659B" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics", "ShowDemographics", "Show Demographics", @"When enabled the Demographics panel will be displayed.", 0, @"true", "61646E05-FA15-44BF-B9F8-DF55556B2AE7" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Connection Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Connection Status", "ShowConnectionStatus", "Show Demographics > Connection Status", @"When enabled the Connection Status chart will be displayed on the Demographics panel.", 1, @"true", "7E1F3258-E85D-4B81-AFE6-C3E02DC0E1E8" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Age
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Age", "ShowAge", "Show Demographics > Age", @"When enabled the Age chart will be displayed on the Demographics panel.", 2, @"true", "41808730-DF80-44AD-84CD-BDB07924FDD9" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Marital Status
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Marital Status", "ShowMaritalStatus", "Show Demographics > Marital Status", @"When enabled the Marital Status chart will be displayed on the Demographics panel.", 3, @"true", "6A2F4AE1-EBDE-4DC2-A8A0-C92E21BB708F" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Gender
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Gender", "ShowGender", "Show Demographics > Gender", @"When enabled the Gender chart will be displayed on the Demographics panel.", 4, @"true", "7B250327-F9F4-4FBD-AF25-9F02D97E91FE" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Ethnicity
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Ethnicity", "ShowEthnicity", "Show Demographics > Ethnicity", @"When enabled the Ethnicity chart will be displayed on the Demographics panel.", 5, @"false", "6FAEF5AB-D481-4277-96BF-61C44515A6C7" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Race
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Demographics > Race", "ShowRace", "Show Demographics > Race", @"When enabled the Race chart will be displayed on the Demographics panel.", 6, @"false", "DEB3A407-9895-4F42-B0F9-59DC4EB4B705" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics", "ShowInformationStatistics", "Show Information Statistics", @"When enabled the Information Statistics panel will be displayed.", 7, @"true", "7A5B2495-888A-4B22-8261-C33F7DAD22E6" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Information Completeness
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > Information Completeness", "ShowInformationCompleteness", "Show Information Statistics > Information Completeness", @"When enabled the Information Completeness chart will be displayed on the Information Statistics panel.", 8, @"true", "EB83B9ED-326A-40ED-87CC-24A89279DFFE" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > % of Active Individuals with Assessments
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > % of Active Individuals with Assessments", "ShowPercentageOfActiveIndividualsWithAssessments", "Show Information Statistics > % of Active Individuals with Assessments", @"When enabled the % of Active Individuals with Assessments chart will be displayed on the Information Statistics panel.", 9, @"true", "01CD974A-8657-4CB7-814B-EB6910BA1E06" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Record Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "551DB463-A013-476C-A619-57CC234DC410", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Information Statistics > Record Statuses", "ShowRecordStatuses", "Show Information Statistics > Record Statuses", @"When enabled the Record Statuses chart will be displayed on the Information Statistics panel.", 10, @"true", "8A813761-C98F-4B3C-A9DF-9514925A3307" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Included Inactive List
            RockMigrationHelper.DeleteAttribute( "08F71A8F-6437-4441-9100-593862211951" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Record Statuses
            RockMigrationHelper.DeleteAttribute( "8A813761-C98F-4B3C-A9DF-9514925A3307" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > % of Active Individuals with Assessments
            RockMigrationHelper.DeleteAttribute( "01CD974A-8657-4CB7-814B-EB6910BA1E06" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics > Information Completeness
            RockMigrationHelper.DeleteAttribute( "EB83B9ED-326A-40ED-87CC-24A89279DFFE" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Information Statistics
            RockMigrationHelper.DeleteAttribute( "7A5B2495-888A-4B22-8261-C33F7DAD22E6" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Race
            RockMigrationHelper.DeleteAttribute( "DEB3A407-9895-4F42-B0F9-59DC4EB4B705" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Ethnicity
            RockMigrationHelper.DeleteAttribute( "6FAEF5AB-D481-4277-96BF-61C44515A6C7" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Gender
            RockMigrationHelper.DeleteAttribute( "7B250327-F9F4-4FBD-AF25-9F02D97E91FE" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Marital Status
            RockMigrationHelper.DeleteAttribute( "6A2F4AE1-EBDE-4DC2-A8A0-C92E21BB708F" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Age
            RockMigrationHelper.DeleteAttribute( "41808730-DF80-44AD-84CD-BDB07924FDD9" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics > Connection Status
            RockMigrationHelper.DeleteAttribute( "7E1F3258-E85D-4B81-AFE6-C3E02DC0E1E8" );

            // Attribute for BlockType
            //   BlockType: Insights
            //   Category: Reporting
            //   Attribute: Show Demographics
            RockMigrationHelper.DeleteAttribute( "61646E05-FA15-44BF-B9F8-DF55556B2AE7" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "DA083BEB-06A7-409E-A1AB-C22137BE659B" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Disable Captcha Support
            RockMigrationHelper.DeleteAttribute( "3F1B3615-15FD-44FF-A0E4-66CA8E5EAD17" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Save Communication History
            RockMigrationHelper.DeleteAttribute( "33325DC1-EA98-42C7-9E13-4D36C597EE42" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Enable Debug
            RockMigrationHelper.DeleteAttribute( "85E37189-13FD-4140-94E1-C1F4FB07CDE6" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button CSS Class
            RockMigrationHelper.DeleteAttribute( "02A92FD5-DD08-4A62-99EC-39C7112EA0F4" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button Wrap CSS Class
            RockMigrationHelper.DeleteAttribute( "6E7BE51B-AAFF-4829-81C8-067001659F10" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Submit Button Text
            RockMigrationHelper.DeleteAttribute( "CDD5380B-B7D3-4BC2-A28A-373770CAB61D" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Response Page
            RockMigrationHelper.DeleteAttribute( "AE777577-85FE-4F47-9090-9F37315E69D1" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Response Message
            RockMigrationHelper.DeleteAttribute( "06DEF1E1-DE38-488B-A317-150B07C516E8" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Message Body
            RockMigrationHelper.DeleteAttribute( "9E93D2CE-0877-444D-AA25-98D25D66775A" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: HTML Form
            RockMigrationHelper.DeleteAttribute( "0D47BD35-4A97-43E2-974E-F267B0A98EEC" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: From Name
            RockMigrationHelper.DeleteAttribute( "10245E56-BDB8-400E-BA0B-C869FC6C64EC" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: From Email
            RockMigrationHelper.DeleteAttribute( "DDEDEB55-A046-47DC-AF64-51AB9B492B4B" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Subject
            RockMigrationHelper.DeleteAttribute( "08143969-556B-49B3-AF5F-89017DC3FE41" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: BCC Email(s)
            RockMigrationHelper.DeleteAttribute( "E4E64ABC-CD08-49C7-AC8C-975841FD72AF" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: CC Email(s)
            RockMigrationHelper.DeleteAttribute( "E663BF32-4B73-4A49-971F-FBCEB7DCB822" );

            // Attribute for BlockType
            //   BlockType: Email Form
            //   Category: CMS
            //   Attribute: Recipient Email(s)
            RockMigrationHelper.DeleteAttribute( "B603278B-0932-4778-BD04-0723584C9A9B" );

            // Attribute for BlockType
            //   BlockType: External Application List
            //   Category: Administration
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "49D305A5-01F0-45A5-BF6D-069414AFA6A5" );

            // Attribute for BlockType
            //   BlockType: External Application List
            //   Category: Administration
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "1C8C5247-820B-4D0A-816E-0DE81CE55AF2" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "74904791-47DC-470D-89F6-D3813582185F" );

            // Attribute for BlockType
            //   BlockType: Step Type List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "B8B03F0D-4DC2-40F7-BA53-D991D01A6D8A" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.DeleteAttribute( "6303114C-065F-4DEA-A4F4-2F3B47970678" );

            // Attribute for BlockType
            //   BlockType: Step Program List
            //   Category: Steps
            //   Attribute: core.CustomActionsConfigs
            RockMigrationHelper.DeleteAttribute( "34F4C462-4417-4DF7-AA6B-B9285154AF36" );

            // Attribute for BlockType
            //   BlockType: Step Program Detail
            //   Category: Steps
            //   Attribute: Enable List View Display Options
            RockMigrationHelper.DeleteAttribute( "9F0A2FE5-87EF-4595-B9E5-A9744E71596E" );

            // Delete BlockType 
            //   Name: OpenID Connect Scope Detail
            //   Category: Security > OIDC
            //   Path: -
            //   EntityType: Auth Scope Detail
            RockMigrationHelper.DeleteBlockType( "1A2520F9-4990-485A-8A4D-38CF1440D71D" );

            // Delete BlockType 
            //   Name: Insights
            //   Category: Reporting
            //   Path: -
            //   EntityType: Insights
            RockMigrationHelper.DeleteBlockType( "551DB463-A013-476C-A619-57CC234DC410" );

            // Delete BlockType 
            //   Name: Email Form
            //   Category: CMS
            //   Path: -
            //   EntityType: Email Form
            RockMigrationHelper.DeleteBlockType( "956174B7-109C-4821-841A-AC1830B97A13" );

            // Delete BlockType 
            //   Name: External Application List
            //   Category: Administration
            //   Path: -
            //   EntityType: External Application List
            RockMigrationHelper.DeleteBlockType( "2A18F4BF-633F-47CE-A228-3F908AA5A189" );
        }
    }
}
