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
    public partial class GroupIsPublic : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Group", "IsPublic", c => c.Boolean( nullable: false, defaultValue: true ) );

            RockMigrationHelper.AddBlockTypeAttribute( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Business Link", "AddBusinessLink", "", "Select the page where a new business can be added. If specified, a link will be shown which will open in a new window when clicked", 0, @"", "B5327385-CD67-4519-B83D-1DA1E438356F" );

            // Add Business Link value (of the existing Business Detail page) on the existing TransactionMatching block instance
            RockMigrationHelper.AddBlockAttributeValue( "A18A0A0A-0B71-43B4-B830-44B802C272D4", "B5327385-CD67-4519-B83D-1DA1E438356F", @"d2b43273-c64f-4f57-9aae-9571e1982bac" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "B5327385-CD67-4519-B83D-1DA1E438356F" );
            
            DropColumn( "dbo.Group", "IsPublic" );
        }
    }
}
