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
    public partial class LoginConfirmationAlert : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateSystemCommunication(
                "Security", // category
                "Login Confirmation Alert", // title
                "", // from
                "", // from name
                "", // to
                "", // cc
                "", // bcc
                "New Device Sign-in On Your {{ 'Global' | Attribute:'OrganizationName' }} Account", // subject
                @"
{{ 'Global' | Attribute:'EmailHeader' }}

<p>Hi {{ Person.FirstName }},</p>
<p>We noticed a new login to your account from a different device.</p>
<br />
<p>What Device?<br />
{{ UserAgent }}</p>

<p>Where From?<br />
{{ IPAddress }}
{% if Location != empty %}
({{ Location }})
{% endif %}
</p>

<p>When?<br />
{{ LoginDateTime | Date:'M/d/yyyy h:mm:ss tt K' }}</p>

<p>If this was you, no further action is needed.</p>

<p>If this wasn’t you, please reach out to us immediately at {{ 'Global' | Attribute:'OrganizationEmail' }}.</p>

{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.LOGIN_CONFIRMATION_ALERT // guid
                );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
