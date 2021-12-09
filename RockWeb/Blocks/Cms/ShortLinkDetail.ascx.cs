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
using System.Data.Entity;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName("Short Link Detail")]
    [Category("CMS")]
    [Description("Displays the details for a specific short link.")]

    #region Block Attributes

    [IntegerField(
        "Minimum Token Length",
        Key = AttributeKey.MinimumTokenLength,
        Description = "The minimum number of characters for the token.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 0)]

    #endregion Block Attributes
    public partial class ShortLinkDetail : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string MinimumTokenLength = "MinimumTokenLength";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string ShortLinkId = "ShortLinkId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            RockPage.AddScriptLink( this.Page, "~/Scripts/clipboard.js/clipboard.min.js" );
            string script = @"
    new ClipboardJS('.js-copy-clipboard');
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
                ShowDetail( PageParameter( PageParameterKey.ShortLinkId ).AsInteger() );
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
            var shortLink = new PageShortLinkService( new RockContext() ).Get( int.Parse( hfShortLinkId.Value ) );
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
                PageShortLink link = null;

                using ( var rockContext = new RockContext() )
                {
                    var service = new PageShortLinkService( rockContext );

                    var errors = new List<string>();

                    int? linkId = hfShortLinkId.Value.AsIntegerOrNull();
                    if ( linkId.HasValue )
                    {
                        link = service.Get( linkId.Value );
                    }

                    if ( link == null )
                    {
                        link = new PageShortLink();
                        service.Add( link );
                    }

                    int? siteId = ddlSite.SelectedValueAsInt();
                    string token = tbToken.Text.Trim();
                    string url = tbUrl.Text.RemoveCrLf().Trim();

                    if ( !siteId.HasValue )
                    {
                        errors.Add( "Please select a valid site." );
                    }

                    int minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;
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
                        nbError.Text = "Please correct the following:<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
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
                var shortLink = new PageShortLinkService( new RockContext() ).Get( int.Parse( hfShortLinkId.Value ) );
                ShowReadonlyDetails( shortLink );
            }
        }

        protected void btnDelete_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new PageShortLinkService( rockContext );
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
            PageShortLink shortLink = null;

            if ( !shortLinkId.Equals( 0 ) )
            {
                shortLink = new PageShortLinkService( new RockContext() ).Get( shortLinkId );
                pdAuditDetails.SetEntity( shortLink, ResolveRockUrl( "~" ) );
            }

            if (shortLink == null )
            {
                shortLink = new PageShortLink { Id = 0 };

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfShortLinkId.Value = shortLink.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Rock.Model.PageShortLink.FriendlyTypeName );
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
        private void ShowEditDetails( Rock.Model.PageShortLink shortLink )
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
        private void ShowReadonlyDetails( Rock.Model.PageShortLink shortLink )
        {
            SetEditMode( false );

            hfShortLinkId.SetValue( shortLink.Id );

            lSite.Text = shortLink.Site.Name;
            lToken.Text = shortLink.Token;
            lUrl.Text = shortLink.Url;

            var url = shortLink.Site.DefaultDomainUri.ToString();
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
        }

        #endregion

    }
}