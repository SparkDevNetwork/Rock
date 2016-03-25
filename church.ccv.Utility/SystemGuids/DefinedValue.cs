using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace church.ccv.Utility.SystemGuids
{
    public static class DefinedValue
    {
        /// <summary>
        /// Coach wants to follow up with the group member at a later date
        /// </summary>
        public const string NEXT_STEPS_FOLLOW_UP_LATER = "A6A94945-D7B5-4EF7-9284-D42C4E1AB2D3";

        /// <summary>
        /// Member does not want to be contacted anymore
        /// </summary>
        public const string NEXT_STEPS_DO_NOT_CONTACT = "EFDF942F-66D2-4066-A560-59BD6753FB2E";

        /// <summary>
        /// Coach needs this person reassigned for some reason
        /// </summary>
        public const string NEXT_STEPS_REASSIGN_TO_NEW_COACH = "1B005C59-A2A8-4DF8-A2F2-4EE6BE34BCB3";

        /// <summary>
        /// Coach can't get ahold of the member
        /// </summary>
        public const string NEXT_STEPS_UNABLE_TO_REACH = "8A675978-A443-48AE-90FE-6B860A4943FC";

        /// <summary>
        /// Member wants to be in a neighborhood group (and therefore shouldn't be in a NS group)
        /// </summary>
        public const string NEXT_STEPS_REGISTERED_FOR_NEIGHBORHOOD_GROUP = "929D0921-3FDB-4AE3-82BE-0EE322276BF4";
        
        /// <summary>
        /// Member is no longet attending CCV
        /// </summary>
        public const string NEXT_STEPS_NO_LONGER_ATTENDING_CCV = "BA6C1F11-9020-4C15-BC2B-9E12FDF84168";
    }
}
