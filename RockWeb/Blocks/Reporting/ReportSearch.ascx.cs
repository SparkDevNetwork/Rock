﻿// <copyright>
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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// "Handles displaying group search results and redirects to the group detail page (via route ~/Group/) when only one match was found.
    /// </summary>
    [DisplayName( "Report Search" )]
    [Category( "Reporting" )]
    [Description( "Handles displaying report search results and redirects to the report result page (via route ~/reporting/reports) when only one match was found." )]
    [CodeEditorField( "Report URL Format", "The URL to use for linking to a report. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "/reporting/reports?ReportId={{ Report.Id }}" )]

    [Rock.SystemGuid.BlockTypeGuid( "13955B32-11F4-4606-8C31-4C6E5324C81A" )]
    public partial class ReportSearch : RockBlock
    {
        #region Fields

        private Dictionary<string, object> _commonMergeFields = new Dictionary<string, object>();

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            BindGrid();
        }

        #endregion

        #region Events

        #endregion

        #region Methods

        private void BindGrid()
        {
            string type = PageParameter( "SearchType" );
            string term = PageParameter( "SearchTerm" );

            var reportService = new ReportService( new RockContext() );
            var reports = new List<Report>();

            if ( !string.IsNullOrWhiteSpace( type ) && !string.IsNullOrWhiteSpace( term ) )
            {
                switch ( type.ToLower() )
                {
                    case "name":
                        {
                            reports = reportService.Queryable()
                                .Include( dv => dv.Category )
                                .Where( g =>
                                    g.Name.Contains( term ) )
                                .OrderBy( g => g.Name )
                                .ToList();

                            break;
                        }
                }
            }

            _commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

            if ( reports.Count == 1 )
            {
                var url = ResolveReportDetailsUrl( reports[0] );

                Response.Redirect( url, false );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                gGroups.EntityTypeId = EntityTypeCache.Get<Group>().Id;
                gGroups.DataSource = reports
                    .Select( g => new
                    {
                        g.Id,
                        Name = g.Name,
                        Structure = string.Format( ParentStructure( g.Category ), $"<a href='{ResolveReportDetailsUrl( g )}'>{g.Name}</a>" ),
                    } )
                    .ToList();
                gGroups.DataBind();
            }
        }

        private string ParentStructure( Category category, List<int> parentIds = null )
        {
            if ( category == null )
            {
                return "{0}";
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

            if ( !string.IsNullOrWhiteSpace( prefix ) )
            {
                prefix += " <i class='fa fa-angle-right'></i> ";
            }

            prefix += ParentStructure( category.ParentCategory, parentIds );

            return prefix;
        }

        private string ResolveReportDetailsUrl( Report report )
        {
            var mergeFields = new Dictionary<string, object>( _commonMergeFields )
            {
                {"Report", report}
            };
            var url = GetAttributeValue( "ReportURLFormat" ).ResolveMergeFields( mergeFields );
            return url;
        }

        #endregion
    }
}