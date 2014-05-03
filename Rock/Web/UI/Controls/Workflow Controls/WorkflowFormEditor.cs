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
    public class WorkflowFormEditor : CompositeControl
    {
        private HiddenField _hfFormGuid;
        private RockTextBox _tbHeaderText;
        private RockTextBox _tbFooterText;
        private RockTextBox _tbInactiveMessage;
        private RockControlWrapper _rcwActions;
        private KeyValueList _kvlActions;

        /// <summary>
        /// Gets or sets the form.
        /// </summary>
        /// <value>
        /// The form.
        /// </value>
        public WorkflowActionForm Form 
        {
            get
            {
                EnsureChildControls();
                var form = new WorkflowActionForm();
                form.Guid = _hfFormGuid.Value.AsGuid();
                if ( form.Guid != Guid.Empty )
                {
                    form.Header = _tbHeaderText.Text;
                    form.Footer = _tbFooterText.Text;
                    form.Actions = _kvlActions.Value;
                    form.InactiveMessage = _tbInactiveMessage.Text;

                    foreach ( var row in AttributeRows )
                    {
                        var formAttribute = new WorkflowActionFormAttribute();
                        formAttribute.Attribute = new Rock.Model.Attribute { Guid = row.AttributeGuid };
                        formAttribute.Guid = row.Guid;
                        formAttribute.Order = row.Order;
                        formAttribute.IsVisible = row.IsVisible;
                        formAttribute.IsReadOnly = !row.IsEditable;
                        formAttribute.IsRequired = row.IsRequired;
                        form.FormAttributes.Add( formAttribute );
                    }

                    return form;
                }
                return null;
            }

            set
            {
                EnsureChildControls();
                
                if ( value != null )
                {
                    _hfFormGuid.Value = value.Guid.ToString();
                    _tbHeaderText.Text = value.Header;
                    _tbFooterText.Text = value.Footer;
                    _kvlActions.Value = value.Actions;
                    _tbInactiveMessage.Text = value.InactiveMessage;

                    UpdateRows( value.FormAttributes.OrderBy( a => a.Order ) );
                }
                else
                {
                    _hfFormGuid.Value = string.Empty;
                    _tbHeaderText.Text = string.Empty;
                    _tbFooterText.Text = string.Empty;
                    _kvlActions.Value = "Submit^Submit";
                    _tbInactiveMessage.Text = string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the workflow attributes.
        /// </summary>
        /// <value>
        /// The workflow attributes.
        /// </value>
        public Dictionary<Guid, string> WorkflowAttributes
        {
            get
            {
                var workflowAttributes = ViewState["WorkflowAttributes"] as Dictionary<Guid, string>;
                if ( workflowAttributes == null )
                {
                    workflowAttributes = new Dictionary<Guid, string>();
                    ViewState["WorkflowAttributes"] = workflowAttributes;
                }
                return workflowAttributes;
            }

            set
            {
                ViewState["WorkflowAttributes"] = value;
                UpdateRows();
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
                return _kvlActions.CustomValues;
            }

            set
            {
                EnsureChildControls();
                _kvlActions.CustomValues = value;
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

            _tbHeaderText = new RockTextBox();
            _tbHeaderText.Label = "Form Header";
            _tbHeaderText.Help = "Text to display to user above the form fields.";
            _tbHeaderText.ID = this.ID + "_tbHeaderText";
            _tbHeaderText.TextMode = TextBoxMode.MultiLine;
            _tbHeaderText.Rows = 3;
            Controls.Add( _tbHeaderText );
            
            _tbFooterText = new RockTextBox();
            _tbFooterText.Label = "Form Footer";
            _tbFooterText.Help = "Text to display to user below the form fields.";
            _tbFooterText.ID = this.ID + "_tbFooterText";
            _tbFooterText.TextMode = TextBoxMode.MultiLine;
            _tbFooterText.Rows = 3;
            Controls.Add( _tbFooterText );

            _rcwActions = new RockControlWrapper();
            _rcwActions.Label = "Action Buttons";
            _rcwActions.Help = "The Action button text and the action value to save when user clicks the action.";
            _rcwActions.ID = this.ID + "_rcwActions";
            Controls.Add( _rcwActions );

            _kvlActions = new KeyValueList();
            _kvlActions.ID = this.ID + "_kvlActions";
            _kvlActions.KeyPrompt = "Button Text";
            _kvlActions.ValuePrompt = "Action Value";
            _rcwActions.Controls.Add( _kvlActions );

            _tbInactiveMessage = new RockTextBox();
            _tbInactiveMessage.Label = "Inactive Message";
            _tbInactiveMessage.Help = "Text to display to user when attempting to view entry form when action or activity is not active.";
            _tbInactiveMessage.ID = this.ID + "_tbInactiveMessage";
            _tbInactiveMessage.TextMode = TextBoxMode.MultiLine;
            _tbInactiveMessage.Rows = 2;
            Controls.Add( _tbInactiveMessage );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl ( HtmlTextWriter writer )
        {
            if ( _hfFormGuid.Value.AsGuid() != Guid.Empty )
            {
                _tbHeaderText.RenderControl( writer );

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
                    writer.Write( "Field" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( "Visible" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( "Editable" );
                    writer.RenderEndTag();

                    writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( "Required" );
                    writer.RenderEndTag();

                    writer.RenderEndTag();  // tr
                    writer.RenderEndTag();  // thead

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-formfield-list" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                    foreach ( var row in AttributeRows )
                    {
                        row.RenderControl( writer );
                    }

                    writer.RenderEndTag();  // tbody

                    writer.RenderEndTag();  // table

                    writer.RenderEndTag();  // Div.form-group
                }

                _tbFooterText.RenderControl( writer );
                _rcwActions.RenderControl( writer );
                _tbInactiveMessage.RenderControl( writer );
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

        /// <summary>
        /// Updates the rows.
        /// </summary>
        /// <param name="formAttributes">The form attributes.</param>
        public void UpdateRows( IEnumerable<WorkflowActionFormAttribute> formAttributes = null )
        {
            // Get attributes
            var workflowAttributes = WorkflowAttributes;

            // Find any existing rows that are still valid and remove any invalid rows
            var existingRows = new Dictionary<Guid, WorkflowFormAttributeRow>();
            for ( int i = Controls.Count - 1; i >= 0; i-- )
            {
                var row = Controls[i] as WorkflowFormAttributeRow;
                if ( row != null )
                {
                    if ( workflowAttributes.ContainsKey( row.AttributeGuid ) )
                    {
                        existingRows.Add( row.AttributeGuid, row );
                    }
                    else
                    {
                        Controls.RemoveAt( i );
                    }
                }
            }

            int artificialOrder = 1000;
            foreach ( var workflowAttribute in workflowAttributes )
            {
                WorkflowFormAttributeRow row = null;
                if ( existingRows.ContainsKey( workflowAttribute.Key ) )
                {
                    row = existingRows[workflowAttribute.Key];
                }
                else
                {
                    row = new WorkflowFormAttributeRow();
                    row.AttributeGuid = workflowAttribute.Key;
                    row.AttributeName = workflowAttribute.Value;
                    Controls.Add( row );
                }

                row.Order = artificialOrder++;

                if ( formAttributes != null )
                {
                    var formAttribute = formAttributes
                        .Where( a => a.Attribute.Guid == workflowAttribute.Key )
                        .FirstOrDefault();

                    if ( formAttribute != null )
                    {
                        row.Guid = formAttribute.Guid;
                        row.Order = formAttribute.Order;
                        row.IsVisible = formAttribute.IsVisible;
                        row.IsEditable = !formAttribute.IsReadOnly;
                        row.IsRequired = formAttribute.IsRequired;
                    }
                }

                if ( row.Guid.IsEmpty() )
                {
                    row.Guid = Guid.NewGuid();
                }

            }
        }
    }
}