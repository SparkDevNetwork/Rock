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

public partial class MarketingCampaigns : RockBlock
{

    #region Child Grid Dictionarys

    /// <summary>
    /// Gets or sets the marketing campaign audiences dictionary.
    /// </summary>
    /// <value>
    /// The marketing campaign audiences dictionary.
    /// </value>
    private Dictionary<int, string> MarketingCampaignAudiencesDictionary
    {
        get
        {
            Dictionary<int, string> marketingCampaignAudiencesDictionary = ViewState["MarketingCampaignAudiencesDictionary"] as Dictionary<int, string>;
            return marketingCampaignAudiencesDictionary;
        }

        set
        {
            ViewState["MarketingCampaignAudiencesDictionary"] = value;
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

        /*
        string errorMessage;
        if ( !marketingCampaignService.CanDelete( marketingCampaignId, out errorMessage ) )
        {
            nbGridWarning.Text = errorMessage;
            nbGridWarning.Visible = true;
            return;
        }
         */

        MarketingCampaign MarketingCampaign = marketingCampaignService.Get( marketingCampaignId );
        if ( CurrentBlock != null )
        {
            marketingCampaignService.Delete( MarketingCampaign, CurrentPersonId );
            marketingCampaignService.Save( MarketingCampaign, CurrentPersonId );
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

    #region MarketingCampaignAudience Grid and Picker

    /// <summary>
    /// Handles the Add event of the gMarketingCampaignAudiences control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void gMarketingCampaignAudiences_Add( object sender, EventArgs e )
    {
        MarketingCampaignAudienceService MarketingCampaignAudienceService = new MarketingCampaignAudienceService();

        // populate dropdown with all MarketingCampaignAudiences that aren't already MarketingCampaignAudiences
        var qry = from gt in MarketingCampaignAudienceService.Queryable()
                  where !( from cgt in MarketingCampaignAudiencesDictionary.Keys
                           select cgt ).Contains( gt.Id )
                  select gt;

        List<MarketingCampaignAudience> list = qry.ToList();
        if ( list.Count == 0 )
        {
            MarketingCampaignAudience noneMarketingCampaignAudience = new MarketingCampaignAudience();
            noneMarketingCampaignAudience.Id = None.Id;
            noneMarketingCampaignAudience.AudienceTypeValue = new Rock.Core.DefinedValue { Name = None.Text };

            list.Add( noneMarketingCampaignAudience );
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
        int MarketingCampaignAudienceId = (int)gMarketingCampaignAudiences.DataKeys[e.RowIndex]["key"];
        MarketingCampaignAudiencesDictionary.Remove( MarketingCampaignAudienceId );
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
    /// Binds the child group types grid.
    /// </summary>
    private void BindMarketingCampaignAudiencesGrid()
    {
        gMarketingCampaignAudiences.DataSource = MarketingCampaignAudiencesDictionary.OrderBy( a => a.Value );
        gMarketingCampaignAudiences.DataBind();
    }

    /// <summary>
    /// Handles the Click event of the btnAddMarketingCampaignAudience control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void btnAddMarketingCampaignAudience_Click( object sender, EventArgs e )
    {
        MarketingCampaignAudiencesDictionary.Add( int.Parse( ddlMarketingCampaignAudiences.SelectedValue ), ddlMarketingCampaignAudiences.SelectedItem.Text );

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

        MarketingCampaignAudienceService marketingCampaignAudienceService = new MarketingCampaignAudienceService();

        marketingCampaign.MarketingCampaignAudiences = new List<MarketingCampaignAudience>();
        marketingCampaign.MarketingCampaignAudiences.Clear();
        foreach ( var item in MarketingCampaignAudiencesDictionary )
        {
            MarketingCampaignAudience marketingCampaignAudience = marketingCampaignAudienceService.Get( item.Key );
            if ( marketingCampaignAudience != null )
            {
                marketingCampaign.MarketingCampaignAudiences.Add( marketingCampaignAudience );
            }
        }

        MarketingCampaignCampusService marketingCampaignCampusService = new MarketingCampaignCampusService();

        marketingCampaign.MarketingCampaignCampuses = new List<MarketingCampaignCampus>();
        marketingCampaign.MarketingCampaignCampuses.Clear();
        foreach ( var campus in CampusesDictionary )
        {
            var marketingCampaignCampus = marketingCampaignCampusService.Queryable().FirstOrDefault( a => a.MarketingCampaignId == marketingCampaign.Id || a.CampusId == campus.Key);
            if ( marketingCampaignCampus != null )
            {
                marketingCampaign.MarketingCampaignCampuses.Add( new MarketingCampaignCampus { CampusId = campus.Key } );
            }
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
        //TODO limit to grouptype Event
        List<Group> groups = groupService.Queryable().OrderBy( a => a.Name ).ToList();
        groups.Insert( 0, new Group { Id = None.Id, Name = None.Text } );
        ddlEventGroup.DataSource = groups;
        ddlEventGroup.DataBind();

        PersonService personService = new PersonService();
        List<Person> persons = personService.Queryable().OrderBy( a => a.NickName ).ThenBy( a => a.LastName).ToList();
        persons.Insert( 0, new Person { Id = None.Id, GivenName = None.Text } );
        ddlContactPerson.DataSource = persons;
        ddlContactPerson.DataBind();
    }

    /// <summary>
    /// Shows the edit.
    /// </summary>
    /// <param name="marketingCampaignId">The group type id.</param>
    protected void ShowEdit( int marketingCampaignId )
    {
        pnlList.Visible = false;
        pnlDetails.Visible = true;

        MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
        MarketingCampaign marketingCampaign = marketingCampaignService.Get( marketingCampaignId );
        MarketingCampaignAudiencesDictionary = new Dictionary<int, string>();
        CampusesDictionary = new Dictionary<int, string>();

        if ( marketingCampaign != null )
        {
            hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();
            tbTitle.Text = marketingCampaign.Title;
            tbContactEmail.Text = marketingCampaign.ContactEmail;
            tbContactFullName.Text = marketingCampaign.ContactFullName;
            tbContactPhoneNumber.Text = marketingCampaign.ContactPhoneNumber;

            //todo
            lbMarketingCampaignAds.Text = string.Format( "Marketing Campaign Ads - {0}", 0 );

            ddlContactPerson.SelectedValue = ( marketingCampaign.ContactPersonId ?? None.Id ).ToString();
            ddlEventGroup.SelectedValue = ( marketingCampaign.EventGroupId ?? None.Id ).ToString();

            marketingCampaign.MarketingCampaignAudiences.ToList().ForEach( a => MarketingCampaignAudiencesDictionary.Add( a.Id, a.Name ) );
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
            lbMarketingCampaignAds.Text = "Marketing Campaign Ads - 0";
        }

        BindMarketingCampaignAudiencesGrid();
        BindCampusGrid();
    }

    /// <summary>
    /// Handles the Click event of the lbMarketingCampaignAds control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    protected void lbMarketingCampaignAds_Click( object sender, EventArgs e )
    {
        //todo
    }

    #endregion
}