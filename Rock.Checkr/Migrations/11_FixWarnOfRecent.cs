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
using Rock.Plugin;

namespace Rock.Migrations
{
    [MigrationNumber( 11, "1.8.0" )]
    public class Checker_FixWarnOfRecent : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update Background Check:Initial Request:Get Details to fix "Warn of Recent" condition.
            RockMigrationHelper.UpdateWorkflowActionForm( @"
<h1>Background Request Details</h1>
<p> {{CurrentPerson.NickName}}, please complete the form below to start the background request process. </p>
{% assign warnOfRecent = Workflow | Attribute:'WarnOfRecent' %}
{% if warnOfRecent == 'Yes' %}
    <div class='alert alert-warning'>
        Notice: It's been less than a year since this person's last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />",
                @"",
                "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^999e3f46-924c-48c9-add6-4685115c11a7^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^7448a73f-0cee-48bf-a2b4-38bac2bf8efa^The request has been canceled.",
                "",
                false,
                "",
                "644D005C-CC28-4050-994C-C6E53A930F69" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}