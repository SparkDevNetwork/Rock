// <copyright>
// Copyright by LCBC Church
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.lcbcchurch.Care.Migrations
{
    [MigrationNumber( 1, "1.0.14" )]
    public class AddCarePages : Migration
    {
        public override void Up()
        {
            // Page: Care
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Care", "", "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4", "fa fa-plug" ); // Site:Rock RMS
                                                                                                                                                                                             // Add Block to Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4","","3F69E04F-F966-4CAE-B89D-F97DFEF6407A","My Connection Opportunities","Main", @"<script type=""text/javascript"">
function renameConnectionTextAtOnload() {
    $('.checkbox-inline:contains(""Connected"")').each( function(){
        var replacementText = $(this).text().replace( ""Connected"", 'Completed' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.panel-title:contains(""Connection Request"")').each( function(){
        var replacementText = $(this).text().replace( ""Connection Request"", 'Care Request' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.label-success:contains(""Connected"")').each( function(){
        var replacementText = $(this).text().replace( ""Connected"", 'Completed' );
        $(this).contents().last().replaceWith( replacementText );
    });

    $('.col-md-3:contains(""Connected"")').each( function(){
        var replacementText = $(this).text().replace( ""Connected"", 'Completed' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.grid-select-cell:contains(""Connected"")').each( function(){
        var replacementText = $(this).html().replace( ""Connected"", 'Completed' );
        $(this).html( replacementText );
    });
    
    $('label:contains(""Connector"")').each( function(){
        var replacementText = $(this).text().replace( ""Connector"", 'Assigned PRT' );
        $(this).contents().last().replaceWith( replacementText );
    });    
    
    $('a:contains(""Connector"")').each( function(){
        var replacementText = $(this).text().replace( ""Connector"", 'Assigned PRT' );
        $(this).contents().last().replaceWith( replacementText );
    });
}

function startListener() {
    var mutationObserver = new MutationObserver( function( mutations ) {
        mutations.forEach(function(mutation) {
            for (var i = 0; i < mutation.addedNodes.length; i++) {
                renameConnectionTextAtOnload();
            }
        });
    });
  
    mutationObserver.observe(document.documentElement, {
        childList: true,
        subtree: true
    });
}
  
$(document ).ready( function() {
    startListener();
});

</script>

","",0,"A02BD97D-E78A-4A51-95C1-C9730A53D9B7");
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "70C77D07-4D28-4E71-BD0F-3DF0F61CE12D", @"9cc19684-7ad2-4d4e-a7c4-10dae56e7fa6" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Detail Page Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "E2A64E17-7310-4ED4-85F4-65D0ED97A513", @"0afb6a46-50f3-4e82-939b-88d4248a657b" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Connection Types Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "1A322AE7-F018-417C-94BC-A77A2C079495", @"6bec4205-4943-4b5e-8794-fe7d15015274" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Show Request Total Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "07A73CAB-25FB-4AA5-9665-9994068FB2A1", @"True" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Show Last Activity Note Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "6CAC4307-AC17-4711-B7F7-E911D11E0D27", @"False" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Status Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "AD3951D5-3B1F-473B-B39A-E0BC61B3A228", @"<div class='pull-left badge-legend padding-r-md'>     <span class='pull-left badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>     <span class='pull-left badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'>&nbsp;</span>     <span class='pull-left badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'>&nbsp;</span>     <span class='pull-left badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span>  </div>" );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Opportunity Summary Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "FE0C3AE0-0496-47C2-989F-BF1AE1090220", @"<span class=""item-count"" title=""There are {{ 'active connection' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:'#,###,##0' }}</span> <i class='{{ OpportunitySummary.IconCssClass }}'></i> <h3>{{ OpportunitySummary.Name }}</h3> <div class='status-list'>     <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>     <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>     <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>     <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span> </div> " );
            // Attrib Value for Block:My Connection Opportunities, Attribute:Connection Request Status Icons Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7", "6650B724-0F01-4671-B8B8-C695EAA0BCD4", @"<div class='status-list'>     {% if ConnectionRequestStatusIcons.IsAssignedToYou %}     <span class='badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsUnassigned %}     <span class='badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsCritical %}     <span class='badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsIdle %}     <span class='badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span>      {% endif %} </div> " );

            // Page: Care Request Detail
            RockMigrationHelper.AddPage( "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Care Request Detail", "", "0AFB6A46-50F3-4E82-939B-88D4248A657B", "fa fa-plug" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "LCBC Connection Request Detail", "Displays the details of the given connection request for editing state, status, etc.", "~/Plugins/com_bemadev/Connection/LCBCConnectionRequestDetail.ascx", "LCBC > Connection", "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A" );
            // Add Block to Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0AFB6A46-50F3-4E82-939B-88D4248A657B", "", "4A89FF55-A6A3-4A9B-8D1D-2ADE092565F5", "Entity Attribute Values", "Main", "", "", 2, "E9C8AFF1-623C-4D2C-A7AD-6EA633446580" );
            // Add Block to Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0AFB6A46-50F3-4E82-939B-88D4248A657B","","FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A","LCBC Connection Request Detail","Main", @"{% assign isRockAdmin = false %}
{% groupmember where:'GroupId == 2 && PersonId == {{CurrentPerson.Id}}'%}
    {% for gm in groupmemberItems %}
        {% assign isRockAdmin = true %}
    {% endfor %}
{% endgroupmember %}

{% assign personId = 'Global' | PageParameter:'PersonId' %}
{% assign connectionRequestId = 'Global' | PageParameter:'ConnectionRequestId' %}

<script type=""text/javascript"">
function renameConnectionTextAtOnload() {
    $('.radio-inline:contains(""Connected"")').each( function(){
        var replacementText = $(this).text().replace( ""Connected"", 'Completed' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.btn-success:contains(""Connect"")').each( function(){
        var replacementText = $(this).text().replace( ""Connect"", 'Complete' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.label-success:contains(""Connected"")').each( function(){
        var replacementText = $(this).text().replace( ""Connected"", 'Completed' );
        $(this).contents().last().replaceWith( replacementText );
    });
    
    $('.grid-select-cell:contains(""Connected"")').each( function(){
        var replacementText = $(this).html().replace( ""Connected"", 'Completed' );
		$(this).html( replacementText );
    });
    
    $('label:contains(""Connector"")').each( function(){
        var replacementText = $(this).text().replace( ""Connector"", 'Assigned PRT' );
        $(this).contents().last().replaceWith( replacementText );
    });    
    
    $('th:contains(""Connector"")').each( function(){
        var replacementText = $(this).text().replace( ""Connector"", 'Assigned PRT' );
        $(this).contents().last().replaceWith( replacementText );
    });    
    
    $('.form-group:contains(""Placement Group"")').each( function(){
        $(this).hide();
    });

    {% if isRockAdmin == false %}
    $('.grid-delete-button').each( function(){
        $(this).hide();
    });
    {% endif %}

    {% if connectionRequestId == 0 %}
        {% person id: '{{personId}}' %}
            $('.picker-selectedperson').each( function(){
                $(this).html( ""{{person.FullName}}"" );
            }); 

            $('.js-person-name').each( function(){
                $(this).val( ""{{person.FullName}}"" );
            });  

            $('.js-person-id').each( function(){
                $(this).val({ { person.Id} });
            });
        {% endperson %}
    {% endif %}
}

function startListener() {
    var mutationObserver = new MutationObserver( function( mutations ) {
        mutations.forEach(function(mutation) {
            for (var i = 0; i < mutation.addedNodes.length; i++) {
                renameConnectionTextAtOnload();
            }
        });
    });
  
    mutationObserver.observe(document.documentElement, {
        childList: true,
        subtree: true
    });
}
  
$(document ).ready( function() {
    startListener();
});

</script>

","",1,"10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C");
            // Attrib for BlockType: LCBC Connection Request Detail:Badges
            RockMigrationHelper.UpdateBlockTypeAttribute( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A", "3F1AE891-7DC8-46D2-865D-11543B34FB60", "Badges", "Badges", "", "The person badges to display in this block.", 0, @"", "E1FB0932-8573-4B92-8E61-A7D9EAD99D56" );
            // Attrib for BlockType: LCBC Connection Request Detail:Person Profile Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page used for viewing a person's profile. If set a view profile button will show for each group member.", 0, @"", "C850169C-6CC3-4081-A134-64A9C481762E" );
            // Attrib for BlockType: LCBC Connection Request Detail:Workflow Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "Page used to display details about a workflow.", 1, @"", "022E1F16-B214-4909-AEAF-44BC58E34AB6" );
            // Attrib for BlockType: LCBC Connection Request Detail:Workflow Entry Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Entry Page", "WorkflowEntryPage", "", "Page used to launch a new workflow of the selected type.", 2, @"", "BE7B9CD9-4C3A-49F7-BECB-7A4A5CE166AD" );
            // Attrib for BlockType: LCBC Connection Request Detail:Group Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "Page used to display group details.", 3, @"", "CE11D39F-83F7-426A-8A53-967EA1865591" );
            // Attrib Value for Block:Entity Attribute Values, Attribute:Entity Type Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E9C8AFF1-623C-4D2C-A7AD-6EA633446580", "9A64DF98-22D9-43A6-9F92-C83A94AAC896", @"36b0d0c7-8125-48fa-9da2-729aaa65f718" );
            // Attrib Value for Block:LCBC Connection Request Detail, Attribute:Badges Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C", "E1FB0932-8573-4B92-8E61-A7D9EAD99D56", @"" );
            // Attrib Value for Block:LCBC Connection Request Detail, Attribute:Person Profile Page Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C", "C850169C-6CC3-4081-A134-64A9C481762E", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
            // Attrib Value for Block:LCBC Connection Request Detail, Attribute:Workflow Detail Page Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C", "022E1F16-B214-4909-AEAF-44BC58E34AB6", @"ba547eed-5537-49cf-bd4e-c583d760788c" );
            // Attrib Value for Block:LCBC Connection Request Detail, Attribute:Workflow Entry Page Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C", "BE7B9CD9-4C3A-49F7-BECB-7A4A5CE166AD", @"0550d2aa-a705-4400-81ff-ab124fdf83d7" );
            // Attrib Value for Block:LCBC Connection Request Detail, Attribute:Group Detail Page Page: Care Request Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C", "CE11D39F-83F7-426A-8A53-967EA1865591", @"4e237286-b715-4109-a578-c1445ec02707" );
            // Add/Update PageContext for Page:Care Request Detail, Entity: Rock.Model.ConnectionRequest, Parameter: ConnectionRequestId
            RockMigrationHelper.UpdatePageContext( "0AFB6A46-50F3-4E82-939B-88D4248A657B", "Rock.Model.ConnectionRequest", "ConnectionRequestId", "46C08434-8322-409C-96B9-CAC72AF1DC26" );
            // Add/Update PageContext for Page:Care Request Detail, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "0AFB6A46-50F3-4E82-939B-88D4248A657B", "Rock.Model.Person", "PersonId", "7F1681F5-7CD9-49BB-BEAE-5AADFBB1B089" );

            // Page: Care
            RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Care", "", "F9D12AAF-1E27-46BD-85FF-10D280D9C245", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPageRoute( "F9D12AAF-1E27-46BD-85FF-10D280D9C245", "Person/{PersonId}/Care" );
            // Add Block to Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F9D12AAF-1E27-46BD-85FF-10D280D9C245", "", "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "Dynamic Data", "SectionC1", "", "", 1, "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06" );
            // Add Block to Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F9D12AAF-1E27-46BD-85FF-10D280D9C245", "", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Add New Care Request", "SectionC1", "", "", 0, "3DFB18A8-909F-435F-AE5A-409338F498CC" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "19B61D65-37E3-459F-A44F-DEF0089118A3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 11, @"False", "04C15DC1-DFB6-4D63-A7BC-0507D0E33EF4" );
            // Attrib Value for Block:Dynamic Data, Attribute:Merge Fields Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "8EB882CE-5BB1-4844-9C28-10190903EECD", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Formatted Output Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "6A233402-446C-47E9-94A5-6A247C29BC21", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Person Report Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "8104CE53-FDB3-4E9F-B8E7-FD9E06E7551C", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Communication Recipient Person Id Columns Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "75DDB977-9E71-44E8-924B-27134659D3A4", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Excel Export Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "E11B57E5-EC7D-4C42-9ADA-37594D71F145", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Communicate Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "5B2C115A-C187-4AB3-93AE-7010644B39DA", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Person Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "8762ABE3-726E-4629-BD4D-3E42E1FBCC9E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Bulk Update Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "D01510AA-1B8D-467C-AFC6-F7554CB7CF78", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Stored Procedure Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "A4439703-5432-489A-9C14-155903D6A43E", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Merge Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "6697B0A2-C8FE-497A-B5B4-A9D459474338", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Paneled Grid Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "5449CB61-2DFC-4B55-A697-38F1C2AF128B", @"False" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Grid Filter Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "E582FD3C-9990-47D1-A57F-A3DB753B1D0C", @"True" );
            // Attrib Value for Block:Dynamic Data, Attribute:Timeout Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "BEEE38DD-2791-4242-84B6-0495904143CC", @"30" );
            // Attrib Value for Block:Dynamic Data, Attribute:Show Columns Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "202A82BF-7772-481C-8419-600012607972", @"False" );

            // Attrib Value for Block:Dynamic Data, Attribute:Url Mask Page: Care, Site: Rock RMS
            // Need to query for Care Request Detail Page ID since it won't be the same across Rock instances
            string careRequestDetailPageId = SqlScalar( "SELECT TOP 1 P.Id FROM [Page] P WHERE P.[Guid] = '0AFB6A46-50F3-4E82-939B-88D4248A657B'" ).ToString().Trim();
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "B9163A35-E09C-466D-8A2D-4ED81DF0114C", @"~/page/" + careRequestDetailPageId + "?ConnectionRequestId={ConnectionRequestId}&ConnectionOpportunityId={ConnectionOpportunityId}" );

            // Attrib Value for Block:Dynamic Data, Attribute:Query Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "71C8BA4E-8EF2-416B-BFE9-D1D88D9AA356", @"Declare @LastActivityDates table(  ConnectionRequestId int,  LastActivityDate datetime  ) Insert Into @LastActivityDates Select ConnectionRequestId, max(CreatedDateTime) From ConnectionRequestActivity Group By ConnectionRequestId  Select cr.Id as 'ConnectionRequestId',         co.Id as 'ConnectionOpportunityId',         cr.CreatedDateTime as 'Request Submitted',   cr.Comments as 'Description',   c.Name as 'Campus',   connector.FirstName+' '+connector.LastName as 'Assigned To',   Case    When cr.ConnectionState = 1 then 'Inactive'    When cr.ConnectionState = 2 then 'FutureFollowUp'    When cr.ConnectionState = 3 then 'Complete'    Else cs.Name   End as 'Request Status',   Case       When cat.Name = 'Connected' then 'Completed'       Else cat.Name       End as 'Last Activity',   ca.Note as 'Activity Note',   ca.CreatedDateTime as 'Activity Date' From ConnectionRequest cr left Join @LastActivityDates lastActivityDates on cr.Id = lastActivityDates.ConnectionRequestId Join ConnectionOpportunity co on cr.ConnectionOpportunityId = co.Id Join PersonAlias requester on cr.PersonAliasId = requester.Id Join ConnectionStatus cs on cr.ConnectionStatusId = cs.Id left Join ConnectionRequestActivity ca on ca.ConnectionRequestId = cr.Id and ca.CreatedDateTime = lastActivityDates.LastActivityDate left Join ConnectionActivityType cat on ca.ConnectionActivityTypeId = cat.Id left join PersonAlias connectorAlias on cr.ConnectorPersonAliasId = connectorAlias.Id left join Person connector on connectorAlias.PersonId = connector.Id left join Campus c on cr.CampusId = c.Id Where requester.PersonId = {{PersonId}} Order by [Activity Date] desc" );
            // Attrib Value for Block:Dynamic Data, Attribute:Columns Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "90B0E6AF-B2F4-4397-953B-737A40D4023B", @"ConnectionRequestId,ConnectionOpportunityId" );
            // Attrib Value for Block:Dynamic Data, Attribute:Query Params Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "B0EC41B9-37C0-48FD-8E4E-37A8CA305012", @"" );
            // Attrib Value for Block:Dynamic Data, Attribute:Update Page Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06", "230EDFE8-33CA-478D-8C9A-572323AF3466", @"True" );
            // Add/Update PageContext for Page:Care, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "F9D12AAF-1E27-46BD-85FF-10D280D9C245", "Rock.Model.Person", "PersonId", "F31FBA33-019C-4316-B49D-0378F34AB70C" );

            // Add Block to Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "My Connection Opportunities Lava", "Sidebar1", "", "", 6, "E4F04EC5-8D01-4833-8916-9A8BEA834D00" );
            // Attrib Value for Block:My Connection Opportunities Lava, Attribute:Detail Page Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E4F04EC5-8D01-4833-8916-9A8BEA834D00", "848484B1-0666-4B2A-B63B-22CFBD00540E", @"0afb6a46-50f3-4e82-939b-88d4248a657b" );
            // Attrib Value for Block:My Connection Opportunities Lava, Attribute:Connection Types Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E4F04EC5-8D01-4833-8916-9A8BEA834D00", "2687FEFB-9012-499A-A37D-32838C952D3D", @"6bec4205-4943-4b5e-8794-fe7d15015274" );
            // Attrib Value for Block:My Connection Opportunities Lava, Attribute:Contents Page: Internal Homepage, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E4F04EC5-8D01-4833-8916-9A8BEA834D00", "03C85B59-C957-4216-A750-B667188B4CB9", @"<link rel=""stylesheet"" href=""/Styles/bootstrap-sortable.css""> <div class='panel panel-block'>   <div class='panel-heading'>     <h4 class='panel-title'>My PRT Requests</h4>   </div>   {% assign midnightToday = 'Now' | Date:'M/d/yyyy' | DateAdd:1,'d' %}   {% if ConnectionRequests.size > 0 %}     <table class=""table sortable"">       <thead>         <tr>           <th></th>           <th>Name</th>           <th>Last Activity</th>         </tr>       </thead>       <tbody>         {% for connectionRequest in ConnectionRequests %}           {% assign lastActivity = LastActivityLookup[connectionRequest.Id] %}           {% assign isCritical = false %}           {% if connectionRequest.ConnectionStatus.IsCritical and connectionRequest.ConnectionState == 0 %}             {% assign isCritical = true %}           {% assign daysTillDue = null %}           {% elseif connectionRequest.ConnectionStatus.IsCritical and connectionRequest.ConnectionState == 2 %}             {% assign daysTillDue = midnightToday | DateDiff:connectionRequest.FollowupDate, 'd' %}             {% if daysTillDue < 0 %}               {% assign isCritical = true %}             {% endif %}           {% endif %}             {% if lastActivity.CreatedDateTime %}             {% assign idleDays = lastActivity.CreatedDateTime | DateDiff:'Now', 'd' %}           {% else %}             {% assign idleDays = connectionRequest.CreatedDateTime | DateDiff:'Now', 'd' %}           {% endif %}            {% assign statusIcons = '' %}           {% if isCritical %}           {% assign statusIcons = statusIcons | Append:'<span class=""badge badge-critical"" title=""Critical"">&nbsp;</span> ' %}           {% endif %}           {% assign daysUntilRequestIdle = 14 %}           {% if connectionRequest.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle %}             {% assign daysUntilRequestIdle = connectionRequest.ConnectionOpportunity.ConnectionType.DaysUntilRequestIdle %}           {% endif %}           {% if idleDays > daysUntilRequestIdle %}           {% assign daysAsString = daysUntilRequestIdle | Format:'#' %}           {% assign statusHtml = ' <span class=""badge badge-danger"" title=""Idle (no activity in ' | Append:daysAsString | Append:' days)"">&nbsp;</span>' %}           {% assign statusIcons = statusIcons | Append:statusHtml %}           {% endif %}             <tr>             <td align=""left""><div class=""status-list"">{{ statusIcons }}</div></td>             {% if LinkedPages.DetailPage %}               <td><a href=""{{ LinkedPages.DetailPage }}?ConnectionRequestId={{ connectionRequest.Id }}&ConnectionOpportunityId={{ connectionRequest.ConnectionOpportunityId }}"">{{ connectionRequest.PersonAlias.Person.LastName }}, {{ connectionRequest.PersonAlias.Person.NickName }} </a></td>             {% else %}               <td>{{ connectionRequest.PersonAlias.Person.LastName }}, {{ connectionRequest.PersonAlias.Person.NickName }} </td>             {% endif %}             <td>{{ lastActivity.ConnectionActivityType.Name }} (<span class='small'>{{ lastActivity.CreatedDateTime | HumanizeDateTime  }}</span>)</td>           </tr>         {% endfor %}       </tbody>     </table>   {% else %}     <div class=""panel-body"">       <div class='alert alert-info'>There are no PRT requests assigned to you.</div>     </div>   {% endif %} </div>  <script src=""/Scripts/moment.min.js""></script> <script src=""/Scripts/bootstrap-sortable.js""></script>" );

            

            RockMigrationHelper.AddSecurityAuthForPage( "F9D12AAF-1E27-46BD-85FF-10D280D9C245", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "3CFD91BA-1F8B-4609-A351-B3C278799294" ); // Page:Care
            RockMigrationHelper.AddSecurityAuthForPage( "F9D12AAF-1E27-46BD-85FF-10D280D9C245", 2, "View", false, "", 1, "328182F3-7C73-4B52-B74A-402CD5316ACB" ); // Page:Care
            RockMigrationHelper.AddSecurityAuthForPage( "F9D12AAF-1E27-46BD-85FF-10D280D9C245", 1, "View", true, "2C112948-FF4C-46E7-981A-0257681EADF4", 0, "BC56B097-6B5F-4C31-929B-C741F3ECCAE6" ); // Page:Care
            RockMigrationHelper.AddSecurityAuthForPage( "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4", 0, "Edit", true, "060971D2-EAF9-4C0D-B6F6-F01725CAA5AC", 0, "177244BA-76F5-4589-A376-3F9212A37AB2" ); // Page:Care
            RockMigrationHelper.AddSecurityAuthForPage( "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4", 0, "View", true, "060971D2-EAF9-4C0D-B6F6-F01725CAA5AC", 0, "8BDDF07F-0BBC-4CA7-8307-17604088F8FD" ); // Page:Care
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "8BDDF07F-0BBC-4CA7-8307-17604088F8FD" ); // Page:Care Group: 060971D2-EAF9-4C0D-B6F6-F01725CAA5AC ( RSR - Connection Administration ),
            RockMigrationHelper.DeleteSecurityAuth( "177244BA-76F5-4589-A376-3F9212A37AB2" ); // Page:Care Group: 060971D2-EAF9-4C0D-B6F6-F01725CAA5AC ( RSR - Connection Administration ),
            RockMigrationHelper.DeleteSecurityAuth( "BC56B097-6B5F-4C31-929B-C741F3ECCAE6" ); // Page:Care Group: 2C112948-FF4C-46E7-981A-0257681EADF4 ( RSR - Staff Workers ),
            RockMigrationHelper.DeleteSecurityAuth( "328182F3-7C73-4B52-B74A-402CD5316ACB" ); // Page:Care Group: <all users>
            RockMigrationHelper.DeleteSecurityAuth( "3CFD91BA-1F8B-4609-A351-B3C278799294" ); // Page:Care Group: 628C51A8-4613-43ED-A18D-4A6FB999273E ( RSR - Rock Administration ),

            RockMigrationHelper.DeleteBlock( "E4F04EC5-8D01-4833-8916-9A8BEA834D00" );


            RockMigrationHelper.DeleteBlock( "3DFB18A8-909F-435F-AE5A-409338F498CC" );
            RockMigrationHelper.DeleteBlock( "BAFAE24E-3BEC-4823-8F3E-9B124EC97D06" );
            RockMigrationHelper.DeletePage( "F9D12AAF-1E27-46BD-85FF-10D280D9C245" ); //  Page: Care
            // Delete PageContext for Page:Care, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "F31FBA33-019C-4316-B49D-0378F34AB70C" );


            RockMigrationHelper.DeleteAttribute( "C850169C-6CC3-4081-A134-64A9C481762E" );
            RockMigrationHelper.DeleteAttribute( "022E1F16-B214-4909-AEAF-44BC58E34AB6" );
            RockMigrationHelper.DeleteAttribute( "BE7B9CD9-4C3A-49F7-BECB-7A4A5CE166AD" );
            RockMigrationHelper.DeleteAttribute( "E1FB0932-8573-4B92-8E61-A7D9EAD99D56" );
            RockMigrationHelper.DeleteAttribute( "CE11D39F-83F7-426A-8A53-967EA1865591" );
            RockMigrationHelper.DeleteBlock( "10D902F6-1F8C-4AFE-A033-0BD4ED8CE23C" );
            RockMigrationHelper.DeleteBlock( "E9C8AFF1-623C-4D2C-A7AD-6EA633446580" );
            RockMigrationHelper.DeleteBlockType( "FF3B07E0-D6F5-4874-B7A3-A0D5B770AF8A" );
            RockMigrationHelper.DeletePage( "0AFB6A46-50F3-4E82-939B-88D4248A657B" ); //  Page: Care Request Detail

            // Delete PageContext for Page:Care Request Detail, Entity: Rock.Model.ConnectionRequest, Parameter: ConnectionRequestId
            RockMigrationHelper.DeletePageContext( "46C08434-8322-409C-96B9-CAC72AF1DC26" );
            // Delete PageContext for Page:Care Request Detail, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "7F1681F5-7CD9-49BB-BEAE-5AADFBB1B089" );


            RockMigrationHelper.DeleteBlock( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7" );
            RockMigrationHelper.DeletePage( "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4" ); //  Page: Care           
        }
    }
}
