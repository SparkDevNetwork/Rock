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
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using Rock;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Configuration;
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

            Rock.WebStartup.RockApplicationStartupHelper.ConfigureServices( services );
            services.AddSingleton<IConnectionStringProvider, NetCoreConnectionStringProvider>();
            services.AddSingleton<IInitializationSettings, NetCoreInitializationSettings>();
            services.AddSingleton<IHostingSettings, NetCoreHostingSettings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IWebHostEnvironment env )
        {
            if ( env.IsDevelopment() )
            {
                app.UseDeveloperExceptionPage();
            }

            RockApp.Current = new RockApp( app.ApplicationServices );

            // This must run after RockApp.Current is set and before any
            // requests are handled, same as the WebForms AssemblyInitializer.
            Rock.WebStartup.RockApplicationStartupHelper.RunApplicationStartup();

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

            // The web root is set to the RockWeb folder in Program.cs.
            app.UseStaticFiles();

            app.UseRockBundles();
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

            if ( string.IsNullOrWhiteSpace( _connectionString ) )
            {
                throw new InvalidOperationException( "The RockContext connection string is not configured. Copy appsettings.Local.json.example to appsettings.Local.json and set ConnectionStrings:RockContext." );
            }

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
        /// The configuration section that holds the Rock settings. The keys
        /// match the WebForms web.config appSettings keys, except where noted.
        /// </summary>
        private const string SectionName = "Rock";

        /// <summary>
        /// Creates a new <see cref="NetCoreInitializationSettings"/> instance
        /// and loads all the settings from the "Rock" configuration section.
        /// </summary>
        public NetCoreInitializationSettings( IConnectionStringProvider connectionStringProvider, IConfiguration configuration )
            : base( connectionStringProvider )
        {
            var section = configuration.GetSection( SectionName );
            Func<string, string> GetValue = key => section[key];

            // WebForms names this "RunJobsInIISContext", which does not apply
            // outside of IIS.
            IsRunScheduledJobsEnabled = GetValue( "RunJobs" )?.AsBoolean() ?? false;
            OrganizationTimeZone = GetValue( "OrgTimeZone" )?.ToStringSafe();
            PasswordKey = GetValue( "PasswordKey" )?.ToStringSafe();
            DataEncryptionKey = GetValue( "DataEncryptionKey" )?.ToStringSafe();
            RockStoreUrl = GetValue( "RockStoreUrl" )?.ToStringSafe();
            IsDuplicateGroupMemberRoleAllowed = GetValue( "AllowDuplicateGroupMembers" )?.AsBoolean() ?? false;
            IsCacheStatisticsEnabled = GetValue( "CacheManagerEnableStatistics" )?.AsBoolean() ?? false;
            ObservabilityServiceName = GetValue( "ObservabilityServiceName" )?.ToStringSafe();
            AzureSignalREndpoint = GetValue( "AzureSignalREndpoint" )?.ToStringSafe();
            AzureSignalRAccessKey = GetValue( "AzureSignalRAccessKey" )?.ToStringSafe();
            SparkApiUrl = GetValue( "SparkApiUrl" )?.ToStringSafe();
            NodeName = GetValue( "NodeName" )?.ToStringSafe();
            DeploymentEnvironment = GetValue( "DeploymentEnvironment" )?.ConvertToEnumOrNull<DeploymentEnvironment>() ?? DeploymentEnvironment.Production;

            OldPasswordKeys = GetNumberedValues( GetValue, "OldPasswordKey" );
            OldDataEncryptionKeys = GetNumberedValues( GetValue, "OldDataEncryptionKey" );
        }

        /// <inheritdoc/>
        public override void Save()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// The Net Core implementation of <see cref="IHostingSettings"/>.
    /// </summary>
    /// <remarks>
    /// This exists separately from <c>Rock.Configuration.HostingSettings</c>
    /// because the Rock assembly cannot reference <see cref="IWebHostEnvironment"/>
    /// on .NET Core. The two can be merged if <c>HostingSettings</c> is moved
    /// to an assembly where the interface is available (for example, a future
    /// Rock.Web assembly that contains general web related code for both
    /// WebForms and .NET Core).
    /// </remarks>
    internal class NetCoreHostingSettings : IHostingSettings
    {
        /// <inheritdoc/>
        public DateTime ApplicationStartDateTime { get; }

        /// <inheritdoc/>
        public string DotNetVersion => Environment.Version.ToString();

        /// <inheritdoc/>
        public string WebRootPath { get; }

        /// <inheritdoc/>
        public string VirtualRootPath { get; }

        /// <inheritdoc/>
        public string MachineName { get; }

        /// <inheritdoc/>
        public string NodeName { get; }

        /// <inheritdoc/>
        public bool IsDevelopmentEnvironment { get; }

        /// <summary>
        /// Creates a new <see cref="NetCoreHostingSettings"/> instance
        /// from the ASP.NET Core hosting environment.
        /// </summary>
        /// <param name="initializationSettings">The initialization settings that provide the node name.</param>
        /// <param name="webHostEnvironment">The ASP.NET Core hosting environment.</param>
        public NetCoreHostingSettings( IInitializationSettings initializationSettings, IWebHostEnvironment webHostEnvironment )
        {
            ApplicationStartDateTime = Rock.RockDateTime.Now;
            MachineName = Environment.MachineName;

            // WebRootPath can contain ".." segments from UseWebRoot(), and
            // IHostingSettings promises a trailing slash.
            WebRootPath = Path.GetFullPath( webHostEnvironment.WebRootPath ?? webHostEnvironment.ContentRootPath );

            if ( !WebRootPath.EndsWith( Path.DirectorySeparatorChar ) )
            {
                WebRootPath += Path.DirectorySeparatorChar;
            }

            // Hosting under a path base (virtual directory) is not supported yet.
            VirtualRootPath = "/";

            IsDevelopmentEnvironment = webHostEnvironment.IsDevelopment();

            NodeName = initializationSettings.NodeName;

            if ( string.IsNullOrWhiteSpace( NodeName ) )
            {
                NodeName = MachineName;
            }

            if ( string.IsNullOrWhiteSpace( NodeName ) )
            {
                NodeName = Guid.NewGuid().ToString();
            }
        }
    }
}
