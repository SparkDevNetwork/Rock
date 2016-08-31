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
using System.ComponentModel.DataAnnotations;
using System.Web.Routing;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false )]
    sealed public class RouteAttribute : ValidationAttribute
    {
        /// <summary>
        /// Determines whether the specified value of the object is valid.
        /// </summary>
        /// <param name="value">The value of the object to validate.</param>
        /// <returns>
        /// true if the specified value is valid; otherwise, false.
        /// </returns>
        public override bool IsValid( object value )
        {
            try
            {
                string routeText = ( value as string );
                Route testRoute = new Route( routeText, null );
                return true;
            }
            catch ( Exception ex )
            {
                string[] exceptionLines = ex.Message.Split( new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries );
                
                // just get first line of message, and escape any braces that might be in it
                ErrorMessage = exceptionLines[0].Replace( "{", "&#123;" ).Replace( "}", "&#125;" );
                return false;
            }
        }
    }
}