//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock
{
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public static class SystemGuids
    {
        /* NAMING CONVENTION 
         * 
         * Type           Prefix
         * --------       -------------------
         * Job            JOB_
         * Page           PAGE_
         * 
        */

        /// <summary>
        /// Job Pulse Attribute Guid
        /// </summary>
        public static Guid JOB_PULSE_ATTRIBUTE_GUID { get { return new Guid( "254F45EE-071C-4337-A522-DFDC20B7966A" ); } }

        public static Guid DEFINED_TYPE_PERSON_RECORD_TYPE { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_RECORD_STATUS { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_RECORD_STATUS_REASON { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_STATUS { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_TITLE { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_SUFFIX { get { return new Guid( "" ); } }
        public static Guid DEFINED_TYPE_PERSON_MARITAL_STATUS { get { return new Guid( "" ); } }

    }
}