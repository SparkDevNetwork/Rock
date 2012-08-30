//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

using Rock.Data;

namespace Rock.Com.CCVOnline.Service
{
	/// <summary>
	/// Auth POCO Service class
	/// </summary>
	public partial class RecordingService : Service<Recording, RecordingDto>
    {
		static public Rock.Net.WebResponse SendRecordingRequest( string app, string streamName, string recordingName, string action )
        {
            var globalAttributes = Rock.Web.Cache.GlobalAttributes.Read();

            if ( globalAttributes.AttributeValues.ContainsKey( "ccvonlineWowzaServer" ) )
            {
                string wowzaServerUrl = globalAttributes.AttributeValues["ccvonlineWowzaServer"].Value;
                if ( !string.IsNullOrWhiteSpace( wowzaServerUrl ) )
                {
                    Dictionary<string, string> parms = new Dictionary<string, string>();
                    parms.Add( "app", HttpUtility.UrlEncode( app ) );
                    parms.Add( "streamname", HttpUtility.UrlEncode( streamName ) );
                    parms.Add( "recordingname", HttpUtility.UrlEncode( recordingName ) );
                    parms.Add( "action", HttpUtility.UrlEncode( action ) );

                    return Rock.Net.WebRequest.Send( wowzaServerUrl, "GET", parms, null );
                }
            }

            throw new ApplicationException( "missing 'ccvonlineWowzaServer' Global Attribute value" );
        }

        static public string ParseResponse(string message)
        {
            Match match = Regex.Match( message, @"((?<=(\<h1\>)).*(?=(\<\/h1\>)))", RegexOptions.IgnoreCase );
            if ( match.Success )
                return match.Value;
            return string.Empty;
        }

        public Recording StartRecording( int? campusId, string label, string app, string streamName, string recordingName, int? personId )
        {
            Rock.Net.WebResponse response = SendRecordingRequest( app, streamName, recordingName, "start" );

            if (response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Recording recording = new Recording();
                this.Add( recording, personId );

                recording.CampusId = campusId;
                recording.Date = DateTime.Today;
                recording.Label = label;
                recording.App = app;
                recording.StreamName = streamName;
                recording.RecordingName = recordingName;
                recording.StartTime = DateTime.Now;
                recording.StartResponse = ParseResponse( response.Message );
                this.Save( recording, personId );

                return recording;
            }

            return null;
        }

        public Recording StopRecording( int? campusId, string label, string app, string streamName, string recordingName, int? personId )
        {
            Rock.Net.WebResponse response = SendRecordingRequest( app, streamName, recordingName, "stop" );

            if ( response != null && response.HttpStatusCode == System.Net.HttpStatusCode.OK )
            {
                IQueryable<Recording> recordings = Queryable().
                    Where( r =>
                        r.CampusId == campusId &&
                        r.Label == label &&
                        r.App == app &&
                        r.StreamName == streamName &&
                        r.RecordingName == recordingName &&
                        r.StartTime != null &&
                        r.StopTime == null );

                Recording stoppedRecording = new Recording();
                DateTime stopTime = DateTime.Now;
                string responseMessage = ParseResponse( response.Message );

                foreach ( var recording in recordings.OrderBy( r => r.CreatedDateTime ).ToList() )
                {
                    recording.StopTime = stopTime;
                    recording.StopResponse = responseMessage;
                    this.Save( recording, personId );

                    stoppedRecording = recording;
                }

                return stoppedRecording;
            }

            return null;
        }
    }
}
