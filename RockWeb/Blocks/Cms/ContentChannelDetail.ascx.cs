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
    [DisplayName("Content Channel Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content channel.")]
    public partial class ContentChannelDetail : RockBlock, IDetailBlock
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

            string script = @"
    $('.js-content-channel-enable-rss').change( function() {
        $(this).closest('div.form-group').siblings('div.js-content-channel-rss').slideToggle()
    });
";
            ScriptManager.RegisterStartupScript( cbEnableRss, cbEnableRss.GetType(), "enable-rss", script, true );

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
                ShowDetail( PageParameter( "contentChannelId" ).AsInteger(), PageParameter("contentTypeId").AsIntegerOrNull() );
            }
            else
            {
                if ( pnlEditDetails.Visible )
                {
                    var rockContext = new RockContext();
                    ContentChannel channel;
                    int? channelId = PageParameter( "contentChannelId" ).AsIntegerOrNull();
                    if ( channelId.HasValue && channelId.Value > 0 )
                    {
                        channel = new ContentChannelService( rockContext ).Get( channelId.Value );
                    }
                    else
                    {
                        channel = new ContentChannel { Id = 0, ContentTypeId = PageParameter("contentTypeId").AsInteger() };
                    }
                    channel.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( channel, phAttributes, false );

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

            int? contentChannelId = PageParameter( pageReference, "contentChannelId" ).AsIntegerOrNull();
            if ( contentChannelId != null )
            {
                ContentChannel contentChannel = new ContentChannelService( new RockContext() ).Get( contentChannelId.Value );
                if ( contentChannel != null )
                {
                    breadCrumbs.Add( new BreadCrumb( contentChannel.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Content Channel", pageReference ) );
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
            ShowEditDetails( GetContentChannel( hfContentChannelId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannel contentChannel;

            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            int contentChannelId = hfContentChannelId.Value.AsInteger();
             
            if ( contentChannelId == 0 )
            {
                contentChannel = new ContentChannel { ContentTypeId = hfContentTypeId.ValueAsInt() };
                contentChannelService.Add( contentChannel );
            }
            else
            {
                contentChannel = contentChannelService.Get( contentChannelId );
            }

            if ( contentChannel != null )
            {
                contentChannel.Name = tbName.Text;
                contentChannel.Description = tbDescription.Text;
                contentChannel.IconCssClass = tbIconCssClass.Text;
                contentChannel.RequiresApproval = cbRequireApproval.Checked;
                contentChannel.EnableRss = cbEnableRss.Checked;
                contentChannel.ChannelUrl = tbChannelUrl.Text;
                contentChannel.ItemUrl = tbItemUrl.Text;
                contentChannel.TimeToLive = nbTimetoLive.Text.AsIntegerOrNull();

                contentChannel.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, contentChannel );

                if ( !Page.IsValid || !contentChannel.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    contentChannel.SaveAttributeValues( rockContext );
                } );

                var pageReference = RockPage.PageReference;
                pageReference.Parameters.AddOrReplace( "contentChannelId", contentChannel.Id.ToString() );
                Response.Redirect( pageReference.BuildUrl(), false );
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int contentChannelId = hfContentChannelId.ValueAsInt();
            if ( contentChannelId != 0 )
            {
                ShowReadonlyDetails( GetContentChannel( contentChannelId ) );
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
            int contentChannelId = hfContentChannelId.ValueAsInt();
            if ( contentChannelId != 0 )
            {
                ShowReadonlyDetails( GetContentChannel( contentChannelId ) );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentChannelId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentChannel GetContentChannel( int contentChannelId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentChannel = new ContentChannelService( rockContext )
                .Queryable( "ContentType" )
                .Where( t => t.Id == contentChannelId )
                .FirstOrDefault();
            return contentChannel;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentChannelId">The marketing campaign ad type identifier.</param>
        public void ShowDetail( int contentChannelId )
        {
            ShowDetail( contentChannelId, null );
        }

        public void ShowDetail( int contentChannelId, int? contentTypeId )
        {
            ContentChannel contentChannel = null;

            bool editAllowed = true;

            var rockContext = new RockContext();

            if ( !contentChannelId.Equals( 0 ) )
            {
                contentChannel = GetContentChannel( contentChannelId );
                if ( contentChannel != null )
                {
                    editAllowed = contentChannel.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            if ( contentChannel == null && contentTypeId.HasValue )
            {
                var contentType = new ContentTypeService(rockContext).Get( contentTypeId.Value );
                if (contentType != null)
                {
                    contentChannel = new ContentChannel { 
                        Id = 0, 
                        ContentType = contentType,
                        ContentTypeId = contentType.Id
                    };
                }
            }

            if ( contentChannel != null )
            {
                hfContentChannelId.Value = contentChannel.Id.ToString();
                hfContentTypeId.Value = contentChannel.ContentTypeId.ToString();

                bool readOnly = false;
                nbEditModeMessage.Text = string.Empty;

                if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentChannel.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    lbEdit.Visible = false;
                    ShowReadonlyDetails( contentChannel );
                }
                else
                {
                    lbEdit.Visible = true;
                    if ( contentChannel.Id > 0 )
                    {
                        ShowReadonlyDetails( contentChannel );
                    }
                    else
                    {
                        ShowEditDetails( contentChannel );
                    }
                }

                lbSave.Visible = !readOnly;
            }

        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        private void ShowReadonlyDetails( ContentChannel contentChannel )
        {
            SetEditMode( false );

            if ( contentChannel != null )
            {
                hfContentChannelId.SetValue( contentChannel.Id );

                SetHeadingInfo( contentChannel, contentChannel.Name );
                SetEditMode( false );

                lGroupDescription.Text = contentChannel.Description;

                var descriptionList = new DescriptionList();
                if ( contentChannel.EnableRss )
                {
                    descriptionList
                    .Add( "Item's Require Approval", contentChannel.RequiresApproval.ToString() )
                    .Add( "Channel Url", contentChannel.ChannelUrl )
                    .Add( "Item Url", contentChannel.ItemUrl );
                }

                contentChannel.LoadAttributes();
                foreach ( var attribute in contentChannel.Attributes
                    .Where( a => a.Value.IsGridColumn )
                    .OrderBy( a => a.Value.Order )
                    .Select( a => a.Value ) )
                {
                    if ( contentChannel.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        string value = attribute.FieldType.Field.FormatValue( null,
                            contentChannel.AttributeValues[attribute.Key].Value, attribute.QualifierValues, false );
                        descriptionList.Add( attribute.Name, value );
                    }
                }

                lDetails.Text = descriptionList.Html;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        protected void ShowEditDetails( ContentChannel contentChannel )
        {
            if ( contentChannel != null )
            {
                hfContentChannelId.Value = contentChannel.Id.ToString();
                string title = contentChannel.Id > 0 ?
                    ActionTitle.Edit( ContentChannel.FriendlyTypeName ) :
                    ActionTitle.Add( ContentChannel.FriendlyTypeName );

                SetHeadingInfo( contentChannel, title );

                SetEditMode( true );

                tbName.Text = contentChannel.Name;
                tbDescription.Text = contentChannel.Description;
                tbIconCssClass.Text = contentChannel.IconCssClass;
                cbRequireApproval.Checked = contentChannel.RequiresApproval;
                cbEnableRss.Checked = contentChannel.EnableRss;

                divRss.Attributes["style"] = cbEnableRss.Checked ? "display:block" : "display:none";
                tbChannelUrl.Text = contentChannel.ChannelUrl;
                tbItemUrl.Text = contentChannel.ItemUrl;
                nbTimetoLive.Text = ( contentChannel.TimeToLive ?? 0 ).ToString();

                contentChannel.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( contentChannel, phAttributes, true );
            }
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        /// <param name="title">The title.</param>
        private void SetHeadingInfo( ContentChannel contentChannel, string title )
        {
            string cssIcon = contentChannel.IconCssClass;
            if (string.IsNullOrWhiteSpace(cssIcon))
            {
                cssIcon = "fa fa-bullhorn";
            }
            lIcon.Text = string.Format("<i class='{0}'></i>", cssIcon);
            lTitle.Text = title.FormatAsHtmlTitle();
            hlContentType.Text = contentChannel.ContentType.Name;
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