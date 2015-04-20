// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Workflow Action Form Editor
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActionFormEditor runat=server></{0}:WorkflowActionFormEditor>" )]
    public class WorkflowFormEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenField _hfFormGuid;
        private RockDropDownList _ddlNotificationSystemEmail;
        private RockCheckBox _cbIncludeActions;
        private RockCheckBox _cbAllowNotes;
        private CodeEditor _ceHeaderText;
        private CodeEditor _ceFooterText;
        private WorkflowFormActionList _falActions;
        private RockDropDownList _ddlActionAttribute;

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
        /// Gets or sets the form.
        /// </summary>
        /// <value>
        /// The form.
        /// </value>
        public WorkflowActionForm GetForm()
        {
            EnsureChildControls();
            var form = new WorkflowActionForm();
            form.Guid = _hfFormGuid.Value.AsGuid();
            if ( form.Guid != Guid.Empty )
            {
                form.NotificationSystemEmailId = _ddlNotificationSystemEmail.SelectedValueAsId();
                form.IncludeActionsInNotification = _cbIncludeActions.Checked;
                form.Header = _ceHeaderText.Text;
                form.Footer = _ceFooterText.Text;
                form.Actions = _falActions.Value;
                form.AllowNotes = _cbAllowNotes.Checked;

                foreach ( var row in AttributeRows )
                {
                    var formAttribute = new WorkflowActionFormAttribute();
                    formAttribute.Attribute = new Rock.Model.Attribute { Guid = row.AttributeGuid, Name = row.AttributeName };
                    formAttribute.Guid = row.Guid;
                    formAttribute.Order = row.Order;
                    formAttribute.IsVisible = row.IsVisible;
                    formAttribute.IsReadOnly = !row.IsEditable;
                    formAttribute.IsRequired = row.IsRequired;
                    formAttribute.HideLabel = row.HideLabel;
                    formAttribute.PreHtml = row.PreHtml;
                    formAttribute.PostHtml = row.PostHtml;
                    form.FormAttributes.Add( formAttribute );
                }

                form.ActionAttributeGuid = _ddlActionAttribute.SelectedValueAsGuid();

                return form;
            }
            return null;
        }

        /// <summary>
        /// Sets the form.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="workflowTypeAttributes">The workflow type attributes.</param>
        public void SetForm( WorkflowActionForm value, Dictionary<Guid, Rock.Model.Attribute> workflowTypeAttributes )
        {
            EnsureChildControls();

            if ( value != null )
            {
                _hfFormGuid.Value = value.Guid.ToString();
                _ddlNotificationSystemEmail.SetValue( value.NotificationSystemEmailId );
                _cbIncludeActions.Checked = value.IncludeActionsInNotification;
                _ceHeaderText.Text = value.Header;
                _ceFooterText.Text = value.Footer;
                _falActions.Value = value.Actions;
                _cbAllowNotes.Checked = value.AllowNotes.HasValue && value.AllowNotes.Value;

                // Remove any existing rows (shouldn't be any)
                foreach ( var attributeRow in Controls.OfType<WorkflowFormAttributeRow>() )
                {
                    Controls.Remove( attributeRow );
                }

                foreach ( var formAttribute in value.FormAttributes.OrderBy( a => a.Order ) )
                {
                    var row = new WorkflowFormAttributeRow();
                    row.AttributeGuid = formAttribute.Attribute.Guid;
                    row.AttributeName = formAttribute.Attribute.Name;
                    row.Guid = formAttribute.Guid;
                    row.IsVisible = formAttribute.IsVisible;
                    row.IsEditable = !formAttribute.IsReadOnly;
                    row.IsRequired = formAttribute.IsRequired;
                    row.HideLabel = formAttribute.HideLabel;
                    row.PreHtml = formAttribute.PreHtml;
                    row.PostHtml = formAttribute.PostHtml;
                    Controls.Add( row );
                }

                _ddlActionAttribute.Items.Clear();
                _ddlActionAttribute.Items.Add( new ListItem() );
                foreach ( var attributeItem in workflowTypeAttributes )
                {
                    var fieldType = FieldTypeCache.Read( attributeItem.Value.FieldTypeId );
                    if ( fieldType != null && fieldType.Field is Rock.Field.Types.TextFieldType )
                    {
                        var li = new ListItem( attributeItem.Value.Name, attributeItem.Key.ToString() );
                        li.Selected = value.ActionAttributeGuid.HasValue && value.ActionAttributeGuid.Value.ToString() == li.Value;
                        _ddlActionAttribute.Items.Add( li );
                    }
                }        
                
            }
            else
            {
                _hfFormGuid.Value = string.Empty;
                _ddlNotificationSystemEmail.SelectedIndex = 0;
                _cbIncludeActions.Checked = true;
                _ceHeaderText.Text = string.Empty;
                _ceFooterText.Text = string.Empty;
                _falActions.Value = "Submit^^^Your information has been submitted successfully.";
                _ddlNotificationSystemEmail.SelectedIndex = 0;
                _cbAllowNotes.Checked = false;
            }
        }

        /// <summary>
        /// Gets or sets the workflow activities.
        /// </summary>
        /// <value>
        /// The workflow activities.
        /// </value>
        public Dictionary<string, string> WorkflowActivities
        {
            get
            {
                EnsureChildControls();
                return _falActions.Activities;
            }

            set
            {
                EnsureChildControls();
                _falActions.Activities = value;
            }
        }

        /// <summary>
        /// Gets the attribute rows.
        /// </summary>
        /// <value>
        /// The attribute rows.
        /// </value>
        public List<WorkflowFormAttributeRow> AttributeRows
        {
            get
            {
                var rows = new List<WorkflowFormAttributeRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is WorkflowFormAttributeRow )
                    {
                        var workflowFormAttributeRow = control as WorkflowFormAttributeRow;
                        if ( workflowFormAttributeRow != null )
                        {
                            rows.Add( workflowFormAttributeRow );
                        }
                    }
                }

                return rows.OrderBy( r => r.Order ).ToList();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfFormGuid = new HiddenField();
            _hfFormGuid.ID = this.ID + "_hfFormGuid";
            Controls.Add( _hfFormGuid );

            _ddlNotificationSystemEmail = new RockDropDownList();
            _ddlNotificationSystemEmail.DataValueField = "Id";
            _ddlNotificationSystemEmail.DataTextField = "Title";
            _ddlNotificationSystemEmail.Label = "Notification Email";
            _ddlNotificationSystemEmail.Help = "An optional system email that should be sent to the person or people assigned to this activity (Any System Email with a category of 'Workflow').";
            _ddlNotificationSystemEmail.ID = this.ID + "_ddlNotificationSystemEmail";
            Controls.Add( _ddlNotificationSystemEmail );

            Guid? systemEmails = Rock.SystemGuid.Category.SYSTEM_EMAIL_WORKFLOW.AsGuid();
            if ( systemEmails.HasValue )
            {
                _ddlNotificationSystemEmail.DataSource = new SystemEmailService( new RockContext() ).Queryable()
                    .Where( e => e.Category.Guid.Equals( systemEmails.Value ) )
                    .OrderBy( e => e.Title )
                    .ToList();
                _ddlNotificationSystemEmail.DataBind();
            }
            _ddlNotificationSystemEmail.Items.Insert( 0, new ListItem( "None", "0" ) );

            _cbIncludeActions = new RockCheckBox();
            _cbIncludeActions.Label = "Include Actions in Email";
            _cbIncludeActions.Text = "Yes";
            _cbIncludeActions.Help = "Should the email include the option for recipient to select an action directly from within the email? Note: This only applies if none of the the form fields are required.";
            _cbIncludeActions.ID = this.ID + "_cbIncludeActions";
            Controls.Add( _cbIncludeActions );

            _ceHeaderText = new CodeEditor();
            _ceHeaderText.Label = "Form Header";
            _ceHeaderText.Help = "Text to display to user above the form fields. <span class='tip tip-lava'></span> <span class='tip tip-html'>";
            _ceHeaderText.ID = this.ID + "_tbHeaderText";
            _ceHeaderText.EditorMode = CodeEditorMode.Html;
            _ceHeaderText.EditorTheme = CodeEditorTheme.Rock;
            _ceHeaderText.EditorHeight = "200";
            Controls.Add( _ceHeaderText );

            _ceFooterText = new CodeEditor();
            _ceFooterText.Label = "Form Footer";
            _ceFooterText.Help = "Text to display to user below the form fields. <span class='tip tip-lava'></span> <span class='tip tip-html'>";
            _ceFooterText.ID = this.ID + "_tbFooterText";
            _ceFooterText.EditorMode = CodeEditorMode.Html;
            _ceFooterText.EditorTheme = CodeEditorTheme.Rock;
            _ceFooterText.EditorHeight = "200";
            Controls.Add( _ceFooterText );

            _falActions = new WorkflowFormActionList();
            _falActions.ID = this.ID + "_falActions";
            Controls.Add( _falActions );

            _ddlActionAttribute = new RockDropDownList();
            _ddlActionAttribute.ID = this.ID + "_ddlActionAttribute";
            _ddlActionAttribute.Label = "Command Selected Attribute";
            _ddlActionAttribute.Help = "Optional text attribute that should be updated with the selected command label.";
            Controls.Add( _ddlActionAttribute );

            _cbAllowNotes = new RockCheckBox();
            _cbAllowNotes.Label = "Enable Note Entry";
            _cbAllowNotes.Text = "Yes";
            _cbAllowNotes.Help = "Should this form include an area for viewing and editing notes related to the workflow?";
            _cbAllowNotes.ID = this.ID + "_cbAllowNotes";
            Controls.Add( _cbAllowNotes );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( _hfFormGuid.Value.AsGuid() != Guid.Empty )
            {
                _hfFormGuid.RenderControl( writer );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ddlNotificationSystemEmail.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbIncludeActions.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();  // row

                _cbAllowNotes.RenderControl( writer );

                _ceHeaderText.ValidationGroup = ValidationGroup;
                _ceHeaderText.RenderControl( writer );

                // Attributes
                if ( AttributeRows.Any() )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                    writer.Write( "Form Fields" );

                    writer.AddAttribute( "class", "help" );
                    writer.AddAttribute( "href", "#" );
                    writer.RenderBeginTag( HtmlTextWriterTag.A );
                    writer.AddAttribute( "class", "fa fa-question-circle" );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    writer.AddAttribute( "class", "alert alert-info" );
                    writer.AddAttribute( "style", "display:none" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.RenderBeginTag( HtmlTextWriterTag.Small );
                    writer.Write( "The fields (attributes) to display on the entry form" );
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    writer.RenderEndTag();      // Label

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-table table table-condensed table-light" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Table );

                    writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                    writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-columncommand" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( "&nbsp;" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-3" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Field" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-9" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Visible" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Editable" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Required" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Hide Label" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Pre-HTML" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-2" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    writer.Write( "Post-HTML" );
                    writer.RenderEndTag();

                    writer.RenderEndTag();      // row

                    writer.RenderEndTag();      // col-xs-9

                    writer.RenderEndTag();      // row

                    writer.RenderEndTag();      // th

                    writer.RenderEndTag();      // tr
                    writer.RenderEndTag();      // thead

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-formfield-list" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                    foreach ( var row in AttributeRows )
                    {
                        row.RenderControl( writer );
                    }

                    writer.RenderEndTag();      // tbody

                    writer.RenderEndTag();      // table

                    writer.RenderEndTag();      // Div.form-group
                }

                _ceFooterText.ValidationGroup = ValidationGroup;
                _ceFooterText.RenderControl( writer );
                _falActions.RenderControl( writer );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _ddlActionAttribute.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.RenderEndTag();

                writer.RenderEndTag();  // row
            }
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                if ( Controls[i] is WorkflowFormAttributeRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }
    }
}