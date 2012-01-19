//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;

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
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="templateGuid">The template GUID.</param>
        public Email( Guid templateGuid )
        {
            Rock.CRM.EmailTemplateService service = new CRM.EmailTemplateService();
            Rock.CRM.EmailTemplate template = service.GetByGuid( templateGuid );
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
        /// <param name="recipientMergeValues">The recipient merge values.</param>
        public void Send( Dictionary<string, List<object>> recipientMergeValues )
        {
            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            foreach ( KeyValuePair<string, List<object>> recipient in recipientMergeValues )
            {
                List<string> to = SplitRecipient( To );
                to.Add( recipient.Key );

                string subject = ResolveMergeCodes(Subject, recipient.Value);
                string body = ResolveMergeCodes(Body, recipient.Value);

                Email.Send( From, to, cc, bcc, subject, body );
            }

        }

        private string ResolveMergeCodes( string content, List<object> objects )
        {
            if (!content.Contains("{"))
                return content;

            string result = content;

            foreach ( object item in objects )
                result = ResolveMergeCodes(result, item);

            var orgAttributes = Rock.Web.Cache.OrganizationAttributes.Read();
            result = ResolveMergeCodes( result, orgAttributes.AttributeValues );

            return result;
        }

        private string ResolveMergeCodes( string content, object item )
        {
            string result = content;

            if ( item is Dictionary<string, string> )
            {
                Dictionary<string, string> items = item as Dictionary<string, string>;
                foreach(KeyValuePair<string, string> kvp in items)
                    result = result.ReplaceCaseInsensitive( "{" + kvp.Key + "}", kvp.Value );
            }

            else if ( item is Dictionary<string, KeyValuePair<string, string>> )
            {
                Dictionary<string, KeyValuePair<string, string>> items = item as Dictionary<string, KeyValuePair<string, string>>;
                foreach ( KeyValuePair<string, KeyValuePair<string, string>> kvp in items )
                    result = result.ReplaceCaseInsensitive( "{" + kvp.Key + "}", kvp.Value.Value );
            }

            else if ( item is List<object> )
            {
                List<object> itemList = item as List<object>;
                if ( itemList.Count > 0 )
                {
                    Type type = itemList[0].GetType();
                    if ( itemList[0] is Rock.Data.IModel )
                        type = type.BaseType;

                    string itemName = type.Name.ToLower();

                    string contentLc = content.ToLower();
                    int start = contentLc.IndexOf( "{" + itemName + ":repeatbegin}" );
                    int end = contentLc.IndexOf( "{" + itemName + ":repeatend}" );

                    if ( start >= 0 && end > start )
                    {
                        int startOffset = start + itemName.Length + 14;
                        string targetContent = content.Substring( startOffset, end - startOffset );

                        StringBuilder sb = new StringBuilder();
                        foreach ( object subItem in itemList )
                            sb.Append( ResolveMergeCodes( targetContent, subItem ) );

                        result = result.Substring( 0, start ) + sb.ToString() + result.Substring( end + itemName.Length + 12 );
                    }
                }

            }

            else
            {
                Type type = item.GetType();
                if ( item is Rock.Data.IModel )
                    type = type.BaseType;

                string itemName = type.Name;

                string resultLc = result.ToLower();

                PropertyInfo[] properties = item.GetType().GetProperties();
                foreach ( PropertyInfo propertyInfo in properties )
                {
                    string mergeCode = string.Format( "{{{0}:{1}}}", type.Name, propertyInfo.Name );
                    if ( resultLc.Contains( mergeCode.ToLower() ) )
                    {
                        object value = propertyInfo.GetValue( item, null );
                        result = result.ReplaceCaseInsensitive( mergeCode, value != null ? value.ToString() : "" );
                    }
                }
            }

            return result;

        }

        private List<string> SplitRecipient( string recipients )
        {
            if ( String.IsNullOrWhiteSpace( recipients ) )
                return new List<string>();
            else
                return new List<string>( recipients.Split( ';' ) );
        }

        /// <summary>
        /// Sends the specified from.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="cc">The cc.</param>
        /// <param name="bcc">The BCC.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        public static void Send( string from, List<string> to, List<string> cc, List<string> bcc, string subject, string body )
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress( from );

            foreach ( string e in to )
                message.To.Add( new MailAddress( e ) );

            foreach ( string e in cc )
                message.CC.Add( new MailAddress( e ) );

            foreach ( string e in bcc )
                message.Bcc.Add( new MailAddress( e ) );

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.Priority = MailPriority.Normal;

            SmtpClient smtpClient = new SmtpClient( "ccvexchange07" );
            smtpClient.Send( message );
        }
    }
}