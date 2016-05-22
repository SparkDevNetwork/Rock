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
    public partial class ConnectionStatusCleanup : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"INSERT INTO [DefinedValue] 
	                    ([IsSystem], [DefinedTypeId], [Order], [Name], [Description], [Guid])
	                VALUES
		                ('0', '4', '3', 'Participant', 'Is involved in non-service or ministry events or programs.', NEWID())");

            Sql(@"UPDATE [DefinedValue]
	                SET [Order] = 0
		                , [Description] = 'Applied to individuals who have completed all requirements established to become a member.'
		                , [IsSystem] = 0
	                WHERE [Guid] = '41540783-D9EF-4C70-8F1D-C9E83D91ED5F'");

            Sql(@"UPDATE [DefinedValue]
	                SET [Order] = 1
		                , [Description] = 'Denotes an individual who is a consistently active participant in your services and/or ministry events.'
		                , [IsSystem] = 0
	                WHERE [Guid] = '39F491C5-D6AC-4A9B-8AC0-C431CB17D588'");

            Sql(@"UPDATE [DefinedValue]
	                SET [Order] = 2
		                , [Description] = 'Used when a person first enters through your first-time visitor process. As they continue to attend they will become an attendee and possibly a member.'
		                , [IsSystem] = 0
	                WHERE [Guid] = 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2'");

            Sql(@"UPDATE [DefinedValue]
	                SET [Order] = 4
		                , [Description] = 'The default status given to a record that is added from the website.'
		                , [IsSystem] = 0
	                WHERE [Guid] = '368DD475-242C-49C4-A42C-7278BE690CC2'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
