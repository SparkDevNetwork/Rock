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
    public class Category
    {

        #region History Categories

        /// <summary>
        /// History changes for person
        /// </summary>
        public const string HISTORY_PERSON = "6F09163D-7DDD-4E1E-8D18-D7CAA04451A7";

        /// <summary>
        /// History of person demographic changes
        /// </summary>
        public const string HISTORY_PERSON_DEMOGRAPHIC_CHANGES = "51D3EC5A-D079-45ED-909E-B0AB2FD06835";

        /// History of Family changes
        /// </summary>
        public const string HISTORY_PERSON_FAMILY_CHANGES = "5C4CCE5A-D7D0-492F-A241-96E13A3F7DF8";

        /// <summary>
        /// history of group membership
        /// </summary>
        public const string HISTORY_PERSON_GROUP_MEMBERSHIP = "325278A4-FACA-4F38-A405-9C090B3BAA34";

        /// <summary>
        /// History of person communications
        /// </summary>
        public const string HISTORY_PERSON_COMMUNICATIONS = "F291034B-7581-48F3-B522-E31B8534D529";

        /// <summary>
        /// History of person activity
        /// </summary>
        public const string HISTORY_PERSON_ACTIVITY = "0836845E-5ED8-4ABE-8787-3B61EF2F0FA5";

        #endregion

        #region Schedule Categories

        /// <summary>
        /// Gets the Service Times schedule category guid
        /// </summary>
        public const string SCHEDULE_SERVICE_TIMES = "4FECC91B-83F9-4269-AE03-A006F401C47E";

        #endregion

    }
}