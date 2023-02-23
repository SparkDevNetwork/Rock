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
    public partial class PersonNoteColors : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPersonNoteColors();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// N.A.: Migration to Add default colors for Person Note types which are empty/default
        /// </summary>
        private void AddPersonNoteColors()
        {
            Sql( @"
                -- Personal Note: #009CE3
                UPDATE [NoteType] SET [BorderColor] = '#009CE3' WHERE [Guid] = '66a1b9d7-7efa-40f3-9415-e54437977d60' AND [BorderColor] IS NULL

                -- Pastoral Note: #EE7624
                UPDATE [NoteType] SET [BorderColor] = '#EE7624' WHERE [Guid] = '5B7BE8F4-481B-4BA3-99A2-B26307A2BA42' AND [BorderColor] IS NULL

                --Event Registration: #15BE85
                UPDATE [NoteType] SET [BorderColor] = '#15BE85' WHERE [Guid] = 'bbada8ef-23fc-4b46-b7a7-0f6d31f8c045' AND [BorderColor] IS NULL

                -- Phone Note: #83758F
                UPDATE [NoteType] SET [BorderColor] = '#83758F' WHERE [Guid] = 'b54f9d90-9af3-4e8a-8f33-9338c7c1287f' AND [BorderColor] IS NULL

                -- Communication Note: #FFC870
                UPDATE [NoteType] SET [BorderColor] = '#FFC870' WHERE [Guid] = '87bacb34-db87-45e0-ab60-bfabf7ceecdb' AND [BorderColor] IS NULL

                -- Also change Person CSV Import Error (Rock.SystemGuid.NoteType.PERSON_CSV_IMPORT_ERROR_NOTE) note type name and set to not be UserSelectable
                UPDATE [NoteType] SET [UserSelectable] = 0, [Name] = 'CSV Import Error' WHERE [Guid] = '4E22E9DA-06A7-45DB-9EB3-BFCB7A2A7F21'" );
        }
    }
}
