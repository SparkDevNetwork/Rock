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
    /// Add the optional ConnectionRequestId to the BackgroundCheck model with ON DELETE SET NULL.
    /// </summary>
    public partial class AddConnectionRequestToBackgroundCheck : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BackgroundCheck", "ConnectionRequestId", c => c.Int(nullable: true));
            CreateIndex("dbo.BackgroundCheck", "ConnectionRequestId");
            // Instead of the regular scaffolded AddForeignKey( "dbo.BackgroundCheck", "ConnectionRequestId", "dbo.ConnectionRequest", "Id", cascadeDelete: true);
            // Create the FK manually so that we can add the 'ON DELETE SET NULL'
            Sql( @"
ALTER TABLE [dbo].[BackgroundCheck]  WITH CHECK ADD CONSTRAINT [FK_dbo.BackgroundCheck_dbo.ConnectionRequest] FOREIGN KEY ([ConnectionRequestId])
REFERENCES [dbo].[ConnectionRequest] ([Id])
ON DELETE SET NULL
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
IF EXISTS (SELECT 1 FROM sys.foreign_keys c WHERE c.name = 'FK_dbo.BackgroundCheck_dbo.ConnectionRequest')
	ALTER TABLE [dbo].[BackgroundCheck] DROP CONSTRAINT [FK_dbo.BackgroundCheck_dbo.ConnectionRequest]
" );
            DropIndex("dbo.BackgroundCheck", new[] { "ConnectionRequestId" });
            DropColumn("dbo.BackgroundCheck", "ConnectionRequestId");
        }
    }
}
