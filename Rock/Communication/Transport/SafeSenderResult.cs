using System.Net.Mail;

namespace Rock.Communication.Transport
{
    public class SafeSenderResult
    {
        public bool IsUnsafeDomain { get; set; }
        public MailAddress SafeFromAddress { get; set; }
        public MailAddress ReplyToAddress { get; set; }
    }
}
