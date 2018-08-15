using System;

namespace org.newpointe.ExtraActions
{
    public class ObjectConverter
    {
        public static object ConvertObject( string theObject, Type objectType, bool tryToNull = true )
        {
            if ( objectType.IsEnum )
            {
                return string.IsNullOrWhiteSpace( theObject ) ? null : Enum.Parse( objectType, theObject, true );
            }

            Type underType = Nullable.GetUnderlyingType( objectType );
            if ( underType == null ) // not nullable
            {
                return Convert.ChangeType( theObject, objectType );
            }

            if ( tryToNull && string.IsNullOrWhiteSpace( theObject ) )
            {
                return null;
            }
            return Convert.ChangeType( theObject, underType );
        }
    }
}
