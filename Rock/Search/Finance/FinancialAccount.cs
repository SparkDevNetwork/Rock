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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Search.Finance
{
    /// <summary>
    /// Searches for financial accounts with a matching name
    /// </summary>
    [Description( "Financial Account Search" )]
    [Export( typeof( SearchComponent ) )]
    [ExportMetadata( "ComponentName", "Account" )]
    [Rock.SystemGuid.EntityTypeGuid( "A1511DF5-7B3B-4E0D-BDEE-E4C4B56AACA7")]
    public class FinancialAccount : SearchComponent
    {
        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        public override Dictionary<string, string> AttributeValueDefaults
        {
            get
            {
                var defaults = new Dictionary<string, string>();
                defaults.Add( "SearchLabel", "Account" );
                defaults.Add( "ResultURL", "Account/Search/name/?SearchTerm={0}" );
                defaults.Add( "Active", true.ToString() );
                return defaults;
            }
        }

        /// <summary>
        /// Gets the financial accounts that match the search term and that the current person is allowed to view.
        /// </summary>
        /// <param name="searchTerm">The search term used to find results.</param>
        /// <returns>The viewable matching accounts, in database order.</returns>
        private List<Model.FinancialAccount> GetViewableSearchResults( string searchTerm )
        {
            if ( searchTerm.IsSingleSpecialCharacter() )
            {
                return new List<Model.FinancialAccount>();
            }

            var currentPerson = RockRequestContextAccessor.Current?.CurrentPerson;
            var financialAccountService = new FinancialAccountService( RockApp.Current.CreateRockContext() );

            // VIEW can't be checked in SQL, so every match is loaded before the filter.
            var accounts = financialAccountService.GetAccountsBySearchTerm( searchTerm )?.ToList()
                ?? new List<Model.FinancialAccount>();

            return accounts
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            var accounts = GetViewableSearchResults( searchTerm )
                .OrderBy( a => a.PublicName.IsNullOrWhiteSpace() ? a.Name : a.PublicName )
                .Cast<object>()
                .ToList();

            // This is a bit of a cheat. Since everything will have the same
            // order .OrderBy() will return an IOrderedQueryable in the original
            // order of the results.
            return accounts
                .AsQueryable()
                .OrderBy( a => true );
        }

        /// <summary>
        /// Returns a list of matching financial accounts
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchTerm )
        {
            /*
                10/18/2026 - MSE

                Smart Search renders each result string as HTML, so the account name, GL code and
                campus values are HTML encoded. Encoding can't be done in SQL, so the results are
                materialized first and capped at 50 after the VIEW filter.

                Reason: Prevent stored XSS through account names in Smart Search results.
            */
            var accountItems = GetViewableSearchResults( searchTerm )
                .Take( 50 )
                .Select( v =>
                {
                    var campus = v.CampusId.HasValue ? CampusCache.Get( v.CampusId.Value ) : null;

                    return new
                    {
                        AccountName = v.PublicName == null || v.PublicName.Length == 0 ? v.Name : v.PublicName,
                        v.GlCode,
                        HasCampus = campus != null,
                        CampusText = campus?.ShortCode.IsNotNullOrWhiteSpace() == true ? campus.ShortCode : campus?.Name
                    };
                } )
                .ToList();

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            var qry = accountItems
                .Select( v =>
                    !v.HasCampus
                        ? v.AccountName.EncodeHtml() + ( v.GlCode == null || v.GlCode.Length == 0 ? "" : " (" + v.GlCode.EncodeHtml() + ")" )
                        : v.AccountName.EncodeHtml() + ( v.GlCode == null || v.GlCode.Length == 0 ? "" : " (" + v.GlCode.EncodeHtml() + ")" ) + "                                                  <span class='search-accessory label label-default pull-right'>" + v.CampusText.EncodeHtml() + "</span>" )
                .AsQueryable();
            return qry;
        }
    }
}
