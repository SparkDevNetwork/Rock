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
    public partial class FixBirthDay : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixPersonBirthDateColumn();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Fixes the person birth date column.
        /// </summary>
        private void FixPersonBirthDateColumn()
        {
            // Drop the index
            Sql( @"
                IF (SELECT COUNT([object_id]) FROM sys.indexes WHERE name = 'IX_BirthDate' AND object_id = OBJECT_ID('Person') ) = 1
                BEGIN
                    DROP INDEX [IX_BirthDate] ON [Person]
                END" );

            // SQL Server won't let us do this in one batch because BirthDate is a computed column. Msg 271
            // Drop the column
            Sql( @"
                IF (SELECT COUNT([object_id]) FROM sys.columns WHERE [is_computed] = 1 AND [object_id] = OBJECT_ID('Person') AND [name] = 'BirthDate') = 1
                BEGIN
	                ALTER TABLE [dbo].[Person]
	                DROP COLUMN BirthDate;
                END" );

            // Add the column
            Sql( @"
                IF (SELECT COUNT([object_id]) FROM sys.columns WHERE [object_id] = OBJECT_ID('Person') AND [name] = 'BirthDate') = 0
                BEGIN
	                ALTER TABLE [dbo].[Person]
	                ADD BirthDate DATE NULL;
                END" );

            // populate the column
            Sql( @"UPDATE Person
	                SET [BirthDate]  = (SELECT CASE 
		                WHEN [BirthYear] IS NOT NULL 
		                THEN TRY_CONVERT([date],(((CONVERT([varchar],[BirthYear])+'-')+CONVERT([varchar],[BirthMonth]))+'-')+CONVERT([varchar],[BirthDay]),(126))  
		                END)
	                FROM Person" );

            // Create the index again
            Sql( @"
                IF (SELECT COUNT([object_id]) FROM sys.indexes WHERE [name] = 'IX_BirthDate' AND [object_id] = OBJECT_ID('Person') ) = 0
                BEGIN
                    CREATE INDEX [IX_BirthDate] ON [Person] ([BirthDate])
                END" );
        }
    }
}
