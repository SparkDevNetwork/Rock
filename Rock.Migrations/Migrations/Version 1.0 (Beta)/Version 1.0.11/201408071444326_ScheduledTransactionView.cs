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
    public partial class ScheduledTransactionView : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Page: Scheduled Transaction View
            RockMigrationHelper.AddPage( "F23C5BF7-4F52-448F-8445-6BAEEE3030AB", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Scheduled Transaction", "", "996F5541-D2E1-47E4-8078-80A388203CEC", "fa fa-credit-card" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Scheduled Transaction View", "View an existing scheduled transaction.", "~/Blocks/Finance/ScheduledTransactionView.ascx", "Finance", "85753750-7465-4241-97A6-E5F27EA38C8B" );
            RockMigrationHelper.AddBlock( "996F5541-D2E1-47E4-8078-80A388203CEC", "", "85753750-7465-4241-97A6-E5F27EA38C8B", "Scheduled Transaction View", "Main", "", "", 0, "909E5FAE-F8B9-4D3D-BFDC-68DD4F9ECEF2" );
            RockMigrationHelper.AddBlock( "996F5541-D2E1-47E4-8078-80A388203CEC", "", "E04320BC-67C3-452D-9EF6-D74D8C177154", "Transaction List", "Main", "", "", 1, "E8A2E317-38AD-4FF0-A75B-C06D3087FCC4" );
            RockMigrationHelper.AddBlockTypeAttribute( "85753750-7465-4241-97A6-E5F27EA38C8B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Update Page", "UpdatePage", "", "The page used to update in existing scheduled transaction.", 0, @"", "98FE689B-DCBC-4E29-9269-A96FE8066C50" );
            RockMigrationHelper.AddBlockAttributeValue( "909E5FAE-F8B9-4D3D-BFDC-68DD4F9ECEF2", "98FE689B-DCBC-4E29-9269-A96FE8066C50", @"f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" ); // Update Page
            RockMigrationHelper.AddBlockAttributeValue( "E8A2E317-38AD-4FF0-A75B-C06D3087FCC4", "C6D07A89-84C9-412A-A584-E37E59506566", @"b67e38cb-2ef1-43ea-863a-37daa1c7340f" ); // Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "E8A2E317-38AD-4FF0-A75B-C06D3087FCC4", "29A6C37A-EFB3-41CC-A522-9CEFAAEEA910", @"76824e8a-ccc4-4085-84d9-8af8c0807e20" ); // Entity Type
            RockMigrationHelper.AddBlockAttributeValue( "E8A2E317-38AD-4FF0-A75B-C06D3087FCC4", "A4E3B5C6-B386-45B5-A929-8FD9379BABBC", @"Processed Transactions" ); // Title

            // Add new BatchPage attribute to the transaction detail block
            RockMigrationHelper.AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Batch Detail Page", "BatchDetailPage", "", "Page used to view batch.", 0, @"", "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7" );
            RockMigrationHelper.AddBlockAttributeValue( "25F645D5-50B9-4DCC-951F-C780C49CD913", "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" ); // Batch Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" ); // Batch Detail Page

            // Add ViewPage attribute to Scheduled Transaction List block
            RockMigrationHelper.AddBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Page", "ViewPage", "", "", 0, @"", "47B99CD1-FB63-44D7-8586-45BDCDF51137" );
            RockMigrationHelper.AddBlockAttributeValue( "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "47B99CD1-FB63-44D7-8586-45BDCDF51137", @"996f5541-d2e1-47e4-8078-80a388203cec" ); // View Page
            RockMigrationHelper.AddBlockAttributeValue( "B33DF8C4-29B2-4DC5-B182-61FC255B01C0", "47B99CD1-FB63-44D7-8586-45BDCDF51137", @"996f5541-d2e1-47e4-8078-80a388203cec" ); // View Page

            // Update the scheduled transaction detail attribute on transaction detail to point to new view page
            RockMigrationHelper.AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"996f5541-d2e1-47e4-8078-80a388203cec" ); // Scheduled Transaction Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "25F645D5-50B9-4DCC-951F-C780C49CD913", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"996f5541-d2e1-47e4-8078-80a388203cec" ); // Scheduled Transaction Detail Page

            Sql( @"
    -- Update icon for another transaction detail page
    UPDATE [Page] SET [IconCssClass] = 'fa fa-credit-card' WHERE [Guid] = 'B67E38CB-2EF1-43EA-863A-37DAA1C7340F'

    -- Update block type name to ScheduledTransactionEdit instead of ScheduledTransactionDetail
    UPDATE [BlockType] SET
          [Path] = '~/Blocks/Finance/ScheduledTransactionEdit.ascx'
        , [Name] = 'Scheduled Transaction Edit'
    WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE'

    -- Change the page title of existing 'Scheduled Contributions' page
    UPDATE [Page] SET
          [InternalName] = 'Scheduled Transactions'
        , [PageTitle] = 'Scheduled Transactions'
        , [BrowserTitle] = 'Scheduled Transactions'
    WHERE [Guid] = 'F23C5BF7-4F52-448F-8445-6BAEEE3030AB'

    -- Move the Scheduled Transaction Edit page underneath the Scheduled Transaction View page and change name from 'Contribution Detail'
    DECLARE @ParentPageId int = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '996F5541-D2E1-47E4-8078-80A388203CEC')   
    UPDATE [Page] SET 
          [ParentPageId] = @ParentPageId
        , [InternalName] = 'Edit Scheduled Transaction'
        , [PageTitle] = 'Edit Scheduled Transaction'
        , [BrowserTitle] = 'Edit Scheduled Transaction'
    WHERE [Guid] = 'F1C3BBD3-EE91-4DDD-8880-1542EBCD8041'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Update block type name to ScheduledTransactionDetail instead of ScheduledTransactionEdit
            Sql( @"
    UPDATE [BlockType] SET
          [Path] = '~/Blocks/Finance/ScheduledTransactionDetail.ascx'
        , [Name] = 'Scheduled Transaction Detail'
    WHERE [Guid] = '5171C4E5-7698-453E-9CC8-088D362296DE'
" );
            RockMigrationHelper.DeleteAttribute( "47B99CD1-FB63-44D7-8586-45BDCDF51137" );
            RockMigrationHelper.DeleteAttribute( "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7" );
            RockMigrationHelper.DeleteAttribute( "98FE689B-DCBC-4E29-9269-A96FE8066C50" );
            RockMigrationHelper.DeleteBlock( "E8A2E317-38AD-4FF0-A75B-C06D3087FCC4" );
            RockMigrationHelper.DeleteBlock( "909E5FAE-F8B9-4D3D-BFDC-68DD4F9ECEF2" );
            RockMigrationHelper.DeleteBlockType( "85753750-7465-4241-97A6-E5F27EA38C8B" );
            RockMigrationHelper.DeletePage( "996F5541-D2E1-47E4-8078-80A388203CEC" ); //  Page: Scheduled Transaction
        }
    }
}
