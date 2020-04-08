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
    public partial class RenameInteractionComponentChannelId : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameColumn(table: "dbo.InteractionComponent", name: "ChannelId", newName: "InteractionChannelId");
            RenameIndex(table: "dbo.InteractionComponent", name: "IX_ChannelId", newName: "IX_InteractionChannelId");

            Sql( "sp_rename '[FK_dbo.InteractionComponent_dbo.InteractionChannel_ChannelId]', 'FK_dbo.InteractionComponent_dbo.InteractionChannel_InteractionChannelId';" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RenameIndex(table: "dbo.InteractionComponent", name: "IX_InteractionChannelId", newName: "IX_ChannelId");
            RenameColumn(table: "dbo.InteractionComponent", name: "InteractionChannelId", newName: "ChannelId");

            Sql( "sp_rename '[FK_dbo.InteractionComponent_dbo.InteractionChannel_InteractionChannelId]', 'FK_dbo.InteractionComponent_dbo.InteractionChannel_ChannelId';" );
        }
    }
}
