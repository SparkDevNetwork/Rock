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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Security;

    
    /// <summary>
    ///
    /// </summary>
    public partial class Thing : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Calendar Administration", "Group of individuals who can administrate the various parts of the calendar functionality.", Rock.SystemGuid.Group.GROUP_CALENDAR_ADMINISTRATORS );

            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CONNECTIONS, 0, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS, 0, "C3D16431-E179-4BE0-BB97-3806C6EF43E0" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CONNECTIONS, 0, Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS, 0, "31E0CC2C-D132-4125-8A61-B9EFC824A018" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 0, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_CALENDAR_ADMINISTRATORS, 0, "D0FF6E3F-92D4-43C3-8A02-89E68DC62424" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.CALENDARS, 0, Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_CALENDAR_ADMINISTRATORS, 0, "EC2476BC-8689-4187-8A64-234F1040C502" );           
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
