//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ScheduledJobs : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            gScheduledJobs.DataKeyNames = new string[] { "id" };
            gScheduledJobs.Actions.IsAddEnabled = true;
            gScheduledJobs.Actions.AddClick += gScheduledJobs_Add;
            gScheduledJobs.GridRebind += gScheduledJobs_GridRebind; 

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            mdGridWarning.Hide();
            nbWarning.Visible = false;

            if ( !Page.IsPostBack )
            {
                BindGrid();
                LoadDropDowns();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="jobId">The job id.</param>
        protected void ShowEdit( int jobId )
        {
            pnlDetails.Visible = true;
            pnlGrid.Visible = false;
            hfId.Value = jobId.ToString();

            ServiceJob job = null;
            if ( jobId != 0 )
            {
                job = ServiceJob.Read( jobId );
                lActionTitle.Text = ActionTitle.Edit( ServiceJob.FriendlyTypeName );
            }
            else
            {
                job = new ServiceJob { IsActive = true };
                lActionTitle.Text = ActionTitle.Add( ServiceJob.FriendlyTypeName );
            }

            tbName.Text = job.Name;
            tbDescription.Text = job.Description;
            cbActive.Checked = job.IsActive.HasValue ? job.IsActive.Value : false;
            tbAssembly.Text = job.Assembly;
            tbClass.Text = job.Class;
            tbNotificationEmails.Text = job.NotificationEmails;
            drpNotificationStatus.SetValue( (int)job.NotificationStatus );
            tbCronExpression.Text = job.CronExpression;
        }

        /// <summary>
        /// Handles the Delete event of the grdScheduledJobs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gScheduledJobs_Delete( object sender, RowEventArgs e )
        {
            ServiceJobService jobService = new ServiceJobService();
            ServiceJob job = jobService.Get( (int)e.RowKeyValue );

            string errorMessage;
            if ( !jobService.CanDelete( job, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            jobService.Delete( job, CurrentPersonId );
            jobService.Save( job, CurrentPersonId );

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

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlGrid.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ServiceJob job;
            ServiceJobService jobService = new ServiceJobService();
            
            int jobId = int.Parse( hfId.Value );

            if ( jobId == 0 )
            {
                job = new ServiceJob();
                jobService.Add( job, CurrentPersonId );
            }
            else
            {
                job = jobService.Get( jobId );
            }

            job.Name = tbName.Text;
            job.Description = tbDescription.Text;
            job.IsActive = cbActive.Checked;
            job.Assembly = tbAssembly.Text;
            job.Class = tbClass.Text;
            job.NotificationEmails = tbNotificationEmails.Text;
            job.NotificationStatus = (JobNotificationStatus)int.Parse( drpNotificationStatus.SelectedValue );
            job.CronExpression = tbCronExpression.Text;

            if ( !job.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
                {
                    jobService.Save( job, CurrentPersonId );
                } );

            BindGrid();
            pnlDetails.Visible = false;
            pnlGrid.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the scheduled jobs.
        /// </summary>
        private void BindGrid()
        {
            ServiceJobService jobService = new ServiceJobService();
            SortProperty sortProperty = gScheduledJobs.SortProperty;

            if ( sortProperty != null )
            {
                gScheduledJobs.DataSource = jobService.GetActiveJobs().Sort( sortProperty ).ToList();
            }
            else
            {
                gScheduledJobs.DataSource = jobService.GetActiveJobs().OrderByDescending(a => a.LastRunDateTime).ToList();
            }
            
            gScheduledJobs.DataBind();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            drpNotificationStatus.BindToEnum( typeof( JobNotificationStatus ) );
        }

        #endregion
    }
}