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

using Rock.Extension;
using Rock.Model;

namespace Rock.Pbx
{
    /// <summary>
    /// MEF Component for PBX Systems
    /// </summary>
    public abstract class PbxComponent : Component
    {
        public abstract bool Originate( string fromPhone, string toPhone, string callerId, out string message );

        public abstract bool Originate( Person fromPerson, string toPhone, string callerId, out string message );

        public abstract int DownloadCdr( DateTime? startDate = null );
    }

}