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
    public partial class DataSelectPageBlock : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Data Selects", "Manage the available data selects used to define reports", "227FDFB9-8C29-4B34-ABE5-E0579A3A6018", "fa fa-long-arrow-right" ); // Site:Rock RMS

            // Add Block to Page: Data Selects, Site: Rock RMS
            AddBlock( "227FDFB9-8C29-4B34-ABE5-E0579A3A6018", "", "21F5F466-59BC-40B2-8D73-7314D936C3CB", "Selects", "Main", "", "", 0, "66024082-B8B4-43A8-A94E-F313A0998596" );

            // Attrib for BlockType: Attributes:Enable Show In Grid
            AddBlockTypeAttribute( "E5EA2F6D-43A2-48E0-B59C-4409B78AC830", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Show In Grid", "EnableShowInGrid", "", "Should the 'Show In Grid' option be displayed when editing attributes?", 2, @"False", "920FE120-AD75-4D5C-BFE0-FA5745B1118B" );

            // Attrib Value for Block:Selects, Attribute:Component Container Page: Data Selects, Site: Rock RMS
            AddBlockAttributeValue( "66024082-B8B4-43A8-A94E-F313A0998596", "259AF14D-0214-4BE4-A7BF-40423EA07C99", @"Rock.Reporting.DataSelectContainer, Rock" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Attributes:Enable Show In Grid
            DeleteAttribute( "920FE120-AD75-4D5C-BFE0-FA5745B1118B" );
            // Remove Block: Selects, from Page: Data Selects, Site: Rock RMS
            DeleteBlock( "66024082-B8B4-43A8-A94E-F313A0998596" );
            DeletePage( "227FDFB9-8C29-4B34-ABE5-E0579A3A6018" ); // Page: Data SelectsLayout: Full Width, Site: Rock RMS
        }
    }
}
