// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//
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
using Rock.Web.UI.Controls;

using tech.triumph.WistiaIntegration.Model;

namespace RockWeb.Plugins.tech_triumph.WistiaIntegration
{
    /// <summary>
    /// Lists all the Wistia accounts and allows for managing them.
    /// </summary>
    [DisplayName( "Project List" )]
    [Category( "Triumph Tech > Wistia Integration" )]
    [Description( "Lists all the Wistia projects and allows for managing them." )]

    [LinkedPage( "Detail Page" )]
    public partial class ProjectList : RockBlock
    {
        #region Private Variables

        private WistiaAccount _account = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int accountId = PageParameter( "AccountId" ).AsInteger();

            if ( accountId != 0 )
            {
                string key = string.Format( "WistiaAccount:{0}", accountId );
                _account = RockPage.GetSharedItem( key ) as WistiaAccount;
                if ( _account == null )
                {
                    _account = new WistiaAccountService( new RockContext() ).Get( accountId );
                    RockPage.SaveSharedItem( key, _account );
                }
            }

            if ( _account != null && _account.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                gfProjectsFilter.ApplyFilterClick += gfProjectsFilter_ApplyFilterClick;

                gProject.DataKeyNames = new string[] { "Id" };
                gProject.GridRebind += gProject_GridRebind;
                gProject.ShowConfirmDeleteDialog = false;
                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
                //gProject.IsDeleteEnabled = false; //  canAddEditDelete;
            }

            string deleteScript = @"
    $('table.js-grid-projects a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this project and media files ?', function (result) {
            if (result) {
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                }
            });
    });
";
            ScriptManager.RegisterStartupScript( gProject, gProject.GetType(), "deleteInstanceScript", deleteScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _account != null )
                {
                    pnlContent.Visible = true;
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    pnlContent.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the Edit event of the gProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gProject_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ProjectId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gProject_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var accountService = new WistiaAccountService( rockContext );

            var account = accountService.Get( e.RowKeyId );

            if ( account != null )
            {
                string errorMessage;
                if ( !accountService.CanDelete( account, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                accountService.Delete( account );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gProject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gProject_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfProjectsFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gfProjectsFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfProjectsFilter.SaveUserPreference( "ShowInIntegration", ddlShowInIntegration.SelectedValue );
            BindGrid();
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlShowInIntegration.SelectedValue = gfProjectsFilter.GetUserPreference( "ShowInIntegration" );
        }

        /// <summary>
        /// Binds the grid for defined types.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var projects = new WistiaProjectService( rockContext ).Queryable().AsNoTracking()
                    .Where( a => a.WistiaAccountId == _account.Id );

                string activeFilter = gfProjectsFilter.GetUserPreference( "ShowInIntegration" );
                if ( !string.IsNullOrWhiteSpace( activeFilter ) )
                {
                    projects = projects.Where( p => p.ShowInIntegration == ( activeFilter == "Yes" ) );
                }
                else
                {
                    projects = projects.Where( p => p.ShowInIntegration );
                }

                var items = projects.Select( c => new ProjectResult
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ShowInIntegration = c.ShowInIntegration,
                    MediaCount = c.WistiaMediaItems.Count(),
                    IsPublic = c.IsPublic,
                    PlayCount = c.PlayCount,
                    HoursWatched = c.HoursWatched
                } ).AsQueryable();



                gProject.EntityTypeId = EntityTypeCache.Get<WistiaProject>().Id;
                var sortProperty = gProject.SortProperty;

                if ( sortProperty != null )
                {
                    gProject.DataSource = items.Sort( sortProperty ).ToList();
                }
                else
                {
                    gProject.DataSource = items.OrderBy( p => p.Name ).ToList();
                }

                gProject.DataBind();
            }
        }

        #endregion

        /// <summary>
        /// Project result class
        /// </summary>
        private class ProjectResult
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is public.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is public; otherwise, <c>false</c>.
            /// </value>
            public bool IsPublic { get; set; }

            /// <summary>
            /// Gets or sets the play count.
            /// </summary>
            /// <value>
            /// The play count.
            /// </value>
            public int? PlayCount { get; set; }

            /// <summary>
            /// Gets or sets the hours watched.
            /// </summary>
            /// <value>
            /// The hours watched.
            /// </value>
            public decimal? HoursWatched { get; set; }

            /// <summary>
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description
            {
                get
                {
                    if (_description == null )
                    {
                        return string.Empty;
                    }

                    return _description.SanitizeHtml( true );
                }
                set
                {
                    _description = value;
                }
            }
            private string _description = string.Empty;

            /// <summary>
            /// Gets or sets a value indicating whether [show integration].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [show integration]; otherwise, <c>false</c>.
            /// </value>
            public bool ShowInIntegration { get; set; }

            /// <summary>
            /// Gets or sets the media count.
            /// </summary>
            /// <value>
            /// The media count.
            /// </value>
            public int MediaCount { get; set; }
        }

    }
}