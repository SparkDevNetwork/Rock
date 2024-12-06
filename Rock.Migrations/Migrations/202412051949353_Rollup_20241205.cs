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
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20241205 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //DropIndex("dbo.AnalyticsFactFinancialTransaction", new[] { "TransactionTypeValueId", "TransactionDateTime", "AuthorizedPersonAliasId", "AccountId" });
            //DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionTypeValueId", "TransactionDateTime", "AuthorizedPersonAliasId", "AccountId" });
            //CreateIndex("dbo.AnalyticsFactFinancialTransaction", "AccountId");
            //CreateIndex("dbo.AnalyticsSourceFinancialTransaction", "AccountId");

            SwapBlocksUp();
            CleanupNoteApprovalStatusUp();
            UpdateCategorizedFieldTypeNameUp();
            UpdateAdaptiveMessageListPageUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "AccountId" });
            //DropIndex("dbo.AnalyticsFactFinancialTransaction", new[] { "AccountId" });
            //CreateIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionTypeValueId", "TransactionDateTime", "AuthorizedPersonAliasId", "AccountId" });
            //CreateIndex("dbo.AnalyticsFactFinancialTransaction", new[] { "TransactionTypeValueId", "TransactionDateTime", "AuthorizedPersonAliasId", "AccountId" });

            UpdateCategorizedFieldTypeNameDown();
        }

        #region JC: Swap blocks for v1.17.0.33

        private void SwapBlocksUp()
        {
            RockMigrationHelper.ReplaceWebformsWithObsidianBlockMigration(
                    "Swap Block Types - 1.17.0.33",
                    blockTypeReplacements: new Dictionary<string, string> {
                        { "B5EB66A1-7391-49D5-B613-5ED804A31E7B", "07BCB48D-746E-4364-80F3-C5BEB9075FC6" }, // Group Member Schedule Template Detail ( Group Scheduling )
                        { "D930E08B-ACD3-4ADD-9FAC-3B61C021D0F7", "2B8A5A3D-BF9D-4319-B7E5-06757FA44759" }, // Group Member Schedule Template List ( Group Scheduling )
                    },
                    migrationStrategy: "Swap",
                    jobGuid: SystemGuid.ServiceJob.DATA_MIGRATIONS_170_SWAP_OBSIDIAN_BLOCKS,
                    null
                  );
        }

        #endregion

        #region SMC: Cleanup Note ApprovalStatus

        private void CleanupNoteApprovalStatusUp()
        {
            Sql( @"UPDATE [Note] SET [ApprovalStatus] = 1 WHERE [NoteTypeId] IN (SELECT [Id] FROM [NoteType] WHERE [RequiresApprovals] = 0);" );
        }

        #endregion

        #region KA: Migration to Update CategorizedFieldType Name

        private void UpdateCategorizedFieldTypeNameUp()
        {
            RockMigrationHelper.UpdateFieldType( "Cat. Def. Value", "", "Rock", "Rock.Field.Types.CategorizedDefinedValueFieldType", "3217C31F-85B6-4E0D-B6BE-2ADB0D28588D" );
        }

        private void UpdateCategorizedFieldTypeNameDown()
        {
            RockMigrationHelper.UpdateFieldType( "Defined Value (Categorized)", "", "Rock", "Rock.Field.Types.CategorizedDefinedValueFieldType", "3217C31F-85B6-4E0D-B6BE-2ADB0D28588D" );
        }

        #endregion

        #region SK: Update Adaptive Message List Page

        private void UpdateAdaptiveMessageListPageUp()
        {
            Sql( @"
      DECLARE @LayoutId INT = (SELECT Id FROM [Layout] WHERE [Guid]='0CB60906-6B74-44FD-AB25-026050EF70EB')
      UPDATE 
          [Page]
      SET [LayoutId] = @LayoutId
      WHERE [Guid] = '73112D38-E051-4452-AEF9-E473EEDD0BCB'" );

            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ADE003C7-649B-466A-872B-B8AC952E7841".AsGuid(), "Category Tree View", "Sidebar1", @"", @"", 0, "9912C605-6699-4484-B88B-469171F2F693" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "515DC5C2-4FBD-4EEA-9D8E-A807409DEFDE".AsGuid(), "Category Detail", "Main", @"", @"", 0, "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6" );
            RockMigrationHelper.AddBlock( true, "73112D38-E051-4452-AEF9-E473EEDD0BCB".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "A81FE4E0-DF9F-4978-83A7-EB5459F37938".AsGuid(), "Adaptive Message Detail", "Main", @"", @"", 2, "859B5FE9-9068-40EC-B7AD-78598BEDC6AA" );
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '859B5FE9-9068-40EC-B7AD-78598BEDC6AA'" );
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '2DBFA85E-BA20-4FF2-8372-80688C8B9CD1'" );
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6'" );
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '9912C605-6699-4484-B88B-469171F2F693'" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", @"09053C7C-9374-4489-8A7B-71F02E3E7D89" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "831403EB-262E-4BC5-8B5E-F16153493BF5", @"8B53F981-6FF6-4657-9CD5-01E36EB0DF51" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "F9E1AD87-CA8A-49DF-84AC-8F18895FCB87", @"bec30e90-0434-43c4-b839-09e11775e497" );
            RockMigrationHelper.AddBlockAttributeValue( "2DBFA85E-BA20-4FF2-8372-80688C8B9CD1", "0AC7BE6D-22FB-4A4F-9188-C1FAB8AC1EC1", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "845AC4E4-ACD1-40CC-96F6-8D22136C30CC", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fa fa-list-ol" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AA057D3E-00CC-42BD-9998-600873356EDB", @"AdaptiveMessageCategoryId" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"73112d38-e051-4452-aef9-e473eedd0bcb" );
            RockMigrationHelper.AddBlockAttributeValue( "9912C605-6699-4484-B88B-469171F2F693", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"73112d38-e051-4452-aef9-e473eedd0bcb" );
            RockMigrationHelper.AddBlockAttributeValue( "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6", "3C6E056B-5087-4E02-B9FD-853B658E3C85", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
            RockMigrationHelper.AddBlockAttributeValue( "F10D62AB-46D0-4C01-9A34-FAE2BE8CFDB6", "3C6E056B-5087-4E02-B9FD-853B658E3C85", @"d47bda25-03a3-46ee-a0a6-f8b220e39e4a" );
        }

        #endregion
    }
}
