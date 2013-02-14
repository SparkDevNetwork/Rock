//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:ActivityEditor runat=server></{0}:ActivityEditor>" )]
    public class ActivityEditor : CompositeControl
    {
        private Label lblActivityName;
        private Label lblActivityDescription;
        private LinkButton lbDeleteActivity;

        private LabeledCheckBox cbActivityIsActive;
        private DataTextBox tbActivityName;
        private DataTextBox tbActivityDescription;
        private LabeledCheckBox cbActivityIsActivatedWithWorkflow;

        private LinkButton lbAddAction;

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
        /// Gets or sets the name of the activity.
        /// </summary>
        /// <value>
        /// The name of the activity.
        /// </value>
        public string ActivityName
        {
            get
            {
                EnsureChildControls();
                return tbActivityName.Text;
            }

            set
            {
                EnsureChildControls();
                lblActivityName.Text = value;
                tbActivityName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the activity description.
        /// </summary>
        /// <value>
        /// The activity description.
        /// </value>
        public string ActivityDescription
        {
            get
            {
                EnsureChildControls();
                return tbActivityDescription.Text;
            }

            set
            {
                EnsureChildControls();
                lblActivityDescription.Text = value;
                tbActivityDescription.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [activity is active].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [activity is active]; otherwise, <c>false</c>.
        /// </value>
        public bool ActivityIsActive
        {
            get
            {
                EnsureChildControls();
                return cbActivityIsActive.Checked;
            }

            set
            {
                EnsureChildControls();
                cbActivityIsActive.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [activity is activated with workflow].
        /// </summary>
        /// <value>
        /// <c>true</c> if [activity is activated with workflow]; otherwise, <c>false</c>.
        /// </value>
        public bool ActivityIsActivatedWithWorkflow
        {
            get
            {
                EnsureChildControls();
                return cbActivityIsActivatedWithWorkflow.Checked;
            }
            
            set
            {
                EnsureChildControls();
                cbActivityIsActivatedWithWorkflow.Checked = value;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            lblActivityName = new Label();
            lblActivityDescription = new Label();

            lbDeleteActivity = new LinkButton();
            lbDeleteActivity.ID = this.ID + "_lbDeleteActivity";
            lbDeleteActivity.CssClass = "btn btn-mini btn-danger";
            lbDeleteActivity.Click += lbDeleteActivity_Click;
            lbDeleteActivity.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );

            cbActivityIsActive = new LabeledCheckBox { LabelText = "Active" };
            cbActivityIsActive.ID = this.ID + "_cbActivityIsActive";

            tbActivityName = new DataTextBox { LabelText = "Name" };
            tbActivityDescription = new DataTextBox { LabelText = "Description" };
            
            cbActivityIsActivatedWithWorkflow = new LabeledCheckBox { LabelText = "Activated with Workflow" };
            cbActivityIsActivatedWithWorkflow.ID = this.ID + "_cbActivityIsActivatedWithWorkflow";

            lbAddAction = new LinkButton();
            lbAddAction.ID = this.ID + "_lbAddAction";
            lbAddAction.CssClass = "btn btn-mini";
            lbAddAction.Click += lbAddAction_Click;
            lbAddAction.Controls.Add( new LiteralControl { Text = "<i class='icon-plus'></i> Add Action" } );
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
            lblActivityName.RenderControl( writer );
            
            // H3 tag
            writer.RenderEndTag();
            lblActivityDescription.RenderControl( writer );
            
            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine("<a class='btn btn-mini'><i class='icon-reorder'></i></a>");
            writer.WriteLine("<a class='btn btn-mini'><i class='workflow-activity-state icon-chevron-down'></i></a>");

            if ( IsDeleteEnabled )
            {
                lbDeleteActivity.Visible = true;
                lbDeleteActivity.RenderControl( writer );
            }
            else
            {
                lbDeleteActivity.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();
            
            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );
            writer.AddStyleAttribute( "display", "none" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // activity edit fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            tbActivityName.RenderControl( writer );
            tbActivityDescription.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cbActivityIsActive.RenderControl( writer );
            cbActivityIsActivatedWithWorkflow.RenderControl( writer );
            writer.RenderEndTag();
            
            writer.RenderEndTag();

            // actions
            writer.RenderBeginTag( "fieldset" );

            writer.RenderBeginTag( "legend" );
            writer.WriteLine( "Actions" );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            lbAddAction.RenderControl( writer );
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
        /// Handles the Click event of the lbDeleteActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDeleteActivity_Click( object sender, EventArgs e )
        {
            if ( DeleteActivityClick != null )
            {
                DeleteActivityClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void lbAddAction_Click( object sender, EventArgs e )
        {
            if ( AddActionClick != null )
            {
                AddActionClick( sender, e );
            }
        }

        /// <summary>
        /// Occurs when [delete activity click].
        /// </summary>
        public event EventHandler DeleteActivityClick;

        /// <summary>
        /// Occurs when [add action click].
        /// </summary>
        public event EventHandler AddActionClick;
    }
}