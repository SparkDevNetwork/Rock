using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes
{
    /// <summary>
    /// Represents the result of a function call.
    /// </summary>
    internal sealed class ToolResultContent
    {
        // Reusable serializer options for this type
        private static readonly JsonSerializerOptions s_options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,                  // emits camelCase: callId, pluginName, etc.
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,       // skip nulls
            WriteIndented = false
        };

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string CallId { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string PluginName { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string FunctionName { get; }

        // NOTE: Complex JSON will deserialize to JsonElement when the type is object.
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public object Result { get; }

        [JsonConstructor]
        public ToolResultContent(
            string functionName = null,
            string pluginName = null,
            string callId = null,
            object result = null )
        {
            FunctionName = functionName;
            PluginName = pluginName;
            CallId = callId;
            Result = result;
        }

        /// <summary>
        /// Serialize this instance to JSON using System.Text.Json.
        /// </summary>
        public string ToJson( bool indented = false )
        {
            if ( indented )
            {
                var pretty = new JsonSerializerOptions( s_options ) { WriteIndented = true };
                return JsonSerializer.Serialize( this, pretty );
            }

            return JsonSerializer.Serialize( this, s_options );
        }

        /// <summary>
        /// Deserialize JSON into a ToolResultContent using System.Text.Json.
        /// </summary>
        public static ToolResultContent FromJson( string json )
        {
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                throw new ArgumentException( "JSON is null/empty.", nameof( json ) );
            }

            var value = JsonSerializer.Deserialize<ToolResultContent>( json, s_options );
            if ( value is null )
            {
                throw new JsonException( "Deserialization produced null ToolResultContent." );
            }

            return value;
        }

        /// <summary>
        /// Convenience helper: get Result as a specific type. If Result is a JsonElement, it will be deserialized.
        /// </summary>
        public T GetResult<T>()
        {
            if ( Result is null ) return default;

            if ( Result is JsonElement je )
            {
                // Handle the common case where 'result' was an object/array in JSON.
                return je.Deserialize<T>( s_options );
            }

            // If it's already the right type (e.g., string/bool/number), try a direct cast.
            if ( Result is T typed ) return typed;

            // Last resort: serialize then re-deserialize (covers boxed primitives, etc.)
            var reJson = JsonSerializer.Serialize( Result, s_options );
            return JsonSerializer.Deserialize<T>( reJson, s_options );
        }
    }
}