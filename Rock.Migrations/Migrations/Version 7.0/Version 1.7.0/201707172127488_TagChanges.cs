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
    public partial class TagChanges : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Tag", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Tag", "CategoryId", c => c.Int());
            CreateIndex("dbo.Tag", "CategoryId");
            AddForeignKey("dbo.Tag", "CategoryId", "dbo.Category", "Id");

            Sql( "UPDATE [Tag] SET [IsActive]=1" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Tag", "CategoryId", "dbo.Category");
            DropIndex("dbo.Tag", new[] { "CategoryId" });
            DropColumn("dbo.Tag", "CategoryId");
            DropColumn("dbo.Tag", "IsActive");
        }
    }
}
