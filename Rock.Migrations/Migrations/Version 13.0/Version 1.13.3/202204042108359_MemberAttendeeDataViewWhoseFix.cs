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
    public partial class MemberAttendeeDataViewWhoseFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"UPDATE DataView
                    SET Description = N'Lists people whose record status is ''Active'' and connection status is ''Member'' or ''Attendee''.'
                    WHERE Guid = 'cb4bb264-a1f4-4edb-908f-2ccf3a534bc7' AND
                        Description = N'Lists people whos record status is ''Active'' and connection status is ''Member'' or ''Attendee''.'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"UPDATE DataView
                    SET Description = N'Lists people whos record status is ''Active'' and connection status is ''Member'' or ''Attendee''.'
                    WHERE Guid = 'cb4bb264-a1f4-4edb-908f-2ccf3a534bc7' AND
                        Description != N'Lists people whos record status is ''Active'' and connection status is ''Member'' or ''Attendee''.'" );
        }
    }
}
