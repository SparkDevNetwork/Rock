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
using Rock.Cms;
using Rock.Constants;
using Rock.Core;
using Rock.Crm;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

/// <summary>
/// 
/// </summary>
public partial class MarketingCampaigns : RockBlock
{
    #region Child Grid States

    /// <summary>
    /// Gets or sets the state of the marketing campaign ads.
    /// </summary>
    /// <value>
    /// The state of the marketing campaign ads.
    /// </value>
    private List<MarketingCampaignAdDto> MarketingCampaignAdsState
    {
        get
        {
            return ViewState["MarketingCampaignAdsState"] as List<MarketingCampaignAdDto>;
        }

        set
        {
            ViewState["MarketingCampaignAdsState"] = value;
        }
    }

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

    /// <summary>
    /// Gets or sets the state of the campuses.
    /// </summary>
    /// <value>
    /// The state of the campuses.
    /// </value>
    private List<CampusDto> CampusesState
    {
        get
        {
            return ViewState["CampusesState"] as List<CampusDto>;
        }

        set
        {
            ViewState["CampusesState"] = value;
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


            gMarketingCampaignAds.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAds.Actions.IsAddEnabled = true;
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );

            gMarketingCampaignAudiences.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAudiences.Actions.IsAddEnabled = true;
            gMarketingCampaignAudiences.Actions.AddClick += gMarketingCampaignAudiences_Add;
            gMarketingCampaignAudiences.GridRebind += gMarketingCampaignAudiences_GridRebind;
            gMarketingCampaignAudiences.EmptyDataText = Server.HtmlEncode( None.Text );

            gCampuses.DataKeyNames = new string[] { "key" };
            gCampuses.Actions.IsAddEnabled = true;
            gCampuses.Actions.AddClick += gCampus_Add;
            gCampuses.GridRebind += gCampus_GridRebind;
            gCampuses.EmptyDataText = Server.HtmlEncode( None.Text );
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
            gMarketingCampaigns.Visible = false;
            nbWarning.Text = WarningMessage.NotAuthorizedToEdit( MarketingCampaign.FriendlyTypeName );
            nbWarning.Visible = true;
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

        /* TODO
        string errorMessage;
        if ( !marketingCampaignService.CanDelete( marketingCampaignId, out errorMessage ) )
        {
            nbGridWarning.Text = errorMessage;
            nbGridWarning.Visible = true;
            return;
        }
         */

        MarketingCampaign marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
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
        //todo
        gMarketingCampaignAds_Edit( sender, new RowEventArgs( null ) );
    }

    /// <summary>
    /// Handles the Edit event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_Edit( object sender, RowEventArgs e )
    {
        if ( e.RowIndex < 0 )
        {
            //ToDo: add new one
        }

        //todo

        pnlMarketingCampaignAdEditor.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAds_Delete( object sender, RowEventArgs e )
    {
        //todo
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
        gMarketingCampaignAds.DataSource = MarketingCampaignAdsState.OrderBy( a => a.StartDate ).ThenBy( a => a.Priority ).ThenBy( a => a.Name );
        gMarketingCampaignAds.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnSaveAd control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnSaveAd_Click( object sender, EventArgs e )
    {
        //TODO

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

    #endregion

    #region MarketingCampaignAudience Grid and Picker

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAudiences control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiences_Add( object sender, EventArgs e )
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
            btnAddMarketingCampaignAudience.CssClass = "btn primary disabled";
        }
        else
        {
            btnAddMarketingCampaignAudience.Enabled = true;
            btnAddMarketingCampaignAudience.CssClass = "btn primary";
        }

        ddlMarketingCampaignAudiences.DataSource = list;
        ddlMarketingCampaignAudiences.DataBind();
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
        gMarketingCampaignAudiences.DataSource = MarketingCampaignAudiencesState.OrderBy( a => a.Name );
        gMarketingCampaignAudiences.DataBind();
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

    #region Campus Grid and Picker

    /// <summary>
    /// Handles the Add event of the gCampuss control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gCampus_Add( object sender, EventArgs e )
    {
        CampusService campusService = new CampusService();

        // populate dropdown with all db Campuses that aren't already CampusesState
        var qry = from campusInDB in campusService.Queryable()
                  where !( from campusStateItem in CampusesState
                           select campusStateItem.Id ).Contains( campusInDB.Id )
                  select campusInDB;

        List<Campus> list = qry.ToList();
        if ( list.Count == 0 )
        {
            list.Add( new Campus { Id = None.Id, Name = None.Text } );
            btnAddCampus.Enabled = false;
            btnAddCampus.CssClass = "btn primary disabled";
        }
        else
        {
            btnAddCampus.Enabled = true;
            btnAddCampus.CssClass = "btn primary";
        }

        ddlCampus.DataSource = list;
        ddlCampus.DataBind();
        pnlCampusPicker.Visible = true;
        pnlDetails.Visible = false;
        pnlList.Visible = false;
    }

    /// <summary>
    /// Handles the Delete event of the gCampus control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gCampus_Delete( object sender, RowEventArgs e )
    {
        int campusId = (int)e.RowKeyValue;
        CampusesState.RemoveDto( campusId );
        BindCampusGrid();
    }

    /// <summary>
    /// Handles the GridRebind event of the gCampus control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gCampus_GridRebind( object sender, EventArgs e )
    {
        BindCampusGrid();
    }

    /// <summary>
    /// Binds the campus grid.
    /// </summary>
    private void BindCampusGrid()
    {
        gCampuses.DataSource = CampusesState.OrderBy( a => a.Name );
        gCampuses.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddCampus control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddCampus_Click( object sender, EventArgs e )
    {
        int campusId = int.Parse( ddlCampus.SelectedValue );
        CampusesState.Add( new CampusDto( Campus.Read( campusId ) ) );

        pnlCampusPicker.Visible = false;
        pnlDetails.Visible = true;
        pnlList.Visible = false;

        BindCampusGrid();
    }

    /// <summary>
    /// Handles the Click event of the btnCancelAddCampus control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnCancelAddCampus_Click( object sender, EventArgs e )
    {
        pnlCampusPicker.Visible = false;
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

            // Update MarketingCampaignCampuses with UI values
            if ( marketingCampaign.MarketingCampaignCampuses == null )
            {
                marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
            }

            // take care of deleted Campuses
            MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService();
            var deletedCampuses = from mcc in marketingCampaign.MarketingCampaignCampuses.AsQueryable()
                                  where !( from cs in CampusesState
                                           select cs.Id ).Contains( mcc.CampusId )
                                  select mcc;

            deletedCampuses.ToList().ForEach( a =>
            {
                var c = marketingCampaignCampusService.Get( a.Guid );
                marketingCampaignCampusService.Delete( c, CurrentPersonId );
                marketingCampaignCampusService.Save( c, CurrentPersonId );
            } );

            // add or update the Campuses that are assigned in the UI
            foreach ( var campus in CampusesState )
            {
                MarketingCampaignCampus marketingCampaignCampus = marketingCampaign.MarketingCampaignCampuses.FirstOrDefault( a => a.CampusId.Equals( campus.Id ) );
                if ( marketingCampaignCampus == null )
                {
                    marketingCampaignCampus = new MarketingCampaignCampus();
                    marketingCampaign.MarketingCampaignCampuses.Add( marketingCampaignCampus );
                }

                marketingCampaignCampus.CampusId = campus.Id;
            }

            marketingCampaignService.Save( marketingCampaign, CurrentPersonId );
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
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="marketingCampaignId">The marketing campaign id.</param>
    protected void ShowEdit( int marketingCampaignId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        MarketingCampaign marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
        MarketingCampaignAudiencesState = new List<MarketingCampaignAudienceDto>();
        CampusesState = new List<CampusDto>();
        MarketingCampaignAdsState = new List<MarketingCampaignAdDto>();

        if ( marketingCampaign != null )
        {
            hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();
            tbTitle.Text = marketingCampaign.Title;
            tbContactEmail.Text = marketingCampaign.ContactEmail;
            tbContactFullName.Text = marketingCampaign.ContactFullName;
            tbContactPhoneNumber.Text = marketingCampaign.ContactPhoneNumber;

            ddlContactPerson.SelectedValue = ( marketingCampaign.ContactPersonId ?? None.Id ).ToString();
            ddlEventGroup.SelectedValue = ( marketingCampaign.EventGroupId ?? None.Id ).ToString();

            foreach ( var ad in marketingCampaign.MarketingCampaignAds )
            {
                MarketingCampaignAdsState.Add( new MarketingCampaignAdDto( ad ) );
            }

            foreach ( var audience in marketingCampaign.MarketingCampaignAudiences )
            {
                MarketingCampaignAudiencesState.Add( new MarketingCampaignAudienceDto( audience ) );
            }

            marketingCampaign.MarketingCampaignCampuses.ToList().ForEach( a => CampusesState.Add( new CampusDto( a.Campus ) ) );

            lActionTitle.Text = ActionTitle.Edit( MarketingCampaign.FriendlyTypeName );
            btnCancel.Text = "Cancel";
        }
        else
        {
            lActionTitle.Text = ActionTitle.Add( MarketingCampaign.FriendlyTypeName );

            hfMarketingCampaignId.Value = 0.ToString();
            tbTitle.Text = string.Empty;
            tbContactEmail.Text = string.Empty;
            tbContactFullName.Text = string.Empty;
            tbContactPhoneNumber.Text = string.Empty;
        }

        BindMarketingCampaignAdsGrid();
        BindMarketingCampaignAudiencesGrid();
        BindCampusGrid();
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