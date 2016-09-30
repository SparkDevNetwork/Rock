using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using Rock.UniversalSearch.IndexModels.Attributes;

namespace Rock.UniversalSearch.IndexModels
{
    public class IndexModelBase : DynamicObject
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

        [RockIndexField( Type = IndexFieldType.Number )]
        public Int64 Id { get; set; }

        [RockIndexField( Index = IndexType.NotIndexed )]
        public string SourceIndexModel { get; set; }

        [RockIndexField(Index = IndexType.NotIndexed)]
        public string IndexModelType {
            get
            {
                return InstanceType.ToString();
            }
        }

        /// <summary>
        /// Formats the search result.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public virtual FormattedSearchResult FormatSearchResult( Person person, Dictionary<string, object> displayOptions = null )
        {
            string result = string.Empty;

            // try returning some common properties
            if ( this["Name"] != null )
            {
                result = this.GetPropertyValue( "Name" ).ToString();
            }

            if ( this["Title"] != null )
            {
                result = this.GetPropertyValue( "Title" ).ToString();
            }

            // otherwise return not implemented
            return new FormattedSearchResult() { IsViewAllowed = true, FormattedResult = result};
        }

        public IndexModelBase()
        {
            Instance = this;
            InstanceType = this.GetType();
        }

        [RockIndexField( Index = IndexType.NotIndexed )]
        public virtual string IconCssClass
        {
            get
            {
                return iconCssClass;
            }
            set
            {
                iconCssClass = value;
            }
        }
        private string iconCssClass = "fa fa-file";

        protected static void AddIndexableAttributes( IndexModelBase indexModel, IHasAttributes sourceModel )
        {
            sourceModel.LoadAttributes();

            foreach ( var attributeValue in sourceModel.AttributeValues )
            {
                // check that the attribute is marked as IsIndexEnabled
                var attribute = AttributeCache.Read(attributeValue.Value.AttributeId);

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
            _members[binder.Name] = value;
            return true;
        }

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

        protected bool SetProperty( object instance, string name, object value )
        {
            if ( instance == null )
                instance = this;

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
            return false;
        }

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
                if ( _members.ContainsKey( key ) )
                {
                    _members[key] = value;
                    return;
                }

                // check instance for existance of type first
                var miArray = InstanceType.GetMember( key, BindingFlags.Public | BindingFlags.GetProperty );
                if ( miArray != null && miArray.Length > 0 )
                    SetProperty( Instance, key, value );
                else
                    _members[key] = value;
            }
        }

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

            return propertyNames;
        }

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

        #region Static Methods

        #endregion
    }
}
