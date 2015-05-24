// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    [MigrationNumber( 3, "1.3.4" )]
    public class FixAttendanceEmail : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
    CREATE NONCLUSTERED INDEX [IX_LocationId_ScheduleId_GroupId_StartDateTime] ON [dbo].[Attendance]
    (
	    [LocationId] ASC,
	    [ScheduleId] ASC,
	    [GroupId] ASC,
	    [StartDateTime] ASC
    )
    INCLUDE ( 	
        [DidAttend],
	    [PersonAliasId],
	    [DidNotOccur]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            Sql( @"
    DROP INDEX [IX_LocationId_ScheduleId_GroupId_StartDateTime] ON [dbo].[Attendance]
" );
        }
    }
}
