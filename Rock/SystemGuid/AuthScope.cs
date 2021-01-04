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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.SystemGuid
{
    /// <summary>
    /// List of System Guids for OIDC Scopes.
    /// </summary>
    public static class AuthScope
    {
        /// <summary>
        /// The address scope
        /// </summary>
        public const string ADDRESS = "F12EEBE7-7CE4-46C5-80DA-A2208011216E";
        /// <summary>
        /// The email scope
        /// </summary>
        public const string EMAIL = "27F77AE2-81A2-4602-B530-8D75376201F2";
        /// <summary>
        /// The offline scope
        /// </summary>
        public const string OFFLINE = "580662DE-EE9B-4CA2-847A-615B7E61E15B";
        /// <summary>
        /// The phone scope
        /// </summary>
        public const string PHONE = "3AF58238-D3B9-4FCE-971C-A55BEB51241E";
        /// <summary>
        /// The profile scope
        /// </summary>
        public const string PROFILE = "5FE34F25-288A-48C7-AAE1-F471EADCE1EE";
    }
}
