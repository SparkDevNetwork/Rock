//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.UI.Controls;
using Rock.Com.CCVOnline.Service;

namespace RockWeb.Plugins.CCVOnline.Service
{
    public partial class Recordings : Rock.Web.UI.Block
    {
        #region Fields

        RecordingService recordingService = new RecordingService();

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
            {
                gRecordings.DataKeyNames = new string[] { "id" };
                gRecordings.Actions.IsAddEnabled = true;
                gRecordings.Actions.AddClick += gRecordings_Add;
                gRecordings.RowDataBound += gRecordings_RowDataBound;
                gRecordings.RowCommand += gRecordings_RowCommand;
                gRecordings.GridRebind += gRecordings_GridRebind;
            }

            string script = @"
        Sys.Application.add_load(function () {
            $('td.grid-icon-cell.delete a').click(function(){
                return confirm('Are you sure you want to delete this recording?');
                });
        });
    ";
            this.Page.ClientScript.RegisterStartupScript( this.GetType(), string.Format( "grid-confirm-delete-{0}", gRecordings.ClientID ), script, true );

            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            if ( CurrentPage.IsAuthorized( "Configure", CurrentPerson ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();

                    ddlCampus.Items.Clear();
                    foreach ( var campus in new Rock.Crm.CampusService().Queryable().OrderBy( c => c.Name ) )
                        ddlCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
                }
            }
            else
            {
                gRecordings.Visible = false;
                nbMessage.Text = "You are not authorized to edit recordings";
                nbMessage.Visible = true;
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events

        void gRecordings_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            Recording recording = e.Row.DataItem as Recording;
            LinkButton lbStart = ( LinkButton )e.Row.FindControl( "lbStart" );
            LinkButton lbStop = ( LinkButton )e.Row.FindControl( "lbStop" );

            if ( recording != null && lbStart != null && lbStop != null )
            {
                lbStart.Visible = !recording.StartTime.HasValue && !recording.StopTime.HasValue;
                lbStop.Visible = recording.StartTime.HasValue && !recording.StopTime.HasValue;
            }
        }

        protected void gRecordings_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )gRecordings.DataKeys[e.RowIndex]["id"] );
        }

        protected void gRecordings_Delete( object sender, RowEventArgs e )
        {
            Recording recording = recordingService.Get( ( int )gRecordings.DataKeys[e.RowIndex]["id"] );
            if ( recording != null )
            {
                recordingService.Delete( recording, CurrentPersonId );
                recordingService.Save( recording, CurrentPersonId );
            }

            BindGrid();
        }

        void gRecordings_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void gRecordings_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "START" || e.CommandName == "STOP" )
            {
                Recording recording = recordingService.Get( Int32.Parse( e.CommandArgument.ToString() ) );
                if ( recording != null && SendRequest( e.CommandName.ToString().ToLower(), recording ) )
                    recordingService.Save( recording, CurrentPersonId );
            }

            BindGrid();
        }

        void gRecordings_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Edit Events

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            Recording recording = null;

            recordingService = new RecordingService();

            int recordingId = 0;
            if ( !Int32.TryParse( hfRecordingId.Value, out recordingId ) )
                recordingId = 0;

            if ( recordingId == 0 )
            {
                recording = new Recording();
                recordingService.Add( recording, CurrentPersonId );
            }
            else
            {
                recording = recordingService.Get( recordingId );
            }

            int campusId = 0;
            Int32.TryParse( ddlCampus.SelectedValue, out campusId );

            recording.CampusId = campusId;
            recording.App = tbApp.Text;
            recording.Date = Convert.ToDateTime( tbDate.Text );
            recording.StreamName = tbStream.Text;
            recording.Label = tbLabel.Text;
            recording.RecordingName = tbRecording.Text;

            if ( recordingId == 0 && cbStartRecording.Visible && cbStartRecording.Checked )
                SendRequest( "start", recording );

            recordingService.Save( recording, CurrentPersonId );

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            var queryable = recordingService.Queryable();

            var sortProperty = gRecordings.SortProperty;
            if ( sortProperty != null )
                queryable = queryable.Sort( sortProperty );
            else
                queryable = queryable.OrderByDescending( s => s.Date );

            gRecordings.DataSource = queryable.ToList();
            gRecordings.DataBind();
        }

        protected void ShowEdit( int recordingId )
        {
            Recording recording = recordingService.Get( recordingId );

            if ( recording != null )
            {
                lAction.Text = "Edit";
                hfRecordingId.Value = recording.Id.ToString();

                ddlCampus.SelectedValue = recording.CampusId.ToString();
                tbApp.Text = recording.App ?? string.Empty;
                tbDate.Text = recording.Date.HasValue ? recording.Date.Value.ToShortDateString() : string.Empty;
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
            }
            else
            {
                lAction.Text = "Add";

                tbApp.Text = string.Empty;
                tbDate.Text = string.Empty;
                tbStream.Text = string.Empty;
                tbLabel.Text = string.Empty;
                tbRecording.Text = string.Empty;

                lStarted.Visible = false;
                lStartResponse.Visible = false;
                lStopped.Visible = false;
                lStopResponse.Visible = false;

                cbStartRecording.Visible = true;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

		public bool SendRequest( string action, Recording recording )
		{
			Rock.Net.WebResponse response = RecordingService.SendRecordingRequest( recording.App, recording.StreamName, recording.RecordingName, action.ToLower() );

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