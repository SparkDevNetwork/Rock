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
using System.Reflection;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.RestControllerService"/> entity objects.
    /// </summary>
    public partial class RestControllerService
    {
        /// <summary>
        /// Gets the API identifier.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <returns></returns>
        [RockObsolete( "1.14" )]
        [Obsolete( "Use the method with the 'rockGuid' property instead." )]
        public static string GetApiId( MethodInfo methodInfo, string httpMethod, string controllerName )
        {
            return GetApiId( methodInfo, httpMethod, controllerName, out _ );
        }

        /// <summary>
        /// Gets the API identifier.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="restActionGuid">If this RestAction has a well-known guid, returns the Guid associated with this API.</param>
        /// <returns></returns>
        public static string GetApiId( MethodInfo methodInfo, string httpMethod, string controllerName, out Guid? restActionGuid )
        {
            // If the Method has a RestActionGuidAttribute, output what it is.
            // Note that inherited methods won't have a RockGuid since that Method applies to multiple controllers
            restActionGuid = methodInfo.GetCustomAttribute<Rock.SystemGuid.RestActionGuidAttribute>( inherit: false )?.Guid;

            var strippedClassname = Regex.Replace( methodInfo.ToString(), @"((?:(?:\w+)?\.(?<name>[a-z_A-Z]\w+))+)", "${name}" );
            return $"{httpMethod}{controllerName}^{strippedClassname}";
        }

        /// <summary>
        /// Gets the Guid for the RestController that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = RestControllerCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

    }
}
