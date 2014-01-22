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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Group Types
    /// </summary>
    public static class GroupType
    {
        /// <summary>
        /// Gets the Security Role group type
        /// </summary>
        public const string GROUPTYPE_SECURITY_ROLE= "AECE949F-704C-483E-A4FB-93D5E4720C4C";

        /// <summary>
        /// Gets the Family group type
        /// </summary>
        public const string GROUPTYPE_FAMILY= "790E3215-3B10-442B-AF69-616C0DCB998E";

        /// <summary>
        /// Gets the Event Attendees GroupType
        /// </summary>
        public const string GROUPTYPE_EVENTATTENDEES = "3311132B-268D-44E9-811A-A56A0835E50A";

        /// <summary>
        /// Know relationship group type
        /// </summary>
        public const string GROUPTYPE_KNOWN_RELATIONSHIPS = "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF";

        /// <summary>
        /// Implied relationship group type
        /// </summary>
        public const string GROUPTYPE_IMPLIED_RELATIONSHIPS = "8C0E5852-F08F-4327-9AA5-87800A6AB53E";
    }
}