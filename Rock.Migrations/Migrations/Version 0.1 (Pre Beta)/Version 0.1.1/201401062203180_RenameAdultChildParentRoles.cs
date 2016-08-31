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
    public partial class RenameAdultChildParentRoles : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Child'
                WHERE 
                    [Guid] = 'F87DF00F-E86D-4771-A3AE-DBF79B78CF5D'");

            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Parent'
                WHERE 
                    [Guid] = '6F3FADC4-6320-4B54-9CF6-02EF9586A660'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Adult Child'
                WHERE 
                    [Guid] = 'F87DF00F-E86D-4771-A3AE-DBF79B78CF5D'");

            Sql(@"
                UPDATE 
                    [GroupTypeRole]
                SET 
                    [Name] = 'Adult Parent Of'
                WHERE 
                    [Guid] = '6F3FADC4-6320-4B54-9CF6-02EF9586A660'");
        }
    }
}
