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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.ReportSearch;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Handles displaying report search results and redirects to the report result page (via route ~/reporting/reports) when only one match was found.
    /// </summary>
    [DisplayName( "Report Search" )]
    [Category( "Reporting" )]
    [Description( "Handles displaying report search results and redirects to the report result page (via route ~/reporting/reports) when only one match was found." )]
    [IconCssClass( "ti ti-search" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [CodeEditorField( "Report URL Format",
        Description = "The URL to use for linking to a report. <span class='tip tip-lava'></span>",
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Lava,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "/reporting/reports?ReportId={{ Report.Id }}",
        Key = AttributeKey.ReportUrlFormat )]

    [Rock.SystemGuid.EntityTypeGuid( "8E34EB41-DB93-4C59-8A2E-2FC22350C3C3" )]
    [Rock.SystemGuid.BlockTypeGuid( "13955B32-11F4-4606-8C31-4C6E5324C81A" )]
    [CustomizedGrid]
    public class ReportSearch : RockListBlockType<ReportSearchResultBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ReportUrlFormat = "ReportURLFormat";
        }

        #endregion Keys

        #region Methods

        /// <summary>
        /// Gets the initialization information required by the Obsidian block.
        /// </summary>
        /// <returns>The block initialization data.</returns>
        public override object GetObsidianBlockInitialization()
        {
            return new ListBlockBox<ReportSearchOptionsBag>
            {
                IsAddEnabled = false,
                IsDeleteEnabled = false,
                ExpectedRowCount = null,
                Options = new ReportSearchOptionsBag(),
                GridDefinition = GetGridBuilder().BuildDefinition()
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ReportSearchResultBag> GetListQueryable( RockContext rockContext )
        {
            return GetSearchResults( rockContext ).AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<ReportSearchResultBag> GetOrderedListQueryable( IQueryable<ReportSearchResultBag> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( r => r.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ReportSearchResultBag> GetGridBuilder()
        {
            return new GridBuilder<ReportSearchResultBag>()
                .WithBlock( this )
                .AddField( "id", r => r.Id )
                .AddTextField( "name", r => r.Name )
                .AddTextField( "structure", r => r.Structure )
                .AddTextField( "url", r => r.Url );
        }

        /// <summary>
        /// Gets the current report search results.
        /// </summary>
        /// <param name="rockContext">The context used to access the database.</param>
        /// <returns>The search results.</returns>
        private List<ReportSearchResultBag> GetSearchResults( RockContext rockContext )
        {
            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            var reports = new List<Report>();

            if ( !string.IsNullOrWhiteSpace( type ) && !string.IsNullOrWhiteSpace( term ) )
            {
                switch ( type.ToLowerInvariant() )
                {
                    case "name":
                        reports = new ReportService( rockContext )
                            .Queryable()
                            .Include( r => r.Category )
                            .Where( r => r.Name.Contains( term ) )
                            .ToList();
                        break;
                }
            }

            var commonMergeFields = RequestContext.GetCommonMergeFields( RequestContext.CurrentPerson );

            /*
                 3/13/2026 - NA

                 Server-side redirects are not currently supported in an  Obsidian
                 block during this stage of the rendering pipeline. Attempting to call
                 RequestContext.Response.RedirectToUrl() here will not work as expected.

                 In this scenario (reports.Count == 1), the redirect must instead be
                 handled on the client side.

                 The example below represents the preferred server-side behavior if the
                 Obsidian framework later supports safe redirects at this stage.

                 Example (currently not functional):

                 if ( reports.Count == 1 )
                 {
                     var url = ResolveReportDetailsUrl( reports[0], commonMergeFields );
                     RequestContext.Response.RedirectToUrl( url );
                 }
            */

            return reports
                .Select( r => new ReportSearchResultBag
                {
                    Id = r.Id,
                    Name = r.Name,
                    Structure = ParentStructure( r.Category ),
                    Url = ResolveReportDetailsUrl( r, commonMergeFields )
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the category structure display text used by the search grid.
        /// </summary>
        /// <param name="category">The category to inspect.</param>
        /// <param name="parentIds">The categories already visited while building the tree path.</param>
        /// <returns>A formatted category structure string.</returns>
        private string ParentStructure( Category category, List<int> parentIds = null )
        {
            if ( category == null )
            {
                return string.Empty;
            }

            string prefix = category.Name;

            // Create or add this node to the history stack for this tree walk.
            if ( parentIds == null )
            {
                parentIds = new List<int>();
            }
            else
            {
                // If we have encountered this node before during this tree walk, we have found an infinite recursion in the tree.
                // Truncate the path with an error message and exit.
                if ( parentIds.Contains( category.Id ) )
                {
                    return "#Invalid-Parent-Reference#";
                }
            }

            parentIds.Add( category.Id );

            var parentStructure = ParentStructure( category.ParentCategory, parentIds );

            if ( !string.IsNullOrWhiteSpace( parentStructure ) )
            {
                prefix += " <i class='ti ti-chevron-right'></i> " + parentStructure;
            }

            return prefix;
        }

        /// <summary>
        /// Resolves the report details URL.
        /// </summary>
        /// <param name="report">The report that will be linked to.</param>
        /// <param name="commonMergeFields">The merge fields shared across all rows.</param>
        /// <returns>The resolved report details URL.</returns>
        private string ResolveReportDetailsUrl( Report report, Dictionary<string, object> commonMergeFields )
        {
            var mergeFields = new Dictionary<string, object>( commonMergeFields )
            {
                ["Report"] = report
            };

            return GetAttributeValue( AttributeKey.ReportUrlFormat ).ResolveMergeFields( mergeFields );
        }

        #endregion Methods
    }
}
