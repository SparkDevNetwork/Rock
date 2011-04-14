using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Models.Core
{
    public partial class Attribute 
    {
        public string GetValue (int entityId)
        {
            AttributeValue attributeValue = this.AttributeValues.Where( v => v.EntityId == entityId ).FirstOrDefault();
            if ( attributeValue != null )
                return attributeValue.Value;
            return DefaultValue;
        }
    }
}
