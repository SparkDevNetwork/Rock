//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.SystemGuid
    
    /// <summary>
    /// Static Guids used by the Rock ChMS application
    /// </summary>
    public static class DefinedValue
        
        /// <summary>
        /// Active Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_ACTIVE      get      return new Guid( "618F906C-C33D-4FA3-8AEF-E58CB7B63F1E" ); } }

        /// <summary>
        /// Inactive Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_INACTIVE      get      return new Guid( "1DAD99D5-41A9-4865-8366-F269902B80A4" ); } }

        /// <summary>
        /// Pending Record Status
        /// </summary>
        public static Guid PERSON_RECORD_STATUS_PENDING      get      return new Guid( "283999EC-7346-42E3-B807-BCE9B2BABB49" ); } }

        /// <summary>
        /// Person Record Type
        /// </summary>
        public static Guid PERSON_RECORD_TYPE_PERSON      get      return new Guid( "36CF10D6-C695-413D-8E7C-4546EFEF385E" ); } }

        /// <summary>
        /// Business Record Type
        /// </summary>
        public static Guid PERSON_RECORD_TYPE_BUSINESS      get      return new Guid( "BF64ADD3-E70A-44CE-9C4B-E76BBED37550" ); } }
    }
}