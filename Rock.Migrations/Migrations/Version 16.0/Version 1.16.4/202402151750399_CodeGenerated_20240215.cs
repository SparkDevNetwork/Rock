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
    public partial class CodeGenerated_20240215 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: SMS Character Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B0182DA2-82F7-4798-A48E-88EBE61F2109", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "SMS Character Limit", "SmsCharacterLimit", "SMS Character Limit", @"The number of characters to limit the SMS communication body to. Set to 0 to disable.", 9, @"0", "7AEC21F2-0878-43A2-9D38-545800A1EF5C" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Communication Entry
            //   Category: Mobile > Communication
            //   Attribute: SMS Character Limit
            RockMigrationHelper.DeleteAttribute( "7AEC21F2-0878-43A2-9D38-545800A1EF5C" );
        }
    }
}
