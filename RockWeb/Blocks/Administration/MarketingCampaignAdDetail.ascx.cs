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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [AdditionalActions( new string[] { "Approve" } )]
    public partial class MarketingCampaignAdDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
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
                string itemId = PageParameter( "marketingCampaignAdId" );
                string campaignId = PageParameter( "marketingCampaignId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( campaignId ) )
                    {
                        ShowDetail( "marketingCampaignAdId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "marketingCampaignAdId", int.Parse( itemId ), int.Parse( campaignId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                LoadAdAttributes( new MarketingCampaignAd(), true, false );
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
            var adtypes = marketingCampaignAdTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlMarketingCampaignAdType.DataSource = adtypes;
            ddlMarketingCampaignAdType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="marketingCampaignId">The marketing campaign id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? marketingCampaignId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "marketingCampaignAdId" ) )
            {
                return;
            }

            MarketingCampaignAd marketingCampaignAd = null;

            btnApproveAd.Visible = IsUserAuthorized( "Approve" );
            btnDenyAd.Visible = IsUserAuthorized( "Approve" );

            if ( !itemKeyValue.Equals( 0 ) )
            {
                marketingCampaignAd = new MarketingCampaignAdService().Get( itemKeyValue );
                marketingCampaignAd.LoadAttributes();
            }
            else
            {
                // only create a new marketing Campaign Ad if the marketingCampaignId was specified
                if ( marketingCampaignId != null )
                {
                    marketingCampaignAd = new MarketingCampaignAd { Id = 0, MarketingCampaignAdStatus = MarketingCampaignAdStatus.PendingApproval };
                    marketingCampaignAd.MarketingCampaignId = marketingCampaignId.Value;
                    marketingCampaignAd.MarketingCampaign = new MarketingCampaignService().Get( marketingCampaignAd.MarketingCampaignId );
                }
            }

            if ( marketingCampaignAd == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfMarketingCampaignAdId.Value = marketingCampaignAd.Id.ToString();
            hfMarketingCampaignId.Value = marketingCampaignAd.MarketingCampaignId.ToString();

            if ( marketingCampaignAd.Id.Equals( 0 ) )
            {
                lActionTitleAd.Text = ActionTitle.Add( "Marketing Ad for " + marketingCampaignAd.MarketingCampaign.Title );
            }
            else
            {
                lActionTitleAd.Text = ActionTitle.Edit( "Marketing Ad for " + marketingCampaignAd.MarketingCampaign.Title );
            }

            LoadDropDowns();
            ddlMarketingCampaignAdType.SetValue( marketingCampaignAd.MarketingCampaignAdTypeId );
            tbPriority.Text = marketingCampaignAd.Priority.ToString();

            SetApprovalValues( marketingCampaignAd.MarketingCampaignAdStatus, new PersonService().Get( marketingCampaignAd.MarketingCampaignStatusPersonId ?? 0 ) );

            if ( itemKeyValue.Equals( 0 ) )
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

            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlMarketingCampaignAdType_SelectedIndexChanged( object sender, EventArgs e )
        {
            MarketingCampaignAd marketingCampaignAd = new MarketingCampaignAd();

            LoadAdAttributes( marketingCampaignAd, false, false );
            Rock.Attribute.Helper.GetEditValues( phAttributes, marketingCampaignAd );

            SetApprovalValues( MarketingCampaignAdStatus.PendingApproval, null );

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

            MarketingCampaignAdType marketingCampaignAdType = new MarketingCampaignAdTypeService().Get( marketingAdTypeId );
            tbAdDateRangeEndDate.Visible = marketingCampaignAdType.DateRangeType.Equals( DateRangeTypeEnum.DateRange );

            List<Rock.Web.Cache.AttributeCache> attributesForAdType = GetAttributesForAdType( marketingAdTypeId );

            marketingCampaignAd.Attributes = marketingCampaignAd.Attributes ?? new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            marketingCampaignAd.AttributeValues = marketingCampaignAd.AttributeValues ?? new Dictionary<string, List<AttributeValue>>();
            foreach ( var attribute in attributesForAdType )
            {
                marketingCampaignAd.Attributes[attribute.Key] = attribute;
                if ( marketingCampaignAd.AttributeValues.Count( v => v.Key.Equals( attribute.Key ) ) == 0 )
                {
                    List<AttributeValue> attributeValues = new List<AttributeValue>();
                    attributeValues.Add( new AttributeValue { Value = attribute.DefaultValue } );
                    marketingCampaignAd.AttributeValues.Add( attribute.Key, attributeValues );
                }
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
        private static List<Rock.Web.Cache.AttributeCache> GetAttributesForAdType( int marketingAdTypeId )
        {
            MarketingCampaignAd temp = new MarketingCampaignAd();
            temp.MarketingCampaignAdTypeId = marketingAdTypeId;
            temp.LoadAttributes();
            return temp.Attributes.Values.ToList();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            int marketingCampaignAdId = int.Parse( hfMarketingCampaignAdId.Value );
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();

            MarketingCampaignAd marketingCampaignAd;
            if ( marketingCampaignAdId.Equals( 0 ) )
            {
                marketingCampaignAd = new MarketingCampaignAd { Id = 0 };
                marketingCampaignAd.MarketingCampaignId = hfMarketingCampaignId.ValueAsInt();
            }
            else
            {
                marketingCampaignAd = marketingCampaignAdService.Get( marketingCampaignAdId );
            }

            if ( string.IsNullOrWhiteSpace( ddlMarketingCampaignAdType.SelectedValue ) )
            {
                ddlMarketingCampaignAdType.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlMarketingCampaignAdType.LabelText ) );
                return;
            }

            marketingCampaignAd.MarketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
            marketingCampaignAd.MarketingCampaignAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );
            marketingCampaignAd.Priority = tbPriority.Text.AsInteger() ?? 0;
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

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["marketingCampaignId"] = hfMarketingCampaignId.Value;
            NavigateToParentPage( qryString );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["marketingCampaignId"] = hfMarketingCampaignId.Value;
            NavigateToParentPage( qryString );
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
            switch ( status )
            {
                case MarketingCampaignAdStatus.Approved: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-success";
                    break;
                case MarketingCampaignAdStatus.Denied: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-error";
                    break;
                default: ltMarketingCampaignAdStatus.TextCssClass = "alert MarketingCampaignAdStatus alert-info";
                    break;
            }

            hfMarketingCampaignAdStatus.Value = status.ConvertToInt().ToString();
            lblMarketingCampaignAdStatusPerson.Visible = person != null;
            if ( person != null )
            {
                lblMarketingCampaignAdStatusPerson.Text = "by " + person.FullName;
                hfMarketingCampaignAdStatusPersonId.Value = person.Id.ToString();
            }
        }

        #endregion
    }
}