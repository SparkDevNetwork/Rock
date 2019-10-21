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
    public partial class BadgeNamespace : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Remove the records that Rock created because it saw the new code. We want to keep the old records rather than 
            // create new records because of their associated attributes and values
            RemoveByNamespace( "Rock.Badge.Component." );

            // Rename the namespace with the greater specificity first so that the renames do not interfere with each other
            RenameEntityTypeNamespace( "Rock.PersonProfile.AlertNote.", "Rock.Badge.Component." ); // AlertNote had it's own namespace
            RenameEntityTypeNamespace( "Rock.PersonProfile.Badge.", "Rock.Badge.Component." ); // All the other badge components
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove the records that Rock created because it saw the old(?) code
            RemoveByNamespace( "Rock.PersonProfile.Badge." );
            RemoveByNamespace( "Rock.PersonProfile.AlertNote." );

            // Rename the namespace with the greater specificity first so that the renames do not interfere with each other
            RenameEntityTypeNamespace( "Rock.Badge.Component.AlertNote", "Rock.PersonProfile.AlertNote.AlertNote" ); // AlertNote had it's own namespace
            RenameEntityTypeNamespace( "Rock.Badge.Component.", "Rock.PersonProfile.Badge." ); // All the other badge components
        }

        /// <summary>
        /// Renames the entity type namespace within the matching entity type records.
        /// </summary>
        /// <param name="oldNamespace">The old name space.</param>
        /// <param name="newNamespace">The new name space.</param>
        private void RenameEntityTypeNamespace( string oldNamespace, string newNamespace )
        {
            Sql(
$@"UPDATE EntityType SET Name = REPLACE(Name, '{oldNamespace}', '{newNamespace}') WHERE Name LIKE '{oldNamespace}%';
UPDATE EntityType SET AssemblyName = REPLACE(AssemblyName, '{oldNamespace}', '{newNamespace}') WHERE AssemblyName LIKE '{oldNamespace}%';"
            );
        }

        /// <summary>
        /// Removes by namespace.
        /// </summary>
        /// <param name="newNameSpace">The new name space.</param>
        private void RemoveByNamespace( string nameSpace )
        {         
            Sql( $"DELETE FROM EntityType WHERE Name LIKE '{nameSpace}%';" );
        }
    }
}
