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
    public partial class PersonToken : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonToken",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        Token = c.String(maxLength: 32),
                        ExpireDateTime = c.DateTime(),
                        TimesUsed = c.Int(nullable: false),
                        UsageLimit = c.Int(),
                        LastUsedDateTime = c.DateTime(),
                        PageId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Page", t => t.PageId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .Index(t => t.PersonAliasId)
                .Index(t => t.Token, unique: true)
                .Index(t => t.PageId)
                .Index(t => t.Guid, unique: true);

            // Add global attributes for Person Tokens
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.BOOLEAN, "", "", "Person Token Use Legacy Fallback", @"Use the pre-v7 person token lookup if the impersonation token can't be found using the v7 person tokens.", 0, true.ToString(), "8063EAE0-5FFC-4113-8F7B-A45CC0BE3B63", "core.PersonTokenUseLegacyFallback" );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.INTEGER, "", "", "Person Token Expire Days", @"The default number of days a person token is valid after it is issued.", 0, 30.ToString(), "D4EDDB65-5861-442B-8109-A4EBBE9A961F", "core.PersonTokenExpireDays" );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.INTEGER, "", "", "Person Token Usage Limit", @"The default maximum number of times a person token can be used.", 0, "", "28D921E5-045F-49BE-A8F3-C8FA60331D45", "core.PersonTokenUsageLimit" );

            // Assign 'Config' category to the Person Token Global attributes
            Sql( @"
  DECLARE @ConfigCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'BB40B563-18D1-4133-94B9-D7F67D95E4E3')
  INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId]) select Id, @ConfigCategoryId from [Attribute] where [Guid] in ('8063EAE0-5FFC-4113-8F7B-A45CC0BE3B63', 'D4EDDB65-5861-442B-8109-A4EBBE9A961F', '28D921E5-045F-49BE-A8F3-C8FA60331D45');
  " );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete global attributes for Person Tokens
            RockMigrationHelper.DeleteAttribute( "8063EAE0-5FFC-4113-8F7B-A45CC0BE3B63" );
            RockMigrationHelper.DeleteAttribute( "D4EDDB65-5861-442B-8109-A4EBBE9A961F" );
            RockMigrationHelper.DeleteAttribute( "28D921E5-045F-49BE-A8F3-C8FA60331D45" );

            DropForeignKey("dbo.PersonToken", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonToken", "PageId", "dbo.Page");
            DropIndex("dbo.PersonToken", new[] { "Guid" });
            DropIndex("dbo.PersonToken", new[] { "PageId" });
            DropIndex("dbo.PersonToken", new[] { "Token" });
            DropIndex("dbo.PersonToken", new[] { "PersonAliasId" });
            DropTable("dbo.PersonToken");
        }
    }
}
