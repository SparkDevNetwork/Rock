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
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Logging;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Web.Cache.Entities;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Factory class for constructing and configuring chat agents within the Rock AI framework.
    /// Handles agent configuration loading, dependency injection, kernel creation,
    /// and skill/plugin registration for semantic and native AI agent capabilities.
    /// </summary>
    internal class ChatAgentFactory
    {
        #region Fields

        /// <summary>
        /// The service provider used to resolve dependencies for the agent.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The configuration for the agent being built. This contains the agent ID, provider, and any additional settings.
        /// </summary>
        private readonly AgentConfiguration _agentConfiguration;

        /// <summary>
        /// The kernel builder used to create the kernel for the agent. This is where the services and plugins are registered.
        /// </summary>
        private readonly IKernelBuilder _kernelBuilder;

        /// <summary>
        /// The request context accessor that provides access to the current request context.
        /// </summary>
        private readonly IRockRequestContextAccessor _requestContextAccessor;

        /// <summary>
        /// The logger factory used to create loggers for the agent and its components.
        /// </summary>
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// The Rock context factory used to create Rock contexts for database operations.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        /// <summary>
        /// The options for configuring the chat agent, such as debug settings and other behaviors.
        /// </summary>
        private readonly ChatAgentOptions _options;

        /// <summary>
        /// The logger used for logging messages related to the chat agent factory.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentFactory"/> class for internal use,
        /// primarily used by other constructors to handle shared setup logic.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve services.</param>
        /// <param name="requestContextAccessor">Provides access to the current Rock request context.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="rockContextFactory">Factory to create Rock database contexts.</param>
        /// <param name="options">Options for configuring the chat agent, such as debug settings.</param>
        private ChatAgentFactory( IServiceProvider serviceProvider, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory, ChatAgentOptions options )
        {
            _serviceProvider = serviceProvider; ;
            _requestContextAccessor = requestContextAccessor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChatAgentFactory>();
            _rockContextFactory = rockContextFactory;
            _options = options ?? throw new ArgumentNullException( nameof( options ) );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentFactory"/> class using an agent ID
        /// and loads its configuration from the database.
        /// </summary>
        /// <param name="agentId">The ID of the agent to load.</param>
        /// <param name="serviceProvider">The service provider used to resolve services.</param>
        /// <param name="rockContext">The Rock database context for data access.</param>
        /// <param name="requestContextAccessor">Provides access to the current Rock request context.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="rockContextFactory">Factory to create Rock database contexts.</param>
        /// <param name="options">Options for configuring the chat agent, such as debug settings.</param>
        public ChatAgentFactory( int agentId, IServiceProvider serviceProvider, RockContext rockContext, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory, ChatAgentOptions options )
            : this( serviceProvider, requestContextAccessor, loggerFactory, rockContextFactory, options )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var provider = AgentProviderContainer.GetActiveComponent();
            _kernelBuilder = CreateKernelBuilder( provider, null );

            var agent = AIAgentCache.Get( agentId, rockContext );

            _agentConfiguration = new AgentConfiguration( agent, GetSkillConfigurations( agentId, rockContext ), provider );
            sw.Stop();

            _logger.LogInformation( "Initialized factory in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatAgentFactory"/> class with a preconfigured agent
        /// and provider, typically used in testing or custom configurations.
        /// </summary>
        /// <param name="provider">The agent provider component used to register chat services.</param>
        /// <param name="agentConfiguration">The pre-loaded agent configuration.</param>
        /// <param name="serviceProvider">The service provider used to resolve services.</param>
        /// <param name="requestContextAccessor">Provides access to the current Rock request context.</param>
        /// <param name="loggerFactory">The logger factory used to create loggers.</param>
        /// <param name="rockContextFactory">Factory to create Rock database contexts.</param>
        /// <param name="configureServices">Optional callback to configure additional kernel services.</param>
        /// <param name="options">Options for configuring the chat agent, such as debug settings.</param>
        internal ChatAgentFactory( AgentProviderComponent provider, AgentConfiguration agentConfiguration, IServiceProvider serviceProvider, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory, Action<IServiceCollection> configureServices, ChatAgentOptions options )
            : this( serviceProvider, requestContextAccessor, loggerFactory, rockContextFactory, options )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _kernelBuilder = CreateKernelBuilder( provider, configureServices );
            _agentConfiguration = agentConfiguration;

            sw.Stop();

            _logger.LogInformation( "Initialized factory in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates and configures an <see cref="IKernelBuilder"/> using the specified agent provider and optional service configuration.
        /// </summary>
        /// <param name="provider">The agent provider used to add chat completion services to the kernel.</param>
        /// <param name="configureServices">An optional delegate to further configure kernel services.</param>
        /// <returns>An initialized kernel builder instance.</returns>
        private IKernelBuilder CreateKernelBuilder( AgentProviderComponent provider, Action<IServiceCollection> configureServices )
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton<AgentRequestContext>();
            kernelBuilder.Services.AddSingleton( _loggerFactory );
            kernelBuilder.Services.AddSingleton( typeof( ILogger<> ), typeof( Logger<> ) );

            configureServices?.Invoke( kernelBuilder.Services );

            foreach ( ModelServiceRole role in Enum.GetValues( typeof( ModelServiceRole ) ) )
            {
                provider.AddChatCompletion( role, kernelBuilder.Services );
            }

            return kernelBuilder;
        }

        /// <summary>
        /// Builds and returns an <see cref="IChatAgent"/> instance using the configured kernel and agent configuration.
        /// </summary>
        /// <returns>A constructed chat agent instance.</returns>
        public IChatAgent Build()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var kernel = _kernelBuilder.Build();
            sw.Stop();

            _logger.LogInformation( "Kernel built in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );

            sw.Restart();
            LoadPluginsForAgent( kernel );
            sw.Stop();

            _logger.LogInformation( "Plugins loaded in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );

            return new ChatAgent( kernel, _agentConfiguration, _rockContextFactory, _requestContextAccessor, _options );
        }

        /// <summary>
        /// Registers the plug-ins for the agent.
        /// </summary>
        /// <param name="kernel"></param>
        private void LoadPluginsForAgent( Kernel kernel )
        {
            LoadNativeSkills( kernel.Plugins, kernel.Services, _serviceProvider );
            LoadVirtualSkills( kernel.Plugins, kernel.Services );
        }

        /// <summary>
        /// Registers the native skills with the kernel.
        /// </summary>
        /// <param name="kernel"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void LoadNativeSkills( KernelPluginCollection pluginCollection, IServiceProvider kernelServiceProvider, IServiceProvider serviceProvider )
        {
            // Register native skills
            var nativeSkills = _agentConfiguration.Skills
                .Where( s => s.NativeType != null )
                .ToList();

            foreach ( var skillConfiguration in nativeSkills )
            {
                var skill = ( AgentSkillComponent ) ActivatorUtilities.CreateInstance( serviceProvider, skillConfiguration.NativeType );
                var methods = skillConfiguration.NativeType.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );
                var pluginFunctions = new List<KernelFunction>();

                skill.Initialize( skillConfiguration.ConfigurationValues, kernelServiceProvider.GetRequiredService<AgentRequestContext>() );

                // Register the C# method functions.
                foreach ( var method in methods )
                {
                    if ( method.GetCustomAttribute<KernelFunctionAttribute>() == null )
                    {
                        continue;
                    }

                    var functionGuid = method.GetCustomAttribute<AgentFunctionGuidAttribute>()?.Guid;

                    if ( !functionGuid.HasValue )
                    {
                        continue;
                    }

                    if ( skillConfiguration.DisabledFunctions.Contains( functionGuid.Value ) )
                    {
                        continue;
                    }

                    pluginFunctions.Add( KernelFunctionFactory.CreateFromMethod( method, skill, loggerFactory: _loggerFactory ) );
                }

                // Register dynamic functions
                var virtualFunctions = GetVirtualSkillFunctions( skill.GetSemanticFunctions(), kernelServiceProvider );
                pluginFunctions.AddRange( virtualFunctions );

                if ( pluginFunctions.Count == 0 )
                {
                    continue;
                }

                var distinctFunctions = pluginFunctions
                    .DistinctBy( kf => kf.Name );

                // Register the plug-in with the native and semantic functions.
                var plugin = KernelPluginFactory.CreateFromFunctions( skillConfiguration.NativeType.Name, skillConfiguration.Instructions, distinctFunctions );
                pluginCollection.Add( plugin );
            }
        }

        /// <summary>
        /// Loads the virtual skills. These are skills that are not native to the system but are defined in the database.
        /// </summary>
        /// <param name="kernel"></param>
        private void LoadVirtualSkills( KernelPluginCollection pluginCollection, IServiceProvider kernelServiceProvider )
        {
            foreach ( var skill in _agentConfiguration.Skills )
            {
                var pluginFunctions = GetVirtualSkillFunctions( skill.Functions, kernelServiceProvider );

                if ( pluginFunctions.Count > 0 )
                {
                    var plugin = KernelPluginFactory.CreateFromFunctions( skill.Key, skill.Instructions, pluginFunctions );
                    pluginCollection.Add( plugin );
                }
            }
        }

        /// <summary>
        /// Retrieves a collection of virtual skill functions (semantic or proxy) from a list of agent functions.
        /// </summary>
        /// <param name="functions">The collection of agent functions to process.</param>
        /// <returns>A collection of kernel functions representing the agent's virtual skills.</returns>
        private ICollection<KernelFunction> GetVirtualSkillFunctions( IReadOnlyCollection<AgentFunction> functions, IServiceProvider kernelServiceProvider )
        {
            var pluginFunctions = new Dictionary<string, KernelFunction>();

            if ( functions == null )
            {
                return Array.Empty<KernelFunction>();
            }

            var requestContext = _requestContextAccessor.RockRequestContext;
            var mergeFields = requestContext.GetCommonMergeFields();
            var schemaBuilder = new ParamaterSchemaBuilder();

            foreach ( var function in functions )
            {
                if ( function.FunctionType == FunctionType.AIPrompt )
                {
                    var prompt = function.Prompt;

                    if ( function.EnableLavaPreRendering )
                    {
                        prompt = prompt.ResolveMergeFields( mergeFields );
                    }

                    var semanticFunction = KernelFunctionFactory.CreateFromPrompt(
                        promptTemplate: prompt,
                        functionName: function.Key,
                        description: function.Instructions,
                        executionSettings: _agentConfiguration.Provider.GetFunctionPromptExecutionSettingsForRole( function ),
                        loggerFactory: _loggerFactory
                    );

                    pluginFunctions[function.Key] = semanticFunction;
                }

                else if ( function.FunctionType == FunctionType.ExecuteLava )
                {
                    var parameters = function.Parameters.Select( schemaBuilder.BuildKernelParameterMetadata ).ToList();
                    var proxySkill = new ProxyFunction( kernelServiceProvider.GetRequiredService<AgentRequestContext>(), requestContext );

                    var proxyFunction = KernelFunctionFactory.CreateFromMethod(
                        method: ( Func<KernelArguments, string> ) ( args => proxySkill.ExecuteLava( function, args ) ),
                        functionName: function.Key,
                        description: function.Instructions,
                        parameters: parameters,
                        loggerFactory: _loggerFactory
                    );

                    pluginFunctions[function.Key] = proxyFunction;
                }
            }

            return pluginFunctions.Values;
        }

        /// <summary>
        /// Retrieves the skill configurations for a given agent from the database.
        /// </summary>
        /// <param name="agentId">The ID of the agent to load skills for.</param>
        /// <param name="rockContext">The database context for data access.</param>
        /// <returns>A list of skill configurations associated with the agent.</returns>
        private List<SkillConfiguration> GetSkillConfigurations( int agentId, RockContext rockContext )
        {
            var agent = AIAgentCache.Get( agentId, rockContext );
            var requestContext = _requestContextAccessor.RockRequestContext;

            return agent.GetSkillConfigurations( requestContext.CurrentPerson, _options.IsSecurityEnabled, rockContext );
        }

        #endregion
    }
}
