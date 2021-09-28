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
    public partial class MakeCoreSchedulePagesConsistent : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block Schedule Category Exclusion List to Page: Schedules, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ACF84335-34A1-4DD6-B242-20119B8D0967".AsGuid(), "Schedule Category Exclusion List", "Main", @"", @"", 2, "9606894A-5742-4000-ABBF-D7517B12977B" );

            // Add Block Schedule List to Page: Schedules, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "F5D6D7DD-FD5F-494C-83DC-E2AF63C705D1".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "C1B934D1-2139-471E-B2B8-B22FF4499B2F".AsGuid(), "Schedule List", "Main", @"", @"", 3, "DF73981E-DA99-40F5-8C25-12124083CD4A" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "DF73981E-DA99-40F5-8C25-12124083CD4A", "295A1429-9581-49CF-87D9-9FA912707646", @"True" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "DF73981E-DA99-40F5-8C25-12124083CD4A", "00F227B2-C977-4BA6-816A-F45C6FE9EF5A", @"" );

            // Block Attribute Value for Schedule List ( Page: Schedules, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "DF73981E-DA99-40F5-8C25-12124083CD4A", "A39C21A9-4DE7-421A-90D3-05889C9D26A1", @"True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
