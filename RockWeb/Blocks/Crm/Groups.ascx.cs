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
using Rock.Constants;
using Rock.Crm;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

public partial class Groups : RockBlock
{
    #region Control Methods

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
        {
            gGroups.DataKeyNames = new string[] { "id" };
            gGroups.Actions.IsAddEnabled = true;
            gGroups.Actions.AddClick += gGroups_Add;
            gGroups.GridRebind += gGroups_GridRebind;
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad( EventArgs e )
    {
        nbMessage.Visible = false;

        if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
                
            }
        }
        else
        {
            gGroups.Visible = false;
            nbMessage.Text = WarningMessage.NotAuthorizedToEdit( Group.FriendlyTypeName );
            nbMessage.Visible = true;
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events

    /// <summary>
    /// Handles the Add event of the gGroups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gGroups_Add( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gGroups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroups_Edit( object sender, RowEventArgs e )
    {
        ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Handles the Delete event of the gGroups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroups_Delete( object sender, RowEventArgs e )
    {
        GroupService groupService = new GroupService();
        Group group = groupService.Get( (int)e.RowKeyValue );
        if ( CurrentBlock != null )
        {
            string errorMessage;
            if ( !groupService.CanDelete( group.Id, out errorMessage ) )
            {
                nbGridWarning.Text = errorMessage;
                nbGridWarning.Visible = true;
                return;
            }
            
            groupService.Delete( group, CurrentPersonId );
            groupService.Save( group, CurrentPersonId );
        }

        BindGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gGroups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gGroups_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
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
        pnlDetails.Visible = false;
        pnlList.Visible = true;
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
            group = groupService.Get( groupId );
        }
        
        group.Name = tbName.Text;
        group.Description = tbDescription.Text;
        group.CampusId = ddlCampus.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlCampus.SelectedValue );
        group.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
        group.ParentGroupId = ddlParentGroup.SelectedValue.Equals( None.IdValue ) ? (int?)null : int.Parse( ddlParentGroup.SelectedValue );
        group.IsSecurityRole = cbIsSecurityRole.Checked;
        
        // check for duplicates within GroupType
        if ( groupService.Queryable().Where(g => g.GroupTypeId.Equals(group.GroupTypeId)).Count( a => a.Name.Equals( group.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( group.Id ) ) > 0 )
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

        BindGrid();
        pnlDetails.Visible = false;
        pnlList.Visible = true;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Binds the grid.
    /// </summary>
    private void BindGrid()
    {
        GroupService groupService = new GroupService();
        SortProperty sortProperty = gGroups.SortProperty;

        if ( sortProperty != null )
        {
            gGroups.DataSource = groupService.Queryable().Sort( sortProperty ).ToList();
        }
        else
        {
            gGroups.DataSource = groupService.Queryable().OrderBy( p => p.Name ).ToList();
        }

        gGroups.DataBind();
    }

    /// <summary>
    /// Loads the drop downs.
    /// </summary>
    private void LoadDropDowns()
    {
        GroupTypeService groupTypeService = new GroupTypeService();
        List<GroupType> groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();
        ddlGroupType.DataSource = groupTypes;
        ddlGroupType.DataBind();

        int currentGroupId = int.Parse(hfGroupId.Value);

        // TODO: Only include valid Parent choices (no circular references)
        GroupService groupService = new GroupService();
        List<Group> groups = groupService.Queryable().Where(g => g.Id != currentGroupId).OrderBy( a => a.Name).ToList();
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
    /// Shows the edit.
    /// </summary>
    /// <param name="groupId">The group type id.</param>
    protected void ShowEdit( int groupId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        GroupService groupService = new GroupService();
        Group group = groupService.Get( groupId );
        bool readOnly = false;

        hfGroupId.Value = groupId.ToString();
        LoadDropDowns();

        if ( group != null )
        {
            iconIsSystem.Visible = group.IsSystem;
            hfGroupId.Value = group.Id.ToString();
            tbName.Text = group.Name;
            tbDescription.Text = group.Description;
            ddlGroupType.SelectedValue = group.GroupTypeId.ToString();
            ddlParentGroup.SelectedValue = ( group.ParentGroupId ?? None.Id ).ToString();
            ddlCampus.SelectedValue = ( group.CampusId ?? None.Id ).ToString();
            cbIsSecurityRole.Checked = group.IsSecurityRole;
           
            readOnly = group.IsSystem;

            if ( group.IsSystem )
            {
                lActionTitle.Text = ActionTitle.View( Group.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( Group.FriendlyTypeName );
                btnCancel.Text = "Cancel";
            }
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( Group.FriendlyTypeName );
            iconIsSystem.Visible = false;
            hfGroupId.Value = 0.ToString();
            tbName.Text = string.Empty;
            tbDescription.Text = string.Empty;
            ddlGroupType.SelectedValue = null;
            ddlParentGroup.SelectedValue = None.IdValue;
            ddlCampus.SelectedValue = None.IdValue;
            cbIsSecurityRole.Checked = false;
        }

        ddlGroupType.Enabled = !readOnly;
        ddlParentGroup.Enabled = !readOnly;
        ddlCampus.Enabled = !readOnly;
        cbIsSecurityRole.Enabled = !readOnly;
        
        tbName.ReadOnly = readOnly;
        tbDescription.ReadOnly = readOnly;
        btnSave.Visible = !readOnly;
    }

    #endregion
}
