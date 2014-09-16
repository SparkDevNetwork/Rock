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
    
    public partial class ContentItemDetail : RockBlock, IDetailBlock
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
                ShowDetail( PageParameter( "contentItemId" ).AsInteger(), PageParameter( "contentChannelId" ).AsIntegerOrNull() );
            }
            else
            {
                LoadAdAttributes( new ContentItem(), true, false );
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
            ContentTypeService contentTypeService = new ContentTypeService( new RockContext() );
            var adtypes = contentTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlContentType.DataSource = adtypes;
            ddlContentType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentItemId">The marketing campaign ad identifier.</param>
        public void ShowDetail( int contentItemId )
        {
            ShowDetail( contentItemId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentItemId">The marketing campaign ad identifier.</param>
        /// <param name="contentChannelId">The marketing campaign id.</param>
        public void ShowDetail( int contentItemId, int? contentChannelId )
        {
            pnlDetails.Visible = false;

            ContentItem contentItem = null;

            lbApprove.Visible = IsUserAuthorized( "Approve" );
            lbDeny.Visible = IsUserAuthorized( "Approve" );

            var rockContext = new RockContext();

            if ( !contentItemId.Equals( 0 ) )
            {
                contentItem = new ContentItemService( rockContext ).Get( contentItemId );
            }

            if (contentItem == null && contentChannelId.HasValue )
            {
                contentItem = new ContentItem { Id = 0, ContentItemStatus = ContentItemStatus.PendingApproval };
                contentItem.ContentChannelId = contentChannelId.Value;
                contentItem.ContentChannel = new ContentChannelService( rockContext ).Get( contentItem.ContentChannelId );
            }

            if ( contentItem == null )
            {
                return;
            }

            contentItem.LoadAttributes();

            pnlDetails.Visible = true;
            hfContentItemId.Value = contentItem.Id.ToString();
            hfContentChannelId.Value = contentItem.ContentChannelId.ToString();

            if ( contentItem.Id.Equals( 0 ) )
            {
                lActionTitleAd.Text = ActionTitle.Add( "Marketing Ad for " + contentItem.ContentChannel.Title ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitleAd.Text = ActionTitle.Edit( "Marketing Ad for " + contentItem.ContentChannel.Title ).FormatAsHtmlTitle();
            }

            LoadDropDowns();
            ddlContentType.SetValue( contentItem.ContentTypeId );
            tbPriority.Text = contentItem.Priority.ToString();

            SetApprovalValues( contentItem.ContentItemStatus, new PersonService( rockContext ).Get( contentItem.ContentChannelStatusPersonId ?? 0 ) );

            if ( contentItemId.Equals( 0 ) )
            {
                drpAdDateRange.LowerValue = null;
                drpAdDateRange.UpperValue = null;
                dpAdSingleDate.SelectedDate = null;
            }
            else
            {
                drpAdDateRange.LowerValue = contentItem.StartDate;
                drpAdDateRange.UpperValue = contentItem.EndDate;
                dpAdSingleDate.SelectedDate = contentItem.StartDate;
            }

            tbUrl.Text = contentItem.Url;

            LoadAdAttributes( contentItem, true, true );

            pnlDetails.Visible = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlContentType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ContentItem contentItem = new ContentItem();

            LoadAdAttributes( contentItem, false, false );
            Rock.Attribute.Helper.GetEditValues( phAttributes, contentItem );

            SetApprovalValues( ContentItemStatus.PendingApproval, null );

            LoadAdAttributes( contentItem, true, true );
        }

        /// <summary>
        /// Loads the attribute controls.
        /// </summary>
        /// <param name="contentItem">The marketing campaign ad.</param>
        /// <param name="createControls">if set to <c>true</c> [create controls].</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void LoadAdAttributes( Rock.Attribute.IHasAttributes contentItem, bool createControls, bool setValues )
        {
            if ( string.IsNullOrWhiteSpace( ddlContentType.SelectedValue ) )
            {
                return;
            }

            int marketingAdTypeId = int.Parse( ddlContentType.SelectedValue );

            ContentType contentType = new ContentTypeService( new RockContext() ).Get( marketingAdTypeId );
            drpAdDateRange.Visible = contentType.DateRangeType.Equals( DateRangeTypeEnum.DateRange );
            dpAdSingleDate.Visible = contentType.DateRangeType.Equals( DateRangeTypeEnum.SingleDate );

            List<Rock.Web.Cache.AttributeCache> attributesForAdType = GetAttributesForAdType( marketingAdTypeId );

            contentItem.Attributes = contentItem.Attributes ?? new Dictionary<string, Rock.Web.Cache.AttributeCache>();
            contentItem.AttributeValues = contentItem.AttributeValues ?? new Dictionary<string, AttributeValue>();
            foreach ( var attribute in attributesForAdType )
            {
                contentItem.Attributes.AddOrReplace( attribute.Key, attribute );

                if ( !contentItem.AttributeValues.ContainsKey( attribute.Key ) ||
                    contentItem.AttributeValues[attribute.Key] == null )
                {
                    contentItem.AttributeValues.AddOrReplace( attribute.Key, new AttributeValue { Value = attribute.DefaultValue } );
                }
            }

            if ( createControls )
            {
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentItem, phAttributes, setValues );
            }
        }

        /// <summary>
        /// Gets the type of the attributes for ad.
        /// </summary>
        /// <param name="marketingAdTypeId">The marketing ad type id.</param>
        /// <returns></returns>
        private static List<Rock.Web.Cache.AttributeCache> GetAttributesForAdType( int marketingAdTypeId )
        {
            ContentItem temp = new ContentItem();
            temp.ContentTypeId = marketingAdTypeId;
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
            int contentItemId = int.Parse( hfContentItemId.Value );

            var rockContext = new RockContext();
            ContentItemService contentItemService = new ContentItemService( rockContext );

            ContentItem contentItem;
            if ( contentItemId.Equals( 0 ) )
            {
                contentItem = new ContentItem { Id = 0 };
                contentItem.ContentChannelId = hfContentChannelId.ValueAsInt();
            }
            else
            {
                contentItem = contentItemService.Get( contentItemId );
            }

            if ( string.IsNullOrWhiteSpace( ddlContentType.SelectedValue ) )
            {
                ddlContentType.ShowErrorMessage( WarningMessage.CannotBeBlank( ddlContentType.Label ) );
                return;
            }

            contentItem.ContentChannelId = int.Parse( hfContentChannelId.Value );
            contentItem.ContentTypeId = int.Parse( ddlContentType.SelectedValue );
            contentItem.Priority = tbPriority.Text.AsInteger();
            contentItem.ContentItemStatus = (ContentItemStatus)int.Parse( hfApprovalStatus.Value );
            if ( !string.IsNullOrWhiteSpace( hfApprovalStatusPersonId.Value ) )
            {
                contentItem.ContentChannelStatusPersonId = int.Parse( hfApprovalStatusPersonId.Value );
            }
            else
            {
                contentItem.ContentChannelStatusPersonId = null;
            }

            if (drpAdDateRange.Visible)
            {
                contentItem.StartDate = drpAdDateRange.LowerValue ?? DateTime.MinValue;
                contentItem.EndDate = drpAdDateRange.UpperValue ?? DateTime.MaxValue;
            }

            if (dpAdSingleDate.Visible)
            {
                contentItem.StartDate = dpAdSingleDate.SelectedDate ?? DateTime.MinValue;
                contentItem.EndDate = contentItem.StartDate;
            }

            contentItem.Url = tbUrl.Text;

            LoadAdAttributes( contentItem, false, false );
            Rock.Attribute.Helper.GetEditValues( phAttributes, contentItem );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !contentItem.IsValid )
            {
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( contentItem.Id.Equals( 0 ) )
                {
                    contentItemService.Add( contentItem );
                }

                rockContext.SaveChanges();

                contentItem.SaveAttributeValues( rockContext );
            } );

            Dictionary<string, string> qryString = new Dictionary<string, string>();
            qryString["contentChannelId"] = hfContentChannelId.Value;
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
            qryString["contentChannelId"] = hfContentChannelId.Value;
            NavigateToParentPage( qryString );
        }

        /// <summary>
        /// Handles the Click event of the lbApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbApprove_Click( object sender, EventArgs e )
        {
            SetApprovalValues( ContentItemStatus.Approved, CurrentPerson );
        }

        /// <summary>
        /// Handles the Click event of the lbDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeny_Click( object sender, EventArgs e )
        {
            SetApprovalValues( ContentItemStatus.Denied, CurrentPerson );
        }

        /// <summary>
        /// Sets the approval values.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="person">The person.</param>
        private void SetApprovalValues( ContentItemStatus status, Person person )
        {
            string cssClass = string.Empty;

            switch ( status )
            {
                case ContentItemStatus.Approved: cssClass = "label label-success";
                    break;
                case ContentItemStatus.Denied: cssClass = "label label-danger";
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