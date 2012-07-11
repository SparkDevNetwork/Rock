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
using Rock. Com.CCVOnline.Service;

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
            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                gRecordings.DataKeyNames = new string[] { "id" };
                gRecordings.Actions.EnableAdd = true;
                gRecordings.Actions.AddClick += gRecordings_Add;
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

            if ( PageInstance.Authorized( "Configure", CurrentUser ) )
            {
                if ( !Page.IsPostBack )
                {
                    BindGrid();
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

        protected void gRecordings_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )gRecordings.DataKeys[e.RowIndex]["id"] );
        }

        protected void gRecordings_Delete( object sender, RowEventArgs e )
        {
            Recording recording = recordingService.Get( ( int )gRecordings.DataKeys[e.RowIndex]["id"] );
            if ( BlockInstance != null )
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

            if ( recordingId == 0 && cbStartRecording.Visible && cbStartRecording.Checked )
                recording = recordingService.StartRecording(1, tbLabel.Text, tbApp.Text, tbStream.Text, tbRecording.Text, CurrentPersonId);

            if (recording == null)
            {
                if ( recordingId == 0 )
                {
                    recording = new Recording();
                    recordingService.Add( recording, CurrentPersonId );
                }
                else
                {
                    recording = recordingService.Get( recordingId );
                }

                recording.App = tbApp.Text;
                recording.Date = Convert.ToDateTime( tbDate.Text );
                recording.StreamName = tbStream.Text;
                recording.Label = tbLabel.Text;
                recording.RecordingName = tbRecording.Text;
                recording.StartTime = Convert.ToDateTime( tbStarted.Text );
                recording.StopTime = Convert.ToDateTime( tbStopped.Text );

                recordingService.Save( recording, CurrentPersonId );
            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Internal Methods

        private void BindGrid()
        {
            gRecordings.DataSource = recordingService.Queryable().OrderByDescending( s => s.Date ).ToList();
            gRecordings.DataBind();
        }

        protected void ShowEdit( int recordingId )
        {
            Recording recording = recordingService.Get( recordingId );

            if ( recording != null )
            {
                lAction.Text = "Edit";
                hfRecordingId.Value = recording.Id.ToString();

                tbApp.Text = recording.App ?? string.Empty;
                tbDate.Text = recording.Date.HasValue ? recording.Date.Value.ToShortDateString() : string.Empty;
                tbStream.Text = recording.StreamName ?? string.Empty;
                tbLabel.Text = recording.Label ?? string.Empty;
                tbRecording.Text = recording.RecordingName ?? string.Empty;
                tbStarted.Text = recording.StartTime.HasValue ? recording.StartTime.Value.ToShortDateString() : string.Empty;
                tbStopped.Text = recording.StopTime.HasValue ? recording.StopTime.Value.ToShortDateString() : string.Empty;
                tbRecordingPath.Text = recording.RecordingPath ?? string.Empty;

                tbStarted.Visible = recording.StartTime.HasValue;
                tbStopped.Visible = recording.StopTime.HasValue;
                tbRecordingPath.Visible = !string.IsNullOrEmpty( recording.RecordingPath );
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

                tbStarted.Visible = false;
                tbStopped.Visible = false;
                tbRecordingPath.Visible = false;
                cbStartRecording.Visible = true;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        #endregion

    }
}