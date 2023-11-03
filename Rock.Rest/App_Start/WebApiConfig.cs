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
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
#if REVIEW_NET5_0_OR_GREATER
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
#else
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.ValueProviders;
using System.Web.Routing;
#endif

using Rock;
using Rock.Rest.Utility;
using Rock.Rest.Utility.ValueProviders;
using Rock.Tasks;

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
#if REVIEW_NET5_0_OR_GREATER
        public class HttpConfiguration
        {
            public Microsoft.AspNetCore.Routing.IRouteBuilder Routes { get; set; }
        }

        public static void UseRockApi( this IApplicationBuilder app )
        {
            app.UseMvc( routeBuilder =>
            {
                var config = new HttpConfiguration
                {
                    Routes = routeBuilder
                };

                Register( config );
            } );
        }
#endif

        public static void Register( HttpConfiguration config )
        {
#if REVIEW_NET5_0_OR_GREATER
#else
            config.EnableCors( new Rock.Rest.EnableCorsFromOriginAttribute() );
            config.Filters.Add( new Rock.Rest.Filters.ValidateAttribute() );
            config.Filters.Add( new Rock.Rest.Filters.RockCacheabilityAttribute() );
            config.Services.Replace( typeof( IExceptionLogger ), new RockApiExceptionLogger() );
            config.Services.Replace( typeof( IExceptionHandler ), new RockApiExceptionHandler() );
            config.Services.Replace( typeof( IAssembliesResolver ), new RockAssembliesResolver() );
            config.Services.Replace( typeof( IHttpControllerSelector ), new Handler.RockHttpControllerSelector( config ) );

            // Configure the API to handle differences between v1 and v2 endpoints.
            config.Services.Replace( typeof( IActionValueBinder ), new RockActionValueBinder() );
            config.Services.Clear( typeof( ValueProviderFactory ) );
            config.Services.Add( typeof( ValueProviderFactory ), new RockQueryStringValueProviderFactory() );
            config.Services.Add( typeof( ValueProviderFactory ), new RockRouteDataValueProviderFactory() );

            config.Formatters.Insert( 0, new Utility.ApiPickerJsonMediaTypeFormatter() );

            // register Swagger and its routes first
            Rock.Rest.Swagger.SwaggerConfig.Register( config );
#endif

            // Add API route for dataviews
            config.Routes.MapHttpRoute(
                name: "DataViewApi",
                routeTemplate: "api/{controller}/DataView/{id}",
                defaults: new
                {
                    action = "DataView"
                } );

            config.Routes.MapHttpRoute(
               name: "FollowedItemsApi",
               routeTemplate: "api/{controller}/FollowedItems",
               defaults: new
               {
                   action = "FollowedItems"
               } );

            config.Routes.MapHttpRoute(
                name: "InDataViewApi",
                routeTemplate: "api/{controller}/InDataView/{dataViewId}/{entityId}",
                defaults: new
                {
                    action = "InDataView"
                } );

            // Add API route for Launching a Workflow
            config.Routes.MapHttpRoute(
                name: "LaunchWorkflowApi",
                routeTemplate: "api/{controller}/LaunchWorkflow/{id}",
                defaults: new
                {
                    action = "LaunchWorkflow"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "POST" } ),
                } );

            // Add API route for DeleteAttributeValue
            config.Routes.MapHttpRoute(
                name: "DeleteAttributeValueApi",
                routeTemplate: "api/{controller}/AttributeValue/{id}",
                defaults: new
                {
                    action = "DeleteAttributeValue"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "DELETE" } ),
                } );

            // Add API route for SetAttributeValue
            config.Routes.MapHttpRoute(
                name: "SetAttributeValueApi",
                routeTemplate: "api/{controller}/AttributeValue/{id}",
                defaults: new
                {
                    action = "SetAttributeValue"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "POST" } ),
                } );

            // Add API route for setting context
            config.Routes.MapHttpRoute(
                name: "SetContextApi",
                routeTemplate: "api/{controller}/SetContext/{id}",
                defaults: new
                {
                    action = "SetContext"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "PUT", "OPTIONS" } ),
                } );

            // Add any custom HTTP API routes. Do this before the attribute route mapping to allow
            // derived classes to override the parent class route attributes.
            foreach ( var type in Reflection.FindTypes( typeof( IHasCustomHttpRoutes ) ) )
            {
                try
                {
                    var controller = Activator.CreateInstance( type.Value ) as IHasCustomHttpRoutes;
                    if ( controller != null )
                    {
                        controller.AddRoutes( config.Routes );
                    }
                }
                catch
                {
                    // ignore, and skip adding routes if the controller raises an exception
                }
            }

#if REVIEW_NET5_0_OR_GREATER
#else
            // finds all [Route] attributes on REST controllers and creates the routes
            config.MapHttpAttributeRoutes();
#endif

            //// Add Default API Service routes
            //// Instead of being able to use one default route that gets action from http method, have to
            //// have a default route for each method so that other actions do not match the default (i.e. DataViews).
            //// Also, this will make controller routes case-insensitive (vs the odata routing)
            config.Routes.MapHttpRoute(
                name: "DefaultApiGetByAttributeValue",
                routeTemplate: "api/{controller}/GetByAttributeValue",
                defaults: new
                {
                    action = "GetByAttributeValue"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET", "OPTIONS" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                } );

            // Add GetByCampus API methods for controllers of types that implement ICampusFilterable
            foreach ( var type in Reflection.FindTypes( typeof( Rock.Data.ICampusFilterable ) ) )
            {
                try
                {
                    Type typeValue = ( Type ) type.Value;
                    string pluralizedName = typeValue.Name.Pluralize();

                    config.Routes.MapHttpRoute(
                    name: $"Api{pluralizedName}GetByCampus",
                    routeTemplate: $"api/{pluralizedName}/GetByCampus",
                    defaults: new
                    {
                        action = "GetByCampus",
                        controller = pluralizedName
                    },
                    constraints: new
                    {
                        httpMethod = new HttpMethodConstraint( new string[] { "GET", "OPTIONS" } ),
                        controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                    } );
                }
                catch
                {
                    // ignore, and skip adding routes if the controller raises an exception
                }
            }

            config.Routes.MapHttpRoute(
                name: "DefaultApiGetById",
                routeTemplate: "api/{controller}/{id}",
                defaults: new
                {
                    action = "GetById"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET", "OPTIONS" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                } );

            config.Routes.MapHttpRoute(
                name: "DefaultApiGetFunction",
                routeTemplate: "api/{controller}({key})",
                defaults: new
                {
                    action = "GET"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET", "OPTIONS" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                } );

            config.Routes.MapHttpRoute(
                name: "DefaultApiGetList",
                routeTemplate: "api/{controller}",
                defaults: new
                {
                    action = "GET"
                },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint( new string[] { "GET", "OPTIONS" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                } );

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
                   httpMethod = new HttpMethodConstraint( new string[] { "PUT", "OPTIONS" } ),
                   controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
               } );

            config.Routes.MapHttpRoute(
               name: "DefaultApiPatch",
               routeTemplate: "api/{controller}/{id}",
               defaults: new
               {
                   action = "PATCH",
                   id = System.Web.Http.RouteParameter.Optional
               },
               constraints: new
               {
                   httpMethod = new HttpMethodConstraint( new string[] { "PATCH", "OPTIONS" } ),
                   controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
               } );

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
                    httpMethod = new HttpMethodConstraint( new string[] { "POST", "OPTIONS" } )
                } );

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
                    httpMethod = new HttpMethodConstraint( new string[] { "DELETE", "OPTIONS" } ),
                    controllerName = new Rock.Rest.Constraints.ValidControllerNameConstraint()
                } );

            // build OData model and create service route (mainly for metadata)
#if REVIEW_NET5_0_OR_GREATER
            var builder = new ODataConventionModelBuilder();
#else
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder( config );
#endif

            var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IEntity ) )
                .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) && ( a.Value.GetCustomAttribute<DataContractAttribute>() != null ) )
                .OrderBy( a => a.Key ).Select( a => a.Value );

            foreach ( var entityType in entityTypeList )
            {
#if REVIEW_NET5_0_OR_GREATER
                var entityTypeConfig = builder.AddEntityType( entityType );
#else
                var entityTypeConfig = builder.AddEntity( entityType );
#endif

                var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
                string name;
                if ( tableAttribute != null )
                {
                    name = tableAttribute.Name.Pluralize();
                }
                else
                {
                    name = entityType.Name.Pluralize();
                }

#if REVIEW_NET5_0_OR_GREATER
                foreach ( var ignoredProperties in entityType.GetCustomAttributes<Rock.Data.IgnorePropertiesAttribute>( true ) )
                {
                    foreach ( var propertyName in ignoredProperties.Properties )
                    {
                        var pi = entityType.GetProperty( propertyName );
                        if ( pi != null )
                        {
                            entityTypeConfig.RemoveProperty( pi );
                        }
                    }
                }
#endif

                var entitySetConfig = builder.AddEntitySet( name, entityTypeConfig );
            }

#if REVIEW_NET5_0_OR_GREATER
            config.Routes.Count().Filter().OrderBy().Expand().Select().MaxTop( null );
#endif

            var defaultConventions = ODataRoutingConventions.CreateDefault();
            // Disable the api/$metadata route
            var conventions = defaultConventions.Except( defaultConventions.OfType<MetadataRoutingConvention>() );

            config.Routes.MapODataServiceRoute( "api", "api", builder.GetEdmModel(), pathHandler: new DefaultODataPathHandler(), routingConventions: conventions );


            new Rock.Transactions.RegisterControllersTransaction().Enqueue();
        }
    }
}