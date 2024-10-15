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
using Rock.Security;
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
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Transaction Detail Page",
        "The page for viewing details about a payment",
        Key = AttributeKey.DetailPage,
        DefaultValue = Rock.SystemGuid.Page.TRANSACTION_DETAIL_TRANSACTIONS,
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Registration Page",
        "The page for editing registration and registrant information",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "3842853c-75b2-4568-8397-2b9e4409fd44" )]
    [Rock.SystemGuid.BlockTypeGuid( "e804f6b4-e4c2-47e5-b1de-2147222bf3a2" )]
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

        private static class PreferenceKey
        {
            public const string FilterPaymentDateRange = "filter-payment-date-range";
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
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
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
                RegistrationTemplateIdKey = registrationInstance?.RegistrationTemplate?.IdKey,
                ExportFileName = $"{registrationInstance?.Name} RegistrationPayments",
                ExportTitle = $"{registrationInstance?.Name} - Registration Payments",
                CurrencyInfo = currencyInfoBag
            };
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

                // Get all the transactions relate to these registrations
                qry = new FinancialTransactionService( RockContext )
                    .Queryable().AsNoTracking()
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
                .AddTextField( "id", a => a.Id.ToString() )
                .AddTextField( "person", a => a.AuthorizedPersonAlias?.Person?.FullNameReversed )
                .AddDateTimeField( "transactionDateTime", a => a.TransactionDateTime )
                .AddField( "totalAmount", a => a.TotalAmount )
                .AddTextField( "paymentMethod", a => a.FinancialPaymentDetail.CurrencyAndCreditCardType )
                .AddTextField( "account", a => a.FinancialPaymentDetail.AccountNumberMasked )
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
                        { PageParameterKey.RegistrationId, registration.Id.ToString() }
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
                .Select( registration => string.Format( registration.PersonAlias.Person.FullName ) )
                .ToList();

            return registrars;
        }

        /// <summary>
        /// Gets the registration instance.
        /// </summary>
        /// <returns></returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            if ( _registrationInstance == null )
            {
                var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

                if ( registrationInstanceId.HasValue )
                {
                    _registrationInstance = new RegistrationInstanceService( RockContext )
                        .Queryable()
                        .Include( a => a.RegistrationTemplate )
                        .Where( a => a.Id == registrationInstanceId.Value )
                        .AsNoTracking()
                        .FirstOrDefault();

                    if ( _registrationInstance == null )
                    {
                        return null;
                    }

                    // Load the Registration Template.
                    if ( _registrationInstance.RegistrationTemplate == null && _registrationInstance.RegistrationTemplateId > 0 )
                    {
                        _registrationInstance.RegistrationTemplate = new RegistrationTemplateService( RockContext ).Get( _registrationInstance.RegistrationTemplateId );
                    }
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

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new FinancialTransactionService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{FinancialTransaction.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {FinancialTransaction.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
