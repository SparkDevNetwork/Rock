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
    public partial class AddUpdateStepsCategory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Steps", "", "", "517BB2D3-0A50-4132-818E-63BB3C81EAE9", 8, "6F09163D-7DDD-4E1E-8D18-D7CAA04451A7" );
            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C405062-72BB-4362-9738-90C9ED5ACDDE')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [Category] where [Guid] = '517BB2D3-0A50-4132-818E-63BB3C81EAE9')

                DELETE FROM [AttributeValue] WHERE [Guid] = 'E26C06E4-DCF5-4742-94A1-CB3DAF36EF8A'

                INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'~/Steps/Record/{0}','E26C06E4-DCF5-4742-94A1-CB3DAF36EF8A')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
