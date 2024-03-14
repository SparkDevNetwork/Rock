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
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using System.Web.Routing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Rock;
using Rock.Data;
using Rock.Net;
using Rock.Rest.Handler;
using Rock.Rest.Utility;
using Rock.Rest.Utility.ValueProviders;
using Rock.Rest.Validation;

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
            config.EnableCors( new Rock.Rest.EnableCorsFromOriginAttribute() );
            config.Filters.Add( new Rock.Rest.Filters.ValidateAttribute() );
            config.Filters.Add( new Rock.Rest.Filters.RockCacheabilityAttribute() );
            config.Services.Replace( typeof( IExceptionLogger ), new RockApiExceptionLogger() );
            config.Services.Replace( typeof( IExceptionHandler ), new RockApiExceptionHandler() );
            config.Services.Replace( typeof( IAssembliesResolver ), new RockAssembliesResolver() );
            config.Services.Replace( typeof( IHttpControllerSelector ), new Handler.RockHttpControllerSelector( config ) );
            config.Services.Replace( typeof( IBodyModelValidator ), new RockBodyModelValidator() );

            ConfigureServiceProvider( config );

            // Configure the API to handle differences between v1 and v2 endpoints.
            config.Services.Replace( typeof( IActionValueBinder ), new RockActionValueBinder() );
            config.Services.Clear( typeof( ValueProviderFactory ) );
            config.Services.Add( typeof( ValueProviderFactory ), new RockQueryStringValueProviderFactory() );
            config.Services.Add( typeof( ValueProviderFactory ), new RockRouteDataValueProviderFactory() );

            config.Formatters.Insert( 0, new Utility.ApiPickerJsonMediaTypeFormatter() );

            // register Swagger and its routes first
            Rock.Rest.Swagger.SwaggerConfig.Register( config );

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

            // finds all [Route] attributes on REST controllers and creates the routes
            config.MapHttpAttributeRoutes();

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
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder( config );

            var entityTypeList = Reflection.FindTypes( typeof( Rock.Data.IEntity ) )
                .Where( a => !a.Value.IsAbstract && ( a.Value.GetCustomAttribute<NotMappedAttribute>() == null ) && ( a.Value.GetCustomAttribute<DataContractAttribute>() != null ) )
                .OrderBy( a => a.Key ).Select( a => a.Value );

            foreach ( var entityType in entityTypeList )
            {
                var entityTypeConfig = builder.AddEntity( entityType );

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

                var entitySetConfig = builder.AddEntitySet( name, entityTypeConfig );
            }

            var defaultConventions = ODataRoutingConventions.CreateDefault();
            // Disable the api/$metadata route
            var conventions = defaultConventions.Except( defaultConventions.OfType<MetadataRoutingConvention>() );

            config.Routes.MapODataServiceRoute( "api", "api", builder.GetEdmModel(), pathHandler: new DefaultODataPathHandler(), routingConventions: conventions );

            new Rock.Transactions.RegisterControllersTransaction().Enqueue();
        }

        /// <summary>
        /// Configures the service provider used during API requests. This really
        /// should be moved to the BeginRequest method as an HTTP handler, but
        /// for now this will work until we unify everything. Just keep it all
        /// private and internal.
        /// </summary>
        /// <param name="config">The HTTP configuration.</param>
        private static void ConfigureServiceProvider( HttpConfiguration config )
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IRockRequestContextAccessor, RockRequestContextAccessor>();
            serviceCollection.AddScoped<RockContext>();
            serviceCollection.AddSingleton<IWebHostEnvironment>( provider => new Rock.Utility.WebHostEnvironment
            {
                WebRootPath = AppDomain.CurrentDomain.BaseDirectory
            } );

            var apiServiceProvider = serviceCollection.BuildServiceProvider();

            // Replace the standard controller activator with one of ours
            // that uses the standard dependency injection patterm used in
            // ASP.Net Core.
            config.Services.Replace( typeof( IHttpControllerActivator ), new RockDependencyControllerActivator() );

            // Add a new message handler that will create scopes for each request.
            config.MessageHandlers.Add( new ServiceScopeHandler( apiServiceProvider ) );
        }
    }
}