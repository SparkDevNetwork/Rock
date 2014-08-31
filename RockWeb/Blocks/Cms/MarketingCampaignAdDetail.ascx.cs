// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Marketing Campaign - Ad Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details for an Ad." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve Ads." )]
    
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
                ShowDetail( PageParameter( "marketingCampaignAdId" ).AsInteger(), PageParameter( "marketingCampaignId" ).AsIntegerOrNull() );
            }
            else
            {
                LoadAdAttributes( new MarketingCampaignAd(), true, false );
            }

            // if the current user can't approve their own edits, set the approval to Not-Approved when they change something
            if ( !IsUserAuthorized( "Approve" ) )
            {
                // change approval status to pending if any values are changed
                string onchangeScriptFormat = @"
                    $('#{0}').removeClass('alert-success alert-danger').addClass('alert-info');
                    $('#{0}').text('Pending Approval');
                    $('#{1}').val('1');
                    $('#{2}').val('');
                    $('#{3}').hide();";

                string onchangeScript = string.Format( onchangeScriptFormat, lblApprovalStatus.ClientID, hfApprovalStatus.ClientID, hfApprovalStatusPersonId.ClientID, lblApprovalStatusPerson.ClientID );

                // catch changes from any HtmlEditors (special case)
                var htmlEditors = phAttributes.ControlsOfTypeRecursive<Rock.Web.UI.Controls.HtmlEditor>();
                foreach ( var htmlEditor in htmlEditors )
                {
                    htmlEditor.OnChangeScript = onchangeScript;
                }

                // catch changes from any other inputs in the detail panel
                string otherInputsChangeScript = string.Format( @"$('#{0} :input').on('change', function () {{", upDetail.ClientID ) + Environment.NewLine;
                otherInputsChangeScript += onchangeScript + Environment.NewLine;
                otherInputsChangeScript += "});";

                ScriptManager.RegisterStartupScript( this, this.GetType(), "approval-status-script_" + this.ClientID, "Sys.Application.add_load(function () {" + otherInputsChangeScript + " });", true );
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService( new RockContext() );
            var adtypes = marketingCampaignAdTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlMarketingCampaignAdType.DataSource = adtypes;
            ddlMarketingCampaignAdType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="marketingCampaignAdId">The marketing campaign ad identifier.</param>
        public void ShowDetail( int marketingCampaignAdId )
        {
            ShowDetail( marketingCampaignAdId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="marketingCampaignAdId">The marketing campaign ad identifier.</param>
        /// <param name="marketingCampaignId">The marketing campaign id.</param>
        public void ShowDetail( int marketingCampaignAdId, int? marketingCampaignId )
        {
            pnlDetails.Visible = false;

            MarketingCampaignAd marketingCampaignAd = null;

            lbApprove.Visible = IsUserAuthorized( "Approve" );
            lbDeny.Visible = IsUserAuthorized( "Approve" );

            var rockContext = new RockContext();

            if ( !marketingCampaignAdId.Equals( 0 ) )
            {
                marketingCampaignAd = new MarketingCampaignAdService( rockContext ).Get( marketingCampaignAdId );
            }

            if (marketingCampaignAd == null && marketingCampaignId.HasValue )
            {
                marketingCampaignAd = new MarketingCampaignAd { Id = 0, MarketingCampaignAdStatus = MarketingCampaignAdStatus.PendingApproval };
                marketingCampaignAd.MarketingCampaignId = marketingCampaignId.Value;
                marketingCampaignAd.MarketingCampaign = new MarketingCampaignService( rockContext ).Get( marketingCampaignAd.MarketingCampaignId );
            }

            if ( marketingCampaignAd == null )
            {
                return;
            }

            marketingCampaignAd.LoadAttributes();

            pnlDetails.Visible = true;
            hfMarketingCampaignAdId.Value = marketingCampaignAd.Id.ToString();
            hfMarketingCampaignId.Value = marketingCampaignAd.MarketingCampaignId.ToString();

            if ( marketingCampaignAd.Id.Equals( 0 ) )
            {
                lActionTitleAd.Text = ActionTitle.Add( "Marketing Ad for " + marketingCampaignAd.MarketingCampaign.Title ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitleAd.Text = ActionTitle.Edit( "Marketing Ad for " + marketingCampaignAd.MarketingCampaign.Title ).FormatAsHtmlTitle();
            }

            LoadDropDowns();
            ddlMarketingCampaignAdType.SetValue( marketingCampaignAd.MarketingCampaignAdTypeId );
            tbPriority.Text = marketingCampaignAd.Priority.ToString();

            SetApprovalValues( marketingCampaignAd.MarketingCampaignAdStatus, new PersonService( rockContext ).Get( marketingCampaignAd.MarketingCampaignStatusPersonId ?? 0 ) );

            if ( marketingCampaignAdId.Equals( 0 ) )
            {
                drpAdDateRange.LowerValue = null;
                drpAdDateRange.UpperValue = null;
                dpAdSingleDate.SelectedDate = null;
            }
            else
            {
                drpAdDateRange.LowerValue = marketingCampaignAd.StartDate;
                drpAdDateRange.UpperValue = marketingCampaignAd.EndDate;
                dpAdSingleDate.SelectedDate = marketingCampaignAd.StartDate;
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

            MarketingCampaignAdType marketingCampaignAdType = new MarketingCampaignAdTypeService( new RockContext() ).Get( marketingAdTypeId );
            drpAdDateRange.Visible = marketingCampaignAdType.DateRangeType.Equals( DateRangeTypeEnum.DateRange );
            dpAdSingleDate.Visible = marketingCampaignAdType.DateRangeType.Equals( DateRangeTypeEnum.SingleDate );

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

            var rockContext = new RockContext();
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService( rockContext );

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
                ddlMarketingCampaignAdType.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlMarketingCampaignAdType.Label ) );
                return;
            }

            marketingCampaignAd.MarketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
            marketingCampaignAd.MarketingCampaignAdTypeId = int.Parse( ddlMarketingCampaignAdType.SelectedValue );
            marketingCampaignAd.Priority = tbPriority.Text.AsInteger();
            marketingCampaignAd.MarketingCampaignAdStatus = (MarketingCampaignAdStatus)int.Parse( hfApprovalStatus.Value );
            if ( !string.IsNullOrWhiteSpace( hfApprovalStatusPersonId.Value ) )
            {
                marketingCampaignAd.MarketingCampaignStatusPersonId = int.Parse( hfApprovalStatusPersonId.Value );
            }
            else
            {
                marketingCampaignAd.MarketingCampaignStatusPersonId = null;
            }

            if (drpAdDateRange.Visible)
            {
                marketingCampaignAd.StartDate = drpAdDateRange.LowerValue ?? DateTime.MinValue;
                marketingCampaignAd.EndDate = drpAdDateRange.UpperValue ?? DateTime.MaxValue;
            }

            if (dpAdSingleDate.Visible)
            {
                marketingCampaignAd.StartDate = dpAdSingleDate.SelectedDate ?? DateTime.MinValue;
                marketingCampaignAd.EndDate = marketingCampaignAd.StartDate;
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

            rockContext.WrapTransaction( () =>
            {
                if ( marketingCampaignAd.Id.Equals( 0 ) )
                {
                    marketingCampaignAdService.Add( marketingCampaignAd );
                }

                rockContext.SaveChanges();

                marketingCampaignAd.SaveAttributeValues( rockContext );
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
        /// Handles the Click event of the lbApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbApprove_Click( object sender, EventArgs e )
        {
            SetApprovalValues( MarketingCampaignAdStatus.Approved, CurrentPerson );
        }

        /// <summary>
        /// Handles the Click event of the lbDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeny_Click( object sender, EventArgs e )
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
            string cssClass = string.Empty;

            switch ( status )
            {
                case MarketingCampaignAdStatus.Approved: cssClass = "label label-success";
                    break;
                case MarketingCampaignAdStatus.Denied: cssClass = "label label-danger";
                    break;
                default: cssClass = "label label-info";
                    break;
            }

            lblApprovalStatus.Text = string.Format( "<span class='{0}'>{1}</span>", cssClass, status.ConvertToString() );

            hfApprovalStatus.Value = status.ConvertToInt().ToString();
            lblApprovalStatusPerson.Visible = person != null;
            if ( person != null )
            {
                lblApprovalStatusPerson.Text = "by " + person.FullName;
                hfApprovalStatusPersonId.Value = person.Id.ToString();
            }
        }

        #endregion
    }
}