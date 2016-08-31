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
    public partial class AttendanceHistory : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add the Attendance History block type
            AddBlockType( "Attendance History", "Displays the attendance history of a person or group", "~/Blocks/Checkin/AttendanceHistoryList.ascx", "21FFA70E-18B3-4148-8FC4-F941100B49B8" );
            // Add the Attendance History block to the Person details page History tab
            AddBlock( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "", "21FFA70E-18B3-4148-8FC4-F941100B49B8", "Attendance History", "SectionC1", "", "", 0, "2D99AB97-4B9C-4D72-B207-8F36AE90D495" );
            // Change the Order of the Person History block already on the Person Details page History tab so that it comes after this attendance history
            Sql( @"
                UPDATE Block SET [Order] = 1 WHERE Guid = 'F98649D7-E522-46CB-8F67-01DB7F59E3AA'
            " );
            // Add the Context Entity attribute for the Attendance History and set it's value to Rock.Model.Person.
            AddBlockTypeAttribute( "21FFA70E-18B3-4148-8FC4-F941100B49B8", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block.", 0, "", "B5F26E61-BFA8-4FEF-A11F-ECD0B3BE54B1" );
            AddBlockAttributeValue( "2D99AB97-4B9C-4D72-B207-8F36AE90D495", "B5F26E61-BFA8-4FEF-A11F-ECD0B3BE54B1", "72657ED8-D16E-492E-AC12-144C5E7567E7" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttributeValue( "2D99AB97-4B9C-4D72-B207-8F36AE90D495", "B5F26E61-BFA8-4FEF-A11F-ECD0B3BE54B1" );
            DeleteBlockAttribute( "B5F26E61-BFA8-4FEF-A11F-ECD0B3BE54B1" );
            Sql( @"
                UPDATE Block SET [Order] = 0 WHERE Guid = 'F98649D7-E522-46CB-8F67-01DB7F59E3AA'
            " );
            DeleteBlock( "2D99AB97-4B9C-4D72-B207-8F36AE90D495" );
            DeleteBlockType( "21FFA70E-18B3-4148-8FC4-F941100B49B8" );
        }
    }
}
