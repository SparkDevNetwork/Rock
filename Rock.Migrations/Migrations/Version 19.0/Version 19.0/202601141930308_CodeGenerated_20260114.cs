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
    public partial class CodeGenerated_20260114 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.AddContact
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.AddContact", "Add Contact", "Rock.Blocks.Types.Mobile.Engagement.AddContact, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "2742790B-031B-4CF3-9489-B84E50EA99BA" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard", "Beacon Dashboard", "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "A3D9F1C4-E3C1-4D3A-8C2E-7F4B5B6D9F1C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.ContactProfile
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.ContactProfile", "Contact Profile", "Rock.Blocks.Types.Mobile.Engagement.ContactProfile, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "5A8E8F2C-1F1E-4D3A-9C8E-3B6D9F1C4E2B" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.MyContacts
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.MyContacts", "My Contacts", "Rock.Blocks.Types.Mobile.Engagement.MyContacts, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "D1CFF2E3-0E3A-4F2D-8F1D-4C3E3F2B6F2A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.OutreachOnboarding
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.OutreachOnboarding", "Outreach Onboarding", "Rock.Blocks.Types.Mobile.Engagement.OutreachOnboarding, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "C1A3F4E2-7F4B-4D3A-8C2E-5B6D9F1C4E3C" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.OutreachRecentActivity
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.OutreachRecentActivity", "Outreach Recent Activity", "Rock.Blocks.Types.Mobile.Engagement.OutreachRecentActivity, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "73CA6269-28BA-4914-92F8-7493E732DCED" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.TouchpointDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Engagement.TouchpointDetail", "Touchpoint Detail", "Rock.Blocks.Types.Mobile.Engagement.TouchpointDetail, Rock, Version=19.0.3.0, Culture=neutral, PublicKeyToken=null", false, false, "83D3D9F1-901B-4FE6-9F83-898D1BE1BC43" );

            // Add/Update Obsidian Block Type
            //   Name:Add Contacts
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.AddContact
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Add Contacts", "Allows you to add contact.", "Rock.Blocks.Types.Mobile.Engagement.AddContact", "Engagement", "DE2F490D-2598-40E8-8170-4753DDC0A0B0" );

            // Add/Update Obsidian Block Type
            //   Name:Beacon Dashboard
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Beacon Dashboard", "Beacon dashboard allows you to view your touchpoint statistic and as well as start connecting with your contact.", "Rock.Blocks.Types.Mobile.Engagement.BeaconDashboard", "Engagement", "A1B2C3D4-E5F6-4789-ABCD-1234567890AB" );

            // Add/Update Obsidian Block Type
            //   Name:Contact Profile
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.ContactProfile
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Contact Profile", "Allow you to view the contact detail.", "Rock.Blocks.Types.Mobile.Engagement.ContactProfile", "Engagement", "40E200AD-1E29-4855-A8A4-8055A63753FF" );

            // Add/Update Obsidian Block Type
            //   Name:My Contact
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.MyContacts
            RockMigrationHelper.AddOrUpdateEntityBlockType( "My Contact", "Allows you to view and edit an existing contact.", "Rock.Blocks.Types.Mobile.Engagement.MyContacts", "Engagement", "5C8E3D6E-1F2D-4A2D-8E3C-2F3B1C6D7E8F" );

            // Add/Update Obsidian Block Type
            //   Name:Outreach Onboarding
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.OutreachOnboarding
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Outreach Onboarding", "On boarding for Outreach", "Rock.Blocks.Types.Mobile.Engagement.OutreachOnboarding", "Engagement", "5F1E3C4B-6D7E-4A8F-9C2B-3D4E5F6A7B8C" );

            // Add/Update Obsidian Block Type
            //   Name:Outreach Recent Activity
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.OutreachRecentActivity
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Outreach Recent Activity", "Recent Activity allows you to view recent touchpoints.", "Rock.Blocks.Types.Mobile.Engagement.OutreachRecentActivity", "Engagement", "469B0581-C7F6-4F8B-913F-E20F5B49E39D" );

            // Add/Update Obsidian Block Type
            //   Name:Touchpoint Detail
            //   Category:Engagement
            //   EntityType:Rock.Blocks.Types.Mobile.Engagement.TouchpointDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Touchpoint Detail", "Touchpoint Detail block allows you to connect, prayed and celebrate special events for your contacts.", "Rock.Blocks.Types.Mobile.Engagement.TouchpointDetail", "Engagement", "616DD7C7-CB7A-444E-A99E-25F2398D56EB" );

            // Attribute for BlockType
            //   BlockType: Scheduled Transaction Edit (V2)
            //   Category: Finance
            //   Attribute: Use Account Campus Mapping Logic
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F1ADF375-7442-4B30-BAC3-C387EA9B6C18", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Use Account Campus Mapping Logic", "UseAccountCampusMappingLogic", "Use Account Campus Mapping Logic", @"Controls how the selected Financial Account is mapped to the selected Campus:<ul>
    <li><b>Enabled</b> – Always use campus-based child account mapping.</li>
    <li><b>Disabled</b> – Never use campus-based child account mapping.</li>
    <li><b>Use Financial Account Setting</b> – Use mapping only if the first selected Financial Account has <em>Use Campus Child Account Matching</em> enabled.</li>
</ul>
When mapping is used:<br/>
&nbsp; - If no campus is selected, the selected account will be used.<br/>
&nbsp; - If an active direct child account matches the selected campus, it will be used.<br/>
&nbsp; - If no matching child account matches the selected campus, the selected account will be used.", 6, @"Enabled", "28893E46-D356-4274-98DA-2BA86F9F4E90" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Asset Manager
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Asset Manager", "EnableAssetManager", "Enable Asset Manager", @"Allows individuals to have access to the asset manager. This includes browsing existing files as well as modifying existing and uploading new files.", 19, @"False", "E0077DD0-4788-4108-8650-4DC21F19E861" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB756565-8A35-42E2-BC79-8D11F57E4004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"This setting filters the list of campuses by type that are displayed in the chart.", 0, @"", "2BAB1723-62F8-45C2-A682-04A07E6CAF85" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DB756565-8A35-42E2-BC79-8D11F57E4004", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This setting filters the list of campuses by statuses that are displayed in the chart.", 1, @"", "561E864E-0418-4004-A1ED-5A13CE4EE8CA" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0DCE130-0C91-4AA0-8161-57E8FA523392", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public classes will be excluded.", 5, @"True", "9EB09C20-8CE4-4900-BEC6-17D27EDE61FC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5D6BA94F-342A-4EC1-B024-FC5046FFE14D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Public Only", "PublicOnly", "Public Only", @"If selected, all non-public courses will be excluded.", 4, @"True", "F4905031-D091-4342-8A22-951F58F99FB9" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B1F65833-CECA-4054-BCC3-2DE5692741ED", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Watched Note Lava Template", "WatchedNoteLavaTemplate", "Watched Note Lava Template", @"The Lava template to use to show the watched note type. <span class='tip tip-lava'></span>", 0, @"", "619D2DBE-C74E-4E22-A2E7-57E5E88D952E" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", @"The Lava template to use to show a header above the various state templates. Merge fields include: LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 1, @"
<div class=""hero-section"">
    <div class=""hero-section-image"" style=""background-image: url('/GetImage.ashx?guid={{ LearningClass.LearningCourse.ImageBinaryFile.Guid }}')""></div>
    <div class=""hero-section-content"">
        <h1 class=""hero-section-title""> {{ LearningClass.LearningCourse.PublicName }} </h1>
        <p class=""hero-section-description""> {{ LearningClass.LearningCourse.Summary }} </p>
    </div>
</div>
", "802EF6C1-6946-4FB5-AECF-C418D9989193" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Confirmation Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirmation Lava Template", "ConfirmationLavaTemplate", "Confirmation Lava Template", @"The Lava template to use when displaying the confirmation messaging to the individual. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 2, @"
//-Variable Assignments
{% assign facilitatorCount = Facilitators | Size %}
{% assign facilitatorsText = '' %}

{% for f in Facilitators %}
    {%- capture name -%}
        {{f | Property:'Name'}}{%- unless forloop.last -%}, {% endunless %}
    {%- endcapture -%}
    {% assign facilitatorsText = facilitatorsText | Append:name %}
{% endfor %}

{% if facilitatorsText == empty %}
    {% assign facilitatorsText = 'TBD' %}
{% endif %}
{% assign credits = LearningClass.LearningCourse.Credits | AsInteger %}
{% assign location = LearningClass.GroupLocations | First %}
{% assign locationNameLength = location.Name | Size %}
{% assign schedule = LearningClass.Schedule %}
{% assign scheduleNameLength = schedule.Name | Size %}
{% assign hasLocation = locationNameLength > 0 %}
{% assign hasSchedule = scheduleNameLength > 0 %}


{% stylesheet %}
    .confirmation-details {
        width: 100%;
        max-width: 545px;
    }
    
    .detail-table {
        border: 1px solid var(--color-interface-softer);
        width: 100%;
    }
    
    .detail-row {
        border-bottom: 1px solid var(--color-interface-softer);
        padding: 8px;
        width: 100%;
    }
    
    .detail-row:last-child {
        border-bottom: none;
    }

    
{% endstylesheet %}



<div class=""d-flex flex-column w-100 justify-content-center gap-4 my-4"">
    
    //- 1 REVIEW HEADING
    <div>
        <h3 class=""text-center"">Enrollment Review</h3>
        <div class=""text-center"">Please review class details before confirming enrollment:</div>
    </div>
    
    //- 2 TABLE
    <div class=""d-flex flex-column mt-3 gap-3"">
        <div>
            <h5>Participant Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Name
                    </div>
                    <div class=""field-value"">
                        {{Registrant.FullName}}
                    </div>
                </div>
                <div class=""detail-row participant-email d-flex justify-content-between"">
                    <div class=""field-title"">
                        Email
                    </div>
                    <div class=""field-value"">
                        {{Registrant.Email}}
                    </div>
                </div>
            </div>
        </div>
        
        <div>
            <h5>Class Details</h5>
            <div class=""detail-table"">
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Name
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.PublicName}}
                    </div>
                </div>
                {% if LearningClass.LearningCourse.CourseCode != empty %}
                <div class=""detail-row  d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Code
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningCourse.CourseCode}}
                    </div>
                </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Course Configuration
                    </div>
                    <div class=""field-value "">
                        {% if LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 0 %}
                            Academic Calendar
                        {% elseif LearningClass.LearningCourse.LearningProgram.ConfigurationMode == 1 %}
                            On-Demand
                        {% endif %}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        {{ 'Facilitator' | PluralizeForQuantity:facilitatorCount }}:
                    </div>
                    <div class=""field-value"">
                        {{facilitatorsText}}
                    </div>
                </div>
        
                {% if credits > 0 %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Credits
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningCourse.Credits}}
                        </div>
                    </div>
                {% endif %}
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Grading System
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningGradingSystem.Name}}
                    </div>
                </div>
        
                <div class=""detail-row d-flex justify-content-between"">
                    <div class=""field-title"">
                        Semester
                    </div>
                    <div class=""field-value"">
                        {{LearningClass.LearningSemester.Name}}
                    </div>
                </div>
        
                {% if hasLocation %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Location
                        </div>
                        <div class=""field-value"">
                            {{location.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if hasSchedule %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Schedule
                        </div>
                        <div class=""field-value"">
                            {{schedule.Name}}
                        </div>
                    </div>
                {% endif %}
        
                {% if LearningClass.LearningSemester.StartDate %}
                    <div class=""detail-row d-flex justify-content-between"">
                        <div class=""field-title"">
                            Starts
                        </div>
                        <div class=""field-value"">
                            {{LearningClass.LearningSemester.StartDate |  Date:'sd' }}
                        </div>
                    </div>
                {% endif %}
            </div>
        </div>
    </div>

</div>

", "0D94C61E-FC32-43AE-AEFC-D154EC09984F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Completion Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Completion Lava Template", "CompletionLavaTemplate", "Completion Lava Template", @"The Lava template to use to show the completed message. Merge fields include: UnmetRequirements, LearningClass, Facilitators, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 3, @"
<div class=""completion-container d-flex flex-column justify-content-center my-5"">
    <i class=""ti ti-circle-check ti-4x text-success text-center""></i>
    <h3 class=""completion-header text-center"">Successfully Enrolled!</h3>
    <div class=""completion-sub-header text-center"">
        You are now enrolled in this class.
        Click “Go to Class Workspace” to begin your learning experience.
    </div>
</div>
", "3D976E4D-84CB-46D0-9529-ABF935A066BB" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Enrollment Error Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "E80F9006-3C00-4F36-839E-7A0883F9E229", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Enrollment Error Lava Template", "EnrollmentErrorLavaTemplate", "Enrollment Error Lava Template", @"The Lava template to use when the individual is not able to enroll. Merge fields include: ErrorKey (one of: 'unmet_course_requirements', 'class_full', 'enrollment_closed', 'already_enrolled'), UnmetRequirements, Facilitators, LearningClass, Registrant, CurrentPerson and other Common Merge Fields. <span class='tip tip-lava'></span>", 4, @"
<div class=""error-container d-flex flex-column justify-content-center my-5"">
    <i class=""ti ti-alert-triangle ti-4x text-danger text-center""></i>
    <h3 class=""error-header text-center"">Cannot Enroll in Class</h3>
    <div class=""error-sub-header text-center"">
        {% case ErrorKey %}
        {% when 'unmet_course_requirements' %}
            You have not completed the following 
            {{ 'prerequisite' | PluralizeForQuantity:UnmetRequirements }} for this course:
            <ul class=""d-inline-block"">
                {% for requirement in UnmetRequirements %}
                <li>{{requirement.RequiredLearningCourse.Name}}</li>
                {% endfor %}
            </ul>
        {% when 'class_full' %}
            This class has reached it's capacity. Please go back to the Course Detail and try again.
        {% when 'enrollment_closed' %}
            Enrollment is closed for this class.
        {% when 'already_enrolled' %}
            You're already enrolled in this class.
        {% else %}
            Something went wrong with your enrollment. Please contact the facilitator for further support.
        {% endcase %}
    </div>
</div>
", "86B1677A-0A27-4CBB-8195-7012F9C6E483" );

            // Attribute for BlockType
            //   BlockType: Add Contacts
            //   Category: Engagement
            //   Attribute: Post Save Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "DE2F490D-2598-40E8-8170-4753DDC0A0B0", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "Post Save Action", "PostSave", "Post Save Action", @"The navigation action to perform when the delete button is pressed.", 0, @"{""Type"": 1, ""PopCount"": 1}", "A319956D-AEB7-4B17-B960-A761A81CF5F9" );

            // Attribute for BlockType
            //   BlockType: Beacon Dashboard
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"The page to link to when user taps on a Start Connecting.", 1, @"", "7F10BDF3-7E96-4CA7-A7A1-B92B08074C95" );

            // Attribute for BlockType
            //   BlockType: Beacon Dashboard
            //   Category: Engagement
            //   Attribute: My Contact Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "My Contact Page", "MyContact", "My Contact Page", @"The page to link to when user taps on a contact button", 2, @"", "ED8C54C5-AE39-4C2C-99E5-2755725F3C3C" );

            // Attribute for BlockType
            //   BlockType: My Contact
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C8E3D6E-1F2D-4A2D-8E3C-2F3B1C6D7E8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Contact Page", "AddContact", "Add Contact Page", @"Page to link to when user taps on the plus button.", 0, @"", "6B7EE3D9-9770-4CE2-8857-E3E72B069E43" );

            // Attribute for BlockType
            //   BlockType: My Contact
            //   Category: Engagement
            //   Attribute: Contact Profile
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5C8E3D6E-1F2D-4A2D-8E3C-2F3B1C6D7E8F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Contact Profile", "ContactProfile", "Contact Profile", @"Page to link to when the user taps on the contact.", 1, @"", "48A37D4A-E486-4341-98A7-E8DBC1E70DCC" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F1E3C4B-6D7E-4A8F-9C2B-3D4E5F6A7B8C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Contact Page", "AddContact", "Add Contact Page", @"Page to link to when user taps on the plus button.", 0, @"", "5B35F2C8-0D2C-4835-BD97-402B2C4E85B0" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: After Finish Action
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5F1E3C4B-6D7E-4A8F-9C2B-3D4E5F6A7B8C", "8AF3E49F-4FF1-47D8-BCD2-150201B7F1B8", "After Finish Action", "AfterFinishAction", "After Finish Action", @"The navigation action to perform when the delete button is pressed.", 1, @"{""Type"": 1, ""PopCount"": 1}", "3F79A3D5-262B-411B-A806-DD0B4DCF6DBE" );

            // Attribute for BlockType
            //   BlockType: Touchpoint Detail
            //   Category: Engagement
            //   Attribute: Baptism Info
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "616DD7C7-CB7A-444E-A99E-25F2398D56EB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Baptism Info", "BaptismInfo", "Baptism Info", @"URL to navigate to when in the pulse touchpoint baptism questionnaire.", 0, @"", "8D3F9A27-AA9F-454D-AE85-37E21484975C" );

            // Add Block Attribute Value
            //   Block: Note Watch Detail
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Block Location: Page=Note Watch Detail, Site=Rock RMS
            //   Attribute: Watched Note Lava Template
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "4CA5CB96-5423-41A2-AC84-1ABF618B990F", "619D2DBE-C74E-4E22-A2E7-57E5E88D952E", @"" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attribute for BlockType
            //   BlockType: Touchpoint Detail
            //   Category: Engagement
            //   Attribute: Baptism Info
            RockMigrationHelper.DeleteAttribute( "8D3F9A27-AA9F-454D-AE85-37E21484975C" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: After Finish Action
            RockMigrationHelper.DeleteAttribute( "3F79A3D5-262B-411B-A806-DD0B4DCF6DBE" );

            // Attribute for BlockType
            //   BlockType: Outreach Onboarding
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.DeleteAttribute( "5B35F2C8-0D2C-4835-BD97-402B2C4E85B0" );

            // Attribute for BlockType
            //   BlockType: My Contact
            //   Category: Engagement
            //   Attribute: Contact Profile
            RockMigrationHelper.DeleteAttribute( "48A37D4A-E486-4341-98A7-E8DBC1E70DCC" );

            // Attribute for BlockType
            //   BlockType: My Contact
            //   Category: Engagement
            //   Attribute: Add Contact Page
            RockMigrationHelper.DeleteAttribute( "6B7EE3D9-9770-4CE2-8857-E3E72B069E43" );

            // Attribute for BlockType
            //   BlockType: Beacon Dashboard
            //   Category: Engagement
            //   Attribute: My Contact Page
            RockMigrationHelper.DeleteAttribute( "ED8C54C5-AE39-4C2C-99E5-2755725F3C3C" );

            // Attribute for BlockType
            //   BlockType: Beacon Dashboard
            //   Category: Engagement
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute( "7F10BDF3-7E96-4CA7-A7A1-B92B08074C95" );

            // Attribute for BlockType
            //   BlockType: Add Contacts
            //   Category: Engagement
            //   Attribute: Post Save Action
            RockMigrationHelper.DeleteAttribute( "A319956D-AEB7-4B17-B960-A761A81CF5F9" );

            // Attribute for BlockType
            //   BlockType: Note Watch Detail
            //   Category: Core
            //   Attribute: Watched Note Lava Template
            RockMigrationHelper.DeleteAttribute( "619D2DBE-C74E-4E22-A2E7-57E5E88D952E" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Statuses
            RockMigrationHelper.DeleteAttribute( "561E864E-0418-4004-A1ED-5A13CE4EE8CA" );

            // Attribute for BlockType
            //   BlockType: Tithing Overview
            //   Category: Reporting
            //   Attribute: Campus Types
            RockMigrationHelper.DeleteAttribute( "2BAB1723-62F8-45C2-A682-04A07E6CAF85" );

            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Communication
            //   Attribute: Enable Asset Manager
            RockMigrationHelper.DeleteAttribute( "E0077DD0-4788-4108-8650-4DC21F19E861" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course List
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "F4905031-D091-4342-8A22-951F58F99FB9" );

            // Attribute for BlockType
            //   BlockType: Public Learning Course Detail
            //   Category: LMS
            //   Attribute: Public Only
            RockMigrationHelper.DeleteAttribute( "9EB09C20-8CE4-4900-BEC6-17D27EDE61FC" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Enrollment Error Lava Template
            RockMigrationHelper.DeleteAttribute( "86B1677A-0A27-4CBB-8195-7012F9C6E483" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Completion Lava Template
            RockMigrationHelper.DeleteAttribute( "3D976E4D-84CB-46D0-9529-ABF935A066BB" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Confirmation Lava Template
            RockMigrationHelper.DeleteAttribute( "0D94C61E-FC32-43AE-AEFC-D154EC09984F" );

            // Attribute for BlockType
            //   BlockType: Public Learning Class Enrollment
            //   Category: LMS
            //   Attribute: Header Lava Template
            RockMigrationHelper.DeleteAttribute( "802EF6C1-6946-4FB5-AECF-C418D9989193" );

            // Delete BlockType 
            //   Name: Touchpoint Detail
            //   Category: Engagement
            //   Path: -
            //   EntityType: Touchpoint Detail
            RockMigrationHelper.DeleteBlockType( "616DD7C7-CB7A-444E-A99E-25F2398D56EB" );

            // Delete BlockType 
            //   Name: Outreach Recent Activity
            //   Category: Engagement
            //   Path: -
            //   EntityType: Outreach Recent Activity
            RockMigrationHelper.DeleteBlockType( "469B0581-C7F6-4F8B-913F-E20F5B49E39D" );

            // Delete BlockType 
            //   Name: Outreach Onboarding
            //   Category: Engagement
            //   Path: -
            //   EntityType: Outreach Onboarding
            RockMigrationHelper.DeleteBlockType( "5F1E3C4B-6D7E-4A8F-9C2B-3D4E5F6A7B8C" );

            // Delete BlockType 
            //   Name: My Contact
            //   Category: Engagement
            //   Path: -
            //   EntityType: My Contacts
            RockMigrationHelper.DeleteBlockType( "5C8E3D6E-1F2D-4A2D-8E3C-2F3B1C6D7E8F" );

            // Delete BlockType 
            //   Name: Contact Profile
            //   Category: Engagement
            //   Path: -
            //   EntityType: Contact Profile
            RockMigrationHelper.DeleteBlockType( "40E200AD-1E29-4855-A8A4-8055A63753FF" );

            // Delete BlockType 
            //   Name: Beacon Dashboard
            //   Category: Engagement
            //   Path: -
            //   EntityType: Beacon Dashboard
            RockMigrationHelper.DeleteBlockType( "A1B2C3D4-E5F6-4789-ABCD-1234567890AB" );

            // Delete BlockType 
            //   Name: Add Contacts
            //   Category: Engagement
            //   Path: -
            //   EntityType: Add Contact
            RockMigrationHelper.DeleteBlockType( "DE2F490D-2598-40E8-8170-4753DDC0A0B0" );


            RockMigrationHelper.DeleteEntityType( "A3D9F1C4-E3C1-4D3A-8C2E-7F4B5B6D9F1C" );
        }
    }
}
