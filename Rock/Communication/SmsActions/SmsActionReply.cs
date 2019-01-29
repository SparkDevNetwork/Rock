using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Communication.SmsActions
{
    [Description( "Sends a response to the message." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reply" )]
    public class SmsActionReply : SmsActionComponent
    {
        public override string Title => "Reply";

        public override string IconCssClass => "fa fa-comment-o";

        public override string Description => "Sends a response to the message.";

        public override bool ProcessMessage( SmsMessage message, out SmsMessage response )
        {
            response = new SmsMessage
            {
                Message = "Goodbye Cruel World!"
            };

            return true;
        }
    }
}
