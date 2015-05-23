// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Check-in Configuration" )]
    [Category( "Check-in" )]
    [Description( "Helps to configure the check-in workflow." )]
    public partial class CheckinConfiguration : RockBlock, IDetailBlock
    {
        #region Properties

        private List<Guid> ProcessedGroupTypeIds = new List<Guid>();
        private List<Guid> ProcessedGroupIds = new List<Guid>();

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Save and Cancel should not confirm exit
            btnSave.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );
            btnCancel.OnClientClick = string.Format( "javascript:$('#{0}').val('');return true;", confirmExit.ClientID );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "groupTypeId" ).AsInteger() );
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
                    var siblingGroupEditors = allCheckinGroupEditors
                        .Where( a => 
                            a.GroupTypeId == sortedGroupEditor.GroupTypeId &&
                            a.Parent.ClientID == sortedGroupEditor.Parent.ClientID )
                        .ToList();

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

                    ExpandGroupEditorParent( sortedGroupEditor );
                }
            }
        }

        #endregion Control Methods

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
            var rockContext = new RockContext();

            // save all the base grouptypes (along with their children) to viewstate
            var groupTypeList = new List<GroupType>();
            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>() )
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType( rockContext );
                groupTypeList.Add( groupType );
            }

            ViewStateList<GroupType> groupTypeViewStateList = new ViewStateList<GroupType>();
            groupTypeViewStateList.AddAll( groupTypeList );
            ViewState["CheckinGroupTypes"] = groupTypeViewStateList;

            // get all GroupTypes' editors to save groups and labels
            var recursiveGroupTypeEditors = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().ToList();

            // save each GroupTypes' Groups to ViewState (since GroupType.Groups are not Serialized)
            var groupTypeGroupsList = new List<Group>();
            foreach ( var editor in recursiveGroupTypeEditors )
            {
                var groupType = editor.GetCheckinGroupType( rockContext );
                groupTypeGroupsList.AddRange( groupType.Groups );
            }

            ViewStateList<Group> checkinGroupTypesGroups = new ViewStateList<Group>();
            checkinGroupTypesGroups.AddAll( groupTypeGroupsList );
            ViewState["CheckinGroupTypesGroups"] = checkinGroupTypesGroups;

            // save all the checkinlabels for all the grouptypes (recursively) to viewstate
            GroupTypeCheckinLabelAttributesState = new Dictionary<Guid, List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>>();
            foreach ( var checkinGroupTypeEditor in recursiveGroupTypeEditors )
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
            var rockContext = new RockContext();

            // GroupTypeViewStateList only contains parent GroupTypes, so get all the child GroupTypes and assign their groups
            ViewStateList<GroupType> groupTypeViewStateList = ViewState["CheckinGroupTypes"] as ViewStateList<GroupType>;
            var allGroupTypesList = groupTypeViewStateList.Flatten<GroupType>( gt => gt.ChildGroupTypes );

            // load each GroupTypes' Groups from ViewState (since GroupType.Groups are not Serialized)
            ViewStateList<Group> checkinGroupTypesGroups = ViewState["CheckinGroupTypesGroups"] as ViewStateList<Group>;
            foreach ( var groupTypeGroups in checkinGroupTypesGroups.GroupBy( g => g.GroupType.Guid ) )
            {
                var groupType = allGroupTypesList.FirstOrDefault( a => a.Guid == groupTypeGroups.Key );

                if ( groupType != null )
                {
                    groupType.Groups = new List<Group>();
                    foreach ( var group in groupTypeGroups )
                    {
                        groupType.Groups.Add( group );
                    }
                }
            }

            // Build out Parent GroupTypes controls (Child GroupTypes controls are built recursively)
            ProcessedGroupTypeIds = new List<Guid>();
            foreach ( var groupType in groupTypeViewStateList )
            {
                CreateGroupTypeEditorControls( groupType, phCheckinGroupTypes, rockContext );
            }
        }

        /// <summary>
        /// Creates the group type editor controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="createExpanded">if set to <c>true</c> [create expanded].</param>
        private void CreateGroupTypeEditorControls( GroupType groupType, Control parentControl, RockContext rockContext, bool createExpanded = false )
        {
            ProcessedGroupTypeIds.Add( groupType.Guid );

            CheckinGroupTypeEditor groupTypeEditor = new CheckinGroupTypeEditor();
            groupTypeEditor.ID = "GroupTypeEditor_" + groupType.Guid.ToString( "N" );
            groupTypeEditor.SetGroupType( groupType.Id, groupType.Guid, groupType.Name, groupType.InheritedGroupTypeId );
            groupTypeEditor.AddGroupClick += groupTypeEditor_AddGroupClick;
            groupTypeEditor.AddGroupTypeClick += groupTypeEditor_AddGroupTypeClick;
            groupTypeEditor.DeleteCheckinLabelClick += groupTypeEditor_DeleteCheckinLabelClick;
            groupTypeEditor.AddCheckinLabelClick += groupTypeEditor_AddCheckinLabelClick;
            groupTypeEditor.DeleteGroupTypeClick += groupTypeEditor_DeleteGroupTypeClick;
            groupTypeEditor.CheckinLabels = null;
            if ( createExpanded )
            {
                groupTypeEditor.Expanded = true;
            }

            if ( GroupTypeCheckinLabelAttributesState.ContainsKey( groupType.Guid ) )
            {
                groupTypeEditor.CheckinLabels = GroupTypeCheckinLabelAttributesState[groupType.Guid];
            }

            if ( groupTypeEditor.CheckinLabels == null )
            {
                // load CheckInLabels from Database if they haven't been set yet
                groupTypeEditor.CheckinLabels = new List<CheckinGroupTypeEditor.CheckinLabelAttributeInfo>();

                groupType.LoadAttributes( rockContext );
                List<string> labelAttributeKeys = CheckinGroupTypeEditor.GetCheckinLabelAttributes( groupType.Attributes, rockContext ).Select( a => a.Key ).ToList();
                BinaryFileService binaryFileService = new BinaryFileService( rockContext );

                foreach ( string key in labelAttributeKeys )
                {
                    var attributeValue = groupType.GetAttributeValue( key );
                    Guid binaryFileGuid = attributeValue.AsGuid();
                    var fileName = binaryFileService.Queryable().Where( a => a.Guid == binaryFileGuid ).Select( a => a.FileName ).FirstOrDefault();
                    if ( fileName != null )
                    {
                        groupTypeEditor.CheckinLabels.Add( new CheckinGroupTypeEditor.CheckinLabelAttributeInfo { AttributeKey = key, BinaryFileGuid = binaryFileGuid, FileName = fileName } );
                    }
                }
            }

            parentControl.Controls.Add( groupTypeEditor );

            // get the GroupType from the control just in case the InheritedFrom changed
            var childGroupGroupType = groupTypeEditor.GetCheckinGroupType( rockContext );

            // Find the groups of this type, who's parent is null, or another group type ( "root" groups ).
            var allGroupIds = groupType.Groups.Select( g => g.Id).ToList();
            ProcessedGroupIds = new List<Guid>();
            foreach ( var childGroup in groupType.Groups
                .Where( g => 
                    !g.ParentGroupId.HasValue ||
                    !allGroupIds.Contains( g.ParentGroupId.Value ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                childGroup.GroupType = childGroupGroupType;
                CreateGroupEditorControls( childGroup, groupTypeEditor, rockContext, false );
            }

            foreach ( var childGroupType in groupType.ChildGroupTypes
                .Where( t => !ProcessedGroupTypeIds.Contains( t.Guid ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                CreateGroupTypeEditorControls( childGroupType, groupTypeEditor, rockContext );
            }
        }

        /// <summary>
        /// Handles the DeleteGroupTypeClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_DeleteGroupTypeClick( object sender, EventArgs e )
        {
            CheckinGroupTypeEditor groupTypeEditor = sender as CheckinGroupTypeEditor;
            var rockContext = new RockContext();
            var groupType = GroupTypeCache.Read( groupTypeEditor.GroupTypeGuid );
            if ( groupType != null )
            {
                // Warn if this GroupType or any of its child grouptypes (recursive) is being used as an Inherited Group Type. Probably shouldn't happen, but just in case
                if ( IsInheritedGroupTypeRecursive( groupType, rockContext ) )
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static bool IsInheritedGroupTypeRecursive( GroupTypeCache groupType, RockContext rockContext )
        {
            if ( new GroupTypeService( rockContext ).Queryable().Any( a => a.InheritedGroupType.Guid == groupType.Guid ) )
            {
                return true;
            }

            foreach ( var childGroupType in groupType.ChildGroupTypes )
            {
                if ( IsInheritedGroupTypeRecursive( childGroupType, rockContext ) )
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
            int parentGroupTypeId = this.PageParameter( "groupTypeid" ).AsInteger();
            var rockContext = new RockContext();
            GroupType parentGroupType = new GroupTypeService( rockContext ).Get( parentGroupTypeId );

            // CheckinArea is GroupType entity
            GroupType checkinArea = new GroupType();
            checkinArea.Guid = Guid.NewGuid();
            checkinArea.IsSystem = false;
            checkinArea.ShowInNavigation = false;
            checkinArea.TakesAttendance = true;
            checkinArea.AttendanceRule = AttendanceRule.AddOnCheckIn;
            checkinArea.AttendancePrintTo = PrintTo.Default;
            checkinArea.ParentGroupTypes = new List<GroupType>();
            checkinArea.ParentGroupTypes.Add( parentGroupType );

            ProcessedGroupTypeIds = new List<Guid>();
            CreateGroupTypeEditorControls( checkinArea, phCheckinGroupTypes, rockContext, true );
        }

        /// <summary>
        /// Handles the AddGroupTypeClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_AddGroupTypeClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            CheckinGroupTypeEditor parentEditor = sender as CheckinGroupTypeEditor;
            parentEditor.Expanded = true;

            // CheckinArea is GroupType entity
            GroupType checkinArea = new GroupType();
            checkinArea.Guid = Guid.NewGuid();
            checkinArea.IsSystem = false;
            checkinArea.ShowInNavigation = false;
            checkinArea.TakesAttendance = true;
            checkinArea.AttendanceRule = AttendanceRule.AddOnCheckIn;
            checkinArea.AttendancePrintTo = PrintTo.Default;
            checkinArea.ParentGroupTypes = new List<GroupType>();
            checkinArea.ParentGroupTypes.Add( parentEditor.GetCheckinGroupType( rockContext ) );

            ProcessedGroupTypeIds = new List<Guid>();
            CreateGroupTypeEditorControls( checkinArea, parentEditor, rockContext, true );
        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupTypeEditor_AddGroupClick( object sender, EventArgs e )
        {
            CheckinGroupTypeEditor parentGroupTypeEditor = sender as CheckinGroupTypeEditor;
            parentGroupTypeEditor.Expanded = true;

            Group checkinGroup = new Group();
            checkinGroup.Guid = Guid.NewGuid();
            checkinGroup.IsActive = true;
            checkinGroup.IsSystem = false;

            // set GroupType by Guid (just in case the parent groupType hasn't been added to the database yet)
            checkinGroup.GroupType = new GroupType { Guid = parentGroupTypeEditor.GroupTypeGuid };

            var rockContext = new RockContext();

            ProcessedGroupIds = new List<Guid>();
            CreateGroupEditorControls( checkinGroup, parentGroupTypeEditor, rockContext, true );
        }

        /// <summary>
        /// Creates the group editor controls.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="createExpanded">if set to <c>true</c> [create expanded].</param>
        private void CreateGroupEditorControls( Group group, Control parentControl, RockContext rockContext, bool createExpanded = false )
        {
            ProcessedGroupIds.Add( group.Guid );

            CheckinGroupEditor groupEditor = new CheckinGroupEditor();
            groupEditor.ID = "GroupEditor_" + group.Guid.ToString( "N" );
            if ( createExpanded )
            {
                groupEditor.Expanded = true;
            }

            parentControl.Controls.Add( groupEditor );
            groupEditor.SetGroup( group, rockContext );
            var locationService = new LocationService( rockContext );
            var locationQry = locationService.Queryable().Select( a => new { a.Id, a.ParentLocationId, a.Name } );

            groupEditor.Locations = new List<CheckinGroupEditor.LocationGridItem>();
            foreach ( var location in group.GroupLocations.Select( a => a.Location ).OrderBy( o => o.Name ) )
            {
                var gridItem = new CheckinGroupEditor.LocationGridItem();
                gridItem.LocationId = location.Id;
                gridItem.Name = location.Name;
                gridItem.FullNamePath = location.Name;
                gridItem.ParentLocationId = location.ParentLocationId;

                var parentLocationId = location.ParentLocationId;
                while ( parentLocationId != null )
                {
                    var parentLocation = locationQry.FirstOrDefault( a => a.Id == parentLocationId );
                    gridItem.FullNamePath = parentLocation.Name + " > " + gridItem.FullNamePath;
                    parentLocationId = parentLocation.ParentLocationId;
                }

                groupEditor.Locations.Add( gridItem );
            }

            groupEditor.AddGroupClick += groupEditor_AddGroupClick;
            groupEditor.AddLocationClick += groupEditor_AddLocationClick;
            groupEditor.DeleteLocationClick += groupEditor_DeleteLocationClick;
            groupEditor.DeleteGroupClick += groupEditor_DeleteGroupClick;

            foreach ( var childGroup in group.Groups
                .Where( a => 
                    !ProcessedGroupIds.Contains( a.Guid ) &&
                    a.GroupTypeId == group.GroupTypeId
                    )
                .OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                childGroup.GroupType = group.GroupType;
                CreateGroupEditorControls( childGroup, groupEditor, rockContext, false );
            }

        }

        /// <summary>
        /// Handles the AddGroupClick event of the groupTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupEditor_AddGroupClick( object sender, EventArgs e )
        {
            CheckinGroupEditor parentGroupEditor = sender as CheckinGroupEditor;
            parentGroupEditor.Expanded = true;

            Group checkinGroup = new Group();
            checkinGroup.Guid = Guid.NewGuid();
            checkinGroup.IsActive = true;
            checkinGroup.IsSystem = false;

            // set GroupType by Guid (just in case the parent groupType hasn't been added to the database yet)
            checkinGroup.GroupType = new GroupType { Guid = parentGroupEditor.GetParentGroupTypeEditor().GroupTypeGuid };

            var rockContext = new RockContext();

            ProcessedGroupIds = new List<Guid>();
            CreateGroupEditorControls( checkinGroup, parentGroupEditor, rockContext, true );
        }

        /// <summary>
        /// Handles the DeleteGroupClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void groupEditor_DeleteGroupClick( object sender, EventArgs e )
        {
            CheckinGroupEditor groupEditor = sender as CheckinGroupEditor;
            GroupService groupService = new GroupService( new RockContext() );
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

        #endregion ViewState and Dynamic Controls

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

            var binaryFileService = new BinaryFileService( new RockContext() );

            ddlCheckinLabel.Items.Clear();
            ddlCheckinLabel.AutoPostBack = false;
            ddlCheckinLabel.Required = true;
            ddlCheckinLabel.Items.Add( new ListItem() );

            var list = binaryFileService.Queryable().Where( a => a.BinaryFileType.Guid.Equals( binaryFileTypeCheckinLabelGuid ) && a.IsTemporary == false ).OrderBy( a => a.FileName ).ToList();

            foreach ( var item in list )
            {
                // add checkinlabels to dropdownlist if they aren't already a checkin label for this grouptype
                if ( !checkinGroupTypeEditor.CheckinLabels.Select( a => a.BinaryFileGuid ).Contains( item.Guid ) )
                {
                    ddlCheckinLabel.Items.Add( new ListItem( item.FileName, item.Guid.ToString() ) );
                }
            }

            mdAddCheckinLabel.Show();
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
            checkinGroupTypeEditor.Expanded = true;
        }

        /// <summary>
        /// Handles the Click event of the btnAddCheckinLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCheckinLabel_Click( object sender, EventArgs e )
        {
            var groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().FirstOrDefault( a => a.GroupTypeGuid == new Guid( hfAddCheckinLabelGroupTypeGuid.Value ) );
            groupTypeEditor.Expanded = true;

            var checkinLabelAttributeInfo = new CheckinGroupTypeEditor.CheckinLabelAttributeInfo();
            checkinLabelAttributeInfo.BinaryFileGuid = ddlCheckinLabel.SelectedValue.AsGuid();
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

            pnlDetails.Visible = true;
        }

        #endregion CheckinLabel Add/Delete

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
            checkinGroupEditor.Expanded = true;
            ExpandGroupEditorParent( checkinGroupEditor );

            mdLocationPicker.Show();
        }

        /// <summary>
        /// Handles the DeleteLocationClick event of the groupEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void groupEditor_DeleteLocationClick( object sender, RowEventArgs e )
        {
            CheckinGroupEditor checkinGroupEditor = sender as CheckinGroupEditor;
            checkinGroupEditor.Expanded = true;
            ExpandGroupEditorParent( checkinGroupEditor );

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

            // Add the location (ignore if they didn't pick one, or they picked one that already is selected)
            var location = new LocationService( new RockContext() ).Get( locationPicker.SelectedValue.AsInteger() );
            if ( location != null )
            {
                if ( !checkinGroupEditor.Locations.Any( a => a.LocationId == location.Id ) )
                {
                    CheckinGroupEditor.LocationGridItem gridItem = new CheckinGroupEditor.LocationGridItem();
                    gridItem.LocationId = location.Id;
                    gridItem.Name = location.Name;
                    gridItem.FullNamePath = location.Name;
                    gridItem.ParentLocationId = location.ParentLocationId;
                    var parentLocation = location.ParentLocation;
                    while ( parentLocation != null )
                    {
                        gridItem.FullNamePath = parentLocation.Name + " > " + gridItem.FullNamePath;
                        parentLocation = parentLocation.ParentLocation;
                    }

                    checkinGroupEditor.Locations.Add( gridItem );
                }
            }

            checkinGroupEditor.Expanded = true;
            ExpandGroupEditorParent( checkinGroupEditor );

            mdLocationPicker.Hide();
        }

        #endregion Location Add/Delete

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            bool hasValidationErrors = false;

            var rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            GroupService groupService = new GroupService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            GroupLocationService groupLocationService = new GroupLocationService( rockContext );

            int parentGroupTypeId = hfParentGroupTypeId.ValueAsInt();

            var groupTypeUIList = new List<GroupType>();

            foreach ( var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>().ToList() )
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType( rockContext );
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

            int binaryFileFieldTypeID = FieldTypeCache.Read( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() ).Id;

            rockContext.WrapTransaction( () =>
            {
                // delete in reverse order to get deepest child items first
                groupsToDelete.Reverse();
                foreach ( var groupToDelete in groupsToDelete )
                {
                    groupService.Delete( groupToDelete );
                }

                // delete in reverse order to get deepest child items first
                groupTypesToDelete.Reverse();
                foreach ( var groupTypeToDelete in groupTypesToDelete )
                {
                    groupTypeService.Delete( groupTypeToDelete );
                }

                rockContext.SaveChanges();

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
                        groupTypeDB.IsSystem = false;
                        groupTypeDB.ShowInNavigation = false;
                        groupTypeDB.ShowInGroupList = false;
                        groupTypeDB.TakesAttendance = true;
                        groupTypeDB.AttendanceRule = AttendanceRule.None;
                        groupTypeDB.AttendancePrintTo = PrintTo.Default;
                        groupTypeDB.AllowMultipleLocations = true;
                        groupTypeDB.EnableLocationSchedules = true;

                        GroupTypeRole defaultRole = new GroupTypeRole();
                        defaultRole.Name = "Member";
                        groupTypeDB.Roles.Add( defaultRole );
                    }

                    groupTypeDB.Name = groupTypeUI.Name;
                    groupTypeDB.Order = groupTypeUI.Order;
                    groupTypeDB.InheritedGroupTypeId = groupTypeUI.InheritedGroupTypeId;

                    groupTypeDB.Attributes = groupTypeUI.Attributes;
                    groupTypeDB.AttributeValues = groupTypeUI.AttributeValues;

                    if ( groupTypeDB.Id == 0 )
                    {
                        groupTypeService.Add( groupTypeDB );
                    }

                    if ( !groupTypeDB.IsValid )
                    {
                        hasValidationErrors = true;
                        CheckinGroupTypeEditor groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().First( a => a.GroupTypeGuid == groupTypeDB.Guid );
                        groupTypeEditor.Expanded = true;

                        return;
                    }

                    rockContext.SaveChanges();

                    groupTypeDB.SaveAttributeValues( rockContext );

                    // get fresh from database to make sure we have Id so we can update the CheckinLabel Attributes
                    groupTypeDB = groupTypeService.Get( groupTypeDB.Guid );

                    // rebuild the CheckinLabel attributes from the UI (brute-force)
                    foreach ( var labelAttributeDB in CheckinGroupTypeEditor.GetCheckinLabelAttributes( groupTypeDB.Attributes, rockContext ) )
                    {
                        var attribute = attributeService.Get( labelAttributeDB.Value.Guid );
                        Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                        attributeService.Delete( attribute );
                    }

                    // Make sure default role is set
                    if ( !groupTypeDB.DefaultGroupRoleId.HasValue && groupTypeDB.Roles.Any() )
                    {
                        groupTypeDB.DefaultGroupRoleId = groupTypeDB.Roles.First().Id;
                    }

                    rockContext.SaveChanges();

                    foreach ( var checkinLabelAttributeInfo in GroupTypeCheckinLabelAttributesState[groupTypeUI.Guid] )
                    {
                        var attribute = new Rock.Model.Attribute();
                        attribute.AttributeQualifiers.Add( new AttributeQualifier { Key = "binaryFileType", Value = Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL } );
                        attribute.Guid = Guid.NewGuid();
                        attribute.FieldTypeId = binaryFileFieldTypeID;
                        attribute.EntityTypeId = EntityTypeCache.GetId( typeof( GroupType ) );
                        attribute.EntityTypeQualifierColumn = "Id";
                        attribute.EntityTypeQualifierValue = groupTypeDB.Id.ToString();
                        attribute.DefaultValue = checkinLabelAttributeInfo.BinaryFileGuid.ToString();
                        attribute.Key = checkinLabelAttributeInfo.AttributeKey;
                        attribute.Name = checkinLabelAttributeInfo.FileName;

                        if ( !attribute.IsValid )
                        {
                            hasValidationErrors = true;
                            CheckinGroupTypeEditor groupTypeEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupTypeEditor>().First( a => a.GroupTypeGuid == groupTypeDB.Guid );
                            groupTypeEditor.Expanded = true;

                            return;
                        }

                        attributeService.Add( attribute );
                    }

                    rockContext.SaveChanges();
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
                            groupLocationService.Delete( groupLocationDB );
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
                    var parentGroupUI = groupsToAddUpdate.Where( g => g.Groups.Any( g2 => g2.Guid.Equals( groupUI.Guid ) ) ).FirstOrDefault();
                    if ( parentGroupUI != null )
                    {
                        groupDB.ParentGroupId = groupService.Get( parentGroupUI.Guid ).Id;
                    }
                    groupDB.Attributes = groupUI.Attributes;
                    groupDB.AttributeValues = groupUI.AttributeValues;

                    if ( groupDB.Id == 0 )
                    {
                        groupService.Add( groupDB );
                    }

                    if ( !groupDB.IsValid )
                    {
                        hasValidationErrors = true;
                        hasValidationErrors = true;
                        CheckinGroupEditor groupEditor = phCheckinGroupTypes.ControlsOfTypeRecursive<CheckinGroupEditor>().First( a => a.GroupGuid == groupDB.Guid );
                        groupEditor.Expanded = true;

                        return;
                    }

                    rockContext.SaveChanges();

                    groupDB.SaveAttributeValues();
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

                rockContext.SaveChanges();

                // loop thru all the other GroupTypes in the UI and save their childgrouptypes
                foreach ( var groupTypeUI in groupTypesToAddUpdate )
                {
                    var groupTypeDB = groupTypeService.Get( groupTypeUI.Guid );
                    groupTypeDB.ChildGroupTypes = new List<GroupType>();
                    groupTypeDB.ChildGroupTypes.Clear();
                    groupTypeDB.ChildGroupTypes.Add( groupTypeDB );
                    foreach ( var childGroupTypeUI in groupTypeUI.ChildGroupTypes )
                    {
                        var childGroupTypeDB = groupTypeService.Get( childGroupTypeUI.Guid );
                        groupTypeDB.ChildGroupTypes.Add( childGroupTypeDB );
                    }
                }

                rockContext.SaveChanges();
            } );

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
            foreach ( var childGroupTypeDB in groupTypeDB.ChildGroupTypes.Where( g => g.Id != groupTypeDB.Id ) )
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
                    PopulateGroupList( groupUI, groupsToAddUpdate );
                }
            }
        }

        private static void PopulateGroupList( Group groupUI, List<Group> groupsToAddUpdate )
        {
            int groupSortOrder = 0;
            foreach ( var childGroupUI in groupUI.Groups )
            {
                childGroupUI.Order = groupSortOrder++;
                groupsToAddUpdate.Add( childGroupUI );

                PopulateGroupList( childGroupUI, groupsToAddUpdate );
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
        /// <param name="groupTypeId">The group type identifier.</param>
        public void ShowDetail( int groupTypeId )
        {
            // hide the details panel until we verify the page params are good and that the user has edit access
            pnlDetails.Visible = false;

            var rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            GroupType parentGroupType = groupTypeService.Get( groupTypeId );

            if ( parentGroupType == null )
            {
                pnlDetails.Visible = false;
                return;
            }

            lCheckinAreasTitle.Text = parentGroupType.Name.FormatAsHtmlTitle();

            hfParentGroupTypeId.Value = parentGroupType.Id.ToString();

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
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
                CreateGroupTypeEditorControls( groupType, phCheckinGroupTypes, rockContext );
            }
        }

        private void ExpandGroupEditorParent( CheckinGroupEditor groupEditor )
        {
            if ( groupEditor.Parent is CheckinGroupEditor )
            {
                ( (CheckinGroupEditor)groupEditor.Parent ).Expanded = true;
            }
            else if ( groupEditor.Parent is CheckinGroupTypeEditor )
            {
                ( (CheckinGroupTypeEditor)groupEditor.Parent ).Expanded = true;
            }

        }
    }
}