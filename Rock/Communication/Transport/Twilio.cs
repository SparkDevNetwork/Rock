//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using Twilio;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Communication transport for sending SMS messages using Twilio
    /// </summary>
    [Description( "Sends a communication through Twilio API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Twilio" )]
    [TextField( "SID", "Your Twilio Account SID (find at https://www.twilio.com/user/account)", true, "", "", 0 )]
    [TextField( "Token", "Your Twilio Account Token", true, "", "", 1 )]
    public class Twilio : TransportComponent
    {
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="CurrentPersonId">The current person id.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication, int? CurrentPersonId )
        {
            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( DateTime.Now ) > 0 ) )
            {
                string fromPhone = string.Empty;
                string fromValue = communication.GetChannelDataValue( "FromValue" );
                int fromValueId = int.MinValue;
                if ( int.TryParse( fromValue, out fromValueId ) )
                {
                    fromPhone = DefinedValueCache.Read( fromValueId ).Description;
                }

                if ( !string.IsNullOrWhiteSpace( fromPhone ) )
                {
                    string accountSid = GetAttributeValue( "SID" );
                    string authToken = GetAttributeValue( "Token" );
                    var twilio = new TwilioRestClient( accountSid, authToken );

                    var recipientService = new CommunicationRecipientService();

                    var globalConfigValues = GlobalAttributesCache.GetMergeFields( null );

                    bool recipientFound = true;
                    while ( recipientFound )
                    {
                        RockTransactionScope.WrapTransaction( () =>
                        {
                            var recipient = recipientService.Get( communication.Id, CommunicationRecipientStatus.Pending ).FirstOrDefault();
                            if ( recipient != null )
                            {
                                string phoneNumber = recipient.Person.PhoneNumbers
                                    .Where( p => p.IsMessagingEnabled )
                                    .Select( p => p.Number )
                                    .FirstOrDefault();

                                if ( string.IsNullOrWhiteSpace( phoneNumber ) )
                                {
                                    recipient.Status = CommunicationRecipientStatus.Failed;
                                    recipient.StatusNote = "No Phone Number with Messaging Enabled";
                                }
                                else
                                {
                                    // Create merge field dictionary
                                    var mergeObjects = MergeValues( globalConfigValues, recipient );
                                    string subject = communication.Subject.ResolveMergeFields( mergeObjects );

                                    try
                                    {
                                        twilio.SendMessage( fromPhone, phoneNumber, subject );
                                        recipient.Status = CommunicationRecipientStatus.Success;
                                    }
                                    catch ( Exception ex )
                                    {
                                        recipient.Status = CommunicationRecipientStatus.Failed;
                                        recipient.StatusNote = "Twilio Exception: " + ex.Message;
                                    }
                                }
                                recipientService.Save( recipient, CurrentPersonId );
                            }
                            else
                            {
                                recipientFound = false;
                            }
                        } );
                    }
                }
            }
        }

    }
}
