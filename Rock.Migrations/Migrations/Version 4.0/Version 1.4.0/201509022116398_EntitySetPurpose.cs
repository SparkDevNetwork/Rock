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
    public partial class EntitySetPurpose : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // MP: EntitySet Purpose
            AddColumn("dbo.EntitySet", "EntitySetPurposeValueId", c => c.Int());
            AddColumn("dbo.EntitySet", "Note", c => c.String());
            CreateIndex("dbo.EntitySet", "EntitySetPurposeValueId");
            AddForeignKey("dbo.EntitySet", "EntitySetPurposeValueId", "dbo.DefinedValue", "Id");

            RockMigrationHelper.AddDefinedType( "Global", "Entity Set Purpose", "Allows a developer to specify that an EntitySet has a specific purpose", "618BBF3F-794F-4FF9-9615-9211CDBAF723", @"" );
            RockMigrationHelper.AddDefinedValue( "618BBF3F-794F-4FF9-9615-9211CDBAF723", "Person Merge Request", "An EntitySet that contains a list a person records that have been requested to be merged", "214EB26F-5493-4540-B2EF-F0887C8FBB9E", true );

            // MP: Fix description of Background check dataview
            Sql( @"UPDATE DataView
SET [Description] = 'Returns people that have been background checked within the last three years'
WHERE [Guid] = 'AED692A5-4BB0-40FA-8C62-7948FAB894C5'
    AND [Description] = 'Returns people that have been background checked within the last two years'" );

            // NA: PageMenu new block setting
            // New IncludePageList block setting for PageMenu
            RockMigrationHelper.AddBlockTypeAttribute( "CACB9D1A-A820-4587-986A-D66A69EE9948", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Include Page List", "IncludePageList", "", "List of pages to include in the Lava. Any ~/ will be resolved by Rock. Enable debug for assistance. Example 'Give Now' with '~/page/186' or 'Me' with '~/MyAccount'.", 0, @"", "0A49DABE-42EE-40E5-9E06-0E6530944865" );

            // MP: Add Age Requirement to Background Check dataviews
            // make sure Rock.Reporting.DataSelect.Person.AgeSelect has a known Guid
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataFilter.Person.AgeFilter", "Age Filter", "Rock.Reporting.DataFilter.Person.AgeFilter, Rock, Version=1.3.4.0, Culture=neutral, PublicKeyToken=null", false, true, "4911C63D-71BB-4686-AAA3-D66EA41DA465" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Background check about to expire
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '526B31CC-2B30-4D87-95FB-0D86AB5D99A1') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'F924B369-FF1B-4254-AEB0-48BF89647205'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'256|18|,','526B31CC-2B30-4D87-95FB-0D86AB5D99A1')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'A0C00B93-C656-44E3-B64A-28036D9B9E5E') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')
    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'256|18|,','A0C00B93-C656-44E3-B64A-28036D9B9E5E')
END
" );
            // JE: Remove time off of Service Bulletin Content Channels
            Sql( @"  UPDATE [ContentChannelType]
  SET [IncludeTime] = 0
  WHERE [Guid] = '206CFC34-1C86-46F5-A1EA-6D71B25A8D33'" );

            // DT: Update account [IsPublic] field
            Sql( @"UPDATE [FinancialAccount] SET [IsPublic] = 1 
WHERE ( [IsPublic] IS NULL AND [PublicName] IS NOT NULL AND [PublicName] <> '' )" );

            // DT: Update exception subject
            Sql( @"UPDATE [SystemEmail] SET [Subject] = 'Rock Exception Notification {% if Person %}[{{Person.FullName}}]{% endif %}' 
WHERE [Guid] = '75cb0a4a-b1c5-4958-adeb-8621bd231520' and [Subject] = 'Rock Exception Notification'
" );

            // MP: Fix FinancialTransactionImage wrong BinaryFileType
            // MP: Fix Business Address not getting set as IsMailingLocation = true
            // JE: Updated Following Events
            // JE: Updated Following Suggestions
            Sql( MigrationSQL._201509022116398_EntitySetPurpose );

            // MP: Update Statement Generator Stored Procs
            Sql( MigrationSQL._201509022116398_EntitySetPurpose_ufnCrm_GetFamilyTitle );
            Sql( MigrationSQL._201509022116398_EntitySetPurpose_spFinance_ContributionStatementQuery );

            // DT: Payment Accounts Page
            RockMigrationHelper.AddPage( "8BB303AF-743C-49DC-A7FF-CC1236B4B1D9", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Payment Accounts", "", "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Saved Account List", "List of a person's saved accounts that can be used to delete an account.", "~/Blocks/Finance/SavedAccountList.ascx", "Finance", "CE9F1E41-33E6-4FED-AA08-BD9DCA061498" );
            RockMigrationHelper.AddBlock( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", "", "CE9F1E41-33E6-4FED-AA08-BD9DCA061498", "Saved Account List", "Main", "", "", 0, "4DA02EC9-75EA-4031-AC1F-79805638E4FD" );
            RockMigrationHelper.DeleteSecurityAuthForPage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520" );
            RockMigrationHelper.AddSecurityAuthForPage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", 0, "Administrate", true, "1918E74F-C00D-4DDD-94C4-2E7209CE12C3", 0, "4ACA28DA-6FB7-47C4-8593-3535A555AF18" );
            RockMigrationHelper.AddSecurityAuthForPage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", 0, "Edit", true, "1918E74F-C00D-4DDD-94C4-2E7209CE12C3", 0, "91413EB3-8679-4744-83AC-122BCDE1DEA0" );
            RockMigrationHelper.AddSecurityAuthForPage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", 0, "View", true, null, 2, "323FCB2D-C0A6-42FE-9E30-9FCF712C1D74" );
            RockMigrationHelper.AddSecurityAuthForPage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520", 1, "View", false, null, 1, "5F98F43B-9B69-42D7-BE79-15D0081404C9" );

            // MP: Update Known FieldTypes
            RockMigrationHelper.UpdateFieldType( "Connection Activity Type", "", "Rock", "Rock.Field.Types.ConnectionActivityTypeFieldType", "39356C8F-B69E-4744-906C-0A182671B9F8" );
            RockMigrationHelper.UpdateFieldType( "Connection Opportunity", "", "Rock", "Rock.Field.Types.ConnectionOpportunityFieldType", "B188B729-FE6D-498B-8871-65AB8FD1E11E" );
            RockMigrationHelper.UpdateFieldType( "Connection Request", "", "Rock", "Rock.Field.Types.ConnectionRequestFieldType", "73A4B6C6-502B-4E5B-BAA0-A85B7CCEC544" );
            RockMigrationHelper.UpdateFieldType( "Connection State", "", "Rock", "Rock.Field.Types.ConnectionStateFieldType", "B0FB7AFB-B43C-4E2B-8502-293F07BB465A" );
            RockMigrationHelper.UpdateFieldType( "Connection Status", "", "Rock", "Rock.Field.Types.ConnectionStatusFieldType", "EC381D5D-729F-4581-A8F7-8C1FCE44DA98" );
            RockMigrationHelper.UpdateFieldType( "Connection Type", "", "Rock", "Rock.Field.Types.ConnectionTypeFieldType", "50DA6F25-E81E-46E8-A773-4B479B4FB9E6" );
            RockMigrationHelper.UpdateFieldType( "Connection Types", "", "Rock", "Rock.Field.Types.ConnectionTypesFieldType", "E4E72958-4604-498F-956B-BA095976A60B" );
            
            // DT: MyConnections Type Filter
            RockMigrationHelper.AddBlockTypeAttribute( "3F69E04F-F966-4CAE-B89D-F97DFEF6407A", "E4E72958-4604-498F-956B-BA095976A60B", "Connection Types", "ConnectionTypes", "", "Optional list of connection types to limit the display to (All will be displayed by default).", 2, @"", "1A322AE7-F018-417C-94BC-A77A2C079495" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // DT: MyConnections Type Filter
            RockMigrationHelper.DeleteAttribute( "1A322AE7-F018-417C-94BC-A77A2C079495" );
            
            // DT: Payment Accounts Page
            RockMigrationHelper.DeleteBlock( "4DA02EC9-75EA-4031-AC1F-79805638E4FD" );
            RockMigrationHelper.DeleteBlockType( "CE9F1E41-33E6-4FED-AA08-BD9DCA061498" ); // Saved Account List
            RockMigrationHelper.DeletePage( "D0ADBAF0-3DC9-4123-BEE3-4E8E34BCD520" ); //  Page: Payment Accounts, Layout: FullWidth, Site: External Website

            // DT/MP: Payment Accounts Page
            RockMigrationHelper.DeleteSecurityAuth( "4ACA28DA-6FB7-47C4-8593-3535A555AF18" );
            RockMigrationHelper.DeleteSecurityAuth( "91413EB3-8679-4744-83AC-122BCDE1DEA0" );
            RockMigrationHelper.DeleteSecurityAuth( "323FCB2D-C0A6-42FE-9E30-9FCF712C1D74" );
            RockMigrationHelper.DeleteSecurityAuth( "5F98F43B-9B69-42D7-BE79-15D0081404C9" );

            // MP: Add Age Requirement to Background Check dataviews
            // Delete DataViewFilter for DataView: Background check about to expire
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '526B31CC-2B30-4D87-95FB-0D86AB5D99A1'" );
            // Delete DataViewFilter for DataView: Background check is still valid
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'A0C00B93-C656-44E3-B64A-28036D9B9E5E'" );
            
            // NA: PageMenu new block setting
            // Remove the IncludePageList block setting
            RockMigrationHelper.DeleteAttribute( "0A49DABE-42EE-40E5-9E06-0E6530944865" );
            
            // MP: EntitySet Purpose
            DropForeignKey("dbo.EntitySet", "EntitySetPurposeValueId", "dbo.DefinedValue");
            DropIndex("dbo.EntitySet", new[] { "EntitySetPurposeValueId" });
            DropColumn("dbo.EntitySet", "Note");
            DropColumn("dbo.EntitySet", "EntitySetPurposeValueId");

            RockMigrationHelper.DeleteDefinedValue( "214EB26F-5493-4540-B2EF-F0887C8FBB9E" ); // Person Merge Request
            RockMigrationHelper.DeleteDefinedType( "618BBF3F-794F-4FF9-9615-9211CDBAF723" ); // Entity Set Purpose
        }
    }
}
