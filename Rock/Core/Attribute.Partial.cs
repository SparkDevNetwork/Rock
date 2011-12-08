using System.Linq;

namespace Rock.Core
{
    public partial class Attribute 
    {
        /// <summary>
        /// Gets the value of an attribute
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public string GetValue (int entityId)
        {
            AttributeValue attributeValue = this.AttributeValues.Where( v => v.EntityId == entityId ).FirstOrDefault();
            if ( attributeValue != null )
                return attributeValue.Value;
            return DefaultValue;
        }
    }
}
