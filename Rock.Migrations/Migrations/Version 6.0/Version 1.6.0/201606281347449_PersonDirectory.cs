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
    public partial class PersonDirectory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create [GroupAll] DataViewFilter for DataView: Member & Attendees
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for  will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','22901502-CA57-4D32-830B-F9F92CEA1A63')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Member & Attendees
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '4D36ADB2-9FE1-4E24-BADF-C5E53599393A') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""RecordStatusValueId"",
  ""618f906c-c33d-4fa3-8aef-e58cb7b63f1e""
]','4D36ADB2-9FE1-4E24-BADF-C5E53599393A')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Member & Attendees
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'F821C17C-B1F2-4D64-BC8B-41CFABD69BCC') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""ConnectionStatusValueId"",
  ""41540783-d9ef-4c70-8f1d-c9e83d91ed5f,39f491c5-d6ac-4a9b-8ac0-c431cb17d588""
]','F821C17C-B1F2-4D64-BC8B-41CFABD69BCC')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Member & Attendees
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'F68FFEA5-6EEE-4AD0-A19C-54021D04D038') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""IsDeceased"",
  ""1"",
  ""False""
]','F68FFEA5-6EEE-4AD0-A19C-54021D04D038')
END
" );

            // Create DataView: Member & Attendees
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'CB4BB264-A1F4-4EDB-908F-2CCF3A534BC7') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'BDD2C36F-7575-48A8-8B70-3A566E3811ED'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Member & Attendees','Lists people whos record status is ''Active'' and connection status is ''Member'' or ''Attendee''.',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'CB4BB264-A1F4-4EDB-908F-2CCF3A534BC7')
END
" );

            // Page: Directory
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Directory", "", "215932E5-0FFB-48A4-B867-5DD7AD71216A", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Person Directory", "A directory of people in database.", "~/Blocks/CRM/PersonDirectory.ascx", "CRM", "FAA234E0-9B34-4539-9987-F15E3318B4FF" );
            RockMigrationHelper.AddBlock( "215932E5-0FFB-48A4-B867-5DD7AD71216A", "", "FAA234E0-9B34-4539-9987-F15E3318B4FF", "Person Directory", "Main", "", "", 0, "1C953121-21DB-46A1-935E-A1D35FB63A7D" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "BD72BBF1-0269-407E-BDBE-EEED4F1F207F", "Data View", "DataView", "", "The data view to use as the source for the directory. Only those people returned by the data view filter will be displayed on this directory.", 0, @"cb4bb264-a1f4-4edb-908f-2ccf3a534bc7", "7D79BC2D-211D-433A-BB21-955D7CB281CD" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Opt-out Group", "OptOut", "", "A group that contains people that should be excluded from this list.", 1, @"", "D24E07C9-1D52-4797-9DAC-A6F657FDDED5" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Show By", "ShowBy", "", "People can be displayed indivually, or grouped by family", 2, @"Individual", "33A23D44-4A05-475D-96CA-D45EB582C1E8" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show All People", "ShowAllPeople", "", "Display all people by default? If false, a search is required first, and only those matching search criteria will be displayed.", 3, @"False", "E0799820-4319-4C46-8E88-7F136F2B928F" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "First Name Characters Required", "FirstNameCharactersRequired", "", "The number of characters that need to be entered before allowing a search.", 4, @"1", "DF2BBE9E-920B-43E6-A3E1-4D0DA9FB3734" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Last Name Characters Required", "LastNameCharactersRequired", "", "The number of characters that need to be entered before allowing a search.", 5, @"3", "7CA0B843-6557-4A75-9046-9196441A0AEC" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Email", "ShowEmail", "", "Should email address be included in the directory?", 6, @"True", "326CF336-26C9-4479-83F7-B316B9C6AABE" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Address", "ShowAddress", "", "Should email address be included in the directory?", 7, @"True", "550DF59B-7BD6-4ECE-B52B-A1F1288007A9" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Show Phones", "ShowPhones", "", "The phone numbers to be included in the directory", 8, @"", "AF86E220-50D5-41BB-8EB8-98635CECB00A" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Birthday", "ShowBirthday", "", "Should email address be included in the directory?", 9, @"True", "093433C9-7AB4-481A-ACF4-4218844A897D" );
            RockMigrationHelper.AddBlockTypeAttribute( "FAA234E0-9B34-4539-9987-F15E3318B4FF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Gender", "ShowGender", "", "Should email address be included in the directory?", 10, @"True", "33B29E31-8F54-4719-876D-9A85F5E7F9A8" );
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "093433C9-7AB4-481A-ACF4-4218844A897D", @"True" ); // Show Birthday
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "DF2BBE9E-920B-43E6-A3E1-4D0DA9FB3734", @"1" ); // First Name Characters Required
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "7CA0B843-6557-4A75-9046-9196441A0AEC", @"3" ); // Last Name Characters Required
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "E0799820-4319-4C46-8E88-7F136F2B928F", @"False" ); // Show All People
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "33B29E31-8F54-4719-876D-9A85F5E7F9A8", @"True" ); // Show Gender
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "7D79BC2D-211D-433A-BB21-955D7CB281CD", @"cb4bb264-a1f4-4edb-908f-2ccf3a534bc7" ); // Data View
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "33A23D44-4A05-475D-96CA-D45EB582C1E8", @"Individual" ); // Show By
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "326CF336-26C9-4479-83F7-B316B9C6AABE", @"True" ); // Show Email
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "550DF59B-7BD6-4ECE-B52B-A1F1288007A9", @"True" ); // Show Address
            RockMigrationHelper.AddBlockAttributeValue( "1C953121-21DB-46A1-935E-A1D35FB63A7D", "AF86E220-50D5-41BB-8EB8-98635CECB00A", @"407e7e45-7b2e-4fcd-9605-ecb1339f2453,aa8732fb-2cea-4c76-8d6d-6aaa2c6a4303,2cc66d5a-f61c-4b74-9af9-590a9847c13c" ); // Show Phones

            Sql( @"
    DECLARE @ParentPageId int = ( SELECT [Id] FROM [Page] WHERE [Guid] = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701' )
    UPDATE [Page] SET [Order] = [Order] + 1 WHERE [ParentPageId] = @ParentPageId AND [Guid] <> '215932E5-0FFB-48A4-B867-5DD7AD71216A'
    UPDATE [Page] SET [Order] = 0 WHERE [Guid] = '215932E5-0FFB-48A4-B867-5DD7AD71216A'
" );

            // v5.0 hotfix migrations
            Sql( @"
    DECLARE @CountryCode nvarchar(3) = ( SELECT TOP 1 [Value] FROM [DefinedValue] WHERE [DefinedTypeId] IN ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = '45E9EF7C-91C7-45AB-92C1-1D6219293847' ) ORDER BY [Order] )
    IF @CountryCode IS NOT NULL
    BEGIN
	    UPDATE [PhoneNumber]
	    SET [CountryCode] = @CountryCode
	    WHERE [CountryCode] IS NULL
    END
" );

            Sql( @"
    UPDATE [SystemEmail] 
    SET [Body] = REPLACE( [Body], ' != '' %', ' != '''' %' )
    WHERE [Body] LIKE '% != '' \%%' ESCAPE '\'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "AF86E220-50D5-41BB-8EB8-98635CECB00A" );
            RockMigrationHelper.DeleteAttribute( "550DF59B-7BD6-4ECE-B52B-A1F1288007A9" );
            RockMigrationHelper.DeleteAttribute( "326CF336-26C9-4479-83F7-B316B9C6AABE" );
            RockMigrationHelper.DeleteAttribute( "33A23D44-4A05-475D-96CA-D45EB582C1E8" );
            RockMigrationHelper.DeleteAttribute( "D24E07C9-1D52-4797-9DAC-A6F657FDDED5" );
            RockMigrationHelper.DeleteAttribute( "7D79BC2D-211D-433A-BB21-955D7CB281CD" );
            RockMigrationHelper.DeleteAttribute( "33B29E31-8F54-4719-876D-9A85F5E7F9A8" );
            RockMigrationHelper.DeleteAttribute( "E0799820-4319-4C46-8E88-7F136F2B928F" );
            RockMigrationHelper.DeleteAttribute( "7CA0B843-6557-4A75-9046-9196441A0AEC" );
            RockMigrationHelper.DeleteAttribute( "DF2BBE9E-920B-43E6-A3E1-4D0DA9FB3734" );
            RockMigrationHelper.DeleteAttribute( "093433C9-7AB4-481A-ACF4-4218844A897D" );
            RockMigrationHelper.DeleteBlock( "1C953121-21DB-46A1-935E-A1D35FB63A7D" );
            RockMigrationHelper.DeleteBlockType( "FAA234E0-9B34-4539-9987-F15E3318B4FF" );
            RockMigrationHelper.DeletePage( "215932E5-0FFB-48A4-B867-5DD7AD71216A" ); //  Page: Directory

            // Delete DataView: Member & Attendees
            Sql( @"DELETE FROM DataView where [Guid] = 'CB4BB264-A1F4-4EDB-908F-2CCF3A534BC7'" );
            
            // Delete DataViewFilter for DataView: Member & Attendees
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'F68FFEA5-6EEE-4AD0-A19C-54021D04D038'" );
            // Delete DataViewFilter for DataView: Member & Attendees
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'F821C17C-B1F2-4D64-BC8B-41CFABD69BCC'" );
            // Delete DataViewFilter for DataView: Member & Attendees
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '4D36ADB2-9FE1-4E24-BADF-C5E53599393A'" );
            // Delete DataViewFilter for DataView: Member & Attendees
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '22901502-CA57-4D32-830B-F9F92CEA1A63'" );
        }
    }
}
