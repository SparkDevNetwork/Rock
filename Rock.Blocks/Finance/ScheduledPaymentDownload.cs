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
using System.Collections.Generic;
using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks.Finance.ScheduledPaymentDownload;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range.
    /// </summary>
    [DisplayName( "Scheduled Payment Download" )]
    [Category( "Finance" )]
    [Description( "Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range." )]
    [IconCssClass( "ti ti-download" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [TextField(
        "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch",
        IsRequired = false,
        DefaultValue = "Online Giving",
        Order = 0 )]

    [LinkedPage(
        "Batch Detail Page",
        Key = AttributeKey.BatchDetailPage,
        Description = "The page used to display details of a batch.",
        IsRequired = false,
        Order = 1 )]

    [SystemCommunicationField(
        "Receipt Email",
        Key = AttributeKey.ReceiptEmail,
        Description = "The system email to use to send the receipts.",
        IsRequired = false,
        Order = 2 )]

    [SystemCommunicationField(
        "Failed Payment Email",
        Key = AttributeKey.FailedPaymentEmail,
        Description = "The system email to use to send a notice about a scheduled payment that failed.",
        IsRequired = false,
        Order = 3 )]

    [WorkflowTypeField(
        "Failed Payment Workflow",
        Key = AttributeKey.FailedPaymentWorkflow,
        Description = "An optional workflow to start whenever a scheduled payment has failed.",
        AllowMultiple = false,
        IsRequired = false,
        Order = 4 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "0F725C1F-B32D-4FBE-8011-892415FCBAF4" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "71FF09C3-3E50-4E97-9329-3CD57AACCA53" )]
    [Rock.SystemGuid.BlockTypeGuid( "EF56215C-111B-4B12-BD13-213C06ED3B75" )]
    public class ScheduledPaymentDownload : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string BatchDetailPage = "BatchDetailPage";
            public const string ReceiptEmail = "ReceiptEmail";
            public const string FailedPaymentEmail = "FailedPaymentEmail";
            public const string FailedPaymentWorkflow = "FailedPaymentWorkflow";
        }

        private static class PageParameterKey
        {
            public const string BatchId = "BatchId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc />
        public override object GetObsidianBlockInitialization()
        {
            var bag = new ScheduledPaymentDownloadInitializationBag();

            return bag;
        }

        /// <summary>
        /// Gets the financial gateway for the unique identifier with its attributes loaded.
        /// </summary>
        /// <param name="financialGatewayGuid">The unique identifier of the financial gateway.</param>
        /// <returns>The financial gateway, or <c>null</c> if not found.</returns>
        private FinancialGateway GetSelectedGateway( Guid? financialGatewayGuid )
        {
            if ( !financialGatewayGuid.HasValue )
            {
                return null;
            }

            var financialGateway = new FinancialGatewayService( RockContext ).Get( financialGatewayGuid.Value );

            financialGateway?.LoadAttributes( RockContext );

            return financialGateway;
        }

        /// <summary>
        /// Gets the batch detail page URL as a format string with a {0} placeholder for the batch identifier.
        /// </summary>
        /// <returns>The batch detail page URL format string.</returns>
        private string GetBatchUrlFormat()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.BatchId] = "((Key))"
            };

            return this.GetLinkedPageUrl( AttributeKey.BatchDetailPage, queryParams ).Replace( "((Key))", "{0}" );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Downloads the scheduled payments processed by the selected gateway during the requested date range and creates the matching Rock transactions.
        /// </summary>
        /// <param name="bag">The gateway and date range to download.</param>
        /// <returns>The HTML summary of the download results.</returns>
        [BlockAction]
        public BlockActionResult DownloadTransactions( DownloadTransactionsRequestBag bag )
        {
            // The helper already makes End exclusive, so no extra day is added here.
            var dateRange = bag?.DateRange?.ToActualDateRange();

            if ( dateRange?.Start == null || dateRange.End == null || dateRange.End.Value < dateRange.Start.Value )
            {
                return ActionBadRequest( "Please select a valid Date Range!" );
            }

            var financialGateway = GetSelectedGateway( bag.FinancialGatewayGuid );

            if ( financialGateway == null || !financialGateway.IsActive )
            {
                return ActionBadRequest( "Please select a valid Payment Gateway!" );
            }

            var gatewayComponent = financialGateway.GetGatewayComponent();

            if ( gatewayComponent == null || !gatewayComponent.IsActive )
            {
                return ActionBadRequest( "Selected Payment Gateway does not have a valid payment processor!" );
            }

            var payments = gatewayComponent.GetPayments( financialGateway, dateRange.Start.Value, dateRange.End.Value, out var errorMessage );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage );
            }

            var batchNamePrefix = GetAttributeValue( AttributeKey.BatchNamePrefix );
            var receiptEmail = GetAttributeValue( AttributeKey.ReceiptEmail ).AsGuidOrNull();
            var failedPaymentEmail = GetAttributeValue( AttributeKey.FailedPaymentEmail ).AsGuidOrNull();
            var failedPaymentWorkflowType = GetAttributeValue( AttributeKey.FailedPaymentWorkflow ).AsGuidOrNull();

            var resultSummary = FinancialScheduledTransactionService.ProcessPayments( financialGateway, batchNamePrefix, payments, GetBatchUrlFormat(), receiptEmail, failedPaymentEmail, failedPaymentWorkflowType );

            // The summary embeds gateway-supplied ids, so strip any script content before it is rendered as HTML.
            return ActionOk( new DownloadTransactionsResultBag
            {
                SummaryHtml = resultSummary.IsNotNullOrWhiteSpace()
                    ? $"<ul>{resultSummary}</ul>".SanitizeHtml( strict: false )
                    : "There were not any transactions downloaded."
            } );
        }

        #endregion Block Actions
    }
}
