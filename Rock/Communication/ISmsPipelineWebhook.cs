using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Communication
{
    /// <summary>
    /// This should be used by any transport that as a webhook that rock needs to know about. This is currently used by ths Sms Pipeline Details block to show the web hook path.
    /// </summary>
    public interface ISmsPipelineWebhook
    {
        /// <summary>
        /// Gets the sms pipeline webhook path that should be used by this transport.
        /// </summary>
        /// <value>
        /// The sms pipeline webhook path.
        /// </value>
        /// <note>
        /// This should be from the application root (https://www.rocksolidchurch.com/).
        /// In other words you don't need a leading forward slash.
        /// For example, you can just return Webhooks/TwilioSms.ashx
        /// </note>
        string SmsPipelineWebhookPath { get; }
    }
}
