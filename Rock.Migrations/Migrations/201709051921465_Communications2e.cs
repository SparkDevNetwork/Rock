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
    using System.Linq;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Communications2e : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Migrate the medium data for all templates, and the most recent 5000 communications. A job will run
            // to convert the remaining communications
            Jobs.MigrateCommunicationMediumData.UpdateCommunicationRecords( true, 5000 );

            // Job for Migrating Interaction Data
            Sql( @"
    INSERT INTO [dbo].[ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus] ,[Guid] )
    VALUES ( 0, 1, 'Convert communication medium data', 'Converts communication medium data to field values.', 
        'Rock.Jobs.MigrateCommunicationMediumData', '0 0 3 1/1 * ? *', 3, 'E7C54AAB-451E-4E89-8083-CF398D37416E')" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
