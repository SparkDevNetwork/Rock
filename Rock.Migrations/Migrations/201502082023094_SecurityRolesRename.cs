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
    public partial class SecurityRolesRename : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"-- change group type and name of the background check administration group
UPDATE [Group]
	SET [Name] = 'RSR - Background Check Administration'
		, [GroupTypeId] = 1
		, [IsSecurityRole] = 1
	WHERE [Guid] = 'A6BCC49E-103F-46B0-8BAC-84EA03FF04D5'

UPDATE [GroupMember]
	SET [GroupRoleId] = 1
	WHERE [GroupId] = (SELECT TOP 1 [Id] FROM [Group] WHERE [Guid] = 'A6BCC49E-103F-46B0-8BAC-84EA03FF04D5')


UPDATE [Group]
	SET [Name] = 'RSR - Rock Administration'
	WHERE [Name] = 'Rock Administrators'

UPDATE [Group]
	SET [Name] = 'RSR - Staff Workers'
	WHERE [Name] = 'Staff Users'

UPDATE [Group]
	SET [Name] = 'WEB - Administration'
	WHERE [Name] = 'Website Administrators'

UPDATE [Group]
	SET [Name] = 'WEB - General Editor'
	WHERE [Name] = 'Website Content Editors'

UPDATE [Group]
	SET [Name] = 'RSR - Finance Administration'
	WHERE [Name] = 'Finance Administrators'

UPDATE [Group]
	SET [Name] = 'RSR - Finance Worker'
	WHERE [Name] = 'Finance Users'

UPDATE [Group]
	SET [Name] = 'RSR - Communication Administration'
	WHERE [Name] = 'Communication Administrators'

UPDATE [Group]
	SET [Name] = 'RSR - Staff Like Workers'
	WHERE [Name] = 'Staff-Like Users'

UPDATE [Group]
	SET [Name] = 'RSR - Prayer Administration'
	WHERE [Name] = 'Prayer Administrators'

UPDATE [Group]
	SET [Name] = 'RSR - Prayer Access'
	WHERE [Name] = 'Prayer Team'

UPDATE [Group]
	SET [Name] = 'APP - Check-in Devices'
	WHERE [Name] = 'Check-in Devices'

UPDATE [Group]
	SET [Name] = 'RSR - Pastoral Workers'
	WHERE [Name] = 'Pastoral Staff'

UPDATE [Group]
	SET [Name] = 'RSR - Data Integrity Worker'
	WHERE [Name] = 'Data Integrity User'

UPDATE [Group]
	SET [Name] = 'RSR - Safety & Security Workers'
	WHERE [Name] = 'Safety & Security Team'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
