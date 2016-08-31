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
    public partial class PersonSearchAddAttribute : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Crm > Person Search:Person Detail Page
            AddBlockTypeAttribute( "764D3E67-2D01-437A-9F45-9F8C97878434", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Detail Page", "PersonDetailPage", "", "", 0, @"", "F6E44A84-AFDD-4F66-9ED3-64ECF45BC7DA" );
            // Attrib Value for Block:Person Search, Attribute:Person Detail Page Page: Person Search, Site: Rock Internal
            AddBlockAttributeValue( "434CB505-016B-418A-B27A-D0FDD07DD928", "F6E44A84-AFDD-4F66-9ED3-64ECF45BC7DA", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Crm > Person Search:Person Detail Page
            DeleteBlockAttribute( "F6E44A84-AFDD-4F66-9ED3-64ECF45BC7DA" );
        }
    }
}
