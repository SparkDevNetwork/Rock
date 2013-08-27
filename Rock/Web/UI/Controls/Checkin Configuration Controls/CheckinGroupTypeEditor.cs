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
        private HiddenField hfGroupTypeId;
        private Label lblGroupTypeName;
        private LinkButton lbDeleteGroupType;

        private NotificationBox nbInheritedFromChangesWarning;
        private LabeledDropDownList ddlGroupTypeInheritFrom;

        private DataTextBox tbGroupTypeName;
        private PlaceHolder phGroupTypeAttributes;
        private Grid gCheckinLabels;

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
        /// Gets or sets the checkin labels.
        /// </summary>
        /// <value>
        /// The checkin labels.
        /// </value>
        public Dictionary<int, string> CheckinLabels
        {
            get
            {
                return ViewState["CheckinLabels"] as Dictionary<int, string>;
            }

            set
            {
                ViewState["CheckinLabels"] = value;
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

            //// NOTE:  A GroupType that was added will have an Id of 0 since it hasn't been saved to the database. 
            //// So, we'll use Guid to uniquely identify in this Control since that'll work in both Saved and Unsaved cases.
            //// If it is saved, we do need the Id so that Attributes will work
            
            result.Id = hfGroupTypeId.ValueAsInt();
            result.Guid = new Guid( hfGroupTypeGuid.Value );

            result.Name = tbGroupTypeName.Text;
            result.InheritedGroupTypeId = ddlGroupTypeInheritFrom.SelectedValueAsId();

            result.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phGroupTypeAttributes, result );

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
            hfGroupTypeId.Value = value.Id.ToString();
            hfGroupTypeGuid.Value = value.Guid.ToString();
            tbGroupTypeName.Text = value.Name;
            ddlGroupTypeInheritFrom.SetValue( value.InheritedGroupTypeId );

            value.LoadAttributes();
            phGroupTypeAttributes.Controls.Clear();

            List<string> exclude = new List<string>();
            Rock.Attribute.Helper.AddEditControls( value, phGroupTypeAttributes, true, new List<string>() { "Labels" } );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            hfGroupTypeGuid = new HiddenField();
            hfGroupTypeGuid.ID = this.ID + "_hfGroupTypeGuid";

            hfGroupTypeId = new HiddenField();
            hfGroupTypeId.ID = this.ID + "_hfGroupTypeId";

            lblGroupTypeName = new Label();
            lblGroupTypeName.ClientIDMode = ClientIDMode.Static;
            lblGroupTypeName.ID = this.ID + "_lblGroupTypeName";

            lbDeleteGroupType = new LinkButton();
            lbDeleteGroupType.CausesValidation = false;
            lbDeleteGroupType.ID = this.ID + "_lbDeleteGroupType";
            lbDeleteGroupType.CssClass = "btn btn-mini btn-danger";
            lbDeleteGroupType.Click += lbDeleteGroupType_Click;
            lbDeleteGroupType.Controls.Add( new LiteralControl { Text = "<i class='icon-remove'></i>" } );
            
            ddlGroupTypeInheritFrom = new LabeledDropDownList();
            ddlGroupTypeInheritFrom.ID = this.ID + "_ddlGroupTypeInheritFrom";
            ddlGroupTypeInheritFrom.LabelText = "Inherit from";
            ddlGroupTypeInheritFrom.AutoPostBack = true;
            ddlGroupTypeInheritFrom.SelectedIndexChanged += ddlGroupTypeInheritFrom_SelectedIndexChanged;

            nbInheritedFromChangesWarning = new NotificationBox();
            nbInheritedFromChangesWarning.ID = this.ID + "_nbInheritedFromChangesWarning";
            nbInheritedFromChangesWarning.NotificationBoxType = NotificationBoxType.Warning;
            nbInheritedFromChangesWarning.Visible = false;

            ddlGroupTypeInheritFrom.Items.Add( Rock.Constants.None.ListItem );
            var qryGroupTypeCheckinFilter = new GroupTypeService().Queryable().Where( a => a.GroupTypePurposeValue.Guid == new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER ) );
            foreach ( var groupType in qryGroupTypeCheckinFilter.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                ddlGroupTypeInheritFrom.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            tbGroupTypeName = new DataTextBox();
            tbGroupTypeName.ID = this.ID + "_tbGroupTypeName";
            tbGroupTypeName.LabelText = "Check-in Area Name";

            // set label when they exit the edit field
            tbGroupTypeName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", lblGroupTypeName.ID );
            tbGroupTypeName.SourceTypeName = "Rock.Model.GroupType, Rock";
            tbGroupTypeName.PropertyName = "Name";

            phGroupTypeAttributes = new PlaceHolder();
            phGroupTypeAttributes.ID = this.ID + "_phGroupTypeAttributes";
            
            // Check-in Labels grid
            CreateCheckinLabelsGrid();
            
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
            Controls.Add( hfGroupTypeId );
            Controls.Add( lblGroupTypeName );
            Controls.Add( ddlGroupTypeInheritFrom );
            Controls.Add( nbInheritedFromChangesWarning );
            Controls.Add( tbGroupTypeName );
            Controls.Add( phGroupTypeAttributes );
            Controls.Add( gCheckinLabels );
            Controls.Add( lbDeleteGroupType );
            Controls.Add( lbAddCheckinGroupType );
            Controls.Add( lbAddCheckinGroup );
        }

        /// <summary>
        /// Creates the checkin labels grid.
        /// </summary>
        private void CreateCheckinLabelsGrid()
        {
            gCheckinLabels = new Grid();
            gCheckinLabels.ID = this.ID + "_gCheckinLabels";
            gCheckinLabels.DisplayType = GridDisplayType.Light;
            gCheckinLabels.ShowActionRow = true;
            gCheckinLabels.RowItemText = "Label";
            gCheckinLabels.Actions.ShowAdd = true;

            gCheckinLabels.Actions.AddClick += AddCheckinLabel_Click;
            gCheckinLabels.Columns.Add( new BoundField { DataField = "Value", HeaderText = "Name" } );
            DeleteField deleteField = new DeleteField();
            deleteField.Click += DeleteCheckinLabel_Click;
            
            
            gCheckinLabels.Columns.Add( deleteField );
            
        }

        /// <summary>
        /// Handles the Click event of the DeleteCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteCheckinLabel_Click( object sender, RowEventArgs e )
        {
            if ( DeleteCheckinLabelClick != null )
            {
                DeleteCheckinLabelClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the AddCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddCheckinLabel_Click( object sender, EventArgs e )
        {
            if ( AddCheckinLabelClick != null )
            {
                AddCheckinLabelClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupTypeInheritFrom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupTypeInheritFrom_SelectedIndexChanged( object sender, EventArgs e )
        {
            //// Changing "Inherit From" can impact the attributes of this GroupType and it's groups, plus any other GroupTypes and their groups that are in the inheritance chain.
            //// For now, just show a message until we can figure out better way to do this
            
            nbInheritedFromChangesWarning.NotificationBoxType = NotificationBoxType.Warning;
            nbInheritedFromChangesWarning.Text = "WARNING - The inherited configuration settings have changed. Please hit the Save button and revisit this page to edit the check-in configuration.";
            nbInheritedFromChangesWarning.Visible = true;
            this.ForceContentVisible = true;
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

            // make two span6 columns: Left Column for Name and Attributes. Right Column for Labels
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            
            // grouptype edit fields
            ddlGroupTypeInheritFrom.RenderControl( writer );
            nbInheritedFromChangesWarning.RenderControl( writer );
            tbGroupTypeName.RenderControl( writer );

            // attributes 
            phGroupTypeAttributes.RenderControl( writer );

            writer.RenderEndTag();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Check-in Labels grid
            writer.WriteLine( "<h3>Check-in Labels</h3>" );
            gCheckinLabels.DataSource = this.CheckinLabels;
            gCheckinLabels.DataBind();
            gCheckinLabels.RenderControl( writer );

            writer.RenderEndTag();
            writer.RenderEndTag();

            // groups

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "checkin-group-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            foreach ( CheckinGroupTypeEditor checkinGroupTypeEditor in this.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                checkinGroupTypeEditor.RenderControl( writer );
            }

            foreach ( CheckinGroupEditor checkinGroupEditor in this.Controls.OfType<CheckinGroupEditor>() )
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

        /// <summary>
        /// Occurs when [delete checkin label click].
        /// </summary>
        public event EventHandler<RowEventArgs> DeleteCheckinLabelClick;

        /// <summary>
        /// Occurs when [add checkin label click].
        /// </summary>
        public event EventHandler AddCheckinLabelClick;
    }
}