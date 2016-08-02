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
    public partial class DigitalSignature3 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.ProcessSignatureDocuments", "Resend Invite After Number Days", "Number of days after sending last invite to sign, that a new invite should be resent.", 0, "5", "8B5EB6D6-12D1-4889-BBF4-89B85AC81142" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.ProcessSignatureDocuments", "Max Invites", "Maximum number of times an invite should be sent", 1, "3", "04724293-AEF7-4B85-AE64-1F0F5CB801E2" );

            Sql( @"
    DECLARE @JobId int = ( SELECT TOP 1 [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ProcessSignatureDocuments' )
    IF @JobId IS NULL
    BEGIN

	    -- Suggestion Notification Job
	    INSERT INTO [ServiceJob] ( [IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [Guid], [NotificationStatus] )
	    VALUES ( 0, 1, 'Process Signature Documents', 'Sends any digital signature invites that need to be sent for groups that require a signed document.',
		    'Rock.Jobs.ProcessSignatureDocuments','0 0 9 1/1 * ? *','77B2F2D4-D188-4716-9A79-F93AD4673F8C', 3 )
	    SET @JobId = SCOPE_IDENTITY()

	    DECLARE @AttributeId int

	    -- Resend Invite After Number Days attribute
	    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8B5EB6D6-12D1-4889-BBF4-89B85AC81142' )
	    IF @AttributeId IS NOT NULL
	    BEGIN
		    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		    VALUES ( 0, @AttributeId, @JobId, '5', NEWID() )
	    END

	    -- Max Invites attribute
	    SET @AttributeId = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '04724293-AEF7-4B85-AE64-1F0F5CB801E2' )
	    IF @AttributeId IS NOT NULL
	    BEGIN
		    INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
		    VALUES ( 0, @AttributeId, @JobId, '3', NEWID() )
	    END

    END
" );
            // JE - Reset the History Categories Page to Category Manager
            Sql( @"
    DECLARE @CategoryPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '95ACFF8C-B9EE-41C6-BAC0-D117D6E1FADC')

    -- update the page name
    UPDATE [Page]
    SET [BrowserTitle] = 'Category Manager', [PageTitle] = 'Category Manager', [InternalName] = 'Category Manager'
    WHERE [Id] = @CategoryPageId

    -- update the block settings
    DECLARE @CategoryBlockTypeId int = (SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '620FC4A2-6587-409F-8972-22065919D9AC')
    DECLARE @EntityTypeBlockAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = @CategoryBlockTypeId AND [Key] = 'EntityType')

    DECLARE @CategoryBlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [PageId] = @CategoryPageId AND [BlockTypeId] = @CategoryBlockTypeId)

    DELETE FROM [AttributeValue] 
    WHERE [AttributeId] = @EntityTypeBlockAttributeId AND [EntityId] = @CategoryBlockId
" );

            // TC - Migration for the 'Hide Attachment Uploader' Pull Request
            RockMigrationHelper.AddBlockTypeAttribute( "D9834641-7F39-4CFA-8CB2-E64068127565", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Attachment Uploader", "ShowAttachmentUploader", "", "Should the attachment uploader be shown for email communications.", 6, @"True", "068DF91F-EDA8-49C1-963B-AB34AA12DE5E" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "068DF91F-EDA8-49C1-963B-AB34AA12DE5E" );
        }
    }
}
