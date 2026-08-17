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
using Rock.Model;
using Rock.ViewModels.Blocks.Event.RegistrationInstanceNavigation;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Renders the tabbed navigation strip at the top of a Registration Instance
    /// page, carrying the current Registration Instance context forward to each
    /// sibling page.
    /// </summary>
    [DisplayName( "Registration Instance - Navigation" )]
    [Category( "Event" )]
    [Description( "Provides the navigation for the tabs navigation section of the Registration Instance Page/Layout." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Wait List Page",
        Description = "The Page that shows the Wait List.",
        Key = AttributeKey.WaitListPage,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Page.REGISTRATION_INSTANCE_WAIT_LIST,
        Order = 0 )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "BABBB6AE-803F-4A0E-BA3B-5C8A822D1818" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "AC464890-9D1A-4800-A0EF-30967ED9AEC9" )]
    [Rock.SystemGuid.BlockTypeGuid( "AF0740C9-BC60-434B-A360-EB70A7CEA108" )]
    public class RegistrationInstanceNavigation : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string WaitListPage = "WaitListPage";
        }

        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationTemplatePlacementId = "RegistrationTemplatePlacementId";
            public const string RegistrantId = "RegistrantId";
        }

        /// <summary>
        /// Page parameter keys that must NOT be carried into tab URLs. These
        /// are page-specific values that should not leak between sibling pages.
        /// </summary>
        private static readonly HashSet<string> ExcludedTabParameterKeys = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
        {
            "PageId",
            PageParameterKey.RegistrationTemplatePlacementId,
            PageParameterKey.RegistrantId
        };

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new RegistrationInstanceNavigationOptionsBag
            {
                Tabs = BuildTabs()
            };
        }

        /// <summary>
        /// Resolves the Registration Instance from the page parameter,
        /// accepting Id, IdKey, or Guid. Eager-loads RegistrationTemplate so
        /// WaitListEnabled can be checked without a second round trip.
        /// </summary>
        private RegistrationInstance GetRegistrationInstance()
        {
            var key = PageParameter( PageParameterKey.RegistrationInstanceId );
            if ( key.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new RegistrationInstanceService( RockContext )
                .GetQueryableByKey( key, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( a => a.RegistrationTemplate )
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Builds the ordered list of navigation tabs for the current
        /// Registration Instance. Returns an empty list when no Registration
        /// Instance is in scope.
        /// </summary>
        private List<NavigationTabBag> BuildTabs()
        {
            var registrationInstance = GetRegistrationInstance();
            if ( registrationInstance?.RegistrationTemplate == null )
            {
                return new List<NavigationTabBag>();
            }

            // LinkedPage attribute values are stored as "pageGuid,routeGuid"
            // when a route is selected, so we need the first segment before
            // parsing as a Guid.
            var waitListPageGuid = GetAttributeValue( AttributeKey.WaitListPage )
                .SplitDelimitedValues()
                .FirstOrDefault()
                .AsGuidOrNull();
            var showWaitListTab = registrationInstance.RegistrationTemplate.WaitListEnabled;

            var pages = PageCache.ParentPage.GetPages( RockContext )
                .Where( page => page.DisplayInNav( RequestContext.CurrentPerson ) )
                .Where( page => page.DisplayInNavWhen != DisplayInNavWhen.Never )
                .OrderBy( page => page.Order )
                .ToList();

            if ( !showWaitListTab && waitListPageGuid.HasValue )
            {
                pages = pages.Where( p => p.Guid != waitListPageGuid.Value ).ToList();
            }

            var tabParameters = RequestContext.GetPageParameters()
                .Where( kvp => !ExcludedTabParameterKeys.Contains( kvp.Key ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );

            var currentPageId = PageCache.Id;

            return pages.Select( page =>
            {
                var pageReference = new PageReference( page.Id )
                {
                    Parameters = new Dictionary<string, string>( tabParameters )
                };

                return new NavigationTabBag
                {
                    PageIdKey = page.IdKey,
                    Title = page.PageTitle,
                    Url = pageReference.BuildUrl(),
                    IsActive = page.Id == currentPageId
                };
            } ).ToList();
        }

        #endregion
    }
}
