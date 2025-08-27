using System;
using System.Collections.Generic;
using System.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using Rock.AI.Agent.Providers;
using Rock.Configuration;
using Rock.Data;
using Rock.Enums.AI.Agent;
using Rock.Lava;
using Rock.Lava.Fluid;
using Rock.Model;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests;

/// <summary>
/// Base class for function call tests that provides common functionality for
/// setting up and configuring the chat agent to call a specific function.
/// </summary>
public abstract class BaseFunctionCallTests : MockDatabaseTestsBase
{
    /// <summary>
    /// Configure a chat agent for testing with the specified function and seed.
    /// </summary>
    /// <param name="seed">This may be ignored currently, but it is supported by Azure AI as a "best effort" to provide predictable results.</param>
    /// <param name="function">The function to be registered with the agent for testing.</param>
    /// <returns>An instance of the chat agent, a list that will contain the output after calling the agent, and a list that will contain the logs after calling the agent.</returns>
    internal static (IChatAgent Chat, List<string> Output, List<string> Logs) ConfigureChatAgent( long? seed, AgentFunction function )
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
        providerMock.Setup( m => m.GetAttributeValue( "CodeModel" ) ).Returns( "gpt-5-mini" );
        providerMock.Setup( m => m.GetAttributeValue( "ResearchModel" ) ).Returns( "gpt-5-mini" );
        providerMock.Setup( m => m.GetAttributeValue( "DefaultModel" ) ).Returns( "gpt-5-mini" );
        providerMock.Setup( m => m.GetAttributeValue( "DefaultTemperature" ) ).Returns( "1" );
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

        rockRequestContextMock
            .Setup( m => m.GetCommonMergeFields( It.IsAny<Person>(), It.IsAny<Lava.CommonMergeFieldsOptions>() ) )
            .Returns( () => [] );
        requestContextAccessorMock
            .Setup( m => m.RockRequestContext )
            .Returns( rockRequestContextMock.Object );

        // Create the agent configuration that will be used for this test.
        var agentConfiguration = new AgentConfiguration( 1,
            providerMock.Object,
            "Test Agent",
            AgentType.Chat,
            AudienceType.Public,
            "You are a helpful assistant for Rock RMS.",
            new ChatAgentSettings(),
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
        var loggerFactory = new StringLoggerFactory( messages );

        // Create a lava engine with a log block that captures log messages to a list.
        var engine = new FluidEngine();
        var output = new List<string>();
        engine.RegisterBlock( "output", _ => new OutputBlock( output ) );
        LavaService.SetCurrentEngine( engine );

        // Create the factory that will build the chat agent.
        var factory = new ChatAgentFactory( providerMock.Object,
            agentConfiguration,
            RockApp.Current,
            requestContextAccessorMock.Object,
            loggerFactory,
            rockContextFactoryMock.Object,
            sc =>
            {
                sc.AddSingleton<ILoggerFactory>( loggerFactory );
            },
            new ChatAgentOptions() );

        var chat = factory.Build();

        return (chat, output, messages);
    }

    /// <summary>
    /// Get a random long value using a cryptographically secure random number generator.
    /// </summary>
    /// <returns>A random number.</returns>
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
