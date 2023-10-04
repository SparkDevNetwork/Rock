using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Plugins.org_lakepointe.Forms
{
    [DisplayName( "Employee Coaching" )]
    [Category( "LPC > Forms" )]
    [Description( "Form to be filled in jointly by the employee and their supervisor for periodic reviews." )]

    [CodeEditorField(
        name: "Confirmation Text Message Template",
        description: "The lava template for the message to show on the Confirmation page under normal circumstances.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "Your confirmation text goes here. (In HTML, please. Lava is also acceptable.)",
        order: 1 )]

    public partial class EmployeeCoaching : RockBlock
    {
        #region Fields

        private Guid EmployeeCoachingWorkflowTypeGuid = "9BB14104-B559-4243-BA89-B2BC3274A70D".AsGuid();

        private List<Person> _staff;
        private List<Person> Staff
        {
            get
            {
                if ( _staff == null )
                {
                    _staff = GetStaffMembers( new RockContext() );
                    var blankPerson = new Person();
                    blankPerson.Id = -1;
                    _staff.Insert( 0, blankPerson );
                }
                return _staff;
            }
        }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbCancel.Click += LbCancel_Click;
            lbSave.Click += lbSave_click;
            lbSubmit.Click += LbSubmit_Click;
            lbSupervisorSubmit.Click += LbSubmit_Click;

            BlockUpdated += EvaluationBlockUpdated;
            AddConfigurationUpdateTrigger( upEmployeeCoaching );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                InitializeForm();
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        #endregion

        #region Events

        protected void lbSave_click( object sender, EventArgs e )
        {
            SaveReview( hfCurrentOwner.Value );

            maAlert.Show( "Your information has been saved.", ModalAlertType.Information );
        }

        private void LbSubmit_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                SubmitReview();
            }
        }

        private void LbCancel_Click( object sender, EventArgs e )
        {
            Cancel();
        }

        protected void EvaluationBlockUpdated( object sender, EventArgs e )
        {
            InitializeForm();
        }

        #endregion

        #region Methods

        private List<Person> GetStaffMembers( RockContext context )
        {
            var taggedItemsQuery = new TaggedItemService( context ).Queryable()
                .AsNoTracking();

            var staffPeople = new PersonService( context ).Queryable().AsNoTracking()
                .Join( taggedItemsQuery, p => p.Guid, i => i.EntityGuid,
                    ( p, i ) => new { Person = p, TaggedItem = i } )
                .Where( i => i.TaggedItem.TagId == 1 )
                .Select( p => p.Person )
                .OrderBy( p => p.NickName )
                .ThenBy( p => p.LastName )
                .ToList();
            return staffPeople;
        }

        private void InitializeForm()
        {
            nbWarning.Text = string.Empty;

            using ( var context = new RockContext() )
            {
                dddlSupervisor.DataSource = Staff;
                dddlSupervisor.DataBind();

                int workflowId;
                string reportParam = Request.Params["report"];  // check querystring
                if ( !reportParam.IsNullOrWhiteSpace() )
                {
                    if ( !int.TryParse( reportParam, out workflowId ) )
                    {
                        nbWarning.Text = "Improperly formatted querystring.";
                        ScrollToTop();
                        return;
                    }

                    if ( !LoadReview( workflowId ) )
                    {
                        nbWarning.Text = "Error: specified report does not exist.";
                        pnlInfo.Visible = false;
                        nbInfo.Text = string.Empty;
                        pnlConfirmation.Visible = false;
                        pnlNavigation.Visible = false;
                        ScrollToTop();
                        return;
                    }
                }
                else // Otherwise first run ... initialize assuming employee
                {
                    InitializeReview();
                }

                bool isEmployee = CurrentPerson.Id == ppEmployee.SelectedValue;
                bool isSupervisor = dddlSupervisor.SelectedIndex >= 0 ? CurrentPerson.Id == Staff[dddlSupervisor.SelectedIndex].Id : false;
                bool isHR = new GroupMemberService( context ).Queryable().Where( gm => gm.GroupId == 799127 && gm.PersonId == CurrentPerson.Id ).Any();
                bool canEmployeeEdit = isEmployee && !hfCurrentOwner.Value.Equals( "HR" );
                bool canSupervisorEdit = isSupervisor && hfCurrentOwner.Value.Equals( "Supervisor" );

                bddlMonth.Enabled = canEmployeeEdit || canSupervisorEdit;
                dddlSupervisor.Enabled = ( canEmployeeEdit || isHR ) && !isSupervisor;
                ddlStatusFilter.Visible = isHR;
                ppEmployee.Enabled = false;
                tbPosition.Enabled = canEmployeeEdit || canSupervisorEdit;
                rsLove.Enabled = canEmployeeEdit;
                rsLoveSupervisor.Visible = true; // isSupervisor || isHR;
                rsLoveSupervisor.Enabled = canSupervisorEdit;
                rsHonor.Enabled = canEmployeeEdit;
                rsHonorSupervisor.Visible = true; // isSupervisor || isHR;
                rsHonorSupervisor.Enabled = canSupervisorEdit;
                rsFun.Enabled = canEmployeeEdit;
                rsFunSupervisor.Visible = true; // isSupervisor || isHR;
                rsFunSupervisor.Enabled = canSupervisorEdit;
                rsGreat.Enabled = canEmployeeEdit;
                rsGreatSupervisor.Visible = true; // isSupervisor || isHR;
                rsGreatSupervisor.Enabled = canSupervisorEdit;
                rsWhatever.Enabled = canEmployeeEdit;
                rsWhateverSupervisor.Visible = true; // isSupervisor || isHR;
                rsWhateverSupervisor.Enabled = canSupervisorEdit;
                rsLakepointe.Enabled = canEmployeeEdit;
                rsLakepointeSupervisor.Visible = true; // isSupervisor || isHR;
                rsLakepointeSupervisor.Enabled = canSupervisorEdit;
                rsExecution.Enabled = canEmployeeEdit;
                rsExecutionSupervisor.Visible = true;
                rsExecutionSupervisor.Enabled = canSupervisorEdit;
                rsJoy.Enabled = canEmployeeEdit;
                tbWins.Enabled = canEmployeeEdit;
                tbHelp.Enabled = canEmployeeEdit;
                tbStrengths.Enabled = canEmployeeEdit;
                tbOpportunities.Enabled = canEmployeeEdit;
                tbNextSteps.Enabled = canEmployeeEdit;
                tbBugs.Enabled = canEmployeeEdit;
                cbAcknowledge.Enabled = canEmployeeEdit;
                tbFeedback.Enabled = canSupervisorEdit;

                tbSupervisorConfidential.Visible = isSupervisor || isHR;
                tbSupervisorConfidential.Enabled = canSupervisorEdit;
                // tbHRConfidential.Visible = isHR;

                lbSave.Visible = isHR || canEmployeeEdit || canSupervisorEdit;
                lbSubmit.Visible = isEmployee;
                lbSupervisorSubmit.Visible = isSupervisor;
            }
        }

        private void SaveReview( string newOwner )
        {
            // ::: consider checking to see if the report has been modified externally before saving?
            // ::: or make sure it hasn't since been locked by the Supervisor (sent to HR).
            if ( ppEmployee.PersonId.HasValue )
            {
                using ( var context = new RockContext() )
                {
                    var personService = new PersonService( context );
                    var employee = personService.Get( ppEmployee.PersonId.Value );
                    var supervisor = Staff[dddlSupervisor.SelectedIndex];

                    var workflowId = hfWorkflowId.Value.AsIntegerOrNull();
                    if ( workflowId.HasValue )
                    {
                        var workflow = new WorkflowService( context ).Get( workflowId.Value );
                        workflow.LoadAttributes();

                        if ( !workflow.AttributeValues.ContainsKey( "LovesAndFollowsJesusSupervisor" ) ) // migration path for a few early reviews that didn't have these attributes
                        {
                            workflow.AttributeValues.Add( "LovesAndFollowsJesusSupervisor", null );
                            workflow.AttributeValues.Add( "HonorsUpDownAndAllAroundSupervisor", null );
                            workflow.AttributeValues.Add( "MakesItFunSupervisor", null );
                            workflow.AttributeValues.Add( "RejectsGoodForGreatSupervisor", null );
                            workflow.AttributeValues.Add( "WhateverItTakesSupervisor", null );
                            workflow.AttributeValues.Add( "LovesLakepointeSupervisor", null );
                        }

                        workflow.AttributeValues["Supervisor"].Value = supervisor.PrimaryAlias.Guid.ToString();
                        workflow.AttributeValues["Employee"].Value = employee.PrimaryAlias.Guid.ToString();
                        workflow.AttributeValues["ReportDate"].Value = dtpSubmitDate.SelectedDateTime.ToISO8601DateString();
                        workflow.AttributeValues["SupervisorSubmitDate"].Value = dtpSupervisorDate.SelectedDateTime.ToISO8601DateString();
                        workflow.AttributeValues["PositionTitle"].Value = tbPosition.Text;
                        workflow.AttributeValues["Month"].Value = bddlMonth.SelectedValue;
                        workflow.AttributeValues["LovesfollowsJesus"].Value = rsLove.SelectedValue.HasValue ? rsLove.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["LovesAndFollowsJesusSupervisor"].Value = rsLoveSupervisor.SelectedValue.HasValue ? rsLoveSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["HonorsUpDownAllAround"].Value = rsHonor.SelectedValue.HasValue ? rsHonor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["HonorsUpDownAndAllAroundSupervisor"].Value = rsHonorSupervisor.SelectedValue.HasValue ? rsHonorSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["MakesItFun"].Value = rsFun.SelectedValue.HasValue ? rsFun.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["MakesItFunSupervisor"].Value = rsFunSupervisor.SelectedValue.HasValue ? rsFunSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["RejectsGoodforGreat"].Value = rsGreat.SelectedValue.HasValue ? rsGreat.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["RejectsGoodForGreatSupervisor"].Value = rsGreatSupervisor.SelectedValue.HasValue ? rsGreatSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["WhateverItTakes"].Value = rsWhatever.SelectedValue.HasValue ? rsWhatever.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["WhateverItTakesSupervisor"].Value = rsWhateverSupervisor.SelectedValue.HasValue ? rsWhateverSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["LovesLakepointe"].Value = rsLakepointe.SelectedValue.HasValue ? rsLakepointe.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["LovesLakepointeSupervisor"].Value = rsLakepointeSupervisor.SelectedValue.HasValue ? rsLakepointeSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["Execution"].Value = rsExecution.SelectedValue.HasValue ? rsExecution.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["ExecutionSupervisor"].Value = rsExecutionSupervisor.SelectedValue.HasValue ? rsExecutionSupervisor.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["Joy"].Value = rsJoy.SelectedValue.HasValue ? rsJoy.SelectedValue.Value.ToString() : "";
                        workflow.AttributeValues["Wins"].Value = tbWins.Text;
                        workflow.AttributeValues["Help"].Value = tbHelp.Text;
                        workflow.AttributeValues["Strengths"].Value = tbStrengths.Text;
                        workflow.AttributeValues["Opportunities"].Value = tbOpportunities.Text;
                        workflow.AttributeValues["NextSteps"].Value = tbNextSteps.Text;
                        workflow.AttributeValues["Bugs"].Value = tbBugs.Text;
                        workflow.AttributeValues["Acknowledgement"].Value = cbAcknowledge.Checked ? "1" : "0";
                        workflow.AttributeValues["Comments"].Value = tbFeedback.Text;
                        workflow.AttributeValues["SupervisorConfidentialCommentstoHR"].Value = tbSupervisorConfidential.Text;
                        // workflow.AttributeValues["HRConfidentialComments"].Value = tbHRConfidential.Text;

                        if ( hfCurrentOwner.Value.Equals( "HR" ) )
                        {
                            workflow.AttributeValues["CurrentOwner"].Value = ddlStatusFilter.SelectedValue; // enables HR to send the report back to the employee or manager for cleanup
                        }
                        else
                        {
                            workflow.AttributeValues["CurrentOwner"].Value = newOwner;
                        }

                        workflow.SaveAttributeValues();
                        workflow.Status = "Completed";

                        context.SaveChanges();
                    }
                    else
                    {
                        var attributes = new Dictionary<string, string>();
                        attributes.Add( "Supervisor", supervisor.PrimaryAlias.Guid.ToString() );
                        attributes.Add( "Employee", employee.PrimaryAlias.Guid.ToString() );
                        attributes.Add( "ReportDate", dtpSubmitDate.SelectedDateTime.ToISO8601DateString() );
                        attributes.Add( "SupervisorSubmitDate", dtpSupervisorDate.SelectedDateTime.ToISO8601DateString() );
                        attributes.Add( "PositionTitle", tbPosition.Text );
                        attributes.Add( "Month", bddlMonth.SelectedValue );
                        attributes.Add( "LovesfollowsJesus", rsLove.SelectedValue.HasValue ? rsLove.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "LovesAndFollowsJesusSupervisor", rsLoveSupervisor.SelectedValue.HasValue ? rsLoveSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "HonorsUpDownAllAround", rsHonor.SelectedValue.HasValue ? rsHonor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "HonorsUpDownAndAllAroundSupervisor", rsHonorSupervisor.SelectedValue.HasValue ? rsHonorSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "MakesItFun", rsFun.SelectedValue.HasValue ? rsFun.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "MakesItFunSupervisor", rsFunSupervisor.SelectedValue.HasValue ? rsFunSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "RejectsGoodforGreat", rsGreat.SelectedValue.HasValue ? rsGreat.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "RejectsGoodForGreatSupervisor", rsGreatSupervisor.SelectedValue.HasValue ? rsGreatSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "WhateverItTakes", rsWhatever.SelectedValue.HasValue ? rsWhatever.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "WhateverItTakesSupervisor", rsWhateverSupervisor.SelectedValue.HasValue ? rsWhateverSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "LovesLakepointe", rsLakepointe.SelectedValue.HasValue ? rsLakepointe.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "LovesLakepointeSupervisor", rsLakepointeSupervisor.SelectedValue.HasValue ? rsLakepointeSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "Execution", rsExecution.SelectedValue.HasValue ? rsExecution.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "ExecutionSupervisor", rsExecutionSupervisor.SelectedValue.HasValue ? rsExecutionSupervisor.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "Joy", rsJoy.SelectedValue.HasValue ? rsJoy.SelectedValue.Value.ToString() : "" );
                        attributes.Add( "Wins", tbWins.Text );
                        attributes.Add( "Help", tbHelp.Text );
                        attributes.Add( "Strengths", tbStrengths.Text );
                        attributes.Add( "Opportunities", tbOpportunities.Text );
                        attributes.Add( "NextSteps", tbNextSteps.Text );
                        attributes.Add( "Bugs", tbBugs.Text );
                        attributes.Add( "Acknowledgement", cbAcknowledge.Checked ? "1" : "0" );
                        attributes.Add( "Comments", tbFeedback.Text );
                        attributes.Add( "CurrentOwner", newOwner );
                        attributes.Add( "SupervisorConfidentialCommentstoHR", tbSupervisorConfidential.Text );
                        // attributes.Add( "HRConfidentialComments", tbHRConfidential.Text );

                        hfWorkflowId.Value = SaveNewWorkflow( EmployeeCoachingWorkflowTypeGuid, attributes ).ToString();
                    }
                }
            }
            ScrollToTop();
        }

        private int SaveNewWorkflow( Guid? workflowTypeGuid, Dictionary<string, string> attributes )
        {
            using ( var rockContext = new RockContext() )
            {
                WorkflowTypeCache workflowType = null;
                if ( workflowTypeGuid.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                }

                if ( workflowType == null && workflowTypeGuid.HasValue )
                {
                    workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                }

                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, "" );
                    workflow.InitiatorPersonAliasId = CurrentPersonAliasId;

                    foreach ( var keyVal in attributes )
                    {
                        workflow.SetAttributeValue( keyVal.Key, keyVal.Value );
                    }

                    workflow.IsPersisted = true;
                    workflow.IsProcessing = true;

                    new WorkflowService( rockContext ).Add( workflow );

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                        workflow.SaveAttributeValues( rockContext );
                    } );

                    return workflow.Id;
                }
            }
            return 0;
        }

        private void Cancel()
        {
            // Flip block to the confirmation page
            pnlInfo.Visible = false;
            nbConfirmation.Title = "Canceled";
            nbConfirmation.Text = "No changes have been made.";
            nbInfo.Text = string.Empty;
            nbWarning.Text = string.Empty;
            pnlConfirmation.Visible = true;
            pnlNavigation.Visible = false;
        }

        private void SubmitReview()
        {
            bool isEmployee = CurrentPerson.Id == ppEmployee.SelectedValue;
            bool isSupervisor = dddlSupervisor.SelectedIndex >= 0 ? CurrentPerson.Id == Staff[dddlSupervisor.SelectedIndex].Id : false;
            if ( isEmployee )
            {
                dtpSubmitDate.SelectedDateTime = RockDateTime.Now;
                SaveReview( "Supervisor" );
                nbConfirmation.Text = "Your information has been submitted for your supervisor to review.";
            }
            else if ( isSupervisor )
            {
                dtpSupervisorDate.SelectedDateTime = RockDateTime.Now;
                SaveReview( "HR" );
                nbConfirmation.Text = "This form has been submitted to HR.";
            }

            // Flip block to the confirmation page
            pnlInfo.Visible = false;
            nbConfirmation.Title = "Success";
            nbConfirmation.Text = "Your information has been saved.";
            nbInfo.Text = string.Empty;
            nbWarning.Text = string.Empty;
            pnlConfirmation.Visible = true;
            pnlNavigation.Visible = false;
        }

        private bool LoadReview( int workflowId, bool copy = false )
        {
            using ( var context = new RockContext() )
            {
                var personAliasService = new PersonAliasService( context );
                var workflow = new WorkflowService( context ).Get( workflowId );
                if ( workflow == null )
                {
                    return false;
                }

                workflow.LoadAttributes();

                dddlSupervisor.SelectedIndex = Staff.FindIndex( p => p.Id != -1 && p.PrimaryAlias.Guid == workflow.AttributeValues["Supervisor"].Value.AsGuid() );
                ppEmployee.SetValue( personAliasService.Get( workflow.AttributeValues["Employee"].Value.AsGuid() ).Person );
                if ( !copy )
                {
                    // Once these date/time pickers are set they can't be unset, so don't set them if this is copying an old review to populate a new one
                    dtpSubmitDate.SelectedDateTime = workflow.AttributeValues["ReportDate"].Value.AsDateTime();
                    dtpSupervisorDate.SelectedDateTime = workflow.AttributeValues["SupervisorSubmitDate"].Value.AsDateTime();
                }
                tbPosition.Text = workflow.AttributeValues["PositionTitle"].Value;
                bddlMonth.SetValue( workflow.AttributeValues["Month"].Value );
                rsLove.SelectedValue = GetIndexValue( workflow, "LovesfollowsJesus" );
                rsLoveSupervisor.SelectedValue = GetIndexValue( workflow, "LovesAndFollowsJesusSupervisor" );
                rsHonor.SelectedValue = GetIndexValue( workflow, "HonorsUpDownAllAround" );
                rsHonorSupervisor.SelectedValue = GetIndexValue( workflow, "HonorsUpDownAndAllAroundSupervisor" );
                rsFun.SelectedValue = GetIndexValue( workflow, "MakesItFun" );
                rsFunSupervisor.SelectedValue = GetIndexValue( workflow, "MakesItFunSupervisor" );
                rsGreat.SelectedValue = GetIndexValue( workflow, "RejectsGoodforGreat" );
                rsGreatSupervisor.SelectedValue = GetIndexValue( workflow, "RejectsGoodForGreatSupervisor" );
                rsWhatever.SelectedValue = GetIndexValue( workflow, "WhateverItTakes" );
                rsWhateverSupervisor.SelectedValue = GetIndexValue( workflow, "WhateverItTakesSupervisor" );
                rsLakepointe.SelectedValue = GetIndexValue( workflow, "LovesLakepointe" );
                rsLakepointeSupervisor.SelectedValue = GetIndexValue( workflow, "LovesLakepointeSupervisor" );
                rsExecution.SelectedValue = GetIndexValue( workflow, "Execution" );
                rsExecutionSupervisor.SelectedValue = GetIndexValue( workflow, "ExecutionSupervisor" );
                rsJoy.SelectedValue = GetIndexValue( workflow, "Joy" );
                tbWins.Text = workflow.AttributeValues["Wins"].Value;
                tbHelp.Text = workflow.AttributeValues["Help"].Value;

                // These won't exist in old forms, will result in error
                try
                {
                    tbStrengths.Text = workflow.AttributeValues["Strengths"].Value;
                    tbOpportunities.Text = workflow.AttributeValues["Opportunities"].Value;
                    tbNextSteps.Text = workflow.AttributeValues["NextSteps"].Value;
                }
                catch
                {
                    // Continue.. Errors are expected
                }

                tbBugs.Text = workflow.AttributeValues["Bugs"].Value;
                cbAcknowledge.Checked = workflow.AttributeValues.ContainsKey( "Acknowledgement" ) ? workflow.AttributeValues["Acknowledgement"].Value.Equals( "1" ) : false;
                tbFeedback.Text = workflow.AttributeValues["Comments"].Value;
                hfCurrentOwner.Value = workflow.AttributeValues["CurrentOwner"].Value;
                ddlStatusFilter.SelectedValue = workflow.AttributeValues["CurrentOwner"].Value;
                hfWorkflowId.Value = workflowId.ToString();
                tbSupervisorConfidential.Text = workflow.AttributeValues["SupervisorConfidentialCommentstoHR"].Value;
                // tbHRConfidential.Text = workflow.AttributeValues["HRConfidentialComments"].Value;
            }
            return true;
        }

        private static int? GetIndexValue( Workflow workflow, string key, int d = 5 )
        {
            if ( workflow.AttributeValues.ContainsKey( key ) )
            {
                if ( workflow.AttributeValues[key].Value.IsNotNullOrWhiteSpace() )
                {
                    return workflow.AttributeValues[key].Value.AsIntegerOrNull();
                }
            }
            return d;
        }

        private void InitializeReview()
        {
            using ( var context = new RockContext() )
            {
                var personAliasService = new PersonAliasService( context );
                var workflowService = new WorkflowService( context );
                var attributeValueService = new AttributeValueService( context );

                // Look for last month's review and use it to pre-populate
                var employeeWorkflowIds = attributeValueService.Queryable()
                    .Where( av => av.AttributeId == 70872 && av.Value == CurrentPerson.PrimaryAlias.Guid.ToString() )  // employee attribute id
                    .Select( av => av.EntityId );
                var lastMonth = RockDateTime.Today.AddMonths( -1 );
                var lastMonthIds = attributeValueService.Queryable()
                    .Where( av => av.AttributeId == 70870 // month attribute id
                     && employeeWorkflowIds.Contains( av.EntityId ) // for this employee
                     && av.Value == lastMonth.Month.ToString()
                     && av.CreatedDateTime.Value.Year == lastMonth.Year )
                    .Select( av => av.EntityId );

                if ( lastMonthIds.Any() )
                {
                    if ( LoadReview( lastMonthIds.FirstOrDefault().Value, true ) )
                    {
                        // Note that setting these two date pickers to null doesn't actually work because of the implementation
                        // of the datepicker control. We had to hack LoadReview() to not set them instead. Left here for clarity.
                        dtpSubmitDate.SelectedDateTime = null; // wipe submission date
                        dtpSupervisorDate.SelectedDateTime = null;

                        bddlMonth.SetValue( RockDateTime.Today.Month );
                        hfWorkflowId.Value = string.Empty;  // force data to be saved to a new workflow
                        hfCurrentOwner.Value = "Employee";
                        ddlStatusFilter.SelectedValue = "Employee";
                        cbAcknowledge.Checked = false;
                        tbFeedback.Text = "[This was last month's feedback and is included for your guidance.]\n" +
                            tbFeedback.Text;
                        tbSupervisorConfidential.Text = string.Empty;
                        // tbHRConfidential.Text = string.Empty;
                        nbInfo.Text = "Please Note: This form has been pre-populated with last month's data. Be sure to review all information before submitting.";
                        return;
                    }
                }

                // No prior data to work with ... minimal presets
                ppEmployee.SetValue( CurrentPerson );
                bddlMonth.SetValue( RockDateTime.Today.Month );
                rsLove.SelectedValue = 5;
                rsLoveSupervisor.SelectedValue = 5;
                rsHonor.SelectedValue = 5;
                rsHonorSupervisor.SelectedValue = 5;
                rsFun.SelectedValue = 5;
                rsFunSupervisor.SelectedValue = 5;
                rsGreat.SelectedValue = 5;
                rsGreatSupervisor.SelectedValue = 5;
                rsWhatever.SelectedValue = 5;
                rsWhateverSupervisor.SelectedValue = 5;
                rsLakepointe.SelectedValue = 5;
                rsLakepointeSupervisor.SelectedValue = 5;
                rsExecution.SelectedValue = 5;
                rsExecutionSupervisor.SelectedValue = 5;
                rsJoy.SelectedValue = 5;
                hfCurrentOwner.Value = "Employee";
                ddlStatusFilter.SelectedValue = "Employee";
                hfWorkflowId.Value = string.Empty;
            }
        }

        private void ScrollToTop( int intPosY = 0 )
        {
            string strScript = @"var manager = Sys.WebForms.PageRequestManager.getInstance(); 
            manager.add_beginRequest(beginRequest); 
            function beginRequest() 
            { 
                manager._scrollPosition = null; 
            }
            window.scroll(0, " + intPosY.ToString() + ");";

            ScriptManager.RegisterStartupScript( upEmployeeCoaching, upEmployeeCoaching.GetType(), "Error_" + RockDateTime.Now.Ticks.ToString(), strScript, true );

            return;
        }

        #endregion

        protected void CheckBoxRequired_ServerValidate( object source, System.Web.UI.WebControls.ServerValidateEventArgs args )
        {
            args.IsValid = CurrentPerson.Id != ppEmployee.SelectedValue || cbAcknowledge.Checked;
        }

        protected void SupervisorConfidential_ServerValidate( object source, System.Web.UI.WebControls.ServerValidateEventArgs args )
        {
            args.IsValid = ( dddlSupervisor.SelectedIndex >= 0 && CurrentPerson.Id == Staff[dddlSupervisor.SelectedIndex].Id ) || tbSupervisorConfidential.Text.IsNotNullOrWhiteSpace();
        }
    }
}