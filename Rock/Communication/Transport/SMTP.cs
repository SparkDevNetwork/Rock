//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    [Description( "Sends a communication through SMTP protocol" )]
    [Export(typeof(TransportComponent))]
    [ExportMetadata("ComponentName", "SMTP")]
    [TextField("Server", "", true, "", "", 0)]
    [IntegerField("Port", "", false, 25, "", 1 )]
    [TextField("User Name", "", false, "", "", 2)]
    [TextField("Password", "", false, "", "", 3)]
    [BooleanField("Use SSL", "", false, "", 4)]
    public class SMTP : TransportComponent
    {
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication, int? CurrentPersonId )
        {
            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() )
            {
                // From
                MailMessage message = new MailMessage();
                message.From = new MailAddress(
                    communication.GetChannelDataValue( "FromAddress" ),
                    communication.GetChannelDataValue( "FromName" ) );

                // Reply To
                string replyTo = communication.GetChannelDataValue( "ReplyTo" );
                if ( !string.IsNullOrWhiteSpace( replyTo ) )
                {
                    message.ReplyToList.Add( new MailAddress( replyTo ) );
                }

                // CC
                string cc = communication.GetChannelDataValue( "CC" );
                if ( !string.IsNullOrWhiteSpace( cc ) )
                {
                    foreach ( string ccRecipient in cc.SplitDelimitedValues() )
                    {
                        message.CC.Add( new MailAddress( ccRecipient ) );
                    }
                }

                // BCC
                string bcc = communication.GetChannelDataValue( "BCC" );
                if ( !string.IsNullOrWhiteSpace( bcc ) )
                {
                    foreach ( string bccRecipient in bcc.SplitDelimitedValues() )
                    {
                        message.Bcc.Add( new MailAddress( bccRecipient ) );
                    }
                }

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;

                SmtpClient smtpClient = new SmtpClient( GetAttributeValue( "Server" ) );

                int port = int.MinValue;
                if ( int.TryParse( GetAttributeValue( "Port" ), out port ) )
                {
                    smtpClient.Port = port;
                }

                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                bool useSSL = false;
                smtpClient.EnableSsl = bool.TryParse( GetAttributeValue( "UseSSL" ), out useSSL ) && useSSL;

                string userName = GetAttributeValue( "UserName" );
                string password = GetAttributeValue( "Password" );
                if ( !string.IsNullOrEmpty( userName ) )
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential( userName, password );
                }

                var recipientService = new CommunicationRecipientService();

                var globalConfigValues = GetGlobalConfigValues().ToList();

                bool recipientFound = true;
                while ( recipientFound )
                {
                    RockTransactionScope.WrapTransaction( () =>
                    {
                        var recipient = recipientService.Get( communication.Id, CommunicationRecipientStatus.Pending ).FirstOrDefault();
                        if ( recipient != null )
                        {
                            if ( string.IsNullOrWhiteSpace( recipient.Person.Email ) )
                            {
                                recipient.Status = CommunicationRecipientStatus.Failed;
                                recipient.StatusNote = "No Email Address";
                            }
                            else
                            {
                                message.To.Clear();
                                message.To.Add( new MailAddress( recipient.Person.Email, recipient.Person.FullName ) );

                                var mergeObjects = new Dictionary<string, object>();
                                mergeObjects.Add( "Person", recipient.Person );
                                globalConfigValues.ForEach( c => mergeObjects.Add( c.Key, c.Value ) );
                                recipient.AdditionalMergeValues.ToList().ForEach( v => mergeObjects.Add( v.Key, v.Value ) );

                                message.Subject = communication.Subject.ResolveMergeFields( mergeObjects );
                                message.Body = communication.GetChannelDataValue( "HtmlMessage" ).ResolveMergeFields( mergeObjects );

                                try
                                {
                                    smtpClient.Send( message );
                                    recipient.Status = CommunicationRecipientStatus.Success;
                                }
                                catch ( Exception ex )
                                {
                                    recipient.Status = CommunicationRecipientStatus.Failed;
                                    recipient.StatusNote = "SMTP Exception: " + ex.Message;
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

        private List<string> SplitRecipient( string recipients )
        {
            if ( String.IsNullOrWhiteSpace( recipients ) )
                return new List<string>();
            else
                return new List<string>( recipients.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) );
        }



    }
}
