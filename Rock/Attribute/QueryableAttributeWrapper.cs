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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// This is a special use class that takes a set of attribute cache and
    /// attribute value cache objects and then imitates an object that has
    /// attributes. Primarily this is used for testing the queryable
    /// attribute code to ensure the values match correctly with the
    /// non-queryable code values.
    /// </summary>
    internal class QueryableAttributeWrapper : IHasAttributes
    {
        #region Fields

        /// <summary>
        /// An empty dictionary for default values so we only create it once.
        /// </summary>
        private static readonly Dictionary<string, string> _emptyDefaultValues = new Dictionary<string, string>();

        #endregion

        #region Properties

        /// <inheritdoc/>
        public int Id { get; }

        /// <inheritdoc/>
        public Dictionary<string, AttributeCache> Attributes { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

        /// <inheritdoc/>
        public Dictionary<string, string> AttributeValueDefaults => _emptyDefaultValues;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableAttributeWrapper"/> class
        /// that has no attributes or values.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        public QueryableAttributeWrapper( int id )
        {
            Id = id;
            Attributes = new Dictionary<string, AttributeCache>();
            AttributeValues = new Dictionary<string, AttributeValueCache>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableAttributeWrapper"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="attributes">The attributes the entity will have.</param>
        /// <param name="attributeValues">The attribute values the entity will have.</param>
        public QueryableAttributeWrapper( int id, Dictionary<string, AttributeCache> attributes, Dictionary<string, AttributeValueCache> attributeValues )
        {
            Id = id;
            Attributes = attributes;
            AttributeValues = attributeValues;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableAttributeWrapper"/> class.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <param name="values">The attribute values from the queryable data.</param>
        public QueryableAttributeWrapper( int id, IEnumerable<QueryableAttributeValue> values )
        {
            var attributes = new Dictionary<string, AttributeCache>();
            var attributeValues = new Dictionary<string, AttributeValueCache>();

            foreach ( var value in values )
            {
                var attribute = AttributeCache.Get( value.AttributeId );

                if ( attributes == null )
                {
                    continue;
                }

                var valueCache = new AttributeValueCache( value.AttributeId, id,
                    value.Value,
                    value.PersistedTextValue,
                    value.PersistedHtmlValue,
                    value.PersistedCondensedTextValue,
                    value.PersistedCondensedHtmlValue,
                    value.IsPersistedValueDirty );

                attributes.AddOrIgnore( value.Key, attribute );
                attributeValues.AddOrIgnore( value.Key, valueCache );
            }

            Id = id;
            Attributes = attributes;
            AttributeValues = attributeValues;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public string GetAttributeValue( string key )
        {
            if ( AttributeValues != null && AttributeValues.ContainsKey( key ) )
            {
                return AttributeValues[key].Value;
            }

            if ( Attributes != null && Attributes.ContainsKey( key ) )
            {
                return Attributes[key].DefaultValue;
            }

            return null;
        }

        /// <inheritdoc/>
        public List<string> GetAttributeValues( string key )
        {
            var value = GetAttributeValue( key );

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                return value.SplitDelimitedValues().ToList();
            }

            return new List<string>();
        }

        /// <inheritdoc/>
        public void SetAttributeValue( string key, string value )
        {
            if ( AttributeValues != null && AttributeValues.ContainsKey( key ) )
            {
                AttributeValues[key].Value = value;
            }
        }

        #endregion
    }
}
