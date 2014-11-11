// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Attribute;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    [Description( "Sends a communication through SMTP protocol" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "SMTP" )]

    [TextField( "Server", "", true, "", "", 0 )]
    [IntegerField( "Port", "", false, 25, "", 1 )]
    [TextField( "User Name", "", false, "", "", 2 )]
    [TextField( "Password", "", false, "", "", 3, null, true )]
    [BooleanField( "Use SSL", "", false, "", 4 )]
    public class SMTP : SMTPComponent
    {
        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public override string Username
        {
            get
            {
                return GetAttributeValue( "UserName" );
            }
        }
    }
}
