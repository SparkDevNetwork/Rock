//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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