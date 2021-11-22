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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.PersonalDevice"/> entity type objects.
    /// </summary>
    public partial class PersonalDeviceService
    {
        /// <summary>
        /// Gets the by mac address.
        /// </summary>
        /// <param name="macAddress">The mac address.</param>
        /// <returns></returns>
        public PersonalDevice GetByMACAddress( string macAddress )
        {
            return Queryable().FirstOrDefault( d => d.MACAddress == macAddress );
        }

    }
}
