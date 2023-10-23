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
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
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

    [Rock.SystemGuid.BlockTypeGuid( "6D3F924E-BDD0-4C78-981E-B698351E75AD" )]
    public partial class ScheduledJobList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string HistoryPage = "HistoryPage";
        }

        public static class GridUserPreferenceKey
        {
            public const string Name = "Name";
            public const string ActiveStatus = "Active Status";
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
                BindGridFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Binds the grid filter.
        /// </summary>
        private void BindGridFilter()
        {
            tbNameFilter.Text = gfSettings.GetFilterPreference( GridUserPreferenceKey.Name );

            // Set the Active Status
            var activeStatusFilter = gfSettings.GetFilterPreference( GridUserPreferenceKey.ActiveStatus );
            ddlActiveFilter.SetValue( activeStatusFilter );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SetFilterPreference( GridUserPreferenceKey.ActiveStatus, string.Empty );
            }
            else
            {
                gfSettings.SetFilterPreference( GridUserPreferenceKey.ActiveStatus, ddlActiveFilter.SelectedValue );
            }

            gfSettings.SetFilterPreference( GridUserPreferenceKey.Name, tbNameFilter.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ClearFilterClick( object sender, EventArgs e )
        {
            gfSettings.DeleteFilterPreferences();
            BindGridFilter();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the DataBound event of the gScheduledJobs_History control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_History_DataBound( object sender, RowEventArgs e )
        {
            var job = e.Row.DataItem as ServiceJob;
            if ( job == null )
            {
                return;
            }

            // Remove the "Run Now" option and "History" button from the Job Pulse job
            if ( job.Guid == Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
            {
                var btnShowHistory = sender as LinkButton;
                if ( btnShowHistory != null )
                {
                    btnShowHistory.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the DataBound event of the gScheduledJobs_RunNow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_RunNow_DataBound( object sender, RowEventArgs e )
        {
            var job = e.Row.DataItem as ServiceJob;
            if ( job == null )
            {
                return;
            }

            // Remove the "Run Now" option and "History" button from the Job Pulse job
            if ( job.Guid == Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() )
            {
                var btnRunNow = sender as LinkButton;
                if ( btnRunNow != null )
                {
                    btnRunNow.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gScheduledJobs_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                ServiceJob serviceJob = e.Row.DataItem as ServiceJob;
                if ( serviceJob == null )
                {
                    return;
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
                    lLastStatusMessageAsHtml.Text = serviceJob.LastStatusMessageAsHtml;
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
                // Force the job to run as an entirely new activity instead of
                // inheriting the curring page activity. This must be done on
                // a task otherwise we will wipe out the current activity.
                System.Threading.Tasks.Task.Run( () =>
                {
                    System.Diagnostics.Activity.Current = null;

                    new ProcessRunJobNow.Message { JobId = job.Id }.Send();
                } );

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

            var jobsQuery = jobService.GetAllJobs();

            var nameFilter = gfSettings.GetFilterPreference( GridUserPreferenceKey.Name );
            if ( nameFilter.IsNotNullOrWhiteSpace() )
            {
                jobsQuery = jobsQuery.Where( a => a.Name.Contains( nameFilter ) );
            }

            var activeFilter = gfSettings.GetFilterPreference( GridUserPreferenceKey.ActiveStatus );
            if ( activeFilter.IsNotNullOrWhiteSpace() )
            {
                var activeStatus = activeFilter.AsBoolean();
                jobsQuery = jobsQuery.Where( a => a.IsActive == activeStatus );
            }

            if ( sortProperty != null )
            {
                gScheduledJobs.DataSource = jobsQuery.Sort( sortProperty ).ToList();
            }
            else
            {
                gScheduledJobs.DataSource = jobsQuery.OrderByDescending( a => a.LastRunDateTime ).ThenBy( a => a.Name ).ToList();
            }

            gScheduledJobs.DataBind();
        }

        #endregion

        
    }
}