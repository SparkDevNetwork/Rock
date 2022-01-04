using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Apps.StatementGenerator
{
    public class ConfigSettingsStringConverter : JsonConverter
    {
        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof( JTokenType );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            var token = JToken.Load( reader );
            if (token.Type == JTokenType.Object)
            {
                return token.ToString();
            }
            return null;
        }

        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var token = JToken.Parse( value.ToString() );
            writer.WriteToken( token.CreateReader() );
        }
    }
}
