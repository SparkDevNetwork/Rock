using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.AI.Agent;
using Rock.Tests.Shared;

namespace Rock.AI.Agent.Tests;

/**
 * NOTE: These tests are meant to verify the call structure to the language 
 * model. They are NOT meant to verify we get the exact right response back.
 * This is why we have multiple possible expected outputs for each test. We
 * do check the output to make sure that it passed at least SOME data to the
 * function, but we don't care exactly what it is as long as it doesn't come
 * back completely blank.
 */

[TestClass]
[MethodIgnoreIf( nameof( HasRequiredConfiguration ), "Missing configuration settings in app.TestSettings.config file." )]
[MethodIgnoreIf( nameof( TestsAreDisabled ), "Test disabled in in app.TestSettings.config file." )]
[Ignore( "These tests are used for local testing of function calls. They are not part of the normal testing process." )]
public class FunctionCallTests : BaseFunctionCallTests
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
    [DataRow( 1234L, "search jason j", new[] { "jason j" } )]
    [DataRow( 1234L, "search for the person entity with a name like jason j", new[] { "jason j" } )]
    [DataRow( 1234L, "search for the decker group", new[] { "decker group" } )]
    public async Task FunctionSingleParameter_IsCalledOnce( long? seed, string prompt, string[] expectedOutput )
    {
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "searchTerm",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'.",
                IsRequired = true,
            }
        };

        var function = new AgentTool
        {
            Name = "Search",
            Instructions = new ToolInstructionSettings
            {
                Purposes = ["This function searches the database for people, groups, content channels that match the query from the user."],
                Usages = ["This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages."]
            },
            FunctionType = FunctionType.ExecuteLava,
            Prompt = "{% output %}{{ searchTerm }}{% endoutput %}No Results Found.",
            Parameters = parameters,
            Temperature = 0,
            MaxTokens = 128,
        };

        var (chat, output, logs) = ConfigureChatAgent( seed, function );

        await chat.AddMessageAsync( AuthorRole.User, prompt );
        _ = await chat.GetChatMessageResponseAsync();

        // That no function calls failed and that we had one succeed.
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

        // Ensure the output data is valid.
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        if ( !expectedOutput.Contains( output[0] ) )
        {
            Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
        }
    }

    [ConditionalTestMethod]
    [DataRow( 1234L, "search jason j", new[] { "jason/j//", "/jason/j/", "jason j/Jason/J/", "jason j/Jason//" } )]
    [DataRow( 1234L, "search for the person entity with a name like jason j", new[] { "jason j/jason/j/", "/jason/j/", "jason j/Jason/J/", "/Jason/J/" } )]
    [DataRow( 1234L, "search for the decker group", new[] { "///decker", "decker group///decker group", "///Decker Group", "///decker group" } )]
    public async Task FunctionFourParameter_IsCalledOnce( long? seed, string prompt, string[] expectedOutput )
    {
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "searchTerm",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "firstName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The first or nick name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "lastName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The last name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "groupName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The name of the group. Leave blank if empty.",
                IsRequired = false,
            }
        };

        var function = new AgentTool
        {
            Name = "Search",
            Instructions = new ToolInstructionSettings
            {
                Purposes = ["This function searches the database for people, groups, content channels that match the query from the user."],
                Usages = ["This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages."]
            },
            FunctionType = FunctionType.ExecuteLava,
            Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}{% endoutput %}No Results Found.",
            Parameters = parameters,
            Temperature = 0,
            MaxTokens = 128,
        };

        var (chat, output, logs) = ConfigureChatAgent( seed, function );

        await chat.AddMessageAsync( AuthorRole.User, prompt );
        _ = await chat.GetChatMessageResponseAsync();

        // That no function calls failed and that we had one succeed.
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

        // Ensure the output data is valid.
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        if ( !expectedOutput.Contains( output[0] ) )
        {
            Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
        }
    }

    [ConditionalTestMethod]
    [DataRow( 1234L, "search jason j", new[] { "/jason/j//person", "jason j////", "jason j/Jason/J//person", "/Jason/J//person" } )]
    [DataRow( 1234L, "search for the person entity with a name like jason j", new[] { "/jason/j//person", "jason j/Jason/J//person", "jason j/jason/j//person" } )]
    [DataRow( 1234L, "search for the decker group", new[] { "///decker/group", "decker group///decker/group", "decker group///Decker Group/group", "decker group///decker group/group" } )]
    [DataRow( 1234L, "search for decker as either a person entity or group entity", new[] { "decker////person,group" } )]
    public async Task FunctionWithArrayParameter_IsCalledOnce( long? seed, string prompt, string[] expectedOutput )
    {
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "entityTypes",
                DataType = ParameterSchemaDataType.String,
                Instructions = "Pass an array of strings for the type of entities to search for, such as 'person', 'group' or 'content channel'. Leave empty if not known, but make a good effort to fill this in based on the request.",
                IsRequired = false,
                IsCollection = true,
                AllowedValues = ["person", "group", "content channel"]
            },
            new ParameterSchema
            {
                Name = "searchTerm",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "firstName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The first or nick name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "lastName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The last name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "groupName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The name of the group. Leave blank if empty.",
                IsRequired = false,
            }
        };

        var function = new AgentTool
        {
            Name = "Search",
            Instructions = new ToolInstructionSettings
            {
                Purposes = ["This function searches the database for people, groups, content channels that match the query from the user."],
                Usages = ["This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages."]
            },
            FunctionType = FunctionType.ExecuteLava,
            Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}/{{ entityTypes | Join:',' }}{% endoutput %}No Results Found.",
            Parameters = parameters,
            Temperature = 0,
            MaxTokens = 128,
        };

        var (chat, output, logs) = ConfigureChatAgent( seed, function );

        await chat.AddMessageAsync( AuthorRole.User, prompt );
        _ = await chat.GetChatMessageResponseAsync();

        // That no function calls failed and that we had one succeed.
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

        // Ensure the output data is valid.
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        if ( !expectedOutput.Contains( output[0] ) )
        {
            Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
        }
    }

    [ConditionalTestMethod]
    [DataRow( 1234L, "search jason j", new[] { "/jason/j//", "/j/j//", "/jason/j//person", "jason j/Jason/J//person" } )]
    [DataRow( 1234L, "search for the person entity with a name like jason j", new[] { "/jason/j//person", "jason j/jason/j//person", "jason j/Jason/J//person" } )]
    [DataRow( 1234L, "search for the decker group", new[] { "///decker/", "///decker/group", "decker///decker/group", "///decker group/group" } )]
    [DataRow( 1234L, "search for decker as either a person entity or group entity", new[] { "decker////person,group" } )]
    public async Task FunctionWithCommaDelimitedParameter_IsCalledOnce( long? seed, string prompt, string[] expectedOutput )
    {
        var parameters = new List<ParameterSchema>
        {
            new ParameterSchema
            {
                Name = "entityTypes",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The type of entities to search for as a comma separated string, such as 'person', 'group' or 'content channel'. Leave empty if not known, but make a good effort to fill this in based on the request. CRITICAL: If multiple values are to be used, then they must all be passed in a single function call. Never call this function twice because of multiple values.",
                IsRequired = false,
                //AllowedValues = ["person", "group", "content channel"]
            },
            new ParameterSchema
            {
                Name = "searchTerm",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The term the user wants to search for. For example: 't dec', 'michaels', 'a marble'. For example, if searching for an entity type of person, then 'firstName' and 'lastName' are preferred this should be blank.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "firstName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The first or nick name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "lastName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The last name the person was looking for. Leave blank if they did not specify a value.",
                IsRequired = false,
            },
            new ParameterSchema
            {
                Name = "groupName",
                DataType = ParameterSchemaDataType.String,
                Instructions = "The name of the group. Leave blank if empty.",
                IsRequired = false,
            }
        };

        var function = new AgentTool
        {
            Name = "Search",
            Instructions = new ToolInstructionSettings
            {
                Purposes = ["This function searches the database for people, groups, content channels that match the query from the user."],
                Usages = ["This function must be called if the user is trying to search for an entity. Do not attempt to infer the data from previous messages."]
            },
            FunctionType = FunctionType.ExecuteLava,
            Prompt = "{% output %}{{ searchTerm }}/{{ firstName }}/{{ lastName }}/{{ groupName }}/{{ entityTypes }}{% endoutput %}No Results Found.",
            Parameters = parameters,
            Temperature = 0,
            MaxTokens = 128,
        };

        var (chat, output, logs) = ConfigureChatAgent( seed, function );

        await chat.AddMessageAsync( AuthorRole.User, prompt );
        _ = await chat.GetChatMessageResponseAsync();

        // That no function calls failed and that we had one succeed.
        Assert.That.AreEqual( 0, logs.Count( l => l.Contains( "Function Test-Search failed." ) ), "Function call failed." );
        Assert.That.AreEqual( 1, logs.Count( l => l == "Function Test-Search succeeded." ), "Multiple successful invocations may have been detected." );

        // Ensure the output data is valid.
        Assert.That.AreEqual( 1, output.Count, "Multiple output messages were logged." );
        if ( !expectedOutput.Contains( output[0] ) )
        {
            Assert.Fail( $"Expected one of {expectedOutput.Select( s => $"<{s}>" ).JoinStrings( "," )} but got <{output[0]}>." );
        }
    }
}
