using System.IO;
using System.Xml.XPath;

using GraphQL.Server.Ui.Voyager;

using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rock.Data;
using Rock.Rest;

namespace RockWebCore
{
    public class Subtype : ObjectType
    {
        protected override void Configure( IObjectTypeDescriptor descriptor )
        {
            descriptor.Name( "Models" );
            descriptor.Include<RockRestQueryRoot>();
        }
    }

    public class QueryType : ObjectType
    {
        protected override void Configure( IObjectTypeDescriptor descriptor )
        {
            descriptor.Name( "Root" );
            descriptor.Field( "models" ).Resolver( ctx => new Subtype() );
            descriptor.Include<TestQueryRoot>();
        }
    }

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers()
                .AddNewtonsoftJson( options =>
                {
                    options.SerializerSettings.ContractResolver = new RockContractResolver();
                    options.UseCamelCasing( true );
                } );

            services.AddRockApi();

            services.AddDbContext<RockContext>( ServiceLifetime.Scoped, ServiceLifetime.Singleton );
            services.AddDbContextFactory<RockContext>();

            services.AddSwaggerGen( c =>
            {
                c.SwaggerDoc( "v2", new OpenApiInfo { Title = "Rock.Rest", Version = "v2" } );

                var xmlPath = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
                var xmlFiles = Directory.GetFiles( xmlPath, "*.xml" );
                foreach ( var xmlFile in xmlFiles )
                {
                    c.IncludeXmlComments( xmlFile );

                    var xdoc = new XPathDocument( xmlFile );
                    c.SchemaFilter<EnumTypesSchemaFilter>( xdoc );
                }
            } );
            services.AddSwaggerGenNewtonsoftSupport();

            services.AddGraphQLServer()
                .AddQueryType<QueryType>()
                .ConfigureSchemaServices( s =>
                {
                    s.AddSingleton<ITypeInspector, RockTypeInspector>();
                } );

            services.AddSingleton<Rock.Rest.IEntityServiceFactory, EntityServiceFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRockRestApi();

            app.UseSwagger();
            app.UseSwaggerUI( c =>
            {
                c.DisplayRequestDuration();
                c.EnableDeepLinking();

                c.SwaggerEndpoint( "/swagger/v2/swagger.json", "Rock.Rest v2" );
            } );

            app.UseReDoc( c =>
            {
                c.DocumentTitle = "Rock Rest API Documentation";
                c.SpecUrl = "/swagger/v2/swagger.json";
            } );

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGraphQL();
            } );

            app.UseGraphQLVoyager( new VoyagerOptions()
            {
                GraphQLEndPoint = "/graphql"
            }, "/graphql-voyager" );
        }
    }
}
