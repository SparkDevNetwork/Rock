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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class ResidencyGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
begin

declare 
  @groupTypeId int

INSERT INTO [dbo].[GroupType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[GroupMemberTerm]
           ,[DefaultGroupRoleId]
           ,[ShowInNavigation]
           ,[ShowInGroupList]
           ,[IconCssClass]
           ,[Guid])
     VALUES
           (0
           ,'Residency'
           ,'Group Types for the Residency program'
           ,'Resident'
           ,null
           ,1
           ,1
           ,'icon-md'
           ,'00043CE6-EB1B-43B5-A12A-4552B91A3E28')

select @groupTypeId = @@IDENTITY

INSERT INTO [dbo].[GroupRole] 
    ([IsSystem] ,[GroupTypeId] ,[Name] ,[Description] ,[SortOrder] ,[MaxCount] ,[MinCount] ,[Guid] ,[IsLeader])
     VALUES
    (0, @groupTypeId, 'Resident', 'A Resident in the Residency program', 0, null, null, 'AC1CD9C9-782C-42A6-A28B-78B38C3AC833', 0)

update [GroupType] set [DefaultGroupRoleId] = @@IDENTITY where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28'

end
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
update [GroupType] set [DefaultGroupRoleId] = null where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28';
delete from [Group] where [GroupTypeId] in (select Id from [GroupType] where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28');
delete from [GroupRole] where [GroupTypeId] in (select Id from [GroupType] where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28');
delete from [GroupType] where [Guid] = '00043CE6-EB1B-43B5-A12A-4552B91A3E28';
" );
        }
    }
}
