using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Services.Core
{
    public partial class AttributeService
    {
        public Rock.Models.Core.Attribute GetAttributesByEntityQualifierAndKey( string entity, string entityQualifier, int entityQualifierId, string key )
        {
            return Repository.FirstOrDefault( t =>
                t.Entity == entity &&
                t.EntityQualifier == entityQualifier &&
                t.EntityQualifierId == entityQualifierId &&
                t.Key == key );
        }
    }
}
