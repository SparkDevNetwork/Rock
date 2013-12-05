//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.AttributeQualifier"/> entity objects.
    /// </summary>
    public partial class AttributeQualifierService
    {
        /// <summary>
        /// Returns an enumerable collection containing the <see cref="Rock.Model.AttributeQualifier">AttributeQualifiers</see> by <see cref="Rock.Model.Attribute"/>.
        /// </summary>
        /// <param name="attributeId">A <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Attribute"/> to retrieve <see cref="Rock.Model.AttributeQualifier"/>.</param>
        /// <returns>An enumerable collection containing the <see cref="Rock.Model.AttributeQualifier">AttributeQualifiers</see> that the specified <see cref="Rock.Model.Attribute"/> uses.</returns>
        public IEnumerable<AttributeQualifier> GetByAttributeId( int attributeId )
        {
            return Repository.Find( t => t.AttributeId == attributeId );
        }
    }
}
