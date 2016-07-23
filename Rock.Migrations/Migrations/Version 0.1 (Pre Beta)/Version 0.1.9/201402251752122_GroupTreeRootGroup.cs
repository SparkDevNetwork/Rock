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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GroupTreeRootGroup : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE
 [Attribute]
SET
 [Name] = 'Root Group'
 ,[Key] = 'RootGroup'
 ,[Description] = 'Select the root group to use as a starting point for the tree view.'
WHERE
 [Guid] = '0E1768CD-87CC-4361-8BCD-01981FBFE24B'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
UPDATE
 [Attribute]
SET
 [Name] = 'Group'
 ,[Key] = 'Group'
 ,[Description] = 'Select the root group to use as a starting point for the tree view.'
WHERE
 [Guid] = '0E1768CD-87CC-4361-8BCD-01981FBFE24B'" );

        }
    }
}
