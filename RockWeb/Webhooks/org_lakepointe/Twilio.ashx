<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.org_lakepointe.TwilioAsync" %>
// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Webhooks.org_lakepointe
{

    public class TwilioAsync : IHttpAsyncHandler
    {
        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, Object extraData )
        {
            TwilioResponseAsync twilioAsync = new TwilioResponseAsync( cb, context, extraData );
            twilioAsync.StartAsyncWork();
            return twilioAsync;
        }

        public void EndProcessRequest( IAsyncResult result )
        {
        }

        public void ProcessRequest( HttpContext context )
        {
            throw new InvalidOperationException();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    class TwilioResponseAsync : IAsyncResult
    {
        private bool _completed;
        private Object _state;
        private AsyncCallback _callback;
        private HttpContext _context;

        bool IAsyncResult.IsCompleted { get { return _completed; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
        Object IAsyncResult.AsyncState { get { return _state; } }
        bool IAsyncResult.CompletedSynchronously { get { return false; } }
        private const string PHONE_NOTE_GUID = "B54F9D90-9AF3-4E8A-8F33-9338C7C1287F";

        private const bool ENABLE_LOGGING = false;

        public TwilioResponseAsync( AsyncCallback callback, HttpContext context, Object state )
        {
            _callback = callback;
            _context = context;
            _state = state;
            _completed = false;
        }

        public void StartAsyncWork()
        {
            ThreadPool.QueueUserWorkItem( new WaitCallback( StartAsyncTask ), null );
        }

        private void StartAsyncTask( Object workItemState )
        {
            var request = _context.Request;
            var response = _context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid request type." );
            }
            else
            {

                // determine if we should log
                if ( ( !string.IsNullOrEmpty( request.QueryString["Log"] ) && request.QueryString["Log"] == "true" ) || ENABLE_LOGGING )
                {
                    WriteToLog();
                }

                if ( request.Form["SmsStatus"] != null )
                {
                    switch ( request.Form["SmsStatus"] )
                    {
                        case "received":
                            MessageReceived();
                            break;
                        case "undelivered":
                            MessageUndelivered();
                            break;
                    }

                    response.StatusCode = 200;
                }
                else
                {
                    response.StatusCode = 500;
                }
            }

            _completed = true;
            _callback( this );
        }

        private void MessageUndelivered()
        {

            var request = _context.Request;
            string messageSid = string.Empty;

            if ( !string.IsNullOrEmpty( request.Form["MessageSid"] ) )
            {
                messageSid = request.Form["MessageSid"];

                // get communication from the message side
                RockContext rockContext = new RockContext();
                CommunicationRecipientService recipientService = new CommunicationRecipientService( rockContext );

                var communicationRecipient = recipientService.Queryable().Where( r => r.UniqueMessageId == messageSid ).FirstOrDefault();
                if ( communicationRecipient != null )
                {
                    communicationRecipient.Status = CommunicationRecipientStatus.Failed;
                    communicationRecipient.StatusNote = "Message failure notified from Twilio on " + RockDateTime.Now.ToString();
                    rockContext.SaveChanges();
                }
                else
                {
                    WriteToLog( "No recipient was found with the specified MessageSid value!" );
                }
            }
        }

        private void MessageReceived()
        {

            var request = _context.Request;
            var response = _context.Response;
            string fromPhone = string.Empty;
            string toPhone = string.Empty;
            string body = string.Empty;

            if ( !string.IsNullOrEmpty( request.Form["To"] ) )
            {
                toPhone = request.Form["To"];
            }

            if ( !string.IsNullOrEmpty( request.Form["From"] ) )
            {
                fromPhone = request.Form["From"];
            }

            if ( !string.IsNullOrEmpty( request.Form["Body"] ) )
            {
                body = request.Form["Body"];
            }

            if ( !( string.IsNullOrWhiteSpace( toPhone ) ) && !( string.IsNullOrWhiteSpace( fromPhone ) ) )
            {
                string errorMessage = string.Empty;

                string[] blockedNumbers = GlobalAttributesCache.Value( "BlockedPhoneNumbers" ).Split(',');
                if ( toPhone != "20411" && blockedNumbers.Contains(fromPhone.Replace( "+", "" ) ) )
                {
                    // This number is blocked - do not process the response
                    // Do not set errorMessage since that will be sent as a reply if it has a value
                }
                else
                {
                    new Rock.Communication.Medium.Sms().ProcessResponse( toPhone, fromPhone, body, out errorMessage );
                }

                var regexStop = new System.Text.RegularExpressions.Regex( @"^(off|stop|unsubscribe)$" );
                var regexStart = new System.Text.RegularExpressions.Regex( @"^(start|on|subscribe)$" );

                if (regexStop.IsMatch(body.ToLower().Trim()))
                {
                    ProcessStop( fromPhone, toPhone, body );
                }

                if ( regexStart.IsMatch( body.ToLower().Trim() ) )
                {
                    ProcessStart( fromPhone, toPhone, body );
                }

                if ( errorMessage != string.Empty )
                {
                    response.Write( errorMessage );
                }
            }
        }

        private void ProcessStart( string fromPhone, string toPhone, string body )
        {
            var context = new RockContext();
            var mobileDVId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), context ).Id;
            var phoneNoteType = NoteTypeCache.Get( PHONE_NOTE_GUID.AsGuid(), context );

            var noteService = new NoteService( context );
            bool hasPendingUpdates = false;

            fromPhone = fromPhone.Replace( "+", "" );

            var phones = new PhoneNumberService( context ).Queryable( "Person.Aliases" )
                    .Where( p => p.NumberTypeValueId == mobileDVId )
                    .Where( p => p.IsMessagingEnabled == false )
                    .Where( p => ( p.CountryCode + p.Number ) == fromPhone )
                    .ToList();

            foreach ( var phone in phones )
            {
                hasPendingUpdates = true;
                phone.IsMessagingEnabled = true;
                phone.ModifiedByPersonAliasId = phone.Person.PrimaryAliasId;

                if ( phoneNoteType != null )
                {
                    PhoneNumber.CleanNumber( toPhone );
                    var note = new Note()
                    {
                        NoteTypeId = phoneNoteType.Id,
                        EntityId = phone.PersonId,
                        IsSystem = false,
                        IsPrivateNote = false,
                        IsAlert = false,
                        Caption = string.Empty,
                        Text = String.Format( "User texted keyword {0} to {1} to enable text messaging for phone number {2}.",
                            body,PhoneNumber.FormattedNumber(toPhone.Substring(1,1), toPhone.Substring(2)) , phone.NumberFormatted ),
                        CreatedByPersonAliasId = phone.Person.PrimaryAliasId,
                        CreatedDateTime = RockDateTime.Now
                    };
                    noteService.Add( note );
                }

            }
            if ( hasPendingUpdates )
            {
                context.SaveChanges();
            }
        }
        private void ProcessStop( string fromPhone, string toPhone, string body )
        {
            var context = new RockContext();
            var mobileDVId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), context ).Id;
            var phoneNoteType = NoteTypeCache.Get( PHONE_NOTE_GUID.AsGuid(), context );

            var noteService = new NoteService( context );
            bool hasPendingUpdates = false;
            fromPhone = fromPhone.Replace( "+", "" );

            var phones = new PhoneNumberService( context ).Queryable( "Person.Aliases" )
                    .Where( p => p.NumberTypeValueId == mobileDVId )
                    .Where( p => p.IsMessagingEnabled == true )
                    .Where( p => ( p.CountryCode + p.Number ) == fromPhone )
                    .ToList();

            foreach ( var phone in phones )
            {
                hasPendingUpdates = true;
                phone.IsMessagingEnabled = false;
                phone.ModifiedByPersonAliasId = phone.Person.PrimaryAliasId;

                if ( phoneNoteType != null )
                {
                    var note = new Note()
                    {
                        NoteTypeId = phoneNoteType.Id,
                        EntityId = phone.PersonId,
                        IsSystem = false,
                        IsPrivateNote = false,
                        IsAlert = false,
                        Caption = string.Empty,
                        Text = String.Format( "User texted keyword {0} to {1} to disable text messaging for phone number {2}.",
                            body, PhoneNumber.FormattedNumber(toPhone.Substring(1,1), toPhone.Substring(2)) , phone.NumberFormatted ),
                        CreatedByPersonAliasId = phone.Person.PrimaryAliasId,
                        CreatedDateTime = RockDateTime.Now
                    };
                    noteService.Add( note );
                }

            }
            if ( hasPendingUpdates )
            {
                context.SaveChanges();
            }
        }

        private void WriteToLog()
        {
            var request = _context.Request;
            var formValues = new List<string>();
            foreach ( string name in request.Form.AllKeys )
            {
                formValues.Add( string.Format( "{0}: '{1}'", name, request.Form[name] ) );
            }

            WriteToLog( formValues.AsDelimited( ", " ) );
        }

        private void WriteToLog( string message )
        {
            string logFile = _context.Server.MapPath( "~/App_Data/Logs/TwilioLog.txt" );

            // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
            var maxRetry = 3;
            for ( int retry = 0; retry < maxRetry; retry++ )
            {
                try
                {
                    using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
                    {
                        using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                        {
                            sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
                        }
                    }
                }
                catch ( System.IO.IOException )
                {
                    if ( retry < maxRetry - 1 )
                    {
                        System.Threading.Thread.Sleep( 2000 );
                    }
                }
            }

        }

    }
}