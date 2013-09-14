//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        private Literal lblGroupName;
        private LinkButton lbDeleteGroup;

        private DataTextBox tbGroupName;
        private PlaceHolder phGroupAttributes;
        private Grid gLocations;

        /// <summary>
        /// Gets or sets a value indicating whether [force content visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force content visible]; otherwise, <c>false</c>.
        /// </value>
        public bool ForceContentVisible
        {
            private get
            {
                return _forceContentVisible;
            }

            set
            {
                _forceContentVisible = value;
                if ( _forceContentVisible )
                {
                    CheckinGroupTypeEditor parentGroupTypeEditor = this.Parent as CheckinGroupTypeEditor;
                    while ( parentGroupTypeEditor != null )
                    {
                        parentGroupTypeEditor.ForceContentVisible = true;
                        parentGroupTypeEditor = parentGroupTypeEditor.Parent as CheckinGroupTypeEditor;
                    }
                }
            }
        }
        private bool _forceContentVisible;

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

            if ( !Page.IsPostBack )
            {
                ScriptManager.RegisterStartupScript( hfGroupGuid, hfGroupGuid.GetType(), "CheckinGroupEditorScript", script, true );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                HandleGridEvents();
            }
        }

        /// <summary>
        /// Handles the grid events.
        /// </summary>
        private void HandleGridEvents()
        {
            // manually wireup the grid events since they don't seem to do it automatically 
            string eventTarget = Page.Request.Params["__EVENTTARGET"];
            if ( eventTarget.StartsWith( gLocations.UniqueID ) )
            {
                List<string> subTargetList = eventTarget.Replace( gLocations.UniqueID, string.Empty ).Split( new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                EnsureChildControls();
                string lblAddControlId = subTargetList.Last();
                var lblAdd = gLocations.Actions.FindControl( lblAddControlId );
                if ( lblAdd != null )
                {
                    AddLocation_Click( this, new EventArgs() );
                }
                else
                {
                    // rowIndex is determined by the numeric suffix of the control id after the Grid, subtract 2 (one for the header, and another to convert from 0 to 1 based index)
                    int rowIndex = subTargetList.First().AsNumeric().AsInteger().Value - 2;
                    RowEventArgs rowEventArgs = new RowEventArgs( rowIndex, this.Locations[rowIndex].LocationId );
                    DeleteLocation_Click( this, rowEventArgs );
                }
            }
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
        /// 
        /// </summary>
        [Serializable]
        public class LocationGridItem
        {
            /// <summary>
            /// Gets or sets the location unique identifier.
            /// </summary>
            /// <value>
            /// The location unique identifier.
            /// </value>
            [DataMember]
            public int LocationId { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            [DataMember]
            public string Name { get; set; }
        }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        public List<LocationGridItem> Locations
        {
            get
            {
                return ViewState["Locations"] as List<LocationGridItem>;
            }

            set
            {
                ViewState["Locations"] = value;
            }
        }

        /// <summary>
        /// Gets the group unique identifier.
        /// </summary>
        /// <value>
        /// The group unique identifier.
        /// </value>
        public Guid GroupGuid
        {
            get
            {
                return new Guid( hfGroupGuid.Value );
            }
        }

        /// <summary>
        /// Gets the group type unique identifier.
        /// </summary>
        /// <value>
        /// The group type unique identifier.
        /// </value>
        public int GroupTypeId
        {
            get
            {
                return hfGroupTypeId.ValueAsInt();
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

            // get the current InheritedGroupTypeId from the Parent Editor just in case it hasn't been saved to the database
            CheckinGroupTypeEditor checkinGroupTypeEditor = this.Parent as CheckinGroupTypeEditor;
            if ( checkinGroupTypeEditor != null )
            {
                result.GroupType = new GroupType();
                result.GroupType.Id = result.GroupTypeId;
                result.GroupType.Guid = checkinGroupTypeEditor.GroupTypeGuid;
                result.GroupType.InheritedGroupTypeId = checkinGroupTypeEditor.InheritedGroupTypeId;
            }

            result.Name = tbGroupName.Text;
            result.LoadAttributes();

            // populate groupLocations with whatever is currently in the grid, with just enough info to repopulate it and save it later
            result.GroupLocations = new List<GroupLocation>();
            foreach ( var item in this.Locations )
            {
                var groupLocation = new GroupLocation();
                groupLocation.LocationId = item.LocationId;
                groupLocation.Location = new Location { Id = item.LocationId, Name = item.Name };
                result.GroupLocations.Add( groupLocation );
            }

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

            CreateGroupAttributeControls( value );
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

            lblGroupName = new Literal();
            lblGroupName.ClientIDMode = ClientIDMode.Static;
            lblGroupName.ID = this.ID + "_lblGroupName";

            lbDeleteGroup = new LinkButton();
            lbDeleteGroup.CausesValidation = false;
            lbDeleteGroup.ID = this.ID + "_lbDeleteGroup";
            lbDeleteGroup.CssClass = "btn btn-mini btn-danger";
            lbDeleteGroup.Click += lbDeleteGroup_Click;
            lbDeleteGroup.Attributes["onclick"] = string.Format( "javascript: return Rock.controls.grid.confirmDelete(event, '{0}', '{1}');", "group", "Once saved, you will lose all attendance data." );

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

            // Locations Grid
            CreateLocationsGrid();

            Controls.Add( lbDeleteGroup );
        }

        /// <summary>
        /// Creates the locations grid.
        /// </summary>
        private void CreateLocationsGrid()
        {
            gLocations = new Grid();

            // make the ID static so we can handle Postbacks from the Add and Delete actions
            gLocations.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            gLocations.ID = this.ClientID + "_gCheckinLabels";

            gLocations.DisplayType = GridDisplayType.Light;
            gLocations.ShowActionRow = true;
            gLocations.RowItemText = "Location";
            gLocations.Actions.ShowAdd = true;

            //// Handle AddClick manually in OnLoad()
            gLocations.Actions.AddClick += AddLocation_Click;

            gLocations.DataKeyNames = new string[] { "LocationId" };
            gLocations.Columns.Add( new BoundField { DataField = "Name", HeaderText = "Name" } );

            DeleteField deleteField = new DeleteField();

            //// handle manually in OnLoad()
            deleteField.Click += DeleteLocation_Click;

            gLocations.Columns.Add( deleteField );

            Controls.Add( gLocations );
        }

        /// <summary>
        /// Handles the Click event of the DeleteLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteLocation_Click( object sender, RowEventArgs e )
        {
            if ( DeleteLocationClick != null )
            {
                DeleteLocationClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the AddLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void AddLocation_Click( object sender, EventArgs e )
        {
            if ( AddLocationClick != null )
            {
                AddLocationClick( sender, e );
            }
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

            // make two span6 columns: Left Column for Name and Attributes. Right Column for Locations Grid
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // edit fields
            tbGroupName.RenderControl( writer );

            // attributes
            phGroupAttributes.RenderControl( writer );

            writer.RenderEndTag();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Locations grid
            writer.WriteLine( "<h3>Locations</h3>" );
            gLocations.DataSource = this.Locations;
            gLocations.DataBind();
            gLocations.RenderControl( writer );

            // span6
            writer.RenderEndTag();

            // rowfluid
            writer.RenderEndTag();

            // widget-content div
            writer.RenderEndTag();

            // article tag
            writer.RenderEndTag();
        }

        /// <summary>
        /// Creates the group attribute controls.
        /// </summary>
        public void CreateGroupAttributeControls(Group group)
        {
            // get the current InheritedGroupTypeId from the Parent Editor just in case it hasn't been saved to the database
            CheckinGroupTypeEditor checkinGroupTypeEditor = this.Parent as CheckinGroupTypeEditor;
            if ( checkinGroupTypeEditor != null )
            {
                group.GroupType = new GroupType();
                group.GroupType.Id = group.GroupTypeId;
                group.GroupType.InheritedGroupTypeId = checkinGroupTypeEditor.InheritedGroupTypeId;
            }

            if ( group.Attributes == null )
            {
                group.LoadAttributes();
            }

            phGroupAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, true );
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

        /// <summary>
        /// Occurs when [delete location click].
        /// </summary>
        public event EventHandler<RowEventArgs> DeleteLocationClick;

        /// <summary>
        /// Occurs when [add location click].
        /// </summary>
        public event EventHandler AddLocationClick;
    }
}