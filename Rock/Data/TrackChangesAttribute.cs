//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Data
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