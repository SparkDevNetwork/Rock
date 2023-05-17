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
    public partial class UpdateSignUpSystemCommunications : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            #region Update Exsting Sign-Up System Communications

            RockMigrationHelper.UpdateSystemCommunication(
                "Sign-Up Group Confirmation", // category
                "Sign-Up Group Registration", // title
                string.Empty, // from
                string.Empty, // fromName
                string.Empty, // to
                string.Empty, // cc
                string.Empty, // bcc
                "{{ ProjectName }} Registration Confirmation", // subject
                @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>{{ ProjectName }} Registration Confirmation</h1>
<p>Hi {{ Registrant.NickName }}!</p>
<p>This is a confirmation that you have signed up for {{ ProjectName }}{% if OpportunityName and OpportunityName != empty %} ({{ OpportunityName }}){% endif %}.</p>
{% if StartDateTime != null %}
    <p><strong>Date/Time:</strong> {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}</p>
{% endif %}
{% if FriendlyLocation and FriendlyLocation != empty %}
    <p><strong>Location:</strong> {{ FriendlyLocation }}</p>
{% endif %}
{% if AdditionalDetails and AdditionalDetails != empty %}
    {{ AdditionalDetails }}
{% endif %}
<p>Thanks!</p>
<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>
{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.SIGNUP_GROUP_REGISTRATION_CONFIRMATION, // guid
                true, // isActive
                "This is a confirmation from {{ 'Global' | Attribute:'OrganizationName' }} that you have signed up for {{ ProjectName }} on {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}." // smsMessage
            );

            RockMigrationHelper.UpdateSystemCommunication(
                "Sign-Up Group Confirmation", // category
                "Sign-Up Group Reminder", // title
                string.Empty, // from
                string.Empty, // fromName
                string.Empty, // to
                string.Empty, // cc
                string.Empty, // bcc
                "{{ ProjectName }} Reminder", // subject
                @"{{ 'Global' | Attribute:'EmailHeader' }}
<h1>{{ ProjectName }} Reminder</h1>
<p>Hi {{ Registrant.NickName }}!</p>
<p>This is a reminder that you have signed up for {{ ProjectName }}{% if OpportunityName and OpportunityName != empty %} ({{ OpportunityName }}){% endif %}.</p>
{% if StartDateTime != null %}
    <p><strong>Date/Time:</strong> {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}</p>
{% endif %}
{% if FriendlyLocation and FriendlyLocation != empty %}
    <p><strong>Location:</strong> {{ FriendlyLocation }}</p>
{% endif %}
{% if AdditionalDetails and AdditionalDetails != empty %}
    {{ AdditionalDetails }}
{% endif %}
<p>Thanks!</p>
<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>
{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.SIGNUP_GROUP_REMINDER, // guid
                true, // isActive
                "This is a reminder from {{ 'Global' | Attribute:'OrganizationName' }} that you have signed up for {{ ProjectName }} on {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}." // smsMessage
            );

            #endregion

            #region Update Existing Sign-Up Detail Block Setting

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The Lava template to use to show the details of the project. Merge fields include: Project. <span class='tip tip-lava'></span>", 2, @"{% if Project != null %}
    <div class=""panel panel-block"">
        <div class=""panel-heading"">
            <h1 class=""panel-title pull-left"">{{ Project.Name }}</h1>
        </div>
        <div class=""panel-body"">
            <div class=""row"">
                <div class=""col-md-6 mb-3"">
                    <div class=""d-flex justify-content-between"">
                        <h4>{{ Project.Name }}</h4>
                        {% if Project.CampusName and Project.CampusName != empty %}
                            <div class=""panel-labels"">
                                <span class=""label label-default"">{{ Project.CampusName }}</span>
                            </div>
                        {% endif %}
                    </div>
                    {% if Project.ScheduleName and Project.ScheduleName != empty %}
                        <p class=""text-muted mb-3"">{{ Project.ScheduleName }}</p>
                    {% endif %}
                    {% if Project.Description and Project.Description != empty %}
                        <p>{{ Project.Description }}</p>
                    {% endif %}
                    {% if Project.AvailableSpots != null %}
                        <span class=""badge badge-info"">Available Spots: {{ Project.AvailableSpots }}</span>
                    {% endif %}
                    {% if Project.ScheduleHasFutureStartDateTime %}
                        <div class=""mt-4"">
                            <a href=""{{ Project.RegisterPageUrl }}"" class=""btn btn-warning"">Register</a>
                        </div>
                    {% endif %}
                </div>
                {% if Project.MapCenter and Project.MapCenter != empty %}
                    <div class=""col-md-6 mb-3"">
                        {[ googlestaticmap center:'{{ Project.MapCenter }}' zoom:'15' ]}
                        {[ endgooglestaticmap ]}
                    </div>
                {% endif %}
            </div>
        </div>
    </div>
{% endif %}", "50FB027E-6C1F-41A3-9077-F98DFCAD301D" );

            #endregion
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Nothing to do.
        }
    }
}
