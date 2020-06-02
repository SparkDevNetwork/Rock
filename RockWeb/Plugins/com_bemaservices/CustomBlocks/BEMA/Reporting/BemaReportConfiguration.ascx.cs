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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;

using DotLiquid;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Model;
using System.Web.UI.WebControls;
using Rock.Security;

namespace RockWeb.Plugins.com_bemaservices.CustomBlocks.BEMA.Reporting
{
    [DisplayName( "BEMA Report Configuration" )]
    [Category( "BEMA Services > Reporting" )]
    [Description( "Renders a page menu based on a root page and lava template." )]
    [LinkedPage( "Root Page", "The root page to use for the page collection. Defaults to the current page instance if not set.", false, "" )]

    public partial class BemaReportConfiguration : RockBlock
    {
        private static readonly string ROOT_PAGE = "RootPage";

        protected override void OnInit( EventArgs e )
        {
            // this.EnableViewState = false;

            base.OnInit( e );

            this.BlockUpdated += PageMenu_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PageMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void PageMenu_BlockUpdated( object sender, EventArgs e )
        {
        }

        protected void rptReports_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            var page = e.Item.DataItem as Rock.Model.Page;
            LinkButton lbAdd = e.Item.FindControl( "lbAdd" ) as LinkButton;
            LinkButton lbView = e.Item.FindControl( "lbView" ) as LinkButton;
            LinkButton lbRemove = e.Item.FindControl( "lbRemove" ) as LinkButton;

            lbAdd.Visible = IsHidden( page, Authorization.VIEW );
            lbRemove.Visible = lbView.Visible = !IsHidden( page, Authorization.VIEW );

        }
        protected void rptReports_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            string selectedPageGuid = e.CommandArgument.ToString();
            string command = e.CommandName.ToString();

            LinkButton lbAdd = e.Item.FindControl( "lbAdd" ) as LinkButton;
            LinkButton lbView = e.Item.FindControl( "lbView" ) as LinkButton;
            LinkButton lbRemove = e.Item.FindControl( "lbRemove" ) as LinkButton;

            var page = new PageService( new RockContext() ).Get( selectedPageGuid.AsGuid() );
            if ( page != null )
            {
                switch ( command )
                {
                    case "Add":
                        {
                            MakeUnHidden( page, Authorization.VIEW );
                            lbAdd.Visible = false;
                            lbRemove.Visible = lbView.Visible = true;
                            break;
                        }

                    case "View":
                        {
                            NavigateToPage( page.Guid, new Dictionary<string, string>() );
                            break;
                        }

                    case "Remove":
                        {
                            MakeHidden( page, Authorization.VIEW );
                            lbAdd.Visible = true;
                            lbRemove.Visible = lbView.Visible = false;
                            break;
                        }
                }
            }

        }

        private void ShowDetail()
        {
            try
            {
                PageCache currentPage = PageCache.Get( RockPage.PageId );
                PageCache rootPageCache = null;

                var pageRouteValuePair = GetAttributeValue( ROOT_PAGE ).SplitDelimitedValues( false ).AsGuidOrNullList();
                if ( pageRouteValuePair.Any() && pageRouteValuePair[0].HasValue && !pageRouteValuePair[0].Value.IsEmpty() )
                {
                    rootPageCache = PageCache.Get( pageRouteValuePair[0].Value );
                }

                // If a root page was not found, use current page
                if ( rootPageCache == null )
                {
                    rootPageCache = currentPage;
                }

                var rootPage = new PageService( new RockContext() ).Get( rootPageCache.Id );
                rptReports.DataSource = rootPage.Pages;
                rptReports.DataBind();

            }
            catch ( Exception ex )
            {
                LogException( ex );
                StringBuilder errorMessage = new StringBuilder();
                errorMessage.Append( "<div class='alert alert-warning'>" );
                errorMessage.Append( "An error has occurred while generating the page menu. Error details:" );
                errorMessage.Append( ex.Message );
                errorMessage.Append( "</div>" );

                phContent.Controls.Add( new LiteralControl( errorMessage.ToString() ) );
            }
        }

        #region Methods

        private string CacheKey()
        {
            return string.Format( "Rock:PageMenu:{0}", BlockId );
        }

        /// <summary>
        /// Gets the site *PageId properties.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A dictionary of various page ids for the site.</returns>
        private Dictionary<string, object> GetSiteProperties( SiteCache site )
        {
            var properties = new Dictionary<string, object>();
            properties.Add( "DefaultPageId", site.DefaultPageId );
            properties.Add( "LoginPageId", site.LoginPageId );
            properties.Add( "PageNotFoundPageId", site.PageNotFoundPageId );
            properties.Add( "CommunicationPageId", site.CommunicationPageId );
            properties.Add( "RegistrationPageId ", site.RegistrationPageId );
            properties.Add( "MobilePageId", site.MobilePageId );
            return properties;
        }

        public static bool IsHidden( ISecured entity, string action )
        {
            var authorizations = Authorization.Get();

            // If there are entries in the Authorizations object for this entity type and entity instance, evaluate each 
            // one to find the first one specific to the selected user or a role that the selected user belongs 
            // to.  If a match is found return whether the user is allowed (true) or denied (false) access
            if ( authorizations == null || !authorizations.Keys.Contains( entity.TypeId ) ||
                !authorizations[entity.TypeId].Keys.Contains( entity.Id ) ||
                !authorizations[entity.TypeId][entity.Id].Keys.Contains( action ) ||
                authorizations[entity.TypeId][entity.Id][action].Count != 1 )
                return false;

            var firstRule = authorizations[entity.TypeId][entity.Id][action][0];

            // If first rule allows current user, and second rule denies all other users then entity is private
            if ( firstRule.AllowOrDeny == 'D' &&
                 firstRule.SpecialRole == SpecialRole.AllUsers )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Makes the entity private by setting up two authorization rules, one granting the selected person, and
        /// then another that denies all other users.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void MakeHidden( ISecured entity, string action, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyMakeHidden( entity, action, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyMakeHidden( entity, action, myRockContext );
                }
            }
        }

        /// <summary>
        /// Removes that two authorization rules that made the entity private.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void MakeUnHidden( ISecured entity, string action, RockContext rockContext = null )
        {
            if ( rockContext != null )
            {
                MyMakeUnHidden( entity, action, rockContext );
            }
            else
            {
                using ( var myRockContext = new RockContext() )
                {
                    MyMakeUnHidden( entity, action, myRockContext );
                }
            }
        }

        /// <summary>
        /// Makes the entity private for the selected action and person
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyMakeHidden( ISecured entity, string action, RockContext rockContext )
        {
            if ( IsHidden( entity, action ) )
                return;

            var authService = new AuthService( rockContext );

            // Delete any existing rules in database
            foreach ( var auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }

            rockContext.SaveChanges();

            var auth1 = new Auth
            {
                EntityTypeId = entity.TypeId,
                EntityId = entity.Id,
                Order = 0,
                Action = action,
                AllowOrDeny = "D",
                SpecialRole = SpecialRole.AllUsers
            };
            authService.Add( auth1 );

            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            Authorization.RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        /// <summary>
        /// If the entity is currently private for selected person, removes all the rules 
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private static void MyMakeUnHidden( ISecured entity, string action, RockContext rockContext )
        {
            if ( !IsHidden( entity, action ) )
                return;

            var authService = new AuthService( rockContext );

            // Delete any existing rules in database
            foreach ( var auth in authService
                .GetAuths( entity.TypeId, entity.Id, action ) )
            {
                authService.Delete( auth );
            }
            rockContext.SaveChanges();

            // Reload the static dictionary for this action
            Authorization.RefreshAction( entity.TypeId, entity.Id, action, rockContext );
        }

        #endregion

    }
}