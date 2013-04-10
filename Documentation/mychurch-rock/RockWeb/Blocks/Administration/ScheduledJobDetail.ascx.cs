//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ScheduledJobDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "serviceJobId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "serviceJobId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
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
            NavigateToParentPage();
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

            NavigateToParentPage();
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="jobId">The job id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // return if unexpected itemKey 
            if ( itemKey != "serviceJobId" )
            {
                return;
            }

            pnlDetails.Visible = true;
            LoadDropDowns();

            // Load depending on Add(0) or Edit
            ServiceJob job = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                job = new ServiceJobService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( ServiceJob.FriendlyTypeName );
            }
            else
            {
                job = new ServiceJob { Id = 0, IsActive = true };
                lActionTitle.Text = ActionTitle.Add( ServiceJob.FriendlyTypeName );
            }

            hfId.Value = job.Id.ToString();
            tbName.Text = job.Name;
            tbDescription.Text = job.Description;
            cbActive.Checked = job.IsActive.HasValue ? job.IsActive.Value : false;
            tbAssembly.Text = job.Assembly;
            tbClass.Text = job.Class;
            tbNotificationEmails.Text = job.NotificationEmails;
            drpNotificationStatus.SetValue( (int)job.NotificationStatus );
            tbCronExpression.Text = job.CronExpression;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ServiceJob.FriendlyTypeName );
            }

            if ( job.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( ServiceJob.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( ServiceJob.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            cbActive.Enabled = !readOnly;
            tbAssembly.ReadOnly = readOnly;
            tbClass.ReadOnly = readOnly;
            tbNotificationEmails.ReadOnly = readOnly;
            drpNotificationStatus.Enabled = !readOnly;
            tbCronExpression.ReadOnly = readOnly;

            btnSave.Visible = !readOnly;
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