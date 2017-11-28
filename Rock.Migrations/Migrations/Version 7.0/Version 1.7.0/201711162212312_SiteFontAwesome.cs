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
    public partial class SiteFontAwesome : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.SiteIconExtensions",
                c => new
                    {
                        SiteId = c.Int(nullable: false),
                        DefinedValueId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SiteId, t.DefinedValueId })
                .ForeignKey("dbo.Site", t => t.SiteId, cascadeDelete: true)
                .ForeignKey("dbo.DefinedValue", t => t.DefinedValueId, cascadeDelete: true)
                .Index(t => t.SiteId)
                .Index(t => t.DefinedValueId);
            
            AddColumn("dbo.Site", "IconCssWeight", c => c.Int(nullable: false));

            // MP: Update Communication Binary File Types
            Sql( @"
-- Create BinaryFileTypeIdCOMMUNICATION_ATTACHMENT
DECLARE @StorageEntityTypeFileSystemId INT = (
        SELECT Id
        FROM EntityType
        WHERE [Guid] = 'A97B6002-454E-4890-B529-B99F8F2F376A'
        )

    IF NOT EXISTS (
        SELECT Id
        FROM BinaryFileType
        WHERE [Guid] = '10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C'
    )
    BEGIN
        INSERT INTO [dbo].[BinaryFileType] (
            [IsSystem]
            ,[Name]
            ,[Description]
            ,[IconCssClass]
            ,[StorageEntityTypeId]
            ,[AllowCaching]
            ,[Guid]
            ,[RequiresViewSecurity]
        )
        VALUES (
             1
            ,'Communication Attachment'
            ,'Used when attaching a file to Email or SMS Communications'
            ,'fa fa-comment-o'
            ,@StorageEntityTypeFileSystemId
            ,1
            ,'10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C'
            ,0
        );
    END
    ELSE
    BEGIN
        UPDATE [dbo].[BinaryFileType]
        SET 
             [Name] = 'Communication Attachment'
            ,[IsSystem] = 1
            ,[Description] = 'Used when attaching a file to Email or SMS Communications'
            ,[IconCssClass] = 'fa fa-comment-o'
            ,[StorageEntityTypeId] = @StorageEntityTypeFileSystemId
            ,[AllowCaching] = 1
            ,[RequiresViewSecurity] = 0
        WHERE [Guid] = '10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C'
    END


DECLARE @BinaryFileTypeIdCOMMUNICATION_IMAGE INT = (
		SELECT TOP 1 ID
		FROM BinaryFileType
		WHERE [Guid] = '60B896C3-F00C-411C-A31C-2D5D4CCBB65F'
		)
	,@BinaryFileTypeIdCOMMUNICATION_ATTACHMENT INT = (
		SELECT TOP 1 ID
		FROM BinaryFileType
		WHERE [Guid] = '10FD7FE8-7187-45CC-A1E7-D9F71BD90E6C'
		)
	,@AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH INT = (
		SELECT TOP 1 ID
		FROM Attribute
		WHERE [Guid] = '3CAFA34D-9208-439B-A046-CB727FB729DE'
		)

-- Set RootPath for @BinaryFileTypeIdCOMMUNICATION_IMAGE
IF NOT EXISTS (
		SELECT *
		FROM Attributevalue
		WHERE AttributeId = @AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
			AND EntityId = @BinaryFileTypeIdCOMMUNICATION_IMAGE
		)
BEGIN
	INSERT INTO AttributeValue (
		IsSystem
		,AttributeId
		,EntityId
		,Value
		,[Guid]
		)
	VALUES (
		1
		,@AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
		,@BinaryFileTypeIdCOMMUNICATION_IMAGE
		,'~/App_Data/Communication Images'
		,'235B8331-1B2F-43B9-B2B2-51D5A50F5667'
		)
END
ELSE
BEGIN
	UPDATE AttributeValue
	SET [Value] = '~/App_Data/Communication Images'
		,[Guid] = '235B8331-1B2F-43B9-B2B2-51D5A50F5667'
	WHERE AttributeId = @AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
		AND EntityId = @BinaryFileTypeIdCOMMUNICATION_IMAGE
END

-- Set RootPath for @BinaryFileTypeIdCOMMUNICATION_ATTACHMENT
IF NOT EXISTS (
		SELECT *
		FROM Attributevalue
		WHERE AttributeId = @AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
			AND EntityId = @BinaryFileTypeIdCOMMUNICATION_ATTACHMENT
		)
BEGIN
	INSERT INTO AttributeValue (
		IsSystem
		,AttributeId
		,EntityId
		,Value
		,[Guid]
		)
	VALUES (
		1
		,@AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
		,@BinaryFileTypeIdCOMMUNICATION_ATTACHMENT
		,'~/App_Data/Communication Attachments'
		,'165195E1-EBE4-436B-849D-8035040E0180'
		)
END
ELSE
BEGIN
	UPDATE AttributeValue
	SET [Value] = '~/App_Data/Communication Attachments'
		,[Guid] = '165195E1-EBE4-436B-849D-8035040E0180'
	WHERE AttributeId = @AttributeIdBINARYFILETYPE_FILESTORAGE_ROOTPATH
		AND EntityId = @BinaryFileTypeIdCOMMUNICATION_ATTACHMENT
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.SiteIconExtensions", "DefinedValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.SiteIconExtensions", "SiteId", "dbo.Site");
            DropIndex("dbo.SiteIconExtensions", new[] { "DefinedValueId" });
            DropIndex("dbo.SiteIconExtensions", new[] { "SiteId" });
            DropColumn("dbo.Site", "IconCssWeight");
            DropTable("dbo.SiteIconExtensions");
        }
    }
}
