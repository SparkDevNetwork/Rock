using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Rock.AI.Agent.Providers;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.Core.AI.Agent;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests
{
    [TestClass]
    public class ProxyFunctionTests : MockDatabaseTestsBase
    {
        // Function Test-Search failed. Error: Missing argument for function parameter 'promptAsJson'
        private bool IgnoreCallFailures = false;

        [TestMethod]
        [DataRow( 1234L, false, false, false, "search jason j", "jason j" )]
        [DataRow( 1234L, false, false, false, "search for the person entity with a name like jason j", "jason j" )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the decker group", "decker group" )]
        //[DataRow( 1234L, true, false, false, "search jason j", "jason j" )] // Random Fail: Double nested promptAsJson parameter.
        //[DataRow( 1234L, false, true, false, "search jason j", "jason j" )]
        //[DataRow( 1234L, false, false, true, "search jason j", "jason j" )]
        public async Task PromptAsJson_SingleVirtualInput( long? seed, bool wrapSchema, bool improvedParameterUsageHint, bool schemaInUsageHint, string prompt, string expectedOutput )
        {
            var unwrappedSchema = """
                {
                    "type": "object",
                    "properties": {
                        "searchTerm": {
                            "type": "string",
                            "description": "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'."
                        }
                    },
                    "required": [ "searchTerm" ]
                }
                """;

            var usageHint = """
                🎯 Purpose:
                1. This function searches the database for people, groups, content channels that match the query from the user.

                🧭 Usage Guidance:
                1. This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages.
                """;

            var function = new AgentFunction
            {
                Name = "Search",
                UsageHint = usageHint,
                FunctionType = FunctionType.ExecuteLava,
                Prompt = "{% output %}{{ searchTerm }}{% endoutput %}No Results Found.",
                InputSchema = unwrappedSchema,
                Temperature = 0,
                MaxTokens = 128,
            };

            PrepareFunction( function, wrapSchema, schemaInUsageHint );

            var (chat, output, logs) = ConfigureChatAgent( seed, function, improvedParameterUsageHint );

            await chat.AddMessageAsync( AuthorRole.User, prompt );
            _ = await chat.GetChatMessageContentAsync();

            // That no function calls failed and that we had one succeed.
            if ( !IgnoreCallFailures )
            {
                Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
            }
            Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

            // Ensure the output data is valid.
            Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
            Assert.That.AreEqual( expectedOutput, output[0] );
        }

        [TestMethod]
        [DataRow( 1234L, false, false, false, "search jason j", new[] { "jason/j//", "/jason/j/" } )]
        [DataRow( 1234L, false, false, false, "search for the person entity with a name like jason j", new[] { "jason j/jason/j/", "/jason/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the decker group", new[] { "///decker" } )]
        //[DataRow( 1234L, true, false, false, "search jason j", "jason/j//" )] // Fail: Double nested promptAsJson parameter.
        //[DataRow( 1234L, false, true, false, "search jason j", "jason/j//" )]
        //[DataRow( 1234L, false, false, true, "search jason j", "jason j/jason/j/" )]
        public async Task PromptAsJson_FourVirtualInputs( long? seed, bool wrapSchema, bool improvedParameterUsageHint, bool schemaInUsageHint, string prompt, string[] expectedOutput )
        {
            var unwrappedSchema = """
            {
                "type": "object",
                "properties": {
                    "searchTerm": {
                        "type": "string",
                        "description": "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank."
                    },
                    "firstName": {
                        "type": ["string", "null"],
                        "description": "The first or nick name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "lastName": {
                        "type": ["string", "null"],
                        "description": "The last name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "groupName": {
                        "type": ["string", "null"],
                        "description": "The name of the group. Leave blank if empty."
                    }
                },
                "required": [ ]
            }
            """;

            var usageHint = """
                    🎯 Purpose:
                    1. This function searches the database for people, groups, content channels that match the query from the user.

                    🧭 Usage Guidance:
                    1. This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages.
                    """;

            var function = new AgentFunction
            {
                Name = "Search",
                UsageHint = usageHint,
                FunctionType = FunctionType.ExecuteLava,
                Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}{% endoutput %}No Results Found.",
                InputSchema = unwrappedSchema,
                Temperature = 0,
                MaxTokens = 128,
            };

            PrepareFunction( function, wrapSchema, schemaInUsageHint );

            var (chat, output, logs) = ConfigureChatAgent( seed, function, improvedParameterUsageHint );

            await chat.AddMessageAsync( AuthorRole.User, prompt );
            _ = await chat.GetChatMessageContentAsync();

            // That no function calls failed and that we had one succeed.
            if ( !IgnoreCallFailures )
            {
                Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
            }
            Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

            // Ensure the output data is valid.
            Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
            if ( !expectedOutput.Contains( output[0] ) )
            {
                Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
            }
        }

        [TestMethod]
        [DataRow( 1234L, false, false, false, "search jason j", new[] { "/jason/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the person entity with a name like jason j", new[] { "/jason/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the decker group", new[] { "///decker" } )]
        //[DataRow( 1234L, true, false, false, "search jason j", new[] { "/jason/j/" } )] 
        //[DataRow( 1234L, false, true, false, "search jason j", new[] { "/jason/j/" } )] // Random Fails: Arguments not wrapped in promptAsJson.
        //[DataRow( 1234L, false, false, true, "search jason j", new[] { "jason j///", "jason j/jason/j/" } )] // Random Fails: Arguments not wrapped in promptAsJson.
        public async Task PromptAsJson_NestedVirtualInputs( long? seed, bool wrapSchema, bool improvedParameterUsageHint, bool schemaInUsageHint, string prompt, string[] expectedOutput )
        {
            var unwrappedSchema = """
            {
                "type": "object",
                "properties": {
                    "entityTypes": {
                        "type": "array",
                        "description": "Pass an array of strings for the type of entities to search for, such as 'person', 'group' or 'content channel'. Leave empty if not known, but make a good effort to fill this in based on the request.",
                        "items": {
                            "type": "string",
                            "enum": ["person", "group", "content channel item"]
                        }
                    },
                    "searchTerm": {
                        "type": "string",
                        "description": "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. Prefer usage of 'firstName', 'lastName' and 'groupName' parameters over this one. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank."
                    },
                    "firstName": {
                        "type": ["string", "null"],
                        "description": "The first or nick name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "lastName": {
                        "type": ["string", "null"],
                        "description": "The last name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "groupName": {
                        "type": ["string", "null"],
                        "description": "The name of the group. Leave blank if empty."
                    }
                },
                "required": [ "entityTypes" ]
            }
            """;

            var usageHint = """
                    🎯 Purpose:
                    1. This function searches the database for people, groups, content channels that match the query from the user.

                    🧭 Usage Guidance:
                    1. This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages.
                    """;

            var function = new AgentFunction
            {
                Name = "Search",
                UsageHint = usageHint,
                FunctionType = FunctionType.ExecuteLava,
                Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}{% endoutput %}No Results Found.",
                InputSchema = unwrappedSchema,
                Temperature = 0,
                MaxTokens = 128,
            };

            PrepareFunction( function, wrapSchema, schemaInUsageHint );

            var (chat, output, logs) = ConfigureChatAgent( seed, function, improvedParameterUsageHint );

            await chat.AddMessageAsync( AuthorRole.User, prompt );
            _ = await chat.GetChatMessageContentAsync();

            // That no function calls failed and that we had one succeed.
            if ( !IgnoreCallFailures )
            {
                Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
            }
            Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

            // Ensure the output data is valid.
            Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
            if ( !expectedOutput.Contains( output[0] ) )
            {
                Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
            }
        }

        [TestMethod]
        [DataRow( 1234L, false, false, false, "search jason j", new[] { "/jason/j/", "/j/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the person entity with a name like jason j", new[] { "/jason/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        [DataRow( 1234L, false, false, false, "search for the decker group", new[] { "///decker" } )]
        //[DataRow( 1234L, true, false, false, "search jason j", new[] { "/jason/j/" } )] // Fail: Double nested promptAsJson parameter.
        //[DataRow( 1234L, false, true, false, "search jason j", new[] { "/jason/j/" } )] // Fail: Arguments not wrapped in promptAsJson.
        //[DataRow( 1234L, false, false, true, "search jason j", new[] { "jason j///", "jason j/jason/j/" } )]
        public async Task PromptAsJson_ArrayAsStringVirtualInputs( long? seed, bool wrapSchema, bool improvedParameterUsageHint, bool schemaInUsageHint, string prompt, string[] expectedOutput )
        {
            var unwrappedSchema = """
            {
                "type": "object",
                "properties": {
                    "entityTypes": {
                        "type": "string",
                        "description": "The type of entities to search for as a comma separated string, valid values are 'person', 'group' and 'content channel'. Leave blank if not known, but try to infer based on the request and which other parameters were filled in."
                    },
                    "searchTerm": {
                        "type": "string",
                        "description": "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. Prefer usage of 'firstName', 'lastName' and 'groupName' parameters over this one. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank."
                    },
                    "firstName": {
                        "type": ["string", "null"],
                        "description": "The first or nick name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "lastName": {
                        "type": ["string", "null"],
                        "description": "The last name the person was looking for. Leave blank if they did not specify a value."
                    },
                    "groupName": {
                        "type": ["string", "null"],
                        "description": "The name of the group. Leave blank if empty."
                    }
                },
                "required": [ "entityTypes" ]
            }
            """;

            var usageHint = """
                    🎯 Purpose:
                    1. This function searches the database for people, groups, content channels that match the query from the user.

                    🧭 Usage Guidance:
                    1. This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages.
                    """;

            var function = new AgentFunction
            {
                Name = "Search",
                UsageHint = usageHint,
                FunctionType = FunctionType.ExecuteLava,
                Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}{% endoutput %}No Results Found.",
                InputSchema = unwrappedSchema,
                Temperature = 0,
                MaxTokens = 128,
            };

            PrepareFunction( function, wrapSchema, schemaInUsageHint );

            var (chat, output, logs) = ConfigureChatAgent( seed, function, improvedParameterUsageHint );

            await chat.AddMessageAsync( AuthorRole.User, prompt );
            _ = await chat.GetChatMessageContentAsync();

            // That no function calls failed and that we had one succeed.
            if ( !IgnoreCallFailures )
            {
                Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
            }
            Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

            // Ensure the output data is valid.
            Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
            if ( !expectedOutput.Contains( output[0] ) )
            {
                Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
            }
        }

        private (IChatAgent Chat, List<string> Output, List<string> Logs) ConfigureChatAgent( long? seed, AgentFunction function, bool improvedParameterUsageHint = false )
        {
            var apiKey = ConfigurationManager.AppSettings["AzureOpenAIApiKey"];
            var endpoint = ConfigurationManager.AppSettings["AzureOpenAIEndpoint"];

            // Create a mock provider that uses the standard Azure Open AI provider
            // but is configured with the test API key and endpoint rather than needing
            // to get them from the database.
            var providerMock = new Mock<AzureOpenAIProvider>( false )
            {
                CallBase = true
            };

            if ( !seed.HasValue )
            {
                seed = GetRandomLong();
                Console.WriteLine( $"Configured a seed value of {seed}." );
            }
            else if ( seed == 0 )
            {
                seed = null;
                Console.WriteLine( "Configured with no seed value." );
            }

            providerMock.Setup( m => m.GetAttributeValue( "ApiKey" ) ).Returns( apiKey );
            providerMock.Setup( m => m.GetAttributeValue( "Endpoint" ) ).Returns( endpoint );
            providerMock.Setup( m => m.GetAttributeValue( "DefaultTemperature" ) ).Returns( "0" );
            providerMock.Setup( m => m.GetAttributeValue( "DefaultTopP" ) ).Returns( "1" );
            providerMock.Setup( m => m.GetAttributeValue( "Seed" ) ).Returns( seed.ToString() );

            // Create mocks for accessing the database.
            var rockContextMock = MockDatabaseHelper.GetRockContextMock();
            var rockContextFactoryMock = new Mock<IRockContextFactory>();

            // Create a mock for accessing the request context. This is used to
            // get the Lava merge fields so we need to override that.
            var requestContextAccessorMock = new Mock<Net.IRockRequestContextAccessor>();
            var rockRequestContextMock = new Mock<Net.RockRequestContext>
            {
                CallBase = true
            };

            rockRequestContextMock.Setup( m => m.GetCommonMergeFields( It.IsAny<Person>(), It.IsAny<Lava.CommonMergeFieldsOptions>() ) )
                .Returns( new Dictionary<string, object>() );
            requestContextAccessorMock.Setup( m => m.RockRequestContext ).Returns( rockRequestContextMock.Object );

            // Create the agent configuration that will be used for this test.
            var agentConfiguration = new AgentConfiguration( 1,
                providerMock.Object,
                "You are a helpful assistant for Rock RMS.",
                new AgentSettings(),
                new List<SkillConfiguration>
                {
                    new SkillConfiguration( "Test",
                        "The only skill available for use.",
                        new List<AgentFunction>
                        {
                            function
                        } )
                } );

            // Create a logger factory that captures log messages to a list.
            var messages = new List<string>();
            var loggerFactory = new TestLoggerFactory( messages );

            // Create a lava engine with a log block that captures log messages to a list.
            var engine = new FluidEngine();
            var output = new List<string>();
            engine.RegisterBlock( "output", _ => new OutputBlock( output ) );
            LavaService.SetCurrentEngine( engine );

            // Create the factory that will build the chat agent.
            var factory = new ChatAgentFactory( providerMock.Object,
                agentConfiguration,
                RockApp.Current,
                rockContextMock.Object,
                requestContextAccessorMock.Object,
                loggerFactory,
                rockContextFactoryMock.Object,
                sc =>
                {
                    sc.AddSingleton<ILoggerFactory>( loggerFactory );
                } );

            if ( improvedParameterUsageHint )
            {
                factory.ExecuteLavaParameterHint = "A JSON object with the parameters defined in the schema. This is the **only input parameter**. Do **not separate fields** or try to flatten the schema. All values mus tbe included as a single JSON object inside this string.";
            }

            var chat = factory.Build();

            return (chat, output, messages);
        }

        static void PrepareFunction( AgentFunction function, bool wrapSchema, bool schemaInUsageHint )
        {
            if ( wrapSchema )
            {
                function.InputSchema = $"{{ \"type\": \"object\", \"properties\": {{ \"promptAsJson\": {function.InputSchema} }}, \"required\": [ \"promptAsJson\" ] }}";
            }

            if ( schemaInUsageHint )
            {
                function.UsageHint += "\n\nThis function takes a single parameter called 'promptAsJson' which is a string representing the JSON parameters conforming to this schema:\n" + function.InputSchema;
                function.InputSchema = null;
            }
        }

        static long GetRandomLong()
        {
            var buffer = new byte[8];
            using ( var rng = System.Security.Cryptography.RandomNumberGenerator.Create() )
            {
                rng.GetBytes( buffer );
            }

            return BitConverter.ToInt64( buffer, 0 );
        }
    }

    class TestLoggerFactory : ILoggerFactory
    {
        private readonly List<string> _messages;

        public TestLoggerFactory( List<string> messages )
        {
            _messages = messages;
        }

        public void AddProvider( ILoggerProvider provider )
        {
        }

        public ILogger CreateLogger( string categoryName )
        {
            return new TestLogger( _messages );
        }

        public void Dispose()
        {
        }

        private class TestLogger : ILogger
        {
            private readonly List<string> _messages;

            public TestLogger( List<string> messages )
            {
                _messages = messages;
            }

            public IDisposable BeginScope<TState>( TState state ) => throw new NotImplementedException();

            public bool IsEnabled( LogLevel logLevel ) => true;

            public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter )
            {
                Console.WriteLine( formatter( state, exception ) );
                lock ( _messages )
                {
                    _messages.Add( formatter( state, exception ) );
                }
            }
        }
    }

    class OutputBlock : LavaBlockBase
    {
        private readonly List<string> _logs;

        public OutputBlock( List<string> logs )
        {
            _logs = logs;
        }

        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            using var writer = new StringWriter();

            base.OnRender( context, writer );
            var logText = writer.ToString();

            _logs.Add( logText );
        }
    }
}
