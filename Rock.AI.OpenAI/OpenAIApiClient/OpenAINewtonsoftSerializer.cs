using System.IO;

using Newtonsoft.Json;

using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace Rock.AI.OpenAI.OpenAIApiClient
{
    internal class OpenAINewtonsoftJsonSerializer : ISerializer, IDeserializer
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the OpenAINewtonsoftJsonSerializer using the provider serializer.
        /// </summary>
        /// <param name="serializer">The JsonSerializer to be wrapped for use by RestSharp.</param>
        public OpenAINewtonsoftJsonSerializer( Newtonsoft.Json.JsonSerializer serializer )
        {
            this.serializer = serializer;
        }

        public string ContentType
        {
            get { return "application/json"; }
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public string Serialize( object obj )
        {
            using ( var stringWriter = new StringWriter() )
            {
                using ( var jsonTextWriter = new JsonTextWriter( stringWriter ) )
                {
                    serializer.Serialize( jsonTextWriter, obj );

                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Deserializes the response content using the serializer provided to the constructor.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="response">The response whose content should be deserialized.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>( RestSharp.IRestResponse response )
        {
            var content = response.Content;

            using ( var stringReader = new StringReader( content ) )
            {
                using ( var jsonTextReader = new JsonTextReader( stringReader ) )
                {
                    return serializer.Deserialize<T>( jsonTextReader );
                }
            }
        }

        public static OpenAINewtonsoftJsonSerializer Default
        {
            get
            {
                return new OpenAINewtonsoftJsonSerializer( new Newtonsoft.Json.JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                } );
            }
        }
    }
}
