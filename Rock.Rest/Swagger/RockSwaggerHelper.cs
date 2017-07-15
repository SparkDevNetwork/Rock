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
using System.Web;
using System.Web.Http.Description;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// 
    /// </summary>
    public static class RockSwaggerHelper
    {
        /// <summary>
        /// Rocks the version support resolver and controller filter.
        /// Filters the Swagger Doc to only include the actions for the specified controller
        /// </summary>
        /// <param name="apiDesc">The API desc.</param>
        /// <param name="targetApiVersion">The target API version.</param>
        /// <returns></returns>
        public static bool RockVersionSupportResolverAndControllerFilter( ApiDescription apiDesc, string targetApiVersion )
        {
            string controllerName = null;
            var requestParams = HttpContext.Current?.Request?.Params;
            if ( requestParams != null )
            {
                controllerName = requestParams["controllerName"];

                if ( apiDesc?.ActionDescriptor?.ControllerDescriptor?.ControllerName == controllerName )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
