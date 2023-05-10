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
    public partial class FixMigrationEncoding_Rollup_20230323 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixSignUpGroupsQAFindingsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            FixSignUpGroupsQAFindingsDown();
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
DECLARE @Now [datetime] = (SELECT GETDATE());

IF EXISTS
(
    SELECT [Id]
    FROM [ServiceJob]
    WHERE [Guid] = '51D3FEAD-BEE7-493B-87F0-1E7CB12FCE6F'
)
BEGIN
    UPDATE [ServiceJob]
    SET [Name] = 'Send Sign-Up Reminders'
        , [Description] = 'Send any sign-up reminders that are due to be sent.'
        , [Class] = 'Rock.Jobs.SendSignUpReminders'
        , [CronExpression] = '0 0 8 1/1 * ? *'
        , [ModifiedDateTime] = @Now
    WHERE [Guid] = '51D3FEAD-BEE7-493B-87F0-1E7CB12FCE6F';
END
ELSE
BEGIN
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
    }
}
