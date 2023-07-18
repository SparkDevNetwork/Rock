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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Prayer.PrayerRequestEntry
{
    /// <summary>
    /// The bag that contains all the information to save a prayer request.
    /// </summary>
    public class PrayerRequestEntrySaveRequestBag
    {
        /// <summary>
        /// The category unique identifier to which the prayer request belongs.
        /// </summary>
        public Guid? CategoryGuid { get; set; }

        /// <summary>
        /// The first name of the person making the prayer request.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the person making the prayer request.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The email of the person making the prayer request.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The mobile phone number of the person making the prayer request.
        /// </summary>
        public string MobilePhoneNumber { get; set; }

        /// <summary>
        /// The mobile phone number country code of the person making the prayer request.
        /// </summary>
        public string MobilePhoneNumberCountryCode { get; set; }

        /// <summary>
        /// The campus unique identifier of the person making the prayer request.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// The prayer request text.
        /// </summary>
        public string Request { get; set; }

        /// <summary>
        /// Determines if this is an urgent prayer request.
        /// </summary>
        public bool? IsUrgent { get; set; }

        /// <summary>
        /// Determines if comments can be made against this prayer request.
        /// </summary>
        public bool? AllowComments { get; set; }

        /// <summary>
        /// Determines if the prayer request is public.
        /// <para>Defaults to the block setting value if <c>null</c>.</para>
        /// </summary>
        public bool? IsPublic { get; set; }

        /// <summary>
        /// The prayer request attribute values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
