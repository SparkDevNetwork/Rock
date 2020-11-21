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
using System.Web;
using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class RockJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        /// <summary>
        /// 
        /// </summary>
        private class LoadAttributesOptions
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LoadAttributesOptions"/> class.
            /// </summary>
            /// <param name="loadAttributesEnabled">if set to <c>true</c> [load attributes enabled].</param>
            /// <param name="serializeInSimpleMode">if set to <c>true</c> [serialize in simple mode].</param>
            /// <param name="limitToAttributeKeyList">The limit to attribute key list.</param>
            /// <param name="person">The person.</param>
            public LoadAttributesOptions( bool loadAttributesEnabled, bool serializeInSimpleMode, string[] limitToAttributeKeyList, Rock.Model.Person person )
            {
                LoadAttributesEnabled = loadAttributesEnabled;
                SerializeInSimpleMode = serializeInSimpleMode;
                LimitToAttributeKeyList = limitToAttributeKeyList ?? new string[0];
                Person = person;
            }

            /// <summary>
            /// The context items key
            /// </summary>
            public static readonly string ContextItemsKey = $"{typeof( RockJsonMediaTypeFormatter ).FullName}:LoadAttributesOptions";

            /// <summary>
            /// Gets or sets a value indicating whether [load attributes].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [load attributes]; otherwise, <c>false</c>.
            /// </value>
            public readonly bool LoadAttributesEnabled;

            /// <summary>
            /// Gets or sets the limit to attribute key list.
            /// </summary>
            /// <value>
            /// The limit to attribute key list.
            /// </value>
            public readonly string[] LimitToAttributeKeyList;

            /// <summary>
            /// Gets or sets a value indicating whether [serialize in simple mode].
            /// </summary>
            /// <value>
            /// <c>true</c> if [serialize in simple mode]; otherwise, <c>false</c>.
            /// </value>
            public readonly bool SerializeInSimpleMode;

            /// <summary>
            /// Gets or sets the person that initiated the REST request
            /// </summary>
            /// <value>
            /// The person.
            /// </value>
            public readonly Rock.Model.Person Person;

            public override string ToString()
            {
                return $"LoadAttributesEnabled:{LoadAttributesEnabled}&{SerializeInSimpleMode}, {Person}";
            }
        }

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

            string[] limitToAttributeKeyList = qryParams["AttributeKeys"]?.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Trim() ).ToArray();

            // if "simple" or True is specified in the LoadAttributes param, tell the formatter to serialize in Simple mode
            bool serializeInSimpleMode = loadAttributes.Equals( "simple", StringComparison.OrdinalIgnoreCase ) || ( loadAttributes.AsBooleanOrNull() ?? false );

            // if either "simple", "expanded", or True is specified in the LoadAttributes param, tell the formatter to load the attributes on the way out
            bool loadAttributesEnabled = serializeInSimpleMode || loadAttributes.Equals( "expanded", StringComparison.OrdinalIgnoreCase );

            Rock.Model.Person person = null;

            // NOTE: request.Properties["Person"] gets set in Rock.Rest.Filters.SecurityAttribute.OnActionExecuting
            if ( loadAttributesEnabled && request.Properties.ContainsKey( "Person" ) )
            {
                person = request.Properties["Person"] as Rock.Model.Person;
            }

            // store the request options in HttpContext.Current.Items so they are thread safe, and only for this request
            var loadAttributesOptions = new LoadAttributesOptions( loadAttributesEnabled, serializeInSimpleMode, limitToAttributeKeyList, person );
            HttpContext.Current.Items[LoadAttributesOptions.ContextItemsKey] = loadAttributesOptions;

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
            bool isSelectAndExpand = false;
            List<object> selectAndExpandList = null;
            if ( value is IQueryable<object> && typeof( IQueryable<Rock.Data.IEntity> ).IsAssignableFrom( type ) )
            {
                Type valueType = value.GetType();

                // if this is an OData 'SelectAndExpand', we have convert stuff to Dictionary manually to get the stuff to serialize the way we want
                if ( valueType.IsGenericType && valueType.GenericTypeArguments[0].Name == "SelectAllAndExpand`1" )
                {
                    isSelectAndExpand = true;

                    var selectExpandQry = value as IQueryable<object>;

                    selectAndExpandList = selectExpandQry.ToList();
                }
            }

            var loadAttributesOptions = HttpContext.Current.Items[LoadAttributesOptions.ContextItemsKey] as LoadAttributesOptions;
            if ( loadAttributesOptions == null )
            {
                // shouldn't happen, but just in case
                loadAttributesOptions = new LoadAttributesOptions( false, false, null, null );
            }

            // query should be filtered by now, so iterate thru items and load attributes before the response is serialized
            if ( loadAttributesOptions.LoadAttributesEnabled )
            {
                IEnumerable<Attribute.IHasAttributes> items = null;

                if ( value is IEnumerable<Attribute.IHasAttributes> )
                {
                    // if the REST call specified that Attributes should be loaded and we are returning a list of IHasAttributes..
                    // Also, do a ToList() to fetch the query into a list (instead re-querying multiple times)
                    items = ( value as IEnumerable<Attribute.IHasAttributes> ).ToList();

                    // Assign the items list back to value
                    value = items;
                }
                else if ( value is Attribute.IHasAttributes )
                {
                    // if the REST call specified that Attributes should be loaded and we are returning a single IHasAttributes..
                    items = new List<Attribute.IHasAttributes>( new Attribute.IHasAttributes[] { value as Attribute.IHasAttributes } );
                }
                else if ( isSelectAndExpand && selectAndExpandList != null )
                {
                    //// 'SelectAndExpand' buries the Entity in a private field called 'Instance', 
                    //// so use reflection to get that and load the attributes for each

                    var itemsList = new List<Attribute.IHasAttributes>();
                    foreach ( var selectExpandItem in selectAndExpandList )
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

                List<AttributeCache> limitToAttributes = null;
                if ( loadAttributesOptions.LimitToAttributeKeyList?.Any() == true && type.IsGenericType )
                {
                    var entityTypeType = type.GenericTypeArguments[0];
                    if ( entityTypeType != null )
                    {
                        var entityType = EntityTypeCache.Get( entityTypeType );
                        var entityAttributesList = AttributeCache.GetByEntity( entityType.Id )?.SelectMany( a => a.AttributeIds ).ToList().Select( a => AttributeCache.Get( a ) ).Where( a => a != null ).ToList();
                        limitToAttributes = entityAttributesList?.Where( a => loadAttributesOptions.LimitToAttributeKeyList.Contains( a.Key, StringComparer.OrdinalIgnoreCase ) ).ToList();
                    }
                }

                if ( items != null )
                {
                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        foreach ( var item in items )
                        {
                            Rock.Attribute.Helper.LoadAttributes( item, rockContext, limitToAttributes );
                        }

                        FilterAttributes( rockContext, items, loadAttributesOptions.Person );
                    }
                }
            }

            // Special Code if an $expand clause is specified and a $select clause is NOT specified
            // This fixes a couple of issues:
            //  1) our special loadAttributes stuff didn't work if $expand is specified
            //  2) Only non-virtual,non-inherited fields were included (for example: Person.PrimaryAliasId, etc, wasn't getting included) if $expand was specified
            if ( isSelectAndExpand )
            {
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    List<Dictionary<string, object>> valueAsDictionary = GetSelectAndExpandDictionaryObject( type, selectAndExpandList, rockContext, loadAttributesOptions );
                    base.WriteToStream( type, valueAsDictionary, writeStream, effectiveEncoding );
                }

                return;
            }

            base.WriteToStream( type, value, writeStream, effectiveEncoding );
        }

        /// <summary>
        /// Gets the select and expand dictionary object.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="selectAndExpandList">The select and expand list.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="loadAttributesOptions">The load attributes options.</param>
        /// <returns></returns>
        private static List<Dictionary<string, object>> GetSelectAndExpandDictionaryObject( Type type, List<object> selectAndExpandList, Rock.Data.RockContext rockContext, LoadAttributesOptions loadAttributesOptions )
        {
            // The normal SelectAllAndExpand converts stuff into dictionaries, but some stuff ends up missing
            // So, this will do all that manually using reflection and our own dictionary of each Entity
            var valueAsDictionary = new List<Dictionary<string, object>>();
            var isPersonModel = type.IsGenericType && type.GenericTypeArguments[0] == typeof( Rock.Model.Person );
            Dictionary<int, Rock.Model.PersonAlias> personAliasLookup = null;
            if ( isPersonModel )
            {
                var personIds = selectAndExpandList.Select( a => ( a.GetPropertyValue( "Instance" ) as Rock.Model.Person ).Id ).ToList();

                // NOTE: If this is a really long list of PersonIds (20000+ or so), this might time out or get an error, 
                // so if it is more than 20000 just get *all* the PersonAlias records which would probably be much faster than a giant where clause
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

            foreach ( var selectExpandItem in selectAndExpandList )
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
                while ( expandedStuff != null )
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
                if ( loadAttributesOptions.LoadAttributesEnabled && ( entity is Attribute.IHasAttributes ) )
                {
                    // Add Attributes and AttributeValues
                    valueDictionary.Add( "Attributes", ( entity as Attribute.IHasAttributes ).Attributes );
                    valueDictionary.Add( "AttributeValues", ( entity as Attribute.IHasAttributes ).AttributeValues );
                }

                valueAsDictionary.Add( valueDictionary );
            }

            return valueAsDictionary;
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

            var entityType = EntityTypeCache.Get( itemType );
            if ( entityType == null )
            {
                // shouldn't happen
                return;
            }

            var entityAttributes = AttributeCache.GetByEntity( entityType.Id );

            // only return attributes that the person has VIEW auth to
            // NOTE: There are some Attributes that even Admin doesn't have VIEW auth so (For example, some of obsolete DISC attributes)
            foreach ( var entityAttribute in entityAttributes )
            {
                foreach ( var attributeId in entityAttribute.AttributeIds )
                {
                    var attribute = AttributeCache.Get( attributeId );
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
            var loadAttributesOptions = HttpContext.Current.Items[LoadAttributesOptions.ContextItemsKey] as LoadAttributesOptions;
            if ( loadAttributesOptions == null )
            {
                // shouldn't happen, but just in case
                loadAttributesOptions = new LoadAttributesOptions( false, false, null, null );
            }

            if ( loadAttributesOptions.LoadAttributesEnabled && loadAttributesOptions.SerializeInSimpleMode )
            {
                // Use the RockJsonTextWriter when we need to Serialize Model.AttributeValues and Model.Attributes in simple mode
                return new RockJsonTextWriter( new System.IO.StreamWriter( writeStream ), loadAttributesOptions.SerializeInSimpleMode );
            }
            else
            {
                return base.CreateJsonWriter( type, writeStream, effectiveEncoding );
            }
        }
    }
}
