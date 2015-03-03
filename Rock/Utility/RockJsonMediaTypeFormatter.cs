// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Text;

namespace Rock.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class RockJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        /// <summary>
        /// Gets or sets a value indicating whether [load attributes].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [load attributes]; otherwise, <c>false</c>.
        /// </value>
        private bool LoadAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [serialize in simple mode].
        /// </summary>
        /// <value>
        /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
        /// </value>
        private bool SerializeInSimpleMode { get; set; }

        /// <summary>
        /// Returns a specialized instance of the <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" /> that can format a response for the given parameters.
        /// </summary>
        /// <param name="type">The type to format.</param>
        /// <param name="request">The request.</param>
        /// <param name="mediaType">The media type.</param>
        /// <returns>
        /// Returns <see cref="T:System.Net.Http.Formatting.MediaTypeFormatter" />.
        /// </returns>
        public override MediaTypeFormatter GetPerRequestFormatterInstance( Type type, System.Net.Http.HttpRequestMessage request, System.Net.Http.Headers.MediaTypeHeaderValue mediaType )
        {
            var qryParams = System.Web.HttpUtility.ParseQueryString( request.RequestUri.Query );
            string loadAttributes = qryParams["LoadAttributes"] ?? string.Empty;

            // if "simple" or True is specified in the LoadAttributes param, tell the formatter to serialize in Simple mode
            SerializeInSimpleMode = loadAttributes.Equals( "simple", StringComparison.OrdinalIgnoreCase ) || ( loadAttributes.AsBooleanOrNull() ?? false );

            // if either "simple", "expanded", or True is specified in the LoadAttributes param, tell the formatter to load the attributes on the way out
            LoadAttributes = SerializeInSimpleMode || loadAttributes.Equals( "expanded", StringComparison.OrdinalIgnoreCase );

            return base.GetPerRequestFormatterInstance( type, request, mediaType );
        }

        /// <summary>
        /// Called during serialization to write an object of the specified type to the specified stream.
        /// </summary>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="value">The object to write.</param>
        /// <param name="writeStream">The stream to write to.</param>
        /// <param name="effectiveEncoding">The encoding to use when writing.</param>
        public override void WriteToStream( Type type, object value, System.IO.Stream writeStream, Encoding effectiveEncoding )
        {
            // query should be filtered by now, so iterate thru items and load attributes before the response is serialized
            if ( LoadAttributes )
            {
                if ( value is IEnumerable<Rock.Attribute.IHasAttributes> )
                {
                    var rockContext = new Rock.Data.RockContext();

                    // if the REST call specified that Attributes should be loaded and we are returning a list of IHasAttributes..
                    foreach ( var item in value as IEnumerable<Rock.Attribute.IHasAttributes> )
                    {
                        item.LoadAttributes( rockContext );
                    }
                }
                else if (value is Rock.Attribute.IHasAttributes)
                {
                    var rockContext = new Rock.Data.RockContext();
                    
                    // if the REST call specified that Attributes should be loaded and we are returning a single IHasAttributes..
                    (value as Rock.Attribute.IHasAttributes).LoadAttributes( rockContext );
                }
            }

            base.WriteToStream( type, value, writeStream, effectiveEncoding );
        }

        /// <summary>
        /// Called during serialization to get the <see cref="T:Newtonsoft.Json.JsonWriter" />.
        /// </summary>
        /// <param name="type">The type of the object to write.</param>
        /// <param name="writeStream">The stream to write to.</param>
        /// <param name="effectiveEncoding">The encoding to use when writing.</param>
        /// <returns>
        /// The writer to use during serialization.
        /// </returns>
        public override Newtonsoft.Json.JsonWriter CreateJsonWriter( Type type, System.IO.Stream writeStream, Encoding effectiveEncoding )
        {
            if ( LoadAttributes && SerializeInSimpleMode )
            {
                // Use the RockJsonTextWriter when we need to Serialize Model.AttributeValues and Model.Attributes in simple mode
                return new RockJsonTextWriter( new System.IO.StreamWriter( writeStream ), this.SerializeInSimpleMode );
            }
            else
            {
                return base.CreateJsonWriter( type, writeStream, effectiveEncoding );
            }
        }
    }
}
