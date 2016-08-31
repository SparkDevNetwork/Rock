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
    public partial class FixGroupRoles : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                DECLARE @PrincipalGroupTypeRoleId int
                SET @PrincipalGroupTypeRoleId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '27198949-FAD3-4BD6-820C-FEB98AA61E7D')
                
                DECLARE @BusinessContactGroupTypeRoleId int
                SET @BusinessContactGroupTypeRoleId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '102E6AF5-62C2-4767-B473-C9C228D75FB6')                

                UPDATE [GroupMember] SET [GroupRoleId] = @BusinessContactGroupTypeRoleId WHERE [GroupRoleId] = @PrincipalGroupTypeRoleId
                " );

            Sql( @"
                DELETE FROM [GroupTypeRole] 
                WHERE [Guid] = '27198949-FAD3-4BD6-820C-FEB98AA61E7D'" );

            Sql( @"
                UPDATE [GroupTypeRole]
                SET [Description] = 'A role to identify the business a person is associated with.' 
                WHERE [Guid] = '7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0'" );

            Sql( @"
                DELETE FROM [AttributeValue]
                WHERE [Guid] = '4659F638-9BD0-4ADA-BAB1-6E9BD0F6BF73'" );

            Sql( @"
                DELETE FROM [AttributeValue]
                WHERE [Guid] = '79F1B549-4B3D-45CE-A697-CDD6A8608D45'" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '102E6AF5-62C2-4767-B473-C9C228D75FB6')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0'
                    ,'C84E6B6C-7F20-4080-8CF7-05B4455A1C92')" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'102E6AF5-62C2-4767-B473-C9C228D75FB6'
                    ,'B80D4392-6AF6-4FD1-8664-8E38BB1AA66C')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                DELETE FROM [AttributeValue]
                WHERE [Guid] = 'B80D4392-6AF6-4FD1-8664-8E38BB1AA66C'" );

            Sql( @"
                DELETE FROM [AttributeValue]
                WHERE [Guid] = 'C84E6B6C-7F20-4080-8CF7-05B4455A1C92'" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '27198949-FAD3-4BD6-820C-FEB98AA61E7D')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0'
                    ,'79F1B549-4B3D-45CE-A697-CDD6A8608D45')" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (0
                    ,@AttributeId
                    ,@EntityId
                    ,0
                    ,'27198949-FAD3-4BD6-820C-FEB98AA61E7D'
                    ,'4659F638-9BD0-4ADA-BAB1-6E9BD0F6BF73')" );

            Sql( @"
                UPDATE [GroupTypeRole]
                SET [Description] = 'A role to identify the business a person owns.' 
                WHERE [Guid] = '7FC58BB2-7C1E-4C5C-B2B3-4738258A0BE0'" );

            Sql( @"
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
                   ,'Principle'
                   ,'A role to identify the owner of a business.'
                   ,0
                   ,0
                   ,'27198949-FAD3-4BD6-820C-FEB98AA61E7D')" );

            Sql( @"
                DECLARE @PrincipalGroupTypeRoleId int
                SET @PrincipalGroupTypeRoleId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '27198949-FAD3-4BD6-820C-FEB98AA61E7D')
                
                DECLARE @BusinessContactGroupTypeRoleId int
                SET @BusinessContactGroupTypeRoleId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '102E6AF5-62C2-4767-B473-C9C228D75FB6')                

                UPDATE [GroupMember] SET [GroupRoleId] = @PrincipalGroupTypeRoleId WHERE [GroupRoleId] = @BusinessContactGroupTypeRoleId
                " );
        }
    }
}
