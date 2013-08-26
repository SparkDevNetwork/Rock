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
    [ToolboxData( "<{0}:CheckinGroupTypeEditor runat=server></{0}:CheckinGroupTypeEditor>" )]
    public class CheckinGroupTypeEditor : CompositeControl
    {
        private HiddenField hfGroupTypeGuid;
        private Label lblGroupTypeName;
        private LinkButton lbDeleteGroupType;

        private DataTextBox tbGroupTypeName;

        private LinkButton lbAddCheckinGroup;
        
        private LinkButton lbAddCheckinGroupType;

        public bool ForceContentVisible { private get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// checkin-grouptype animation
$('.checkin-grouptype > header').click(function () {
    $(this).siblings('.widget-content').slideToggle();

    $('i.checkin-grouptype-state', this).toggleClass('icon-chevron-down');
    $('i.checkin-grouptype-state', this).toggleClass('icon-chevron-up');
});

// fix so that the Remove button will fire its event, but not the parent event 
$('.checkin-grouptype a.btn-danger').click(function (event) {
    event.stopImmediatePropagation();
});

// fix so that the Reorder button will fire its event, but not the parent event 
$('.checkin-grouptype a.checkin-grouptype-reorder').click(function (event) {
    event.stopImmediatePropagation();
});

";

            ScriptManager.RegisterStartupScript( hfGroupTypeGuid, hfGroupTypeGuid.GetType(), "CheckinGroupTypeEditorScript", script, true );
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
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public Guid GroupTypeGuid
        {
            get
            {
                return new Guid( hfGroupTypeGuid.Value );
            }
        }

        /// <summary>
        /// Gets the type of the checkin group.
        /// </summary>
        /// <returns></returns>
        public GroupType GetCheckinGroupType()
        {
            EnsureChildControls();
            GroupType result = new GroupType();
            result.Guid = new Guid( hfGroupTypeGuid.Value );
            result.Name = tbGroupTypeName.Text;

            result.ChildGroupTypes = new List<GroupType>();
            int childGroupTypeOrder = 0;
            foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                GroupType childGroupType = checkinGroupTypeEditor.GetCheckinGroupType();
                childGroupType.Order = childGroupTypeOrder++;
                result.ChildGroupTypes.Add( childGroupType );
            }

            result.Groups = new List<Group>();
            int childGroupOrder = 0;
            foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>() )
            {
                Group childGroup = checkinGroupEditor.GetGroup();
                childGroup.Order = childGroupOrder++;
                result.Groups.Add( childGroup );
            }
            
            return result;
        }

        /// <summary>
        /// Sets the type of the group.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetGroupType( GroupType value )
        {
            EnsureChildControls();
            hfGroupTypeGuid.Value = value.Guid.ToString();
            tbGroupTypeName.Text = value.Name;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            hfGroupTypeGuid = new HiddenField();
            hfGroupTypeGuid.ID = this.ID + "_hfGroupTypeGuid";

            lblGroupTypeName = new Label();
            lblGroupTypeName.ClientIDMode = ClientIDMode.Static;
            lblGroupTypeName.ID = this.ID + "_lblGroupTypeName";

            lbDeleteGroupType = new LinkButton();
            lbDeleteGroupType.CausesValidation = false;
            lbDeleteGroupType.ID = this.ID + "_lbDeleteGroupType";
            lbDeleteGroupType.CssClass = "btn btn-mini btn-danger";
            lbDeleteGroupType.Click += lbDeleteGroupType_Click;
            lbDeleteGroupType.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );

            tbGroupTypeName = new DataTextBox();
            tbGroupTypeName.ID = this.ID + "_tbGroupTypeName";
            tbGroupTypeName.LabelText = "Check-in Area Name";

            // set label when they exit the edit field
            tbGroupTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblGroupTypeName.ID );
            tbGroupTypeName.SourceTypeName = "Rock.Model.GroupType, Rock";
            tbGroupTypeName.PropertyName = "Name";

            lbAddCheckinGroupType = new LinkButton();
            lbAddCheckinGroupType.ID = this.ID + "_lblbAddCheckinGroupType";
            lbAddCheckinGroupType.CssClass = "btn btn-mini btn-primary";
            lbAddCheckinGroupType.Click += lbAddCheckinGroupType_Click;
            lbAddCheckinGroupType.CausesValidation = false;
            lbAddCheckinGroupType.Controls.Add( new LiteralControl { Text = "<i class='icon-plus'></i> Add Sub-Area" } );
            
            lbAddCheckinGroup = new LinkButton();
            lbAddCheckinGroup.ID = this.ID + "_lbAddCheckinGroup";
            lbAddCheckinGroup.CssClass = "btn btn-mini btn-primary";
            lbAddCheckinGroup.Click += lbAddGroup_Click;
            lbAddCheckinGroup.CausesValidation = false;
            lbAddCheckinGroup.Controls.Add( new LiteralControl { Text = "<i class='icon-plus'></i> Add Check-in Group" } );
            
            Controls.Add( hfGroupTypeGuid );
            Controls.Add( lblGroupTypeName );
            Controls.Add( tbGroupTypeName );
            Controls.Add( lbDeleteGroupType );
            Controls.Add( lbAddCheckinGroupType );
            Controls.Add( lbAddCheckinGroup );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget widget-dark checkin-grouptype" );
            writer.AddAttribute( "data-key", hfGroupTypeGuid.Value );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ID + "_section" );
            writer.RenderBeginTag( "section" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "clearfix clickable" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-toogle pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            lblGroupTypeName.Text = tbGroupTypeName.Text;
            lblGroupTypeName.RenderControl( writer );

            // H3 tag
            writer.RenderEndTag();

            // Name/Description div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            lbAddCheckinGroupType.RenderControl( writer );
            writer.WriteLine();
            lbAddCheckinGroup.RenderControl( writer );
            writer.WriteLine();

            writer.WriteLine( "<a class='btn btn-mini checkin-grouptype-reorder'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-mini'><i class='checkin-grouptype-state icon-chevron-down'></i></a>" );

            if ( IsDeleteEnabled )
            {
                lbDeleteGroupType.Visible = true;
                lbDeleteGroupType.RenderControl( writer );
            }
            else
            {
                lbDeleteGroupType.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            bool forceContentVisible = !GetCheckinGroupType().IsValid || ForceContentVisible;

            if ( !forceContentVisible )
            {
                foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>() )
                {
                    // TODO
                    
                    /*if ( !workflowActionTypeEditor.WorkflowActionType.IsValid || workflowActionTypeEditor.ForceContentVisible )
                    {
                        forceContentVisible = true;
                        break;
                    }
                     */ 
                }

                foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>() )
                {
                    // TODO
                }
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "widget-content" );

            if ( !forceContentVisible )
            {
                // hide details if the grouptype and groups are valid
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // grouptype edit fields
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            tbGroupTypeName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // groups

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-group-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>())
            {
                checkinGroupTypeEditor.RenderControl( writer );
            }

            foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>())
            {
                checkinGroupEditor.RenderControl( writer );
            }

            // checkin-group-list div
            writer.RenderEndTag();

            // widget-content div
            writer.RenderEndTag();

            // section tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteGroupType_Click( object sender, EventArgs e )
        {
            if ( DeleteGroupTypeClick != null )
            {
                DeleteGroupTypeClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroup_Click( object sender, EventArgs e )
        {
            if ( AddGroupClick != null )
            {
                AddGroupClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCheckinGroupType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCheckinGroupType_Click( object sender, EventArgs e )
        {
            if ( AddGroupTypeClick != null )
            {
                AddGroupTypeClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [delete group type click].
        /// </summary>
        public event EventHandler DeleteGroupTypeClick;

        /// <summary>
        /// Occurs when [add group click].
        /// </summary>
        public event EventHandler AddGroupClick;

        /// <summary>
        /// Occurs when [add group type click].
        /// </summary>
        public event EventHandler AddGroupTypeClick;
    }
}