using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using PayPal.Payments.Transactions;
using Rock;
using Rock.Data;
using Rock.Model;

namespace Plugins.org_newpointe.TransferAssignee
{

    /// <summary>
    /// Block to pick a person and transfer their open Workflows and Connection Requests to someone else.
    /// </summary>
    [DisplayName("Transfer Stakeholder")]
    [Category("NewPointe Workflows")]
    [Description("Changes who is assigned as the stakeholder on an open workflow.")]



    public partial class TransferStakeholder : Rock.Web.UI.RockBlock
    {
        //public Guid typeGuid;
        RockContext rockContext = new RockContext();

        public int? FromPersonId;
        public int? ToPersonId;



        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Returns the number of Workflows for the selected From person, then populates grids.
        /// </summary>
        protected void UpdateUi()
        {
            // Get the selected person
            FromPersonId = ppFrom.PersonId;

            // Create Needed Services
            WorkflowService workflowService = new WorkflowService(rockContext);
            AttributeValueService attributeValueService = new AttributeValueService(rockContext);

            // Get Workflows from From person
            var activeWorkflows = workflowService.Queryable().Where(w => w.CompletedDateTime == null );

            var assignedWorkflows =
                attributeValueService.Queryable()
                    .Where(
                        c =>
                            c.Attribute.Name.Contains("Stakeholder") && c.Attribute.EntityTypeId == 113 &&
                            c.ValueAsPersonId == FromPersonId);

            var activeAssignedWfs = from asW in assignedWorkflows
                join actW in activeWorkflows on asW.EntityId equals actW.Id into wj
                //where wj.FirstOrDefault() != null
                select asW;
            



            // UI Updates
            if (ppFrom.PersonId != null)
            {

                lbCount.Text = activeAssignedWfs.Count() + " open workflows assigned have this person as the stakeholder.";

                //gWorkflows.DataSource = activeAssignedWfs.ToList();
                //gWorkflows.DataBind();

                
                if (assignedWorkflows.Any())
                {
                    btnSaveWorkflows.Visible = true;
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
        /// Updates the workflows to use the new To person
        /// </summary>
        protected void UpdateWorkflows()
        {
            //Get the selected people
            FromPersonId = ppFrom.PersonId;
            ToPersonId = ppTo.PersonId;

            // Create Needed Services
            WorkflowService workflowService = new WorkflowService(rockContext);
            AttributeValueService attributeValueService = new AttributeValueService(rockContext);
            PersonService personService = new PersonService(rockContext);

            // Get Person Object of ToPerson
            var toPerson = personService.Queryable().FirstOrDefault(a => a.Id == ToPersonId);
            
            // Get Workflows from From person
            var activeWorkflows = workflowService.Queryable().Where(w => w.CompletedDateTime == null);


            var assignedWorkflows =
                attributeValueService.Queryable()
                    .Where(
                        c =>
                            c.Attribute.Name.Contains("Stakeholder") && c.Attribute.EntityTypeId == 113 &&
                            c.ValueAsPersonId == FromPersonId);

            var activeAssignedWfs = from asW in assignedWorkflows
                                    join actW in activeWorkflows on asW.EntityId equals actW.Id into wj
                                    //where wj.FirstOrDefault() != null
                                    select asW;

            //Set each Workflow in From person to To person
            foreach (var x in activeAssignedWfs)
            {
                x.Value = toPerson.PrimaryAlias.Guid.ToString();
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
