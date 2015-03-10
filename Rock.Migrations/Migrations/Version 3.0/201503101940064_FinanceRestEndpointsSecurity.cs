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
    
    /// <summary>
    ///
    /// </summary>
    public partial class FinanceRestEndpointsSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.DefinedTypesController", 0, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, Model.SpecialRole.None, "56BA42F4-3F83-4C61-92EF-80482788353D" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.DefinedTypesController", 1, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_USERS, Model.SpecialRole.None, "FCC6D349-A034-464C-8720-135F2DE22E97" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.CampusesController", 0, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, Model.SpecialRole.None, "B958AADE-A35B-4433-A013-7FE4DBD40FC4" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.CampusesController", 1, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_USERS, Model.SpecialRole.None, "3CC62B43-1E8C-45FF-A731-FF2D41D46E97" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.CampusController", 0, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, Model.SpecialRole.None, "29C68393-A3C6-4C2B-9063-E3BFC1CDB8B8" );
            RockMigrationHelper.AddSecurityAuthForRestController( "Rock.Rest.Controllers.CampusController", 1, Rock.Security.Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_FINANCE_USERS, Model.SpecialRole.None, "99F884F3-23F5-4D23-A0DF-2B5866124D02" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
