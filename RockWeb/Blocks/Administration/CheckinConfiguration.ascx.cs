using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
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
            else
            {
                // assume if there is a PostBack, that something changed  and that confirmExit should be enabled
                confirmExit.Enabled = true;
            }

            // handle sort events
            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-grouptype" ) || eventParam.Equals( "re-order-group" ) )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            SortGroupTypeListContents( eventParam, values );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the group type list contents.
        /// </summary>
        /// <param name="eventParam">The event parameter.</param>
        /// <param name="values">The values.</param>
        private void SortGroupTypeListContents( string eventParam, string[] values )
        {
            if ( eventParam.Equals( "re-order-grouptype" ) )
            {
                var allCheckinGroupTypeEditors = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>();
                Guid groupTypeGuid = new Guid( values[0] );
                int newIndex = int.Parse( values[1] );

                CheckinGroupTypeEditor sortedGroupTypeEditor = allCheckinGroupTypeEditors.FirstOrDefault( a => a.GroupTypeGuid.Equals( groupTypeGuid ) );
                if ( sortedGroupTypeEditor != null )
                {
                    var siblingGroupTypes = allCheckinGroupTypeEditors.Where( a => a.ParentGroupTypeEditor == sortedGroupTypeEditor.ParentGroupTypeEditor ).ToList();
                    Control parentControl = sortedGroupTypeEditor.Parent;
                    parentControl.Controls.Remove( sortedGroupTypeEditor );
                    if ( newIndex >= siblingGroupTypes.Count() )
                    {
                        parentControl.Controls.Add( sortedGroupTypeEditor );
                    }
                    else
                    {
                        parentControl.Controls.AddAt( newIndex, sortedGroupTypeEditor );
                    }
                }
            }
            else if ( eventParam.Equals( "re-order-group" ) )
            {
                var allCheckinGroupEditors = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupEditor>();

                Guid groupGuid = new Guid( values[0] );
                int newIndex = int.Parse( values[1] );

                CheckinGroupEditor sortedGroupEditor = allCheckinGroupEditors.FirstOrDefault( a => a.GroupGuid.Equals( groupGuid ) );
                if ( sortedGroupEditor != null )
                {
                    var siblingGroupEditors = allCheckinGroupEditors.Where( a => a.GroupTypeId == sortedGroupEditor.GroupTypeId ).ToList();
                    
                    Control parentControl = sortedGroupEditor.Parent;
                    // parent control has other controls, so just just remove all the checkingroupeditors, sort them, and add them back in the new order
                    foreach ( var item in siblingGroupEditors )
                    {
                        parentControl.Controls.Remove( item );
                    }

                    siblingGroupEditors.Remove( sortedGroupEditor );
                    if ( newIndex >= siblingGroupEditors.Count() )
                    {
                        siblingGroupEditors.Add( sortedGroupEditor );
                    }
                    else
                    {
                        siblingGroupEditors.Insert( newIndex, sortedGroupEditor );
                    }

                    foreach ( var item in siblingGroupEditors )
                    {
                        parentControl.Controls.Add( item );
                    }

                    (sortedGroupEditor.Parent as CheckinGroupTypeEditor).ForceContentVisible = true;
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
            SaveGroupTypeControlsToViewState();

            return base.SaveViewState();
        }

        /// <summary>
        /// Saves the state of the group type controls to viewstate.
        /// </summary>
        private void SaveGroupTypeControlsToViewState()
        {
            // save all the base grouptypes (along with their children) to viewstate
            var groupTypeList = new List<GroupType>();
            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType();
                groupTypeList.Add( groupType );
            }

            ViewStateList<GroupType> groupTypeViewStateList = new ViewStateList<GroupType>();
            groupTypeViewStateList.AddAll( groupTypeList );

            ViewState["CheckinGroupTypes"] = groupTypeViewStateList;

            // save all the checkinlabels for all the grouptypes (recursively) to viewstate
            GroupTypeCheckinLabelAttributesState = new Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>>();
            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().ToList() )
            {
                GroupTypeCheckinLabelAttributesState.Add( checkinGroupTypeEditor.GroupTypeGuid, checkinGroupTypeEditor.CheckinLabels );
            }
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
        private void BuildGroupTypeEditorControlsFromViewState()
        {
            phCheckinGroupTypes.Controls.Clear();

            ViewStateList<GroupType> groupTypeViewStateList = ViewState["CheckinGroupTypes"] as ViewStateList<GroupType>;

            foreach ( var groupType in groupTypeViewStateList )
            {
                CreateGroupTypeEditorControls( groupType, phCheckinGroupTypes );
            }
        }

        /// <summary>
        /// Creates the group type editor controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        private void CreateGroupTypeEditorControls( GroupType groupType, Control parentControl )
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
                CreateGroupTypeEditorControls( childGroupType, groupTypeEditor );
            }
        }

        /// <summary>
        /// Handles the DeleteGroupTypeClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_DeleteGroupTypeClick( object sender, EventArgs e )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            CheckinGroupTypeEditor groupTypeEditor = sender as CheckinGroupTypeEditor;
            GroupType groupType = groupTypeService.Get( groupTypeEditor.GroupTypeGuid );
            if ( groupType != null )
            {
                // Warn if this GroupType or any of its child grouptypes (recursive) is being used as an Inherited Group Type. Probably shouldn't happen, but just in case
                if ( IsInheritedGroupTypeRecursive( groupType ) )
                {
                    nbDeleteWarning.Text = "WARNING - Cannot delete. This group type or one of its child group types is assigned as an inherited group type.";
                    nbDeleteWarning.Visible = true;
                    return;
                }
            }

            groupTypeEditor.Parent.Controls.Remove( groupTypeEditor );
        }

        /// <summary>
        /// Determines whether [is inherited group type recursive] [the specified group type].
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <returns></returns>
        private static bool IsInheritedGroupTypeRecursive( GroupType groupType )
        {
            if ( new GroupTypeService().Queryable().Any( a => a.InheritedGroupType.Guid == groupType.Guid ) )
            {
                return true;
            }

            foreach ( var childGroupType in groupType.ChildGroupTypes )
            {
                if ( IsInheritedGroupTypeRecursive( childGroupType ) )
                {
                    return true;
                }
            }

            return false;
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
            parentEditor.ForceContentVisible = true;

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
            parentGroupTypeEditor.ForceContentVisible = true;

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
        protected void groupEditor_DeleteGroupClick( object sender, EventArgs e )
        {
            CheckinGroupEditor groupEditor = sender as CheckinGroupEditor;
            GroupService groupService = new GroupService();
            Group groupDB = groupService.Get( groupEditor.GroupGuid );
            if ( groupDB != null )
            {
                string errorMessage;
                if ( !groupService.CanDelete( groupDB, out errorMessage ) )
                {
                    nbDeleteWarning.Text = "WARNING - Cannot Delete: " + errorMessage;
                    nbDeleteWarning.Visible = true;
                    return;
                }
            }

            groupEditor.Parent.Controls.Remove( groupEditor );
        }

        #endregion

        #region CheckinLabel Add/Delete

        /// <summary>
        /// Handles the AddCheckinLabelClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
            btnAddCheckinLabel.Enabled = ddlCheckinLabel.Items.Count > 0;

            pnlCheckinLabelPicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the DeleteCheckinLabelClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
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
            btnAddLocation.Enabled = ddlLocation.Items.Count > 0;

            pnlLocationPicker.Visible = true;
            pnlDetails.Visible = false;
        }

        /// <summary>
        /// Handles the DeleteLocationClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
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
            bool hasValidationErrors = false;

            using ( new UnitOfWorkScope() )
            {
                GroupTypeService groupTypeService = new GroupTypeService();
                GroupService groupService = new GroupService();
                AttributeService attributeService = new AttributeService();

                int parentGroupTypeId = hfParentGroupTypeId.ValueAsInt();

                var groupTypeUIList = new List<GroupType>();

                foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>().ToList() )
                {
                    var groupType = checkinGroupTypeEditor.GetCheckinGroupType();
                    groupTypeUIList.Add( groupType );
                }

                var groupTypeDBList = new List<GroupType>();

                var groupTypesToDelete = new List<GroupType>();
                var groupsToDelete = new List<Group>();

                var groupTypesToAddUpdate = new List<GroupType>();
                var groupsToAddUpdate = new List<Group>();

                GroupType parentGroupTypeDB = groupTypeService.Get( parentGroupTypeId );
                GroupType parentGroupTypeUI = parentGroupTypeDB.Clone( false );
                parentGroupTypeUI.ChildGroupTypes = groupTypeUIList;

                PopulateDeleteLists( groupTypesToDelete, groupsToDelete, parentGroupTypeDB, parentGroupTypeUI );
                PopulateAddUpdateLists( groupTypesToAddUpdate, groupsToAddUpdate, parentGroupTypeUI );

                int binaryFileFieldTypeID = new FieldTypeService().Get( new Guid( Rock.SystemGuid.FieldType.BINARY_FILE ) ).Id;
                int binaryFileTypeId = new BinaryFileTypeService().Get( new Guid( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL ) ).Id;

                RockTransactionScope.WrapTransaction( () =>
                {
                    // delete in reverse order to get deepest child items first
                    groupsToDelete.Reverse();
                    foreach ( var groupToDelete in groupsToDelete )
                    {
                        groupService.Delete( groupToDelete, this.CurrentPersonId );
                        groupService.Save( groupToDelete, this.CurrentPersonId );
                    }

                    // delete in reverse order to get deepest child items first
                    groupTypesToDelete.Reverse();
                    foreach ( var groupTypeToDelete in groupTypesToDelete )
                    {
                        groupTypeService.Delete( groupTypeToDelete, this.CurrentPersonId );
                        groupTypeService.Save( groupTypeToDelete, this.CurrentPersonId );
                    }

                    // Add/Update grouptypes and groups that are in the UI
                    // Note:  We'll have to save all the groupTypes without changing the DB value of ChildGroupTypes, then come around again and save the ChildGroupTypes 
                    // since the ChildGroupTypes may not exist in the database yet
                    foreach ( GroupType groupTypeUI in groupTypesToAddUpdate )
                    {
                        GroupType groupTypeDB = groupTypeService.Get( groupTypeUI.Guid );
                        if ( groupTypeDB == null )
                        {
                            groupTypeDB = new GroupType();
                            groupTypeDB.Id = 0;
                            groupTypeDB.Guid = groupTypeUI.Guid;
                        }

                        groupTypeDB.Name = groupTypeUI.Name;
                        groupTypeDB.Order = groupTypeUI.Order;
                        groupTypeDB.InheritedGroupTypeId = groupTypeUI.InheritedGroupTypeId;

                        groupTypeDB.Attributes = groupTypeUI.Attributes;
                        groupTypeDB.AttributeValues = groupTypeUI.AttributeValues;

                        if ( groupTypeDB.Id == 0 )
                        {
                            groupTypeService.Add( groupTypeDB, this.CurrentPersonId );
                        }

                        if ( !groupTypeDB.IsValid )
                        {
                            hasValidationErrors = true;
                            CheckinGroupTypeEditor groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().First( a => a.GroupTypeGuid == groupTypeDB.Guid );
                            groupTypeEditor.ForceContentVisible = true;

                            return;
                        }

                        groupTypeService.Save( groupTypeDB, this.CurrentPersonId );

                        Rock.Attribute.Helper.SaveAttributeValues( groupTypeDB, this.CurrentPersonId );

                        // get fresh from database to make sure we have Id so we can update the CheckinLabel Attributes
                        groupTypeDB = groupTypeService.Get( groupTypeDB.Guid );

                        // rebuild the CheckinLabel attributes from the UI (brute-force)
                        foreach ( var labelAttributeDB in CheckinGroupTypeEditor.GetCheckinLabelAttributes( groupTypeDB ) )
                        {
                            var attribute = attributeService.Get( labelAttributeDB.Value.Guid );
                            Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                            attributeService.Delete( attribute, this.CurrentPersonId );
                        }

                        foreach ( var checkinLabelAttributeInfo in GroupTypeCheckinLabelAttributesState[groupTypeUI.Guid] )
                        {
                            var attribute = new Rock.Model.Attribute();
                            attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "binaryFileType", Value = binaryFileTypeId.ToString() } );
                            attribute.Guid = Guid.NewGuid();
                            attribute.FieldTypeId = binaryFileFieldTypeID;
                            attribute.EntityTypeId = EntityTypeCache.GetId( typeof( GroupType ) );
                            attribute.EntityTypeQualifierColumn = "Id";
                            attribute.EntityTypeQualifierValue = groupTypeDB.Id.ToString();
                            attribute.DefaultValue = checkinLabelAttributeInfo.BinaryFileId.ToString();
                            attribute.Key = checkinLabelAttributeInfo.AttributeKey;
                            attribute.Name = checkinLabelAttributeInfo.FileName;

                            if ( !attribute.IsValid )
                            {
                                hasValidationErrors = true;
                                CheckinGroupTypeEditor groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().First( a => a.GroupTypeGuid == groupTypeDB.Guid );
                                groupTypeEditor.ForceContentVisible = true;

                                return;
                            }

                            attributeService.Add( attribute, this.CurrentPersonId );
                            attributeService.Save( attribute, this.CurrentPersonId );
                        }
                    }

                    // Add/Update Groups
                    foreach ( var groupUI in groupsToAddUpdate )
                    {
                        Group groupDB = groupService.Get( groupUI.Guid );
                        if ( groupDB == null )
                        {
                            groupDB = new Group();
                            groupDB.Guid = groupUI.Guid;
                        }

                        groupDB.Name = groupUI.Name;

                        // delete any GroupLocations that were removed in the UI
                        foreach ( var groupLocationDB in groupDB.GroupLocations.ToList() )
                        {
                            if ( !groupUI.GroupLocations.Select( a => a.LocationId ).Contains( groupLocationDB.LocationId ) )
                            {
                                groupDB.GroupLocations.Remove( groupLocationDB );
                            }
                        }

                        // add any GroupLocations that were added in the UI
                        foreach ( var groupLocationUI in groupUI.GroupLocations )
                        {
                            if ( !groupDB.GroupLocations.Select( a => a.LocationId ).Contains( groupLocationUI.LocationId ) )
                            {
                                GroupLocation groupLocationDB = new GroupLocation { LocationId = groupLocationUI.LocationId };
                                groupDB.GroupLocations.Add( groupLocationDB );
                            }
                        }

                        groupDB.Order = groupUI.Order;

                        // get GroupTypeId from database in case the groupType is new
                        groupDB.GroupTypeId = groupTypeService.Get( groupUI.GroupType.Guid ).Id;
                        groupDB.Attributes = groupUI.Attributes;
                        groupDB.AttributeValues = groupUI.AttributeValues;

                        if ( groupDB.Id == 0 )
                        {
                            groupService.Add( groupDB, this.CurrentPersonId );
                        }

                        if ( !groupDB.IsValid )
                        {
                            hasValidationErrors = true;
                            hasValidationErrors = true;
                            CheckinGroupEditor groupEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupEditor>().First( a => a.GroupGuid == groupDB.Guid );
                            groupEditor.ForceContentVisible = true;

                            return;
                        }

                        groupService.Save( groupDB, this.CurrentPersonId );

                        Rock.Attribute.Helper.SaveAttributeValues( groupDB, this.CurrentPersonId );
                    }

                    /* now that we have all the grouptypes saved, now lets go back and save them again with the current UI ChildGroupTypes */

                    // save main parentGroupType with current UI ChildGroupTypes
                    parentGroupTypeDB.ChildGroupTypes = new List<GroupType>();
                    parentGroupTypeDB.ChildGroupTypes.Clear();
                    foreach ( var childGroupTypeUI in parentGroupTypeUI.ChildGroupTypes )
                    {
                        var childGroupTypeDB = groupTypeService.Get( childGroupTypeUI.Guid );
                        parentGroupTypeDB.ChildGroupTypes.Add( childGroupTypeDB );
                    }

                    groupTypeService.Save( parentGroupTypeDB, this.CurrentPersonId );

                    // loop thru all the other GroupTypes in the UI and save their childgrouptypes
                    foreach ( var groupTypeUI in groupTypesToAddUpdate )
                    {
                        var groupTypeDB = groupTypeService.Get( groupTypeUI.Guid );
                        groupTypeDB.ChildGroupTypes = new List<GroupType>();
                        groupTypeDB.ChildGroupTypes.Clear();
                        foreach ( var childGroupTypeUI in groupTypeUI.ChildGroupTypes )
                        {
                            var childGroupTypeDB = groupTypeService.Get( childGroupTypeUI.Guid );
                            groupTypeDB.ChildGroupTypes.Add( childGroupTypeDB );
                        }

                        groupTypeService.Save( groupTypeDB, this.CurrentPersonId );
                    }
                } );
            }

            if ( !hasValidationErrors )
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Populates the delete lists (recursive)
        /// </summary>
        /// <param name="groupTypesToDelete">The group types to delete.</param>
        /// <param name="groupsToDelete">The groups to delete.</param>
        /// <param name="groupTypeDB">The group type DB.</param>
        /// <param name="groupTypeUI">The group type UI.</param>
        private static void PopulateDeleteLists( List<GroupType> groupTypesToDelete, List<Group> groupsToDelete, GroupType groupTypeDB, GroupType groupTypeUI )
        {
            // limit to child group types that are not Templates
            int[] templateGroupTypes = new int[] {
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE).Id, 
                DefinedValueCache.Read(Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_FILTER).Id
            };

            // delete non-template childgrouptypes that were deleted in this ui
            foreach ( var childGroupTypeDB in groupTypeDB.ChildGroupTypes )
            {
                if ( !templateGroupTypes.Contains( childGroupTypeDB.GroupTypePurposeValueId ?? 0 ) )
                {
                    GroupType childGroupTypeUI = null;
                    if ( groupTypeUI != null )
                    {
                        childGroupTypeUI = groupTypeUI.ChildGroupTypes.FirstOrDefault( a => a.Guid == childGroupTypeDB.Guid );
                    }

                    PopulateDeleteLists( groupTypesToDelete, groupsToDelete, childGroupTypeDB, childGroupTypeUI );

                    if ( childGroupTypeUI == null )
                    {
                        groupTypesToDelete.Add( childGroupTypeDB );

                        // delete all the groups that are in the GroupType that is getting deleted
                        foreach ( var group in childGroupTypeDB.Groups )
                        {
                            groupsToDelete.Add( group );
                        }
                    }
                    else
                    {
                        // delete all the groups that are no longer in the UI for this groupType
                        foreach ( var childGroupDB in childGroupTypeDB.Groups )
                        {
                            if ( childGroupTypeUI.Groups.FirstOrDefault( a => a.Guid == childGroupDB.Guid ) == null )
                            {
                                groupsToDelete.Add( childGroupDB );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates the add update lists.
        /// </summary>
        /// <param name="groupTypesToAddUpdate">The group types to add update.</param>
        /// <param name="groupsToAddUpdate">The groups to add update.</param>
        /// <param name="groupTypeUI">The group type UI.</param>
        private static void PopulateAddUpdateLists( List<GroupType> groupTypesToAddUpdate, List<Group> groupsToAddUpdate, GroupType groupTypeUI )
        {
            int groupTypeSortOrder = 0;
            int groupSortOrder = 0;
            foreach ( var childGroupTypeUI in groupTypeUI.ChildGroupTypes )
            {
                PopulateAddUpdateLists( groupTypesToAddUpdate, groupsToAddUpdate, childGroupTypeUI );
                childGroupTypeUI.Order = groupTypeSortOrder++;
                groupTypesToAddUpdate.Add( childGroupTypeUI );
                foreach ( var groupUI in childGroupTypeUI.Groups )
                {
                    groupUI.Order = groupSortOrder++;
                    groupsToAddUpdate.Add( groupUI );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
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