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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Updates the default "Payment Information Instructions" for the Payment Entry
    /// workflow action so the "Amount: $X" line is only rendered when an amount has
    /// been supplied. Without this guard, the individual-entered amount case renders
    /// both a stale "Amount: $0.00" line and the CurrencyBox "Amount" label, which
    /// visually collide. Fix for issue #6951.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 310, "19.5" )]
    public class FixPaymentEntryInstructionsDefault6951 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                8/4/26 - NA

                The default value for the PaymentInformationInstructions attribute on the
                Payment Entry workflow action unconditionally rendered "<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}".
                When the workflow is configured to let the individual enter the amount
                (Amount attribute is blank), that line still rendered as "Amount: ..."
                and stacked visually with the CurrencyBox "Amount" label below it.

                Wrapping the line in {% if PaymentConfiguration.Amount %}...{% endif %}
                makes the two mutually exclusive. Both the Attribute default and any
                AttributeValue rows still holding the old shipped default are updated;
                customized values are left alone. The DefaultPersisted* columns are
                cleared and IsDefaultPersistedValueDirty is set so Rock re-renders them
                from the new DefaultValue on the next persistence pass.

                Reason: https://github.com/SparkDevNetwork/Rock/issues/6951
            */
            Sql( @"
DECLARE @WorkflowActionTypeEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.WorkflowActionType' );
DECLARE @EntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Workflow.Action.PaymentEntry' );
DECLARE @AttributeId INT = ( SELECT [Id] FROM [Attribute]
    WHERE [Key] = 'PaymentInformationInstructions'
      AND [EntityTypeId] = @WorkflowActionTypeEntityTypeId
      AND [EntityTypeQualifierColumn] = 'EntityTypeId'
      AND [EntityTypeQualifierValue] = CAST( @EntityTypeId AS NVARCHAR( 100 ) ) );

DECLARE @OldFragment NVARCHAR(MAX) = '""<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}""';
DECLARE @NewFragment NVARCHAR(MAX) = '""{% if PaymentConfiguration.Amount %}<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}{% endif %}""';

IF @AttributeId IS NOT NULL
BEGIN
    -- Update the Attribute default so newly created Payment Entry actions inherit the corrected default.
    UPDATE [Attribute]
    SET [DefaultValue] = REPLACE( [DefaultValue], @OldFragment, @NewFragment ),
        [DefaultPersistedTextValue] = NULL,
        [DefaultPersistedHtmlValue] = NULL,
        [DefaultPersistedCondensedTextValue] = NULL,
        [DefaultPersistedCondensedHtmlValue] = NULL,
        [IsDefaultPersistedValueDirty] = 1
    WHERE [Id] = @AttributeId

    -- Update any existing AttributeValue rows still holding the old shipped default.
    UPDATE [AttributeValue]
    SET [Value] = REPLACE( [Value], @OldFragment, @NewFragment ),
        [PersistedTextValue] = NULL,
        [PersistedHtmlValue] = NULL,
        [PersistedCondensedTextValue] = NULL,
        [PersistedCondensedHtmlValue] = NULL,
        [IsPersistedValueDirty] = 1
    WHERE [AttributeId] = @AttributeId
      AND [Value] LIKE '%' + @OldFragment + '%';
END
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
