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
    public partial class V6Rollup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // AF: Update Universal Content Channel Type to 'Universal Single Date Channel Type' and add Universal Date Range Content Channel Type
            Sql( @"INSERT INTO [ContentChannelType]
( [IsSystem], [Name], [DateRangeType], [Guid], [DisablePriority] )
VALUES
( 1, 'Universal Date Range Channel Type', 2, 'a2f62edc-576a-45f4-9df1-f7867b21cdee', 0 )

UPDATE[ContentChannelType]
SET[Name] = 'Universal Single Date Channel Type'
WHERE[Guid] = '0A69DA05-F671-454F-A25D-99A01E10ADB8'" );

            
            // JE: Tender Type -> Currency Type and Non-Cash
            Sql( @"IF NOT EXISTS( SELECT * FROM[DefinedValue] dv INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId] WHERE dv.[Value] = 'Non-Cash' AND dt.[Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81' )
  BEGIN
  DECLARE @CurrencyTypeDefinedTypeId int = (SELECT TOP 1[Id] FROM[DefinedType] WHERE[Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81' )
  INSERT INTO[DefinedValue]
  ([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid])
  VALUES(0, @CurrencyTypeDefinedTypeId, 99, 'Non-Cash', 'Used to track non-cash transactions.', '7950FF66-80EE-E8AB-4A77-4A13EDEB7513')
END");

            Sql( @"UPDATE [DefinedType] 
	            SET[Name] = 'Currency Type'
                WHERE[Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81'");

            // AF: Change the cache on the TransactionLink HTML Content block on the Profile Page Contributions tab to 0 if it was previously set to 3600 (Fixes #1693)
            Sql( @"DECLARE @BlockId int;SELECT TOP 1  @BlockId = [Id]  FROM [dbo].[Block]  WHERE [Name] = 'Transaction Links';DECLARE @BlockTypeId int;SELECT TOP 1 @BlockTypeId = [Id]  FROM [dbo].[BlockType]  WHERE [Path] = '~/Blocks/Cms/HtmlContentDetail.ascx';DECLARE @AttributeId int;SELECT TOP 1 @AttributeId = [Id] FROM [Attribute] WHERE EntityTypeQualifierColumn = 'BlockTypeId' and EntityTypeQualifierValue = @BlockTypeId and [Key] = 'CacheDuration';UPDATE[dbo].[AttributeValue]  SET [Value] = 0  WHERE AttributeId = @AttributeId and EntityId = @BlockId;" );

            // TC: Remove Connection/ RecordStatus attributes from PublicProfileEdit
            RockMigrationHelper.DeleteAttribute( "F15444C3-41D1-46D0-AE87-7472B4749481" );
            RockMigrationHelper.DeleteAttribute( "3D1DA9C7-8902-4C39-839F-DCB426EB3108" );

            // TC: Add Public Profile Edit to the MyAccount page
            RockMigrationHelper.DeleteBlock( "54DA7430-6884-4C2E-9A28-123DA211E02E" );

            RockMigrationHelper.AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "Public Profile Edit", "Main", "", "", 0, "3F68A42E-B609-40DD-822A-8B2AF2CB10E6" );
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "7DC6314E-F510-491F-95E8-7DE7A1CB198B", @"True" ); // Show Family Members
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "B2F06218-829A-40DE-9922-87752C9CACCF", @"8c52e53c-2a66-435a-ae6e-5ee307d9a0dc" ); // Address Type
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "C359107A-B83A-4DEE-9CC4-742844BB2687", @"407e7e45-7b2e-4fcd-9605-ecb1339f2453,aa8732fb-2cea-4c76-8d6d-6aaa2c6a4303,2cc66d5a-f61c-4b74-9af9-590a9847c13c" ); // Phone Numbers
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "3DBDB8AC-D051-4459-8392-2026DACE0A25", @"e1f9de5a-cf99-4af5-bee6-efc04f6de57a" ); // Workflow Launch Page
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "714875EF-F019-44A4-B56F-B4F425D4F7BD", @"" ); // Family Attributes
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "7C622140-54D4-4D08-9B97-B5B1D28D7CB0", @"" ); // Person Attributes (adults)
            RockMigrationHelper.AddBlockAttributeValue( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6", "B601F321-D764-430E-BE19-A4E44C52F11F", @"4abf0bf2-49ba-4363-9d85-ac48a0f7e92a" ); // Person Attributes (children)

            // Page: Workflow Entry
            RockMigrationHelper.AddPage( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Workflow Entry", "", "E1F9DE5A-CF99-4AF5-BEE6-EFC04F6DE57A", "" ); // Site:External Website
            RockMigrationHelper.AddBlock( "E1F9DE5A-CF99-4AF5-BEE6-EFC04F6DE57A", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "59A5CF40-54DA-4B5A-8783-715FC462589A" );
            RockMigrationHelper.AddBlockAttributeValue( "59A5CF40-54DA-4B5A-8783-715FC462589A", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"f5af8224-44dc-4918-aab7-c7c9a5a6338d" ); // Workflow Type

            RockMigrationHelper.AddSecurityAuthForPage( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", 0, Rock.Security.Authorization.VIEW, true, null, Rock.Model.SpecialRole.AllAuthenticatedUsers.ConvertToInt(), "8513F1A1-0C2C-492F-90FE-2C352974BBEA" );
            RockMigrationHelper.AddSecurityAuthForPage( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", 1, Rock.Security.Authorization.VIEW, false, null, Rock.Model.SpecialRole.AllUsers.ConvertToInt(), "0BE9A74C-9EE9-4F09-961E-F3F55FEB8CD9" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // AF: Update Universal Content Channel Type to 'Universal Single Date Channel Type' and add Universal Date Range Content Channel Type
            Sql( @"DELETE FROM [ContentChannelType]
WHERE [Guid] = 'a2f62edc-576a-45f4-9df1-f7867b21cdee'
UPDATE [ContentChannelType]
SET [Name]='Universal Channel Type'
WHERE [Guid] = '0A69DA05-F671-454F-A25D-99A01E10ADB8'" );

            // TC: Remove Connection/RecordStatus attributes from PublicProfileEdit
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 1, @"368DD475-242C-49C4-A42C-7278BE690CC2", "F15444C3-41D1-46D0-AE87-7472B4749481" );
            RockMigrationHelper.AddBlockTypeAttribute( "841D1670-8BFD-4913-8409-FB47EB7A2AB9", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 2, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "3D1DA9C7-8902-4C39-839F-DCB426EB3108" );

            // TC: Add Public Profile Edit to the MyAccount page
            RockMigrationHelper.DeleteSecurityAuth( "0BE9A74C-9EE9-4F09-961E-F3F55FEB8CD9" );
            RockMigrationHelper.DeleteSecurityAuth( "8513F1A1-0C2C-492F-90FE-2C352974BBEA" );

            RockMigrationHelper.DeleteBlock( "59A5CF40-54DA-4B5A-8783-715FC462589A" );
            RockMigrationHelper.DeletePage( "E1F9DE5A-CF99-4AF5-BEE6-EFC04F6DE57A" ); //  Page: Workflow Entry

            RockMigrationHelper.DeleteBlock( "3F68A42E-B609-40DD-822A-8B2AF2CB10E6" );
            RockMigrationHelper.AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1", "My Account", "Main", "", "", 0, "54DA7430-6884-4C2E-9A28-123DA211E02E" );
            RockMigrationHelper.AddBlockAttributeValue( "54DA7430-6884-4C2E-9A28-123DA211E02E", "534190B5-3EC9-4497-B394-31122154FCE5", @"4a4655d1-bdd9-4ece-a3f6-b655f0bdf9f5" ); // Detail Page
        }
    }
}
