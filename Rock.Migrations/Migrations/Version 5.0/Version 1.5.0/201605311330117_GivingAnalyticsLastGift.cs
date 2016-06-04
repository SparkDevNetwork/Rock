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
    public partial class GivingAnalyticsLastGift : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201605311330117_GivingAnalyticsLastGift );

            // JE: Add System Email Category for Plugins to use
            Sql( @"
    DECLARE @CategoryId int
    DECLARE @SystemEmailEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.SystemEmail')
  
    INSERT INTO[Category] ([IsSystem], [EntityTypeId], [Name], [Guid], [Order])
    VALUES(0, @SystemEmailEntityTypeId, 'Plugins', 'a6195cb5-053a-83bd-4ee0-4719ed40e299', 0)        
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
