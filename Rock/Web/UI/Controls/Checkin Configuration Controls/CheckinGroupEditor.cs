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
        private HiddenField _hfGroupGuid;
        private HiddenField _hfGroupId;
        private HiddenField _hfGroupTypeId;
        private Literal _lblGroupName;
        private LinkButton _lbDeleteGroup;

        private DataTextBox _tbGroupName;
        private PlaceHolder _phGroupAttributes;
        private Grid _gLocations;

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
    $(this).siblings('.panel-body').slideToggle();

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

            ScriptManager.RegisterStartupScript( this.Page, this.Page.GetType(), "CheckinGroupEditorScript", script, true );

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
            if ( eventTarget.StartsWith( _gLocations.UniqueID ) )
            {
                List<string> subTargetList = eventTarget.Replace( _gLocations.UniqueID, string.Empty ).Split( new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                EnsureChildControls();
                string lblAddControlId = subTargetList.Last();
                var lblAdd = _gLocations.Actions.FindControl( lblAddControlId );
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
                return new Guid( _hfGroupGuid.Value );
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
                return _hfGroupTypeId.ValueAsInt();
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

            result.Id = _hfGroupTypeId.ValueAsInt();
            result.Guid = new Guid( _hfGroupGuid.Value );
            result.GroupTypeId = _hfGroupTypeId.ValueAsInt();

            // get the current InheritedGroupTypeId from the Parent Editor just in case it hasn't been saved to the database
            CheckinGroupTypeEditor checkinGroupTypeEditor = this.Parent as CheckinGroupTypeEditor;
            if ( checkinGroupTypeEditor != null )
            {
                result.GroupType = new GroupType();
                result.GroupType.Id = result.GroupTypeId;
                result.GroupType.Guid = checkinGroupTypeEditor.GroupTypeGuid;
                result.GroupType.InheritedGroupTypeId = checkinGroupTypeEditor.InheritedGroupTypeId;
            }

            result.Name = _tbGroupName.Text;
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

            Rock.Attribute.Helper.GetEditValues( _phGroupAttributes, result );
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

            _hfGroupGuid.Value = value.Guid.ToString();
            _hfGroupId.Value = value.Id.ToString();
            _hfGroupTypeId.Value = value.GroupTypeId.ToString();
            _tbGroupName.Text = value.Name;

            CreateGroupAttributeControls( value );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfGroupGuid = new HiddenField();
            _hfGroupGuid.ID = this.ID + "_hfGroupGuid";

            _hfGroupId = new HiddenField();
            _hfGroupId.ID = this.ID + "_hfGroupId";

            _hfGroupTypeId = new HiddenField();
            _hfGroupTypeId.ID = this.ID + "_hfGroupTypeId";

            _lblGroupName = new Literal();
            _lblGroupName.ClientIDMode = ClientIDMode.Static;
            _lblGroupName.ID = this.ID + "_lblGroupName";

            _lbDeleteGroup = new LinkButton();
            _lbDeleteGroup.CausesValidation = false;
            _lbDeleteGroup.ID = this.ID + "_lbDeleteGroup";
            _lbDeleteGroup.CssClass = "btn btn-xs btn-danger";
            _lbDeleteGroup.Click += lbDeleteGroup_Click;
            _lbDeleteGroup.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', '{1}');", "group", "Once saved, you will lose all attendance data." );

            var iDelete = new HtmlGenericControl( "i" );
            _lbDeleteGroup.Controls.Add( iDelete );
            iDelete.AddCssClass( "icon-remove" );

            _tbGroupName = new DataTextBox();
            _tbGroupName.ID = this.ID + "_tbGroupName";
            _tbGroupName.Label = "Check-in Group Name";

            // set label when they exit the edit field
            _tbGroupName.Attributes["onblur"] = string.Format( "javascript: $('#{0}').text($(this).val());", _lblGroupName.ID );
            _tbGroupName.SourceTypeName = "Rock.Model.Group, Rock";
            _tbGroupName.PropertyName = "Name";

            _phGroupAttributes = new PlaceHolder();
            _phGroupAttributes.ID = this.ID + "_phGroupAttributes";

            Controls.Add( _hfGroupGuid );
            Controls.Add( _hfGroupId );
            Controls.Add( _hfGroupTypeId );
            Controls.Add( _lblGroupName );
            Controls.Add( _tbGroupName );
            Controls.Add( _phGroupAttributes );

            // Locations Grid
            CreateLocationsGrid();

            Controls.Add( _lbDeleteGroup );
        }

        /// <summary>
        /// Creates the locations grid.
        /// </summary>
        private void CreateLocationsGrid()
        {
            _gLocations = new Grid();

            // make the ID static so we can handle Postbacks from the Add and Delete actions
            _gLocations.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _gLocations.ID = this.ClientID + "_gCheckinLabels";

            _gLocations.DisplayType = GridDisplayType.Light;
            _gLocations.ShowActionRow = true;
            _gLocations.RowItemText = "Location";
            _gLocations.Actions.ShowAdd = true;

            //// Handle AddClick manually in OnLoad()
            _gLocations.Actions.AddClick += AddLocation_Click;

            _gLocations.DataKeyNames = new string[] { "LocationId" };
            _gLocations.Columns.Add( new BoundField { DataField = "Name", HeaderText = "Name" } );

            DeleteField deleteField = new DeleteField();

            //// handle manually in OnLoad()
            deleteField.Click += DeleteLocation_Click;

            _gLocations.Columns.Add( deleteField );

            Controls.Add( _gLocations );
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
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget checkin-group" );
            writer.AddAttribute( "data-key", _hfGroupGuid.Value );
            writer.RenderBeginTag( "article" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix clickable" );
            writer.RenderBeginTag( "header" );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _lblGroupName.Text = _tbGroupName.Text;
            _lblGroupName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.WriteLine( "<a class='btn btn-xs checkin-group-reorder'><i class='icon-reorder'></i></a>" );
            writer.WriteLine( "<a class='btn btn-xs'><i class='checkin-group-state icon-chevron-down'></i></a>" );

            if ( IsDeleteEnabled )
            {
                _lbDeleteGroup.Visible = true;

                _lbDeleteGroup.RenderControl( writer );
            }
            else
            {
                _lbDeleteGroup.Visible = false;
            }

            // Add/ChevronUpDown/Delete div
            writer.RenderEndTag();

            // header div
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );

            Group group = this.GetGroup();

            bool forceContentVisible = !group.IsValid || ForceContentVisible;

            if ( !forceContentVisible )
            {
                // hide details if the name has already been filled in
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // make two span6 columns: Left Column for Name and Attributes. Right Column for Locations Grid
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // edit fields
            _tbGroupName.RenderControl( writer );

            // attributes
            _phGroupAttributes.RenderControl( writer );

            writer.RenderEndTag();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Locations grid
            writer.WriteLine( "<h3>Locations</h3>" );
            _gLocations.DataSource = this.Locations;
            _gLocations.DataBind();
            _gLocations.RenderControl( writer );

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
        public void CreateGroupAttributeControls( Group group )
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

            _phGroupAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( group, _phGroupAttributes, true );
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