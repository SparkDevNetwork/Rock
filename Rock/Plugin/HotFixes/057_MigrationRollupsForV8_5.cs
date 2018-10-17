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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 57, "1.8.4" )]
    public class MigrationRollupsForV8_5 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdatespCheckin_BadgeAttendance();
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Pull Request: Add migration for the item that updated via this Pull Request
        /// Asana https://app.asana.com/0/517335866408894/843680915532957
        /// </summary>
        private void UpdatespCheckin_BadgeAttendance()
        {
            Sql( HotFixMigrationResource._057_MigrationRollupsForV8_5_spCheckin_BadgeAttendance );
        }
    }
}
