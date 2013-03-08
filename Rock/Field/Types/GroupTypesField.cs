//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select 0 or more GroupTypes 
    /// </summary>
    public class GroupTypesField : SelectFromListFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> ListSource
        {
            get
            {
                GroupTypeService groupTypeService = new GroupTypeService();
                return groupTypeService.Queryable().OrderBy( a => a.Name ).ToDictionary( k => k.Id.ToString(), v => v.Name );
            }
        }
    }
}