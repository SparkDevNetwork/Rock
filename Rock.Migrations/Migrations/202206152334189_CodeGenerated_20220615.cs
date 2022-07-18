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
    public partial class CodeGenerated_20220615 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: DataView Search
            //   Category: Reporting
            //   Attribute: DataView URL Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BFB625F7-75CA-48FE-9C82-90E47374242B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "DataView URL Format", "DataViewURLFormat", "DataView URL Format", @"The URL to use for linking to a dataView. <span class='tip tip-lava'></span>", 0, @"/reporting/dataViews?DataViewId={{ DataView.Id }}", "F91A4929-AC8F-4479-B70A-45F36D658D05" );

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: Report URL Format
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "13955B32-11F4-4606-8C31-4C6E5324C81A", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Report URL Format", "ReportURLFormat", "Report URL Format", @"The URL to use for linking to a report. <span class='tip tip-lava'></span>", 0, @"/reporting/reports?ReportId={{ Report.Id }}", "53A228B9-B90C-41FA-BABB-7039A83ABA48" );

            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "6FBE0419-5404-4866-85A1-135542D33725", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "B46D5CBE-8440-46DD-BA74-65A3041D409E" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Persisted Data View List
            //   Category: Reporting
            //   Attribute: Detail Page
            RockMigrationHelper.DeleteAttribute("B46D5CBE-8440-46DD-BA74-65A3041D409E");

            // Attribute for BlockType
            //   BlockType: Report Search
            //   Category: Reporting
            //   Attribute: Report URL Format
            RockMigrationHelper.DeleteAttribute("53A228B9-B90C-41FA-BABB-7039A83ABA48");

            // Attribute for BlockType
            //   BlockType: DataView Search
            //   Category: Reporting
            //   Attribute: DataView URL Format
            RockMigrationHelper.DeleteAttribute("F91A4929-AC8F-4479-B70A-45F36D658D05");
        }
    }
}
