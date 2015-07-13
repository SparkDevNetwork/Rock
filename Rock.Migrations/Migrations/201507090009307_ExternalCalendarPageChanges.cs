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
    public partial class ExternalCalendarPageChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Event Calendar", "", "Rock", "Rock.Field.Types.EventCalendarFieldType", "EC0D9528-1A22-404E-A776-566404987363" );

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Small Calendar", "ShowSmallCalendar", "", "Determines whether the calendar widget is shown", 0, @"True", "E0A4EB5B-2625-4594-9ECB-2B6EAD6E5D04" );

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "EC0D9528-1A22-404E-A776-566404987363", "Event Calendar", "EventCalendar", "", "The event calendar to be displayed", 0, @"1", "D4CD78A2-E893-46D3-A68A-2F4D0EFCA97A" );

            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "E0A4EB5B-2625-4594-9ECB-2B6EAD6E5D04", @"True" ); // Show Small Calendar

            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "D4CD78A2-E893-46D3-A68A-2F4D0EFCA97A", @"8a444668-19af-4417-9c74-09f842572974" ); // Event Calendar

            RockMigrationHelper.DeleteAttribute( "BF06F16A-E40F-497A-B1A9-409D4FFCD972" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteFieldType( "EC0D9528-1A22-404E-A776-566404987363" );

            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Event Calendar Id", "EventCalendarId", "", "The Id of the event calendar to be displayed", 0, @"1", "BF06F16A-E40F-497A-B1A9-409D4FFCD972" );

            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "BF06F16A-E40F-497A-B1A9-409D4FFCD972", @"1" ); // Event Calendar Id

            RockMigrationHelper.DeleteAttribute( "D4CD78A2-E893-46D3-A68A-2F4D0EFCA97A" );

            RockMigrationHelper.DeleteAttribute( "E0A4EB5B-2625-4594-9ECB-2B6EAD6E5D04" );
        }
    }
}
