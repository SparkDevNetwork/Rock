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
    public partial class AddMissingGroupTypeAdministratorTerms : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add default constraint to prevent AdministratorTerm from being null moving forward (the UI requires it)
            Sql( @"ALTER TABLE [GroupType]
ADD CONSTRAINT [DF__GroupType__AdministratorTerm]
DEFAULT 'Administrator' FOR [AdministratorTerm];" );

            // Add the default value for any records whose AdministratorTerm is currently NULL
            Sql( @"UPDATE [GroupType]
SET [AdministratorTerm] = 'Administrator'
WHERE ([AdministratorTerm] IS NULL);" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove default AdministratorTerm constraint
            Sql( @"ALTER TABLE [GroupType]
DROP CONSTRAINT [DF__GroupType__AdministratorTerm];" );
        }
    }
}
