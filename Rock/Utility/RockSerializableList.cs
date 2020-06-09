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
    /// A list of string values that can be serialized to a Rock-specific formatted string for portability and storage.
    /// </summary>
    /// <remarks>
    /// The serialization format for the list string is: Value1|Value2|Value3
    /// </remarks>
    public class RockSerializableList
    {
        List<string> _list = new List<string>();

        private const string _keyValuePairEntrySeparator = "|";

        /// <summary>
        /// Create a new instance with an empty list.
        /// </summary>
        public RockSerializableList()
        {
            //
        }

        /// <summary>
        /// Create a new instance from an existing list.
        /// </summary>
        /// <param name="list"></param>
        public RockSerializableList( List<string> list )
        {
            _list = list;

            if ( _list == null )
            {
                _list = new List<string>();
            }
        }

        /// <summary>
        /// The underlying list object.
        /// </summary>
        public List<string> List
        {
            get
            {
                return _list;
            }
        }

        /// <summary>
        /// Creates a formatted string where the keys and values are Uri-encoded.
        /// </summary>
        /// <returns></returns>
        public string ToUriEncodedString()
        {
            return RockSerializableList.ToUriEncodedString( _list );
        }

#region Static methods

        /// <summary>
        /// Creates a formatted string representation of the provided list where the keys and values are Uri-encoded.
        /// </summary>
        /// <returns></returns>
        public static string ToUriEncodedString( List<string> values )
        {
            var sb = new StringBuilder();

            var isFirstItem = true;

            foreach ( var value in values )
            {
                if ( isFirstItem )
                {
                    isFirstItem = false;
                }
                else
                {
                    sb.Append( _keyValuePairEntrySeparator );
                }

                // Make sure that any special characters in the value string is encoded, to prevent confusion with the list string delimiters.
                sb.Append( System.Uri.EscapeDataString( value ) );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a new instance from a Uri-encoded string.
        /// </summary>
        /// <param name="uriEncodedString"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RockSerializableList FromUriEncodedString( string uriEncodedString, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries )
        {
            var valuesList = new List<string>();

            var values = uriEncodedString.Split( new string[] { _keyValuePairEntrySeparator }, options ).ToList();

            foreach ( var value in values )
            {
                // Decode the values. Use UnescapeDataString() here because HttpUtility.UrlDecode() replaces "+" with " " which is unwanted behavior.
                valuesList.Add( System.Uri.UnescapeDataString( value ) );                
            }

            return new RockSerializableList(valuesList);
        }

        #endregion

    }
}