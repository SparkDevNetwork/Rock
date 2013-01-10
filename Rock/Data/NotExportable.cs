using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Data
{
    /// <summary>
    /// Attribute to decorate class properties that should not be exported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotExportable : System.Attribute
    {
    }
}