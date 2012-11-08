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
using Rock.Web.UI;
using Rock.Web.UI.Controls;

/// <summary>
/// 
/// </summary>
public partial class GroupRoles : RockBlock
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
            gGroupRoles.DataKeyNames = new string[] { "id" };
            gGroupRoles.Actions.IsAddEnabled = true;
            gGroupRoles.Actions.AddClick += gGroupRoles_Add;
            gGroupRoles.GridRebind += gGroupRoles_GridRebind;
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
            gGroupRoles.Visible = false;
            nbMessage.Text = WarningMessage.NotAuthorizedToEdit( GroupRole.FriendlyTypeName );
            nbMessage.Visible = true;
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events

    /// <summary>
    /// Handles the Add event of the gGroupRoles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gGroupRoles_Add( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gGroupRoles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroupRoles_Edit( object sender, RowEventArgs e )
    {
        ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Handles the Delete event of the gGroupRoles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroupRoles_Delete( object sender, RowEventArgs e )
    {
        GroupRoleService groupRoleService = new GroupRoleService();
        int groupRoleId = (int)e.RowKeyValue;

        string errorMessage;
        if (!groupRoleService.CanDelete(groupRoleId, out errorMessage))
        {
            nbGridWarning.Text = errorMessage;
            nbGridWarning.Visible = true;
            return;
        }

        GroupRole groupRole = groupRoleService.Get( groupRoleId );
        if ( CurrentBlock != null )
        {
            groupRoleService.Delete( groupRole, CurrentPersonId );
            groupRoleService.Save( groupRole, CurrentPersonId );
        }

        BindGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gGroupRoles control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gGroupRoles_GridRebind( object sender, EventArgs e )
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
        GroupRole groupRole;
        GroupRoleService groupRoleService = new GroupRoleService();

        int groupRoleId = int.Parse( hfGroupRoleId.Value );

        if ( groupRoleId == 0 )
        {
            groupRole = new GroupRole();
            groupRole.IsSystem = false;
            groupRoleService.Add( groupRole, CurrentPersonId );
        }
        else
        {
            groupRole = groupRoleService.Get( groupRoleId );
        }

        groupRole.Name = tbName.Text;
        groupRole.Description = tbDescription.Text;
        groupRole.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
        groupRole.SortOrder = tbSortOrder.TextAsInteger();
        groupRole.MinCount = tbMinCount.TextAsInteger();
        groupRole.MaxCount = tbMaxCount.TextAsInteger();

        // validate Control values
        if ( !tbSortOrder.IsValid )
        {
            return;
        }

        if ( !tbMaxCount.IsValid )
        {
            return;
        }

        if ( !tbMinCount.IsValid )
        {
            return;
        }

        // validate Min/Max count comparison
        if ( groupRole.MinCount != null && groupRole.MaxCount != null )
        {
            if ( groupRole.MinCount > groupRole.MaxCount )
            {
                tbMinCount.ShowErrorMessage( "Min Count cannot be larger than Max Count" );
                return;
            }
        }

        // check for duplicates within GroupType
        if ( groupRoleService.Queryable().Where( g => ( g.GroupTypeId ?? 0 ).Equals( ( groupRole.GroupTypeId ?? 0 ) ) ).Count( a => a.Name.Equals( groupRole.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( groupRole.Id ) ) > 0 )
        {
            tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", Group.FriendlyTypeName ) );
            return;
        }

        if ( !groupRole.IsValid )
        {
            // Controls will render the error messages                    
            return;
        }

        groupRoleService.Save( groupRole, CurrentPersonId );

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
        GroupRoleService groupRoleService = new GroupRoleService();
        SortProperty sortProperty = gGroupRoles.SortProperty;

        if ( sortProperty != null )
        {
            gGroupRoles.DataSource = groupRoleService.Queryable().Sort( sortProperty ).ToList();
        }
        else
        {
            gGroupRoles.DataSource = groupRoleService.Queryable().OrderBy( p => p.Name ).ToList();
        }

        gGroupRoles.DataBind();
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
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="groupRoleId">The group role id.</param>
    protected void ShowEdit( int groupRoleId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        GroupRoleService groupRoleService = new GroupRoleService();
        GroupRole groupRole = groupRoleService.Get( groupRoleId );
        bool readOnly = false;

        hfGroupRoleId.Value = groupRoleId.ToString();
        LoadDropDowns();

        if ( groupRole != null )
        {
            iconIsSystem.Visible = groupRole.IsSystem;
            hfGroupRoleId.Value = groupRole.Id.ToString();
            tbName.Text = groupRole.Name;
            tbDescription.Text = groupRole.Description;
            ddlGroupType.SelectedValue = groupRole.GroupTypeId.ToString();
            tbSortOrder.Text = groupRole.SortOrder != null ? groupRole.SortOrder.ToString() : string.Empty;
            tbMaxCount.Text = groupRole.MaxCount != null ? groupRole.MaxCount.ToString() : string.Empty;
            tbMinCount.Text = groupRole.MinCount != null ? groupRole.MinCount.ToString() : string.Empty;

            readOnly = groupRole.IsSystem;

            if ( groupRole.IsSystem )
            {
                lActionTitle.Text = ActionTitle.View( GroupRole.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( GroupRole.FriendlyTypeName );
                btnCancel.Text = "Cancel";
            }
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( GroupRole.FriendlyTypeName );
            iconIsSystem.Visible = false;
            hfGroupRoleId.Value = 0.ToString();
            tbName.Text = string.Empty;
            tbDescription.Text = string.Empty;
            ddlGroupType.SelectedValue = null;
            tbSortOrder.Text = string.Empty;
            tbMinCount.Text = string.Empty;
            tbMaxCount.Text = string.Empty;
        }

        tbName.ReadOnly = readOnly;
        tbDescription.ReadOnly = readOnly;
        ddlGroupType.Enabled = !readOnly;
        tbSortOrder.ReadOnly = readOnly;
        tbMaxCount.ReadOnly = readOnly;
        tbMinCount.ReadOnly = readOnly;
        
        btnSave.Visible = !readOnly;
    }

    #endregion
}
