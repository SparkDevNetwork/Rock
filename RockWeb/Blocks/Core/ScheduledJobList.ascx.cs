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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    [DisplayName( "Scheduled Job List" )]
    [Category( "Core" )]
    [Description( "Lists all scheduled jobs." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [LinkedPage( "History Page",
        Description = "The page to display group history.",
        Key = AttributeKey.HistoryPage )]

    public partial class ScheduledJobList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string HistoryPage = "HistoryPage";
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            gScheduledJobs.DataKeyNames = new string[] { "Id" };
            gScheduledJobs.Actions.ShowAdd = true;
            gScheduledJobs.Actions.AddClick += gScheduledJobs_Add;
            gScheduledJobs.GridRebind += gScheduledJobs_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gScheduledJobs.Actions.ShowAdd = canAddEditDelete;
            gScheduledJobs.IsDeleteEnabled = canAddEditDelete;

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
        /// Handles the RowDataBound event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var site = RockPage.Site;
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                ServiceJob serviceJob = e.Row.DataItem as ServiceJob;
                if ( serviceJob == null )
                {
                    return;
                }

                // Remove the "Run Now" option and "History" button from the Job Pulse job
                Guid? jobGuid = serviceJob.Guid;
                if ( jobGuid.HasValue && jobGuid.Value.Equals( Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() ) )
                {
                    var runNowColumn = gScheduledJobs.ColumnsOfType<EditField>().Where( a => a.HeaderText == "Run Now" ).FirstOrDefault();
                    e.Row.Cells[gScheduledJobs.GetColumnIndex( runNowColumn )].Text = string.Empty;

                    var historyColumn = gScheduledJobs.ColumnsOfType<LinkButtonField>().Where( a => a.HeaderText == "History" ).FirstOrDefault();
                    e.Row.Cells[gScheduledJobs.GetColumnIndex( historyColumn )].Text = string.Empty;
                }

                // format duration
                if ( serviceJob.LastRunDurationSeconds.HasValue )
                {
                    int durationSeconds = serviceJob.LastRunDurationSeconds.Value;
                    TimeSpan duration = TimeSpan.FromSeconds( durationSeconds );

                    var lLastRunDurationSeconds = e.Row.FindControl( "lLastRunDurationSeconds" ) as Literal;

                    if ( lLastRunDurationSeconds != null )
                    {
                        if ( duration.Days > 0 )
                        {
                            lLastRunDurationSeconds.Text = duration.TotalHours.ToString( "F2" ) + " hours";
                        }
                        else if ( duration.Hours > 0 )
                        {
                            lLastRunDurationSeconds.Text = String.Format( "{0:%h}h {0:%m}m {0:%s}s", duration );
                        }
                        else if ( duration.Minutes > 0 )
                        {
                            lLastRunDurationSeconds.Text = String.Format( "{0:%m}m {0:%s}s", duration );
                        }
                        else
                        {
                            lLastRunDurationSeconds.Text = String.Format( "{0:%s}s", duration );
                        }
                    }
                }

                // format inactive jobs
                if ( serviceJob.IsActive == false )
                {
                    e.Row.AddCssClass( "inactive" );
                }

                // format last status
                var lLastStatus = e.Row.FindControl( "lLastStatus" ) as Literal;
                if ( serviceJob.LastStatus.IsNotNullOrWhiteSpace() )
                {
                    string lastStatus = serviceJob.LastStatus;

                    switch ( lastStatus )
                    {
                        case "Success":
                            lLastStatus.Text = "<span class='label label-success'>Success</span>";
                            break;
                        case "Running":
                            lLastStatus.Text = "<span class='label label-info'>Running</span>";
                            break;
                        case "Exception":
                            lLastStatus.Text = "<span class='label label-danger'>Failed</span>";
                            break;
                        case "Warning":
                            lLastStatus.Text = "<span class='label label-warning'>Warning</span>";
                            break;
                        case "":
                            lLastStatus.Text = "";
                            break;
                        default:
                            lLastStatus.Text = String.Format( "<span class='label label-warning'>{0}</span>", lastStatus );
                            break;
                    }
                }

                var lLastStatusMessageAsHtml = e.Row.FindControl( "lLastStatusMessageAsHtml" ) as Literal;
                if ( lLastStatusMessageAsHtml != null )
                {
                    if ( serviceJob.LastStatusMessageAsHtml.Length > 255 )
                    {
                        // if over 255 chars, limit the height to 100px so we don't get a giant summary displayed in the grid
                        // Also, we don't want to use .Truncate(255) since that could break any html that is in the LastStatusMessageAsHtml
                        lLastStatusMessageAsHtml.Text = string.Format( "<div style='max-height:100px;overflow:hidden'>{0}</div>", serviceJob.LastStatusMessageAsHtml );
                    }
                    else
                    {
                        lLastStatusMessageAsHtml.Text = serviceJob.LastStatusMessageAsHtml;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Add event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ServiceJobId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "ServiceJobId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the RunNow event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_RunNow( object sender, RowEventArgs e )
        {
            var job = new ServiceJobService( new RockContext() ).Get( e.RowKeyId );
            if ( job != null )
            {
                var transaction = new Rock.Transactions.RunJobNowTransaction( job.Id );

                // Process the transaction on another thread
                System.Threading.Tasks.Task.Run( () => transaction.Execute() );

                mdGridWarning.Show( string.Format( "The '{0}' job has been started.", job.Name ), ModalAlertType.Information );

                // wait a split second for the job to start so that the grid will show the status (if it changed)
                System.Threading.Tasks.Task.Delay( 250 ).Wait();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the grdScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var jobService = new ServiceJobService( rockContext );
            ServiceJob job = jobService.Get( e.RowKeyId );

            string errorMessage;
            if ( !jobService.CanDelete( job, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            jobService.Delete( job );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gScheduledJobs_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the History event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_History( object sender, RowEventArgs e )
        {
            var pageParams = new Dictionary<string, string>();
            pageParams.Add( "ScheduledJobId", e.RowKeyId.ToString() );
            string groupHistoryUrl = LinkedPageUrl( AttributeKey.HistoryPage, pageParams );
            Response.Redirect( groupHistoryUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the scheduled jobs.
        /// </summary>
        private void BindGrid()
        {
            var jobService = new ServiceJobService( new RockContext() );
            SortProperty sortProperty = gScheduledJobs.SortProperty;

            if ( sortProperty != null )
            {
                gScheduledJobs.DataSource = jobService.GetAllJobs().Sort( sortProperty ).ToList();
            }
            else
            {
                gScheduledJobs.DataSource = jobService.GetAllJobs().OrderByDescending( a => a.LastRunDateTime ).ThenBy( a => a.Name ).ToList();
            }

            gScheduledJobs.DataBind();
        }

        #endregion
    }
}