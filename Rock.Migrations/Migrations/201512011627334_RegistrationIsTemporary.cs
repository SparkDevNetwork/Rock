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
    public partial class RegistrationIsTemporary : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Registration", "IsTemporary", c => c.Boolean(nullable: false, defaultValue: false ) );

            // update the 'liquid' badge to be 'lava'
            Sql( @"UPDATE [EntityType]
	                SET [FriendlyName] = 'Lava'
	                WHERE [Guid] = '95912004-62B5-4460-951F-D752427D44FE'" );

            RockMigrationHelper.AddSecurityAuthForBlock( "9382B285-3EF6-47F7-94BB-A47C498196A3", 0, "Edit", true, SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, Model.SpecialRole.None, "52EEC3EA-3404-91B4-4650-2A97BD7A04A3" );
            RockMigrationHelper.AddSecurityAuthForBlock( "9382B285-3EF6-47F7-94BB-A47C498196A3", 0, "Edit", true, SystemGuid.Group.GROUP_FINANCE_USERS, Model.SpecialRole.None, "E4C158D7-E72B-039A-4021-426419973D09" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Registration", "IsTemporary");
        }
    }
}
