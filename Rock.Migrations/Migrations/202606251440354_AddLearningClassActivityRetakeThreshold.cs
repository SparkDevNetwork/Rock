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

    /// <summary>
    ///
    /// </summary>
    public partial class AddLearningClassActivityRetakeThreshold : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddLearningClassActivityRetakeThreshold_Up();
            JPH_AddRetakeRequiredSystemCommunication_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddRetakeRequiredSystemCommunication_Down();
            JPH_AddLearningClassActivityRetakeThreshold_Down();
        }

        /// <summary>
        /// JPH: Add the RetakeThreshold column to the LearningClassActivity table - up.
        /// </summary>
        private void JPH_AddLearningClassActivityRetakeThreshold_Up()
        {
            AddColumn( "dbo.LearningClassActivity", "RetakeThreshold", c => c.Int() );
        }

        /// <summary>
        /// JPH: Add the RetakeThreshold column to the LearningClassActivity table - down.
        /// </summary>
        private void JPH_AddLearningClassActivityRetakeThreshold_Down()
        {
            DropColumn( "dbo.LearningClassActivity", "RetakeThreshold" );
        }

        /// <summary>
        /// JPH: Add the "Retake Required" system communication - up.
        /// </summary>
        private void JPH_AddRetakeRequiredSystemCommunication_Up()
        {
            var body = @"{{ 'Global' | Attribute:'EmailHeader' }}
<p>
    You did not receive a passing grade on {{ Activity.ActivityName }}. A retake has been assigned. Please complete the activity below to receive credit.
</p>
<p>
    <strong>Activity:</strong>
    <a href=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}learn/{{ Program.ProgramIdKey }}/courses/{{ Course.CourseIdKey }}/{{ Class.ClassIdKey }}?activity={{ Activity.LearningClassActivityIdKey }}"">{{ Activity.ActivityName }}</a>
    {% if Activity.DueDate and Activity.DueDate != empty %}
    <br />
    <strong>Due:</strong>
    {{ Activity.DueDate | HumanizeDateTime }}
    {% endif %}
</p>
{{ 'Global' | Attribute:'EmailFooter' }}";

            RockMigrationHelper.UpdateSystemCommunication(
                "Learning Management", // category
                "Retake Required", // title
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Assessment Graded: Retake Required", // subject
                body, // body
                Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_RETAKE_REQUIRED );
        }

        /// <summary>
        /// JPH: Add the "Retake Required" system communication - down.
        /// </summary>
        private void JPH_AddRetakeRequiredSystemCommunication_Down()
        {
            RockMigrationHelper.DeleteSystemCommunication( Rock.SystemGuid.SystemCommunication.LEARNING_ACTIVITY_RETAKE_REQUIRED );
        }
    }
}
