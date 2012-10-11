//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Field.Types
    
    /// <summary>
    /// Field used to save and dispaly a page reference
    /// </summary>
    [Serializable]
    public class PageReference : FieldType
        
        // assumed output is = "int1,int2"
        // where int1 = page id
        // and int2 = route id
    }
}