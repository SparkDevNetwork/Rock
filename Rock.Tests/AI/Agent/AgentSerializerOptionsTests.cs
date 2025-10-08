using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent;
using Rock.AI.Agent.Annotations;
using Rock.Enums.AI.Agent;

namespace Rock.Tests.AI.Agent
{
    [TestClass]
    public class AgentSerializerOptionsTests
    {
        #region JsonIgnoreAgentTypeAttribute

        [TestMethod]
        public void PropertyExcludedForChat_WithChatAgent_ExcludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsFalse( jObject.ContainsKey( "excludedForChat" ) );
        }

        [TestMethod]
        public void PropertyExcludedForChat_WithMcpAgent_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "excludedForChat" ) );
        }

        [TestMethod]
        public void PropertyExcludedForMcp_WithChatAgent_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "excludedForMcp" ) );
        }

        [TestMethod]
        public void PropertyExcludedForMcp_WithMcpAgent_ExcludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsFalse( jObject.ContainsKey( "excludedForMcp" ) );
        }

        [TestMethod]
        public void PropertyNotExcluded_WithChatAgent_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "includedForAll" ) );
        }

        [TestMethod]
        public void PropertyNotExcluded_WithMcpAgent_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Mcp, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "includedForAll" ) );
        }

        #endregion

        #region JsonIgnoreAudienceTypeAttribute

        [TestMethod]
        public void PropertyExcludedForInternal_WithInternalAudience_ExcludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsFalse( jObject.ContainsKey( "excludedForInternal" ) );
        }

        [TestMethod]
        public void PropertyExcludedForInternal_WithPublicAudience_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Public );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "excludedForInternal" ) );
        }

        [TestMethod]
        public void PropertyExcludedForPublic_WithInternalAudience_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "excludedForPublic" ) );
        }

        [TestMethod]
        public void PropertyExcludedForPublic_WithPublicAudience_ExcludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Public );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsFalse( jObject.ContainsKey( "excludedForPublic" ) );
        }

        [TestMethod]
        public void PropertyNotExcluded_WithInternalAudience_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "includedForAll" ) );
        }

        [TestMethod]
        public void PropertyNotExcluded_WithPublicAudience_IncludesProperty()
        {
            var poco = new TestPoco();
            var serializerSettings = AgentSerializerOptions.GetOptions( AgentType.Chat, AudienceType.Internal );

            var json = JsonSerializer.Serialize( poco, serializerSettings );

            var jObject = JsonSerializer.Deserialize<JsonObject>( json );

            Assert.IsTrue( jObject.ContainsKey( "includedForAll" ) );
        }

        #endregion

        private class TestPoco
        {
            [JsonIgnoreAgentType( AgentType.Chat )]
            public int ExcludedForChat { get; set; } = 1;

            [JsonIgnoreAgentType( AgentType.Mcp )]
            public int ExcludedForMcp { get; set; } = 2;

            [JsonIgnoreAudienceType( AudienceType.Internal )]
            public int ExcludedForInternal { get; set; } = 3;

            [JsonIgnoreAudienceType( AudienceType.Public )]
            public int ExcludedForPublic { get; set; } = 4;

            public int IncludedForAll { get; set; } = 3;
        }
    }
}
