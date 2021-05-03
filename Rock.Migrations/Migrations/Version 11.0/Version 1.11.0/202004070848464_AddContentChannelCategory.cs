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
    public partial class AddContentChannelCategory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ContentChannelCategory",
                c => new
                {
                    ContentChannelId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.ContentChannelId, t.CategoryId } )
                .ForeignKey( "dbo.ContentChannel", t => t.ContentChannelId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.ContentChannelId )
                .Index( t => t.CategoryId );

            // Attrib for BlockType: Content Channel Navigation:Show Category Filter             
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Category Filter", "ShowCategoryFilter", "Show Category Filter", @"Should block add an option to allow filtering by category?", 5, @"True", "28443364-4FE0-4A87-99DC-C799D4978640" );
            // Attrib for BlockType: Content Channel Navigation:Parent Category              
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0E023AE3-BF08-48E0-93F8-08C32EB5CAFA", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Parent Category", "ParentCategory", "Parent Category", @"The parent category to use as the root category available for the user to pick from.", 6, @"", "366E80AB-8D9D-40C2-8C8E-9911BF49F27D" );

            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Content Channel Categories", "", "0F1B45B8-032D-4306-834D-670FA3933589", "fa fa-folder" );
            // Add Block to Page: Content Channel Categories Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "0F1B45B8-032D-4306-834D-670FA3933589".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "620FC4A2-6587-409F-8972-22065919D9AC".AsGuid(), "Categories", "Main", @"", @"", 0, "3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9" );
            RockMigrationHelper.AddBlockAttributeValue( "3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9", "C405A507-7889-4287-8342-105B89710044", @"44484685-477e-4668-89a6-84f29739eb68" );
            RockMigrationHelper.AddBlockAttributeValue( "3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9", "736C3F4B-34CC-4B4B-9811-7171C82DDC41", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9", "547BF211-7B74-4CE2-A72B-561B4715D230", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9", "4B9D48AB-495A-4029-B784-6A8F96129397", @"False" );
            RockMigrationHelper.UpdateCategory( "44484685-477E-4668-89A6-84F29739EB68", "External Website", "", "", "1AD93BC0-5FD2-43AE-9831-DB144E107F84" );
            RockMigrationHelper.UpdateCategory( "44484685-477E-4668-89A6-84F29739EB68", "Internal", "", "", "FF5FA5F7-B701-4F83-94D2-054F2A569808" );

            //External Website - External Website Ads
            UpdateContentChannelCategory( "1AD93BC0-5FD2-43AE-9831-DB144E107F84", "8E213BB1-9E6F-40C1-B468-B3F8A60D5D24" );
            //External Website - Website Blog
            UpdateContentChannelCategory( "1AD93BC0-5FD2-43AE-9831-DB144E107F84", "2B408DA7-BDD1-4E71-B6AC-F22D786B605F" );
            //External Website - Podcast Series
            UpdateContentChannelCategory( "1AD93BC0-5FD2-43AE-9831-DB144E107F84", "E2C598F1-D299-1BAA-4873-8B679E3C1998" );
            //External Website - Podcast Message
            UpdateContentChannelCategory( "1AD93BC0-5FD2-43AE-9831-DB144E107F84", "0A63A427-E6B5-2284-45B3-789B293C02EA" );
            //External Website - Internal Communications - Homepage
            UpdateContentChannelCategory( "FF5FA5F7-B701-4F83-94D2-054F2A569808", "78D01959-0EA6-4FE4-B6F8-A3765F0CEDBF" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute("366E80AB-8D9D-40C2-8C8E-9911BF49F27D");
            RockMigrationHelper.DeleteAttribute("28443364-4FE0-4A87-99DC-C799D4978640");
            RockMigrationHelper.DeleteBlock("3D6C13E9-C92C-44D0-9AE7-07AA9F1931E9");
            RockMigrationHelper.DeletePage( "0F1B45B8-032D-4306-834D-670FA3933589" ); //  Page: Content Channel Categories, Layout: Full Width, Site: Rock RMS

            DropForeignKey("dbo.ContentChannelCategory", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.ContentChannelCategory", "ContentChannelId", "dbo.ContentChannel");
            DropIndex("dbo.ContentChannelCategory", new[] { "CategoryId" });
            DropIndex("dbo.ContentChannelCategory", new[] { "ContentChannelId" });
            DropTable("dbo.ContentChannelCategory");
        }

        private void UpdateContentChannelCategory( string categoryGuid, string contentChannelGuid )
        {
            Sql( string.Format( @"

                DECLARE @ContentChannelId int
                SET @ContentChannelId = (SELECT [Id] FROM [ContentChannel] WHERE [Guid] = '{0}')

                DECLARE @CategoryId int
                SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{1}')

                IF (@CategoryId is not null and @ContentChannelId is not null)
				BEGIN
                    IF NOT EXISTS (
                        SELECT *
                        FROM [ContentChannelCategory]
                        WHERE [ContentChannelId] = @ContentChannelId
                        AND [CategoryId] = CategoryId )
                    BEGIN
                        INSERT INTO [ContentChannelCategory] ( [ContentChannelId], [CategoryId] )
                        VALUES( @ContentChannelId, @CategoryId )
                    END
                END
",
                  contentChannelGuid,
                  categoryGuid )
          );
        }

    }
}
