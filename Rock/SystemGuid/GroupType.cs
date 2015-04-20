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
        /// Security Role group type guid
        /// </summary>
        public const string GROUPTYPE_SECURITY_ROLE= "AECE949F-704C-483E-A4FB-93D5E4720C4C";

        /// <summary>
        /// Family group type guid
        /// </summary>
        public const string GROUPTYPE_FAMILY= "790E3215-3B10-442B-AF69-616C0DCB998E";

        /// <summary>
        /// Event Attendees group type guid
        /// </summary>
        public const string GROUPTYPE_EVENTATTENDEES = "3311132B-268D-44E9-811A-A56A0835E50A";

        /// <summary>
        /// Know relationship group type guid
        /// </summary>
        public const string GROUPTYPE_KNOWN_RELATIONSHIPS = "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF";

        /// <summary>
        /// Implied relationship group type guid
        /// </summary>
        public const string GROUPTYPE_IMPLIED_RELATIONSHIPS = "8C0E5852-F08F-4327-9AA5-87800A6AB53E";

        /// <summary>
        /// Serving Team group type guid
        /// </summary>
        public const string GROUPTYPE_SERVING_TEAM = "2C42B2D4-1C5F-4AD5-A9AD-08631B872AC4";

        /// <summary>
        /// Small Group Section group type guid
        /// </summary>
        public const string GROUPTYPE_SMALL_GROUP_SECTION = "FAB75EC6-0402-456A-BE34-252097DE4F20";

        /// <summary>
        /// Small Group group type guid
        /// </summary>
        public const string GROUPTYPE_SMALL_GROUP = "50FCFB30-F51A-49DF-86F4-2B176EA1820B";

        /// <summary>
        /// Application Group group type guid
        /// </summary>
        public const string GROUPTYPE_APPLICATION_GROUP = "3981CF6D-7D15-4B57-AACE-C0E25D28BD49";

        /// <summary>
        /// Organization Unit group type guid
        /// </summary>
        public const string GROUPTYPE_ORGANIZATION_UNIT = "AAB2E9F4-E828-4FEE-8467-73DC9DAB784C";
    }
}