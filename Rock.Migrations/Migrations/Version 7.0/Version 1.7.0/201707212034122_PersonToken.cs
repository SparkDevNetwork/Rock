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
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int( nullable: false ),
                    Token = c.String( maxLength: 32 ),
                    ExpireDateTime = c.DateTime(),
                    TimesUsed = c.Int( nullable: false ),
                    UsageLimit = c.Int(),
                    LastUsedDateTime = c.DateTime(),
                    PageId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Page", t => t.PageId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true )
                .Index( t => t.PersonAliasId )
                .Index( t => t.Token, unique: true )
                .Index( t => t.PageId )
                .Index( t => t.Guid, unique: true );

            // Add global attributes for Person Tokens
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.BOOLEAN, "", "", "Person Token Use Legacy Fallback", @"Use the pre-v7 person token lookup if the impersonation token can't be found using the v7 person tokens.", 0, true.ToString(), "8063EAE0-5FFC-4113-8F7B-A45CC0BE3B63", "core.PersonTokenUseLegacyFallback" );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.INTEGER, "", "", "Person Token Expire Minutes", @"The default number of minutes a person token is valid after it is issued.", 0, ( 30 * 24 * 60 ).ToString(), "D4EDDB65-5861-442B-8109-A4EBBE9A961F", "core.PersonTokenExpireMinutes" );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.INTEGER, "", "", "Person Token Usage Limit", @"The default maximum number of times a person token can be used.", 0, "", "28D921E5-045F-49BE-A8F3-C8FA60331D45", "core.PersonTokenUsageLimit" );

            // Assign 'Config' category to the Person Token Global attributes
            Sql( @"
  DECLARE @ConfigCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'BB40B563-18D1-4133-94B9-D7F67D95E4E3')
  INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId]) select Id, @ConfigCategoryId from [Attribute] where [Guid] in ('8063EAE0-5FFC-4113-8F7B-A45CC0BE3B63', 'D4EDDB65-5861-442B-8109-A4EBBE9A961F', '28D921E5-045F-49BE-A8F3-C8FA60331D45');
  " );

            // register new Person Transaction Links block type
            RockMigrationHelper.UpdateBlockType( "Person Transaction Links", "Block for displaying links to add and schedule transactions for a person.", "~/Blocks/Crm/PersonDetail/TransactionLinks.ascx", "CRM > Person Detail", "2BB707AC-F29A-44DF-A103-7454077509B4" );

            // Attrib for BlockType: Person Transaction Links:Person Token Usage Limit
            RockMigrationHelper.UpdateBlockTypeAttribute( "2BB707AC-F29A-44DF-A103-7454077509B4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "", "The maximum number of times the person token for the transaction can be used.", 3, @"1", "0B458C5E-A15F-4C05-9926-7A298C377D63" );

            // Attrib for BlockType: Person Transaction Links:Is Secondary Block
            RockMigrationHelper.UpdateBlockTypeAttribute( "2BB707AC-F29A-44DF-A103-7454077509B4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Is Secondary Block", "IsSecondaryBlock", "", "Flag indicating whether this block is considered secondary and should be hidden when other secondary blocks are hidden.", 1, @"False", "53F61E84-CABF-44D0-AFC4-FCC777B9F3BE" );

            // Attrib for BlockType: Person Transaction Links:Person Token Expire Minutes
            RockMigrationHelper.UpdateBlockTypeAttribute( "2BB707AC-F29A-44DF-A103-7454077509B4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "", "The number of minutes the person token for the transaction is valid after it is issued.", 2, @"60", "D2B1BD60-04F1-4809-AA19-75581B29341D" );

            // Attrib for BlockType: Person Transaction Links:Add Transaction Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "2BB707AC-F29A-44DF-A103-7454077509B4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Transaction Page", "AddTransactionPage", "", "", 0, @"B1CA86DC-9890-4D26-8EBD-488044E1B3DD", "A37EF84F-95AF-45E5-A6CC-0C8E591F80DB" );


            /* Update Transaction Links HTML blocks to use TranactionLinks block instead */
            // delete old Transaction Links HTML block from Person Contributions tab
            RockMigrationHelper.DeleteBlock( "6F7F97D3-6C7D-4B58-A6E7-9A21BF55428A" );

            // Add Block to Page: Person | Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "2BB707AC-F29A-44DF-A103-7454077509B4", "Person Transaction Links", "SectionA2", @"<div class=""panel panel-block""><div class=""panel-body"">", @"", 0, "013ACB2A-48AD-4325-9566-6A6B821C8C21" );

            // delete old Transaction Links HTML block from Business Detail
            RockMigrationHelper.DeleteBlock( "84F800D3-C32E-4A16-9F84-081F8CB4DCBF" );

            // Add Block to Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "D2B43273-C64F-4F57-9AAE-9571E1982BAC", "", "2BB707AC-F29A-44DF-A103-7454077509B4", "Transaction Links", "Main", @"<div class='col-md-4'>
<div class=""panel panel-block js-hide-nocontent""><div class=""panel-body"">", @"", 2, "4A7394DA-4E92-4E15-B75E-0C79E691A9B2" );

            // Attrib Value for Block:Transaction Links, Attribute:Is Secondary Block Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4A7394DA-4E92-4E15-B75E-0C79E691A9B2", "53F61E84-CABF-44D0-AFC4-FCC777B9F3BE", @"True" );
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

            DropForeignKey( "dbo.PersonToken", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonToken", "PageId", "dbo.Page" );
            DropIndex( "dbo.PersonToken", new[] { "Guid" } );
            DropIndex( "dbo.PersonToken", new[] { "PageId" } );
            DropIndex( "dbo.PersonToken", new[] { "Token" } );
            DropIndex( "dbo.PersonToken", new[] { "PersonAliasId" } );
            DropTable( "dbo.PersonToken" );
        }
    }
}
