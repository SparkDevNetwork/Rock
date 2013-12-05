//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// Gets the External File file type guid
        /// </summary>
        public const string EXTERNAL_FILE = "29EFF9B7-6814-4B9F-A922-77FA0448EBFA";
    }
}