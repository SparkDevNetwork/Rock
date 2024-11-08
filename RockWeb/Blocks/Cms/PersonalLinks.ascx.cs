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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Personal Links" )]
    [Category( "CMS" )]
    [Description( "This block is used to show both personal and shared bookmarks as well as 'Quick Return' links." )]

    #region Block Attributes

    [LinkedPage(
        "Manage Links Page",
        Description = "The page where a person can manage their sections and personal links.",
        Order = 0,
        Key = AttributeKey.ManageLinksPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "4D42DF90-97A3-470B-A7D4-A6FD00673761" )]
    public partial class PersonalLinks : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ManageLinksPage = "ManageLinksPage";
        }

        #endregion Attribute Keys

        #region Data Attribute Keys

        private static class DataAttributeKey
        {
            public const string LastSharedLinkUpdateDateTime = "data-last-shared-link-update-rock-date-time";
            public const string QuickLinksLocalStorageKey = "data-quick-links-local-storage-key";
            public const string PersonalLinksModificationHash = "data-personal-links-modification-hash";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // We don't bundle personalLinks.js, we'll only add it as needed.
            // We only need it on this block, and for any page that uses RockFilters.AddQuickReturn lava filter.
            RockPage.AddScriptLink( "~/Scripts/Rock/Controls/PersonalLinks/personalLinks.js" );

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
            SetDataAttributes();

            if ( !Page.IsPostBack )
            {
                if ( CurrentPersonAliasId.HasValue )
                {
                    lbBookmark.Visible = true;
                    ShowView();
                }
            }

            base.OnLoad( e );
        }

        protected override void OnPreRender( EventArgs e )
        {
            // update this on every load in pre-render since it could change due to other blocks
            SetDataAttributes();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the lbManageLinks control.
        /// </summary>
        protected void lbManageLinks_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ManageLinksPage );
        }

        /// <summary>
        /// Handles the Click event of the lbAddSection control.
        /// </summary>
        protected void lbAddSection_Click( object sender, EventArgs e )
        {
            pnlView.Visible = false;
            pnlAddSection.Visible = true;
            pnlAddLink.Visible = false;
            tbSectionName.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the lbAddLink control.
        /// </summary>
        protected void lbAddLink_Click( object sender, EventArgs e )
        {
            pnlView.Visible = false;
            pnlAddSection.Visible = false;
            pnlAddLink.Visible = true;
            tbLinkName.Text = RockPage.Title;
            urlLink.Text = Page.Request.UrlProxySafe().ToString();
            BindSectionDropdown();
        }

        /// <summary>
        /// Handles the Click event of the btnSectionSave control.
        /// </summary>
        protected void btnSectionSave_Click( object sender, EventArgs e )
        {
            SaveSection( tbSectionName.Text );
            ShowView();
            pnlView.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnLinkSave control.
        /// </summary>
        protected void btnLinkSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personalLinkService = new PersonalLinkService( rockContext );
            var sectionId = ddlSection.SelectedValueAsId();
            if ( !sectionId.HasValue )
            {
                sectionId = SaveSection( "Links" );
            }

            if ( sectionId.HasValue )
            {
                var personalLink = new PersonalLink()
                {
                    Id = 0,
                    SectionId = sectionId.Value,
                    PersonAliasId = CurrentPersonAliasId.Value,
                    Name = tbLinkName.Text,
                    Url = urlLink.Text
                };

                personalLinkService.Add( personalLink );
                rockContext.SaveChanges();

                ShowView();
                pnlView.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlView.Visible = true;
            pnlAddSection.Visible = false;
            pnlAddLink.Visible = false;
            pnlView.RemoveCssClass( "d-none" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the data attributes.
        /// </summary>
        private void SetDataAttributes()
        {
            divPersonalLinks.Attributes[DataAttributeKey.LastSharedLinkUpdateDateTime] = SharedPersonalLinkSectionCache.LastModifiedDateTime.ToISO8601DateString();
            divPersonalLinks.Attributes[DataAttributeKey.QuickLinksLocalStorageKey] = PersonalLinkService.GetQuickLinksLocalStorageKey( this.CurrentPerson );
            divPersonalLinks.Attributes[DataAttributeKey.PersonalLinksModificationHash] = PersonalLinkService.GetPersonalLinksModificationHash( this.CurrentPerson );
            upnlContent.Update();
        }

        /// <summary>
        /// Sets View mode
        /// </summary>
        private void ShowView()
        {
            pnlAddSection.Visible = false;
            pnlAddLink.Visible = false;
        }

        /// <summary>
        /// Save the section.
        /// </summary>
        private int SaveSection( string sectionName )
        {
            var rockContext = new RockContext();
            var personalLinkSectionService = new PersonalLinkSectionService( rockContext );
            var personalLinkSection = new PersonalLinkSection()
            {
                Id = 0,
                IsShared = false,
                PersonAliasId = CurrentPersonAliasId.Value,
                Name = sectionName
            };

            personalLinkSectionService.Add( personalLinkSection );
            rockContext.SaveChanges();

            personalLinkSection.MakePrivate( Authorization.VIEW, CurrentPerson, rockContext );
            personalLinkSection.MakePrivate( Authorization.EDIT, CurrentPerson, rockContext );
            personalLinkSection.MakePrivate( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
            return personalLinkSection.Id;
        }

        /// <summary>
        /// Binds the connection types repeater.
        /// </summary>
        private void BindSectionDropdown()
        {
            var rockContext = new RockContext();
            var personalLinkService = new PersonalLinkService( rockContext );
            var sectionsQuery = personalLinkService.GetOrderedPersonalLinkSectionsQuery( this.CurrentPerson );

            // limit to ones that are non-shared
            var orderedPersonalLinkSections = sectionsQuery
                .AsNoTracking()
                .ToList()
                .Where( a => a.PersonAliasId.HasValue && a.PersonAlias.PersonId == this.CurrentPersonId )
                .ToList();

            ddlSection.DataSource = orderedPersonalLinkSections;
            ddlSection.DataTextField = "Name";
            ddlSection.DataValueField = "Id";
            ddlSection.DataBind();
            if ( orderedPersonalLinkSections.Any() )
            {
                ddlSection.Items.Insert( 0, new ListItem() );
                ddlSection.Required = true;
            }
            else
            {
                // if there aren't any link sections, use a section called 'Links' as a default
                ddlSection.Items.Insert( 0, new ListItem( "Links" ) );
                ddlSection.Required = false;
            }
        }

        #endregion
    }
}