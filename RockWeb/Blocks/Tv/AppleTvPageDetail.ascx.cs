// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Linq;
using Rock.Web.UI;
using Rock.Web;
using Newtonsoft.Json;
using Rock.Utility;
using Rock.Common.Tv;
using Rock.Tv;

namespace RockWeb.Blocks.Tv
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Apple TV Page Detail" )]
    [Category( "TV > TV Apps" )]
    [Description( "Allows a person to edit an Apple TV page." )]

    #region Block Attributes

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "23CA8858-6D02-48A8-92C4-CE415DAB41B6" )]
    public partial class AppleTvPageDetail : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            //public const string ShowEmailAddress = "ShowEmailAddress";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string SiteId = "SiteId";
            public const string SitePageId = "SitePageId";
        }

        #endregion PageParameterKeys

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowEdit();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? pageId = PageParameter( pageReference, PageParameterKey.SitePageId ).AsIntegerOrNull();

            if ( pageId != null )
            {
                var detailBreadCrumb = pageReference.BreadCrumbs.FirstOrDefault( x => x.Name == "Application Screen Detail" );
                if ( detailBreadCrumb != null )
                {
                    pageReference.BreadCrumbs.Remove( detailBreadCrumb );
                }

                var page = PageCache.Get( pageId.Value );

                if ( page != null )
                {
                    breadCrumbs.Add( new BreadCrumb( page.InternalName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Page", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowEdit();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAndEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAndEdit_Click( object sender, EventArgs e )
        {
            SavePage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {

            SavePage();

            // If the save was successful, reload the page using the new record Id.
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();

            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.SiteId] = applicationId.ToString();
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            // If the save was successful, reload the page using the new record Id.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.SiteId] = PageParameter( PageParameterKey.SiteId );

            NavigateToParentPage( qryParams );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Saves the page.
        /// </summary>
        private void SavePage()
        {
            var applicationId = PageParameter( PageParameterKey.SiteId ).AsInteger();
            var pageId = PageParameter( PageParameterKey.SitePageId ).AsInteger();

            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            var page = pageService.Get( pageId );

            var site = SiteCache.Get( applicationId );

            // Site is new so create one
            if ( page == null )
            {
                page = new Rock.Model.Page();
                pageService.Add( page );
                page.ParentPageId = site.DefaultPageId;
                page.LayoutId = site.DefaultPage.LayoutId;

                // Set the order of the new page to be the last one
                var currentMaxOrder = pageService.GetByParentPageId( site.DefaultPageId )
                    .OrderByDescending( p => p.Order )
                    .Select( p => p.Order )
                    .FirstOrDefault();
                page.Order = currentMaxOrder + 1;
            }

            page.InternalName = tbPageName.Text;
            page.BrowserTitle = tbPageName.Text;
            page.PageTitle = tbPageName.Text;

            page.Description = ceTvml.Text;

            var pageResponse = page.GetAdditionalSettings<AppleTvPageSettings>();
            pageResponse.Content = ceTvml.Text;

            page.SetAdditionalSettings( pageResponse );

            page.Description = tbDescription.Text;

            page.DisplayInNavWhen = cbShowInMenu.Checked ? DisplayInNavWhen.WhenAllowed : DisplayInNavWhen.Never;

            page.CacheControlHeaderSettings = cpCacheSettings.CurrentCacheability.ToJson();

            avcAttributes.GetEditValues( page );
            page.SaveAttributeValues();

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Edit the site.
        /// </summary>
        private void ShowEdit()
        {
            var pageId = PageParameter( PageParameterKey.SitePageId ).AsInteger();

            if ( pageId != 0 )
            {
                var page = new PageService( new RockContext() ).Get( pageId );

                if ( page != null )
                {
                    tbDescription.Text = page.Description;

                    var pageResponse = page.GetAdditionalSettings<AppleTvPageSettings>();

                    ceTvml.Text = pageResponse.Content;
                    tbPageName.Text = page.InternalName;
                    //
                    // Configure Copy Page Guid
                    //
                    RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
                    string script = string.Format( @"
    new ClipboardJS('#{0}');
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
                    ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );

                    btnCopyToClipboard.Attributes["data-clipboard-text"] = page.Guid.ToString();
                    btnCopyToClipboard.Attributes["title"] = string.Format( "Copy the Guid {0} to the clipboard.", page.Guid.ToString() );

                    cbShowInMenu.Checked = page.DisplayInNavWhen == DisplayInNavWhen.WhenAllowed;

                    page.LoadAttributes();
                    avcAttributes.AddEditControls( page, Rock.Security.Authorization.EDIT, CurrentPerson );
                }

                if ( page.CacheControlHeaderSettings != null )
                {
                    cpCacheSettings.CurrentCacheability = JsonConvert.DeserializeObject<RockCacheability>( page.CacheControlHeaderSettings );
                }
                else
                {
                    cpCacheSettings.CurrentCacheability = new RockCacheability
                    {
                        RockCacheablityType = RockCacheablityType.Private
                    };
                }
            }
            else
            {
                btnCopyToClipboard.Visible = false;
            }
        }

        #endregion
    }
}