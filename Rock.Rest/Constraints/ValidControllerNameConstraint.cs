using System.Collections.Generic;
using System.Web.Http.Routing;

namespace Rock.Rest.Constraints
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidControllerNameConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Determines whether this instance equals a specified route.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        public bool Match( System.Net.Http.HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection )
        {
            if (values.ContainsKey("controller"))
            {
                string controllerName = ( values["controller"] as string );
                if (controllerName.Length > 0)
                {
                    // make sure the Controller parameter starts with an Alpha character (mainly so that api/$metadata will routed correctly)
                    return char.IsLetter( controllerName[0] );
                }
            }

            return true;
        }
    }
}
