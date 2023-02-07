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

using Rock.Data;
using Rock.Model;

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

        /// <inheritdoc/>
        public override IOrderedQueryable<object> SearchQuery( string searchTerm )
        {
            var rockContext = new RockContext();
            var financialAccountService = new FinancialAccountService( rockContext );
            var qry = financialAccountService.GetAccountsBySearchTerm( searchTerm );

            return qry
                .OrderBy( p => p.PublicName.IsNullOrWhiteSpace() ? p.Name : p.PublicName );
        }

        /// <summary>
        /// Returns a list of matching financial accounts
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public override IQueryable<string> Search( string searchTerm )
        {
            var rockContext = new RockContext();
            var financialAccountService = new FinancialAccountService( rockContext );

            // Note: extra spaces intentional with the label span to keep the markup from showing in the search input on selection
            var qry = financialAccountService.GetAccountsBySearchTerm( searchTerm )
                .Select( v =>
                    v.Campus == null
                        ? ( v.PublicName == null || v.PublicName.Length==0 ? v.Name : v.PublicName ) + ( v.GlCode==null || v.GlCode.Length==0 ? "" : " (" + v.GlCode + ")" )
                        : (v.PublicName == null || v.PublicName.Length == 0 ? v.Name : v.PublicName) + (v.GlCode == null || v.GlCode.Length == 0 ? "" : " (" + v.GlCode + ")") + "                                                  <span class='search-accessory label label-default pull-right'>" + ( v.Campus.ShortCode != "" ? v.Campus.ShortCode : v.Campus.Name ) + "</span>" );
            return qry;
        }
    }
}
