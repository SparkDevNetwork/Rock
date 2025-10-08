using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Mcp;
using Rock.AI.Agent.Mcp.Protocol;
using Rock.Enums.AI.Agent;
using Rock.SystemGuid;
using Rock.Tests.Shared.TestFramework;

namespace Rock.AI.Agent.Tests.Mcp
{
    [TestClass]
    public class McpServerTests : MockDatabaseTestsBase
    {
        private readonly JsonSerializerOptions _serializerOptions = AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Public );

        #region HandleRequest Tests

        [TestMethod]
        public async Task HandleRequest_WithNullAgent_ThrowsException()
        {
            var mcp = new McpServer();
            var request = new McpRequest
            {
                Content = JsonRpcRequestTests.ToStream( "" )
            };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>( async () => await mcp.HandleRequestAsync( null, request, CancellationToken.None ) );
        }

        [TestMethod]
        public async Task HandleRequest_WithMissingId_ReturnsEmptyResponse()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var request = new McpRequest
            {
                Content = JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"method\":\"initialize\"}" )
            };

            var response = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, request, CancellationToken.None );

            Assert.IsNotNull( response );
            Assert.IsNull( response.Content );
        }

        [TestMethod]
        public async Task HandleRequest_WithNotification_ReturnsEmptyResponse()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var request = new McpRequest
            {
                Content = JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"notifications/test\"}" )
            };

            var response = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, request, CancellationToken.None );

            Assert.IsNotNull( response );
            Assert.IsNull( response.Content );
        }

        [TestMethod]
        public async Task HandleRequest_WithNonNotification_ReturnsResponse()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var request = new McpRequest
            {
                Content = JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\"}" )
            };

            var response = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, request, CancellationToken.None );

            Assert.IsNotNull( response );
            Assert.IsNotNull( response.Content );
        }

        [TestMethod]
        public async Task HandleRequest_WithUnknownMethod_ReturnsError()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"unknowntest\"}" ), _serializerOptions );

            var response = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( response );
            Assert.IsNotNull( response.Error );
            Assert.AreEqual( JsonRpcErrorCode.MethodNotFound, response.Error.Code );
        }

        #endregion

        #region Initialize Tests

        [TestMethod]
        public async Task Initialize_WithoutParameters_ReturnsSuccess()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"initialize\"}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNull( rpcResult.Error );
        }

        [TestMethod]
        public async Task Initialize_IncludesToolsCapability()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"initialize\", \"params\":{}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( InitializeResult ) rpcResult.Result;

            Assert.IsTrue( result.Capabilities.ContainsKey( "tools" ) );
        }

        [TestMethod]
        public async Task Initialize_WithProtocolVersion20250618_ReturnsMatchedVersion()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"initialize\", \"params\":{\"protocolVersion\":\"2025-06-18\"}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( InitializeResult ) rpcResult.Result;

            Assert.AreEqual( "2025-06-18", result.ProtocolVersion );
        }

        [TestMethod]
        public async Task Initialize_WithProtocolVersion20250326_ReturnsMatchedVersion()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"initialize\", \"params\":{\"protocolVersion\":\"2025-03-26\"}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( InitializeResult ) rpcResult.Result;

            Assert.AreEqual( "2025-03-26", result.ProtocolVersion );
        }

        [TestMethod]
        public async Task Initialize_WithSkillInstructions_IncludesInstructions()
        {
            var expectedInstructions = "Custom Test Instructions";
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration( expectedInstructions ) )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"initialize\", \"params\":{\"protocolVersion\":\"2025-03-26\"}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( InitializeResult ) rpcResult.Result;

            Assert.Contains( expectedInstructions, result.Instructions );
        }

        #endregion

        #region Ping Tests

        [TestMethod]
        public async Task Ping_WithoutParameters_ReturnsSuccess()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"ping\"}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNull( rpcResult.Error );
        }

        #endregion

        #region ToolsList Tests

        [TestMethod]
        public async Task ToolsList_WithoutParameters_ReturnsSuccess()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/list\"}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNull( rpcResult.Error );
        }

        [TestMethod]
        public async Task ToolsList_UsesDoubleUnderscoreAsSeparator()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithFunction( new AgentTool
                {
                    Name = "TestTool",
                    ToolType = ToolType.ExecuteLava
                } )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/list\", \"params\":{}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( ListToolsResult ) rpcResult.Result;

            Assert.AreEqual( 1, result.Tools.Count );
            Assert.AreEqual( "IndividualFunctions__TestTool", result.Tools[0].Name );
        }

        #endregion

        #region ToolsCall Tests

        [TestMethod]
        public async Task ToolsCall_WithoutParameters_ReturnsError()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\"}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Error );

            Assert.AreEqual( JsonRpcErrorCode.InvalidParams, rpcResult.Error.Code );
        }

        [TestMethod]
        public async Task ToolsCall_WithInvalidToolName_ReturnsError()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestTool\"}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Error );

            Assert.AreEqual( JsonRpcErrorCode.InvalidParams, rpcResult.Error.Code );
            Assert.Contains( "Tool name", rpcResult.Error.Message );
        }

        [TestMethod]
        public async Task ToolsCall_WithoutArguments_ReturnsError()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder().Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"IndividualFunctions__TestTool\"}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Error );

            Assert.AreEqual( JsonRpcErrorCode.InvalidParams, rpcResult.Error.Code );
            Assert.Contains( "arguments", rpcResult.Error.Message );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolReturningString_IncludesUnstructuredContent()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__StringTool\", \"arguments\": {}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.AreEqual( 1, result.Content.Count );
            Assert.AreEqual( "text", result.Content[0].Type );
            Assert.AreEqual( "String Tool", result.Content[0].Text );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolReturningClass_IncludesUnstructuredContent()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__StructuredTool\", \"arguments\": {}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.AreEqual( 1, result.Content.Count );

            var element = JsonSerializer.Deserialize<JsonElement>( result.Content[0].Text );

            Assert.AreEqual( "Tool", element.GetProperty( "Structured" ).GetString() );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolReturningClass_IncludesStructuredContent()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__StructuredTool\", \"arguments\": {}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.IsNotNull( result.StructuredContent );
            Assert.IsInstanceOfType( result.StructuredContent, typeof( Dictionary<string, string> ) );

            var dictionary = ( Dictionary<string, string> ) result.StructuredContent;

            Assert.IsTrue( dictionary.ContainsKey( "Structured" ) );
            Assert.AreEqual( "Tool", dictionary["Structured"] );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolWithIntegerEnum_ParsesEnum()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__EnumToString\", \"arguments\": {\"agentType\": 1}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.AreEqual( AgentType.Mcp.ToString(), result.Content[0].Text );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolWithNamedEnum_ParsesEnum()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__EnumToString\", \"arguments\": {\"agentType\": \"Mcp\"}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.AreEqual( AgentType.Mcp.ToString(), result.Content[0].Text );
        }

        [TestMethod]
        public async Task ToolsCall_CallingToolWithNullString_CallsMethodWithNull()
        {
            var mcp = new McpServer();
            var agent = new AgentBuilder()
                .WithSkill( GetSkillConfiguration() )
                .Build();
            var rpcRequest = new JsonRpcRequest( JsonRpcRequestTests.ToStream( "{\"jsonrpc\":\"2.0\",\"id\": 1,\"method\":\"tools/call\", \"params\":{\"name\": \"TestSkill__Echo\", \"arguments\": {\"input\": null}}}" ), _serializerOptions );

            var rpcResult = await mcp.HandleRequestAsync( ( ChatAgent ) agent.Agent, rpcRequest, _serializerOptions, CancellationToken.None );

            Assert.IsNotNull( rpcResult );
            Assert.IsNotNull( rpcResult.Result );

            var result = ( CallToolResult ) rpcResult.Result;

            Assert.IsNull( result.StructuredContent );
        }

        #endregion

        #region Support

        private SkillConfiguration GetSkillConfiguration( string purpose = null )
        {
            var tools = new List<AgentTool>
            {
                new AgentTool
                {
                    Guid = new Guid("7dd480ca-a1d3-445c-9064-b75748deb6e3" ),
                    Name = "String Tool",
                    ToolType = ToolType.ExecuteCode
                },
                new AgentTool
                {
                    Guid = new Guid( "e4802a5e-6d6c-450e-abb0-dab0f65cab0e" ),
                    Name = "Structured Tool",
                    ToolType = ToolType.ExecuteCode
                },
                new AgentTool
                {
                    Guid = new Guid( "9ace29b2-a1fa-497a-9743-6daf076b5181" ),
                    Name = "Enum To String",
                    ToolType = ToolType.ExecuteCode
                },
                new AgentTool
                {
                    Guid = new Guid( "dd15667a-4caf-4f77-b73d-b2648bbd933a" ),
                    Name = "Echo",
                    ToolType = ToolType.ExecuteCode
                }
            };

            var instructionSettings = new SkillInstructionSettings();

            if ( purpose != null )
            {
                instructionSettings.Purposes.Add( purpose );
            }

            return new SkillConfiguration(
                "TestSkill",
                instructionSettings,
                tools,
                typeof( TestSkill ),
                new AgentSkillSettings() );
        }

        private class TestSkill : AgentSkillComponent
        {
            [AgentToolGuid( "7dd480ca-a1d3-445c-9064-b75748deb6e3" )]
            public string StringTool()
            {
                return "String Tool";
            }

            [AgentToolGuid( "e4802a5e-6d6c-450e-abb0-dab0f65cab0e" )]
            public Dictionary<string, string> StructuredTool()
            {
                return new Dictionary<string, string>
                {
                    ["Structured"] = "Tool"
                };
            }

            [AgentToolGuid( "9ace29b2-a1fa-497a-9743-6daf076b5181" )]
            public string EnumToString( AgentType agentType )
            {
                return agentType.ToString();
            }

            [AgentToolGuid( "dd15667a-4caf-4f77-b73d-b2648bbd933a" )]
            public string Echo( string input )
            {
                return input;
            }
        }

        #endregion
    }
}
