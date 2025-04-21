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
using Rock.Model;

namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class CreateCheckInLabelModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CheckInLabel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
                        LabelFormat = c.Int(nullable: false),
                        LabelType = c.Int(nullable: false),
                        Content = c.String(),
                        PreviewImage = c.Binary(),
                        AdditionalSettingsJson = c.String(),
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
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.CheckInLabel",
                SystemGuid.EntityType.CHECK_IN_LABEL,
                true,
                true );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.CheckInLabel",
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "E0AA03EF-A255-4C6B-ACB6-6DD92E227BC7" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.CheckInLabel",
                1,
                Security.Authorization.VIEW,
                false,
                string.Empty,
                ( int ) SpecialRole.AllUsers,
                "3545614F-0EEA-4030-B6F5-B4DEEB812CAB" );

            // We fixed a bug in this same commit that caused duplicate REST
            // controller names to get merged together into one RestController
            // model. This happened with Rock.Rest.Controllers.CheckinController
            // and Rock.Rest.v2.CheckInController. The fix is to attempt to put
            // the RestController back and then when Rock finishes startup it
            // will properly register and fixup the rest of the RestAction data.
            //
            // Use a case sensitive search to find the right entity.
            Sql( @"
IF NOT EXISTS (SELECT 1 FROM [RestController] WHERE [Guid] = '921331A3-879C-46BB-B37B-A274CF00378E')
BEGIN
    UPDATE [RestController] SET
        [ClassName] = 'Rock.Rest.Controllers.CheckinController',
        [Guid] = '921331A3-879C-46BB-B37B-A274CF00378E'
    WHERE [Name] = 'Checkin' COLLATE Latin1_General_Bin
    AND [Guid] = '52b3c68a-da8d-4374-a199-8bc8368a22bc'
END
" );

            RockMigrationHelper.AddRestController( "CheckIn", "Rock.Rest.v2.CheckInController" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "3545614F-0EEA-4030-B6F5-B4DEEB812CAB" );
            RockMigrationHelper.DeleteSecurityAuth( "E0AA03EF-A255-4C6B-ACB6-6DD92E227BC7" );

            DropForeignKey( "dbo.CheckInLabel", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CheckInLabel", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.CheckInLabel", new[] { "Guid" });
            DropIndex("dbo.CheckInLabel", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CheckInLabel", new[] { "CreatedByPersonAliasId" });
            DropTable("dbo.CheckInLabel");
        }
    }
}
