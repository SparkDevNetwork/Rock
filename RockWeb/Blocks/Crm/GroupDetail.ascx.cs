//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm
{
    [GroupTypesField( 0, "Group Types", false, "", "", "", "Select group types to show in this block.  Leave all unchecked to show all group types." )]
    [BooleanField( 3, "Show Edit", true )]
    [BooleanField( 6, "Limit to Security Role Groups", false )]
    public partial class GroupDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" ) && AttributeValue( "ShowEdit" ).FromTrueFalse();
            btnSave.Enabled = canAddEditDelete;
            btnSave.ToolTip = canAddEditDelete ? string.Empty : "Not authorized";

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
                string itemId = PageParameter( "groupId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "groupId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Group group;
            GroupService groupService = new GroupService();

            int groupId = int.Parse( hfGroupId.Value );

            if ( groupId == 0 )
            {
                group = new Group();
                group.IsSystem = false;
                groupService.Add( group, CurrentPersonId );
            }
            else
            {
                // just in case this group is or was a SecurityRole
                Rock.Security.Role.Flush( groupId );

                group = groupService.Get( groupId );
            }

            group.Name = tbName.Text;
            group.Description = tbDescription.Text;
            group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
            group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
            group.ParentGroupId = ddlParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlParentGroup.SelectedValue );
            group.IsSecurityRole = cbIsSecurityRole.Checked;

            // check for duplicates within GroupType
            if ( groupService.Queryable().Where( g => g.GroupTypeId.Equals( group.GroupTypeId ) ).Count( a => a.Name.Equals( group.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( group.Id ) ) > 0 )
            {
                tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", Group.FriendlyTypeName ) );
                return;
            }

            if ( !group.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                groupService.Save( group, CurrentPersonId );
            } );

            // just in case this group is or was a SecurityRole
            Rock.Security.Authorization.Flush();

            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns(int currentGroupId)
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            var groupTypeQry = groupTypeService.Queryable();

            // limit GroupType selection to what Block Attributes allow
            List<int> groupTypeIds = AttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
            if ( groupTypeIds.Count > 0 )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeIds.Contains( a.Id ) );
            }

            List<GroupType> groupTypes = groupTypeQry.OrderBy( a => a.Name ).ToList();
            ddlGroupType.DataSource = groupTypes;
            ddlGroupType.DataBind();

            GroupService groupService = new GroupService();
            List<Group> groups = groupService.Queryable().Where( g => g.Id != currentGroupId ).OrderBy( a => a.Name ).ToList();
            groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
            ddlParentGroup.DataSource = groups;
            ddlParentGroup.DataBind();

            CampusService campusService = new CampusService();
            List<Campus> campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList();
            campuses.Insert( 0, new Campus { Id = None.Id, Name = None.Text } );
            ddlCampus.DataSource = campuses;
            ddlCampus.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The group id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "groupId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            Group group = null;

            if (!itemKeyValue.Equals(0))
            {
                group = Group.Read(itemKeyValue);
            }
            else
            {
                group = new Group { Id = 0 };
            }

            LoadDropDowns( group.Id );

            hfGroupId.Value = group.Id.ToString();
            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            ddlGroupType.SetValue(group.GroupTypeId);
            ddlParentGroup.SetValue( group.ParentGroupId );
            ddlCampus.SetValue( group.CampusId );
            cbIsSecurityRole.Checked = group.IsSecurityRole;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if (!IsUserAuthorized("Edit"))
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed;
            }

            if (group.IsSystem)
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem;
            }
            
            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( Group.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( Group.FriendlyTypeName );
                btnCancel.Text = "Cancel";
            }

            ddlGroupType.Enabled = !readOnly;
            ddlParentGroup.Enabled = !readOnly;
            ddlCampus.Enabled = !readOnly;
            cbIsSecurityRole.Enabled = !readOnly;

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;

            // if this block's attribute limit group to SecurityRoleGroups, don't let them edit the SecurityRole checkbox value
            if ( AttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                cbIsSecurityRole.Enabled = false;
                cbIsSecurityRole.Checked = true;
            }
        }

        #endregion
    }
}