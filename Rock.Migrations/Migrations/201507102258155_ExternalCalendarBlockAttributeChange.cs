// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ExternalCalendarBlockAttributeChange : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.DeleteAttribute( "9DEA73E9-E5AF-4386-BFBA-4C1440D315C8" );

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Show Category Filter", "ShowCategoryFilter", "", "Determines whether the campus filters are shown", 0, @"", "6C82C102-4561-446C-A511-A9863EE9A47B" );

            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "6C82C102-4561-446C-A511-A9863EE9A47B", @"" ); // Show Category fil
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "6C82C102-4561-446C-A511-A9863EE9A47B" );

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Filter", "ShowCategoryFilter", "", "Determines whether the campus filters are shown", 0, @"False", "9DEA73E9-E5AF-4386-BFBA-4C1440D315C8" );
           
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "9DEA73E9-E5AF-4386-BFBA-4C1440D315C8", @"False" ); // Show Category Filter
        }
    }
}
