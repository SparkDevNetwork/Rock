using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

using com.bemaservices.HrManagement.SystemGuid;
using Rock.Web.Cache;
using Rock.Lava.Blocks;
using System.Security.AccessControl;

namespace com.bemaservices.HrManagement.Migrations
{
    [MigrationNumber( 4, "1.9.4" )]
    public class WorkflowEmailsLavaFix : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
         //   PtoAllocationWorkflow();
            PtoRequestWorkflow();
        }

        private void PtoRequestWorkflow()
        {


            #region PTO Request

            RockMigrationHelper.AddActionTypeAttributeValue( "E6B880BD-3903-41D5-8734-91DBDB91EE09", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}
{% assign selection = Activity | Attribute:'Selection'%}

{{ 'Global' | Attribute:'EmailHeader' }}
{% if selection == 'Submit Changes' %}
    An updated {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% elseif selection == 'Submit Request' %}
    A new {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% else %}
    A {{ Workflow | Attribute:'Type' }} Time Off Request has been CANCELLED by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% endif %}

    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Year to Date</strong>
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Add / Modify Request:Send Email to Supervisor if one exists:Body
        RockMigrationHelper.AddActionTypeAttributeValue( "C889FAA5-3FC6-4920-BC4D-8C70E17061F4", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture reviewLink %}{{ 'Global' | Attribute:'InternalApplicationRoot' }}Person/{{ Workflow | Attribute:'Person','Id' }}/HR{% endcapture %}
{% capture reviewText %}Review Request{% endcapture %}
{% capture endDate %}{{ Workflow | Attribute:'EndDate'}}{% endcapture %}
{% assign selection = Activity | Attribute:'Selection' %}

{{ 'Global' | Attribute:'EmailHeader' }}

{% if selection == 'Submit Changes' %}
    An updated {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% elseif selection == 'Submit Request' %}
    A new {{ Workflow | Attribute:'Type' }} Time Off Request has been submitted by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% else %}
    A {{ Workflow | Attribute:'Type' }} Time Off Request has been CANCELLED by {{ Workflow | Attribute:'Person' }}. <br /> <br />
{% endif %}

    {% if endDate != empty %}
        <strong>Date(s):</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} - {{ Workflow | Attribute:'EndDate' | Date:'dddd, MMM d, yyyy'}} <br /> <br />
    {% else %}
        <strong>Date:</strong> {{ Workflow | Attribute:'StartDate' | Date:'dddd, MMM d, yyyy' }} <br /> <br />
    {% endif %}
        
    
    <strong>Request Hours:</strong> {{ Workflow | Attribute:'RequestedHours' }}<br />
    <strong>Reason:</strong> {{ Workflow | Attribute:'Reason' }}<br /><br />
    
    <strong>Total Year to Date</strong>
    <strong>Total Requested Hours YTD:</strong> {{ Workflow | Attribute:'RequestedHoursYTD' }}<br />
    <strong>Remaining Hours:</strong> {{ Workflow | Attribute:'RemainingHours' }}<br />
            

Thank you!<br/>
<br/>
<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ reviewLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
			<w:anchorlock/>
			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{reviewText}}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ reviewLink }}""
		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{reviewText}}</a></div>

	</td>
 </tr>
</table>
{{ 'Global' | Attribute:'EmailFooter' }}
" ); // PTO Request:Add / Modify Request:Send Email to HR if no supervisor exists:Body

            #endregion


        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
