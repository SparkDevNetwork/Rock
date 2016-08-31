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
    public partial class AddPastoralStaffRole : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"INSERT INTO [Group]
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
                       (0
                       ,null
                       ,1
                       ,null
                       ,'Pastoral Staff'
                       ,'Group of individuals who can access information limited to just pastors on staff.'
                       ,1
                       ,1
                       ,0
                       ,'26E7148C-2059-4F45-BCFE-32230A12F0DC')");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"DELETE FROM [Group] 
                    WHERE [Guid] = '26E7148C-2059-4F45-BCFE-32230A12F0DC'");
        }
    }
}
