//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Core;
using Rock.Crm;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

public partial class GroupTypes : RockBlock
{
    #region Child Grid Dictionarys

    /// <summary>
    /// Gets the child group types dictionary.
    /// </summary>
    /// <returns></returns>
    private Dictionary<int, string> ChildGroupTypesDictionary
    {
        get
        {
            Dictionary<int, string> childGroupTypesDictionary = ViewState["ChildGroupTypesDictionary"] as Dictionary<int, string>;
            return childGroupTypesDictionary;
        }

        set
        {
            ViewState["ChildGroupTypesDictionary"] = value;
        }
    }

    /// <summary>
    /// Gets or sets the location types dictionary.
    /// </summary>
    /// <value>
    /// The location types dictionary.
    /// </value>
    private Dictionary<int, string> LocationTypesDictionary
    {
        get
        {
            Dictionary<int, string> locationTypesDictionary = ViewState["LocationTypesDictionary"] as Dictionary<int, string>;
            return locationTypesDictionary;
        }

        set
        {
            ViewState["LocationTypesDictionary"] = value;
        }
    }

    #endregion

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
            gChildGroupTypes.EmptyDataText = Server.HtmlEncode( None.Text );

            gLocationTypes.DataKeyNames = new string[] { "key" };
            gLocationTypes.Actions.IsAddEnabled = true;
            gLocationTypes.Actions.AddClick += gLocationTypes_Add;
            gLocationTypes.GridRebind += gLocationTypes_GridRebind;
            gLocationTypes.EmptyDataText = Server.HtmlEncode( None.Text );
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad( EventArgs e )
    {
        mdGridWarning.Hide();
        nbWarning.Visible = false;

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
            nbWarning.Text = WarningMessage.NotAuthorizedToEdit( GroupType.FriendlyTypeName );
            nbWarning.Visible = true;
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events (main grid)

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
        ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Handles the Delete event of the gGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gGroupType_Delete( object sender, RowEventArgs e )
    {
        GroupTypeService groupTypeService = new GroupTypeService();
        GroupType groupType = groupTypeService.Get( (int)e.RowKeyValue );

        string errorMessage;
        if ( !groupTypeService.CanDelete( groupType, out errorMessage ) )
        {
            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
            return;
        }

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

    #endregion

    #region Child GroupType Grid and Picker

    /// <summary>
    /// Handles the Add event of the gChildGroupTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gChildGroupTypes_Add( object sender, EventArgs e )
    {
        GroupTypeService groupTypeService = new GroupTypeService();
        int currentGroupTypeId = int.Parse( hfGroupTypeId.Value );

        // populate dropdown with all grouptypes that aren't already childgroups or the current grouptype
        var qry = from gt in groupTypeService.Queryable()
                  where gt.Id != currentGroupTypeId
                  where !( from cgt in ChildGroupTypesDictionary.Keys
                           select cgt ).Contains( gt.Id )
                  select gt;

        List<GroupType> list = qry.ToList();
        if ( list.Count == 0 )
        {
            list.Add( new GroupType { Id = None.Id, Name = None.Text } );
            btnAddChildGroupType.Enabled = false;
            btnAddChildGroupType.CssClass = "btn primary disabled";
        }
        else
        {
            btnAddChildGroupType.Enabled = true;
            btnAddChildGroupType.CssClass = "btn primary";
        }

        ddlChildGroupType.DataSource = list;
        ddlChildGroupType.DataBind();
        pnlChildGroupTypePicker.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Delete event of the gChildGroupTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gChildGroupTypes_Delete( object sender, RowEventArgs e )
    {
        int childGroupTypeId = (int)e.RowKeyValue;
        ChildGroupTypesDictionary.Remove( childGroupTypeId );
        BindChildGroupTypesGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gChildGroupTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gChildGroupTypes_GridRebind( object sender, EventArgs e )
    {
        BindChildGroupTypesGrid();
    }

    /// <summary>
    /// Binds the child group types grid.
    /// </summary>
    private void BindChildGroupTypesGrid()
    {
        gChildGroupTypes.DataSource = ChildGroupTypesDictionary.OrderBy( a => a.Value );
        gChildGroupTypes.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddChildGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddChildGroupType_Click( object sender, EventArgs e )
    {
        ChildGroupTypesDictionary.Add( int.Parse( ddlChildGroupType.SelectedValue ), ddlChildGroupType.SelectedItem.Text );

        pnlChildGroupTypePicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;

        BindChildGroupTypesGrid();
    }

    /// <summary>
    /// Handles the Click event of the btnCancelAddChildGroupType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnCancelAddChildGroupType_Click( object sender, EventArgs e )
    {
        pnlChildGroupTypePicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;
    }

    #endregion

    #region LocationType Grid and Picker

    /// <summary>
    /// Handles the Add event of the gLocationTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gLocationTypes_Add( object sender, EventArgs e )
    {
        DefinedValueService definedValueService = new DefinedValueService();

        // populate dropdown with all locationtypes that aren't already locationtypes
        var qry = from dlt in definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.LOCATION_LOCATION_TYPE )
                  where !( from lt in LocationTypesDictionary.Keys
                           select lt ).Contains( dlt.Id )
                  select dlt;

        List<DefinedValue> list = qry.ToList();
        if ( list.Count == 0 )
        {
            list.Add( new DefinedValue { Id = None.Id, Name = None.Text } );
            btnAddLocationType.Enabled = false;
            btnAddLocationType.CssClass = "btn primary disabled";
        }
        else
        {
            btnAddLocationType.Enabled = true;
            btnAddLocationType.CssClass = "btn primary";
        }

        ddlLocationType.DataSource = list;
        ddlLocationType.DataBind();
        pnlLocationTypePicker.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Delete event of the gLocationTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gLocationTypes_Delete( object sender, RowEventArgs e )
    {
        int locationTypeId = (int)e.RowKeyValue;
        LocationTypesDictionary.Remove( locationTypeId );
        BindLocationTypesGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gLocationTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gLocationTypes_GridRebind( object sender, EventArgs e )
    {
        BindLocationTypesGrid();
    }

    /// <summary>
    /// Binds the location types grid.
    /// </summary>
    private void BindLocationTypesGrid()
    {
        gLocationTypes.DataSource = LocationTypesDictionary.OrderBy( a => a.Value );
        gLocationTypes.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddLocationType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddLocationType_Click( object sender, EventArgs e )
    {
        LocationTypesDictionary.Add( int.Parse( ddlLocationType.SelectedValue ), ddlLocationType.SelectedItem.Text );

        pnlLocationTypePicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;

        BindLocationTypesGrid();
    }

    /// <summary>
    /// Handles the Click event of the btnCancelAddLocationType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnCancelAddLocationType_Click( object sender, EventArgs e )
    {
        pnlLocationTypePicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;
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

        groupType.ChildGroupTypes = new List<GroupType>();
        groupType.ChildGroupTypes.Clear();
        foreach ( var item in ChildGroupTypesDictionary )
        {
            var childGroupType = groupTypeService.Get( item.Key );
            if ( childGroupType != null )
            {
                groupType.ChildGroupTypes.Add( childGroupType );
            }
        }

        DefinedValueService definedValueService = new DefinedValueService();

        groupType.LocationTypes = new List<GroupTypeLocationType>();
        groupType.LocationTypes.Clear();
        foreach ( var item in LocationTypesDictionary )
        {
            var locationType = definedValueService.Get( item.Key );
            if ( locationType != null )
            {
                groupType.LocationTypes.Add( new GroupTypeLocationType { LocationTypeId = locationType.Id } );
            }
        }

        // check for duplicates
        if ( groupTypeService.Queryable().Count( a => a.Name.Equals( groupType.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( groupType.Id ) ) > 0 )
        {
            tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", GroupType.FriendlyTypeName ) );
            return;
        }

        if ( !groupType.IsValid )
        {
            // Controls will render the error messages                    
            return;
        }

        RockTransactionScope.WrapTransaction( () =>
            {
                groupTypeService.Save( groupType, CurrentPersonId );
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
        ChildGroupTypesDictionary = new Dictionary<int, string>();
        LocationTypesDictionary = new Dictionary<int, string>();

        if ( groupType != null )
        {
            hfGroupTypeId.Value = groupType.Id.ToString();
            tbName.Text = groupType.Name;
            tbDescription.Text = groupType.Description;
            ddlDefaultGroupRole.SelectedValue = ( groupType.DefaultGroupRoleId ?? None.Id ).ToString();
            groupType.ChildGroupTypes.ToList().ForEach( a => ChildGroupTypesDictionary.Add( a.Id, a.Name ) );
            groupType.LocationTypes.ToList().ForEach( a => LocationTypesDictionary.Add( a.LocationTypeId, a.LocationType.Name ) );
            readOnly = groupType.IsSystem;

            if ( groupType.IsSystem )
            {
                lActionTitle.Text = ActionTitle.View( GroupType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( GroupType.FriendlyTypeName );
                btnCancel.Text = "Cancel";
            }
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( GroupType.FriendlyTypeName );

            hfGroupTypeId.Value = 0.ToString();
            tbName.Text = string.Empty;
            tbDescription.Text = string.Empty;
        }

        iconIsSystem.Visible = readOnly;
        ddlDefaultGroupRole.Enabled = !readOnly;
        tbName.ReadOnly = readOnly;
        tbDescription.ReadOnly = readOnly;
        btnSave.Visible = !readOnly;

        BindChildGroupTypesGrid();
        BindLocationTypesGrid();
    }

    #endregion
}
