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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration.
    /// </summary>
    [MigrationNumber( 95, "1.10.1" )]
    public class PrayerRequestCommentsNotificationEmailTemplate : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //AddPrayerRequestCommentsNotificationEmailTemplateUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //AddPrayerRequestCommentsNotificationEmailTemplateDown();
        }

        /// <summary>
        /// Adds the prayer request comments notification email template up.
        /// </summary>
        private void AddPrayerRequestCommentsNotificationEmailTemplateUp()
        {
            const string PrayerRequestCommentsNotificationTemplateGuid = "FAEA9DE5-62CE-4EEE-960B-C06103E97AA9";

            RockMigrationHelper.UpdateSystemEmail( "System", "Prayer Request Comments Digest", "", "", "", "", "", "Prayer Request Update",
        @"
{{ 'Global' | Attribute:'EmailHeader' }}
{% assign firstName = FirstName  %}
{% if PrayerRequest.RequestedByPersonAlias.Person.NickName %}
   {% assign firstName = PrayerRequest.RequestedByPersonAlias.Person.NickName %}
{% endif %}

<p>
{{ firstName }}, below are recent comments from the prayer request you submitted on {{ PrayerRequest.EnteredDateTime | Date:'dddd, MMMM dd' }}.
</p>
<p>
<strong>Request</strong>
<br/>
{{ PrayerRequest.Text }}
</p>
<p>
<strong>Comments</strong>
<br/>
{% for comment in Comments %}
<i>{{ comment.CreatedByPersonName }} ({{ comment.CreatedDateTime | Date:'sd' }} - {{ comment.CreatedDateTime | Date:'h:mmtt' }})</i><br/>
{{ comment.Text }}<br/><br/>
{% endfor %}
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
",
                                                    PrayerRequestCommentsNotificationTemplateGuid );
        }

        /// <summary>
        /// Adds the prayer request comments notification email template down.
        /// </summary>
        private void AddPrayerRequestCommentsNotificationEmailTemplateDown()
        {
            const string PrayerRequestCommentsNotificationTemplateGuid = "FAEA9DE5-62CE-4EEE-960B-C06103E97AA9";

            RockMigrationHelper.DeleteSystemEmail( PrayerRequestCommentsNotificationTemplateGuid );
        }
    }
}
