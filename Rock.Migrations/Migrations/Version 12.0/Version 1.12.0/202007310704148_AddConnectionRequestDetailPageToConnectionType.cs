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
    public partial class AddConnectionRequestDetailPageToConnectionType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddConnectionRequestDetailPageInConnectionType();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveConnectionDetailPageInConnectionType();
        }

        private void AddConnectionRequestDetailPageInConnectionType()
        {
            AddColumn( "dbo.ConnectionType", "ConnectionRequestDetailPageId", c => c.Int() );
            AddColumn( "dbo.ConnectionType", "ConnectionRequestDetailPageRouteId", c => c.Int() );
            CreateIndex( "dbo.ConnectionType", "ConnectionRequestDetailPageId" );
            CreateIndex( "dbo.ConnectionType", "ConnectionRequestDetailPageRouteId" );
            AddForeignKey( "dbo.ConnectionType", "ConnectionRequestDetailPageId", "dbo.Page", "Id" );
            AddForeignKey( "dbo.ConnectionType", "ConnectionRequestDetailPageRouteId", "dbo.PageRoute", "Id" );

            // Attribute for BlockType: My Connection Opportunities:Use Connection Request Detail Page From Connection Type        
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Connection Request Detail Page From Connection Type", "UseConnectionRequestDetailPageFromConnectionType", "Use Connection Request Detail Page From Connection Type", @"If enabled, the Connection Request Detail page defined by the Connection Type will be used to view the request(if it's not empty/unset). Otherwise the Connection Request Detail page configured on this block will be used.", 1, @"True", "C7C479A1-4047-42D4-AAE4-927DD7E1BD2E" );
            // Attribute for BlockType: Connection Requests:Use Connection Request Detail Page From Connection Type        
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Connection Request Detail Page From Connection Type", "UseConnectionRequestDetailPageFromConnectionType", "Use Connection Request Detail Page From Connection Type", @"If enabled, the Connection Request Detail page defined by the Connection Type will be used to view the request(if it's not empty/unset). Otherwise the Connection Request Detail page configured on this block will be used.", 2, @"True", "9F7D1BD2-472B-4C1C-9905-C956B9104304" );
        }

        private void RemoveConnectionDetailPageInConnectionType()
        {
            // Use Connection Request Detail Page From Connection Type Attribute for BlockType: Connection Requests         
            RockMigrationHelper.DeleteAttribute( "9F7D1BD2-472B-4C1C-9905-C956B9104304" );
            // Use Connection Request Detail Page From Connection Type Attribute for BlockType: My Connection Opportunities   
            RockMigrationHelper.DeleteAttribute( "C7C479A1-4047-42D4-AAE4-927DD7E1BD2E" );
            DropForeignKey( "dbo.ConnectionType", "ConnectionRequestDetailPageRouteId", "dbo.PageRoute" );
            DropForeignKey( "dbo.ConnectionType", "ConnectionRequestDetailPageId", "dbo.Page" );
            DropIndex( "dbo.ConnectionType", new[] { "ConnectionRequestDetailPageRouteId" } );
            DropIndex( "dbo.ConnectionType", new[] { "ConnectionRequestDetailPageId" } );
            DropColumn( "dbo.ConnectionType", "ConnectionRequestDetailPageRouteId" );
            DropColumn( "dbo.ConnectionType", "ConnectionRequestDetailPageId" );
        }

    }
}
