//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:WorkflowActionTypeEditor runat=server></{0}:WorkflowActionTypeEditor>" )]
    public class WorkflowActionTypeEditor : CompositeControl
    {
        private HiddenField hfActionTypeGuid;
        private Label lblActionTypeName;
        private LinkButton lbDeleteActionType;

        private DataTextBox tbActionTypeName;
        private LabeledDropDownList ddlEntityType;
        private LabeledCheckBox cbIsActionCompletedOnSuccess;
        private LabeledCheckBox cbIsActivityCompletedOnSuccess;

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
    $(this).siblings('.widget-content').slideToggle();

    $('i.workflow-action-state', this).toggleClass('icon-chevron-down');
    $('i.workflow-action-state', this).toggleClass('icon-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.workflow-action a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.workflow-action a.workflow-action-reorder').click(function (event) {
    event.stopImmediatePropagation();
});
";

            ScriptManager.RegisterStartupScript( hfActionTypeGuid, hfActionTypeGuid.GetType(), "WorkflowActionTypeEditorScript", script, true );
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
        public WorkflowActionType WorkflowActionType
        {
            get
            {
                EnsureChildControls();
                WorkflowActionType result = new WorkflowActionType();
                result.Guid = new Guid( hfActionTypeGuid.Value );
                result.Name = tbActionTypeName.Text;
                result.EntityTypeId = ddlEntityType.SelectedValueAsInt() ?? 0;
                result.IsActionCompletedOnSuccess = cbIsActionCompletedOnSuccess.Checked;
                result.IsActivityCompletedOnSuccess = cbIsActivityCompletedOnSuccess.Checked;
                return result;
            }

            set
            {
                EnsureChildControls();
                hfActionTypeGuid.Value = value.Guid.ToString();
                tbActionTypeName.Text = value.Name;
                ddlEntityType.SetValue( value.EntityTypeId );
                cbIsActionCompletedOnSuccess.Checked = value.IsActionCompletedOnSuccess;
                cbIsActivityCompletedOnSuccess.Checked = value.IsActivityCompletedOnSuccess;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            hfActionTypeGuid = new HiddenField();
            hfActionTypeGuid.ID = this.ID + "_hfActionTypeGuid";

            lblActionTypeName = new Label();
            lblActionTypeName.ClientIDMode = ClientIDMode.Static;
            lblActionTypeName.ID = this.ID + "_lblActionTypeName";

            lbDeleteActionType = new LinkButton();
            lbDeleteActionType.CausesValidation = false;
            lbDeleteActionType.ID = this.ID + "_lbDeleteActionType";
            lbDeleteActionType.CssClass = "btn btn-mini btn-danger";
            lbDeleteActionType.Click += lbDeleteActionType_Click;

            var iDelete = new HtmlGenericControl( "i" );
            lbDeleteActionType.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );

            tbActionTypeName = new DataTextBox();
            tbActionTypeName.ID = this.ID + "_tbActionTypeName";
            tbActionTypeName.LabelText = "Name";

            ddlEntityType = new LabeledDropDownList();
            ddlEntityType.ID = this.ID + "_ddlEntityType";
            ddlEntityType.LabelText = "Action Type";

            foreach ( var item in WorkflowActionContainer.Instance.Components.Values.OrderBy(a => a.Value.EntityType.FriendlyName) )
            {
                var entityType = item.Value.EntityType;
                ddlEntityType.Items.Add( new ListItem( entityType.FriendlyName, entityType.Id.ToString() ));
            }

            // set label when they exit the edit field
            tbActionTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblActionTypeName.ID );
            tbActionTypeName.SourceTypeName = "Rock.Model.WorkflowActionType, Rock";
            tbActionTypeName.PropertyName = "Name";

            cbIsActionCompletedOnSuccess = new LabeledCheckBox { LabelText = "Action is Completed on Success" };
            cbIsActionCompletedOnSuccess.ID = this.ID + "_cbIsActionCompletedOnSuccess";

            cbIsActivityCompletedOnSuccess = new LabeledCheckBox { LabelText = "Activity is Completed on Success" };
            cbIsActivityCompletedOnSuccess.ID = this.ID + "_cbIsActivityCompletedOnSuccess";

            Controls.Add( hfActionTypeGuid );
            Controls.Add( lblActionTypeName );
            Controls.Add( tbActionTypeName );
            Controls.Add( cbIsActionCompletedOnSuccess );
            Controls.Add( cbIsActivityCompletedOnSuccess );
            Controls.Add( lbDeleteActionType );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget workflow-action" );
            writer.AddAttribute( "data-key", hfActionTypeGuid.Value );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix clickable" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            lblActionTypeName.Text = tbActionTypeName.Text;
            lblActionTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-mini workflow-action-reorder'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-mini'><i class='workflow-action-state icon-chevron-down'></i></a>" );

            if ( IsDeleteEnabled )
            {
                lbDeleteActionType.Visible = true;

                lbDeleteActionType.RenderControl( writer );
            }
            else
            {
                lbDeleteActionType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );
            if ( !string.IsNullOrEmpty( WorkflowActionType.Name ) )
            {
                // hide details if the name has already been filled in
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // action edit fields
            tbActionTypeName.RenderControl( writer );
            ddlEntityType.RenderControl( writer );
            cbIsActionCompletedOnSuccess.RenderControl( writer );
            cbIsActivityCompletedOnSuccess.RenderControl( writer );

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
    }
}