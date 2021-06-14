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
using System;

using Rock.Plugin;

namespace Rock.StatementGenerator.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 2, "1.6.10" )]
    [Obsolete( "Use FinancialStatementTemplate instead" )]
    [RockObsolete( "12.4" )]
    public class StatementGeneratorUpdateTemplates : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Rock Default
            this.UpdateDefinedValueIfNotModified(
                "7C504683-0DE2-41ED-A640-867125713D23",
                "Rock Default",
                "The default statement generator lava template. It includes a  transaction list, account summary, non-cash contributions section and a pledges section. Use this as a starting point for making a custom template. A logo size of 240 x 80px works best for this template.",
                0
                );

            this.UpdateDefinedValueAttributeValueIfNotModified(
                "7C504683-0DE2-41ED-A640-867125713D23",
                Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_LAVA_TEMPLATE,
                MigrationResource._002_StatementGeneratorUpdateTemplates_RockDefault
                );

            // Rock Alternate 1
            this.UpdateDefinedValueIfNotModified(
                "8D1FF6BC-4FCD-42DA-AAEB-735461467302",
                "Rock Alternate 1",
                "A custom template that is similar to the default. It has a borderless transaction list that includes a currency column. A logo size of 240 x 80px works best for this template.",
                1
                );

            this.UpdateDefinedValueAttributeValueIfNotModified(
                "8D1FF6BC-4FCD-42DA-AAEB-735461467302",
                Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_LAVA_TEMPLATE,
                MigrationResource._002_StatementGeneratorUpdateTemplates_Alternate1
                );

            // Rock Alternate 2
            this.UpdateDefinedValueIfNotModified(
                "05F8D725-73BE-4C39-9EF1-5A926D5FAB67",
                "Rock Alternate 2",
                "A custom template that is similar to the default. There are minor tweaks for some of the text, it does not include an account summary and the pledges are arranged horizontally into 4 columns. A logo size of 240 x 80px works best for this template.",
                2
                );

            this.UpdateDefinedValueAttributeValueIfNotModified(
                "05F8D725-73BE-4C39-9EF1-5A926D5FAB67",
                Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_LAVA_TEMPLATE,
                MigrationResource._002_StatementGeneratorUpdateTemplates_Alternate2
                );
        }

        /// <summary>
        /// Updates the defined value if not modified.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="order">The order.</param>
        private void UpdateDefinedValueIfNotModified( string definedValueGuid, string value, string description, int order )
        {
            Sql( $@"
UPDATE [DefinedValue] 
   SET 
        [Value] = '{value.Replace( "'", "''" )}', 
        [Description] = '{description.Replace( "'", "''" )}',
        [Order] = {order}
   WHERE [Guid] =  '{definedValueGuid}'
     AND ModifiedDateTime is null
" );
        }

        /// <summary>
        /// Updates the defined value attribute value if not modified.
        /// </summary>
        /// <param name="definedValueGuid">The defined value unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        /// <param name="attributeValue">The attribute value.</param>
        private void UpdateDefinedValueAttributeValueIfNotModified( string definedValueGuid, string attributeGuid, string attributeValue )
        {
            Sql( $@"
DECLARE @DefinedValueId INT

SET @DefinedValueId = (
		SELECT [Id]
		FROM [DefinedValue]
		WHERE [Guid] = '{definedValueGuid}'
		)

DECLARE @AttributeId INT

SET @AttributeId = (
		SELECT [Id]
		FROM [Attribute]
		WHERE [Guid] = '{attributeGuid}'
		)

UPDATE AttributeValue
SET [Value] = '{attributeValue.Replace( "'", "''" )}'
WHERE EntityId = @DefinedValueId
	AND AttributeId = @AttributeId
	AND ModifiedDateTime is null
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
