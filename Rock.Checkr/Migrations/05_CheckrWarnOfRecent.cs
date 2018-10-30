﻿// <copyright>
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
    [MigrationNumber( 5, "1.8.0" )]
    public class Checkr_WarnOfRecent : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE [WorkflowActionForm]
SET [Header] = '{% assign WarnOfRecent = Workflow | Attribute:''WarnOfRecent'' %}<h1>Background Request Details</h1>
<p>
    {{CurrentPerson.NickName}}, please complete the form below to start the background
    request process.
</p>
{% if WarnOfRecent == ''Yes'' %}
    <div class=''alert alert-warning''>
        Notice: It''s been less than a year since this person''s last background check was processed.
        Please make sure you want to continue with this request!
    </div>
{% endif %}
<hr />'
WHERE [Guid] = '644D005C-CC28-4050-994C-C6E53A930F69'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
