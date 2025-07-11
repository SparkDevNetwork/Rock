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
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.Logging;
using Rock.Model;
using Rock.Net;
using Rock.SystemGuid;
using Rock.Web.Cache.Entities;

namespace Rock.AI.Agent
{
    internal class ChatAgentFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly AgentConfiguration _agentConfiguration;

        private readonly IKernelBuilder _kernelBuilder;

        private readonly IRockRequestContextAccessor _requestContextAccessor;

        private readonly ILoggerFactory _loggerFactory;

        private readonly IRockContextFactory _rockContextFactory;

        private readonly ILogger _logger;

        internal string ExecuteLavaParameterHint { get; set; } = "A JSON object with the parameters defined in the schema.";

        private ChatAgentFactory( IServiceProvider serviceProvider, RockContext rockContext, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory )
        {
            _serviceProvider = serviceProvider; ;
            _requestContextAccessor = requestContextAccessor;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<ChatAgentFactory>();
            _rockContextFactory = rockContextFactory;
        }

        public ChatAgentFactory( int agentId, IServiceProvider serviceProvider, RockContext rockContext, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory )
            : this( serviceProvider, rockContext, requestContextAccessor, loggerFactory, rockContextFactory )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var provider = AgentProviderContainer.GetActiveComponent();
            _kernelBuilder = CreateKernelBuilder( provider, null );

            var agent = AIAgentCache.Get( agentId, rockContext );
            var settings = agent.GetAdditionalSettings<AgentSettings>();

            _agentConfiguration = new AgentConfiguration( agentId, provider, string.Empty, settings, GetSkillConfigurations( agentId, rockContext ) );
            sw.Stop();

            _logger.LogInformation( "Initialized factory in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );
        }

        internal ChatAgentFactory( AgentProviderComponent provider, AgentConfiguration agentConfiguration, IServiceProvider serviceProvider, RockContext rockContext, IRockRequestContextAccessor requestContextAccessor, ILoggerFactory loggerFactory, IRockContextFactory rockContextFactory, Action<IServiceCollection> configureServices )
            : this( serviceProvider, rockContext, requestContextAccessor, loggerFactory, rockContextFactory )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            _kernelBuilder = CreateKernelBuilder( provider, configureServices );
            _agentConfiguration = agentConfiguration;

            sw.Stop();

            _logger.LogInformation( "Initialized factory in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );
        }

        private static IKernelBuilder CreateKernelBuilder( AgentProviderComponent provider, Action<IServiceCollection> configureServices  )
        {
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddSingleton<AgentRequestContext>();
            kernelBuilder.Services.AddRockLogging();

            configureServices?.Invoke( kernelBuilder.Services );

            foreach ( ModelServiceRole role in Enum.GetValues( typeof( ModelServiceRole ) ) )
            {
                provider.AddChatCompletion( role, kernelBuilder.Services );
            }

            return kernelBuilder;
        }

        public IChatAgent Build()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var kernel = _kernelBuilder.Build();
            sw.Stop();

            _logger.LogInformation( "Kernel built in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );

            sw.Restart();
            LoadPluginsForAgent( kernel, _serviceProvider );
            sw.Stop();

            _logger.LogInformation( "Plugins loaded in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentConfiguration.AgentId );

            return new ChatAgent( kernel, _agentConfiguration, _rockContextFactory, _requestContextAccessor );
        }

        /// <summary>
        /// Registers the plug-ins for the agent.
        /// </summary>
        /// <param name="kernel"></param>
        private void LoadPluginsForAgent( Kernel kernel, IServiceProvider serviceProvider )
        {
            LoadNativeSkills( kernel.Plugins, kernel.Services, _serviceProvider );
            LoadVirtualSkills( kernel.Plugins );
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
                var virtualFunctions = GetVirtualSkillFunctions( skill.GetSemanticFunctions() );
                pluginFunctions.AddRange( virtualFunctions );

                if ( pluginFunctions.Count == 0 )
                {
                    continue;
                }

                var distinctFunctions = pluginFunctions
                    .DistinctBy( kf => kf.Name );

                // Register the plug-in with the native and semantic functions.
                var plugin = KernelPluginFactory.CreateFromFunctions( skillConfiguration.NativeType.Name, skillConfiguration.UsageHint, distinctFunctions );
                pluginCollection.Add( plugin );
            }
        }

        /// <summary>
        /// Loads the virtual skills. These are skills that are not native to the system but are defined in the database.
        /// </summary>
        /// <param name="kernel"></param>
        private void LoadVirtualSkills( KernelPluginCollection pluginCollection )
        {
            foreach ( var skill in _agentConfiguration.Skills )
            {
                var pluginFunctions = GetVirtualSkillFunctions( skill.Functions );

                if ( pluginFunctions.Count > 0 )
                {
                    var plugin = KernelPluginFactory.CreateFromFunctions( skill.Key, skill.UsageHint, pluginFunctions );
                    pluginCollection.Add( plugin );
                }
            }
        }

        private ICollection<KernelFunction> GetVirtualSkillFunctions( IReadOnlyCollection<AgentFunction> functions )
        {
            var pluginFunctions = new Dictionary<string, KernelFunction>();

            if ( functions == null )
            {
                return Array.Empty<KernelFunction>();
            }

            var requestContext = _requestContextAccessor.RockRequestContext;
            var mergeFields = requestContext.GetCommonMergeFields()
                ?? new Dictionary<string, object>();

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
                        description: function.UsageHint,
                        executionSettings: _agentConfiguration.Provider.GetFunctionPromptExecutionSettingsForRole( function ),
                        loggerFactory: _loggerFactory
                    );

                    pluginFunctions[function.Key] = semanticFunction;
                    //var proxyFunction = KernelFunctionFactory.CreateFromMethod(
                    //    ( Func<Kernel, string, Task<string>> ) ( async ( Kernel kernel, string input ) =>
                    //    {
                    //        return await ExecutePrompt( function, kernel, input );
                    //    } ),
                    //    functionName: function.Key,
                    //    description: function.UsageHint,
                    //    loggerFactory: _loggerFactory
                    //);

                    //pluginFunctions[function.Key] = proxyFunction;
                }

                else if ( function.FunctionType == FunctionType.ExecuteLava )
                {
                    //var functionParameters = new List<KernelParameterMetadata>();
                    //var parameter = new KernelParameterMetadata( "promptAsJson" )
                    //{
                    //    Description = ExecuteLavaParameterHint,
                    //    IsRequired = true,
                    //    Schema = ParseSchema( function.InputSchema )
                    //};

                    //functionParameters.Add( parameter );

                    var proxyFunction = KernelFunctionFactory.CreateFromMethod(
                        ( Func<Kernel, KernelArguments, string> ) ( ( Kernel kernel, KernelArguments args ) =>
                        {
                            // Create a LavaSkill instance that will be used to run the function.
                            var proxySkill = new ProxyFunction( kernel.Services.GetRequiredService<AgentRequestContext>(), requestContext );
                            return proxySkill.ExecuteLava( function, args );
                        } ),
                        functionName: function.Key,
                        description: function.UsageHint,
                        parameters: ParseSchemaParameters( function.InputSchema ),
                        //parameters: functionParameters,
                        loggerFactory: _loggerFactory
                    );

                    pluginFunctions[function.Key] = proxyFunction;
                }
            }

            return pluginFunctions.Values;
        }

        /// <summary>
        /// Parses the input schema into a KernelJsonSchema object.
        /// </summary>
        /// <param name="inputSchema"></param>
        /// <returns></returns>
        private static KernelJsonSchema ParseSchema( string inputSchema )
        {
            if ( string.IsNullOrWhiteSpace( inputSchema ) )
            {
                return null;
            }

            try
            {
                // Deserialize the input schema to a KernelJsonSchema object
                return JsonSerializer.Deserialize<KernelJsonSchema>( inputSchema );
            }
            catch
            {
                return null;
            }
        }

        private static List<KernelParameterMetadata> ParseSchemaParameters( string inputSchema )
        {
            var schema = ParseSchema( inputSchema );

            if ( schema?.RootElement == null )
            {
                return new List<KernelParameterMetadata>();
            }

            if ( !schema.RootElement.TryGetProperty( "properties", out var properties ) || properties.ValueKind != JsonValueKind.Object )
            {
                return new List<KernelParameterMetadata>();
            }

            var requiredParameters = schema.RootElement.TryGetProperty( "required", out var requiredProperty )
                ? requiredProperty.EnumerateArray().Select( p => p.GetString() ).ToList()
                : new List<string>();
            var parameters = new List<KernelParameterMetadata>();

            foreach ( var property in properties.EnumerateObject() )
            {
                if ( property.Value.ValueKind != JsonValueKind.Object )
                {
                    continue;
                }

                var parameter = new KernelParameterMetadata( property.Name )
                {
                    Description = property.Value.TryGetProperty( "description", out var description ) ? description.GetString() : string.Empty,
                    IsRequired = requiredParameters.Contains( property.Name ),
                    Schema = JsonSerializer.Deserialize<KernelJsonSchema>( property.Value )
                };

                parameters.Add( parameter );
            }

            return parameters;
        }

        private static List<SkillConfiguration> GetSkillConfigurations( int agentId, RockContext rockContext )
        {
            var agent = AIAgentCache.Get( agentId, rockContext );

            return agent.GetSkillConfigurations( rockContext );
        }

        //private async Task<string> ExecutePrompt( AgentFunction function, Kernel kernel, string input )
        //{
        //    var originalContext = kernel.Services.GetRequiredService<AgentRequestContext>();

        //    using ( var scope = kernel.Services.CreateScope() )
        //    {
        //        var scopedKernel = new Kernel( scope.ServiceProvider, kernel.Plugins );
        //        var agent = new ChatAgent( scopedKernel, _agentConfiguration, _rockContextFactory, _requestContextAccessor );
        //        var prompt = function.Prompt;

        //        agent.Context.CopyFrom( originalContext );

        //        if ( function.EnableLavaPreRendering )
        //        {
        //            var mergeFields = _requestContextAccessor.RockRequestContext.GetCommonMergeFields();

        //            mergeFields["AgentContext"] = agent.Context;
        //            mergeFields["Input"] = input;

        //            prompt = prompt.ResolveMergeFields( mergeFields );
        //        }

        //        agent.AddMessage( AuthorRole.User, prompt );

        //        var response = await agent.GetChatMessageContentAsync();

        //        return response.Content;
        //    }
        //}
    }
}
