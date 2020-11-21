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
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// System.Enum extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Gets the first SMS number from the phone numbers collection.
        /// Returns empty string if none is found.
        /// </summary>
        /// <param name="phoneNumbers">The phone numbers.</param>
        /// <returns></returns>
        public static string GetFirstSmsNumber( this ICollection<PhoneNumber> phoneNumbers )
        {
            var phoneNumber = phoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
            return phoneNumber == null ? null : phoneNumber.ToSmsNumber();
        }
    }
}
