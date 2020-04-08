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

using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch.IndexModels.Attributes;
using Rock.Web.Cache;

namespace Rock.UniversalSearch.IndexModels
{
    /// <summary>
    /// Base Index Model
    /// </summary>
    /// <seealso cref="System.Dynamic.DynamicObject" />
    public class IndexModelBase : DynamicObject, Lava.ILiquidizable
    {
        private Dictionary<string, object> _members = new Dictionary<string, object>();
        object Instance;
        Type InstanceType;

        PropertyInfo[] InstancePropertyInfo
        {
            get
            {
                if ( _InstancePropertyInfo == null && Instance != null )
                {
                    _InstancePropertyInfo = Instance.GetType().GetProperties();
                }
                return _InstancePropertyInfo;
            }
        }
        PropertyInfo[] _InstancePropertyInfo;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public Int64 Id { get; set; }

        /// <summary>
        /// Gets or sets the source index model.
        /// </summary>
        /// <value>
        /// The source index model.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string SourceIndexModel { get; set; }

        /// <summary>
        /// Gets or sets the model configuration. This is used to set various configuration options for the model that can be queried for in searches.
        /// For instance models that do not have filters will use this field to report that fact ('nofilters') to help when searching with field filters.
        /// </summary>
        /// <value>
        /// The model configuration.
        /// </value>
        public string ModelConfiguration { get; set; }

        /// <summary>
        /// Gets the type of the index model.
        /// </summary>
        /// <value>
        /// The type of the index model.
        /// </value>
        [RockIndexField(Index = IndexType.NotIndexed)]
        public string IndexModelType {
            get
            {
                return InstanceType.ToString();
            }
        }

        /// <summary>
        /// Gets the index model assembly.
        /// </summary>
        /// <value>
        /// The index model assembly.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string IndexModelAssembly
        {
            get
            {
                return InstanceType.Assembly.FullName;
            }
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="displayOptions">The display options.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public virtual FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null, Dictionary<string, object> mergeFields = null )
        {
            string result = string.Empty;

            // get template from entity type
            var sourceModelEntity = EntityTypeCache.All().Where( e => e.Name == this.SourceIndexModel ).FirstOrDefault();

            if ( sourceModelEntity != null ) {
                var template = sourceModelEntity.IndexResultTemplate;

                if ( template.IsNotNullOrWhiteSpace() )
                {
                    if ( mergeFields == null)
                    {
                        mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, person );
                    }

                    mergeFields.AddOrReplace( "IndexDocument", this );
                    mergeFields.AddOrReplace( "DisplayOptions", displayOptions );

                    return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = template.ResolveMergeFields( mergeFields ) };
                }
            }

            // otherwise return not implemented (blank)
            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = result};
        }

        /// <summary>
        /// Gets the document URL.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDocumentUrl( Dictionary<string, object> displayOptions = null )
        {
            // get template from entity type
            var sourceModelEntity = EntityTypeCache.All().Where( e => e.Name == this.SourceIndexModel ).FirstOrDefault();

            if ( sourceModelEntity != null )
            {
                var template = sourceModelEntity.IndexDocumentUrl;

                if ( template.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
                    mergeFields.Add( "IndexDocument", this );
                    mergeFields.Add( "DisplayOptions", displayOptions );

                    return template.ResolveMergeFields( mergeFields ).Trim();
                }
            }

            return string.Empty;;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModelBase"/> class.
        /// </summary>
        public IndexModelBase()
        {
            Instance = this;
            InstanceType = this.GetType();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public string DocumentName { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public virtual string IconCssClass { get; set; } = "fa fa-file";

        /// <summary>
        /// Gets or sets the score.
        /// </summary>
        /// <value>
        /// The score.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the model order.
        /// </summary>
        /// <value>
        /// Used to give preference to certain model types when scores match.
        /// </value>
        [RockIndexField( Index = IndexType.NotIndexed )]
        public int ModelOrder { get; set; } = 1;

        /// <summary>
        /// Adds the indexable attributes.
        /// </summary>
        /// <param name="indexModel">The index model.</param>
        /// <param name="sourceModel">The source model.</param>
        protected static void AddIndexableAttributes( IndexModelBase indexModel, Attribute.IHasAttributes sourceModel )
        {
            sourceModel.LoadAttributes();

            foreach ( var attributeValue in sourceModel.AttributeValues )
            {
                // check that the attribute is marked as IsIndexEnabled
                var attribute = AttributeCache.Get(attributeValue.Value.AttributeId);

                if ( attribute.IsIndexEnabled )
                {

                    var key = attributeValue.Key;

                    // remove invalid characters
                    key = key.Replace( ".", "_" );
                    key = key.Replace( ",", "_" );
                    key = key.Replace( "#", "_" );
                    key = key.Replace( "*", "_" );
                    key = key.StartsWith( "_" ) ? key.Substring( 1 ) : key;

                    indexModel[key] = attributeValue.Value.ValueFormatted;
                }
            }
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
            result = null;

            // first check the dictionary for member
            if ( _members.Keys.Contains( binder.Name ) )
            {
                result = _members[binder.Name];
                return true;
            }


            // next check for public properties via Reflection
            try
            {
                return GetProperty( Instance, binder.Name, out result );
            }
            catch { }
            

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
            if ( Instance != null )
            {
                try
                {
                    bool result = SetProperty( this, binder.Name, value );
                    if ( result )
                        return true;
                }
                catch { }
            }

            // no match - set or add to dictionary
            // ensure name is starts with an upper case 
            var name = char.ToUpper( binder.Name[0] ) + binder.Name.Substring( 1 );
            _members[name] = value;
            return true;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        protected bool GetProperty( object instance, string name, out object result )
        {
            if ( instance == null )
                instance = this;

            var miArray = InstanceType.GetMember( name, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance );
            if ( miArray != null && miArray.Length > 0 )
            {
                var mi = miArray[0];
                if ( mi.MemberType == MemberTypes.Property )
                {
                    result = ((PropertyInfo)mi).GetValue( instance, null );
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        protected bool SetProperty( object instance, string name, object value )
        {
            if ( instance == null )
                instance = this;

            if ( name != null )
            {
                var miArray = InstanceType.GetMember( name, BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance );
                if ( miArray != null && miArray.Length > 0 )
                {
                    var mi = miArray[0];
                    if ( mi.MemberType == MemberTypes.Property )
                    {
                        ((PropertyInfo)mi).SetValue( Instance, value, null );
                        return true;
                    }
                }
            }
            return false;
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
                    return _members[key];
                }
                catch ( KeyNotFoundException )
                {
                    // try reflection on instanceType
                    object result = null;
                    if ( GetProperty( Instance, key, out result ) )
                        return result;

                    // nope doesn't exist
                    return null;
                }
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
                    var miArray = InstanceType.GetMember( key, BindingFlags.Public | BindingFlags.GetProperty );
                    if ( miArray != null && miArray.Length > 0 )
                    {
                        SetProperty( Instance, key, value );
                    }
                    else
                    {
                        _members[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, object>> GetProperties( bool includeInstanceProperties = false )
        {
            if ( includeInstanceProperties && Instance != null )
            {
                foreach ( var prop in this.InstancePropertyInfo )
                    yield return new KeyValuePair<string, object>( prop.Name, prop.GetValue( Instance, null ) );
            }

            foreach ( var key in this._members.Keys )
                yield return new KeyValuePair<string, object>( key, this._members[key] );

        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> propertyNames = new List<string>();

            foreach ( var prop in this.InstancePropertyInfo )
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


        #region ILiquid Implementation
        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public List<string> AvailableKeys
        {
            get
            {
                return GetDynamicMemberNames().ToList();
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object this[object key]
        {
            get
            {
                var propertyKey = key.ToStringSafe();
                return this[propertyKey];
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="includeInstanceProperties">if set to <c>true</c> [include instance properties].</param>
        /// <returns></returns>
        public bool Contains( KeyValuePair<string, object> item, bool includeInstanceProperties = false )
        {
            bool res = _members.ContainsKey( item.Key );
            if ( res )
                return true;

            if ( includeInstanceProperties && Instance != null )
            {
                foreach ( var prop in this.InstancePropertyInfo )
                {
                    if ( prop.Name == item.Key )
                        return true;
                }
            }

            return false;
        }

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
            return this.GetDynamicMemberNames().Contains( key.ToString() );
        }

        #endregion
    }
}
