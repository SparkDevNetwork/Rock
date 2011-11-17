using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rock.Cms;

namespace Rock.Helpers
{
    public static class Attributes
    {
        public static bool CreateAttributes( Type type, string entity, int? currentPersonId )
        {
            return CreateAttributes( type, entity, String.Empty, 0, currentPersonId );
        }
        
        public static bool CreateAttributes( Type type, string entity, string entityQualifier, int entityQualiferId, int? currentPersonId )
        {
            bool attributesUpdated = false;

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                foreach ( object customAttribute in type.GetCustomAttributes( typeof( AttributePropertyAttribute ), true ) )
                {
                    AttributePropertyAttribute blockInstanceProperty = ( AttributePropertyAttribute )customAttribute;
                    attributesUpdated = blockInstanceProperty.UpdateAttribute( 
                        entity, entityQualifier, entityQualiferId, currentPersonId) || attributesUpdated;
                }
            }

            return attributesUpdated;
        }
    }
}