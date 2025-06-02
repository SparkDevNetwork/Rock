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
using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Logging;

namespace Rock.AI.Agent
{
    class ChatAgentFactory
    {
        private readonly int _agentId;

        private readonly AgentConfiguration _agentConfiguration;

        private readonly List<KernelPlugin> _virtualPlugins;

        private readonly IKernelBuilder _kernelBuilder;

        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger _logger;

        public ChatAgentFactory( int agentId, RockContext rockContext, ILoggerFactory loggerFactory )
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            _agentId = agentId;
            _loggerFactory = loggerFactory;

            // Get a AI Agent Provider
            var provider = AgentProviderContainer.GetComponent( "Rock.AI.Agent.Providers.AzureOpenAiAgentProvider" );// TODO: this should be configurable from agentId

            // Register the ModelServiceRoles
            var kernelBuilder = Kernel.CreateBuilder();
            kernelBuilder.Services.AddScoped<AgentRequestContext>();
            kernelBuilder.Services.AddRockLogging();

            foreach ( ModelServiceRole role in Enum.GetValues( typeof( ModelServiceRole ) ) )
            {
                provider.AddChatCompletion( role, kernelBuilder.Services );
            }

            _kernelBuilder = kernelBuilder;

            _agentConfiguration = new AgentConfiguration( provider, string.Empty, GetMockAiSkills() );
            _virtualPlugins = LoadVirtualSkills();
            _logger = loggerFactory.CreateLogger<ChatAgentFactory>();
            sw.Stop();

            _logger.LogInformation( "Initialized factory in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentId );
        }

        public IChatAgent Build( IServiceProvider serviceProvider )
        {
            //for ( int i = 0; i < 10_000; i++ )
            //{
            //    var kernel2 = _kernelBuilder.Build();
            //    LoadPluginsForAgent( kernel2, serviceProvider );
            //}

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var kernel = _kernelBuilder.Build();
            sw.Stop();

            _logger.LogInformation( "Kernel built in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentId );

            sw.Restart();
            LoadPluginsForAgent( kernel, serviceProvider );
            sw.Stop();

            _logger.LogInformation( "Plugins loaded in {ElapsedMilliseconds}ms for AgentId {AgentId}.", sw.Elapsed.TotalMilliseconds, _agentId );

            return new ChatAgent( kernel, _agentConfiguration.Provider );
        }

        /// <summary>
        /// Registers the plug-ins for the agent.
        /// </summary>
        /// <param name="kernel"></param>
        private void LoadPluginsForAgent( Kernel kernel, IServiceProvider serviceProvider )
        {
            LoadNativeSkills( kernel.Plugins, serviceProvider );

            var x = LoadVirtualSkills();
            kernel.Plugins.AddRange( x );
        }

        /// <summary>
        /// Registers the native skills with the kernel.
        /// </summary>
        /// <param name="kernel"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void LoadNativeSkills( KernelPluginCollection pluginCollection, IServiceProvider serviceProvider )
        {
            // Register native skills
            var skillTypes = _agentConfiguration.Skills.Where( s => s.NativeType != null ).Select( s => s.NativeType ).ToList();

            foreach ( var type in skillTypes )
            {
                // Get Semantic Functions
                var skill = ( IRockAiSkill ) ActivatorUtilities.CreateInstance( serviceProvider, type );

                var plugin = KernelPluginFactory.CreateFromObject( skill, type.Name, _loggerFactory );

                // Register dynamic functions
                var skillFunctions = skill.GetSemanticFunctions();

                // Check if the skill has any semantic functions to register
                if ( skillFunctions == null || skillFunctions.Count == 0 )
                {
                    continue;
                }

                // Get the existing plug-in functions
                var pluginFunctions = plugin//Collection[type.Name]
                    .Select( kf => new KeyValuePair<string, KernelFunction>( kf.Name, kf ) )
                    .ToDictionary( x => x.Key, x => x.Value );

                foreach ( var skillFunction in skillFunctions )
                {
                    var semanticFunction = KernelFunctionFactory.CreateFromPrompt(
                        promptTemplate: skillFunction.Prompt, // TODO: process Lava if needed.
                        functionName: skillFunction.Name,
                        description: skillFunction.UsageHint,
                        executionSettings: skillFunction.GetExecutionSettings( _agentConfiguration.Provider ),
                        loggerFactory: _loggerFactory
                    );

                    pluginFunctions[skillFunction.Name] = semanticFunction;
                }

                // Re-register the plug-in with the new semantic functions added
                //pluginCollection.Remove( pluginCollection[type.Name] );
                plugin = KernelPluginFactory.CreateFromFunctions( type.Name, pluginFunctions.Values );
                pluginCollection.Add( plugin );
            }
        }

        /// <summary>
        /// Loads the virtual skills. These are skills that are not native to the system but are defined in the database.
        /// </summary>
        /// <param name="kernel"></param>
        private List<KernelPlugin> LoadVirtualSkills()
        {
            var plugins = new List<KernelPlugin>();

            foreach ( var skill in _agentConfiguration.Skills )
            {
                if ( skill.Functions == null )
                {
                    continue;
                }

                var pluginFunctions = new Dictionary<string, KernelFunction>();

                // The properties for KernelParameterMetadata are init-only, which is
                // not supported on C# 7.3. So we can't set them directly and are
                // required to use reflection.
                //
                // Report here: https://github.com/microsoft/semantic-kernel/issues/12297
                var parameterType = typeof( KernelParameterMetadata );
                var descriptionProperty = parameterType.GetProperty( nameof( KernelParameterMetadata.Description ) );
                var isRequiredProperty = parameterType.GetProperty( nameof( KernelParameterMetadata.IsRequired ) );
                var schemaProperty = parameterType.GetProperty( nameof( KernelParameterMetadata.Schema ) );

                // Register functions
                foreach ( var function in skill.Functions )
                {
                    if ( function.FunctionType == FunctionType.AiPrompt )
                    {
                        var semanticFunction = KernelFunctionFactory.CreateFromPrompt(
                            promptTemplate: function.Prompt, // TODO: process Lava if needed.
                            functionName: function.Key,
                            description: function.UsageHint,
                            executionSettings: function.GetExecutionSettings( _agentConfiguration.Provider ),
                            loggerFactory: _loggerFactory
                        );

                        pluginFunctions[function.Key] = semanticFunction;
                    }

                    else if ( function.FunctionType == FunctionType.ExecuteLava )
                    {
                        var functionParameters = new List<KernelParameterMetadata>();
                        var parameter = new KernelParameterMetadata( "promptAsJson" );

                        descriptionProperty.SetValue( parameter, "A JSON object with the information to register for the event." );
                        isRequiredProperty.SetValue( parameter, true );
                        schemaProperty.SetValue( parameter, ParseSchema( function.InputSchema ) );

                        functionParameters.Add( parameter );

                        var proxyFunction = KernelFunctionFactory.CreateFromMethod(
                            ( Func<Kernel, string, string> ) ( ( Kernel kernel, string promptAsJson ) =>
                            {
                                // Create a LavaSkill instance that will be used to run the function.
                                var proxySkill = new AiProxySkill( kernel.Services.GetRequiredService<AgentRequestContext>() );
                                return proxySkill.RunLavaFromJson( promptAsJson, function.Prompt );
                            } ),
                            functionName: function.Name,
                            description: function.UsageHint,
                            parameters: functionParameters,
                            loggerFactory: _loggerFactory
                        );

                        pluginFunctions[function.Name] = proxyFunction;
                    }
                }

                var plugin = KernelPluginFactory.CreateFromFunctions( skill.Key, pluginFunctions.Values );
                plugins.Add( plugin );
            }

            return plugins;
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

        private static List<SkillConfiguration> GetMockAiSkills()
        {
            return new List<SkillConfiguration>
            {
                new SkillConfiguration( typeof( Skills.GroupManagerSkill ) ),

                new SkillConfiguration(
                    "Knowledge Base",
                    "Use only for internal organizational data, such as staff directories, event details, or ministry-specific content. The knowledge here is limited to the content from the organization.",
                    new List<AgentFunction>
                    {
                        new AgentFunction
                        {
                            Name = "Organization Search",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.AiPrompt,
                            UsageHint = "Performs and AI search of information specific to the organization and it's ministry.",
                            Prompt = "System Prompt:\n{{$system}}\n\nThe organizations phone number is (623) 867-5209"
                        }
                    }
                ),

                new SkillConfiguration(
                    "Individual Updates",
                    "Used for updating information about a person.",
                    new List<AgentFunction>
                    {
                        new AgentFunction
                        {
                            Name = "GetNoteTypes",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.AiPrompt,
                            EnableLavaPreRendering = true,
                            UsageHint = "Used to get a list of valid note types.",
                            Prompt = "{ \"NoteTypes\": [{ \"Id\": 1222, \"Name\": \"General Note\"}, {\"Id\": 1322, \"Name\": \"Pastoral Note\"}]}."
                        },
                        new AgentFunction
                        {
                            Name = "AddNote",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.ExecuteLava,
                            UsageHint = "Used to add a note to a person.",
                            Prompt = @"If no note type was provided, call the function GetValidNoteTypes to retrieve the correct list before continuing. Otherwise provide a message that the note was created..",
                            InputSchema = @"{
                                      ""type"": ""object"",
                                      ""properties"": {
                                        ""NoteTypeId"": { ""type"": ""integer"" },
                                        ""Content"": { ""type"": ""string"" }
                                      },
                                      ""required"": [""NoteTypeId"", ""Content""]
                                    }"
                        },
                    }
                ),

                new SkillConfiguration(
                    "Ministry Tasks",
                    "Used for completing various ministry tasks like scheduling a baptism, wedding or funeral.",
                    new List<AgentFunction>
                    {
                        new AgentFunction
                        {
                            Name = "Schedule Baptism",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.AiPrompt,
                            UsageHint = "Used to schedule a baptism for a person.",
                            Prompt = "Input:\n{{$input}}\n\nBaptism has been scheduled."
                        },
                        new AgentFunction
                        {
                            Name = "Schedule Wedding",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.AiPrompt,
                            UsageHint = "Used to schedule a wedding for a person based on the date provided.",
                            Prompt = "Input:\n{{$input}}\n\nIf their name is Jon say An issue came up while scheduling, please call the office otherwise say it has been completed."
                        },
                        new AgentFunction
                        {
                            Name = "ComposeEmailFromIntent",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.AiPrompt,
                            Prompt = @"You are an assistant that helps draft emails.

                                    Based on the user intent of the request, create a professional but warm message.
                                    Include both a subject line and the body.

                                    First, analyze the user's request and generate an appropriate subject and body.

                                    Unless otherwise stated assume that the email is from the current person.

                                    Present the draft email to the user, and ask:
                                    ""Would you like me to send this?""

                                    DO NOT send the email yet. Wait for user approval.
                                    "
                        },
                        new AgentFunction
                        {
                            Name = "AddPrayer",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.ExecuteLava,
                            UsageHint = "Is used to add a new prayer request to the system. We will need the following items passed to us: RequestText, Topic.",
                            Prompt = "say that they're prayer request is added and say something spiritual."
                        },
                        new AgentFunction
                        {
                            Name = "SendEmail",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.ExecuteLava,
                            UsageHint = "Used to send a new email. Use only after the user has approved the draft. We will need the following items passed to us: PersonId, Message, Subject.",
                            Prompt = "say that the email has been sent."
                        },
                        new AgentFunction
                        {
                            Name = "EventRegistration",
                            Role = ModelServiceRole.Default,
                            FunctionType = FunctionType.ExecuteLava,
                            UsageHint = "Used to register a person for an event. We will need: EventId.",
                            Prompt = "respond that they are registered and we'll see them on Monday July 4th.",
                            InputSchema = @"{
                                    ""type"": ""object"",
                                    ""properties"": {
                                    ""EventId"": { ""type"": ""integer"" },
                                    ""PersonId"": { ""type"": ""integer"" }
                                    },
                                    ""required"": [""EventId"", ""PersonId""]
                                }"
                        }
                    }
                ),
            };
        }
    }
}
