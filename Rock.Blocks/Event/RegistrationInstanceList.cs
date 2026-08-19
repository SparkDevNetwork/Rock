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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceList;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Displays a list of registration instances.
    /// </summary>
    [DisplayName( "Registration Instance List" )]
    [Category( "Event" )]
    [Description( "Displays a list of registration instances." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the registration instance details.",
        Key = AttributeKey.DetailPage )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "5cc98267-2b3c-45ef-9055-31db629d579b" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "051f65ad-9301-4d41-bd5e-d4e93f4dc438" )]
    [Rock.SystemGuid.BlockTypeGuid( "632F63A9-5629-4731-BE6A-AB534EDD9BC9" )]
    [CustomizedGrid]
    public class RegistrationInstanceList : RockEntityListBlockType<RegistrationInstance>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationTemplateId = "RegistrationTemplateId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Keys

        #region Fields

        private RegistrationTemplate _registrationTemplate;

        /// <summary>
        /// Non-wait-list registrant counts keyed by registration instance Id, populated once per grid load in <see cref="GetListItems"/>.
        /// </summary>
        private Dictionary<int, int> _registrantCounts = new Dictionary<int, int>();

        /// <summary>
        /// Wait-list registrant counts keyed by registration instance Id, populated once per grid load in <see cref="GetListItems"/>.
        /// </summary>
        private Dictionary<int, int> _waitListCounts = new Dictionary<int, int>();

        /// <summary>
        /// The set of registration instance Ids that have at least one active payment plan, populated once per grid load in <see cref="GetListItems"/>.
        /// </summary>
        private HashSet<int> _instancesWithActivePaymentPlan = new HashSet<int>();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RegistrationInstanceListOptionsBag>();

            var template = GetRegistrationTemplate();
            if ( template != null && !template.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( RegistrationInstance.FriendlyTypeName );
                return box;
            }

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
        private RegistrationInstanceListOptionsBag GetBoxOptions()
        {
            var options = new RegistrationInstanceListOptionsBag
            {
                TitleIconCssClass = "ti ti-file"
            };

            var template = GetRegistrationTemplate();
            if ( template == null )
            {
                return options;
            }

            options.IsVisible = true;
            options.RegistrationInstanceName = template.Name;
            options.ShowWaitList = template.WaitListEnabled;
            options.ExportTitle = template.Name;

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            var template = GetRegistrationTemplate();
            if ( template == null )
            {
                return false;
            }

            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || template.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string>
                {
                    [PageParameterKey.RegistrationInstanceId] = "((Key))",
                    [PageParameterKey.RegistrationTemplateId] = PageParameter( PageParameterKey.RegistrationTemplateId ) ?? string.Empty
                } )
            };
        }

        /// <summary>
        /// Gets the registration template from the RegistrationTemplateId page
        /// parameter, accepting an Id, IdKey, or Guid. The result is cached so
        /// repeat calls within a single block request only hit the database once.
        /// </summary>
        /// <returns>The registration template, or null if the parameter was missing or did not resolve.</returns>
        private RegistrationTemplate GetRegistrationTemplate()
        {
            if ( _registrationTemplate != null )
            {
                return _registrationTemplate;
            }

            var templateKey = PageParameter( PageParameterKey.RegistrationTemplateId );
            if ( string.IsNullOrWhiteSpace( templateKey ) )
            {
                return null;
            }

            _registrationTemplate = new RegistrationTemplateService( RockContext )
                .Get( templateKey, !PageCache.Layout.Site.DisablePredictableIds );

            return _registrationTemplate;
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetListQueryable( RockContext rockContext )
        {
            var template = GetRegistrationTemplate();
            if ( template == null )
            {
                return Enumerable.Empty<RegistrationInstance>().AsQueryable();
            }

            return base.GetListQueryable( rockContext ).Where( i => i.RegistrationTemplateId == template.Id );
        }

        /// <inheritdoc/>
        protected override IQueryable<RegistrationInstance> GetOrderedListQueryable( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.StartDateTime );
        }

        /// <inheritdoc/>
        protected override List<RegistrationInstance> GetListItems( IQueryable<RegistrationInstance> queryable, RockContext rockContext )
        {
            var items = base.GetListItems( queryable, RockContext );

            if ( items.Count == 0 )
            {
                return items;
            }

            var instanceIds = items.ConvertAll( i => i.Id );

            // Registrant and wait-list counts, grouped in SQL so each is one query, not one per row.
            var counts = new RegistrationRegistrantService( RockContext ).Queryable().AsNoTracking()
                .Where( rr => !rr.Registration.IsTemporary && instanceIds.Contains( rr.Registration.RegistrationInstanceId ) )
                .GroupBy( rr => new { rr.Registration.RegistrationInstanceId, rr.OnWaitList } )
                .Select( g => new { g.Key.RegistrationInstanceId, g.Key.OnWaitList, Count = g.Count() } )
                .ToList();

            _registrantCounts = counts.Where( c => !c.OnWaitList ).ToDictionary( c => c.RegistrationInstanceId, c => c.Count );
            _waitListCounts = counts.Where( c => c.OnWaitList ).ToDictionary( c => c.RegistrationInstanceId, c => c.Count );

            _instancesWithActivePaymentPlan = new HashSet<int>( new RegistrationService( RockContext ).Queryable().AsNoTracking()
                .Where( r => instanceIds.Contains( r.RegistrationInstanceId )
                    && r.PaymentPlanFinancialScheduledTransaction != null
                    && r.PaymentPlanFinancialScheduledTransaction.IsActive )
                .Select( r => r.RegistrationInstanceId )
                .Distinct()
                .ToList() );

            return items;
        }

        /// <inheritdoc/>
        protected override GridBuilder<RegistrationInstance> GetGridBuilder()
        {
            return new GridBuilder<RegistrationInstance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "startDate", a => a.StartDateTime )
                .AddDateTimeField( "endDate", a => a.EndDateTime )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "registrants", a => _registrantCounts.GetValueOrDefault( a.Id, 0 ) )
                .AddField( "waitList", a => _waitListCounts.GetValueOrDefault( a.Id, 0 ) )
                .AddField( "hasPaymentPlans", a => _instancesWithActivePaymentPlan.Contains( a.Id ) )
                .AddAttributeFields( GetGridAttributes() );
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
            var entityService = new RegistrationInstanceService( RockContext );
            var registrationInstance = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( registrationInstance == null )
            {
                return ActionBadRequest( $"{RegistrationInstance.FriendlyTypeName} not found." );
            }

            if ( !entityService.CanDelete( registrationInstance, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var registrationService = new RegistrationService( RockContext );
            var financialScheduledTransactionService = new FinancialScheduledTransactionService( RockContext );
            var errors = new List<string>();
            var warnings = new List<string>();

            foreach ( var registration in registrationInstance.Registrations.ToList() )
            {
                var success = registrationService.TryCancelPaymentPlan( registration, financialScheduledTransactionService, out var error, out var warning );
                string registrationInfo = $"Registration Id {registration.Id} ({registration.FirstName} {registration.LastName})";
                if ( !success )
                {
                    errors.Add( $"{registrationInfo}: {error ?? "Unknown error"}" );
                }
                if ( !string.IsNullOrWhiteSpace( warning ) )
                {
                    warnings.Add( $"{registrationInfo}: {warning}" );
                }
            }

            if ( errors.Any() )
            {
                return ActionBadRequest( "The following registrations could not have their payment plans canceled:\n" + string.Join( "\n", errors ) );
            }
            if ( warnings.Any() )
            {
                return ActionBadRequest( "Warnings occurred for the following registrations:\n" + string.Join( "\n", warnings ) );
            }

            RockContext.SaveChanges();

            RockContext.WrapTransaction( () =>
            {
                registrationService.DeleteRange( registrationInstance.Registrations );
                entityService.Delete( registrationInstance );
                RockContext.SaveChanges();
            } );

            return ActionOk();
        }

        #endregion
    }
}
