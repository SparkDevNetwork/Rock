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
    public partial class UpdateOutreachContactNoteColumn : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Contact", "ContactNote", c => c.String( maxLength: 500 ) );

            // Migrate existing note data into the new combined ContactNote column.
            // If both notes have values they are joined with a newline, then
            // truncated to 500 characters to satisfy the column constraint.
            Sql( @"
UPDATE [dbo].[Contact]
SET [ContactNote] =
    CASE
        WHEN [ConnectionNote] IS NOT NULL AND LEN([ConnectionNote]) > 0
             AND [PrayerNote] IS NOT NULL AND LEN([PrayerNote]) > 0
            THEN LEFT([ConnectionNote] + CHAR(10) + [PrayerNote], 500)
        WHEN [ConnectionNote] IS NOT NULL AND LEN([ConnectionNote]) > 0
            THEN LEFT([ConnectionNote], 500)
        WHEN [PrayerNote] IS NOT NULL AND LEN([PrayerNote]) > 0
            THEN LEFT([PrayerNote], 500)
        ELSE NULL
    END
WHERE ([ConnectionNote] IS NOT NULL AND LEN([ConnectionNote]) > 0)
   OR ([PrayerNote] IS NOT NULL AND LEN([PrayerNote]) > 0)" );

            DropColumn( "dbo.Contact", "ConnectionNote" );
            DropColumn( "dbo.Contact", "PrayerNote" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Contact", "PrayerNote", c => c.String(maxLength: 500));
            AddColumn("dbo.Contact", "ConnectionNote", c => c.String(maxLength: 500));
            DropColumn("dbo.Contact", "ContactNote");
        }
    }
}
