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
using Rock.Model;

namespace Rock.Migrations
{
    /// <summary>
    /// A null structure type used to mean "Any Attendance", but now it means "None". Set those old null values to the new
    /// Any Attendance enum value to maintain the same settings.
    /// </summary>
    public partial class AdjustStreakStructureTypes : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $"UPDATE StreakType SET StructureType = {( int ) StreakStructureType.AnyAttendance} WHERE StructureType IS NULL;" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"UPDATE StreakType SET StructureType = NULL WHERE StructureType = {( int ) StreakStructureType.AnyAttendance};" );
        }
    }
}
