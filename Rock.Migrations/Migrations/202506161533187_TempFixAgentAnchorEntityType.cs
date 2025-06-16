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
    /// <summary>
    ///
    /// </summary>
    public partial class TempFixAgentAnchorEntityType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "EntityTypeId" } );
            AddColumn( "dbo.AIAgentSessionAnchor", "Name", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.AIAgentSessionAnchor", "LastRefreshedDateTime", c => c.DateTime( nullable: false ) );
            AlterColumn( "dbo.AIAgentSessionAnchor", "EntityTypeId", c => c.Int( nullable: false ) );
            AlterColumn( "dbo.AIAgentSessionAnchor", "EntityId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.AIAgentSessionAnchor", "EntityTypeId" );
            DropColumn( "dbo.AIAgentSessionAnchor", "PayloadLastRefreshedDateTime" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn( "dbo.AIAgentSessionAnchor", "PayloadLastRefreshedDateTime", c => c.DateTime( nullable: false ) );
            DropIndex( "dbo.AIAgentSessionAnchor", new[] { "EntityTypeId" } );
            AlterColumn( "dbo.AIAgentSessionAnchor", "EntityId", c => c.Int() );
            AlterColumn( "dbo.AIAgentSessionAnchor", "EntityTypeId", c => c.Int() );
            DropColumn( "dbo.AIAgentSessionAnchor", "LastRefreshedDateTime" );
            DropColumn( "dbo.AIAgentSessionAnchor", "Name" );
            CreateIndex( "dbo.AIAgentSessionAnchor", "EntityTypeId" );
        }
    }
}
