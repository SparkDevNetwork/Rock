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
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Cms.ContentCollection.Attributes;
using Rock.Cms.ContentCollection.Search;
using Rock.Data;
using Rock.Field;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Cms;
using Rock.Web.Cache;

namespace Rock.Cms.ContentCollection.IndexDocuments
{
    /// <summary>
    /// Base index document for the content collection search index.
    /// </summary>
    [RockInternal( "1.14" )]
    internal class IndexDocumentBase : DynamicObject, ILavaDataDictionary, Lava.ILiquidizable
    {
        #region Fields

        /// <summary>
        /// The dynamic members that were set but did not match any properties.
        /// </summary>
        private readonly Dictionary<string, object> _members = new Dictionary<string, object>();

        /// <summary>
        /// The cached instance properties of this document.
        /// </summary>
        private PropertyInfo[] _instanceProperties;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance properties.
        /// </summary>
        /// <value>The instance properties.</value>
        private PropertyInfo[] InstanceProperties
        {
            get
            {
                if ( _instanceProperties == null )
                {
                    _instanceProperties = GetType().GetProperties();
                }

                return _instanceProperties;
            }
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [IndexField]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the original entity this document represents.
        /// </summary>
        /// <value>
        /// The identifier of the original entity this document represents.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the display name of this document.
        /// </summary>
        /// <value>
        /// The display name of htis document.
        /// </value>
        [IndexField( Boost = 3, IsSearched = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name as a sortable field.
        /// </summary>
        /// <value>
        /// The display name as a sortable field.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Keyword )]
        public string NameSort { get; set; }

        /// <summary>
        /// Gets or sets the common detail content of this document.
        /// </summary>
        /// <value>
        /// The common detail content of this document.
        /// </value>
        [IndexField( IsSearched = true )]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the type name of the source item this document was
        /// built up from. This is namespace plus class name.
        /// </summary>
        /// <value>
        /// The type name of the source item this document was built up from.
        /// </value>
        [IndexField]
        public string ItemType { get; set; }

        /// <summary>
        /// Gets or sets the type name of the source entity this documentis
        /// associated with. This is namespace plus class name.
        /// </summary>
        /// <value>
        /// The type name of the source item this document was built up from.
        /// </value>
        [IndexField]
        public string SourceType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the collection source this item belongs to.
        /// </summary>
        /// <value>
        /// The identifier of the collection source this item belongs to.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public int SourceId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the collection source this item belongs to.
        /// </summary>
        /// <value>
        /// The unique identifier of the collection source this item belongs to.
        /// </value>
        /// <inheritdoc/>
        [IndexField]
        public Guid SourceGuid { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the collection source this item belongs to.
        /// </summary>
        /// <value>
        /// The identifier key of the collection source this item belongs to.
        /// </value>
        [IndexField]
        public string SourceIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this item is currently trending.
        /// </summary>
        /// <value>
        /// A value indicating if this item is currently trending.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Boolean )]
        public bool IsTrending { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the current rank if this item is trending.
        /// </summary>
        /// <value>
        /// A value specifying the current rank if this item is trending.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public int TrendingRank { get; set; }

        /// <summary>
        /// Gets or sets the personalization segment identifiers this item belongs to.
        /// </summary>
        /// <value>
        /// The personalization segment identifiers this item belongs to.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public List<int> Segments { get; set; }

        /// <summary>
        /// Gets or sets the personalization request filter identifiers this item belongs to.
        /// </summary>
        /// <value>
        /// The personalization request filter identifiers this item belongs to.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public List<int> RequestFilters { get; set; }

        /// <summary>
        /// Gets or sets the primary date and time for this item. This should be the
        /// next upcoming effective date of the item.
        /// </summary>
        /// <value>
        /// The primary date and time for this item.
        /// </value>
        [IndexField( FieldType = IndexFieldType.DateTime )]
        public DateTime? RelevanceDateTime { get; set; }

        /// <summary>
        /// Gets or sets the year number for this item. This should be either the
        /// year it was posted or the upcoming year it becomes effective.
        /// </summary>
        /// <value>
        /// The year number for this item.
        /// </value>
        [IndexField( FieldType = IndexFieldType.Integer )]
        public int? Year { get; set; }

        /* 04/04/2022

        IndexModelType and IndexModelAssembly are stored as the original type
        of this (PersonIndex, GroupIndex, etc).  When it comes back from the Index
        Server, the original type information will be lost and just say "IndexBase"
        so we'll need to store IndexModelType and IndexModelAssembly in the Index Server
        (ElasticSearch, Lucene)

        */

        /// <summary>
        /// Gets the Type name of the IndexModel that this was when it was stored to the Index server.
        /// </summary>
        /// <value>
        /// The type of the index model.
        /// </value>
        [IndexField]
        public string IndexModelType { get; set; }

        /// <summary>
        /// Gets whether or not this document is approved.
        /// </summary>
        [IndexField]
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Gets the IndexModelAssembly name of the IndexModel that this was when it was stored to the Index server.
        /// </summary>
        /// <value>
        /// The index model assembly.
        /// </value>
        public string IndexModelAssembly { get; set; }

        /// <summary>
        /// Gets or sets the score of the document after a search.
        /// </summary>
        /// <value>
        /// The score of the document.
        /// </value>
        public double Score { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDocumentBase"/> class.
        /// </summary>
        public IndexDocumentBase()
        {
            IndexModelType = GetType().FullName;
            IndexModelAssembly = GetType().Assembly.FullName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a document Id value that is comprised of both the modelId and
        /// the sourceId.
        /// </summary>
        /// <param name="modelId">The identifier of the model.</param>
        /// <param name="sourceId">The identifier of the source related to the model.</param>
        /// <returns>A signed long that represents the document identifier.</returns>
        protected static string GetDocumentId( int modelId, int sourceId )
        {
            return $"{sourceId}_{modelId}";
        }

        /// <summary>
        /// Adds all standard fields to the index model. This should be called
        /// by base classes and will be updated in the future when new standard
        /// fields are added.
        /// </summary>
        /// <param name="sourceModel">The source entity model to be indexed.</param>
        /// <param name="source">The content colleciton source object that describes the index operation.</param>
        /// <returns>A <see cref="Task"/> that represents when this operation has completed.</returns>
        internal async Task AddStandardFieldsAsync( IEntity sourceModel, ContentCollectionSourceCache source )
        {
            if ( sourceModel is IHasAttributes attributeEntity )
            {
                AddIndexableAttributes( attributeEntity, source );
            }

            AddCustomFields( sourceModel, source );

            AddPersonalizationData( sourceModel, source );

            await AddExistingTrendingDataAsync( source );
        }

        /// <summary>
        /// Adds the custom fields defined on the source to the index document.
        /// </summary>
        /// <param name="entity">The entity being indexed.</param>
        /// <param name="source">The source that defines how the document is indexed.</param>
        internal void AddCustomFields( IEntity entity, ContentCollectionSourceCache source )
        {
            var additionalSettings = source.AdditionalSettings.FromJsonOrNull<ContentCollectionSourceAdditionalSettingsBag>();

            if ( additionalSettings == null || additionalSettings.CustomFields == null )
            {
                return;
            }

            var mergeFields = new Dictionary<string, object>
            {
                ["Item"] = entity
            };

            foreach ( var customField in additionalSettings.CustomFields )
            {
                var value = customField.Template.ResolveMergeFields( mergeFields );

                if ( !customField.IsMultiple )
                {
                    this[$"{customField.Key}ValueRaw"] = value;
                    this[$"{customField.Key}ValueFormatted"] = value;

                    FieldValueHelper.AddFieldValue( source.ContentCollectionId, customField.Key, value, value );
                }
                else
                {
                    var values = value.Split( ',' ).Select( s => s.Trim() ).Where( s => s.IsNotNullOrWhiteSpace() ).ToList();

                    this[$"{customField.Key}ValueRaw"] = values;
                    this[$"{customField.Key}ValueFormatted"] = value;

                    foreach ( var v in values )
                    {
                        FieldValueHelper.AddFieldValue( source.ContentCollectionId, customField.Key, v, v );
                    }
                }
            }
        }

        /// <summary>
        /// Adds the indexable attributes to this index.
        /// </summary>
        /// <param name="sourceModel">The source model that has the attributes.</param>
        /// <param name="source">The collection source this model is associated with.</param>
        internal void AddIndexableAttributes( Attribute.IHasAttributes sourceModel, ContentCollectionSourceCache source )
        {
            if ( sourceModel.Attributes == null )
            {
                sourceModel.LoadAttributes();
            }

            foreach ( var attributeValue in sourceModel.AttributeValues )
            {
                var key = MakeAttributeKeySafe( attributeValue.Key );

                if ( !sourceModel.Attributes.TryGetValue( attributeValue.Key, out var attribute ) )
                {
                    continue;
                }

                // If the field type supports splitting multiple values then try
                // to process each individual value after it is split.
                if ( attribute.FieldType.Field is ISplitMultiValueFieldType multiFieldType )
                {
                    var rawValues = multiFieldType.SplitMultipleValues( attributeValue.Value.Value );
                    var formattedValue = attributeValue.Value.ValueFormatted;

                    // If we only got a single value after the split, store it
                    // as a single value for backwards compatibility.
                    if ( rawValues.Count == 1 )
                    {
                        this[$"{key}ValueRaw"] = attributeValue.Value.Value;
                    }
                    else
                    {
                        this[$"{key}ValueRaw"] = rawValues;
                    }

                    this[$"{key}ValueFormatted"] = formattedValue;

                    foreach ( var value in rawValues )
                    {
                        formattedValue = attribute.FieldType.Field.GetTextValue( value, attribute.ConfigurationValues );
                        FieldValueHelper.AddAttributeFieldValue( source.ContentCollectionId, key, value, formattedValue );
                    }
                }
                else
                {
                    var value = attributeValue.Value.Value;
                    var formattedValue = attributeValue.Value.ValueFormatted;

                    this[$"{key}ValueRaw"] = value;
                    this[$"{key}ValueFormatted"] = formattedValue;

                    FieldValueHelper.AddAttributeFieldValue( source.ContentCollectionId, key, value, formattedValue );
                }
            }
        }

        /// <summary>
        /// Adds the existing trending data to the document.
        /// </summary>
        internal async Task AddExistingTrendingDataAsync( ContentCollectionSourceCache source )
        {
            if ( !source.ContentCollection.TrendingEnabled )
            {
                return;
            }

            // Try to get the old index so we can fill in trending values.
            var query = new SearchQuery
            {
                new SearchField
                {
                    Name = nameof( Id ),
                    Value = Id,
                    IsPhrase = true,
                    IsWildcard = false
                }
            };

            try
            {
                var options = new SearchOptions
                {
                    MaxResults = 1
                };

                var results = await ContentIndexContainer.GetActiveComponent()?.SearchAsync( query, options );

                if ( results != null && results.Documents.Count > 0 )
                {
                    IsTrending = results.Documents[0].IsTrending;
                    TrendingRank = results.Documents[0].TrendingRank;
                }
            }
            catch
            {
                // Intentionally ignore any errors.
            }
        }

        /// <summary>
        /// Adds the personalization data to the document.
        /// </summary>
        /// <param name="entity">The entity used to populate this document.</param>
        /// <param name="source">The collection source this model is associated with.</param>
        internal void AddPersonalizationData( IEntity entity, ContentCollectionSourceCache source )
        {
            // Check if personalization is enabled.
            if ( !source.ContentCollection.EnableSegments && !source.ContentCollection.EnableRequestFilters )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var personalizationItems = rockContext.Set<PersonalizedEntity>()
                    .AsNoTracking()
                    .Where( pe => pe.EntityTypeId == entity.TypeId && pe.EntityId == entity.Id )
                    .ToList();

                if ( source.ContentCollection.EnableSegments )
                {
                    Segments = personalizationItems
                        .Where( pi => pi.PersonalizationType == PersonalizationType.Segment )
                        .Select( pi => pi.PersonalizationEntityId )
                        .ToList();
                }

                if ( source.ContentCollection.EnableRequestFilters )
                {
                    RequestFilters = personalizationItems
                        .Where( pi => pi.PersonalizationType == PersonalizationType.RequestFilter )
                        .Select( pi => pi.PersonalizationEntityId )
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Ensures the attribute key is safe to use in the index engine. It
        /// is possible for this to cause duplicate keys, but that is a slim
        /// chance and we are good with that. It would just overwrite one of
        /// the values so it's not a massive problem.
        /// </summary>
        /// <param name="key">The attribute key to be made safe.</param>
        /// <returns>A string that represents the attribute key safe for use in a search index.</returns>
        internal static string MakeAttributeKeySafe( string key )
        {
            // remove invalid characters
            key = key.Replace( ".", "_" );
            key = key.Replace( ",", "_" );
            key = key.Replace( "#", "_" );
            key = key.Replace( "*", "_" );
            key = key.StartsWith( "_" ) ? key.Substring( 1 ) : key;

            return key;
        }

        /// <summary>
        /// Tries to get the property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if the property was found; otherwise <c>false</c>.</returns>
        protected bool TryGetProperty( string name, out object result )
        {
            var miArray = GetType().GetMember( name, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance );

            if ( miArray != null && miArray.Length > 0 )
            {
                var mi = miArray[0];
                if ( mi.MemberType == MemberTypes.Property )
                {
                    var propertyInfo = ( PropertyInfo ) mi;

                    // To avoid a TargetParameterCountException, make sure this is a property that doesn't take parameters (for example the this[string] property)
                    // See remarks at https://docs.microsoft.com/en-us/dotnet/api/system.reflection.propertyinfo.getvalue?view=netframework-4.7.2&f1url=%3FappId%3DDev16IDEF1%26l%3DEN-US%26k%3Dk(System.Reflection.PropertyInfo.GetValue);k(TargetFrameworkMoniker-.NETFramework,Version%253Dv4.7.2);k(DevLang-csharp)%26rd%3Dtrue
                    var hasParameters = propertyInfo.GetIndexParameters().Length > 0;
                    if ( !hasParameters )
                    {
                        result = propertyInfo.GetValue( this, null );
                        return true;
                    }
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Tries to set the property.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value to set the property to.</param>
        /// <returns><c>true</c> if the property was found; otherwise <c>false</c>.</returns>
        protected bool TrySetProperty( string name, object value )
        {
            if ( name != null )
            {
                var miArray = GetType().GetMember( name, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance );
                if ( miArray != null && miArray.Length > 0 )
                {
                    var mi = miArray[0];
                    if ( mi.MemberType == MemberTypes.Property )
                    {
                        ( ( PropertyInfo ) mi ).SetValue( this, value, null );
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetValue( string key )
        {
            return this[key];
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                try
                {
                    // try to get from properties collection first
                    if ( _members.ContainsKey( key ) )
                    {
                        return _members[key];
                    }
                }
                catch ( KeyNotFoundException )
                {
                    // if there is a KeyNotFoundException (even though we check), ignore and use the GetProperty Approach
                }

                // try reflection on instanceType
                if ( TryGetProperty( key, out var result ) )
                {
                    return result;
                }

                // nope doesn't exist
                return null;
            }

            set
            {
                if ( key != null )
                {
                    if ( _members.ContainsKey( key ) )
                    {
                        _members[key] = value;
                        return;
                    }

                    // check instance for existance of type first
                    var miArray = GetType().GetMember( key, BindingFlags.Public | BindingFlags.GetProperty );
                    if ( miArray != null && miArray.Length > 0 )
                    {
                        TrySetProperty( key, value );
                    }
                    else
                    {
                        _members[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( string key )
        {
            return GetDynamicMemberNames().Contains( key.ToString() );
        }

        /// <summary>
        /// Gets the additional member names that have been set but are not
        /// defined as normal properties.
        /// </summary>
        /// <returns>An enumeration of member names.</returns>
        public IEnumerable<string> GetAdditionalMemberNames()
        {
            return _members.Keys.ToList();
        }

        #endregion

        #region DynamicObject overrides

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> propertyNames = new List<string>();

            foreach ( var prop in this.InstanceProperties )
            {
                propertyNames.Add( prop.Name );
            }

            foreach ( var key in this._members.Keys )
            {
                propertyNames.Add( key );
            }

            propertyNames.Remove( "AvailableKeys" );
            propertyNames.Remove( "availableKeys" );

            return propertyNames;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result" />.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {
            // first check the dictionary for member
            if ( _members.Keys.Contains( binder.Name ) )
            {
                result = _members[binder.Name];
                return true;
            }

            // next check for public properties via Reflection
            try
            {
                return TryGetProperty( binder.Name, out result );
            }
            catch
            {
            }

            // failed to retrieve a property
            result = null;
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the <paramref name="value" /> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember( SetMemberBinder binder, object value )
        {
            // first check to see if there's a native property to set
            try
            {
                bool result = TrySetProperty( binder.Name, value );
                if ( result )
                {
                    return true;
                }
            }
            catch
            {
            }

            // no match - set or add to dictionary
            // ensure name starts with an upper case 
            var name = char.ToUpper( binder.Name[0] ) + binder.Name.Substring( 1 );
            _members[name] = value;

            return true;
        }

        #endregion

        #region ILiquid Implementation

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaHidden]
        public List<string> AvailableKeys => GetDynamicMemberNames().ToList();


        /// <summary>
        /// Gets the <see cref="object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[object key] => this[key.ToStringSafe()];

        /// <summary>
        /// Returns liquid for the object
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool ContainsKey( object key )
        {
            return GetDynamicMemberNames().Contains( key.ToString() );
        }

        #endregion
    }
}
