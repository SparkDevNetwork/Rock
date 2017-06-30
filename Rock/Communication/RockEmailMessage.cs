using System.Collections.Generic;
using System.Linq;
using Rock.Communication.Transport;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    class RockEmailMessage : RockMessage
    {
        public string FromName { get; set; }

        public string FromEmail { get; set; }

        public string ReplyToEmail { get; set; }

        public List<string> CCEmails { get; set; } = new List<string>();

        public List<string> BCCEmails { get; set; } = new List<string>();

        public string Subject { get; set; }

        public string Message { get; set; }

        public Dictionary<string, string> MessageMetaData { get; set; } = new Dictionary<string, string>();

        public override bool Send( out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            string errorMessage = string.Empty;
            var transport = TransportComponent.GetByMedium( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL, out errorMessage );
            if ( transport == null )
            {
                errorMessages.Add( errorMessage );
                return false;
            }

            return transport.Send( this, out errorMessages );
        }

    }
}
