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
            return CreateAttributes( type, entity, String.Empty, String.Empty, currentPersonId );
        }
        
        public static bool CreateAttributes( Type type, string entity, string entityQualifierColumn, string entityQualifierValue, int? currentPersonId )
        {
            bool attributesUpdated = false;

            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                foreach ( object customAttribute in type.GetCustomAttributes( typeof( AttributePropertyAttribute ), true ) )
                {
                    AttributePropertyAttribute blockInstanceProperty = ( AttributePropertyAttribute )customAttribute;
                    attributesUpdated = blockInstanceProperty.UpdateAttribute( 
                        entity, entityQualifierColumn, entityQualifierValue, currentPersonId) || attributesUpdated;
                }
            }

            return attributesUpdated;
        }
    }
}