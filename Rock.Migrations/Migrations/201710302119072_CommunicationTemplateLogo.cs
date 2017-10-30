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
    public partial class CommunicationTemplateLogo : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.CommunicationTemplate", "LogoBinaryFileId", c => c.Int());
            AddColumn("dbo.CommunicationTemplate", "CategoryId", c => c.Int());
            CreateIndex("dbo.CommunicationTemplate", "LogoBinaryFileId");
            CreateIndex("dbo.CommunicationTemplate", "CategoryId");
            AddForeignKey("dbo.CommunicationTemplate", "CategoryId", "dbo.Category", "Id");
            AddForeignKey("dbo.CommunicationTemplate", "LogoBinaryFileId", "dbo.BinaryFile", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.CommunicationTemplate", "LogoBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.CommunicationTemplate", "CategoryId", "dbo.Category");
            DropIndex("dbo.CommunicationTemplate", new[] { "CategoryId" });
            DropIndex("dbo.CommunicationTemplate", new[] { "LogoBinaryFileId" });
            DropColumn("dbo.CommunicationTemplate", "CategoryId");
            DropColumn("dbo.CommunicationTemplate", "LogoBinaryFileId");
        }
    }
}
