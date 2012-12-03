//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Cms;
using Rock.Constants;
using Rock.Core;
using Rock.Crm;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

/// <summary>
/// 
/// </summary>
[AdditionalActions( new string[] { "Approve" } )]
public partial class MarketingCampaigns : RockBlock
{
    #region Child Grid States

    /// <summary>
    /// Gets or sets the state of the marketing campaign audiences.
    /// </summary>
    /// <value>
    /// The state of the marketing campaign audiences.
    /// </value>
    private List<MarketingCampaignAudienceDto> MarketingCampaignAudiencesState
    {
        get
        {
            return ViewState["MarketingCampaignAudiencesState"] as List<MarketingCampaignAudienceDto>;
        }

        set
        {
            ViewState["MarketingCampaignAudiencesState"] = value;
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
            gMarketingCampaigns.DataKeyNames = new string[] { "id" };
            gMarketingCampaigns.Actions.IsAddEnabled = true;
            gMarketingCampaigns.Actions.AddClick += gMarketingCampaigns_Add;
            gMarketingCampaigns.GridRebind += gMarketingCampaigns_GridRebind;

            gMarketingCampaignAds.DataKeyNames = new string[] { "Id" };
            gMarketingCampaignAds.Actions.IsAddEnabled = true;
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiencesPrimary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesPrimary.Actions.IsAddEnabled = true;
            gMarketingCampaignAudiencesPrimary.Actions.AddClick += gMarketingCampaignAudiencesPrimary_Add;
            gMarketingCampaignAudiencesPrimary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesPrimary.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiencesSecondary.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiencesSecondary.Actions.IsAddEnabled = true;
            gMarketingCampaignAudiencesSecondary.Actions.AddClick += gMarketingCampaignAudiencesSecondary_Add;
            gMarketingCampaignAudiencesSecondary.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiencesSecondary.EmptyDataText = Server.HtmlEncode( None.Text );

            Rock.Web.UI.RockPage.AddScriptLink( this.Page, "~/scripts/Kendo/kendo.core.min.js" );
        }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
    protected override void OnLoad( EventArgs e )
    {
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
            gMarketingCampaigns.Visible = false;
            nbWarning.Text = WarningMessage.NotAuthorizedToEdit( MarketingCampaign.FriendlyTypeName );
            nbWarning.Visible = true;
        }

        if ( pnlMarketingCampaignAdEditor.Visible )
        {
            LoadAdAttributes( new MarketingCampaignAdDto(), true, false );
        }

        base.OnLoad( e );
    }
    #endregion

    #region Grid Events (main grid)

    /// <summary>
    /// Handles the Add event of the gMarketingCampaigns control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaigns_Add( object sender, EventArgs e )
    {
        ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gMarketingCampaigns control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaigns_Edit( object sender, RowEventArgs e )
    {
        ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaigns control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaigns_Delete( object sender, RowEventArgs e )
    {
        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        int marketingCampaignId = (int)e.RowKeyValue;
        MarketingCampaign marketingCampaign = marketingCampaignService.Get( marketingCampaignId );

        string errorMessage;
        if ( !marketingCampaignService.CanDelete( marketingCampaign, out errorMessage ) )
        {
            mdGridWarning.Show( errorMessage, ModalAlertType.Information );
            return;
        }

        if ( CurrentBlock != null )
        {
            marketingCampaignService.Delete( marketingCampaign, CurrentPersonId );
            marketingCampaignService.Save( marketingCampaign, CurrentPersonId );
        }

        BindGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gMarketingCampaigns control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void gMarketingCampaigns_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
    }

    #endregion

    #region MarketingCampaignAds Grid and Editor

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_Add( object sender, EventArgs e )
    {
        gMarketingCampaignAds_ShowEdit( 0 );
    }

    /// <summary>
    /// Handles the Edit event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_Edit( object sender, RowEventArgs e )
    {
        gMarketingCampaignAds_ShowEdit( (int)e.RowKeyValue );
    }

    /// <summary>
    /// Gs the marketing campaign ads_ show edit.
    /// </summary>
    /// <param name="marketingCampaignAdId">The marketing campaign ad id.</param>
    protected void gMarketingCampaignAds_ShowEdit( int marketingCampaignAdId )
    {
        hfMarketingCampaignAdId.Value = marketingCampaignAdId.ToString();

        btnApproveAd.Visible = IsUserAuthorized( "Approve" );
        btnDenyAd.Visible = IsUserAuthorized("Approve");

        MarketingCampaignAd marketingCampaignAd = null;

        if ( !marketingCampaignAdId.Equals( 0 ) )
        {
            marketingCampaignAd = MarketingCampaignAd.Read( marketingCampaignAdId );
        }
        else
        {
            marketingCampaignAd = new MarketingCampaignAd { Id = 0, MarketingCampaignAdStatus = MarketingCampaignAdStatus.PendingApproval };
        }

        if ( !marketingCampaignAd.Id.Equals( 0 ) )
        {
            lActionTitleAd.Text = ActionTitle.Add( "Marketing Ad for " + tbTitle.Text );
        }
        else
        {
            lActionTitleAd.Text = ActionTitle.Edit( "Marketing Ad for " + tbTitle.Text );
        }

        ddlMarketingCampaignAdType.SetValue( marketingCampaignAd.MarketingCampaignAdTypeId );
        tbPriority.Text = marketingCampaignAd.Priority.ToString();

        SetApprovalValues(marketingCampaignAd.MarketingCampaignAdStatus, Person.Read( marketingCampaignAd.MarketingCampaignStatusPersonId ?? 0 ));

        if ( marketingCampaignAdId.Equals( 0 ) )
        {
            tbAdDateRangeStartDate.SelectedDate = null;
            tbAdDateRangeEndDate.SelectedDate = null;
        }
        else
        {
            tbAdDateRangeStartDate.SelectedDate = marketingCampaignAd.StartDate;
            tbAdDateRangeEndDate.SelectedDate = marketingCampaignAd.EndDate;
        }

        tbUrl.Text = marketingCampaignAd.Url;

        LoadAdAttributes( marketingCampaignAd, true, true );

        pnlMarketingCampaignAdEditor.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlMarketingCampaignAdType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void ddlMarketingCampaignAdType_SelectedIndexChanged( object sender, EventArgs e )
    {
        MarketingCampaignAdDto marketingCampaignAd = new MarketingCampaignAdDto();

        LoadAdAttributes( marketingCampaignAd, false, false );
        Rock.Attribute.Helper.GetEditValues( phAttributes, marketingCampaignAd );

        LoadAdAttributes( marketingCampaignAd, true, true );
    }

    /// <summary>
    /// Loads the attribute controls.
    /// </summary>
    /// <param name="marketingCampaignAd">The marketing campaign ad.</param>
    /// <param name="createControls">if set to <c>true</c> [create controls].</param>
    /// <param name="setValues">if set to <c>true</c> [set values].</param>
    private void LoadAdAttributes( Rock.Attribute.IHasAttributes marketingCampaignAd, bool createControls, bool setValues )
    {
        if ( string.IsNullOrWhiteSpace( ddlMarketingCampaignAdType.SelectedValue ) )
        {
            return;
        }

        int marketingAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );

        MarketingCampaignAdType adType = MarketingCampaignAdType.Read( marketingAdTypeId );
        tbAdDateRangeEndDate.Visible = adType.DateRangeType.Equals( DateRangeTypeEnum.DateRange );

        List<Rock.Core.Attribute> attributesForAdType = GetAttributesForAdType( marketingAdTypeId );

        marketingCampaignAd.Attributes = marketingCampaignAd.Attributes ?? new Dictionary<string, Rock.Web.Cache.AttributeCache>();
        marketingCampaignAd.AttributeCategories = marketingCampaignAd.AttributeCategories ?? new SortedDictionary<string, List<string>>();
        marketingCampaignAd.AttributeValues = marketingCampaignAd.AttributeValues ?? new Dictionary<string, List<AttributeValueDto>>();
        foreach ( var attribute in attributesForAdType )
        {
            marketingCampaignAd.Attributes[attribute.Key] = Rock.Web.Cache.AttributeCache.Read( attribute );
            if ( marketingCampaignAd.AttributeValues.Count( v => v.Key.Equals( attribute.Key ) ) == 0 )
            {
                List<AttributeValueDto> attributeValues = new List<AttributeValueDto>();
                attributeValues.Add( new AttributeValueDto { Value = attribute.DefaultValue } );
                marketingCampaignAd.AttributeValues.Add( attribute.Key, attributeValues );
            }
        }

        foreach ( var category in attributesForAdType.Select( a => a.Category ).Distinct() )
        {
            marketingCampaignAd.AttributeCategories[category] = attributesForAdType.Where( a => a.Category.Equals( category ) ).Select( a => a.Key ).ToList();
        }

        if ( createControls )
        {
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( marketingCampaignAd, phAttributes, setValues );
        }
    }

    /// <summary>
    /// Gets the type of the attributes for ad.
    /// </summary>
    /// <param name="marketingAdTypeId">The marketing ad type id.</param>
    /// <returns></returns>
    private static List<Rock.Core.Attribute> GetAttributesForAdType( int marketingAdTypeId )
    {
        MarketingCampaignAd temp = new MarketingCampaignAd();
        temp.MarketingCampaignAdTypeId = marketingAdTypeId;

        Rock.Attribute.Helper.LoadAttributes( temp );
        List<Rock.Web.Cache.AttributeCache> attribs = temp.Attributes.Values.ToList();

        List<Rock.Core.Attribute> result = new List<Rock.Core.Attribute>();
        foreach ( var item in attribs )
        {
            var attrib = item.ToModel();
            result.Add( attrib );
        }

        return result;
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_Delete( object sender, RowEventArgs e )
    {
        MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
        MarketingCampaignAd marketingCampaignAd = marketingCampaignAdService.Get( (int)e.RowKeyValue );

        marketingCampaignAdService.Delete( marketingCampaignAd, CurrentPersonId );
        marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
        BindMarketingCampaignAdsGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_GridRebind( object sender, EventArgs e )
    {
        BindMarketingCampaignAdsGrid();
    }

    /// <summary>
    /// Binds the marketing campaign ads grid.
    /// </summary>
    private void BindMarketingCampaignAdsGrid()
    {
        MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
        int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
        var qry = marketingCampaignAdService.Queryable().Where( a => a.MarketingCampaignId.Equals( marketingCampaignId ) );

        gMarketingCampaignAds.DataSource = qry.OrderBy( a => a.StartDate ).ThenBy( a => a.Priority ).ThenBy( a => a.MarketingCampaignAdType.Name ).ToList();
        gMarketingCampaignAds.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnSaveAd control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnSaveAd_Click( object sender, EventArgs e )
    {
        int marketingCampaignAdId = int.Parse( hfMarketingCampaignAdId.Value );
        MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();

        MarketingCampaignAd marketingCampaignAd;
        if ( marketingCampaignAdId.Equals( 0 ) )
        {
            marketingCampaignAd = new MarketingCampaignAd { Id = 0 };
        }
        else
        {
            marketingCampaignAd = marketingCampaignAdService.Get( marketingCampaignAdId );
        }

        marketingCampaignAd.MarketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
        marketingCampaignAd.MarketingCampaignAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );
        marketingCampaignAd.Priority = tbPriority.TextAsInteger() ?? 0;
        marketingCampaignAd.MarketingCampaignAdStatus = (MarketingCampaignAdStatus)int.Parse( hfMarketingCampaignAdStatus.Value );
        if ( !string.IsNullOrWhiteSpace( hfMarketingCampaignAdStatusPersonId.Value ) )
        {
            marketingCampaignAd.MarketingCampaignStatusPersonId = int.Parse( hfMarketingCampaignAdStatusPersonId.Value );
        }
        else
        {
            marketingCampaignAd.MarketingCampaignStatusPersonId = null;
        }

        if ( tbAdDateRangeStartDate.SelectedDate == null )
        {
            tbAdDateRangeStartDate.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( "StartDate" ) );
            return;
        }

        marketingCampaignAd.StartDate = tbAdDateRangeStartDate.SelectedDate ?? DateTime.MinValue;

        if ( tbAdDateRangeEndDate.Visible )
        {
            if ( tbAdDateRangeEndDate.SelectedDate == null )
            {
                tbAdDateRangeEndDate.ShowErrorMessage( Rock.Constants.WarningMessage.CannotBeBlank( "EndDate" ) );
                return;
            }

            marketingCampaignAd.EndDate = tbAdDateRangeEndDate.SelectedDate ?? DateTime.MaxValue;
        }
        else
        {
            marketingCampaignAd.EndDate = marketingCampaignAd.StartDate;
        }

        if ( marketingCampaignAd.EndDate < marketingCampaignAd.StartDate )
        {
            tbAdDateRangeStartDate.ShowErrorMessage( WarningMessage.DateRangeEndDateBeforeStartDate() );
        }

        marketingCampaignAd.Url = tbUrl.Text;

        LoadAdAttributes( marketingCampaignAd, false, false );
        Rock.Attribute.Helper.GetEditValues( phAttributes, marketingCampaignAd );
        Rock.Attribute.Helper.SetErrorIndicators( phAttributes, marketingCampaignAd );

        if ( !Page.IsValid )
        {
            return;
        }

        if ( !marketingCampaignAd.IsValid )
        {
            return;
        }

        RockTransactionScope.WrapTransaction( () =>
            {
                if ( marketingCampaignAd.Id.Equals( 0 ) )
                {
                    marketingCampaignAdService.Add( marketingCampaignAd, CurrentPersonId );
                }

                marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
                Rock.Attribute.Helper.SaveAttributeValues( marketingCampaignAd, CurrentPersonId );
            } );

        pnlMarketingCampaignAdEditor.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;

        BindMarketingCampaignAdsGrid();
    }

    /// <summary>
    /// Handles the Click event of the btnCancelAd control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnCancelAd_Click( object sender, EventArgs e )
    {
        pnlMarketingCampaignAdEditor.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Click event of the btnApproveAd control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnApproveAd_Click( object sender, EventArgs e )
    {
        SetApprovalValues( MarketingCampaignAdStatus.Approved, CurrentPerson );
    }

    /// <summary>
    /// Handles the Click event of the btnDenyAd control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnDenyAd_Click( object sender, EventArgs e )
    {
        SetApprovalValues( MarketingCampaignAdStatus.Denied, CurrentPerson );
    }

    /// <summary>
    /// Sets the approval values.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="person">The person.</param>
    private void SetApprovalValues( MarketingCampaignAdStatus status, Person person )
    {
        ltMarketingCampaignAdStatus.Text = status.ConvertToString();
        hfMarketingCampaignAdStatus.Value = status.ConvertToInt().ToString();
        ltMarketingCampaignAdStatusPerson.Visible = person != null;
        if ( person != null )
        {
            ltMarketingCampaignAdStatusPerson.Text = person.FullName;
            hfMarketingCampaignAdStatusPersonId.Value = person.Id.ToString();
        }
    }

    #endregion

    #region MarketingCampaignAudience Grid and Picker

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAudiencesPrimary control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiencesPrimary_Add( object sender, EventArgs e )
    {
        gMarketingCampaignAudiencesAdd( true );
    }

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAudiencesSecondary control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiencesSecondary_Add( object sender, EventArgs e )
    {
        gMarketingCampaignAudiencesAdd( false );
    }

    /// <summary>
    /// Gs the marketing campaign audiences add.
    /// </summary>
    /// <param name="isPrimary">if set to <c>true</c> [is primary].</param>
    private void gMarketingCampaignAudiencesAdd( bool isPrimary )
    {
        DefinedValueService definedValueService = new DefinedValueService();

        // populate dropdown with all MarketingCampaignAudiences that aren't already MarketingCampaignAudiences
        var qry = from audienceTypeValue in definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ).AsQueryable()
                  where !( from mcaudience in MarketingCampaignAudiencesState
                           select mcaudience.AudienceTypeValueId ).Contains( audienceTypeValue.Id )
                  select audienceTypeValue;

        List<DefinedValue> list = qry.ToList();
        if ( list.Count == 0 )
        {
            list.Add( new DefinedValue { Id = None.Id, Name = None.Text } );
            btnAddMarketingCampaignAudience.Enabled = false;
            btnAddMarketingCampaignAudience.CssClass = "btn btn-primary disabled";
        }
        else
        {
            btnAddMarketingCampaignAudience.Enabled = true;
            btnAddMarketingCampaignAudience.CssClass = "btn btn-primary";
        }

        ddlMarketingCampaignAudiences.DataSource = list;
        ddlMarketingCampaignAudiences.DataBind();
        hfMarketingCampaignAudienceIsPrimary.Value = isPrimary.ToTrueFalse();
        pnlMarketingCampaignAudiencePicker.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaignAudiences control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiences_Delete( object sender, RowEventArgs e )
    {
        int marketingCampaignAudienceId = (int)e.RowKeyValue;
        MarketingCampaignAudiencesState.RemoveDto( marketingCampaignAudienceId );
        BindMarketingCampaignAudiencesGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gMarketingCampaignAudiences control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiences_GridRebind( object sender, EventArgs e )
    {
        BindMarketingCampaignAudiencesGrid();
    }

    /// <summary>
    /// Binds the marketing campaign audiences grid.
    /// </summary>
    private void BindMarketingCampaignAudiencesGrid()
    {
        gMarketingCampaignAudiencesPrimary.DataSource = MarketingCampaignAudiencesState.Where( a => a.IsPrimary ).OrderBy( a => a.Name );
        gMarketingCampaignAudiencesPrimary.DataBind();

        gMarketingCampaignAudiencesSecondary.DataSource = MarketingCampaignAudiencesState.Where( a => !a.IsPrimary ).OrderBy( a => a.Name );
        gMarketingCampaignAudiencesSecondary.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddMarketingCampaignAudience control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddMarketingCampaignAudience_Click( object sender, EventArgs e )
    {
        int audienceTypeValueId = int.Parse( ddlMarketingCampaignAudiences.SelectedValue );
        MarketingCampaignAudience marketingCampaignAudience = new MarketingCampaignAudience();
        marketingCampaignAudience.AudienceTypeValueId = audienceTypeValueId;
        marketingCampaignAudience.AudienceTypeValue = DefinedValue.Read( audienceTypeValueId );
        marketingCampaignAudience.IsPrimary = hfMarketingCampaignAudienceIsPrimary.Value.FromTrueFalse();

        MarketingCampaignAudiencesState.Add( new MarketingCampaignAudienceDto( marketingCampaignAudience ) );

        pnlMarketingCampaignAudiencePicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;

        BindMarketingCampaignAudiencesGrid();
    }

    /// <summary>
    /// Handles the Click event of the btnCancelAddMarketingCampaignAudience control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnCancelAddMarketingCampaignAudience_Click( object sender, EventArgs e )
    {
        pnlMarketingCampaignAudiencePicker.Visible = false;
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
        SetEditMode( false );

        if ( hfMarketingCampaignId.Value.Equals( "0" ) )
        {
            // Cancelling on Add. Return to Grid
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }
        else
        {
            // Cancelling on Edit.  Return to Details
            pnlDetails.Visible = true;
            pnlList.Visible = false;
        }
        
    }

    /// <summary>
    /// Handles the Click event of the btnEdit control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnEdit_Click( object sender, EventArgs e )
    {
        SetEditMode( true );
        pnlDetails.Visible = true;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Sets the edit mode.
    /// </summary>
    /// <param name="editable">if set to <c>true</c> [editable].</param>
    private void SetEditMode( bool editable )
    {
        pnlEditDetails.Visible = editable;
        fieldsetViewDetails.Visible = !editable;
        pnlMarketingCampaignAds.Disabled = editable;
        gMarketingCampaignAds.Enabled = !editable;

        btnCancel.Visible = editable;
        btnSave.Visible = editable;
        btnEdit.Visible = !editable;
    }
    
    /// <summary>
    /// Handles the Click event of the btnSave control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnSave_Click( object sender, EventArgs e )
    {
        MarketingCampaign marketingCampaign;
        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();

        int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );

        if ( marketingCampaignId == 0 )
        {
            marketingCampaign = new MarketingCampaign();
            marketingCampaignService.Add( marketingCampaign, CurrentPersonId );
        }
        else
        {
            marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
        }

        marketingCampaign.Title = tbTitle.Text;
        if ( ddlContactPerson.SelectedValue.Equals( None.Id.ToString() ) )
        {
            marketingCampaign.ContactPersonId = null;
        }
        else
        {
            marketingCampaign.ContactPersonId = int.Parse( ddlContactPerson.SelectedValue );
        }

        marketingCampaign.ContactEmail = tbContactEmail.Text;
        marketingCampaign.ContactPhoneNumber = tbContactPhoneNumber.Text;
        marketingCampaign.ContactFullName = tbContactFullName.Text;

        if ( ddlEventGroup.SelectedValue.Equals( None.Id.ToString() ) )
        {
            marketingCampaign.EventGroupId = null;
        }
        else
        {
            marketingCampaign.EventGroupId = int.Parse( ddlEventGroup.SelectedValue );
        }

        // check for duplicates
        if ( marketingCampaignService.Queryable().Count( a => a.Title.Equals( marketingCampaign.Title, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( marketingCampaign.Id ) ) > 0 )
        {
            tbTitle.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "title", MarketingCampaign.FriendlyTypeName ) );
            return;
        }

        if ( !marketingCampaign.IsValid )
        {
            // Controls will render the error messages                    
            return;
        }

        RockTransactionScope.WrapTransaction( () =>
        {
            #region Save MarketingCampaignAudiences to db

            // Update MarketingCampaignAudiences with UI values
            if ( marketingCampaign.MarketingCampaignAudiences == null )
            {
                marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
            }

            // delete Audiences that aren't assigned in the UI anymore
            MarketingCampaignAudienceService marketingCampaignAudienceService = new MarketingCampaignAudienceService();
            var deletedAudiences = from audienceInDB in marketingCampaign.MarketingCampaignAudiences.AsQueryable()
                                   where !( from audienceStateItem in MarketingCampaignAudiencesState
                                            select audienceStateItem.AudienceTypeValueId ).Contains( audienceInDB.AudienceTypeValueId )
                                   select audienceInDB;
            deletedAudiences.ToList().ForEach( a =>
            {
                var aud = marketingCampaignAudienceService.Get( a.Guid );
                marketingCampaignAudienceService.Delete( aud, CurrentPersonId );
                marketingCampaignAudienceService.Save( aud, CurrentPersonId );
            } );

            // add or update the Audiences that are assigned in the UI
            foreach ( var item in MarketingCampaignAudiencesState )
            {
                MarketingCampaignAudience marketingCampaignAudience = marketingCampaign.MarketingCampaignAudiences.FirstOrDefault( a => a.AudienceTypeValueId.Equals( item.AudienceTypeValueId ) );
                if ( marketingCampaignAudience == null )
                {
                    marketingCampaignAudience = new MarketingCampaignAudience();
                    marketingCampaign.MarketingCampaignAudiences.Add( marketingCampaignAudience );
                }

                marketingCampaignAudience.AudienceTypeValueId = item.AudienceTypeValueId;
                marketingCampaignAudience.IsPrimary = item.IsPrimary;
            }

            #endregion

            #region Save MarketingCampaignCampuses to db

            // Update MarketingCampaignCampuses with UI values
            if ( marketingCampaign.MarketingCampaignCampuses == null )
            {
                marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
            }

            // take care of deleted Campuses
            MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService();
            var deletedCampuses = from mcc in marketingCampaign.MarketingCampaignCampuses.AsQueryable()
                                  where !( cpCampuses.SelectedCampusIds ).Contains( mcc.CampusId )
                                  select mcc;

            deletedCampuses.ToList().ForEach( a =>
            {
                var c = marketingCampaignCampusService.Get( a.Guid );
                marketingCampaignCampusService.Delete( c, CurrentPersonId );
                marketingCampaignCampusService.Save( c, CurrentPersonId );
            } );

            // add or update the Campuses that are assigned in the UI
            foreach ( int campusId in cpCampuses.SelectedCampusIds )
            {
                MarketingCampaignCampus marketingCampaignCampus = marketingCampaign.MarketingCampaignCampuses.FirstOrDefault( a => a.CampusId.Equals( campusId ) );
                if ( marketingCampaignCampus == null )
                {
                    marketingCampaignCampus = new MarketingCampaignCampus();
                    marketingCampaign.MarketingCampaignCampuses.Add( marketingCampaignCampus );
                }

                marketingCampaignCampus.CampusId = campusId;
            }

            #endregion

            marketingCampaignService.Save( marketingCampaign, CurrentPersonId );
        } );

        hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();

        // refresh from db using new service context since the above child items were saved outside of the marketingCampaign object
        marketingCampaign = MarketingCampaign.Read( marketingCampaign.Id );
        UpdateReadonlyDetails( marketingCampaign );
        
        SetEditMode( false );
        pnlDetails.Visible = true;
        pnlList.Visible = false;
    }

    #endregion

    #region Internal Methods

    /// <summary>
    /// Binds the grid.
    /// </summary>
    private void BindGrid()
    {
        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        SortProperty sortProperty = gMarketingCampaigns.SortProperty;

        if ( sortProperty != null )
        {
            gMarketingCampaigns.DataSource = marketingCampaignService.Queryable().Sort( sortProperty ).ToList();
        }
        else
        {
            gMarketingCampaigns.DataSource = marketingCampaignService.Queryable().OrderBy( p => p.Title ).ToList();
        }

        gMarketingCampaigns.DataBind();
    }

    /// <summary>
    /// Loads the drop downs.
    /// </summary>
    private void LoadDropDowns()
    {
        // Controls on Main Campaign Panel
        GroupService groupService = new GroupService();
        List<Group> groups = groupService.Queryable().Where( a => a.GroupType.Guid.Equals( Rock.SystemGuid.GroupType.GROUPTYPE_EVENTATTENDEES ) ).OrderBy( a => a.Name ).ToList();
        groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
        ddlEventGroup.DataSource = groups;
        ddlEventGroup.DataBind();

        PersonService personService = new PersonService();
        List<Person> persons = personService.Queryable().OrderBy( a => a.NickName ).ThenBy( a => a.LastName ).ToList();
        persons.Insert( 0, new Person { Id = None.Id, GivenName = None.Text } );
        ddlContactPerson.DataSource = persons;
        ddlContactPerson.DataBind();

        CampusService campusService = new CampusService();

        cpCampuses.Campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList(); ;

        // Controls on Ad Child Panel
        MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
        var adtypes = marketingCampaignAdTypeService.Queryable().OrderBy( a => a.Name ).ToList();
        ddlMarketingCampaignAdType.DataSource = adtypes;
        ddlMarketingCampaignAdType.DataBind();
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="marketingCampaignId">The marketing campaign id.</param>
    protected void ShowEdit( int marketingCampaignId )
    {
        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        MarketingCampaign marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
        MarketingCampaignAudiencesState = new List<MarketingCampaignAudienceDto>();

        if ( marketingCampaign == null )
        {
            marketingCampaign = new MarketingCampaign { Id = 0 };
            marketingCampaign.MarketingCampaignAds = new List<MarketingCampaignAd>();
            marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
            marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
        }

        hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();
        tbTitle.Text = marketingCampaign.Title;
        tbContactEmail.Text = marketingCampaign.ContactEmail;
        tbContactFullName.Text = marketingCampaign.ContactFullName;
        tbContactPhoneNumber.Text = marketingCampaign.ContactPhoneNumber;

        ddlContactPerson.SelectedValue = ( marketingCampaign.ContactPersonId ?? None.Id ).ToString();
        ddlEventGroup.SelectedValue = ( marketingCampaign.EventGroupId ?? None.Id ).ToString();

        foreach ( var audience in marketingCampaign.MarketingCampaignAudiences )
        {
            MarketingCampaignAudiencesState.Add( new MarketingCampaignAudienceDto( audience ) );
        }

        cpCampuses.SelectedCampusIds = marketingCampaign.MarketingCampaignCampuses.Select( a => a.CampusId ).ToList();

        UpdateReadonlyDetails( marketingCampaign );

        if ( marketingCampaign.Id > 0 )
        {
            lActionTitle.Text = ActionTitle.Edit( MarketingCampaign.FriendlyTypeName );
            SetEditMode( false );
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( MarketingCampaign.FriendlyTypeName );
            SetEditMode( true );
        }

        pnlList.Visible = false;
        pnlDetails.Visible = true;

        BindMarketingCampaignAdsGrid();
        BindMarketingCampaignAudiencesGrid();
    }

    /// <summary>
    /// Updates the readonly details.
    /// </summary>
    /// <param name="marketingCampaign">The marketing campaign.</param>
    private void UpdateReadonlyDetails( MarketingCampaign marketingCampaign )
    {
        // make a Description section for nonEdit mode
        string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
        lblMainDetails.Text = @"
<div class='span6'>
    <dl>";

        lblMainDetails.Text += string.Format( descriptionFormat, "Title", marketingCampaign.Title );

        string contactInfo = string.Format( "{0}<br>{1}<br>{2}", marketingCampaign.ContactFullName, marketingCampaign.ContactEmail, marketingCampaign.ContactPhoneNumber );
        contactInfo = string.IsNullOrWhiteSpace( contactInfo.Replace( "<br>", string.Empty ) ) ? None.TextHtml : contactInfo;
        lblMainDetails.Text += string.Format( descriptionFormat, "Contact", contactInfo );
        
        lblMainDetails.Text += string.Format( descriptionFormat, "Event", marketingCampaign.EventGroup == null ? None.TextHtml : marketingCampaign.EventGroup.Name );

        lblMainDetails.Text += @"
    </dl>
</div>
<div class='span6'>
    <dl>";

        string campusList = marketingCampaign.MarketingCampaignCampuses.Select( a => a.Campus.Name ).OrderBy( a => a).ToList().AsDelimited( "<br>" );
        campusList = marketingCampaign.MarketingCampaignCampuses.Count == 0 ? None.TextHtml : campusList;
        lblMainDetails.Text += string.Format( descriptionFormat, "Campuses", campusList ); 
        
        string primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a).ToList().AsDelimited( "<br>" );
        primaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => a.IsPrimary ).Count() == 0 ? None.TextHtml : primaryAudiences;
        lblMainDetails.Text += string.Format( descriptionFormat, "Primary Audience", primaryAudiences );

        string secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Select( a => a.Name ).OrderBy( a => a).ToList().AsDelimited( "<br>" );
        secondaryAudiences = marketingCampaign.MarketingCampaignAudiences.Where( a => !a.IsPrimary ).Count() == 0 ? None.TextHtml : secondaryAudiences;
        lblMainDetails.Text += string.Format( descriptionFormat, "Secondary Audience", secondaryAudiences );

        lblMainDetails.Text += @"
    </dl>
</div>";
    }

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlContactPerson control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void ddlContactPerson_SelectedIndexChanged( object sender, EventArgs e )
    {
        int personId = int.Parse( ddlContactPerson.SelectedValue );
        Person contactPerson = Person.Read( personId );
        if ( contactPerson != null )
        {
            tbContactEmail.Text = contactPerson.Email;
            tbContactFullName.Text = contactPerson.FullName;
            PhoneNumber phoneNumber = contactPerson.PhoneNumbers.FirstOrDefault( a => a.NumberType.Guid == Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_PRIMARY );
            tbContactPhoneNumber.Text = phoneNumber == null ? string.Empty : phoneNumber.Number;
        }
    }

    #endregion
}