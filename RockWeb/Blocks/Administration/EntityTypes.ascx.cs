//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Security;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


/// <summary>
/// 
/// </summary>
public partial class EntityTypes : RockBlock
{
    #region Control Methods

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
        {
            gEntityTypes.DataKeyNames = new string[] { "id" };
            gEntityTypes.Actions.IsAddEnabled = true;
            gEntityTypes.Actions.AddClick += Actions_AddClick;
            gEntityTypes.EditRow += gEntityTypes_EditRow;
            gEntityTypes.GridRebind += gEntityTypes_GridRebind;
            gEntityTypes.RowDataBound += gEntityTypes_RowDataBound;


        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad( EventArgs e )
    {
        nbWarning.Visible = false;

        if ( CurrentPage.IsAuthorized( "Administrate", CurrentPerson ) )
        {
            if ( !Page.IsPostBack )
            {
                new EntityTypeService().RegisterEntityTypes( Request.MapPath( "~" ) );

                BindGrid();
            }
        }
        else
        {
            gEntityTypes.Visible = false;
            nbWarning.Text = WarningMessage.NotAuthorizedToEdit( EntityType.FriendlyTypeName );
            nbWarning.Visible = true;
        }

        base.OnLoad( e );
    }

    #endregion

    #region Grid Events (main grid)

    /// <summary>
    /// Handles the EditRow event of the gEntityTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    void gEntityTypes_EditRow( object sender, RowEventArgs e )
    {
        ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Handles the AddClick event of the Actions control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    void Actions_AddClick( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }


    /// <summary>
    /// Handles the GridRebind event of the gEntityTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gEntityTypes_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
    }

    /// <summary>
    /// Handles the RowDataBound event of the gEntityTypes control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
    void gEntityTypes_RowDataBound( object sender, GridViewRowEventArgs e )
    {
        EntityType entityType = e.Row.DataItem as EntityType;
        if ( entityType != null )
        {
            HtmlAnchor aSecure = e.Row.FindControl( "aSecure" ) as HtmlAnchor;
            if ( aSecure != null )
            {
                if ( entityType.IsSecured )
                {
                    aSecure.Visible = true;
                    aSecure.HRef = string.Format( "~/Secure/{0}/{1}?t={2}&pb=&sb=Done",
                        Rock.Security.Authorization.EncodeEntityTypeName( entityType.AssemblyName ), 0, "Security" );
                }
                else
                {
                    aSecure.Visible = false;
                }
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
        pnlDetails.Visible = false;
        pnlList.Visible = true;
        hfEntityTypeId.Value = string.Empty;
    }

    /// <summary>
    /// Handles the Click event of the btnEdit control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnEdit_Click( object sender, EventArgs e )
    {
        pnlDetails.Visible = true;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Click event of the btnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnSave_Click( object sender, EventArgs e )
    {
        EntityTypeService entityTypeService = new EntityTypeService();
        EntityType entityType = entityTypeService.Get( int.Parse( hfEntityTypeId.Value ) );

        entityType.FriendlyName = tbFriendlyName.Text;

        entityTypeService.Save( entityType, CurrentPersonId );

        BindGrid();

        pnlDetails.Visible = false;
        pnlList.Visible = true;

        hfEntityTypeId.Value = string.Empty;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Binds the grid.
    /// </summary>
    private void BindGrid()
    {
        EntityTypeService entityTypeService = new EntityTypeService();
        SortProperty sortProperty = gEntityTypes.SortProperty;

        if ( sortProperty != null )
        {
            gEntityTypes.DataSource = entityTypeService
                .Queryable()
                .Where( e => e.IsSecured || e.IsEntity)
                .Sort( sortProperty ).ToList();
        }
        else
        {
            gEntityTypes.DataSource = entityTypeService
                .Queryable()
                .Where( e => e.IsSecured || e.IsEntity )
                .OrderBy( p => p.Name ).ToList();
        }

        gEntityTypes.DataBind();
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="entityTypeId">The entity type id.</param>
    protected void ShowEdit( int entityTypeId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        EntityTypeService entityTypeService = new EntityTypeService();
        EntityType entityType = entityTypeService.Get( entityTypeId );

        if ( entityType != null )
        {
            lActionTitle.Text = ActionTitle.Edit( EntityType.FriendlyTypeName );
            hfEntityTypeId.Value = entityType.Id.ToString();
            tbName.Text = entityType.Name;
            tbFriendlyName.Text = entityType.FriendlyName;
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( EntityType.FriendlyTypeName );
            hfEntityTypeId.Value = 0.ToString();
            tbName.Text = string.Empty;
            tbFriendlyName.Text = string.Empty;
        }

        tbName.Enabled = !entityType.IsEntity;

    }

    #endregion
}