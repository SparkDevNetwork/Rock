// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class GroupSync : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Group", "WelcomeSystemEmailId", c => c.Int());
            AddColumn("dbo.Group", "ExitSystemEmailId", c => c.Int());
            AddColumn("dbo.Group", "AddUserAccountsDuringSync", c => c.Boolean());
            AddColumn( "dbo.Group", "SyncDataViewId", c => c.Int() );
            CreateIndex("dbo.Group", "WelcomeSystemEmailId");
            CreateIndex("dbo.Group", "ExitSystemEmailId");
            CreateIndex( "dbo.Group", "SyncDataViewId" );
            AddForeignKey("dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail", "Id");
            AddForeignKey("dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail", "Id");
            AddForeignKey( "dbo.Group", "SyncDataViewId", "dbo.DataView", "Id" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail");
            DropForeignKey("dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail");
            DropForeignKey( "dbo.Group", "SyncDataViewId", "dbo.DataView" );
            DropIndex("dbo.Group", new[] { "ExitSystemEmailId" });
            DropIndex("dbo.Group", new[] { "WelcomeSystemEmailId" });
            DropIndex( "dbo.Group", new[] { "SyncDataViewId" } );
            DropColumn("dbo.Group", "AddUserAccountsDuringSync");
            DropColumn("dbo.Group", "ExitSystemEmailId");
            DropColumn("dbo.Group", "WelcomeSystemEmailId");
            DropColumn( "dbo.Group", "SyncDataViewId" );
        }
    }
}
