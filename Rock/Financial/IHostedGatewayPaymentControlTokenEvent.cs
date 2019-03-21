using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Financial
{
    /// <summary>
    /// HostedGatewayPaymentControls that implement this have a TokenReceived event that can be used to notify that a payment token response has been received
    /// </summary>
    public interface IHostedGatewayPaymentControlTokenEvent
    {
        /// <summary>
        /// Occurs when a payment token is received from the hosted gateway
        /// </summary>
        event EventHandler TokenReceived;
    }
}
