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
    public partial class MiscConfigChanges : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // insert route for security roles

            Sql(@"INSERT INTO [PageRoute]
                       ([IsSystem]
                       ,[PageId]
                       ,[Route]
                       ,[Guid])
                 VALUES
                       (1
                       ,110
                       ,'SecurityRoles'
                       ,'EE9D6A48-9CFC-48FE-9D88-00A65065C432')");

            // add better descriptions for record statuses
            Sql(@"UPDATE [DefinedValue]
	            SET [Description] = 'Denotes an individual that is actively participating in the activities or services of the organization.'
	            WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'");

            Sql(@"UPDATE [DefinedValue]
	            SET [Description] = 'Represents a person who is no longer participating in the activities or services of the organization.'
	            WHERE [Guid] = '1DAD99D5-41A9-4865-8366-F269902B80A4'");

            Sql(@"UPDATE [DefinedValue]
	            SET [Description] = 'Is used by the system to mark a record that needs to be verified before becoming active.  This state is often used when someone registers on-line to allow a staff person to confirm the new individual and check that it is not a duplicate record.'
	            WHERE [Guid] = '283999EC-7346-42E3-B807-BCE9B2BABB49'");

            Sql(@"UPDATE [Site]
	                SET [Name] = 'Rock RMS'
	                WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4'");

            Sql(@"UPDATE [Site]
	                SET [Name] = 'Rock Check-in'
	                WHERE [Guid] = '15AEFC01-ACB3-4F5D-B83E-AB3AB7F2A54A'");

            Sql(@"UPDATE [Site]
	                SET [Name] = 'Rock Solid Church'
	                WHERE [Guid] = 'F3F82256-2D66-432B-9D67-3552CD2F4C2B'");

            Sql(@"UPDATE [Site]
	                SET [Name] = 'Rock Attended Check-in'
	                WHERE [Guid] = '30FB46F7-4814-4691-852A-04FB56CC07F0'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
