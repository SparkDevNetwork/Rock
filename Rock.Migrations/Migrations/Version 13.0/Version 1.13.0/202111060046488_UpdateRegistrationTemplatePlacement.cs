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
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateRegistrationTemplatePlacement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete any orphaned records prior to trying to remove FK constraints
            /* Removes records that have a RegistratioTemplatePlacement RegistrationTemplateId but the RegistrationTemplateId
             * doesn't exist in RegistrationTemplate.
             */
            Sql( @"DELETE FROM RegistrationTemplatePlacement 
                          WHERE RegistrationTemplateId IN (
                              SELECT DISTINCT p.RegistrationTemplateId
                              FROM RegistrationTemplatePlacement p
                              LEFT JOIN RegistrationTemplate t ON t.Id = p.RegistrationTemplateId
                              WHERE t.Id IS NULL
                          )" );

            DropForeignKey( "dbo.RegistrationTemplatePlacement", "RegistrationTemplateId", "dbo.RegistrationTemplate" );
            AddForeignKey( "dbo.RegistrationTemplatePlacement", "RegistrationTemplateId", "dbo.RegistrationTemplate", "Id", cascadeDelete: true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.RegistrationTemplatePlacement", "RegistrationTemplateId", "dbo.RegistrationTemplate" );
            AddForeignKey( "dbo.RegistrationTemplatePlacement", "RegistrationTemplateId", "dbo.RegistrationTemplate", "Id" );
        }
    }
}
