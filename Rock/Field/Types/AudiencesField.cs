//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class AudiencesField : SelectFromListFieldType
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
                var service = new DefinedValueService();
                var qry = service.GetByDefinedTypeGuid(SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE);
                return qry.OrderBy( a => a.Name ).ToDictionary( k => k.Id.ToString(), v => v.Name );
            }
        }
    }
}
