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
    public partial class CalculatePersonDuplicatesTimeout : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // set the commandtimeout for the Calculate Person Duplicates job to 30 minutes
            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FF66ABF1-B01D-4AE7-814E-95D842B2EA99')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [ServiceJob] where [Guid] = 'C386528C-3AC6-44E8-884E-A57B571B65D5')

                DELETE FROM [AttributeValue] WHERE [Guid] = '2BE0AA51-DFED-4755-BC13-24AD1E0EFE14'

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'1800','2BE0AA51-DFED-4755-BC13-24AD1E0EFE14')" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // set the commandtimeout for the Calculate Person Duplicates job to 5 minutes
            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FF66ABF1-B01D-4AE7-814E-95D842B2EA99')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [ServiceJob] where [Guid] = 'C386528C-3AC6-44E8-884E-A57B571B65D5')

                DELETE FROM [AttributeValue] WHERE [Guid] = '2BE0AA51-DFED-4755-BC13-24AD1E0EFE14'

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'300','2BE0AA51-DFED-4755-BC13-24AD1E0EFE14')" );
        }
    }
}
