using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.UI;
using Rock;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CheckinConfiguration : RockBlock
    {
        #region ViewState and Dynamic Controls

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var groupTypeList = new List<GroupType>();
            foreach (var checkinGroupTypeEditor in phCheckinGroupTypes.Controls.OfType<CheckinGroupTypeEditor>().ToList())
            {
                var groupType = checkinGroupTypeEditor.GetCheckinGroupType();
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

            //TODO

            parentControl.Controls.Add( groupTypeEditor );

            foreach ( var childGroup in groupType.Groups )
            {
                CreateGroupEditorControls( childGroup, groupTypeEditor, false );
            }

            foreach ( var childGroupType in groupType.ChildGroupTypes )
            {
                CreateGroupTypeEditorControls( childGroupType, groupTypeEditor, false );
            }
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

            CreateGroupEditorControls(checkinGroup, parentGroupTypeEditor);
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
            groupEditor.Group = group;

            //TODO

            parentControl.Controls.Add( groupEditor );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            //TODO
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
    }
}