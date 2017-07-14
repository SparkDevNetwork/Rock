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
