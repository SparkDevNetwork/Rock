using Newtonsoft.Json.Linq;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.UI;

namespace RockWeb.Plugins.org_lakepointe.Forms
{
    [DisplayName("Employee Annual Evaluation")]
    [Category("LPC > Forms")]
    [Description("Form to be filled in by an employee's supervisor annually to review performance.")]

    [CodeEditorField(
        name: "Confirmation Text Message Template",
        description: "The lava template for the message to show on the Confirmation page under normal circumstances.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "Your confirmation text goes here. (In HTML, please. Lava is also acceptable.)",
        order: 1)]

    public partial class EmployeeEvaluation : RockBlock
    {
        #region Fields

        private Guid EmployeeEvaluationWorkflowTypeGuid = "11B2AFB3-74C5-470C-87E2-3430F78F66A0".AsGuid();

        #endregion

        #region Base Control Methods

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            lbSave.Click += lbSave_click;

            BlockUpdated += EvaluationBlockUpdated;
            AddConfigurationUpdateTrigger(upEmployeeEvaluation);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                InitializeForm();
            }
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
        }

        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbSave_click(object sender, EventArgs e)
        {
            SaveReview();
        }

        protected void EvaluationBlockUpdated(object sender, EventArgs e)
        {
            InitializeForm();
        }

        #endregion

        #region Methods

        private void InitializeForm()
        {
            nbWarning.Text = string.Empty;

            lSupervisor.Text = CurrentPerson.FullName;
            bool isSupervisor = true;

            int workflowId;
            string reportParam = Request.Params["report"];  // check querystring
            if (!reportParam.IsNullOrWhiteSpace())
            {
                if (!int.TryParse(reportParam, out workflowId))
                {
                    nbWarning.Text = "Improperly formatted querystring.";
                    ScrollToTop();
                    return;
                }
                isSupervisor = false;

                LoadReview(workflowId);
            }

            ppEmployee.Enabled = isSupervisor;
            tbPosition.Enabled = isSupervisor;
            rblCurrent.Enabled = isSupervisor;
            rsLove.Enabled = isSupervisor;
            rsHonor.Enabled = isSupervisor;
            rsFun.Enabled = isSupervisor;
            rsGreat.Enabled = isSupervisor;
            rsWhatever.Enabled = isSupervisor;
            rsLakepointe.Enabled = isSupervisor;
            rsExpertise.Enabled = isSupervisor;
            rsExecution.Enabled = isSupervisor;
            tbComments.Enabled = isSupervisor;
            lbSave.Visible = isSupervisor;
        }

        private void SaveReview()
        {
            if (ppEmployee.PersonId.HasValue)
            {
                using (var context = new RockContext())
                {
                    var employee = new PersonService(context).Get(ppEmployee.PersonId.Value);

                    var attributes = new Dictionary<string, string>();
                    attributes.Add("Supervisor", CurrentPersonAlias.Guid.ToString());
                    attributes.Add("Employee", employee.PrimaryAlias.Guid.ToString());
                    attributes.Add("ReportDate", RockDateTime.Now.ToISO8601DateString());
                    attributes.Add("PositionTitle", tbPosition.Text);
                    attributes.Add("IsCurrent", rblCurrent.SelectedValue == "True" ? "1" : "2");
                    attributes.Add("LovesfollowsJesus", rsLove.SelectedValue.HasValue ? rsLove.SelectedValue.Value.ToString() : "");
                    attributes.Add("HonorsUpDownAllAround", rsHonor.SelectedValue.HasValue ? rsHonor.SelectedValue.Value.ToString() : "");
                    attributes.Add("MakesItFun", rsFun.SelectedValue.HasValue ? rsFun.SelectedValue.Value.ToString() : "");
                    attributes.Add("RejectsGoodforGreat", rsGreat.SelectedValue.HasValue ? rsGreat.SelectedValue.Value.ToString() : "");
                    attributes.Add("WhateverItTakes", rsWhatever.SelectedValue.HasValue ? rsWhatever.SelectedValue.Value.ToString() : "");
                    attributes.Add("LovesLakepointe", rsLakepointe.SelectedValue.HasValue ? rsLakepointe.SelectedValue.Value.ToString() : "");
                    attributes.Add("LevelOfExpertise", rsExpertise.SelectedValue.HasValue ? rsExpertise.SelectedValue.Value.ToString() : "");
                    attributes.Add("LevelofExecution", rsExecution.SelectedValue.HasValue ? rsExecution.SelectedValue.Value.ToString() : "");
                    attributes.Add("Comments", tbComments.Text);

                    employee.LaunchWorkflow(EmployeeEvaluationWorkflowTypeGuid, "", attributes, null);

                    // Flip block to the confirmation page
                    pnlInfo.Visible = false;
                    nbConfirmation.Title = "Success";
                    nbConfirmation.Text = "Your information has been submitted.";
                    pnlConfirmation.Visible = true;
                    pnlNavigation.Visible = false;
                }
            }
            ScrollToTop();
        }

        private void LoadReview(int workflowId)
        {
            using (var context = new RockContext())
            {
                var personAliasService = new PersonAliasService(context);
                var workflow = new WorkflowService(context).Get(workflowId);
                workflow.LoadAttributes();
                lSupervisor.Text = personAliasService.Get(workflow.AttributeValues["Supervisor"].Value.AsGuid()).Person.FullName;
                ppEmployee.SetValue(personAliasService.Get(workflow.AttributeValues["Employee"].Value.AsGuid()).Person);
                dpDate.Visible = true;
                dpDate.SelectedDate = workflow.AttributeValues["ReportDate"].Value.AsDateTime();
                dpDate.Enabled = false;
                tbPosition.Text = workflow.AttributeValues["PositionTitle"].Value;
                rblCurrent.SelectedValue = workflow.AttributeValues["IsCurrent"].Value == "1" ? "True" : "False";
                rsLove.SelectedValue = workflow.AttributeValues["LovesfollowsJesus"].Value.AsIntegerOrNull();
                rsHonor.SelectedValue = workflow.AttributeValues["HonorsUpDownAllAround"].Value.AsIntegerOrNull();
                rsFun.SelectedValue = workflow.AttributeValues["MakesItFun"].Value.AsIntegerOrNull();
                rsGreat.SelectedValue = workflow.AttributeValues["RejectsGoodforGreat"].Value.AsIntegerOrNull();
                rsWhatever.SelectedValue = workflow.AttributeValues["WhateverItTakes"].Value.AsIntegerOrNull();
                rsLakepointe.SelectedValue = workflow.AttributeValues["LovesLakepointe"].Value.AsIntegerOrNull();
                rsExpertise.SelectedValue = workflow.AttributeValues["LevelOfExpertise"].Value.AsIntegerOrNull();
                rsExecution.SelectedValue = workflow.AttributeValues["LevelofExecution"].Value.AsIntegerOrNull();
                tbComments.Text = workflow.AttributeValues["Comments"].Value;
            }
        }

        private void ScrollToTop(int intPosY = 0)
        {
            string strScript = @"var manager = Sys.WebForms.PageRequestManager.getInstance(); 
            manager.add_beginRequest(beginRequest); 
            function beginRequest() 
            { 
                manager._scrollPosition = null; 
            }
            window.scroll(0, " + intPosY.ToString() + ");";

            ScriptManager.RegisterStartupScript(upEmployeeEvaluation, upEmployeeEvaluation.GetType(), "Error_" + RockDateTime.Now.Ticks.ToString(), strScript, true);

            return;
        }

        #endregion
    }
}