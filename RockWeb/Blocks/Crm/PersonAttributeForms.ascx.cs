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
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.
    /// </summary>
    [DisplayName( "Person Attribute Forms" )]
    [Category( "CRM" )]
    [Description( "Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure." )]

    // Block Properties

    // Settings
    [BooleanField( "Display Progress Bar", "Determines if the progress bar should be show if there is more than one form.", true, "CustomSetting" )]
    [CustomDropdownListField( "Save Values", "", "PAGE,END", true, "END", "CustomSetting" )]
    [WorkflowTypeField( "Workflow", "The workflow to be launched when complete.", false, false, "", "CustomSetting" )]
    [LinkedPage( "Done Page", "The page to redirect to when done.", false, "", "CustomSetting" )]
    [TextField( "Forms", "The forms to show.", false, "", "CustomSetting" )]

    public partial class PersonAttributeForms : RockBlockCustomSettings
    {
        #region Fields

        private string _mode = "VIEW";
        private bool _saveNavigationHistory = false;
        public decimal PercentComplete = 0;

        #endregion

        #region Properties

        private List<AttributeForm> FormState { get; set; }

        private Dictionary<int, string> AttributeValueState { get; set; }

        /// <summary>
        /// Gets the settings tool tip.
        /// </summary>
        /// <value>
        /// The settings tool tip.
        /// </value>
        public override string SettingsToolTip
        {
            get
            {
                return "Edit Forms and Fields";
            }
        }

        // The current page index
        private int CurrentPageIndex { get; set; }

        protected decimal ProgressBarSteps
        {
            get { return ViewState["ProgressBarSteps"] as decimal? ?? 1.0m; }
            set { ViewState["ProgressBarSteps"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _mode = ViewState["Mode"].ToString();

            string json = ViewState["FormState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FormState = new List<AttributeForm>();
            }
            else
            {
                FormState = JsonConvert.DeserializeObject<List<AttributeForm>>( json );
            }

            json = ViewState["AttributeValueState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributeValueState = new Dictionary<int, string>();
            }
            else
            {
                AttributeValueState = JsonConvert.DeserializeObject<Dictionary<int, string>>( json );
            }

            CurrentPageIndex = ViewState["CurrentPageIndex"] as int? ?? 0;

            if ( _mode == "VIEW" )
            {
                BuildViewControls( false );
            }
            else
            {
                BuildEditControls( false );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BlockUpdated += AttributeForm_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            RegisterClientScript();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var sm = ScriptManager.GetCurrent( Page );
            sm.Navigate += Sm_Navigate;

            nbMain.Visible = false;

            if ( CurrentPerson != null )
            {
                if ( !Page.IsPostBack )
                {
                    ShowDetail();
                }
                else
                {
                    ShowDialog();

                    if ( _mode == "VIEW" )
                    {
                        ParseViewControls();
                    }

                    if ( _mode == "EDIT" )
                    {
                        string postbackArgs = Request.Params["__EVENTARGUMENT"];
                        if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
                        {
                            string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                            if ( nameValue.Count() == 2 )
                            {
                                string[] values = nameValue[1].Split( new char[] { ';' } );
                                if ( values.Count() == 2 )
                                {
                                    Guid guid = values[0].AsGuid();
                                    int newIndex = values[1].AsInteger();

                                    switch ( nameValue[0] )
                                    {
                                        case "re-order-form":
                                            {
                                                SortForms( guid, newIndex + 1 );
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                nbMain.Title = "Sorry";
                nbMain.Text = "You need to login before entering information on this page.";
                nbMain.NotificationBoxType = NotificationBoxType.Warning;
                nbMain.Visible = true;
            }

        }

        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["FormState"] = JsonConvert.SerializeObject( FormState, Formatting.None, jsonSetting );
            ViewState["AttributeValueState"] = JsonConvert.SerializeObject( AttributeValueState, Formatting.None, jsonSetting );
            ViewState["CurrentPageIndex"] = CurrentPageIndex;
            ViewState["Mode"] = _mode;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( _saveNavigationHistory )
            {
                this.AddHistory( "form", CurrentPageIndex.ToString() );
            }
        }

        #endregion

        #region Events

        private void Sm_Navigate( object sender, HistoryEventArgs e )
        {
            var state = e.State["form"];

            if ( state != null )
            {
                CurrentPageIndex = state.AsInteger();
            }
            else
            {
                CurrentPageIndex = 0;
            }

            ShowPage();

        }

        protected void lbPrev_Click( object sender, EventArgs e )
        {
            _saveNavigationHistory = true;

            CurrentPageIndex--;

            ShowPage();

            hfTriggerScroll.Value = "true";
        }

        protected void lbNext_Click( object sender, EventArgs e )
        {
            _saveNavigationHistory = true;

            CurrentPageIndex++;

            bool saveEachPage = GetAttributeValue( "SaveValues" ) == "PAGE";
            if ( saveEachPage || CurrentPageIndex >= FormState.Count )
            {
                if ( CurrentPersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var person = new PersonService( rockContext ).Get( CurrentPersonId.Value );
                        if ( person != null )
                        {
                            person.LoadAttributes( rockContext );

                            var pageAttributeIds = new List<int>(); ;
                            if ( saveEachPage && CurrentPageIndex > 0 && CurrentPageIndex <= FormState.Count )
                            {
                                pageAttributeIds = FormState[CurrentPageIndex - 1].Fields
                                    .Where( f => f.AttributeId.HasValue )
                                    .Select( f => f.AttributeId.Value )
                                    .ToList();
                            }

                            foreach ( var keyVal in AttributeValueState )
                            {
                                var attribute = AttributeCache.Read( keyVal.Key );
                                if ( attribute != null && ( CurrentPageIndex >= FormState.Count || !pageAttributeIds.Any() || pageAttributeIds.Contains( attribute.Id ) ) )
                                {
                                    person.SetAttributeValue( attribute.Key, keyVal.Value );
                                }
                            }

                            person.SaveAttributeValues( rockContext );

                            if ( CurrentPageIndex >= FormState.Count )
                            {
                                Guid? workflowTypeGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
                                if ( workflowTypeGuid.HasValue )
                                {
                                    var workflowType = WorkflowTypeCache.Read( workflowTypeGuid.Value );
                                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                                    {
                                        try
                                        {
                                            var workflow = Workflow.Activate( workflowType, person.FullName );
                                            List<string> workflowErrors;
                                            new WorkflowService( rockContext ).Process( workflow, person, out workflowErrors );
                                        }
                                        catch ( Exception ex )
                                        {
                                            ExceptionLogService.LogException( ex, this.Context );
                                        }
                                    }
                                }

                                if ( GetAttributeValue( "DonePage" ).AsGuidOrNull().HasValue )
                                {
                                    NavigateToLinkedPage( "DonePage" );
                                }
                                else
                                {
                                    pnlView.Visible = false;
                                }
                                upnlContent.Update();
                            }
                            else
                            {
                                ShowPage();
                                hfTriggerScroll.Value = "true";
                            }
                        }
                    }
                }
            }
            else
            {
                ShowPage();
                hfTriggerScroll.Value = "true";
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the AttributeForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AttributeForm_BlockUpdated( object sender, EventArgs e )
        {
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "DisplayProgressBar", cbDisplayProgressBar.Checked.ToString() );
            SetAttributeValue( "SaveValues", ddlSaveValues.SelectedValue );

            var workflowTypeId = wtpWorkflow.SelectedValueAsInt();
            if ( workflowTypeId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId.Value );
                    if ( workflowType != null )
                    {
                        SetAttributeValue( "Workflow", workflowType.Guid.ToString() );
                    }
                    else
                    {
                        SetAttributeValue( "Workflow", "" );
                    }
                }
            }
            else
            {
                SetAttributeValue( "Workflow", "" );
            }

            var ppFieldType = new PageReferenceFieldType();
            SetAttributeValue( "DonePage", ppFieldType.GetEditValue( ppDonePage, null ) );

            ParseEditControls();
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            string json = JsonConvert.SerializeObject( FormState, Formatting.None, jsonSetting );
            SetAttributeValue( "Forms", json );

            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;

            ShowDetail();

            upnlContent.Update();
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowDetail();

            upnlContent.Update();
        }

        #region Form Control Events

        /// <summary>
        /// Handles the Click event of the lbAddForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddForm_Click( object sender, EventArgs e )
        {
            ParseEditControls();

            var form = new AttributeForm();
            form.Guid = Guid.NewGuid();
            form.Expanded = true;
            form.Order = FormState.Any() ? FormState.Max( a => a.Order ) + 1 : 0;
            FormState.Add( form );

            BuildEditControls( true, form.Guid );
        }

        /// <summary>
        /// Handles the DeleteFormClick event of the tfeForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void tfeForm_DeleteFormClick( object sender, EventArgs e )
        {
            ParseEditControls();

            var attributeFormEditor = sender as PersonAttributeFormEditor;
            if ( attributeFormEditor != null )
            {
                var form = FormState.FirstOrDefault( a => a.Guid == attributeFormEditor.FormGuid );
                if ( form != null )
                {
                    FormState.Remove( form );
                }
            }

            BuildEditControls( true );
        }

        /// <summary>
        /// Tfes the form_ add attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_AddFieldClick( object sender, AttributeFormFieldEventArg e )
        {
            ParseEditControls();

            ShowFormFieldEdit( e.FormGuid, Guid.NewGuid() );

            BuildEditControls( true );
        }

        /// <summary>
        /// Tfes the form_ edit attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_EditFieldClick( object sender, AttributeFormFieldEventArg e )
        {
            ParseEditControls();

            ShowFormFieldEdit( e.FormGuid, e.FormFieldGuid );

            BuildEditControls( true );
        }

        /// <summary>
        /// Tfes the form_ reorder attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_ReorderFieldClick( object sender, AttributeFormFieldEventArg e )
        {
            ParseEditControls();

            var form = FormState.FirstOrDefault( f => f.Guid == e.FormGuid );
            if ( form != null )
            {
                SortFields( form.Fields, e.OldIndex, e.NewIndex );
                ReOrderFields( form.Fields );
            }

            BuildEditControls( true, e.FormGuid );
        }

        /// <summary>
        /// Tfes the form_ delete attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_DeleteFieldClick( object sender, AttributeFormFieldEventArg e )
        {
            ParseEditControls();

            var form = FormState.FirstOrDefault( f => f.Guid == e.FormGuid );
            if ( form != null )
            {
                var field = form.Fields.FirstOrDefault( f => f.Guid == e.FormFieldGuid );
                if ( field != null )
                {
                    form.Fields.Remove( field );
                }
            }

            BuildEditControls( true, e.FormGuid );
        }

        /// <summary>
        /// Tfes the form_ rebind attribute click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void tfeForm_RebindFieldClick( object sender, AttributeFormFieldEventArg e )
        {
            ParseEditControls();

            BuildEditControls( true, e.FormGuid );
        }

        #endregion

        #region Field Dialog Events

        /// <summary>
        /// Handles the SaveClick event of the dlgField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgField_SaveClick( object sender, EventArgs e )
        {
            var formGuid = hfFormGuid.Value.AsGuid();
            var attributeGuid = hfAttributeGuid.Value.AsGuid();

            var form = FormState.FirstOrDefault( f => f.Guid == formGuid );
            if ( form != null )
            {
                var field = form.Fields.FirstOrDefault( a => a.Guid.Equals( attributeGuid ) );
                if ( field == null )
                {
                    field = new AttributeFormField();
                    field.Order = form.Fields.Any() ? form.Fields.Max( a => a.Order ) + 1 : 0;
                    field.Guid = attributeGuid;
                    form.Fields.Add( field );
                }

                field.PreText = ceAttributePreText.Text;
                field.PostText = ceAttributePostText.Text;

                field.AttributeId = ddlPersonAttributes.SelectedValueAsInt();
                field.ShowCurrentValue = cbUsePersonCurrentValue.Checked;
                field.IsRequired = cbRequireInInitialEntry.Checked;
            }

            HideDialog();

            BuildEditControls( true );
        }

        #endregion

        #endregion

        #region Methods

        #region View Mode

        private void ShowDetail()
        {
            _mode = "VIEW";

            pnlEditModal.Visible = false;

            string json = GetAttributeValue( "Forms" );
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FormState = new List<AttributeForm>();
            }
            else
            {
                FormState = JsonConvert.DeserializeObject<List<AttributeForm>>( json );
            }

            if ( FormState.Count > 0 )
            {
                pnlView.Visible = true;

                AttributeValueState = new Dictionary<int, string>();
                if ( CurrentPerson != null )
                {
                    if ( CurrentPerson.Attributes == null )
                    {
                        CurrentPerson.LoadAttributes();
                    }

                    foreach ( var form in FormState )
                    {
                        foreach ( var field in form.Fields
                            .Where( a =>
                                a.AttributeId.HasValue &&
                                a.ShowCurrentValue == true ) )
                        {
                            var attributeCache = AttributeCache.Read( field.AttributeId.Value );
                            if ( attributeCache != null )
                            {
                                AttributeValueState.AddOrReplace( field.AttributeId.Value, CurrentPerson.GetAttributeValue( attributeCache.Key ) );
                            }
                        }
                    }
                }
                
                ProgressBarSteps = FormState.Count();
                CurrentPageIndex = 0;
                ShowPage();
            }
            else
            {
                nbMain.Title = "No Forms/Fields";
                nbMain.Text = "No forms or fields have been configured. Use the Block Configuration to add new forms and fields.";
                nbMain.NotificationBoxType = NotificationBoxType.Warning;
                nbMain.Visible = true;
            }
        }

        private void ShowPage()
        {
            decimal currentStep = CurrentPageIndex + 1;
            PercentComplete = ( currentStep / ProgressBarSteps ) * 100.0m;
            pnlProgressBar.Visible = GetAttributeValue( "DisplayProgressBar" ).AsBoolean() && (FormState.Count > 1);

            BuildViewControls( true );

            lbPrev.Visible = CurrentPageIndex > 0;
            lbNext.Visible = CurrentPageIndex < FormState.Count;
            lbNext.Text = CurrentPageIndex < FormState.Count() - 1 ? "Next" : "Finish";

            upnlContent.Update();
        }

        private void BuildViewControls( bool setValues )
        {
            phContent.Controls.Clear();

            if ( FormState != null )
            {
                if ( CurrentPageIndex >= FormState.Count )
                {
                    lTitle.Text = "Done!";
                    lHeader.Text = string.Empty;
                    lFooter.Text = string.Empty;
                }
                else
                {
                    var form = FormState[CurrentPageIndex];

                    var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                    lTitle.Text = form.Name.ResolveMergeFields( mergeFields );
                    lHeader.Text = form.Header.ResolveMergeFields( mergeFields );
                    lFooter.Text = form.Footer.ResolveMergeFields( mergeFields );

                    foreach ( var field in form.Fields
                        .Where( f => f.AttributeId.HasValue )
                        .OrderBy( f => f.Order ) )
                    {
                        if ( !string.IsNullOrWhiteSpace( field.PreText ) )
                        {
                            phContent.Controls.Add( new LiteralControl( field.PreText.ResolveMergeFields( mergeFields ) ) );
                        }

                        string value = null;
                        if ( AttributeValueState.ContainsKey( field.AttributeId.Value ) )
                        {
                            value = AttributeValueState[field.AttributeId.Value];
                        }
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        attribute.AddControl( phContent.Controls, value, BlockValidationGroup, setValues, true, field.IsRequired, null, string.Empty );

                        if ( !string.IsNullOrWhiteSpace( field.PostText ) )
                        {
                            phContent.Controls.Add( new LiteralControl( field.PostText.ResolveMergeFields( mergeFields ) ) );
                        }
                    }
                }
            }
        }

        private void ParseViewControls()
        {
            if ( FormState != null && FormState.Count > CurrentPageIndex )
            {
                var form = FormState[CurrentPageIndex];
                foreach ( var field in form.Fields
                    .Where( f => f.AttributeId.HasValue )
                    .OrderBy( f => f.Order ) )
                {
                    var attribute = AttributeCache.Read( field.AttributeId.Value );
                    string fieldId = "attribute_field_" + attribute.Id.ToString();

                    Control control = phContent.FindControl( fieldId );
                    if ( control != null )
                    {
                        string value = attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues );
                        AttributeValueState.AddOrReplace( attribute.Id, value );
                    }
                }
            }
        }

        #endregion

        #region Edit Mode

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            //NOTE: This isn't shown in a modal :(
            


            cbDisplayProgressBar.Checked = GetAttributeValue( "DisplayProgressBar" ).AsBoolean();
            ddlSaveValues.SetValue( GetAttributeValue( "SaveValues" ) );

            Guid? wtGuid = GetAttributeValue( "Workflow" ).AsGuidOrNull();
            if ( wtGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    wtpWorkflow.SetValue( new WorkflowTypeService( rockContext ).Get( wtGuid.Value ) );
                }
            }
            else
            {
                wtpWorkflow.SetValue( null );
            }

            var ppFieldType = new PageReferenceFieldType();
            ppFieldType.SetEditValue( ppDonePage, null, GetAttributeValue( "DonePage" ) );

            string json = GetAttributeValue( "Forms" );
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                FormState = new List<AttributeForm>();
                FormState.Add( new AttributeForm { Expanded = true } );
            }
            else
            {
                FormState = JsonConvert.DeserializeObject<List<AttributeForm>>( json );
            }

            BuildEditControls( true );

            pnlEditModal.Visible = true;
            pnlView.Visible = false;
            mdEdit.Show();

            _mode = "EDIT";

            upnlContent.Update();
        }

        /// <summary>
        /// Builds the controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="activeFormGuid">The active form unique identifier.</param>
        private void BuildEditControls( bool setValues = false, Guid? activeFormGuid = null )
        {
            phForms.Controls.Clear();

            if ( FormState != null )
            {
                foreach ( var form in FormState.OrderBy( f => f.Order ) )
                {
                    BuildFormControl( phForms, setValues, form, activeFormGuid );
                }
            }

            upnlContent.Update();
        }

        /// <summary>
        /// Builds the form control.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="form">The form.</param>
        /// <param name="activeFormGuid">The active form unique identifier.</param>
        /// <param name="showInvalid">if set to <c>true</c> [show invalid].</param>
        private void BuildFormControl( Control parentControl, bool setValues, AttributeForm form,
            Guid? activeFormGuid = null, bool showInvalid = false )
        {
            var control = new PersonAttributeFormEditor();
            control.ID = form.Guid.ToString( "N" );
            parentControl.Controls.Add( control );

            control.ValidationGroup = mdEdit.ValidationGroup;

            control.DeleteFieldClick += tfeForm_DeleteFieldClick;
            control.ReorderFieldClick += tfeForm_ReorderFieldClick;
            control.EditFieldClick += tfeForm_EditFieldClick;
            control.RebindFieldClick += tfeForm_RebindFieldClick;
            control.DeleteFormClick += tfeForm_DeleteFormClick;
            control.AddFieldClick += tfeForm_AddFieldClick;

            control.SetForm( form );

            control.BindFieldsGrid( form.Fields );

            if ( setValues )
            {
                if ( !control.Expanded )
                {
                    control.Expanded = activeFormGuid.HasValue && activeFormGuid.Equals( form.Guid );
                }
            }
        }

        /// <summary>
        /// Parses the controls.
        /// </summary>
        private void ParseEditControls()
        {
            int order = 0;
            foreach ( var formEditor in phForms.Controls.OfType<PersonAttributeFormEditor>() )
            {
                var form = FormState.FirstOrDefault( f => f.Guid == formEditor.FormGuid );
                if ( form != null )
                {
                    form.Order = order++;
                    form.Name = formEditor.Name;
                    form.Header = formEditor.Header;
                    form.Footer = formEditor.Footer;
                    form.Expanded = formEditor.Expanded;
                }
            }
        }

        /// <summary>
        /// Shows the form field edit.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        private void ShowFormFieldEdit( Guid formGuid, Guid formFieldGuid )
        {
            var form = FormState.FirstOrDefault( f => f.Guid == formGuid );
            if ( form != null )
            {
                var field = form.Fields.FirstOrDefault( a => a.Guid.Equals( formFieldGuid ) );
                if ( field == null )
                {
                    field = new AttributeFormField();
                    field.Guid = formFieldGuid;
                    field.ShowCurrentValue = true;
                    field.IsRequired = false;
                }

                ceAttributePreText.Text = field.PreText;
                ceAttributePostText.Text = field.PostText;

                ddlPersonAttributes.Items.Clear();
                var person = new Person();
                person.LoadAttributes();
                foreach ( var attr in person.Attributes
                    .OrderBy( a => a.Value.Name )
                    .Select( a => a.Value ) )
                {
                    if ( attr.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlPersonAttributes.Items.Add( new ListItem( attr.Name, attr.Id.ToString() ) );
                    }
                }

                ddlPersonAttributes.SetValue( field.AttributeId );

                cbRequireInInitialEntry.Checked = field.IsRequired;
                cbUsePersonCurrentValue.Checked = field.ShowCurrentValue;

                hfFormGuid.Value = formGuid.ToString();
                hfAttributeGuid.Value = formFieldGuid.ToString();

                ShowDialog( "Attributes" );
            }

            BuildEditControls( true );
        }

        /// <summary>
        /// Sorts the forms.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortForms( Guid guid, int newIndex )
        {
            ParseEditControls();

            Guid? activeFormGuid = null;

            var form = FormState.FirstOrDefault( a => a.Guid.Equals( guid ) );
            if ( form != null )
            {
                activeFormGuid = form.Guid;

                FormState.Remove( form );
                if ( newIndex >= FormState.Count() )
                {
                    FormState.Add( form );
                }
                else
                {
                    FormState.Insert( newIndex, form );
                }
            }

            int order = 0;
            foreach ( var item in FormState )
            {
                item.Order = order++;
            }

            BuildEditControls( true );
        }

        /// <summary>
        /// Sorts the fields.
        /// </summary>
        /// <param name="fieldList">The field list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortFields( List<AttributeFormField> fieldList, int oldIndex, int newIndex )
        {
            var movedItem = fieldList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in fieldList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in fieldList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorder fields.
        /// </summary>
        /// <param name="fieldList">The field list.</param>
        private void ReOrderFields( List<AttributeFormField> fieldList )
        {
            fieldList = fieldList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            fieldList.ForEach( a => a.Order = order++ );
        }

        #endregion

        #region Dialog Methods

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgField.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgField.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        /// <summary>
        /// Registers the client script.
        /// </summary>
        private void RegisterClientScript()
        {
            RockPage.AddScriptLink( ResolveUrl( "~/Scripts/jquery.creditCardTypeDetector.js" ) );

            string script = string.Format( @"

    if ( $('#{0}').val() == 'true' ) {{
        setTimeout('window.scrollTo(0,0)',0);
        $('#{0}').val('')
    }}

",
            hfTriggerScroll.ClientID       // {0}
            );

            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "PersonAttributeForms", script, true );

        }

        #endregion

    }

    #region Helper Classes

    [ToolboxData( "<{0}:PersonAttributeFormEditor runat=server></{0}:PersonAttributeFormEditor>" )]
    public class PersonAttributeFormEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfFormGuid;
        private Label _lblFormName;

        private RockTextBox _tbFormName;
        private CodeEditor _tbFormHeader;
        private CodeEditor _tbFormFooter;

        private LinkButton _lbDeleteForm;

        private Grid _gFields;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PersonAttributeFormEditor"/> is expanded.
        /// </summary>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();
                return _hfExpanded.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }
            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the activity type unique identifier.
        /// </summary>
        /// <value>
        /// The activity type unique identifier.
        /// </value>
        public Guid FormGuid
        {
            get
            {
                EnsureChildControls();
                return _hfFormGuid.Value.AsGuid();
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                EnsureChildControls();
                return _tbFormName.Text;
            }
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public string Header
        {
            get
            {
                EnsureChildControls();
                return _tbFormHeader.Text;
            }
        }

        /// <summary>
        /// Gets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        public string Footer
        {
            get
            {
                EnsureChildControls();
                return _tbFormFooter.Text;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.template-form > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.template-form-state', this).toggleClass('fa-chevron-down');
    $('i.template-form-state', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.template-form a.js-activity-delete').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.template-form a.template-form-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

$('.template-form > .panel-body').on('validation-error', function() {
    var $header = $(this).siblings('header');
    $(this).slideDown();

    $expanded = $header.children('input.filter-expanded');
    $expanded.val('True');

    $('i.template-form-state', $header).removeClass('fa-chevron-down');
    $('i.template-form-state', $header).addClass('fa-chevron-up');

    return false;
});
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "PersonAttributeFormEditorScript", script, true );
        }

        /// <summary>
        /// Sets the type of the workflow activity.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetForm( AttributeForm value )
        {
            EnsureChildControls();
            _hfFormGuid.Value = value.Guid.ToString();
            _tbFormName.Text = value.Name;
            _tbFormHeader.Text = value.Header;
            _tbFormFooter.Text = value.Footer;
            Expanded = value.Expanded;
        }

        /// <summary>
        /// Binds the fields grid.
        /// </summary>
        /// <param name="formFields">The fields.</param>
        public void BindFieldsGrid( List<AttributeFormField> formFields )
        {
            _gFields.DataSource = formFields
                .OrderBy( a => a.Order )
                .Select( a => new
                {
                    a.Guid,
                    Name = a.Attribute.Name,
                    FieldType = a.Attribute.FieldTypeId,
                    a.ShowCurrentValue,
                    a.IsRequired,
                } )
                .ToList();
            _gFields.DataBind();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfExpanded = new HiddenFieldWithClass();
            Controls.Add( _hfExpanded );
            _hfExpanded.ID = this.ID + "_hfExpanded";
            _hfExpanded.CssClass = "filter-expanded";
            _hfExpanded.Value = "False";

            _hfFormGuid = new HiddenField();
            Controls.Add( _hfFormGuid );
            _hfFormGuid.ID = this.ID + "_hfFormGuid";

            _lblFormName = new Label();
            Controls.Add( _lblFormName );
            _lblFormName.ClientIDMode = ClientIDMode.Static;
            _lblFormName.ID = this.ID + "_lblFormName";

            _lbDeleteForm = new LinkButton();
            Controls.Add( _lbDeleteForm );
            _lbDeleteForm.CausesValidation = false;
            _lbDeleteForm.ID = this.ID + "_lbDeleteForm";
            _lbDeleteForm.CssClass = "btn btn-xs btn-danger js-activity-delete";
            _lbDeleteForm.Click += lbDeleteForm_Click;
            _lbDeleteForm.Controls.Add( new LiteralControl { Text = "<i class='fa fa-times'></i>" } );

            _tbFormName = new RockTextBox();
            Controls.Add( _tbFormName );
            _tbFormName.ID = this.ID + "_tbFormName";
            _tbFormName.Label = "Form Title";
            _tbFormName.Help = "Title of the form <span class='tip tip-lava'></span>.";
            _tbFormName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblFormName.ID );

            _tbFormHeader = new CodeEditor();
            Controls.Add( _tbFormHeader );
            _tbFormHeader.ID = this.ID + "_tbFormHeader";
            _tbFormHeader.Label = "Form Header";
            _tbFormHeader.Help = "HTML to display above the fields <span class='tip tip-lava'></span>.";
            _tbFormHeader.EditorMode = CodeEditorMode.Html;
            _tbFormHeader.EditorTheme = CodeEditorTheme.Rock;
            _tbFormHeader.EditorHeight = "100";

            _tbFormFooter = new CodeEditor();
            Controls.Add( _tbFormFooter );
            _tbFormFooter.ID = this.ID + "_tbFormFooter";
            _tbFormFooter.Label = "Form Footer";
            _tbFormFooter.Help = "HTML to display below the fields <span class='tip tip-lava'></span>.";
            _tbFormFooter.EditorMode = CodeEditorMode.Html;
            _tbFormFooter.EditorTheme = CodeEditorTheme.Rock;
            _tbFormFooter.EditorHeight = "100";

            _gFields = new Grid();
            Controls.Add( _gFields );
            _gFields.ID = this.ID + "_gFields";
            _gFields.AllowPaging = false;
            _gFields.DisplayType = GridDisplayType.Light;
            _gFields.RowItemText = "Field";
            _gFields.AddCssClass( "field-grid" );
            _gFields.DataKeyNames = new string[] { "Guid" };
            _gFields.Actions.ShowAdd = true;
            _gFields.Actions.AddClick += gFields_Add;
            _gFields.GridRebind += gFields_Rebind;
            _gFields.GridReorder += gFields_Reorder;

            var reorderField = new ReorderField();
            _gFields.Columns.Add( reorderField );

            var nameField = new BoundField();
            nameField.DataField = "Name";
            nameField.HeaderText = "Field";
            _gFields.Columns.Add( nameField );

            var typeField = new FieldTypeField();
            typeField.DataField = "FieldType";
            typeField.HeaderText = "Type";
            _gFields.Columns.Add( typeField );

            var showCurrentValueField = new BoolField();
            showCurrentValueField.DataField = "ShowCurrentValue";
            showCurrentValueField.HeaderText = "Use Current Value";
            _gFields.Columns.Add( showCurrentValueField );

            var isRequiredField = new BoolField();
            isRequiredField.DataField = "IsRequired";
            isRequiredField.HeaderText = "Required";
            _gFields.Columns.Add( isRequiredField );

            var editField = new EditField();
            editField.Click += gFields_Edit;
            _gFields.Columns.Add( editField );

            var delField = new DeleteField();
            delField.Click += gFields_Delete;
            _gFields.Columns.Add( delField );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget template-form" );

            writer.AddAttribute( "data-key", _hfFormGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix clickable" );
            writer.RenderBeginTag( "header" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toggle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "panel-title" );
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            _lblFormName.Text = _tbFormName.Text;
            _lblFormName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();

            // Name div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-xs btn-link form-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='form-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            _lbDeleteForm.RenderControl( writer );

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            if ( !Expanded )
            {
                // hide details if the activity and actions are valid
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // activity edit fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfFormGuid.RenderControl( writer );
            _tbFormName.ValidationGroup = ValidationGroup;
            _tbFormName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();

            _tbFormHeader.RenderControl( writer );

            _gFields.RenderControl( writer );

            _tbFormFooter.RenderControl( writer );

            // widget-content div
            writer.RenderEndTag();

            // section tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDeleteForm_Click( object sender, EventArgs e )
        {
            if ( DeleteFormClick != null )
            {
                DeleteFormClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Rebind event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFields_Rebind( object sender, EventArgs e )
        {
            if ( RebindFieldClick != null )
            {
                var eventArg = new AttributeFormFieldEventArg( FormGuid );
                RebindFieldClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Add event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gFields_Add( object sender, EventArgs e )
        {
            if ( AddFieldClick != null )
            {
                var eventArg = new AttributeFormFieldEventArg( FormGuid, Guid.Empty );
                AddFieldClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFields_Edit( object sender, RowEventArgs e )
        {
            if ( EditFieldClick != null )
            {
                var eventArg = new AttributeFormFieldEventArg( FormGuid, (Guid)e.RowKeyValue );
                EditFieldClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Reorder event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gFields_Reorder( object sender, GridReorderEventArgs e )
        {
            if ( ReorderFieldClick != null )
            {
                var eventArg = new AttributeFormFieldEventArg( FormGuid, e.OldIndex, e.NewIndex );
                ReorderFieldClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gFields control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFields_Delete( object sender, RowEventArgs e )
        {
            if ( DeleteFieldClick != null )
            {
                var eventArg = new AttributeFormFieldEventArg( FormGuid, (Guid)e.RowKeyValue );
                DeleteFieldClick( this, eventArg );
            }
        }


        /// <summary>
        /// Occurs when [delete activity type click].
        /// </summary>
        public event EventHandler DeleteFormClick;

        /// <summary>
        /// Occurs when [add field click].
        /// </summary>
        public event EventHandler<AttributeFormFieldEventArg> RebindFieldClick;

        /// <summary>
        /// Occurs when [add field click].
        /// </summary>
        public event EventHandler<AttributeFormFieldEventArg> AddFieldClick;

        /// <summary>
        /// Occurs when [edit field click].
        /// </summary>
        public event EventHandler<AttributeFormFieldEventArg> EditFieldClick;

        /// <summary>
        /// Occurs when [edit field click].
        /// </summary>
        public event EventHandler<AttributeFormFieldEventArg> ReorderFieldClick;

        /// <summary>
        /// Occurs when [delete field click].
        /// </summary>
        public event EventHandler<AttributeFormFieldEventArg> DeleteFieldClick;

    }

    /// <summary>
    /// 
    /// </summary>
    public class AttributeFormFieldEventArg : EventArgs
    {
        /// <summary>
        /// Gets or sets the activity type unique identifier.
        /// </summary>
        /// <value>
        /// The activity type unique identifier.
        /// </value>
        public Guid FormGuid { get; set; }

        /// <summary>
        /// Gets or sets the field unique identifier.
        /// </summary>
        /// <value>
        /// The field unique identifier.
        /// </value>
        public Guid FormFieldGuid { get; set; }

        /// <summary>
        /// Gets or sets the old index.
        /// </summary>
        /// <value>
        /// The old index.
        /// </value>
        public int OldIndex { get; set; }

        /// <summary>
        /// Gets or sets the new index.
        /// </summary>
        /// <value>
        /// The new index.
        /// </value>
        public int NewIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFormFieldEventArg"/> class.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        public AttributeFormFieldEventArg( Guid activityTypeGuid )
        {
            FormGuid = activityTypeGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFormFieldEventArg"/> class.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        public AttributeFormFieldEventArg( Guid formGuid, Guid formFieldGuid )
        {
            FormGuid = formGuid;
            FormFieldGuid = formFieldGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeFormFieldEventArg" /> class.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public AttributeFormFieldEventArg( Guid formGuid, int oldIndex, int newIndex )
        {
            FormGuid = formGuid;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }

    [Serializable]
    public class AttributeForm : IOrdered
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public int Order { get; set; }
        public bool Expanded { get; set; }
        public virtual List<AttributeFormField> Fields { get; set; }

        public AttributeForm()
        {
            Fields = new List<AttributeFormField>();
        }

        public override string ToString()
        {
            return Name;
        }

    }

    [Serializable]
    public partial class AttributeFormField : IOrdered
    {
        public Guid Guid { get; set; }
        public int? AttributeId { get; set; }
        public bool ShowCurrentValue { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string PreText { get; set; }
        public string PostText { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public AttributeCache Attribute
        {
            get
            {
                if ( AttributeId.HasValue )
                {
                    return AttributeCache.Read( AttributeId.Value );
                }

                return null;
            }
        }

        public override string ToString()
        {
            var attributeCache = this.Attribute;
            if ( attributeCache != null )
            {
                return attributeCache.Name;
            }

            return base.ToString();
        }
    }

    #endregion
}