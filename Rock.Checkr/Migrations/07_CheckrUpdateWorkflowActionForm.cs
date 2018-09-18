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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 7, "1.8.0" )]
    public class Checker_UpdateWorkflowActionForm : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<p> {{CurrentPerson.NickName}}, please complete the form below to start the background request process. </p>
{% if Workflow | Attribute:'WarnOfRecent' == 'Yes' %}
    <div class='alert alert-warning'>
        Notice: It's been less than a year since this person's last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^999e3f46-924c-48c9-add6-4685115c11a7^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^7448a73f-0cee-48bf-a2b4-38bac2bf8efa^The request has been canceled.", "", false, "", "644D005C-CC28-4050-994C-C6E53A930F69" ); // Background Check:Initial Request:Get Details
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request has been submitted for your review.
    If you approve the request it will be sent to the background check provider for processing. If you
    deny the request, it will be sent back to the requester. If you deny the request, please add notes
    explaining why the request was denied.
</div>", @"", "Approve^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been submitted to provider for processing.|Deny^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The requester will be notified that this request has been denied (along with the reason why).", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "FEDCFF5F-92D8-4106-942F-410A8224EFBD", "15FAD0CB-5E05-4932-9EDD-71400B3B2D51" ); // Background Check:Approve Request:Approve or Deny
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<p> {{CurrentPerson.NickName}}, this request has come back from the approval process with the following results. </p>
<div class=""well"">
    <strong>Summary of Security Notes:</strong>
    <br />
    <table class=""table table-condensed table-light margin-b-md"">
        {% for activity in Workflow.Activities %}
            {% if activity.ActivityType.Name == 'Approve Request' %}
                <tr>
                    <td width=""220"">{{activity.CompletedDateTime}}</td>
                    <td width=""220"">{{activity | Attribute:'Approver'}}</td>
                    <td>{{activity | Attribute:'Note'}}</td>
                </tr>
            {% endif %}
        {% endfor %}
    </table>
</div>
<hr />", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^999e3f46-924c-48c9-add6-4685115c11a7^The request has been submitted again to the security team for approval.|Cancel Request^5683E775-B9F3-408C-80AC-94DE0E51CF3A^7448a73f-0cee-48bf-a2b4-38bac2bf8efa^The request has been canceled.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "7666CD74-CC0F-42D7-A464-0B5162EA15A7" ); // Background Check:Review Denial:Review
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<div class='alert alert-danger'>
    An error occurred when submitting the background check. See details below.
</div>", @"", "Re-Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^e124fc14-fce2-4342-b5b3-444aa71d4429^Your information has been submitted successfully.", "", true, "", "6B2ED899-6148-4F0F-B28C-4EA04A91899B" ); // Background Check:Request Error:Display Error Message
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<div class='alert alert-info'>
    {{CurrentPerson.NickName}}, the following background request was submitted and completed, but requires
    your review. Please pass or fail this request. The requester will be notified and the person's record
    will be updated to indicate the result you select.
</div>
<hr />", @"", "Pass^c88fef94-95b9-444a-bc93-58e983f3c047^^The request has been marked as passed. Requester will be notified.|Fail^d6b809a9-c1cc-4ebb-816e-33d8c1e53ea4^^The request has been marked as failed. Requester will be notified.", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "C116EA20-A62A-4560-9CB9-35EEAEF9F0B5", "956E66B5-0896-44D4-8911-03A6C6CABABF" ); // Background Check:Review Result:Review Results
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
