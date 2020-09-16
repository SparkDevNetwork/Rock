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
    [MigrationNumber( 25, "1.6.0" )]
    public class Bugfixes150 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "com.centralaz.RoomManagement.Model.ReservationLocation", 0, "Edit", true, null, 1, "CE7DF09D-39EF-421F-8304-C025CF9680DD" );
            RockMigrationHelper.AddSecurityAuthForEntityType( "com.centralaz.RoomManagement.Model.ReservationResource", 0, "Edit", true, null, 1, "DBBA7014-31E1-4EF1-AE4B-69DA377076B0" );

            var lavaCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );
            RockMigrationHelper.UpdateDefinedValue( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Calendar", "The calendar view for the Room Management home page.", "0D3367F1-62AE-4B2A-BD64-193940CB343D", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D3367F1-62AE-4B2A-BD64-193940CB343D", "466DC361-B813-445A-8883-FED7E5D4229B", @"{% include '~/Plugins/com_centralaz/RoomManagement/Assets/Lava/ReservationCalendar.lava' %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D3367F1-62AE-4B2A-BD64-193940CB343D", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F", lavaCommands );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "CE7DF09D-39EF-421F-8304-C025CF9680DD" );
            RockMigrationHelper.DeleteSecurityAuth( "DBBA7014-31E1-4EF1-AE4B-69DA377076B0" );
        }
    }
}