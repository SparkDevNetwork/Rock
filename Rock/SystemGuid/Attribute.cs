//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
{
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public static class Attribute
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
        public static Guid JOB_PULSE { get { return new Guid( "254F45EE-071C-4337-A522-DFDC20B7966A" ); } }
    }
}