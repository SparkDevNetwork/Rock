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
    public partial class AddHighlightColorToGradingScale : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.LearningGradingSystemScale", "HighlightColor", c => c.String( maxLength: 50 ) );

            // Ensure that any data that extends beyond the allowed range is first shortened.
            Sql( "UPDATE LearningCourse SET CourseCode = LEFT(CourseCode, 12)" );
            AlterColumn( "dbo.LearningCourse", "CourseCode", c => c.String( maxLength: 12 ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn( "dbo.LearningCourse", "CourseCode", c => c.String() );
            DropColumn( "dbo.LearningGradingSystemScale", "HighlightColor" );
        }
    }
}
