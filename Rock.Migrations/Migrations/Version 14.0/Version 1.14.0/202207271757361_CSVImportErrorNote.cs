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
    public partial class CSVImportErrorNote : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // PA: Add New Note Type "Import CSV Error" for Person
            RockMigrationHelper.AddOrUpdateNoteTypeByMatchingNameAndEntityType( "People CSV Import Error",
                "Rock.Model.Person",
                true,
                Rock.SystemGuid.NoteType.PERSON_CSV_IMPORT_ERROR_NOTE,
                true,
                "fa fa-warning",
                true );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE [NoteType] WHERE [Guid] = '{Rock.SystemGuid.NoteType.PERSON_CSV_IMPORT_ERROR_NOTE}'" );
        }
    }
}
