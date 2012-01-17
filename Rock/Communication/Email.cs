using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net.Mail;

namespace Rock.Communication
{
    public class Email
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public Dictionary<string, Dictionary<string, string>> RecipientMergeValues { get; set; }

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

        public Email( string from, string to, string cc, string bcc, string subject, string body )
        {
            From = from;
            To = to;
            Cc = cc;
            Bcc = Bcc;
            Subject = subject;
            Body = body;
        }

        public void Send()
        {
            List<string> to = SplitRecipient( To );
            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            Email.Send( From, to, cc, bcc, Subject, Body );
        }

        public void Send( Dictionary<string, Dictionary<string, string>> recipientMergeValues )
        {
            List<string> cc = SplitRecipient( Cc );
            List<string> bcc = SplitRecipient( Bcc );

            foreach ( KeyValuePair<string, Dictionary<string, string>> recipient in recipientMergeValues )
            {
                List<string> to = SplitRecipient( To );

                to.Add( recipient.Key );

                string subject = Subject;
                string body = Body;

                foreach ( KeyValuePair<string, string> mergeValue in recipient.Value )
                {
                    subject = subject.ReplaceCaseInsensitive( "{" + mergeValue.Key + "}", mergeValue.Value );
                    body = body.ReplaceCaseInsensitive( "{" + mergeValue.Key + "}", mergeValue.Value );
                }

                Email.Send( From, to, cc, bcc, subject, body );
            }

        }

        private List<string> SplitRecipient( string recipients )
        {
            if ( String.IsNullOrWhiteSpace( recipients ) )
                return new List<string>();
            else
                return new List<string>( recipients.Split( ';' ) );
        }

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