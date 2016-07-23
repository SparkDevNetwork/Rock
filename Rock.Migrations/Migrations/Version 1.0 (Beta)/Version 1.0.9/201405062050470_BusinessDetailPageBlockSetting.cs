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
    public partial class BusinessDetailPageBlockSetting : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create the Business Detail Page attribute on the Person Bio block
            AddBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Business Detail Page", "BusinessDetailPage", "", "", 0, "", "509F3D63-6218-49BD-B0A9-CE49B6FCB2CF" );

            // Set "Business Detail Page" attribute on the Person Bio block to the Business Detail page
            AddBlockAttributeValue( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A", "509F3D63-6218-49BD-B0A9-CE49B6FCB2CF", "D2B43273-C64F-4F57-9AAE-9571E1982BAC" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "509F3D63-6218-49BD-B0A9-CE49B6FCB2CF" );
        }
    }
}
