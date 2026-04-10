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
    public partial class ConnectionStatusHistoryUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ConnectionType", "RequestDueDateOffsetInDays", c => c.Int());
            AddColumn("dbo.ConnectionOpportunity", "RequestDueDateOffsetInDays", c => c.Int());
            AddColumn("dbo.ConnectionRequestStatusHistory", "PreviousConnectionStatusId", c => c.Int());
            AddColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffsetInDays", c => c.Int());
            CreateIndex("dbo.ConnectionRequestStatusHistory", "PreviousConnectionStatusId");
            AddForeignKey("dbo.ConnectionRequestStatusHistory", "PreviousConnectionStatusId", "dbo.ConnectionStatus", "Id");
            DropColumn("dbo.ConnectionType", "RequestDueDateOffestInDays");
            DropColumn("dbo.ConnectionOpportunity", "RequestDueDateOffestInDays");
            DropColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffestInDays");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffestInDays", c => c.Int());
            AddColumn("dbo.ConnectionOpportunity", "RequestDueDateOffestInDays", c => c.Int());
            AddColumn("dbo.ConnectionType", "RequestDueDateOffestInDays", c => c.Int());
            DropForeignKey("dbo.ConnectionRequestStatusHistory", "PreviousConnectionStatusId", "dbo.ConnectionStatus");
            DropIndex("dbo.ConnectionRequestStatusHistory", new[] { "PreviousConnectionStatusId" });
            DropColumn("dbo.ConnectionStatus", "RequestStatusDueDateOffsetInDays");
            DropColumn("dbo.ConnectionRequestStatusHistory", "PreviousConnectionStatusId");
            DropColumn("dbo.ConnectionOpportunity", "RequestDueDateOffsetInDays");
            DropColumn("dbo.ConnectionType", "RequestDueDateOffsetInDays");
        }
    }
}
