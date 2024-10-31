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

using Rock.Attribute;
using Rock.CheckIn.v2.Labels.Formatters;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Labels
{
    /// <summary>
    /// A single data source derived from a Date or DateTime attribute.
    /// </summary>
    /// <typeparam name="TLabelData">The type of label data expected.</typeparam>
    internal class DateAttributeFieldDataSource<TLabelData> : FieldDataSource
    {
        private readonly Guid _attributeCacheGuid;

        private readonly Func<TLabelData, IHasAttributes> _entitySelector;

        /// <inheritdoc/>
        public sealed override bool IsCollection => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAttributeFieldDataSource{TLabelData}"/> class.
        /// </summary>
        /// <param name="attributeCache">The attribute cache that identifies the field.</param>
        /// <param name="entityPath">The text path to the entity property.</param>
        /// <param name="entitySelector">The entity selector function.</param>
        public DateAttributeFieldDataSource( AttributeCache attributeCache, string entityPath, Func<TLabelData, IHasAttributes> entitySelector )
        {
            _attributeCacheGuid = attributeCache.Guid;
            _entitySelector = entitySelector;

            Key = $"attribute:{entityPath}:{attributeCache.Guid}";
            Name = attributeCache.Name;
            Category = "Attributes";
            Formatter = DateDataFormatter.Instance;
        }

        /// <inheritdoc/>
        public override List<object> GetValues( LabelField field, PrintLabelRequest printRequest )
        {
            var entity = _entitySelector( ( TLabelData ) printRequest.LabelData );
            var attributeCache = AttributeCache.Get( _attributeCacheGuid );

            if ( entity == null || attributeCache == null )
            {
                return new List<object> { null };
            }

            var value = entity.GetAttributeValue( attributeCache.Key );

            return new List<object> { value?.AsDateTime() };
        }
    }
}
