//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FriendlyTypeNameAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FriendlyTypeNameAttribute" /> class.
        /// </summary>
        /// <param name="friendlyTypeName">Name of the friendly type.</param>
        public FriendlyTypeNameAttribute( string friendlyTypeName )
        {
            FriendlyTypeName = friendlyTypeName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string FriendlyTypeName;
    }
}