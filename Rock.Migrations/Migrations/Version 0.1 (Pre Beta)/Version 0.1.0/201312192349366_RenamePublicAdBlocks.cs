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
    public partial class RenamePublicAdBlocks : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/AdDetail.ascx' AND [GUID] != '98D27912-C4BD-4E94-AEE1-AFBF688D7264'"); // delete block type with the new path as it was probably auto added by the framework
            Sql(@"UPDATE [BlockType] set [Path] = '~/Blocks/Cms/AdDetail.ascx' where [Guid] = '98D27912-C4BD-4E94-AEE1-AFBF688D7264'");

            Sql(@"DELETE FROM [BlockType] WHERE [Path] = '~/Blocks/Cms/AdList.ascx' AND [GUID] != '5A880084-7237-449A-9855-3FA02B6BD79F'"); // delete block type with the new path as it was probably auto added by the framework
            Sql(@"UPDATE [BlockType] set [Path] = '~/Blocks/Cms/AdList.ascx' where [Guid] = '5A880084-7237-449A-9855-3FA02B6BD79F'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
