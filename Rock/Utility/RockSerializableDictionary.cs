// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Utility
{
    /// <summary>
    /// A dictionary of objects that can be serialized to a Rock-specific formatted string for portability and storage.
    /// </summary>
    /// <remarks>
    /// The serialization format for the dictionary string is: Key1^Value1|Key2^Value2a,Value2b,Value2c|Key3^Value3
    /// </remarks>
    public class RockSerializableDictionary
    {
        Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        private const string keyValuePairEntrySeparator = "|";
        private const string keyValuePairInternalSeparator = "^";

        /// <summary>
        /// Create a new instance with an empty dictionary.
        /// </summary>
        public RockSerializableDictionary()
        {
            //
        }

        /// <summary>
        /// Create a new instance from an existing dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public RockSerializableDictionary( Dictionary<string, string> dictionary )
        {
            _dictionary = dictionary;

            if ( _dictionary == null )
            {
                _dictionary = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Create a new instance from an existing dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public RockSerializableDictionary( IDictionary<string, object> dictionary )
        {
            _dictionary = new Dictionary<string, string>();

            foreach (var kvp in dictionary )
            {
                _dictionary.Add( kvp.Key, ( kvp.Value == null ? string.Empty : kvp.Value.ToString() ) );
            }
        }

        /// <summary>
        /// The dictionary.
        /// </summary>
        public Dictionary<string, string> Dictionary
        {
            get
            {
                return _dictionary;
            }
        }

        /// <summary>
        /// Creates a formatted string where the keys and values are Uri-encoded.
        /// </summary>
        /// <returns></returns>
        public string ToUriEncodedString()
        {
            return RockSerializableDictionary.ToUriEncodedString( _dictionary );
        }

        #region Static methods

        /// <summary>
        /// Creates a formatted string representation of the provided dictionary where the keys and values are Uri-encoded.
        /// </summary>
        /// <returns></returns>
        public static string ToUriEncodedString( IDictionary<string, object> dictionary )
        {
            var stringDictionary = new Dictionary<string, string>();

            foreach (var kvp in dictionary )
            {
                stringDictionary.Add( kvp.Key, ( kvp.Value == null ? string.Empty: kvp.Value.ToString() ) );
            }

            return ToUriEncodedString( stringDictionary );
        }

        /// <summary>
        /// Creates a formatted string representation of the provided dictionary where the keys and values are Uri-encoded.
        /// </summary>
        /// <returns></returns>
        public static string ToUriEncodedString( IDictionary<string, string> dictionary )
        {
            var sb = new StringBuilder();

            var isFirstItem = true;

            foreach ( var kvp in dictionary )
            {
                if ( isFirstItem )
                {
                    isFirstItem = false;
                }
                else
                {
                    sb.Append( keyValuePairEntrySeparator );
                }

                // Make sure that any special characters in the key and value strings are encoded, to prevent confusion with the dictionary string delimiters.
                sb.Append( System.Uri.EscapeDataString( kvp.Key ) );
                sb.Append( keyValuePairInternalSeparator );
                sb.Append( System.Uri.EscapeDataString( kvp.Value.ToStringSafe() ) );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a new instance from a Uri-encoded string.
        /// </summary>
        /// <param name="uriEncodedString"></param>
        /// <returns></returns>
        public static RockSerializableDictionary FromUriEncodedString( string uriEncodedString )
        {
            var _dictionary = new Dictionary<string, string>();

            var keyValueStrings = uriEncodedString.Split( new string[] { keyValuePairEntrySeparator }, StringSplitOptions.None ).ToList();

            foreach ( var keyValueString in keyValueStrings )
            {
                var keyValueArray = keyValueString.Split( new string[] { keyValuePairInternalSeparator }, StringSplitOptions.None );

                // Decode the values. Use UnescapeDataString() because HttpUtility.UrlDecode() replaces "+" with " " which is unwanted behavior.
                if ( keyValueArray.Length == 2 )
                {
                    var key = System.Uri.UnescapeDataString( keyValueArray[0] );
                    var value = System.Uri.UnescapeDataString( keyValueArray[1] );

                    _dictionary.AddOrReplace( key, value );
                }
            }

            return new RockSerializableDictionary( _dictionary );
        }

        #endregion

    }
}