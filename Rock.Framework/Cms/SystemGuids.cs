using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Cms
{
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
        

        /* Job Guids */
        public static Guid JOB_PULSE_ATTRIBUTE_GUID 
        {
            get { return new Guid( "254F45EE-071C-4337-A522-DFDC20B7966A" );}
        }
    }
}