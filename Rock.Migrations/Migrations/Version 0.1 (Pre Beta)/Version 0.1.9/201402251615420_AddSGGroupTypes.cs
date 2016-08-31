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
    public partial class AddSGGroupTypes : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // The new "meeting location" group location type
            AddDefinedValue( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Meeting location", "Indicates the place where the group regularly meets.", "96D540F5-071D-4BBD-9906-28F0A64D39C4", true );

            // Update descriptions on Home, Work, and Previous group location types:
            UpdateDefinedValue( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Home", "Address where the family lives.", "8C52E53C-2A66-435A-AE6E-5EE307D9A0DC" );
            UpdateDefinedValue( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Work", "Address where the group works.", "E071472A-F805-4FC4-917A-D5E3C095C35C" );
            UpdateDefinedValue( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Previous", "Address where the family used to live.", "853D98F1-6E08-4321-861B-520B4106CFE0" );

            Sql( @"

                -- Add the Small Group Section and Small Group types
                DECLARE @SGSectionGroupTypeGuid uniqueidentifier = 'FAB75EC6-0402-456A-BE34-252097DE4F20'
                DECLARE @SGGroupTypeGuid uniqueidentifier = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'

                INSERT [GroupType] 
                    ([IsSystem]
                    ,[Name]
                    ,[Description]
                    ,[GroupTerm]
                    ,[GroupMemberTerm]
                    ,[DefaultGroupRoleId]
                    ,[AllowMultipleLocations]
                    ,[ShowInGroupList]
                    ,[ShowInNavigation]
                    ,[IconCssClass]
                    ,[TakesAttendance]
                    ,[AttendanceRule]
                    ,[AttendancePrintTo]
                    ,[Order]
                    ,[InheritedGroupTypeId]
                    ,[LocationSelectionMode]
                    ,[GroupTypePurposeValueId]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime]
                    ,[CreatedByPersonAliasId]
                    ,[ModifiedByPersonAliasId]) 
                VALUES
                    (1
                    ,'Small Group Section'
                    ,'Holds a hierarchy of small groups for a particular segment.'
                    ,'Group'
                    ,'Member'
                    ,NULL
                    ,0
                    ,1
                    ,1
                    ,'fa fa-sitemap'
                    ,0
                    ,0
                    ,0
                    ,0
                    ,NULL
                    ,0
                    ,NULL
                    ,@SGSectionGroupTypeGuid
                    ,CAST(0x0000A2DC00FDBD8A AS DateTime)
                    ,CAST(0x0000A2DC0103005A AS DateTime)
                    ,NULL
                    ,NULL)

                INSERT [GroupType] 
                    ([IsSystem]
                    ,[Name]
                    ,[Description]
                    ,[GroupTerm]
                    ,[GroupMemberTerm]
                    ,[DefaultGroupRoleId]
                    ,[AllowMultipleLocations]
                    ,[ShowInGroupList]
                    ,[ShowInNavigation]
                    ,[IconCssClass]
                    ,[TakesAttendance]
                    ,[AttendanceRule]
                    ,[AttendancePrintTo]
                    ,[Order]
                    ,[InheritedGroupTypeId]
                    ,[LocationSelectionMode]
                    ,[GroupTypePurposeValueId]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime]
                    ,[CreatedByPersonAliasId]
                    ,[ModifiedByPersonAliasId]) 
                VALUES
                    (1
                    ,'Small Group'
                    ,'A group of people who share an interest and meet together with regular frequency.'
                    ,'Group'
                    ,'Member'
                    ,NULL
                    ,0
                    ,1
                    ,1
                    ,'fa fa-home'
                    ,0
                    ,0
                    ,0
                    ,0
                    ,NULL
                    ,19
                    ,NULL
                    ,@SGGroupTypeGuid
                    ,CAST(0x0000A2DC00FFFFE0 AS DateTime)
                    ,CAST(0x0000A2DC01022A56 AS DateTime)
                    ,NULL
                    ,NULL)

                -- Add roles...

                DECLARE @SmallGroupSectionTypeId int
                SET @SmallGroupSectionTypeId = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = @SGSectionGroupTypeGuid )

                -- Add Member role for the Small Group Section type group
                INSERT INTO [GroupTypeRole] 
                    ([IsSystem]
                    ,[GroupTypeId]
                    ,[Name]
                    ,[Description]
                    ,[Order]
                    ,[MaxCount]
                    ,[MinCount]
                    ,[IsLeader]
                    ,[Guid]) 
                VALUES
                    (1
                    ,@SmallGroupSectionTypeId
                    ,'Member'
                    ,''
                    ,0
                    ,NULL
                    ,NULL
                    ,0
                    ,'E1D1C03D-4218-4936-82C1-FB52F0B8A0FF')

                DECLARE @GroupTypeRoleId int = @@IDENTITY

                -- Update the Small Group Section group type with the default role id
                UPDATE [GroupType]
                    SET [DefaultGroupRoleId] = @GroupTypeRoleId
                WHERE
                    [Guid] = @SGSectionGroupTypeGuid

                DECLARE @SmallGroupTypeId int
                SET @SmallGroupTypeId = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B' )

                -- Add Member and Leader roles for the Small Group type group.
                INSERT INTO [GroupTypeRole]
                    ([IsSystem]
                    ,[GroupTypeId]
                    ,[Name]
                    ,[Description]
                    ,[Order]
                    ,[MaxCount]
                    ,[MinCount]
                    ,[IsLeader]
                    ,[Guid]) 
                VALUES
                    (1
                    ,@SmallGroupTypeId
                    ,'Member'
                    ,''
                    ,0
                    ,NULL
                    ,NULL
                    ,0
                    ,'F0806058-7E5D-4CA9-9C04-3BDF92739462')

                SET @GroupTypeRoleId = @@IDENTITY

                INSERT INTO [GroupTypeRole]
                    ([IsSystem]
                    ,[GroupTypeId]
                    ,[Name]
                    ,[Description]
                    ,[Order]
                    ,[MaxCount]
                    ,[MinCount]
                    ,[IsLeader]
                    ,[Guid])
                VALUES
                    (1
                    ,@SmallGroupTypeId
                    ,'Leader'
                    ,'Indicates the person is a leader in the group.'
                    ,1
                    ,NULL
                    ,NULL
                    ,1
                    ,'6D798EFA-0110-41D5-BCE4-30ACEFE4317E')

                -- Update the Small Group group type with the default role id
                UPDATE [GroupType] SET [DefaultGroupRoleId] = @GroupTypeRoleId
                WHERE [Guid] = @SGGroupTypeGuid

                INSERT [GroupTypeAssociation]
                    ([GroupTypeId]
                    ,[ChildGroupTypeId])
                VALUES
                    (@SmallGroupSectionTypeId
                    ,@SmallGroupSectionTypeId)

                INSERT [GroupTypeAssociation]
                    ([GroupTypeId]
                    ,[ChildGroupTypeId])
                VALUES
                    (@SmallGroupSectionTypeId
                    ,@SmallGroupTypeId)

                -- add Meeting Location to the Small Group group type location types
                DECLARE @LocationTypeValueId int
                SET @LocationTypeValueId = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '96D540F5-071D-4BBD-9906-28F0A64D39C4' )

                INSERT [GroupTypeLocationType]
                    ([GroupTypeId]
                    ,[LocationTypeValueId])
                VALUES
                    (@SmallGroupTypeId
                    ,@LocationTypeValueId)

                -- add two small group sections, Section A and Section B
                INSERT [Group]
                    ([IsSystem]
                    ,[ParentGroupId]
                    ,[GroupTypeId]
                    ,[CampusId]
                    ,[Name]
                    ,[Description]
                    ,[IsSecurityRole]
                    ,[IsActive]
                    ,[Order]
                    ,[Guid])
                VALUES
                    (1
                    ,NULL
                    ,@SmallGroupSectionTypeId
                    ,NULL
                    ,'Section A'
                    ,'A placeholder section to demonstrate organizing small groups into separate hierarchies.'
                    ,0
                    ,1
                    ,0
                    ,'C05E60C4-6DFC-420D-8DBA-28DBB5F0E3F9')
                    
                INSERT [Group]
                    ([IsSystem]
                    ,[ParentGroupId]
                    ,[GroupTypeId]
                    ,[CampusId]
                    ,[Name]
                    ,[Description]
                    ,[IsSecurityRole]
                    ,[IsActive]
                    ,[Order]
                    ,[Guid])
                VALUES
                    (1
                    ,NULL
                    ,@SmallGroupSectionTypeId
                    ,NULL
                    ,'Section B'
                    ,'A second placeholder section to demonstrate organizing small groups into separate hierarchies.'
                    ,0
                    ,1
                    ,0
                    ,'3CBC1A3F-DD26-430A-9B53-B476CB385ABC')
" );

            // add the group type group attributes 
            AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Study Topic", "The subject of conversation or discussion during meetings.", 0, "", "A73D3FE8-FC1C-4474-B68D-9A145D9E4A15" );
            AddGroupTypeGroupAttribute( "50FCFB30-F51A-49DF-86F4-2B176EA1820B", "2F8F5EC4-57FA-4F6C-AB15-9D6616994580", "Meeting Time", "Indicates the usual time that the group gathers.", 1, null, "E439FBC6-0098-4419-A89F-0465569A5BEE" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                DELETE FROM [Group]
                WHERE
                    [Guid] = '3CBC1A3F-DD26-430A-9B53-B476CB385ABC'
                    OR [Guid] = 'C05E60C4-6DFC-420D-8DBA-28DBB5F0E3F9'

                DECLARE @SmallGroupSectionTypeId int
                SET @SmallGroupSectionTypeId = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = 'FAB75EC6-0402-456A-BE34-252097DE4F20' )

                DECLARE @SmallGroupTypeId int
                SET @SmallGroupTypeId = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B' )

                DELETE FROM [GroupTypeLocationType]
                WHERE
                    [GroupTypeId] = @SmallGroupTypeId

                DELETE FROM [GroupTypeAssociation]
                WHERE
                    [GroupTypeId] = @SmallGroupSectionTypeId

                DELETE FROM [GroupType]
                    WHERE 
                    [Guid] = 'FAB75EC6-0402-456A-BE34-252097DE4F20'
                    OR [Guid] = '50FCFB30-F51A-49DF-86F4-2B176EA1820B'

                DELETE FROM [GroupTypeRole] 
                WHERE
                    [Guid] = 'E1D1C03D-4218-4936-82C1-FB52F0B8A0FF'
                    OR [Guid] = 'F0806058-7E5D-4CA9-9C04-3BDF92739462'
                    OR [Guid] = '6D798EFA-0110-41D5-BCE4-30ACEFE4317E'

" );
            // remove the meeting location defined value.
            DeleteDefinedValue("96d540f5-071d-4bbd-9906-28f0a64d39c4");
            DeleteAttribute( "A73D3FE8-FC1C-4474-B68D-9A145D9E4A15" );
            DeleteAttribute( "E439FBC6-0098-4419-A89F-0465569A5BEE" );
        }
    }
}
