using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.FieldTypes
{
    /// <summary>
    /// Field used to save and dispaly a page reference
    /// </summary>
    public class PageReference : Field
    {
        // assumed output is = "int1,int2"
        // where int1 = page id
        // and int2 = route id
    }
}