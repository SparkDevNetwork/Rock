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
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security.Oidc
{
    /// <summary>
    /// Block for displaying and editing available OpenID Connect scopes.
    /// </summary>
    [DisplayName( "OpenID Connect Scopes" )]
    [Category( "Security > OIDC" )]
    [Description( "Block for displaying and editing available OpenID Connect scopes." )]
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1)]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OIDC_SCOPE_LIST )]
    public partial class AuthScopeList : RockBlock, ICustomGridColumns
    {
        public class AttributeKey
        {
            /// <summary>
            /// The detail page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        public class PageParameterKey
        {
            /// <summary>
            /// The scope detail identifier
            /// </summary>
            public const string ScopeDetailId = "ScopeId";
        }

        /// <summary>
        /// User Preference Keys
        /// </summary>
        public class UserPreferenceKey
        {
            /// <summary>
            /// The name
            /// </summary>
            public const string Name = "Name";
            /// <summary>
            /// The public name
            /// </summary>
            public const string PublicName = "PublicName";
            /// <summary>
            /// The active status
            /// </summary>
            public const string ActiveStatus = "ActiveStatus";
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gAuthScopes.DataKeyNames = new string[] { "Id" };
            gAuthScopes.Actions.AddClick += gAuthScopes_AddClick;
            gAuthScopes.GridRebind += gAuthScopes_GridRebind;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gAuthScopes.IsDeleteEnabled = canEdit;
            gAuthScopes.Actions.ShowAdd = canEdit;

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            base.OnLoad( e );
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the AddClick event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAuthScopes_AddClick( object sender, EventArgs e )
        {
            NavigateToAuthScopeDetailPage( 0 );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SetFilterPreference( UserPreferenceKey.Name, tbName.Text );
            gfSettings.SetFilterPreference( UserPreferenceKey.PublicName, tbPublicName.Text );

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SetFilterPreference( UserPreferenceKey.ActiveStatus, string.Empty );
            }
            else
            {
                gfSettings.SetFilterPreference( UserPreferenceKey.ActiveStatus, ddlActiveFilter.SelectedValue );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAuthScopes_Delete( object sender, RowEventArgs e )
        {
            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            if ( canEdit )
            {
                using ( var rockContext = new RockContext() )
                {
                    var authScopeService = new AuthScopeService( rockContext );
                    var authScope = authScopeService.Get( e.RowKeyId );
                    if ( authScope != null )
                    {
                        authScopeService.Delete( authScope );
                        rockContext.SaveChanges();
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAuthScopes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gAuthScopes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        /// <summary>
        /// Handles the RowDataBound event of the gAuthScopes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gAuthScopes_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var authScope = e.Row.DataItem as AuthScope;
            if ( authScope == null )
            {
                return;
            }

            var deleteFieldColumn = gAuthScopes.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteFieldColumn == null || !deleteFieldColumn.Visible )
            {
                return;
            }

            var deleteFieldColumnIndex = gAuthScopes.GetColumnIndex( deleteFieldColumn );
            var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
            if ( deleteButton == null )
            {
                return;
            }

            deleteButton.Visible = !authScope.IsSystem;
        }

        /// <summary>
        /// Handles the RowSelected event of the gAuthScopes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gAuthScopes_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToAuthScopeDetailPage( e.RowKeyId );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbName.Text = gfSettings.GetFilterPreference( UserPreferenceKey.Name );
            tbPublicName.Text = gfSettings.GetFilterPreference( UserPreferenceKey.PublicName );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetFilterPreference( UserPreferenceKey.ActiveStatus ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var authScopeService = new AuthScopeService( rockContext );
            var authScopeQuery = authScopeService.Queryable().AsNoTracking();

            if ( tbName.Text.IsNotNullOrWhiteSpace() )
            {
                authScopeQuery = authScopeQuery.Where( s => s.Name.Contains( tbName.Text ) );
            }

            if ( tbPublicName.Text.IsNotNullOrWhiteSpace() )
            {
                authScopeQuery = authScopeQuery.Where( s => s.PublicName.Contains( tbPublicName.Text ) );
            }

            if ( ddlActiveFilter.SelectedIndex > -1 )
            {
                switch ( ddlActiveFilter.SelectedValue )
                {
                    case "active":
                        authScopeQuery = authScopeQuery.Where( s => s.IsActive );
                        break;
                    case "inactive":
                        authScopeQuery = authScopeQuery.Where( s => !s.IsActive );
                        break;
                }
            }

            // Sort
            var sortProperty = gAuthScopes.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "Name", SortDirection.Ascending ) );
            }
            authScopeQuery = authScopeQuery.Sort( sortProperty );

            gAuthScopes.SetLinqDataSource( authScopeQuery );
            gAuthScopes.DataBind();
        }

        /// <summary>
        /// Navigates to authentication scope detail page.
        /// </summary>
        /// <param name="authScopeId">The authentication scope identifier.</param>
        private void NavigateToAuthScopeDetailPage( int authScopeId )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( PageParameterKey.ScopeDetailId, authScopeId.ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }
        #endregion

        
    }
}