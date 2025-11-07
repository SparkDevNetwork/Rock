using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.XPath;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rock.Configuration;
using Rock.Data;
using Rock.Web2;
using Rock.Web2.Routing;

namespace RockWebCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public Startup( IConfiguration configuration, IWebHostEnvironment environment )
        {
            Configuration = configuration;
            Environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services.AddAuthentication( CookieAuthenticationDefaults.AuthenticationScheme )
                .AddCookie( options =>
                {
                    // These keys are from the RockWeb web.config machineKey element.
                    // They are hard coded now to be the sample keys from the repo.
                    // In the future this entire .AddCookie() call should be replaced
                    // with a new .AddRockCookie() extension methods that performs
                    // all of this initialization and uses an IPostConfigureOptions<CookieAuthenticationOptions>
                    // class to create the WebFormsCookieDataFormat and pull the keys
                    // from IConfiguration.
                    var webFormsDecryptionHexKey = "08CA6024B2BC3CA7B61165BAF1398D94212976EC00DAC895B23E4BADEE76386B";
                    var webFormsValidationHexKey = "09FC7CE0D57E55DCF18196743CDCDF77E86E8E522D0DDF2AB2663980F987A288B09BE034018539EAF87565AC5CB285B9C827CE0C251B8D1832627B14A7DC7697";

                    options.Cookie.Name = ".ROCK";
                    options.Cookie.Path = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays( 30 );
                    options.SlidingExpiration = true;
                    options.TicketDataFormat = new WebFormsCookieDataFormat( webFormsDecryptionHexKey, webFormsValidationHexKey, options.Cookie.Path, options.ExpireTimeSpan );

                    options.Events.OnRedirectToLogin = context =>
                    {
                        if ( context.Request.Path.StartsWithSegments( "/api" ) )
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect( context.RedirectUri );

                        return Task.CompletedTask;
                    };
                } );

            services.AddControllers()
                .AddNewtonsoftJson( options =>
                {
                    options.SerializerSettings.ContractResolver = new RockContractResolver();
                    options.UseCamelCasing( true );
                } );

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

            services.AddHttpContextAccessor();

            services.AddBundling()
                            .UseDefaults( Environment )
                            .UseNUglify()
                            .AddLess()
                            .UseTimestampVersioning();

            services.AddHostedService<RockStartupService>();

            services.AddSingleton<IConnectionStringProvider, NetCoreConnectionStringProvider>();
            services.AddSingleton<IInitializationSettings, NetCoreInitializationSettings>();
            services.AddSingleton<IHostingSettings, Rock.Configuration.HostingSettings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            RockApp.Current = new RockApp( app.ApplicationServices );

            app.UseAuthentication();

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

            app.UseEndpoints( endpoints =>
            {
                endpoints.MapControllers();
            } );

            app.UseMiddleware<RockRouterMiddleware>( app );

            var cwd = Directory.GetCurrentDirectory();

            app.UseStaticFiles( new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider( System.IO.Path.Join( cwd, "..", "RockWeb" ) )
            } );

            app.UseRockBundles();

            Rock.Lava.LavaService.SetCurrentEngine( new Rock.Lava.Fluid.FluidEngine() );
        }
    }

    internal class NetCoreConnectionStringProvider : IConnectionStringProvider
    {
        private readonly string _connectionString;
        private readonly string _readOnlyConnectionString;
        private readonly string _analyticsConnectionString;

        public NetCoreConnectionStringProvider( IConfiguration configuration )
        {
            _connectionString = configuration.GetConnectionString( "RockContext" );
            _readOnlyConnectionString = configuration.GetConnectionString( "ReadOnly" ) ?? _connectionString;
            _analyticsConnectionString = configuration.GetConnectionString( "Analytics" ) ?? _readOnlyConnectionString;
        }

        /// <inheritdoc/>
        public string ConnectionString => _connectionString;

        /// <inheritdoc/>
        public string ReadOnlyConnectionString => _readOnlyConnectionString;

        /// <inheritdoc/>
        public string AnalyticsConnectionString => _analyticsConnectionString;
    }

    /// <summary>
    /// The Net Core implementation of <see cref="InitializationSettings"/>.
    /// </summary>
    internal class NetCoreInitializationSettings : InitializationSettings
    {
        /// <summary>
        /// Creates a new <see cref="NetCoreInitializationSettings"/> instance
        /// and loads all the settings from the web.config file.
        /// </summary>
        public NetCoreInitializationSettings( IConnectionStringProvider connectionStringProvider )
            : base( connectionStringProvider )
        {
            //IsRunScheduledJobsEnabled = false;
            //OrganizationTimeZone = string.Empty;
            PasswordKey = "D42E08ECDE448643C528C899F90BADC9411AE07F74F9BA00A81BA06FD17E3D6BA22C4AE6947DD9686A35E8538D72B471F14CDB31BD50B9F5B2A1C26E290E5FC2";
            DataEncryptionKey = "v8Hw27G0dXAhjo9HCzi5MFMwJZznhunhivaillaPjpzc3czzTBkkCz+PzaRyCq61Rsvn7oq2G5zMHoixGT0lvc2uuuoHRIduOZJ0uxTGLa48ZWfJ2iBY8lSgwFmRgYEEOSZZQyA0nGSwJlgNicJRCfArC8IikRMKRWQCzenaLjA=";
            //RockStoreUrl = GetValue( "RockStoreUrl" )?.ToStringSafe();
            //IsDuplicateGroupMemberRoleAllowed = GetValue( "AllowDuplicateGroupMembers" )?.AsBoolean() ?? false;
            //IsCacheStatisticsEnabled = GetValue( "CacheManagerEnableStatistics" )?.AsBoolean() ?? false;
            //ObservabilityServiceName = GetValue( "ObservabilityServiceName" )?.ToStringSafe();
            //AzureSignalREndpoint = GetValue( "AzureSignalREndpoint" )?.ToStringSafe();
            //AzureSignalRAccessKey = GetValue( "AzureSignalRAccessKey" )?.ToStringSafe();
            //SparkApiUrl = GetValue( "SparkApiUrl" )?.ToStringSafe();
            //NodeName = GetValue( "NodeName" )?.ToStringSafe();

            // Load old password keys.
            //var oldPasswordKeys = new List<string>();
            //for ( int i = 0; ; i++ )
            //{
            //    var passwordKey = GetValue( $"OldPasswordKey{i}" );

            //    if ( passwordKey.IsNullOrWhiteSpace() )
            //    {
            //        break;
            //    }

            //    oldPasswordKeys.Add( passwordKey );
            //}

            // Load old decryption keys.
            //var oldDataEncryptionKeys = new List<string>();
            //for ( int i = 0; ; i++ )
            //{
            //    var dataEncryptionKey = GetValue( $"OldDataEncryptionKey{i}" );

            //    if ( dataEncryptionKey.IsNullOrWhiteSpace() )
            //    {
            //        break;
            //    }

            //    oldDataEncryptionKeys.Add( dataEncryptionKey );
            //}

            //OldPasswordKeys = oldPasswordKeys;
            //OldDataEncryptionKeys = oldDataEncryptionKeys;
        }

        /// <inheritdoc/>
        public override void Save()
        {
            throw new NotSupportedException();
        }
    }
}
