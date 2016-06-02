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
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Constants;

namespace com.centralaz.Prayerbook.Migrations
{
    [MigrationNumber( 4, "1.4.5" )]
    public class EntryHistory : Migration
    {
        public override void Up()
        {
            // Add history categories
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Prayer", "", "", com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_PRAYER );
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Up Team Entry", "", "", com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_UPTEAM_ENTRY );
        }

        public override void Down()
        {
            // Delete the categories
            RockMigrationHelper.DeleteCategory( com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_UPTEAM_ENTRY );
            RockMigrationHelper.DeleteCategory( com.centralaz.Prayerbook.SystemGuid.Category.HISTORY_PRAYER );
        }
    }
}
