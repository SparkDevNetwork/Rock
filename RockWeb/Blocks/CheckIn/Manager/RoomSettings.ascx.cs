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

using Rock;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
    /// </summary>
    [DisplayName( "Room Settings" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to open and close classrooms, Etc." )]
    [Rock.SystemGuid.BlockTypeGuid( "C809C18F-A611-40F8-A67C-CB7289431507" )]
    public partial class RoomSettings : Rock.Web.UI.RockBlock
    {
        #region Page Parameter Keys

        private class PageParameterKey
        {
            public const string LocationId = "LocationId";
        }

        #endregion Page Parameter Keys

        #region ViewState Keys

        /// <summary>
        /// Keys to use for ViewState.
        /// </summary>
        private class ViewStateKey
        {
            public const string CurrentCampusId = "CurrentCampusId";
            public const string CurrentLocationId = "CurrentLocationId";
        }

        #endregion ViewState Keys

        #region Properties

        /// <summary>
        /// The current campus identifier.
        /// </summary>
        public int CurrentCampusId
        {
            get
            {
                return ( ViewState[ViewStateKey.CurrentCampusId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.CurrentCampusId] = value.ToString();
            }
        }

        /// <summary>
        /// The current location identifier.
        /// </summary>
        public int CurrentLocationId
        {
            get
            {
                return ( ViewState[ViewStateKey.CurrentLocationId] as string ).AsInteger();
            }

            set
            {
                ViewState[ViewStateKey.CurrentLocationId] = value.ToString();
            }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            ShowSettings();

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // The page LifeCycle will show the settings.
        }

        /// <summary>
        /// Handles the SelectLocation event of the lpLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lpLocation_SelectLocation( object sender, EventArgs e )
        {
            Location location = lpLocation.Location;
            if ( location != null )
            {
                CheckinManagerHelper.SetSelectedLocation( this, lpLocation, location.Id, CurrentCampusId );
            }
            else
            {
                CheckinManagerHelper.SetSelectedLocation( this, lpLocation, 0, CurrentCampusId );
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglRoom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglRoom_CheckedChanged( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var location = new LocationService( rockContext ).Get( CurrentLocationId );
                if ( location != null )
                {
                    location.IsActive = tglRoom.Checked;

                    rockContext.SaveChanges();
                    Rock.CheckIn.KioskDevice.Clear();
                }
            }
        }

        #endregion Control Events

        #region Internal Methods

        /// <summary>
        /// Shows the settings.
        /// </summary>
        private void ShowSettings()
        {
            ResetControlVisibility();

            CampusCache campus = GetCampusFromContext();
            if ( campus == null )
            {
                ShowWarningMessage( "Please select a Campus.", true );
                return;
            }

            // If the Campus selection has changed, we need to reload the LocationItemPicker with the Locations specific to that Campus.
            if ( campus.Id != CurrentCampusId )
            {
                CurrentCampusId = campus.Id;
                lpLocation.NamedPickerRootLocationId = campus.LocationId.GetValueOrDefault();
            }

            var locationId = CheckinManagerHelper.GetSelectedLocation( this, campus, lpLocation );
            if ( !locationId.HasValue )
            {
                ShowWarningMessage( "Please select a Location.", false );
            }

            CheckinManagerHelper.SetSelectedLocation( this, lpLocation, locationId, CurrentCampusId );
            if ( this.Response.IsRequestBeingRedirected )
            {
                return;
            }

            InitializeSubPageNav( locationId.Value );

            // If the Location changed, we need to reload the settings.
            if ( locationId != CurrentLocationId )
            {
                CurrentLocationId = locationId.Value;

                LoadSettings();
            }
        }

        /// <summary>
        /// Resets control visibility to default values.
        /// </summary>
        private void ResetControlVisibility()
        {
            nbWarning.Visible = false;
            lpLocation.Visible = true;
            pnlSubPageNav.Visible = true;
            pnlSettings.Visible = true;
        }

        /// <summary>
        /// Gets the campus from the current context.
        /// </summary>
        private CampusCache GetCampusFromContext()
        {
            CampusCache campus = null;

            var campusEntityType = EntityTypeCache.Get( "Rock.Model.Campus" );
            if ( campusEntityType != null )
            {
                var campusContext = RockPage.GetCurrentContext( campusEntityType ) as Campus;

                campus = CampusCache.Get( campusContext );
            }

            return campus;
        }

        /// <summary>
        /// Shows a warning message, and optionally hides the content panels.
        /// </summary>
        /// <param name="warningMessage">The warning message to show.</param>
        /// <param name="hideLocationPicker">Whether to hide the lpLocation control.</param>
        private void ShowWarningMessage( string warningMessage, bool hideLocationPicker )
        {
            nbWarning.Text = warningMessage;
            nbWarning.Visible = true;
            lpLocation.Visible = !hideLocationPicker;
            pnlSubPageNav.Visible = false;
            pnlSettings.Visible = false;
        }

        /// <summary>
        /// Initializes the sub page navigation.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        private void InitializeSubPageNav( int locationId )
        {
            RockPage rockPage = this.Page as RockPage;
            if ( rockPage != null )
            {
                PageCache page = PageCache.Get( rockPage.PageId );
                if ( page != null )
                {
                    pbSubPages.RootPageId = page.ParentPageId ?? 0;
                }
            }

            pbSubPages.QueryStringParametersToAdd = new NameValueCollection
            {
                { PageParameterKey.LocationId, locationId.ToString() }
            };
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            Location location = null;
            using ( var rockContext = new RockContext() )
            {
                location = new LocationService( rockContext ).Get( CurrentLocationId );
            }

            if ( location == null )
            {
                ShowWarningMessage( "The specified Location is not valid.", false );
            }

            tglRoom.Checked = location.IsActive;
        }

        #endregion Internal Methods
    }
}