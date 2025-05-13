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

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Finance.TransactionList;
using Rock.Common.Mobile.ViewModel;
using Rock.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// A Block for displaying a list of transaction made by a person.
    /// </summary>
    [DisplayName( "Transaction List" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Transaction List block." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [IntegerField(
        "Past Years Filter Limit",
        Description = "Sets the maximum number of past years a user can filter when viewing transaction history.",
        IsRequired = true,
        DefaultIntegerValue = 6,
        Key = AttributeKey.PastYearsFilterLimit,
        Order = 0 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a Transaction List. TransactionDetailGuid is passed in the query string.",
        IsRequired = true,
        Key = AttributeKey.TransactionDetail,
        Order = 1 )]

    [MobileNavigationActionField(
        "Give Now Action",
        Description = "When no result are shown how should the 'Give Now' button behave.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PushPageValue,
        Key = AttributeKey.GiveNowAction,
        Order = 2
        )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_TRANSACTION_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_TRANSACTION_LIST )]
    public class TransactionList : RockBlockType
    {

        #region Constants

        /// <summary>
        /// The maximum number of past years a user can filter when viewing transaction history.
        /// </summary>
        private int PastYearsFilterLimit => GetAttributeValue( AttributeKey.PastYearsFilterLimit ).AsInteger();

        /// <summary>
        /// The Transaction Detail page GUID.
        /// </summary>
        private Guid? TransactionDetailPageGuid => GetAttributeValue( AttributeKey.TransactionDetail ).AsGuidOrNull();

        /// <summary>
        /// Get the Mobile Navigation Action specify by individual.
        /// </summary>
        private MobileNavigationActionViewModel GiveNowAction => GetAttributeValue( AttributeKey.GiveNowAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel();

        #endregion

        #region Keys

        /// <summary>
        /// Attribute keys used in the block setting.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Transaction detail attribute key.
            /// </summary>
            public const string TransactionDetail = "TransactionDetail";

            /// <summary>
            /// Past years filter limit attribute key.
            /// </summary>
            public const string PastYearsFilterLimit = "PastYearsFilterLimit";

            /// <summary>
            /// Give now action attribute key.
            /// </summary>
            public const string GiveNowAction = "GiveNowAction";
        }

        /// <summary>
        /// Page parameter keys used for passing data.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// Person page parameter key.
            /// </summary>
            public const string Person = "Person";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Retrieves a list of transactions for the specified person and year.
        /// </summary>
        /// <param name="options">The request parameters, including the selected year.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the transaction list.</returns>
        [BlockAction]
        public BlockActionResult GetTransactionList( TransactionListRequestBag options )
        {
            int? personId = GetCurrentPerson().Id;
            if (personId == null )
            {
                return ActionOk( new TransactionListResponseBag
                {
                    Transactions = new List<TransactionItemBag>(),
                } );
            }

            var transactions = new FinancialTransactionService( RockContext )
                .Queryable()
                .Where( t => t.TransactionDateTime.HasValue && t.TransactionDateTime.Value.Year == options.Year )
                .Where( t => t.AuthorizedPersonAlias.PersonId == personId )
                .ToList()
                .Select( t => new TransactionItemBag
                {
                    Amount = t.TotalAmount.FormatAsCurrency(),
                    TransactionDateTime = t.TransactionDateTime,
                    Accounts = t.TransactionDetails.Select( td => td.Account.Name ).ToList(),
                    IdKey = t.IdKey,
                } ).ToList();

            return ActionOk( new TransactionListResponseBag
            {
                Transactions = transactions,
            } );
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Finance.TransactionList.Configuration
            {
                PastYearsFilterLimit = PastYearsFilterLimit,
                TransactionDetailPageGuid = TransactionDetailPageGuid,
                GiveNowAction = GiveNowAction,
            };
        }

        #endregion
    }
}
