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
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Finance.TransactionDetail;
using Rock.Lava;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// A Block for displaying the transaction detail.
    /// </summary>
    [DisplayName( "Transaction Detail" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Transaction Detail block." )]
    [IconCssClass( "fa fa-receipt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_TRANSACTION_DETAIL_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_TRANSACTION_DETAIL )]
    public class TransactionDetail : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The page parameter keys for the TransactionDetail block.
        /// </summary>
        public static class PageParameterKeys
        {
            /// <summary>
            /// The transaction Identifier
            /// </summary>
            public const string Transaction = "Transaction";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the credit card type image URL based on the payment details.
        /// </summary>
        /// <param name="financialPaymentDetail">The financial payment detail object.</param>
        /// <returns>A string representing the URL of the credit card type image.</returns>
        private string GetPaymentMethodImage( FinancialPaymentDetail financialPaymentDetail )
        {
            if ( financialPaymentDetail.CreditCardTypeValue == null )
            {
                return string.Empty;
            }

            var creditCardTypeValueCache = DefinedValueCache.Get( financialPaymentDetail.CreditCardTypeValueId.Value );
            return creditCardTypeValueCache?.GetAttributeValue( SystemKey.CreditCardTypeAttributeKey.IconImage );
        }

        /// <summary>
        /// Get the transaction detail response bag.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction to extract details from.</param>
        /// <returns>A <see cref="TransactionDetailResponseBag"/> with the transaction details.</returns>
        private TransactionDetailResponseBag GetTransactionDetailResponseBag( FinancialTransaction financialTransaction )
        {
            var financialPaymentDetail = financialTransaction.FinancialPaymentDetail;

            var creditCardTypeImage = string.Empty;
            var accountNumberMasked = string.Empty;
            if ( financialPaymentDetail != null )
            {
                creditCardTypeImage = GetPaymentMethodImage( financialPaymentDetail );
                accountNumberMasked = financialPaymentDetail.AccountNumberMasked;
            }

            var financialTransactionDetailList = financialTransaction.TransactionDetails;
            var transactionDetailList = financialTransactionDetailList.Select( ( td ) =>
            {
                var amount = td.FeeCoverageAmount.HasValue ? ( td.Amount - td.FeeCoverageAmount ) : td.Amount;

                return new TransactionDetailItem
                {
                    AccountName = td.Account.Name,
                    AmountFormat = amount.FormatAsCurrency(),
                };
            } ).ToList();

            if ( financialTransaction.TotalFeeCoverageAmount.HasValue )
            {
                transactionDetailList.Add( new TransactionDetailItem
                {
                    AccountName = "Fee Coverage",
                    AmountFormat = financialTransaction.TotalFeeCoverageAmount.FormatAsCurrency()
                } );
            }

            if ( accountNumberMasked.IsNotNullOrWhiteSpace() && accountNumberMasked.Length > 4 )
            {
                accountNumberMasked = accountNumberMasked.Substring( accountNumberMasked.Length - 5 );
            }

            return new TransactionDetailResponseBag
            {
                TotalAmount = financialTransaction.TotalAmount.FormatAsCurrency(),
                Details = transactionDetailList,
                DateTime = financialTransaction.TransactionDateTime?.ToString( @"MMM dd, yyyy" ) ?? "Unknown Date",
                AccountNumberMasked = accountNumberMasked,
                PaymentMethodType = financialPaymentDetail.CurrencyAndCreditCardType,
                PaymentMethodImage = MobileHelper.BuildPublicApplicationRootUrl( creditCardTypeImage )
            };
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Retrieves the details of a transaction based on the transaction identifier provided.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetTransactionDetails()
        {
            var transactionId = PageParameter( PageParameterKeys.Transaction );
            if ( transactionId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "You must pass in a transaction identifier." );
            }

            var financialTransactionService = new FinancialTransactionService( RockContext );

            var financialTransaction = financialTransactionService.Get( transactionId, !this.PageCache.Layout.Site.DisablePredictableIds );
            if ( financialTransaction == null )
            {
                return ActionNotFound( "There is no transaction with the provided identifier." );
            }

            return ActionOk( GetTransactionDetailResponseBag( financialTransaction ) );
        }

        #endregion
    }
}
