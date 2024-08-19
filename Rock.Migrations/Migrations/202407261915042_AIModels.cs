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
    public partial class AIModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AIProvider",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 50 ),
                    Order = c.Int( nullable: false ),
                    ProviderComponentEntityTypeId = c.Int( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    IsActive = c.Boolean( nullable: false ),
                    Description = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.ProviderComponentEntityTypeId )
                .Index( t => t.ProviderComponentEntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.NoteType", "AdditionalSettingsJson", c => c.String() );
            AddColumn( "dbo.PrayerRequest", "OriginalRequest", c => c.String() );
            AddColumn( "dbo.PrayerRequest", "SentimentEmotionValueId", c => c.Int() );
            AddColumn( "dbo.PrayerRequest", "ModerationFlags", c => c.Long( nullable: false ) );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.AIProvider", "ProviderComponentEntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.AIProvider", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AIProvider", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.AIProvider", new[] { "Guid" } );
            DropIndex( "dbo.AIProvider", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AIProvider", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AIProvider", new[] { "ProviderComponentEntityTypeId" } );
            DropColumn( "dbo.PrayerRequest", "ModerationFlags" );
            DropColumn( "dbo.PrayerRequest", "SentimentEmotionValueId" );
            DropColumn( "dbo.PrayerRequest", "OriginalRequest" );
            DropColumn( "dbo.NoteType", "AdditionalSettingsJson" );
            DropTable( "dbo.AIProvider" );
        }
    }
}
