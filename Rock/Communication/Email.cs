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

using DotLiquid;
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
        /// Gets or sets the SMTP server.
        /// </summary>
        /// <value>
        /// The SMTP server.
        /// </value>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the SMTP port.
        /// </summary>
        /// <value>
        /// The SMTP port.
        /// </value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use SSL.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use SSL]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSSL { get; set; }

        /// <summary>
        /// Gets or sets the name of the SMTP user.
        /// </summary>
        /// <value>
        /// The name of the SMTP user.
        /// </value>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the SMTP password.
        /// </summary>
        /// <value>
        /// The SMTP password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="templateGuid">The template GUID.</param>
        public Email( Guid templateGuid )
        {
            Rock.Crm.EmailTemplateService service = new Crm.EmailTemplateService();
            Rock.Crm.EmailTemplate template = service.GetByGuid( templateGuid );
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

            SetSMTPParameters();

            Email.Send( From, to, cc, bcc, Subject, Body, Server, Port, UseSSL, UserName, Password );
        }

        /// <summary>
        /// Sends the specified recipient merge values.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void Send( Dictionary<string, Dictionary<string, object>> recipients )
        {
            var configValues = new Dictionary<string,object>();

            // Get all the global attribute values that begin with "Organization" or "Email"
            // TODO: We don't want to allow access to all global attribute values, as that would be a security
            // hole, but not sure limiting to those that start with "Organization" or "Email" is the best
            // solution either.
            Rock.Web.Cache.GlobalAttributesCache.Read().AttributeValues
                .Where( v => 
                    v.Key.StartsWith( "Organization", StringComparison.CurrentCultureIgnoreCase ) ||
                    v.Key.StartsWith( "Email", StringComparison.CurrentCultureIgnoreCase ) )
                .ToList()
                .ForEach( v => configValues.Add( v.Key, v.Value.Value ) );

            // Add any application config values as available merge objects
            foreach ( string key in System.Configuration.ConfigurationManager.AppSettings.AllKeys )
            {
                configValues.Add( "Config_" + key, System.Configuration.ConfigurationManager.AppSettings[key] );
            }

            // Recursively resolve any of the config values that may have other merge codes as part of their value
            foreach ( var item in configValues.ToList() )
            {
                configValues[item.Key] = ResolveConfigValue( item.Value as string, configValues );
            }

            var configValuesList = configValues.ToList();

            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            SetSMTPParameters();

            foreach ( KeyValuePair<string, Dictionary<string, object>> recipient in recipients )
            {
                List<string> to = SplitRecipient( To );
                to.Add( recipient.Key );

                var mergeObjects = recipient.Value;

                // Combine the global and config file merge values with the recipient specific merge values
                configValuesList.ForEach( g => mergeObjects.Add( g.Key, g.Value ) );

                // Resolve any merge codes in the subject and body
                string subject = Resolve( Subject, mergeObjects );
                string body = Resolve( Body, mergeObjects );

                Email.Send( From, to, cc, bcc, subject, body, Server, Port, UseSSL, UserName, Password );
            }
        }

        private void SetSMTPParameters( )
        {
            var globalAttributes = GlobalAttributesCache.Read();

            this.Server = globalAttributes.GetValue( "SMTPServer" );

            int port = 0;
            if ( !Int32.TryParse( globalAttributes.GetValue( "SMTPPort" ), out port ) )
                port = 0;
            this.Port = port;

            bool useSSL = false;
            if ( !bool.TryParse( globalAttributes.GetValue( "SMTPUseSSL" ), out useSSL ) )
                useSSL = false;
            this.UseSSL = useSSL;

            this.UserName = globalAttributes.GetValue( "SMTPUserName" );
            this.Password = globalAttributes.GetValue( "SMTPPassword" );
        }


        private string ResolveConfigValue( string value, Dictionary<string, object> globalAttributes )
        {
            // If the attribute doesn't contain any merge codes, return the content
            if ( !Regex.IsMatch( value, @".*\{\{.+\}\}.*" ) )
                return value;

            // Resolve the merge codes
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template template = Template.Parse( value );
            string result = template.Render( Hash.FromDictionary( globalAttributes ) );

            // If anything was resolved, keep resolving until nothing changed.
            while ( result != value )
            {
                value = result;
                result = ResolveConfigValue( result, globalAttributes );
            }

            return result;
        }

        private string Resolve( string content, Dictionary<string, object> mergeObjects )
        {
            if ( content == null )
                return string.Empty;

            // If there's no merge codes, just return the content
            if ( !Regex.IsMatch( content, @".*\{\{.+\}\}.*" ) )
                return content;

            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template template = Template.Parse( content );

            return template.Render( Hash.FromDictionary(mergeObjects) );
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
        /// <param name="server">The server.</param>
        /// <param name="port">The port.</param>
        /// <param name="useSSL">if set to <c>true</c> [use SSL].</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public static void Send( string from, List<string> to, List<string> cc, List<string> bcc, string subject, string body, 
            string server, int port, bool useSSL, string userName, string password)
        {
            if ( !string.IsNullOrEmpty( server ) )
            {
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