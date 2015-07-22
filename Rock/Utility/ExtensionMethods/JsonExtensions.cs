using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rock
{
    /// <summary>
    /// JSON Extensions
    /// </summary>
    public static class JsonExtensions
    {
        #region Json Extensions

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJson( this object obj )
        {
            return JsonConvert.SerializeObject( obj, Formatting.Indented,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                } );
        }

        /// <summary>
        /// Attempts to deserialize a json string into T.  If it can't be deserialized, returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrNull<T>( this string val )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( val ) )
                {
                    return default( T );
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>( val );
                }
            }
            catch
            {
                return default( T );
            }
        }

        #endregion
    }
}
