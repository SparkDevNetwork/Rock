// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class FinancialHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Financial", "", "", "E41FC407-B60E-4B85-954D-D27F0762114B", 0 );
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Batch", "fa fa-archive", "", "AF6A8CFF-F24F-4AA8-B126-94B6903961C0", 0, "E41FC407-B60E-4B85-954D-D27F0762114B" );
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Transaction", "fa fa-credit-card", "", "477EE3BE-C68F-48BD-B218-FAFC99AF56B3", 1, "E41FC407-B60E-4B85-954D-D27F0762114B" );

            Sql( @"
    DECLARE @UrlMaskAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C405062-72BB-4362-9738-90C9ED5ACDDE' )
    DECLARE @TransactionCategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '477EE3BE-C68F-48BD-B218-FAFC99AF56B3' )
    DECLARE @TransactionDetailPageId int = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '97716641-D003-4663-9EA2-D9BB94E7955B' )
    IF @UrlMaskAttributeId IS NOT NULL AND @TransactionCategoryId IS NOT NULL AND @TransactionDetailPageId IS NOT NULL
    BEGIN

        DELETE [AttributeValue]
        WHERE [AttributeId] = @UrlMaskAttributeId
        AND [EntityId] = @TransactionCategoryId

        INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid] )
        VALUES( 0, @UrlMaskAttributeId, @TransactionCategoryId, '~/page/' + CAST(@TransactionDetailPageId AS varchar) + '?batchId={1}&transactionId={0}', NEWID())
    END
" );

            RockMigrationHelper.AddPage( "606BDA31-A8FE-473A-B3F8-A00ECF7E06EC", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Audit Log", "", "CBE0C5ED-744E-4392-A9D4-0DC57AF11D33", "fa fa-file-text-o" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "History Log", "Block for displaying the history of changes to a particular entity.", "~/Blocks/Core/HistoryLog.ascx", "Core", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" );

            // Add Block to Page: Audit Log, Site: Rock RMS
            RockMigrationHelper.AddBlock( "CBE0C5ED-744E-4392-A9D4-0DC57AF11D33", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 0, "39F5EA5A-6F78-4A4F-ABFE-719DD75B445A" );

            // Attrib for BlockType: Batch Detail:Audit Page
            RockMigrationHelper.AddBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Audit Page", "AuditPage", "", "Page used to display the history of changes to a batch.", 0, @"", "66E7C23A-797D-4945-931F-DC7671C79AC1" );

            // Attrib for BlockType: History Log:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            
            // Attrib Value for Block:Batch Detail, Attribute:Audit Page Page: Financial Batch Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "E7C8C398-0E1D-4BCE-BC54-A02957228514", "66E7C23A-797D-4945-931F-DC7671C79AC1", @"cbe0c5ed-744e-4392-a9d4-0dc57af11d33" );
            
            // Attrib Value for Block:History Log, Attribute:Entity Type Page: Audit Log, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "39F5EA5A-6F78-4A4F-ABFE-719DD75B445A", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"bdd09c8e-2c52-4d08-9062-be7d52d190c2" );
            
            // Add/Update PageContext for Page:Audit Log, Entity: Rock.Model.FinancialBatch, Parameter: BatchId
            RockMigrationHelper.UpdatePageContext( "CBE0C5ED-744E-4392-A9D4-0DC57AF11D33", "Rock.Model.FinancialBatch", "BatchId", "307639F5-824A-453C-8650-B393DEC506F1" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: History Log:Entity Type
            RockMigrationHelper.DeleteAttribute( "8FB690EC-5299-46C5-8695-AAD23168E6E1" );
            // Attrib for BlockType: Batch Detail:Audit Page
            RockMigrationHelper.DeleteAttribute( "66E7C23A-797D-4945-931F-DC7671C79AC1" );
            // Remove Block: History Log, from Page: Audit Log, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "39F5EA5A-6F78-4A4F-ABFE-719DD75B445A" );
            RockMigrationHelper.DeleteBlockType( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" ); // History Log
            RockMigrationHelper.DeletePage( "CBE0C5ED-744E-4392-A9D4-0DC57AF11D33" ); //  Page: Audit Log, Layout: Full Width, Site: Rock RMS

            // Delete PageContext for Page:Audit Log, Entity: Rock.Model.FinancialBatch, Parameter: BatchId
            RockMigrationHelper.DeletePageContext( "307639F5-824A-453C-8650-B393DEC506F1" );


            RockMigrationHelper.DeleteCategory( "477EE3BE-C68F-48BD-B218-FAFC99AF56B3" );
            RockMigrationHelper.DeleteCategory( "AF6A8CFF-F24F-4AA8-B126-94B6903961C0" );
            RockMigrationHelper.DeleteCategory( "E41FC407-B60E-4B85-954D-D27F0762114B" );
        }
    }
}
