using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Rock.AI.Agent.Mcp;
using Rock.Attribute;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;
using Rock.Web.UI;

namespace Rock.Blocks.Types.Mobile.Cms
{
    /// <summary>
    /// A voice agent that help you with you everyday task.
    /// </summary>
    [DisplayName( "Voice Agent" )]
    [Category( "Mobile > Cms" )]
    [Description( "A voice agent that help you with you everyday task." )]
    [IconCssClass( "ti ti-brain" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]


    [TextField( "OpenAI API Key",
        Description = "The API key obtained from the OpenAI developer portal.",
        IsRequired = true,
        DefaultValue = "",
        Key = AttributeKeys.ApiKey,
        Order = 0 )]

    [TextField( "OpenAI Model",
        Description = "The realtime OpenAI model used for audio interactions.",
        IsRequired = true,
        DefaultValue = "gpt-realtime-mini",
        Key = AttributeKeys.Model,
        Order = 1 )]

    [MemoField( "Instruction",
        Description = "Instructions that define how the AI assistant should behave during conversations.",
        IsRequired = false,
        DefaultValue = @"
                  You are a helpful voice assistant.
                    # Tool behavior (very important)
                    - Before ANY tool call, say ONE short natural line out loud like:
                      “I’m checking that now.”
                      “One moment while I look that up.”
                      “Let me pull that up for you.”
                      “Just a second…”
                    - Then immediately call the tool. Do not ask for confirmation first.
                    - After you get the tool result, continue the conversation naturally.
                    (Keep the rest of your normal personality/instructions here...)
                ",
        Key = AttributeKeys.Instruction,
        AllowHtml = true,
        Order = 2 )]

    [CustomDropdownListField( "Rock MCP",
        Description = "Select an MCP agent configured in Rock. This agent will be available to the AI assistant.",
        ListSource = "SELECT [Guid] AS [Value], [Name] AS [Text] FROM [AIAgent] WHERE [AgentType] = 1",
        IsRequired = false,
        Key = AttributeKeys.RockMcp,
        Order = 3 )]

    [ValueListField( "External MCP",
        Description = "Enter one or more MCP server URLs from external systems that should be available to the AI agent.",
        IsRequired = false,
        Key = AttributeKeys.ExternalMcp,
        Order = 4 )]


    [Rock.SystemGuid.EntityTypeGuid( "8654b230-5868-4490-8832-61dbdd1fd6d4" )]
    [Rock.SystemGuid.BlockTypeGuid( "64B2A7B9-0C52-4C03-80DE-A9ABDD213206" )]
    public class VoiceAgent : RockBlockType
    {

        #region Attributes key

        private static class AttributeKeys
        {
            public const string ApiKey = "ApiKey";
            public const string Model = "Model";
            public const string Instruction = "Instruction";
            public const string RockMcp = "RockMcp";
            public const string ExternalMcp = "ExternalMcp";
        }

        #endregion

        #region Private Constants

        private const string OpenAiRealtimeClientSecretUrl = "https://api.openai.com/v1/realtime/client_secrets";

        #endregion

        #region Methods

        private string GetRockMcpUrl( string slug )
        {
            var baseUrl = GlobalAttributesCache.Value( "PublicApplicationRoot" ).TrimEnd( '/' );
            var mcpUrl = $"{baseUrl}/api/v2/mcp/{slug}";

            return mcpUrl;
        }

        /// <summary>
        /// Requests an ephemeral session token from the OpenAI Realtime API so the client
        /// never handles the real API key directly.
        /// </summary>
        /// <returns>An object containing the ephemeral <c>client_secret</c> token, or an error response.</returns>
        private async Task<string> GetEphemeralKey()
        {
            var apiKey = GetAttributeValue( AttributeKeys.ApiKey );
            var model = GetAttributeValue( AttributeKeys.Model );
            var instruction = GetAttributeValue( AttributeKeys.Instruction );

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                using ( var httpClient = new HttpClient() )
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey );
                    httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

                    var payload = new
                    {
                        expires_after = new
                        {
                            anchor = "created_at",
                            seconds = 300
                        },
                        session = new
                        {
                            type = "realtime",
                            model = model,
                            instructions = instruction
                        }
                    };
                    var json = JsonConvert.SerializeObject( payload );
                    var content = new StringContent( json, Encoding.UTF8, "application/json" );

                    var response = await httpClient.PostAsync( OpenAiRealtimeClientSecretUrl, content );
                    var responseBody = await response.Content.ReadAsStringAsync();

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }

                    var sessionData = JsonConvert.DeserializeObject<dynamic>( responseBody );

                    return ( string ) sessionData?.value;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Block Actions



        /// <summary>
        /// Gets the initial data for the block, including any configured MCP URLs.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public async Task<BlockActionResult> GetInitialData()
        {
            var urls = new List<string>();

            var aiAgentSlug = AIAgentCache.Get( GetAttributeValue( AttributeKeys.RockMcp ) )?
                .GetAdditionalSettings<McpAgentSettings>()?.Slug;
            if ( aiAgentSlug != null )
            {
                urls.Add( GetRockMcpUrl( aiAgentSlug ) );
            }

            var additionalMcpUrl = GetAttributeValue( AttributeKeys.ExternalMcp ).SplitDelimitedValues( "|", System.StringSplitOptions.RemoveEmptyEntries );
            urls.AddRange( additionalMcpUrl );

            var initialData = new VoiceAgentInitialData
            {
                EphemeralKey = await GetEphemeralKey(),
                McpUrl = urls
            };

            return ActionOk( initialData );
        }

        #endregion

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Cms.VoiceAgent.Configuration
            {
                Model = GetAttributeValue( AttributeKeys.Model ),
                Instruction = GetAttributeValue( AttributeKeys.Instruction )
            };
        }

    }

    public class VoiceAgentInitialData
    {
        public string EphemeralKey { get; set; }
        public List<string> McpUrl { get; set; }
    }
}
