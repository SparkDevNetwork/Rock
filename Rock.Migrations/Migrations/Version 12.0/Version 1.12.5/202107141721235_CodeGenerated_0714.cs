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
    public partial class CodeGenerated_0714 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType: Group Finder:Group Type Locations
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Type Locations", "GroupTypeLocations", "Group Type Locations", @"", 0, @"", "A2A58722-F10C-497A-BE4B-91AD04A93E43" );

            // Attribute for BlockType: Group Finder:Location Precision Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Location Precision Level", "LocationPrecisionLevel", "Location Precision Level", @"", 0, @"Precise", "06D4EA2C-8D2B-4047-87AF-979A991ECFE6" );

            // Attribute for BlockType: Group Finder:Map Marker
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Map Marker", "MapMarker", "Map Marker", @"", 0, @"", "C49AD513-5BE9-4B26-9BE3-225427C7E8A0" );

            // Attribute for BlockType: Group Finder:Marker Color
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Marker Color", "MarkerColor", "Marker Color", @"", 0, @"", "F9F1D011-DE9C-493E-959C-C3CA02848D9E" );

            // Attribute for BlockType: Group Finder:Load Initial Results
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Load Initial Results", "LoadInitialResults", "Load Initial Results", @"", 0, @"False", "F8A343C8-E04D-4654-9F68-61E91A438245" );

            // Attribute for BlockType: Group Finder:Maximum Zoom Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Maximum Zoom Level", "MaximumZoomLevel", "Maximum Zoom Level", @"", 0, @"", "EF7E63E7-F391-419F-BBCC-EC489FDB00F7" );

            // Attribute for BlockType: Group Finder:Minimum Zoom Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Minimum Zoom Level", "MinimumZoomLevel", "Minimum Zoom Level", @"", 0, @"", "FFCFEDE6-0096-4FD7-A6A7-D3384CD6EBAD" );

            // Attribute for BlockType: Group Finder:Initial Zoom Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Initial Zoom Level", "InitialZoomLevel", "Initial Zoom Level", @"", 0, @"", "29BF3191-1786-4E9B-83D4-4882EB3E3E10" );

            // Attribute for BlockType: Group Finder:Marker Zoom Level
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Marker Zoom Level", "MarkerZoomLevel", "Marker Zoom Level", @"", 0, @"", "66CB78D6-87B9-44D3-BBF5-5B8859AD5DBF" );

            // Attribute for BlockType: Group Finder:Marker Zoom Amount
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Marker Zoom Amount", "MarkerZoomAmount", "Marker Zoom Amount", @"", 0, @"1", "2BCAADD4-8C51-4EB4-ADCD-3EE5DEFE9F89" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Marker Color Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("F9F1D011-DE9C-493E-959C-C3CA02848D9E");

            // Map Marker Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("C49AD513-5BE9-4B26-9BE3-225427C7E8A0");

            // Location Precision Level Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("06D4EA2C-8D2B-4047-87AF-979A991ECFE6");

            // Marker Zoom Amount Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("2BCAADD4-8C51-4EB4-ADCD-3EE5DEFE9F89");

            // Marker Zoom Level Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("66CB78D6-87B9-44D3-BBF5-5B8859AD5DBF");

            // Initial Zoom Level Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("29BF3191-1786-4E9B-83D4-4882EB3E3E10");

            // Minimum Zoom Level Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("FFCFEDE6-0096-4FD7-A6A7-D3384CD6EBAD");

            // Maximum Zoom Level Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("EF7E63E7-F391-419F-BBCC-EC489FDB00F7");

            // Group Type Locations Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("A2A58722-F10C-497A-BE4B-91AD04A93E43");

            // Load Initial Results Attribute for BlockType: Group Finder
            RockMigrationHelper.DeleteAttribute("F8A343C8-E04D-4654-9F68-61E91A438245");
        }
    }
}
