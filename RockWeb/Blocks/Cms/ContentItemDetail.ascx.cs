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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using System.ComponentModel;
using Rock.Security;
using Newtonsoft.Json;
using Rock.Web;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Item Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content item.")]
    public partial class ContentItemDetail : RockBlock, IDetailBlock
    {

        #region Fields

        protected string PendingCss = "btn-default";
        protected string ApprovedCss = "btn-default";
        protected string DeniedCss = "btn-default";

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string script = string.Format( @"
    $('#{0} .btn-toggle').click(function (e) {{

        e.stopImmediatePropagation();

        $(this).find('.btn').removeClass('active');
        $(e.target).addClass('active');

        $(this).find('a').each(function() {{
            if ($(this).hasClass('active')) {{
                $('#{1}').val($(this).attr('data-status'));
                $(this).removeClass('btn-default');
                $(this).addClass( $(this).attr('data-active-css') );
            }} else {{
                $(this).removeClass( $(this).attr('data-active-css') );
                $(this).addClass('btn-default');
            }}
        }});

    }});
", pnlStatus.ClientID, hfStatus.ClientID );
            ScriptManager.RegisterStartupScript( pnlStatus, pnlStatus.GetType(), "status-script-" + this.BlockId.ToString(), script, true );

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
                ShowDetail( PageParameter( "contentItemId" ).AsInteger(), PageParameter("contentChannelId").AsIntegerOrNull() );
            }
            else
            {
                var rockContext = new RockContext();
                ContentItem item;
                int? itemId = PageParameter( "contentItemId" ).AsIntegerOrNull();
                if ( itemId.HasValue && itemId.Value > 0 )
                {
                    item = new ContentItemService( rockContext ).Get( itemId.Value );
                }
                else
                {
                    item = new ContentItem { Id = 0, ContentChannelId = PageParameter("contentChannelId").AsInteger() };
                }
                item.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( item, phAttributes, false );
            }
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? contentItemId = PageParameter( pageReference, "contentItemId" ).AsIntegerOrNull();
            if ( contentItemId != null )
            {
                ContentItem contentItem = new ContentItemService( new RockContext() ).Get( contentItemId.Value );
                if ( contentItem != null )
                {
                    breadCrumbs.Add( new BreadCrumb( contentItem.Title, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Content Item", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentItem contentItem = GetContentItem( rockContext );

            if ( contentItem != null && contentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                contentItem.Title = tbTitle.Text;
                contentItem.Content = ceContent.Text;
                contentItem.Priority = nbPriority.Text.AsInteger();
                contentItem.StartDateTime = dtpStart.SelectedDateTime ?? RockDateTime.Now;
                contentItem.ExpireDateTime = dtpExpire.SelectedDateTime;

                int newStatusID = hfStatus.Value.AsIntegerOrNull() ?? 1;
                int oldStatusId = contentItem.Status.ConvertToInt();
                if ( newStatusID != oldStatusId && contentItem.IsAuthorized(Authorization.APPROVE, CurrentPerson))
                {
                    contentItem.Status = hfStatus.Value.ConvertToEnum<ContentItemStatus>( ContentItemStatus.PendingApproval );
                    if ( contentItem.Status == ContentItemStatus.PendingApproval )
                    {
                        contentItem.ApprovedDateTime = null;
                        contentItem.ApprovedByPersonAliasId = null;
                    }
                    else
                    {
                        contentItem.ApprovedDateTime = RockDateTime.Now;
                        contentItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    }
                }

                contentItem.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, contentItem );

                if ( !Page.IsValid || !contentItem.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    contentItem.SaveAttributeValues( rockContext );
                } );

                ReturnToParentPage();
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            ReturnToParentPage();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfContentItemId.ValueAsInt() );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentItemId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentItem GetContentItem( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentItemService = new ContentItemService( rockContext );
            ContentItem contentItem = null;

            int contentItemId = hfContentItemId.Value.AsInteger();
            if ( contentItemId != 0 )
            {
                contentItem = contentItemService
                    .Queryable( "ContentChannel,ContentType" )
                    .FirstOrDefault( t => t.Id == contentItemId );
            }

            if ( contentItem == null)
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( hfContentChannelId.Value.AsInteger() );
                if ( contentChannel != null )
                {
                    contentItem = new ContentItem
                    {
                        ContentChannel = contentChannel,
                        ContentChannelId = contentChannel.Id,
                        ContentType = contentChannel.ContentType,
                        ContentTypeId = contentChannel.ContentType.Id,
                        StartDateTime = RockDateTime.Now
                    };

                    if ( contentChannel.RequiresApproval )
                    {
                        contentItem.Status = ContentItemStatus.PendingApproval;
                    }
                    else
                    {
                        contentItem.Status = ContentItemStatus.Approved;
                        contentItem.ApprovedDateTime = RockDateTime.Now;
                        contentItem.ApprovedByPersonAliasId = CurrentPersonAliasId;
                    }

                    contentItemService.Add( contentItem );
                }
            }

            return contentItem;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentItemId">The marketing campaign ad type identifier.</param>
        public void ShowDetail( int contentItemId )
        {
            ShowDetail( contentItemId, null );
        }

        public void ShowDetail( int contentItemId, int? contentChannelId )
        {
            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            hfContentItemId.Value = contentItemId.ToString();
            hfContentChannelId.Value = contentChannelId.HasValue ? contentChannelId.Value.ToString() : string.Empty;

            ContentItem contentItem = GetContentItem();

            if ( contentItem != null &&
                contentItem.ContentType != null &&
                contentItem.ContentChannel != null &&
                ( canEdit || contentItem.IsAuthorized( Authorization.EDIT, CurrentPerson ) ) ) 
            {
                ShowApproval( contentItem );

                pnlEditDetails.Visible = true;

                hfContentItemId.Value = contentItem.Id.ToString();
                hfContentChannelId.Value = contentItem.ContentChannelId.ToString();

                string cssIcon = contentItem.ContentChannel.IconCssClass;
                if ( string.IsNullOrWhiteSpace( cssIcon ) )
                {
                    cssIcon = "fa fa-certificate";
                }
                lIcon.Text = string.Format( "<i class='{0}'></i>", cssIcon );

                string title = contentItem.Id > 0 ?
                    ActionTitle.Edit( ContentItem.FriendlyTypeName ) :
                    ActionTitle.Add( ContentItem.FriendlyTypeName );
                lTitle.Text = title.FormatAsHtmlTitle();

                hlContentChannel.Text = contentItem.ContentChannel.Name;

                tbTitle.Text = contentItem.Title;
                ceContent.Text = contentItem.Content;
                nbPriority.Text = contentItem.Priority.ToString();
                dtpStart.SelectedDateTime = contentItem.StartDateTime;

                dtpExpire.Visible = contentItem.ContentType.DateRangeType == DateRangeTypeEnum.DateRange;

                dtpExpire.SelectedDateTime = contentItem.ExpireDateTime;

                contentItem.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentItem, phAttributes, true );
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToEdit( ContentItem.FriendlyTypeName );
                pnlEditDetails.Visible = false;
            }
        }

        private void ShowApproval( ContentItem contentItem )
        {
            if ( contentItem != null &&
                contentItem.ContentChannel != null &&
                contentItem.ContentChannel.RequiresApproval )
            {

                var statusDetail = new System.Text.StringBuilder();

                if ( contentItem.IsAuthorized( Authorization.APPROVE, CurrentPerson ) )
                {
                    pnlStatus.Visible = true;

                    PendingCss = contentItem.Status == ContentItemStatus.PendingApproval ? "btn-default active" : "btn-default";
                    ApprovedCss = contentItem.Status == ContentItemStatus.Approved ? "btn-success active" : "btn-default";
                    DeniedCss = contentItem.Status == ContentItemStatus.Denied ? "btn-danger active" : "btn-default";
                }
                else
                {
                    pnlStatus.Visible = false;

                    string labelCss = "default";
                    if ( contentItem.Status == ContentItemStatus.Approved )
                    {
                        labelCss = "success";
                    }
                    else if ( contentItem.Status == ContentItemStatus.Denied )
                    {
                        labelCss = "danger";
                    }
                    statusDetail.AppendFormat( "<span class='label label-{0}'>{1}</span> ", labelCss, contentItem.Status.ConvertToString() );
                }

                if ( contentItem.Status != ContentItemStatus.PendingApproval )
                {
                    if ( contentItem.ApprovedByPersonAlias != null && contentItem.ApprovedByPersonAlias.Person != null )
                    {
                        statusDetail.AppendFormat( "by {0} ", contentItem.ApprovedByPersonAlias.Person.FullName );
                    }
                    if ( contentItem.ApprovedDateTime.HasValue )
                    {
                        statusDetail.AppendFormat( "on {0} at {1}", contentItem.ApprovedDateTime.Value.ToShortDateString(),
                            contentItem.ApprovedDateTime.Value.ToShortTimeString() );
                    }
                }

                lStatusDetails.Visible = true;
                lStatusDetails.Text = statusDetail.ToString();

            }
            else
            {
                pnlStatus.Visible = false;
                lStatusDetails.Visible = false;
                divStatus.Visible = false;
            }
        }


        /// <summary>
        /// Returns to parent page.
        /// </summary>
        private void ReturnToParentPage()
        {
            var qryParams = new Dictionary<string,string>();
            qryParams.Add( "contentChannelId", hfContentChannelId.Value );
            NavigateToParentPage( qryParams );
        }

        #endregion

}
}