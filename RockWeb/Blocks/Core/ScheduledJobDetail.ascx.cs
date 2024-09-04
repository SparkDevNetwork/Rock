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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User controls for managing scheduled jobs.
    /// </summary>
    [DisplayName( "Scheduled Job Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given scheduled job." )]
    [Rock.SystemGuid.BlockTypeGuid( "C5EC90C9-26C4-493A-84AC-4B5DEF9EA472" )]
    public partial class ScheduledJobDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ServiceJobId" ).AsInteger() );
            }

            base.OnLoad( e );
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
            try
            {
                if ( !ServiceJobService.IsValidCronDescription( tbCronExpression.Text ) )
                {
                    tbCronExpression.ShowErrorMessage( "Invalid Cron Expression" );
                    return;
                }
            }
            catch ( Exception ex )
            {
                tbCronExpression.ShowErrorMessage( "Invalid Cron Expression: " + ex.Message );
                return;
            }


            ServiceJob job;
            var rockContext = new RockContext();
            ServiceJobService jobService = new ServiceJobService( rockContext );

            int jobId = int.Parse( hfId.Value );

            if ( jobId == 0 )
            {
                job = new ServiceJob();
                jobService.Add( job );
            }
            else
            {
                job = jobService.Get( jobId );
            }

            job.Name = tbName.Text;
            job.Description = tbDescription.Text;
            job.IsActive = cbActive.Checked;

            if ( job.Class != ddlJobTypes.SelectedValue )
            {
                job.Class = ddlJobTypes.SelectedValue;

                //// if the Class has changed, the current Assembly value might not match, 
                //// so set the Assembly to null to have Rock figure it out automatically
                job.Assembly = null;
            }

            job.NotificationEmails = tbNotificationEmails.Text;
            job.NotificationStatus = ( JobNotificationStatus ) int.Parse( ddlNotificationStatus.SelectedValue );
            job.CronExpression = tbCronExpression.Text;
            job.HistoryCount = nbHistoryCount.Text.AsInteger();

            if ( !job.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                avcAttributes.GetEditValues( job );
                job.SaveAttributeValues( rockContext );

            } );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlJobTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlJobTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            ServiceJob job;
            var itemId = PageParameter( "ServiceJobId" ).AsInteger();
            if ( itemId == 0 )
            {
                job = new ServiceJob { Id = 0, IsActive = true };
            }
            else
            {
                job = new ServiceJobService( new RockContext() ).Get( itemId );
            }

            job.Class = ddlJobTypes.SelectedValue;
            job.LoadAttributes();
            if ( tbDescription.Text.IsNullOrWhiteSpace() && tbName.Text.IsNullOrWhiteSpace() )
            {
                try
                {
                    Type selectedJobType = job.GetCompiledType();
                    tbName.Text = Rock.Reflection.GetDisplayName( selectedJobType );
                    tbDescription.Text = Rock.Reflection.GetDescription( selectedJobType );
                }
                catch
                {
                    // ignore if there is a problem getting the description from the selected job.class
                }
            }

            avcAttributes.AddEditControls( job );
        }

        /// <summary>
        /// Handles the TextChanged event of the tbCronExpression control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tbCronExpression_TextChanged( object sender, EventArgs e )
        {
            lCronExpressionDesc.Text = ServiceJobService.GetCronDescription( tbCronExpression.Text );
            lCronExpressionDesc.Visible = true;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        public void ShowDetail( int serviceJobId )
        {
            pnlDetails.Visible = true;
            LoadDropDowns();

            // Load depending on Add(0) or Edit
            ServiceJob job = null;
            if ( !serviceJobId.Equals( 0 ) )
            {
                job = new ServiceJobService( new RockContext() ).Get( serviceJobId );
                lActionTitle.Text = ActionTitle.Edit( ServiceJob.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( job, ResolveRockUrl( "~" ) );
            }

            if ( job == null )
            {
                job = new ServiceJob { Id = 0, IsActive = true };
                lActionTitle.Text = ActionTitle.Add( ServiceJob.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfId.Value = job.Id.ToString();
            tbName.Text = job.Name;
            tbDescription.Text = job.Description;
            cbActive.Checked = job.IsActive.HasValue ? job.IsActive.Value : false;
            if ( job.Class.IsNotNullOrWhiteSpace() )
            {
                if ( ddlJobTypes.Items.FindByValue( job.Class ) == null )
                {
                    nbJobTypeError.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbJobTypeError.Text = "Unable to find Job Type: " + job.Class;
                    nbJobTypeError.Visible = true;
                }
            }

            ddlJobTypes.SetValue( job.Class );

            tbNotificationEmails.Text = job.NotificationEmails;
            ddlNotificationStatus.SetValue( ( int ) job.NotificationStatus );
            tbCronExpression.Text = job.CronExpression;
            nbHistoryCount.Text = job.HistoryCount.ToString();

            if ( job.Id == 0 )
            {
                job.Class = ddlJobTypes.SelectedValue;
                lCronExpressionDesc.Visible = false;
                lLastStatusMessage.Visible = false;
            }
            else
            {
                lCronExpressionDesc.Text = ServiceJobService.GetCronDescription( job.CronExpression );
                lCronExpressionDesc.Visible = true;

                lLastStatusMessage.Text = job.LastStatusMessage.ConvertCrLfToHtmlBr();
                lLastStatusMessage.Visible = true;
            }

            job.LoadAttributes();
            avcAttributes.AddEditControls( job );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ServiceJob.FriendlyTypeName );
            }

            if ( job.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( ServiceJob.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( ServiceJob.FriendlyTypeName ).FormatAsHtmlTitle();
                btnCancel.Text = "Close";
                avcAttributesReadOnly.AddDisplayControls( job );
                avcAttributesReadOnly.Visible = true;
                avcAttributes.Visible = false;
                tbCronExpression.Text = job.CronExpression;
            }

            tbName.ReadOnly = readOnly || job.IsSystem;
            tbDescription.ReadOnly = readOnly || job.IsSystem;
            cbActive.Enabled = !( readOnly || job.IsSystem );
            ddlJobTypes.Enabled = !( readOnly || job.IsSystem );
            tbNotificationEmails.ReadOnly = readOnly;
            ddlNotificationStatus.Enabled = !readOnly;
            tbCronExpression.ReadOnly = readOnly || job.IsSystem;

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlNotificationStatus.BindToEnum<JobNotificationStatus>();

            int? jobEntityTypeId = EntityTypeCache.Get( "Rock.Model.ServiceJob" ).Id;
            var rockJobType = typeof( Rock.Jobs.RockJob );

            var rockJobs = Rock.Reflection.FindTypes( rockJobType ).Values;


#pragma warning disable CS0618 // Type or member is obsolete
            var obsoleteQuartzJobs = Rock.Reflection.FindTypes( typeof( Quartz.IJob ) ).Values.Where( a => !rockJobType.IsAssignableFrom( a ) );
#pragma warning restore CS0618 // Type or member is obsolete

            var jobs = rockJobs.ToList();
            foreach(var obsoleteQuartzJob in obsoleteQuartzJobs)
            {
                jobs.Add( obsoleteQuartzJob, true );
            }

            var rockContext = new RockContext();
            List<string> jobTypeErrors = new List<string>();
            var jobsList = jobs.ToList();
            foreach ( var job in jobs )
            {
                try
                {
                    ServiceJobService.UpdateAttributesIfNeeded( job );
                }
                catch ( Exception ex )
                {
                    jobsList.Remove( job );
                    jobTypeErrors.Add( string.Format( "Job: {0}, Error: {1}", job.Name, ex.Message ) );
                }
            }

            ddlJobTypes.Items.Clear();
            ddlJobTypes.Items.Add( new ListItem() );
            foreach ( var job in jobsList.OrderBy( a => a.FullName ) )
            {
                ddlJobTypes.Items.Add( new ListItem( CreateJobTypeFriendlyName( job.FullName ), job.FullName ) );
            }

            nbJobTypeError.Visible = jobTypeErrors.Any();
            nbJobTypeError.Text = "Error loading job types";
            nbJobTypeError.Details = jobTypeErrors.AsDelimited( "<br/>" );
        }

        /// <summary>
        /// Create Job Type Friendly Name
        /// </summary>
        private string CreateJobTypeFriendlyName( string jobType )
        {
            string friendlyName;
            if ( jobType.Contains( "Rock.Jobs." ) )
            {
                friendlyName = jobType.Replace( "Rock.Jobs.", string.Empty ).SplitCase();
            }
            else
            {
                friendlyName = string.Format( "{0} (Plugin)", jobType.Split( '.' ).Last().SplitCase() );
            }
            return friendlyName;
        }

        #endregion
    }
}