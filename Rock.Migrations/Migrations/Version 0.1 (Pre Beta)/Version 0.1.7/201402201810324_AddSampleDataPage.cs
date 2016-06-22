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
    public partial class AddSampleDataPage : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Sample Data", "", "844ABF2A-D085-4370-945B-86C89580C6D5", "fa fa-flask" ); // Site:Rock RMS
            // Add Block to Page: Rock Solid Church Sample Data, Site: Rock RMS
            AddBlock("844ABF2A-D085-4370-945B-86C89580C6D5","","A42E0031-B2B9-403A-845B-9C968D7716A6","Rock Solid Church Sample Data","Main","","",0,"34CA1FA0-F8F1-449F-9788-B5E6315DC058");   
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Rock Solid Church Sample Data, from Page: Rock Solid Church Sample Data, Site: Rock RMS
            DeleteBlock("34CA1FA0-F8F1-449F-9788-B5E6315DC058");
            // Page: Rock Solid Church Sample DataLayout: Full Width, Site: Rock RMS
            DeletePage( "844ABF2A-D085-4370-945B-86C89580C6D5" );
        }
    }
}
