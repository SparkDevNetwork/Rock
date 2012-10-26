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

public partial class GroupTypes : RockBlock
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
            gGroupType.DataKeyNames = new string[] { "id" };
            gGroupType.Actions.IsAddEnabled = true;
            gGroupType.Actions.AddClick += gGroupType_Add;
            gGroupType.GridRebind += gGroupType_GridRebind;

            gChildGroupTypes.DataKeyNames = new string[] { "key" };
            gChildGroupTypes.Actions.IsAddEnabled = true;
            gChildGroupTypes.Actions.AddClick += gChildGroupTypes_Add;
            gChildGroupTypes.GridRebind += gChildGroupTypes_GridRebind;
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
                LoadDropDowns();
            }
        }
        else
        {
            gGroupType.Visible = false;
            nbMessage.Text = WarningMessage.NotAuthorizedToEdit( GroupType.EntityTypeFriendlyName );
            nbMessage.Visible = true;
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events

    /// <summary>
    /// Handles the Add event of the gGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gGroupType_Add( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroupType_Edit( object sender, RowEventArgs e )
    {
        ShowEdit( (int)gGroupType.DataKeys[e.RowIndex]["id"] );
    }

    /// <summary>
    /// Handles the Delete event of the gGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroupType_Delete( object sender, RowEventArgs e )
    {
        GroupTypeService groupTypeService = new GroupTypeService();
        GroupType groupType = groupTypeService.Get( (int)gGroupType.DataKeys[e.RowIndex]["id"] );
        if ( CurrentBlock != null )
        {
            groupTypeService.Delete( groupType, CurrentPersonId );
            groupTypeService.Save( groupType, CurrentPersonId );
        }

        BindGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gGroupType_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
    }


    protected void gChildGroupTypes_Add( object sender, EventArgs e )
    {
        throw new NotImplementedException();
    }

    protected void gChildGroupTypes_Delete( object sender, RowEventArgs e )
    {
        throw new NotImplementedException();
    }

    protected void gChildGroupTypes_GridRebind( object sender, EventArgs e )
    {
        BindChildGroupTypesGrid();
    }

    /// <summary>
    /// Binds the child group types grid.
    /// </summary>
    private void BindChildGroupTypesGrid()
    {
        gChildGroupTypes.DataSource = ViewState["ChildGroupTypes"];
        gChildGroupTypes.DataBind();
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
        GroupType groupType;
        GroupTypeService groupTypeService = new GroupTypeService();

        int groupTypeId = int.Parse( hfGroupTypeId.Value );

        if ( groupTypeId == 0 )
        {
            groupType = new GroupType();
            groupTypeService.Add( groupType, CurrentPersonId );
        }
        else
        {
            groupType = groupTypeService.Get( groupTypeId );
        }

        groupType.Name = tbName.Text;
        if ( ddlDefaultGroupRole.SelectedValue.Equals( None.Id.ToString() ) )
        {
            groupType.DefaultGroupRoleId = null;
        }
        else
        {
            groupType.DefaultGroupRoleId = int.Parse( ddlDefaultGroupRole.SelectedValue );
        }

        groupType.Description = tbDescription.Text;

        // check for duplicates
        if ( groupTypeService.Queryable().Count( a => a.Name.Equals( groupType.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( groupType.Id ) ) > 0 )
        {
            nbMessage.Text = WarningMessage.DuplicateFoundMessage( "name", GroupType.EntityTypeFriendlyName );
            nbMessage.Visible = true;
            return;
        }

        if ( !groupType.IsValid )
        {
            // Controls will render the error messages                    
            return;
        }

        groupTypeService.Save( groupType, CurrentPersonId );

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
        GroupTypeService groupTypeService = new GroupTypeService();
        SortProperty sortProperty = gGroupType.SortProperty;

        if ( sortProperty != null )
        {
            gGroupType.DataSource = groupTypeService.Queryable().Sort( sortProperty ).ToList();
        }
        else
        {
            gGroupType.DataSource = groupTypeService.Queryable().OrderBy( p => p.Name ).ToList();
        }

        gGroupType.DataBind();
    }

    /// <summary>
    /// Loads the drop downs.
    /// </summary>
    private void LoadDropDowns()
    {
        GroupRoleService groupRoleService = new GroupRoleService();
        List<GroupRole> groupRoles = groupRoleService.Queryable().OrderBy( a => a.Name ).ToList();
        groupRoles.Insert( 0, new GroupRole { Id = None.Id, Name = None.Text } );
        ddlDefaultGroupRole.DataSource = groupRoles;
        ddlDefaultGroupRole.DataBind();
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="groupTypeId">The group type id.</param>
    protected void ShowEdit( int groupTypeId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        GroupTypeService groupTypeService = new GroupTypeService();
        GroupType groupType = groupTypeService.Get( groupTypeId );
        bool readOnly = false;

        if ( groupType != null )
        {
            hfGroupTypeId.Value = groupType.Id.ToString();
            tbName.Text = groupType.Name;
            tbDescription.Text = groupType.Description;
            ddlDefaultGroupRole.SelectedValue = ( groupType.DefaultGroupRoleId ?? None.Id ).ToString();
            var list = new Dictionary<int, string>();
            groupType.ChildGroupTypes.ToList().ForEach( a => list.Add( a.Id, a.Name ) );
            ViewState["ChildGroupTypes"] = list;
            readOnly = groupType.IsSystem;

            if ( groupType.IsSystem )
            {
                lActionTitle.Text = ActionTitle.View( GroupType.EntityTypeFriendlyName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( GroupType.EntityTypeFriendlyName );
                btnCancel.Text = "Cancel";
            }
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( GroupType.EntityTypeFriendlyName );

            hfGroupTypeId.Value = 0.ToString();
            tbName.Text = string.Empty;
            tbDescription.Text = string.Empty;
            ViewState["ChildGroupTypes"] = new Dictionary<int, string>();
        }

        iconIsSystem.Visible = readOnly;
        ddlDefaultGroupRole.Enabled = !readOnly;
        tbName.ReadOnly = readOnly;
        tbDescription.ReadOnly = readOnly;
        btnSave.Visible = !readOnly;

        BindChildGroupTypesGrid();
    }

    #endregion
}
