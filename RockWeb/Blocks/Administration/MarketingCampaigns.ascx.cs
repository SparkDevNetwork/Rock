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
using Rock.Web.UI;
using Rock.Web.UI.Controls;

/// <summary>
/// 
/// </summary>
public partial class MarketingCampaigns : RockBlock
{
    #region Child Grid Dictionarys

    [Serializable]
    private class MarketingCampaignAdViewStateItem : IComparable
    {
        public MarketingCampaignAdViewStateItem( MarketingCampaignAd ad )
        {
            this.Name = ad.MarketingCampaignAdType.Name;
            this.DateText = StartDate.ToShortDateString();
            if ( EndDate != StartDate )
            {
                this.DateText += "-" + EndDate.ToShortDateString();
            }
            this.StatusText = ad.MarketingCampaignAdStatus.ConvertToString().SplitCase();
            this.MarketingCampaignAdTypeId = ad.MarketingCampaignAdTypeId;
            this.Priority = ad.Priority;
            this.MarketingCampaignAdStatus = ad.MarketingCampaignAdStatus;
            this.MarketingCampaignAdStatusPersonId = ad.MarketingCampaignStatusPersonId;
            this.StartDate = ad.StartDate;
            this.EndDate = ad.EndDate;
            this.Url = ad.Url;
            this.Guid = ad.Guid;
            this.Attributes = ad.Attributes;
        }
        
        public string Name { get; set; }
        public string DateText { get; set; }
        public string StatusText { get; set; }

        public int MarketingCampaignAdTypeId { get; set; }
        public int Priority { get; set; }
        public MarketingCampaignAdStatus MarketingCampaignAdStatus { get; set; }
        public int? MarketingCampaignAdStatusPersonId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Url { get; set; }
        public Guid Guid { get; set; }
        public Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes { get; set; }

        public int CompareTo( object obj )
        {
            if ( obj == null )
            {
                return 1;
            }
            else
            {
                var other = obj as MarketingCampaignAdViewStateItem;
                int result = this.StartDate.CompareTo( other.StartDate );
                if ( result == 0 )
                {
                    result = this.Priority.CompareTo( other.Priority );
                }
                if ( result == 0 )
                {
                    result = this.Name.CompareTo( other.Name );
                }
                return result;
            }
        }
    }

    /// <summary>
    /// Gets or sets the state of the marketing campaign ads view.
    /// </summary>
    /// <value>
    /// The state of the marketing campaign ads view.
    /// </value>
    private Dictionary<int, MarketingCampaignAdViewStateItem> MarketingCampaignAdsViewState
    {
        get
        {
            Dictionary<int, MarketingCampaignAdViewStateItem> marketingCampaignAdsViewState = ViewState["MarketingCampaignAdsViewState"] as Dictionary<int, MarketingCampaignAdViewStateItem>;
            return marketingCampaignAdsViewState;
        }

        set
        {
            ViewState["MarketingCampaignAdsViewState"] = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    private class MarketingCampaignAudienceViewStateItem : IComparable
    {
        /// <summary>
        /// Gets or sets the name of the audience type value.
        /// </summary>
        /// <value>
        /// The name of the audience type value.
        /// </value>
        public string AudienceTypeValueName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primary; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        public int CompareTo( object obj )
        {
            if ( obj == null )
            {
                return 1;
            }
            else
            {
                var other = obj as MarketingCampaignAudienceViewStateItem;
                return this.AudienceTypeValueName.CompareTo( other.AudienceTypeValueName );
            }
        }
    }

    /// <summary>
    /// Gets or sets the state of the marketing campaign audiences view.
    /// </summary>
    /// <value>
    /// The state of the marketing campaign audiences view.
    /// </value>
    private Dictionary<int, MarketingCampaignAudienceViewStateItem> MarketingCampaignAudiencesViewState
    {
        get
        {
            Dictionary<int, MarketingCampaignAudienceViewStateItem> marketingCampaignAudiencesViewState = ViewState["MarketingCampaignAudiencesViewState"] as Dictionary<int, MarketingCampaignAudienceViewStateItem>;
            return marketingCampaignAudiencesViewState;
        }

        set
        {
            ViewState["MarketingCampaignAudiencesViewState"] = value;
        }
    }

    /// <summary>
    /// Gets or sets the campuses dictionary.
    /// </summary>
    /// <value>
    /// The campuses dictionary.
    /// </value>
    private Dictionary<int, string> CampusesDictionary
    {
        get
        {
            Dictionary<int, string> campusesDictionary = ViewState["CampusesDictionary"] as Dictionary<int, string>;
            return campusesDictionary;
        }

        set
        {
            ViewState["CampusesDictionary"] = value;
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


            gMarketingCampaignAds.DataKeyNames = new string[] { "key" };
            gMarketingCampaignAds.Actions.IsAddEnabled = true;
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );
            
            gMarketingCampaignAudiences.DataKeyNames = new string[] { "key" };
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
        ShowEdit( (int)gMarketingCampaigns.DataKeys[e.RowIndex]["id"] );
    }

    /// <summary>
    /// Handles the Delete event of the gMarketingCampaigns control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaigns_Delete( object sender, RowEventArgs e )
    {
        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        int marketingCampaignId = (int)gMarketingCampaigns.DataKeys[e.RowIndex]["id"];

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
        gMarketingCampaignAds_Edit( sender, new RowEventArgs(-1) );
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
        gMarketingCampaignAds.DataSource = MarketingCampaignAdsViewState.OrderBy( a => a.Value );
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
        var qry = from a in definedValueService.GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ).AsQueryable()
                  where !( from k in MarketingCampaignAudiencesViewState.Keys
                           select k ).Contains( a.Id )
                  select a;

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
        int marketingCampaignAudienceId = (int)gMarketingCampaignAudiences.DataKeys[e.RowIndex]["key"];
        MarketingCampaignAudiencesViewState.Remove( marketingCampaignAudienceId );
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
        gMarketingCampaignAudiences.DataSource = MarketingCampaignAudiencesViewState.OrderBy( a => a.Value );
        gMarketingCampaignAudiences.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddMarketingCampaignAudience control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddMarketingCampaignAudience_Click( object sender, EventArgs e )
    {
        MarketingCampaignAudienceViewStateItem item = new MarketingCampaignAudienceViewStateItem();
        int audienceTypeValueId = int.Parse( ddlMarketingCampaignAudiences.SelectedValue );
        item.AudienceTypeValueName = ddlMarketingCampaignAudiences.SelectedItem.Text;
        item.IsPrimary = ckMarketingCampaignAudienceIsPrimary.Checked;

        MarketingCampaignAudiencesViewState.Add( audienceTypeValueId, item );

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

        // populate dropdown with all Campuss that aren't already Campuss
        var qry = from dlt in campusService.Queryable()
                  where !( from lt in CampusesDictionary.Keys
                           select lt ).Contains( dlt.Id )
                  select dlt;

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
        int campusId = (int)gCampuses.DataKeys[e.RowIndex]["key"];
        CampusesDictionary.Remove( campusId );
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
        gCampuses.DataSource = CampusesDictionary.OrderBy( a => a.Value );
        gCampuses.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddCampus control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddCampus_Click( object sender, EventArgs e )
    {
        CampusesDictionary.Add( int.Parse( ddlCampus.SelectedValue ), ddlCampus.SelectedItem.Text );

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

        // Update MarketingCampaignAudiences with UI values
        if ( marketingCampaign.MarketingCampaignAudiences == null )
        {
            marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
        }

        // delete Audiences that aren't assigned in the UI anymore
        MarketingCampaignAudienceService marketingCampaignAudienceService = new MarketingCampaignAudienceService();
        var deletedAudiences = from mca in marketingCampaign.MarketingCampaignAudiences.AsQueryable()
                               where !( from v in MarketingCampaignAudiencesViewState.Keys
                                        select v ).Contains( mca.AudienceTypeValueId )
                               select mca;
        deletedAudiences.ToList().ForEach( a =>
        {
            var aud = marketingCampaignAudienceService.Get( a.Guid );
            marketingCampaignAudienceService.Delete( aud, CurrentPersonId );
            marketingCampaignAudienceService.Save( aud, CurrentPersonId );
        } );

        // add or update the Audiences that are assigned in the UI
        foreach ( var item in MarketingCampaignAudiencesViewState )
        {
            MarketingCampaignAudience marketingCampaignAudience = marketingCampaign.MarketingCampaignAudiences.FirstOrDefault( a => a.AudienceTypeValueId.Equals( item.Key ) );
            if ( marketingCampaignAudience == null )
            {
                marketingCampaignAudience = new MarketingCampaignAudience();
                marketingCampaign.MarketingCampaignAudiences.Add( marketingCampaignAudience );
            }

            marketingCampaignAudience.AudienceTypeValueId = item.Key;
            marketingCampaignAudience.IsPrimary = item.Value.IsPrimary;
        }

        // Update MarketingCampaignCampuses with UI values
        if ( marketingCampaign.MarketingCampaignCampuses == null )
        {
            marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
        }

        // take care of deleted Campuses
        MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService();
        var deletedCampuses = from mcc in marketingCampaign.MarketingCampaignCampuses.AsQueryable()
                              where !( from v in CampusesDictionary.Keys
                                       select v ).Contains( mcc.CampusId )
                              select mcc;

        deletedCampuses.ToList().ForEach( a =>
        {
            var c = marketingCampaignCampusService.Get( a.Guid );
            marketingCampaignCampusService.Delete( c, CurrentPersonId );
            marketingCampaignCampusService.Save( c, CurrentPersonId );
        } );

        // add or update the Campuses that are assigned in the UI
        foreach ( var campus in CampusesDictionary )
        {
            MarketingCampaignCampus marketingCampaignCampus = marketingCampaign.MarketingCampaignCampuses.FirstOrDefault( a => a.CampusId.Equals( campus.Key ) );
            if ( marketingCampaignCampus == null )
            {
                marketingCampaignCampus = new MarketingCampaignCampus();
                marketingCampaign.MarketingCampaignCampuses.Add( marketingCampaignCampus );
            }

            marketingCampaignCampus.CampusId = campus.Key;
        }

        marketingCampaignService.Save( marketingCampaign, CurrentPersonId );

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
        MarketingCampaignAudiencesViewState = new Dictionary<int, MarketingCampaignAudienceViewStateItem>();
        CampusesDictionary = new Dictionary<int, string>();
        MarketingCampaignAdsViewState = new Dictionary<int, MarketingCampaignAdViewStateItem>();

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
                MarketingCampaignAdViewStateItem item = new MarketingCampaignAdViewStateItem( ad );
                MarketingCampaignAdsViewState.Add( ad.Id, item );
            }

            foreach ( var a in marketingCampaign.MarketingCampaignAudiences )
            {
                MarketingCampaignAudienceViewStateItem item = new MarketingCampaignAudienceViewStateItem();
                item.IsPrimary = a.IsPrimary;
                item.AudienceTypeValueName = a.AudienceTypeValue.Name;
                int audienceTypeValueId = a.AudienceTypeValueId;
                MarketingCampaignAudiencesViewState.Add( audienceTypeValueId, item );
            }

            marketingCampaign.MarketingCampaignCampuses.ToList().ForEach( a => CampusesDictionary.Add( a.CampusId, a.Campus.Name ) );

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