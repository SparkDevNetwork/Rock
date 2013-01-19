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
using Rock.Web.UI.Controls;

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

            gGroupMembers.Actions.AddClick += gGroupMembers_AddClick;
            gGroupMembers.Actions.IsAddEnabled = true;
            gGroupMembers.IsDeleteEnabled = true;
            gGroupMembers.GridRebind += gGroupMembers_GridRebind;
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

            if ( pnlDetails.Visible )
            {
                var group = new Group { GroupTypeId = ddlGroupType.SelectedValueAsInt() ?? 0 };
                if ( group.GroupTypeId > 0 )
                {
                    group.LoadAttributes();
                    phGroupAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, false );
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
            if ( hfGroupId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupService groupService = new GroupService();
                Group group = groupService.Get(int.Parse(hfGroupId.Value));
                ShowReadonlyDetails(group);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            GroupService groupService = new GroupService();
            Group group = groupService.Get( int.Parse( hfGroupId.Value ) );
            ShowEditDetails( group );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            pnlGroupMembers.Disabled = editable;
            gGroupMembers.Enabled = !editable;
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
            bool wasSecurityRole = false;

            if ( groupId == 0 )
            {
                group = new Group();
                group.IsSystem = false;
                group.Name = string.Empty;
            }
            else
            {
                group = groupService.Get( groupId );
                wasSecurityRole = group.IsSecurityRole;
            }

            bool updateTreeViewNodeOnSave = false;
            if ( !group.Name.Equals( tbName.Text ) )
            {
                updateTreeViewNodeOnSave = true;
                group.Name = tbName.Text;
            }

            group.Description = tbDescription.Text;
            group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
            group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
            group.ParentGroupId = ddlParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlParentGroup.SelectedValue );
            group.IsSecurityRole = cbIsSecurityRole.Checked;
            group.IsActive = cbIsActive.Checked;

            // check for duplicates within GroupType
            if ( groupService.Queryable().Where( g => g.GroupTypeId.Equals( group.GroupTypeId ) ).Count( a => a.Name.Equals( group.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( group.Id ) ) > 0 )
            {
                tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", Group.FriendlyTypeName ) );
                return;
            }

            group.LoadAttributes();
            
            Rock.Attribute.Helper.GetEditValues( phGroupAttributes, group );
            Rock.Attribute.Helper.SetErrorIndicators( phGroupAttributes, group );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !group.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {

                if ( group.Id.Equals( 0 ) )
                {
                    groupService.Add( group, CurrentPersonId );
                }

                groupService.Save( group, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( group, CurrentPersonId );

                if ( updateTreeViewNodeOnSave )
                {
                    //TODO
                }
            } );

            if ( wasSecurityRole )
            {
                if ( !group.IsSecurityRole )
                {
                    // if this group was a SecurityRole, but no longer is, flush
                    Rock.Security.Role.Flush( groupId );
                    Rock.Security.Authorization.Flush();
                }
            }
            else
            {
                if ( group.IsSecurityRole )
                {
                    // new security role, flush
                    Rock.Security.Authorization.Flush();
                }
            }

            // reload group from db using a new context
            group = new GroupService().Get( group.Id );
            ShowReadonlyDetails( group );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns( int currentGroupId )
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            var groupTypeQry = groupTypeService.Queryable();

            // limit GroupType selection to what Block Attributes allow
            List<int> groupTypeIds = GetAttributeValue( "GroupTypes" ).SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();
            if ( groupTypeIds.Count > 0 )
            {
                groupTypeQry = groupTypeQry.Where( a => groupTypeIds.Contains( a.Id ) );
            }

            List<GroupType> groupTypes = groupTypeQry.OrderBy( a => a.Name ).ToList();
            ddlGroupType.DataSource = groupTypes;
            ddlGroupType.DataBind();

            GroupService groupService = new GroupService();

            // optimization to only fetch Id, Name from db
            var qryGroup = from g in groupService.Queryable()
                           where g.Id != currentGroupId
                           orderby g.Name
                           select new { Id = g.Id, Name = g.Name };
            List<Group> groups = qryGroup.ToList().ConvertAll<Group>( a => new Group { Id = a.Id, Name = a.Name } );

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

            if ( !itemKeyValue.Equals( 0 ) )
            {
                group = new GroupService().Get( itemKeyValue );
            }
            else
            {
                group = new Group { Id = 0, IsActive = true };
            }
            hfGroupId.Value = group.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Group.FriendlyTypeName );
            }

            if ( group.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            if ( readOnly )
            {
                
                btnEdit.Visible = false;
                ShowReadonlyDetails( group );
            }
            else
            {
                btnEdit.Visible = true;
                if ( group.Id > 0 )
                {
                    ShowReadonlyDetails( group );
                }
                else
                {
                    ShowEditDetails( group );
                }
            }

            BindGroupMembersGrid();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( Group group )
        {
            if ( group.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( Group.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( Group.FriendlyTypeName );
            }
            
            SetEditMode( true );
            
            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            cbIsSecurityRole.Checked = group.IsSecurityRole;
            cbIsActive.Checked = group.IsActive;

            LoadDropDowns( group.Id );
            ddlGroupType.SetValue( group.GroupTypeId );
            ddlParentGroup.SetValue( group.ParentGroupId );

            ddlParentGroup.LabelText = "Parent Group";
            ddlCampus.SetValue( group.CampusId );

            phGroupTypeAttributes.Controls.Clear();
            GroupType groupType = new GroupTypeService().Get( group.GroupTypeId );
            if ( groupType != null )
            {
                groupType.LoadAttributes();
                Rock.Attribute.Helper.AddDisplayControls( groupType, phGroupTypeAttributes );
            }
            
            phGroupAttributes.Controls.Clear();
            group.LoadAttributes();
            Rock.Attribute.Helper.AddEditControls( group, phGroupAttributes, true );

            // if this block's attribute limit group to SecurityRoleGroups, don't let them edit the SecurityRole checkbox value
            if ( GetAttributeValue( "LimittoSecurityRoleGroups" ).FromTrueFalse() )
            {
                cbIsSecurityRole.Enabled = false;
                cbIsSecurityRole.Checked = true;
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Group group )
        {
            SetEditMode( false );

            string groupIconHtml = string.Empty;
            if (!string.IsNullOrWhiteSpace(group.GroupType.IconCssClass))
            {
                groupIconHtml = string.Format( "<i class='{0} icon-large' ></i>", group.GroupType.IconCssClass );
            }
            else
            {
                var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
                string imageUrlFormat = "<img src='" + appPath + "/Image.ashx?id={0}&width=50&height=50' />";
                if ( group.GroupType.IconLargeFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconLargeFileId );
                }
                else if ( group.GroupType.IconSmallFileId != null )
                {
                    groupIconHtml = string.Format( imageUrlFormat, group.GroupType.IconSmallFileId );
                }
            }

            hfGroupId.SetValue( group.Id );
            lGroupIconHtml.Text = groupIconHtml;
            lReadOnlyTitle.Text = group.Name;
            string activeHtmlFormat = "<span class='label {0} pull-right' >{1}</span>";
            if ( group.IsActive )
            {
                lblActiveHtml.Text = string.Format(activeHtmlFormat, "label-success", "Active");
            }
            else
            {
                lblActiveHtml.Text = string.Format(activeHtmlFormat, "label-important", "Inactive");
            }
            
            
            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div class='span5'>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Group Type", group.GroupType.Name );
            
            lblMainDetails.Text += string.Format( descriptionFormat, "Group Description", group.Description );
            if ( group.ParentGroup != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, group.ParentGroup.GroupType.Name, group.ParentGroup.Name );
            }
            
            lblMainDetails.Text += @"
    </dl>
</div>
<div class='span4'>
    <dl>";

            if ( group.Campus != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Campus", group.Campus == null ? None.TextHtml : group.Campus.Name );
            }

            lblMainDetails.Text += @"
    </dl>
</div>";
            
            GroupType groupType = new GroupTypeService().Get( group.GroupTypeId );
            groupType.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( groupType, phGroupTypeAttributesReadOnly );

            group.LoadAttributes();
            Rock.Attribute.Helper.AddDisplayControls( group, phGroupAttributesReadOnly );
        }

        #endregion

        #region GroupMembers Grid

        /// <summary>
        /// Handles the Click event of the DeleteGroupMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteGroupMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            // todo
            throw new NotImplementedException();
        }

        void gGroupMembers_AddClick( object sender, EventArgs e )
        {
            // todo
            throw new NotImplementedException();
        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindGroupMembersGrid()
        {
            GroupMemberService groupMemberService = new GroupMemberService();
            int groupId = int.Parse( hfGroupId.Value );

            var qry = groupMemberService.Queryable().Where( a => a.GroupId.Equals( groupId ) );

            SortProperty sortProperty = gGroupMembers.SortProperty;

            if (sortProperty != null)
            {
                gGroupMembers.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gGroupMembers.DataSource = qry.OrderBy( a => a.Person.LastName ).ThenBy( a => a.Person.NickName ).ThenBy( a => a.Person.GivenName).ToList();
            }

            gGroupMembers.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void gGroupMembers_GridRebind( object sender, EventArgs e )
        {
            BindGroupMembersGrid();
        }

        #endregion
    }
}