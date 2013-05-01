//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Field attribute for selecting a DateTime.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DateFieldAttribute : FieldAttribute
    {
        public DateFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null)
            : base( name, description, required, defaultValue, category, order, key, typeof( DateFieldType ).FullName )
        {
        }
    }
}
