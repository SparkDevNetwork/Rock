using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Workflow;

namespace RockWeb.Plugins.org_lakepointe.Connections
{
    [DisplayName("Connection Signup")]
    [Category("LPC > Connections")]
    [Description("Form that will allow a person to sign up for a connection opportunity.")]

    /*General Configuration */
    [BooleanField(
        name: "Enable Person Edit",
        description: "Enable Name and Birthday edit for authenticated people.",
        defaultValue: true,
        order: 0)]
    [CodeEditorField(
        name: "Intro Message Template",
        description: "The lava template for the message to show on the Welcome Screen.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        order: 1)]
    [CodeEditorField(
        name: "Confirmation Message Template",
        description: "The lava template for the message to show on the Confirmation Screen.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        order: 2)]
    [LavaCommandsField(
        name: "Enabled Lava Commands",
        description: "The Lava commans that are enabled for this block",
        required: false,
        defaultValue: "",
        order: 3)]
    [WorkflowTypeField(
        name: "Connection Workflow",
        description: "The workflow that this form activates. The workflow should accept a First Name, Last Name, Email, Phone, Birthday, and Connection Opportunity.",
        allowMultiple: false,
        required: true,
        defaultWorkflowTypeGuid: "",
        order: 4)]
    [LinkedPage(
        name: "Opportunity Home Page",
        description: "Serving/Connection opportunity home page.",
        required: true,
        order: 5)]


    public partial class ConnectionOpportunitySignup : RockBlock
    {
        #region Fields
        RockContext _context;
        Guid SERVING_AREA_DEFINED_VALUE_GUID = "edf53115-1066-4767-a808-f19a27a1cde3".AsGuid();
        DefinedValueCache _servingArea;
        CampusCache _campus;
        Person _selectedPerson;
        #endregion

        #region Properties
        private Guid? PersonGuid { get; set; }
        private int? ServingAreaId { get; set; }
        private int? CampusId { get; set; }

        private CampusCache Campus
        {
            get
            {
                if (_campus == null && CampusId.HasValue)
                {
                    _campus = CampusCache.Get(CampusId.Value, _context);
                }
                return _campus;
            }
            set
            {
                _campus = value;

                if (value != null)
                {
                    CampusId = value.Id;
                }
                else
                {
                    CampusId = null;
                }
            }
        }

        private Person SelectedPerson
        {
            get
            {
                if (_selectedPerson == null && PersonGuid.HasValue)
                {
                    _selectedPerson = new PersonService(_context).Get(PersonGuid.Value);
                }
                return _selectedPerson;
            }
            set
            {
                _selectedPerson = value;

                if (value != null)
                {
                    PersonGuid= value.Guid;
                }
                else
                {
                    PersonGuid = null;
                }
            }
        }

        private DefinedValueCache ServingArea
        {
            get
            {
                if (_servingArea == null && ServingAreaId.HasValue)
                {
                    _servingArea = DefinedValueCache.Get(ServingAreaId.Value, _context);
                }

                return _servingArea;
            }
            set
            {
                _servingArea = value;

                if (value != null)
                {
                    ServingAreaId = value.Id;
                }
                else
                {
                    ServingAreaId = null;
                }
            }
        }

        #endregion

        #region Base Control Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lbSubmit.Click += lbSubmit_Click;
            lbReset.Click += lbReset_Click;

            if (_context == null)
            {
                _context = new RockContext();
            }

            this.BlockUpdated += ConnectionOpportunitySignup_BlockUpdated;
            this.AddConfigurationUpdateTrigger(upConnectionOpportunitySignup);

         
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
            PersonGuid = (Guid?)ViewState["PersonGuid"];
            CampusId = (int?)ViewState["CampusId"];
            ServingAreaId = (int?)ViewState["AreaId"];

        }

        protected override object SaveViewState()
        {
            ViewState["PersonGuid"] = PersonGuid;
            ViewState["CampusId"] = CampusId;
            ViewState["AreaId"] = ServingAreaId;
            return base.SaveViewState();
        }
        #endregion

        #region Events

        private void ConnectionOpportunitySignup_BlockUpdated(object sender, EventArgs e)
        {
            InitializeForm();
        }
        private void lbReset_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadPersonFields();
        }

        private void lbSubmit_Click(object sender, EventArgs e)
        {
            StartConnectionWorkflow();
            pnlSignupForm.Visible = false;
            pnlConfirmation.Visible = true;
            LoadConfirmationLava();
        }
        #endregion

        #region Methods

        private void ClearFields()
        {
            tbFirstName.Text = string.Empty;
            tbLastName.Text = string.Empty;
            dpBirthDate.SelectedDate = null;
            tbEmail.Text = string.Empty;
            tbPhone.Text = string.Empty;
            dlOpportunities.SelectedIndex = 0;
            tbAdditionalInfo.Text = string.Empty;

        }

        private void InitializeForm()
        {
            pnlSignupForm.Visible = false;

            SelectedPerson = CurrentPerson;

            var campusQSValue = PageParameter("CampusId").AsIntegerOrNull();
            var areaQSValue = PageParameter("AreaId").AsIntegerOrNull();
     
            if (campusQSValue.HasValue)
            {
                Campus = CampusCache.Get(campusQSValue.Value, _context);
            }

            var servingAreaDefinedType = DefinedTypeCache.Get(SERVING_AREA_DEFINED_VALUE_GUID, _context);

            if (areaQSValue.HasValue)
            {
                ServingArea = servingAreaDefinedType.DefinedValues
                    .Where(v => v.Id == areaQSValue)
                    .Where(v => v.IsActive)
                    .SingleOrDefault();
            }


            if (Campus == null || ServingArea == null)
            {
                RedirectToOpportunityHome();
                return;
            }

            lCampus.Text = Campus.Name;

            LoadServingOpportunities();
            LoadPersonFields();
            LoadIntroLava();

            pnlSignupForm.Visible = true;
        }

        private void LoadConfirmationLava()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            lConfirmation.Text = GetAttributeValue("ConfirmationMessageTemplate").ResolveMergeFields(mergeFields, GetAttributeValue("EnabledLavaCommands"));
        }
        private void LoadIntroLava()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false });
            lIntro.Text = GetAttributeValue("IntroMessageTemplate").ResolveMergeFields(mergeFields, GetAttributeValue("EnabledLavaCommands"));      
        }

        private void LoadPersonFields()
        {
            bool allowPersonEdit = SelectedPerson == null || GetAttributeValue("EnablePersonEdit").AsBoolean(false);

            tbFirstName.Enabled = allowPersonEdit;
            tbLastName.Enabled = allowPersonEdit;
            dpBirthDate.Enabled = allowPersonEdit;

            if (SelectedPerson != null)
            {
                var mobilePhone = SelectedPerson.GetPhoneNumber(Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());

                tbFirstName.Text = SelectedPerson.NickName.IsNullOrWhiteSpace() ? SelectedPerson.FirstName : SelectedPerson.NickName;
                tbLastName.Text = SelectedPerson.LastName;
                dpBirthDate.SelectedDate = SelectedPerson.BirthDate;
                tbEmail.Text = SelectedPerson.Email;
                tbPhone.Number = mobilePhone != null ? mobilePhone.NumberFormatted : String.Empty;

            }

        }

        private void LoadServingOpportunities()
        {
            dlOpportunities.Items.Clear();

            if (Campus == null || ServingArea == null)
            {
                return;
            }

            var connectionTypeGuid = ServingArea.GetAttributeValue("ConnectionType").AsGuidOrNull();
            if (connectionTypeGuid == null)
            {
                return;
            }

            var connectionType = new ConnectionTypeService(_context).Queryable("ConnectionOpportunities.ConnectionOpportunityCampuses")
                .AsNoTracking()
                .Where(t => t.IsActive)
                .Where(t => t.Guid == connectionTypeGuid.Value)
                .SingleOrDefault();

            if (connectionType == null)
            {
                return;
            }

            var opportunities = connectionType.ConnectionOpportunities
                .Where(o => o.IsActive)
                .Where(o => o.ConnectionOpportunityCampuses.Select(c => c.CampusId).Contains(Campus.Id))
                .OrderBy(o => o.PublicName)
                .ToList();

            dlOpportunities.DataSource = opportunities;
            dlOpportunities.DataValueField = "Guid";
            dlOpportunities.DataTextField = "PublicName";
            dlOpportunities.DataBind();

            dlOpportunities.Items.Insert(0, new ListItem("", ""));


        }

        private void RedirectToOpportunityHome()
        {
            if (IsUserAuthorized("Edit"))
            {
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.AppendFormat("If you could not edit this page, you would have been redirected to <a href=\"{0}\">{0}</a> due to the following errors.<br /> ", LinkedPageUrl("OpportunityHomePage"));
                errorMessage.Append("<ul>");
                if (Campus == null)
                {
                    errorMessage.Append("<li>Campus selection was invalid or not provided.</li>");
                }
                if (ServingArea == null)
                {
                    errorMessage.Append("<li>Ministry Area selection was invalid or not provided.</li>");
                }
                errorMessage.Append("</ul>");

                nbWarning.Title = "Error";
                nbWarning.Text = errorMessage.ToString();
            }
            else
            {
                NavigateToLinkedPage("OpportunityHomePage");
            }

        }

        private void StartConnectionWorkflow()
        {
            var workflowGuid = GetAttributeValue("ConnectionWorkflow").AsGuid();

            var workflowType = new WorkflowTypeService(_context).Get(workflowGuid);

            if (workflowType == null)
            {
                throw new Exception("Signup workflow type not provided or is invalid.");
            }

            var title = string.Format("Volunteer Info Request - {0} {1} - {2}", tbFirstName.Text, tbLastName.Text, ServingArea.Value);
            var phoneNumberFormatted = PhoneNumber.FormattedNumber("1", PhoneNumber.CleanNumber(tbPhone.Number.Trim()), false);

            Rock.Transactions.LaunchWorkflowTransaction transaction = null;

            var workflowAttributes = new Dictionary<string, string>();
            workflowAttributes.Add("FirstName", tbFirstName.Text.Trim());
            workflowAttributes.Add("LastName", tbLastName.Text.Trim());
            workflowAttributes.Add("BirthDate", dpBirthDate.SelectedDate.Value.ToShortDateString());
            workflowAttributes.Add("Phone", phoneNumberFormatted);
            workflowAttributes.Add("Email", tbEmail.Text.Trim());
            workflowAttributes.Add("ConnectionOpportunity", dlOpportunities.SelectedValueAsGuid().ToString());
            workflowAttributes.Add("Campus", Campus.Guid.ToString());

            if (!tbAdditionalInfo.Text.IsNotNullOrWhiteSpace())
            {
                workflowAttributes.Add("MoreInformation", tbAdditionalInfo.Text.Trim());
            }

            if (SelectedPerson == null)
            {
                transaction = new Rock.Transactions.LaunchWorkflowTransaction(workflowGuid, title);
            }
            else
            {
                transaction = new Rock.Transactions.LaunchWorkflowTransaction<Person>(workflowGuid, title, SelectedPerson.Id);
            }
            transaction.WorkflowAttributeValues = workflowAttributes;
            Rock.Transactions.RockQueue.GetStandardQueuedTransactions().Add(transaction);

        }

        #endregion 
    }
}