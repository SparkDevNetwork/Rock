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
using System.IO;
using System.Web.UI;

namespace RockWeb.Plugins.org_lakepointe.Crm
{
    [DisplayName( "Student Registration" )]
    [Category( "LPC > CRM" )]
    [Description( "Form that will allow a new student to register for check-in." )]

    [CampusField(
        name: "Campus",
        description: "The campus that will be assigned to all registrations from this page.",
        required: false,
        defaultCampusId: "3",
        category: "",
        order: 0,
        includeInactive: false )]
    [CodeEditorField(
        name: "Intro Message Template",
        description: "The lava template for the message to show on the Welcome Screen.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        order: 1 )]
    [CodeEditorField(
        name: "Confirmation Message Template",
        description: "The lava template for the message to show on the Confirmation Screen.",
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        height: 200,
        required: true,
        defaultValue: "",
        order: 2 )]
    [LavaCommandsField(
        name: "Enabled Lava Commands",
        description: "The Lava commans that are enabled for this block",
        required: false,
        defaultValue: "",
        order: 3 )]
    [WorkflowTypeField(
        name: "Registration Workflow",
        description: "The workflow that this form activates to complete the student registration.",
        allowMultiple: false,
        required: true,
        defaultWorkflowTypeGuid: "",
        order: 4 )]
    [BooleanField(
        name: "Display Campus Picker",
        description: "Display a campus picker control.",
        defaultValue: false,
        order: 5 )]


    public partial class StudentRegistration : RockBlock
    {
        #region Fields

        RockContext _context;

        #endregion

        #region Properties

        protected int PhotoId { get; set; }
        protected string HomeLocationGuid { get; set; }
        protected string Program { get; set; }
        protected CampusCache Campus { get; set; }
        protected bool ShowCampusSelector { get; set; }

        #endregion

        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbNext.Click += LbNext_Click;

            lbSubmit.Click += lbSubmit_Click;
            lbReset.Click += lbReset_Click;
            lbCancel.Click += LbCancel_Click;
            lbFinished.Click += lbFinished_Click;

            if ( _context == null )
            {
                _context = new RockContext();
            }

            this.BlockUpdated += OnBlockUpdated;
            this.AddConfigurationUpdateTrigger( upStudentRegistration );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ClearFields();
                InitializeForm();
            }
        }

        protected override object SaveViewState()
        {
            ViewState["CampusId"] = Campus != null ? Campus.Id : ( int? ) null;
            ViewState["Program"] = Program;
            return base.SaveViewState();
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            var campusId = ViewState["CampusId"] as int?;

            if ( campusId.HasValue )
            {
                Campus = CampusCache.Get( campusId.Value );
            }

            Program = ViewState["Program"] as String;

        }

        #endregion

        #region Events

        private void OnBlockUpdated( object sender, EventArgs e )
        {
            InitializeForm();
        }

        private void lbReset_Click( object sender, EventArgs e )
        {
            ClearFields();
        }

        private void lbSubmit_Click( object sender, EventArgs e )
        {
            SaveImage();
            SaveLocation();
            CheckCredentials();
            StartConnectionWorkflow();
            pnlSignupForm.Visible = false;
            pnlSnapshot.Visible = false;
            pnlConfirmation.Visible = true;
            LoadConfirmationLava();
        }

        private void LbCancel_Click( object sender, EventArgs e )
        {
            pnlSnapshot.Visible = false;
            ClearFields();
            InitializeForm();
        }

        private void LbNext_Click( object sender, EventArgs e )
        {
            pnlSignupForm.Visible = false;
            pnlSnapshot.Visible = true;
        }

        private void lbFinished_Click( object sender, EventArgs e )
        {
            ClearFields();
            InitializeForm();
        }

        [System.Web.Services.WebMethod()]
        [System.Web.Script.Services.ScriptMethod()]
        protected void lbCamera_Click( object sender, EventArgs e )
        {
            IndexedSnap();
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
            cbPermissionToText.Checked = true;
            tbSchool.Text = string.Empty;
            tbFriend.Text = string.Empty;
            tbAddress.Text = string.Empty;
            tbCity.Text = string.Empty;
            tbState.Text = "Texas";
            tbZip.Text = string.Empty;
            tbParentFirstName.Text = string.Empty;
            tbParentLastName.Text = string.Empty;
            pnbParentPhone.Text = string.Empty;
            embParentEmail.Text = string.Empty;
            rrblGender.ClearSelection();
            gpGrade.ClearSelection();
            rrblService.ClearSelection();
            rddlPhoneType.ClearSelection();
            PhotoId = 0;

            Campus = null;
        }

        private void InitializeForm()
        {
            var programParameter = PageParameter( "Program" );
            Program = programParameter.IsNotNullOrWhiteSpace() ? programParameter : "LP Students";

            var campusQSValue = PageParameter( "campus" ).AsIntegerOrNull();
            if ( campusQSValue.HasValue )
            {
                Campus = CampusCache.Get( campusQSValue.Value, _context );
            }
            else
            {
                var campusId = GetAttributeValue( "Campus" ).AsIntegerOrNull();
                if ( campusId.HasValue )
                {
                    Campus = CampusCache.Get( campusId.Value, _context );
                }
            }

            ShowCampusSelector = GetAttributeValue( "DisplayCampusPicker" ).AsBoolean();
            pnlCampusPicker.Visible = ShowCampusSelector; // do this on the wrapper panel because CampusPicker overrides the implementation of the Visible property.
            if ( ShowCampusSelector )
            {
                cpCampus.SelectedCampusId = ( Campus == null ) ? 0 : Campus.Id;
            }

            pnlConfirmation.Visible = false;
            pnlSnapshot.Visible = false;

            gpGrade.UseGradeOffsetAsValue = true;

            LoadIntroLava();
            pnlSignupForm.Visible = true;
        }

        private void LoadConfirmationLava()
        {
            var programParameter = PageParameter( "Program" );
            Program = programParameter.IsNotNullOrWhiteSpace() ? programParameter : "LP Students";

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Program", Program );
            mergeFields.Add( "Phone", tbPhone.Number.Trim() );
            lConfirmation.Text = GetAttributeValue( "ConfirmationMessageTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        private void LoadIntroLava()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson,
                new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Program", Program );
            lIntro.Text = GetAttributeValue( "IntroMessageTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        private void CheckCredentials()
        {
            if ( tbPhone.Number.Trim().IsNullOrWhiteSpace() )
            {
                cbPermissionToText.Checked = false; // if no phone, don't enable SMS


                if ( ShowCampusSelector )
                {
                    Campus = CampusCache.Get( cpCampus.SelectedCampusId ?? 3 );
                }

                // create a fake phone number
                tbPhone.Number = string.Format( "{1:D2}{0}", dpBirthDate.SelectedDate.Value.ToString( "MMddyyyy" ), Campus.Id );
            }

            if ( !rddlPhoneType.SelectedValue.Equals( "Mine" ) )
            {
                cbPermissionToText.Checked = false;
            }
        }

        private void StartConnectionWorkflow()
        {
            var workflowGuid = GetAttributeValue( "RegistrationWorkflow" ).AsGuid();
            var workflowType = WorkflowTypeCache.Get( workflowGuid );
            if ( workflowType == null )
            {
                throw new Exception( "Student Registration workflow type not provided." );
            }

            var workflowAttributes = new Dictionary<string, string>();
            workflowAttributes.Add( "FirstName", tbFirstName.Text.Trim() );
            workflowAttributes.Add( "LastName", tbLastName.Text.Trim() );
            workflowAttributes.Add( "BirthDate", dpBirthDate.SelectedDate.Value.ToShortDateString() );
            workflowAttributes.Add( "Gender", rrblGender.SelectedValue );
            workflowAttributes.Add( "Phone", PhoneNumber.FormattedNumber( "1", PhoneNumber.CleanNumber( tbPhone.Number.Trim() ), false ) );
            workflowAttributes.Add( "TextOkay", cbPermissionToText.Checked ? "Yes" : "No" );
            workflowAttributes.Add( "WhosePhone", rddlPhoneType.SelectedValue );
            workflowAttributes.Add( "Email", tbEmail.Text.Trim() );
            workflowAttributes.Add( "School", tbSchool.Text.Trim() );
            workflowAttributes.Add( "Friend", tbFriend.Text.Trim() );
            workflowAttributes.Add( "GraduationYear", Person.GraduationYearFromGradeOffset( gpGrade.SelectedValue.AsIntegerOrNull() ).ToString() );
            workflowAttributes.Add( "Home", HomeLocationGuid );
            workflowAttributes.Add( "ParentFirstName", tbParentFirstName.Text.Trim() );
            workflowAttributes.Add( "ParentLastName", tbParentLastName.Text.Trim() );
            workflowAttributes.Add( "ParentPhone", PhoneNumber.FormattedNumber( "1", PhoneNumber.CleanNumber( pnbParentPhone.Number.Trim() ), false ) );
            workflowAttributes.Add( "ParentEmail", embParentEmail.Text.Trim() );
            workflowAttributes.Add( "PhotoId", PhotoId.ToString() );
            if ( ShowCampusSelector )
            {
                Campus = CampusCache.Get( cpCampus.SelectedCampusId ?? 3 );
                workflowAttributes.Add( "Campus", Campus.Guid.ToString() );
            }
            else
            {
                if ( Campus != null )
                {
                    workflowAttributes.Add( "Campus", Campus.Guid.ToString() );
                }
            }
            workflowAttributes.Add( "Program", Program );
            workflowAttributes.Add( "ParentServicePreference", rrblService.SelectedValue );

            var workflow = Workflow.Activate( workflowType, "New Student Registration" );
            workflow.LoadAttributes( _context );

            foreach ( var attributeItem in workflowAttributes )
            {
                workflow.SetAttributeValue( attributeItem.Key, attributeItem.Value );
            }

            var errorMessages = new List<string>();
            new WorkflowService( _context ).Process( workflow, out errorMessages );
        }

        private void IndexedSnap()
        {
            ScriptManager.RegisterStartupScript( dlgCamera, dlgCamera.GetType(), "callSnapImage" + RockDateTime.Now.Ticks.ToString(), "snapImage();", true );
            dlgCamera.Show();
        }

        private void SaveImage()
        {
            if ( hfImage.Value.Length > 23 )
            {
                // always create a new BinaryFile record of IsTemporary when a file is uploaded
                var binaryFileService = new BinaryFileService( _context );
                Guid fileTypeGuid = Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid();
                BinaryFileType binaryFileType = new BinaryFileTypeService( _context ).Get( fileTypeGuid );

                var binaryFile = new BinaryFile();

                binaryFileService.Add( binaryFile );
                var uploadedFile = hfImage.Value.Substring( 23 );

                byte[] imageBytes = Convert.FromBase64String( uploadedFile );
                MemoryStream ms = new MemoryStream( imageBytes, 0, imageBytes.Length );
                binaryFile.ContentStream = ms;

                // assume file is temporary so files that don't end up getting used will get cleaned up
                binaryFile.IsTemporary = true;
                binaryFile.BinaryFileTypeId = binaryFileType.Id;
                binaryFile.MimeType = "image/jpeg";
                binaryFile.FileName = "new picture";

                _context.SaveChanges();

                PhotoId = binaryFile.Id;
            }
        }

        private void SaveLocation()
        {
            if ( tbAddress.Text.Trim().IsNullOrWhiteSpace() || tbCity.Text.Trim().IsNullOrWhiteSpace() || tbState.Text.Trim().IsNullOrWhiteSpace() || tbZip.Text.Trim().IsNullOrWhiteSpace() )
            {
                HomeLocationGuid = string.Empty;
            }
            else
            {
                var locationService = new LocationService( _context );
                var location = locationService.Get( tbAddress.Text.Trim(), string.Empty, tbCity.Text.Trim(), tbState.Text.Trim(), tbZip.Text.Trim(), "US" );
                _context.SaveChanges();
                HomeLocationGuid = ( location == null ) ? string.Empty : location.Guid.ToString();
            }
        }

        #endregion 
    }
}
