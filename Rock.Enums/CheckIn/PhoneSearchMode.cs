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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// The different ways a phone number search will be performed.
    /// </summary>
    public enum PhoneSearchMode
    {
        /// <summary>
        /// The phone number must contain the search term anywhere inside of it.
        /// </summary>
        Contains = 0,

        /// <summary>
        /// The phone number must end with the search term.
        /// </summary>
        EndsWith = 1
    }
}
