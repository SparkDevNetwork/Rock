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
    public partial class StoreCategoryHeader : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block to Page: Rock Shop, Site: Rock RMS              
            RockMigrationHelper.AddBlock( "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA", "", "470C6EFF-091C-4593-848C-49547D0EBEEE", "Package Category Header List", "Main", "", "", 1, "A239E904-3E32-462E-B97D-388E7E87C37F" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955'" );  // Page: Rock Shop,  Zone: Main,  Block: Promo Rotator
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F'" );  // Page: Rock Shop,  Zone: Main,  Block: Package Category Header List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '8D23BB71-69D9-4409-8368-1D965A3C5128'" );  // Page: Rock Shop,  Zone: Main,  Block: Featured Promos
                                                                                                             // Attrib Value for Block:Package Category Header List, Attribute:Enable Debug Page: Rock Shop, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue( "A239E904-3E32-462E-B97D-388E7E87C37F", "12BDD62A-D1D9-473A-803F-42C4C1183111", @"False" );
            // Attrib Value for Block:Package Category Header List, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS             
            RockMigrationHelper.AddBlockAttributeValue( "A239E904-3E32-462E-B97D-388E7E87C37F", "15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53", @"{% include '~/Assets/Lava/Store/PackageCategoryListHeader.lava' %}" );
            // Attrib Value for Block:Package Category Header List, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS            
            RockMigrationHelper.AddBlockAttributeValue( "A239E904-3E32-462E-B97D-388E7E87C37F", "CFEA2CB4-2303-429C-B096-CBD2413AD56B", @"50d17fe7-88db-46b2-9c58-df8c0de376a4" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Package Category Header List, from Page: Rock Shop, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock( "A239E904-3E32-462E-B97D-388E7E87C37F" );
        }
    }
}
