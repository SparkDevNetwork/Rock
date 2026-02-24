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
    public partial class Rollup_20260224 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CodeGenExceptionDetailObsidianBlock_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        #region Convert ExceptionDetail to Obsidian

        /// <summary>
        /// Registers the Obsidian Exception Detail block entity type and block type.
        /// </summary>
        private void CodeGenExceptionDetailObsidianBlock_Up()
        {
            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Core.ExceptionDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Core.ExceptionDetail", "Exception Detail", "Rock.Blocks.Core.ExceptionDetail, Rock.Blocks, Version=18.0.0.0, Culture=neutral, PublicKeyToken=null", false, false, "4467CF3A-9944-48AF-8D2E-7E270DEBD852" );

            // Add/Update Obsidian Block Type
            //   Name:Exception Detail
            //   Category:Core
            //   EntityType:Rock.Blocks.Core.ExceptionDetail
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Exception Detail", "Displays the details of the given exception.", "Rock.Blocks.Core.ExceptionDetail", "Core", "6CFEAD0B-D36E-4CB9-B691-B7E6B06C9993" );
        }

        #endregion
    }
}
