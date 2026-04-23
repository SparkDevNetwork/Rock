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
using Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrationList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays the list of Registrations related to a Registration Instance.
    /// </summary>
    [DisplayName( "Registration Instance - Registration List" )]
    [Category( "Event" )]
    [Description( "Displays the list of Registrations related to a Registration Instance." )]
    [IconCssClass( "ti ti-user" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Registration Page",
        Description = "The page for editing registration and registrant information.",
        Key = AttributeKey.RegistrationPage,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_DETAIL,
        IsRequired = false,
        Order = 1 )]

    [BooleanField(
        "Display Discount Codes",
        Description = "Display the discount code used with a payment.",
        Key = AttributeKey.DisplayDiscountCodes,
        DefaultBooleanValue = false,
        Order = 2 )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "3483E8EA-4DD8-4A0E-B71C-D410A019450C" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D40ACB30-15FF-45CC-89AE-AC779784C1B2" )]
    [Rock.SystemGuid.BlockTypeGuid( "A8DB2C89-F80A-43A2-AA53-36C78673F504" )]
    [CustomizedGrid]
    public class RegistrationInstanceRegistrationList : RockEntityListBlockType<Registration>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string RegistrationPage = "RegistrationPage";
            public const string DisplayDiscountCodes = "DisplayDiscountCodes";
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
            public const string FilterDateRange = "filter-date-range";
            public const string FilterPaymentStatus = "filter-payment-status";
            public const string FilterRegisteredBy = "filter-registered-by";
            public const string FilterCampuses = "filter-campuses";
        }

        #endregion Keys

        #region Fields

        private RegistrationInstance _registrationInstance;
        private Dictionary<int, decimal> _registrationPayments = new Dictionary<int, decimal>();
        private bool? _instanceHasCost;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current person's block-scoped preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => GetBlockPersonPreferences();

        /// <summary>
        /// Gets the registration date range filter from person preferences.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterDateRange ) )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the payment status filter from person preferences.
        /// </summary>
        private RegistrationPaymentStatus? FilterPaymentStatus => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterPaymentStatus ) )
            .ConvertToEnumOrNull<RegistrationPaymentStatus>();

        /// <summary>
        /// Gets the "Registered By" person filter from person preferences.
        /// </summary>
        private ListItemBag FilterRegisteredBy => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterRegisteredBy ) )
            .FromJsonOrNull<ListItemBag>();

        /// <summary>
        /// Gets the campus guids filter from person preferences.
        /// </summary>
        private List<Guid> FilterCampuses => BlockPersonPreferences
            .GetValue( MakeKeyUniqueToRegistrationTemplate( PreferenceKey.FilterCampuses ) )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        /// <summary>
        /// Gets a value indicating whether the current person is allowed to
        /// edit this block's content. True when the user has block EDIT,
        /// instance EDIT, or instance ADMINISTRATE.
        /// </summary>
        private bool UserCanEditBlockContent
        {
            get
            {
                var registrationInstance = GetRegistrationInstance();

                if ( registrationInstance == null )
                {
                    return false;
                }

                var currentPerson = RequestContext.CurrentPerson;

                return BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                    || registrationInstance.IsAuthorized( Authorization.EDIT, currentPerson )
                    || registrationInstance.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceRegistrationListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private RegistrationInstanceRegistrationListOptionsBag GetBoxOptions()
        {
            var registrationInstance = GetRegistrationInstance();
            var currencyInfo = new RockCurrencyCodeInfo();

            var options = new RegistrationInstanceRegistrationListOptionsBag
            {
                CampusItems = CampusCache.All().ToListItemBagList(),
                DisplayDiscountCodes = GetAttributeValue( AttributeKey.DisplayDiscountCodes ).AsBoolean(),
                InstanceHasCost = GetInstanceHasCost(),
                CurrencyInfo = new CurrencyInfoBag
                {
                    Symbol = currencyInfo.Symbol,
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    SymbolLocation = currencyInfo.SymbolLocation
                }
            };

            if ( registrationInstance != null )
            {
                options.ExportTitle = $"{registrationInstance.Name} — Registrations";
                options.RegistrationTemplateGuid = registrationInstance.RegistrationTemplate?.Guid;
            }

            return options;
        }

        /// <summary>
        /// Determines whether the add and delete actions should be enabled at
        /// the block level. True when the user can edit the block content or
        /// has "Register" authorization on the registration instance.
        /// </summary>
        /// <returns>A boolean value indicating whether add/delete should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return UserCanEditBlockContent
                || GetRegistrationInstance()?.IsAuthorized( Authorization.REGISTER, RequestContext.CurrentPerson ) == true;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var registrationInstance = GetRegistrationInstance();

            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.RegistrationId, "((Key))" },
                { PageParameterKey.RegistrationInstanceId, registrationInstance?.IdKey }
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.RegistrationPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Registration> GetListQueryable( RockContext rockContext )
        {
            var registrationInstance = GetRegistrationInstance();

            if ( registrationInstance == null )
            {
                return Enumerable.Empty<Registration>().AsQueryable();
            }

            var qry = new RegistrationService( rockContext ).Queryable()
                .Include( r => r.PersonAlias.Person )
                .Include( r => r.Registrants.Select( rg => rg.PersonAlias.Person ) )
                .Include( r => r.Registrants.Select( rg => rg.Fees.Select( f => f.RegistrationTemplateFee ) ) )
                .Include( r => r.Campus )
                .Include( r => r.PaymentPlanFinancialScheduledTransaction )
                .AsNoTracking()
                .Where( r => r.RegistrationInstanceId == registrationInstance.Id && !r.IsTemporary );

            // Apply the registration date range filter.
            var dateRange = FilterDateRange?.ToActualDateRange();

            if ( dateRange?.Start != null )
            {
                var start = dateRange.Start.Value;
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime.Value >= start );
            }

            if ( dateRange?.End != null )
            {
                var end = dateRange.End.Value;
                qry = qry.Where( r => r.CreatedDateTime.HasValue && r.CreatedDateTime.Value < end );
            }

            // Apply the "Registered By" person filter.
            var registeredByAliasGuid = FilterRegisteredBy?.Value.AsGuidOrNull();

            if ( registeredByAliasGuid.HasValue )
            {
                var filteredPersonId = new PersonAliasService( rockContext ).GetPersonId( registeredByAliasGuid.Value );

                if ( filteredPersonId.HasValue )
                {
                    var personId = filteredPersonId.Value;
                    qry = qry.Where( r => r.PersonAlias != null && r.PersonAlias.PersonId == personId );
                }
                else
                {
                    // The selected person alias could not be resolved — no rows should match.
                    qry = qry.Where( r => false );
                }
            }

            // Apply the campus filter.
            var campusGuids = FilterCampuses;

            if ( campusGuids.Any() )
            {
                var campusIds = campusGuids
                    .Select( guid => CampusCache.Get( guid )?.Id )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                if ( campusIds.Any() )
                {
                    qry = qry.Where( r => r.CampusId.HasValue && campusIds.Contains( r.CampusId.Value ) );
                }
            }

            /*
                4/21/2026 - MSE

                The payment-status filter runs in GetListItems. DiscountedCost
                branches on OnWaitList / DiscountApplies / per-fee rules that
                don't translate easily into SQL, so we filter after materialization.
            */

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<Registration> GetOrderedListQueryable( IQueryable<Registration> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( r => r.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override List<Registration> GetListItems( IQueryable<Registration> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            if ( items.Count == 0 )
            {
                return items;
            }

            var registrationEntityTypeId = EntityTypeCache.Get<Registration>( false )?.Id;

            if ( !registrationEntityTypeId.HasValue )
            {
                return items;
            }

            /*
                4/21/2026 - MSE

                Payments are loaded once here so the Balance Due column and
                the payment-status filter below read from the same dictionary
                without an N+1 per row.
            */
            var registrationIds = items.ConvertAll( r => r.Id );

            _registrationPayments = new FinancialTransactionDetailService( rockContext )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.EntityTypeId.HasValue
                    && d.EntityId.HasValue
                    && d.EntityTypeId.Value == registrationEntityTypeId.Value
                    && registrationIds.Contains( d.EntityId.Value ) )
                .GroupBy( d => d.EntityId.Value )
                .ToDictionary( g => g.Key, g => g.Sum( d => d.Amount ) );

            // Apply the payment-status filter.
            var paymentStatus = FilterPaymentStatus;

            if ( paymentStatus.HasValue )
            {
                items = items.Where( r =>
                {
                    var cost = r.Registrants.Sum( registrant => ( decimal? ) registrant.DiscountedCost( r.DiscountPercentage, r.DiscountAmount ) ) ?? 0.0m;
                    var paid = _registrationPayments.GetValueOrDefault( r.Id, 0m );

                    return paymentStatus.Value == RegistrationPaymentStatus.PaidInFull
                        ? cost <= paid
                        : cost > paid;
                } ).ToList();
            }

            return items;
        }

        /// <inheritdoc/>
        protected override List<AttributeCache> BuildGridAttributes()
        {
            var registrationInstance = GetRegistrationInstance();
            var entityTypeId = EntityTypeCache.Get<Registration>( false )?.Id;

            if ( registrationInstance == null || !entityTypeId.HasValue )
            {
                return new List<AttributeCache>();
            }

            var templateId = registrationInstance.RegistrationTemplateId;
            var currentPerson = RequestContext.CurrentPerson;

            return AttributeCache
                .GetByEntityTypeQualifier( entityTypeId, "RegistrationTemplateId", templateId.ToString(), false )
                .Where( a => a.IsGridColumn )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ThenBy( a => a.Id )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<Registration> GetGridBuilder()
        {
            return new GridBuilder<Registration>()
                .WithBlock( this )
                .AddTextField( "idKey", r => r.IdKey )
                .AddTextField( "registeredBy", r => GetRegisteredByName( r ) )
                .AddTextField( "personIdKey", r => r.PersonAlias?.Person?.IdKey )
                .AddTextField( "campus", r => r.Campus?.Name )
                .AddTextField( "confirmationEmail", r => r.ConfirmationEmail )
                .AddField( "registrants", BuildRegistrantList )
                .AddDateTimeField( "createdDateTime", r => r.CreatedDateTime )
                .AddTextField( "discountCode", r => r.DiscountCode )
                .AddField( "totalCost", r => r.DiscountedCost )
                .AddField( "balanceDue", r => r.DiscountedCost - _registrationPayments.GetValueOrDefault( r.Id, 0m ) )
                .AddField( "hasActivePaymentPlan", r => r.IsPaymentPlanActive )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the display name of the person who registered. Prefers the
        /// linked Person's reversed full name and falls back to the LastName /
        /// FirstName values stored directly on the registration.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The display name for the "Registered By" column.</returns>
        private static string GetRegisteredByName( Registration registration )
        {
            var person = registration.PersonAlias?.Person;

            if ( person != null )
            {
                return person.FullNameReversed;
            }

            return $"{registration.LastName}, {registration.FirstName}";
        }

        /// <summary>
        /// Builds the list of registrants that will be displayed in the
        /// Registrants column. Registrants are sorted by nickname / last name
        /// and flagged when on the wait list.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>The ordered list of registrant entries.</returns>
        private static List<RegistrantInfoBag> BuildRegistrantList( Registration registration )
        {
            if ( registration.Registrants == null )
            {
                return new List<RegistrantInfoBag>();
            }

            return registration.Registrants
                .Where( registrant => registrant.PersonAlias?.Person != null )
                .OrderBy( registrant => registrant.PersonAlias.Person.NickName )
                .ThenBy( registrant => registrant.PersonAlias.Person.LastName )
                .Select( registrant => new RegistrantInfoBag
                {
                    DisplayName = $"{registrant.PersonAlias.Person.NickName} {registrant.PersonAlias.Person.LastName}",
                    IsOnWaitList = registrant.OnWaitList
                } )
                .ToList();
        }

        /// <summary>
        /// Determines if the specified registration may be deleted by the
        /// current person. True when the user has block EDIT, "Register" on
        /// the registration, or EDIT / ADMINISTRATE on the registration
        /// itself. Instance-level EDIT / ADMINISTRATE is intentionally NOT a
        /// grant here — it gates the add/delete button visibility
        /// (<see cref="UserCanEditBlockContent"/>) but not per-row delete
        /// actions.
        /// </summary>
        /// <param name="registration">The registration to check.</param>
        /// <returns><c>true</c> if the registration may be deleted; otherwise, <c>false</c>.</returns>
        private bool CanDeleteRegistration( Registration registration )
        {
            if ( registration == null )
            {
                return false;
            }

            var currentPerson = RequestContext.CurrentPerson;

            return BlockCache.IsAuthorized( Authorization.EDIT, currentPerson )
                || registration.IsAuthorized( Authorization.REGISTER, currentPerson )
                || registration.IsAuthorized( Authorization.EDIT, currentPerson )
                || registration.IsAuthorized( Authorization.ADMINISTRATE, currentPerson );
        }

        /// <summary>
        /// Determines whether the registration instance (or its template)
        /// specifies a non-zero cost. Used to toggle visibility of the
        /// Total Cost and Balance Due columns.
        /// </summary>
        /// <returns><c>true</c> if the instance has a cost; otherwise, <c>false</c>.</returns>
        private bool GetInstanceHasCost()
        {
            if ( _instanceHasCost.HasValue )
            {
                return _instanceHasCost.Value;
            }

            var registrationInstance = GetRegistrationInstance();
            var template = registrationInstance?.RegistrationTemplate;

            if ( template == null )
            {
                _instanceHasCost = false;
                return false;
            }

            var cost = template.Cost;

            if ( template.SetCostOnInstance == true )
            {
                cost = registrationInstance.Cost ?? 0.0m;
            }

            _instanceHasCost = cost > 0.0m;
            return _instanceHasCost.Value;
        }

        /// <summary>
        /// Gets the registration instance from the RegistrationInstanceId page
        /// parameter, accepting an Id, IdKey, or Guid. The result is cached so
        /// repeat calls within a single block request only hit the database
        /// once. The RegistrationTemplate is eagerly loaded so it can be used
        /// for cost/attribute lookups.
        /// </summary>
        /// <returns>The registration instance, or null if the parameter is missing or does not resolve.</returns>
        private RegistrationInstance GetRegistrationInstance()
        {
            if ( _registrationInstance != null )
            {
                return _registrationInstance;
            }

            var registrationInstanceKey = PageParameter( PageParameterKey.RegistrationInstanceId );

            if ( registrationInstanceKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            _registrationInstance = new RegistrationInstanceService( RockContext )
                .GetQueryableByKey( registrationInstanceKey, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( a => a.RegistrationTemplate )
                .AsNoTracking()
                .FirstOrDefault();

            return _registrationInstance;
        }

        /// <summary>
        /// Returns the given preference key scoped to the current registration
        /// template's Guid so that switching between templates does not leak
        /// filter state. Falls back to the raw key if the template Guid is not
        /// available.
        /// </summary>
        /// <param name="key">The preference key.</param>
        /// <returns>The scoped preference key.</returns>
        private string MakeKeyUniqueToRegistrationTemplate( string key )
        {
            var templateGuid = GetRegistrationInstance()?.RegistrationTemplate?.Guid;

            if ( templateGuid.HasValue )
            {
                return $"{templateGuid.Value}-{key}";
            }

            return key;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified registration. The payment plan (if active) is
        /// deactivated in the same unit of work so the database never has a
        /// cancelled plan referencing a deleted registration.
        /// </summary>
        /// <param name="key">The identifier of the registration to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var registrationService = new RegistrationService( RockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );

            var registration = registrationService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( registration == null )
            {
                return ActionBadRequest( $"{Registration.FriendlyTypeName} not found." );
            }

            if ( !CanDeleteRegistration( registration ) )
            {
                return ActionBadRequest( "You are not authorized to delete this registration." );
            }

            if ( !registrationService.CanDelete( registration, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( !registrationService.TryCancelPaymentPlan( registration, financialScheduledTransactionService, out var cancelError, out var cancelWarning ) )
            {
                return ActionBadRequest( cancelError ?? "An unknown error occurred while deactivating a payment plan. The registration was not cancelled." );
            }

            if ( cancelWarning.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( cancelWarning );
            }

            /*
                4/21/2026 - MSE

                SaveChanges runs here — after TryCancelPaymentPlan has marked
                the payment plan cancelled in-memory, but before deleting the
                registration — so that any error/warning from the cancel flow
                exits early without persisting a cancelled plan that still
                references a live registration record.

                Reason: Transactional safety for the payment-plan cancel + delete pair.
            */
            RockContext.SaveChanges();

            var changes = new History.HistoryChangeList();
            changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Registration" );

            RockContext.WrapTransaction( () =>
            {
                HistoryService.SaveChanges(
                    RockContext,
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    changes );

                registrationService.Delete( registration );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion
    }
}
