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
    public partial class AddTitleToTransactions : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {

            // Attrib for BlockType: Finance > Transaction List:Title
            AddBlockTypeAttribute("E04320BC-67C3-452D-9EF6-D74D8C177154", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Title", "Title", "", "Title to display above the grid. Leave blank to hide.", 0, @"", "A4E3B5C6-B386-45B5-A929-8FD9379BABBC");

            // Attrib Value for Block:Transaction List, Attribute:Title Page: Financial Batch Detail, Site: Rock Internal
            AddBlockAttributeValue("1133795B-3325-4D81-B603-F442F0AC892B", "A4E3B5C6-B386-45B5-A929-8FD9379BABBC", @"Transactions");

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
