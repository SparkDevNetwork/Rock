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
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by registration template detail block to edit registrant forms
    /// </summary>
    [ToolboxData( "<{0}:RegistrationTemplateFormEditor runat=server></{0}:RegistrationTemplateFormEditor>" )]
    public class RegistrationTemplateFormEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfFormGuid;
        private HiddenField _hfFormId;
        private Label _lblFormName;
        private LinkButton _lbDeleteForm;

        private RockTextBox _tbFormName;

        private Grid _gFields;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RegistrationTemplateFormEditor"/> is expanded.
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
        /// Gets the form identifier.
        /// </summary>
        /// <value>
        /// The form identifier.
        /// </value>
        public int FormId
        {
            get
            {
                EnsureChildControls();
                return _hfFormId.ValueAsInt();
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.template-form > header').on('click', function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.template-form-state', this).toggleClass('fa-chevron-down');
    $('i.template-form-state', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event
$('.template-form a.js-activity-delete').on('click', function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event
$('.template-form a.template-form-reorder').on('click', function (event) {
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

            ScriptManager.RegisterStartupScript( this, this.GetType(), "TemplateFormEditorScript", script, true );
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is delete enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is delete enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDeleteEnabled
        {
            get => ViewState["IsDeleteEnabled"] as bool? ?? true;
            set => ViewState["IsDeleteEnabled"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default form.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is default form; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefaultForm
        {
            get => ViewState["IsDefaultForm"] as bool? ?? false;
            set => ViewState["IsDefaultForm"] = value;
        }

        /// <summary>
        /// Gets or sets the type of the workflow activity.
        /// </summary>
        /// <value>
        /// The type of the workflow activity.
        /// </value>
        public RegistrationTemplateForm GetForm( bool expandInvalid )
        {
            EnsureChildControls();
            RegistrationTemplateForm result = new RegistrationTemplateForm();
            result.Id = _hfFormId.ValueAsInt();
            result.Guid = new Guid( _hfFormGuid.Value );
            result.Name = _tbFormName.Text;

            if ( expandInvalid && !Expanded && !result.IsValid )
            {
                Expanded = true;
            }

            return result;
        }

        /// <summary>
        /// Sets the type of the workflow activity.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetForm( RegistrationTemplateForm value )
        {
            EnsureChildControls();
            _hfFormGuid.Value = value.Guid.ToString();
            _hfFormId.Value = value.Id.ToString();
            _tbFormName.Text = value.Name;
        }

        /// <summary>
        /// The filterable fields count. If there is less than 2, don't show the FilterButton (since there would be no other fields to use as criteria)
        /// </summary>
        private int _filterableFieldsCount = 0;

        /// <summary>
        /// Binds the fields grid.
        /// </summary>
        /// <param name="formFields">The fields.</param>
        public void BindFieldsGrid( List<RegistrationTemplateFormField> formFields )
        {
            _filterableFieldsCount = formFields.Where( a => a.FieldSource != RegistrationFieldSource.PersonField ).Count();
            _gFields.DataSource = formFields.OrderBy( a => a.Order ).ToList();
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

            _hfFormId = new HiddenField();
            Controls.Add( _hfFormId );
            _hfFormId.ID = this.ID + "_hfFormId";

            _lblFormName = new Label();
            Controls.Add( _lblFormName );
            _lblFormName.ClientIDMode = ClientIDMode.Static;
            _lblFormName.ID = this.ID + "_lblFormName";

            _lbDeleteForm = new LinkButton();
            Controls.Add( _lbDeleteForm );
            _lbDeleteForm.CausesValidation = false;
            _lbDeleteForm.ID = this.ID + "_lbDeleteForm";
            _lbDeleteForm.CssClass = "btn btn-xs btn-square btn-danger js-activity-delete";
            _lbDeleteForm.Click += lbDeleteForm_Click;
            _lbDeleteForm.Controls.Add( new LiteralControl { Text = "<i class='fa fa-times'></i>" } );

            _tbFormName = new RockTextBox();
            Controls.Add( _tbFormName );
            _tbFormName.ID = this.ID + "_tbFormName";
            _tbFormName.Label = "Form Name";
            _tbFormName.Required = true;
            _tbFormName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblFormName.ID );

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

            var nameField = new RockLiteralField();
            nameField.HeaderText = "Field";
            nameField.DataBound += NameField_DataBound;
            _gFields.Columns.Add( nameField );

            var sourceField = new RockLiteralField();
            sourceField.HeaderText = "Source";
            sourceField.DataBound += SourceField_DataBound;
            _gFields.Columns.Add( sourceField );

            var typeField = new RockLiteralField();
            typeField.HeaderText = "Type";
            typeField.DataBound += TypeField_DataBound;
            _gFields.Columns.Add( typeField );

            var isInternalField = new BoolField();
            isInternalField.DataField = "IsInternal";
            isInternalField.HeaderText = "Internal";
            _gFields.Columns.Add( isInternalField );

            var isSharedValueField = new BoolField();
            isSharedValueField.DataField = "IsSharedValue";
            isSharedValueField.HeaderText = "Common";
            _gFields.Columns.Add( isSharedValueField );

            var lockExistingValueField = new BoolField
            {
                DataField = nameof( RegistrationTemplateFormField.IsLockedIfValuesExist ),
                HeaderText = "Lock Existing Value"
            };
            _gFields.Columns.Add( lockExistingValueField );

            var showCurrentValueField = new BoolField();
            showCurrentValueField.DataField = "ShowCurrentValue";
            showCurrentValueField.HeaderText = "Use Current Value";
            _gFields.Columns.Add( showCurrentValueField );

            var isRequiredField = new BoolField();
            isRequiredField.DataField = "IsRequired";
            isRequiredField.HeaderText = "Required";
            _gFields.Columns.Add( isRequiredField );

            var isGridField = new BoolField();
            isGridField.DataField = "IsGridField";
            isGridField.HeaderText = "Show on Grid";
            _gFields.Columns.Add( isGridField );

            var showOnWaitListField = new BoolField();
            showOnWaitListField.DataField = "ShowOnWaitlist";
            showOnWaitListField.HeaderText = "Show on Wait List";
            _gFields.Columns.Add( showOnWaitListField );

            var btnFieldFilterField = new LinkButtonField();
            btnFieldFilterField.CssClass = "btn btn-default btn-sm attribute-criteria";
            btnFieldFilterField.Text = "<i class='fa fa-filter'></i>";
            btnFieldFilterField.Click += gFields_btnFieldFilterField_Click;
            btnFieldFilterField.DataBound += btnFieldFilterField_DataBound;
            _gFields.Columns.Add( btnFieldFilterField );

            var editField = new EditField();
            editField.Click += gFields_Edit;
            _gFields.Columns.Add( editField );

            var delField = new DeleteField();
            delField.Click += gFields_Delete;
            delField.DataBound += delField_DataBound;
            _gFields.Columns.Add( delField );
        }

        /// <summary>
        /// Handles the DataBound event of the delField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void delField_DataBound( object sender, RowEventArgs e )
        {
            LinkButton linkButton = sender as LinkButton;
            RegistrationTemplateFormField field = e.Row.DataItem as RegistrationTemplateFormField;
            if ( this.IsDefaultForm )
            {
                if ( field != null && linkButton != null )
                {
                    if ( field.FieldSource == RegistrationFieldSource.PersonField && ( field.PersonFieldType == RegistrationPersonFieldType.FirstName || field.PersonFieldType == RegistrationPersonFieldType.LastName ) )
                    {
                        // hide the Delete button if this is the Person FirstName or LastName field on the DefaultForm
                        linkButton.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DataBound event of the btnFieldFilterField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void btnFieldFilterField_DataBound( object sender, RowEventArgs e )
        {
            LinkButton linkButton = sender as LinkButton;
            RegistrationTemplateFormField field = e.Row.DataItem as RegistrationTemplateFormField;

            if ( field != null && linkButton != null )
            {
                if ( ( field.FieldSource == RegistrationFieldSource.PersonField ) || _filterableFieldsCount < 2 )
                {
                    linkButton.Visible = false;
                }
                else
                {
                    if ( field.FieldVisibilityRules.RuleList.Any() )
                    {
                        linkButton.RemoveCssClass( "btn-default" );
                        linkButton.AddCssClass( "btn-warning" );
                        linkButton.AddCssClass( "criteria-exists" );
                    }
                    else
                    {
                        linkButton.AddCssClass( "btn-default" );
                        linkButton.RemoveCssClass( "btn-warning" );
                        linkButton.RemoveCssClass( "criteria-exists" );
                    }

                    linkButton.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the DataBound event of the TypeField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void TypeField_DataBound( object sender, RowEventArgs e )
        {
            Literal literal = sender as Literal;
            RegistrationTemplateFormField field = e.Row.DataItem as RegistrationTemplateFormField;
            if ( field != null && literal != null )
            {
                if ( field.FieldSource != RegistrationFieldSource.PersonField && field.Attribute.FieldTypeId != 0 )
                {
                    var fieldType = FieldTypeCache.Get( field.Attribute.FieldTypeId );
                    if ( fieldType != null )
                    {
                        literal.Text = fieldType.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DataBound event of the SourceField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void SourceField_DataBound( object sender, RowEventArgs e )
        {
            Literal literal = sender as Literal;
            RegistrationTemplateFormField field = e.Row.DataItem as RegistrationTemplateFormField;
            if ( field != null && literal != null )
            {
                literal.Text = field.FieldSource.ConvertToString();
            }
        }

        /// <summary>
        /// Handles the DataBound event of the NameField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void NameField_DataBound( object sender, RowEventArgs e )
        {
            Literal literal = sender as Literal;
            RegistrationTemplateFormField field = e.Row.DataItem as RegistrationTemplateFormField;
            if ( field != null && literal != null )
            {
                literal.Text = ( field.FieldSource != RegistrationFieldSource.PersonField && field.Attribute != null ) ?
                            field.Attribute.Name : field.PersonFieldType.ConvertToString();
            }
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

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _lblFormName.Text = _tbFormName.Text;
            _lblFormName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();

            // Name div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !this.IsDefaultForm )
            {
                writer.WriteLine( "<a class='btn btn-xs btn-link form-reorder'><i class='fa fa-bars'></i></a>" );
            }

            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='form-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            if ( IsDeleteEnabled )
            {
                _lbDeleteForm.Visible = true;
                _lbDeleteForm.RenderControl( writer );
            }
            else
            {
                _lbDeleteForm.Visible = false;
            }

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
            _hfFormId.RenderControl( writer );
            _tbFormName.Visible = !IsDefaultForm;
            _tbFormName.ValidationGroup = ValidationGroup;
            _tbFormName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();

            _gFields.RenderControl( writer );

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
                var eventArg = new TemplateFormFieldEventArg( FormGuid );
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
                var eventArg = new TemplateFormFieldEventArg( FormGuid, Guid.Empty );
                AddFieldClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Click event of the gFields_btnFieldFilterField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gFields_btnFieldFilterField_Click( object sender, RowEventArgs e )
        {
            if ( FilterFieldClick != null )
            {
                var eventArg = new TemplateFormFieldEventArg( FormGuid, ( Guid ) e.RowKeyValue );
                FilterFieldClick( this, eventArg );
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
                var eventArg = new TemplateFormFieldEventArg( FormGuid, ( Guid ) e.RowKeyValue );
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
                var eventArg = new TemplateFormFieldEventArg( FormGuid, e.OldIndex, e.NewIndex );
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
                var eventArg = new TemplateFormFieldEventArg( FormGuid, ( Guid ) e.RowKeyValue );
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
        public event EventHandler<TemplateFormFieldEventArg> RebindFieldClick;

        /// <summary>
        /// Occurs when [add field click].
        /// </summary>
        public event EventHandler<TemplateFormFieldEventArg> AddFieldClick;

        /// <summary>
        /// Occurs when [filter field click].
        /// </summary>
        public event EventHandler<TemplateFormFieldEventArg> FilterFieldClick;

        /// <summary>
        /// Occurs when [edit field click].
        /// </summary>
        public event EventHandler<TemplateFormFieldEventArg> EditFieldClick;

        /// <summary>
        /// Occurs when [edit field click].
        /// </summary>
        public event EventHandler<TemplateFormFieldEventArg> ReorderFieldClick;

        /// <summary>
        /// Occurs when [delete field click].
        /// </summary>
        public event EventHandler<TemplateFormFieldEventArg> DeleteFieldClick;

    }

    /// <summary>
    ///
    /// </summary>
    public class TemplateFormFieldEventArg : EventArgs
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
        /// Initializes a new instance of the <see cref="TemplateFormFieldEventArg"/> class.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        public TemplateFormFieldEventArg( Guid activityTypeGuid )
        {
            FormGuid = activityTypeGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateFormFieldEventArg"/> class.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="formFieldGuid">The form field unique identifier.</param>
        public TemplateFormFieldEventArg( Guid formGuid, Guid formFieldGuid )
        {
            FormGuid = formGuid;
            FormFieldGuid = formFieldGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateFormFieldEventArg" /> class.
        /// </summary>
        /// <param name="formGuid">The form unique identifier.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public TemplateFormFieldEventArg( Guid formGuid, int oldIndex, int newIndex )
        {
            FormGuid = formGuid;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }


    }
}