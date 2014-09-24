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
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using com.ccvonline.CommandCenter.Model;
using com.ccvonline.CommandCenter.Data;
using Rock.Web.Cache;

namespace RockWeb.Plugins.com_ccvonline.CommandCenter
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Recording Detail" )]
    [Category( "CCV > Command Center" )]
    [Description( "Displays the details of a recording." )]
    public partial class RecordingDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            cpCampus.Campuses = CampusCache.All();
            cpCampus.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
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
                string recordingId = PageParameter( "recordingId" );
                if ( !string.IsNullOrWhiteSpace( recordingId ) )
                {
                    ShowDetail( recordingId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
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
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Recording recording;
            var dataContext = new CommandCenterContext();
            var service = new RecordingService( dataContext );

            int recordingId = 0;
            if ( !Int32.TryParse( hfRecordingId.Value, out recordingId ) )
                recordingId = 0;

            if ( recordingId == 0 )
            {
                recording = new Recording();
                service.Add( recording );
            }
            else
            {
                recording = service.Get( recordingId );
            }

            recording.CampusId = cpCampus.SelectedCampusId;
            recording.App = tbApp.Text;
            recording.Date = dpDate.SelectedDate;
            recording.StreamName = tbStream.Text;
            recording.Label = tbLabel.Text;
            recording.RecordingName = tbRecording.Text;

            if ( recordingId == 0 && cbStartRecording.Visible && cbStartRecording.Checked )
            {
                SendRequest( "start", recording );
            }

            dataContext.SaveChanges();

            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="recordingId">The recording identifier.</param>
        public void ShowDetail( int recordingId )
        {
            pnlDetails.Visible = true;

            Recording recording = null;
            if ( !recordingId.Equals( 0 ) )
            {
                recording = new RecordingService( new CommandCenterContext() ).Get( recordingId );
                lActionTitle.Text = ActionTitle.Edit( Recording.FriendlyTypeName );
            }

            if ( recording == null )
            {
                recording = new Recording { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Recording.FriendlyTypeName );
            }

            hfRecordingId.Value = recording.Id.ToString();

            cpCampus.SelectedCampusId = recording.CampusId;
            tbApp.Text = recording.App ?? string.Empty;
            dpDate.SelectedDate = recording.Date;
            tbStream.Text = recording.StreamName ?? string.Empty;
            tbLabel.Text = recording.Label ?? string.Empty;
            tbRecording.Text = recording.RecordingName ?? string.Empty;
            lStarted.Text = recording.StartTime.HasValue ? recording.StartTime.Value.ToString() : string.Empty;
            lStartResponse.Text = recording.StartResponse ?? string.Empty;
            lStopped.Text = recording.StopTime.HasValue ? recording.StopTime.Value.ToString() : string.Empty;
            lStopResponse.Text = recording.StopResponse ?? string.Empty;

            lStarted.Visible = recording.StartTime.HasValue;
            lStartResponse.Visible = !string.IsNullOrEmpty( recording.StartResponse );
            lStopped.Visible = recording.StopTime.HasValue;
            lStopResponse.Visible = !string.IsNullOrEmpty( recording.StopResponse );

            cbStartRecording.Visible = false;

            bool readOnly = !IsUserAuthorized( Rock.Security.Authorization.EDIT );
            nbEditModeMessage.Text = string.Empty;

            if ( readOnly )
            {
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Recording.FriendlyTypeName );
                lActionTitle.Text = ActionTitle.View( Recording.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            cpCampus.Enabled = !readOnly;
            tbApp.ReadOnly = readOnly;
            dpDate.ReadOnly = readOnly;
            tbStream.ReadOnly = readOnly;
            tbLabel.ReadOnly = readOnly;
            tbRecording.ReadOnly = readOnly;

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="recording">The recording.</param>
        /// <returns></returns>
        public bool SendRequest( string action, Recording recording )
        {
            Rock.Net.RockWebResponse response = RecordingService.SendRecordingRequest( recording.App, recording.StreamName, recording.RecordingName, action.ToLower() );

            if ( response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK )
            {

                if ( action.ToLower() == "start" )
                {
                    recording.StartTime = DateTime.Now;
                    recording.StartResponse = RecordingService.ParseResponse( response.Message );
                }
                else
                {
                    recording.StopTime = DateTime.Now;
                    recording.StopResponse = RecordingService.ParseResponse( response.Message );
                }

                return true;
            }

            return false;
        }

        #endregion
    }
}