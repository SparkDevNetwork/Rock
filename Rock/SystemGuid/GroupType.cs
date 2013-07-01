//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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