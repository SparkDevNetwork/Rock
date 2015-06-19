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
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using Rock.Web.Cache;

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
        /// Gets or sets the person that initiated the REST request
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        private Rock.Model.Person Person { get; set; }

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

            // NOTE: request.Properties["Person"] gets set in Rock.Rest.Filters.SecurityAttribute.OnActionExecuting
            this.Person = request.Properties["Person"] as Rock.Model.Person;

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
            IEnumerable<Attribute.IHasAttributes> items = null;

            // query should be filtered by now, so iterate thru items and load attributes before the response is serialized
            if ( LoadAttributes )
            {
                if ( value is IEnumerable<Rock.Attribute.IHasAttributes> )
                {
                    // if the REST call specified that Attributes should be loaded and we are returning a list of IHasAttributes..
                    items = value as IEnumerable<Rock.Attribute.IHasAttributes>;
                }
                else if ( value is Rock.Attribute.IHasAttributes )
                {
                    // if the REST call specified that Attributes should be loaded and we are returning a single IHasAttributes..
                    items = new List<Attribute.IHasAttributes>( new Attribute.IHasAttributes[] { value as Rock.Attribute.IHasAttributes } );
                }

                if ( items != null )
                {
                    var rockContext = new Rock.Data.RockContext();
                    foreach ( var item in items )
                    {
                        Rock.Attribute.Helper.LoadAttributes( item, rockContext );
                    }

                    FilterAttributes( rockContext, items, this.Person );
                }
            }

            base.WriteToStream( type, value, writeStream, effectiveEncoding );
        }

        /// <summary>
        /// Filters the attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="items">The items.</param>
        /// <param name="person">The person.</param>
        private static void FilterAttributes( Data.RockContext rockContext, IEnumerable<Attribute.IHasAttributes> items, Rock.Model.Person person )
        {
            if ( !items.Any() )
            {
                return;
            }

            var itemType = items.First().GetType();

            var entityType = EntityTypeCache.Read( itemType );
            if ( entityType == null )
            {
                // shouldn't happen
                return;
            }

            var entityAttributes = AttributeCache.GetByEntity( entityType.Id );

            // only return attributes that the person has VIEW auth to
            foreach ( var entityAttribute in entityAttributes )
            {
                foreach ( var attributeId in entityAttribute.AttributeIds )
                {
                    var attribute = AttributeCache.Read( attributeId );
                    if ( !attribute.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
                    {
                        foreach ( var item in items )
                        {
                            if ( item.AttributeValues.ContainsKey( attribute.Key ) )
                            {
                                item.AttributeValues.Remove( attribute.Key );
                            }

                            if ( item.Attributes.ContainsKey( attribute.Key ) )
                            {
                                item.Attributes.Remove( attribute.Key );
                            }
                        }
                    }
                }
            }
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
