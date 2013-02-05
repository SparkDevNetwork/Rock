//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Attribute
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class LinkedPageAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkedPageAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="key">The key.</param>
        public LinkedPageAttribute(string name, string key)
            : base( 0, name, false, "", key, "Advanced", "", typeof( Rock.Field.Types.PageReference ).FullName )
        {
        }
    }
}