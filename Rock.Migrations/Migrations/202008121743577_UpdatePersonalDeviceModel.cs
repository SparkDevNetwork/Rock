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
    /// Update Personal Device Model
    /// </summary>
    public partial class UpdatePersonalDeviceModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.PersonalDevice", "SiteId", c => c.Int());
            AddColumn("dbo.PersonalDevice", "Manufacturer", c => c.String(maxLength: 50));
            AddColumn("dbo.PersonalDevice", "Model", c => c.String(maxLength: 50));
            AddColumn("dbo.PersonalDevice", "Name", c => c.String(maxLength: 50));
            CreateIndex("dbo.PersonalDevice", "SiteId");
            AddForeignKey("dbo.PersonalDevice", "SiteId", "dbo.Site", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PersonalDevice", "SiteId", "dbo.Site");
            DropIndex("dbo.PersonalDevice", new[] { "SiteId" });
            DropColumn("dbo.PersonalDevice", "Name");
            DropColumn("dbo.PersonalDevice", "Model");
            DropColumn("dbo.PersonalDevice", "Manufacturer");
            DropColumn("dbo.PersonalDevice", "SiteId");
        }
    }
}
