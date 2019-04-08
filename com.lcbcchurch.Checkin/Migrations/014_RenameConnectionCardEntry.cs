// <copyright>
// Copyright by LCBC Church
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Checkin.Migrations
{
    [MigrationNumber( 14, "1.0.14" )]
    public class RenameConnectionCardEntry : Migration
    {
        public override void Up()
        {
            Sql( @"
                Update [BlockType]
                Set Path = '~/Plugins/com_bemadev/Checkin/WeekendAttendanceEntry.ascx',
                    Name = 'Weekend Attendance Entry'
                Where [Guid] = '449C6C4A-112F-401D-8903-19D6D0C68962'
            " );

            Sql( @" Update [Page]
                   Set  [InternalName] = 'Weekend Attendance Entry',
                        [PageTitle] = 'Weekend Attendance Entry',
                        [BrowserTitle] = 'Weekend Attendance Entry'
                   Where [Guid] = '9F6D0A84-6FF7-44C8-B650-DCCC0D8C2D19'
            " );

            Sql( @" Update [Block]
                   Set Name = 'Weekend Attendance Entry'
                   Where [Guid] = '56057841-D64E-477E-90E4-073EF819BDFC'
            " );
        }
        public override void Down()
        {
           
        }
    }
}
