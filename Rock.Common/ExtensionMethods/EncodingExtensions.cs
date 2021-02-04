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
using System.Text;

namespace Rock.Common
{
    /// <summary>
    /// Encoding Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Encoding Extensions

        /// <summary>
        /// Turns a string into a properly XML Encoded string.
        /// </summary>
        /// <param name="str">Plain text to convert to XML Encoded string</param>
        /// <param name="isAttribute">If <c>true</c> then additional encoding is done to ensure proper use in an XML attribute value.</param>
        /// <returns>XML encoded string</returns>
        public static string EncodeXml( this string str, bool isAttribute = false )
        {
            var sb = new StringBuilder( str.Length );

            foreach ( var chr in str )
            {
                if ( chr == '<' )
                {
                    sb.Append( "&lt;" );
                }
                else if ( chr == '>' )
                {
                    sb.Append( "&gt;" );
                }
                else if ( chr == '&' )
                {
                    sb.Append( "&amp;" );
                }
                else if ( isAttribute && chr == '\"' )
                {
                    sb.Append( "&quot;" );
                }
                else if ( isAttribute && chr == '\'' )
                {
                    sb.Append( "&apos;" );
                }
                else if ( chr == '\n' )
                {
                    sb.Append( isAttribute ? "&#xA;" : "\n" );
                }
                else if ( chr == '\r' )
                {
                    sb.Append( isAttribute ? "&#xD;" : "\r" );
                }
                else if ( chr == '\t' )
                {
                    sb.Append( isAttribute ? "&#x9;" : "\t" );
                }
                else
                {
                    sb.Append( chr );
                }
            }

            return sb.ToString();
        }

        #endregion Encoding Extensions
    }
}
