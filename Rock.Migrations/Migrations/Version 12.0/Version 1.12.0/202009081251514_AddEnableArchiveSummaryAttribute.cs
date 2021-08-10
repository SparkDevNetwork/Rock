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
    public partial class AddEnableArchiveSummaryAttribute : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType: Content Channel View:Enable Tag List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( Rock.SystemGuid.BlockType.CONTENT_CHANNEL_VIEW,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Enable Archive Summary",
                "EnableArchiveSummary",
                "Enable Archive Summary",
                "When enabled an additional \"ArchiveSummary\" collection will be available in Lava to help create a summary list of content channel items by month/year. This collection will be cached using the same duration as the Item Cache and will hold the following properties: Month (int), MonthName, Year, Count.",
                0,
                @"False",
                Rock.SystemGuid.Attribute.ENABLE_ARCHIVE_SUMMARY );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
