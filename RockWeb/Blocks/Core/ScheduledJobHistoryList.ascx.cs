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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Scheduled Job History" )]
    [Category( "Core" )]
    [Description( "Lists all scheduled job's History." )]
    public partial class ScheduledJobHistoryList : RockBlock, ICustomGridColumns
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            gScheduledJobHistory.DataKeyNames = new string[] { "Id" };
            gScheduledJobHistory.GridRebind += gScheduledJobHistory_GridRebind;

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the RowDataBound event of the gScheduledJobHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobHistory_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var site = RockPage.Site;
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                // format duration
                if ( e.Row.DataItem.GetPropertyValue( "DurationSeconds" ) != null )
                {
                    int durationSeconds = e.Row.DataItem.GetPropertyValue( "DurationSeconds" ).ToString().AsIntegerOrNull() ?? 0;
                    TimeSpan duration = TimeSpan.FromSeconds( durationSeconds );

                    var lDurationSeconds = e.Row.FindControl( "lDurationSeconds" ) as Literal;

                    if ( lDurationSeconds != null )
                    {
                        if ( duration.Days > 0 )
                        {
                            lDurationSeconds.Text = duration.TotalHours.ToString( "F2" ) + " hours";
                        }
                        else if ( duration.Hours > 0 )
                        {
                            lDurationSeconds.Text = String.Format( "{0:%h}h {0:%m}m {0:%s}s", duration );
                        }
                        else if ( duration.Minutes > 0 )
                        {
                            lDurationSeconds.Text = String.Format( "{0:%m}m {0:%s}s", duration );
                        }
                        else
                        {
                            lDurationSeconds.Text = String.Format( "{0:%s}s", duration );
                        }
                    }
                }

                // format last status
                var lStatus = e.Row.FindControl( "lStatus" ) as Literal;
                if ( e.Row.DataItem.GetPropertyValue( "Status" ) != null && lStatus != null )
                {
                    string status = e.Row.DataItem.GetPropertyValue( "Status" ).ToString();

                    switch ( status )
                    {
                        case "Success":
                            lStatus.Text = "<span class='label label-success'>Success</span>";
                            break;
                        case "Running":
                            lStatus.Text = "<span class='label label-info'>Running</span>";
                            break;
                        case "Exception":
                            lStatus.Text = "<span class='label label-danger'>Failed</span>";
                            break;
                        case "Warning":
                            lStatus.Text = "<span class='label label-warning'>Warning</span>";
                            break;
                        case "":
                            lStatus.Text = "";
                            break;
                        default:
                            lStatus.Text = String.Format( "<span class='label label-warning'>{0}</span>", status );
                            break;
                    }

                    var lStatusMessageAsHtml = e.Row.FindControl( "lStatusMessageAsHtml" ) as Literal;
                    if ( lStatusMessageAsHtml != null )
                    {
                        var statusMessageAsHtml = e.Row.DataItem.GetPropertyValue( "StatusMessageAsHtml" ) as string;
                        if ( statusMessageAsHtml.Length > 255 )
                        {
                            // if over 255 chars, limit the height to 100px so we don't get a giant summary displayed in the grid
                            // Also, we don't want to use .Truncate(255) since that could break any html that is in the LastStatusMessageAsHtml
                            lStatusMessageAsHtml.Text = string.Format( "<div style='max-height:100px;overflow:hidden'>{0}</div>", statusMessageAsHtml );
                        }
                        else
                        {
                            lStatusMessageAsHtml.Text = statusMessageAsHtml;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gScheduledJobHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gScheduledJobHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the scheduled jobs.
        /// </summary>
        private void BindGrid()
        {
            int? scheduledJobId = PageParameter( "ScheduledJobId" ).AsIntegerOrNull();

            if ( scheduledJobId == null )
            {
                return;
            }

            var rockContext = new RockContext();

            ServiceJobService jobService = new ServiceJobService( rockContext );

            var job = jobService.Get( scheduledJobId.Value );
            lJobName.Text = job.Name;

            var jobHistoryService = new ServiceJobHistoryService( rockContext );
            SortProperty sortProperty = gScheduledJobHistory.SortProperty;

            var qry = jobHistoryService.GetServiceJobHistory( scheduledJobId );

            if ( sortProperty != null )
            {
                if ( sortProperty.Property == "DurationSeconds" )
                {
                    if ( sortProperty.Direction == SortDirection.Ascending )
                    {
                        qry = qry.OrderBy( a => System.Data.Entity.DbFunctions.DiffSeconds( a.StopDateTime, a.StartDateTime ) );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( a => System.Data.Entity.DbFunctions.DiffSeconds( a.StopDateTime, a.StartDateTime ) );
                    }

                }
                else
                {
                    qry = qry.Sort( sortProperty );
                }
            }
            else
            {
                qry = qry.OrderByDescending( a => a.StartDateTime );
            }

            gScheduledJobHistory.SetLinqDataSource( qry );

            gScheduledJobHistory.DataBind();
        }

        #endregion
    }
}