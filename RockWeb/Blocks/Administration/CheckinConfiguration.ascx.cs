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
        /// <summary>
        /// Handles the Click event of the lbAddCheckinArea control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCheckinArea_Click( object sender, EventArgs e )
        {
            int parentGroupTypeId = this.PageParameter("groupTypeid").AsInteger() ?? 0;
            GroupType parentGroupType = new GroupTypeService().Get(parentGroupTypeId);
            
            // CheckinArea is GroupType entity
            GroupType checkinArea = new GroupType();
            checkinArea.Guid = Guid.NewGuid();
            checkinArea.IsSystem = false;
            checkinArea.TakesAttendance = true;
            checkinArea.AttendanceRule = AttendanceRule.AddOnCheckIn;
            checkinArea.AttendancePrintTo = PrintTo.Default;
            checkinArea.ParentGroupTypes = new List<GroupType>();
            checkinArea.ParentGroupTypes.Add( parentGroupType );

            CreateGroupTypeEditorControls( checkinArea );
        }

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
                CreateGroupTypeEditorControls( groupType, groupType.Guid.Equals( activeGroupTypeGuid ?? Guid.Empty ) );
            }
        }

        /// <summary>
        /// Creates the group type editor controls.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="forceContentVisible">if set to <c>true</c> [force content visible].</param>
        private void CreateGroupTypeEditorControls( GroupType groupType, bool forceContentVisible = false )
        {
            CheckinGroupTypeEditor groupTypeEditor = new CheckinGroupTypeEditor();
            groupTypeEditor.ID = "GroupTypeEditor_" + groupType.Guid.ToString( "N" );
            groupTypeEditor.SetGroupType( groupType );

            //TODO

            phCheckinGroupTypes.Controls.Add( groupTypeEditor );
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {

        }
    }
}