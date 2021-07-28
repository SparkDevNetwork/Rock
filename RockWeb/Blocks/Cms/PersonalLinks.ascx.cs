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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

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
    public partial class PersonalLinks : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ManageLinksPage = "ManageLinksPage";
        }

        #endregion Attribute Keys

        #region Fields

        // Used for private variables.

        #endregion

        #region Properties

        // Used for public / protected properties.

        #endregion

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( CurrentPersonAliasId.HasValue )
                {
                    lbBookmark.Visible = true;
                    BindView();
                }
            }
        }

        #endregion

        #region Events

        // Handlers called by the controls on your block.

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptPersonalLinkSection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptPersonalLinkSection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            var rptLinks = e.Item.FindControl( "rptLinks" ) as Repeater;
            var section = e.Item.DataItem as PersonalLinkSection;
            rptLinks.DataSource = section.PersonalLinks.OrderBy( a => a.Order );
            rptLinks.DataBind();
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
            quickreturns.AddCssClass( "d-none" );
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
            quickreturns.AddCssClass( "d-none" );
            tbLinkName.Text = RockPage.Title;
            urlLink.Text = Page.Request.Url.ToString();
            BindSectionDropdown();
        }

        /// <summary>
        /// Handles the Click event of the btnSectionSave control.
        /// </summary>
        protected void btnSectionSave_Click( object sender, EventArgs e )
        {
            SaveSection( tbSectionName.Text );
            BindView();
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
                BindView();
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
            quickreturns.RemoveCssClass( "d-none" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the connection types repeater.
        /// </summary>
        private void BindView()
        {
            pnlAddSection.Visible = false;
            pnlAddLink.Visible = false;
            var rockContext = new RockContext();
            var personalLinkSections = GetPersonalLinkSections( rockContext, true );
            var personalLinkSectionOrders = GetPersonalLinkSectionOrders( rockContext );
            rptPersonalLinkSection.DataSource = GetOrderedPersonalLinkSection( personalLinkSections, personalLinkSectionOrders );
            rptPersonalLinkSection.DataBind();
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
            var personalLinkSections = GetPersonalLinkSections( rockContext, false );
            var personalLinkSectionOrders = GetPersonalLinkSectionOrders( rockContext );
            var orderedPersonalLinkSection = GetOrderedPersonalLinkSection( personalLinkSections, personalLinkSectionOrders );

            ddlSection.DataSource = orderedPersonalLinkSection;
            ddlSection.DataTextField = "Name";
            ddlSection.DataValueField = "Id";
            ddlSection.DataBind();
            if ( personalLinkSections.Any() )
            {
                ddlSection.Items.Insert( 0, new ListItem() );
                ddlSection.Required = true;
            }
            else
            {
                ddlSection.Items.Insert( 0, new ListItem( "Links" ) );
                ddlSection.Required = false;
            }
        }

        /// <summary>
        /// Gets the personal link sections.
        /// </summary>
        /// <returns></returns>
        private List<PersonalLinkSection> GetPersonalLinkSections( RockContext rockContext, bool isView )
        {
            rockContext = rockContext ?? new RockContext();
            var personalLinkSections = new List<PersonalLinkSection>();
            var qry = new PersonalLinkSectionService( rockContext )
                .Queryable()
                .Include( a => a.PersonalLinks )
                .AsNoTracking()
                .Where( a => a.IsShared || a.PersonAliasId == CurrentPersonAliasId.Value );


            foreach ( var personalLinkSection in qry.ToList() )
            {
                bool isViewable = false;
                if ( isView )
                {
                    isViewable = !personalLinkSection.IsShared || ( personalLinkSection.IsShared && personalLinkSection.IsAuthorized( Authorization.VIEW, CurrentPerson ) );
                }
                else
                {
                    isViewable = !personalLinkSection.IsShared || ( personalLinkSection.IsShared && personalLinkSection.IsAuthorized( Authorization.EDIT, CurrentPerson ) );
                }

                if ( isViewable )
                {
                    personalLinkSections.Add( personalLinkSection );
                }
            }

            return personalLinkSections;
        }

        /// <summary>
        /// Gets the personal link section orders.
        /// </summary>
        /// <returns></returns>
        private List<PersonalLinkSectionOrder> GetPersonalLinkSectionOrders( RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();

            var qry = new PersonalLinkSectionOrderService( rockContext )
                .Queryable();

            qry = qry.Where( a => a.PersonAliasId == CurrentPersonAliasId );

            return qry.ToList();
        }

        private IEnumerable<PersonalLinkSection> GetOrderedPersonalLinkSection( List<PersonalLinkSection> personalLinkSections, List<PersonalLinkSectionOrder> personalLinkSectionOrders )
        {
            return personalLinkSections
                .Where( a => a.PersonalLinks.Any() )
                .Select( a => new
                {
                    PersonalLinkSection = a,
                    Order = personalLinkSectionOrders.Where( b => b.SectionId == a.Id ).Select( b => b.Order ).DefaultIfEmpty().FirstOrDefault()
                } )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.PersonalLinkSection.Name )
                .Select( a => a.PersonalLinkSection )
                .ToList();
        }

        #endregion
    }
}