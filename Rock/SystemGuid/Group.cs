using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.SystemGuid
    
    /// <summary>
    /// System Groups
    /// </summary>
    public static class Group
        
        /// <summary>
        /// Gets the administrator group guid
        /// </summary>
        public static Guid GROUP_ADMINISTRATORS      get      return new Guid( "628C51A8-4613-43ED-A18D-4A6FB999273E" ); } }

        /// <summary>
        /// Gets the staff member group guid
        /// </summary>
        public static Guid GROUP_STAFF_MEMBERS      get      return new Guid( "2C112948-FF4C-46E7-981A-0257681EADF4" ); } }

    }
}