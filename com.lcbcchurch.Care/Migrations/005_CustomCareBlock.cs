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
    [MigrationNumber( 5, "1.0.14" )]
    public class CustomCareBlock : Migration
    {
        public override void Up()
        {
            RockMigrationHelper.DeleteBlock( "A02BD97D-E78A-4A51-95C1-C9730A53D9B7" );
            RockMigrationHelper.UpdateBlockType( "My Care Opportunities", "Block to display the connection opportunities that user is authorized to view, and the opportunities that are currently assigned to the user.", "~/Plugins/com_bemadev/Care/MyCareOpportunities.ascx", "BEMA Services > Care", "06E6FF1F-1E72-4C83-A759-107D28008F80" );
            // Add Block to Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "19D2AEA1-7A31-4980-93C6-D2CA7286D5A4","","06E6FF1F-1E72-4C83-A759-107D28008F80","My Care Opportunities","Main", @"<script type=""text / javascript"">
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

", "",1,"3665DFDB-7745-4C58-8966-5BBCD68D1187");   
            // Attrib for BlockType: My Care Opportunities:Configuration Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Configuration Page","ConfigurationPage","","Page used to modify and create connection opportunities.",0,@"","60066DB4-B010-4DEC-BCBD-AAAE4636D445");  
            // Attrib for BlockType: My Care Opportunities:Detail Page
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Detail Page","DetailPage","","Page used to view details of an requests.",1,@"","00A2C897-7739-4304-84D0-A97B3CF6B09E");  
            // Attrib for BlockType: My Care Opportunities:Connection Types
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","E4E72958-4604-498F-956B-BA095976A60B","Connection Types","ConnectionTypes","","Optional list of connection types to limit the display to (All will be displayed by default).",2,@"","71A15A45-C713-4CDF-8A14-1EA6D783130F");  
            // Attrib for BlockType: My Care Opportunities:Show Request Total
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Request Total","ShowRequestTotal","","If enabled, the block will show the total number of requests.",3,@"True","9D1D68FA-0BF8-4088-9ED6-EC00B907E55F");  
            // Attrib for BlockType: My Care Opportunities:Show Last Activity Note
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Show Last Activity Note","ShowLastActivityNote","","If enabled, the block will show the last activity note for each request in the list.",4,@"False","8B34CE28-D704-4658-82FC-B417C4ECF5F6");  
            // Attrib for BlockType: My Care Opportunities:Status Template
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Status Template","StatusTemplate","","Lava Template that can be used to customize what is displayed in the status bar. Includes common merge fields plus ConnectionOpportunities, ConnectionTypes and the default IdleTooltip.",5,@"<div class='pull-left badge-legend padding-r-md'>      <span class='pull-left badge badge-info badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>      <span class='pull-left badge badge-warning badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'><span class='sr-only'>Unassigned Item</span></span>      <span class='pull-left badge badge-critical badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'><span class='sr-only'>Critical Status</span></span>      <span class='pull-left badge badge-danger badge-circle js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>  </div>","4C92F390-20D2-489A-AAAD-E5D614873608");  
            // Attrib for BlockType: My Care Opportunities:Opportunity Summary Template
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Opportunity Summary Template","OpportunitySummaryTemplate","","Lava Template that can be used to customize what is displayed in each Opportunity Summary. Includes common merge fields plus the OpportunitySummary, ConnectionOpportunity, and its ConnectionRequests.",6,@"<span class=""item-count"" title=""There are {{ 'active connection' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:'#,###,##0' }}</span>  <i class='{{ OpportunitySummary.IconCssClass }}'></i>  <h3>{{ OpportunitySummary.Name }}</h3>  <div class='status-list'>      <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>      <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>      <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>      <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span>  </div>  ","9C822757-F9D3-4F23-834E-B5C6E67F9DAA");  
            // Attrib for BlockType: My Care Opportunities:Connection Request Status Icons Template
            RockMigrationHelper.UpdateBlockTypeAttribute("06E6FF1F-1E72-4C83-A759-107D28008F80","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Connection Request Status Icons Template","ConnectionRequestStatusIconsTemplate","","Lava Template that can be used to customize what is displayed for the status icons in the connection request grid.",7,@"  <div class='status-list'>      {% if ConnectionRequestStatusIcons.IsAssignedToYou %}      <span class='badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'><span class='sr-only'>Assigned To You</span></span>      {% endif %}      {% if ConnectionRequestStatusIcons.IsUnassigned %}      <span class='badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'><span class='sr-only'>Unassigned</span></span>      {% endif %}      {% if ConnectionRequestStatusIcons.IsCritical %}      <span class='badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'><span class='sr-only'>Critical</span></span>      {% endif %}      {% if ConnectionRequestStatusIcons.IsIdle %}      <span class='badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'><span class='sr-only'>{{ IdleTooltip }}</span></span>      {% endif %}  </div>  ","23A52F2B-98C0-407A-863A-2E9E0D65A32A");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Configuration Page Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","60066DB4-B010-4DEC-BCBD-AAAE4636D445",@"9cc19684-7ad2-4d4e-a7c4-10dae56e7fa6");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Detail Page Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","00A2C897-7739-4304-84D0-A97B3CF6B09E",@"0afb6a46-50f3-4e82-939b-88d4248a657b");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Connection Types Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","71A15A45-C713-4CDF-8A14-1EA6D783130F",@"");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Show Request Total Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","9D1D68FA-0BF8-4088-9ED6-EC00B907E55F",@"True");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Show Last Activity Note Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","8B34CE28-D704-4658-82FC-B417C4ECF5F6",@"False");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Status Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","4C92F390-20D2-489A-AAAD-E5D614873608",@"<div class='pull-left badge-legend padding-r-md'>     <span class='pull-left badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>     <span class='pull-left badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned Item'>&nbsp;</span>     <span class='pull-left badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical Status'>&nbsp;</span>     <span class='pull-left badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span>  </div>");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Opportunity Summary Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","9C822757-F9D3-4F23-834E-B5C6E67F9DAA",@"<span class=""item-count"" title=""There are {{ 'active connection' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:'#,###,##0' }}</span> <i class='{{ OpportunitySummary.IconCssClass }}'></i> <h3>{{ OpportunitySummary.Name }}</h3> <div class='status-list'>     <span class='badge badge-info'>{{ OpportunitySummary.AssignedToYou | Format:'#,###,###' }}</span>     <span class='badge badge-warning'>{{ OpportunitySummary.UnassignedCount | Format:'#,###,###' }}</span>     <span class='badge badge-critical'>{{ OpportunitySummary.CriticalCount | Format:'#,###,###' }}</span>     <span class='badge badge-danger'>{{ OpportunitySummary.IdleCount | Format:'#,###,###' }}</span> </div> ");  
            // Attrib Value for Block:My Care Opportunities, Attribute:Connection Request Status Icons Template Page: Care, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("3665DFDB-7745-4C58-8966-5BBCD68D1187","23A52F2B-98C0-407A-863A-2E9E0D65A32A",@"<div class='status-list'>     {% if ConnectionRequestStatusIcons.IsAssignedToYou %}     <span class='badge badge-info js-legend-badge' data-toggle='tooltip' data-original-title='Assigned To You'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsUnassigned %}     <span class='badge badge-warning js-legend-badge' data-toggle='tooltip' data-original-title='Unassigned'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsCritical %}     <span class='badge badge-critical js-legend-badge' data-toggle='tooltip' data-original-title='Critical'>&nbsp;</span>     {% endif %}     {% if ConnectionRequestStatusIcons.IsIdle %}     <span class='badge badge-danger js-legend-badge' data-toggle='tooltip' data-original-title='{{ IdleTooltip }}'>&nbsp;</span>      {% endif %} </div> ");  
        }
        public override void Down()
        {
             }
    }
}
