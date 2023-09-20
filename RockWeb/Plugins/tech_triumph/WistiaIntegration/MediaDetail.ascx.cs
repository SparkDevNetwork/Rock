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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

using tech.triumph.WistiaIntegration.Model;

namespace RockWeb.Plugins.tech_triumph.WistiaIntegration
{
    /// <summary>
    /// Displays the details of the given Media for a wistia Project.
    /// </summary>
    [DisplayName( "Media Detail" )]
    [Category( "Triumph Tech > Wistia Integration" )]
    [Description( "Displays the details of the given Media for a wistia Project." )]
    public partial class MediaDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var mediaId = PageParameter( "mediaId" ).AsInteger();

            if ( !Page.IsPostBack )
            {
                if ( mediaId.Equals( 0 ) )
                {
                    this.Visible = false;
                    return;
                }

                ShowDetail( mediaId );
            }

        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? mediaId = PageParameter( pageReference, "MediaId" ).AsIntegerOrNull();
            if ( mediaId != null )
            {
                string mediaName = new WistiaMediaService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .Where( p => p.Id == mediaId.Value )
                    .Select( p => p.Name )
                    .FirstOrDefault();

                if ( !string.IsNullOrWhiteSpace( mediaName ) )
                {
                    breadCrumbs.Add( new BreadCrumb( mediaName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Media File", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the read-only details.
        /// </summary>
        /// <param name="media">Wistia Media.</param>
        private void ShowReadonlyDetails( WistiaMedia media )
        {
            if ( media == null )
            {
                return;
            }

            lTitle.Text = media.Name.FormatAsHtmlTitle();

            DescriptionList detailDescription = new DescriptionList();
            detailDescription.Add( "Name", media.Name );
            detailDescription.Add( "Project", media.WistiaProject.Name );
            if ( !string.IsNullOrEmpty( media.Description ) )
            {
                detailDescription.Add( "Description", media.Description );
            }
            if ( !string.IsNullOrEmpty( media.Section ) )
            {
                detailDescription.Add( "Section", media.Section );
            }
            detailDescription.Add( "Duration", string.Format( "{0:N1}m", media.Duration / 60 ) );


            if ( media.Engagement.HasValue )
            {
                lEngagement.Text = ((media.Engagement.Value) * 100).ToString( "N0" ) + "%";
            }

            if ( media.PlayCount.HasValue )
            {
                lPlayCount.Text = media.PlayCount.Value.ToString( "N0" );
            }

            if ( media.LoadCount.HasValue )
            {
                lLoadCount.Text = media.LoadCount.Value.ToString( "N0" );
            }

            if ( media.HoursWatched.HasValue )
            {
                lHoursWatched.Text = media.HoursWatched.Value.ToString( "#,##0.0" );
            }

            lblMainDetails.Text = detailDescription.Html;

            imgThumbnail.ImageUrl = media.ThumbnailUrl;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mediaId">The Media identifier.</param>
        public void ShowDetail( int mediaId )
        {
            pnlDetails.Visible = true;
            WistiaMedia media = null;

            if ( !mediaId.Equals( 0 ) )
            {
                media = new WistiaMediaService( new RockContext() ).Get( mediaId );
            }

            ShowReadonlyDetails( media );
        }

        #endregion

    }
}