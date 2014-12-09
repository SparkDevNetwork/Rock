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
    public partial class FacebookFriends : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateGroupTypeRole( "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF", "Facebook Friend",
                "A Facebook friend. NOTE: This role is automatically added/removed whenever this person (or the friend) logs into Rock using Facebook.",
                0, null, null, "AB69816C-4DFA-4A7A-86A5-9BFCBA6FED1E" );

            Sql( @"
                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852')

                DECLARE @EntityId int
                SET @EntityId = (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = 'AB69816C-4DFA-4A7A-86A5-9BFCBA6FED1E')

                INSERT INTO [AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Value]
                    ,[Guid])
                VALUES
                    (1
                    ,@AttributeId
                    ,@EntityId
                    ,'AB69816C-4DFA-4A7A-86A5-9BFCBA6FED1E'
                    ,'408B5A5C-F66E-42FE-B956-F0CAC3C7DBFC')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteGroupTypeRole("AB69816C-4DFA-4A7A-86A5-9BFCBA6FED1E");
        }
    }
}
