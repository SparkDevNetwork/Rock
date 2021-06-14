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
using System.Text;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using System.Dynamic;

namespace Rock.Rest
{
    public static class IRouteBuilderExtensions
    {
        public static void MapHttpRoute( this IRouteBuilder routeBuilder, string name, string routeTemplate )
        {
            routeBuilder.MapRoute( name, routeTemplate );
        }

        public static void MapHttpRoute( this IRouteBuilder routeBuilder, string name, string routeTemplate, object defaults )
        {
            MapHttpRoute( routeBuilder, name, routeTemplate, defaults, null );
        }

        public static void MapHttpRoute( this IRouteBuilder routeBuilder, string name, string routeTemplate, object defaults, object constraints )
        {
            if ( defaults == null )
            {
                routeBuilder.MapRoute( name, routeTemplate, defaults, constraints );
                return;
            }

            var expando = new ExpandoObject();
            var expandoDict = ( IDictionary<string, object> ) expando;
            string template = routeTemplate;

            foreach ( var pi in defaults.GetType().GetProperties() )
            {
                if ( pi.CanRead )
                {
                    var value = pi.GetValue( defaults ) as string;

                    if ( value == System.Web.Http.RouteParameter.Optional )
                    {
                        template = template.Replace( $"{{{ pi.Name }}}", $"{{{ pi.Name }?}}" );
                    }
                    else
                    {
                        expandoDict.Add( pi.Name, value );
                    }
                }
            }

            routeBuilder.MapRoute( name, template, expando, constraints );
        }
    }
}
