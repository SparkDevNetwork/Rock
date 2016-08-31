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
    public partial class AddGroupRoleFormerSpouse : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql(@"
                INSERT INTO [GroupTypeRole]
                   ([IsSystem]
                   ,[GroupTypeId]
                   ,[Name]
                   ,[Description]
                   ,[Order]
                   ,[IsLeader]
                   ,[Guid])
                VALUES
                   (0
                   ,11
                   ,'Former Spouse'
                   ,'Role to identify former spouses after divorce.'
                   ,12
                   ,0
                   ,'60C6142E-8E00-4678-BC2F-983BB7BDE80B')");

            Sql(@"INSERT INTO [AttributeValue] 
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                    SELECT 0
                        ,540
                        ,[Id]
                        ,0
                        ,'60c6142e-8e00-4678-bc2f-983bb7bde80b'
                        ,'150069D7-079B-46B5-BE74-98167BA42CC4'
	                FROM [GROUPTypeRole]
	                WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql(@"DELETE FROM [AttributeValue] WHERE [Guid] = '150069D7-079B-46B5-BE74-98167BA42CC4'");
            Sql(@"DELETE FROM [GroupTypeRole] WHERE [Guid] = '60C6142E-8E00-4678-BC2F-983BB7BDE80B'");
        }
    }
}
