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
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Web.UI;
using Rock;
using Rock.Cms;
using Rock.Web.UI.Controls;

public partial class MarketingCampaignAdTypes : RockBlock
{
    #region Child Grid Dictionarys

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
            gMarketingCampaignAdType.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAdType.Actions.IsAddEnabled = true;
            gMarketingCampaignAdType.Actions.AddClick += gMarketingCampaignAdType_Add;
            gMarketingCampaignAdType.GridRebind += gMarketingCampaignAdType_GridRebind;

            gMarketingCampaignAdAttributeTypes.DataKeyNames = new string[] { "key" };
            gMarketingCampaignAdAttributeTypes.Actions.IsAddEnabled = true;
            gMarketingCampaignAdAttributeTypes.Actions.AddClick += gMarketingCampaignAdAttributeType_Add;
            gMarketingCampaignAdAttributeTypes.GridRebind += gMarketingCampaignAdAttributeType_GridRebind;
            gMarketingCampaignAdAttributeTypes.EmptyDataText = Server.HtmlEncode( None.Text );
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad( EventArgs e )
    {
        nbGridWarning.Visible = false;
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
            gMarketingCampaignAdType.Visible = false;
            nbWarning.Text = WarningMessage.NotAuthorizedToEdit( MarketingCampaignAdType.FriendlyTypeName );
            nbWarning.Visible = true;
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events (main grid)

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAdType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAdType_Add( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gMarketingCampaignAdType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAdType_Edit( object sender, RowEventArgs e )
    {
        ShowEdit( (int)gMarketingCampaignAdType.DataKeys[e.RowIndex]["id"] );
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaignAdType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAdType_Delete( object sender, RowEventArgs e )
    {
        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
        int marketingCampaignAdTypeId = (int)gMarketingCampaignAdType.DataKeys[e.RowIndex]["id"];

        /*
        string errorMessage;
        if ( !MarketingCampaignAdTypeService.CanDelete( MarketingCampaignAdTypeId, out errorMessage ) )
        {
            nbGridWarning.Text = errorMessage;
            nbGridWarning.Visible = true;
            return;
        }
        */

        MarketingCampaignAdType marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdTypeId );
        if ( CurrentBlock != null )
        {
            marketingCampaignAdTypeService.Delete( marketingCampaignAdType, CurrentPersonId );
            marketingCampaignAdTypeService.Save( marketingCampaignAdType, CurrentPersonId );
        }

        BindGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gMarketingCampaignAdType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gMarketingCampaignAdType_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
    }

    #endregion

    #region AttributeTypes Grid and Picker

    // TODO
    protected void gMarketingCampaignAdAttributeType_Add( object sender, EventArgs e )
    {
    }

    protected void gMarketingCampaignAdAttributeType_Delete( object sender, RowEventArgs e )
    {
    }

    protected void gMarketingCampaignAdAttributeType_GridRebind( object sender, EventArgs e )
    {
    }

    private void BindMarketingCampaignAdAttributeTypeGrid()
    {
        //
        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();

        int marketingCampaignAdTypeId = int.Parse( hfMarketingCampaignAdTypeId.Value );
        MarketingCampaignAdType marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdTypeId );
        //marketingCampaignAdType.Attributes

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
        MarketingCampaignAdType marketingCampaignAdType;
        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();

        int marketingCampaignAdTypeId = int.Parse( hfMarketingCampaignAdTypeId.Value );

        if ( marketingCampaignAdTypeId == 0 )
        {
            marketingCampaignAdType = new MarketingCampaignAdType();
            marketingCampaignAdTypeService.Add( marketingCampaignAdType, CurrentPersonId );
        }
        else
        {
            marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdTypeId );
        }

        marketingCampaignAdType.Name = tbName.Text;
        marketingCampaignAdType.DateRangeType = (DateRangeTypeEnum)int.Parse( ddlDateRangeType.SelectedValue );

        // check for duplicates
        if ( marketingCampaignAdTypeService.Queryable().Count( a => a.Name.Equals( marketingCampaignAdType.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( marketingCampaignAdType.Id ) ) > 0 )
        {
            tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", MarketingCampaignAdType.FriendlyTypeName ) );
            return;
        }

        if ( !marketingCampaignAdType.IsValid )
        {
            // Controls will render the error messages                    
            return;
        }

        marketingCampaignAdTypeService.Save( marketingCampaignAdType, CurrentPersonId );

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
        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
        SortProperty sortProperty = gMarketingCampaignAdType.SortProperty;

        if ( sortProperty != null )
        {
            gMarketingCampaignAdType.DataSource = marketingCampaignAdTypeService.Queryable().Sort( sortProperty ).ToList();
        }
        else
        {
            gMarketingCampaignAdType.DataSource = marketingCampaignAdTypeService.Queryable().OrderBy( p => p.Name ).ToList();
        }

        gMarketingCampaignAdType.DataBind();
    }

    /// <summary>
    /// Loads the drop downs.
    /// </summary>
    private void LoadDropDowns()
    {
        ddlDateRangeType.Items.Clear();

        foreach ( DateRangeTypeEnum dateRangeType in Enum.GetValues( typeof( DateRangeTypeEnum ) ) )
        {
            ddlDateRangeType.Items.Add( new ListItem( dateRangeType.ConvertToString().SplitCase(), ( (int)dateRangeType ).ToString() ) );
        }
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="marketingCampaignAdTypeId">The marketing campaign ad type id.</param>
    protected void ShowEdit( int marketingCampaignAdTypeId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
        MarketingCampaignAdType marketingCampaignAdType = marketingCampaignAdTypeService.Get( marketingCampaignAdTypeId );
        bool readOnly = false;

        if ( marketingCampaignAdType != null )
        {
            hfMarketingCampaignAdTypeId.Value = marketingCampaignAdType.Id.ToString();
            tbName.Text = marketingCampaignAdType.Name;
            ddlDateRangeType.SelectedValue = ( (int)marketingCampaignAdType.DateRangeType ).ToString();
            readOnly = marketingCampaignAdType.IsSystem;

            if ( marketingCampaignAdType.IsSystem )
            {
                lActionTitle.Text = ActionTitle.View( MarketingCampaignAdType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }
            else
            {
                lActionTitle.Text = ActionTitle.Edit( MarketingCampaignAdType.FriendlyTypeName );
                btnCancel.Text = "Cancel";
            }
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( MarketingCampaignAdType.FriendlyTypeName );

            hfMarketingCampaignAdTypeId.Value = 0.ToString();
            tbName.Text = string.Empty;
        }

        iconIsSystem.Visible = readOnly;
        btnSave.Visible = !readOnly;

        BindMarketingCampaignAdAttributeTypeGrid();
    }

    #endregion
}