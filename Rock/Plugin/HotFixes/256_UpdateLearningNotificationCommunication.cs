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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 256, "17.3" )]
    public class UpdateLearningNotificationCommunication : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLearningActivityAvailableSystemCommunicationUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateLearningActivityAvailableSystemCommunicationDown();
        }

        #region KH: Update the Learning Activity Available System Communication

        private void UpdateLearningActivityAvailableSystemCommunicationUp()
        {
            // d40a9c32-f179-4e5e-9b0d-ce208c5d1870 is Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
UPDATE [SystemCommunication]
    SET Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your available activities as of {{ currentDate }}.
</p>

<h2 style=""margin:0; margin-bottom: 16px"">
	{{ Program.ProgramName }}
</h2>
{% for course in Courses %}
	{% assign availableActivities = course.Classes | SelectMany:''Activities'' %}
	{% assign availableActivitiesCount = availableActivities | Size %}
	{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}
	<h3 style=""margin: 0 0 0 16px;"">{{ course.CourseName }} {% if course.CourseCode and course.CourseCode != empty %}- {{ course.CourseCode }}{% endif %} </h3>

	{% for class in course.Classes %}
		{% assign orderedActivities = class.Activities | OrderBy:''Order'' %}
		{% for activity in orderedActivities %}
			<p style=""margin: 16px 0 16px 32px;"">
				<strong>Activity:</strong>
				<a href=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}learn/{{ Program.ProgramIdKey }}/courses/{{ course.CourseIdKey }}/{{ class.ClassIdKey }}?activity={{ activity.LearningClassActivityIdKey }}"">{{ activity.ActivityName }}</a>
					{% if activity.AvailableDate and activity.AvailableDate != empty %}
						(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
					{% endif %}
				<br />
				{% if activity.DueDate and activity.DueDate != empty %}
					<strong>Due:</strong>
					{{ activity.DueDate | HumanizeDateTime }}
				{% endif %}
			</p>
		{% endfor %}
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
',
    SMSMessage = '
New {{ Program.ProgramName }} {%if ActivityCount > 1 %}Activities{%else%}Activity{%endif%} Available:{% for course in Courses %}{% assign availableActivities = course.Classes | SelectMany:''Activities'' %}{% assign availableActivitiesCount = availableActivities | Size %}{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}

{{ course.CourseName }}{% if course.CourseCode and course.CourseCode != empty %} - {{ course.CourseCode }}{% endif %}:{% for class in course.Classes %}{% assign orderedActivities = class.Activities | OrderBy:''Order'' %}{% for activity in orderedActivities %}

Activity: {{ activity.ActivityName }}
{% if activity.AvailableDate and activity.AvailableDate != empty %}Available: {{ activity.AvailableDate | Date: ''MMM d'' }}{% endif %}{% if activity.DueDate and activity.DueDate != empty %}{% if activity.AvailableDate and activity.AvailableDate != empty %} | {% endif %}Due: {{ activity.DueDate | HumanizeDateTime }} {% endif %}Link: {{ ''Global'' | Attribute:''PublicApplicationRoot'' }}learn/{{ Program.ProgramIdKey }}/courses/{{ course.CourseIdKey }}/{{ class.ClassIdKey }}?activity={{ activity.LearningClassActivityIdKey }}{% endfor %}{% endfor %}{% endfor %}
'
WHERE [Guid] = @activityCommunicationGuid;" );
        }

        private void UpdateLearningActivityAvailableSystemCommunicationDown()
        {
            // d40a9c32-f179-4e5e-9b0d-ce208c5d1870 is Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_NOTIFICATIONS
            Sql( @"
DECLARE @activityCommunicationGuid UNIQUEIDENTIFIER = 'd40a9c32-f179-4e5e-9b0d-ce208c5d1870';
UPDATE [SystemCommunication]
    SET Body = '
{% assign currentDate = ''Now'' | Date:''MMMM d, yyyy'' %}
{{ ''Global'' | Attribute:''EmailHeader'' }}
<h1 style=""margin:0;"">
	Your Activities
</h1>

<p>
    Below are your available activities as of {{ currentDate }}.
</p>

{% for course in Courses %}
	{% assign availableActivitiesCount = course.Activities | Size %}
	{% if availableActivitiesCount == 0 %}{% continue %}{% endif %}
	<h2> {{ course.ProgramName }}: {{ course.CourseName }} {% if course.CourseCode and course.CourseCode != empty %}- {{ course.CourseCode }}{% endif %} </h2>

	{% assign orderedActivities = course.Activities | OrderBy:''Order'' %}
	{% for activity in orderedActivities %}
		<p class=""mb-4"">
			<strong>Activity:</strong>
			{{ activity.ActivityName }}
				{% if activity.AvailableDate and activity.AvailableDate != empty %}
					(available {{ activity.AvailableDate | Date: ''MMM dd'' }})
				{% endif %}
			<br />
			{% if activity.DueDate and activity.DueDate != empty %}
			    <strong>Due:</strong>
				{{ activity.DueDate | HumanizeDateTime }}
			{% endif %}
		</p>	
	{% endfor %}
{% endfor %}

{{ ''Global'' | Attribute:''EmailFooter'' }}
',
    SMSMessage = NULL
WHERE [Guid] = @activityCommunicationGuid;" );
        }
    }

    #endregion
}