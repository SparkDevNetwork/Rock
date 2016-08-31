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
    public partial class FixAdultMembersDataView : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateEntityType( string name, string friendlyName, string assemblyName, bool isEntity, bool isSecured, string guid )

            UpdateEntityType( "Rock.Reporting.DataFilter.Person.InGroupGroupTypeFilter", "In Group Group Type Filter", "Rock.Reporting.DataFilter.Person.InGroupGroupTypeFilter, Rock, Version=1.0.2.0, Culture=neutral, PublicKeyToken=null", false, true, "0E239967-6D33-4205-B19F-08AD8FF6ED0B" );
            
            Sql( @"
declare
  @parentId int = (select [DataViewFilterId] from [DataView] where [Guid] = '0da5f82f-cffe-45af-b725-49b3899a1f72'),
  @entityTypeId int = (select [id] from [EntityType] where [Guid] = '0E239967-6D33-4205-B19F-08AD8FF6ED0B'),
  @groupTypeId int = (select [id] from [GroupType] where [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' /* GROUPTYPE_FAMILY */),
  @groupRoleId int = (select [id] from [GroupTypeRole] where [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' /* GROUPROLE_FAMILY_MEMBER_ADULT */)


delete from DataViewFilter where [Guid] = '5E8418A6-E1CE-49CF-BA14-F56ACD19FBD0'

INSERT INTO [dbo].[DataViewFilter]
           ([ExpressionType]
           ,[ParentId]
           ,[EntityTypeId]
           ,[Selection]
           ,[Guid]
           )
     VALUES
           (0
           ,@parentId
           ,@entityTypeId
           ,CONVERT(nvarchar(max),@groupTypeId) + N'|' + CONVERT(nvarchar(max),@groupRoleId)
           ,'5E8418A6-E1CE-49CF-BA14-F56ACD19FBD0'
           )


" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "delete from DataViewFilter where [Guid] = '5E8418A6-E1CE-49CF-BA14-F56ACD19FBD0'" );
        }
    }
}
