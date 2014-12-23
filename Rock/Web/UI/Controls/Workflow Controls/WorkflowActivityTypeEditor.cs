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
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by WorkflowTypeDetail block to edit a workflow activity type
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActivityTypeEditor runat=server></{0}:WorkflowActivityTypeEditor>" )]
    public class WorkflowActivityTypeEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenFieldWithClass _hfExpanded;
        private HiddenField _hfActivityTypeGuid;
        private Label _lblActivityTypeName;
        private Label _lblActivityTypeDescription;
        private Label _lblInactive;
        private LinkButton _lbDeleteActivityType;

        private RockCheckBox _cbActivityTypeIsActive;
        private RockTextBox _tbActivityTypeName;
        private RockTextBox _tbActivityTypeDescription;
        private RockCheckBox _cbActivityTypeIsActivatedWithWorkflow;

        private SecurityButton _sbSecurity;
        private LinkButton _lbAddActionType;

        private PanelWidget _pwAttributes;
        private Grid _gAttributes;

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
                return _hfExpanded.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [attributes expanded].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attributes expanded]; otherwise, <c>false</c>.
        /// </value>
        public bool AttributesExpanded
        {
            get
            {
                EnsureChildControls();
                return _pwAttributes.Expanded;
            }

            set
            {
                EnsureChildControls();
                _pwAttributes.Expanded = value;
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
        public Guid ActivityTypeGuid
        {
            get 
            {
                EnsureChildControls();
                return _hfActivityTypeGuid.Value.AsGuid();
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
                return _tbActivityTypeName.Text;
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
    return Rock.dialogs.confirmDelete(event, 'Activity Type', 'This will also delete all the activities of this type from any existing persisted workflows!');
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.workflow-activity a.workflow-activity-reorder').click(function (event) {
    event.stopImmediatePropagation();
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

            ScriptManager.RegisterStartupScript( this, this.GetType(), "WorkflowActivityTypeEditorScript", script, true );
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
        /// Gets the expanded actions.
        /// </summary>
        /// <value>
        /// The expanded actions.
        /// </value>
        public List<Guid> ExpandedActions
        {
            get
            {
                var result = new List<Guid>();
                foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>() )
                {
                    if (workflowActionTypeEditor.Expanded)
                    {
                        result.Add( workflowActionTypeEditor.ActionTypeGuid );
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the type of the workflow activity.
        /// </summary>
        /// <value>
        /// The type of the workflow activity.
        /// </value>
        public WorkflowActivityType GetWorkflowActivityType( bool expandInvalid )
        {
            EnsureChildControls();
            WorkflowActivityType result = new WorkflowActivityType();
            result.Id = _sbSecurity.EntityId;
            result.Guid = new Guid( _hfActivityTypeGuid.Value );
            result.Name = _tbActivityTypeName.Text;
            result.Description = _tbActivityTypeDescription.Text;
            result.IsActive = _cbActivityTypeIsActive.Checked;
            result.IsActivatedWithWorkflow = _cbActivityTypeIsActivatedWithWorkflow.Checked;
            result.ActionTypes = new List<WorkflowActionType>();
            int order = 0;
            foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>() )
            {
                bool wasExpanded = workflowActionTypeEditor.Expanded;
                WorkflowActionType workflowActionType = workflowActionTypeEditor.GetWorkflowActionType( expandInvalid );
                workflowActionType.Order = order++;
                result.ActionTypes.Add( workflowActionType );

                // If action was expanded because it's invalid, expand the activity also
                if ( expandInvalid && !wasExpanded && workflowActionTypeEditor.Expanded )
                {
                    Expanded = true;
                }
            }

            if (expandInvalid && !Expanded && !result.IsValid)
            {
                Expanded = true;
            }

            return result;
        }

        /// <summary>
        /// Sets the type of the workflow activity.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetWorkflowActivityType( WorkflowActivityType value )
        {
            EnsureChildControls();
            _hfActivityTypeGuid.Value = value.Guid.ToString();
            _sbSecurity.EntityId = value.Id;
            _tbActivityTypeName.Text = value.Name;
            _tbActivityTypeDescription.Text = value.Description;
            _cbActivityTypeIsActive.Checked = value.IsActive ?? false;
            _cbActivityTypeIsActivatedWithWorkflow.Checked = value.IsActivatedWithWorkflow;
        }

        /// <summary>
        /// Binds the attributes grid.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        public void BindAttributesGrid( List<Rock.Model.Attribute> attributes )
        {
            if ( attributes.Any() )
            {
                _pwAttributes.Title = string.Format( "Attributes ({0})", attributes.Count.ToString( "N0" ) );
            }
            else
            {
                _pwAttributes.Title = "Attributes";
            } 
            
            _gAttributes.DataSource = attributes
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Guid,
                    a.Name,
                    a.Description,
                    FieldType = FieldTypeCache.GetName( a.FieldTypeId ),
                    a.IsRequired
                } )
                .ToList();
            _gAttributes.DataBind();
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

            _hfActivityTypeGuid = new HiddenField();
            Controls.Add( _hfActivityTypeGuid );
            _hfActivityTypeGuid.ID = this.ID + "_hfActivityTypeGuid";

            _lblActivityTypeName = new Label();
            Controls.Add( _lblActivityTypeName );
            _lblActivityTypeName.ClientIDMode = ClientIDMode.Static;
            _lblActivityTypeName.ID = this.ID + "_lblActivityTypeName";
            
            _lblActivityTypeDescription = new Label();
            Controls.Add( _lblActivityTypeDescription );
            _lblActivityTypeDescription.ClientIDMode = ClientIDMode.Static;
            _lblActivityTypeDescription.ID = this.ID + "_lblActivityTypeDescription";

            _lblInactive = new Label();
            Controls.Add( _lblInactive );
            _lblInactive.ClientIDMode = ClientIDMode.Static;
            _lblInactive.ID = this.ID + "_lblInactive";
            _lblInactive.CssClass = "pull-right";
            _lblInactive.Text = "<span class='label label-danger'>Inactive</span>";

            _lbDeleteActivityType = new LinkButton();
            Controls.Add( _lbDeleteActivityType );
            _lbDeleteActivityType.CausesValidation = false;
            _lbDeleteActivityType.ID = this.ID + "_lbDeleteActivityType";
            _lbDeleteActivityType.CssClass = "btn btn-xs btn-danger js-activity-delete";
            _lbDeleteActivityType.Click += lbDeleteActivityType_Click;
            _lbDeleteActivityType.Controls.Add( new LiteralControl { Text = "<i class='fa fa-times'></i>" } );

            _sbSecurity = new SecurityButton();
            Controls.Add( _sbSecurity );
            _sbSecurity.ID = this.ID + "_sbSecurity";
            _sbSecurity.Attributes["class"] = "btn btn-security btn-xs security pull-right";
            _sbSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.WorkflowActivityType ) ).Id;

            _cbActivityTypeIsActive = new RockCheckBox { Text = "Active" };
            Controls.Add( _cbActivityTypeIsActive );
            _cbActivityTypeIsActive.ID = this.ID + "_cbActivityTypeIsActive";
            string checkboxScriptFormat = @"
javascript: 
    if ($(this).is(':checked')) {{ 
        $('#{0}').hide(); 
        $('#{1}').removeClass('workflow-activity-inactive'); 
    }} 
    else {{ 
        $('#{0}').show(); 
        $('#{1}').addClass('workflow-activity-inactive'); 
    }}
";

            _cbActivityTypeIsActive.InputAttributes.Add( "onclick", string.Format( checkboxScriptFormat, _lblInactive.ID, this.ID + "_section" ) );

            _tbActivityTypeName = new RockTextBox();
            Controls.Add( _tbActivityTypeName );
            _tbActivityTypeName.ID = this.ID + "_tbActivityTypeName";
            _tbActivityTypeName.Label = "Name";
            _tbActivityTypeName.Required = true;
            _tbActivityTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblActivityTypeName.ID );

            _tbActivityTypeDescription = new RockTextBox();
            Controls.Add( _tbActivityTypeDescription );
            _tbActivityTypeDescription.ID = this.ID + "_tbActivityTypeDescription";
            _tbActivityTypeDescription.Label = "Description";
            _tbActivityTypeDescription.TextMode = TextBoxMode.MultiLine;
            _tbActivityTypeDescription.Rows = 2;
            _tbActivityTypeDescription.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblActivityTypeDescription.ID );

            _cbActivityTypeIsActivatedWithWorkflow = new RockCheckBox { Text = "Activated with Workflow" };
            Controls.Add( _cbActivityTypeIsActivatedWithWorkflow );
            _cbActivityTypeIsActivatedWithWorkflow.ID = this.ID + "_cbActivityTypeIsActivatedWithWorkflow";
            checkboxScriptFormat = @"
javascript: 
    if ($(this).is(':checked')) {{ 
        $('#{0}').addClass('activated-with-workflow'); 
    }} 
    else {{ 
        $('#{0}').removeClass('activated-with-workflow'); 
    }}
";
            _cbActivityTypeIsActivatedWithWorkflow.InputAttributes.Add( "onclick", string.Format( checkboxScriptFormat, this.ID + "_section" ) );


            _lbAddActionType = new LinkButton();
            Controls.Add( _lbAddActionType );
            _lbAddActionType.ID = this.ID + "_lbAddAction";
            _lbAddActionType.CssClass = "btn btn-xs btn-action";
            _lbAddActionType.Click += lbAddActionType_Click;
            _lbAddActionType.CausesValidation = false;
            _lbAddActionType.Controls.Add( new LiteralControl { Text = "<i class='fa fa-plus'></i> Add Action" } );

            _pwAttributes = new PanelWidget();
            Controls.Add( _pwAttributes );
            _pwAttributes.ID = this.ID + "_pwAttributes";
            _pwAttributes.Title = "Activity Attributes";
            _pwAttributes.CssClass = "attribute-panel";

            _gAttributes = new Grid();
            _pwAttributes.Controls.Add( _gAttributes );
            _gAttributes.ID = this.ID + "_gAttributes";
            _gAttributes.AllowPaging = false;
            _gAttributes.DisplayType = GridDisplayType.Light;
            _gAttributes.RowItemText = "Activity Attribute";
            _gAttributes.AddCssClass( "attribute-grid" );
            _gAttributes.DataKeyNames = new string[] { "Guid" };
            _gAttributes.Actions.ShowAdd = true;
            _gAttributes.Actions.AddClick += gAttributes_Add;
            _gAttributes.GridRebind += gAttributes_Rebind;
            _gAttributes.GridReorder += gAttributes_Reorder;

            var reorderField = new ReorderField();
            _gAttributes.Columns.Add( reorderField );

            var nameField = new BoundField();
            nameField.DataField = "Name";
            nameField.HeaderText = "Attribute";
            _gAttributes.Columns.Add( nameField );

            var descField = new BoundField();
            descField.DataField = "Description";
            descField.HeaderText = "Description";
            _gAttributes.Columns.Add( descField );

            var fieldTypeField = new BoundField();
            fieldTypeField.DataField = "FieldType";
            fieldTypeField.HeaderText = "Field Type";
            _gAttributes.Columns.Add( fieldTypeField );

            var reqField = new BoolField();
            reqField.DataField = "IsRequired";
            reqField.HeaderText = "Required";
            _gAttributes.Columns.Add( reqField );

            var editField = new EditField();
            editField.Click += gAttributes_Edit;
            _gAttributes.Columns.Add( editField );

            var delField = new DeleteField();
            delField.Click += gAttributes_Delete;
            _gAttributes.Columns.Add( delField );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( !Expanded )
            {
                foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>() )
                {
                    if ( workflowActionTypeEditor.Expanded )
                    {
                        Expanded = true;
                        break;
                    }
                }
            }

            string inactiveCss = _cbActivityTypeIsActive.Checked ? string.Empty : " workflow-activity-inactive";
            string activatedWithWorkflowCss = _cbActivityTypeIsActivatedWithWorkflow.Checked ? " activated-with-workflow" : string.Empty;
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget workflow-activity" + inactiveCss + activatedWithWorkflowCss );

            writer.AddAttribute( "data-key", _hfActivityTypeGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix clickable" );
            writer.RenderBeginTag( "header" );

            // Hidden Field to track expansion
            _hfExpanded.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toggle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute("class", "panel-title");
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            _lblActivityTypeName.Text = _tbActivityTypeName.Text;
            _lblActivityTypeName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();
            _lblActivityTypeDescription.Text = _tbActivityTypeDescription.Text;
            _lblActivityTypeDescription.RenderControl( writer );

            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-xs btn-link workflow-activity-reorder'><i class='fa fa-bars'></i></a>" );
            writer.WriteLine( string.Format( "<a class='btn btn-xs btn-link'><i class='workflow-activity-state fa {0}'></i></a>",
                Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" ) );

            if ( IsDeleteEnabled )
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

            _lblInactive.Style[HtmlTextWriterStyle.Display] = _cbActivityTypeIsActive.Checked ? "none" : string.Empty;
            _lblInactive.RenderControl( writer );

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
            _tbActivityTypeName.ValidationGroup = ValidationGroup;
            _tbActivityTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbActivityTypeIsActive.ValidationGroup = ValidationGroup;
            _cbActivityTypeIsActive.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( _sbSecurity.EntityId > 0 )
            {
                _sbSecurity.Title = _tbActivityTypeName.Text;
                _sbSecurity.RenderControl( writer );
            }
            
            _cbActivityTypeIsActivatedWithWorkflow.ValidationGroup = ValidationGroup;
            _cbActivityTypeIsActivatedWithWorkflow.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            _tbActivityTypeDescription.ValidationGroup = ValidationGroup;
            _tbActivityTypeDescription.RenderControl( writer );

            _pwAttributes.RenderControl( writer );

            // actions
            writer.RenderBeginTag( "fieldset" );

            writer.RenderBeginTag( "legend" );
            writer.WriteLine( "Actions" );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            _lbAddActionType.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-action-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>() )
            {
                workflowActionTypeEditor.ValidationGroup = ValidationGroup;
                workflowActionTypeEditor.RenderControl( writer );
            }

            // workflow-action-list div
            writer.RenderEndTag();

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
        /// Handles the Click event of the lbAddActionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddActionType_Click( object sender, EventArgs e )
        {
            if ( AddActionTypeClick != null )
            {
                AddActionTypeClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Rebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttributes_Rebind( object sender, EventArgs e )
        {
            if ( RebindAttributeClick != null )
            {
                var eventArg = new WorkflowActivityTypeAttributeEventArg( ActivityTypeGuid );
                RebindAttributeClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            if ( AddAttributeClick != null )
            {
                var eventArg = new WorkflowActivityTypeAttributeEventArg( ActivityTypeGuid, Guid.Empty );
                AddAttributeClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            if ( EditAttributeClick != null )
            {
                var eventArg = new WorkflowActivityTypeAttributeEventArg( ActivityTypeGuid, (Guid)e.RowKeyValue );
                EditAttributeClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Reorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_Reorder( object sender, GridReorderEventArgs e )
        {
            if ( ReorderAttributeClick != null )
            {
                var eventArg = new WorkflowActivityTypeAttributeEventArg( ActivityTypeGuid, e.OldIndex, e.NewIndex );
                ReorderAttributeClick( this, eventArg );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            if ( DeleteAttributeClick != null )
            {
                var eventArg = new WorkflowActivityTypeAttributeEventArg( ActivityTypeGuid, (Guid)e.RowKeyValue );
                DeleteAttributeClick( this, eventArg );
            }
        }


        /// <summary>
        /// Occurs when [delete activity type click].
        /// </summary>
        public event EventHandler DeleteActivityTypeClick;

        /// <summary>
        /// Occurs when [add action type click].
        /// </summary>
        public event EventHandler AddActionTypeClick;

        /// <summary>
        /// Occurs when [add attribute click].
        /// </summary>
        public event EventHandler<WorkflowActivityTypeAttributeEventArg> RebindAttributeClick;

        /// <summary>
        /// Occurs when [add attribute click].
        /// </summary>
        public event EventHandler<WorkflowActivityTypeAttributeEventArg> AddAttributeClick;

        /// <summary>
        /// Occurs when [edit attribute click].
        /// </summary>
        public event EventHandler<WorkflowActivityTypeAttributeEventArg> EditAttributeClick;

        /// <summary>
        /// Occurs when [edit attribute click].
        /// </summary>
        public event EventHandler<WorkflowActivityTypeAttributeEventArg> ReorderAttributeClick;

        /// <summary>
        /// Occurs when [delete attribute click].
        /// </summary>
        public event EventHandler<WorkflowActivityTypeAttributeEventArg> DeleteAttributeClick;

    }

    /// <summary>
    /// 
    /// </summary>
    public class WorkflowActivityTypeAttributeEventArg : EventArgs
    {
        /// <summary>
        /// Gets or sets the activity type unique identifier.
        /// </summary>
        /// <value>
        /// The activity type unique identifier.
        /// </value>
        public Guid ActivityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>
        /// The attribute unique identifier.
        /// </value>
        public Guid AttributeGuid { get; set; }

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
        /// Initializes a new instance of the <see cref="WorkflowActivityTypeAttributeEventArg"/> class.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        public WorkflowActivityTypeAttributeEventArg( Guid activityTypeGuid )
        {
            ActivityTypeGuid = activityTypeGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityTypeAttributeEventArg"/> class.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        public WorkflowActivityTypeAttributeEventArg( Guid activityTypeGuid, Guid attributeGuid )
        {
            ActivityTypeGuid = activityTypeGuid;
            AttributeGuid = attributeGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActivityTypeAttributeEventArg"/> class.
        /// </summary>
        /// <param name="activityTypeGuid">The activity type unique identifier.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public WorkflowActivityTypeAttributeEventArg( Guid activityTypeGuid, int oldIndex, int newIndex )
        {
            ActivityTypeGuid = activityTypeGuid;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }


    }
}