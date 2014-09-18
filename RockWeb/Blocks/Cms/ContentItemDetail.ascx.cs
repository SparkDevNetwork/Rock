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
                if ( pnlEditDetails.Visible )
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
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetContentItem( hfContentItemId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentItem contentItem = null;

            ContentItemService contentItemService = new ContentItemService( rockContext );

            int contentItemId = hfContentItemId.Value.AsInteger();
             
            if ( contentItemId == 0 )
            {
                var contentChannel = new ContentChannelService(rockContext).Get( hfContentChannelId.Value.AsInteger() );
                if ( contentChannel != null )
                {
                    contentItem = new ContentItem
                    {
                        ContentChannel = contentChannel,
                        ContentChannelId = contentChannel.Id,
                        ContentType = contentChannel.ContentType,
                        ContentTypeId = contentChannel.ContentType.Id
                    };
                    contentItemService.Add( contentItem );
                }
            }
            else
            {
                contentItem = contentItemService.Get( contentItemId );
            }

            if ( contentItem != null )
            {
                contentItem.Title = tbTitle.Text;
                contentItem.Content = ceContent.Text;
                contentItem.StartDateTime = dtpStart.SelectedDateTime ?? RockDateTime.Now;
                contentItem.ExpireDateTime = dtpExpire.SelectedDateTime;
                contentItem.Permalink = tbPermalink.Text;

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

                ShowReadonlyDetails( GetContentItem( contentItem.Id ) );
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int contentItemId = hfContentItemId.ValueAsInt();
            if ( contentItemId != 0 )
            {
                ShowReadonlyDetails( GetContentItem( contentItemId ) );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int contentItemId = hfContentItemId.ValueAsInt();
            if ( contentItemId != 0 )
            {
                ShowReadonlyDetails( GetContentItem( contentItemId ) );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentItemId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentItem GetContentItem( int contentItemId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentItem = new ContentItemService( rockContext )
                .Queryable( "ContentChannel,ContentType" )
                .Where( t => t.Id == contentItemId )
                .FirstOrDefault();
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
            ContentItem contentItem = null;

            bool editAllowed = true;

            var rockContext = new RockContext();

            if ( !contentItemId.Equals( 0 ) )
            {
                contentItem = GetContentItem( contentItemId );
                if ( contentItem != null )
                {
                    editAllowed = contentItem.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            if ( contentItem == null && contentChannelId.HasValue )
            {
                var contentChannel = new ContentChannelService(rockContext).Get( contentChannelId.Value );
                if (contentChannel != null)
                {
                    contentItem = new ContentItem { 
                        Id = 0, 
                        ContentChannel = contentChannel,
                        ContentChannelId = contentChannel.Id,
                        ContentType = contentChannel.ContentType,
                        ContentTypeId = contentChannel.ContentType.Id
                    };
                }
            }

            if ( contentItem != null )
            {
                hfContentItemId.Value = contentItem.Id.ToString();
                hfContentChannelId.Value = contentItem.ContentChannelId.ToString();

                bool readOnly = false;
                nbEditModeMessage.Text = string.Empty;

                if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentItem.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    lbEdit.Visible = false;
                    ShowReadonlyDetails( contentItem );
                }
                else
                {
                    lbEdit.Visible = true;
                    if ( contentItem.Id > 0 )
                    {
                        ShowReadonlyDetails( contentItem );
                    }
                    else
                    {
                        ShowEditDetails( contentItem );
                    }
                }

                lbSave.Visible = !readOnly;
            }

        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="contentItem">Type of the content.</param>
        private void ShowReadonlyDetails( ContentItem contentItem )
        {
            SetEditMode( false );

            if ( contentItem != null )
            {
                hfContentItemId.SetValue( contentItem.Id );

                SetHeadingInfo( contentItem, contentItem.Title );
                SetEditMode( false );

                lContent.Text = contentItem.Content;

                var descriptionList = new DescriptionList()
                    .Add( "Start", contentItem.StartDateTime.ToShortDateString() + " " + contentItem.StartDateTime.ToShortTimeString() );
                
                if (contentItem.ExpireDateTime.HasValue)
                {
                    descriptionList.Add( "Expire", contentItem.ExpireDateTime.Value.ToShortDateString() + " " +
                        contentItem.ExpireDateTime.Value.ToShortTimeString() );
                }
                descriptionList.Add( "Permalink", contentItem.Permalink );
                lDetails.Text = descriptionList.Html;

                var attributeList = new DescriptionList();
                contentItem.LoadAttributes();
                foreach ( var attribute in contentItem.Attributes
                    .Where( a => a.Value.IsGridColumn )
                    .OrderBy( a => a.Value.Order )
                    .Select( a => a.Value ) )
                {
                    if ( contentItem.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        string value = attribute.FieldType.Field.FormatValue( null,
                            contentItem.AttributeValues[attribute.Key].Value, attribute.QualifierValues, false );
                        attributeList.Add( attribute.Name, value );
                    }
                }
                lAttributes.Text = attributeList.Html;

            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="contentItem">Type of the content.</param>
        protected void ShowEditDetails( ContentItem contentItem )
        {
            if ( contentItem != null )
            {
                hfContentItemId.Value = contentItem.Id.ToString();
                string title = contentItem.Id > 0 ?
                    ActionTitle.Edit( ContentItem.FriendlyTypeName ) :
                    ActionTitle.Add( ContentItem.FriendlyTypeName );

                SetHeadingInfo( contentItem, title );

                SetEditMode( true );

                tbTitle.Text = contentItem.Title;
                ceContent.Text = contentItem.Content;
                dtpStart.SelectedDateTime = contentItem.StartDateTime;
                dtpExpire.SelectedDateTime = contentItem.ExpireDateTime;
                tbPermalink.Text = contentItem.Permalink;

                contentItem.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentItem, phAttributes, true );
            }
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="contentItem">Type of the content.</param>
        /// <param name="title">The title.</param>
        private void SetHeadingInfo( ContentItem contentItem, string title )
        {
            string cssIcon = contentItem.ContentChannel.IconCssClass;
            if (string.IsNullOrWhiteSpace(cssIcon))
            {
                cssIcon = "fa fa-certificate";
            }
            lIcon.Text = string.Format("<i class='{0}'></i>", cssIcon);
            lTitle.Text = title.FormatAsHtmlTitle();
            hlContentChannel.Text = contentItem.ContentChannel.Name;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion


}
}