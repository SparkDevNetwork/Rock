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
    public partial class Rollup_20241017 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            InactivateCustomUpdatePersistedDatasetJobs();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region PA: Disable all custom Update Persisted Dataset Job if the system job is active.

        private void InactivateCustomUpdatePersistedDatasetJobs()
        {
            Sql( $@"
          IF NOT EXISTS (
              SELECT 1
              FROM [ServiceJob]
              WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}' AND [IsActive] = 0
          )
          BEGIN
              -- Disable the custom Update Persisted Dataset Jobs
              UPDATE [ServiceJob]
              SET [IsActive] = 0
              WHERE [Guid] <> '{SystemGuid.ServiceJob.UPDATE_PERSISTED_DATASETS}' AND [Class] = 'Rock.Jobs.UpdatePersistedDatasets';
          END" );
        }

        #endregion
    }
}
