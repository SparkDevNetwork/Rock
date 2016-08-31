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
    public partial class CategoryTreeView : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Category Tree View:Show Unnamed Entity Items
            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Unnamed Entity Items", "ShowUnnamedEntityItems", "", "Set to false to hide any EntityType items that have a blank name.", 0, @"True", "C48600CD-2C65-46EF-84E8-975F0DE8C28E" );

            // Attrib Value for Block:Schedule Categories, Attribute:Show Unnamed Entity Items Page: Schedules, Site: Rock RMS
            AddBlockAttributeValue( "35D97498-A085-4745-928C-7E119ADF8833", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"False" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Category Tree View:Show Unnamed Entity Items
            DeleteAttribute( "C48600CD-2C65-46EF-84E8-975F0DE8C28E" );
        }
    }
}
