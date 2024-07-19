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
using Rock.ViewModels.Blocks.Finance.BenevolenceRequestList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a list of benevolence requests.
    /// </summary>
    [DisplayName( "Benevolence Request List" )]
    [Category( "Finance" )]
    [Description( "Block used to list Benevolence Requests." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the benevolence request details.",
        Key = AttributeKey.DetailPage )]

    [SecurityRoleField( "Case Worker Role",
        Description = "The security role to draw case workers from",
        IsRequired = true,
        DefaultValue = Rock.SystemGuid.Group.GROUP_BENEVOLENCE,
        Key = AttributeKey.CaseWorkerRole )]

    [LinkedPage(
        "Configuration Page",
        Description = "Page used to modify and create benevolence type.",
        IsRequired = true,
        Order = 2,
        DefaultValue = Rock.SystemGuid.Page.BENEVOLENCE_TYPES,
        Key = AttributeKey.ConfigurationPage )]

    [CustomCheckboxListField( "Hide Columns on Grid",
        Description = "The grid columns that should be hidden.",
        ListSource = "Assigned To, Government Id, Total Amount, Total Results",
        IsRequired = false,
        Order = 3,
        RepeatColumns = 3,
        Key = AttributeKey.HideColumnsAttributeKey )]

    [CustomCheckboxListField( "Include Benevolence Types",
        Description = "The benevolence types to display in the list.<br/><i>If none are selected, all types will be included.<i>",
        ListSource = FilterBenevolenceTypesSql,
        IsRequired = false,
        Order = 4,
        RepeatColumns = 3,
        Key = AttributeKey.FilterBenevolenceTypesAttributeKey )]

    [Rock.SystemGuid.EntityTypeGuid( "d1245f63-a9ba-4289-bd82-44a489f9da9a" )]
    [Rock.SystemGuid.BlockTypeGuid( "8adb5c0d-9a4f-4396-ab0f-deb552c094e1" )]
    [CustomizedGrid]
    [ContextAware( typeof( Person ) )]
    public class BenevolenceRequestList : RockEntityListBlockType<BenevolenceRequest>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ConfigurationPage = "ConfigurationPage";
            public const string HideColumnsAttributeKey = "HideColumnsAttributeKey";
            public const string FilterBenevolenceTypesAttributeKey = "FilterBenevolenceTypesAttributeKey";
            public const string DetailPage = "BenevolenceRequestDetail";
            public const string CaseWorkerRole = "CaseWorkerRole";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string ConfigurationPage = "ConfigurationPage";
        }

        private static class PreferenceKey
        {
            public const string FilterStartDateUpperValue = "filter-start-date-upper-value";

            public const string FilterStartDateLowerValue = "filter-start-date-lower-value";

            public const string FilterCampus = "filter-campus";

            public const string FilterFirstName = "filter-first-name";

            public const string FilterLastName = "filter-last-name";

            public const string FilterGovernmentId = "filter-government-id";

            public const string FilterCaseWorker = "filter-case-worker";

            public const string FilterResult = "filter-result";

            public const string FilterRequestStatus = "filter-request-status";

            public const string FilterBenevolenceTypes = "filter-benevolence-types";
        }

        #endregion Keys

        #region SQL Constants
        private const string FilterBenevolenceTypesSql = @"SELECT
                                                             bt.[Guid] AS [Value],
                                                             bt.[Name] AS [Text]
                                                           FROM BenevolenceType AS bt";
        #endregion SQL Constants

        #region Properties

        protected DateTime? FilterStartDateLowerValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStartDateLowerValue )
            .AsDateTime();

        protected DateTime? FilterStartDateUpperValue => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterStartDateUpperValue )
            .AsDateTime();

        protected Guid? FilterCampus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCampus )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected string FilterFirstName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterFirstName );

        protected string FilterLastName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterLastName );

        protected string FilterGovernmentId => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterGovernmentId );

        protected Guid? FilterCaseWorker => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCaseWorker )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected Guid? FilterResult => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterResult )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected Guid? FilterRequestStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterRequestStatus )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        protected List<Guid> FilterBenevolenceTypes => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterBenevolenceTypes )
            .FromJsonOrNull<List<Guid>>() ?? new List<Guid>();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<BenevolenceRequestListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
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
        private BenevolenceRequestListOptionsBag GetBoxOptions()
        {
            var options = new BenevolenceRequestListOptionsBag();

            var currencyInfo = new RockCurrencyCodeInfo();
            options.CurrencyInfo = new ViewModels.Utility.CurrencyInfoBag
            {
                Symbol = currencyInfo.Symbol,
                DecimalPlaces = currencyInfo.DecimalPlaces,
                SymbolLocation = currencyInfo.SymbolLocation
            };
            options.BenevolenceTypes = GetBenevolenceTypes();
            options.CaseWorkers = GetCaseWorkers();
            options.ColumnsToHide = GetAttributeValue( AttributeKey.HideColumnsAttributeKey ).Split( ',' ).ToList();
            options.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() );

            return options;
        }

        /// <summary>
        /// Gets the available case workers for the case worker filter dropdown.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetCaseWorkers()
        {
            var groupGuid = GetAttributeValue( AttributeKey.CaseWorkerRole ).AsGuid();
            return new GroupMemberService( new RockContext() ).Queryable( "Person" )
                .Where( gm => gm.Group.Guid == groupGuid )
                .Select( gm => gm.Person )
                .ToList()
                .ConvertAll( p => new ListItemBag() { Text = p.FullName, Value = p.Guid.ToString() } );
        }

        /// <summary>
        /// Gets the available benevolence types for the benevolence type filter dropdown.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetBenevolenceTypes()
        {
            return new BenevolenceTypeService( RockContext ).Queryable().Select( bt => new ListItemBag() { Text = bt.Name, Value = bt.Guid.ToString() } ).ToList();
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var targetPerson = RequestContext.GetContextEntity<Person>();
            var qryParams = new Dictionary<string, string>
            {
                { "BenevolenceRequestId", "((Key))" }
            };

            if ( targetPerson != null )
            {
                qryParams.Add( "PersonId", targetPerson.IdKey );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams ),
                [NavigationUrlKey.ConfigurationPage] = this.GetLinkedPageUrl( AttributeKey.ConfigurationPage ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<BenevolenceRequest> GetListQueryable( RockContext rockContext )
        {
            var benevolenceRequestService = new BenevolenceRequestService( rockContext );

            var benevolenceRequests = benevolenceRequestService
                .Queryable( "BenevolenceResults,RequestedByPersonAlias,RequestedByPersonAlias.Person,CaseWorkerPersonAlias,CaseWorkerPersonAlias.Person,RequestStatusValue,ConnectionStatusValue,Campus" ).AsNoTracking();

            var benevolenceTypeFilter = GetAttributeValue( AttributeKey.FilterBenevolenceTypesAttributeKey )?.Split( ',' ).Where( v => v.IsNotNullOrWhiteSpace() ).Select( v => new Guid( v ) );

            var targetPerson = RequestContext.GetContextEntity<Person>();

            // Filter by Start Date
            if ( FilterStartDateLowerValue != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestDateTime >= FilterStartDateLowerValue );
            }

            // Filter by End Date
            if ( FilterStartDateUpperValue != null )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestDateTime <= FilterStartDateUpperValue );
            }

            // Filter by Campus
            if ( FilterCampus.HasValue )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.Campus.Guid == FilterCampus.Value );
            }

            if ( targetPerson != null )
            {
                // show benevolence request for the target person and also for their family members
                var qryFamilyMembers = targetPerson.GetFamilyMembers( true, rockContext );
                benevolenceRequests = benevolenceRequests.Where( a => a.RequestedByPersonAliasId.HasValue && qryFamilyMembers.Any( b => b.PersonId == a.RequestedByPersonAlias.PersonId ) );
            }
            else
            {
                // Filter by First Name 
                if ( !string.IsNullOrWhiteSpace( FilterFirstName ) )
                {
                    benevolenceRequests = benevolenceRequests.Where( b => b.FirstName.StartsWith( FilterFirstName ) );
                }

                // Filter by Last Name 
                if ( !string.IsNullOrWhiteSpace( FilterLastName ) )
                {
                    benevolenceRequests = benevolenceRequests.Where( b => b.LastName.StartsWith( FilterLastName ) );
                }
            }

            // Filter by Government Id
            if ( !string.IsNullOrWhiteSpace( FilterGovernmentId ) )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.GovernmentId.StartsWith( FilterGovernmentId ) );
            }

            // Filter by Case Worker
            if ( FilterCaseWorker.HasValue )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.CaseWorkerPersonAlias.Guid == FilterCaseWorker.Value );
            }

            // Filter by Result
            if ( FilterResult.HasValue )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.BenevolenceResults.Any( r => r.ResultTypeValue.Guid == FilterResult.Value ) );
            }

            // Filter by Request Status
            if ( FilterRequestStatus.HasValue )
            {
                benevolenceRequests = benevolenceRequests.Where( b => b.RequestStatusValue.Guid == FilterRequestStatus.Value );
            }

            // Filter by Benevolence Types
            if ( FilterBenevolenceTypes.Count > 0 )
            {
                benevolenceRequests = benevolenceRequests.Where( b => FilterBenevolenceTypes.Contains( b.BenevolenceType.Guid ) );
            }

            if ( benevolenceTypeFilter?.Count() > 0 )
            {
                benevolenceRequests = benevolenceRequests.Where( b => benevolenceTypeFilter.Contains( b.BenevolenceType.Guid ) );
            }

            return benevolenceRequests;
        }

        /// <inheritdoc/>
        protected override IQueryable<BenevolenceRequest> GetOrderedListQueryable( IQueryable<BenevolenceRequest> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.RequestDateTime ).ThenByDescending( a => a.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<BenevolenceRequest> GetGridBuilder()
        {
            return new GridBuilder<BenevolenceRequest>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "date", a => a.RequestDateTime )
                .AddTextField( "campus", a => a.Campus?.Name )
                .AddTextField( "connectionStatus", a => a.ConnectionStatusValue?.Value )
                .AddTextField( "governmentId", a => a.GovernmentId )
                .AddTextField( "requestText", a => a.RequestText )
                .AddPersonField( "requestedBy", a => a.RequestedByPersonAlias?.Person )
                .AddTextField( "requestedByIdKey", a => a.RequestedByPersonAlias?.Person?.IdKey )
                .AddTextField( "requestedByFullName", a => a.RequestedByPersonAlias?.Person?.FullName )
                .AddTextField( "firstName", a => a.FirstName )
                .AddTextField( "lastName", a => a.LastName )
                .AddTextField( "resultSummary", a => a.ResultSummary )
                .AddTextField( "benevolenceType", a => a.BenevolenceType?.Name )
                .AddPersonField( "caseWorker", a => a.CaseWorkerPersonAlias?.Person )
                .AddTextField( "requestStatus", a => a.RequestStatusValue?.Value )
                .AddField( "totalAmount", a => a.TotalAmount )
                .AddField( "resultSpecifics", a => ToBenevolenceResultBag( a.BenevolenceResults ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Converts the BenevolenceResults to a list of BenevolenceResultBag.
        /// </summary>
        /// <param name="benevolenceResults">The benevolence results.</param>
        /// <returns></returns>
        private List<BenevolenceResultBag> ToBenevolenceResultBag( ICollection<BenevolenceResult> benevolenceResults )
        {
            return benevolenceResults.Select( r => new BenevolenceResultBag() { Amount = r.Amount, Result = r.ResultTypeValue.Value } ).ToList();
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
            var entityService = new BenevolenceRequestService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{BenevolenceRequest.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {BenevolenceRequest.FriendlyTypeName}." );
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
