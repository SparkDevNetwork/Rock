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
    public partial class ComputedSundayDate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: Sunday Date
            Sql( MigrationSQL._201510082215386_ComputedSundayDate_ufnUtility_GetSundayDate );

            Sql( @"
ALTER TABLE Attendance ADD SundayDate AS (dbo.ufnUtility_GetSundayDate(StartDateTime)) persisted
CREATE INDEX IX_SundayDate ON Attendance (SundayDate)
ALTER TABLE FinancialTransaction ADD SundayDate AS (dbo.ufnUtility_GetSundayDate(TransactionDateTime)) persisted
CREATE INDEX IX_SundayDate ON FinancialTransaction (SundayDate)
" );

            // DT - Update merge person proc
            Sql( MigrationSQL._201510082215386_ComputedSundayDate_spCrm_PersonMerge );

            // JE - Security for Group Member Notes
            Sql( MigrationSQL._201510082215386_ComputedSundayDate_SecurityForGroupMemberNotes );
            
            // MP/DT: Add IsPrivateNote to Note
            AddColumn( "dbo.Note", "IsPrivateNote", c => c.Boolean( nullable: false ) );

            Sql( MigrationSQL._201510082215386_ComputedSundayDate_PrivateNoteMigration );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.Note", "IsPrivateNote" );

            Sql( "DROP INDEX [IX_SundayDate] ON [dbo].[Attendance]" );
            Sql( "DROP INDEX [IX_SundayDate] ON [dbo].[FinancialTransaction]" );
            DropColumn( "dbo.FinancialTransaction", "SundayDate" );
            DropColumn( "dbo.Attendance", "SundayDate" );
        }
    }
}
