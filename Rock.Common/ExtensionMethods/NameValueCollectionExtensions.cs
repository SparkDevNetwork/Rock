using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Rock
{
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Converts the <paramref name="nameValueCollection"/> to a case-insensitive dictionary.
        /// <para>If the <paramref name="nameValueCollection"/> has duplicate keys for some reason,
        /// then the resulting dictionary will take the last value.</para>
        /// <para><c>null</c> and <c>""</c> keys are omitted from the dictionary.</para>
        /// </summary>
        /// <remarks>
        /// The keys in a NameValueCollection are case-insensitive.
        /// 
        /// If multiple values are added to a NameValueCollection with the same case-insensitive key,
        /// then accessing the value through the NameValueCollection indexer
        /// will return a concatenation of the values separated by commas.
        ///
        /// When converting to a dictionary, we will store each key with its concatenated value.
        /// </remarks>
        /// <example>
        /// var nameValueCollection = new NameValueCollection();
        /// nameValueCollection.Add( "key", "value1" ); // nameValueCollection["key"] == "value1"
        /// nameValueCollection.Add( "key", "value2" ); // nameValueCollection["key"] == "value1,value2"
        ///
        /// var dictionary = nameValueCollection.ToSimpleQueryString(); // { { "key", "value1,value2" } }
        /// </example>
        /// <param name="nameValueCollection">The name value collection.</param>
        /// <returns>The dictionary of query string parameters.</returns>
        public static Dictionary<string, string> ToSimpleQueryStringDictionary( this NameValueCollection nameValueCollection )
        {
            var dictionary = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

            if ( nameValueCollection != null )
            {
                foreach ( string key in nameValueCollection.AllKeys.Where( k => !k.IsNullOrWhiteSpace() ) )
                {
                    dictionary.AddOrReplace( key, nameValueCollection[key] );
                }
            }

            return dictionary;
        }

    }
}
