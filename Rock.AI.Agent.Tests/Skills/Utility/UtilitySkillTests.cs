using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Core.AI.Agent;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Skills.Utility;

[TestClass]
[MethodIgnoreIf( nameof( HasRequiredConfiguration ), "Missing configuration settings in app.TestSettings.config file." )]
[MethodIgnoreIf( nameof( TestsAreDisabled ), "Test disabled in in app.TestSettings.config file." )]
public class UtilitySkillTests : MockDatabaseTestsBase
{
    /// <summary>
    /// Checks if the required configuration settings for Azure OpenAI are present.
    /// </summary>
    /// <returns><c>true</c> if the configuration is valid; <c>false</c> otherwise.</returns>
    public static bool HasRequiredConfiguration()
    {
        return !string.IsNullOrWhiteSpace( ConfigurationManager.AppSettings["AzureOpenAIApiKey"] )
            && !string.IsNullOrWhiteSpace( ConfigurationManager.AppSettings["AzureOpenAIEndpoint"] );
    }

    /// <summary>
    /// Checks if the Azure Open AI tests are disabled.
    /// </summary>
    /// <returns><c>true</c> if the tests are disabled; otherwise <c>false</c>.</returns>
    public static bool TestsAreDisabled()
    {
        return !ConfigurationManager.AppSettings["SkipAzureOpenAI"].ToStringSafe().AsBoolean();
    }


    [ConditionalTestMethod]
    [DataRow( "Determine the date range for this week", new[] { "this week" } )]
    [DataRow( "Show me the date range for last 3 months", new[] { "last 3 months" } )]
    [DataRow( "I want a range for March 14th to July 31st", new[] { "March 14th to July 31st" } )]

    public async Task DetermineDateRangeFunctionCall_WorksAsExpected( string prompt, string[] expectedQueries )
    {
        // Use AgentBuilder to collect both output and logs
        var (agent, output, logs) = new AgentBuilder()
            .WithSkill( GetMockUtilitySkill() )
            .Build();

        // Act: Run prompt through the agent
        await agent.AddMessageAsync( AuthorRole.User, prompt );
        _ = await agent.GetChatMessageResponseAsync();

        // Assert: Function call logs
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function UtilitySkill-DetermineDateRange failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function UtilitySkill-DetermineDateRange succeeded." ), "Multiple successful invocations may have been detected." );

        // Assert: Output contains the expected query string
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        if ( !expectedQueries.Contains( output[0] ) )
        {
            Assert.Fail( $"Expected one of {string.Join( ", ", expectedQueries.Select( x => $"<{x}>" ) )} but got <{output[0]}>." );
        }
    }

    [ConditionalTestMethod]
    [DataRow( "what is today", "2025-07-30T12:00:00" )]
    [DataRow( "what is the current date and time?", "2025-07-30T12:00:00" )]
    public async Task GetCurrentDateTimeFunctionCall_WorksAsExpected( string prompt, string expectedOutput )
    {
        // Arrange: Build the agent with mock utility skill
        var (agent, output, logs) = new AgentBuilder()
            .WithSkill( GetMockUtilitySkill() )
            .Build();

        // Act: Send prompt and process response
        await agent.AddMessageAsync( AuthorRole.User, prompt );
        _ = await agent.GetChatMessageResponseAsync();

        // Assert: The correct function was called and succeeded
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function UtilitySkill-GetCurrentDateTime failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function UtilitySkill-GetCurrentDateTime succeeded." ), "Multiple successful invocations may have been detected." );

        // Assert: Output contains the expected ISO datetime string
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        Assert.That.AreEqual( expectedOutput, output[0], $"Expected '{expectedOutput}' but got '{output[0]}'" );
    }

    #region Mock Skills / Functions

    /// <summary>
    /// Returns a mock UtilitySkill with both DetermineDateRange and GetCurrentDateTime functions.
    /// </summary>
    private SkillConfiguration GetMockUtilitySkill()
    {
        return new SkillConfiguration(
            "Utility Skill",
            "Used for a variety of standard functions, such as retrieving the current date or converting simple data types.",
            [ GetMockDetermineDateRangeFunction(), GetMockGetCurrentDateFunction() ]
        );
    }

    /// <summary>
    /// Returns a mock DetermineDateRange AgentFunction (for parameter/response tests).
    /// </summary>
    private AgentFunction GetMockDetermineDateRangeFunction()
    {
        return new AgentFunction
        {
            Name = "DetermineDateRange",
            UsageHint = "🎯 Purpose:\n1. Determines a date range from a natural language string.\n\n🧭 Usage Guidance:\n1. Use for extracting a start and end date from a user's query.",
            FunctionType = FunctionType.ExecuteLava,
            Prompt = "{% output %}{{ query }}{% endoutput %}{\r\n  \"StartDate\": \"2025-07-01T00:00:00\",\r\n  \"EndDate\": \"2025-07-31T00:00:00\"\r\n}",
            Parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "query",
                DataType = ParameterSchemaDataType.String,
                UsageHint = "A natural language string, such as 'last week', 'tomorrow', or 'March 1st to March 10th'.",
                IsRequired = true
            }
        },
            Temperature = 1,
            MaxTokens = 128
        };
    }

    /// <summary>
    /// Returns a mock GetCurrentDateTime AgentFunction (for parameter/response tests).
    /// </summary>
    private AgentFunction GetMockGetCurrentDateFunction()
    {
        return new AgentFunction
        {
            Name = "GetCurrentDateTime",
            UsageHint = "🎯 Purpose:\n1. Returns the current system date/time as an ISO string.",
            FunctionType = FunctionType.ExecuteLava,
            // Outputs the current date for testing (could be fixed or dynamic for a real test)
            Prompt = "{% output %}2025-07-30T12:00:00{% endoutput %}2025-07-30T12:00:00",
            Parameters = new List<ParameterSchema>(), // No parameters for "get current date/time"
            Temperature = 1,
            MaxTokens = 128
        };
    }

    #endregion
}
