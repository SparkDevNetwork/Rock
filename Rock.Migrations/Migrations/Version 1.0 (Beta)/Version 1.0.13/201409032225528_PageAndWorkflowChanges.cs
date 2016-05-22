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
    public partial class PageAndWorkflowChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add person profile link attribute to group member block
            RockMigrationHelper.AddBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The person profile page to link to.", 0, "", "15E2C1EA-B0A1-469F-AC25-45C93FEC8140", false );
            RockMigrationHelper.AddBlockAttributeValue( "C66D11C8-DA55-40EA-925C-C9D7AC71F879", "15E2C1EA-B0A1-469F-AC25-45C93FEC8140", "08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            
            // rename workflow action 'Set Attribute From Person'
            Sql( @"UPDATE [EntityType] SET [FriendlyName] = 'Set Attribute From Person' WHERE [Name] = 'Rock.Workflow.Action.SetAttributeFromPerson'" );

            // change the name of the person workflow category to 'Data Integrity' and remove it from workflow display block
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "FB420F14-3D9D-4304-878F-124902E2CEAB", "78e38655-d951-41db-a0ff-d6474775cfa1,cb99421e-9adc-488e-8c71-94bb14f27f56" );

            Sql( @"UPDATE [Category]
              SET [Name] = 'Data Integrity', [IconCssClass] = 'fa fa-magic'
              WHERE [Guid] = 'BBAE05FD-8192-4616-A71E-903A927E0D90'" );

            
            // update system email for workflow change query parm 'action' to 'command'
            Sql( @"UPDATE [SystemEmail]
SET [Body] = '{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.NickName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href=''{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h4>Details:</h4>
<p>
{% for attribute in Action.FormAttributes %}

    <strong>{{ attribute.Name }}:</strong> 

    {% if attribute.Url != Empty %}
        <a href=''{{ attribute.Url }}''>{{ attribute.Value }}</a>
    {% else %}
        {{ attribute.Value }}
    {% endif %}
    <br/>

    {% if attribute.IsRequired %}
        {% assign RequiredFields = true %}
    {% endif %}

{% endfor %}
</p>


{% if Action.ActionType.WorkflowForm.IncludeActionsInNotification == true %}

    {% if RequiredFields != true %}

        <p>
        {% for button in Action.ActionType.WorkflowForm.Buttons %}

            {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
            {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&Command={{ button.Name }}{% endcapture %}
            {% capture ButtonHtml %}{{ button.Html | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

            {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
            {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
            {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}

        {% endfor %}
        </p>

    {% endif %}

{% endif %}

{{ GlobalAttribute.EmailFooter }}'
WHERE [Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'" );

            
            //Remove default marital status in Add Family block:             
            RockMigrationHelper.AddBlockAttributeValue( "613536BE-86BC-4755-B815-807C236B92E6", "815D526D-671A-48B0-8988-9264D65BAB26", @"" );

            // Photo Request Changes
            // add new block type
            RockMigrationHelper.UpdateBlockType( "Send Photo Request", "Block for selecting criteria to build a list of people who should receive a photo request.", "~/Blocks/Crm/PhotoRequest/SendRequest.ascx", "CRM > PhotoRequest", "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC" );

            // Add Block to Page: Send Requests, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B64D0429-488C-430E-8C32-5C7F32589F73", "", "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC", "Send Photo Request", "Main", "", "", 0, "E1FA5543-593A-4806-BE01-44109AF15E1B" );

            // Attrib for BlockType: Send Photo Request:Photo Request Template
            RockMigrationHelper.AddBlockTypeAttribute( "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC", "C3B37465-DCAF-4C8C-930C-9A9B5D066CA9", "Photo Request Template", "PhotoRequestTemplate", "", "The template to use with this block to send requests.", 0, @"B9A0489C-A823-4C5C-A9F9-14A206EC3B88", "89B9849E-E6B3-4459-ACF5-8D89C39AB2B8" );

            // Attrib for BlockType: Send Photo Request:Maximum Recipients
            RockMigrationHelper.AddBlockTypeAttribute( "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Recipients", "MaximumRecipients", "", "The maximum number of recipients allowed before communication will need to be approved", 0, @"300", "4B0BF992-408F-4DC9-AC4B-74591BE597DE" );

            // update photo request template
            Sql( @"
        UPDATE [CommunicationTemplate]
            SET [ChannelDataJson] = N'{ ""HtmlMessage"": ""{{ GlobalAttribute.EmailStyles }}\n{{ GlobalAttribute.EmailHeader }}\n<p>{{ Person.NickName }},</p>\n\n<p>We&#39;re all about people and we&#39;d like to personalize our relationship by having a recent photo of you in our membership system. Please take a minute to send us your photo using the button below - we&#39;d really appreciate it.</p>\n\n<p><a href=\""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/Upload/{{ Person.UrlEncodedKey }}\"">Upload Photo </a></p>\n\n<p>Your picture will remain confidential and will only be visible to staff and volunteers in a leadership position within {{ GlobalAttribute.OrganizationName }}.</p>\n\n<p><a href=\""{{ GlobalAttribute.PublicApplicationRoot }}PhotoRequest/OptOut/{{ Person.UrlEncodedKey }}\"">I prefer not to receive future photo requests.</a></p>\n\n<p><a href=\""{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}\"">I&#39;m no longer involved with {{ GlobalAttribute.OrganizationName }}. Please remove me from all future communications.</a></p>\n\n<p>{{ Communication.ChannelData.FromName }}<br />\n</p>\n{{ GlobalAttribute.EmailFooter }}"" }'
        WHERE [Guid] = 'B9A0489C-A823-4C5C-A9F9-14A206EC3B88'
" );
            // Remove unnecessary Block: Member List, from Page: View Request Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DEC91774-50DD-46F4-9441-2A35B6FAC843" );
            // Remove unnecessary Page: View Request Details, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "2B941997-0C2C-4153-8E1E-ACC874DCC7DB" );

            // move photo request blocks
            Sql( @"  
  UPDATE [BlockType]
  SET [Path] = '~/Blocks/Crm/PhotoUpload.ascx', [Name] = 'Photo Upload', [Category] = 'CRM'
  WHERE [Path] = '~/Blocks/Crm/PhotoRequest/Upload.ascx'

  UPDATE [BlockType]
  SET [Path] = '~/Blocks/Crm/PhotoOptOut.ascx', [Name] = 'Photo Opt-Out', [Category] = 'CRM'
  WHERE [Path] = '~/Blocks/Crm/PhotoRequest/PhotoOptOut.ascx'

  UPDATE [BlockType]
  SET [Path] = '~/Blocks/Crm/PhotoVerify.ascx', [Name] = 'Photo Verify', [Category] = 'CRM'
  WHERE [Path] = '~/Blocks/Crm/PhotoRequest/VerifyPhoto.ascx'

  UPDATE [BlockType]
  SET [Path] = '~/Blocks/Crm/PhotoSendRequest.ascx', [Name] = 'Photo Send Request', [Category] = 'CRM'
  WHERE [Path] = '~/Blocks/Crm/PhotoRequest/SendRequest.ascx'" );

            // update the message for the upload photo page
            Sql( @"  UPDATE [HtmlContent]
  SET [Content] = '<p>
    {% if Person.NickName != Empty %}{{ Person.NickName }}! {% endif %} Thanks for taking the time to improve the quality of {{ GlobalAttribute.OrganizationName }}.
    To upload a photo simply click the ''Select Photo'' next to
    the individuals below, select the desired photo, and
    press ''Open''. It''s that easy!
</p>
'
  WHERE [Guid] = '33A47BDE-2CFE-487B-9786-4847CE45C44F'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // reverse change the name of the person workflow category to 'Data Integrity' and remove it from workflow display block
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "FB420F14-3D9D-4304-878F-124902E2CEAB", "78e38655-d951-41db-a0ff-d6474775cfa1,cb99421e-9adc-488e-8c71-94bb14f27f56,BBAE05FD-8192-4616-A71E-903A927E0D90" );

            Sql( @"UPDATE [Category]
              SET [Name] = 'Person', [IconCssClass] = 'fa fa-user'
              WHERE [Guid] = 'BBAE05FD-8192-4616-A71E-903A927E0D90'" );

            // photo request changes
            // Attrib for BlockType: Send Photo Request:Maximum Recipients
            RockMigrationHelper.DeleteAttribute( "4B0BF992-408F-4DC9-AC4B-74591BE597DE" );
            // Attrib for BlockType: Send Photo Request:Photo Request Template
            RockMigrationHelper.DeleteAttribute( "89B9849E-E6B3-4459-ACF5-8D89C39AB2B8" );
            // Remove Block: Send Photo Request, from Page: Send Requests, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E1FA5543-593A-4806-BE01-44109AF15E1B" );
            RockMigrationHelper.DeleteBlockType( "DE1AF7AE-92A8-484F-B5F2-03D2D4B320EC" ); // Send Photo Request

            // Add the View Request Details page back in
            RockMigrationHelper.AddPage( "325B50D6-545D-461A-9CB7-72B001E82F21", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "View Request Details", "", "2B941997-0C2C-4153-8E1E-ACC874DCC7DB", "fa fa-list-alt" ); // Site:Rock RMS
            // Add Block to Page: View Request Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2B941997-0C2C-4153-8E1E-ACC874DCC7DB", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Member List", "Main", "", "", 0, "DEC91774-50DD-46F4-9441-2A35B6FAC843" );
        }
    }
}
