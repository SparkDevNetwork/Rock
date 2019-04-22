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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 53, "1.8.1" )]
    public class DuplicateDataIntegrityReports : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter", "Has Duplicate Phone Filter", "Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter, Rock, Version=1.8.1.0, Culture=neutral, PublicKeyToken=null", false, true, "32ea43cc-8744-4180-9bc7-9915f467e54b" );
//            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter", "Has Duplicate Email Filter", "Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter, Rock, Version=1.8.1.0, Culture=neutral, PublicKeyToken=null", false, true, "2771a757-dd80-420c-a128-a9fb24f40da9" );
//            RockMigrationHelper.AddEntityAttribute( "Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "805303CC-35B7-4C49-AD2D-C93D59F53494" );
//            RockMigrationHelper.AddEntityAttribute( "Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "416114CC-6F31-4ACC-85C9-8641B5239AB0" );
//            RockMigrationHelper.AddEntityAttribute( "Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "", "", "Order", "", "The order that this service should be used (priority)", 0, "", "57D6A71E-EC69-45D4-819E-36506C7DD3FA" );
//            RockMigrationHelper.AddEntityAttribute( "Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "False", "25CC2F66-1CFF-4C0E-9AAB-F17F3C678993" );

//            // Create [GroupAll] DataViewFilter for DataView: People with Duplicate Phone Numbers
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'ECD5554E-9F69-46E2-861C-4238A0B8428A') BEGIN    
//    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
//    values (1,NULL,NULL,'','ECD5554E-9F69-46E2-861C-4238A0B8428A')
//END
//" );
//            // Create Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter DataViewFilter for DataView: People with Duplicate Phone Numbers
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '4EB44900-7381-4BCF-B527-1B4571134828') BEGIN    
//    DECLARE
//        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'ECD5554E-9F69-46E2-861C-4238A0B8428A'),
//        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '32EA43CC-8744-4180-9BC7-9915F467E54B')

//    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
//    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','4EB44900-7381-4BCF-B527-1B4571134828')
//END
//" );
//            // Create DataView: People with Duplicate Phone Numbers
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataView where [Guid] = '93AFFF40-CA13-4F19-8BCA-CC5CF2896384') BEGIN
//DECLARE
//    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'BDD2C36F-7575-48A8-8B70-3A566E3811ED'),
//    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
//    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = 'ECD5554E-9F69-46E2-861C-4238A0B8428A')

//INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
//VALUES(0,'People with Duplicate Phone Numbers','People who have duplicate phone numbers.',@categoryId,@entityTypeId,@dataViewFilterId,NULL,'93AFFF40-CA13-4F19-8BCA-CC5CF2896384')
//END
//" );

//            // Create [GroupAll] DataViewFilter for DataView: People with Duplicate Emails
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '0BC16586-B9D1-49B4-9E4E-2AF69A0C63A5') BEGIN    
//    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
//    values (1,NULL,NULL,'','0BC16586-B9D1-49B4-9E4E-2AF69A0C63A5')
//END
//" );
//            // Create Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter DataViewFilter for DataView: People with Duplicate Emails
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'CE323E30-5C26-4AE6-AF37-C39B9A05192B') BEGIN    
//    DECLARE
//        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '0BC16586-B9D1-49B4-9E4E-2AF69A0C63A5'),
//        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '2771A757-DD80-420C-A128-A9FB24F40DA9')

//    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
//    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','CE323E30-5C26-4AE6-AF37-C39B9A05192B')
//END
//" );
//            // Create DataView: People with Duplicate Emails
//            Sql( @"
//IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'E62BBEC8-2B7B-41AF-9598-28B2BF21944D') BEGIN
//DECLARE
//    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'BDD2C36F-7575-48A8-8B70-3A566E3811ED'),
//    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
//    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '0BC16586-B9D1-49B4-9E4E-2AF69A0C63A5')

//INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
//VALUES(0,'People with Duplicate Emails','People who have duplicate email addresses.',@categoryId,@entityTypeId,@dataViewFilterId,NULL,'E62BBEC8-2B7B-41AF-9598-28B2BF21944D')
//END
//" );

//            RockMigrationHelper.AddReport( "d738d12d-bc3b-47b0-8a90-f7924d137595", "93afff40-ca13-4f19-8bca-cc5cf2896384", "72657ed8-d16e-492e-ac12-144c5e7567e7", "Individuals with Duplicate Phone Numbers", "People who have duplicate phone numbers.", "cccbf8d5-3968-42d1-87e9-ca909a9b5d00" );
//            RockMigrationHelper.AddReport( "d738d12d-bc3b-47b0-8a90-f7924d137595", "e62bbec8-2b7b-41af-9598-28b2bf21944d", "72657ed8-d16e-492e-ac12-144c5e7567e7", "Individuals with Duplicate Emails", "People who have duplicate email addresses.", "69d012db-bebd-4a83-bfb6-880655e1b571" );
//            RockMigrationHelper.AddReportField( "cccbf8d5-3968-42d1-87e9-ca909a9b5d00", Model.ReportFieldType.DataSelectComponent, true, "6301f6b4-b2ef-469a-8ec2-7d5f06b55c60", "{\"ShowAsLink\":\"True\",\"DisplayOrder\":\"0\"}", 0, "Name", "04532a6c-0bc7-4eb6-ae5b-d636e0707fff" );
//            RockMigrationHelper.AddReportField( "cccbf8d5-3968-42d1-87e9-ca909a9b5d00", Model.ReportFieldType.DataSelectComponent, true, "5cbfbc11-826a-4b83-845e-247dd6268fff", "407e7e45-7b2e-4fcd-9605-ecb1339f2453|False", 1, "Mobile Phone", "2a0961fd-f658-4989-b23d-9a6d2762ff7c" );
//            RockMigrationHelper.AddReportField( "cccbf8d5-3968-42d1-87e9-ca909a9b5d00", Model.ReportFieldType.DataSelectComponent, true, "5cbfbc11-826a-4b83-845e-247dd6268fff", "aa8732fb-2cea-4c76-8d6d-6aaa2c6a4303|False", 2, "Home Phone", "a90872df-614a-4a82-b6f6-d83838a9b184" );
//            RockMigrationHelper.AddReportField( "cccbf8d5-3968-42d1-87e9-ca909a9b5d00", Model.ReportFieldType.DataSelectComponent, true, "5cbfbc11-826a-4b83-845e-247dd6268fff", "2cc66d5a-f61c-4b74-9af9-590a9847c13c|False", 3, "Work Phone", "92f93f8d-8b5c-4caf-9338-2d117fe273c7" );
//            RockMigrationHelper.AddReportField( "69d012db-bebd-4a83-bfb6-880655e1b571", Model.ReportFieldType.DataSelectComponent, true, "6301f6b4-b2ef-469a-8ec2-7d5f06b55c60", "{\"ShowAsLink\":\"True\",\"DisplayOrder\":\"0\"}", 0, "Name", "60efff7b-10e7-4784-9e71-d9246b785cf4" );
//            RockMigrationHelper.AddReportField( "69d012db-bebd-4a83-bfb6-880655e1b571", Model.ReportFieldType.Property, true, new Guid().ToString(), "Email", 1, "Email", "4fe499da-8c2a-463a-ad2f-87045ccdfaa4" );
        }



        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //RockMigrationHelper.DeleteReportField( "04532a6c-0bc7-4eb6-ae5b-d636e0707fff" );
            //RockMigrationHelper.DeleteReportField( "2a0961fd-f658-4989-b23d-9a6d2762ff7c" );
            //RockMigrationHelper.DeleteReportField( "a90872df-614a-4a82-b6f6-d83838a9b184" );
            //RockMigrationHelper.DeleteReportField( "92f93f8d-8b5c-4caf-9338-2d117fe273c7" );
            //RockMigrationHelper.DeleteReportField( "60efff7b-10e7-4784-9e71-d9246b785cf4" );
            //RockMigrationHelper.DeleteReportField( "4fe499da-8c2a-463a-ad2f-87045ccdfaa4" );

            //RockMigrationHelper.DeleteReport( "cccbf8d5-3968-42d1-87e9-ca909a9b5d00" );
            //RockMigrationHelper.DeleteReport( "69d012db-bebd-4a83-bfb6-880655e1b571" );

            //// Delete DataView: People with Duplicate Phone Numbers
            //Sql( @"DELETE FROM DataView where [Guid] = '93AFFF40-CA13-4F19-8BCA-CC5CF2896384'" );
            //// Delete DataViewFilter for DataView: People with Duplicate Phone Numbers
            //Sql( @"DELETE FROM DataViewFilter where [Guid] = '4EB44900-7381-4BCF-B527-1B4571134828'" );
            //// Delete DataViewFilter for DataView: People with Duplicate Phone Numbers
            //Sql( @"DELETE FROM DataViewFilter where [Guid] = 'ECD5554E-9F69-46E2-861C-4238A0B8428A'" );

            //// Delete DataView: People with Duplicate Emails
            //Sql( @"DELETE FROM DataView where [Guid] = 'E62BBEC8-2B7B-41AF-9598-28B2BF21944D'" );
            //// Delete DataViewFilter for DataView: People with Duplicate Emails
            //Sql( @"DELETE FROM DataViewFilter where [Guid] = 'CE323E30-5C26-4AE6-AF37-C39B9A05192B'" );
            //// Delete DataViewFilter for DataView: People with Duplicate Emails
            //Sql( @"DELETE FROM DataViewFilter where [Guid] = '0BC16586-B9D1-49B4-9E4E-2AF69A0C63A5'" );

            //RockMigrationHelper.DeleteAttribute( "805303CC-35B7-4C49-AD2D-C93D59F53494" );    // Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter: Order
            //RockMigrationHelper.DeleteAttribute( "416114CC-6F31-4ACC-85C9-8641B5239AB0" );    // Rock.Reporting.DataFilter.Person.HasDuplicateEmailFilter: Active
            //RockMigrationHelper.DeleteAttribute( "57D6A71E-EC69-45D4-819E-36506C7DD3FA" );    // Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter: Order
            //RockMigrationHelper.DeleteAttribute( "25CC2F66-1CFF-4C0E-9AAB-F17F3C678993" );    // Rock.Reporting.DataFilter.Person.HasDuplicatePhoneFilter: Active

            //RockMigrationHelper.DeleteEntityType( "32ea43cc-8744-4180-9bc7-9915f467e54b" );
            //RockMigrationHelper.DeleteEntityType( "2771a757-dd80-420c-a128-a9fb24f40da9" );
        }
    }
}
