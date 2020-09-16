// <copyright>
// Copyright by the Central Christian Church
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
using System;
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 28, "1.9.4" )]
    public class Bugfixes155 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"begin transaction
Declare @DuplicateGuids table(
DuplicateGuid uniqueidentifier,
DuplicateCount int
)

Insert into @DuplicateGuids
Select Guid, GuidCount
From (
Select [Guid], count(0) as GuidCount
From _com_centralaz_RoomManagement_ReservationResource
Group By [Guid]
) t1
Where GuidCount > 1

Insert into @DuplicateGuids
Select Guid, GuidCount
From (
Select [Guid], count(0) as GuidCount
From _com_centralaz_RoomManagement_ReservationLocation
Group By [Guid]
) t1
Where GuidCount > 1

Update _com_centralaz_RoomManagement_ReservationResource
Set Guid = NewId()
Where Guid in ( Select DuplicateGuid from @DuplicateGuids)

Update _com_centralaz_RoomManagement_ReservationLocation
Set Guid = NewId()
Where Guid in ( Select DuplicateGuid from @DuplicateGuids)

commit transaction" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}