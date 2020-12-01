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
    public partial class CheckinClientInstaller : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateLinkToCheckinClientInstaller();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Updates the link to checkin client installer to the latest v11.1 version
        /// </summary>
        private void UpdateLinkToCheckinClientInstaller()
        {
            // update new location of checkinclient installer
            Sql( @"
                UPDATE [AttributeValue] 
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.11.1/checkinclient.msi'
                WHERE [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'" );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
            UPDATE [dbo].[__MigrationHistory]
            SET [Model] = 0x
            WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
