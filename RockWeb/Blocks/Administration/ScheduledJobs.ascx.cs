//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Rock;
using Rock.Model;
using Rock.Util;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    public partial class ScheduledJobs : RockBlock
    {
        private JobService jobService = new JobService();


        #region Control Methods

        protected override void OnInit(EventArgs e)
        {
            if (CurrentPage.IsAuthorized("Configure", CurrentPerson))
            {
                grdScheduledJobs.DataKeyNames = new string[] { "id" };
                grdScheduledJobs.Actions.IsAddEnabled = true;
                grdScheduledJobs.Actions.AddClick += grdScheduledJobs_Add;
                grdScheduledJobs.GridRebind += grdScheduledJobs_GridRebind;
            }

            string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this job?');
                });
        });
    ";
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), string.Format("grid-confirm-delete-{0}", grdScheduledJobs.ClientID), script, true);

            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            nbMessage.Visible = false;
            if (CurrentPage.IsAuthorized("Configure", CurrentPerson))
            {
                if (!Page.IsPostBack)
                {
                    BindScheduledJobs();
                    foreach (int value in Enum.GetValues(typeof(JobNotificationStatus)))
                    {
                        string text = ((JobNotificationStatus)value).ToString();
                        ListItem item = new ListItem(text, value.ToString());
                        drpNotificationStatus.Items.Add(item);
                    }
                }
            }
            else
            {
                grdScheduledJobs.Visible = false;
                nbMessage.Text = "You are not authorized to edit campuses";
                nbMessage.Visible = true;
            }

            base.OnLoad(e);
        }

        #endregion

        protected void grdScheduledJobs_Add(object sender, EventArgs e)
        {
            grdScheduledJobs.SelectedIndex = -1;
            ResetDetailFields();
            pnlDetails.Visible = true;
            pnlGrid.Visible = false;
        }

        protected void grdScheduledJobs_GridRebind(object sender, EventArgs e)
        {
            BindScheduledJobs();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            int id = 0;
            if (grdScheduledJobs.SelectedIndex != -1)
            {
                id = (int)grdScheduledJobs.DataKeys[grdScheduledJobs.SelectedIndex].Value;
            }

            ServiceJob job = id > 0 ? jobService.Get(id) : new ServiceJob();
            if (job.Id <= 0)
            {
                jobService.Add(job, CurrentPersonId);
            }
            job.Name = tbName.Text;
            job.Description = tbDescription.Text;
            job.IsActive = cbActive.Checked;
            job.Assemby = tbAssembly.Text;
            job.Class = tbClass.Text;
            job.NotificationEmails = tbNotificationEmails.Text;
            job.NotificationStatus = (JobNotificationStatus)int.Parse(drpNotificationStatus.SelectedValue);
            job.CronExpression = tbCronExpression.Text;
            jobService.Save(job, CurrentPersonId);

            pnlDetails.Visible = false;
            pnlGrid.Visible = true;
            BindScheduledJobs();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlDetails.Visible = false;
            pnlGrid.Visible = true;
            grdScheduledJobs.SelectedIndex = -1;
        }

        protected void grdScheduledJobs_Delete(object sender, RowEventArgs e)
        {
            int id = (int)grdScheduledJobs.DataKeys[e.RowIndex].Value;
            ServiceJob job = jobService.Get(id);
            jobService.Delete(job, CurrentPersonId);
            jobService.Save(job, CurrentPersonId);
            BindScheduledJobs();
        }

        protected void grdScheduledJobs_Edit(object sender, RowEventArgs e)
        {
            pnlDetails.Visible = true;
            pnlGrid.Visible = false;

            grdScheduledJobs.SelectedIndex = e.RowIndex;
            int id = (int)grdScheduledJobs.DataKeys[e.RowIndex].Value;
            ServiceJob job = jobService.Get(id);
            tbName.Text = job.Name;
            tbDescription.Text = job.Description;
            cbActive.Checked = job.IsActive.HasValue ? job.IsActive.Value : false;
            tbAssembly.Text = job.Assemby;
            tbClass.Text = job.Class;
            tbNotificationEmails.Text = job.NotificationEmails;
            drpNotificationStatus.SelectedValue = ((int)job.NotificationStatus).ToString();
            tbCronExpression.Text = job.CronExpression;
            foreach (int value in Enum.GetValues(typeof(JobNotificationStatus)))
            {
                string text = ((JobNotificationStatus)value).ToString();
                ListItem item = new ListItem(text, value.ToString());
                drpNotificationStatus.Items.Add(item);
            }
        }

        private void BindScheduledJobs()
        {
            grdScheduledJobs.DataSource = jobService.GetActiveJobs().ToList();
            grdScheduledJobs.DataBind();
        }

        private void ResetDetailFields()
        {
            tbName.Text = String.Empty;
            tbDescription.Text = String.Empty;
            cbActive.Checked = true;
            tbAssembly.Text = String.Empty;
            tbClass.Text = String.Empty;
            tbNotificationEmails.Text = String.Empty;
            drpNotificationStatus.ClearSelection();
            tbCronExpression.Text = String.Empty;
        }

    }
}