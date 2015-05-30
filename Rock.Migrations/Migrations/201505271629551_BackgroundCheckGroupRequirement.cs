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
    public partial class BackgroundCheckGroupRequirement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.DATAVIEW, "Group Requirements", "", "", "E62709E3-0060-4778-AA34-4B0FD9F6DF2E" );


            // Create DataViewFilter for DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '00000000-0000-0000-0000-000000000000'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '00000000-0000-0000-0000-000000000000')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (1,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'','D294FB17-8872-47C8-AC29-96714B3DDE9F')
END
" );
            // Create DataViewFilter for DataView: Background check is still valid
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'BD7FE43A-B887-41A0-9C65-A2E6D9685596') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundChecked"",
  ""True""
]','BD7FE43A-B887-41A0-9C65-A2E6D9685596')
END
" );
            // Create DataViewFilter for DataView: Background check is still valid
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '256E15E6-9D9A-4539-8BE1-0B0F68BD2342') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundCheckDate"",
  ""256"",
  ""CURRENT:-730""
]','256E15E6-9D9A-4539-8BE1-0B0F68BD2342')
END
" );
            // Create DataViewFilter for DataView: Background check is still valid
            /* NOTE to Developer. Review that the generated DataViewFilter.Selection for Rock.Reporting.DataFilter.PropertyFilter will work on different databases */
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = 'F020509F-C8D0-477A-AC9C-22541918A2CC') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values (0,@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'[
  ""BackgroundCheckResult"",
  ""Pass""
]','F020509F-C8D0-477A-AC9C-22541918A2CC')
END
" );
            // Create DataView: Background check is still valid
            Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = 'AED692A5-4BB0-40FA-8C62-7948FAB894C5') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = 'E62709E3-0060-4778-AA34-4B0FD9F6DF2E'),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7'),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '00000000-0000-0000-0000-000000000000')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'Background check is still valid','Returns people that have been background checked within the last two years',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'AED692A5-4BB0-40FA-8C62-7948FAB894C5')
END
" );

            // Create Group Requirement Type using Background Check dataview
            Sql( @"
DECLARE @GroupRequirementTypeBackgroundCheck UNIQUEIDENTIFIER = '1C21C346-A861-4A9A-BD6D-BAA7D92419D5'
    ,@RequirementCheckTypeDataview INT = 1
    ,@DataViewIdBackgroundCheck int = (SELECT top 1 Id from DataView where [Guid] = 'AED692A5-4BB0-40FA-8C62-7948FAB894C5')

IF NOT EXISTS (
        SELECT *
        FROM GroupRequirementType
        WHERE Guid = @GroupRequirementTypeBackgroundCheck
        )
BEGIN
    INSERT INTO [GroupRequirementType] (
        [Name]
        ,[Description]
        ,[CanExpire]
        ,[ExpireInDays]
        ,[RequirementCheckType]
        ,[DataViewId]
        ,[PositiveLabel]
        ,[NegativeLabel]
        ,[CheckboxLabel]
        ,[Guid]
        )
    VALUES (
        'Background Check Required'
        ,'Ensures that group members have an up-to-date background check'
        ,1
        ,30
        ,@RequirementCheckTypeDataview
        ,@DataViewIdBackgroundCheck
        ,'Background Checked'
        ,'No Current Background Check'
        ,NULL
        ,@GroupRequirementTypeBackgroundCheck
        )
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            /// Delete DataViewFilter for DataView: Background check is still valid
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'F020509F-C8D0-477A-AC9C-22541918A2CC'" );
            // Delete DataViewFilter for DataView: Background check is still valid
            Sql( @"DELETE FROM DataViewFilter where [Guid] = '256E15E6-9D9A-4539-8BE1-0B0F68BD2342'" );
            // Delete DataViewFilter for DataView: Background check is still valid
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'BD7FE43A-B887-41A0-9C65-A2E6D9685596'" );
            // Delete DataViewFilter for DataView: Background check is still valid
            Sql( @"DELETE FROM DataViewFilter where [Guid] = 'D294FB17-8872-47C8-AC29-96714B3DDE9F'" );

            // Delete DataView: Background check is still valid
            Sql( @"DELETE FROM DataView where [Guid] = 'AED692A5-4BB0-40FA-8C62-7948FAB894C5'" );
        }
    }
}
