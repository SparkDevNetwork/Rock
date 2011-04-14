using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Models
{
    /// <summary>
    /// Custom attribute used to decorate model properties that should be tracked.  Any changes to
    /// properties with this attribute will be logged in the coreEntityChange table
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class TrackChangesAttribute : System.Attribute
    {
    }
}