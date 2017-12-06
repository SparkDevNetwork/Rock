// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Attribute;
using Rock.Model;

namespace Rock.Pbx.Provider
{

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Pbx.PbxComponent" />
    [Description( "A test PBX provider." )]
    [Export( typeof( PbxComponent ) )]
    [ExportMetadata( "ComponentName", "PBX Tester" )]

    [TextField( "Server URL", "The URL of the PBX node (http://myserver:80)", true, key: "ServerUrl", order: 0 )]
    [TextField( "Username", "The username to use to connect with.", true, order: 1 )]
    [TextField( "Password", "The password to use to connect with.", true, order: 2 )]
    public class TestPbx : PbxComponent
    {
        public override bool SupportsOrigination
        {
            get
            {
                return false;
            }
        }

        public override string DownloadCdr( DateTime? startDate = null )
        {
            return "Not implemented";
        }

        public override bool Originate( string fromPhone, string toPhone, string callerId, out string message )
        {
            message = string.Empty;
            return true;
        }

        public override bool Originate( Person fromPerson, string toPhone, string callerId, out string message )
        {
            message = string.Empty;
            return true;
        }
    }
}


