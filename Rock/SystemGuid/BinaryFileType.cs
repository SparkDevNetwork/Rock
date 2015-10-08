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

namespace Rock.SystemGuid
{
    /// <summary>
    /// System file types.  
    /// </summary>
    public class BinaryFiletype
    {
        /// <summary>
        /// The Default file type guid
        /// </summary>
        public const string DEFAULT = "C1142570-8CD6-4A20-83B1-ACB47C1CD377";

        /// <summary>
        /// Gets the Check-in Label File type guid
        /// </summary>
        public const string CHECKIN_LABEL = "DE0E5C50-234B-474C-940C-C571F385E65F";

        /// <summary>
        /// Gets the Contribution-Image (scanned check, scanned envelope, etc) file type guid
        /// </summary>
        public const string CONTRIBUTION_IMAGE = "6D18A9C4-34AB-444A-B95B-C644019465AC";

        /// <summary>
        /// The Person Image file type guid
        /// </summary>
        public const string PERSON_IMAGE = "03BD8476-8A9F-4078-B628-5B538F967AFC";

        /// <summary>
        /// The Location Image file type guid
        /// </summary>
        public const string LOCATION_IMAGE = "DAB74416-3272-4411-BA69-70944B549A4B";

        /// <summary>
        /// The Content Channel Item Image file type guid
        /// </summary>
        public const string CONTENT_CHANNEL_ITEM_IMAGE = "8DBF874C-F3C2-4848-8137-C963C431EB0B";

        /// <summary>
        /// The Media File file type guid
        /// </summary>
        public const string MEDIA_FILE = "6CBEA3B0-E983-40C1-9712-BD3FA2466EAE";

        /// <summary>
        /// The Merge Template file type guid
        /// </summary>
        public const string MERGE_TEMPLATE = "BD63EC0C-2DF8-4C55-97E3-616870C67C59";
    }
}