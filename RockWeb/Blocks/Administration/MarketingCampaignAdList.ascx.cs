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
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [BooleanField( "Show Marketing Campaign Title")]
    [ContextAware( typeof( MarketingCampaign ) )]
    [LinkedPage("Detail Page")]
    public partial class MarketingCampaignAdList : RockBlock, IDimmableBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            BindFilter();

            gMarketingCampaignAds.DataKeyNames = new string[] { "Id" };
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );

            // Block Security on Ads grid (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMarketingCampaignAds.Actions.ShowAdd = canAddEditDelete;
            gMarketingCampaignAds.IsDeleteEnabled = canAddEditDelete;

            Dictionary<string, BoundField> boundFields = gMarketingCampaignAds.Columns.OfType<BoundField>().ToDictionary( a => a.DataField );
            boundFields["MarketingCampaign.Title"].Visible = GetAttributeValue( "ShowMarketingCampaignTitle" ).FromTrueFalse();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                MarketingCampaign marketingCampaign = this.ContextEntity<MarketingCampaign>();
                if ( marketingCampaign != null )
                {
                    hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();
                    BindGrid();
                }
                else
                {
                    // if there isn't a marketingCampaign context, don't allow new ads to be added (since we don't know which campaign to add the ad to)
                    gMarketingCampaignAds.Actions.ShowAdd = false;
                    hfMarketingCampaignId.Value = null;
                    BindGrid();
                }
            }
        }

        #endregion

        #region Grid Filter

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlApprovalStatus.BindToEnum( typeof( MarketingCampaignAdStatus ) );
            ddlApprovalStatus.Items.Insert( 0, Rock.Constants.All.ListItem );

            ddlApprovalStatus.SetValue( rFilter.GetUserPreference( "Approval Status" ) );

            string priorityRangeValues = rFilter.GetUserPreference( "Priority Range" );

            if ( !string.IsNullOrWhiteSpace( priorityRangeValues ) )
            {
                string[] upperLowerValues = priorityRangeValues.Split( new char[] { ',' }, StringSplitOptions.None );

                if ( upperLowerValues.Length == 2 )
                {
                    pPriorityRange.LowerValue = upperLowerValues[0].AsInteger( false );
                    pPriorityRange.UpperValue = upperLowerValues[1].AsInteger( false );
                }
            }

            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
            var adTypeList = marketingCampaignAdTypeService.Queryable().Select( a => new { a.Id, a.Name } ).OrderBy( a => a.Name ).ToList();
            foreach ( var adType in adTypeList )
            {
                ddlAdType.Items.Add( new ListItem( adType.Name, adType.Id.ToString() ) );
            }

            ddlAdType.Items.Insert( 0, Rock.Constants.All.ListItem );

            ddlAdType.SetValue( rFilter.GetUserPreference( "Ad Type" ) );

            string dateRangeValues = rFilter.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValues ) )
            {
                string[] upperLowerValues = dateRangeValues.Split( new char[] { ',' }, StringSplitOptions.None );

                if ( upperLowerValues.Length == 2 )
                {
                    string lowerValue = upperLowerValues[0];
                    if ( !string.IsNullOrWhiteSpace( lowerValue ) )
                    {
                        pDateRange.LowerValue = DateTime.Parse( lowerValue );
                    }

                    string upperValue = upperLowerValues[1];
                    if ( !string.IsNullOrWhiteSpace( upperValue ) )
                    {
                        pDateRange.UpperValue = DateTime.Parse( upperValue );
                    }

                }
            }
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Approval Status":

                    int approvalStatusValue = e.Value.AsInteger( false ) ?? Rock.Constants.All.Id;
                    if ( approvalStatusValue != Rock.Constants.All.Id )
                    {
                        e.Value = e.Value.ConvertToEnum<MarketingCampaignAdStatus>().ConvertToString();
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;

                case "Priority Range":
                case "Date Range":

                    string[] values = e.Value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( values.Length == 2 )
                    {
                        if ( string.IsNullOrWhiteSpace( values[0] ) && string.IsNullOrWhiteSpace( values[1] ) )
                        {
                            e.Value = Rock.Constants.All.Text;
                        }
                        else if ( string.IsNullOrWhiteSpace( values[0] ) )
                        {
                            e.Value = "less than " + values[1];
                        }
                        else if ( string.IsNullOrWhiteSpace( values[1] ) )
                        {
                            e.Value = "greater than " + values[0];
                        }
                        else
                        {
                            e.Value = string.Format( "{0} to {1}", values[0], values[1] );
                        }
                    }

                    break;

                case "Ad Type":

                    var adType = new MarketingCampaignAdTypeService().Get( e.Value.AsInteger() ?? 0 );
                    if ( adType != null )
                    {
                        e.Value = adType.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;

            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Approval Status", ddlApprovalStatus.SelectedValue );
            rFilter.SaveUserPreference( "Priority Range", string.Format( "{0},{1}", pPriorityRange.LowerValue, pPriorityRange.UpperValue ) );
            rFilter.SaveUserPreference( "Ad Type", ddlAdType.SelectedValue );
            rFilter.SaveUserPreference( "Date Range",
                string.Format( "{0},{1}",
                pDateRange.LowerValue.HasValue ? pDateRange.LowerValue.Value.ToString( "d" ) : string.Empty,
                pDateRange.UpperValue.HasValue ? pDateRange.UpperValue.Value.ToString( "d" ) : string.Empty ) );

            BindGrid();
        }

        #endregion

        #region MarketingCampaignAds Grid

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
            gMarketingCampaignAds_ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Gs the marketing campaign ads_ show edit.
        /// </summary>
        /// <param name="marketingCampaignAdId">The marketing campaign ad id.</param>
        protected void gMarketingCampaignAds_ShowEdit( int marketingCampaignAdId )
        {
            if ( marketingCampaignAdId == 0 )
            {
                NavigateToLinkedPage( "DetailPage", "marketingCampaignAdId", 0, "marketingCampaignId", hfMarketingCampaignId.Value.AsInteger().Value );
            }
            else
            {
                NavigateToLinkedPage( "DetailPage", "marketingCampaignAdId", marketingCampaignAdId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
                MarketingCampaignAd marketingCampaignAd = marketingCampaignAdService.Get( e.RowKeyId );
                if ( marketingCampaignAd != null )
                {
                    string errorMessage;
                    if ( !marketingCampaignAdService.CanDelete( marketingCampaignAd, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    marketingCampaignAdService.Delete( marketingCampaignAd, CurrentPersonId );
                    marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the marketing campaign ads grid.
        /// </summary>
        private void BindGrid()
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            var qry = marketingCampaignAdService.Queryable( "MarketingCampaign, MarketingCampaignAdType" );

            // limit to current marketingCampaign context (if there is one)
            int? marketingCampaignId = hfMarketingCampaignId.Value.AsInteger( false );
            if ( marketingCampaignId.HasValue )
            {
                qry = qry.Where( a => a.MarketingCampaignId == marketingCampaignId );
            }

            // Approval Status
            int approvalStatus = ddlApprovalStatus.SelectedValueAsInt( false ) ?? Rock.Constants.All.Id;
            if ( approvalStatus != Rock.Constants.All.Id )
            {
                MarketingCampaignAdStatus marketingCampaignAdStatus = ddlApprovalStatus.SelectedValueAsEnum<MarketingCampaignAdStatus>();
                qry = qry.Where( a => a.MarketingCampaignAdStatus == marketingCampaignAdStatus );
            }

            // Priority Range
            if ( pPriorityRange.LowerValue.HasValue )
            {
                int lowerValue = (int)pPriorityRange.LowerValue.Value;
                qry = qry.Where( a => a.Priority >= lowerValue );
            }

            if ( pPriorityRange.UpperValue.HasValue )
            {
                int upperValue = (int)pPriorityRange.UpperValue.Value;
                qry = qry.Where( a => a.Priority <= upperValue );
            }

            // Ad Type
            int adTypeId = ddlAdType.SelectedValueAsInt() ?? Rock.Constants.All.Id;
            if ( adTypeId != Rock.Constants.All.Id )
            {
                qry = qry.Where( a => a.MarketingCampaignAdTypeId == adTypeId );
            }

            // Date Range
            if ( pDateRange.LowerValue.HasValue )
            {
                DateTime startDate = pDateRange.LowerValue.Value.Date;
                qry = qry.Where( a => a.StartDate >= startDate );
            }

            if ( pDateRange.UpperValue.HasValue )
            {
                // add a whole day to the selected endDate since users will expect to see all the stuff that happened 
                // on the endDate up until the very end of that day.

                // calculate the query endDate before including it in the qry statement to avoid Linq error
                var endDate = pDateRange.UpperValue.Value.AddDays( 1 );
                qry = qry.Where( a => a.StartDate < endDate );
            }

            SortProperty sortProperty = gMarketingCampaignAds.SortProperty;

            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( a => a.StartDate ).ThenBy( a => a.Priority ).ThenBy( a => a.MarketingCampaignAdType.Name );
            }

            gMarketingCampaignAds.DataSource = qry.ToList();
            gMarketingCampaignAds.DataBind();
        }

        #endregion

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlMarketingCampaignAds.Disabled = dimmed;
            gMarketingCampaignAds.Enabled = !dimmed;
        }

        #endregion
    }
}