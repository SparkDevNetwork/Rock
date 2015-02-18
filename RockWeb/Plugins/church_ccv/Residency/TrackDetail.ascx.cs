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
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;
using church.ccv.Residency.Data;
using church.ccv.Residency.Model;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using System.ComponentModel;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Track Detail" )]
    [Category( "CCV > Residency" )]
    [Description( "Displays the details of a residency track." )]

    public partial class TrackDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string trackId = PageParameter( "TrackId" );
                if ( !string.IsNullOrWhiteSpace( trackId ) )
                {
                    ShowDetail( trackId.AsInteger(), PageParameter( "PeriodId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? trackId = this.PageParameter( pageReference, "TrackId" ).AsInteger();
            if ( trackId != null )
            {
                Track track = new ResidencyService<Track>( new ResidencyContext() ).Get( trackId.Value );
                if ( track != null )
                {
                    breadCrumbs.Add( new BreadCrumb( track.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Track", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfTrackId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                // if this page was called from the Period Detail page, return to that
                string periodId = PageParameter( "PeriodId" );
                if ( !string.IsNullOrWhiteSpace( periodId ) )
                {
                    Dictionary<string, string> qryString = new Dictionary<string, string>();
                    qryString["PeriodId"] = periodId;
                    NavigateToParentPage( qryString );
                }
                else
                {
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ResidencyService<Track> service = new ResidencyService<Track>( new ResidencyContext() );
                Track item = service.Get( hfTrackId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            ResidencyService<Track> service = new ResidencyService<Track>( new ResidencyContext() );
            Track item = service.Get( hfTrackId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Track track;
            var residencyContext = new ResidencyContext();
            ResidencyService<Track> trackService = new ResidencyService<Track>( residencyContext );

            int trackId = hfTrackId.ValueAsInt();
            int periodId = hfPeriodId.ValueAsInt();

            if ( trackId == 0 )
            {
                track = new Track();
                trackService.Add( track );

                int maxDisplayOrder = trackService.Queryable()
                        .Where( a => a.PeriodId.Equals( periodId ) )
                        .Select( a => a.DisplayOrder ).DefaultIfEmpty( 0 ).Max();
                track.DisplayOrder = maxDisplayOrder + 1;
            }
            else
            {
                track = trackService.Get( trackId );
            }

            track.Name = tbName.Text;
            track.Description = tbDescription.Text;
            track.PeriodId = periodId;

            if ( !track.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            residencyContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["TrackId"] = track.Id.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="trackId">The track identifier.</param>
        public void ShowDetail( int trackId )
        {
            ShowDetail( trackId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="trackId">The track identifier.</param>
        /// <param name="periodId">The period id.</param>
        public void ShowDetail( int trackId, int? periodId )
        {
            pnlDetails.Visible = true;

            var residencyContext = new ResidencyContext();

            // Load depending on Add(0) or Edit
            Track track = null;
            if ( !trackId.Equals( 0 ) )
            {
                track = new ResidencyService<Track>( residencyContext ).Get( trackId );
            }
            
            if ( track == null )
            {
                track = new Track { Id = 0 };
                track.PeriodId = periodId ?? 0;
                track.Period = new ResidencyService<Period>( residencyContext ).Get( track.PeriodId );
            }

            hfTrackId.Value = track.Id.ToString();
            hfPeriodId.Value = track.PeriodId.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Track.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                ShowReadonlyDetails( track );
            }
            else
            {
                btnEdit.Visible = true;
                if ( track.Id > 0 )
                {
                    ShowReadonlyDetails( track );
                }
                else
                {
                    ShowEditDetails( track );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="track">The track.</param>
        private void ShowEditDetails( Track track )
        {
            if ( track.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Track.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = track.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = track.Name;
            tbDescription.Text = track.Description;
            lblPeriod.Text = track.Period.Name;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="track">The track.</param>
        private void ShowReadonlyDetails( Track track )
        {
            lReadOnlyTitle.Text = track.Name.FormatAsHtmlTitle();
            
            SetEditMode( false );

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Description", track.Description ).Html;

            lblMainDetailsCol1.Text = new DescriptionList()
                .Add( "Period", track.Period.Name )
                .Html;
        }

        #endregion
    }
}