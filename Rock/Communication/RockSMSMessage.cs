using System.Collections.Generic;
using Rock.Communication.Transport;
using Rock.Web.Cache;

namespace Rock.Communication
{
    class RockSMSMessage : RockMessage
    {
        public DefinedValueCache FromNumber { get; set; }

        public string Message { get; set; }

        public override bool Send( out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            string errorMessage = string.Empty;
            var transport = TransportComponent.GetByMedium( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS, out errorMessage );
            if ( transport == null )
            {
                errorMessages.Add( errorMessage );
                return false;
            }

            return transport.Send( this, out errorMessages );
        }

    }
}
