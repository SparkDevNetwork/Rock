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

using Rock.Attribute;

namespace Rock.Communication.Channel
{
    /// <summary>
    /// An email communication
    /// </summary>
    [Description( "An email communication" )]
    [Export(typeof(ChannelComponent))]
    [ExportMetadata("ComponentName", "Email")]
    public class Email : ChannelComponent
    {
        /// <summary>
        /// Gets the control path.
        /// </summary>
        /// <value>
        /// The control path.
        /// </value>
        public override string ControlPath
        {
            get { return "~/Blocks/Communication/Email.ascx"; }
        }
 
    }
}
