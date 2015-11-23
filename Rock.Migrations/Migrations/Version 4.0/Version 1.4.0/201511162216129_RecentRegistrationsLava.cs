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
    public partial class RecentRegistrationsLava : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // catchups
            RockMigrationHelper.UpdateBlockType( "Schedule Context Setter", "Block that can be used to set the default schedule context for the site.", "~/Blocks/Core/ScheduleContextSetter.ascx", "Core", "6553821F-9667-4576-924F-DAF1BB3F3223" );
            
            // MP: Add Registration List Lava
            RockMigrationHelper.UpdateBlockType( "Registration List Lava", "List recent registrations using a Lava template.", "~/Blocks/Event/RegistrationListLava.ascx", "Event", "92E4BFE8-DF80-49D7-819D-417E579E282D" );
            
            // Add Block to Page: My Account, Site: External Website
            RockMigrationHelper.AddBlock( "C0854F84-2E8B-479C-A3FB-6B47BE89B795", "", "92E4BFE8-DF80-49D7-819D-417E579E282D", "Recent Registrations", "Sidebar1", "", "", 2, "E5596525-B176-4753-A337-25F1F9B83FCE" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '87068AAB-16A7-42CC-8A31-5A957D6C4DD5'" );  // Page: My Account,  Zone: Sidebar1,  Block: Actions
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8'" );  // Page: My Account,  Zone: Sidebar1,  Block: Group List Personalized Lava
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'E5596525-B176-4753-A337-25F1F9B83FCE'" );  // Page: My Account,  Zone: Sidebar1,  Block: Recent Registrations
            // Attrib for BlockType: Registration List Lava:Limit to registrations where money is still owed
            RockMigrationHelper.AddBlockTypeAttribute( "92E4BFE8-DF80-49D7-819D-417E579E282D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to registrations where money is still owed", "LimitToOwed", "", "", 8, @"True", "368D86B0-E922-4742-8EAD-87A5D37947B4" );

            // Attrib for BlockType: Registration List Lava:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute( "92E4BFE8-DF80-49D7-819D-417E579E282D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Show merge data to help you see what's available to you.", 9, @"False", "B81B869E-042A-4AF9-B572-5D516C1E9BA2" );

            // Attrib for BlockType: Registration List Lava:Lava Template
            RockMigrationHelper.AddBlockTypeAttribute( "92E4BFE8-DF80-49D7-819D-417E579E282D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display content", 2, @"{% include '~~/Assets/Lava/RegistrationListSidebar.lava' %}", "57BAF6E9-AFE8-40A6-BEDF-7AFBAC7F02D2" );

            // Attrib for BlockType: Registration List Lava:Max Results
            RockMigrationHelper.AddBlockTypeAttribute( "92E4BFE8-DF80-49D7-819D-417E579E282D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Results", "MaxResults", "", "The maximum number of results to display.", 3, @"100", "08D0269D-4D47-4C19-A1A6-DA6300B35686" );

            // Attrib for BlockType: Registration List Lava:Date Range
            RockMigrationHelper.AddBlockTypeAttribute( "92E4BFE8-DF80-49D7-819D-417E579E282D", "55810BC5-45EA-4044-B783-0CCE0A445C6F", "Date Range", "DateRange", "", "Date range to limit by.", 7, @"", "B52F866D-B6E5-49DF-B1AF-445D64C95C29" );

            // Attrib Value for Block:Recent Registrations, Attribute:Limit to registrations where money is still owed Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E5596525-B176-4753-A337-25F1F9B83FCE", "368D86B0-E922-4742-8EAD-87A5D37947B4", @"True" );

            // Attrib Value for Block:Recent Registrations, Attribute:Enable Debug Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E5596525-B176-4753-A337-25F1F9B83FCE", "B81B869E-042A-4AF9-B572-5D516C1E9BA2", @"False" );

            // Attrib Value for Block:Recent Registrations, Attribute:Lava Template Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E5596525-B176-4753-A337-25F1F9B83FCE", "57BAF6E9-AFE8-40A6-BEDF-7AFBAC7F02D2", @"{% include '~~/Assets/Lava/RegistrationListSidebar.lava' %}" );

            // Attrib Value for Block:Recent Registrations, Attribute:Max Results Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E5596525-B176-4753-A337-25F1F9B83FCE", "08D0269D-4D47-4C19-A1A6-DA6300B35686", @"100" );

            // Attrib Value for Block:Recent Registrations, Attribute:Date Range Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "E5596525-B176-4753-A337-25F1F9B83FCE", "B52F866D-B6E5-49DF-B1AF-445D64C95C29", @"All||||" );


            // MP Workflow Indexes
            Sql( @"  
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_CompletedDateTimeActivatedDateTIme' AND object_id = OBJECT_ID('WorkflowActivity'))
BEGIN
  CREATE NONCLUSTERED INDEX [IX_CompletedDateTimeActivatedDateTIme] ON [dbo].[WorkflowActivity] ([CompletedDateTime],[ActivatedDateTime])
END

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_CompletedDateTIme' AND object_id = OBJECT_ID('WorkflowAction'))
BEGIN
  CREATE NONCLUSTERED INDEX [IX_CompletedDateTIme] ON [dbo].[WorkflowAction] ([CompletedDateTime])
END
" );
            // JE: Fix Following Emails for iOS
            Sql(@"
  UPDATE [FollowingEventType]
	SET [EntityNotificationFormatLava] = REPLACE([EntityNotificationFormatLava], 'padding-bottom: 12px; padding-right: 12px;', 'padding-bottom: 12px; padding-right: 12px; min-width: 87px;')

  UPDATE [FollowingEventType]
	SET [EntityNotificationFormatLava] = REPLACE([EntityNotificationFormatLava], 'valign=""top"" style=''padding-bottom: 12px;', 'valign=""top"" style=''padding-bottom: 12px; min-width: 300px;')

 UPDATE [FollowingSuggestionType]
	SET [EntityNotificationFormatLava] = REPLACE([EntityNotificationFormatLava], 'padding-bottom: 12px; padding-right: 12px;', 'padding-bottom: 12px; padding-right: 12px; min-width: 87px;')

  UPDATE [FollowingSuggestionType]
	SET [EntityNotificationFormatLava] = REPLACE([EntityNotificationFormatLava], 'valign=""top"" style=''padding-bottom: 12px;', 'valign=""top:"" style=''padding-bottom: 12px; min-width: 300px;')
");

            // JE: Merge Template Updates
            Sql( MigrationSQL._201511162216129_RecentRegistrationsLava_MergeTemplateUpdates );

            // MP: Fix registration reminder email template
            Sql( @"
UPDATE RegistrationTemplate
SET ReminderEmailTemplate = REPLACE(ReminderEmailTemplate, 'using our <a href=""{{ externalSite }}/Registration?RegistrationInstanceId={{ RegistrationInstance.Id }}"">', 'using our <a href=""{{ externalSite }}/Registration?RegistrationId={{ Registration.Id }}"">')
WHERE [ReminderEmailTemplate] LIKE '%using our <a href=""{{ externalSite }}/Registration?RegistrationInstanceId={{ RegistrationInstance.Id }}"">%'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // MP: Registration List Lava
            // Attrib for BlockType: Registration List Lava:Date Range
            RockMigrationHelper.DeleteAttribute( "B52F866D-B6E5-49DF-B1AF-445D64C95C29" );
            // Attrib for BlockType: Registration List Lava:Max Results
            RockMigrationHelper.DeleteAttribute( "08D0269D-4D47-4C19-A1A6-DA6300B35686" );
            // Attrib for BlockType: Registration List Lava:Lava Template
            RockMigrationHelper.DeleteAttribute( "57BAF6E9-AFE8-40A6-BEDF-7AFBAC7F02D2" );
            // Attrib for BlockType: Registration List Lava:Enable Debug
            RockMigrationHelper.DeleteAttribute( "B81B869E-042A-4AF9-B572-5D516C1E9BA2" );
            // Attrib for BlockType: Registration List Lava:Limit to registrations where money is still owed
            RockMigrationHelper.DeleteAttribute( "368D86B0-E922-4742-8EAD-87A5D37947B4" );
            // Remove Block: Recent Registrations, from Page: My Account, Site: External Website
            RockMigrationHelper.DeleteBlock( "E5596525-B176-4753-A337-25F1F9B83FCE" );
            RockMigrationHelper.DeleteBlockType( "92E4BFE8-DF80-49D7-819D-417E579E282D" ); // Registration List Lava
        }
    }
}
