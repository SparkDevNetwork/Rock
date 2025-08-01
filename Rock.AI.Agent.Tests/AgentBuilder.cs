using System;
using System.Collections.Generic;
using System.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Moq;

using Rock.AI.Agent.Providers;
using Rock.Configuration;
using Rock.Data;
using Rock.Lava.Fluid;
using Rock.Lava;
using Rock.Tests.Shared.TestFramework;
using Microsoft.Extensions.Logging;
using Rock.Model;

namespace Rock.AI.Agent.Tests;

/// <summary>
/// Builder pattern for setting up a test chat agent with configurable skills and functions.
/// </summary>
internal class AgentBuilder
{
    #region Fields

    private readonly List<AgentFunction> _individualFunctions = [];
    private readonly List<SkillConfiguration> _skills = [];
    private long? _seed;
    private string _persona = "You are a helpful assistant for Rock RMS.";
    private static int _agentIdCounter = 1;

    #endregion

    #region Fluent Configuration

    /// <summary>
    /// Adds an individual function to the agent.
    /// </summary>
    /// <param name="function">The function to add.</param>
    /// <returns>The builder for chaining.</returns>
    internal AgentBuilder WithFunction( AgentFunction function )
    {
        _individualFunctions.Add( function );
        return this;
    }

    /// <summary>
    /// Adds a full skill configuration (group of functions) to the agent.
    /// </summary>
    /// <param name="skill">The skill to add.</param>
    /// <returns>The builder for chaining.</returns>
    internal AgentBuilder WithSkill( SkillConfiguration skill )
    {
        _skills.Add( skill );
        return this;
    }

    /// <summary>
    /// Sets the random seed for the agent (for deterministic responses in some LLMs).
    /// </summary>
    /// <param name="seed">The seed value (nullable).</param>
    /// <returns>The builder for chaining.</returns>
    internal AgentBuilder WithSeed( long? seed )
    {
        _seed = seed;
        return this;
    }

    /// <summary>
    /// Sets the system prompt/persona for the agent.
    /// </summary>
    /// <param name="prompt">The prompt text.</param>
    /// <returns>The builder for chaining.</returns>
    internal AgentBuilder WithPersona( string prompt )
    {
        _persona = prompt;
        return this;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Builds the IChatAgent instance using the configured functions, skills, and settings.
    /// </summary>
    /// <returns>A configured IChatAgent instance.</returns>
    internal ( IChatAgent Agent, List<string> output, List<string> logs ) Build()
    {
        var apiKey = ConfigurationManager.AppSettings["AzureOpenAIApiKey"];
        var endpoint = ConfigurationManager.AppSettings["AzureOpenAIEndpoint"];

        // Create a mock provider configured with the test API key and endpoint.
        var providerMock = new Mock<AzureOpenAIProvider>( false ) { CallBase = true };

        var seed = _seed;
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
        providerMock.Setup( m => m.GetAttributeValue( "Seed" ) ).Returns( seed?.ToString() );

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

        _agentIdCounter++;

        var agentConfiguration = new AgentConfiguration(
            agentId: _agentIdCounter,
            provider: providerMock.Object,
            persona: _persona,
            settings: new AgentSettings(),
            skills: GetSkills()
        );

        var messages = new List<string>();
        var loggerFactory = new StringLoggerFactory( messages );

        // Create a lava engine with a log block that captures log messages to a list.
        var engine = new FluidEngine();
        var output = new List<string>();
        engine.RegisterBlock( "output", _ => new OutputBlock( output ) );
        LavaService.SetCurrentEngine( engine );

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

       return ( factory.Build(), output, messages );
    }

    /// <summary>
    /// Wraps any individual functions in a SkillConfiguration for inclusion in the agent.
    /// </summary>
    /// <returns>The skill containing individual functions, or null if none exist.</returns>
    private SkillConfiguration GetIndividualFunctionsSkill()
    {
        if ( _individualFunctions.Count == 0 )
        {
            return null;
        }

        return new SkillConfiguration(
            "Individual Functions",
            "A collection of individual functions that can be used by the agent.",
            _individualFunctions
        );
    }

    /// <summary>
    /// Collects all configured skills, including any individual functions.
    /// </summary>
    /// <returns>List of skill configurations.</returns>
    private List<SkillConfiguration> GetSkills()
    {
        var skills = new List<SkillConfiguration>();
        var individualFunctionsSkill = GetIndividualFunctionsSkill();
        if ( individualFunctionsSkill != null )
        {
            skills.Add( individualFunctionsSkill );
        }

        skills.AddRange( _skills );
        return skills;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Get a random long value using a cryptographically secure random number generator.
    /// </summary>
    /// <returns>A random number.</returns>
    private static long GetRandomLong()
    {
        var buffer = new byte[8];
        using ( var rng = System.Security.Cryptography.RandomNumberGenerator.Create() )
        {
            rng.GetBytes( buffer );
        }

        return BitConverter.ToInt64( buffer, 0 );
    }

    #endregion
}