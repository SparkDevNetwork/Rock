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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Shortened Links" )]
    [Category( "Administration" )]
    [Description( "Displays a dialog for adding a short link to the current page." )]

    #region Block Attributes

    [IntegerField( "Minimum Token Length",
        Key = AttributeKey.MinimumTokenLength,
        Description = "The minimum number of characters for the token.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 0 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "86FB6B0E-E426-4581-96C0-A7654D6A5C7D" )]
    public partial class ShortLink : RockBlock
    {
        private int _minTokenLength = 7;

        private static class AttributeKey
        {
            public const string MinimumTokenLength = "MinimumTokenLength";
        }

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

            _minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;

            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = string.Format(
    @"function updateClipboardText() {{
        var scLink = $('#{0}').val() + $('#{1}').val();
        $('.js-copy-to-clipboard').attr('data-clipboard-text', scLink );
        $('.js-copy-to-clipboard').html(scLink);
        $('#btnSave').attr('data-clipboard-text', scLink );
    }}

    $('#{1}').on('input', function() {{ updateClipboardText(); }});

    new ClipboardJS('#btnSave');
    new ClipboardJS('.js-copy-to-clipboard');
    $('.js-copy-to-clipboard').tooltip();

    updateClipboardText();

",
hfSiteUrl.ClientID,
tbToken.ClientID );
            ScriptManager.RegisterStartupScript( tbToken, tbToken.GetType(), "save-short-link", script, true );

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                using ( var rockContext = new RockContext() )
                {
                    LoadSites( rockContext );
                    int? siteId = ddlSite.SelectedValueAsInt();
                    if ( siteId.HasValue )
                    {
                        tbToken.Text = new PageShortLinkService( rockContext ).GetUniqueToken( siteId.Value, _minTokenLength );
                    }
                }

                SetSiteUrl();

                string url = PageParameter( "Url" );
                if ( url.IsNotNullOrWhiteSpace() )
                {
                    tbUrl.Text = url;
                }

                tbToken.Focus();
            }

            base.OnLoad( e );
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
                PageShortLink link = null;

                using ( var rockContext = new RockContext() )
                {
                    var service = new PageShortLinkService( rockContext );

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
                        nbError.Text = "Please correct the following:<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                        nbError.Visible = true;
                        return;
                    }

                    link = new PageShortLink();
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
            foreach ( SiteCache site in new SiteService( rockContext )
                .Queryable().AsNoTracking()
                .Where( s => s.EnabledForShortening )
                .OrderBy( s => s.Name )
                .Select( a => a.Id )
                .ToList()
                .Select( a => SiteCache.Get( a ) ) )
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
                    hfSiteUrl.Value = new SiteService( rockContext ).GetDefaultDomainUri( siteId.Value ).ToString();
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