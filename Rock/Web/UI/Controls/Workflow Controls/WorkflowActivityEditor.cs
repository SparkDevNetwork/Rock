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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by WorkflowDetail block to edit a workflow activity
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActivityEditor runat=server></{0}:WorkflowActivityEditor>" )]
    public class WorkflowActivityEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenField _hfExpanded;
        private HiddenField _hfActivityGuid;
        private Label _lblActivityTypeName;
        private Label _lblActivityTypeDescription;
        private Label _lblStatus;
        private LinkButton _lbDeleteActivityType;

        private PersonPicker _ppAssignedToPerson;
        private GroupPicker _gpAssignedToGroup;
        private RockDropDownList _ddlAssignedToRole;

        private RockLiteral _lAssignedToPerson;
        private RockLiteral _lAssignedToGroup;
        private RockLiteral _lAssignedToRole;

        private RockCheckBox _cbActivityIsComplete;

        private Literal _lState;

        private PlaceHolder _phAttributes;

        /// <summary>
        /// Gets or sets the activity unique identifier.
        /// </summary>
        /// <value>
        /// The activity unique identifier.
        /// </value>
        public Guid ActivityGuid
        {
            get
            {
                EnsureChildControls();
                return _hfActivityGuid.Value.AsGuid();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="WorkflowActivityTypeEditor"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();

                bool expanded = false;
                if ( !bool.TryParse( _hfExpanded.Value, out expanded ) )
                {
                    expanded = false;
                }

                return expanded;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can edit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can edit; otherwise, <c>false</c>.
        /// </value>
        public bool CanEdit
        {
            get
            {
                return ViewState["CanEdit"] as bool? ?? false;
            }
            set
            {
                ViewState["CanEdit"] = value;
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.workflow-activity > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('i.workflow-activity-state', this).toggleClass('fa-chevron-down');
    $('i.workflow-activity-state', this).toggleClass('fa-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.workflow-activity a.js-activity-delete').click(function (event) {
    event.stopImmediatePropagation();
    return Rock.dialogs.confirmDelete(event, 'Activity' );
});

$('.workflow-activity > .panel-body').on('validation-error', function() {
    var $header = $(this).siblings('header');
    $(this).slideDown();

    $expanded = $header.children('input.filter-expanded');
    $expanded.val('True');

    $('i.workflow-activity-state', $header).removeClass('fa-chevron-down');
    $('i.workflow-activity-state', $header).addClass('fa-chevron-up');

    return false;
});
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "WorkflowActivityEditorScript", script, true );
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
        /// Gets or sets the workflow activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <param name="expandInvalid">if set to <c>true</c> [expand invalid].</param>
        /// <value>
        /// The workflow activity.
        /// </value>
        public void GetWorkflowActivity( WorkflowActivity activity, bool expandInvalid )
        {
            EnsureChildControls();

            if ( !activity.CompletedDateTime.HasValue && _cbActivityIsComplete.Checked )
            {
                activity.CompletedDateTime = RockDateTime.Now;
            }
            else if ( activity.CompletedDateTime.HasValue && !_cbActivityIsComplete.Checked )
            {
                activity.CompletedDateTime = null;
            }

            if (_ppAssignedToPerson.SelectedValue.HasValue)
            {
                activity.AssignedPersonAliasId = _ppAssignedToPerson.PersonAliasId;
            } 
            else
            {
                activity.AssignedPersonAliasId = null;
            }

            
            if (_gpAssignedToGroup.SelectedValueAsInt().HasValue)
            {
                activity.AssignedGroupId = _gpAssignedToGroup.SelectedValueAsInt();
            }
            else if (_ddlAssignedToRole.SelectedValueAsInt().HasValue)
            {
                activity.AssignedGroupId = _ddlAssignedToRole.SelectedValueAsInt();
            }
            else
            {
                activity.AssignedGroupId = null;
            }

            Attribute.Helper.GetEditValues( _phAttributes, activity );

            foreach ( WorkflowActionEditor actionEditor in this.Controls.OfType<WorkflowActionEditor>() )
            {
                var action = activity.Actions.Where( a => a.Guid.Equals(actionEditor.ActionGuid)).FirstOrDefault();
                if (action != null)
                {
                    actionEditor.GetWorkflowAction( action );
                }
            }

            if (expandInvalid && !Expanded && !activity.IsValid)
            {
                Expanded = true;
            }
        }

        /// <summary>
        /// Sets the workflow activity.
        /// </summary>
        /// <param name="activity">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        public void SetWorkflowActivity( WorkflowActivity activity, RockContext rockContext, bool setValues = false )
        {
            EnsureChildControls();

            _hfActivityGuid.Value = activity.Guid.ToString();

            _lblActivityTypeName.Text = activity.ActivityType.Name;
            _lblActivityTypeDescription.Text = activity.ActivityType.Description;

            if ( activity.CompletedDateTime.HasValue )
            {
                _lblStatus.Visible = true;
                _lblStatus.Text = "<span class='label label-default'>Completed</span>";
            }
            else if (activity.ActivatedDateTime.HasValue)
            {
                _lblStatus.Visible = true;
                _lblStatus.Text = "<span class='label label-success'>Active</span>";
            }
            else
            {
                _lblStatus.Visible = false;
                _lblStatus.Text = string.Empty;
            }

            _cbActivityIsComplete.Checked = activity.CompletedDateTime.HasValue;

            if ( activity.AssignedPersonAliasId.HasValue )
            {
                var person =  new PersonAliasService( rockContext).Queryable()
                    .Where( a => a.Id == activity.AssignedPersonAliasId.Value )
                    .Select( a => a.Person )
                    .FirstOrDefault();
                if ( person != null )
                {
                    _ppAssignedToPerson.SetValue( person );
                    _lAssignedToPerson.Text = person.FullName;
                }
            }

            if ( activity.AssignedGroupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( activity.AssignedGroupId.Value );
                if ( group != null )
                {
                    if ( group.IsSecurityRole )
                    {
                        _ddlAssignedToRole.SetValue( group.Id );
                        _gpAssignedToGroup.SetValue( null );
                        _lAssignedToRole.Text = group.Name;
                    }
                    else
                    {
                        _ddlAssignedToRole.SelectedIndex = -1;
                        _gpAssignedToGroup.SetValue( group );
                        _lAssignedToGroup.Text = group.Name;
                    }
                }
            }

            var sbState = new StringBuilder();
            if ( activity.ActivatedDateTime.HasValue )
            {
                sbState.AppendFormat( "<strong>Activated:</strong> {0} {1} ({2})<br/>",
                    activity.ActivatedDateTime.Value.ToShortDateString(),
                    activity.ActivatedDateTime.Value.ToShortTimeString(),
                    activity.ActivatedDateTime.Value.ToRelativeDateString() );
            }
            if ( activity.CompletedDateTime.HasValue )
            {
                sbState.AppendFormat( "<strong>Completed:</strong> {0} {1} ({2})",
                    activity.CompletedDateTime.Value.ToShortDateString(),
                    activity.CompletedDateTime.Value.ToShortTimeString(),
                    activity.CompletedDateTime.Value.ToRelativeDateString() );
            }
            _lState.Text = sbState.ToString();

            _phAttributes.Controls.Clear();
            if ( CanEdit )
            {
                Rock.Attribute.Helper.AddEditControls( activity, _phAttributes, setValues, ValidationGroup );
            }
            else
            {
                Rock.Attribute.Helper.AddDisplayControls( activity, _phAttributes );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfExpanded = new HiddenField();
            Controls.Add( _hfExpanded );
            _hfExpanded.ID = this.ID + "_hfExpanded";
            _hfExpanded.Value = "False";

            _hfActivityGuid = new HiddenField();
            Controls.Add( _hfActivityGuid );
            _hfActivityGuid.ID = this.ID + "_hfActivityTypeGuid";

            _lblActivityTypeName = new Label();
            Controls.Add( _lblActivityTypeName );
            _lblActivityTypeName.ClientIDMode = ClientIDMode.Static;
            _lblActivityTypeName.ID = this.ID + "_lblActivityTypeName";
            
            _lblActivityTypeDescription = new Label();
            Controls.Add( _lblActivityTypeDescription );
            _lblActivityTypeDescription.ClientIDMode = ClientIDMode.Static;
            _lblActivityTypeDescription.ID = this.ID + "_lblActivityTypeDescription";

            _lblStatus = new Label();
            Controls.Add( _lblStatus );
            _lblStatus.ClientIDMode = ClientIDMode.Static;
            _lblStatus.ID = this.ID + "_lblInactive";
            _lblStatus.CssClass = "pull-right";

            _lbDeleteActivityType = new LinkButton();
            Controls.Add( _lbDeleteActivityType );
            _lbDeleteActivityType.CausesValidation = false;
            _lbDeleteActivityType.ID = this.ID + "_lbDeleteActivityType";
            _lbDeleteActivityType.CssClass = "btn btn-xs btn-danger js-activity-delete";
            _lbDeleteActivityType.Click += lbDeleteActivityType_Click;
            _lbDeleteActivityType.Controls.Add( new LiteralControl { Text = "<i class='fa fa-times'></i>" } );

            _cbActivityIsComplete = new RockCheckBox { Text = "Complete" };
            Controls.Add( _cbActivityIsComplete );
            _cbActivityIsComplete.ID = this.ID + "_cbActivityTypeIsActive";
            _cbActivityIsComplete.Label = "Activity Completed";
            _cbActivityIsComplete.Text = "Yes";

            _ppAssignedToPerson = new PersonPicker();
            _ppAssignedToPerson.ID = this.ID + "_ppAssignedToPerson";
            Controls.Add( _ppAssignedToPerson );
            _ppAssignedToPerson.Label = "Assign to Person";

            _lAssignedToPerson = new RockLiteral();
            _lAssignedToPerson.ID = this.ID + "_lAssignedToPerson";
            Controls.Add( _lAssignedToPerson );
            _lAssignedToPerson.Label = "Assigned to Person";

            _gpAssignedToGroup = new GroupPicker();
            _gpAssignedToGroup.ID = this.ID + "_gpAssignedToGroup";
            Controls.Add( _gpAssignedToGroup );
            _gpAssignedToGroup.Label = "Assign to Group";

            _lAssignedToGroup = new RockLiteral();
            _lAssignedToGroup.ID = this.ID + "_lAssignedToGroup";
            Controls.Add( _lAssignedToGroup );
            _lAssignedToGroup.Label = "Assigned to Group";

            _ddlAssignedToRole = new RockDropDownList();
            Controls.Add( _ddlAssignedToRole );
            _ddlAssignedToRole.ID = this.ID + "_ddlAssignedToRole";
            _ddlAssignedToRole.Label = "Assign to Security Role";

            _lAssignedToRole = new RockLiteral();
            _lAssignedToRole.ID = this.ID + "_lAssignedToRole";
            Controls.Add( _lAssignedToRole );
            _lAssignedToRole.Label = "Assigned to Security Role";

            _lState = new Literal();
            Controls.Add( _lState );
            _lState.ID = this.ID + "_lState";

            _ddlAssignedToRole.Items.Add( new ListItem( string.Empty, "0" ) );
            var roles = new GroupService( new RockContext() ).Queryable().Where( g => g.IsSecurityRole ).OrderBy( t => t.Name );
            if ( roles.Any() )
            {
                foreach ( var role in roles )
                {
                    _ddlAssignedToRole.Items.Add( new ListItem( role.Name, role.Id.ToString() ) );
                }
            }

            _phAttributes = new PlaceHolder();
            Controls.Add( _phAttributes );
            _phAttributes.ID = this.ID + "_phAttributes";
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            string inactiveCss = _cbActivityIsComplete.Checked ? string.Empty : " workflow-activity-inactive";
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget workflow-activity" + inactiveCss );

            writer.AddAttribute( "data-key", _hfActivityGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix clickable" );
            writer.RenderBeginTag( "header" );

            // Hidden Field to track expansion
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
            _hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toggle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute("class", "panel-title");
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            _lblActivityTypeName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();
            _lblActivityTypeDescription.RenderControl( writer );

            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right activity-controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='workflow-activity-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            if ( CanEdit && IsDeleteEnabled )
            {
                _lbDeleteActivityType.Visible = true;
                _lbDeleteActivityType.RenderControl( writer );
            }
            else
            {
                _lbDeleteActivityType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            _lblStatus.RenderControl( writer );

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
            if ( CanEdit )
            {
                _ppAssignedToPerson.ValidationGroup = ValidationGroup;
                _ppAssignedToPerson.RenderControl( writer );
                _gpAssignedToGroup.ValidationGroup = ValidationGroup;
                _gpAssignedToGroup.RenderControl( writer );
                _ddlAssignedToRole.ValidationGroup = ValidationGroup;
                _ddlAssignedToRole.RenderControl( writer );
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( _lAssignedToPerson.Text ) )
                {
                    _lAssignedToPerson.RenderControl( writer );
                }
                if ( !string.IsNullOrWhiteSpace( _lAssignedToGroup.Text ) )
                {
                    _lAssignedToGroup.RenderControl( writer );
                }
                if ( !string.IsNullOrWhiteSpace( _lAssignedToRole.Text ) )
                {
                    _lAssignedToRole.RenderControl( writer );
                }
            }
            writer.RenderEndTag();  // col-md-4

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbActivityIsComplete.Enabled = CanEdit;
            _cbActivityIsComplete.ValidationGroup = ValidationGroup;
            _cbActivityIsComplete.RenderControl( writer );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "&nbsp;" );
            writer.RenderEndTag();  // control-label
            _lState.RenderControl( writer );
            writer.RenderEndTag();  // form-group
            writer.RenderEndTag();  // col-md-6

            writer.RenderEndTag();  // row

            _phAttributes.RenderControl( writer );

            // actions
            writer.RenderBeginTag( "fieldset" );

            writer.RenderBeginTag( "legend" );
            writer.WriteLine( "Actions" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-table table table-condensed table-light" );
            writer.RenderBeginTag( HtmlTextWriterTag.Table );

            writer.RenderBeginTag( HtmlTextWriterTag.Thead );
            writer.RenderBeginTag( HtmlTextWriterTag.Tr );

            writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.Write( "Action" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.Write( "Last Processed" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.Write( "Completed" );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Scope, "col" );
            writer.RenderBeginTag( HtmlTextWriterTag.Th );
            writer.RenderEndTag();

            writer.RenderEndTag();  // tr
            writer.RenderEndTag();  // thead

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-formfield-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

            foreach ( WorkflowActionEditor workflowActionEditor in this.Controls.OfType<WorkflowActionEditor>() )
            {
                workflowActionEditor.ValidationGroup = ValidationGroup;
                workflowActionEditor.RenderControl( writer );
            } 
            
            writer.RenderEndTag();  // tbody

            writer.RenderEndTag();  // table

            // actions fieldset
            writer.RenderEndTag();

            // widget-content div
            writer.RenderEndTag();

            // section tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteActivityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDeleteActivityType_Click( object sender, EventArgs e )
        {
            if ( DeleteActivityTypeClick != null )
            {
                DeleteActivityTypeClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete activity type click].
        /// </summary>
        public event EventHandler DeleteActivityTypeClick;

    }
}