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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web;
using Rock.Web.Cache;
using System.IO;
using System.ComponentModel;
using Rock.Security;
using Rock.Attribute;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Short Link Detail")]
    [Category("CMS")]
    [Description("Displays the details for a specific short link.")]

    [IntegerField( "Minimum Token Length", "The minimum number of characters for the token.", false, 7, "", 0 )]
    public partial class ShortLinkDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = @"
    new Clipboard('.js-copy-clipboard');
    $('.js-copy-clipboard').tooltip();
";
            ScriptManager.RegisterStartupScript( btnCopy, btnCopy.GetType(), "copy-short-link", script, true );

            base.OnInit( e );
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "ShortLinkId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var shortLink = new SiteUrlMapService( new RockContext() ).Get( int.Parse( hfShortLinkId.Value ) );
            ShowEditDetails( shortLink );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                SiteUrlMap link = null;

                using ( var rockContext = new RockContext() )
                {
                    var service = new SiteUrlMapService( rockContext );

                    var errors = new List<string>();

                    int? linkId = hfShortLinkId.Value.AsIntegerOrNull();
                    if ( linkId.HasValue )
                    {
                        link = service.Get( linkId.Value );
                    }

                    if ( link == null )
                    {
                        link = new SiteUrlMap();
                        service.Add( link );
                    }

                    int? siteId = ddlSite.SelectedValueAsInt();
                    string token = tbToken.Text.Trim();
                    string url = tbUrl.Text.Trim();

                    if ( !siteId.HasValue )
                    {
                        errors.Add( "Please select a valid site." );
                    }

                    int minTokenLength = GetAttributeValue( "MinimumTokenLength" ).AsIntegerOrNull() ?? 7;
                    if ( token.IsNullOrWhiteSpace() || token.Length < minTokenLength )
                    {
                        errors.Add( string.Format( "Please enter a token that is a least {0} characters long.", minTokenLength ) );
                    }
                    else if ( siteId.HasValue && !service.VerifyUniqueToken( siteId.Value, link.Id, tbToken.Text ) )
                    {
                        errors.Add( "The selected token is already being used. Please enter a different token." );
                    }

                    if ( url.IsNullOrWhiteSpace() )
                    {
                        errors.Add( "Please enter a valid URL." );
                    }

                    if ( errors.Any() )
                    {
                        nbError.Text = "Please Correct the Following<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                        nbError.Visible = true;
                        return;
                    }

                    link.SiteId = siteId.Value;
                    link.Token = token;
                    link.Url = url;

                    if ( !link.IsValid )
                    {
                        return;
                    }

                    rockContext.SaveChanges();
                }

                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams["ShortLinkId"] = link.Id.ToString();
                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfShortLinkId.Value.Equals( "0" ) )
            {
                // Cancelling on Add
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit
                var shortLink = new SiteUrlMapService( new RockContext() ).Get( int.Parse( hfShortLinkId.Value ) );
                ShowReadonlyDetails( shortLink );
            }
        }

        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new SiteUrlMapService( rockContext );
                var link = service.Get( hfShortLinkId.ValueAsInt() );
                if ( link != null )
                {
                    service.Delete( link );
                    rockContext.SaveChanges();

                    NavigateToParentPage();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="shortLinkId">The shortLink identifier.</param>
        /// <param name="siteId">The group id.</param>
        public void ShowDetail( int shortLinkId )
        {
            SiteUrlMap shortLink = null;

            if ( !shortLinkId.Equals( 0 ) )
            {
                shortLink = new SiteUrlMapService( new RockContext() ).Get( shortLinkId );
                pdAuditDetails.SetEntity( shortLink, ResolveRockUrl( "~" ) );
            }

            if (shortLink == null )
            {
                shortLink = new SiteUrlMap { Id = 0 };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfShortLinkId.Value = shortLink.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Rock.Model.SiteUrlMap.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                //btnDelete.Visible = false;
                ShowReadonlyDetails( shortLink );
            }
            else
            {
                btnEdit.Visible = true;
                //btnDelete.Visible = !shortLink.IsSystem;
                if ( shortLink.Id > 0 )
                {
                    ShowReadonlyDetails( shortLink );
                }
                else
                {
                    ShowEditDetails( shortLink );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="shortLink">The shortLink.</param>
        private void ShowEditDetails( Rock.Model.SiteUrlMap shortLink )
        {
            SetEditMode( true );

            LoadSites();

            ddlSite.SetValue( shortLink.SiteId );
            tbToken.Text = shortLink.Token;
            tbUrl.Text = shortLink.Url;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="shortLink">The shortLink.</param>
        private void ShowReadonlyDetails( Rock.Model.SiteUrlMap shortLink )
        {
            SetEditMode( false );

            hfShortLinkId.SetValue( shortLink.Id );

            lSite.Text = shortLink.Site.Name;
            lToken.Text = shortLink.Token;
            lUrl.Text = shortLink.Url;

            var uri = shortLink.Site.DefaultDomainUri;
            var url = uri != null ? uri.ToString() : Rock.Web.Cache.GlobalAttributesCache.Read().GetValue( "PublicApplicationRoot" );
            string link = url.EnsureTrailingForwardslash() + shortLink.Token;

            btnCopy.Attributes["data-clipboard-text"] = link;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the sites.
        /// </summary>
        private void LoadSites()
        {
            ddlSite.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                foreach ( SiteCache site in new SiteService( rockContext ).Queryable().OrderBy( s => s.Name ).Select( a => a.Id ).ToList().Select( a => SiteCache.Read( a ) ) )
                {
                    ddlSite.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
                }
            }
        }

        #endregion

    }
}