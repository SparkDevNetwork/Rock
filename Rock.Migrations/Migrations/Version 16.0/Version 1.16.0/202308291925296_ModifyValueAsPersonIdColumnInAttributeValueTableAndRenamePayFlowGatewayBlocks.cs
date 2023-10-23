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

    /// <summary>
    ///
    /// </summary>
    public partial class ModifyValueAsPersonIdColumnInAttributeValueTableAndRenamePayFlowGatewayBlocks : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RemoveForeignKeyFromValueAsPersonIdColumnInAttributeValueTableUp();
            AddTermLegacyToPayFlowProFinancialGatewayUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveForeignKeyFromValueAsPersonIdColumnInAttributeValueTableDown();
        }

        /// <summary>
        /// NA:Add the Term "Legacy" to Pay Flow Pro Financial Gateway
        /// </summary>
        private void AddTermLegacyToPayFlowProFinancialGatewayUp()
        {
            Sql( @"
            DECLARE @PayFlowProEntityTypeId INT = (SELECT ID FROM [EntityType] WHERE [Guid] = 'D4A40C4A-336F-49A6-9F44-88F149726126')
            UPDATE [FinancialGateway] SET [Name] = CONCAT([Name], ' (Legacy)') 
            WHERE EntityTypeId = @PayFlowProEntityTypeId AND [Name] NOT LIKE '%Legacy%'" );
        }

        /// <summary>
        /// PA: Remove FK from AttributeValue.ValueAsPersonId to speed up Person Merge
        /// </summary>
        private void RemoveForeignKeyFromValueAsPersonIdColumnInAttributeValueTableUp()
        {
            DropForeignKey( "dbo.AttributeValue", "ValueAsPersonId", "dbo.Person" );
            DropIndex( "dbo.AttributeValue", new[] { "ValueAsPersonId" } );
        }


        private void RemoveForeignKeyFromValueAsPersonIdColumnInAttributeValueTableDown()
        {
            CreateIndex( "dbo.AttributeValue", "ValueAsPersonId" );
            AddForeignKey( "dbo.AttributeValue", "ValueAsPersonId", "dbo.Person", "Id" );
        }
    }
}
