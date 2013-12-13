//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Email class
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Gets or sets from.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string From { get; set; }

        /// <summary>
        /// Gets or sets to.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the cc.
        /// </summary>
        /// <value>
        /// The cc.
        /// </value>
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the BCC.
        /// </summary>
        /// <value>
        /// The BCC.
        /// </value>
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email" /> class.
        /// </summary>
        /// <param name="templateGuid">The template GUID.</param>
        public Email( string templateGuid )
            : this( new Guid( templateGuid ) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="templateGuid">The template GUID.</param>
        public Email( Guid templateGuid )
        {
            Rock.Model.EmailTemplateService service = new Model.EmailTemplateService();
            Rock.Model.EmailTemplate template = service.GetByGuid( templateGuid );
            if ( template != null )
            {
                To = template.To;
                From = template.From;
                Cc = template.Cc;
                Bcc = template.Bcc;
                Subject = template.Subject;
                Body = template.Body;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public Email( string from, string to, string cc, string bcc, string subject, string body )
        {
            From = from;
            To = to;
            Cc = cc;
            Bcc = Bcc;
            Subject = subject;
            Body = body;
        }

        /// <summary>
        /// Sends this instance.
        /// </summary>
        public void Send()
        {
            List<string> to = SplitRecipient( To );
            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            Email.Send( From, to, cc, bcc, Subject, Body );
        }

        /// <summary>
        /// Sends the specified recipient merge values.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void Send( Dictionary<string, Dictionary<string, object>> recipients )
        {
            var configValues = GlobalAttributesCache.GetMergeFields( null );

            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            foreach ( KeyValuePair<string, Dictionary<string, object>> recipient in recipients )
            {
                List<string> to = SplitRecipient( To );
                to.Add( recipient.Key );

                var mergeObjects = recipient.Value;

                // Combine the global merge values with the recipient specific merge values
                configValues.ToList().ForEach( g => mergeObjects.Add( g.Key, g.Value ) );

                // Resolve any merge codes in the subject and body
                string subject = Subject.ResolveMergeFields( mergeObjects );
                string body = Body.ResolveMergeFields( mergeObjects );

                Email.Send( From, to, cc, bcc, subject, body );
            }
        }

        private List<string> SplitRecipient( string recipients )
        {
            if ( String.IsNullOrWhiteSpace( recipients ) )
                return new List<string>();
            else
                return new List<string>( recipients.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) );
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public static void Send( string from, List<string> to, List<string> cc, List<string> bcc, string subject, string body)
        {
            var globalAttributes = GlobalAttributesCache.Read();

            string server = globalAttributes.GetValue( "SMTPServer" );
            if ( !string.IsNullOrEmpty( server ) )
            {
                int port = 0;
                if ( !Int32.TryParse( globalAttributes.GetValue( "SMTPPort" ), out port ) )
                    port = 0;

                bool useSSL = false;
                if ( !bool.TryParse( globalAttributes.GetValue( "SMTPUseSSL" ), out useSSL ) )
                    useSSL = false;

                string userName = globalAttributes.GetValue( "SMTPUserName" );
                string password = globalAttributes.GetValue( "SMTPPassword" );
            
                if (string.IsNullOrWhiteSpace(from))
                {
                    from = globalAttributes.GetValue( "OrganizationEmail" );
                }

                MailMessage message = new MailMessage();
                message.From = new MailAddress( from );

                foreach ( string e in to )
                    if ( !string.IsNullOrWhiteSpace( e ) )
                        message.To.Add( new MailAddress( e ) );

                if ( message.To.Count > 0 )
                {
                    foreach ( string e in cc )
                        if ( !string.IsNullOrWhiteSpace( e ) )
                            message.CC.Add( new MailAddress( e ) );

                    foreach ( string e in bcc )
                        if ( !string.IsNullOrWhiteSpace( e ) )
                            message.Bcc.Add( new MailAddress( e ) );
 
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.Priority = MailPriority.Normal;

                    SmtpClient smtpClient = new SmtpClient( server );
                    if ( port != 0 )
                        smtpClient.Port = port;

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = useSSL;

                    if ( !string.IsNullOrEmpty( userName ) )
                    {
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new System.Net.NetworkCredential( userName, password );
                    }

                    smtpClient.Send( message );
                }
            }
        }
    }
}