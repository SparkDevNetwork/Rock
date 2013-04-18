using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    [Description( "Sends a communication through SMTP protocol" )]
    [Export(typeof(TransportComponent))]
    [ExportMetadata("ComponentName", "SMTP")]
    [TextField("Server", "", true, "", "", 0)]
    [IntegerField("Port", "", false, 25, "", 1 )]
    [TextField("User Name", "", false, "", "", 2)]
    [TextField("Password", "", false, "", "", 3)]
    [BooleanField("Use SSL", "", false, "", 4)]
    public class SMTP : TransportComponent
    {
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Send( Model.Communication communication )
        {
            return false;
        }

    }
}
