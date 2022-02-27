// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.minecartstudio.WistiaIntegration.Model;
using Rock.Model;
using Rock.Web.Cache;
using com.minecartstudio.WistiaIntegration;
using System.Data.Entity;

namespace RockWeb.Plugins.com_mineCartStudio.WistiaIntegration
{
    /// <summary>
    /// Lists all Media and allows for managing them.
    /// </summary>
    [DisplayName( "Media List" )]
    [Category( "Mine Cart Studio > Wistia Integration" )]
    [Description( "Lists all the Wistia Media and allows for managing them." )]

    [LinkedPage( "Detail Page" )]
    public partial class MediaList : RockBlock
    {
        #region Private Variables

        private WistiaProject _project = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int projectId = PageParameter( "ProjectId" ).AsInteger();

            if ( projectId != 0 )
            {
                string key = string.Format( "WistiaProject:{0}", projectId );
                _project = RockPage.GetSharedItem( key ) as WistiaProject;
                if ( _project == null )
                {
                    _project = new WistiaProjectService( new RockContext() ).Get( projectId );
                    RockPage.SaveSharedItem( key, _project );
                }
            }

            if ( _project != null && _project.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;

                gMedia.DataKeyNames = new string[] { "Id" };
                gMedia.GridRebind += gMedia_GridRebind;
                // Block Security and special attributes (RockPage takes care of View)
                bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
                gMedia.IsDeleteEnabled = canAddEditDelete;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( _project != null )
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
        /// Handles the Edit event of the gMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMedia_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "MediaId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMedia_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var mediaService = new WistiaMediaService( rockContext );

            var media = mediaService.Get( e.RowKeyId );

            if ( media != null )
            {
                string errorMessage;
                if ( !mediaService.CanDelete( media, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                mediaService.Delete( media );

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMedia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMedia_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Section", ddlSection.SelectedValue );
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var listData = new WistiaMediaService( new RockContext() ).Queryable()
              .Where( gm => gm.WistiaProjectId == _project.Id && !string.IsNullOrEmpty( gm.Section ) )
              .Select( gm => gm.Section.Trim() ).Distinct()
              .ToList();

            ddlSection.Items.Clear();
            if ( listData.Count > 0 )
            {
                ddlSection.DataSource = listData;
                ddlSection.DataBind();
            }
            ddlSection.Items.Insert( 0, new ListItem() );
            ddlSection.SetValue( rFilter.GetUserPreference( "Section" ) );
        }

        /// <summary>
        /// Binds the grid for Wistia Media.
        /// </summary>
        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var media = new WistiaMediaService( rockContext ).Queryable()
                    .Where( a => a.WistiaProjectId == _project.Id );

                string sectionFilter = rFilter.GetUserPreference( "Section" );
                if ( !string.IsNullOrWhiteSpace( sectionFilter ) )
                {
                    media = media.Where( p => p.Section == sectionFilter );
                }

                var items = media.Select( c => new MediaResult
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Duration = c.Duration,
                    Section = c.Section,
                    MediaStats = c.MediaStats,
                    MediaData = c.MediaData,
                    PlayCount = c.PlayCount,
                    HoursWatched = c.HoursWatched
                } ).AsQueryable().AsNoTracking();

                gMedia.EntityTypeId = EntityTypeCache.Get<WistiaMedia>().Id;
                var sortProperty = gMedia.SortProperty;

                if ( sortProperty != null )
                {
                    gMedia.DataSource = items.Sort( sortProperty ).ToList();
                }
                else
                {
                    gMedia.DataSource = items.OrderBy( p => p.Name ).ToList();
                }

                gMedia.DataBind();
            }
        }

        /// <summary>
        /// Media Result Object
        /// </summary>
        private class MediaResult
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
            /// Gets or sets the description.
            /// </summary>
            /// <value>
            /// The description.
            /// </value>
            public string Description {
                get
                {
                    return _description.SanitizeHtml( true );
                }
                set
                {
                    _description = value;
                }
            }
            /// <summary>
            /// The description
            /// </summary>
            private string _description = string.Empty;

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
            /// Gets or sets the duration.
            /// </summary>
            /// <value>
            /// The duration.
            /// </value>
            public decimal? Duration { get; set; }

            /// <summary>
            /// Gets the duration formatted.
            /// </summary>
            /// <value>
            /// The duration formatted.
            /// </value>
            public string DurationFormatted {
                get {

                    if ( this.Duration.HasValue ) 
                    {
                        return string.Format( "{0:N1}m", this.Duration.Value / 60 );
                    }

                    return string.Empty;
                }
            }

            /// <summary>
            /// Gets or sets the section.
            /// </summary>
            /// <value>
            /// The section.
            /// </value>
            public string Section { get; set; }

            /// <summary>
            /// Gets or sets the media stats.
            /// </summary>
            /// <value>
            /// The media stats.
            /// </value>
            public string MediaStats { get; set; }

            /// <summary>
            /// Gets the media stats information.
            /// </summary>
            /// <value>
            /// The media stats information.
            /// </value>
            public WistiaApiStatMedia MediaStatsInfo
            {
                get
                {
                    return WistiaMedia.TranslateMediaStats( this.MediaStats );
                }
            }

            /// <summary>
            /// Gets or sets the media data.
            /// </summary>
            /// <value>
            /// The media data.
            /// </value>
            public string MediaData { get; set; }

            /// <summary>
            /// Gets the media data information.
            /// </summary>
            /// <value>
            /// The media data information.
            /// </value>
            public WistiaApiDataMedia MediaDataInfo
            {
                get
                {
                    return WistiaMedia.TranslateMediaData( this.MediaData );
                }
            }
        }

        #endregion
    }
}