//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    /// 
    /// </summary>
    public partial class AddPrayerRequestAdminPages : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Get or Add PrayerRequest Entity Type and then
            // Get or Add NoteType and then
            // Add default prayer request Categories unless they already exist
            Sql( @"
                -- Get the entitytype for PrayerRequest
                DECLARE @PrayerRequestEntityTypeId int
                SELECT @PrayerRequestEntityTypeId = [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.PrayerRequest'
                IF @PrayerRequestEntityTypeId IS NULL
                BEGIN
                    INSERT INTO [EntityType] ([Name], [FriendlyName], [Guid], [IsEntity], [IsSecured])
                    VALUES ('Rock.Model.PrayerRequest', 'Prayer Request', 'F13C8FD2-7702-4C79-A6A9-86440DD5DE13', 1, 1)
                    SET @PrayerRequestEntityTypeId = SCOPE_IDENTITY()
                END

                -- Add 'Prayer Comment' NoteType if it's not already there
                DECLARE @PrayerCommentNoteTypeId int
                SELECT @PrayerCommentNoteTypeId = [Id] FROM [NoteType] WHERE [Guid] = '0EBABD75-0890-4756-A9EE-62626282BB5D'
                IF @PrayerCommentNoteTypeId IS NULL
                BEGIN
                    INSERT INTO [NoteType]
                           ([IsSystem]
                           ,[EntityTypeId]
                           ,[Name]
                           ,[SourcesTypeId]
                           ,[EntityTypeQualifierColumn]
                           ,[EntityTypeQualifierValue]
                           ,[Guid])
                     VALUES
                           (1
                           ,@PrayerRequestEntityTypeId
                           ,'Prayer Comment'
                           ,NULL
                           ,''
                           ,''
                           ,'0EBABD75-0890-4756-A9EE-62626282BB5D')
                    SET @PrayerCommentNoteTypeId = SCOPE_IDENTITY() -- not used
                END

                -- Add default prayer Categories
                DECLARE @TopLevelPrayerCategoryId int
                SELECT @TopLevelPrayerCategoryId = [Id] FROM [Category] WHERE [Guid] = '5a94e584-35f0-4214-91f1-d72531cc6325'
                IF @TopLevelPrayerCategoryId IS NULL
                BEGIN
                    INSERT INTO [Category]
                               ([IsSystem]
                               ,[ParentCategoryId]
                               ,[EntityTypeId]
                               ,[EntityTypeQualifierColumn]
                               ,[EntityTypeQualifierValue]
                               ,[Name]
                               ,[Guid]
                               ,[IconSmallFileId]
                               ,[IconLargeFileId]
                               ,[IconCssClass])
                         VALUES
                               (1
                               ,NULL
                               ,@PrayerRequestEntityTypeId
                               ,NULL
                               ,NULL
                               ,'All Church'
                               ,'5a94e584-35f0-4214-91f1-d72531cc6325'
                               ,NULL
                               ,NULL
                               ,NULL)
                    SET @TopLevelPrayerCategoryId = SCOPE_IDENTITY()
                END

                DECLARE @GeneralPrayerCategoryId int
                SELECT @GeneralPrayerCategoryId = [Id] FROM [Category] WHERE [Guid] = '4b2d88f5-6e45-4b4b-8776-11118c8e8269'
                IF @GeneralPrayerCategoryId IS NULL
                BEGIN
                    INSERT INTO [Category]
                               ([IsSystem]
                               ,[ParentCategoryId]
                               ,[EntityTypeId]
                               ,[EntityTypeQualifierColumn]
                               ,[EntityTypeQualifierValue]
                               ,[Name]
                               ,[Guid]
                               ,[IconSmallFileId]
                               ,[IconLargeFileId]
                               ,[IconCssClass])
                         VALUES
                               (1
                               ,@TopLevelPrayerCategoryId
                               ,@PrayerRequestEntityTypeId
                               ,NULL
                               ,NULL
                               ,'General'
                               ,'4b2d88f5-6e45-4b4b-8776-11118c8e8269'
                               ,NULL
                               ,NULL
                               ,NULL)
                END
"
                    );

            // new pages
            AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "Prayer Administration", "A place to manage the prayer requests and comments in the prayer system.", "Default", "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF" );
            AddPage( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "Prayer Request Detail", "", "Default", "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48" );

            // new block types
            AddBlockType( "Prayer Request List", "Displays a list of unapproved Prayer Requests for a given configured top-level Category.", "~/Blocks/Prayer/Admin/PrayerRequestList.ascx", "4D6B686A-79DF-4EFC-A8BA-9841C248BF74" );
            AddBlockType( "Prayer Request Detail", "Displays the details of a given Prayer Request for viewing or edit.", "~/Blocks/Prayer/Admin/PrayerRequestDetail.ascx", "F791046A-333F-4B2A-9815-73B60326162D" );
            AddBlockType( "Prayer Comment List", "Displays a list of unapproved prayer comments for a given top-level category.", "~/Blocks/Prayer/Admin/PrayerCommentList.ascx", "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22" );
            AddBlockType( "Prayer Comment Detail", "Shows a list of prayer comments and allows the noteId that is passed in (via querystring) to be editable.", "~/Blocks/Prayer/Admin/PrayerCommentDetail.ascx", "4F3778DF-A25C-4E59-9242-B1D6813311E1" );

            // new block instances on those pages.
            AddBlock( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "Unapproved Prayer Requests", "", "Content", 0, "E27EA67E-AB6E-4F61-A03B-D7697BBE922C" );
            AddBlock( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48", "F791046A-333F-4B2A-9815-73B60326162D", "Prayer Request Detail", "", "Content", 0, "716DABF0-CE00-40CB-9796-3EC2A5918468" );
            AddBlock( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF", "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "Unapproved Prayer Comments", "", "Content", 1, "601DDB93-555D-4E08-AAA3-EE0807BFD3E1" );
            AddBlock( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48", "4F3778DF-A25C-4E59-9242-B1D6813311E1", "Prayer Comments", "", "Content", 1, "64A70C1B-37D1-4F2F-B658-8D752DAAAF89" );

            // new block attributes for those new block instances
            AddBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Group Category Id", "GroupCategoryId", "Filtering", "The id of a 'top level' Category.  Only prayer requests under this category will be shown.", 1, "-1", "78F370A6-7527-41BD-BDC0-3A9794DCFFB2" );
            AddBlockTypeAttribute( "F791046A-333F-4B2A-9815-73B60326162D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Group Category Id", "GroupCategoryId", "Filtering", "The id of 'prayer group category'.  Only prayer requests under this category will be shown.", 1, "-1", "FFDE065C-3748-4795-B942-E4C475B22E07" );
            AddBlockTypeAttribute( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "A1275318-F6B4-4C66-B782-99037E6E16C0" );
            AddBlockTypeAttribute( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Group Category Id", "GroupCategoryId", "Filtering", "The id of a 'top level' Category.  Only prayer requests comments under this category will be shown.", 1, "-1", "4C8B7B5B-14D8-4529-B075-464E9BEDB6E6" );
            AddBlockTypeAttribute( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "", "", 0, "", "8F9A2D85-E2AE-46A8-B64E-ADE8AC0555E8" );
            AddBlockTypeAttribute( "4F3778DF-A25C-4E59-9242-B1D6813311E1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Type", "NoteType", "Behavior", "The note type name associated with the context entity to use (If it doesn't exist it will be created. Default is 'Prayer Comment').", 0, "Prayer Comment", "1F2621D8-789C-4884-9845-55BEC772967C" );
            
            // Attrib Value for Unapproved Prayer Requests:Group Category Id
            AddBlockAttributeValue("E27EA67E-AB6E-4F61-A03B-D7697BBE922C","78F370A6-7527-41BD-BDC0-3A9794DCFFB2","-1");  
            // Attrib Value for Prayer Request Detail:Group Category Id
            AddBlockAttributeValue("716DABF0-CE00-40CB-9796-3EC2A5918468","FFDE065C-3748-4795-B942-E4C475B22E07","1");  
            // Attrib Value for Unapproved Prayer Requests:Detail Page Guid
            AddBlockAttributeValue("E27EA67E-AB6E-4F61-A03B-D7697BBE922C","A1275318-F6B4-4C66-B782-99037E6E16C0","89c3db4a-bafd-45c8-88c6-45d8fec48b48");  
            // Attrib Value for Unapproved Prayer Comments:Group Category Id
            AddBlockAttributeValue("601DDB93-555D-4E08-AAA3-EE0807BFD3E1","4C8B7B5B-14D8-4529-B075-464E9BEDB6E6","-1");  
            // Attrib Value for Unapproved Prayer Comments:Detail Page Guid
            AddBlockAttributeValue("601DDB93-555D-4E08-AAA3-EE0807BFD3E1","8F9A2D85-E2AE-46A8-B64E-ADE8AC0555E8","89c3db4a-bafd-45c8-88c6-45d8fec48b48");

            // Add a PageContext for the 'Prayer Request Detail' page
            Sql( @"
                -- Page Context
                INSERT INTO [dbo].[PageContext]([IsSystem],[PageId],[Entity],[IdParameter],[CreatedDateTime],[Guid])       VALUES(1             ,(select [Id] from [Page] where [Guid] = '89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48')             ,'Rock.Model.PrayerRequest'             ,'prayerRequestId'             ,SYSDATETIME()             ,'22435C8F-A481-4806-AA7D-11713ED96986')
"
                );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete the page context
            Sql( @"DELETE FROM [dbo].[PageContext] where [Guid] = '22435C8F-A481-4806-AA7D-11713ED96986'" );

            // delete the attributes 
            DeleteAttribute( "78F370A6-7527-41BD-BDC0-3A9794DCFFB2" ); // Group Category Id
            DeleteAttribute( "FFDE065C-3748-4795-B942-E4C475B22E07" ); // Group Category Id
            DeleteAttribute( "A1275318-F6B4-4C66-B782-99037E6E16C0" ); // Detail Page Guid
            DeleteAttribute( "4C8B7B5B-14D8-4529-B075-464E9BEDB6E6" ); // Group Category Id
            DeleteAttribute( "8F9A2D85-E2AE-46A8-B64E-ADE8AC0555E8" ); // Detail Page Guid
            DeleteAttribute( "1F2621D8-789C-4884-9845-55BEC772967C" ); // Note Type

            // delete block instances
            DeleteBlock( "E27EA67E-AB6E-4F61-A03B-D7697BBE922C" ); // Unapproved Prayer Requests
            DeleteBlock( "716DABF0-CE00-40CB-9796-3EC2A5918468" ); // Prayer Request Detail
            DeleteBlock( "601DDB93-555D-4E08-AAA3-EE0807BFD3E1" ); // Unapproved Prayer Comments
            DeleteBlock( "64A70C1B-37D1-4F2F-B658-8D752DAAAF89" ); // Prayer Comments

            // delete the block types
            DeleteBlockType( "4D6B686A-79DF-4EFC-A8BA-9841C248BF74" ); // Prayer Request List
            DeleteBlockType( "F791046A-333F-4B2A-9815-73B60326162D" ); // Prayer Request Detail
            DeleteBlockType( "DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22" ); // Prayer Comment List
            DeleteBlockType( "4F3778DF-A25C-4E59-9242-B1D6813311E1" ); // Prayer Comment Detail

            // delete the pages
            DeletePage( "89C3DB4A-BAFD-45C8-88C6-45D8FEC48B48" ); // Prayer Request Detail
            DeletePage( "1A3437C8-D4CB-4329-A366-8D6A4CBF79BF" ); // Prayer Administration

            // Delete the "Prayer Comment" Note Type and default Categories
            Sql( @"
                -- Delete the NoteType but ONLY if it's not being used...
                IF NOT EXISTS(SELECT * FROM  [Note] WHERE [NoteTypeId] IN (SELECT [Id] FROM [NoteType] WHERE [Guid] = '0EBABD75-0890-4756-A9EE-62626282BB5D' ) )
                BEGIN
                    DELETE [NoteType] WHERE [Guid] = '0EBABD75-0890-4756-A9EE-62626282BB5D'
                END

                -- Delete the prayer categories if they are not being used...
                IF NOT EXISTS(SELECT * FROM  [PrayerRequest] WHERE [CategoryId] IN (SELECT [Id] FROM [Category] WHERE [Guid] = '4b2d88f5-6e45-4b4b-8776-11118c8e8269' ) )
                BEGIN
                    DELETE [Category] WHERE [Guid] = '4b2d88f5-6e45-4b4b-8776-11118c8e8269'
                END

                -- Delete the prayer categories if they are not being used...
                IF NOT EXISTS(SELECT * FROM  [PrayerRequest] WHERE [CategoryId] IN (SELECT [Id] FROM [Category] WHERE [Guid] = '5a94e584-35f0-4214-91f1-d72531cc6325' ) )
                BEGIN
                    DELETE [Category] WHERE [Guid] = '5a94e584-35f0-4214-91f1-d72531cc6325'
                END
                " );
        }
    }
}
