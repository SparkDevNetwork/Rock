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
    public partial class AddNewContentChannelNavigationSetting : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenMigrationsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

        private void CodeGenMigrationsUp()
        {
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "59F4C589-9C86-4ABC-910A-5FC0EA888FDC", "Content Channels Filter", "ContentChannelsFilter", "Content Channels Filter", @"Select the content channels you would like displayed. This setting will override the Content Channel Types Include/Exclude settings.", 4, @"", "A78E159D-9B02-4199-9B98-573004379C4A" );
        }

        private void CodeGenMigrationsDown()
        {
            // Attrib for BlockType: Content Channel Navigation:Content Channels Filter
            RockMigrationHelper.DeleteAttribute( "A78E159D-9B02-4199-9B98-573004379C4A" );
        }
    }
}
