//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:CheckinGroupEditor runat=server></{0}:CheckinGroupEditor>" )]
    public class CheckinGroupEditor : CompositeControl
    {
        private HiddenField hfGroupGuid;
        private HiddenField hfGroupId;
        private HiddenField hfGroupTypeId;
        private Label lblGroupName;
        private LinkButton lbDeleteGroup;

        private DataTextBox tbGroupName;
        private PlaceHolder phGroupAttributes;

        public bool ForceContentVisible { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// group animation
$('.checkin-group > header').click(function () {
    $(this).siblings('.widget-content').slideToggle();

    $('i.checkin-group-state', this).toggleClass('icon-chevron-down');
    $('i.checkin-group-state', this).toggleClass('icon-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.checkin-group a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.checkin-group a.checkin-group-reorder').click(function (event) {
    event.stopImmediatePropagation();
});
";

            ScriptManager.RegisterStartupScript( hfGroupGuid, hfGroupGuid.GetType(), "CheckinGroupEditorScript", script, true );
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
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group GetGroup()
        {
            EnsureChildControls();
            Group result = new Group();
            
            result.Id = hfGroupTypeId.ValueAsInt();
            result.Guid = new Guid( hfGroupGuid.Value );
            result.GroupTypeId = hfGroupTypeId.ValueAsInt();
            result.Name = tbGroupName.Text;
            result.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phGroupAttributes, result );
            return result;
        }

        /// <summary>
        /// Sets the group.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetGroup( Group value )
        {
            EnsureChildControls();

            //// NOTE:  A Group that was added will have an Id since it hasn't been saved to the database. 
            //// So, we'll use Guid to uniquely identify in this Control since that'll work in both Saved and Unsaved cases.
            //// If it is saved, we do need the Id so that Attributes will work

            hfGroupGuid.Value = value.Guid.ToString();
            hfGroupId.Value = value.Id.ToString();
            hfGroupTypeId.Value = value.GroupTypeId.ToString();
            tbGroupName.Text = value.Name;

            value.LoadAttributes();
            phGroupAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( value, phGroupAttributes, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            hfGroupGuid = new HiddenField();
            hfGroupGuid.ID = this.ID + "_hfGroupGuid";

            hfGroupId = new HiddenField();
            hfGroupId.ID = this.ID + "_hfGroupId";
            
            hfGroupTypeId = new HiddenField();
            hfGroupTypeId.ID = this.ID + "_hfGroupTypeId";

            lblGroupName = new Label();
            lblGroupName.ClientIDMode = ClientIDMode.Static;
            lblGroupName.ID = this.ID + "_lblGroupName";

            lbDeleteGroup = new LinkButton();
            lbDeleteGroup.CausesValidation = false;
            lbDeleteGroup.ID = this.ID + "_lbDeleteGroup";
            lbDeleteGroup.CssClass = "btn btn-mini btn-danger";
            lbDeleteGroup.Click += lbDeleteGroup_Click;

            var iDelete = new HtmlGenericControl( "i" );
            lbDeleteGroup.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );

            tbGroupName = new DataTextBox();
            tbGroupName.ID = this.ID + "_tbGroupName";
            tbGroupName.LabelText = "Check-in Group Name";

            // set label when they exit the edit field
            tbGroupName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblGroupName.ID );
            tbGroupName.SourceTypeName = "Rock.Model.Group, Rock";
            tbGroupName.PropertyName = "Name";

            phGroupAttributes = new PlaceHolder();
            phGroupAttributes.ID = this.ID + "_phGroupAttributes";

            Controls.Add( hfGroupGuid );
            Controls.Add( hfGroupId );
            Controls.Add( hfGroupTypeId );
            Controls.Add( lblGroupName );
            Controls.Add( tbGroupName );
            Controls.Add( phGroupAttributes );
            Controls.Add( lbDeleteGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget checkin-group" );
            writer.AddAttribute( "data-key", hfGroupGuid.Value );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix clickable" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            lblGroupName.Text = tbGroupName.Text;
            lblGroupName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-mini checkin-group-reorder'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-mini'><i class='checkin-group-state icon-chevron-down'></i></a>" );

            if ( IsDeleteEnabled )
            {
                lbDeleteGroup.Visible = true;

                lbDeleteGroup.RenderControl( writer );
            }
            else
            {
                lbDeleteGroup.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );

            Group group = this.GetGroup();

            bool forceContentVisible = !group.IsValid || ForceContentVisible;

            if ( !forceContentVisible )
            {
                // hide details if the name has already been filled in
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // action edit fields
            tbGroupName.RenderControl( writer );

            // action attributes
            phGroupAttributes.RenderControl( writer );

            // widget-content div
            writer.RenderEndTag();

            // article tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteGroup_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupClick != null )
            {
                DeleteGroupClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete group click].
        /// </summary>
        public event EventHandler DeleteGroupClick;
    }
}