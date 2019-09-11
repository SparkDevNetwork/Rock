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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by WorkflowTypeDetail block to edit a workflow action type
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActionTypeEditor runat=server></{0}:WorkflowActionTypeEditor>" )]
    public class WorkflowActionTypeEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfActionTypeGuid;
        private Label _lblActionTypeName;
        private LinkButton _lbDeleteActionType;

        private RockDropDownList _ddlCriteriaAttribute;
        private RockDropDownList _ddlCriteriaComparisonType;
        private RockTextOrDropDownList _tbddlCriteriaValue;

        private RockTextBox _tbActionTypeName;
        private WorkflowActionTypePicker  _wfatpEntityType;
        private RockLiteral _rlEntityTypeOverview;
        private RockCheckBox _cbIsActionCompletedOnSuccess;
        private RockCheckBox _cbIsActivityCompletedOnSuccess;
        private WorkflowFormEditor _formEditor;
        private PlaceHolder _phActionAttributes;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="WorkflowActionTypeEditor"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
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
        public Guid ActionTypeGuid
        {
            get
            {
                EnsureChildControls();
                return _hfActionTypeGuid.Value.AsGuid();
            }
        }

        /// <summary>
        /// Gets the form editor.
        /// </summary>
        /// <value>
        /// The form editor.
        /// </value>
        public WorkflowFormEditor FormEditor
        {
            get
            {
                EnsureChildControls();
                return _formEditor;
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
// action animation
$('.workflow-action > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.workflow-action-state', this).toggleClass('fa-chevron-down');
    $('i.workflow-action-state', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event
$('.workflow-action a.js-action-delete').click(function (event) {
    event.stopImmediatePropagation();
    return Rock.dialogs.confirmDelete(event, 'Action Type', 'This will also delete all the actions of this type from any existing persisted workflows!');
});

$('.workflow-action a.js-workflow-action-criteria').click(function (event) {
    event.stopImmediatePropagation();
    $(this).closest('.workflow-action').find('div.conditional-run-criteria').slideToggle();
});

$(document).on('focusout', '.js-conditional-run-criteria', function (e) {
    if ( $(this).val() == '' ) {
        $(this).closest('.workflow-action').find('a.js-workflow-action-criteria').removeClass('criteria-exists');
    } else {
        $(this).closest('.workflow-action').find('a.js-workflow-action-criteria').addClass('criteria-exists');
    }
});

$('.js-action-criteria-comparison').change( function (event) {
    var $valueRow = $(this).closest('div.conditional-run-criteria').find('div.js-text-or-ddl-row');
    if ($(this).val() == '32' || $(this).val() == '64') {
        $valueRow.slideUp();
    } else {
        $valueRow.slideDown();
    }
});

// fix so that the Reorder button will fire its event, but not the parent event
$('.workflow-action a.workflow-action-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

$('a.workflow-formfield-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

$('.workflow-action > .panel-body').on('validation-error', function() {
    var $header = $(this).siblings('header');
    $(this).slideDown();

    $expanded = $header.children('input.filter-expanded');
    $expanded.val('True');

    $('i.workflow-action-state', $header).removeClass('fa-chevron-down');
    $('i.workflow-action-state', $header).addClass('fa-chevron-up');
});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "WorkflowActionTypeEditorScript", script, true );
        }

        /// <summary>
        /// Sets the workflow activities.
        /// </summary>
        /// <value>
        /// The workflow activities.
        /// </value>
        public Dictionary<string, string> WorkflowActivities
        {
            set
            {
                EnsureChildControls();
                _formEditor.WorkflowActivities = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delete enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteEnabled
        {
            get
            {
                bool? b = ViewState["IsDeleteEnabled"] as bool?;
                return ( b == null ) ? true : b.Value;
            }

            set
            {
                ViewState["IsDeleteEnabled"] = value;
            }
        }

        /// <summary>
        /// Gets the type of the workflow action.
        /// </summary>
        /// <returns></returns>
        public WorkflowActionType GetWorkflowActionType( bool expandInvalid )
        {
            EnsureChildControls();
            WorkflowActionType result = new WorkflowActionType();
            result.Guid = new Guid( _hfActionTypeGuid.Value );

            result.CriteriaAttributeGuid = _ddlCriteriaAttribute.SelectedValueAsGuid();
            result.CriteriaComparisonType = _ddlCriteriaComparisonType.SelectedValueAsEnum<ComparisonType>();
            result.CriteriaValue = _tbddlCriteriaValue.SelectedValue;

            result.Name = _tbActionTypeName.Text;
            result.EntityTypeId = _wfatpEntityType.SelectedValueAsInt() ?? 0;
            result.IsActionCompletedOnSuccess = _cbIsActionCompletedOnSuccess.Checked;
            result.IsActivityCompletedOnSuccess = _cbIsActivityCompletedOnSuccess.Checked;

            var entityType = EntityTypeCache.Get( result.EntityTypeId );
            if ( entityType != null && entityType.Name == typeof( Rock.Workflow.Action.UserEntryForm ).FullName )
            {
                result.WorkflowForm = _formEditor.GetForm();
                if ( result.WorkflowForm == null )
                {
                    result.WorkflowForm = new WorkflowActionForm();
                    result.WorkflowForm.Actions = "Submit^^^Your information has been submitted successfully.";
                    var systemEmail = new SystemEmailService(new RockContext()).Get(SystemGuid.SystemEmail.WORKFLOW_FORM_NOTIFICATION.AsGuid());
                    if ( systemEmail != null )
                    {
                        result.WorkflowForm.NotificationSystemEmailId = systemEmail.Id;
                    }
                }
            }
            else
            {
                result.WorkflowForm = null;
            }

            result.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( _phActionAttributes, result );

            if (expandInvalid && !result.IsValid)
            {
                Expanded = true;
            }

            return result;
        }

        /// <summary>
        /// Sets the type of the workflow action.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="workflowTypeAttributes">The workflow type attributes.</param>
        public void SetWorkflowActionType(WorkflowActionType value, Dictionary<Guid, Rock.Model.Attribute> workflowTypeAttributes )
        {
            EnsureChildControls();
            _hfActionTypeGuid.Value = value.Guid.ToString();

            _ddlCriteriaAttribute.Items.Clear();
            _ddlCriteriaAttribute.Items.Add( new ListItem() );

            _tbddlCriteriaValue.DropDownList.Items.Clear();
            _tbddlCriteriaValue.DropDownList.Items.Add( new ListItem() );
            foreach ( var attribute in workflowTypeAttributes )
            {
                var li = new ListItem( attribute.Value.Name, attribute.Key.ToString() );
                li.Selected = value.CriteriaAttributeGuid.HasValue && value.CriteriaAttributeGuid.Value.ToString() == li.Value;
                _ddlCriteriaAttribute.Items.Add( li );

                _tbddlCriteriaValue.DropDownList.Items.Add( new ListItem( attribute.Value.Name, attribute.Key.ToString() ) );
            }

            _ddlCriteriaComparisonType.SetValue( value.CriteriaComparisonType.ConvertToInt() );
            _tbddlCriteriaValue.SelectedValue = value.CriteriaValue;

            _tbActionTypeName.Text = value.Name;
            _wfatpEntityType.SetValue( EntityTypeCache.Get( value.EntityTypeId ) );
            _cbIsActivityCompletedOnSuccess.Checked = value.IsActivityCompletedOnSuccess;

            var entityType = EntityTypeCache.Get( value.EntityTypeId );
            if ( entityType != null && entityType.Name == typeof( Rock.Workflow.Action.UserEntryForm ).FullName )
            {
                if (value.WorkflowForm == null)
                {
                    value.WorkflowForm = new WorkflowActionForm();
                    value.WorkflowForm.Actions = "Submit^^^Your information has been submitted successfully.";
                    var systemEmail = new SystemEmailService( new RockContext() ).Get( SystemGuid.SystemEmail.WORKFLOW_FORM_NOTIFICATION.AsGuid() );
                    if ( systemEmail != null )
                    {
                        value.WorkflowForm.NotificationSystemEmailId = systemEmail.Id;
                    }
                }
                _formEditor.SetForm( value.WorkflowForm, workflowTypeAttributes );
                _cbIsActionCompletedOnSuccess.Checked = true;
                _cbIsActionCompletedOnSuccess.Enabled = false;
            }
            else
            {
                _formEditor.SetForm( null, workflowTypeAttributes );
                _cbIsActionCompletedOnSuccess.Checked = value.IsActionCompletedOnSuccess;
                _cbIsActionCompletedOnSuccess.Enabled = true;
            }

            _phActionAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( value, _phActionAttributes, true, ValidationGroup, new List<string>() { "Active", "Order" } );
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

            _hfActionTypeGuid = new HiddenField();
            Controls.Add( _hfActionTypeGuid );
            _hfActionTypeGuid.ID = this.ID + "_hfActionTypeGuid";

            _lblActionTypeName = new Label();
            Controls.Add( _lblActionTypeName );
            _lblActionTypeName.ClientIDMode = ClientIDMode.Static;
            _lblActionTypeName.ID = this.ID + "_lblActionTypeName";

            _lbDeleteActionType = new LinkButton();
            Controls.Add( _lbDeleteActionType );
            _lbDeleteActionType.CausesValidation = false;
            _lbDeleteActionType.ID = this.ID + "_lbDeleteActionType";
            _lbDeleteActionType.CssClass = "btn btn-xs btn-square btn-danger js-action-delete";
            _lbDeleteActionType.Click += lbDeleteActionType_Click;

            var iDelete = new HtmlGenericControl( "i" );
            _lbDeleteActionType.Controls.Add( iDelete );
            iDelete.AddCssClass( "fa fa-times" );

            _ddlCriteriaAttribute = new RockDropDownList();
            Controls.Add( _ddlCriteriaAttribute );
            _ddlCriteriaAttribute.ID = this.ID + "_ddlCriteriaAttribute";
            _ddlCriteriaAttribute.EnableViewState = false;
            _ddlCriteriaAttribute.CssClass = "js-conditional-run-criteria";
            _ddlCriteriaAttribute.Label = "Run If";
            _ddlCriteriaAttribute.Help = "Optional criteria to prevent the action from running.  If the criteria is not met, this action will be skipped when this activity is processed.";

            _ddlCriteriaComparisonType = new RockDropDownList();
            Controls.Add( _ddlCriteriaComparisonType );
            _ddlCriteriaComparisonType.ID = this.ID + "_ddlCriteriaComparisonType";
            _ddlCriteriaComparisonType.EnableViewState = false;
            _ddlCriteriaComparisonType.CssClass = "js-action-criteria-comparison";
            _ddlCriteriaComparisonType.BindToEnum<ComparisonType>();
            _ddlCriteriaComparisonType.Label = "&nbsp;";

            _tbddlCriteriaValue = new RockTextOrDropDownList();
            Controls.Add( _tbddlCriteriaValue );
            _tbddlCriteriaValue.ID = this.ID + "_tbddlCriteriaValue";
            _tbddlCriteriaValue.EnableViewState = false;
            _tbddlCriteriaValue.TextBox.Label = "Text Value";
            _tbddlCriteriaValue.DropDownList.Label = "Attribute Value";

            _tbActionTypeName = new RockTextBox();
            Controls.Add( _tbActionTypeName );
            _tbActionTypeName.ID = this.ID + "_tbActionTypeName";
            _tbActionTypeName.Label = "Name";
            _tbActionTypeName.Required = true;
            _tbActionTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblActionTypeName.ID );

            _wfatpEntityType = new WorkflowActionTypePicker();
            _wfatpEntityType.SelectItem += wfatpEntityType_SelectItem;
            Controls.Add( _wfatpEntityType );
            _wfatpEntityType.ID = this.ID + "_wfatpEntityType";
            _wfatpEntityType.Label = "Action Type";

            _rlEntityTypeOverview = new RockLiteral();
            Controls.Add( _rlEntityTypeOverview );
            _rlEntityTypeOverview.ID = this.ID + "";
            _rlEntityTypeOverview.Label = "Action Type Overview";

            _cbIsActionCompletedOnSuccess = new RockCheckBox { Text = "Action is Completed on Success" };
            Controls.Add( _cbIsActionCompletedOnSuccess );
            _cbIsActionCompletedOnSuccess.ID = this.ID + "_cbIsActionCompletedOnSuccess";

            _cbIsActivityCompletedOnSuccess = new RockCheckBox { Text = "Activity is Completed on Success" };
            Controls.Add( _cbIsActivityCompletedOnSuccess );
            _cbIsActivityCompletedOnSuccess.ID = this.ID + "_cbIsActivityCompletedOnSuccess";

            _formEditor = new WorkflowFormEditor();
            Controls.Add( _formEditor );
            _formEditor.ID = this.ID + "_formEditor";

            _phActionAttributes = new PlaceHolder();
            Controls.Add( _phActionAttributes );
            _phActionAttributes.ID = this.ID + "_phActionAttributes";

        }

        void wfatpEntityType_SelectItem( object sender, EventArgs e )
        {
            var workflowActionType = GetWorkflowActionType( false );
            workflowActionType.EntityTypeId = _wfatpEntityType.SelectedValueAsInt() ?? 0;

            _hfActionTypeGuid.Value = workflowActionType.Guid.ToString();

            if ( ChangeActionTypeClick != null )
            {
                ChangeActionTypeClick( this, e );
            }

        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget workflow-action" );
            writer.AddAttribute( "data-key", _hfActionTypeGuid.Value );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix clickable panel-heading" );
            writer.RenderBeginTag( "header" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left workflow-action-name" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _lblActionTypeName.Text = _tbActionTypeName.Text;
            _lblActionTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            string criteriaExistsClass = _ddlCriteriaAttribute.SelectedValueAsGuid().HasValue ? " criteria-exists" : string.Empty;
            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link js-workflow-action-criteria{0}'><i class='fa fa-filter'></i></a>", criteriaExistsClass ) );
            writer.WriteLine( "<a class='btn btn-xs btn-link workflow-action-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='workflow-action-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            if ( IsDeleteEnabled )
            {
                _lbDeleteActionType.Visible = true;

                _lbDeleteActionType.RenderControl( writer );
            }
            else
            {
                _lbDeleteActionType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            if ( !Expanded )
            {
                // hide details if the name has already been filled in
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // add Criteria fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row conditional-run-criteria alert-warning" );
            if ( !_ddlCriteriaAttribute.SelectedValueAsGuid().HasValue )
            {
                writer.AddStyleAttribute( "display", "none" );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-lg-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-7" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlCriteriaAttribute.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-5" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlCriteriaComparisonType.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            writer.RenderEndTag();  // col-md-6

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-lg-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            var comparisonType = _ddlCriteriaComparisonType.SelectedValueAsEnum<ComparisonType>();
            _tbddlCriteriaValue.Style["display"] = ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank ) ? "none" : "block";
            _tbddlCriteriaValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // action edit fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbActionTypeName.ValidationGroup = ValidationGroup;
            _tbActionTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbIsActionCompletedOnSuccess.ValidationGroup = ValidationGroup;
            _cbIsActionCompletedOnSuccess.RenderControl( writer );
            _cbIsActivityCompletedOnSuccess.ValidationGroup = ValidationGroup;
            _cbIsActivityCompletedOnSuccess.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-lg-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _wfatpEntityType.ValidationGroup = ValidationGroup;
            _wfatpEntityType.RenderControl( writer );
            writer.RenderEndTag();

            // Add an overview(description) of the selected action type
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-lg-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            EntityTypeCache entityType = null;
            int? entityTypeId = _wfatpEntityType.SelectedValueAsInt();
            if ( entityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Get( entityTypeId.Value );
                if ( entityType != null )
                {
                    var component = ActionContainer.GetComponent( entityType.Name );
                    if ( component != null )
                    {
                        string description = string.Empty;
                        var propAttribute = component.GetType().GetCustomAttributes( typeof( System.ComponentModel.DescriptionAttribute ), false ).FirstOrDefault();
                        if ( propAttribute != null )
                        {
                            var descAttribute = propAttribute as System.ComponentModel.DescriptionAttribute;
                            if ( descAttribute != null )
                            {
                                _rlEntityTypeOverview.Label = string.Format( "'{0}' Overview", entityType.FriendlyName );
                                _rlEntityTypeOverview.Text = descAttribute.Description;
                                _rlEntityTypeOverview.RenderControl( writer );
                            }
                        }
                    }
                }
            }
            writer.RenderEndTag();  // col-md-8

            writer.RenderEndTag();  // row

            if ( entityType != null && entityType.Name == typeof( Rock.Workflow.Action.UserEntryForm ).FullName )
            {
                _formEditor.ValidationGroup = ValidationGroup;
                _formEditor.RenderControl( writer );
            }

            _phActionAttributes.RenderControl( writer );

            // widget-content div
            writer.RenderEndTag();

            // article tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteActionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDeleteActionType_Click( object sender, EventArgs e )
        {
            if ( DeleteActionTypeClick != null )
            {
                DeleteActionTypeClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete action type click].
        /// </summary>
        public event EventHandler DeleteActionTypeClick;

        /// <summary>
        /// Occurs when [change action type click].
        /// </summary>
        public event EventHandler ChangeActionTypeClick;

    }
}