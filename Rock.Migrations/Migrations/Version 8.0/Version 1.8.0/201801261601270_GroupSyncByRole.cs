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
    public partial class GroupSyncByRole : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.GroupSync",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        GroupTypeRoleId = c.Int(nullable: false),
                        SyncDataViewId = c.Int(nullable: false),
                        WelcomeSystemEmailId = c.Int(),
                        ExitSystemEmailId = c.Int(),
                        AddUserAccountsDuringSync = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.SystemEmail", t => t.ExitSystemEmailId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupTypeRoleId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.DataView", t => t.SyncDataViewId)
                .ForeignKey("dbo.SystemEmail", t => t.WelcomeSystemEmailId)
                .Index(t => new { t.GroupId, t.GroupTypeRoleId }, unique: true, name: "IX_GroupIdGroupTypeRoleId")
                .Index(t => t.SyncDataViewId)
                .Index(t => t.WelcomeSystemEmailId)
                .Index(t => t.ExitSystemEmailId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            Sql( @"INSERT INTO [GroupSync] (GroupId
	                , GroupTypeRoleId
	                , SyncDataViewId
	                , WelcomeSystemEmailId
	                , ExitSystemEmailId
	                , AddUserAccountsDuringSync
	                , Guid)
                SELECT [Group].Id
	                , [GroupType].DefaultGroupRoleId
	                , [Group].SyncDataViewId
	                , [Group].WelcomeSystemEmailId
	                , [Group].ExitSystemEmailId
	                , [Group].AddUserAccountsDuringSync
	                , NEWID()
                FROM [Group]
                INNER JOIN GroupType ON [Group].GroupTypeId = GroupType.Id
                WHERE [Group].SyncDataViewId IS NOT NULL" );

            DropForeignKey( "dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail" );
            DropForeignKey( "dbo.Group", "SyncDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail" );
            DropIndex( "dbo.Group", new[] { "WelcomeSystemEmailId" } );
            DropIndex( "dbo.Group", new[] { "ExitSystemEmailId" } );
            DropIndex( "dbo.Group", new[] { "SyncDataViewId" } );

            DropColumn("dbo.Group", "WelcomeSystemEmailId");
            DropColumn("dbo.Group", "ExitSystemEmailId");
            DropColumn("dbo.Group", "SyncDataViewId");
            DropColumn("dbo.Group", "AddUserAccountsDuringSync");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Group", "AddUserAccountsDuringSync", c => c.Boolean());
            AddColumn("dbo.Group", "SyncDataViewId", c => c.Int());
            AddColumn("dbo.Group", "ExitSystemEmailId", c => c.Int());
            AddColumn("dbo.Group", "WelcomeSystemEmailId", c => c.Int());
            DropForeignKey("dbo.GroupSync", "WelcomeSystemEmailId", "dbo.SystemEmail");
            DropForeignKey("dbo.GroupSync", "SyncDataViewId", "dbo.DataView");
            DropForeignKey("dbo.GroupSync", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.GroupSync", "GroupTypeRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.GroupSync", "GroupId", "dbo.Group");
            DropForeignKey("dbo.GroupSync", "ExitSystemEmailId", "dbo.SystemEmail");
            DropForeignKey("dbo.GroupSync", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.GroupSync", new[] { "Guid" });
            DropIndex("dbo.GroupSync", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.GroupSync", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.GroupSync", new[] { "ExitSystemEmailId" });
            DropIndex("dbo.GroupSync", new[] { "WelcomeSystemEmailId" });
            DropIndex("dbo.GroupSync", new[] { "SyncDataViewId" });
            DropIndex("dbo.GroupSync", "IX_GroupIdGroupTypeRoleId");
            DropTable("dbo.GroupSync");
            CreateIndex("dbo.Group", "SyncDataViewId");
            CreateIndex("dbo.Group", "ExitSystemEmailId");
            CreateIndex("dbo.Group", "WelcomeSystemEmailId");
            AddForeignKey("dbo.Group", "WelcomeSystemEmailId", "dbo.SystemEmail", "Id");
            AddForeignKey("dbo.Group", "SyncDataViewId", "dbo.DataView", "Id");
            AddForeignKey("dbo.Group", "ExitSystemEmailId", "dbo.SystemEmail", "Id");
        }
    }
}
