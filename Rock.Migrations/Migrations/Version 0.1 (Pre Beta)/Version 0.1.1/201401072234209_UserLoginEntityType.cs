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
    public partial class UserLoginEntityType : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.UserLogin", "EntityTypeId", c => c.Int( nullable: true ) );

            Sql( @"
    UPDATE L 
    SET [EntityTypeId] = E.[Id]
    FROM [UserLogin] L
    INNER JOIN [EntityType] E ON E.[Name] = L.[ServiceName]
" );
            AlterColumn( "dbo.UserLogin", "EntityTypeId", c => c.Int( nullable: false ) );
            CreateIndex( "dbo.UserLogin", "EntityTypeId" );
            AddForeignKey( "dbo.UserLogin", "EntityTypeId", "dbo.EntityType", "Id" );
            DropColumn( "dbo.UserLogin", "ServiceName" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.UserLogin", "ServiceName", c => c.String(nullable: false, maxLength: 200));
            DropForeignKey("dbo.UserLogin", "EntityTypeId", "dbo.EntityType");
            DropIndex("dbo.UserLogin", new[] { "EntityTypeId" });

            Sql( @"
    UPDATE L 
    SET [ServiceName] = E.[Name]
    FROM [UserLogin] L
    INNER JOIN [EntityType] E ON E.[ID] = L.[EntityTypeId]
" );
            DropColumn("dbo.UserLogin", "EntityTypeId");
        }
    }
}
