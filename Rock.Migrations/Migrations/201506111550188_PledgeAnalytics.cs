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
    public partial class PledgeAnalytics : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Pledge Analytics", "", "FEB2332D-4605-4E2B-8EF2-2C6B1A9612C3", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Email Form", "Block that takes and HTML form and emails the contents to an address of your choosing.", "~/Blocks/Cms/EmailForm.ascx", "CMS", "48253494-F8A0-4DD8-B645-6CB481CEB7BD" );
            RockMigrationHelper.UpdateBlockType( "Pledge Analytics", "Used to look at pledges using various criteria.", "~/Blocks/Finance/PledgeAnalytics.ascx", "Finance", "72B4BBC0-1E8A-46B7-B956-A399624F513C" );
            // Add Block to Page: Pledge Analytics, Site: Rock RMS
            RockMigrationHelper.AddBlock( "FEB2332D-4605-4E2B-8EF2-2C6B1A9612C3", "", "72B4BBC0-1E8A-46B7-B956-A399624F513C", "Pledge Analytics", "Main", "", "", 0, "2DFEEEAE-0DC3-4DF5-B2B8-3C33C81624B3" ); 


            //
            // rename blocks
            //

            // ReportData -> Dynamic Report
            Sql( @"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Reporting/DynamicReport.ascx'" );
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/Reporting/DynamicReport.ascx', [Name] = 'Dynamic Report' WHERE [Guid] = 'C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB'" );

            // AttendanceAnalysis -> Attendance Analytics
            Sql( @"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/CheckIn/AttendanceAnalytics.ascx'" );
            Sql( @"UPDATE [BlockType] SET [Path] = '~/Blocks/CheckIn/AttendanceAnalytics.ascx', [Name] = 'Attendance Analytics' WHERE [Guid] = '3CD3411C-C076-4344-A9D5-8F3B4F01E31D'" );
        
            // rename attendance analysis page
            Sql( @"UPDATE [Page]
                    SET [BrowserTitle] = 'Attendance Analytics', [PageTitle] = 'Attendance Analytics', [InternalName] = 'Attendance Analytics'
                    WHERE [Guid] = '7A3CF259-1090-403C-83B7-2DB3A53DEE26'" );
        
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Pledge Analytics, from Page: Pledge Analytics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2DFEEEAE-0DC3-4DF5-B2B8-3C33C81624B3" );
            RockMigrationHelper.DeleteBlockType( "72B4BBC0-1E8A-46B7-B956-A399624F513C" ); // Pledge Analytics
            RockMigrationHelper.DeleteBlockType( "48253494-F8A0-4DD8-B645-6CB481CEB7BD" ); // Email Form
            RockMigrationHelper.DeletePage( "FEB2332D-4605-4E2B-8EF2-2C6B1A9612C3" ); //  Page: Pledge Analytics, Layout: Full Width, Site: Rock RMS
        }
    }
}
