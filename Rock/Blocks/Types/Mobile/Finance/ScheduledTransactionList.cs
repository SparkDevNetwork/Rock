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
using Rock.Model;
using Rock.ViewModels.Blocks.Crm.PersonDetail.GivingConfiguration;
using System.ComponentModel;
using System.Linq;
using System.Data.Entity;
using Rock;
using Rock.Common.Mobile.Blocks.Finance.ScheduledTransactionList;
using Rock.Common.Mobile.ViewModel;
using System;
using System.Collections.Generic;

namespace Rock.Blocks.Types.Mobile.Finance
{
    /// <summary>
    /// The Rock Mobile Scheduled Transaction List block, used to display
    /// a list of scheduled transaction.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Scheduled Transaction List" )]
    [Category( "Mobile > Finance" )]
    [Description( "The Scheduled Transaction List block." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [BlockTemplateField(
        "Result Item Template",
        Description = "Lava template for rendering each result item. The Lava merge field will be 'Item'.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST,
        IsRequired = true,
        DefaultValue = "AE0A060A-EDC6-43B2-86B9-5FAA4C148CF0",
        Key = AttributeKey.ResultItemTemplate,
        Order = 0 )]

    [BooleanField(
        "Include Inactive",
        Description = "Indicates whether to dispaly inactive scheduled transactions.",
        IsRequired = false,
        Key = AttributeKey.IncludeInactive,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a Scheduled Transaction List. ScheduledTransactionGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    [AccountsField(
        "Accounts",
        Key = AttributeKey.Accounts,
        Description = "A selection of accounts to use for checking if transactions for the current user exist.",
        IsRequired = false,
        Order = 3 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_FINANCE_SCHEDULED_TRANSACTION_LIST )]
    public class ScheduledTransactionList : RockBlockType
    {
        #region Constants

        /// <summary>
        /// Gets the Scheduled Transaction Template.
        /// </summary>
        private string ScheduledTransactionItemTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.ResultItemTemplate ) );

        /// <summary>
        /// Gets the Detail Page Guid.
        /// </summary>
        private Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the value determine whether to include the inactive scheduled transaction or not.
        /// </summary>
        private bool IncludeInactive => GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean();

        #endregion

        #region Attribute Keys

        /// <summary>
        /// The block setting attribute keys.
        /// </summary>
        private static class AttributeKey
        {
            public const string HistoricalResultItemTemplate = "HistoricalResultItemTemplate";
            public const string Accounts = "Accounts";
            public const string ResultItemTemplate = "ResultItemTemplate";
            public const string DetailPage = "DetailPageGuid";
            public const string IncludeInactive = "IncludeInactive";
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Get the scheduled transactions list of the current person.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction( "GetScheduledTransactions" )]
        public BlockActionResult GetScheduledTransactions( GetScheduledTransactionOptions options )
        {
            var person = GetCurrentPerson();
            if ( person == null )
            {
                return ActionOk( new { Transactions = new List<FinancialScheduledTransactionBag>(), HasInactiveTransactions = false } );
            }

            var givingGroupId = person.GivingGroupId;

            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var qry = financialScheduledTransactionService.Queryable();

            if ( givingGroupId.HasValue )
            {
                // Person contributes with family
                qry = qry.Where( t => t.AuthorizedPersonAlias.Person.GivingGroupId == givingGroupId );
            }
            else
            {
                // Person contributes individually
                qry = qry.Where( t => t.AuthorizedPersonAlias.PersonId == person.Id );
            }

            var accountGuids = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().AsGuidList();
            if ( accountGuids.Any() )
            {
                qry = qry.Where( t => t.ScheduledTransactionDetails.Any( d => accountGuids.Contains( d.Account.Guid ) ) );
            }

            if ( !options.IncludeInactive )
            {
                qry = qry.Where( t => t.IsActive );
            }

            qry = qry.OrderByDescending( t => t.NextPaymentDate );
            var scheduledTransactions = qry.ToList();

            financialScheduledTransactionService.GetStatus( scheduledTransactions, true );

            var transactionBags = scheduledTransactions.Select( st => new FinancialScheduledTransactionBag
            {
                TotalAmount = st.TotalAmount,
                NextPaymentDate = st.NextPaymentDate,
                FrequencyText = st.TransactionFrequencyValue?.Value,
                IsActive = st.IsActive,
            } ).ToList();

            var templates = transactionBags.Select( ( bag ) =>
            {
                var mergeFields = RequestContext.GetCommonMergeFields();
                var template = ScheduledTransactionItemTemplate;

                mergeFields.Add( "ScheduledTransactionInfo", new Lava.LavaDataWrapper( bag ) );
                mergeFields.Add( "DetailPage", DetailPageGuid );

                return new ListItemViewModel
                {
                    Text = bag.Guid.ToString(),
                    Value = template.ResolveMergeFields( mergeFields ),
                };
            } );

            return ActionOk( new GetScheduledTransactionsResponseBag
            {
                ScheduledTransactionItems = templates.ToList(),
            } );
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Finance.ScheduledTransactionList.Configuration
            {
                IncludeInactive = IncludeInactive,
            };
        }

        #endregion
    }
}
