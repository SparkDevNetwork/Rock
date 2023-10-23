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
    public partial class Rollup_20231019 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MarkMyWellGatewayInRockAsLegacyUp();
            MarkWebformsBlockAsLegacyUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// PA: Mark MyWell Gateway in Rock as Legacy
        /// </summary>
        private void MarkMyWellGatewayInRockAsLegacyUp()
        {
            Sql( @"
   DECLARE @MyWellEntityTypeId INT = (SELECT ID FROM [EntityType] WHERE [Guid] = 'E81ED723-E807-4BDE-ADF1-AB9686241637')
UPDATE [FinancialGateway] SET [Name] = CONCAT([Name], ' (Legacy)') 
WHERE EntityTypeId = @MyWellEntityTypeId AND [Name] NOT LIKE '%Legacy%';

UPDATE [FinancialGateway] SET [Description] = CONCAT([Description], ' This legacy version of the My Well Gateway is no longer supported.  Please contact MyWell and install their new gateway from the RockShop.') 
WHERE EntityTypeId = @MyWellEntityTypeId;

UPDATE [EntityType] SET [FriendlyName] = CONCAT([FriendlyName], ' (Legacy)') WHERE [Id] = @MyWellEntityTypeId;" );
        }

        /// <summary>
        /// NA: Mark Webforms Notes block as Legacy
        /// </summary>
        private void MarkWebformsBlockAsLegacyUp()
        {
            Sql( $"UPDATE [BlockType] SET [Name] = 'Notes (Legacy)' WHERE [Guid] = '{Rock.SystemGuid.BlockType.NOTES}'" );
        }
    }
}
