// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

    [LinkedPage("Detail Page")]
    public partial class ScheduledJobList : RockBlock
    {
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

        protected void gScheduledJobs_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var site = RockPage.Site;
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                // Remove the "Run Now" option from the Job Pulse job
                Guid? jobGuid = e.Row.DataItem.GetPropertyValue( "Guid" ).ToString().AsGuidOrNull();
                if ( jobGuid.HasValue && jobGuid.Value.Equals( Rock.SystemGuid.ServiceJob.JOB_PULSE.AsGuid() ))
                {
                    e.Row.Cells[8].Text = string.Empty;
                }

                // format duration
                if ( e.Row.DataItem.GetPropertyValue( "LastRunDurationSeconds" ) != null )
                {
                    int durationSeconds = 0;
                    int.TryParse( e.Row.DataItem.GetPropertyValue( "LastRunDurationSeconds" ).ToString(), out durationSeconds );

                    TimeSpan duration = new TimeSpan( 0, 0, durationSeconds );

                    if ( durationSeconds >= 60 )
                    {
                        e.Row.Cells[3].Text = String.Format( "{0:%m}m {0:%s}s", duration );
                    }
                    else
                    {
                        e.Row.Cells[3].Text = String.Format( "{0:%s}s", duration );
                    }
                }

                // format last status
                if ( e.Row.DataItem.GetPropertyValue( "LastStatus" ) != null )
                {
                    string lastStatus = e.Row.DataItem.GetPropertyValue( "LastStatus" ).ToString();

                    switch ( lastStatus )
                    {
                        case "Success":
                            e.Row.Cells[4].Text = "<span class='label label-success'>Success</span>";
                            break;
                        case "Exception":
                            e.Row.Cells[4].Text = "<span class='label label-danger'>Failed</span>";
                            break;
                        case "":
                            e.Row.Cells[4].Text = "";
                            break;
                        default:
                            e.Row.Cells[4].Text = String.Format( "<span class='label label-warning'>{0}</span>", lastStatus );
                            break;
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
            NavigateToLinkedPage( "DetailPage", "serviceJobId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "serviceJobId", e.RowKeyId );
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
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

                mdGridWarning.Show( string.Format( "The '{0}' job has been triggered to run and will start within the next two minutes ( during the next transaction cycle ).", job.Name ), ModalAlertType.Information );
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
                gScheduledJobs.DataSource = jobService.GetAllJobs().OrderByDescending( a => a.LastRunDateTime ).ToList();
            }
            
            gScheduledJobs.DataBind();
        }

        #endregion
}
}