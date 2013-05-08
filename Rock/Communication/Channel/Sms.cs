//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

using Rock.Attribute;
using Rock.Web.UI.Controls.Communication;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An SMS communication
    /// </summary>
    [Description( "An SMS communication" )]
    [Export(typeof(ChannelComponent))]
    [ExportMetadata("ComponentName", "SMS")]
    public class Sms : ChannelComponent
    {
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override ChannelControl Control
        {
            get { return new Rock.Web.UI.Controls.Communication.Sms(); }
        }
 
    }
}
