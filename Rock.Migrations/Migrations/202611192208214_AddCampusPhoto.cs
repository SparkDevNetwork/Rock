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
    /// Adds the PhotoId column to the Campus table, with an index and a foreign key to the BinaryFile table.
    /// </summary>
    public partial class AddCampusPhoto : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Campus", "PhotoId", c => c.Int());
            CreateIndex("dbo.Campus", "PhotoId");
            AddForeignKey("dbo.Campus", "PhotoId", "dbo.BinaryFile", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Campus", "PhotoId", "dbo.BinaryFile");
            DropIndex("dbo.Campus", new[] { "PhotoId" });
            DropColumn("dbo.Campus", "PhotoId");
        }
    }
}
