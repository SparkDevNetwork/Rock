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
    public partial class UpdateShortTermServingProjects : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ShortTermServingProjects_UpdateSystemCommunications();
            ShortTermServingProjects_UpdatePublicBlockTypes();
            ShortTermServingProjects_UpdatePublicBlocks();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            ShortTermServingProjects_DeletePublicBlocks();
            ShortTermServingProjects_DeletePublicBlockTypes();
            ShortTermServingProjects_DeleteSystemCommunications();
        }

        /// <summary>
        /// JPH: Update system communications needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_UpdateSystemCommunications()
        {
            #region New SystemCommunications

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
<p>This is a confirmation that you have signed up for {{ ProjectName }}{% if OpportunityName != empty %} ({{ OpportunityName }}){% endif %}.</p>
{% if StartDateTime != null %}
    <p><strong>Date/Time:</strong> {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}</p>
{% endif %}
{% if FriendlyLocation != empty %}
    <p><strong>Location:</strong> {{ FriendlyLocation }}</p>
{% endif %}
<p>Thanks!</p>
<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>
{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.SIGNUP_GROUP_REGISTRATION_CONFIRMATION, // guid
                true, // isActive
                "This is a confirmation from {{ 'Global' | Attribute:'OrganizationName' }} that you have signed up for {{ ProjectName }} on {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}." // smsMessage
            );

            #endregion

            #region Updated SystemCommunications

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
<p>This is a reminder that you have signed up for {{ ProjectName }}{% if OpportunityName != empty %} ({{ OpportunityName }}){% endif %}.</p>
{% if StartDateTime != null %}
    <p><strong>Date/Time:</strong> {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}</p>
{% endif %}
{% if FriendlyLocation != empty %}
    <p><strong>Location:</strong> {{ FriendlyLocation }}</p>
{% endif %}
<p>Thanks!</p>
<p>{{ 'Global' | Attribute:'OrganizationName' }}</p>
{{ 'Global' | Attribute:'EmailFooter' }}", // body
                SystemGuid.SystemCommunication.SIGNUP_GROUP_REMINDER, // guid
                true, // isActive
                "This is a reminder from {{ 'Global' | Attribute:'OrganizationName' }} that you have signed up for {{ ProjectName }} on {{ StartDateTime | Date:'dddd, MMM d h:mm tt' }}." // smsMessage
            );

            #endregion
        }

        /// <summary>
        /// JPH: Update public block types needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_UpdatePublicBlockTypes()
        {
            #region Updated Block Type Attributes

            #region Sign-Up Finder Block

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "Results Lava Template", "The Lava template to use to show the results of the search. Merge fields include: Projects. <span class='tip tip-lava'></span>", 0, @"{% assign projectCount = Projects | Size %}
{% if projectCount > 0 %}
    <div class=""row d-flex flex-wrap"">
        {% for project in Projects %}
            <div class=""col-md-4 col-sm-6 col-xs-12 mb-4"">
                <div class=""card h-100"">
                    <div class=""card-body"">
                        <h3 class=""card-title mt-0"">{{ project.Name }}</h3>
                        {% if project.ScheduleName and project.ScheduleName != empty %}
                            <p class=""card-subtitle text-muted mb-3"">{{ project.ScheduleName }}</p>
                        {% endif %}
                        <p class=""mb-2"">{{ project.FriendlySchedule }}</p>
                        <div class=""d-flex justify-content-between mb-3"">
                            {% if project.AvailableSpots != null %}
                                <span class=""badge badge-info"">Available Spots: {{ project.AvailableSpots }}</span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                            {% if project.DistanceInMiles != null %}
                                <span class=""badge"">{{ project.DistanceInMiles | Format:'0.0' }} miles<span>
                            {% else %}
                                &nbsp;
                            {% endif %}
                        </div>
                        {% if project.MapCenter and project.MapCenter != empty %}
                            <div class=""mb-3"">
                                {[ googlestaticmap center:'{{ project.MapCenter }}' zoom:'15' ]}
                                {[ endgooglestaticmap ]}
                            </div>
                        {% endif %}
                        {% if project.Description and project.Description != empty %}
                            <p class=""card-text"">
                                {{ project.Description }}
                            </p>
                        {% endif %}
                    </div>
                    {% if project.ScheduleHasFutureStartDateTime %}
                        <div class=""card-footer bg-white border-0"">
                            <a href=""{{ project.ProjectDetailPageUrl }}"" class=""btn btn-link btn-xs pl-0 text-muted"">Details</a>
                            <a href=""{{ project.RegisterPageUrl }}"" class=""btn btn-warning btn-xs pull-right"">Register</a>
                        </div>
                    {% endif %}
                </div>
            </div>
        {% endfor %}
    </div>
{% else %}
    <div>
        No projects found.
    </div>
{% endif %}", "5B123108-1C30-4F91-BCA1-1A91561EA07E" );

            #endregion

            #region Sign-Up Register Block

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "Workflow", "Workflow to launch when the sign-up is complete. In addition to the GroupMember entity, the following attribute keys will be passed to the Workflow: Registrar, Group, Location and Schedule. <span class='tip tip-lava'></span>", 2, "", "794B86B3-174A-4518-BCDC-1944469B8C67" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "161587D9-7B74-4D61-BF8E-3CDB38F16A12", "72ED40C7-4D64-4D60-9411-4FFB2B9E833E", "Registrant Confirmation System Communication", "RegistrantConfirmationSystemCommunication", "Registrant Confirmation System Communication", "Confirmation email to be sent to each registrant (in Family mode, only send to adults and the child if they were the registrar). Merge fields include Registrant, ProjectName, OpportunityName, FriendlyLocation, StartDateTime, Group, Location and Schedule. <span class='tip tip-lava'></span>", 3, "", "AD847694-C431-4163-AEAF-6A3A2AB4B608" );

            #endregion

            #endregion

            #region New Block Type

            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Engagement.SignUp.SignUpDetail", "Sign Up Detail", "Rock.Blocks.Engagement.SignUp.SignUpDetail, Rock.Blocks, Version=1.15.0.12, Culture=neutral, PublicKeyToken=null", false, false, "3B92EA37-579A-4928-88C4-6A6808116D40" );
            RockMigrationHelper.UpdateMobileBlockType( "Sign-Up Detail", "Block used to show the details of a sign-up group/project.", "Rock.Blocks.Engagement.SignUp.SignUpDetail", "Engagement > Sign-Up", "432123B4-8FDD-4A2E-BAF7-927C2B049CAB" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Set Page Title", "SetPageTitle", "Set Page Title", @"When enabled, sets the page title to be the name of the sign-up project.", 1, @"True", "98C213E3-F2B6-4AC6-BD6C-C47D4D333949" );
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "Registration Page", @"The page reference to pass to the Lava template for the registration page.", 0, @"", "75C5B324-A5B4-4D03-BD1B-050B5F272870" );
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
                                <span class=""label label-default"">hello</span>
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
        /// JPH: Update public blocks needed for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_UpdatePublicBlocks()
        {
            #region New Block Setting Value for Existing Block

            // [Attribute Value]: Registrant Confirmation System Communication for [Block]: Sign-Up Register for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.AddBlockAttributeValue( "9D0AF2E9-7BE2-4320-A81C-CF22D0E94BD4", "AD847694-C431-4163-AEAF-6A3A2AB4B608", "b546c11d-6c92-400f-ba56-aaa22d7bac01" );

            #endregion

            #region New Block Placement & Setting Value

            // [Block]: Sign-Up Detail for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlock( true, "7F22B3B0-64F8-47DB-A6C0-5A1CE5F68BEF".AsGuidOrNull(), null, null, "432123B4-8FDD-4A2E-BAF7-927C2B049CAB".AsGuidOrNull(), "Sign-Up Detail", "Main", "", "", 0, "98CB28AC-9E1F-4B8A-99B4-9D4502C77B7B" );
            // [Attribute Value]: Registration Page for [Block]: Sign-Up Detail for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.AddBlockAttributeValue( "98CB28AC-9E1F-4B8A-99B4-9D4502C77B7B", "75C5B324-A5B4-4D03-BD1B-050B5F272870", "bbb8a41d-e65f-4aff-8987-bff3458a46c1,e6685354-09c3-479f-8b6a-c6bd0a18a675" );

            #endregion
        }

        /// <summary>
        /// JPH: Delete public blocks added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeletePublicBlocks()
        {
            // [Attribute Value]: Registration Page for [Block]: Sign-Up Detail for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlockAttributeValue( "98CB28AC-9E1F-4B8A-99B4-9D4502C77B7B", "75C5B324-A5B4-4D03-BD1B-050B5F272870" );
            // [Block]: Sign-Up Detail for [Page]: Sign-Up Finder > Sign-Up Detail
            RockMigrationHelper.DeleteBlock( "98CB28AC-9E1F-4B8A-99B4-9D4502C77B7B" );

            // [Attribute Value]: Registrant Confirmation System Communication for [Block]: Sign-Up Register for [Page]: Sign-Up Finder > Sign-Up Register
            RockMigrationHelper.DeleteBlockAttributeValue( "9D0AF2E9-7BE2-4320-A81C-CF22D0E94BD4", "AD847694-C431-4163-AEAF-6A3A2AB4B608" );
        }

        /// <summary>
        /// JPH: Delete public block types added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeletePublicBlockTypes()
        {
            RockMigrationHelper.DeleteAttribute( "98C213E3-F2B6-4AC6-BD6C-C47D4D333949" );
            RockMigrationHelper.DeleteAttribute( "50FB027E-6C1F-41A3-9077-F98DFCAD301D" );
            RockMigrationHelper.DeleteAttribute( "75C5B324-A5B4-4D03-BD1B-050B5F272870" );

            RockMigrationHelper.DeleteBlockType( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB" );
            RockMigrationHelper.DeleteEntityType( "3B92EA37-579A-4928-88C4-6A6808116D40" );
        }

        /// <summary>
        /// JPH: Delete system communications added for Sign-Up Groups.
        /// </summary>
        private void ShortTermServingProjects_DeleteSystemCommunications()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SIGNUP_GROUP_REGISTRATION_CONFIRMATION );
        }
    }
}
