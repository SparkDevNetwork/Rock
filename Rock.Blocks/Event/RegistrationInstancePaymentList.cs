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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstancePaymentList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the payments related to an event registration instance.
    /// </summary>
    [DisplayName( "Registration Instance - Payment List" )]
    [Category( "Event" )]
    [Description( "Displays the payments related to an event registration instance." )]
    [IconCssClass( "ti ti-credit-card" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Transaction Detail Page",
        Description = "The page for viewing details about a payment",
        Key = AttributeKey.DetailPage,
        DefaultValue = Rock.SystemGuid.Page.TRANSACTION_DETAIL_TRANSACTIONS,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Registration Page",
        Description = "The page for editing registration and registrant information",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 2 )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "3842853c-75b2-4568-8397-2b9e4409fd44" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "e804f6b4-e4c2-47e5-b1de-2147222bf3a2" )]
    [Rock.SystemGuid.BlockTypeGuid( "762BEE39-15DF-477C-9831-DB5AA73DCB24" )]
    public class RegistrationInstancePaymentList : RockEntityListBlockType<FinancialTransaction>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "TransactionDetailPage";
            public const string RegistrationPage = "RegistrationPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationId = "RegistrationId";
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _registrationInstance;
        private List<Registration> _paymentRegistrations;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstancePaymentListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RegistrationInstancePaymentListOptionsBag GetBoxOptions()
        {
            var registrationInstance = GetRegistrationInstance();
            var currencyInfo = new RockCurrencyCodeInfo();
            var currencyInfoBag = new ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };

            var options = new RegistrationInstancePaymentListOptionsBag()
            {
                CurrencyInfo = currencyInfoBag
            };

            if ( registrationInstance != null )
            {
                options.ExportFileName = $"{registrationInstance.Name}RegistrationPayments";
                options.ExportTitle = $"{registrationInstance.Name} - Registration Payments";
            }

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "TransactionId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransaction> GetListQueryable( RockContext rockContext )
        {
            var registrationInstance = GetRegistrationInstance();
            IEnumerable<FinancialTransaction> qry = new List<FinancialTransaction>();

            if ( registrationInstance?.Id != null )
            {
                // If configured for a registration and registration is null, return
                int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;

                // Get all the registrations for this instance
                var paymentRegistrations = GetPaymentRegistrations();

                // Get the Registration Ids
                var registrationIds = paymentRegistrations.ConvertAll( r => r.Id );

                // Get all the transactions relate to these registrations.
                // Eager-load the navigation properties the grid reads per row
                // ( authorized person, payment detail, transaction details ) so they
                // are not lazy-loaded one row at a time.
                qry = new FinancialTransactionService( RockContext )
                    .Queryable().AsNoTracking()
                    .Include( t => t.AuthorizedPersonAlias.Person )
                    .Include( t => t.FinancialPaymentDetail.CurrencyTypeValue )
                    .Include( t => t.FinancialPaymentDetail.CreditCardTypeValue )
                    .Include( t => t.TransactionDetails )
                    .Where( t => t.TransactionDetails
                        .Any( d =>
                            d.EntityTypeId.HasValue &&
                            d.EntityTypeId.Value == registrationEntityTypeId &&
                            d.EntityId.HasValue &&
                            registrationIds.Contains( d.EntityId.Value ) ) );
            }

            return qry.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<FinancialTransaction> GetOrderedListQueryable( IQueryable<FinancialTransaction> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( t => t.TransactionDateTime ).ThenByDescending( t => t.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<FinancialTransaction> GetGridBuilder()
        {
            return new GridBuilder<FinancialTransaction>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddPersonField( "person", a => a.AuthorizedPersonAlias?.Person )
                .AddDateTimeField( "transactionDateTime", a => a.TransactionDateTime )
                .AddField( "totalAmount", a => a.TotalAmount )
                .AddTextField( "paymentMethod", a => a.FinancialPaymentDetail?.CurrencyAndCreditCardType )
                .AddTextField( "account", a => a.FinancialPaymentDetail?.AccountNumberMasked )
                .AddTextField( "transactionCode", a => a.TransactionCode )
                .AddField( "registrarsHtml", a => GetRegistrarsHtml( a ) )
                .AddField( "registrars", a => GetRegistrars( a ) )
                .AddField( "registrants", a => GetRegistrants( a ) );
        }

        /// <summary>
        /// Gets the registrants.
        /// </summary>
        /// <param name="transaction">a.</param>
        /// <returns></returns>
        private List<string> GetRegistrants( FinancialTransaction transaction )
        {
            var registrants = new List<string>();

            var registrationIds = transaction.TransactionDetails.Select( d => d.EntityId ).ToList();
            foreach ( var registration in _paymentRegistrations
                .Where( r => registrationIds.Contains( r.Id ) ) )
            {
                if ( registration.PersonAlias?.Person != null )
                {
                    registrants.AddRange( registration.Registrants
                        .Where( registrant => registrant.PersonAlias?.Person != null )
                        .Select( registrant => registrant.PersonAlias.Person.FullName ) );
                }
            }

            return registrants;
        }

        /// <summary>
        /// Gets the registrars html.
        /// </summary>
        /// <param name="transaction">a.</param>
        /// <returns></returns>
        private List<string> GetRegistrarsHtml( FinancialTransaction transaction )
        {
            var registrars = new List<string>();

            var registrationIds = transaction.TransactionDetails.Select( d => d.EntityId ).ToList();
            foreach ( var registration in _paymentRegistrations
                .Where( r => registrationIds.Contains( r.Id ) ) )
            {
                if ( registration.PersonAlias?.Person != null )
                {
                    var qryParams = new Dictionary<string, string>
                    {
                        { PageParameterKey.RegistrationId, registration.IdKey }
                    };
                    string url = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, qryParams );
                    registrars.Add( string.Format( "<a href='{0}'>{1}</a>", url, registration.PersonAlias.Person.FullName ) );
                }
            }

            return registrars;
        }

        /// <summary>
        /// Gets the registrars.
        /// </summary>
        /// <param name="transaction">a.</param>
        /// <returns></returns>
        private List<string> GetRegistrars( FinancialTransaction transaction )
        {
            var registrationIds = transaction.TransactionDetails.Select( d => d.EntityId ).ToList();

            var registrars = _paymentRegistrations
                .Where( r => registrationIds.Contains( r.Id ) )
                .Where( registration => registration.PersonAlias?.Person != null )
                .Select( registration => registration.PersonAlias.Person.FullName )
                .ToList();

            return registrars;
        }

        /// <summary>
        /// Gets the registration instance from the RegistrationInstanceId page
        /// parameter, accepting an Id, IdKey, or Guid. The result is cached so
        /// repeat calls within a single block request only hit the database once.
        /// </summary>
        /// <returns>The registration instance, or null if the parameter was missing or did not resolve.</returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            if ( _registrationInstance == null )
            {
                var registrationInstanceKey = PageParameter( PageParameterKey.RegistrationInstanceId );

                if ( registrationInstanceKey.IsNotNullOrWhiteSpace() )
                {
                    _registrationInstance = new RegistrationInstanceService( RockContext )
                        .Get( registrationInstanceKey, !PageCache.Layout.Site.DisablePredictableIds );
                }
            }

            return _registrationInstance;
        }

        /// <summary>
        /// Gets the payment registrations.
        /// </summary>
        /// <returns></returns>
        private List<Registration> GetPaymentRegistrations()
        {
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return _paymentRegistrations;
            }

            if ( _paymentRegistrations == null )
            {
                _paymentRegistrations = new RegistrationService( RockContext )
                    .Queryable( "PersonAlias.Person,Registrants.PersonAlias.Person" ).AsNoTracking()
                    .Where( r =>
                        r.RegistrationInstanceId == registrationInstance.Id
                        && !r.IsTemporary )
                    .ToList();
            }

            return _paymentRegistrations;
        }

        #endregion
    }
}
