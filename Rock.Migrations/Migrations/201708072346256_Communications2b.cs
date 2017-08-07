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
    public partial class Communications2b : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Person", "CommunicationPreference", c => c.Int(nullable: false));
            AddColumn("dbo.Communication", "CommunicationTemplateId", c => c.Int());
            CreateIndex("dbo.Communication", "CommunicationTemplateId");
            
            // AddForeignKey("dbo.Communication", "CommunicationTemplateId", "dbo.CommunicationTemplate", "Id");
            // Instead of AddForeignKey, do it manually so it can be a ON DELETE SET NULL
            Sql( @"ALTER TABLE dbo.Communication ADD CONSTRAINT [FK_dbo.Communication_dbo.CommunicationTemplate_CommunicationTemplateId] 
                    FOREIGN KEY (CommunicationTemplateId) REFERENCES dbo.CommunicationTemplate (Id) ON DELETE SET NULL" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Communication", "CommunicationTemplateId", "dbo.CommunicationTemplate");
            DropIndex("dbo.Communication", new[] { "CommunicationTemplateId" });
            DropColumn("dbo.Communication", "CommunicationTemplateId");
            DropColumn("dbo.Person", "CommunicationPreference");
        }
    }
}
