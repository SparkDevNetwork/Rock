using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Ajax.Utilities;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Communication;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;


namespace RockWeb.Plugins.org_newpointe.TransferAssignee
{

    /// <summary>
    /// Block to pick a person and transfer their open Workflows and Connection Requests to someone else.
    /// </summary>
    [DisplayName("Transfer Assignee")]
    [Category("NewPointe Workflows")]
    [Description("Changes who is assigned to a workflow or connection request.")]



    public partial class TransferAssignee : Rock.Web.UI.RockBlock
    {
        //public Guid typeGuid;
        RockContext rockContext = new RockContext();

        public int? FromPersonId;
        public int? ToPersonId;



        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Returns the number of Workflows and Connection Requests for the selected Fron person, then populates grids.
        /// </summary>
        protected void UpdateUi()
        {
            // Get the selected person
            FromPersonId = ppFrom.PersonId;

            // Construct List
            var workflowResultList = new List<WorkflowActivity>();
            var connectionRequestResultList = new List<ConnectionRequest>();

            // Create Needed Services
            WorkflowActivityService workflowActivityService = new WorkflowActivityService(rockContext);
            ConnectionRequestService connectionRequestService = new ConnectionRequestService(rockContext);

            // Get Workflows from From person
            var assignedWorkflows =
                workflowActivityService.Queryable()
                    .Where(
                        a =>
                            a.AssignedPersonAliasId == FromPersonId && a.CompletedDateTime == null &&
                            a.Workflow.CompletedDateTime == null);


            // Get Connection Requests from From person
            var assignedConnectionRequests =
                connectionRequestService.Queryable()
                    .Where(
                        b =>
                            b.ConnectorPersonAliasId == FromPersonId && b.ConnectionState != ConnectionState.Connected &&
                            b.ConnectionState != ConnectionState.Inactive);

            // UI Updates
            if (ppFrom.PersonId != null)
            {
                foreach (var workflow in assignedWorkflows)
                {
                    workflowResultList.Add(workflow);
                }

                foreach (var connectionRequest in assignedConnectionRequests)
                {
                    connectionRequestResultList.Add(connectionRequest);
                }


                lbCount.Text = assignedWorkflows.Count() + " open workflows assigned to this person.<br />" +
                               assignedConnectionRequests.Count() + " open Connection Requests assigned to this person.";

                gWorkflows.DataSource = workflowResultList;
                gWorkflows.DataBind();

                gConnections.DataSource = connectionRequestResultList;
                gConnections.DataBind();

              

                if (assignedWorkflows.Any())
                {
                    btnSaveWorkflows.Visible = true;
                    ppTo.Visible = true;
                }

                if (assignedConnectionRequests.Any())
                {
                    btnSaveConnections.Visible = true;
                    ppTo.Visible = true;
                }
            }

            else
            {
                nbSuccess.Visible = false;
                nbWarningMessage.Visible = false;
            }


        }


        /// <summary>
        /// Updates the Workflows to us the new To person
        /// </summary>
        protected void UpdateWorkflows()
        {
            //Get the selected people
            FromPersonId = ppFrom.PersonId;
            ToPersonId = ppTo.PersonId;

            if (ppTo.PersonId != null)
            {
                // Create Needed Services
                WorkflowActivityService workflowActivityService = new WorkflowActivityService(rockContext);

                //Get Workflows from From person
                var assigned = workflowActivityService.Queryable().Where(a => a.AssignedPersonAliasId == FromPersonId);

                //Set each Workflow in From person to To person
                foreach (var x in assigned)
                {
                    x.AssignedPersonAliasId = ToPersonId;
                    x.ModifiedDateTime = DateTime.Now;
                    x.ModifiedByPersonAliasId = CurrentPersonAliasId;

                }

                //Save changes
                rockContext.SaveChanges();

                //Update UI
                nbSuccess.Visible = true;
                nbWarningMessage.Visible = false;
            }



        }

        /// <summary>
        /// Updates the Connection Requests to us the new To person
        /// </summary>
        protected void UpdateConnectionRequests()
        {
            //Get the selected people
            FromPersonId = ppFrom.PersonId;
            ToPersonId = ppTo.PersonId;

            // Create Needed Services
            ConnectionRequestService connectionRequestService = new ConnectionRequestService(rockContext);

            //Get Connection Requests from From person
            var assigned = connectionRequestService.Queryable().Where(a => a.ConnectorPersonAliasId == FromPersonId);

            //Set each Connection Request in From person to To person
            foreach (var x in assigned)
            {
                x.ConnectorPersonAliasId = ToPersonId;
                x.ModifiedDateTime = DateTime.Now;
                x.ModifiedByPersonAliasId = CurrentPersonAliasId;
            }

            //Save changes
            rockContext.SaveChanges();

            //Update UI
            nbSuccess.Visible = true;


        }

        /// <summary>
        /// Handles the OnSelect event of the From PersonPicker.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppFrom_OnSelect(object sender, EventArgs e)
        {
            UpdateUi();
        }


        /// <summary>
        /// Handles the OnClick event of the Update Workflows button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveWorkflows_Click(object sender, EventArgs e)
        {
            UpdateWorkflows();
            nbSuccess.Visible = true;
            nbWarningMessage.Visible = false;
            nbSuccess.Text = "Workflows were transfered.";
            UpdateUi();
        }


        /// <summary>
        /// Handles the OnClick event of the Update Connection Requests button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveConnections_Click(object sender, EventArgs e)
        {
            UpdateConnectionRequests();
            nbSuccess.Visible = true;
            nbWarningMessage.Visible = false;
            nbSuccess.Text = "Connection Requests were transfered.";
            UpdateUi();

        }


        /// <summary>
        /// Returns true if the form is valid; false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            // Check length in case the client side js didn't
            int charLimit = GetAttributeValue("CharacterLimit").AsInteger();

            if (errors != null && errors.Count() > 0)
            {
                nbWarningMessage.Visible = true;
                nbWarningMessage.Text = errors.Aggregate(new StringBuilder("<ul>"), (sb, s) => sb.AppendFormat("<li>{0}</li>", s)).Append("</ul>").ToString();
                return false;
            }
            else
            {
                nbWarningMessage.Visible = false;
                return true;
            }
        }
    }
}
