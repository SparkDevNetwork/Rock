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
using System.Linq;
using System.Linq.Expressions;

using Rock.AI.Agent.Classes.Entity;
using Rock.Attribute;
using Rock.Data;
using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Extension methods related to working with attributes and their values
    /// inside tools.
    /// </summary>
    internal static class AttributeValueExtensions
    {
        #region Constants

        /// <summary>
        /// The expression for <see cref="GetAttributeValueResults(IEnumerable{QueryableAttributeValue}, AgentRequestContext)"/>
        /// and <see cref="GetAttributeValueResultsExpression"/>.
        /// </summary>
        private static readonly Expression<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> _getAttributeValueResultsExpression = ( attributeValues, context ) => attributeValues
            .Where( a => context.AudienceType == AudienceType.Internal || a.IsPublic )
            .Select( a => new AttributeValueResult
            {
                AttributeId = a.AttributeId,
                Value = a.PersistedTextValue,
                Name = a.Name,
                Key = a.Key,
                TextValue = a.PersistedTextValue,
            } );

        /// <summary>
        /// The compiled function for <see cref="GetAttributeValueResults(IEnumerable{QueryableAttributeValue}, AgentRequestContext)"/>.
        /// </summary>
        private static readonly Lazy<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> _getAttributeValueResultsFunc =
            new Lazy<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>>( () => _getAttributeValueResultsExpression.Compile() );

        /// <summary>
        /// The expression for <see cref="GetGridAttributeValueResults(IEnumerable{QueryableAttributeValue}, AgentRequestContext)"/>
        /// and <see cref="GetGridAttributeValueResultsExpression"/>.
        /// </summary>
        private static readonly Expression<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> _getGridAttributeValueResultsExpression = ( attributeValues, context ) => attributeValues
            .Where( a => ( context.AudienceType == AudienceType.Internal || a.IsPublic )
                && a.IsGridColumn )
            .Select( a => new AttributeValueResult
            {
                AttributeId = a.AttributeId,
                Value = a.PersistedTextValue,
                Name = a.Name,
                Key = a.Key,
                TextValue = a.PersistedTextValue,
            } );

        /// <summary>
        /// The compiled function for <see cref="GetGridAttributeValueResults(IEnumerable{QueryableAttributeValue}, AgentRequestContext)"/>.
        /// </summary>
        private static readonly Lazy<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> _getGridAttributeValueResultsFunc =
            new Lazy<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>>( () => _getGridAttributeValueResultsExpression.Compile() );

        #endregion

        #region Methods

        /// <summary>
        /// Returns a collection of attribute value results from the queryable
        /// attribute values. This should be used when retrieving attribute values
        /// directly from the database without materializing the entity.
        /// </summary>
        /// <param name="attributeValues">The colleciton of queryable attribute values to be converted.</param>
        /// <param name="agentRequestContext">The agent's request context, this is used to perform additional security checks.</param>
        /// <returns>A collection of <see cref="AttributeValueResult"/> objects that represent the attribute values.</returns>
        [Expandable( nameof( GetAttributeValueResultsExpression ) )]
        public static IEnumerable<AttributeValueResult> GetAttributeValueResults( this IEnumerable<QueryableAttributeValue> attributeValues, AgentRequestContext agentRequestContext )
        {
            if ( attributeValues == null )
            {
                return Enumerable.Empty<AttributeValueResult>();
            }

            return _getAttributeValueResultsFunc.Value( attributeValues, agentRequestContext );
        }

        /// <summary>
        /// Returns a collection of attribute values that should be included
        /// in the entity result object. This should be used when the entity
        /// has been fully materialized into memory.
        /// </summary>
        /// <param name="entity">The entity whose attributes need to be retrieved.</param>
        /// <param name="agentRequestContext"></param>
        /// <returns>A collection of <see cref="AttributeValueResult"/> objects that represent the attribute values.</returns>
        public static IEnumerable<AttributeValueResult> GetAttributeValueResults( this IHasAttributes entity, AgentRequestContext agentRequestContext )
        {
            if ( entity == null )
            {
                return Enumerable.Empty<AttributeValueResult>();
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( agentRequestContext.RockContext );
            }

            var isInternal = agentRequestContext.AudienceType == AudienceType.Internal;

            return entity.Attributes.Values
                .Where( a => isInternal || a.IsPublic )
                .Select( a => new AttributeValueResult
                {
                    AttributeId = a.Id,
                    Value = entity.GetAttributeValue( a.Key ),
                    Name = a.Name,
                    Key = a.Key,
                    TextValue = entity.GetAttributeTextValue( a.Key ),
                } );
        }

        /// <summary>
        /// Returns a collection of attribute value results from the queryable
        /// attribute values. This should be used when retrieving attribute values
        /// directly from the database without materializing the entity.
        /// </summary>
        /// <returns>An expression that can be used in LINQ to SQL to gather the attribute values.</returns>
        private static Expression<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> GetAttributeValueResultsExpression()
        {
            return _getAttributeValueResultsExpression;
        }

        /// <summary>
        /// Returns a collection of attribute value results from the queryable
        /// attribute values. This should be used when retrieving attribute values
        /// directly from the database without materializing the entity. Only
        /// values for attributes marked as Show on Grid are included.
        /// </summary>
        /// <param name="attributeValues">The colleciton of queryable attribute values to be converted.</param>
        /// <param name="agentRequestContext">The agent's request context, this is used to perform additional security checks.</param>
        /// <returns>A collection of <see cref="AttributeValueResult"/> objects that represent the attribute values.</returns>
        [Expandable( nameof( GetGridAttributeValueResultsExpression ) )]
        public static IEnumerable<AttributeValueResult> GetGridAttributeValueResults( this IEnumerable<QueryableAttributeValue> attributeValues, AgentRequestContext agentRequestContext )
        {
            if ( attributeValues == null )
            {
                return Enumerable.Empty<AttributeValueResult>();
            }

            return _getGridAttributeValueResultsFunc.Value( attributeValues, agentRequestContext );
        }

        /// <summary>
        /// Returns a collection of attribute values that should be included
        /// in the entity result object. This should be used when the entity
        /// has been fully materialized into memory.
        /// </summary>
        /// <param name="entity">The entity whose attributes need to be retrieved.</param>
        /// <param name="agentRequestContext"></param>
        /// <returns>A collection of <see cref="AttributeValueResult"/> objects that represent the attribute values.</returns>
        public static IEnumerable<AttributeValueResult> GetGridAttributeValueResults( this IHasAttributes entity, AgentRequestContext agentRequestContext )
        {
            if ( entity == null )
            {
                return Enumerable.Empty<AttributeValueResult>();
            }

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( agentRequestContext.RockContext );
            }

            var isInternal = agentRequestContext.AudienceType == AudienceType.Internal;

            return entity.Attributes.Values
                .Where( a => ( isInternal || a.IsPublic )
                    && a.IsGridColumn )
                .Select( a => new AttributeValueResult
                {
                    AttributeId = a.Id,
                    Value = entity.GetAttributeValue( a.Key ),
                    Name = a.Name,
                    Key = a.Key,
                    TextValue = entity.GetAttributeTextValue( a.Key ),
                } );
        }

        /// <summary>
        /// Returns a collection of attribute value results from the queryable
        /// attribute values. This should be used when retrieving attribute values
        /// directly from the database without materializing the entity.
        /// </summary>
        /// <returns>An expression that can be used in LINQ to SQL to gather the attribute values.</returns>
        private static Expression<Func<IEnumerable<QueryableAttributeValue>, AgentRequestContext, IEnumerable<AttributeValueResult>>> GetGridAttributeValueResultsExpression()
        {
            return _getGridAttributeValueResultsExpression;
        }

        #endregion
    }
}
