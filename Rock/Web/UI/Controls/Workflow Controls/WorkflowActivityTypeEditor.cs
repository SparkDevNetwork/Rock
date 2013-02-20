//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
        private LinkButton lbDeleteActivityType;

        private LabeledCheckBox cbActivityTypeIsActive;
        private DataTextBox tbActivityTypeName;
        private DataTextBox tbActivityTypeDescription;
        private LabeledCheckBox cbActivityTypeIsActivatedWithWorkflow;

        private LinkButton lbAddActionType;

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

$('.workflow-activity .icon-remove').click(function (event) {
    event.stopImmediatePropagation();
});

$('.workflow-activity .icon-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

// action animation
$('.workflow-action > header').click(function () {
    $(this).siblings('.widget-content').slideToggle();

    $('i.workflow-action-state', this).toggleClass('icon-chevron-down');
    $('i.workflow-action-state', this).toggleClass('icon-chevron-up');
});

$('.workflow-action .icon-remove').click(function (event) {
    event.stopImmediatePropagation();
});

$('.workflow-action .icon-reorder').click(function (event) {
    event.stopImmediatePropagation();
});
";

            ScriptManager.RegisterStartupScript( hfActivityTypeGuid, hfActivityTypeGuid.GetType(), "WorkflowActivityTypeEditorScript", script, true );
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
        public WorkflowActivityType WorkflowActivityType
        {
            get
            {
                EnsureChildControls();
                WorkflowActivityType result = new WorkflowActivityType();
                result.Guid = new Guid( hfActivityTypeGuid.Value );
                result.Name = tbActivityTypeName.Text;
                result.Description = tbActivityTypeDescription.Text;
                result.IsActive = cbActivityTypeIsActive.Checked;
                result.IsActivatedWithWorkflow = cbActivityTypeIsActivatedWithWorkflow.Checked;
                return result;
            }

            set
            {
                EnsureChildControls();
                hfActivityTypeGuid.Value = value.Guid.ToString();
                lbDeleteActivityType.CommandName = "WorkflowActivityTypeGuid";
                lbDeleteActivityType.CommandArgument = value.Guid.ToString();
                tbActivityTypeName.Text = value.Name;
                tbActivityTypeDescription.Text = value.Description;
                cbActivityTypeIsActive.Checked = value.IsActive ?? false;
                cbActivityTypeIsActivatedWithWorkflow.Checked = value.IsActivatedWithWorkflow;
            }
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

            lbDeleteActivityType = new LinkButton();
            lbDeleteActivityType.CausesValidation = false;
            lbDeleteActivityType.ID = this.ID + "_lbDeleteActivityType";
            lbDeleteActivityType.CssClass = "btn btn-mini btn-danger";
            lbDeleteActivityType.Click += lbDeleteActivityType_Click;
            lbDeleteActivityType.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );

            cbActivityTypeIsActive = new LabeledCheckBox { LabelText = "Active" };
            cbActivityTypeIsActive.ID = this.ID + "_cbActivityTypeIsActive";

            tbActivityTypeName = new DataTextBox();
            tbActivityTypeName.ID = this.ID + "_tbActivityTypeName";
            tbActivityTypeName.LabelText = "Name";
            
            // set label when they exit the edit field
            tbActivityTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblActivityTypeName.ID );
            tbActivityTypeName.SourceTypeName = "Rock.Model.WorkflowActivityType, Rock";
            tbActivityTypeName.PropertyName = "Name";

            tbActivityTypeDescription = new DataTextBox();
            tbActivityTypeDescription.ID = this.ID + "_tbActivityTypeDescription";
            tbActivityTypeDescription.LabelText = "Description";
            
            // set label when they exit the edit field
            tbActivityTypeDescription.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblActivityTypeDescription.ID );
            tbActivityTypeDescription.SourceTypeName = "Rock.Model.WorkflowActivityType, Rock";
            tbActivityTypeDescription.PropertyName = "Description";

            cbActivityTypeIsActivatedWithWorkflow = new LabeledCheckBox { LabelText = "Activated with Workflow" };
            cbActivityTypeIsActivatedWithWorkflow.ID = this.ID + "_cbActivityTypeIsActivatedWithWorkflow";

            lbAddActionType = new LinkButton();
            lbAddActionType.ID = this.ID + "_lbAddAction";
            lbAddActionType.CssClass = "btn btn-mini";
            lbAddActionType.Click += lbAddActionType_Click;
            lbAddActionType.CausesValidation = false;
            lbAddActionType.Controls.Add( new LiteralControl { Text = "<i class='icon-plus'></i> Add Action" } );

            Controls.Add( hfActivityTypeGuid );
            Controls.Add( lblActivityTypeName );
            Controls.Add( lblActivityTypeDescription );
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
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget widget-dark workflow-activity" );
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

            writer.WriteLine( "<a class='btn btn-mini'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-mini'><i class='workflow-activity-state icon-chevron-down'></i></a>" );

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

            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );
            if ( !string.IsNullOrEmpty( WorkflowActivityType.Name ) )
            {
                // hide details if the name has already been filled in
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

            foreach ( Control control in this.Controls )
            {
                if ( control is ActionEditor )
                {
                    control.RenderControl( writer );
                }
            }

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
                DeleteActivityTypeClick( sender, e );
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
                AddActionTypeClick( sender, e );
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