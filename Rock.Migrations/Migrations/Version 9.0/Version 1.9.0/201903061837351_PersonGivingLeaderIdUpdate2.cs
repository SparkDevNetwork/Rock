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
    public partial class PersonGivingLeaderIdUpdate2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixPersonGivingLeaderIdPersistedIndexed();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Fixes the Person GivingLeaderID column to be NOT NULL so that it matches the Not-Null Person.GivingLeaderId in Person.cs
        /// </summary>
        private void FixPersonGivingLeaderIdPersistedIndexed()
        {
            // Drop the index if it already exists so that we can alter the column
            Sql( @"IF EXISTS (
		SELECT *
		FROM sys.indexes
		WHERE name = 'IX_GivingLeaderId'
			AND object_id = OBJECT_ID('Person')
		)
BEGIN
	DROP INDEX IX_GivingLeaderId ON [Person]
END
" );
            // set the Default value to 0 so that we can make it not null
            // Stuff inserted thru Rock will set this correctly to a non-zero value, but we'll make a default of 0 so that person records added thru SQL won't get an exception
            Sql( @"IF NOT EXISTS (
		SELECT *
		FROM sys.columns
		WHERE name = 'GivingLeaderId'
			AND object_id = object_id('dbo.Person')
			AND object_definition(default_object_id) IS NOT NULL
		)
BEGIN
	ALTER TABLE Person ADD DEFAULT(0)
	FOR GivingLeaderId
END" );

            Sql( @"
ALTER TABLE Person ALTER COLUMN GivingLeaderId INT NOT NULL

CREATE INDEX IX_GivingLeaderId ON [Person] ([GivingLeaderId])
" );


        }
    }
}
