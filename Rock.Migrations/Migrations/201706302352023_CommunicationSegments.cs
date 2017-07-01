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
    public partial class CommunicationSegments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.DATAVIEW, "Communication Segments", string.Empty, "Dataviews that can be used to refine a communication recipient list when creating a communication", "FF7081F8-7223-43D4-BE28-CB030DC4E13B" );

            // Create [GroupAll] DataViewFilter for DataView: 35 and older
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: 35 and older
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '256|35|,' for Rock.Reporting.DataFilter.Person.AgeFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '90563175-E8C6-4845-9D60-78C0BBD5A9BE') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'256|35|,','90563175-E8C6-4845-9D60-78C0BBD5A9BE')
END
" );
            // Create DataView: 35 and older
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = '5537D54C-1B9B-4B81-AA63-F10D676FAE77') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'35 and older','A filter to help refine a communications recipient list to include only people that are 35 and older',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'5537D54C-1B9B-4B81-AA63-F10D676FAE77')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Female
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','01CDD06D-810E-4861-85EE-69FB8C97EA3C')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Female
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '[
              "Property_Gender",
              "Female"
            ]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'CD9ADB66-67E5-4F3E-A481-13EE257407EC') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""Property_Gender"",
  ""Female""
]','CD9ADB66-67E5-4F3E-A481-13EE257407EC')
END
" );
            // Create DataView: Female
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Female','A filter to help refine a communications recipient list to include only females',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Male
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','348FF1A5-2D80-4FC6-86AF-0FC3C117982A')
END
" );
            // Create Rock.Reporting.DataFilter.PropertyFilter DataViewFilter for DataView: Male
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '[
              "Property_Gender",
              "Male"
            ]' for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '03D9CA62-4F58-43C6-A508-AE8597B27539') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""Property_Gender"",
  ""Male""
]','03D9CA62-4F58-43C6-A508-AE8597B27539')
END
" );
            // Create DataView: Male
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'C43983D7-1F22-4E94-9F5C-342DA3A0E168') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Male','A filter to help refine a communications recipient list to include only males',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'C43983D7-1F22-4E94-9F5C-342DA3A0E168')
END
" );

            // Create [GroupAll] DataViewFilter for DataView: Under 35
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','8760D688-01B9-4030-BF19-898D68CBA757')
END
" );
            // Create Rock.Reporting.DataFilter.Person.AgeFilter DataViewFilter for DataView: Under 35
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection '512|35|,' for Rock.Reporting.DataFilter.Person.AgeFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '9423C06E-C5D3-4589-8EA7-813A6F59F13B') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '4911C63D-71BB-4686-AAA3-D66EA41DA465')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'512|35|,','9423C06E-C5D3-4589-8EA7-813A6F59F13B')
END
" );
            // Create DataView: Under 35
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'FF608842-BB10-4C9C-AA18-9D5C407590D3') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'FF7081F8-7223-43D4-BE28-CB030DC4E13B'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Under 35','A filter to help refine a communications recipient list to include only people that under age of 35',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'FF608842-BB10-4C9C-AA18-9D5C407590D3')
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Delete DataView: 35 and older
            Sql( @"DELETE FROM DataView where [Guid] = '5537D54C-1B9B-4B81-AA63-F10D676FAE77'" );
            // Delete DataViewFilter for DataView: 35 and older
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '90563175-E8C6-4845-9D60-78C0BBD5A9BE'" );
            // Delete DataViewFilter for DataView: 35 and older
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '6D8E0255-2BEE-4E7F-AD79-1A224E07D5AF'" );

            // Delete DataView: Female
            Sql( @"DELETE FROM DataView where [Guid] = 'E2CC2258-BF35-4DB2-91E2-9BE1B68156A3'" );
            // Delete DataViewFilter for DataView: Female
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'CD9ADB66-67E5-4F3E-A481-13EE257407EC'" );
            // Delete DataViewFilter for DataView: Female
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '01CDD06D-810E-4861-85EE-69FB8C97EA3C'" );

            // Delete DataView: Male
            Sql( @"DELETE FROM DataView where [Guid] = 'C43983D7-1F22-4E94-9F5C-342DA3A0E168'" );
            // Delete DataViewFilter for DataView: Male
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '03D9CA62-4F58-43C6-A508-AE8597B27539'" );
            // Delete DataViewFilter for DataView: Male
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '348FF1A5-2D80-4FC6-86AF-0FC3C117982A'" );

            // Delete DataView: Under 35
            Sql( @"DELETE FROM DataView where [Guid] = 'FF608842-BB10-4C9C-AA18-9D5C407590D3'" );
            // Delete DataViewFilter for DataView: Under 35
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '9423C06E-C5D3-4589-8EA7-813A6F59F13B'" );
            // Delete DataViewFilter for DataView: Under 35
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '8760D688-01B9-4030-BF19-898D68CBA757'" );
        }
    }
}
