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
    public partial class AddIconToPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Block", "AdditionalSettings", c => c.String() );
            AddColumn( "dbo.Page", "IconBinaryFileId", c => c.Int() );
            AddColumn( "dbo.Page", "AdditionalSettings", c => c.String() );
            AddColumn( "dbo.Site", "ConfigurationMobilePhoneBinaryFileId", c => c.Int() );
            AddColumn( "dbo.Site", "ConfigurationMobileTabletBinaryFileId", c => c.Int() );
            AddColumn( "dbo.Site", "ThumbnailBinaryFileId", c => c.Int() );
            CreateIndex( "dbo.Page", "IconBinaryFileId" );
            CreateIndex( "dbo.Site", "ConfigurationMobilePhoneBinaryFileId" );
            CreateIndex( "dbo.Site", "ConfigurationMobileTabletBinaryFileId" );
            CreateIndex( "dbo.Site", "ThumbnailBinaryFileId" );
            AddForeignKey( "dbo.Page", "IconBinaryFileId", "dbo.BinaryFile", "Id" );
            AddForeignKey( "dbo.Site", "ConfigurationMobilePhoneBinaryFileId", "dbo.BinaryFile", "Id" );
            AddForeignKey( "dbo.Site", "ConfigurationMobileTabletBinaryFileId", "dbo.BinaryFile", "Id" );
            AddForeignKey( "dbo.Site", "ThumbnailBinaryFileId", "dbo.BinaryFile", "Id" );
            DropColumn( "dbo.Site", "ConfigurationMobilePhoneFileId" );
            DropColumn( "dbo.Site", "ConfigurationMobileTabletFileId" );
            DropColumn( "dbo.Site", "ThumbnailFileId" );

            AddMobileAppBundleBinaryFileType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.Site", "ThumbnailFileId", c => c.Int());
            AddColumn("dbo.Site", "ConfigurationMobileTabletFileId", c => c.Int());
            AddColumn("dbo.Site", "ConfigurationMobilePhoneFileId", c => c.Int());
            DropForeignKey("dbo.Site", "ThumbnailBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Site", "ConfigurationMobileTabletBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Site", "ConfigurationMobilePhoneBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.Page", "IconBinaryFileId", "dbo.BinaryFile");
            DropIndex("dbo.Site", new[] { "ThumbnailBinaryFileId" });
            DropIndex("dbo.Site", new[] { "ConfigurationMobileTabletBinaryFileId" });
            DropIndex("dbo.Site", new[] { "ConfigurationMobilePhoneBinaryFileId" });
            DropIndex("dbo.Page", new[] { "IconBinaryFileId" });
            DropColumn("dbo.Site", "ThumbnailBinaryFileId");
            DropColumn("dbo.Site", "ConfigurationMobileTabletBinaryFileId");
            DropColumn("dbo.Site", "ConfigurationMobilePhoneBinaryFileId");
            DropColumn("dbo.Page", "AdditionalSettings");
            DropColumn("dbo.Page", "IconBinaryFileId");
            DropColumn("dbo.Block", "AdditionalSettings");
        }

        private void AddMobileAppBundleBinaryFileType()
        {
#pragma warning disable 0618
            RockMigrationHelper.UpdateBinaryFileType( "A97B6002-454E-4890-B529-B99F8F2F376A", "Mobile App Bundle", "File type for mobile app files.", "", "ED456BF6-78F4-4954-9043-CA6849DA2D7E", false, false );
#pragma warning restore 0618
            
            Sql( @"
                DECLARE @BinaryFileTypeId INT = (SELECT [Id] FROM [BinaryFileType] WHERE [Guid]='ED456BF6-78F4-4954-9043-CA6849DA2D7E')
                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid]='3CAFA34D-9208-439B-A046-CB727FB729DE')

                IF NOT EXISTS(SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BinaryFileTypeId)
                                BEGIN
                                    INSERT INTO [AttributeValue] (
                                          [IsSystem]
		                                , [AttributeId]
		                                , [EntityId]
		                                , [Value]
		                                , [Guid])
                                    VALUES(
                                          1
		                                , @AttributeId
		                                , @BinaryFileTypeId
		                                , '~/App_Data/MobileAppBundles'
		                                , '1BC4FD30-6C6B-491D-9317-503D1B7AF1F5')
                                END
" );
        }
    }
}
