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
using System.Linq;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Services.NuGet;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Shortened Links" )]
    [Category( "Administration" )]
    [Description( "Displays a dialog for adding a short link to the current page." )]

    [IntegerField( "Minimum Token Length", "The minimum number of characters for the token.", false, 7, "", 0 )]
    public partial class ShortLink : RockBlock
    {
        private int _minTokenLength = 7;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            DialogPage dialogPage = this.Page as DialogPage;
            if ( dialogPage != null )
            {
                dialogPage.OnSave += new EventHandler<EventArgs>( masterPage_OnSave );
            }

            _minTokenLength = GetAttributeValue( "MinimumTokenLength" ).AsIntegerOrNull() ?? 7;

            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format( @"
    function updateClipboardText() {{
        $('#btnSave').attr('data-clipboard-text', $('#{0}').val() + $('#{1}').val() );
    }}

    $('#{1}').on('input', function() {{ updateClipboardText(); }});

    new Clipboard('#btnSave');
    updateClipboardText();
", hfSiteUrl.ClientID, tbToken.ClientID );
            ScriptManager.RegisterStartupScript( tbToken, tbToken.GetType(), "save-short-link", script, true );

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
                using ( var rockContext = new RockContext() )
                {
                    LoadSites( rockContext );
                    int? siteId = ddlSite.SelectedValueAsInt();
                    if ( siteId.HasValue )
                    {
                        tbToken.Text = new SiteUrlMapService( rockContext ).GetUniqueToken( siteId.Value, _minTokenLength );
                    }
                }

                SetSiteUrl();

                string url = PageParameter( "url" );
                if ( url.IsNotNullOrWhitespace() )
                {
                    tbUrl.Text = url;
                }

                tbToken.Focus();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSite control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSite_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetSiteUrl();
        }

        /// <summary>
        /// Handles the OnSave event of the masterPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void masterPage_OnSave( object sender, EventArgs e )
        {
            Page.Validate( BlockValidationGroup );
            if ( Page.IsValid )
            {
                SiteUrlMap link = null;

                using ( var rockContext = new RockContext() )
                {
                    var service = new SiteUrlMapService( rockContext );

                    var errors = new List<string>();

                    int? siteId = ddlSite.SelectedValueAsInt();
                    string token = tbToken.Text.Trim();
                    string url = tbUrl.Text.Trim();

                    if ( !siteId.HasValue )
                    {
                        errors.Add( "Please select a valid site." );
                    }

                    if ( token.IsNullOrWhiteSpace() || token.Length < _minTokenLength )
                    {
                        errors.Add( string.Format( "Please enter a token that is a least {0} characters long.", _minTokenLength ) );
                    }
                    else if ( siteId.HasValue && !service.VerifyUniqueToken( siteId.Value, 0, tbToken.Text ) )
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

                    link = new SiteUrlMap();
                    link.SiteId = siteId.Value;
                    link.Token = token;
                    link.Url = url;

                    service.Add( link );
                    rockContext.SaveChanges();
                }

                if ( link != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        string script = "window.parent.Rock.controls.modal.close();";
                        ScriptManager.RegisterStartupScript( this.Page, this.GetType(), "close-modal", script, true );
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the sites.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void LoadSites( RockContext rockContext )
        {
            ddlSite.Items.Clear();
            foreach ( SiteCache site in new SiteService( rockContext ).Queryable().OrderBy( s => s.Name ).Select( a => a.Id ).ToList().Select( a => SiteCache.Read( a ) ) )
            {
                ddlSite.Items.Add( new ListItem( site.Name, site.Id.ToString() ) );
            }
        }

        private void SetSiteUrl()
        {
            int? siteId = ddlSite.SelectedValueAsInt();
            if ( siteId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var siteUrl = new SiteService( rockContext ).GetDefaultDomainUri( siteId.Value ) ??
                        new Uri( Request.Url.GetLeftPart( UriPartial.Authority ) );
                    hfSiteUrl.Value = siteUrl.ToString();
                }
            }
            else
            {
                hfSiteUrl.Value = string.Empty;

            }
        }

        #endregion

    }
}