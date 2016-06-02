// <copyright>
// Copyright by Central Christian Church
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

using Rock.Plugin;

namespace com.centralaz.Utility.Migrations
{
    [MigrationNumber( 1, "1.0.8" )]
    public class AddAutoAssignNumberBlock : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Auto Assign Number", "Automatically increments and assigns the next number for a configured person attribute.", "~/Plugins/com_centralaz/Utility/AutoAssignNumber.ascx", "com_centralaz > Utility", "84D8D97A-8FC3-43CD-8AA0-E56A064B760E " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlockType( "84D8D97A-8FC3-43CD-8AA0-E56A064B760E" ); // Auto Assign Number
        }
    }
}
