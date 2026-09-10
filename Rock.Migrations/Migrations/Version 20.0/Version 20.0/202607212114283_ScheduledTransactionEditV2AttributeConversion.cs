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
    public partial class ScheduledTransactionEditV2AttributeConversion : Rock.Migrations.RockMigration
    {
        #region Guids

        private const string ObsidianBlockTypeGuid = "F1ADF375-7442-4B30-BAC3-C387EA9B6C18";
        private const string ExternalGivingProfilePageGuid = "2072F4BC-53B4-4481-BC15-38F14425C6C9";
        private const string InternalPeoplePageGuid = "D360B64F-1267-4518-95CD-99CD5AB87D88";
        private const string InternalFinancePageGuid = "F1C3BBD3-EE91-4DDD-8880-1542EBCD8041";

        #endregion Guids

        #region Success Lava Templates

        private const string InternalSuccessAlert = @"<div class='alert alert-success'>
    <p class='margin-b-none'>Success! Your scheduled transaction information has been updated.</p>
</div>";

        private const string ExternalSuccessAlert = @"<div class='alert alert-success'>
    <strong>Giving profile successfully updated.</strong>
    <p class='margin-b-none'>Thank you! Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our mission. We are so grateful for your commitment.</p>
</div>";

        private const string InternalFinishLavaTemplate = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

{% assign total = 0 %}
{% for transactionDetail in transactionDetails %}
    {% assign total = total | Plus:transactionDetail.Amount %}
{% endfor %}

<div class='alert alert-success'>
    <p class='margin-b-none'>Success! Your scheduled transaction information has been updated.</p>
</div>

<h4>Gift Information</h4>
<div style='border:1px solid var(--color-interface-soft);border-radius:var(--rounded-small);overflow:hidden;margin-bottom:var(--spacing-large);'>
    <table style='width:100%;border-collapse:collapse;'>
        <tbody>
            {% for transactionDetail in transactionDetails %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ transactionDetail.Account.PublicName }}</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ transactionDetail.Amount | FormatAsCurrency }}</td>
                </tr>
            {% endfor %}
            <tr>
                <td style='padding:12px 16px;'><strong>Total</strong></td>
                <td class='text-right' style='padding:12px 16px;'><strong>{{ total | FormatAsCurrency }}</strong></td>
            </tr>
        </tbody>
    </table>
</div>

<h4>Payment &amp; Confirmation</h4>
<div style='border:1px solid var(--color-interface-soft);border-radius:var(--rounded-small);overflow:hidden;'>
    <table style='width:100%;border-collapse:collapse;'>
        <tbody>
            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Payment Method</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ PaymentDetail.CurrencyTypeValue.Value }}</td>
            </tr>

            {% if PaymentDetail.AccountNumberMasked != '' %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Account Number</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>
                        {% if PaymentDetail.CreditCardTypeValue %}
                            {{ PaymentDetail.CreditCardTypeValue.Value }} Ending in {{ PaymentDetail.AccountNumberMasked | Right:4 }}
                        {% else %}
                            {{ PaymentDetail.AccountNumberMasked }}
                        {% endif %}
                    </td>
                </tr>
            {% endif %}

            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>When</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>
                    {% if Transaction.TransactionFrequencyValue %}
                        {{ Transaction.TransactionFrequencyValue.Value }}
                        {% if Transaction.EndDate %}
                            starting on {{ Transaction.NextPaymentDate | Date:'sd' }} and ending on {{ Transaction.EndDate | Date:'sd' }}
                        {% else %}
                            starting on {{ Transaction.NextPaymentDate | Date:'sd' }}
                        {% endif %}
                    {% else %}
                        Today
                    {% endif %}
                </td>
            </tr>

            <tr>
                <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Name</td>
                <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ Person.FullName }}</td>
            </tr>

            {% if Person.Email != '' %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Email</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ Person.Email }}</td>
                </tr>
            {% endif %}

            {% if BillingLocation %}
                <tr>
                    <td class='text-muted' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>Address</td>
                    <td class='text-right' style='padding:12px 16px;border-bottom:1px solid var(--color-interface-soft);'>{{ BillingLocation.Street1 }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</td>
                </tr>
            {% endif %}

            <tr>
                <td class='text-muted' style='padding:12px 16px;'>Confirmation</td>
                <td class='text-right' style='padding:12px 16px;'><span class='label label-info'>{{ Transaction.TransactionCode }}</span></td>
            </tr>
        </tbody>
    </table>
</div>
";

        // The exact WebForms default that shipped before the conversion; used only to recognize uncustomized instances.
        private const string OldWebFormsFinishLavaTemplate = @"
{% if Transaction.ScheduledTransactionDetails %}
    {% assign transactionDetails = Transaction.ScheduledTransactionDetails %}
{% else %}
    {% assign transactionDetails = Transaction.TransactionDetails %}
{% endif %}

<h1>Thank You!</h1>

<p>Your support is helping {{ 'Global' | Attribute:'OrganizationName' }} actively achieve our
mission. We are so grateful for your commitment.</p>

<dl>
    <dt>Confirmation Code</dt>
    <dd>{{ Transaction.TransactionCode }}</dd>
    <dd></dd>

    <dt>Name</dt>
    <dd>{{ Person.FullName }}</dd>
    <dd></dd>
    <dd>{{ Person.Email }}</dd>
    <dd>{{ BillingLocation.Street }} {{ BillingLocation.City }}, {{ BillingLocation.State }} {{ BillingLocation.PostalCode }}</dd>
</dl>

<dl class='dl-horizontal'>
    {% for transactionDetail in transactionDetails %}
        <dt>{{ transactionDetail.Account.PublicName }}</dt>
        <dd>{{ transactionDetail.Amount }}</dd>
    {% endfor %}
    <dd></dd>

    <dt>Payment Method</dt>
    <dd>{{ PaymentDetail.CurrencyTypeValue.Description}}</dd>

    {% if PaymentDetail.AccountNumberMasked  != '' %}
        <dt>Account Number</dt>
        <dd>{{ PaymentDetail.AccountNumberMasked }}</dd>
    {% endif %}

    <dt>When<dt>
    <dd>

    {% if Transaction.TransactionFrequencyValue %}
        {{ Transaction.TransactionFrequencyValue.Value }} //- Updated to include EndDate
{% if Transaction.EndDate %}starting on {{ Transaction.NextPaymentDate | Date:'sd' }} and ending on {{ Transaction.EndDate | Date:'sd' }}{% else %}starting on {{ Transaction.NextPaymentDate | Date:'sd' }}{% endif %}
    {% else %}
        Today
    {% endif %}
    </dd>
</dl>
";

        // The short placeholder template that shipped as the People/Finance default; also treated as uncustomized.
        private const string OldSimpleFinishLavaTemplate = @"<p>The transaction has been updated.</p>";

        #endregion Success Lava Templates

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                ObsidianBlockTypeGuid,
                Rock.SystemGuid.FieldType.TEXT,
                "Panel Title",
                "PanelTitle",
                "Panel Title",
                "The title displayed in the panel header.",
                1,
                "",
                "98636E01-8761-4688-940E-E4E4A952831E" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                ObsidianBlockTypeGuid,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Show Block Header Section",
                "ShowBlockHeader",
                "Show Block Header Section",
                "When enabled, displays a title and description at the top of the block.",
                2,
                "True",
                "34A4886F-7650-428F-920C-98E3907C865F" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                ObsidianBlockTypeGuid,
                Rock.SystemGuid.FieldType.BOOLEAN,
                "Show Section Descriptions",
                "ShowSectionDescriptions",
                "Show Section Descriptions",
                "When enabled, displays the supporting description text below each section header.",
                4,
                "True",
                "DE1DBB5B-A170-4F38-B392-39529D9C6243" );

            RockMigrationHelper.AddOrUpdateBlockTypeAttribute(
                ObsidianBlockTypeGuid,
                Rock.SystemGuid.FieldType.CODE_EDITOR,
                "Success Template",
                "FinishLavaTemplate",
                "Success Template",
                "The Lava-enabled HTML displayed after the transaction is saved.",
                19,
                InternalFinishLavaTemplate,
                "9F8D74CB-6E0D-47ED-B522-F6A3E3289326" );

            var internalTemplate = InternalFinishLavaTemplate;
            var externalTemplate = InternalFinishLavaTemplate.Replace( InternalSuccessAlert, ExternalSuccessAlert );

            // Fail loudly rather than silently writing the internal template to external instances if the alert swap did not match.
            if ( externalTemplate == internalTemplate )
            {
                throw new Exception( "EditScheduledTransactionV2Conversion: the internal success alert was not found in the template, so the external template could not be generated." );
            }

            var internalTemplateSql = internalTemplate.Replace( "'", "''" );
            var externalTemplateSql = externalTemplate.Replace( "'", "''" );
            var oldTemplateSql = OldWebFormsFinishLavaTemplate.Replace( "'", "''" );
            var oldSimpleTemplateSql = OldSimpleFinishLavaTemplate.Replace( "'", "''" );

            // Compares the stored value to each recognized legacy default with all whitespace removed so formatting drift is ignored.
            var strippedStoredValue = StripWhitespaceSql( "[av].[Value]" );
            var strippedOldDefault = StripWhitespaceSql( $"'{oldTemplateSql}'" );
            var strippedOldSimpleDefault = StripWhitespaceSql( $"'{oldSimpleTemplateSql}'" );

            Sql( $@"
DECLARE @BlockEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '{Rock.SystemGuid.EntityType.BLOCK}' );
DECLARE @BlockTypeId INT = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Guid] = '{ObsidianBlockTypeGuid}' );

DECLARE @ExternalGivingProfilePageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{ExternalGivingProfilePageGuid}' );
DECLARE @InternalPeoplePageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{InternalPeoplePageGuid}' );
DECLARE @InternalFinancePageId INT = ( SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = '{InternalFinancePageGuid}' );

IF @BlockEntityTypeId IS NOT NULL AND @BlockTypeId IS NOT NULL
BEGIN
    DECLARE @FinishAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR(20) ) AND [Key] = 'FinishLavaTemplate' );
    DECLARE @ShowBlockHeaderAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR(20) ) AND [Key] = 'ShowBlockHeader' );
    DECLARE @ShowSectionDescriptionsAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR(20) ) AND [Key] = 'ShowSectionDescriptions' );
    DECLARE @PanelTitleAttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @BlockEntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS NVARCHAR(20) ) AND [Key] = 'PanelTitle' );

    -- Gather every block instance on one of the three known pages, flagging the two internal Rock RMS pages.
    DECLARE @Blocks TABLE ( BlockId INT, IsInternal BIT );
    INSERT INTO @Blocks ( BlockId, IsInternal )
    SELECT [b].[Id], CASE WHEN [b].[PageId] = @ExternalGivingProfilePageId THEN 0 ELSE 1 END
    FROM [Block] AS [b]
    WHERE [b].[BlockTypeId] = @BlockTypeId
        AND [b].[PageId] IN ( @ExternalGivingProfilePageId, @InternalPeoplePageId, @InternalFinancePageId );

    -- 1. Replace the Success Lava Template on instances still on the old WebForms default; internal keeps the plain alert, external gets the donor-facing alert.
    IF @FinishAttributeId IS NOT NULL
    BEGIN
        UPDATE [av]
        SET [av].[Value] = CASE WHEN [blk].[IsInternal] = 1 THEN '{internalTemplateSql}' ELSE '{externalTemplateSql}' END
            , [av].[PersistedTextValue] = NULL
            , [av].[PersistedHtmlValue] = NULL
            , [av].[PersistedCondensedTextValue] = NULL
            , [av].[PersistedCondensedHtmlValue] = NULL
            , [av].[IsPersistedValueDirty] = 1
        FROM [AttributeValue] AS [av]
        INNER JOIN @Blocks AS [blk] ON [blk].[BlockId] = [av].[EntityId]
        WHERE [av].[AttributeId] = @FinishAttributeId
            AND {strippedStoredValue} IN ( {strippedOldDefault}, {strippedOldSimpleDefault} );

        -- The block's built-in default renders the internal alert, so external instances without a stored template need the donor-facing template written explicitly.
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty] )
        SELECT 0, @FinishAttributeId, [blk].[BlockId], '{externalTemplateSql}', NEWID(), 1
        FROM @Blocks AS [blk]
        WHERE [blk].[IsInternal] = 0
            AND NOT EXISTS ( SELECT 1 FROM [AttributeValue] AS [av] WHERE [av].[AttributeId] = @FinishAttributeId AND [av].[EntityId] = [blk].[BlockId] );
    END

    -- 2. Internal instances hide the block header section (external keeps the True default).
    IF @ShowBlockHeaderAttributeId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty] )
        SELECT 0, @ShowBlockHeaderAttributeId, [blk].[BlockId], 'False', NEWID(), 1
        FROM @Blocks AS [blk]
        WHERE [blk].[IsInternal] = 1
            AND NOT EXISTS ( SELECT 1 FROM [AttributeValue] AS [av] WHERE [av].[AttributeId] = @ShowBlockHeaderAttributeId AND [av].[EntityId] = [blk].[BlockId] );
    END

    -- 3. Internal instances hide the section descriptions (external keeps the True default).
    IF @ShowSectionDescriptionsAttributeId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty] )
        SELECT 0, @ShowSectionDescriptionsAttributeId, [blk].[BlockId], 'False', NEWID(), 1
        FROM @Blocks AS [blk]
        WHERE [blk].[IsInternal] = 1
            AND NOT EXISTS ( SELECT 1 FROM [AttributeValue] AS [av] WHERE [av].[AttributeId] = @ShowSectionDescriptionsAttributeId AND [av].[EntityId] = [blk].[BlockId] );
    END

    -- 4. Internal instances get a panel title; the external instance keeps the empty default.
    IF @PanelTitleAttributeId IS NOT NULL
    BEGIN
        INSERT INTO [AttributeValue] ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid], [IsPersistedValueDirty] )
        SELECT 0, @PanelTitleAttributeId, [blk].[BlockId], 'Edit Scheduled Transaction', NEWID(), 1
        FROM @Blocks AS [blk]
        WHERE [blk].[IsInternal] = 1
            AND NOT EXISTS ( SELECT 1 FROM [AttributeValue] AS [av] WHERE [av].[AttributeId] = @PanelTitleAttributeId AND [av].[EntityId] = [blk].[BlockId] );
    END
END
" );
        }

        /// <summary>
        /// Wraps a SQL expression in nested REPLACE calls that strip tab, line feed, carriage return, and space so a comparison ignores formatting differences.
        /// </summary>
        /// <param name="expression">The SQL expression (column reference or quoted literal) to strip.</param>
        /// <returns>The SQL fragment that evaluates to the whitespace-stripped value.</returns>
        private static string StripWhitespaceSql( string expression )
        {
            return $"REPLACE(REPLACE(REPLACE(REPLACE({expression}, CHAR(9), ''), CHAR(10), ''), CHAR(13), ''), ' ', '')";
        }
    }
}
