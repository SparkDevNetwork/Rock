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
    public partial class RemoveIconFileFields : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.BinaryFileType", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.BinaryFileType", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.GroupType", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.GroupType", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Category", "IconLargeFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Category", "IconSmallFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Page", "IconFileId", "dbo.BinaryFile");
            DropIndex("dbo.BinaryFileType", new[] { "IconLargeFileId" });
            DropIndex("dbo.BinaryFileType", new[] { "IconSmallFileId" });
            DropIndex("dbo.GroupType", new[] { "IconLargeFileId" });
            DropIndex("dbo.GroupType", new[] { "IconSmallFileId" });
            DropIndex("dbo.Category", new[] { "IconLargeFileId" });
            DropIndex("dbo.Category", new[] { "IconSmallFileId" });
            DropIndex("dbo.Page", new[] { "IconFileId" });
            DropColumn("dbo.GroupType", "IconSmallFileId");
            DropColumn("dbo.GroupType", "IconLargeFileId");
            DropColumn("dbo.BinaryFileType", "IconSmallFileId");
            DropColumn("dbo.BinaryFileType", "IconLargeFileId");
            DropColumn("dbo.Category", "IconSmallFileId");
            DropColumn("dbo.Category", "IconLargeFileId");
            DropColumn("dbo.Page", "IconFileId");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Page", "IconFileId", c => c.Int());
            AddColumn("dbo.Category", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.Category", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.BinaryFileType", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.BinaryFileType", "IconSmallFileId", c => c.Int());
            AddColumn("dbo.GroupType", "IconLargeFileId", c => c.Int());
            AddColumn("dbo.GroupType", "IconSmallFileId", c => c.Int());
            CreateIndex("dbo.Page", "IconFileId");
            CreateIndex("dbo.Category", "IconSmallFileId");
            CreateIndex("dbo.Category", "IconLargeFileId");
            CreateIndex("dbo.GroupType", "IconSmallFileId");
            CreateIndex("dbo.GroupType", "IconLargeFileId");
            CreateIndex("dbo.BinaryFileType", "IconSmallFileId");
            CreateIndex("dbo.BinaryFileType", "IconLargeFileId");
            AddForeignKey("dbo.Page", "IconFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.Category", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.Category", "IconLargeFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.GroupType", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.GroupType", "IconLargeFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.BinaryFileType", "IconSmallFileId", "dbo.BinaryFile", "Id");
            AddForeignKey("dbo.BinaryFileType", "IconLargeFileId", "dbo.BinaryFile", "Id");
        }
    }
}
