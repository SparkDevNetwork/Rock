//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActivityTypeEditor runat=server></{0}:WorkflowActivityTypeEditor>" )]
    public class WorkflowActivityTypeEditor : CompositeControl
    {
        private HiddenField hfActivityTypeGuid;
        private Label lblActivityTypeName;
        private Label lblActivityTypeDescription;
        private Label lblInactive;
        private LinkButton lbDeleteActivityType;

        private RockCheckBox cbActivityTypeIsActive;
        private DataTextBox tbActivityTypeName;
        private DataTextBox tbActivityTypeDescription;
        private RockCheckBox cbActivityTypeIsActivatedWithWorkflow;

        private LinkButton lbAddActionType;

        public bool ForceContentVisible { private get; set; }

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
    $(this).siblings('.widget-content').slideToggle();

    $('i.workflow-activity-state', this).toggleClass('icon-chevron-down');
    $('i.workflow-activity-state', this).toggleClass('icon-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.workflow-activity a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.workflow-activity a.workflow-activity-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

";

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "WorkflowActivityTypeEditorScript", script, true );
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
        /// Gets or sets the type of the workflow activity.
        /// </summary>
        /// <value>
        /// The type of the workflow activity.
        /// </value>
        public WorkflowActivityType GetWorkflowActivityType()
        {
            EnsureChildControls();
            WorkflowActivityType result = new WorkflowActivityType();
            result.Guid = new Guid( hfActivityTypeGuid.Value );
            result.Name = tbActivityTypeName.Text;
            result.Description = tbActivityTypeDescription.Text;
            result.IsActive = cbActivityTypeIsActive.Checked;
            result.IsActivatedWithWorkflow = cbActivityTypeIsActivatedWithWorkflow.Checked;
            result.ActionTypes = new List<WorkflowActionType>();
            int order = 0;
            foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>() )
            {
                WorkflowActionType workflowActionType = workflowActionTypeEditor.WorkflowActionType;
                workflowActionType.Order = order++;
                result.ActionTypes.Add( workflowActionType );
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
            hfActivityTypeGuid.Value = value.Guid.ToString();
            tbActivityTypeName.Text = value.Name;
            tbActivityTypeDescription.Text = value.Description;
            cbActivityTypeIsActive.Checked = value.IsActive ?? false;
            cbActivityTypeIsActivatedWithWorkflow.Checked = value.IsActivatedWithWorkflow;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            hfActivityTypeGuid = new HiddenField();
            hfActivityTypeGuid.ID = this.ID + "_hfActivityTypeGuid";

            lblActivityTypeName = new Label();
            lblActivityTypeName.ClientIDMode = ClientIDMode.Static;
            lblActivityTypeName.ID = this.ID + "_lblActivityTypeName";
            lblActivityTypeDescription = new Label();
            lblActivityTypeDescription.ClientIDMode = ClientIDMode.Static;
            lblActivityTypeDescription.ID = this.ID + "_lblActivityTypeDescription";

            lblInactive = new Label();
            lblInactive.ClientIDMode = ClientIDMode.Static;
            lblInactive.ID = this.ID + "_lblInactive";
            lblInactive.CssClass = "label label-important pull-right";
            lblInactive.Text = "Inactive";

            lbDeleteActivityType = new LinkButton();
            lbDeleteActivityType.CausesValidation = false;
            lbDeleteActivityType.ID = this.ID + "_lbDeleteActivityType";
            lbDeleteActivityType.CssClass = "btn btn-xs btn-danger";
            lbDeleteActivityType.Click += lbDeleteActivityType_Click;
            lbDeleteActivityType.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );

            cbActivityTypeIsActive = new RockCheckBox { Label = "Active" };
            cbActivityTypeIsActive.ID = this.ID + "_cbActivityTypeIsActive";
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

            cbActivityTypeIsActive.InputAttributes.Add( "onclick", string.Format( checkboxScriptFormat, lblInactive.ID, this.ID + "_section" ) );

            tbActivityTypeName = new DataTextBox();
            tbActivityTypeName.ID = this.ID + "_tbActivityTypeName";
            tbActivityTypeName.Label = "Name";

            // set label when they exit the edit field
            tbActivityTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblActivityTypeName.ID );
            tbActivityTypeName.SourceTypeName = "Rock.Model.WorkflowActivityType, Rock";
            tbActivityTypeName.PropertyName = "Name";

            tbActivityTypeDescription = new DataTextBox();
            tbActivityTypeDescription.ID = this.ID + "_tbActivityTypeDescription";
            tbActivityTypeDescription.Label = "Description";
            tbActivityTypeDescription.TextMode = TextBoxMode.MultiLine;
            tbActivityTypeDescription.Rows = 4;

            // set label when they exit the edit field
            tbActivityTypeDescription.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblActivityTypeDescription.ID );
            tbActivityTypeDescription.SourceTypeName = "Rock.Model.WorkflowActivityType, Rock";
            tbActivityTypeDescription.PropertyName = "Description";

            cbActivityTypeIsActivatedWithWorkflow = new RockCheckBox { Label = "Activated with Workflow" };
            cbActivityTypeIsActivatedWithWorkflow.ID = this.ID + "_cbActivityTypeIsActivatedWithWorkflow";

            lbAddActionType = new LinkButton();
            lbAddActionType.ID = this.ID + "_lbAddAction";
            lbAddActionType.CssClass = "btn btn-xs";
            lbAddActionType.Click += lbAddActionType_Click;
            lbAddActionType.CausesValidation = false;
            lbAddActionType.Controls.Add( new LiteralControl { Text = "<i class='icon-plus'></i> Add Action" } );

            Controls.Add( hfActivityTypeGuid );
            Controls.Add( lblActivityTypeName );
            Controls.Add( lblActivityTypeDescription );
            Controls.Add( lblInactive );
            Controls.Add( tbActivityTypeName );
            Controls.Add( tbActivityTypeDescription );
            Controls.Add( cbActivityTypeIsActive );
            Controls.Add( cbActivityTypeIsActivatedWithWorkflow );
            Controls.Add( lbDeleteActivityType );
            Controls.Add( lbAddActionType );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( cbActivityTypeIsActive.Checked )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget widget-dark workflow-activity" );
            }
            else
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget widget-dark workflow-activity workflow-activity-inactive" );
            }

            writer.AddAttribute( "data-key", hfActivityTypeGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix clickable" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toogle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            lblActivityTypeName.Text = tbActivityTypeName.Text;
            lblActivityTypeName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();
            lblActivityTypeDescription.Text = tbActivityTypeDescription.Text;
            lblActivityTypeDescription.RenderControl( writer );

            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-xs workflow-activity-reorder'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-xs'><i class='workflow-activity-state icon-chevron-down'></i></a>" );

            if ( IsDeleteEnabled )
            {
                lbDeleteActivityType.Visible = true;
                lbDeleteActivityType.RenderControl( writer );
            }
            else
            {
                lbDeleteActivityType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            lblInactive.Style[HtmlTextWriterStyle.Display] = cbActivityTypeIsActive.Checked ? "none" : string.Empty;
            lblInactive.RenderControl( writer );

            // header div
            writer.RenderEndTag();

            bool forceContentVisible = !GetWorkflowActivityType().IsValid || ForceContentVisible;

            if ( !forceContentVisible )
            {
                foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>().OrderBy( a => a.WorkflowActionType.Order ) )
                {
                    if ( !workflowActionTypeEditor.WorkflowActionType.IsValid || workflowActionTypeEditor.ForceContentVisible )
                    {
                        forceContentVisible = true;
                        break;
                    }
                }
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );

            if ( !forceContentVisible )
            {
                // hide details if the activity and actions are valid
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // activity edit fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            tbActivityTypeName.RenderControl( writer );
            tbActivityTypeDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbActivityTypeIsActive.RenderControl( writer );
            cbActivityTypeIsActivatedWithWorkflow.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // actions
            writer.RenderBeginTag( "fieldset" );

            writer.RenderBeginTag( "legend" );
            writer.WriteLine( "Actions" );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            lbAddActionType.RenderControl( writer );
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "workflow-action-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in this.Controls.OfType<WorkflowActionTypeEditor>().OrderBy( a => a.WorkflowActionType.Order ) )
            {
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
        /// Occurs when [delete activity type click].
        /// </summary>
        public event EventHandler DeleteActivityTypeClick;

        /// <summary>
        /// Occurs when [add action type click].
        /// </summary>
        public event EventHandler AddActionTypeClick;
    }
}