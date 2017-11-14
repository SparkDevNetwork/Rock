// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data.Entity;
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
            if ( LoadAttributes && request.Properties.ContainsKey( "Person" ) )
            {
                this.Person = request.Properties["Person"] as Rock.Model.Person;
            }

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
                else if ( value is IQueryable )
                {
                    Type valueType = value.GetType();
                    if ( valueType.IsGenericType && valueType.GenericTypeArguments[0].Name == "SelectAllAndExpand`1" )
                    {
                        // 'SelectAndExpand' buries the Entity in a private field called 'Instance', 
                        // so use reflection to get that and load the attributes for each
                        var selectExpandQry = value as IQueryable;
                        var itemsList = new List<Attribute.IHasAttributes>();
                        foreach ( var selectExpandItem in selectExpandQry )
                        {
                            var entityProperty = selectExpandItem.GetType().GetProperty( "Instance" );
                            var entity = entityProperty.GetValue( selectExpandItem ) as Attribute.IHasAttributes;
                            if ( entity != null )
                            {
                                itemsList.Add( entity );
                            }

                            entityProperty.SetValue( selectExpandItem, entity );
                        }

                        items = itemsList;
                    }

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

            // Special Code if an $expand clause is specified and a $select clause is NOT specified
            // This fixes a couple of issues:
            //  1) our special loadAttributes stuff did't work if $expand is specified
            //  2) Only non-virtual,non-inherited fields were included (for example: Person.PrimaryAliasId, etc, wasn't getting included) if $expand was specified
            if ( value is IQueryable && typeof( IQueryable<Rock.Data.IEntity> ).IsAssignableFrom( type ) )
            {
                Type valueType = value.GetType();

                // if this is an OData 'SelectAndExpand', we have convert stuff to Dictionary manually to get the stuff to serialize the way we want
                if ( valueType.IsGenericType && valueType.GenericTypeArguments[0].Name == "SelectAllAndExpand`1" )
                {
                    var rockContext = new Rock.Data.RockContext();

                    // The normal SelectAllAndExpand converts stuff into dictionaries, but some stuff ends up missing
                    // So, this will do all that manually using reflection and our own dictionary of each Entity
                    var valueAsDictionary = new List<Dictionary<string, object>>();
                    var selectExpandQry = value as IQueryable;
                    var selectExpandList = ( selectExpandQry as IQueryable<object> ).ToList();
                    var isPersonModel = type.IsGenericType && type.GenericTypeArguments[0] == typeof( Rock.Model.Person );
                    Dictionary<int, Rock.Model.PersonAlias> personAliasLookup = null;
                    if ( isPersonModel )
                    {
                        var personIds = selectExpandList.Select( a => ( a.GetPropertyValue( "Instance" ) as Rock.Model.Person ).Id ).ToList();

                        // NOTE: If this is a really long list of PersonIds (20000+ or so), this might time out or get an error, 
                        // so if it is more than 20000 just get *all* the personalias records which would probably be much faster than a giant where clause
                        var personAliasQry = new Rock.Model.PersonAliasService( rockContext ).Queryable().AsNoTracking();
                        if ( personIds.Count < 20000 )
                        {
                            personAliasLookup = personAliasQry.Where( a => personIds.Contains( a.PersonId ) && a.AliasPersonId == a.PersonId ).ToDictionary( k => k.PersonId, v => v );
                        }
                        else
                        {
                            personAliasLookup = personAliasQry.Where( a => a.AliasPersonId == a.PersonId ).ToDictionary( k => k.PersonId, v => v );
                        }
                    }

                    foreach ( var selectExpandItem in selectExpandList )
                    {
                        var entityProperty = selectExpandItem.GetType().GetProperty( "Instance" );
                        var entity = entityProperty.GetValue( selectExpandItem ) as Rock.Data.IEntity;
                        if ( entity is Rock.Model.Person )
                        {
                            // if this is a SelectAndExpand of Person, we manually need to load Aliases so that the PrimaryAliasId is populated
                            var person = entity as Rock.Model.Person;
                            if ( !person.Aliases.Any() )
                            {
                                var primaryAlias = personAliasLookup.GetValueOrNull( person.Id );
                                if ( primaryAlias != null )
                                {
                                    person.Aliases.Add( personAliasLookup[person.Id] );
                                }
                            }
                        }

                        Dictionary<string, object> valueDictionary = new Dictionary<string, object>();

                        // add the "Expanded" stuff first to emulate the default behavior
                        var expandedStuff = selectExpandItem.GetPropertyValue( "Container" );
                        while ( expandedStuff != null)
                        {
                            var expandedName = expandedStuff.GetPropertyValue( "Name" ) as string;
                            var expandedValue = expandedStuff.GetPropertyValue( "Value" );
                            valueDictionary.Add( expandedName, expandedValue );
                            expandedStuff = expandedStuff.GetPropertyValue( "Next" );
                        }

                        // add each of the Entity's properties
                        foreach ( var entityKeyValue in entity.ToDictionary() )
                        {
                            valueDictionary.Add( entityKeyValue.Key, entityKeyValue.Value );
                        }

                        // if LoadAttributes was specified, add those last
                        if ( LoadAttributes && ( entity is Rock.Attribute.IHasAttributes ) )
                        {
                            // Add Attributes and AttributeValues
                            valueDictionary.Add( "Attributes", ( entity as Rock.Attribute.IHasAttributes ).Attributes );
                            valueDictionary.Add( "AttributeValues", ( entity as Rock.Attribute.IHasAttributes ).AttributeValues );
                        }

                        valueAsDictionary.Add( valueDictionary );
                    }
                    
                    base.WriteToStream( type, valueAsDictionary, writeStream, effectiveEncoding );
                    return;
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
