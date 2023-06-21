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
    public partial class AddAdultAttendanceTypeAttributeToFamilyAttendanceBadge : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// The Migration to add a new parameter ShowAsIndividual to the Badge Attendance Stored Procedure
        /// This was required to toggle between individual and family attendance for Adults.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._202305262246526_AddAdultAttendanceTypeAttributeToFamilyAttendanceBadge );
        }

        /// <summary>
        /// As this was run as part of plugin migration <see cref="Rock.Plugin.HotFixes.AddAdultAttendanceTypeAttributeToFamilyAttendanceBadge"/> we are skipping the down
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
