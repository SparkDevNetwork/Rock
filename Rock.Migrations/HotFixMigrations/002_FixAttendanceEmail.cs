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
using Rock.Plugin;

namespace Rock.Migrations.HotFixMigrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 2, "1.3.2" )]
    public class FixAttendanceEmail : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
// NOTE: This was included in normal migration with v4.0
/*
            Sql( @"
    UPDATE [SystemEmail]
    SET [Body] = REPLACE( [Body], '?GroupId={{ Group.Id }}&Occurrence=', '?{{ Person.ImpersonationParameter }}&GroupId={{ Group.Id }}&Occurrence=' )
    WHERE [Guid] = 'ED567FDE-A3B4-4827-899D-C2740DF3E5DA'
" );
*/
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
