using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace com.kfs.EventRegistration.Advanced
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:ResourceGroup runat=server></{0}:ResourceGroup>" )]
    public class ResourceGroup : CompositeControl, IHasValidationGroup
    {
        private HiddenField _hfGroupGuid;
        private HiddenField _hfGroupId;
        private HiddenField _hfGroupTypeId;
        private Literal _lblGroupName;

        private DataTextBox _tbGroupName;
        private RockCheckBox _cbIsActive;
        private RockDropDownList _ddlGroupRequirement;
        private PlaceHolder _phGroupAttributes;
        private Grid _gLocations;

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
        /// Gets or sets a value indicating whether [enable add locations].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable add locations] or not set; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAddLocations
        {
            get
            {
                bool? b = ViewState["EnableAddLocations"] as bool?;
                return ( b == null ) ? true : b.Value;
            }

            set
            {
                ViewState["EnableAddLocations"] = value;
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
            if ( eventTarget.StartsWith( _gLocations.UniqueID ) )
            {
                List<string> subTargetList = eventTarget.Replace( _gLocations.UniqueID, string.Empty ).Split( new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                EnsureChildControls();

                // make sure it's not a reorder event
                if ( subTargetList.Count != 0 )
                {
                    string lblAddControlId = subTargetList.Where( n => n.EndsWith( "Add" ) ).LastOrDefault();
                    if ( lblAddControlId != null )
                    {
                        AddLocation_Click( this, new EventArgs() );
                    }
                    else
                    {
                        // rowIndex is determined by the numeric suffix of the control id after the Grid, subtract 2 (one for the header, and another to convert from 0 to 1 based index)
                        int rowIndex = subTargetList.First().AsNumeric().AsInteger() - 2;
                        RowEventArgs rowEventArgs = new RowEventArgs( rowIndex, this.Locations.OrderBy( l => l.Order ).ThenBy( l => l.FullNamePath ).ToList()[rowIndex].LocationId );
                        DeleteLocation_Click( this, rowEventArgs );
                    }
                }
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

            /// <summary>
            /// Gets or sets the name in the format ParentLocation1 > ParentLocation0 > Name
            /// </summary>
            /// <value>
            /// The full name path.
            /// </value>
            [DataMember]
            public string FullNamePath { get; set; }

            /// <summary>
            /// Gets or sets the parent location identifier.
            /// </summary>
            /// <value>
            /// The parent location identifier.
            /// </value>
            [DataMember]
            public int? ParentLocationId { get; set; }

            /// <summary>
            /// Gets or sets the order.
            /// </summary>
            /// <value>
            /// The order.
            /// </value>
            [DataMember]
            public int? Order { get; set; }
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
                EnsureChildControls();
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
                EnsureChildControls();
                return _hfGroupTypeId.ValueAsInt();
            }
        }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <value>
        /// The group.
        /// </value>
        public void GetGroupValues( Group group )
        {
            group.Name = _tbGroupName.Text;
            group.IsActive = _cbIsActive.Checked;

            var groupRequirementId = _ddlGroupRequirement.SelectedValueAsId();
            if ( groupRequirementId.HasValue )
            {
                if ( !group.GroupRequirements.Any( r => r.Id.Equals( (int)groupRequirementId ) ) )
                {
                    group.GroupRequirements.Add( new GroupRequirement { GroupRequirementTypeId = (int)groupRequirementId, MustMeetRequirementToAddMember = true } );
                }
            }
            else
            {
                group.GroupRequirements.Clear();
            }

            Rock.Attribute.Helper.GetEditValues( _phGroupAttributes, group );
        }

        /// <summary>
        /// Sets the group.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetGroup( Group value, RockContext rockContext )
        {
            EnsureChildControls();

            //// NOTE:  A Group that was added will have an Id since it hasn't been saved to the database.
            //// So, we'll use Guid to uniquely identify in this Control since that'll work in both Saved and Unsaved cases.
            //// If it is saved, we do need the Id so that Attributes will work

            if ( value != null )
            {
                _hfGroupGuid.Value = value.Guid.ToString();
                _hfGroupId.Value = value.Id.ToString();
                _hfGroupTypeId.Value = value.GroupTypeId.ToString();
                _tbGroupName.Text = value.Name;
                _cbIsActive.Checked = value.IsActive;

                CreateGroupRequirementControls( value, rockContext );
                CreateGroupAttributeControls( value, rockContext );
            }
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
            _lblGroupName.ID = this.ID + "_lblGroupName";

            _tbGroupName = new DataTextBox();
            _tbGroupName.ID = this.ID + "_tbGroupName";
            _tbGroupName.Label = "Group Name";

            _cbIsActive = new RockCheckBox();
            _cbIsActive.ID = this.ID + "_cbIsActive";
            _cbIsActive.Text = "Active";

            _ddlGroupRequirement = new RockDropDownList();
            _ddlGroupRequirement.ID = this.ID + "_ddlGroupRequirement";
            _ddlGroupRequirement.Label = "Group Requirement";
            _ddlGroupRequirement.Help = "The requirement to add to this group. To add more than a single requirement, use the Group Viewer.";

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
            Controls.Add( _cbIsActive );
            Controls.Add( _ddlGroupRequirement );
            Controls.Add( _phGroupAttributes );

            // Locations Grid
            CreateLocationsGrid();
        }

        /// <summary>
        /// Creates the locations grid.
        /// </summary>
        private void CreateLocationsGrid()
        {
            _gLocations = new Grid();

            _gLocations.ID = this.ID + "_gCheckinLabels";

            _gLocations.DisplayType = GridDisplayType.Light;
            _gLocations.ShowActionRow = true;
            _gLocations.RowItemText = "Location";
            _gLocations.Actions.ShowAdd = true;

            //// Handle AddClick manually in OnLoad()
            _gLocations.Actions.AddClick += AddLocation_Click;
            _gLocations.GridReorder += gLocations_Reorder;

            var reorderField = new ReorderField();
            _gLocations.Columns.Add( reorderField );
            _gLocations.ShowHeader = false;
            _gLocations.DataKeyNames = new string[] { "LocationId" };
            _gLocations.Columns.Add( new BoundField { DataField = "FullNamePath", HeaderText = "Name" } );

            DeleteField deleteField = new DeleteField();

            //// handle manually in OnLoad()
            deleteField.Click += DeleteLocation_Click;

            _gLocations.Columns.Add( deleteField );

            Controls.Add( _gLocations );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                _hfGroupGuid.RenderControl( writer );
                _hfGroupId.RenderControl( writer );
                _hfGroupTypeId.RenderControl( writer );

                writer.AddAttribute( HtmlTextWriterAttribute.Style, "margin-top:0;" );
                writer.RenderBeginTag( HtmlTextWriterTag.H3 );
                _lblGroupName.Text = _tbGroupName.Text;
                _lblGroupName.RenderControl( writer );
                writer.RenderEndTag();

                _tbGroupName.RenderControl( writer );
                _cbIsActive.RenderBaseControl( writer );
                _ddlGroupRequirement.RenderControl( writer );
                _phGroupAttributes.RenderControl( writer );

                if ( EnableAddLocations )
                {
                    writer.WriteLine( "<h3>Locations</h3>" );
                    if ( this.Locations != null )
                    {
                        _gLocations.DataSource = this.Locations.OrderBy( l => l.Order ).ThenBy( l => l.FullNamePath ).ToList();
                        _gLocations.DataBind();
                    }

                    _gLocations.RenderControl( writer );
                }
            }
        }

        /// <summary>
        /// Creates the group requirement controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        public void CreateGroupRequirementControls( Group group, RockContext rockContext )
        {
            EnsureChildControls();

            _ddlGroupRequirement.Items.Clear();
            _ddlGroupRequirement.SelectedIndex = -1;

            if ( group != null )
            {
                var selectedRequirement = group.GroupRequirements.Select( r => r.GroupRequirementTypeId ).FirstOrDefault();
                var groupRequirements = new GroupRequirementTypeService( rockContext ).Queryable().ToList();

                _ddlGroupRequirement.Items.Add( Rock.Constants.None.ListItem );
                _ddlGroupRequirement.Items.AddRange(
                    groupRequirements.Select( r => new ListItem
                    {
                        Text = r.Name,
                        Value = r.Id.ToString()
                    } ).ToArray()
                );

                _ddlGroupRequirement.Items.FindByValue( selectedRequirement.ToString() ).Selected = true;
            }
        }

        /// <summary>
        /// Creates the group attribute controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="rockContext">The rock context.</param>
        public void CreateGroupAttributeControls( Group group, RockContext rockContext )
        {
            EnsureChildControls();

            _phGroupAttributes.Controls.Clear();

            if ( group != null )
            {
                if ( group.Attributes == null )
                {
                    group.LoadAttributes( rockContext );
                }
                Rock.Attribute.Helper.AddEditControls( group, _phGroupAttributes, true, this.ValidationGroup );
            }
        }

        /// <summary>
        /// Handles the Reorder event of the gLocations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gLocations_Reorder( object sender, GridReorderEventArgs e )
        {
            if ( ReorderLocationClick != null )
            {
                var eventArg = new ResourceGroupEventArg( GroupGuid, e.DataKey, e.OldIndex, e.NewIndex );
                ReorderLocationClick( this, eventArg );
            }
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
        /// Occurs when [reorder field click].
        /// </summary>
        public event EventHandler<ResourceGroupEventArg> ReorderLocationClick;

        /// <summary>
        /// Occurs when [delete location click].
        /// </summary>
        public event EventHandler<RowEventArgs> DeleteLocationClick;

        /// <summary>
        /// Occurs when [add location click].
        /// </summary>
        public event EventHandler AddLocationClick;
    }

    /// <summary>
    /// An event args class used for reordering a resource group's locations
    /// </summary>
    public class ResourceGroupEventArg : EventArgs
    {
        /// <summary>
        /// Gets or sets the resource group unique identifier.
        /// </summary>
        /// <value>
        /// The resource group's unique identifier.
        /// </value>
        public Guid GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the resource group location id.
        /// </summary>
        /// <value>
        /// The location id.
        /// </value>
        public int LocationId { get; set; }

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
        /// Initializes a new instance of the <see cref="ResourceGroupEventArg"/> class.
        /// </summary>
        /// <param name="groupGuid">The resource group unique identifier.</param>
        public ResourceGroupEventArg( Guid groupGuid )
        {
            GroupGuid = groupGuid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGroupEventArg"/> class.
        /// </summary>
        /// <param name="groupGuid">The resource group unique identifier.</param>
        /// <param name="dataKey">The resource group location Id.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        public ResourceGroupEventArg( Guid groupGuid, string dataKey, int oldIndex, int newIndex )
        {
            GroupGuid = groupGuid;
            LocationId = dataKey.AsInteger();
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }
    }
}
