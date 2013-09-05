using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CheckinConfiguration : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "groupTypeId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "groupTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region ViewState and Dynamic Controls

        /// <summary>
        /// ViewState of CheckinLabels per GroupType
        /// </summary>
        /// <value>
        /// The state of the group type checkin label attributes.
        /// </value>
        public Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>> GroupTypeCheckinLabelAttributesState
        {
            get
            {
                Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>> result = ViewState["GroupTypeCheckinLabelAttributes"] as Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>>;
                if ( result == null )
                {
                    result = new Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>>();
                }

                return result;
            }

            set
            {
                ViewState["GroupTypeCheckinLabelAttributes"] = value;
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            GroupTypeCheckinLabelAttributesState = new Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>>();

            var groupTypeList = new List<GroupType>();
            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>().ToList() )
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType();
                GroupTypeCheckinLabelAttributesState.Add( checkinGroupTypeEditor.GroupTypeGuid, checkinGroupTypeEditor.CheckinLabels );
                groupTypeList.Add( groupType );
            }

            ViewStateList<GroupType> groupTypeViewStateList = new ViewStateList<GroupType>();
            groupTypeViewStateList.AddAll( groupTypeList );

            ViewState["CheckinGroupTypes"] = groupTypeViewStateList;
            return base.SaveViewState();
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            BuildGroupTypeEditorControlsFromViewState();
        }

        /// <summary>
        /// Builds the state of the group type editor controls from view.
        /// </summary>
        /// <param name="activeGroupTypeGuid">The active group type GUID.</param>
        private void BuildGroupTypeEditorControlsFromViewState( Guid? activeGroupTypeGuid = null )
        {
            phCheckinGroupTypes.Controls.Clear();

            ViewStateList<GroupType> groupTypeViewStateList = ViewState["CheckinGroupTypes"] as ViewStateList<GroupType>;

            foreach ( var groupType in groupTypeViewStateList )
            {
                CreateGroupTypeEditorControls( groupType, phCheckinGroupTypes, groupType.Guid.Equals( activeGroupTypeGuid ?? Guid.Empty ) );
            }
        }

        /// <summary>
        /// Creates the group type editor controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="forceContentVisible">if set to <c>true</c> [force content visible].</param>
        private void CreateGroupTypeEditorControls( GroupType groupType, Control parentControl, bool forceContentVisible = false )
        {
            CheckinGroupTypeEditor groupTypeEditor = new CheckinGroupTypeEditor();
            groupTypeEditor.ID = "GroupTypeEditor_" + groupType.Guid.ToString( "N" );
            groupTypeEditor.SetGroupType( groupType );
            groupTypeEditor.AddGroupClick += groupTypeEditor_AddGroupClick;
            groupTypeEditor.AddGroupTypeClick += groupTypeEditor_AddGroupTypeClick;
            groupTypeEditor.DeleteCheckinLabelClick += groupTypeEditor_DeleteCheckinLabelClick;
            groupTypeEditor.AddCheckinLabelClick += groupTypeEditor_AddCheckinLabelClick;
            groupTypeEditor.DeleteGroupTypeClick += groupTypeEditor_DeleteGroupTypeClick;
            groupTypeEditor.CheckinLabels = null;

            if ( GroupTypeCheckinLabelAttributesState.ContainsKey( groupType.Guid ) )
            {
                groupTypeEditor.CheckinLabels = GroupTypeCheckinLabelAttributesState[groupType.Guid];
            }

            if ( groupTypeEditor.CheckinLabels == null )
            {
                // load CheckInLabels from Database if they haven't been set yet
                groupTypeEditor.CheckinLabels = new List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>();

                groupType.LoadAttributes();
                List<string> labelAttributeKeys = CheckinGroupTypeEditor.GetCheckinLabelAttributes( groupType ).Select( a => a.Key ).ToList();
                BinaryFileService binaryFileService = new BinaryFileService();

                foreach ( string key in labelAttributeKeys )
                {
                    var attributeValue = groupType.GetAttributeValue( key );
                    int binaryFileId = attributeValue.AsInteger() ?? 0;
                    var binaryFile = binaryFileService.Get( binaryFileId );
                    if ( binaryFile != null )
                    {
                        groupTypeEditor.CheckinLabels.Add( new CheckinGroupTypeEditor.CheckinLabelAttributeInfo { AttributeKey = key, BinaryFileId = binaryFileId, FileName = binaryFile.FileName } );
                    }
                }
            }

            parentControl.Controls.Add( groupTypeEditor );

            foreach ( var childGroup in groupType.Groups.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                // get the GroupType from the control just in case it the InheritedFrom changed
                childGroup.GroupType = groupTypeEditor.GetCheckinGroupType();

                CreateGroupEditorControls( childGroup, groupTypeEditor, false );
            }

            foreach ( var childGroupType in groupType.ChildGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                CreateGroupTypeEditorControls( childGroupType, groupTypeEditor, false );
            }
        }

        /// <summary>
        /// Handles the DeleteGroupTypeClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void groupTypeEditor_DeleteGroupTypeClick( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles the Click event of the lbAddCheckinArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCheckinArea_Click( object sender, EventArgs e )
        {
            int parentGroupTypeId = this.PageParameter( "groupTypeid" ).AsInteger() ?? 0;
            GroupType parentGroupType = new GroupTypeService().Get( parentGroupTypeId );

            // CheckinArea is GroupType entity
            GroupType checkinArea = new GroupType();
            checkinArea.Guid = Guid.NewGuid();
            checkinArea.IsSystem = false;
            checkinArea.TakesAttendance = true;
            checkinArea.AttendanceRule = AttendanceRule.AddOnCheckIn;
            checkinArea.AttendancePrintTo = PrintTo.Default;
            checkinArea.ParentGroupTypes = new List<GroupType>();
            checkinArea.ParentGroupTypes.Add( parentGroupType );
            checkinArea.LoadAttributes();

            CreateGroupTypeEditorControls( checkinArea, phCheckinGroupTypes );
        }

        /// <summary>
        /// Handles the AddGroupTypeClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_AddGroupTypeClick( object sender, EventArgs e )
        {
            CheckinGroupTypeEditor parentEditor = sender as CheckinGroupTypeEditor;

            // CheckinArea is GroupType entity
            GroupType checkinArea = new GroupType();
            checkinArea.Guid = Guid.NewGuid();
            checkinArea.IsSystem = false;
            checkinArea.TakesAttendance = true;
            checkinArea.AttendanceRule = AttendanceRule.AddOnCheckIn;
            checkinArea.AttendancePrintTo = PrintTo.Default;
            checkinArea.ParentGroupTypes = new List<GroupType>();
            checkinArea.ParentGroupTypes.Add( parentEditor.GetCheckinGroupType() );
            checkinArea.LoadAttributes();

            CreateGroupTypeEditorControls( checkinArea, parentEditor );
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_AddGroupClick( object sender, EventArgs e )
        {
            CheckinGroupTypeEditor parentGroupTypeEditor = sender as CheckinGroupTypeEditor;

            Group checkinGroup = new Group();
            checkinGroup.Guid = Guid.NewGuid();
            checkinGroup.IsActive = true;
            checkinGroup.IsSystem = false;

            // set GroupType by Guid (just in case the parent groupType hasn't been added to the database yet)
            checkinGroup.GroupType = new GroupType { Guid = parentGroupTypeEditor.GroupTypeGuid };

            CreateGroupEditorControls( checkinGroup, parentGroupTypeEditor );
        }

        /// <summary>
        /// Creates the group editor controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="forceContentVisible">if set to <c>true</c> [force content visible].</param>
        private void CreateGroupEditorControls( Group group, Control parentControl, bool forceContentVisible = false )
        {
            CheckinGroupEditor groupEditor = new CheckinGroupEditor();
            groupEditor.ID = "GroupEditor_" + group.Guid.ToString( "N" );
            groupEditor.SetGroup( group );
            groupEditor.Locations = group.GroupLocations
                .Select( a =>
                    new CheckinGroupEditor.LocationGridItem()
                        {
                            LocationId = a.LocationId,
                            Name = a.Location.Name
                        } )
                        .OrderBy( o => o.Name )
                        .ToList();

            groupEditor.AddLocationClick += groupEditor_AddLocationClick;
            groupEditor.DeleteLocationClick += groupEditor_DeleteLocationClick;
            groupEditor.DeleteGroupClick += groupEditor_DeleteGroupClick;

            parentControl.Controls.Add( groupEditor );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void groupEditor_DeleteGroupClick( object sender, EventArgs e )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region CheckinLabel Add/Delete

        /// <summary>
        /// Handles the AddCheckinLabelClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void groupTypeEditor_AddCheckinLabelClick( object sender, EventArgs e )
        {
            CheckinGroupTypeEditor checkinGroupTypeEditor = sender as CheckinGroupTypeEditor;

            // set a hidden field value for the GroupType Guid so we know which GroupType to add the label to
            hfAddCheckinLabelGroupTypeGuid.Value = checkinGroupTypeEditor.GroupTypeGuid.ToString();

            Guid binaryFileTypeCheckinLabelGuid = new Guid( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL );

            var binaryFileService = new BinaryFileService();

            ddlCheckinLabel.Items.Clear();
            var list = binaryFileService.Queryable().Where( a => a.BinaryFileType.Guid.Equals( binaryFileTypeCheckinLabelGuid ) ).OrderBy( a => a.FileName ).ToList();

            foreach ( var item in list )
            {
                // add checkinlabels to dropdownlist if they aren't already a checkin label for this grouptype
                if ( !checkinGroupTypeEditor.CheckinLabels.Select( a => a.BinaryFileId ).Contains( item.Id ) )
                {
                    ddlCheckinLabel.Items.Add( new ListItem( item.FileName, item.Id.ToString() ) );
                }
            }

            // only enable the Add button if there are labels that can be added
            btnAddCheckinLabel.Enabled = ( ddlCheckinLabel.Items.Count > 0 );

            pnlCheckinLabelPicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the DeleteCheckinLabelClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void groupTypeEditor_DeleteCheckinLabelClick( object sender, RowEventArgs e )
        {
            CheckinGroupTypeEditor checkinGroupTypeEditor = sender as CheckinGroupTypeEditor;
            string attributeKey = e.RowKeyValue as string;

            var label = checkinGroupTypeEditor.CheckinLabels.FirstOrDefault( a => a.AttributeKey == attributeKey );
            checkinGroupTypeEditor.CheckinLabels.Remove( label );
            checkinGroupTypeEditor.ForceContentVisible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnAddCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCheckinLabel_Click( object sender, EventArgs e )
        {
            var groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().FirstOrDefault( a => a.GroupTypeGuid == new Guid( hfAddCheckinLabelGroupTypeGuid.Value ) );
            groupTypeEditor.ForceContentVisible = true;

            var checkinLabelAttributeInfo = new CheckinGroupTypeEditor.CheckinLabelAttributeInfo();
            checkinLabelAttributeInfo.BinaryFileId = ddlCheckinLabel.SelectedValueAsInt() ?? 0;
            checkinLabelAttributeInfo.FileName = ddlCheckinLabel.SelectedItem.Text;

            // have the attribute key just be the filename without spaces, but make sure it is not a duplicate
            string attributeKey = checkinLabelAttributeInfo.FileName.Replace( " ", string.Empty );
            checkinLabelAttributeInfo.AttributeKey = attributeKey;
            int duplicateInc = 1;
            while ( groupTypeEditor.CheckinLabels.Select( a => a.AttributeKey ).Contains( attributeKey + duplicateInc.ToString() ) )
            {
                // append number to end until it isn't a duplicate
                checkinLabelAttributeInfo.AttributeKey += attributeKey + duplicateInc.ToString();
                duplicateInc++;
            }

            groupTypeEditor.CheckinLabels.Add( checkinLabelAttributeInfo );

            pnlCheckinLabelPicker.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelAddCheckinLabel_Click( object sender, EventArgs e )
        {
            var groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().FirstOrDefault( a => a.GroupTypeGuid == new Guid( hfAddCheckinLabelGroupTypeGuid.Value ) );
            groupTypeEditor.ForceContentVisible = true;

            pnlCheckinLabelPicker.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

        #region Location Add/Delete

        /// <summary>
        /// Handles the AddLocationClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void groupEditor_AddLocationClick( object sender, EventArgs e )
        {
            CheckinGroupEditor checkinGroupEditor = sender as CheckinGroupEditor;

            // set a hidden field value for the Group Guid so we know which Group to add the location to
            hfAddLocationGroupGuid.Value = checkinGroupEditor.GroupGuid.ToString();

            var locationService = new LocationService();

            ddlLocation.Items.Clear();
            var list = locationService.Queryable().Where( a => a.IsLocation ).OrderBy( a => a.Name );

            foreach ( var item in list )
            {
                // add locations to dropdownlist if they aren't already a location for this group
                if ( !checkinGroupEditor.Locations.Select( a => a.LocationId ).Contains( item.Id ) )
                {
                    ddlLocation.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
                }
            }

            // only enable the Add button if there are labels that can be added
            btnAddLocation.Enabled = ( ddlLocation.Items.Count > 0 );

            pnlLocationPicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the DeleteLocationClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void groupEditor_DeleteLocationClick( object sender, RowEventArgs e )
        {
            CheckinGroupEditor checkinGroupEditor = sender as CheckinGroupEditor;
            checkinGroupEditor.ForceContentVisible = true;
            ( checkinGroupEditor.Parent as CheckinGroupTypeEditor ).ForceContentVisible = true;

            var location = checkinGroupEditor.Locations.FirstOrDefault( a => a.LocationId == e.RowKeyId );
            checkinGroupEditor.Locations.Remove( location );
        }

        /// <summary>
        /// Handles the Click event of the btnAddLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddLocation_Click( object sender, EventArgs e )
        {
            CheckinGroupEditor checkinGroupEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupEditor>().FirstOrDefault( a => a.GroupGuid == new Guid( hfAddLocationGroupGuid.Value ) );
            checkinGroupEditor.ForceContentVisible = true;
            ( checkinGroupEditor.Parent as CheckinGroupTypeEditor ).ForceContentVisible = true;

            CheckinGroupEditor.LocationGridItem gridItem = new CheckinGroupEditor.LocationGridItem();
            gridItem.LocationId = ddlLocation.SelectedValueAsId() ?? 0;
            gridItem.Name = ddlLocation.SelectedItem.Text;

            checkinGroupEditor.Locations.Add( gridItem );

            pnlLocationPicker.Visible = false;
            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAddLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelAddLocation_Click( object sender, EventArgs e )
        {
            CheckinGroupEditor checkinGroupEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupEditor>().FirstOrDefault( a => a.GroupGuid == new Guid( hfAddLocationGroupGuid.Value ) );
            checkinGroupEditor.ForceContentVisible = true;
            ( checkinGroupEditor.Parent as CheckinGroupTypeEditor ).ForceContentVisible = true;

            pnlLocationPicker.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            GroupService groupService = new GroupService();

            int parentGroupTypeId = hfParentGroupTypeId.ValueAsInt();

            var groupTypeUIList = new List<GroupType>();
            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>().ToList() )
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType();
                GroupTypeCheckinLabelAttributesState.Add( checkinGroupTypeEditor.GroupTypeGuid, checkinGroupTypeEditor.CheckinLabels );
                groupTypeUIList.Add( groupType );
            }

            var groupTypeDBList = new List<GroupType>();

            // limit to child group types that are not Templates
            int[] templateGroupTypes = new int[] {
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id, 
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER).Id
            };

            var groupTypesToDelete = new List<GroupType>();
            var groupsToDelete = new List<Group>();

            foreach (GroupType groupTypeUI in groupTypeUIList)
            {
                GroupType groupTypeDB = groupTypeService.Get( groupTypeUI.Guid );
                if ( groupTypeDB == null )
                {
                    groupTypeDB = new GroupType();
                    groupTypeDB.Guid = groupTypeUI.Guid;
                }

                groupTypeDB.Name = groupTypeUI.Name;
                groupTypeDB.InheritedGroupTypeId = groupTypeUI.InheritedGroupTypeId;
                
                // delete non-template childgrouptypes that were deleted in this ui
                foreach (var childGroupTypeDB in groupTypeDB.ChildGroupTypes)
                {
                    if (!templateGroupTypes.Contains(childGroupTypeDB.GroupTypePurposeValueId ?? 0))
                    {
                        if (!groupTypeUI.ChildGroupTypes.Select(a => a.Guid).Contains(childGroupTypeDB.Guid))
                        {
                            // TODO Delete GroupType and its Group and ChildGroupTypes recursively
                        }
                    }
                }

                foreach (var childGroup in groupTypeDB.Groups)
                {
                    if (!groupTypeUI.Groups.Select(a => a.Guid).Contains(childGroup.Guid))
                    {
                        // TODO Delete Group
                    }
                }

                // add/update Groups of each GroupType
                foreach (var childGroupUI in groupTypeUI.Groups)
                {
                    var childGroupDB = groupService.Get(childGroupUI.Guid);
                    if (childGroupDB == null)
                    {
                        childGroupDB = new Group();
                    }
                }

                // groupTypeDB.ChildGroupTypes

                // groupTypeDBList.Add( groupTypeDB );
            }





        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            //TODO
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            // hide the details panel until we verify the page params are good and that the user has edit access
            pnlDetails.Visible = false;

            if ( itemKey != "groupTypeId" )
            {
                return;
            }

            GroupTypeService groupTypeService = new GroupTypeService();
            GroupType parentGroupType = groupTypeService.Get( itemKeyValue );

            if ( parentGroupType == null )
            {
                pnlDetails.Visible = false;
                return;
            }

            hfParentGroupTypeId.Value = parentGroupType.Id.ToString();

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                // this UI doesn't have a ReadOnly mode, so just show a message and keep the Detail panel hidden
                nbEditModeMessage.Heading = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( "check-in configuration" );
                return;
            }

            pnlDetails.Visible = true;

            // limit to child group types that are not Templates
            int[] templateGroupTypes = new int[] {
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id, 
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER).Id
            };

            List<GroupType> checkinGroupTypes = parentGroupType.ChildGroupTypes.Where( a => !templateGroupTypes.Contains( a.GroupTypePurposeValueId ?? 0 ) ).ToList();

            // Load the Controls
            foreach ( GroupType groupType in checkinGroupTypes.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                groupType.LoadAttributes();
                CreateGroupTypeEditorControls( groupType, phCheckinGroupTypes );
            }
        }
    }
}