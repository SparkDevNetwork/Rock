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
using System.Linq;
using System.Net;
using System.Text;

using Rock.Attribute;
using Rock.Cms;
using Rock.Enums.Cms;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SmartSearch;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Provides extensible options for searching in Rock.
    /// </summary>
    [DisplayName( "Smart Search" )]
    [Category( "Core" )]
    [Description( "Provides extensible options for searching in Rock." )]
    [IconCssClass( "ti ti-search" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [DefaultBlockRole( BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "A9F9C061-0073-4A7A-93DB-693A1F17D585" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9DAFE2D5-AC68-44AC-B648-A83CE39C8788" )]
    [Rock.SystemGuid.BlockTypeGuid( "9D406BD5-88C1-45E5-AFEA-70F9CFB66C74" )]
    public class SmartSearch : RockBlockType
    {
        private List<SearchFilterBag> _searchFilters;

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CustomBlockBox<object, SmartSearchOptionsBag>
            {
                Options = new SmartSearchOptionsBag
                {
                    SearchFilters = GetSearchFilters()
                }
            };
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            /*
                 3/23/2026 - MSE

                 Added server-rendered HTML so the search field is visible before the Vue component loads.
                 This prevents the field from briefly disappearing during page refresh.

                 Reason: Improve page load experience and prevent UI flicker.
            */
            var filters = GetSearchFilters();
            if ( filters == null || !filters.Any() )
            {
                return string.Empty;
            }

            var firstFilter = filters.First();
            var firstLabel = WebUtility.HtmlEncode( firstFilter.Label );

            var sb = new StringBuilder();
            sb.Append( "<div class=\"smartsearch searchinput\">" );
            sb.Append( "<i class=\"ti ti-search\"></i>" );
            sb.Append( "<ul class=\"nav pull-right smartsearch-type\">" );
            sb.Append( "<li class=\"dropdown\">" );
            sb.Append( $"<a class=\"dropdown-toggle navbar-link\" data-toggle=\"dropdown\"><span>{firstLabel}</span><b class=\"ti ti-caret-down-filled\"></b></a>" );
            sb.Append( "<ul class=\"dropdown-menu\">" );

            foreach ( var filter in filters )
            {
                var text = WebUtility.HtmlEncode( filter.Label );
                var target = WebUtility.HtmlEncode( filter.ResultUrl );
                sb.Append( $"<li data-key=\"{filter.Key}\" data-target=\"{target}\"><a>{text}</a></li>" );
            }

            sb.Append( "</ul></li></ul>" );
            sb.Append( "<input type=\"search\" accesskey=\"q\" class=\"searchinput tt-query\" autocomplete=\"off\" spellcheck=\"false\" style=\"position: relative; vertical-align: top; background-color: transparent;\" dir=\"auto\">" );
            sb.Append( "<input type=\"hidden\" name=\"searchField_hSearchFilter\" id=\"searchField_hSearchFilter\">" );
            sb.Append( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Gets the authorized, active search filters for the current person.
        /// Results are cached for the duration of the request.
        /// </summary>
        private List<SearchFilterBag> GetSearchFilters()
        {
            if ( _searchFilters != null )
            {
                return _searchFilters;
            }

            _searchFilters = new List<SearchFilterBag>();
            var currentPerson = RequestContext.CurrentPerson;

            foreach ( KeyValuePair<int, Lazy<Search.SearchComponent, Extension.IComponentData>> service in Search.SearchContainer.Instance.Components )
            {
                var searchComponent = service.Value.Value;
                if ( searchComponent.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                {
                    if ( !searchComponent.AttributeValues.ContainsKey( "Active" ) || bool.Parse( searchComponent.AttributeValues["Active"].Value ) )
                    {
                        _searchFilters.Add( new SearchFilterBag
                        {
                            Key = service.Key.ToString(),
                            Label = searchComponent.SearchLabel,
                            ResultUrl = searchComponent.ResultUrl,
                        } );
                    }
                }
            }

            return _searchFilters;
        }

        #endregion Methods
    }
}
