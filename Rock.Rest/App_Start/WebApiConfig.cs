using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Routing;

using Rock;

namespace Rock.Rest
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Maps ODataService Route and registers routes for any controller actions that use a [Route] attribute
        /// </summary>
        /// <param name="config">The configuration.</param>
        public static void Register( HttpConfiguration config )
        {
            // Add API route for dataviews
            config.Routes.MapHttpRoute(
                name: "DataViewApi",
                routeTemplate: "api/{controller}/DataView/{id}",
                defaults: new
                {
                    action = "DataView"
                }
            );

            // finds all [Route] attributes on REST controllers and creates the routes 
            config.MapHttpAttributeRoutes();

            // Add any custom api routes
            foreach ( var type in Rock.Reflection.FindTypes(
                typeof( Rock.Rest.IHasCustomRoutes ) ) )
            {
                var controller = (Rock.Rest.IHasCustomRoutes)Activator.CreateInstance( type.Value );
                if ( controller != null )
                {
                    controller.AddRoutes( RouteTable.Routes );
                }
            }

            //// Add Default API Service routes
            //// Instead of being able to use one default route that gets action from http method, have to 
            //// have a default route for each method so that other actions do not match the default (i.e. DataViews).
            //// Also, this will make controller routes case-insensitive (vs the odata routing)
            config.Routes.MapHttpRoute(
                name: "DefaultApiGetById",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "GetById"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiGetFunction",
                routeTemplate: "api/{controller}({key})",
                defaults: new
                {
                    action = "GET"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiGetList",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    action = "GET"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                }
            );

            config.Routes.MapHttpRoute(
               name: "DefaultApiPut",
               routeTemplate: "api/{controller}/{id}",
               defaults: new
               {
                   action = "PUT",
                   id = System.Web.Http.RouteParameter.Optional
               },
               constraints: new
               {
                   httpMethod = new HttpMethodConstraint( new string[] { "PUT" } ),
                   controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
               }
           );

            config.Routes.MapHttpRoute(
                name: "DefaultApiPost",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "POST",
                    id = System.Web.Http.RouteParameter.Optional,
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "POST" } )
                }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApiDelete",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "DELETE",
                    id = System.Web.Http.RouteParameter.Optional
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "DELETE" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                }
            );

            // build OData model and create service route (mainly for metadata)
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IEntity ) )
                .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) )
                .OrderBy( a => a.Key ).Select( a => a.Value );

            foreach ( var entityType in entityTypeList )
            {
                var entityTypeConfig = builder.AddEntity( entityType );
                var entitySetConfig = builder.AddEntitySet( entityType.Name.Pluralize(), entityTypeConfig );
            }

            config.Routes.MapODataServiceRoute( "api", "api", builder.GetEdmModel() );
        }
    }
}
