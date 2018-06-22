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
    public partial class Rollup_0225 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateFieldType( "Check List", "", "Rock", "Rock.Field.Types.CheckListFieldType", "31532E03-CF25-4A9F-855C-BA16C4075679" );
            RockMigrationHelper.UpdateFieldType( "Content Channel Item", "", "Rock", "Rock.Field.Types.ContentChannelItemFieldType", "19BFB635-DC31-4C1E-8BB5-CDA120890BDE" );

            // Attrib for BlockType: Calendar Lava:Date Parameter Name
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Date Parameter Name", "DateParameterName", "", @"The page parameter name that contains the selected date.", 19, @"date", "06879E0A-4C8F-4725-AB3B-40D3260CCB2C" );
            // Attrib for BlockType: Dynamic Data:Encrypted Fields
            RockMigrationHelper.UpdateBlockTypeAttribute( "E31E02E9-73F6-4B3E-98BA-E0E4F86CA126", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Encrypted Fields", "EncryptedFields", "", @"Any fields that need to be decrypted before displaying their value", 0, @"", "AF7714D4-D825-419A-B136-FF8293396635" );
            // Attrib for BlockType: Login:Show Internal Login
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Internal Login", "ShowInternalLogin", "", @"Show the default (non-remote) login", 11, @"True", "DCBCB463-54C7-46CB-A46D-168FD23101DF" );
            // Attrib for BlockType: Login:Remote Authorization Prompt Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Remote Authorization Prompt Message", "RemoteAuthorizationPromptMessage", "", @"Optional text (HTML) to display above remote authorization options.", 9, @"Login with social account", "86AC2462-229E-4094-BEF4-D5D9D3D2FED1" );

            // Attrib Value for Block:Global Attributes, Attribute:Entity Page: Global Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3CBB177B-DBFB-4FB2-A1A7-957DC6C350EB", "5B33FE25-6BF0-4890-91C6-49FB1629221E", @"00000000-0000-0000-0000-000000000000" );

            // Attrib for BlockType: Calendar Lava:Campuses
            RockMigrationHelper.UpdateBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "69254F91-C97F-4C2D-9ACB-1683B088097B", "Campuses", "Campuses", "", @"Select campuses to display calendar events for. No selection will show all.", 4, @"", "2CFAF7B4-6789-46BE-BDC0-99877F22CFB7" );
            RockMigrationHelper.UpdateFieldType( "Month Day", "", "Rock", "Rock.Field.Types.MonthDayFieldType", "8BED8DD8-8167-4052-B807-A1E72C133611" );

            Sql( MigrationSQL._201802260423490_Rollup_0225_spCheckin_AttendanceAnalyticsQuery_Attendees );
            Sql( MigrationSQL._201802260423490_Rollup_0225_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
