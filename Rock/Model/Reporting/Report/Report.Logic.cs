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

using Rock.Data;
using Rock.Reporting;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Report (based off of a <see cref="Rock.Model.DataView"/> in Rock).
    /// </summary>
    public partial class Report : Model<Report>, ICategorized
    {
        /// <summary>
        /// Gets the parent security authority for the Report which is its Category
        /// </summary>
        /// <value>
        /// The parent authority of the DataView.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.Category != null )
                {
                    return this.Category;
                }

                return base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the attribute value expression.
        /// </summary>
        /// <param name="attributeValues">The attribute values.</param>
        /// <param name="attributeValueParameter">The attribute value parameter.</param>
        /// <param name="parentIdProperty">The parent identifier property.</param>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <returns></returns>
        private Expression GetAttributeValueExpression( IQueryable<AttributeValue> attributeValues, ParameterExpression attributeValueParameter, Expression parentIdProperty, int attributeId )
        {
            MemberExpression attributeIdProperty = Expression.Property( attributeValueParameter, "AttributeId" );
            MemberExpression entityIdProperty = Expression.Property( attributeValueParameter, "EntityId" );
            Expression attributeIdConstant = Expression.Constant( attributeId );

            Expression attributeIdCompare = Expression.Equal( attributeIdProperty, attributeIdConstant );
            Expression entityIdCompre = Expression.Equal( entityIdProperty, Expression.Convert( parentIdProperty, typeof( int? ) ) );
            Expression andExpression = Expression.And( attributeIdCompare, entityIdCompre );

            var match = new Expression[] {
                Expression.Constant(attributeValues),
                Expression.Lambda<Func<AttributeValue, bool>>( andExpression, new ParameterExpression[] { attributeValueParameter })
            };

            Expression whereExpression = Expression.Call( typeof( Queryable ), "Where", new Type[] { typeof( AttributeValue ) }, match );

            var attributeCache = AttributeCache.Get( attributeId );
            var attributeValueFieldName = "Value";
            Type attributeValueFieldType = typeof( string );
            if ( attributeCache != null )
            {
                attributeValueFieldName = attributeCache.FieldType.Field.AttributeValueFieldName;
                attributeValueFieldType = attributeCache.FieldType.Field.AttributeValueFieldType;
            }

            MemberExpression valueProperty = Expression.Property( attributeValueParameter, attributeValueFieldName );

            Expression valueLambda = Expression.Lambda( valueProperty, new ParameterExpression[] { attributeValueParameter } );

            Expression selectValue = Expression.Call( typeof( Queryable ), "Select", new Type[] { typeof( AttributeValue ), attributeValueFieldType }, whereExpression, valueLambda );

            Expression firstOrDefault = Expression.Call( typeof( Queryable ), "FirstOrDefault", new Type[] { attributeValueFieldType }, selectValue );

            return firstOrDefault;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
