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
using System.Web;

namespace Rock.Web.Utilities
{
    /// <summary>
    /// A helper class for debugging ASP.Net Web Forms.
    /// </summary>
    public class WebDebugHelper
    {
        /// <summary>
        /// Gets a readable string representation of ViewState from the specified HttpRequest.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetDecodedViewState( HttpRequest request )
        {
            return GetDecodedViewState( request.Form["__VIEWSTATE"] );
        }

        /// <summary>
        /// Gets a readable string representation of ViewState from the encoded ViewState string.
        /// </summary>
        /// <param name="viewStateString"></param>
        /// <returns></returns>
        public static string GetDecodedViewState( string viewStateString )
        {


            if ( string.IsNullOrWhiteSpace( viewStateString ) )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var encoding = new System.Text.UTF8Encoding();
            var strViewStateDecoded = encoding.GetString( Convert.FromBase64String( viewStateString ) );
            var astrDecoded = strViewStateDecoded.Replace( "<", "<\n" )
                .Replace( ">", "\n>" )
                .Replace( ";", ";\n" )
                .Split( '\n' );

            var indentLevel = 0;
            foreach ( string str in astrDecoded )
            {
                if ( str.Length == 0 )
                {
                    continue;
                }

                if ( str.EndsWith( @"\<" ) )
                {
                    sb.Append( GetFormattedOutput( str, indentLevel ) );
                }
                else if ( str.EndsWith( @"\" ) )
                {
                    sb.Append( GetFormattedOutput( str, indentLevel ) );
                }
                else if ( str.EndsWith( "<" ) )
                {
                    sb.AppendLine( GetFormattedOutput( str, indentLevel ) );
                    indentLevel++;
                }
                else if ( str.StartsWith( ">;" ) || str.StartsWith( ">" ) )
                {
                    indentLevel--;
                    sb.AppendLine( GetFormattedOutput( str, indentLevel ) );
                }
                else if ( str.EndsWith( @"\;" ) )
                {
                    sb.Append( GetFormattedOutput( str, indentLevel ) );
                }
                else
                {
                    sb.AppendLine( GetFormattedOutput( str, indentLevel ) );
                }
            }

            return sb.ToString();
        }

        private static string GetFormattedOutput( string str, int indentLevel )
        {
            const int indentSize = 4;

            return new string( ' ', indentLevel * indentSize ) + str;
        }
    }
}