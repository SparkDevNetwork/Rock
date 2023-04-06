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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230323 : Rock.Migrations.RockMigration
    {

        private static readonly string JobClassName = "PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianBlocks";
        private static readonly string FullyQualifiedJobClassName = $"Rock.Jobs.{JobClassName}";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixSignUpGroupsQAFindingsUp();
            UpdateEmailRemindersToUseAvatars();
            UpdateDefaultChartColors();
            FixRaceAndEthnicityLabelsForGroupTypes();
            MobileSearchBlockTemplateUp();
            RemoveOutdatedCSVBlockUp();
            ReplaceWebFormsBlocksWithObsidianBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FixSignUpGroupsQAFindingsDown();
            MobileSearchBlockTemplateDown();
            ReplaceWebFormsBlocksWithObsidianBlocksDown();
        }

        /// <summary>
        /// JPH: Fix Sign-Ups QA Findings Up.
        /// </summary>
        private void FixSignUpGroupsQAFindingsUp()
        {
            #region Update Pages and Routes

            Sql( @"
-- Admin: Sign-Ups (root feature page) --

DECLARE @PageId [int] = (SELECT [Id] FROM [Page] WHERE [Guid] = '1941542C-21F2-4341-BDE1-996AA1E0C0A2');

UPDATE [Page]
SET [InternalName] = 'Sign-Ups'
    , [PageTitle] = 'Sign-Ups'
    , [BrowserTitle] = 'Sign-Ups'
WHERE [Id] = @PageId;

DECLARE @PageRouteId [int] = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '75332FE3-DB1B-4B83-9287-5EDDD09A1A4E');

UPDATE [PageRoute]
SET [Route] = 'people/signups'
WHERE [Id] = @PageRouteId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'A9EEE819-13CA-425C-9118-65B421BC9FEB');

UPDATE [PageRoute]
SET [Route] = 'people/sign-ups'
WHERE [Id] = @PageRouteId;

-- Admin: Sign-Up Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'D6AEFD09-630E-40D7-AA56-77CC904C6595');

UPDATE [PageRoute]
SET [Route] = 'people/signups/{GroupId}'
WHERE [Id] = @PageRouteId;

-- Admin: Sign-Up Opportunity Attendee List --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'DB9C7E0D-5EC7-4CC2-BA9E-DD398D4B9714');

UPDATE [PageRoute]
SET [Route] = 'people/signups/{GroupId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Admin: [Sign-Up] Group Member Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '40566DCD-AC73-4C61-95B3-8F9B2E06528C');

UPDATE [PageRoute]
SET [Route] = 'people/signups/{GroupId}/location/{LocationId}/schedule/{ScheduleId}/member/{GroupMemberId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Ups Finder --

SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2DC1906D-C9D5-411D-B961-A2295C9450A4');

UPDATE [Page]
SET [InternalName] = 'Sign-Ups Finder'
    , [PageTitle] = 'Sign-Ups Finder'
    , [BrowserTitle] = 'Sign-Ups Finder'
WHERE [Id] = @PageId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '37B68603-6D07-4C37-A89A-253DB72DBBE3');

UPDATE [PageRoute]
SET [Route] = 'signups'
WHERE [Id] = @PageRouteId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '6D5797CC-7179-41DC-BF49-8BE5DEB6B40D');

UPDATE [PageRoute]
SET [Route] = 'sign-ups'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '7E53FB15-B4C1-4339-AFAB-48B403EDE875');

UPDATE [PageRoute]
SET [Route] = 'signups/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Register --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'E6685354-09C3-479F-8B6A-C6BD0A18A675');

UPDATE [PageRoute]
SET [Route] = 'signups/register/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Attendance Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '33DE9AF2-456C-413D-8559-A58DEA78D62A');

UPDATE [PageRoute]
SET [Route] = 'signups/attendance/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;" );

            #endregion

            #region Update Existing Block Settings

            // Sign-Up Finder - Results Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "Results Lava Template", "The Lava template to use to show the results of the search.", 0, @"{% assign projectCount = Projects | Size %}
{% if projectCount > 0 %}
    <div class=""row d-flex flex-wrap"">
        {% for project in Projects %}
            <div class=""col-xs-12 col-sm-6 col-md-4 mb-4"">
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
                    <div class=""card-footer bg-white border-0"">
                        {% if project.ShowRegisterButton %}
                            <a href=""{{ project.RegisterPageUrl }}"" class=""btn btn-primary btn-xs"">Register</a>
                        {% endif %}
                        <a href=""{{ project.ProjectDetailPageUrl }}"" class=""btn btn-link btn-xs"">Details</a>
                    </div>
                </div>
            </div>
        {% endfor %}
    </div>
{% else %}
    <div>
        No projects found.
    </div>
{% endif %}", "5B123108-1C30-4F91-BCA1-1A91561EA07E" );

            // Sign-Up Finder - Results Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Header Lava Template", "ResultsHeaderLavaTemplate", "Results Header Lava Template", "The Lava Template to use to show the results header.", 0, @"<h3>Results</h3>
<p>Below is a listing of the projects that match your search results.</p>
<hr>", "DF0ADA6D-FF88-4569-957A-EF1509333BF5" );

            // Sign-Up Details - Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "432123B4-8FDD-4A2E-BAF7-927C2B049CAB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"The Lava template to use to show the details of the project. Merge fields include: Project. <span class='tip tip-lava'></span>", 2, @"{% if Project != null %}
    <div class=""panel panel-block"">
        <div class=""panel-heading"">
            <h1 class=""panel-title"">{{ Project.Name }}</h1>
        </div>
        <div class=""panel-body"">
            <div class=""row"">
                <div class=""col-md-6"">
                    <div class=""d-flex justify-content-between align-items-center"">
                        <h4>{{ Project.Name }}</h4>
                        {% if Project.CampusName and Project.CampusName != empty %}
                            <span class=""label label-default"">{{ Project.CampusName }}</span>
                        {% endif %}
                    </div>
                    {% if Project.ScheduleName and Project.ScheduleName != empty %}
                        <p class=""text-muted"">{{ Project.ScheduleName }}</p>
                    {% endif %}
                    {% if Project.Description and Project.Description != empty %}
                        <p>{{ Project.Description }}</p>
                    {% endif %}
                    {% if Project.AvailableSpots != null %}
                        <p>
                            <span class=""badge badge-info"">Available Spots: {{ Project.AvailableSpots }}</span>
                        </p>
                    {% endif %}
                    {% if Project.ShowRegisterButton %}
                        <div class=""actions"">
                            <a href=""{{ Project.RegisterPageUrl }}"" class=""btn btn-primary"">Register</a>
                        </div>
                    {% endif %}
                </div>
                {% if Project.MapCenter and Project.MapCenter != empty %}
                    <div class=""col-md-6 mt-3 mt-md-0"">
                        {[ googlestaticmap center:'{{ Project.MapCenter }}' zoom:'15' ]}
                        {[ endgooglestaticmap ]}
                    </div>
                {% endif %}
            </div>
        </div>
    </div>
{% endif %}", "50FB027E-6C1F-41A3-9077-F98DFCAD301D" );

            // Sign-Up Attendance Detail - Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "96D160D9-5668-46EF-9941-702BD3A577DB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", "The Lava template to show at the top of the page.", 0, @"<h3>{{ Group.Name }}</h3>
<div>
    Please enter attendance for the project below.
    <br>Date: {{ AttendanceOccurrenceDate | Date:'dddd, MMM d' }}
    {% if WasScheduleParamProvided %}
        <br>Schedule: {{ ScheduleName }}
    {% endif %}
    {% if WasLocationParamProvided %}
        <br>Location: {{ LocationName }}
    {% endif %}
</div>
<hr>", "0B3B0549-2353-4B06-A8F6-9C52135AB235" );

            #endregion

            #region Add Send Sign-Up Reminders Job

            Sql( @"
IF NOT EXISTS
(
    SELECT [Id]
    FROM [ServiceJob]
    WHERE [Guid] = '51D3FEAD-BEE7-493B-87F0-1E7CB12FCE6F'
)
BEGIN
    DECLARE @Now [datetime] = (SELECT GETDATE());

    INSERT INTO [ServiceJob]
    (
        [IsSystem]
        , [IsActive]
        , [Name]
        , [Description]
        , [Class]
        , [CronExpression]
        , [NotificationStatus]
        , [Guid]
        , [CreatedDateTime]
        , [ModifiedDateTime]
        , [HistoryCount]
    )
    VALUES
    (
        0
        , 1
        , 'Send Sign-Up Reminders'
        , 'Send any sign-up reminders that are due to be sent.'
        , 'Rock.Jobs.SendSignUpReminders'
        , '0 0 8 1/1 * ? *'
        , 1
        , '51D3FEAD-BEE7-493B-87F0-1E7CB12FCE6F'
        , @Now
        , @Now
        , 500
    );
END" );

            #endregion
        }

        /// <summary>
        /// JPH: Fix Sign-Ups QA Findings Down.
        /// </summary>
        private void FixSignUpGroupsQAFindingsDown()
        {
            #region Remove Send Sign-Up Reminders Job

            Sql( @"
DELETE FROM [ServiceJob]
WHERE [Guid] = '51D3FEAD-BEE7-493B-87F0-1E7CB12FCE6F';" );

            #endregion

            #region Revert Pages and Routes

            Sql( @"
-- Admin: Sign-Ups (root feature page) --

DECLARE @PageId [int] = (SELECT [Id] FROM [Page] WHERE [Guid] = '1941542C-21F2-4341-BDE1-996AA1E0C0A2');

UPDATE [Page]
SET [InternalName] = 'Sign-Up'
    , [PageTitle] = 'Sign-Up'
    , [BrowserTitle] = 'Sign-Up'
WHERE [Id] = @PageId;

DECLARE @PageRouteId [int] = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '75332FE3-DB1B-4B83-9287-5EDDD09A1A4E');

UPDATE [PageRoute]
SET [Route] = 'people/signup'
WHERE [Id] = @PageRouteId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'A9EEE819-13CA-425C-9118-65B421BC9FEB');

UPDATE [PageRoute]
SET [Route] = 'people/sign-up'
WHERE [Id] = @PageRouteId;

-- Admin: Sign-Up Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'D6AEFD09-630E-40D7-AA56-77CC904C6595');

UPDATE [PageRoute]
SET [Route] = 'people/signup/{GroupId}'
WHERE [Id] = @PageRouteId;

-- Admin: Sign-Up Opportunity Attendee List --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'DB9C7E0D-5EC7-4CC2-BA9E-DD398D4B9714');

UPDATE [PageRoute]
SET [Route] = 'people/signup/{GroupId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Admin: [Sign-Up] Group Member Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '40566DCD-AC73-4C61-95B3-8F9B2E06528C');

UPDATE [PageRoute]
SET [Route] = 'people/signup/{GroupId}/location/{LocationId}/schedule/{ScheduleId}/member/{GroupMemberId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Ups Finder --

SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '2DC1906D-C9D5-411D-B961-A2295C9450A4');

UPDATE [Page]
SET [InternalName] = 'Sign-Up Finder'
    , [PageTitle] = 'Sign-Up Finder'
    , [BrowserTitle] = 'Sign-Up Finder'
WHERE [Id] = @PageId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '37B68603-6D07-4C37-A89A-253DB72DBBE3');

UPDATE [PageRoute]
SET [Route] = 'signup'
WHERE [Id] = @PageRouteId;

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '6D5797CC-7179-41DC-BF49-8BE5DEB6B40D');

UPDATE [PageRoute]
SET [Route] = 'sign-up'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '7E53FB15-B4C1-4339-AFAB-48B403EDE875');

UPDATE [PageRoute]
SET [Route] = 'signup/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Register --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = 'E6685354-09C3-479F-8B6A-C6BD0A18A675');

UPDATE [PageRoute]
SET [Route] = 'signup/register/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;

-- Public: Sign-Up Attendance Detail --

SET @PageRouteId = (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '33DE9AF2-456C-413D-8559-A58DEA78D62A');

UPDATE [PageRoute]
SET [Route] = 'signup/attendance/{ProjectId}/location/{LocationId}/schedule/{ScheduleId}'
WHERE [Id] = @PageRouteId;" );

            #endregion

            #region Revert Existing Block Settings

            // Sign-Up Finder - Results Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Lava Template", "ResultsLavaTemplate", "Results Lava Template", "The Lava template to use to show the results of the search.", 0, @"{% assign projectCount = Projects | Size %}
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

            // Sign-Up Finder - Results Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "74A20402-00DF-4A87-98D1-B5A8920F1D32", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Header Lava Template", "ResultsHeaderLavaTemplate", "Results Header Lava Template", "The Lava Template to use to show the results header.", 0, @"<h3>Results</h3>
<p>Below is a listing of the projects that match your search results.</p>
<hr class=""mb-5"" />", "DF0ADA6D-FF88-4569-957A-EF1509333BF5" );

            // Sign-Up Details - Lava Template
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

            // Sign-Up Attendance Detail - Header Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "96D160D9-5668-46EF-9941-702BD3A577DB", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Header Lava Template", "HeaderLavaTemplate", "Header Lava Template", "The Lava template to show at the top of the page.", 0, @"<h3>{{ Group.Name }}</h3>
<div>
    Please enter attendance for the project below.
    <br />Date: {{ AttendanceOccurrenceDate | Date:'dddd, MMM d' }}
    {% if WasScheduleParamProvided %}
        <br />Schedule: {{ ScheduleName }}
    {% endif %}
    {% if WasLocationParamProvided %}
        <br />Location: {{ LocationName }}
    {% endif %}
</div>", "0B3B0549-2353-4B06-A8F6-9C52135AB235" );

            #endregion
        }

        /// <summary>
        /// GJ: Update Email Reminders to Use Avatars
        /// </summary>
        private void UpdateEmailRemindersToUseAvatars()
        {
            Sql( MigrationSQL._202303231713546_Rollup_20230323_UpdateEmailReminderstoUseAvatars );
        }

        /// <summary>
        /// GJ: Update Default Chart Colors
        /// </summary>
        private void UpdateDefaultChartColors()
        {
            Sql( MigrationSQL._202303231713546_Rollup_20230323_ChartColorUpdate );
        }

        /// <summary>
        /// GJ: Fix Race / Ethnicity labels for group types
        /// </summary>
        private void FixRaceAndEthnicityLabelsForGroupTypes()
        {
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display Race on Children", "", 0, "Hide", "12FFAC55-F8C4-4B73-91A4-D7CAE30CFE3D", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONCHILDREN );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display Ethnicity on Children", "", 0, "Hide", "8B3B904E-981B-4257-ACF3-D06B57BBF93D", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONCHILDREN );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display Race on Adults", "", 0, "Hide", "8408517A-4738-4F8F-91DB-D743CC0070AF", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYRACEONADULTS );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525c4cb-ee6b-41d4-9b64-a08048d5a5c0", "GroupTypePurposeValueId", "142", "Display Ethnicity on Adults", "", 0, "Hide", "BA9EEB6F-0C35-4392-9049-3723473523D9", Rock.SystemKey.GroupTypeAttributeKey.CHECKIN_REGISTRATION_DISPLAYETHNICITYONADULTS );

        }

        /// <summary>
        /// BC: Mobile Search Block Template Updates
        /// Updates the mobile search block default template XAML.
        /// </summary>
        private void MobileSearchBlockTemplateUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Update the XAML template.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
               "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3",
               Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH,
               "Default",
               @"{% assign photoUrl = null %}
{% assign itemName = null %}
{% assign itemText = null %}

{% if ItemType == 'Person' %}
    {% capture photoUrl %}{% if Item.PhotoId != null %}{{ Item.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ Item.PhotoUrl | Escape }}{% endif %}{% endcapture %}
    {% assign itemName = Item.FullName %}
    {% assign itemText = Item.Email %}
{% else %}
    {% assign itemName = Item | AsString %}
{% endif %}

<StackLayout
    Spacing=""0"">
    <Frame StyleClass=""search-item-container"" 
        Margin=""0""
        BackgroundColor=""White""
        HasShadow=""false""
        HeightRequest=""40"">
            <StackLayout Orientation=""Horizontal""
                Spacing=""0""
                VerticalOptions=""Center"">
                <Rock:Image Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}""
                    StyleClass=""search-image""
                    VerticalOptions=""Start""
                    Aspect=""AspectFit""
                    Margin=""0, 4, 8, 0""
                    BackgroundColor=""#e4e4e4"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <StackLayout Spacing=""0"" 
                    HorizontalOptions=""FillAndExpand"">
                    <StackLayout Orientation=""Horizontal""
                    VerticalOptions=""Center"">
                        <Label StyleClass=""search-item-name""
                            Text=""{{ itemName }}""
                            LineBreakMode=""TailTruncation""
                            HorizontalOptions=""FillAndExpand"" />

                        <Grid ColumnSpacing=""4"" 
                            RowSpacing=""0""
                            ColumnDefinitions=""*, Auto""
                            VerticalOptions=""Start"">

                            <Rock:Icon IconClass=""chevron-right""
                                VerticalTextAlignment=""Start""
                                Grid.Column=""1"" 
                                StyleClass=""note-read-more-icon""
                                />
                        </Grid>
                    </StackLayout>
                    {% if itemText == null or itemText == """" %}
                        {% assign itemText = ""No Email"" %}
                    {% endif %}
                        <Label StyleClass=""search-item-text""
                            Grid.Column=""0""
                            MaxLines=""2""
                            LineBreakMode=""TailTruncation"">{{ itemText | XamlWrap }}</Label> 
                </StackLayout>
            </StackLayout>
        </Frame>
    <BoxView HorizontalOptions=""FillAndExpand""
        HeightRequest=""1""
        Color=""#cccccc"" />

    <StackLayout.GestureRecognizers>
        <TapGestureRecognizer Command=""{Binding AggregateCommand}"">
            <TapGestureRecognizer.CommandParameter>
                <Rock:AggregateCommandParameters>
                    <Rock:CommandReference Command=""{Binding {{ DetailNavigationActionType }}}"" CommandParameter=""{{ DetailNavigationActionPage }}?{{ ItemType | Append:'Guid' }}={{ Item.Guid }}"" />
                    <Rock:CommandReference Command=""{Binding AppendSearchHistory}"">
                        <Rock:CommandReference.CommandParameter>
                            <Rock:AppendToSearchHistoryParameters Guid=""{{ Item.Guid }}""
                                DetailKey=""{{ ItemType | Append:'Guid' }}"" >
                                <Rock:Parameter Name=""Name"" Value=""{{ itemName }}"" />
                                
                                {% if photoUrl != null and photoUrl != """" %}
                                <Rock:Parameter Name=""PhotoUrl"" Value=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}"" />
                                {% endif %}
                                <Rock:Parameter Name=""Text"" Value=""{{ itemText }}"" />
                            </Rock:AppendToSearchHistoryParameters>
                        </Rock:CommandReference.CommandParameter>
                    </Rock:CommandReference>
                </Rock:AggregateCommandParameters>
            </TapGestureRecognizer.CommandParameter>
        </TapGestureRecognizer>
    </StackLayout.GestureRecognizers>
</StackLayout>",
               standardIconSvg,
               "standard-template.svg",
               "image/svg+xml" );
        }

        /// <summary>
        /// BC: Mobile Search Block Template Updates
        /// Reverts the mobile search block default template XAML.
        /// </summary>
        private void MobileSearchBlockTemplateDown()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            // Revert to the old XAML.
            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
               "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3",
               Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH,
               "Default",
               @"{% assign photoUrl = null %}
{% assign itemName = null %}
{% assign itemText = null %}

{% if ItemType == 'Person' %}
    {% capture photoUrl %}{% if Item.PhotoId != null %}{{ Item.PhotoUrl | Append:'&width=200' | Escape }}{% else %}{{ Item.PhotoUrl | Escape }}{% endif %}{% endcapture %}
    {% assign itemName = Item.FullName %}
    {% assign itemText = Item.Email %}
{% else %}
    {% assign itemName = Item | AsString %}
{% endif %}

<StackLayout Spacing=""0"">
    <StackLayout Orientation=""Horizontal"" StyleClass=""search-result-content"">
        {% if photoUrl != null %}
            <Rock:Image StyleClass=""search-result-image""
                Source=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ photoUrl }}"">
                <Rock:CircleTransformation />
            </Rock:Image>
        {% endif %}
        
        <StackLayout Spacing=""0""
            HorizontalOptions=""FillAndExpand""
            VerticalOptions=""Center"">
            <Label StyleClass=""search-result-name""
                Text=""{{ itemName | Escape }}""
                HorizontalOptions=""FillAndExpand"" />
    
            {% if itemText != null and itemText != '' %}
                <Label StyleClass=""search-result-text"">{{ itemText | XamlWrap }}</Label>
            {% endif %}
        </StackLayout>
        
        <Rock:Icon IconClass=""chevron-right""
            VerticalOptions=""Center""
            StyleClass=""search-result-detail-arrow"" />
    </StackLayout>

    <Rock:Divider />
</StackLayout>",
               standardIconSvg,
               "standard-template.svg",
               "image/svg+xml" );
        }

        /// <summary>
        /// PA: Remove the outdated CSV Block
        /// The Migration to delete the outdated CSV Import Block for the database
        /// </summary>
        private void RemoveOutdatedCSVBlockUp()
        {
            // Delete BlockType CSV Import Block with Path as ~/Blocks/CSVImport/CSVImport.ascx
            RockMigrationHelper.DeleteBlockType( "EDA8F90D-1201-4AFF-9E6D-A8F6D6F618D9" ); // Outdated CSV Import Block with the path as ~/Blocks/CSVImport/CSVImport.ascx
        }

        /// <summary>
        /// JMH: Creates the run-once job that replaces WebForms blocks with Obsidian blocks.
        /// </summary>
        private void ReplaceWebFormsBlocksWithObsidianBlocksUp()
        {
            // Configure run-once job by modifying these variables.
            var commandTimeout = 14000;
            var blockTypeReplacements = new Dictionary<string, string>
  {
        // Group Attendance Detail Block Type
        { SystemGuid.BlockType.GROUP_ATTENDANCE_DETAIL, "308DBA32-F656-418E-A019-9D18235027C1" }
  };
            var shouldNotDeleteOldBlocks = false;

            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{FullyQualifiedJobClassName}' AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}' )
    BEGIN
        INSERT INTO [ServiceJob] (
            [IsSystem]
            ,[IsActive]
            ,[Name]
            ,[Description]
            ,[Class]
            ,[CronExpression]
            ,[NotificationStatus]
            ,[Guid] )
        VALUES ( 
            0
            ,1
            ,'Rock Update Helper v15.0 - Replace WebForms Blocks with Obsidian Blocks'
            ,'This job will replace WebForms blocks with their Obsidian blocks on all sites, pages, and layouts.'
            ,'{FullyQualifiedJobClassName}'
            ,'0 0 21 1/1 * ? *'
            ,1
            ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}'
            );
    END" );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Command Timeout
            var commandTimeoutAttributeGuid = "F4C7151F-864A-4E36-8AF7-79D27DB41C07";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.INTEGER, "Class", FullyQualifiedJobClassName, "Command Timeout", "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, "14000", commandTimeoutAttributeGuid, "CommandTimeout" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, commandTimeoutAttributeGuid, commandTimeout.ToString() );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Block Type Guid Replacement Pairs
            var blockTypeReplacementsAttributeGuid = "9431CD4D-A25A-4730-8724-5D107C6CDDA5";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", FullyQualifiedJobClassName, "Block Type Guid Replacement Pairs", "Block Type Guid Replacement Pairs", "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.", 1, "", blockTypeReplacementsAttributeGuid, "BlockTypeGuidReplacementPairs" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, blockTypeReplacementsAttributeGuid, SerializeDictionary( blockTypeReplacements ) );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianBlocks: Should Keep Old Blocks
            var shouldKeepOldBlocksAttributeGuid = "A1B097B3-310B-445E-ADED-80AB1EFFCEC6";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", FullyQualifiedJobClassName, "Should Keep Old Blocks", "Should Keep Old Blocks", "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.", 2, "False", shouldKeepOldBlocksAttributeGuid, "ShouldKeepOldBlocks" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS, shouldKeepOldBlocksAttributeGuid, shouldNotDeleteOldBlocks.ToTrueFalse() );
        }

        /// <summary>
        /// JMH: Removes the run-once job that replaces WebForms blocks with Obsidian blocks.
        /// </summary>
        private void ReplaceWebFormsBlocksWithObsidianBlocksDown()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_BLOCKS}'" );
            Sql( $"DELETE V FROM [AttributeValue] V INNER JOIN [Attribute] A ON A.[Id] = V.[AttributeId] WHERE A.[EntityTypeQualifierColumn] = 'Class' AND A.[EntityTypeQualifierValue] = '{FullyQualifiedJobClassName}" );
            Sql( $"DELETE FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'Class' AND [EntityTypeQualifierValue] = '{FullyQualifiedJobClassName}" );
        }

        private string SerializeDictionary( Dictionary<string, string> dictionary )
        {
            const string keyValueSeparator = "^";

            if ( dictionary?.Any() != true )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var first = dictionary.First();
            sb.Append( $"{first.Key}{keyValueSeparator}{first.Value}" );

            foreach ( var kvp in dictionary.Skip( 1 ) )
            {
                sb.Append( $"|{kvp.Key}{keyValueSeparator}{kvp.Value}" );
            }

            return sb.ToString();
        }
    }
}
